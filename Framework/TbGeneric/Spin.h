#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//Control specific styles.
#define MSS_VERTICAL        0x0001L
#define MSS_HORIZONTAL      0x0002L
#define MSS_NOPEGSCROLL     0x0004L
#define MSS_INVERTRANGE     0x0008L

#define SPIN_STYLE		MSS_VERTICAL


//============================================================================
TB_EXPORT BOOL RegisterSpinCtrl(HINSTANCE hInstance);

//============================================================================
class TB_EXPORT CSpin : public CWnd
{
	DECLARE_DYNCREATE(CSpin)

public:
	CSpin();
	BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);

	// Attributes
	CWnd* GetAssociate();
	void SetAssociate(CWnd* pNew);
	void GetRange(WORD& nMin, WORD& nMax);
	void SetRange(WORD nMin, WORD nMax);
	UINT GetCurrentPos();
	void SetCurrentPos(UINT nPos);
};

#include "endh.dex"
