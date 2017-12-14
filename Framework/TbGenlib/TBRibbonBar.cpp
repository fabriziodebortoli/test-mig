#include "stdafx.h"

#include <atlimage.h>

#include <TbNameSolver\PathFinder.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGenlib\TBRibbonBar.h>
#include <TbGeneric\ParametersSections.h>
#include <tbgeneric\tbcmdui.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGenlib\AddOnMng.h>
#include <TbGenlib\BaseApp.h>
#include <TbGenlib\OslInfo.h>
#include <TbGenlib\OslBaseInterface.h>
#include <TBNameSolver\ThreadContext.h>
#include <TbGeneric\TBThemeManager.h>

#include "basefrm.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//					CTBRibbonCategory
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CTBRibbonCategory,CBCGPRibbonCategory)

//-------------------------------------------------------------------------------------
CTBRibbonCategory::CTBRibbonCategory() 
{
}

//-------------------------------------------------------------------------------------
CTBRibbonCategory::~CTBRibbonCategory()
{

}

//-------------------------------------------------------------------------------------
CString CTBRibbonCategory::GetName()
{
	return GetInfoOSL()->m_Namespace.GetObjectName();
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel* CTBRibbonCategory::AddPanel (LPCTSTR lpszName, const CString& sText,  HICON pIco)
{
	m_LargeImages.SetImageSize(CSize(RIBBONBAR_LARGE_ICO, RIBBONBAR_LARGE_ICO));
	m_SmallImages.SetImageSize(CSize(RIBBONBAR_SMALL_ICO, RIBBONBAR_SMALL_ICO));

	CTBRibbonPanel* pPanel = (CTBRibbonPanel*) __super::AddPanel(sText, pIco, RUNTIME_CLASS(CTBRibbonPanel));
	if (pPanel)
		pPanel->AttachOSLInfo(GetInfoOSL(), lpszName);
	return pPanel;
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel* CTBRibbonCategory::GetPanel (const CString& sName)
{
	for (int i=0; i < m_arPanels.GetSize(); i++)
	{
		CBCGPRibbonPanel* pPanel = m_arPanels.GetAt(i);
		if (pPanel->IsKindOf(RUNTIME_CLASS(CTBRibbonPanel)))
		{
			CString sNamePanel = ((CTBRibbonPanel*)pPanel)->GetName();
			if (sName.CompareNoCase(sNamePanel) == 0)
			{
				return (CTBRibbonPanel*)pPanel;
			}
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel* CTBRibbonCategory::GetPanel (int nIndex)
{
	CBCGPRibbonPanel* pPanel = m_arPanels.GetAt(nIndex);
	if (pPanel->IsKindOf(RUNTIME_CLASS(CTBRibbonPanel)))
			return (CTBRibbonPanel*) pPanel;	
	return NULL;
}

//-------------------------------------------------------------------------------------
CTBRibbonCategory* CTBRibbonCategory::CreateCopy (CTBRibbonBar* pParent)
{
	m_pParenrRibbonBar = pParent;
	return (CTBRibbonCategory*)__super::CreateCustomCopy(pParent);

}

//-------------------------------------------------------------------------------------
void CTBRibbonCategory::AttachOSLInfo (CInfoOSL* pParent, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::BARPANEL, sName, pParent->m_Namespace);
	
	GetInfoOSL()->m_pParent = pParent;
	GetInfoOSL()->SetType(OSLType_Control);	//TODO OSL da vedere con Riccardo contenitore
	GetInfoOSL()->m_Namespace = aNs;

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());
}


/////////////////////////////////////////////////////////////////////////////
//					CTBRibbonPanel
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CTBRibbonPanel,CBCGPRibbonPanel)

//-------------------------------------------------------------------------------------
CTBRibbonPanel::CTBRibbonPanel(BOOL isNew /*TRUE*/) 
{
	m_bIsNew = isNew;
}

//-------------------------------------------------------------------------------------
CString CTBRibbonPanel::GetName()
{
	return GetInfoOSL()->m_Namespace.GetObjectName();
}

//-------------------------------------------------------------------------------------
CDC* CTBRibbonPanel::GetDC()
{
	if (m_pParent && m_pParent->GetParentRibbonBar ())
		return m_pParent->GetParentRibbonBar ()->GetDC();

	ASSERT_TRACE(FALSE,_T("Panel has not a Ribbon Bar or a DC to load images!"));
	return NULL;
}

//-------------------------------------------------------------------------------------
void CTBRibbonPanel::ReleaseDC(CDC* pDC)
{
	if (m_pParent && m_pParent->GetParentRibbonBar ())
		m_pParent->GetParentRibbonBar ()->ReleaseDC(pDC);
	else
		ASSERT_TRACE(FALSE,_T("Panel has not a Ribbon Bar or a DC to release!"));
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel::~CTBRibbonPanel()
{
	for (int i = m_arButtonOSLInfo.GetUpperBound(); i >=0; i--)
	{
		delete m_arButtonOSLInfo.GetAt(i);
	}

	m_arButtonOSLInfo.RemoveAll();
	m_arButonsGroup.RemoveAll();
}

//-------------------------------------------------------------------------------------
void CTBRibbonPanel::OnUpdateCmdUI (CBCGPRibbonCmdUI* pCmdUI, CFrameWnd* pTarget, BOOL bDisableIfNoHndler)
{
	__super::OnUpdateCmdUI (pCmdUI, pTarget, FALSE);
}

//-------------------------------------------------------------------------------------
const CBCGPToolBarImages*	CTBRibbonPanel::GetIconsImageList()
{
	return (this->m_btnDefault).m_pIconsImageList;
}

//-------------------------------------------------------------------------------------
void  CTBRibbonPanel::SetIconsImageList(CBCGPToolBarImages* pIamgeList)
{
	(this->m_btnDefault).m_pIconsImageList = pIamgeList;
}

//-------------------------------------------------------------------------------------
void CTBRibbonPanel::AddOslInfo(UINT nCommandID, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, sName, GetInfoOSL()->m_Namespace);

	CInfoOSLButton* pInfo = new CInfoOSLButton(nCommandID, sName);
	pInfo->m_pParent = GetInfoOSL();
	pInfo->SetType(OSLType_Control); //TODO OSL da vedere con Riccardo 
	pInfo->m_Namespace = aNs;
	m_arButtonOSLInfo.Add(pInfo);

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(pInfo);
}

//-------------------------------------------------------------------------------------
void CTBRibbonPanel::AttachOSLInfo (CInfoOSL* pParent, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::BARPANEL, sName, pParent->m_Namespace);
	
	GetInfoOSL()->m_pParent = pParent;
	GetInfoOSL()->SetType(OSLType_Control);	//TODO OSL da vedere con Riccardo contenitore
	GetInfoOSL()->m_Namespace = aNs;

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());
}

//-------------------------------------------------------------------------------------
CTBRibbonButton* CTBRibbonPanel::AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText /*= _T("")*/)
{
	HICON pIcoLarg = NULL;
	if (!sImageNameSpace.IsEmpty())
	{
		CDC* pDC = GetDC();
		if (pDC)
		{
			pIcoLarg = TBLoadImage(sImageNameSpace, pDC, RIBBONBAR_LARGE_ICO);
			ReleaseDC(pDC);
		}
	}

	CTBRibbonButton* btn = new CTBRibbonButton(nCommandID, szText, pIcoLarg, FALSE, NULL, FALSE, FALSE);
	Add(btn);
	AddOslInfo(nCommandID, szText);
	return btn;
}

//-------------------------------------------------------------------------------------
CTBRibbonButton* CTBRibbonPanel::AddButton 
	(
		UINT nCommandID, 
		UINT nIDImageLarge,
		UINT nIDImageSmall,
		const CString& aLibNamespace, 
		const CString& sName,
		const CString& szText,
		BOOL bPNG
	)
{
	HICON pIcoLarg = NULL;
	HICON pIcoSmall = NULL;
	if (nIDImageLarge > 0 || nIDImageSmall > 0)
	{
		CDC* pDC = GetDC();
		if (pDC)
		{
			pIcoLarg = TBLoadImage(pDC, NULL, nIDImageLarge, RIBBONBAR_LARGE_ICO, bPNG);
			
			if (nIDImageSmall > 0)	
				pIcoSmall = TBLoadImage(pDC, NULL, nIDImageSmall, RIBBONBAR_SMALL_ICO, bPNG);
			ReleaseDC(pDC);
		}
	}

	CTBRibbonButton* btn = new CTBRibbonButton( nCommandID, szText, pIcoLarg, FALSE, pIcoSmall, FALSE, FALSE );
	Add(btn);
	AddOslInfo(nCommandID, sName);
	return btn;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonPanel::AddLaunchButton(UINT nCommandID, UINT nIdImg, BOOL bPNG)
{
	int nIcon = -1;
	//todo: da finire.!

	/*CBCGPToolBarImages*  pToolBarImages = (CBCGPToolBarImages*) GetIconsImageList();
	if (!pToolBarImages)
	{
		SetIconsImageList(pToolBarImages);
		pToolBarImages->SetImageSize(CSize(RIBBONBAR_LARGE_ICO, RIBBONBAR_LARGE_ICO));
	}

	if (nIdImg > 0)
	{
		CDC* pDC = GetDC();
		if (pDC)
		{
			HICON pIco = CIconList::GetIcon(pDC, RIBBONBAR_SMALL_ICO, nIdImg, bPNG);
			if (pIco)
				nIcon =  (pToolBarImages)->AddIcon(pIco);
			ReleaseDC(pDC);
		}
	}*/

	EnableLaunchButton(nCommandID, nIcon);
	return TRUE;
}


//-------------------------------------------------------------------------------------
BOOL CTBRibbonPanel::AddSeparator(BOOL bIsHoriz /*= FALSE*/)
{
	Add(new CBCGPRibbonSeparator (bIsHoriz));
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonPanel::AddLabel(UINT nID, const CString& sName, LPCTSTR pText, BOOL bIsMultiLine)
{
	CBCGPRibbonLabel* lab = new CBCGPRibbonLabel(pText, bIsMultiLine);
	if (!lab) return FALSE;
	lab->SetID(nID);
	Add(lab);
	AddOslInfo(nID, sName);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonPanel::AddEdit(UINT nID, const CString& sName, LPCTSTR lpszLabel,  int nIDimage, CString aLibNamespace, int nWidth, BOOL bPNG)
{
	int nImage = -1; /*AddButtonsGroupIcon(pButtGroup, nIDimage, aLibNamespace, bPNG);*/
	CBCGPRibbonEdit* edit = new CBCGPRibbonEdit(nID, nWidth, lpszLabel, nImage);
	if (!edit) return FALSE;
	Add(edit);
	AddOslInfo(nID, sName);
	return TRUE;
}

//-------------------------------------------------------------------------------------
CTBRibbonCheckBox* CTBRibbonPanel::AddCheckBox(UINT nID, const CString& sName, const CString& text)
{
	CTBRibbonCheckBox* pCheckBox = new CTBRibbonCheckBox(nID, text);
	if (!pCheckBox) return FALSE;
	Add(pCheckBox);
	AddOslInfo(nID, sName);
	return pCheckBox;
}

//-------------------------------------------------------------------------------------
CTBRibbonButtonsGroup* CTBRibbonPanel::AddGroup	(const CString& sName)
{
	CTBRibbonButtonsGroup* pNewGroup = new CTBRibbonButtonsGroup();
	Add(pNewGroup);

	if (sName.IsEmpty())
		pNewGroup->AttachOSLInfo(GetInfoOSL(), _T("Group"));
	else
		pNewGroup->AttachOSLInfo(GetInfoOSL(), sName);

	m_arButonsGroup.Add(pNewGroup);
	return pNewGroup;
}

/////////////////////////////////////////////////////////////////////////////
//					CTBRibbonButton
/////////////////////////////////////////////////////////////////////////////

CTBRibbonButton::CTBRibbonButton()
{
}

//-------------------------------------------------------------------------------------
CTBRibbonButton::CTBRibbonButton (UINT nID, LPCTSTR lpszText, HICON	hIcon, BOOL bAlwaysShowDescription, HICON	hIconSmall ,
						BOOL	bAutoDestroyIcon , 
						BOOL	bAlphaBlendIcon) :

				CBCGPRibbonButton (nID, lpszText, hIcon, bAlwaysShowDescription, hIconSmall, bAutoDestroyIcon, bAlphaBlendIcon)
{
	
}

//-------------------------------------------------------------------------------------
CTBRibbonButton::~CTBRibbonButton()
{
}

//-------------------------------------------------------------------------------------
void CTBRibbonButton::OnUpdateCmdUI (CBCGPRibbonCmdUI* pCmdUI, CFrameWnd* pTarget, BOOL bDisableIfNoHndler)
{
	__super::OnUpdateCmdUI (pCmdUI, pTarget, FALSE);
}

//
// overload del metodo DrawImage per implementare le icone di size diverso e le icone Grayscale
// che attualmente non gestite dai BCG
//-------------------------------------------------------------------------------------
void CTBRibbonButton::DrawImage (CDC* pDC, RibbonImageType type, CRect rectImage)
{
	ASSERT_VALID (this);
	ASSERT_VALID (pDC);

	if (IsSearchResultMode () && !HasImage (RibbonImageSmall))
	{
		return;
	}

	CBCGPRibbonButton* pOrigButton = DYNAMIC_DOWNCAST (
		CBCGPRibbonButton, m_pOriginal);

	if (pOrigButton != NULL || m_bIsCustom)
	{
		__super::DrawImage (pDC, type, rectImage);
		return;
	}

	if (m_hIcon != NULL || (m_bSmallIconMode && m_hIconSmall != NULL))
	{
		HICON hIcon = type == RibbonImageLarge || m_hIconSmall == NULL ? m_hIcon : m_hIconSmall;

		CSize sizeIcon = type == RibbonImageLarge ? CSize (RIBBONBAR_LARGE_ICO, RIBBONBAR_LARGE_ICO) : CSize (16, 16);

		if (globalData.GetRibbonImageScale () != 1.)
		{
			sizeIcon.cx = (int) (.5 + globalData.GetRibbonImageScale () * sizeIcon.cx);
			sizeIcon.cy = (int) (.5 + globalData.GetRibbonImageScale () * sizeIcon.cy);
		}

		// icons is disabled
		if (m_bIsDisabled)
		{
			hIcon = CreateGrayscaleIcon(hIcon);
		}

		UINT diFlags = DI_NORMAL;

		CWnd* pWndParent = GetParentWnd ();
		if (pWndParent != NULL && (pWndParent->GetExStyle () & WS_EX_LAYOUTRTL))
		{
			diFlags |= 0x0010 /*DI_NOMIRROR*/;
		}

		if (m_pParentGroup != NULL)
		{
			::DrawIconEx (pDC->GetSafeHdc (), 
			rectImage.left, 
			rectImage.top,
			hIcon, sizeIcon.cx, sizeIcon.cy, 0, NULL,
			diFlags);
		}
		else
		{
			::DrawIconEx (pDC->GetSafeHdc (), 
				rectImage.left, 
				rectImage.top,
				hIcon, 
				sizeIcon.cx , 
				sizeIcon.cy , 
				0, NULL,
				diFlags);
		}
		return;
	}
	
	__super::DrawImage (pDC, type, rectImage);
}

//-------------------------------------------------------------------------------------
HICON CTBRibbonButton::CreateGrayscaleIcon( HICON hIcon)
{
	if (hIcon == NULL)
	{
		return NULL;
	}

	// Create Gray scale
	COLORREF pPalette[256];
	for(int i = 0; i < 256; i++)
	{
		pPalette[i] = RGB(255-i, 255-i, 255-i);
	}

	HDC hdc = ::GetDC(NULL);

	HICON      hGrayIcon      = NULL;
	ICONINFO   icInfo         = { 0 };
	ICONINFO   icGrayInfo     = { 0 };
	LPDWORD    lpBits         = NULL;
	LPBYTE     lpBitsPtr      = NULL;
	SIZE sz;
	DWORD c1 = 0;
	BITMAPINFO bmpInfo        = { 0 };
	bmpInfo.bmiHeader.biSize  = sizeof(BITMAPINFOHEADER);

	if (::GetIconInfo(hIcon, &icInfo))
	{
		if (::GetDIBits(hdc, icInfo.hbmColor, 0, 0, NULL, &bmpInfo, DIB_RGB_COLORS) != 0)
		{
			bmpInfo.bmiHeader.biCompression = BI_RGB;

			sz.cx = bmpInfo.bmiHeader.biWidth;
			sz.cy = bmpInfo.bmiHeader.biHeight;
			c1 = sz.cx * sz.cy;

			lpBits = (LPDWORD)::GlobalAlloc(GMEM_FIXED, (c1) * 4);

			if (lpBits && ::GetDIBits(hdc, icInfo.hbmColor, 0, sz.cy, lpBits, &bmpInfo, DIB_RGB_COLORS) != 0)
			{
				lpBitsPtr     = (LPBYTE)lpBits;
				UINT off      = 0;

				for (UINT i = 0; i < c1; i++)
				{
					off = (UINT)( 255 - (( lpBitsPtr[0] + lpBitsPtr[1] + lpBitsPtr[2] ) / 3) );

					if (lpBitsPtr[3] != 0 || off != 255)
					{
						if (off == 0)
						{
							off = 1;
						}

						lpBits[i] = pPalette[off] | ( lpBitsPtr[3] << 24 );
					}

					lpBitsPtr += 4;
				}

				icGrayInfo.hbmColor = ::CreateCompatibleBitmap(hdc, sz.cx, sz.cy);

				if (icGrayInfo.hbmColor != NULL)
				{
					::SetDIBits(hdc, icGrayInfo.hbmColor, 0, sz.cy, lpBits, &bmpInfo, DIB_RGB_COLORS);

					icGrayInfo.hbmMask = icInfo.hbmMask;
					icGrayInfo.fIcon   = TRUE;

					hGrayIcon = ::CreateIconIndirect(&icGrayInfo);

					::DeleteObject(icGrayInfo.hbmColor);
				}

				::GlobalFree(lpBits);
				lpBits = NULL;
			}
		}

		::DeleteObject(icInfo.hbmColor);
		::DeleteObject(icInfo.hbmMask);
	}

	::ReleaseDC(NULL,hdc);

	return hGrayIcon;
}

/////////////////////////////////////////////////////////////////////////////
//					CTBRibbonButtonsGroup
/////////////////////////////////////////////////////////////////////////////
CTBRibbonButtonsGroup::CTBRibbonButtonsGroup()
{
}

//-------------------------------------------------------------------------------------
CTBRibbonButtonsGroup::~CTBRibbonButtonsGroup()
{
	for (int i = m_arButtonOSLInfo.GetUpperBound(); i >=0; i--)
	{
		delete m_arButtonOSLInfo.GetAt(i);
	}
	m_arButtonOSLInfo.RemoveAll();
}

//-------------------------------------------------------------------------------------
CString CTBRibbonButtonsGroup::GetName()
{
	return GetInfoOSL()->m_Namespace.GetObjectName();
}

//-------------------------------------------------------------------------------------
void CTBRibbonButtonsGroup::CopyFrom (const CTBRibbonButtonsGroup& src)
{
	__super::CopyFrom(src);
	*GetInfoOSL() = *(const_cast<CTBRibbonButtonsGroup&>(src).GetInfoOSL());
}

//-------------------------------------------------------------------------------------
void CTBRibbonButtonsGroup::AttachOSLInfo (CInfoOSL* pParent, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::BARPANEL, sName, pParent->m_Namespace);
	
	GetInfoOSL()->m_pParent = pParent;
	GetInfoOSL()->SetType(OSLType_Control);	//TODO OSL da vedere con Riccardo contenitore
	GetInfoOSL()->m_Namespace = aNs;

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());
}

//-------------------------------------------------------------------------------------
void CTBRibbonButtonsGroup::AddOslInfo(UINT nCommandID, const CString& sName)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::TOOLBARBUTTON, sName, GetInfoOSL()->m_Namespace);

	CInfoOSLButton* pInfo = new CInfoOSLButton(nCommandID, sName);
	pInfo->m_pParent = GetInfoOSL();
	pInfo->SetType(OSLType_Control);
	pInfo->m_Namespace = aNs;
	m_arButtonOSLInfo.Add(pInfo);

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(pInfo);
}

//-------------------------------------------------------------------------------------
CTBRibbonButton* CTBRibbonButtonsGroup::AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText /*= _T("")*/)
{
	HICON pIcoLarg = NULL;
	
	if (!sImageNameSpace.IsEmpty())
	{
		CBCGPRibbonBar* pRibbonBar = m_pParent->GetParentRibbonBar();
		if (!pRibbonBar)
		{
			ASSERT_TRACE(FALSE, _T("Cannot have a ribbon bar"));
			return FALSE;
		}

		CDC* pDC = pRibbonBar->GetDC();
		pIcoLarg =  TBLoadImage(sImageNameSpace, pDC, RIBBONBAR_LARGE_ICO);
		pRibbonBar->ReleaseDC(pDC);
	}

	CTBRibbonButton* pBtn = new CTBRibbonButton(nCommandID, szText, pIcoLarg, TRUE, NULL, FALSE, FALSE);

	__super::AddButton(pBtn);
	AddOslInfo(nCommandID, szText);
	return pBtn;
}

//-------------------------------------------------------------------------------------
CTBRibbonButton* CTBRibbonButtonsGroup::AddButton 
	(
		UINT nCommandID, 
		UINT nIDImageLarge,
		UINT nIDImageSmall,
		const CString& aLibNamespace, 
		const CString& sName,
		const CString& szText,
		BOOL bPNG
	)
{
	HICON pIcoLarg = NULL;
	HICON pIcoSmall = NULL;
	if (nIDImageLarge > 0 || nIDImageSmall > 0)
	{
		CBCGPRibbonBar* pRibbonBar = m_pParent->GetParentRibbonBar();
		if (!pRibbonBar)
		{
			ASSERT_TRACE(FALSE,_T("Cannot have a ribbon bar"));
			return FALSE;
		}

		CDC* pDC = pRibbonBar->GetDC();
		if (nIDImageLarge)
			 pIcoLarg = TBLoadImage(pDC, NULL, nIDImageLarge, RIBBONBAR_LARGE_ICO, bPNG);
		
		if (nIDImageSmall > 0)	
			pIcoSmall = TBLoadImage(pDC, NULL, nIDImageSmall, RIBBONBAR_SMALL_ICO, bPNG);
		pRibbonBar->ReleaseDC(pDC);
	}

	CTBRibbonButton* pBtn = new CTBRibbonButton( nCommandID, szText, pIcoLarg, TRUE, pIcoSmall, FALSE, FALSE );

	__super::AddButton(pBtn);
	AddOslInfo(nCommandID, sName);
	return pBtn;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonButtonsGroup::AddSeparator(BOOL bIsHoriz )
{
	__super::AddButton(new CBCGPRibbonSeparator (bIsHoriz));
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonButtonsGroup::AddLabel(UINT nID, const CString& sName, LPCTSTR pText, BOOL bIsMultiLine)
{
	CBCGPRibbonLabel* lab = new CBCGPRibbonLabel(pText, bIsMultiLine);
	if (!lab) return FALSE;
	lab->SetID(nID);
	__super::AddButton(lab);
	AddOslInfo(nID, sName);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonButtonsGroup::AddEdit(UINT nID, const CString& sName, LPCTSTR lpszLabel,  int nIDimage, CString aLibNamespace, int nWidth, BOOL bPNG)
{
	int nImage = -1; /*AddButtonsGroupIcon(pButtGroup, nIDimage, aLibNamespace, bPNG);*/
	CBCGPRibbonEdit* edit = new CBCGPRibbonEdit(nID, nWidth, lpszLabel, nImage);
	if (!edit) return FALSE;
	__super::AddButton(edit);
	AddOslInfo(nID, sName);
	return TRUE;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonButtonsGroup::AddCheckBox(UINT nID, const CString& sName, const CString& text)
{
	CTBRibbonCheckBox* pCheckBox = new CTBRibbonCheckBox(nID, text);
	if (!pCheckBox) return FALSE;
	__super::AddButton(pCheckBox);
	AddOslInfo(nID, sName);
	return TRUE;
}

//-------------------------------------------------------------------------------------
int CTBRibbonButtonsGroup::AddButtonsGroupIcon(UINT nIdImg, const CString& aLibNamespace, BOOL bPNG)
{
	CBCGPRibbonBar* pRibbonBar = m_pParent->GetParentRibbonBar();
	if (!pRibbonBar)
	{
		ASSERT_TRACE(FALSE,_T("Cannot have a ribbon bar"));
		return FALSE;
	}

	CDC* pDC = pRibbonBar->GetDC();

	HICON pIco = TBLoadImage(pDC, NULL, nIdImg, RIBBONBAR_LARGE_ICO, bPNG);
	pRibbonBar->ReleaseDC(pDC);

	if (!pIco) return -1;
	GetImages().SetImageSize(CSize(RIBBONBAR_LARGE_ICO, RIBBONBAR_LARGE_ICO));
	return GetImages().AddIcon(pIco);
}

/////////////////////////////////////////////////////////////////////////////
//					CTBRibbonCheckBox
/////////////////////////////////////////////////////////////////////////////
CTBRibbonCheckBox::CTBRibbonCheckBox()
{	
}

//-------------------------------------------------------------------------------------
CTBRibbonCheckBox::CTBRibbonCheckBox (UINT	nID, LPCTSTR lpszText) :
	CBCGPRibbonCheckBox(nID, lpszText)
{
}

//-------------------------------------------------------------------------------------
CTBRibbonCheckBox::~CTBRibbonCheckBox()
{
}

//-------------------------------------------------------------------------------------
void CTBRibbonCheckBox::SetCheckBox(BOOL bCheckBox)
{
	this->m_bIsChecked = (BOOL) bCheckBox;
	this->OnCheck ((BOOL) bCheckBox);
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonCheckBox::GetCheckBox()
{
	BOOL bRet = (BOOL) this->m_bIsChecked;
	return bRet;
}

//-------------------------------------------------------------------------------------
void CTBRibbonCheckBox::OnClick (CPoint point)
{
	BOOL bRet = (BOOL) this->m_bIsChecked;
	this->m_bIsChecked = (BOOL) !bRet;
	this->OnCheck ((BOOL) !bRet);
	__super::OnClick (point);
}

/////////////////////////////////////////////////////////////////////////////
//					CTBRibbonBar
/////////////////////////////////////////////////////////////////////////////

CTBRibbonBar::CTBRibbonBar()
	: 
	CBCGPRibbonBar	(FALSE)
{
}

//-------------------------------------------------------------------------------------
CTBRibbonBar::~CTBRibbonBar()
{
}

//-------------------------------------------------------------------------------------
CString CTBRibbonBar::GetName()
{
	return GetInfoOSL()->m_Namespace.GetObjectName();
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonBar::Create (CWnd* pParentWnd, CString aName, DWORD dwStyle)
{
	if (aName.IsEmpty())
	{
		ASSERT_TRACE(FALSE, "CTBRibbonBar::Create: ribbon bar name is empty!");
		return FALSE;
	}

	if (!__super::Create(pParentWnd))
	{
		ASSERT_TRACE(FALSE, "CTBRibbonBar::Create: cannot create ribbon bar!");
		return FALSE;
	}

	m_sName = aName;
	if (pParentWnd && pParentWnd->IsKindOf(RUNTIME_CLASS(CLocalizableFrame)))
	{
		CLocalizableFrame* pFrame = (CLocalizableFrame*)pParentWnd;
		CDocument* pDoc = pFrame->GetActiveDocument();
		if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
			AttachOSLInfo(((CBaseDocument*)pDoc)->GetInfoOSL());
			
		pFrame->EnableDocking(CBRS_ALIGN_ANY);
	}
	
	EnableDocking(CBRS_ALIGN_ANY);
	return TRUE;
}

//-------------------------------------------------------------------------------------
CSize CTBRibbonBar::CalcFixedLayout(BOOL bStretch, BOOL bHorz)
{
	CSize sizeRet = __super::CalcFixedLayout(bStretch, bHorz);
	return sizeRet;
}

//-------------------------------------------------------------------------------------
void CTBRibbonBar::AttachOSLInfo (CInfoOSL* pParent)
{
	CTBNamespace aNs;
	aNs.SetChildNamespace(CTBNamespace::TOOLBAR, m_sName, pParent->m_Namespace);
	
	GetInfoOSL()->m_pParent = pParent;
	GetInfoOSL()->SetType(OSLType_Control);	//TODO OSL da vedere con Riccardo contenitore
	GetInfoOSL()->m_Namespace = aNs;

	if (!aNs.IsEmpty() && aNs.IsValid())
		AfxGetSecurityInterface()->GetObjectGrant(GetInfoOSL());
}

//-------------------------------------------------------------------------------------
CTBRibbonCategory*  CTBRibbonBar::AddCategory (LPCTSTR	lpszName, const CString& sText, UINT nSmallImageID /*= 0*/, UINT nLargeImageID /*= 0*/)
{
	CTBRibbonCategory* pCategory = (CTBRibbonCategory*) __super::AddCategory
		(
			sText, 
			nSmallImageID,
			nLargeImageID,
			RIBBONBAR_SMALL_ICO, 
			RIBBONBAR_LARGE_ICO, 
			-1, 
			RUNTIME_CLASS(CTBRibbonCategory)
		);

	if (pCategory)
		pCategory->AttachOSLInfo(GetInfoOSL(), lpszName);

	return pCategory;
}

//-------------------------------------------------------------------------------------
void CTBRibbonBar::SetCheckBox(UINT nID, BOOL bCheckBox)
{
	CArray<CBCGPBaseRibbonElement*, CBCGPBaseRibbonElement*> arButtons;
	this->GetElementsByID(nID, arButtons, TRUE);
	int size = arButtons.GetSize();
	ASSERT(size == 1);
	CTBRibbonCheckBox* pCheckBox = DYNAMIC_DOWNCAST (CTBRibbonCheckBox, arButtons [0]);
	pCheckBox->SetCheckBox(bCheckBox);
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonBar::GetCheckBox(UINT nID)
{
	CArray<CBCGPBaseRibbonElement*, CBCGPBaseRibbonElement*> arButtons;
	this->GetElementsByID(nID, arButtons, TRUE);
	int size = arButtons.GetSize();
	ASSERT(size == 1);
	CTBRibbonCheckBox* pCheckBox = DYNAMIC_DOWNCAST (CTBRibbonCheckBox, arButtons [0]);
	return pCheckBox->GetCheckBox();
}

//-------------------------------------------------------------------------------------
void CTBRibbonBar::SetTextContent(UINT nID, const CString& text)
{
	CArray<CBCGPBaseRibbonElement*, CBCGPBaseRibbonElement*> arButtons;
	this->GetElementsByID(nID, arButtons, TRUE);
	int size = arButtons.GetSize();
	ASSERT(size == 1);
	// set text in Edit box derivate from button
	CBCGPRibbonEdit* pEdit = DYNAMIC_DOWNCAST (CBCGPRibbonEdit, arButtons [0]);
	if (pEdit)	pEdit->SetEditText(text);
}

//-----------------------------------------------------------------------------
CString CTBRibbonBar::GetTextContent(UINT nID)
{
	CArray<CBCGPBaseRibbonElement*, CBCGPBaseRibbonElement*> arButtons;
	this->GetElementsByID(nID, arButtons, TRUE);
	int size = arButtons.GetSize();
	ASSERT(size == 1);
	// set text in Edit box derivate from button
	CBCGPRibbonEdit* pEditBox = DYNAMIC_DOWNCAST (CBCGPRibbonEdit, arButtons [0]);
	if (!pEditBox) return _T("");
	return pEditBox->GetEditText();
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel* CTBRibbonBar::AddPanel (CTBRibbonCategory* pCat, LPCTSTR lpszName, const CString& sText, UINT nIdImg, BOOL bPNG)
{
	HICON pIco = NULL;
	
	if (nIdImg > 0)
	{
		CDC* pDC = GetDC();
		pIco = TBLoadImage(pDC, NULL, nIdImg, RIBBONBAR_LARGE_ICO, bPNG);
		ReleaseDC(pDC);
	}

	CTBRibbonPanel* pPanel = pCat->AddPanel(lpszName, sText, pIco);
	return pPanel;
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel* CTBRibbonBar::GetPanel(const CString& sName)
{
	for (int i = 0; i < m_arCategories.GetSize(); i++)
	{
		CBCGPRibbonCategory* pCategory = m_arCategories.GetAt(i);
		if (pCategory->IsKindOf(RUNTIME_CLASS(CTBRibbonCategory)))
		{
			CTBRibbonPanel* pPanel = ((CTBRibbonCategory*)pCategory)->GetPanel(sName);
			if (pPanel)
				return pPanel;
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
CTBRibbonPanel* CTBRibbonBar::FindOrAddPanel(const CString& sName, const CString& sText /*= _T("")*/)
{
	CTBRibbonPanel* pPanel = GetPanel(sName);
	if (pPanel) return pPanel;

	for (int i = 0; i < m_arCategories.GetSize(); i++)
	{
		CBCGPRibbonCategory* pCategory = m_arCategories.GetAt(i);
		if (pCategory->IsKindOf(RUNTIME_CLASS(CTBRibbonCategory)))
		{
			CTBRibbonPanel* pPanel = ((CTBRibbonCategory*)pCategory)->AddPanel(sName, sText);
			return pPanel;
		}
	}
	return NULL;
}

//-------------------------------------------------------------------------------------
CTBRibbonCategory*	CTBRibbonBar::GetCategory(const CString& sName)
{
	for (int i=0; i < m_arCategories.GetSize(); i++)
	{
		CBCGPRibbonCategory* pCategory = m_arCategories.GetAt(i);
		if (pCategory->IsKindOf(RUNTIME_CLASS(CTBRibbonCategory)) && sName.CompareNoCase(pCategory->GetName()) == 0)
			return (CTBRibbonCategory*) pCategory;
	}

	return NULL;
}

//-------------------------------------------------------------------------------------
CTBRibbonCategory* CTBRibbonBar::GetCategory (int nIndex) const
{
	CBCGPRibbonCategory* pCategory = __super::GetCategory(nIndex);
	if (pCategory->IsKindOf(RUNTIME_CLASS(CTBRibbonCategory)))
		return (CTBRibbonCategory*) pCategory;
	return NULL;
}

//-------------------------------------------------------------------------------------
BOOL CTBRibbonBar::HideButton (UINT nID, BOOL bHide)
{
	POSITION  pos = lstHiddenButtons.Find(nID);
	if (bHide)
	{
		if (pos) return TRUE;
		lstHiddenButtons.AddHead(nID);
	}
	else
	{
		if (!pos) return TRUE;
		lstHiddenButtons.RemoveAt(pos);
	}
	
	CBCGPToolBar::SetNonPermittedCommands(lstHiddenButtons);
	return TRUE;
}
