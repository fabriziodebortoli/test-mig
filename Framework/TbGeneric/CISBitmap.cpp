
#include "StdAfx.h"

#include "CISBitmap.h"
#include "TBThemeManager.h"

//-----------------------------------------------------------------------------
CCISBitmap::CCISBitmap()
{
}

//-----------------------------------------------------------------------------
CCISBitmap::~CCISBitmap()
{

}

//-----------------------------------------------------------------------------
int CCISBitmap::Width()
{
	BITMAP bm;
	GetBitmap(&bm);
	return bm.bmWidth;
}

//-----------------------------------------------------------------------------
int CCISBitmap::Height()
{
	BITMAP bm;
	GetBitmap(&bm);
	return bm.bmHeight;
}

//-----------------------------------------------------------------------------
void CCISBitmap::DrawTransparent(CDC * pDC, int x, int y, COLORREF crColour)
{
	COLORREF crOldBack = pDC->SetBkColor(AfxGetThemeManager()->GetCISBitmapBkgColor());
	COLORREF crOldText = pDC->SetTextColor(AfxGetThemeManager()->GetCISBitmapForeColor());
	CDC dcImage, dcTrans;

	// Create two memory dcs for the image and the mask
	dcImage.CreateCompatibleDC(pDC);
	dcTrans.CreateCompatibleDC(pDC);

	// Select the image into the appropriate dc
	CBitmap* pOldBitmapImage = dcImage.SelectObject(this);

	// Create the mask bitmap
	CBitmap bitmapTrans;
	int nWidth = Width();
	int nHeight = Height();
	bitmapTrans.CreateBitmap(nWidth, nHeight, 1, 1, NULL);

	// Select the mask bitmap into the appropriate dc
	CBitmap* pOldBitmapTrans = dcTrans.SelectObject(&bitmapTrans);

	// Build mask based on transparent colour
	dcImage.SetBkColor(crColour);
	dcTrans.BitBlt(0, 0, nWidth, nHeight, &dcImage, 0, 0, SRCCOPY);

	// Do the work - True Mask method - cool if not actual display
	pDC->BitBlt(x, y, nWidth, nHeight, &dcImage, 0, 0, SRCINVERT);
	pDC->BitBlt(x, y, nWidth, nHeight, &dcTrans, 0, 0, SRCAND);
	pDC->BitBlt(x, y, nWidth, nHeight, &dcImage, 0, 0, SRCINVERT);

	// Restore settings
	dcImage.SelectObject(pOldBitmapImage);
	dcTrans.SelectObject(pOldBitmapTrans);
	pDC->SetBkColor(crOldBack);
	pDC->SetTextColor(crOldText);
}

//-----------------------------------------------------------------------------
void CCISBitmap::DrawTransparent(CDC* pDC, int x, int y)
{
	DrawTransparent(pDC, x, y, m_crTransparent);
}

//-----------------------------------------------------------------------------
void CCISBitmap::SetTransparentColor(COLORREF cr)
{
	m_crTransparent = cr;
}
