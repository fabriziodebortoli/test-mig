#pragma once


class CMessageQueue;
class TBDC;
class TBEdit;
class TBListBox;
class CSpyHook;
class TBMenu;

#include "StreamReaderWriter.h"

class TBWnd : public CStreamReaderWriter
{
	friend class TBMenu;
private:
	TBWnd*			m_pParent;
	int				m_nIndexInParentChild;	//indice di posizione nel parent, lo tengo per motivi di efficienza anche se potrei calcolarmelo
	int				m_nIndexInParentOwned;	//indice di posizione nel parent, lo tengo per motivi di efficienza anche se potrei calcolarmelo 
	int				m_nBorderWidth;
	int				m_nBorderHeight;
	int				m_nCaptionHeight;
	CPoint			m_arScrollRanges[3];
	TBMenu*			m_pContextMenu = NULL;
protected:
	CArray<TBWnd*>	m_Child;//children windows (not popup)
	CArray<TBWnd*>	m_Owned;//owned windows (popup)
	HFONT			m_hFont;
	DWORD			m_dwThreadId;
	CREATESTRUCT	m_cs;
	DWORD			m_id;
	HWND			m_hWnd;
	WNDPROC			m_wndProc;
	LONG			m_dwUserData;
	CString			m_text;
	CString			m_ClassName;
	CPtrArray		m_arDestroyed;
	HBITMAP			m_hDCBitmap;
public:
	TBWnd(HWND hwnd, DWORD dwThreadId);
	virtual ~TBWnd();
	inline HWND GetHWND() { return m_hWnd; }
	inline DWORD GetID() { return m_id; }
	virtual void	SetRectFromDialogUnits(CRect& rect);
	inline DWORD GetThreadID()		{ return m_dwThreadId; }
	
	inline void SetWidth(int w);
	inline int GetWidth()			{ return m_cs.cx; }
	
	inline void SetHeight(int h);
	inline int GetHeight()			{ return m_cs.cy; }
	
	inline void SetX(int x)			{ m_cs.x = x; }
	inline int GetX()				{ return m_cs.x; }
	
	inline void SetY(int y)			{ m_cs.y = y; }
	inline int GetY()				{ return m_cs.y; }
	
	inline void SetMenu(HMENU menu)	{ m_cs.hMenu = menu; }
	inline HMENU GetMenu()			{ return m_cs.hMenu; }
	inline HMENU GetSystemMenu(BOOL bRevert)	{ return NULL; }//TODOPERASSO

	inline void SetCreateParams(LPVOID pParam)		{ m_cs.lpCreateParams = pParam; }
	inline void SetClass(LPCTSTR pszClass)			{ m_ClassName = pszClass; }
	inline LPCTSTR GetClass()						{ return m_ClassName; }
	inline void SetName(LPCTSTR pszName)			{ m_cs.lpszName = pszName; }
	inline BOOL IsWindowEnabled()					{ return (m_cs.style & WS_DISABLED) != WS_DISABLED; } 
	virtual void EnableWindow(BOOL bEnable);

	inline BOOL IsWindowVisible()					{ return (m_cs.style & WS_VISIBLE) == WS_VISIBLE; } 
	BOOL ShowWindow(int cmdShow);

	void GetWindowRect(__out LPRECT lpRect);
	void GetClientRect(__out LPRECT lpRect);
	//void SetWindowRgn(HRGN hRgn) { m_hRgn = hRgn; }
	//HRGN GetWindowRgn() { return m_hRgn; }
	LRESULT	Dispatch(UINT message, WPARAM wParam, LPARAM lParam);
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	TBWnd* GetWindow(UINT uCmd);
	TBWnd* GetTopWindow();
	void SetParent(TBWnd* pParent); 
	TBWnd* GetParent() { return m_pParent; }  
	TBWnd* GetAncestor(UINT gaFlags);
	LONG SetWindowLong(int nIndex, LONG dwNewLong);
	LONG GetWindowLong(int nIndex);
	DWORD GetStyle() { return GetWindowLong(GWL_STYLE); }
	void Destroy();
	TBWnd* GetDlgItem(int nIDDlgItem);
	BOOL IsChild(HWND hwndChild);
	inline BOOL IsChild() { return (m_cs.style & WS_CHILD) == WS_CHILD; }
	inline BOOL IsPopup() { return (m_cs.style & WS_POPUP) == WS_POPUP; }
	void CallCreationHook();
	BOOL SendCreationMessages();
	void ScreenToClient(LPPOINT lpPoint);
	void ClientToScreen(LPPOINT lpPoint);
	void MoveWindow(int X, int Y, int nWidth, int nHeight, BOOL bRepaint);
	virtual void SetWindowPos(HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags);
	virtual BOOL MapDialogRect(LPRECT lpRect);
	BOOL IsTopLevelWindow();
	virtual void SaveToStream(CMemFile*);
	virtual void ReadFromStream(CMemFile*);
	virtual BOOL IsDialogMessage(LPMSG lpMsg) { return FALSE; }
	CMessageQueue* GetMessageQueue();
	LRESULT SendMessage(UINT message, WPARAM wParam, LPARAM lParam);
	BOOL GetScrollRange(int nBar, LPINT lpMinPos, LPINT lpMaxPos);
	BOOL SetScrollRange(int nBar, int nMinPos, int nMaxPos);
	HBITMAP GetDCBitmap();
protected:
	LRESULT NotifyParent(UINT code, void* lParam);

private:
	void GetTopLeftScreenRelative(POINT& p);
	void OffsetToClientArea(POINT& p);
	CArray<TBWnd*>* GetChildOrOwned(BOOL bChild) { return bChild ? &m_Child : &m_Owned; }
	int& GetChildOrOwnedIndex(BOOL bChild) { return bChild ? m_nIndexInParentChild : m_nIndexInParentOwned; }
	void OnCancelMode();
};
class TBToolbar : public TBWnd
{
public:
	CArray<TBBUTTON*> m_arButtons;
	HIMAGELIST m_hImageList;
	HIMAGELIST m_hDisabledImageList;
	HIMAGELIST m_hHotImageList;
	CSize m_BitmapSize;
	CSize m_ButtonSize;
	TBToolbar(HWND hwnd, DWORD dwThreadId) : m_hImageList(NULL), m_hDisabledImageList(NULL), m_hHotImageList(NULL), TBWnd(hwnd, dwThreadId){} 
	~TBToolbar();
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
class TBStatic : public TBWnd
{
	HBITMAP m_hBitmap;
	HCURSOR m_hCursor;
public:
	TBStatic(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId), m_hBitmap(NULL), m_hCursor(NULL){}
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
class TBButton : public TBWnd
{
	HBITMAP m_hBitmap;
	HCURSOR m_hCursor;
	int m_nCheck;
	BOOL m_bState;
public:
	TBButton(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId), m_hBitmap(NULL), m_hCursor(NULL), m_nCheck(0), m_bState(FALSE){}
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
class TBTabControl : public TBWnd
{
	HIMAGELIST m_hImageList;
	CArray<TCITEM*> m_arItems;
	int m_nCurSel;
public:
	TBTabControl(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId), m_hImageList(NULL), m_nCurSel(-1){}
	~TBTabControl();
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
class CMyMemFile;
class TBSpecial : public TBWnd
{
	CMyMemFile* m_pMemFile = NULL;
public:
	TBSpecial(DWORD dwThreadId);
	virtual ~TBSpecial();
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
class CWindowMap : public CMap<HWND, HWND, TBWnd*, TBWnd*>, CStreamReaderWriter
{
public:
	~CWindowMap();
	void SaveToStream(CMemFile*);
	void CleanUpThreadWnd(DWORD dwThreadId);
};
struct CDeferItem 
{
	HWND m_hWnd;
	HWND m_hWndInsertAfter;
	int m_x;
	int m_y;
	int m_cx;
	int m_cy;
	UINT m_uFlags;
	CDeferItem()
	{
		m_hWnd = NULL;
		m_hWndInsertAfter = NULL;
		m_x = 0;
		m_y = 0;
		m_cx = 0;
		m_cy = 0;
		m_uFlags = 0;

	}
	CDeferItem(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, UINT uFlags)
	{
		m_hWnd = hWnd;
		m_hWndInsertAfter = hWndInsertAfter;
		m_x = x;
		m_y = y;
		m_cx = cx;
		m_cy = cy;
		m_uFlags = uFlags;

	}
};
class CDeferItemList : public CArray<CDeferItem*>
{
public:
	~CDeferItemList();
	void Add(HWND hWnd, HWND hWndInsertAfter, int x, int y, int cx, int cy, UINT uFlags);

};
class CDeferMap : public CMap<HDWP, HDWP, CDeferItemList*, CDeferItemList*>
{
};

TBWnd* GetTBWnd(HWND hWnd);
CDeferItemList* GetDeferItemList(HDWP hdwp);
TBWnd* _TBCreateWindow(DWORD dwExStyle, LPCWSTR lpClassName, LPCWSTR lpWindowName, DWORD dwStyle, int nId, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam, WNDPROC wndProc);
