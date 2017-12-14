#pragma once
class TBWnd;
class TBDC
{
	HDC m_hdc;
	HGDIOBJ m_hOld;
public:
	TBDC(HDC hdc);
	TBDC(HWND hwnd);
	virtual ~TBDC();
	BOOL GetTextExtentPoint32(LPCWSTR lpString, int c, LPSIZE psizl);
	BOOL GetTextMetrics(LPTEXTMETRIC lptm);
	inline HDC GetHDC() { return m_hdc; }
};

class CHDCToTBDCMap : public CMap<HDC, HDC, TBDC*, TBDC*>
{
public:
	~CHDCToTBDCMap()
	{
		POSITION pos = GetStartPosition();
		while (pos)
		{
			HDC k;
			TBDC* v;
			GetNextAssoc(pos, k, v);
			delete v;
		}
	}
};
TBDC* GetTBDC(HDC hdc);
HDC CreateTBDC(HDC hdc);
HDC CreateTBDC(HWND hwnd);
BOOL DestroyTBDC(HDC dc);