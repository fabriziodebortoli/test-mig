#pragma once

#include "beginh.dex"

class TB_EXPORT TFXDataTip : public CWnd,  public CTBLockable
{
// Construction
public:
	TFXDataTip();
	virtual ~TFXDataTip();

    void			SetSurrounding(long x, long y)	{ m_TipSurrounding = CSize(x, y); };
    void			SetSurrounding(CSize offset)	{ m_TipSurrounding = offset; };

	virtual BOOL	Create(CWnd* pParentWnd);

	void			ResetText();
	void			AddText(LPCTSTR szText);
	void			SetNewTip(CPoint point);

	void			Set(CPoint point, CString szText);

    BOOL			Hide( );

    void			On(BOOL on);
    BOOL			IsOn() const					{ return m_on; };	

    static void		SetDelay(short delay)			{ m_s_delay = delay; };

	BOOL			IsVisible() const { return m_bVisible && m_ready && this == m_s_pCurrent; }

	void			SetOffset(long x, long y)		{ m_offset = CPoint(x, y); }
	void			SetOffset(CPoint offset)		{ m_offset = offset; }

	CSize			GetSize();
    static void		SetMaxDim(short w, short h)			{ m_s_maxWidth = w; m_s_maxHeight = h; }

public:
	virtual LPCSTR  GetObjectName() const { return "TFXDataTip"; }

	virtual BOOL DestroyWindow();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
protected:
	//{{AFX_MSG(TFXDataTip)
	afx_msg void OnPaint();
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	afx_msg void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	
	void						Display();
	static void					RegisterWnd();
	static void					Initialise();
	static LRESULT CALLBACK		KeyboardHookCallback(int code, WPARAM wParam, LPARAM lParam);

	CWnd				*m_parent;
	CStringList			m_listStrings;
	CPoint				m_TipPosition,		// Position where to place the tip
						m_MousePosition;	// Position of mouse when tip is placed
	CSize				m_TipSurrounding;	// Moving the mouse so far is ok (no hiding of tip)
	BOOL				m_ready;
	BOOL				m_on;
	UINT				m_timer,
						m_timerToHide;
	CPoint				m_offset;			// define the offset above cursor
	
	BOOL				m_bVisible;
	
	DWORD				m_TickCount;

	static BOOL			m_s_registered;
	static short		m_s_delay;
	static short		m_s_count;

	static HHOOK		m_s_hookProc;
	static TFXDataTip	*m_s_pCurrent;

	static short		m_s_maxWidth;
	static short		m_s_maxHeight;
};

#include "endh.dex"
