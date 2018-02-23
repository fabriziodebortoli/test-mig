#pragma once

namespace Gdiplus
{
	class Bitmap;
}

//includere alla fine degli include del .H
#include "beginh.dex"

//enum PicImageFitMode { NORMAL, BEST, HORIZONTAL, VERTICAL };
//
////=============================================================================
//TB_EXPORT CString PicImageFitModeToString(PicImageFitMode f);
////==============================================================================

class TB_EXPORT CTBPicture : public CObject
{
	DECLARE_DYNAMIC(CTBPicture)

protected:
	CString			m_strFileName;

public:
	enum ImageFitMode { NORMAL/*ORIGINAL*/, BEST/*PROPORTIONAL best fit*/, HORIZONTAL/*PROPORTIONAL - vince larghezza*/, VERTICAL/*PROPORTIONAL - vince altezza*/, STRETCH	};
	ImageFitMode	m_ImageFitMode = ImageFitMode::NORMAL;

	BOOL			m_bOk;
protected:
	BOOL				m_bUseGdiPlus;
	Gdiplus::Bitmap*	m_pImage;

public:
	// Constructors
	CTBPicture(ImageFitMode imgFitMode = STRETCH);
	CTBPicture(const CTBPicture&);
	virtual ~CTBPicture();

public:
	CString GetFileName() const { return m_strFileName; }

	void Reload		();
	BOOL ReadFile	(const CString& strFileName, BOOL bCheckExist /*= FALSE*/, BOOL bNoCache = FALSE);

	BOOL SaveFile	(const CString& strFileName);

	BOOL LoadBitmapFromResource(int IDB, HDC hDC);

	BOOL IsOk				() const { return m_bOk && m_pImage; }
	BOOL TopLeftOrientation	();

	void DrawPicture		(CDC& DC, CRect dest);
	void DrawPicture		(CDC& DC, CRect dest, CRect src);

	int GetWidth() const;
	int GetHeight() const;
	Gdiplus::Bitmap* GetImage() const	{ return m_pImage; }
	void Clear();

	void FitImage	(CRect& rect,	const CRect& rectSrc);
	BOOL ImageIsValid() const			{ return m_pImage != NULL; }

	static CString ImageFitModeToString(ImageFitMode f);

	BOOL IsBestFitSetted	()			{ return m_ImageFitMode == ImageFitMode::BEST; }

	HBITMAP GetHBitmap(COLORREF* pRgbBkgColor = NULL);

protected:
	static	void*	g_pGdiPlusContext;

public:
	static BOOL IsImageGdiPlusValid	(CString filePath);
	static void InitializeGdiPlus	();
	static void TerminateGdiPlus	();

};


#include "endh.dex"

