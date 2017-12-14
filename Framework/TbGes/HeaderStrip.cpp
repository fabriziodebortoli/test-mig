#include "stdafx.h"

#include "HeaderStrip.h"
#include "JsonFormEngineEx.h"
#include "HeaderStrip.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//	             class CHeaderTileGroup implementation
/////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CHeaderTileGroup, CTileGroup)

//-----------------------------------------------------------------------------
CHeaderTileGroup::CHeaderTileGroup()
{ 
	SetLayoutType(CLayoutContainer::HBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);
	SetTileDialogStyle(AfxGetTileDialogStyleHeader());
	SetFlex(0);
}

//-----------------------------------------------------------------------------
void CHeaderTileGroup::Customize()
{ 
}


/////////////////////////////////////////////////////////////////////////////
//	             class CHeaderStripTile declaration & implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class CHeaderStripTile : public CTileDialog
{
	friend class CHeaderStrip;

	DECLARE_DYNCREATE(CHeaderStripTile)

public:
	CHeaderStripTile();

protected:
	virtual void BuildDataControlLinks	();
	virtual BOOL OnInitDialog			();

private:
	void SetCaption		(const CString& strCaption);

private:
	CLabelStatic* m_pLabel;
};

//=================================================
IMPLEMENT_DYNCREATE(CHeaderStripTile, CTileDialog)

//-----------------------------------------------------------------------------
CHeaderStripTile::CHeaderStripTile()
	:
	CTileDialog	(_T("HeaderStrip"), IDD_HEADERSTRIP_TILE),
	m_pLabel	(NULL)
{
	SetMaxWidth(FREE);
}

//-----------------------------------------------------------------------------
BOOL CHeaderStripTile::OnInitDialog()
{	
	__super::OnInitDialog();

	SetFont(AfxGetThemeManager()->GetTileStripFont());
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void CHeaderStripTile::BuildDataControlLinks()
{
	m_pLabel = AddLabelLink(IDC_HEADERSTRIP_CAPTION);  

	m_pLabel->SetOwnFont(AfxGetThemeManager()->GetTileStripFont());
	//@@@TODO scoprire perche` senza forzare questi parametri non imposta bene il colore del font
	m_pLabel->SetOwnFont(FALSE, FALSE, FALSE);
	m_pLabel->SetCustomDraw(FALSE);
	m_pLabel->SetBkgColor(AfxGetTileDialogStyleHeader()->GetBackgroundColor());
	m_pLabel->SetTextColor(AfxGetTileDialogStyleHeader()->GetTitleForeColor());

	CDC* pDC = m_pLabel->GetWindowDC();
	CSize size = GetTextSize(pDC, _T("CALCOLOFONT"), AfxGetThemeManager()->GetTileStripFont());
	ReleaseDC(pDC);

	CRect headerRect;
	GetClientRect(headerRect);

	m_pLabel->SetWindowPos(NULL, 10, (headerRect.Height() - size.cy) / 2, headerRect.Width() - 10, headerRect.Height() - (headerRect.Height() - size.cy) / 2, SWP_NOACTIVATE);

}

//-----------------------------------------------------------------------------
void CHeaderStripTile::SetCaption(const CString& strCaption)
{
	m_pLabel->SetWindowText(strCaption);
}

/////////////////////////////////////////////////////////////////////////////
//	             class CHeaderStrip implementation
/////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CHeaderStrip, CHeaderTileGroup)
BEGIN_MESSAGE_MAP(CHeaderStrip, CHeaderTileGroup)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
CHeaderStrip::CHeaderStrip()
	:
	m_pHeaderStripTile(NULL)
{ 
}

//-----------------------------------------------------------------------------
void CHeaderStrip::Customize()
{ 
	__super::Customize();

	m_pHeaderStripTile = (CHeaderStripTile*)AddTile(RUNTIME_CLASS(CHeaderStripTile), IDD_HEADERSTRIP_TILE, _T(""), TILE_STANDARD, 1);
}

//-----------------------------------------------------------------------------
void CHeaderStrip::RebuildLinks(SqlRecord* pRec)
{
	__super::RebuildLinks(pRec);

	UpdateCaption();
}

//-----------------------------------------------------------------------------
void CHeaderStrip::SetCaption(const CString& strCaption)
{
	if (m_pHeaderStripTile)
	{
		if (strCaption.IsEmpty() && GetDocument())
			m_pHeaderStripTile->SetCaption(GetDocument()->GetTitle());
		else
			m_pHeaderStripTile->SetCaption(strCaption);
	}
}

//-----------------------------------------------------------------------------
LRESULT CHeaderStrip::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	CString strId = (LPCTSTR)lParam;
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);
	pDesc->m_Type = CWndObjDescription::HeaderStrip;
	pDesc->UpdateAttributes(this);
	
	return (LRESULT)pDesc;
}
//-----------------------------------------------------------------------------
CHeaderStripFormat* CHeaderStrip::Add(DataObj* pObj)
{
	CHeaderStripFormat* ev = new CHeaderStripFormat(pObj, this);
	m_HeaderFields.Add(ev);
	pObj->AttachEvents(ev); //farà lui la delete
	return ev;
}

//-----------------------------------------------------------------------------
void CHeaderStrip::UpdateCaption()
{
	CString aCaption = _T("");
	if (m_pView)
		aCaption = m_pView->GetCaption();
	SetCaption(aCaption);
}

//-----------------------------------------------------------------------------
CHeaderStrip* CHeaderStrip::AddHeaderStrip(CAbstractFormView* pView, UINT nIDC, const CString& strDefaultCaption, BOOL bCallInitialUpdate /*= TRUE*/, CRect rectWnd /*= CRect(0, 0, 0, 0)*/, CRuntimeClass* pClass /*= NULL*/)
{
	if (!pView)
	{
		ASSERT(FALSE);
		return NULL;
	}
	if (!pClass)
		pClass = RUNTIME_CLASS(CJsonHeaderStrip);

	CHeaderStrip* pStrip = (CHeaderStrip*)pView->AddTileGroup(nIDC, pClass, _NS_TABDLG("HeaderStrip"), bCallInitialUpdate, rectWnd);
	pStrip->m_pView = pView;

	if (pView->GetDocument())
	{
		DataObjArray ar;
		ar.SetOwns(FALSE);
		pView->GetDocument()->GetDocumentObjectCaptionDataObjArray(ar);

		for (int i = 0; i < ar.GetCount(); i++)
			pStrip->Add(ar.GetAt(i));
	}

	pStrip->SetCaption(strDefaultCaption);

	return pStrip;
}

