
#include "stdafx.h"

#include <TbGeneric\minmax.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\ParametersSections.h>
#include <TBGeneric\WndObjDescription.h>

#include <TbGenlib\reswalk.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>
#include <TbOledb\sqlrec.h>

#include "hotlink.h"
#include "tabber.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//==============================================================================================

CGroupControls::CGroupControls (CWnd* pMaster, UINT nTemplateFormIDD)
	:
	m_nPlaceHolderIDC	(0),
	m_nTemplateFormIDD	(nTemplateFormIDD),
	m_pDlgTemplate		(NULL),
	m_pTab				(NULL),
	m_pView				(NULL),
	m_bBuilt			(FALSE)
{
	ASSERT_VALID(pMaster);
	if (pMaster->IsKindOf(RUNTIME_CLASS(CTabDialog)))
		m_pTab = (CTabDialog*) pMaster;
	else if (pMaster->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		m_pView = (CAbstractFormView*) pMaster;
	else
		ASSERT(FALSE);
}

CGroupControls::~CGroupControls ()
{

}

//-----------------------------------------------------------------------------
UINT CGroupControls::LookupIDC (UINT nBornIDC) const 
{
	LinkInfo* pL = FindLink(nBornIDC);
	//ASSERT(pL);
	return pL ? pL->m_nIDC : nBornIDC;
}

//------------------------------------------------------------------------------
CParsedCtrl* CGroupControls::AddLink (UINT nBornIDC)
{
	LinkInfo* pL = FindLink(nBornIDC);
	if (!pL)
	{
		ASSERT(FALSE);
		TRACE1 ("\nCGroupControls::AddLink with  IDC %d\n", nBornIDC);
		return NULL;
	}
	if (/*pL->m_nBornIDC == pL->m_nIDC ||*/ pL->m_nIDC == 0)
	{
		ASSERT(FALSE);
		TRACE1 ("\nCGroupControls::AddLink without new IDC %d\n", nBornIDC);
		return NULL;
	}

	return m_pTab ?
		m_pTab->AddLink (pL->m_nIDC, pL->m_sName, pL->m_pRec, pL->m_pData, pL->m_pRTC, pL->m_pHKL, pL->m_nBtnID)
		:
		m_pView->AddLink (pL->m_nIDC, pL->m_sName, pL->m_pRec, pL->m_pData, pL->m_pRTC, pL->m_pHKL, pL->m_nBtnID)
		;
}

CExtButton* CGroupControls::AddExtButtonLink (UINT nBornIDC)
{
	LinkInfo* pL = FindLink(nBornIDC);
	if (!pL)
	{
		ASSERT(FALSE);
		TRACE1 ("\nCGroupControls::AddLink with  IDC %d\n", nBornIDC);
		return NULL;
	}
	if (pL->m_nBornIDC == pL->m_nIDC || pL->m_nIDC == 0)
	{
		ASSERT(FALSE);
		TRACE1 ("\nCGroupControls::AddLink without new IDC %d\n", nBornIDC);
		return NULL;
	}

	return m_pTab ?
		m_pTab->AddLink (pL->m_nIDC, pL->m_sName, pL->m_pRec, pL->m_pData)
		:
		m_pView->AddLink (pL->m_nIDC, pL->m_sName, pL->m_pRec, pL->m_pData)
		;
}

//-----------------------------------------------------------------------------
CLabelStatic* CGroupControls::AddLabelLink (UINT nBornIDC)
{
	return m_pTab ?
		m_pTab->AddLabelLink (LookupIDC(nBornIDC))
		:
		m_pView->AddLabelLink (LookupIDC(nBornIDC));
		;
}

CGroupBoxBtn* CGroupControls::AddGroupBoxLink (UINT nBornIDC)
{
	return m_pTab ?
		m_pTab->AddGroupBoxLink (LookupIDC(nBornIDC))
		:
		m_pView->AddGroupBoxLink (LookupIDC(nBornIDC));
		;
}

//---------------------------------------------------------------------------
void CGroupControls::HideControlGroup (UINT nIDC, BOOL bHide/* = TRUE*/)	
{ 
	if (GetMasterWnd () == NULL) return;
	::HideControlGroup(GetMasterWnd (), LookupIDC(nIDC), bHide); 
}

//---------------------------------------------------------------------------
BOOL CGroupControls::HideControl (UINT nIDC, BOOL bHide/* = TRUE*/)	
{ 
	if (GetMasterWnd () == NULL) return FALSE;
	return m_pTab ?
		m_pTab->HideControl (LookupIDC(nIDC), bHide)
		:
		m_pView->HideControl (LookupIDC(nIDC), bHide);
		;
}

//-----------------------------------------------------------------------------
BOOL CGroupControls::AddGroupControlsLink (UINT nPlaceHolderIDC, BOOL bBuild/* = TRUE*/)
{
	ASSERT(nPlaceHolderIDC);
	m_nPlaceHolderIDC = nPlaceHolderIDC;

	InitLinks();

	RebindLinks();

	if (bBuild)
	{
		return Build();
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CGroupControls::Build ()
{
	if (m_bBuilt)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bBuilt = TRUE;

	if (!CreateControls())
		return FALSE;

	BuildDataControlLinks();
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CGroupControls::CreateControls ()
{
	if (!m_nTemplateFormIDD)
	{
		ASSERT(FALSE);
		TRACE1 ("\nCGroupControls::CreateControls template resource IDD %d is missing\n", m_nPlaceHolderIDC);
		return FALSE;
	}

	if (!m_pDlgTemplate)
	{
		m_pDlgTemplate = new CLocalizableDialog();
		m_pDlgTemplate->m_bAutoDestroy = TRUE;

		if (!m_pDlgTemplate->Create(m_nTemplateFormIDD/*, GetMasterWnd()*/))
		{
			ASSERT(FALSE);
			TRACE1 ("\nCGroupControls::CreateControls fails to create template resource IDD %d\n", m_nTemplateFormIDD);
			return FALSE;
		}
	}

	CWnd* wndPH = GetMasterWnd()->GetDlgItem(m_nPlaceHolderIDC);
	if (!wndPH)
	{
		ASSERT(FALSE);
		TRACE1 ("\nCGroupControls::CreateControls cannot find place holder IDC %d into the form\n", m_nPlaceHolderIDC);
		return FALSE;
	}

	CRect rectPH;
	wndPH->GetWindowRect(&rectPH);
	GetMasterWnd()->ScreenToClient(rectPH);
	wndPH->DestroyWindow();
	//----

	// scorre tutte le child window della dialog
	for (CWnd* pCtrl = m_pDlgTemplate->GetWindow(GW_CHILD); pCtrl; pCtrl = pCtrl->GetNextWindow())
	{
		if (!pCtrl || !pCtrl->m_hWnd)
			continue;
		//----
		CRect rectCtrl;
		pCtrl->GetWindowRect(&rectCtrl);
		m_pDlgTemplate->ScreenToClient(rectCtrl);

		rectCtrl.OffsetRect(rectPH.left, rectPH.top);
		//----
		UINT nIDC = pCtrl->GetDlgCtrlID();
		if (nIDC != IDC_STATIC)
			nIDC = LookupIDC(nIDC);
		//----
		CString sCaption;	
		//pCtrl->GetWindowText(sCaption);
		//----
		DWORD dwExStyle = pCtrl->GetExStyle();
		DWORD dwWinStyle = pCtrl->GetStyle();
		//----
		TCHAR szClassName[MAX_CLASS_NAME+1];
		GetClassName(pCtrl->m_hWnd, szClassName, MAX_CLASS_NAME);
		//----

		if (_tcsicmp(szClassName, L"EDIT") == 0)
		{
			CBCGPEdit edt;
			VERIFY(edt.CreateEx(dwExStyle, L"EDIT", NULL, dwWinStyle, rectCtrl, GetMasterWnd(), nIDC));
			//VERIFY(edt.Create(dwWinStyle, rectCtrl, GetMasterWnd(), nIDC));
			edt.SetFont(AfxGetThemeManager()->GetControlFont(), FALSE);
			edt.UnsubclassWindow();
		}
		else if (_tcsicmp(szClassName, L"BUTTON") == 0)
		{
			pCtrl->GetWindowText(sCaption);

			CButton btn;
			VERIFY(btn.CreateEx(dwExStyle, L"BUTTON", sCaption, dwWinStyle, rectCtrl, GetMasterWnd(), nIDC));
			btn.SetFont(AfxGetThemeManager()->GetControlFont(), FALSE);
			btn.UnsubclassWindow();
		}
		else if (_tcsicmp(szClassName, L"STATIC") == 0)
		{
			pCtrl->GetWindowText(sCaption);

			CStatic stn;
			VERIFY(stn.CreateEx(dwExStyle, L"STATIC", sCaption, dwWinStyle, rectCtrl, GetMasterWnd(), nIDC));
			stn.SetFont(AfxGetThemeManager()->GetControlFont(), FALSE);
			stn.UnsubclassWindow();
		}
		else if (_tcsicmp(szClassName, L"COMBOBOX") == 0)
		{
			CBCGPComboBox cbx;
			VERIFY(cbx.CreateEx (dwExStyle, L"COMBOBOX", NULL, dwWinStyle, rectCtrl, GetMasterWnd(), nIDC));
			cbx.SetFont(AfxGetThemeManager()->GetControlFont(), FALSE);
			cbx.UnsubclassWindow();
		}
		else if (_tcsicmp(szClassName, L"LISTBOX") == 0)
		{
			CTBListBox lbx;
			VERIFY(lbx.CreateEx(dwExStyle, L"LISTBOX", NULL, dwWinStyle, rectCtrl, GetMasterWnd(), nIDC));
			lbx.SetFont(AfxGetThemeManager()->GetControlFont(), FALSE);
			lbx.UnsubclassWindow();
		}
		else
		{
			pCtrl->GetWindowText(sCaption);
			ASSERT(FALSE);
			TRACE3 ("\nCGroupControls::CreateControls found control IDC %d with caption %s and unsupported classname %s\n", 
				pCtrl->GetDlgCtrlID(), sCaption, szClassName);
		}
	}
	return TRUE;
}

//==============================================================================================
/*
LONG_PTR WINAPI GetWindowLongPtr(
  _In_  HWND hWnd,
  _In_  int nIndex
);

LONG WINAPI GetWindowLong(
  _In_  HWND hWnd,
  _In_  int nIndex
);

*/