
#include "stdafx.h"

#include <TbGenLib\TBLinearGauge.h>
#include <TbGenLib\BaseTileManager.h>
#include <TbGenLib\TilePanel.h>
#include <TbGes\JsonFormEngineEx.h>

// local declaration
#include "StatusTile.h" 
#include <TbGes\PredefinedColors.h>

#include "StatusTile.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


////=============================================================================
////			Class CStatusStaticLabel implementation
////=============================================================================
//IMPLEMENT_DYNCREATE(CStatusStaticLabel, CLabelStatic)
//
//BEGIN_MESSAGE_MAP(CStatusStaticLabel, CLabelStatic)
//	//ON_WM_PAINT()
//	//ON_WM_ERASEBKGND()
//END_MESSAGE_MAP()
//
//////---------------------------------------------------------------------------------------
////BOOL CStatusStaticLabel::OnEraseBkgnd(CDC* pCDC)
////{
////	//return CParsedStatic::OnEraseBkgnd(pCDC);
////	CRect rclientRect;
////	this->GetClientRect(rclientRect);
////		
////	CParsedForm* pParsedForm = ::GetParsedForm(this->GetParent());
////	if (pParsedForm && m_bColored)
////	{
////		CBrush brk(RGB(255, 0, 0)/*m_crBkgnd*/);
////		pCDC->FillRect(&rclientRect, &brk);
////		pCDC->SetTextColor(RGB(255, 0, 0));
////	}
////	return FALSE;
////}

/////////////////////////////////////////////////////////////////////////////////////////////
//					class CBaseStatusTile implementation
//////////////////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBaseStatusTile, CJsonTileDialog)

//------------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBaseStatusTile, CJsonTileDialog)
	ON_WM_PAINT()
	ON_WM_LBUTTONDOWN()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------------------------
CBaseStatusTile::CBaseStatusTile()
	:
	CJsonTileDialog			(),
	m_bHasLeftBorder		(FALSE),
	m_pDescription			(NULL),
	m_pDblValue				(NULL),
	m_pFont					(NULL),
	m_pThemeManager			(NULL),
	m_YMinLeftBorder		(0),
	m_YMaxLeftBorder		(0),
	m_LeftBorderTileColor	(0)
{
	SetCollapsible			(FALSE);
	SetHasTitle				(FALSE);

	m_pThemeManager			= AfxGetThemeManager();
	m_DescriptionTextColor	= m_pThemeManager->GetTooltipForeColor();
	m_pFont					= m_pThemeManager->GetTabSelectorButtonFont();

	//default left border tile color
	m_DefaultLeftBorderTileColor = CLR_STATUS_EMPTY;

	SetMinHeight(ORIGINAL);
	SetMinWidth(ORIGINAL);
	SetMaxWidth(ORIGINAL);

	// Metodo che serve ad informare il framework che la risorsa non va cercata nella dll di classe, ma in quella dove viene definita la classe CLinearStatusTile
	SetResourceModule(GetDllInstance(RUNTIME_CLASS(CBaseStatusTile)));
}
//----------------------------------------------------------------------------------------------------
void CBaseStatusTile::OnUpdateControls(BOOL bParentIsVisible /*= TRUE*/)
{
	__super::OnUpdateControls(bParentIsVisible);
	RedrawStatusTileObj();
}
//----------------------------------------------------------------------------------------------------
void CBaseStatusTile::RedrawStatusTileObj()
{
	CWndStatusTileDescription* pStatusTileDesc = (CWndStatusTileDescription*)GetJsonContext()->m_pDescription;
	//Aggiunto per rendere interfaccia GDI aggiornata su cambio valori dei dataobj.OCCHIO potrebbe essere troppo pesante come performance
	pStatusTileDesc->EvaluateExpressions(GetJsonContext());
	
	SetTileClickable(pStatusTileDesc->m_bIsClickable);
	
	if (pStatusTileDesc->m_bHasLeftBorder)
	{
		SetHasLeftBorder();
	}
	
	RedrawDescription	();
	int leftBorderColor = pStatusTileDesc->m_BorderLeftColor;
	if (leftBorderColor && m_LeftBorderTileColor != (COLORREF)leftBorderColor)
	{
		m_LeftBorderTileColor = (COLORREF)leftBorderColor;
		Invalidate();
	}
	CustomizeGauge	();
}

//-------------------------------------------------------------------------------------------------
BOOL CBaseStatusTile::OnInitDialog()
{
	if (((CAbstractFormDoc*)m_pDocument)->m_bBatch)
		m_GaugeBorderFrameColor = m_pThemeManager->GetTileDialogTitleBkgColor();
	else
		m_GaugeBorderFrameColor = m_pThemeManager->GetTileGroupBkgColor();

	return __super::OnInitDialog();
}

//------------------------------------------------------------------------------------------------------
void CBaseStatusTile::OnPaint()
{
	__super::OnPaint();

	if (m_bHasLeftBorder)
	{
		CDC* pDC = this->GetDC();
		CRect edgeLeftRect(4, m_YMinLeftBorder, 6, m_YMaxLeftBorder);
		pDC->FillSolidRect(edgeLeftRect, m_LeftBorderTileColor ? m_LeftBorderTileColor : m_DefaultLeftBorderTileColor);	
		ReleaseDC(pDC);
	}
}

//------------------------------------------------------------------------------------------------------------
void CBaseStatusTile::OnLButtonDown(UINT nFlags, CPoint point)
{
	__super::OnLButtonDown(nFlags, point);

	if (m_bClickable && GetDocument())
	{
		GetDocument()->GetMasterFrame()->PostMessage(WM_COMMAND, this->GetDialogID());
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////
//					class CLinearStatusTile implementation
/////////////////////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLinearStatusTile, CBaseStatusTile)
	ON_WM_SIZE()
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CLinearStatusTile, CBaseStatusTile)

//--------------------------------------------------------------------------------------------------
CLinearStatusTile::CLinearStatusTile(BOOL bClickable /*= FALSE*/)
	:
	CBaseStatusTile				(),
	m_pTileImage				(NULL),
	m_pDescriptionLabel			(NULL),
	m_pAuxLeftDescription		(NULL),
	m_pAuxRightDescription		(NULL),
	m_bAuxLeftBold				(FALSE),
	m_bAuxLeftItalic			(FALSE),
	m_bAuxRightBold				(FALSE),
	m_bAuxRightItalic			(FALSE),
	m_nDblValue					(0),
	m_pLinearGauge				(NULL)
{
	m_pDblValue					= &m_nDblValue;
}

CLinearStatusTile::CLinearStatusTile(CString ns, BOOL bClickable /*= FALSE*/)
:
	CBaseStatusTile(),
	m_pTileImage(NULL),
	m_pDescriptionLabel(NULL),
	m_pAuxLeftDescription(NULL),
	m_pAuxRightDescription(NULL),
	m_bAuxLeftBold(FALSE),
	m_bAuxLeftItalic(FALSE),
	m_bAuxRightBold(FALSE),
	m_bAuxRightItalic(FALSE),
	m_nDblValue(0),
	m_pLinearGauge(NULL)
{
	m_pDblValue = &m_nDblValue;
}

//---------------------------------------------------------------------------------------------------
void CLinearStatusTile::SetDescriptionStyle()
{
	if (!m_pDescriptionLabel)
		return;
	m_pDescriptionLabel->SetOwnFont(GetFont());
	m_pDescriptionLabel->SetOwnFont(FALSE, FALSE, m_bClickable);
}

//---------------------------------------------------------------------------------------------------
void CLinearStatusTile::RedrawDescription()
{
	SetDescriptionStyle();
	
	if (m_pDescriptionLabel)
	{
		Invalidate();
	}
}

//-------------------------------------------------------------------------------------------------
BOOL CLinearStatusTile::OnInitDialog()
{	
	BOOL bRet =  __super::OnInitDialog();

	CRect aRectGauge;
	CRect aRectLabel;
	CRect aRectAuxLeft;
	CRect aRectAuxRight;

	CWnd* pWndImage = GetDlgItem(IDC_CONTROL_IMAGE);
	CWnd* pWndGauge = GetDlgItem(IDC_CONTROL_GAUGE);
	CWnd* pWndLabel = GetDlgItem(IDC_CONTROL_STATIC_UP);
	CWnd* pWndAuxLeftLabel = GetDlgItem(IDC_CONTROL_STATIC_DOWN_LEFT);
	CWnd* pWndAuxRightLabel = GetDlgItem(IDC_CONTROL_STATIC_DOWN_RIGHT);

	if (!pWndImage || !pWndLabel)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pDescriptionLabel = (CLabelStatic*)GetLinkedParsedCtrl(IDC_CONTROL_STATIC_UP);
	DataStr* pObj = dynamic_cast<DataStr*>(m_pDescriptionLabel->GetCtrlData());
	if (pObj)
	{
		SetDescription(pObj);
	}

	CPictureStatic* pPicture = (CPictureStatic*)GetLinkedParsedCtrl(IDC_CONTROL_IMAGE);
	pObj = dynamic_cast<DataStr*>(pPicture->GetCtrlData());
	if (pObj)
	{
		SetImageNS(pObj);
	}
	
	CLabelStatic* pLabelAuxLDesc = (CLabelStatic*)GetLinkedParsedCtrl(IDC_CONTROL_STATIC_DOWN_LEFT);
	pObj = dynamic_cast<DataStr*>(pLabelAuxLDesc->GetCtrlData());
	if (pObj)
	{
		SetAuxLeftDescription(pObj, FALSE, FALSE);  //here we can add attribute in tbjson for second and third parameters
	}

	CLabelStatic* pLabelAuxRDesc = (CLabelStatic*)GetLinkedParsedCtrl(IDC_CONTROL_STATIC_DOWN_RIGHT);
	pObj = dynamic_cast<DataStr*>(pLabelAuxRDesc->GetCtrlData());
	if (pObj)
	{
		SetAuxRightDescription(pObj, FALSE, FALSE);  //here we can add attribute in tbjson for second and third parameters
	}

	m_pLinearGauge = (CTBLinearGaugeCtrl*)GetLinkedParsedCtrl(IDC_CONTROL_GAUGE);
		

	//code as in original BuildDataControlLinks (except for AddLink)
	if (GetDescription())
	{
		SetDescriptionStyle();
	}
	else
		pWndLabel->ShowWindow(SW_HIDE);

	if (!m_pTileImage)
	{
		pWndImage->ShowWindow(SW_HIDE);
	}

	if (m_pLinearGauge)
	{
		m_pLinearGauge->SetTextLabelFormat(_T(""));
		m_pLinearGauge->SetBkgColor(m_pThemeManager->GetTileGroupBkgColor());
		m_pLinearGauge->SetFrameOutlineColor(m_GaugeBorderFrameColor);
		m_pLinearGauge->SetMajorTickMarkSize(0);
		m_pLinearGauge->SetMinorTickMarkSize(0);
		m_pLinearGauge->SetMajorTickMarkStep(0);
		m_pLinearGauge->RemovePointer(0);
		m_pLinearGauge->AddColoredRange(0, 100, CLR_STATUS_EMPTY);
		m_pLinearGauge->SetGaugeRange(0, 100);
		m_pLinearGauge->UpdateCtrlView();
	}

	if (m_pAuxLeftDescription)
	{
		//customize
		pLabelAuxLDesc->GetCtrlCWnd()->ModifyStyle(WS_BORDER, 0);
		pLabelAuxLDesc->SetOwnFont(GetFont());
		pLabelAuxLDesc->SetOwnFont(m_bAuxLeftBold, m_bAuxLeftItalic, FALSE);
		pLabelAuxLDesc->SetTextColor(RGB(0, 0, 0));
	}
	else if (pWndAuxLeftLabel) 
		pWndAuxLeftLabel->ShowWindow(SW_HIDE);

	if (m_pAuxRightDescription)
	{
		//customize
		pLabelAuxRDesc->GetCtrlCWnd()->ModifyStyle(WS_BORDER, 0);
		pLabelAuxRDesc->SetOwnFont(GetFont());
		pLabelAuxRDesc->SetOwnFont(m_bAuxRightBold, m_bAuxRightItalic, FALSE);
		pLabelAuxRDesc->SetTextColor(RGB(0, 0, 0));
	}
	else if (pWndAuxRightLabel) 
		pWndAuxRightLabel->ShowWindow(SW_HIDE);

	return bRet;
}

//---------------------------------------------------------------------------------------------------
void CLinearStatusTile::CustomizeGauge()
{
	if (!m_pLinearGauge)
	{
		return;
	}
	
	CBaseDocument* pDoc = m_pLinearGauge->GetDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		((CAbstractFormDoc*)pDoc)->CustomizeGauge(m_pLinearGauge);
	}
}

//--------------------------------------------------------------------------------------------------
void CLinearStatusTile::BuildDataControlLinks()
{
			
}

//-----------------------------------------------------------------------------------------
void CLinearStatusTile::OnSize(UINT nType, int cx, int cy)
{
	CRect aRectGauge;
	CRect aRectLabel;
	CRect aRectAuxLeft;
	CRect aRectAuxRight;

	CWnd* pWndImage			= GetDlgItem(IDC_CONTROL_IMAGE);
	CWnd* pWndGauge			= GetDlgItem(IDC_CONTROL_GAUGE);
	CWnd* pWndLabel			= GetDlgItem(IDC_CONTROL_STATIC_UP);
	CWnd* pWndAuxLeftLabel	= GetDlgItem(IDC_CONTROL_STATIC_DOWN_LEFT);
	CWnd* pWndAuxRightLabel = GetDlgItem(IDC_CONTROL_STATIC_DOWN_RIGHT);

/*	if (!pWndImage || !pWndGauge || !pWndLabel || !pWndAuxLeftLabel || !pWndAuxRightLabel)
	{
		__super::OnSize(nType, cx, cy);
		return;
	}*/

	__super::OnSize(nType, cx, cy);

	if (!pWndImage)
	{
		return;
	}

	//repositioning
	CRect aRectImage;

	pWndImage->GetWindowRect(aRectImage);
	pWndImage->GetParent()->ScreenToClient(aRectImage);

	int xImage = 0;
	int yImage = 0;
	int hImage = 0;
	int wImage = 0;

	SetImagePos(pWndImage, xImage, yImage, wImage, hImage);

	m_YMinLeftBorder = yImage;
	m_YMaxLeftBorder = yImage + hImage;

	int offsetX = wImage + 1;
	int offsetY = yImage;
	
	if (!pWndLabel)
	{
		return;
	}

	SetDescriptionPos(pWndLabel, xImage, offsetX, offsetY);

	int yGauge = 0;

	if (!pWndGauge)
	{
		return;
	}

	SetGaugePos(pWndGauge, yGauge);

	if (pWndAuxLeftLabel || pWndAuxRightLabel)
	{
		SetAuxDescriptionsPos(pWndAuxLeftLabel, pWndAuxRightLabel, xImage, yGauge);
	}
}

//-------------------------------------------------------------------------------------------------------------------------
void CLinearStatusTile::SetImagePos(CWnd* pWnd, int& xImage, int& yImage, int& wImage, int& hImage)
{
	CRect aRect;
	GetWindowRect(aRect);
	ScreenToClient(aRect);

	CRect aRectImage;

	pWnd->GetWindowRect(aRectImage);
	pWnd->GetParent()->ScreenToClient(aRectImage);

	xImage = aRectImage.left;
	yImage = aRectImage.top;
	hImage = aRectImage.Height();
	wImage = aRectImage.Width();

	if (m_pLinearGauge || m_pAuxLeftDescription || m_pAuxRightDescription)	//first half of height's tile
	{
		yImage = (int)aRect.Height() / 2 - hImage;
		if (yImage <= 5)
			yImage = 4;
	}
	else	//in the middle - in verical
		yImage = (int)aRect.Height() / 2  - (int)hImage / 2;
	
	pWnd->SetWindowPos(NULL, xImage, yImage, wImage, hImage, SWP_NOZORDER);
}

//----------------------------------------------------------------------------------------------------------------------
void CLinearStatusTile::SetDescriptionPos(CWnd* pWnd, int x, int offsetX, int offsetY)
{
	CRect aRect;
	GetWindowRect(aRect);
	ScreenToClient(aRect);

	CRect aRectLabel;
	pWnd->GetWindowRect(aRectLabel);
	pWnd->GetParent()->ScreenToClient(aRectLabel);

	int xLabel = aRectLabel.left;
	int yLabel = aRectLabel.top;
	int hLabel = aRectLabel.Height();

	yLabel = offsetY;

	if (!m_pLinearGauge && !m_pAuxLeftDescription && !m_pAuxRightDescription)	//first half of height's tile
		hLabel = aRect.Height() - yLabel - 1;

	if (m_pTileImage)
		xLabel = x + offsetX;
	else
		xLabel = x;
		
	pWnd->SetWindowPos(NULL, xLabel, yLabel, aRect.right - xLabel - 1, hLabel, SWP_NOZORDER);

}

//---------------------------------------------------------------------------------------------------------------------
void CLinearStatusTile::SetGaugePos(CWnd* pWnd, int& yGauge/*, LONG x, LONG y, LONG width, LONG height*/)
{
	CRect aRect;
	GetWindowRect(aRect);
	ScreenToClient(aRect);

	CRect aRectGauge;
	pWnd->GetWindowRect(aRectGauge);
	pWnd->GetParent()->ScreenToClient(aRectGauge);

	int hGauge = aRectGauge.Height();
	int xGauge = aRectGauge.left - 2;
	yGauge = (int)aRect.Height() / 2;	//always on second half
	int wGauge = aRect.Width() - aRectGauge.left - 2;
	
	if (!m_pLinearGauge)
		return;

	m_YMaxLeftBorder = yGauge + hGauge + 10;

	m_pLinearGauge->SetWindowPos(NULL, xGauge, yGauge, wGauge, hGauge, SWP_NOZORDER);
}

//-----------------------------------------------------------------------------------------------------------------
void CLinearStatusTile::SetAuxDescriptionsPos(CWnd* pWndLeft, CWnd* pWndRight, int xImage, int yGauge)
{
	LONG heightLeft = 0, heightRight = 0;
	
	CRect aRect;
	GetWindowRect(aRect);
	ScreenToClient(aRect);

	if (!m_pAuxLeftDescription && !m_pAuxRightDescription)
		return;

	if (m_pAuxLeftDescription)
	{
		CRect aRectLeft;
		pWndLeft->GetWindowRect(aRectLeft);
		pWndLeft->GetParent()->ScreenToClient(aRectLeft);
		heightLeft = aRectLeft.Height();
	}

	if (m_pAuxRightDescription)
	{
		CRect aRectRight;
		pWndRight->GetWindowRect(aRectRight);
		pWndRight->GetParent()->ScreenToClient(aRectRight);
		heightRight = aRectRight.Height();
	}

	int yAuxLabels	= yGauge - 2;
	int wAux		= aRect.Width() - xImage - 2;

	m_YMaxLeftBorder = yAuxLabels + (heightLeft == 0 ? heightRight : heightLeft);

	if (m_pAuxLeftDescription && !m_pAuxRightDescription)  //I have only Left Aux Description
	{
		//image, description, aux left description
		pWndLeft->SetWindowPos(NULL, xImage, yAuxLabels, wAux, heightLeft, SWP_NOZORDER);
	}
	else if (!m_pAuxLeftDescription && m_pAuxRightDescription)
	{
		pWndRight->SetWindowPos(NULL, xImage, yAuxLabels, wAux, heightRight, SWP_NOZORDER);
	}
	else if (m_pAuxLeftDescription && m_pAuxRightDescription)  //I have both descriptions
	{
		pWndLeft->SetWindowPos(NULL, xImage, yAuxLabels, (int)wAux / 2 - 1, heightLeft, SWP_NOZORDER);
		pWndRight->SetWindowPos(NULL, xImage + wAux / 2 + 1, yAuxLabels, (int)wAux / 2 - 1, heightRight, SWP_NOZORDER);

	}
}

//==============================================================================
IMPLEMENT_DYNCREATE(CWndStatusTileDescription, CWndTileDescription)
REGISTER_WND_OBJ_CLASS(CWndStatusTileDescription, StatusTile)

//-----------------------------------------------------------------------------
void CWndStatusTileDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);
	
	m_bIsClickable = ((CWndStatusTileDescription*)pDesc)->m_bIsClickable;
	m_bHasLeftBorder = ((CWndStatusTileDescription*)pDesc)->m_bHasLeftBorder;
	m_BorderLeftColor = ((CWndStatusTileDescription*)pDesc)->m_BorderLeftColor;
}
//-----------------------------------------------------------------------------
void CWndStatusTileDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bIsClickable, szJsonClickable, false);
	SERIALIZE_BOOL(m_bHasLeftBorder, szJsonBorderLeft, false);
	SERIALIZE_INT(m_BorderLeftColor, szJsonBorderLeftColor, 0);
				
	__super::SerializeJson(strJson);
}

//-----------------------------------------------------------------------------
void CWndStatusTileDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);
	
	PARSE_BOOL(m_bIsClickable, szJsonClickable);
	PARSE_BOOL(m_bHasLeftBorder, szJsonBorderLeft);
	PARSE_INT (m_BorderLeftColor, szJsonBorderLeftColor);
}
