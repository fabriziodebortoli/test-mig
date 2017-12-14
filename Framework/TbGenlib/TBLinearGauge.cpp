#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>

#include "BaseTileDialog.h"
#include "PARSOBJ.H"
#include "TABCORE.H"
#include "TBLinearGauge.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////////////////////////////////
//								CTBGaugeBase implementation
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetFrameSize(int nFrameSize)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetFrameSize(nFrameSize);
}

//-----------------------------------------------------------------------------------------------
int CTBGaugeManager::AddPointer(int nScale /*= 0*/, BOOL bRedraw /*= FALSE*/)
{
	//add pointer
	CBCGPLinearGaugePointer		linearPointer;
	CBCGPCircularGaugePointer	circularPointer;
	int idxPointer = -1;

	if (!m_pGaugeImpl)
		return idxPointer;

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		idxPointer = ((CBCGPLinearGaugeImpl*)m_pGaugeImpl)->AddPointer(linearPointer, nScale, bRedraw);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		idxPointer = ((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->AddPointer(circularPointer, nScale, bRedraw);

	m_bNoPointer = (idxPointer < 0);

	return idxPointer;
}

//--------------------------------------------------------------------------------------------------
void CTBGaugeManager::RemovePointer(int i, BOOL bRedraw /*= FALSE*/)
{
	BOOL bOK = TRUE;

	if (!m_pGaugeImpl)
	{
		m_bNoPointer = bOK;
		return;
	}

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		bOK = ((CBCGPLinearGaugeImpl*)m_pGaugeImpl)->RemovePointer(i, bRedraw);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		bOK = ((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->RemovePointer(i, bRedraw);

	m_bNoPointer = bOK;
}

//-------------------------------------------------------------------------------------------------
void CTBGaugeManager::RemoveAllPointers(BOOL bRedraw /*= FALSE*/)	//do not use this method with Circular Gauge
{
	BOOL bOK = TRUE;

	if (!m_pGaugeImpl)
	{
		m_bNoPointer = bOK;
		return;
	}

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		m_bNoPointer = FALSE;	//circular gauge has 1 pointer always
		return;
	}

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		for (int i = ((CBCGPLinearGaugeImpl*)m_pGaugeImpl)->GetPointersCount() - 1; i >= 0; i--)
			bOK = bOK && ((CBCGPLinearGaugeImpl*)m_pGaugeImpl)->RemovePointer(i, bRedraw);
		
	m_bNoPointer = bOK;
}

//-----------------------------------------------------------------------------------------------
void  CTBGaugeManager::AddColoredRange(double nStartValue, double nFinishValue, COLORREF color /*= -1*/, double nWidth /*= 10.*/)
{
	int nScale = 0;

	CBCGPBrush brFrame;
	
	COLORREF newColor = (color == -1 ? AfxGetThemeManager()->GetTileDialogTitleForeColor() : color);

	CBCGPBrush brColored(newColor);

	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->AddColoredRange(nStartValue, nFinishValue, brColored, brFrame, nScale, nWidth);

}

//-----------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::ModifyRange(int index, double nStartValue, double nFinishValue, COLORREF color /*= -1*/, BOOL bRedraw /*= FALSE*/)
{
	if (!m_pGaugeImpl)
		return;

	CBCGPGaugeColoredRangeObject* pRange = m_pGaugeImpl->GetColoredRange(index);

	if (!pRange)
		return;
	
	pRange->SetRange(nStartValue, nFinishValue);

	if (color != -1)
	{
		CBCGPBrush brColor(color);
		pRange->SetFillBrush(brColor);
	}

	m_pGaugeImpl->ModifyColoredRange(index, *pRange, bRedraw);

	if (m_pGaugeImpl->GetOwner())
		m_pGaugeImpl->GetOwner()->Invalidate();
}

//-------------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::AddFullColoredRange(double nStartValue, double nFinishValue, COLORREF color)
{
	int nScale = 0;

	CBCGPBrush brFrame;
	CBCGPBrush brColored(color);

	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->AddColoredRange(nStartValue, nFinishValue, brColored, brFrame, nScale, 32767);
}

//-----------------------------------------------------------------------------------------------
void CTBGaugeManager::AddColoredRange(double nStartValue, double nFinishValue, COLORREF colorStart, COLORREF colorEnd, LINEAR_GAUGE_GRADIENT_TYPE eGradientType, double nWidth /*= 10.*/)
{
	int nScale = 0;
	
	CBCGPBrush brFrame;
	CBCGPBrush brColored(CBCGPColor(colorStart, m_dOpacity), colorEnd, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);

	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->AddColoredRange(nStartValue, nFinishValue, brColored, brFrame, nScale, nWidth);

}

//----------------------------------------------------------------------------------------------------------
void CTBGaugeManager::AddFullColoredRange(double nStartValue, double nFinishValue, COLORREF colorStart, COLORREF colorEnd, LINEAR_GAUGE_GRADIENT_TYPE eGradientType)
{
	int nScale = 0;
	
	CBCGPBrush brFrame;
	CBCGPBrush brColored(CBCGPColor(colorStart, m_dOpacity), colorEnd, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);

	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->AddColoredRange(nStartValue, nFinishValue, brColored, brFrame, nScale, 32767);
}

//-----------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetFont(CFont* pFont)
{
	LOGFONT lf;

	if (!pFont)
		return;

	pFont->GetLogFont(&lf);

	m_TextFormat.CreateFromLogFont(lf);
	m_TextFormat.SetTextVerticalAlignment(CBCGPTextFormat::BCGP_TEXT_ALIGNMENT_CENTER);
}

//-----------------------------------------------------------------------------------------------
void CTBGaugeManager::AddCustomLabel
							(
								const CString& aLabelText, 
								const COLORREF& aColorText, 
								SUB_GAUGE_POS eSubGaugePos, 
								BOOL bStyleUnderline /*= FALSE*/, 
								BOOL bStyleBold /*= FALSE*/, 
								BOOL bStyleItalic /*= FALSE*/
							)	//use this method with Circular Gauge
{
	
	if (!m_pGaugeImpl || !m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	m_pCustomLabelGauge = new CBCGPTextGaugeImpl(aLabelText, CBCGPColor(aColorText, m_dOpacity));
	CBCGPTextFormat textFormat;

	textFormat = m_TextFormat;

	textFormat.SetFontStyle(bStyleItalic ? CBCGPTextFormat::BCGP_FONT_STYLE_ITALIC : CBCGPTextFormat::BCGP_FONT_STYLE_NORMAL);
	textFormat.SetUnderline(bStyleUnderline);

	if (bStyleBold)
	{
		LOGFONT lf;
		textFormat.ExportToLogFont(lf);
		lf.lfWeight = FW_BOLD;
		textFormat.CreateFromLogFont(lf);
	}

	m_pCustomLabelGauge->SetTextFormat(textFormat);

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->AddSubGauge(m_pCustomLabelGauge, (CBCGPGaugeImpl::BCGP_SUB_GAUGE_POS)eSubGaugePos);
}

////--------------------------------------------------------------------------------------------------------------------------
//void CTBGaugeManager::RemoveSubGauges()
//{
//	if (!m_pGaugeImpl || !m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
//	{
//		ASSERT(FALSE);
//		return;
//	}
//
//	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->RemoveAllSubGauges();
//}

//-------------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetCustomLabelStyle(const CString& aLabelText, const COLORREF& aTextColor, BOOL bStyleUnderline /*= FALSE*/, BOOL bStyleBold /*= FALSE*/, BOOL bStyleItalic /*= FALSE*/)
{
	if (!m_pGaugeImpl || !m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)) || !m_pCustomLabelGauge)
	{
		ASSERT(FALSE);
		return;
	}

	CBCGPTextFormat textFormat = m_pCustomLabelGauge->GetTextFormat();
	textFormat.SetFontStyle(bStyleItalic ? CBCGPTextFormat::BCGP_FONT_STYLE_ITALIC : CBCGPTextFormat::BCGP_FONT_STYLE_NORMAL);
	
	if (bStyleBold)
	{
		LOGFONT lf;
		textFormat.ExportToLogFont(lf);
		lf.lfWeight = FW_BOLD;
		textFormat.CreateFromLogFont(lf);
	}

	textFormat.SetUnderline(bStyleUnderline);
	textFormat.SetTextAlignment(CBCGPTextFormat::BCGP_TEXT_ALIGNMENT::BCGP_TEXT_ALIGNMENT_CENTER);

	m_pCustomLabelGauge->SetText(aLabelText);
	m_pCustomLabelGauge->SetTextFormat(textFormat);
	m_pCustomLabelGauge->SetTextColor(CBCGPColor(aTextColor, m_dOpacity));

}

//----------------------------------------------------------------------------------------------
void CTBGaugeManager::AddCustomImage(const CString& nsImage, SUB_GAUGE_POS eSubGaugePos)
{
	if (!m_pGaugeImpl || !m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	//TODO - immagine non trasparente???
	HBITMAP bitmap = ::LoadBitmapOrPng(nsImage);
	CBCGPImage* pImage = new CBCGPImage(bitmap);
	CBCGPImageGaugeImpl* pSubGauge = new CBCGPImageGaugeImpl();
	pSubGauge->SetImage(*pImage);

	//Cosi' funziona: immagine = trasparente
	//CString ns = _T("C:\\Development40\\Standard\\Applications\\ERP\\Core\\Files\\Images\\Glyph\\Locked.png");
	//CBCGPImage* pImg = new CBCGPImage(ns);
	//CBCGPImageGaugeImpl* pSubGauge = new CBCGPImageGaugeImpl();
	//pSubGauge->SetImage(*pImg);

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->AddSubGauge(pSubGauge, (CBCGPGaugeImpl::BCGP_SUB_GAUGE_POS)eSubGaugePos);

}

//--------------------------------------------------------------------------------------------
void CTBGaugeManager::RemoveAllColoredRanges()
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->RemoveAllColoredRanges();
}

//-----------------------------------------------------------------------------------------------
void CTBGaugeManager::SetGaugeRange(double nStartValue, double nEndValue)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetRange(nStartValue, nEndValue);
}

//--------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetStep(double nStep, int nScale /*= 0*/)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetStep(nStep, nScale);
}

//--------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetMajorTickMarkStep(double nStep, int nScale /*= 0*/)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetMajorTickMarkStep(nStep, nScale);
}

//------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetDirty(BOOL bSet /*= TRUE*/, BOOL bRedraw /*= FALSE*/)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetDirty(bSet, bRedraw);
}

//---------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetValue(double nValue, int nScale /*= 0*/, UINT uiAnimationTime /*= 100*/, BOOL bRedraw /*= FALSE*/)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetValue(nValue, nScale, uiAnimationTime, bRedraw);
}

//-----------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetMajorTickMarkSize(double nSize, int nScale /*= 0*/)
{
	if (!m_pGaugeImpl)
		return;
		
	m_pGaugeImpl->SetTickMarkSize(nSize, TRUE, nScale);
}

//---------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetMinorTickMarkSize(double nSize, int nScale /*= 0*/)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetTickMarkSize(nSize, FALSE, nScale);
}

//-------------------------------------------------------------------------------------------
void CTBGaugeManager::SetTextLabelFormat(const CString& aLabelFormat)
{
	if (!m_pGaugeImpl)
		return;

	m_pGaugeImpl->SetTextLabelFormat(aLabelFormat);
}

//--------------------------------------------------------------------------------------------
void CTBGaugeManager::SetBkgColor(const COLORREF& aBkgColor, double aOpacity /*= 1.0*/)
{
	if (!m_pGaugeImpl)
		return;

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brFill.SetColor(aBkgColor, aOpacity);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brFill.SetColor(aBkgColor, aOpacity);

	SetGaugeColors();
}

//-------------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetBkgGradientColor(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);

	SetGaugeColors();
}

//---------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetFrameBkgColor(const COLORREF& aBkgColor, double aOpacity /*= 1.0*/)
{
	if (!m_pGaugeImpl)
		return;

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brFrameFill.SetColor(aBkgColor, aOpacity);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brFrameFill.SetColor(aBkgColor, aOpacity);

	SetGaugeColors();
}

//-------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetFrameBkgGradientColor(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType)
{
	if (!m_pGaugeImpl)
		return;

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brFrameFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brFrameFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);

	SetGaugeColors();
}

//------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetFrameOutlineColor(const COLORREF& aColor, double opacity)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brFrameOutline.SetColor(aColor, opacity);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brFrameOutline.SetColor(aColor, opacity);

	SetGaugeColors();
}

//----------------------------------------------------------------------------------------
void CTBGaugeManager::SetPointerBkgColor(const COLORREF& aBkgColor)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brPointerFill.SetColor(aBkgColor);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brPointerFill.SetColor(aBkgColor);

	SetGaugeColors();
}

//-----------------------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetPointerBkgGradient(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brPointerFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brPointerFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);

	SetGaugeColors();
}

//--------------------------------------------------------------------------------------
void CTBGaugeManager::SetPointerOutlineColor(const COLORREF& aColor)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brPointerOutline.SetColor(aColor);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brPointerOutline.SetColor(aColor);

	SetGaugeColors();
}

//----------------------------------------------------------------------------------------
void CTBGaugeManager::SetForeColor(const COLORREF& aColor)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brText.SetColor(aColor);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brText.SetColor(aColor);

	SetGaugeColors();
}

//-------------------------------------------------------------------------------------------
void CTBGaugeManager::SetTickMarkBkgColor(const COLORREF& aBkgColor, double aOpacity /*= 1.0*/)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brTickMarkFill.SetColor(aBkgColor, aOpacity);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brTickMarkFill.SetColor(aBkgColor, aOpacity);

	SetGaugeColors();
}

//-------------------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetTickMarkBkgGradientColor(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brTickMarkFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brTickMarkFill.SetColors(aBkgColor, aBkgGradientColor, (CBCGPBrush::BCGP_GRADIENT_TYPE)eGradientType);

	SetGaugeColors();
}

//-------------------------------------------------------------------------------------------
void CTBGaugeManager::SetTickMarkOutlineColor(const COLORREF& aColor)
{
	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		linearColors.m_brTickMarkOutline.SetColor(aColor);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		circularColors.m_brTickMarkOutline.SetColor(aColor);

	SetGaugeColors();
}

//------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetGaugeColors()
{
	if (!m_pGaugeImpl)
		return;

	if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPLinearGaugeImpl)))
		((CBCGPLinearGaugeImpl*)m_pGaugeImpl)->SetColors(linearColors);
	else if (m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
		((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->SetColors(circularColors);
}

//------------------------------------------------------------------------------------------------
void CTBGaugeManager::EnableShapeByTicksArea(BOOL bEnable /*= TRUE*/)	//use this method only with circular Gauge
{
	if (!m_pGaugeImpl)
		return;

	if (!m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->EnableShapeByTicksArea(bEnable);
}

//------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetTicksAreaAngles(double nStartAngle, double nFinishAngle, int nScale /*= 0*/)	//use this method only with circular Gauge
{
	if (!m_pGaugeImpl)
		return;

	if (!m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->SetTicksAreaAngles(nStartAngle, nFinishAngle, nScale);

}

//------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetCapSize(double nSize)	//use this method only with circular Gauge
{
	if (!m_pGaugeImpl)
		return;

	if (!m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->SetCapSize(nSize);

}

//------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetZoom(double nInflateW, double nInflateH, double nWidth, double nHeight)
{
	if (!m_pGaugeImpl)
		return;

	if (!m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	CBCGPSize s;
	s.Inflate(nInflateW, nInflateH);

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->SetScaleRatio(s);

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->SetRect(CBCGPRect(CBCGPPoint(0, 0), CBCGPSize(nWidth, nHeight)));

}

//-----------------------------------------------------------------------------------------------------------------------------
void CTBGaugeManager::SetPointerSize(double nWidth, double nLength, BOOL bExtraLength /*= FALSE*/, int nIdxPointer /*= 0*/)
{
	if (!m_pGaugeImpl)
		return;

	if (!m_pGaugeImpl->IsKindOf(RUNTIME_CLASS(CBCGPCircularGaugeImpl)))
	{
		ASSERT(FALSE);
		return;
	}

	((CBCGPCircularGaugeImpl*)m_pGaugeImpl)->ModifyPointer
	(
		nIdxPointer, 
		CBCGPCircularGaugePointer
		(
			circularColors.m_brPointerFill,
			circularColors.m_brPointerOutline,
			(CBCGPCircularGaugePointer::BCGP_GAUGE_POINTER_STYLE)0,
			.01 * nLength, 
			nWidth, 
			bExtraLength
		), 
		TRUE /* Redraw */
	);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//					class CTBLinearGaugeCtrl implementation
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//==============================================================================================
IMPLEMENT_DYNCREATE(CTBLinearGaugeCtrl, CBCGPLinearGaugeCtrl)

//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBLinearGaugeCtrl, CBCGPLinearGaugeCtrl)
	//{{AFX_MSG_MAP(CTBLinearGaugeCtrl)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//----------------------------------------------------------------------------------------------
CTBLinearGaugeCtrl::CTBLinearGaugeCtrl(const CString sName /*= _T("")*/)
	:
	IDisposingSourceImpl(this)
{
	CParsedCtrl::Attach(this);

	AttachGauge(this->GetGauge());

	m_sName = sName;
}

//TODO = SetValue/GetValue array of DataObj
//------------------------------------------------------------------------------------------------
void CTBLinearGaugeCtrl::SetValue(const DataObj& aValue)
{
	const DataDbl& aDouble = (const DataDbl&) aValue;
	
	if (m_pGauge && !m_bNoPointer)
		m_pGauge->SetValue(aDouble);
	
	Invalidate();
}

//TODO = SetValue/GetValue array of DataObj
//------------------------------------------------------------------------------------------------
void CTBLinearGaugeCtrl::GetValue(DataObj& aValue)
{
	if (m_pGauge && !m_bNoPointer)
		((DataDbl*)m_pData)->Assign(m_pGauge->GetValue(0));
}

//------------------------------------------------------------------------------------------------
BOOL CTBLinearGaugeCtrl::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk = __super::Create(rect, pParentWnd, nID, dwStyle);

	bOk = bOk && 	/*CheckControl(nID, pParentWnd, _T("STATIC")) && TODOBRUNA (picture control al posto di di static)*/
		CreateAssociatedButton(pParentWnd) && CParsedCtrl::InitCtrl();

	return bOk;
}

//------------------------------------------------------------------------------
LRESULT CTBLinearGaugeCtrl::OnRecalcCtrlSize(WPARAM w, LPARAM l)
{
	DoRecalcCtrlSize();
	return 0L;
}

//----------------------------------------------------------------------------------------------
void CTBLinearGaugeCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	GetCtrlParent()->SendMessage(WM_LBUTTONDOWN);
}

//------------------------------------------------------------------------------------------
BOOL CTBLinearGaugeCtrl::SubclassEdit(UINT nID, CWnd* pParent, const CString& sName)
{

	BOOL bOk =
		//CheckControl(nID, pParentWnd, _T("STATIC")) && TODOBRUNA (picture control al posto di di static)
		CBCGPLinearGaugeCtrl::SubclassDlgItem(nID, pParent)  &&
		CParsedCtrl::InitCtrl();

	if (bOk)
		SetNamespace(m_sName);
		
	return bOk;

}

//---------------------------------------------------------------------------------------
BOOL CTBLinearGaugeCtrl::OnInitCtrl()
{
	if (!m_pGauge)
		return FALSE;

	SetFrameSize(1);
	RemoveAllColoredRanges();
	RemoveAllPointers(FALSE);
	if (m_pData)
		AddPointer();

	CTBGaugeManager::SetFont(m_pTBThemeManager->GetTabSelectorButtonFont());

	SetBkgGradientColor		(m_pTBThemeManager->GetLinearGaugeBkgColor(), m_pTBThemeManager->GetLinearGaugeBkgGradientColor(), LINEAR_GAUGE_GRADIENT_DIAGONAL_LEFT);
	//SetFrameOutlineColor	(m_pTBThemeManager->GetLinearGaugeFrameOutLineColor());
	SetFrameOutlineColor(m_pTBThemeManager->GetTileDialogTitleForeColor());

	//set transparent

	CWnd* pParent = GetParentForm(this)->GetFormCWnd();

	if (pParent)
	{
		CBaseTileDialog* pTile = dynamic_cast<CBaseTileDialog*>(pParent);
		if (pTile)
			SetBkgColor(pTile->GetTileStyle()->GetBackgroundColor());
	}

	//set default colors according tb theme
	SetPointerBkgColor		(m_pTBThemeManager->GetLinearGaugePointerBkgColor());
	SetPointerOutlineColor	(m_pTBThemeManager->GetLinearGaugePointerOutLineColor());

	SetForeColor			(m_pTBThemeManager->GetLinearGaugeForeColor());
	SetTickMarkBkgColor		(m_pTBThemeManager->GetLinearGaugeTickMarkBkgColor());
	SetTickMarkOutlineColor	(m_pTBThemeManager->GetLinearGaugeTickMarkOutLineColor());

	SetGaugeColors();
	
	// set all
	SetTextLabelFormat		(m_pTBThemeManager->GetLinearGaugeTextLabelFormat());
	SetMajorTickMarkSize	(m_pTBThemeManager->GetLinearGaugeMajorMarkSize());
	SetMinorTickMarkSize	(m_pTBThemeManager->GetLinearGaugeMinorMarkSize());
	

	SetMajorTickMarkStep	(m_pTBThemeManager->GetLinearGaugeMajorStepMarkSize());
	SetStep					(m_pTBThemeManager->GetLinearGaugeMinorStepMarkSize());

	double minValue = *((DataDbl*)GetMinValue());
	double maxValue = *((DataDbl*)GetMaxValue());
	SetGaugeRange			(minValue, maxValue);

	return TRUE;
}

//---------------------------------------------------------------------------------------
BOOL CTBLinearGaugeCtrl::CheckDataObjType(const DataObj* pDataObj /*NULL*/)
{
	if (!pDataObj)
		return FALSE;
	
	DataType dataType = pDataObj->GetDataType();

	return dataType == DATA_DBL_TYPE || dataType == DATA_QTA_TYPE || dataType == DATA_PERC_TYPE || dataType == DATA_MON_TYPE;
}



/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//					class CTBCircularGaugeCtrl implementation
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//==============================================================================================
IMPLEMENT_DYNCREATE(CTBCircularGaugeCtrl, CBCGPCircularGaugeCtrl)

//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBCircularGaugeCtrl, CBCGPCircularGaugeCtrl)
	//{{AFX_MSG_MAP(CTBCircularGaugeCtrl)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//----------------------------------------------------------------------------------------------
CTBCircularGaugeCtrl::CTBCircularGaugeCtrl(const CString sName /*= _T("")*/)
	:
	IDisposingSourceImpl	(this)
{
	CParsedCtrl::Attach(this);

	AttachGauge(this->GetGauge());

	m_sName = sName;

	m_bFullCircularColor = FALSE;

	m_clrBaseColor = m_pTBThemeManager->GetTabSelectorBkgColor();
}

//---------------------------------------------------------------------------------------------------
void CTBCircularGaugeCtrl::SetColorFirstArea(COLORREF* pColor)
{
	m_pFirstAreaClr = pColor;
}

//-------------------------------------------------------------------------------------------------------
void CTBCircularGaugeCtrl::SetColorSecondArea(COLORREF* pColor)
{
	m_pSecondAreaClr = pColor;
}

//------------------------------------------------------------------------------------------------
void CTBCircularGaugeCtrl::SetValue(const DataObj& aValue)
{
	const DataDbl& aDouble = (const DataDbl&)aValue;

	if (m_pGauge && !m_bNoPointer)
		m_pGauge->SetValue(aDouble);

	//manage full colored areas (to do n-pointers) and manage aValue > m_dMax or aValue < m_dMin
	RemoveAllColoredRanges();
	if (m_pFirstAreaClr && m_pSecondAreaClr)
	{	
		m_bFullCircularColor ? AddFullColoredRange(m_dMin, aDouble, *m_pFirstAreaClr) : AddColoredRange(m_dMin, aDouble, *m_pFirstAreaClr, 20);
		m_bFullCircularColor ? AddFullColoredRange(aDouble, m_dMax, *m_pSecondAreaClr) : AddColoredRange(m_dMin, aDouble, *m_pSecondAreaClr, 20);
		SetGaugeRange(m_dMin, m_dMax);
		RedrawWindow();
	}

	Invalidate();
}

//------------------------------------------------------------------------------------------------
void CTBCircularGaugeCtrl::GetValue(DataObj& aValue)
{
	if (m_pGauge && !m_bNoPointer)
		((DataDbl*)m_pData)->Assign(m_pGauge->GetValue(0));
}

//------------------------------------------------------------------------------------------------
BOOL CTBCircularGaugeCtrl::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	BOOL bOk = __super::Create(rect, pParentWnd, nID, dwStyle);

	bOk = bOk && 	/*CheckControl(nID, pParentWnd, _T("STATIC")) && TODOBRUNA (picture control al posto di di static)*/
		CreateAssociatedButton(pParentWnd) && CParsedCtrl::InitCtrl();

	return bOk;
}

//------------------------------------------------------------------------------
LRESULT CTBCircularGaugeCtrl::OnRecalcCtrlSize(WPARAM w, LPARAM l)
{
	DoRecalcCtrlSize();
	return 0L;
}

//----------------------------------------------------------------------------------------
void CTBCircularGaugeCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	GetCtrlParent()->SendMessage(WM_LBUTTONDOWN);
}

//------------------------------------------------------------------------------------------
BOOL CTBCircularGaugeCtrl::SubclassEdit(UINT nID, CWnd* pParent, const CString& sName)
{

	BOOL bOk =
		//CheckControl(nID, pParentWnd, _T("STATIC")) && TODOBRUNA (picture control al posto di di static)
		CBCGPCircularGaugeCtrl::SubclassDlgItem(nID, pParent) &&
		CParsedCtrl::InitCtrl();

	if (bOk)
		SetNamespace(m_sName);

	return bOk;

}

//---------------------------------------------------------------------------------------
BOOL CTBCircularGaugeCtrl::OnInitCtrl()
{
	if (!m_pGauge)
		return FALSE;

	SetFrameSize(1);
	RemoveAllColoredRanges();
	
	//manage fonts
	CTBGaugeManager::SetFont(m_pTBThemeManager->GetTabSelectorButtonFont());
	
	//*m_pFirstAreaClr = *m_pSecondAreaClr = m_pTBThemeManager->GetTabSelectorBkgColor();
	SetBkgColor(m_clrBaseColor);
	SetFrameOutlineColor(m_pTBThemeManager->GetTileDialogTitleForeColor());

	//set default colors according tb theme
	SetPointerBkgColor(m_pTBThemeManager->GetTileDialogTitleBkgColor() );
	SetPointerOutlineColor(m_pTBThemeManager->GetTileDialogTitleBkgColor());

	SetForeColor(m_pTBThemeManager->GetTileDialogTitleForeColor());
	SetTickMarkBkgColor(m_pTBThemeManager->GetLinearGaugeTickMarkBkgColor());
	SetTickMarkOutlineColor(m_pTBThemeManager->GetLinearGaugeTickMarkOutLineColor());

	SetGaugeColors();

	// set all
	SetTextLabelFormat(m_pTBThemeManager->GetLinearGaugeTextLabelFormat());
	SetMajorTickMarkSize(m_pTBThemeManager->GetLinearGaugeMajorMarkSize());
	SetMinorTickMarkSize(m_pTBThemeManager->GetLinearGaugeMinorMarkSize());


	SetMajorTickMarkStep(m_pTBThemeManager->GetLinearGaugeMajorStepMarkSize());
	SetStep(m_pTBThemeManager->GetLinearGaugeMinorStepMarkSize());

	double minValue = *((DataDbl*)GetMinValue());
	double maxValue = *((DataDbl*)GetMaxValue());
	SetGaugeRange(minValue, maxValue);

	EnableShapeByTicksArea();
	SetTicksAreaAngles(180, 0);
	SetCapSize(5);

	//ModifyPointer(0.1, 0.1 );

	return TRUE;
}

//---------------------------------------------------------------------------------------
BOOL CTBCircularGaugeCtrl::CheckDataObjType(const DataObj* pDataObj /*NULL*/)
{
	if (!pDataObj)
		return FALSE;

	DataType dataType = pDataObj->GetDataType();

	return dataType == DATA_DBL_TYPE || dataType == DATA_QTA_TYPE || dataType == DATA_PERC_TYPE || dataType == DATA_MON_TYPE;
}