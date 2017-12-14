#pragma once

#include "beginh.dex"

class  TB_EXPORT CCISBitmap : public CBitmap  
{
public:
	CCISBitmap();
	virtual ~CCISBitmap();

	// Functions
	int Height();
	int Width();	
	virtual void DrawTransparent(CDC* pDC, int x, int y, COLORREF crColour);	
	virtual void DrawTransparent(CDC* pDC, int x, int y);
	void SetTransparentColor(COLORREF cr);

private:
	COLORREF	m_crTransparent;
};

#include "endh.dex"
