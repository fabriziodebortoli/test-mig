#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>

#include "parsobj.h"
#include "BaseDoc.h"
#include "OslBaseInterface.h"
#include "TBCaptionBar.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR  szHyperLink[]	= _T("Hyperlink");
static const TCHAR  szButton[]		= _T("Button");
static const TCHAR  szImage[]		= _T("Image");
/////////////////////////////////////////////////////////////////////////////
//					CTaskBuilderCaptionBar
/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CTaskBuilderCaptionBar, CBCGPCaptionBar)
	ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,			OnGetControlDescription)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------
CTaskBuilderCaptionBar::CTaskBuilderCaptionBar()
{
}

//-------------------------------------------------------------------------------------------
CTaskBuilderCaptionBar::~CTaskBuilderCaptionBar()
{
}

//-------------------------------------------------------------------------------------------
BOOL CTaskBuilderCaptionBar::Create(UINT nID, CWnd* pParentWnd)
{
	ASSERT(pParentWnd);

	if (!__super::Create (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, pParentWnd, nID, -1, TRUE))
	{
		TRACE0("Failed to create TB Caption Bar\n");
		return FALSE;
	}

	SetFlatBorder(AfxGetThemeManager()->UseFlatStyle());
	SetBkgColor(AfxGetThemeManager()->GetCaptionBarBkgColor());
	SetTextColor(AfxGetThemeManager()->GetCaptionBarForeColor());
	SetBorderColor(AfxGetThemeManager()->GetCaptionBarBorderColor());
	SetFont(AfxGetThemeManager()->GetFormFont());

	return TRUE;
}

//-------------------------------------------------------------------------------------------
CArray<CInfoOSL*>& CTaskBuilderCaptionBar::GetOSLInfos()
{
	if (m_OSLInfos.GetSize() == 0)
	{
		CFrameWnd* pParent = dynamic_cast<CFrameWnd*>(GetParent());
		if (pParent)
		{
			CBaseDocument* pDocument = dynamic_cast<CBaseDocument*>(pParent->GetActiveDocument());
			if (pDocument && pDocument->GetInfoOSL())
				AttachOSLInfos(pDocument->GetInfoOSL());
		}
	}

	return m_OSLInfos;
}

//-------------------------------------------------------------------------------------------
COLORREF CTaskBuilderCaptionBar::GetBkgColor () const
{
	return m_clrBarBackground;
}

//-------------------------------------------------------------------------------------------
COLORREF CTaskBuilderCaptionBar::GetTextColor () const
{
	return m_clrBarText;
}
//-------------------------------------------------------------------------------------------
COLORREF CTaskBuilderCaptionBar::GetBorderColor	() const
{
	return m_clrBarBorder;
}

//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::SetBkgColor (COLORREF crBkg)
{
	m_clrBarBackground = crBkg;
}

//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::SetTextColor	(COLORREF crText)
{
	m_clrBarText = crText;
}
//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::SetBorderColor	(COLORREF crText)
{
	m_clrBarBorder = crText;
}

//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::SetImage (UINT nID)
{
	__super::SetBitmap(nID, AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
}

//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::SetText (const CString& strText, AlignStyle align /*ALIGN_RIGHT*/)
{
	CString sText = strText;
	if (!CanExecuteLink())
		sText.Replace(_T("\a"), _T(""));

	CBCGPCaptionBar::BarElementAlignment barAlign; 
	switch (align)
	{
		case ALIGN_RIGHT:	barAlign = CBCGPCaptionBar::ALIGN_RIGHT;	break;
		case ALIGN_CENTER:	barAlign = CBCGPCaptionBar::ALIGN_CENTER;	break;
		default:
			barAlign = CBCGPCaptionBar::ALIGN_LEFT;	break;
	}
	__super::SetText(sText, barAlign);
}

//-------------------------------------------------------------------------------------------
BOOL CTaskBuilderCaptionBar::OnClickCloseButton()
{	
	return FALSE;
}

//-------------------------------------------------------------------------------------------
CInfoOSL* CTaskBuilderCaptionBar::GetOSLInfoByName(const CString& sName)
{
	for (int i =0; i < GetOSLInfos().GetSize(); i++)
	{
		CInfoOSL* pInfo = GetOSLInfos().GetAt(i);
		if (pInfo && pInfo->m_Namespace.GetObjectName().CompareNoCase(sName) == 0)
			return pInfo;
	}
	return NULL;
}

//-------------------------------------------------------------------------------------------
BOOL CTaskBuilderCaptionBar::CanExecuteLink()
{
	CInfoOSL* pInfo = GetOSLInfoByName(szHyperLink);
	return !pInfo || OSL_CAN_DO( pInfo, OSL_GRANT_EXECUTE);
}

//-------------------------------------------------------------------------------------------
BOOL CTaskBuilderCaptionBar::CanClickImage()
{
	CInfoOSL* pInfo = GetOSLInfoByName(szImage);
	return !pInfo || OSL_CAN_DO( pInfo, OSL_GRANT_EXECUTE);
}

//-------------------------------------------------------------------------------------------
BOOL CTaskBuilderCaptionBar::CanClickButton()
{
	CInfoOSL* pInfo = GetOSLInfoByName(szButton);
	return !pInfo || OSL_CAN_DO( pInfo, OSL_GRANT_EXECUTE);
}

//-------------------------------------------------------------------------------------------
BOOL CTaskBuilderCaptionBar::CanExecuteCommand	(UINT nID, UINT nCode)
{
	if (nID == m_uiBtnID && nCode == BN_CLICKED)
		return CanClickButton();

	return TRUE;
}

//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::SetButton (const CString sTitle, UINT nID, BOOL bDropDown, AlignStyle align /*ALIGN_RIGHT*/)
{
	__super::SetButton(sTitle, nID, (CBCGPCaptionBar::BarElementAlignment) align, bDropDown);
	EnableButton(CanClickButton());		
}

//-------------------------------------------------------------------------------------------
void CTaskBuilderCaptionBar::AttachOSLInfos (CInfoOSL* pParentInfo)
{
	if (!pParentInfo)
		return;

	// parent caption bar
	CInfoOSL* pCaptonInfo = new CInfoOSL(OSLType_Control);
	pCaptonInfo->m_pParent = pParentInfo;
	CTBNamespace aCaptionNs;
	aCaptionNs.AutoCompleteNamespace(CTBNamespace::TOOLBAR, _T("CaptionBar"), pCaptonInfo->m_pParent->m_Namespace.ToString());
	pCaptonInfo->m_Namespace = aCaptionNs;
	m_OSLInfos.Add(pCaptonInfo);
	AfxGetSecurityInterface()->GetObjectGrant (pCaptonInfo);

	// hyperlink
	CTBNamespace aNs;
	CInfoOSL* pHyperLinkInfo = new CInfoOSL(OSLType_Control);
	pHyperLinkInfo->m_pParent = pCaptonInfo;
	aNs.AutoCompleteNamespace(CTBNamespace::CONTROL, szHyperLink, aCaptionNs.ToString());
	pHyperLinkInfo->m_Namespace = aNs;
	m_OSLInfos.Add(pHyperLinkInfo);
	AfxGetSecurityInterface()->GetObjectGrant (pHyperLinkInfo);

	// button
	aNs.Clear();
	CInfoOSL* pButtonInfo = new CInfoOSL(OSLType_Control);
	pButtonInfo->m_pParent = pCaptonInfo;
	aNs.AutoCompleteNamespace(CTBNamespace::CONTROL, szButton, aCaptionNs.ToString());
	pButtonInfo->m_Namespace = aNs;
	m_OSLInfos.Add(pButtonInfo);
	AfxGetSecurityInterface()->GetObjectGrant (pButtonInfo);

	// image
	aNs.Clear();
	CInfoOSL* pImageInfo = new CInfoOSL(OSLType_Control);
	pImageInfo->m_pParent = pCaptonInfo;
	aNs.AutoCompleteNamespace(CTBNamespace::CONTROL, szImage, aCaptionNs.ToString());
	pImageInfo->m_Namespace = aNs;
	m_OSLInfos.Add(pImageInfo);
	AfxGetSecurityInterface()->GetObjectGrant (pImageInfo);
}

//-----------------------------------------------------------------------------
LRESULT CTaskBuilderCaptionBar::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;

	CFrameWnd* pParentFrame = GetParentFrame();

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CCaptionBarDescription* pCaptionBarDesc = (CCaptionBarDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CCaptionBarDescription), strId);	
	pCaptionBarDesc->UpdateAttributes(this);

	pCaptionBarDesc->m_strCmd = cwsprintf(_T("%d%d"), GetParent()->m_hWnd, BCGM_ON_CLICK_CAPTIONBAR_HYPERLINK);
	if (pCaptionBarDesc->m_strText.CompareNoCase(GetText())  == 0)
	{
		pCaptionBarDesc->m_strText = GetText();
		pCaptionBarDesc->m_nTextAlign = (int) GetAlignment(CBCGPCaptionBar::ELEM_TEXT);
		pCaptionBarDesc->SetUpdated(&pCaptionBarDesc->m_nTextAlign);
	}

	if (this->m_Bitmap.GetCount() == 1)
	{
		CWndImageDescription* pImageDescription = new CWndImageDescription(pCaptionBarDesc);
		pImageDescription->SetID(szImage);
		pImageDescription->m_strText = m_strImageDescription;
		pImageDescription->m_strHint = m_strImageToolTip;

		CImage aImage;
		aImage.Attach(this->m_Bitmap.GetImageWell());
		pImageDescription->m_ImageBuffer.Assign(&aImage, szImage);
		aImage.Detach();	
		pImageDescription->m_strHint = m_strImageToolTip;
		pCaptionBarDesc->m_Children.Add(pImageDescription);

		pCaptionBarDesc->m_nImageAlign = (int) GetAlignment(CBCGPCaptionBar::ELEM_ICON);
		pCaptionBarDesc->SetUpdated(&pCaptionBarDesc->m_nImageAlign);
}

	pCaptionBarDesc->m_bHasButton = m_uiBtnID != 0;
	if (pCaptionBarDesc->m_bHasButton)
	{
		CWndButtonDescription* pButtonDescription = new CWndButtonDescription(pCaptionBarDesc);
		pButtonDescription->SetID(szButton);
		pButtonDescription->m_strCmd = cwsprintf(_T("%d"), m_uiBtnID);
		pButtonDescription->SetRect(GetButtonRect(), TRUE);
		pButtonDescription->m_bEnabled = TRUE == m_bBtnEnabled;
		pButtonDescription->m_strText = m_strButtonDescription;
		pButtonDescription->m_strHint = m_strButtonToolTip;
		
		pCaptionBarDesc->m_Children.Add(pButtonDescription);
		
		pCaptionBarDesc->m_nButtonAlign = (int) GetAlignment(CBCGPCaptionBar::ELEM_BUTTON);
	}

	pCaptionBarDesc->AddChildWindows(this);

	return (LRESULT) pCaptionBarDesc;
}
