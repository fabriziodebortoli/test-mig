#include "stdafx.h"

#include <gdiplus.h>

#include <TbNamesolver\Diagnostic.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TBStrings.h>
#include "EmfToPdf.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Drawing;
using namespace PdfSharp;
using namespace PdfSharp::Pdf;
using namespace PdfSharp::Drawing;
using namespace PdfSharp::Pdf::IO;
using namespace PdfSharp::Pdf::Security;
using namespace System::Drawing::Imaging;
using namespace System::Runtime::InteropServices;

using namespace Microarea::TaskBuilderNet::Core::Generic;

#define UNSUPPORTED(record) case record: throw gcnew NotSupportedException(String::Format("Record type not supported: {0}", #record));

public ref class XFontEx : public PdfSharp::Drawing::XFont
{
public:
	int Orientation;

	XFontEx(System::Drawing::Font^ font, XPdfFontOptions^ pdfOptions)
		: XFont (font, pdfOptions)
    {
		Orientation = 0;
	}

	XFontEx(String^ familyName, double emSize, XFontStyle style, XPdfFontOptions^ pdfOptions)
		: XFont(familyName, emSize, style, pdfOptions)
    {
		Orientation = 0;
    }

};

ref class XImageCache : public System::Object
{
public:
	XImage^					image;
	array<System::Byte>^	hash;

	XImageCache(XImage^	img, array<System::Byte>^  h) 
	{
		image = img;
		hash = h;
	}

	bool EqualHash(array<System::Byte>^  h)
	{
		if (hash->Length != h->Length)
			return false;

		//Compare the hash values
        for (int i = 0; i < hash->Length; i++)
        {
            if (hash[i] != h[i])
                return false;
        }
		return true;
	}
};

//=============================================================================
class CInternalPdfDocumentWrapper
{
private:
	gcroot<PdfDocument^>										m_pDocument;
public:
	gcroot<System::Collections::Generic::List<XImageCache^>^>	m_lstXImages;
	gcroot<System::Drawing::ImageConverter^>					m_imgConverter;
	gcroot<System::Security::Cryptography::SHA256Managed^>		m_shaM;

public:
	CInternalPdfDocumentWrapper()
	{
		m_pDocument = gcnew PdfDocument();

		m_pDocument->Options->UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages::Automatic;
		m_pDocument->Options->FlateEncodeMode = PdfFlateEncodeMode::BestCompression;
		m_pDocument->Options->EnableCcittCompressionForBilevelImages = true;
		m_pDocument->Options->CompressContentStreams = true;
		m_pDocument->Options->NoCompression = false;

		//image cache
		m_lstXImages = gcnew System::Collections::Generic::List<XImageCache^>;
		m_imgConverter = gcnew System::Drawing::ImageConverter();
		m_shaM = gcnew System::Security::Cryptography::SHA256Managed();
	}

	~CInternalPdfDocumentWrapper()
	{
		m_pDocument->Close();
		delete m_pDocument;//chiama la dispose

		//image cache
		for (int j = 0; j < m_lstXImages->Count; j++)
		{
			//XImageCache^ xc = m_lstXImages[j];
			XImageCache^ xc = m_lstXImages->default[j];
			delete xc->image;
			delete xc->hash;
			delete xc;
		}
		m_lstXImages->Clear();
		delete m_lstXImages;
		delete m_imgConverter;
		delete m_shaM;
	}

	PdfDocument^ GetPdfDocument()	{ return m_pDocument; }
};
//=================================================================================
ref class PdfCache
{
public:
	CInternalPdfDocumentWrapper* m_pWrpPdfDocument;

	PdfDocument^ Document;
	Hashtable^	Objects;
	XGraphics^	Graphics;
	PdfPage^	Page;
	XFont^		CurrentFont;
	XBrush^		CurrentBrush;	
	XPen^		CurrentPen;
	XColor		BackColor;
	XColor		TextColor;
	int			BackMode;
	XStringFormat^	TextAlign;
	XPoint		CurrentPoint;
	System::Collections::Generic::Stack<MemoryStream^>^	SavedStreams;
	float		ScaleFactor;
	bool		PrinterDC;

	int			MapMode;

	bool		MeasurePage;

	PdfCache(CInternalPdfDocumentWrapper* pWrpPdfDocument, PdfDocument^ document)
	{
		m_pWrpPdfDocument = pWrpPdfDocument;
		Document = document;

		Page = Document->AddPage();
		Graphics = XGraphics::FromPdfPage(Page);
		Objects = gcnew Hashtable();
		CurrentFont = gcnew XFont("Arial", 12);
		CurrentBrush = XBrushes::White;
		CurrentPen = XPens::Blue;
		BackColor = XColors::White;
		TextColor = XColors::Black;
		BackMode = OPAQUE;
		SavedStreams = gcnew System::Collections::Generic::Stack<MemoryStream^>();
		CurrentPoint = XPoint();
		TextAlign = XStringFormat::TopLeft;
		ScaleFactor = 1.0;
		PrinterDC = false;
		MapMode = MM_TEXT;
		MeasurePage = false;
	}

	void SaveToStream()
	{
		/*MemoryStream^ stream = gcnew MemoryStream();
		Document->Save(stream, false);
		SavedStreams->Push(stream);*/
	}

	void RestoreFromStream(int index)
	{
		/*MemoryStream^ stream = SavedStreams->Pop();
		stream->Seek(0, SeekOrigin::Begin);
		delete Document;
		delete Graphics;
		Document = PdfReader::Open(stream);
		Page = Document->Pages[Document->Pages->Count - 1];
		Graphics = XGraphics::FromPdfPage(Page);
		delete stream;*/
	}

	//-------------------------------------------------------------------------
	void SetPageSize(int w, int h)
	{
		//if (!MeasurePage)
		//	return;

		//ATTENZIONE
		//In certi casi non ben identificati, forse dipendenti dal valore dei margini,
		//il calcolo dell'altezza della pagina sborda di 1 (sia in portrait che in landscape)
		//quindi viene prodotto un pdf con le pagine di dimensione non perfettamente A4 (ad esempio)
		//per PosteLite è un problema bloccante!
		//EURISTICA: se è "circa" un A4 lo diventa
		int hm = int(Page->TrimMargins->Top.Millimeter + Page->TrimMargins->Bottom.Millimeter);
		int wm = int(Page->TrimMargins->Left.Millimeter + Page->TrimMargins->Right.Millimeter);

		int hhm = h + hm;
		int wwm = w + wm;

		if ( (abs(hhm - 297) < 2) &&  (abs(wwm - 210) < 2) )
		{	
			h = 297 - hm;
			w = 210 - wm;

			hhm = h + hm;
			wwm = w + wm;
		}
		else if ( (abs(hhm - 210) < 2) && (abs(wwm - 297) < 2) )
		{	
			h = 210 - hm;
			w = 297 - wm;

			hhm = h + hm;
			wwm = w + wm;
		}

		if ( (hhm == 210 && wwm == 297) || (hhm == 297 && wwm == 210) )
		{
			Page->Size = PdfSharp::PageSize::A4;
			//Page->Orientation = (hhm == 210) ? PdfSharp::PageOrientation::Landscape : PdfSharp::PageOrientation::Portrait;
		}

		Page->Width = XUnit::FromMillimeter(w);
		Page->Height = XUnit::FromMillimeter(h);
	}

	void SetPageSizeWithMargin(int w, int h)
	{
		if ( (abs(h - 297) < 2) &&  (abs(w - 210) < 2) )
		{	
			h = 297;
			w = 210;

			Page->Size = PdfSharp::PageSize::A4;
			//Page->Orientation = PdfSharp::PageOrientation::Portrait;
		}
		else if ( (abs(h - 210) < 2) && (abs(w - 297) < 2) )
		{	
			h = 210;
			w = 297;

			Page->Size = PdfSharp::PageSize::A4;
			//Page->Orientation = PdfSharp::PageOrientation::Landscape;
		}

		int wm = int(Page->TrimMargins->Left.Millimeter + Page->TrimMargins->Right.Millimeter);
		int hm = int(Page->TrimMargins->Top.Millimeter + Page->TrimMargins->Bottom.Millimeter);

		Page->Width = XUnit::FromMillimeter(w - wm);
		Page->Height = XUnit::FromMillimeter(h - hm);
	}

	//-------------------------------------------------------------------------
	XUnit Scale(int d)
	{
		switch(MapMode)
		{
		case MM_ANISOTROPIC:
			return PrinterDC 
				? XUnit::FromMillimeter(d / ScaleFactor) 
				: XUnit::FromCentimeter(LPtoMU(d, CM, 1, 3));
		case MM_TEXT:
		default:
			return PrinterDC 
				? XUnit::FromMillimeter(d / ScaleFactor) 
				: XUnit::FromCentimeter(LPtoMU(d, CM, 1, 3));
		}
	} 

	XRect Scale(System::Drawing::Rectangle r)
	{
		return XRect((float)Scale(r.X), (float)Scale(r.Y), (float)Scale(r.Width), (float)Scale(r.Height));
	}
	XSize Scale(System::Drawing::Size sz)
	{
		return XSize((float)Scale(sz.Width), (float)Scale(sz.Height));
	}
	XPoint Scale(System::Drawing::Point pt)
	{
		return XPoint((float)Scale(pt.X), (float)Scale(pt.Y));
	}
	XRect Scale(const CRect& r)
	{
		return XRect((float)Scale(r.left), (float)Scale(r.top), (float)Scale(r.Width()), (float)Scale(r.Height()));
	}

	//--------------------------------------------------------------------------
	int ScaleFontHeight(LPLOGFONT lf)
	{
		int sign = Math::Sign(lf->lfHeight);
		double d = sign * Math::Round(Scale(Math::Abs(lf->lfHeight)));
		return Convert::ToInt32(d);
	}

	//-------------------------------------------------------------------------
	XFont^ FromLogFont(LPLOGFONT lf)
	{
/*DA CENTRALIZZARE
		CStringArray fonts;
		AfxGetPathFinder()->GetAvailableThemeFonts(fonts);
		System::Drawing::Text::PrivateFontCollection^ privFonts = gcnew System::Drawing::Text::PrivateFontCollection();
		for (int i = 0; i <= fonts.GetUpperBound(); i++)
		{
			CString font = fonts[i];
			//int a = AddFontResourceEx(font, FR_PRIVATE, 0);
			privFonts->AddFontFile(gcnew String(font));
			int b = 0;
		}
*/

		ExternalAPI::LOGFONT^ lfManaged = (ExternalAPI::LOGFONT^) Marshal::PtrToStructure((IntPtr)lf, ExternalAPI::LOGFONT::typeid);
		lfManaged->lfHeight = ScaleFontHeight(lf);
		//TRACE(cwsprintf(_T("Height %s: %d %d %d"), lf->lfFaceName, lf->lfHeight, lfManaged->lfHeight, lf->lfWeight));

		XFontEx^ font = nullptr;		
		System::Drawing::Font^ f = nullptr;
		try
		{
			f = System::Drawing::Font::FromLogFont(lfManaged);
		}
		catch (Exception^ e1)
		{
			Exception^ e0 = e1;
			TRACE(cwsprintf(_TB("Font not supported: {0-%s}. {1-%s}"), lf->lfFaceName, CString(e1->Message)));

			if (_tcsicmp(lf->lfFaceName, L"Roboto") == 0 || _tcsicmp(lf->lfFaceName, L"Open Sans") == 0)
			{
				TB_TCSCPY(lf->lfFaceName, L"Segoe UI");
				return FromLogFont(lf);
			}
			return FromLogFontOld(lf);
		}
/*
			LONG l = lf->lfWeight;
			BOOL bNotRegular = 
				(lf->lfWeight) ||
				(lf->lfItalic) ||
				(lf->lfUnderline) ||
				(lf->lfStrikeOut);

			try	//provo a caricare i font manualmente
			{
			
				String^ s;
				for (int i = 0; i < privFonts->Families->Length; i++)
				{
					FontFamily^ FN = privFonts->Families[i];
					s = FN->Name;
					if (s->CompareTo(gcnew String(lf->lfFaceName)) == 0)
					{
						FontStyle^ fs = gcnew FontStyle();
						//Se ho trovato il FontFamily provo a instanziare il font con esso
						f = gcnew System::Drawing::Font(s, (float)abs(lfManaged->lfHeight),
												(
													(lf->lfWeight ? FontStyle::Bold : FontStyle::Regular) |
													(lf->lfItalic ? FontStyle::Italic : FontStyle::Regular) |
													(lf->lfUnderline ? FontStyle::Underline : FontStyle::Regular) |
													(lf->lfStrikeOut ? FontStyle::Strikeout : FontStyle::Regular) 
												),
												GraphicsUnit::World);
						break;
					}
				}
				if (f == nullptr)
					throw e1;
			}
			catch (Exception^)
			{
				if (_tcsicmp(lf->lfFaceName, L"Roboto") == 0 || _tcsicmp(lf->lfFaceName, L"Open Sans") == 0)
				{
					TB_TCSCPY(lf->lfFaceName, L"Segoe UI");
					return FromLogFont(lf);
				}
				return FromLogFontOld(lf);
			}
		}
*/

		try
		{
			font = gcnew XFontEx
				(
				f, 
				gcnew XPdfFontOptions(PdfFontEncoding::Unicode)
				);

			if (lf->lfOrientation != 0)
			{
				font->Orientation = lf->lfOrientation / 10;	//decimi di grado
			}
		}
		catch (Exception^ e)
		{
			CString strFontNameMessage = cwsprintf(_TB("Error creating font {0-%s} \n"), lf->lfFaceName);
			AfxGetDiagnostic()->Add(gcnew String(strFontNameMessage));
			AfxGetDiagnostic()->Add(e->ToString());
			throw gcnew ApplicationException(gcnew String(strFontNameMessage));
		}

		return font;
	}

	/*Vecchia gestione dei font in caso di fallimento nella creazioen del font managed*/
	//---------------------------------------------------------------------------------
	XFont^ FromLogFontOld(LPLOGFONT lf)
	{
		XFontStyle style = XFontStyle::Regular;
		if (lf->lfItalic)
			style = style | XFontStyle::Italic;
		if (lf->lfUnderline)
			style = style | XFontStyle::Underline;
		if (lf->lfStrikeOut)
			style = style | XFontStyle::Strikeout;
		if (lf->lfWeight > 400) //heuristic: what's the real bold weight? 
			style = style | XFontStyle::Bold;

		XFontEx^ font = nullptr;		
		try {
			font = gcnew XFontEx
				(
				gcnew String(lf->lfFaceName), 
				Math::Abs(ScaleFontHeight(lf)),
				style, 
				gcnew XPdfFontOptions(PdfFontEncoding::Unicode)				);

			if (lf->lfOrientation != 0)
			{
				font->Orientation = lf->lfOrientation / 10;	//decimi di grado
			}
		}
		catch (Exception^ e)
		{
			CString strFontNameMessage = cwsprintf(_TB("Error creating font {0-%s} \n"), lf->lfFaceName);
			AfxGetDiagnostic()->Add(gcnew String(strFontNameMessage));
			AfxGetDiagnostic()->Add(e->ToString());
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Font not supported: {0-%s}. {1-%s}"), lf->lfFaceName, CString(e->Message)), CDiagnostic::Warning);
		}

		return font;
	}

	Object^ GetStockObject(DWORD ihObject)
	{
		DWORD handle = (ihObject & ~ENHMETA_STOCK_OBJECT);
		switch (handle)
		{
			case WHITE_BRUSH: return XBrushes::White;
			case LTGRAY_BRUSH: return XBrushes::LightGray;
			case GRAY_BRUSH: return XBrushes::Gray;
			case DKGRAY_BRUSH: return XBrushes::DarkGray;
			case BLACK_BRUSH: return XBrushes::Black;
			case NULL_BRUSH: return XBrushes::Transparent;
			case WHITE_PEN : return XPens::White;
			case BLACK_PEN : return XPens::Black;
			case NULL_PEN : return XPens::Transparent;
			
			case OEM_FIXED_FONT : 
			case ANSI_FIXED_FONT :
			case ANSI_VAR_FONT :
			case SYSTEM_FONT :
			case DEVICE_DEFAULT_FONT :
				{
					CFont f;
					f.CreateStockObject(handle);
					LOGFONT lf;
					f.GetLogFont(&lf);
					return FromLogFont(&lf);
				}
			
			/*case DEFAULT_PALETTE     15
			case SYSTEM_FIXED_FONT   16*/
		}
		return nullptr;
	}
	
	bool Is2Rotate(XFont^ f, int& angle)
	{
		if (f->GetType() == XFontEx::typeid)
		{
			angle = - ((XFontEx^)f)->Orientation;
		}
		else
			angle = 0; //-90; //(3.0 / 2.0) * 3.14; //1.57;
		return angle != 0;
	}

	void DrawString(String^ s, System::Drawing::Rectangle rect)
	{
		if (String::IsNullOrEmpty(s))
			return;
		
		XRect r (Scale(rect));

		//haveToRotate
//XRect ro (r);
		XPoint center = r.Center;
		int angle = 0;
		bool haveToRotate = Is2Rotate(CurrentFont, angle);
		if (haveToRotate)
		{
			XRect rr (r.Center.X - r.Height / 2, r.Center.Y - r.Width / 2, r.Height, r.Width);
			if (abs(angle) != 90 && abs(angle) != 270)
				r.Union(rr);
			else
				r = rr;
		}
		//----

		XSize sz = Graphics->MeasureString(s, CurrentFont);
		
		//adatto il testo al rettangolo che mi viene passato:
		bool left = false;
		while (sz.Width > (r.Width * 1.10) )
		{
			switch(TextAlign->Alignment)
			{
				/*caso allineamento center*/
				//tolgo un carattere a destra e a sinistra alternativamente
				//fino a che entra nel rettangolo
				case (XStringAlignment::Center):
					{
						s = left 
							? s->Substring(1, s->Length - 1) 
							: s->Substring(0, s->Length - 1);
						break;
					}
				/*caso allineamento left*/	
				//tolgo un carattere a destra fino a che entra nel rettangolo
				case (XStringAlignment::Near):
					{
						s = s->Substring(0, s->Length - 1);
						break;
					}
				/*caso allineamento right*/
				//tolgo un carattere a sinistra fino a che entra nel rettangolo
				case (XStringAlignment::Far):
					{
						s = s->Substring(1, s->Length - 1);
						break;
					}
			}
			if (String::IsNullOrEmpty(s))
				return;
			sz = Graphics->MeasureString(s, CurrentFont);
			left = !left;
		}

		//haveToRotate
		XGraphicsState^ state = nullptr;
		if (haveToRotate)
		{
			state = Graphics->Save();

			Graphics->RotateAtTransform(angle, center);

//Graphics->DrawRectangle(gcnew XPen(Color::Red), ro);
//Graphics->DrawRectangle(gcnew XPen(Color::Blue), r);
		}
		//----

		Graphics->DrawString
			(
			s, 
			CurrentFont, 
			gcnew XSolidBrush(TextColor), 
			r, 
			TextAlign
			);

		if (haveToRotate && state != nullptr)
		{
			Graphics->Restore(state);
		}
	}
};

//System::Collections::Generic::List<XImageCache^>^	PdfCache::lstXImages = nullptr;

XColor FromColorRef(COLORREF color)
{
	return XColor::FromArgb(GetRValue(color),GetGValue(color),GetBValue(color));
}


System::Drawing::Rectangle FromRECTL(RECTL r)
{
	//I dont know why, rectangle is one pixel sharper in width and height, so I add 1
	return System::Drawing::Rectangle (r.left, r.top, 1 + r.right - r.left,  1 + r.bottom - r.top);		
}

System::Drawing::Size FromSIZEL(SIZEL sz)
{
	//I dont know why, rectangle is one pixel sharper in width and height, so I add 1
	return System::Drawing::Size (sz.cx + 1, sz.cy + 1);		
}

System::Drawing::Point FromPOINTL(POINTL pt)
{
	return System::Drawing::Point (pt.x, pt.y);		
}
String^ FromMetaRecordW(CONST ENHMETARECORD *lpEMFR, int offset, int count)
{
	LPBYTE pByte = (BYTE*) lpEMFR;
	CStringW s = CStringW((LPWSTR)(pByte + offset), count);
	return gcnew String(s);
}
String^ FromMetaRecordA(CONST ENHMETARECORD *lpEMFR, int offset, int count)
{
	LPBYTE pByte = (BYTE*) lpEMFR;
	CStringA s = CStringA((LPSTR)(pByte + offset), offset);
	return gcnew String(s);
}

//siccome PDFSharp non supporta i 16 bit, devo adattare l'immagine
Bitmap^ AdjustBitDepth(Bitmap^ image)
{
	Bitmap^ newImage = nullptr;
	switch (image->PixelFormat)
	{
	case PixelFormat::Format16bppRgb555:
		newImage = image->Clone(System::Drawing::Rectangle(0, 0, image->Width, image->Height), PixelFormat::Format32bppRgb);
		break;
	case PixelFormat::Format16bppRgb565:
		newImage = image->Clone(System::Drawing::Rectangle(0, 0, image->Width, image->Height), PixelFormat::Format32bppRgb);
		break;
	case PixelFormat::Format16bppArgb1555:
		newImage = image->Clone(System::Drawing::Rectangle(0, 0, image->Width, image->Height), PixelFormat::Format32bppArgb);
		break;
	}

	if (newImage == nullptr)
		return image;
	
	delete image;
	return newImage;
}

XImage^ FromMetaRecord(PdfCache^ cache, BITMAPINFO* pInfo, BYTE* bitmapBytes)
{
	//creo l'oggetto Bitmap GDI PLUS; la distruzione verra` fatta dalla dispose di XImage
	Gdiplus::GpBitmap *gpBitmap = NULL;
	Gdiplus::DllExports::GdipCreateBitmapFromGdiDib(pInfo, bitmapBytes, &gpBitmap);

	//---------
	System::Reflection::MethodInfo^ mi = Bitmap::typeid->GetMethod("FromGDIplus", System::Reflection::BindingFlags::Static | System::Reflection::BindingFlags::NonPublic);
	//creo l'oggetto Bitmap managed a partire al precedente
	Bitmap ^image = (Bitmap^)mi->Invoke(nullptr, gcnew cli::array<Object^> { IntPtr(gpBitmap) });
	
	//---------
	array<System::Byte>^ btImage1 = gcnew array<System::Byte>(1);
    btImage1 = (array<System::Byte>^) cache->m_pWrpPdfDocument->m_imgConverter->ConvertTo(image, btImage1->GetType());

	array<System::Byte>^ hash1 = cache->m_pWrpPdfDocument->m_shaM->ComputeHash((array<System::Byte>^)btImage1);

	//for each(XImageCache^ xc in cache->m_pWrpPdfDocument->m_lstXImages)
	for (int j = 0; j < cache->m_pWrpPdfDocument->m_lstXImages->Count; j++)
	{
		//XImageCache^ xc = cache->m_pWrpPdfDocument->m_lstXImages[j];
		XImageCache^ xc = cache->m_pWrpPdfDocument->m_lstXImages->default[j];

		if (xc->EqualHash(hash1))
		{
			delete hash1;
			delete btImage1;
			delete image;
			//no delete gpBitmap;

			return xc->image;
		}
	}

	//---------
	//aggiusto la risoluzione
	image = AdjustBitDepth(image);
	
	//aggancio l'oggetto XImage
	XImage^ xImage = XImage::FromGdiPlusImage(image);

	XImageCache^ xcNew = gcnew XImageCache(xImage, hash1);
	cache->m_pWrpPdfDocument->m_lstXImages->Add(xcNew);

	return xImage;
}


//motodone che mappa ogni record del metafile nelle corrispondenti istruzioni PDF
//attualmente non tutte sono supportate, solo quelle usate in WOORM
int CALLBACK EnhMetaFileProc(
  HDC hDC,                      // handle to DC
  HANDLETABLE *lpHTable,        // metafile handle table
  CONST ENHMETARECORD *lpEMFR,  // metafile record
  int nObj,                     // count of objects
  LPARAM lpData                 // optional data
  )
{
	try
	{
		PdfCache^ cache = *((PdfCache^*)lpData);
		XGraphics^ xg = cache->Graphics;

		// Which record type is this record?
		switch( lpEMFR->iType )
		{
			UNSUPPORTED(EMR_ABORTPATH);
			UNSUPPORTED(EMR_ALPHABLEND);
			UNSUPPORTED(EMR_ANGLEARC);
			UNSUPPORTED(EMR_ARC);
			UNSUPPORTED(EMR_ARCTO);
			UNSUPPORTED(EMR_BEGINPATH);
			case EMR_BITBLT: 
				{
					EMRBITBLT* p = (EMRBITBLT*)lpEMFR;
					
					if (p->cbBitsSrc)
					{
						BYTE* pb = (BYTE*) p;
						BITMAPINFO* pInfo = (BITMAPINFO*)(pb + p->offBmiSrc);
						BYTE* bitmapBytes = pb + p->offBitsSrc;

						XImage^ xImage = FromMetaRecord(cache, pInfo, bitmapBytes);

						cache->Graphics->DrawImage(xImage, cache->Scale(FromRECTL(p->rclBounds)));
						//delete xImage;
					}
					else
					{
						XRect xRect = cache->Scale(FromRECTL(p->rclBounds));
						//corresponds to DC.FillRect operation
						xg->DrawRectangle(cache->CurrentBrush, xRect);
					}
					break;
				}
			UNSUPPORTED(EMR_CHORD);
			UNSUPPORTED(EMR_CLOSEFIGURE);
			UNSUPPORTED(EMR_COLORCORRECTPALETTE);
			UNSUPPORTED(EMR_COLORMATCHTOTARGETW);
			case EMR_CREATEBRUSHINDIRECT: 
				{
					EMRCREATEBRUSHINDIRECT* p = (EMRCREATEBRUSHINDIRECT*)lpEMFR;
					XColor color = FromColorRef(p->lb.lbColor);
					XBrush^ brush = gcnew XSolidBrush(color);
					cache->Objects[p->ihBrush] = brush;
					break;
				}
			UNSUPPORTED(EMR_CREATECOLORSPACE); 
			UNSUPPORTED(EMR_CREATECOLORSPACEW);
			case EMR_CREATEDIBPATTERNBRUSHPT: 
				{
					EMRCREATEDIBPATTERNBRUSHPT* p = (EMRCREATEDIBPATTERNBRUSHPT*)lpEMFR;
					if (p->cbBits)
					{	
						/*BYTE* pb = (BYTE*) p;
						BITMAPINFO* pInfo = (BITMAPINFO*)(pb + p->offBmi);
						BYTE* bitmapBytes = pb + p->offBits;
						XImage^ xImage = FromMetaRecord(pInfo, bitmapBytes);
						XTextureBrush^ b = gcnew XTextureBrush(xImage);*/
					}
					//soluzione subottima: PDFSharp non supporta i TextureBrush, quindi 
					//uso un brush solido (meglio che schiantarsi...)
					cache->Objects[p->ihBrush] = gcnew XSolidBrush(Color::Black);
					break;
				}
			UNSUPPORTED(EMR_CREATEMONOBRUSH);
			UNSUPPORTED(EMR_CREATEPALETTE);
			case EMR_CREATEPEN: 
				{
					EMRCREATEPEN* p = (EMRCREATEPEN*)lpEMFR;
	   
					XColor color = FromColorRef(p->lopn.lopnColor);
					XPen^ pen = gcnew XPen(color, cache->Scale(p->lopn.lopnWidth.x));
					pen->DashStyle = (PdfSharp::Drawing::XDashStyle)p->lopn.lopnStyle;
					cache->Objects[p->ihPen] = pen;
					break;
				}
			UNSUPPORTED(EMR_DELETECOLORSPACE);
			case EMR_DELETEOBJECT: 
				{
					EMRDELETEOBJECT* p = (EMRDELETEOBJECT*)lpEMFR;
					Object^ obj = cache->Objects[p->ihObject];
					if (obj != nullptr)
						delete obj;//chiama la dispose
					cache->Objects[p->ihObject] = nullptr;
					break;
				}

			//UNSUPPORTED(EMR_ELLIPSE);
			case EMR_ELLIPSE:
			{
				EMRELLIPSE* p = (EMRELLIPSE*)lpEMFR;
				XRect xRect = cache->Scale(FromRECTL(p->rclBox));
				xg->DrawEllipse(cache->CurrentPen, xRect);
				break;
			}

			UNSUPPORTED(EMR_ENDPATH);
			case EMR_EOF: 
				{
					EMREOF* p = (EMREOF*)lpEMFR;
					break;
				}
			case EMR_EXCLUDECLIPRECT:
				{
					EMREXCLUDECLIPRECT* p = (EMREXCLUDECLIPRECT*)lpEMFR;
					//TODOPERASSO
					break;
				}
			case EMR_EXTCREATEFONTINDIRECTW: 
				{
					EMREXTCREATEFONTINDIRECTW* p = (EMREXTCREATEFONTINDIRECTW*)lpEMFR;
					cache->Objects[p->ihFont] = cache->FromLogFont(&p->elfw.elfLogFont);
					break;
				}
			UNSUPPORTED(EMR_EXTCREATEPEN);
			UNSUPPORTED(EMR_EXTFLOODFILL);
			case EMR_EXTSELECTCLIPRGN:
				{
					EMREXTSELECTCLIPRGN* p = (EMREXTSELECTCLIPRGN*)lpEMFR;
					if (p->cbRgnData == 0)
						break;

					CRgn rgn;
					VERIFY(rgn.CreateFromData(NULL, p->cbRgnData, (RGNDATA *) p->RgnData));
					CRect rect;
					rgn.GetRgnBox(&rect);
					
					//cache->SetPageSize(Scale(rect), Replace);
					break;
				}
			case EMR_EXTTEXTOUTA: 
				{
					EMREXTTEXTOUTA* p = (EMREXTTEXTOUTA*) lpEMFR;
					EMRTEXT pEmrText = p->emrtext;
					
					System::Drawing::Rectangle rect = FromRECTL(p->rclBounds);
					String^ s = FromMetaRecordA(lpEMFR, pEmrText.offString, pEmrText.nChars);
					if (s->Trim()->Length > 0)
						cache->DrawString (s, rect);
					break;
				}
			UNSUPPORTED(EMR_FILLPATH);
			UNSUPPORTED(EMR_FILLRGN);
			UNSUPPORTED(EMR_FLATTENPATH);
			UNSUPPORTED(EMR_FRAMERGN);
			case EMR_GDICOMMENT:
				{
					EMRGDICOMMENT* p = (EMRGDICOMMENT*)lpEMFR;
					//non fa nulla, ecco uno stralcio di documentazione Microsoft:
					//The EMRGDICOMMENT structure contains application-specific data.
					//This enhanced metafile record is only meaningful to applications that know the format of the data and how to utilize it.
					//This record is ignored by graphics device interface (GDI) during playback of the enhanced metafile.
					break;
				}
			UNSUPPORTED(EMR_GLSBOUNDEDRECORD);
			UNSUPPORTED(EMR_GLSRECORD);
			UNSUPPORTED(EMR_GRADIENTFILL);
			case EMR_HEADER: 
				{
					ENHMETAHEADER* p = (ENHMETAHEADER*)lpEMFR;

					System::Drawing::Rectangle ^rectl = FromRECTL (p->rclFrame);

					//----
					//if (cache->MeasurePage)
						cache->SetPageSize(rectl->Width, rectl->Height);

					cache->ScaleFactor = (float)p->szlDevice.cx / (float)p->szlMillimeters.cx;
					
					String^ desc = FromMetaRecordW(lpEMFR, p->offDescription, p->nDescription);
					cache->Document->Info->Title = desc;
					break;
				}
			case EMR_INTERSECTCLIPRECT: 
				{
					EMRINTERSECTCLIPRECT* p = (EMRINTERSECTCLIPRECT*)lpEMFR;
					//cache->SetPageSize(Scale(FromRECTL (p->rclClip)), Intersect);
					break;
				}
			UNSUPPORTED(EMR_INVERTRGN);
			case EMR_LINETO:
				{
					EMRLINETO* p = (EMRLINETO*)lpEMFR;
					XPoint xp = cache->Scale(FromPOINTL(p->ptl));
					xg->DrawLine(cache->CurrentPen, cache->CurrentPoint, xp);
					cache->CurrentPoint = xp;
					break;
				}
			UNSUPPORTED(EMR_MASKBLT);
			UNSUPPORTED(EMR_MODIFYWORLDTRANSFORM);
			case EMR_MOVETOEX:
				{
					EMRMOVETOEX* p = (EMRMOVETOEX*)lpEMFR;
					cache->CurrentPoint = cache->Scale(FromPOINTL(p->ptl));
					break;
				}
			UNSUPPORTED(EMR_OFFSETCLIPRGN);
			UNSUPPORTED(EMR_PAINTRGN);
			UNSUPPORTED(EMR_PIE);
			UNSUPPORTED(EMR_PIXELFORMAT);
			UNSUPPORTED(EMR_PLGBLT);
			UNSUPPORTED(EMR_POLYBEZIER);
			UNSUPPORTED(EMR_POLYBEZIER16);
			UNSUPPORTED(EMR_POLYBEZIERTO);
			UNSUPPORTED(EMR_POLYBEZIERTO16);
			UNSUPPORTED(EMR_POLYDRAW);
			UNSUPPORTED(EMR_POLYDRAW16);
			UNSUPPORTED(EMR_POLYGON);
			UNSUPPORTED(EMR_POLYGON16);
			UNSUPPORTED(EMR_POLYLINE);
			UNSUPPORTED(EMR_POLYLINE16);
			UNSUPPORTED(EMR_POLYLINETO);
			UNSUPPORTED(EMR_POLYLINETO16);
			UNSUPPORTED(EMR_POLYPOLYGON);
			UNSUPPORTED(EMR_POLYPOLYGON16);
			UNSUPPORTED(EMR_POLYPOLYLINE);
			UNSUPPORTED(EMR_POLYPOLYLINE16);
			UNSUPPORTED(EMR_POLYTEXTOUTA);
			UNSUPPORTED(EMR_POLYTEXTOUTW);
			UNSUPPORTED(EMR_REALIZEPALETTE);
			UNSUPPORTED(EMR_RESIZEPALETTE);

			case EMR_RESTOREDC: 
				{
					EMRRESTOREDC* p = (EMRRESTOREDC*) lpEMFR;
					cache->RestoreFromStream(p->iRelative);
					break;
				}

			case EMR_RECTANGLE: 
				{
					EMRRECTANGLE* p = (EMRRECTANGLE*) lpEMFR;
					
					XRect xRect = cache->Scale(FromRECTL(p->rclBox));
					xg->DrawRectangle(cache->CurrentPen, cache->CurrentBrush, xRect);

					break;
				}
			case EMR_ROUNDRECT: 
				{
					EMRROUNDRECT* p = (EMRROUNDRECT*) lpEMFR;
					
					XRect xRect = cache->Scale(FromRECTL(p->rclBox));
					XSize xEllipseSize = cache->Scale(FromSIZEL(p->szlCorner));  //Begin An. 19184
					if (xEllipseSize.Width > xRect.Width)
						xEllipseSize.Width = xRect.Width;
					if (xEllipseSize.Height > xRect.Height)
						xEllipseSize.Height = xRect.Height;						//End An. 19184

					xg->DrawRoundedRectangle(cache->CurrentPen, cache->CurrentBrush, xRect, xEllipseSize);

					break;
				}
			case EMR_SAVEDC: 
				{
					EMRSAVEDC* p = (EMRSAVEDC*) lpEMFR;
					
					cache->SaveToStream();
					break;
				}

			case EMR_SETMAPMODE:
				{
					//SCALING
					EMRSETMAPMODE* p = (EMRSETMAPMODE*) lpEMFR;
					cache->MapMode = p->iMode;
					break;
				}
			case EMR_SCALEVIEWPORTEXTEX:
				{
					EMRSCALEVIEWPORTEXTEX* p = (EMRSCALEVIEWPORTEXTEX*) lpEMFR;
					//TODO SCALING

					//cache->Graphics->ScaleTransform((double)p->xNum / p->xDenom);
					break;
				}
			case EMR_SCALEWINDOWEXTEX: 
				{
					EMRSCALEWINDOWEXTEX* p = (EMRSCALEWINDOWEXTEX*) lpEMFR;
					//TODO SCALING

					//cache->Graphics->ScaleTransform(p->xNum / p->xDenom);
					break;
				}
			case EMR_SETVIEWPORTORGEX:
				{
					EMRSETVIEWPORTORGEX* p = (EMRSETVIEWPORTORGEX*) lpEMFR;
					//TODO SCALING

					break;
				}

			UNSUPPORTED(EMR_SELECTCLIPPATH);
			case EMR_SELECTOBJECT: 
				{
					EMRSELECTOBJECT* p = (EMRSELECTOBJECT*) lpEMFR;
					Object^ o = nullptr;
					if (p->ihObject & ENHMETA_STOCK_OBJECT)
						o = cache->GetStockObject(p->ihObject);
					else
						o = cache->Objects[p->ihObject];
					
					if (o == nullptr)
						throw gcnew ArgumentException(String::Format("Invalid object: {0}", p->ihObject));
					
					if (XFont::typeid->IsInstanceOfType(o))
					{
						cache->CurrentFont = (XFont^)o;

					}
					else if (XBrush::typeid->IsInstanceOfType(o))
						cache->CurrentBrush = (XBrush^)o;
					else if (XPen::typeid->IsInstanceOfType(o))
						cache->CurrentPen = (XPen^)o;
					break;
				}
			UNSUPPORTED(EMR_SELECTPALETTE);
			UNSUPPORTED(EMR_SETARCDIRECTION);
			case EMR_SETBKCOLOR:
				{
					EMRSETBKCOLOR* p = (EMRSETBKCOLOR*) lpEMFR;
					cache->BackColor = FromColorRef(p->crColor);
					break;
				}
			case EMR_SETBKMODE:
				{
					EMRSETBKMODE* p = (EMRSETBKMODE*) lpEMFR;
					cache->BackMode = p->iMode;
					break;
				}
			UNSUPPORTED(EMR_SETBRUSHORGEX);
			UNSUPPORTED(EMR_SETCOLORADJUSTMENT);
			UNSUPPORTED(EMR_SETCOLORSPACE);
			case EMR_SETICMMODE:
				{
					EMRSETICMMODE* p = (EMRSETICMMODE*) lpEMFR;
					//TODOPERASSO
					break;
				}
			UNSUPPORTED(EMR_SETICMPROFILEA);
			UNSUPPORTED(EMR_SETICMPROFILEW);

			UNSUPPORTED(EMR_SETLAYOUT);
			UNSUPPORTED(EMR_SETMETARGN);
			UNSUPPORTED(EMR_SETPOLYFILLMODE);
			UNSUPPORTED(EMR_SETROP2);
			//case EMR_SETROP2:
			//case EMR_SETPOLYFILLMODE:
			//case EMR_SETMETARGN:
			//case EMR_SETLAYOUT:
			//	{
			//		//TODO
			//		break;
			//	}

			UNSUPPORTED(EMR_SETMAPPERFLAGS);
			UNSUPPORTED(EMR_SETMITERLIMIT);
			UNSUPPORTED(EMR_SETPALETTEENTRIES);
			UNSUPPORTED(EMR_SETPIXELV);
			case EMR_SETSTRETCHBLTMODE:
				{
					EMRSETSTRETCHBLTMODE* p = (EMRSETSTRETCHBLTMODE*) lpEMFR;
					//TODOPERASSO
					break;
				}
			case EMR_SETTEXTALIGN:
				{
					EMRSETTEXTALIGN* p = (EMRSETTEXTALIGN*) lpEMFR;
					if ((p->iMode & TA_LEFT) == TA_LEFT)
						cache->TextAlign->Alignment = XStringAlignment::Near;
					if ((p->iMode & TA_CENTER) == TA_CENTER)
						cache->TextAlign->Alignment = XStringAlignment::Center;
					if ((p->iMode & TA_RIGHT) == TA_RIGHT)
						cache->TextAlign->Alignment = XStringAlignment::Far;

					if ((p->iMode & TA_TOP) == TA_TOP)
						cache->TextAlign->LineAlignment = XLineAlignment::Near;
					if ((p->iMode & TA_BASELINE) == TA_BASELINE)
						cache->TextAlign->LineAlignment = XLineAlignment::BaseLine;
					if ((p->iMode & TA_BOTTOM) == TA_BOTTOM)
						cache->TextAlign->LineAlignment = XLineAlignment::Far;
					break;
				}
			case EMR_SETTEXTCOLOR:
				{
					EMRSETTEXTCOLOR* p = (EMRSETTEXTCOLOR*) lpEMFR;
					cache->TextColor = FromColorRef(p->crColor);
					break;
				}
			UNSUPPORTED(EMR_SETVIEWPORTEXTEX);
			UNSUPPORTED(EMR_SETWINDOWEXTEX);
			UNSUPPORTED(EMR_SETWINDOWORGEX);
			UNSUPPORTED(EMR_SETWORLDTRANSFORM);
			case EMR_STRETCHBLT:
				{
					EMRSTRETCHBLT* p = (EMRSTRETCHBLT*) lpEMFR;
					//TODOPERASSO
					break;
				}
			case EMR_STRETCHDIBITS:
				{
					EMRSTRETCHDIBITS* p = (EMRSTRETCHDIBITS*) lpEMFR;
					
					if (p->cbBitsSrc)
					{	
						BYTE* pb = (BYTE*) p;
						BITMAPINFO* pInfo = (BITMAPINFO*)(pb + p->offBmiSrc);
						BYTE* bitmapBytes = pb + p->offBitsSrc;

						XImage^ xImage = FromMetaRecord(cache, pInfo, bitmapBytes);

						cache->Graphics->DrawImage(xImage, cache->Scale(FromRECTL(p->rclBounds)));
						//delete xImage;
					}
					break;
				}
			UNSUPPORTED(EMR_STROKEANDFILLPATH);
			UNSUPPORTED(EMR_STROKEPATH);
			UNSUPPORTED(EMR_TRANSPARENTBLT);
			UNSUPPORTED(EMR_WIDENPATH);
			case EMR_EXTTEXTOUTW:
				{
					EMREXTTEXTOUTW* p = (EMREXTTEXTOUTW*) lpEMFR;
					EMRTEXT pEmrText = p->emrtext;
					
					System::Drawing::Rectangle rect = FromRECTL(p->rclBounds);
					String^ s = FromMetaRecordW(lpEMFR, pEmrText.offString, pEmrText.nChars);
					if (s->Trim()->Length > 0)
						cache->DrawString (s, rect);
					break;
				}
			default:
				ASSERT(FALSE);
			break; 
		}
		// Return non-zero to continue enumeration
		return 1;
	}
	catch (Exception^ e)
	{
		AfxGetDiagnostic()->Add(e->Message);
		AfxGetDiagnostic()->Add(_TB("Cannot generate PDF file"));
		return 0;
	}
}

void ApplySecuritySettings(PdfSecuritySettings^ securitySettings, const CString& strPassword)
{
	// Setting one of the passwords automatically sets the security level to 
	// PdfDocumentSecurityLevel.Encrypted128Bit.
	securitySettings->OwnerPassword = gcnew String(strPassword);
	securitySettings->PermitAccessibilityExtractContent = false;
	securitySettings->PermitAnnotations = false;
	securitySettings->PermitAssembleDocument = false;
	securitySettings->PermitFormsFill = false;
	securitySettings->PermitModifyDocument = false;
	securitySettings->PermitExtractContent = true;
	securitySettings->PermitFullQualityPrint = true;
	securitySettings->PermitPrint = true;
}

//-----------------------------------------------------------------------------
BOOL CheckPdf(const CString& strFile)
{
	PdfDocument^ inputDocument = nullptr;
	BOOL bOk = FALSE;

	try
	{
		inputDocument = PdfReader::Open(gcnew String(strFile), PdfDocumentOpenMode::Import);
		bOk = TRUE;
	}
	catch(...)
	{
		;
	}
	finally
	{
		if (inputDocument != nullptr)
			delete inputDocument;
	}
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL ConcatPdf(const CStringArray& strDocs, const CString& strDestination, const CString& strPassword /*= _T("")*/)
{
	PdfDocument^ document = gcnew PdfDocument();
	PdfDocument^ inputDocument = nullptr;

	int c = 0;
	for (int i = 0; i < strDocs.GetCount(); i++)
	{
		try
		{
			inputDocument = PdfReader::Open(gcnew String(strDocs[i]), PdfDocumentOpenMode::Import);

			for (int page = 0; page < inputDocument->PageCount; page++)
				document->AddPage(inputDocument->Pages[page]);

			delete inputDocument;
			c++;
		}
		catch (System::Exception^ e)
		{
			AfxGetDiagnostic()->Add(e->ToString());

			if (inputDocument != nullptr)
				delete inputDocument;
		}
	}

	if (c > 0)
	{
		if (!strPassword.IsEmpty())
			ApplySecuritySettings(document->SecuritySettings, strPassword);
	
		document->Save(gcnew String(strDestination));
	}

	delete document;

	return c == (strDocs.GetCount() - 1);
}

//-----------------------------------------------------------------------------
int GetPdfPageNumber(const CString& strDoc, const CString& /*strPassword = _T("")*/)
{
	PdfDocument^ document = PdfReader::Open(gcnew String(strDoc), PdfDocumentOpenMode::Import);
	int pages = document->PageCount;
	delete document;
	return pages;
}

//-----------------------------------------------------------------------------
CString NumerateFileName(const CString fileName, int number, int extensionLenght)
{
	CString fileNameNumerated(fileName);
	CString blockNumber;
	blockNumber.Format(L"_%d", number);

	//inserisco la "numerazione" prima del punto (+ 1) precedente l'estensione
	fileNameNumerated.Insert(fileNameNumerated.GetLength() - (extensionLenght + 1), blockNumber);
	return fileNameNumerated;
}

//-----------------------------------------------------------------------------
CString GenerateUniqueFileName()
{
	char name[L_tmpnam_s];
	errno_t err;

	err = tmpnam_s(name, L_tmpnam_s);
	if (err)
	{
		AfxMessageBox(_TB("Error creating temp file."));
		return CString("");
	}

	CString returnedString(name);
	return returnedString;
}

//=============================================================================

CPdfDocumentWrapper::CPdfDocumentWrapper()
	:
m_nPagesWithNotFlushedMemory(0)
{
	m_pDocument = new CInternalPdfDocumentWrapper();
}
CPdfDocumentWrapper::~CPdfDocumentWrapper()
{
	delete m_pDocument;
}

void CPdfDocumentWrapper::AddPageFromMetafile
	(
		HENHMETAFILE handle, 
		const CRect& margins, 
		bool bPrinterDC, 
		double scale /*= 1.0*/,
		int	originalMarginX, 
		int rotateAngle /*= 0*/,
		int mmPageWidth /*= 0*/, 
		int mmPageHeight/* = 0*/,
		BOOL invertedOrientation /*= FALSE*/
	//	LOGFONT	lfDefault
	)
{
	//nota: ogni pagina deve avere la sua cache
	PdfCache^ cache = gcnew PdfCache(m_pDocument, m_pDocument->GetPdfDocument());
	cache->PrinterDC = bPrinterDC;
	
	//inversione dell'orientamento del layout corrente rispetto a quello generale
	if (rotateAngle && invertedOrientation)
	{
		int y = mmPageWidth - margins.left - margins.right;
		cache->Graphics->TranslateTransform(0, XUnit::FromMillimeter(y));
		cache->Graphics->RotateTransform(rotateAngle);	
	}

	cache->Page->TrimMargins->Left = XUnit::FromMillimeter(margins.left);
	cache->Page->TrimMargins->Top = XUnit::FromMillimeter(margins.top);
	cache->Page->TrimMargins->Right = XUnit::FromMillimeter(margins.right);
	cache->Page->TrimMargins->Bottom = XUnit::FromMillimeter(margins.bottom);

	if (scale != 1.0)
		cache->Graphics->ScaleTransform(scale);

	if (rotateAngle && !invertedOrientation)
	{
		int deltaX;
		//--------------------------------------------
		cache->Graphics->RotateTransform(rotateAngle);
		//--------------------------------------------
		deltaX = -int((mmPageWidth)* scale) + originalMarginX;
		//--------------------------------------------
		cache->Graphics->TranslateTransform(XUnit::FromMillimeter(deltaX), 0);
		//--------------------------------------------
	}
	//---- Enumerate enhanced MetaFile's records and callback to EnhMetaFileProc
	::EnumEnhMetaFile(NULL, handle, EnhMetaFileProc, &cache, CRect(0, 0, 0, 0));

	//todo verificare se va messo prima dell'EnumEnhMetaFile(...)
	if (mmPageWidth && mmPageHeight) 
	{
		cache->SetPageSizeWithMargin(mmPageWidth, mmPageHeight);
	}

	//ogni tot pagine effettuo un flush della memoria per evitare problemi di out of memory
	m_nPagesWithNotFlushedMemory++;
	if (m_nPagesWithNotFlushedMemory > 50)
	{
		MemoryManagement::Flush();
		m_nPagesWithNotFlushedMemory = 0;
	}
}

void CPdfDocumentWrapper::Save (
				const CString& strPath, 
				UINT nCopies /*= 1*/,
				const CString& strPassword /*= _T("")*/,
				BOOL bOpen /*= FALSE*/
				)
{
	try
	{
		PdfDocument ^ document = m_pDocument->GetPdfDocument();
		bool ownsDocument = false;
		if (nCopies > 1)
		{
			//apro uno stream di appoggio dove memorizzo il documento di origine
			MemoryStream^ ms = gcnew MemoryStream();
			document->Save(ms, false);//tiene lo stream aperto
			document->Close();

			//creo il documento di destinazione che conterra la multicopia
			document = gcnew PdfDocument();
			ms->Seek(0, SeekOrigin::Begin);//mi metto all'inizio dello stream
			//apro il documento in modalita' PdfDocumentOpenMode::Import per travasarlo piu volte 
			//in quello di destinazione
			PdfDocument^ inputDocument = PdfReader::Open(ms, PdfDocumentOpenMode::Import);
			ms->Close();//chiudo lo stream - non mi serve piu`
			for (UINT cp = 0; cp < nCopies; cp++)
			{
				for (int page = 0; page < inputDocument->PageCount; page++)
					document->AddPage(inputDocument->Pages[page]);
			}
			inputDocument->Close();

			ownsDocument = true;//siccome ho creato io il documento, devo anche chiuderlo
		}

		if (!strPassword.IsEmpty())
			ApplySecuritySettings(document->SecuritySettings, strPassword);
		String^ fileName = gcnew String(strPath);
		document->Save(fileName);
		if (bOpen)
			::ShellExecute(NULL, _T("open"), strPath, NULL, NULL, SW_SHOWNORMAL);

		if (ownsDocument)
		{
			document->Close();
			delete document;//chiama la dispose
		}
	}
	catch(Exception^ e)
	{
		AfxGetDiagnostic()->Add(e->ToString());
		throw new CApplicationErrorException(_TB("Error creating PDF file"));
	}
}