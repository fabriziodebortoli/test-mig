#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>

#include <TbGenlib\Baseapp.h>
#include <TbGenlib\ParsCtrl.h>

#include "ContextSelectionDialog.h"

// risorse
#include "ContextSelectionDialog.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const CString szFmtSep = _T(" : ");

//============================================================================
//			 Dialog di selezione di un contesto
//============================================================================

//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CContextSelectionDialog, CParsedDialog)
//--------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CContextSelectionDialog, CParsedDialog)
	//{{AFX_MSG_MAP(CContextSelectionDialog)
	ON_CBN_SELCHANGE	(IDC_CTXSELECTDLG_APPS,		OnApplicationChanged)
	ON_EN_VALUE_CHANGED	(IDC_CTXSELECTDLG_ALLAPPS,	OnAllSelectChanged)
	ON_EN_VALUE_CHANGED	(IDC_CTXSELECTDLG_SELAPPS,	OnAllSelectChanged)
	ON_LBN_SELCHANGE	(IDC_CTXSELECTDLG_MODS,		OnModulesChanged)
	ON_LBN_SELCHANGE	(IDC_CTXSELECTDLG_SELECTIONS,OnSelectedChanged)

	ON_BN_CLICKED		(IDC_CTXSELECTDLG_ADD,		OnAddSelection)
	ON_BN_CLICKED		(IDC_CTXSELECTDLG_REM,		OnRemoveSelection)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CContextSelectionDialog::CContextSelectionDialog (CStringArray* pSelections, CWnd* aParent /*NULL*/)
	:
	CParsedDialog	(IDD_CTXSELECTDIALOG, aParent),
	m_pSelections	(pSelections)
{
	m_sAllAppString = _TB("All application");
	ASSERT(m_pSelections);
}

//------------------------------------------------------------------------------
BOOL CContextSelectionDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_bAll.				SubclassDlgItem(IDC_CTXSELECTDLG_ALLAPPS,	this);
	m_bSelect.			SubclassDlgItem(IDC_CTXSELECTDLG_SELAPPS,	this);
	m_cbxApplications.	SubclassDlgItem(IDC_CTXSELECTDLG_APPS,		this);
	m_lbxModules.		SubclassDlgItem(IDC_CTXSELECTDLG_MODS,		this);
	m_lbxSelected.		SubclassDlgItem(IDC_CTXSELECTDLG_SELECTIONS,this);

	InitDefaults(); 

	m_bAll.		UpdateCtrlView();
	m_bSelect.	UpdateCtrlView();
	
	DoAllSelectedChanged ();
	RefreshSelectedScroll();
	CenterWindow(); 

	return TRUE;
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::InitDefaults ()
{
	if (!m_pSelections)
		return;

	m_bSelect.	SetCheck (m_pSelections->GetCount());
	m_bAll.		SetCheck (!m_bSelect.GetCheck());

	CString sModName, sAppName;
	AddOnApplication* pAddOnApp = NULL;

	// inserisco nelle selezioni le preesistenti
	for (int i=0; i <= m_pSelections->GetUpperBound(); i++)
	{
		sModName =  m_pSelections->GetAt(i);
		CTBNamespace nsModule(sModName);
		if (nsModule.IsValid())
		{
			sAppName = nsModule.GetApplicationName();
			AddOnModule* pAddOnMod = AfxGetAddOnModule(nsModule);
			if (pAddOnMod)
				sModName = pAddOnMod->GetModuleTitle();
			else
				continue;
		}
		else
		{
			sAppName = sModName;
			sModName = m_sAllAppString;
		}
		
		pAddOnApp = AfxGetAddOnApp(sAppName);

		if (pAddOnApp)
			m_lbxSelected.AddString(FormatSelected(pAddOnApp->GetTitle(), sModName));
	}
}

//-----------------------------------------------------------------------------
CString	CContextSelectionDialog::FormatSelected (const CString& sAppName, const CString& sModName)
{
	return sModName + szFmtSep + sAppName;
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::UnFormatSelected (const CString& sSelection, CString& sAppName, CString& sModName)
{
	int nPos = sSelection.Find(szFmtSep);
	if (nPos >= 0)
	{
		sAppName = sSelection.Mid(nPos + szFmtSep.GetLength());
		sModName = sSelection.Mid(0, nPos);
		sAppName.Trim();
		sModName.Trim();
	}
	else
		ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::FillApplications ()
{
	m_cbxApplications.ResetContent();

	const AddOnAppsArray *pAddonApps = AfxGetAddOnAppsTable();
	// se riempio la combo delle applications
	for (int i=0; i <= pAddonApps->GetUpperBound(); i++)
		m_cbxApplications.AddString(pAddonApps->GetAt(i)->GetTitle());

	m_cbxApplications.SetCurSel(0);
	DoApplicationChanged();

	m_lbxModules.UpdateWindow();
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::FillModules (const CString& sAppName)
{
	m_lbxModules.ResetContent();

	CDC*   pDC		= m_lbxModules.GetDC();
	CFont* pFont	= m_lbxModules.GetFont();
	CFont* pOldFont	= pDC->SelectObject(pFont);
	
	TEXTMETRIC  tm;
	pDC->GetTextMetrics(&tm); 

	// tutta l'applicazione
	int	dx = 0;
	m_lbxModules.AddString(m_sAllAppString);

	CSize sz = pDC->GetTextExtent(m_sAllAppString);
	sz.cx += tm.tmAveCharWidth;
	if (sz.cx > dx)
		dx = sz.cx;

	// se riempio la combo dei moduli, ma mi devo occupare
	// di calcolare se ci vogliono le scrollbar orizzontali
	// grazie a MFC
	CString s;
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(sAppName);
	for (int i=0; i <= pAddOnApp->m_pAddOnModules->GetUpperBound(); i++)
	{
		s = pAddOnApp->m_pAddOnModules->GetAt(i)->GetModuleTitle();
		sz = pDC->GetTextExtent(s);
		sz.cx += tm.tmAveCharWidth;
		if (sz.cx > dx)
			dx = sz.cx;

		m_lbxModules.AddString(s);
	}

	// ripristina il vecchio font
	pDC->SelectObject(pOldFont);

	// indica la size del testo più lungo alle scrollbar
	m_lbxModules.ReleaseDC			(pDC);
	m_lbxModules.SetHorizontalExtent(dx);
	m_lbxModules.UpdateWindow		();
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::RefreshSelectedScroll	()
{
	CDC*   pDC		= m_lbxSelected.GetDC();
	CFont* pFont	= m_lbxSelected.GetFont();
	CFont* pOldFont	= pDC->SelectObject(pFont);

	int	dx = 0;
	CSize sz;
	TEXTMETRIC  tm;
	pDC->GetTextMetrics(&tm); 

	// se riempio la combo dei moduli, ma mi devo occupare
	// di calcolare se ci vogliono le scrollbar orizzontali
	// grazie a MFC
	CString s;
	for (int i=0; i < m_lbxSelected.GetCount(); i++)
	{
		m_lbxSelected.GetText(i,s);
		sz = pDC->GetTextExtent(s);
		sz.cx += tm.tmAveCharWidth;
		if (sz.cx > dx)
			dx = sz.cx;
	}

	// ripristina il vecchio font
	pDC->SelectObject(pOldFont);

	// indica la size del testo più lungo alle scrollbar
	m_lbxSelected.ReleaseDC			(pDC);
	m_lbxSelected.SetHorizontalExtent(dx);
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::EnableAddRemoveButtons ()
{
	CWnd* pWnd = GetDlgItem(IDC_CTXSELECTDLG_ADD);
	if (pWnd)
		pWnd->EnableWindow(m_bSelect.GetCheck() && m_lbxModules.GetCount() && m_lbxModules.GetSelCount());

	pWnd = GetDlgItem(IDC_CTXSELECTDLG_REM);
	if (pWnd)
		pWnd->EnableWindow(m_bSelect.GetCheck() && m_lbxSelected.GetCount() && m_lbxSelected.GetSelCount());
}

//-----------------------------------------------------------------------------
CString	CContextSelectionDialog::GetCurrentApplication ()
{
	CString s;
	int nPos = m_cbxApplications.GetCurSel();
	if (nPos >= 0)
		m_cbxApplications.GetLBText(nPos, s);
	
	return s;
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::OnAllSelectChanged ()
{
	DoAllSelectedChanged	();
	EnableAddRemoveButtons	();
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::OnApplicationChanged ()
{
	DoApplicationChanged	();
	EnableAddRemoveButtons	();

	m_lbxModules.UpdateWindow();
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::OnModulesChanged ()
{
	DoModulesChanged		();
	EnableAddRemoveButtons	();
	
	m_lbxModules.UpdateWindow();
}

//------------------------------------------------------------------------------
void CContextSelectionDialog::OnSelectedChanged ()
{
	EnableAddRemoveButtons	();
}

//------------------------------------------------------------------------------
void CContextSelectionDialog::OnAddSelection ()
{
	DoAddSelection			();
	RefreshSelectedScroll	();
	EnableAddRemoveButtons	();

	m_lbxSelected.UpdateWindow ();
}

//------------------------------------------------------------------------------
void CContextSelectionDialog::OnRemoveSelection()
{
	DoRemoveSelection		();
	RefreshSelectedScroll	();
	EnableAddRemoveButtons	();
	
	m_lbxSelected.UpdateWindow ();
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::DoAllSelectedChanged ()
{
	BOOL bSelApps = m_bSelect.GetCheck();

	if (!bSelApps)
		m_lbxSelected.ResetContent();

	m_cbxApplications.	EnableWindow(bSelApps);
	m_lbxModules.		EnableWindow(bSelApps);	
	m_lbxSelected.		EnableWindow(bSelApps);

	if (!m_cbxApplications.GetCount() && bSelApps)
		FillApplications();

	m_cbxApplications.	UpdateWindow();
	m_lbxModules.		UpdateWindow();	
	m_lbxSelected.		UpdateWindow();
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::DoApplicationChanged ()
{
	const AddOnAppsArray *pAddonApps = AfxGetAddOnAppsTable();
	int nPos = m_cbxApplications.GetCurSel();
	if (nPos < 0 || nPos > pAddonApps->GetUpperBound())
	{
		m_lbxModules.ResetContent();
		m_lbxModules.UpdateWindow();
		return;
	}

	AddOnApplication* pAddOnApp = pAddonApps->GetAt(nPos);
	if (pAddOnApp)
		FillModules(pAddOnApp->m_strAddOnAppName);
}

//------------------------------------------------------------------------------
void CContextSelectionDialog::DoModulesChanged ()
{
	int nCount = m_lbxModules.GetSelCount();
	CArray<int,int> arSel;

	arSel.SetSize(nCount);
	m_lbxModules.GetSelItems(nCount, arSel.GetData());

	// cerco se è stata indicata tutta l'applicazione
	BOOL bAllAppInSel = FALSE;
	for (int i=0; i <= arSel.GetUpperBound(); i++)
		if (arSel.GetAt(i) == 0)
		{
			bAllAppInSel = TRUE;
			break;
		}
	
	// se è stata selezionata tutta l'applicazione e i moduli mi arrabbio
	if (bAllAppInSel && arSel.GetSize() > 1)
	{
		AfxMessageBox (_TB("Selection of all application disable selection of single modules."));
		for (int i=0; i < m_lbxModules.GetCount(); i++)
			m_lbxModules.SetSel(i, i==0);
	}
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::DoAddSelection ()
{
	// mi faccio ritornare le selezioni
	int nCount = m_lbxModules.GetSelCount();

	CArray<int,int> arSelections;
	arSelections.SetSize(nCount);
	m_lbxModules.GetSelItems(nCount, arSelections.GetData());

	// compongo lo StringArray di visualizzazione delle selezionate
	CStringArray arSelected;
	CString s;
	for (int i=0; i <= arSelections.GetUpperBound(); i++)
	{
		m_lbxModules.GetText(arSelections.GetAt(i), s);
		s = FormatSelected(GetCurrentApplication(),  s);
		arSelected.Add(s);
	}

	CheckSelectionsToAdd (arSelected);

	// le rimaste vengono aggiunte alle selezioni
	for (int i=0; i <= arSelected.GetUpperBound(); i++)
		m_lbxSelected.AddString(arSelected.GetAt(i));
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::DoRemoveSelection ()
{
	// mi faccio ritornare le selezioni
	int nCount = m_lbxSelected.GetSelCount();

	CArray<int,int> arSelections;
	arSelections.SetSize(nCount);
	m_lbxSelected.GetSelItems(nCount, arSelections.GetData());

	// le sposto un'attimo in un array per ripulirle nel caso di errore
	for (int i=arSelections.GetUpperBound(); i >= 0; i--)
		m_lbxSelected.DeleteString(arSelections.GetAt(i));
}

//------------------------------------------------------------------------------
void CContextSelectionDialog::CheckSelectionsToAdd (CStringArray& arSelections)
{
	BOOL bTuttaInNewSelections	= FALSE;
	CString sTuttaApp			= FormatSelected(GetCurrentApplication(), m_sAllAppString);
	
	// elimino i duplcati già presenti nelle selezionate
	CString sSel;
	for (int i=arSelections.GetUpperBound(); i >=0; i--)
	{
		sSel = arSelections.GetAt(i);
		if (m_lbxSelected.FindString(0, sSel) >= 0)
			arSelections.RemoveAt(i);
		
		if (sSel.CompareNoCase(sTuttaApp) == 0)
			bTuttaInNewSelections = TRUE;
	}

	// tutta è mutuamente esclusiva con i suoi moduli
	if (bTuttaInNewSelections && arSelections.GetSize() > 1)
	{
		AfxMessageBox( _TB("All application is selected, single modules cannot be selected!"));
		arSelections.RemoveAll();
		return;
	}

	BOOL bTuttaInOldSelected	= FALSE;
	BOOL bModulesInOldSelected	= FALSE;

	CString sAppName, sModName;
	for (int i=0; i < m_lbxSelected.GetCount(); i++)
	{
		m_lbxSelected.GetText(i, sSel);
		if (sSel.CompareNoCase(sTuttaApp) == 0)
		{
			bTuttaInOldSelected = TRUE;
			continue;
		}
		
		UnFormatSelected(sSel, sAppName, sModName);

		if (sAppName.CompareNoCase(GetCurrentApplication()) == 0)
			bModulesInOldSelected = TRUE;
	}

	if (bTuttaInNewSelections && bModulesInOldSelected)
	{
		arSelections.RemoveAll();
		AfxMessageBox( _TB("Single modules of this application are already selected, delete all modules from selection for choose all application."));
	}
	else if (bTuttaInOldSelected && arSelections.GetSize())
	{
		arSelections.RemoveAll();
		AfxMessageBox( _TB("All application is already selected!\nFor select single modules delete global indication."));
	}
}

//-----------------------------------------------------------------------------
void CContextSelectionDialog::OnOK ()
{
	// tutte le applicazioni
	m_pSelections->RemoveAll();
	if (m_bAll.GetCheck())
	{
		CParsedDialog::OnOK();
		return;
	}

	// travaso le aree realmente selezionate
	CString sArea, sSel;
	for (int i=0; i < m_lbxSelected.GetCount(); i++)
	{
		m_lbxSelected.GetText(i, sSel);
		sArea = GetContextArea(sSel);
		if (!sArea.IsEmpty())
			m_pSelections->Add(sArea);
	}

	CParsedDialog::OnOK();
}

// decodifica l'area di contesto sulla base del contenuto di una riga di ListBox
//-----------------------------------------------------------------------------
CString CContextSelectionDialog::GetContextArea	(const CString& sSelection)
{
	CString sAppName, sModName;
	UnFormatSelected(sSelection, sAppName, sModName);

	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;

	// cerca nelle AddOnApplication utilizzando il titolo in lingua
	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (pAddOnApp->GetTitle().CompareNoCase(sAppName))
			continue;
		
		// per identificare tutta l'applicazione ne uso il name
		if (sModName.CompareNoCase(m_sAllAppString) == 0)
			return pAddOnApp->m_strAddOnAppName;

		// per identificare il modulo ne uso il namespace
		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);

			if (sModName.CompareNoCase(pAddOnMod->GetModuleTitle()) == 0)
				return pAddOnMod->m_Namespace.ToString();
		}
	}

	return _T("");
}
