
#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbNameSolver\PathFinder.h>
#include <TBNameSolver\TBNamespaces.h>
#include <TBNameSolver\FileSystemFunctions.h>
#include <TBNameSolver\IFileSystemManager.h>

#include <TbGenlib\funproto.h>
#include <TbGenlib\const.h>
#include <TbGenlibUI\TbExplorer.h>

#include <TBOleDb\OleDbMng.h>

#include <TBWoormEngine\ActionsRepEngin.h>
#include <TBWoormEngine\PRGDATA.H>
#include <TBWoormEngine\ASKDLG.H>

#include <TBWoormViewer\viewpars.h>

#include <XEngine\TBXMLEnvelope\XEngineObject.h>
#include <XEngine\TBXMLEnvelope\XMLEnvelopeTags.h>

#include "ProfiliMngWizPage.h"
#include "XmlProfileInfo.h"
#include "GenFunc.h"
#include "ExpCriteriaObj.h"
#include "ExpCriteriaDlg.h"

#include "SoapFunctions.h"

//resource
#include <TBGES\xmlcontrols.hjson> //JSON AUTOMATIC UPDATE
#include "ProfiliMngWizPage.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szMaxDocDef				[] = _T("10");
static const TCHAR szVersionDef				[] = _T("1");

//per i webmethods dei profili
static const TCHAR szDocumentNamespace[]	= _T("documentNamespace");
static const TCHAR szNewProfileName[]		= _T("newProfileName");
static const TCHAR szPosType[]				= _T("posType");
static const TCHAR szUserName[]				= _T("userName");
static const TCHAR szProfilePath[]			= _T("profilePath");
static const TCHAR szNewName[]				= _T("newName");

#define STANDARD_IMAGE_IDX 0
#define CUSTOM_IMAGE_IDX 1
#define PREFERED_IMAGE_IDX 2

//----------------------------------------------------------------------------------------------
//	CProfileDBTPropertiesDlg
//----------------------------------------------------------------------------------------------
CProfileDBTPropertiesDlg::CProfileDBTPropertiesDlg(CXMLDBTInfo* pDBT, BOOL bReadOnly /*FALSE*/)
	:
	CLocalizableDialog(IDD_PROFILE_WIZ_DBT_PROPERTIES, NULL)
{
	m_bReadOnly = bReadOnly;
	m_pDBTInfo = pDBT;
}

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProfileDBTPropertiesDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CProfileDBTPropertiesDlg
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CProfileDBTPropertiesDlg, CLocalizableDialog)
//----------------------------------------------------------------------------------------------
BOOL CProfileDBTPropertiesDlg::OnInitDialog	()
{
	CLocalizableDialog::OnInitDialog();
	SetWindowText(cwsprintf(_TB("Properties of {0-%s}"), m_pDBTInfo->GetTitle()));		

	SetDlgItemText(IDC_PROFMNG_TABLENAME_STATIC, m_pDBTInfo->m_strTableName);
	SetDlgItemText(IDC_PROFMNG_TYPE_STATIC, m_pDBTInfo->GetStrType());

	((CButton*)GetDlgItem(IDC_PROFMNG_EXPORT_CHKBOX))->SetCheck(m_pDBTInfo->m_bExport);
	GetDlgItem(IDC_PROFMNG_EXPORT_CHKBOX)->EnableWindow(m_pDBTInfo->GetType() != CXMLDBTInfo::MASTER_TYPE && !m_bReadOnly);

	if (m_pDBTInfo->GetChooseUpdate())
	{
		switch (m_pDBTInfo->GetUpdateType()) 
		{
			case CXMLDBTInfo::REPLACE: 
				((CButton*)GetDlgItem(IDC_DBT_PROPS_REPLACE))->SetCheck(1); break;
			case CXMLDBTInfo::INSERT_UPDATE:
				((CButton*)GetDlgItem(IDC_DBT_PROPS_INSERT_UPDATE))->SetCheck(1); break;
			case CXMLDBTInfo::ONLY_INSERT:
				((CButton*)GetDlgItem(IDC_DBT_PROPS_ONLY_INSERT))->SetCheck(1); break;
			default: 
				((CButton*)GetDlgItem(IDC_DBT_PROPS_REPLACE))->SetCheck(1); break;
		}

		if (m_pDBTInfo->GetType() != CXMLDBTInfo::BUFFERED_TYPE)
		{
			GetDlgItem(IDC_DBT_PROPS_REPLACE)->SetWindowText(_TB("applies complete replacement"));
			GetDlgItem(IDC_DBT_PROPS_INSERT_UPDATE)->SetWindowText(_TB("inserts the new record or applies the changes if it exists"));
			GetDlgItem(IDC_DBT_PROPS_ONLY_INSERT)->SetWindowText(_TB("inserts the new record and skips if it exists"));
			GetDlgItem(IDC_DBT_PROPS_ONLY_INSERT)->EnableWindow(m_pDBTInfo->GetType() != CXMLDBTInfo::MASTER_TYPE);
		}

		GetDlgItem(IDC_DBT_PROPS_REPLACE)->EnableWindow(!m_bReadOnly);
		GetDlgItem(IDC_DBT_PROPS_INSERT_UPDATE)->EnableWindow(!m_bReadOnly);
		GetDlgItem(IDC_DBT_PROPS_ONLY_INSERT)->EnableWindow(!m_bReadOnly);
	}
	else
	{
		((CButton*)GetDlgItem(IDC_DBT_PROPS_REPLACE))->SetCheck(1);
		CWnd* pChild = GetDlgItem(IDC_PROFMNG_UPDATE_STATIC);
		
		CRect rectDlg;
		GetWindowRect(rectDlg);
		CRect rectChild;
		pChild->GetWindowRect(rectChild);
		ScreenToClient(rectChild);
		
		int nDiff = rectDlg.Height() - rectChild.Height();		
		pChild = GetDlgItem(IDOK);
		CRect btnChild;
		pChild->GetWindowRect(btnChild);
		ScreenToClient(btnChild);
		pChild->SetWindowPos(NULL, btnChild.left, rectChild.top, 0, 0, SWP_NOSIZE|SWP_NOZORDER|SWP_NOACTIVATE);
		
		pChild = GetDlgItem(IDCANCEL);
		pChild->GetWindowRect(btnChild);
		ScreenToClient(btnChild);
		pChild->SetWindowPos(NULL, btnChild.left, rectChild.top, 0, 0, SWP_NOSIZE|SWP_NOZORDER|SWP_NOACTIVATE);
		
		SetWindowPos(NULL, 0, 0, rectDlg.Width(), nDiff, SWP_NOMOVE|SWP_NOZORDER|SWP_NOACTIVATE);
	
		GetDlgItem(IDC_PROFMNG_UPDATE_STATIC)->ShowWindow(SW_HIDE);		
		GetDlgItem(IDC_DBT_PROPS_REPLACE)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_DBT_PROPS_INSERT_UPDATE)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_DBT_PROPS_ONLY_INSERT)->ShowWindow(SW_HIDE);
	}	

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CProfileDBTPropertiesDlg::OnOK	()
{
	if (m_bReadOnly)
	{
		CLocalizableDialog::OnOK();
		return;
	}
		
	m_pDBTInfo->m_bExport = ((CButton*)GetDlgItem(IDC_PROFMNG_EXPORT_CHKBOX))->GetCheck();
	if (m_pDBTInfo->GetChooseUpdate())
	{
		if (((CButton*)GetDlgItem(IDC_DBT_PROPS_REPLACE))->GetCheck())
			m_pDBTInfo->m_eUpdateType = CXMLDBTInfo::REPLACE;
		if (((CButton*)GetDlgItem(IDC_DBT_PROPS_INSERT_UPDATE))->GetCheck())
			m_pDBTInfo->m_eUpdateType = CXMLDBTInfo::INSERT_UPDATE;
		if (((CButton*)GetDlgItem(IDC_DBT_PROPS_ONLY_INSERT))->GetCheck())
			m_pDBTInfo->m_eUpdateType = CXMLDBTInfo::ONLY_INSERT;
	}

	CLocalizableDialog::OnOK();
}

//-----------------------------------------------------------------------------
// CSelectionQueryDlg implementation
//-----------------------------------------------------------------------------
//  
IMPLEMENT_DYNAMIC(CSelectionQueryDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CSelectionQueryDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CSelectionQueryDlg)
		ON_BN_CLICKED (IDC_USER_CRITERIA_MNG_BTN_TEST,		OnTest)
		ON_BN_CLICKED (IDC_USER_CRITERIA_MNG_BTN_PARAMETERS,ShowAskRules)
		ON_WM_CTLCOLOR()
		ON_EN_UPDATE (IDC_USER_CRITERIA_MNG_EDT_WCLAUSE,	OnEnableBtnTest)
		ON_EN_UPDATE (IDC_USER_CRITERIA_MNG_EDT_ORDERBY,	OnEnableBtnTest)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSelectionQueryDlg::CSelectionQueryDlg (CXMLExportCriteria* pXMLExpCriteria)
	:
	CParsedDialog		(IDD_PROFILES_USER_CRITERIA_MNG),
	m_pXMLExpCriteria	(pXMLExpCriteria),
	m_pUserCriteria		(NULL),	
	m_pPrgData			(NULL),
	m_pQueryInfo		(NULL),
	m_bPrgDataOwns		(FALSE),
	m_bNewQuery			(FALSE),
	m_pTableInfo		(NULL)
{
	m_pEdtExpWClause= new CExpEdit;
	m_pEdtOrderBy	= new CEqnEdit (NULL, NULL);

	ASSERT(m_pXMLExpCriteria);
	if (m_pXMLExpCriteria)
	{
		if (m_pXMLExpCriteria->GetUserExportCriteria())
			m_pUserCriteria = new CUserExportCriteria(*m_pXMLExpCriteria->GetUserExportCriteria());
		else
			m_pUserCriteria = new CUserExportCriteria(m_pXMLExpCriteria);
	}

	m_pQueryInfo	= m_pUserCriteria ? m_pUserCriteria->m_pQueryInfo : NULL;
	m_pPrgData		= m_pQueryInfo ? m_pQueryInfo->m_pPrgData : NULL;
	m_pTableInfo	= m_pUserCriteria ? m_pUserCriteria->GetTableInfo() : NULL;
}

//-----------------------------------------------------------------------------
CSelectionQueryDlg::~CSelectionQueryDlg ()
{
	m_brSolidWhite.DeleteObject();

	if (m_bPrgDataOwns && m_pPrgData) 
		delete m_pPrgData;

	ASSERT (m_pEdtExpWClause);
	ASSERT (m_pEdtOrderBy);
	ASSERT (m_pUserCriteria);

	delete m_pEdtExpWClause;
	delete m_pEdtOrderBy;
	delete m_pUserCriteria;	
}

//-----------------------------------------------------------------------------
BOOL CSelectionQueryDlg::OnInitDialog ()
{
	CParsedDialog::OnInitDialog();

	// subclass dei controlli di libreria MicroArea
	VERIFY (m_pEdtExpWClause->	SubclassEdit (IDC_USER_CRITERIA_MNG_EDT_WCLAUSE,	this));
	VERIFY (m_pEdtOrderBy->		SubclassEdit (IDC_USER_CRITERIA_MNG_EDT_ORDERBY,	this));

	// Inizializzazione dei parsed control
	// Abilito il pulsante di test solo se esiste la query
	BtnTestEnableWindow (!m_bNewQuery);

	m_pEdtExpWClause->	SetTableInfo (m_pTableInfo);
	m_pEdtOrderBy->		SetTableInfo (m_pTableInfo);

	InitFields();
	m_pEdtExpWClause->SetFocus ();
	
	m_brSolidWhite.CreateSolidBrush(RGB(255, 255, 255));

	return TRUE;
}

//---------------------------------------------------------------------------
HBRUSH CSelectionQueryDlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor) 
{
	switch (nCtlColor)
	{	   
		case CTLCOLOR_STATIC:
		case CTLCOLOR_EDIT:
		case CTLCOLOR_LISTBOX:
		case CTLCOLOR_SCROLLBAR:
		case CTLCOLOR_BTN:
			pDC->SetTextColor(RGB(0, 0, 0));
			pDC->SetBkColor(RGB(255, 255, 255));
		case CTLCOLOR_DLG:	    
			return m_brSolidWhite;
	}	
	return m_brSolidWhite;
}

//-----------------------------------------------------------------------------
void CSelectionQueryDlg::InitFields()
{
	GetChkBoxNativeExpr()->SetCheck(m_pQueryInfo ? m_pQueryInfo->m_bNativeExpr : 0);

	if (m_pPrgData)
		m_pEdtExpWClause->SetSymbolTable (m_pPrgData->GetSymTable());

	// Conversione delle stringhe per effettuare la corretta visualizzazione
	// a video
	CString strTmp (m_pQueryInfo ? m_pQueryInfo->m_TableInfo.m_strFilter : _T(""));
	ConvertCString(strTmp, LF_TO_CRLF);
	m_pEdtExpWClause-> SetValue (strTmp);

	// Conversione delle stringhe per effettuare la corretta visualizzazione
	// a video
	strTmp = m_pQueryInfo ? m_pQueryInfo->m_TableInfo.m_strSort : _T("");
	ConvertCString(strTmp, LF_TO_CRLF);
	m_pEdtOrderBy-> SetValue (strTmp);
}


// Ritorna TRUE se il controllo sintattico della query ha data esito positivo,
// altrimenti FALSE
//-----------------------------------------------------------------------------
BOOL CSelectionQueryDlg::TestQuery (CString* pStrWClause /* NULL */)
{
	if (!m_pUserCriteria->m_pRecord) 
		return FALSE;

	ASSERT (m_pEdtOrderBy);
	ASSERT (m_pEdtExpWClause);

	SqlTable* pTable = new SqlTable(m_pUserCriteria->m_pRecord, AfxGetDefaultSqlSession());
	SqlTableInfoArray aTableInfoArray(m_pTableInfo);

	// Controllo dl Where Clause
	if (!m_pEdtExpWClause->IsEmpty())
	{
	    WClause aWC (AfxGetDefaultSqlConnection(), m_pPrgData ? m_pPrgData->GetSymTable() : NULL, aTableInfoArray);
		aWC.SetNative(IsNativeExpr());
		
		if (!m_pEdtExpWClause->CheckWC(aWC))
		{
			m_pEdtExpWClause->SetCtrlFocus(TRUE);
			return FALSE;
		}

		GetChkBoxNativeExpr()->SetCheck(aWC.IsNative());

		// Se il puntatore è diverso da NULL allora memorizzo la stringa di Filter
		if (pStrWClause)
			*pStrWClause = aWC.ToString(pTable);
	}

	// Se la stringa di order by è vuota, estraggo secondo l'ordine della tabella
	if (!m_pEdtOrderBy->IsEmpty())
	{
		CString	strTmp (m_pEdtOrderBy->GetValue());
		ConvertCString (strTmp, CRLF_TO_LF);
		Parser lex (strTmp);

		// controllo sintattico dell'espressione
		if (!ParseOrderBy(lex, m_pPrgData ? m_pPrgData->GetSymTable() : NULL, &aTableInfoArray, strTmp))
		{
			m_pEdtOrderBy->SetCtrlFocus(TRUE);
			return FALSE;
		}
	}

	delete pTable;
	BtnTestEnableWindow (FALSE);

	return TRUE;
}

// Abilitazione/disabilitazione del bottone di Test
//-----------------------------------------------------------------------------
void CSelectionQueryDlg::BtnTestEnableWindow (BOOL bEnable /* TRUE */)
{
	GetDlgItem(IDC_USER_CRITERIA_MNG_BTN_TEST)->EnableWindow (bEnable);
}

// Ritorna il Check Box per la gestione della espressione nativa SQL
//-----------------------------------------------------------------------------
CButton* CSelectionQueryDlg::GetChkBoxNativeExpr()
{
	return (CButton*)GetDlgItem(IDC_USER_CRITERIA_MNG_CKB_SET_NATIVE_SQL);
}

//-----------------------------------------------------------------------------
void CSelectionQueryDlg::CovertStrOrderByInLF (CString& strNewOrderBy)
{
	strNewOrderBy = m_pEdtOrderBy->GetValue();
	ConvertCString (strNewOrderBy, CRLF_TO_LF);
}

// Visualizzazione delle dialog per la costruzione della query parametrica
//-----------------------------------------------------------------------------
void CSelectionQueryDlg::ShowAskRules()
{
	//Se non è presente alloco il Program Data
	if (!m_pPrgData)
	{
		m_bPrgDataOwns	= TRUE;
		m_pPrgData		=  new ProgramData(NULL);
	}

	// Visualizzazione della finestra di dialogo per la definizione dei
	// parametri
	CAskRuleDlg	askRuleDlg (m_pPrgData, NULL);
	if (askRuleDlg.DoModal() != IDOK && m_bPrgDataOwns)
	{
		delete m_pPrgData;
		m_pPrgData = NULL;
		return;
	}
	// Occorre settare il flag per evitare di cancellare la program data
	// appena inserito
	m_bPrgDataOwns = FALSE;
	ASSERT (m_pEdtExpWClause);
	ASSERT (m_pPrgData);

	// Associo all'edit della Where Clause la tabella dei simboli appena
	// definita (gestisce i parametri)
	m_pEdtExpWClause->SetSymbolTable(m_pPrgData->GetSymTable());
	m_pEdtExpWClause->SetFocus		();
}


//-----------------------------------------------------------------------------
void CSelectionQueryDlg::OnOK ()
{
	// Controllo la correttezza sintattica della query
	CString strWClause;
	if (!TestQuery (&strWClause))
		return;
    
    CString strOrderByCov;
	CovertStrOrderByInLF (strOrderByCov);
	if (strWClause.IsEmpty() && strOrderByCov.IsEmpty() && !m_pPrgData)
		m_pXMLExpCriteria->SetUserCriteria(NULL);
		return;

	if (m_pQueryInfo)
	{
		m_pQueryInfo->m_bNativeExpr	= IsNativeExpr();
		m_pQueryInfo->m_TableInfo.m_strFilter	= strWClause;
		m_pQueryInfo->m_TableInfo.m_strSort	= strOrderByCov;
		m_pQueryInfo->SetProgramData(m_pPrgData);
	}

	m_pXMLExpCriteria->SetUserCriteria(m_pUserCriteria);

	EndDialog(IDOK);
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CSelectionQueryDlg::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CSelectionQueryDlg\n");
	CParsedDialog::Dump(dc);
}
#endif // _DEBUG


/////////////////////////////////////////////////////////////////////////////
// CXRefTreeCtrl
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CXRefTreeCtrl, CDocDescrTreeCtrl)

BEGIN_MESSAGE_MAP(CXRefTreeCtrl, CDocDescrTreeCtrl)
	//{{AFX_MSG_MAP(CXRefTreeCtrl)
	ON_WM_CONTEXTMENU	()
	ON_NOTIFY_REFLECT	(NM_DBLCLK,							OnDoubleClick)
	ON_COMMAND			(ID_DOC_TREE_XREF_EXPORT_XREF,		OnExportXRef)
	ON_COMMAND			(ID_DOC_TREE_DBT_NO_XREF,			OnUseNoXRef)
	ON_COMMAND			(ID_DOC_TREE_DBT_ALL_XREF,			OnUseAllXRef)
	ON_COMMAND			(ID_DOC_TREE_XREF_HKL,				OnXRefHotKeyLink)
	ON_NOTIFY_REFLECT	(TVN_KEYDOWN,						OnKeydown)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXRefTreeCtrl::CXRefTreeCtrl()
	:
	CDocDescrTreeCtrl	(),
	m_pXMLProfileInfo	(NULL)
{
	m_bDescription = FALSE;
}

//----------------------------------------------------------------------------
void CXRefTreeCtrl::SetBackColor()
{
	m_ImageList.SetBkColor(RGB(255, 255, 255));
}

//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	CRect rcTree;
	GetWindowRect(&rcTree);

	if (!rcTree.PtInRect(mousePos))
		return;

	ScreenToClient(&mousePos);
	
	HTREEITEM hItemToSelect = HitTest(mousePos);
	
	if (hItemToSelect && SelectItem(hItemToSelect))
	{
		CLocalizableMenu menu;
		CMenu* pPopup = NULL;
		
		int nItemType = GetItemType(hItemToSelect);

		if (CDocDescrTreeCtrl::LoadMenu(menu, nItemType))
		{
			pPopup = menu.GetSubMenu(0);
			ASSERT(pPopup);
		}
		if (pPopup)
		{
			if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_DBT)
			{
				if (m_bReadOnly)
				{
					pPopup->RemoveMenu(ID_DOC_TREE_DBT_NO_XREF, MF_BYCOMMAND);
					pPopup->RemoveMenu(ID_DOC_TREE_DBT_ALL_XREF, MF_BYCOMMAND);
					pPopup->RemoveMenu(ID_DOC_TREE_DBT_NEW_XREF, MF_BYCOMMAND);
				}
				else
				{
					pPopup->EnableMenuItem(ID_DOC_TREE_DBT_NO_XREF, MF_BYCOMMAND | MF_ENABLED);
					pPopup->EnableMenuItem(ID_DOC_TREE_DBT_ALL_XREF, MF_BYCOMMAND | MF_ENABLED);
				}
				pPopup->RemoveMenu(ID_DOC_TREE_DBT_NEW_UK_GROUP, MF_BYCOMMAND);
				pPopup->RemoveMenu(ID_DOC_TREE_DBT_FIXED_FIELD, MF_BYCOMMAND);

			}

			if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_XREF)
			{
				HTREEITEM hXRefItem;
				CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);

				if (pCurrXRef->IsOwnedByDoc())
				{
					pPopup->RemoveMenu(ID_DOC_TREE_XREF_REMOVE_XREF, MF_BYCOMMAND);

					//devo settare il menu Item per l'utilizzo o  meno dell'external reference in base allo stato
					// attuale dello stesso ovvero:
					// se è da esportare : il menu item ha dicitura: "Non utilizza"
					// se non è da esportare : il menu item ha dicitura: "Utilizza"
					// inoltre se una delle FK non viene utilizzata dal profilo non rendo possibile abilitare l'xref
					if (pCurrXRef->IsToUse())
					{
						pPopup->EnableMenuItem(ID_DOC_TREE_XREF_EXPORT_XREF, MF_BYCOMMAND | MF_ENABLED);
						pPopup->CheckMenuItem(ID_DOC_TREE_XREF_EXPORT_XREF, MF_CHECKED | MF_BYCOMMAND);
					}
					else
					{
						if (m_pXMLProfileInfo && m_pXMLProfileInfo->IsValidXRef(pCurrXRef) && pCurrXRef->HasValidRefDoc())
							pPopup->EnableMenuItem(ID_DOC_TREE_XREF_EXPORT_XREF, MF_BYCOMMAND | MF_ENABLED);
					}
				}
				if (m_bReadOnly)
				{
					pPopup->RemoveMenu(ID_DOC_TREE_XREF_EXPORT_XREF, MF_BYCOMMAND);
					if (!pCurrXRef->IsOwnedByDoc())
						pPopup->RemoveMenu(ID_DOC_TREE_XREF_REMOVE_XREF, MF_BYCOMMAND);
				}
			}
			ClientToScreen(&mousePos);

			pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON,	mousePos.x, mousePos.y, this);		
		}
	}
}

//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnDoubleClick(NMHDR* pNMHDR, LRESULT* pResult) 
{
	*pResult = 0;

	HTREEITEM hSelItem = GetSelectedItem();
	if (hSelItem)
	{
		if (GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_XREF)
		{
			OnModifyXRef();
			*pResult = 1;
			return;
		}
	}
}

//-------------------------------------------------------------------------------------------
void CXRefTreeCtrl::OnKeydown(NMHDR* pNMHDR, LRESULT* pResult) 
{
	TV_KEYDOWN* pTVKeyDown = (TV_KEYDOWN*)pNMHDR;
	
	//se ha premuto del elimino il profilo corrente
	if (VK_DELETE == pTVKeyDown->wVKey)
	{
		HTREEITEM hXRefItem;
		CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);
		if (!pCurrXRef || !hXRefItem)
			return;
		
		if (!pCurrXRef->IsOwnedByDoc() && !m_bDescription)
			OnRemoveXRef();	
		else
		{
			pCurrXRef->m_bOldUse = pCurrXRef->IsToUse();
			pCurrXRef->SetUse(FALSE);
			UpdateXRefIcon(hXRefItem, pCurrXRef);
		}
	}

	*pResult = 0;
}

//-------------------------------------------------------------------------------
void CXRefTreeCtrl::FillTree(CXMLProfileInfo* pProfileInfo)
{
	if (!GetImageList(TVSIL_NORMAL))
		SetImageList(&m_ImageList, TVSIL_NORMAL);

	DeleteAllItems();
	
	if (!pProfileInfo)
		return;
	
	m_pXMLProfileInfo = pProfileInfo;
	m_pXMLDocObject = pProfileInfo;

	CString strRootText = cwsprintf(_T("%s - %s"), pProfileInfo->GetDocumentTitle(), pProfileInfo->GetName());
	if (pProfileInfo->IsPreferred())
		strRootText += PREFERRED_PROFILE_TYPE_CAPTION;
	
	HTREEITEM hDocTreeItem = InsertItem(strRootText);

	if (!hDocTreeItem || !pProfileInfo->GetDBTInfoArray())
		return;

	SetItemData(hDocTreeItem, (DWORD) pProfileInfo);
	
	for( int nDBTIdx = 0 ; nDBTIdx < pProfileInfo->GetDBTInfoArray()->GetSize() ; nDBTIdx++)
	{
		CXMLDBTInfo* pDBTInfo = pProfileInfo->GetDBTAt(nDBTIdx);

		if (!pDBTInfo || !pDBTInfo->IsToExport())
			continue;

		HTREEITEM hDBTItem = InsertDBTItem (pDBTInfo, hDocTreeItem);
		if (!hDBTItem)
			continue;

		if (!pDBTInfo->GetXMLXRefInfoArray()) 
			continue;

		for (int nXRefIdx = 0 ; nXRefIdx < pDBTInfo->GetXMLXRefInfoArray()->GetSize() ; nXRefIdx++)
		{
			CXMLXRefInfo* pXRefInfo = pDBTInfo->GetXRefAt(nXRefIdx);
			if (!pXRefInfo)
				continue;
			
			HTREEITEM hXRefItem = InsertXRefItem(pXRefInfo,	hDBTItem);
		
			if (!hXRefItem)
				continue;


			for(int nSegIdx = 0 ; nSegIdx < pXRefInfo->GetSegmentsNum() ; nSegIdx++)
			{
				CXMLSegmentInfo* pSegInfo = pXRefInfo->GetSegmentAt(nSegIdx);
				if (!pSegInfo)
					continue;

				InsertXRefSegmentItem(pSegInfo,	hXRefItem);
			}
		}
	}
	
	Expand(hDocTreeItem, TVE_EXPAND);
}

//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnDBTProperties() 
{
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT();
	if (!pCurrDBT)
		return;

	CProfileDBTPropertiesDlg dlg(pCurrDBT, m_bReadOnly);
	if (dlg.DoModal() == IDOK)
		FillTree(m_pXMLProfileInfo);
}

//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnExportXRef()
{
	HTREEITEM hXRefItem;
	
	if (!m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return;
	}


	CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);
	if (!pCurrXRef || !hXRefItem)
		return;
	
	if (pCurrXRef->IsOwnedByDoc() && !m_bDescription)
	{
		BOOL bUse = pCurrXRef->IsToUse();

		//se l'xref non è utilizzato e lo si vuole usare verifico che i segmenti di fk
		//siano stati tutti inseriti nella lista dei field del profilo.
		if (!bUse && !m_pXMLProfileInfo->IsValidXRef(pCurrXRef))
		{
			CString strMessage = _TB("Unable to use the selected external reference.\r\nOne or more fields that make up the relation were not selected from the exportable field list in the Select fields to export box.");
			AfxMessageBox(strMessage);
			return;
		}

		pCurrXRef->m_bOldUse = bUse;
		pCurrXRef->SetUse(!bUse);
		
		UpdateXRefIcon(hXRefItem, pCurrXRef);
	}
}

//mette il flag use di ogni xref a false
//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnUseNoXRef()
{
	HTREEITEM		hCurrDbt;
	CXMLDBTInfo*	pCurrDBT = GetCurrentDBT(&hCurrDbt);
	if (!pCurrDBT || !hCurrDbt || !pCurrDBT->GetXMLXRefInfoArray())
		return;

	pCurrDBT->SetXRefUseFlag(FALSE);
	FillTree(m_pXMLProfileInfo);
}

//mette il flag use di ogni xref a true
//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnUseAllXRef()
{
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT();
	if (!pCurrDBT)
		return;

	pCurrDBT->SetXRefUseFlag(TRUE);
	FillTree(m_pXMLProfileInfo);
}

//----------------------------------------------------------------------------
void CXRefTreeCtrl::OnXRefHotKeyLink()
{
	HTREEITEM hXRefItem;
	CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);
	
	if(!pCurrXRef || !hXRefItem)
		return;

	CXMLDBTInfo* pCurrDBT = (CXMLDBTInfo*)GetItemData(GetParentItem(GetSelectedItem()));
	if (!pCurrDBT)
		return;
	CString strPath = ::GetProfilePath(pCurrXRef->GetDocumentNamespace(), pCurrXRef->GetProfile(), m_pXMLProfileInfo->m_ePosType, m_pXMLProfileInfo->m_strUserName);
	CXMLProfileInfo aProfileInfo(pCurrXRef->GetDocumentNamespace(), strPath);
	aProfileInfo.LoadAllFiles();
	CHotKeyLinkDlg dlg(pCurrDBT, m_pXMLDocObject, pCurrXRef, &aProfileInfo);
	dlg.DoModal();

}

//----------------------------------------------------------------------------------
// CWhiteCheckListBox implementatio
//----------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWhiteCheckListBox, CCheckListBox)

BEGIN_MESSAGE_MAP(CWhiteCheckListBox, CCheckListBox)
	//{{AFX_MSG_MAP(CWhiteCheckListBox)
	ON_WM_CTLCOLOR_REFLECT	()
	ON_WM_DRAWITEM			()
	ON_WM_LBUTTONDOWN		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------
CWhiteCheckListBox::CWhiteCheckListBox()
{
	m_brSolidWhite.CreateSolidBrush(RGB(255, 255, 255));
}
	
//----------------------------------------------------------------------------------
CWhiteCheckListBox::~CWhiteCheckListBox()
{
	m_brSolidWhite.DeleteObject();
}

//----------------------------------------------------------------------------
HBRUSH CWhiteCheckListBox::CtlColor(CDC* pDC, UINT nCtlColor) 
{
	return m_brSolidWhite;
}

//----------------------------------------------------------------------------
void CWhiteCheckListBox::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct)
{
	// You must override DrawItem and MeasureItem for LBS_OWNERDRAWVARIABLE
	ASSERT((GetStyle() & (LBS_OWNERDRAWFIXED | LBS_HASSTRINGS)) ==
		(LBS_OWNERDRAWFIXED | LBS_HASSTRINGS));

	CDC* pDC = CDC::FromHandle(lpDrawItemStruct->hDC);

	if (((LONG)(lpDrawItemStruct->itemID) >= 0) &&
		(lpDrawItemStruct->itemAction & (ODA_DRAWENTIRE | ODA_SELECT)))
	{
		int cyItem = GetItemHeight(lpDrawItemStruct->itemID);
		BOOL fDisabled = !IsWindowEnabled() || !IsEnabled(lpDrawItemStruct->itemID);

		COLORREF newTextColor = fDisabled ?
			RGB(0x80, 0x80, 0x80) : GetSysColor(COLOR_WINDOWTEXT);  // light gray
		COLORREF oldTextColor = pDC->SetTextColor(newTextColor);

		COLORREF newBkColor = RGB(255,255,255);
		COLORREF oldBkColor = pDC->SetBkColor(newBkColor);

		if (newTextColor == newBkColor)
			newTextColor = RGB(0xC0, 0xC0, 0xC0);   // dark gray

		if (!fDisabled && ((lpDrawItemStruct->itemState & ODS_SELECTED) != 0))
		{
			pDC->SetTextColor(GetSysColor(COLOR_HIGHLIGHTTEXT));
			pDC->SetBkColor(GetSysColor(COLOR_HIGHLIGHT));
		}

		if (m_cyText == 0)
			VERIFY(cyItem >= CalcMinimumItemHeight());

		CString strText;
		GetText(lpDrawItemStruct->itemID, strText);

		pDC->ExtTextOut(lpDrawItemStruct->rcItem.left,
			lpDrawItemStruct->rcItem.top + max(0, (cyItem - m_cyText) / 2),
			ETO_OPAQUE, &(lpDrawItemStruct->rcItem), strText, strText.GetLength(), NULL);

		pDC->SetTextColor(oldTextColor);
		pDC->SetBkColor(oldBkColor);
	}

	if ((lpDrawItemStruct->itemAction & ODA_FOCUS) != 0)
		pDC->DrawFocusRect(&(lpDrawItemStruct->rcItem));
}

//----------------------------------------------------------------------------
void CWhiteCheckListBox::OnLButtonDown(UINT nFlags, CPoint point)
{
	SetFocus();

	// determine where the click is
	BOOL bInCheck;
	
	int nIndex = CheckFromPoint(point, bInCheck);

	// if the item is disabled, then eat the click
	if (!IsEnabled(nIndex))
	  return;

	CWnd* pParent = NULL;
	pParent = GetParent();
	ASSERT_VALID( pParent );

	if (m_nStyle != BS_CHECKBOX && m_nStyle != BS_3STATE)
	{
		// toggle the check mark automatically if the check mark was hit
		if (bInCheck)
		{
			int nModulo = (m_nStyle == BS_AUTO3STATE) ? 3 : 2;
			int nCheck;
			int nNewCheck;

			nCheck = GetCheck( nIndex );
			nCheck = (nCheck == nModulo) ? nCheck-1 : nCheck;

			nNewCheck = (nCheck+1)%nModulo;
			SetCheck( nIndex, nNewCheck );
			InvalidateCheck( nIndex );

			CCheckItemData aCheckItemData(nIndex, nNewCheck); 

			if ( (GetStyle()&(LBS_EXTENDEDSEL|LBS_MULTIPLESEL)) && GetSel(nIndex ) )
			{
				// The listbox is a multi-select listbox, and the user clicked on
				// a selected check, so change the check on all of the selected
				// items.
				SetSelectionCheck( nNewCheck );
			}
			else
			{
				CListBox::OnLButtonDown( nFlags, point );
			}

			// Inform parent of check
			pParent->SendMessage( WM_COMMAND, MAKEWPARAM( GetDlgCtrlID(), CLBN_CHKCHANGE ), (LPARAM)m_hWnd );
			pParent->SendMessage(UM_LIST_CHECK_CHANGED, GetDlgCtrlID(), (LPARAM)&aCheckItemData);

			return;
		}
	}

	// do default listbox selection logic
	CListBox::OnLButtonDown( nFlags, point );
}

//----------------------------------------------------------------------------------
// CSimpleProfilesListCtrl dialog
//----------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSimpleProfilesListCtrl, CTBTreeCtrl)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSimpleProfilesListCtrl, CTBTreeCtrl)
	//{{AFX_MSG_MAP(CSimpleProfilesListCtrl)
	ON_WM_CONTEXTMENU		()
	ON_COMMAND				(ID_PROFILE_PROF_NEW, 			OnNewProfile)
	ON_COMMAND				(ID_PROFILE_PROF_CLONE, 		OnCloneCurrentProfile)
	ON_COMMAND				(ID_PROFILE_PROF_REMOVE,		OnDeleteCurrentProfile)
	ON_COMMAND				(ID_PROFILE_PROF_MODIFY,		OnModifyCurrentProfile)
	ON_COMMAND				(ID_PROFILE_PROF_PREFERRED,	OnProfilePrefered)
	ON_COMMAND				(ID_PROFILE_PROF_RENAME,		OnProfileRename)
	ON_NOTIFY_REFLECT		(TVN_BEGINLABELEDIT,			OnItemBeginEdit)
	ON_NOTIFY_REFLECT		(TVN_ENDLABELEDIT,				OnItemEndEdit)
	ON_WM_LBUTTONDBLCLK		()
	ON_WM_RBUTTONDOWN		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSimpleProfilesListCtrl::CSimpleProfilesListCtrl(const CTBNamespace& aNameSpace /* = CTBNamespace()*/)
	:
	m_pImageList		(NULL),
	m_bSaveDefault		(FALSE),
	m_hPrefered			(NULL),
	m_pXMLDefaultInfo	(NULL),
	m_pXMLDocInfo		(NULL)
{
	m_bMultiSelect = TRUE;
	m_nsDoc = aNameSpace;
}

//-----------------------------------------------------------------------------
CSimpleProfilesListCtrl::~CSimpleProfilesListCtrl()
{
	if (m_pImageList)
		delete m_pImageList;

	if (m_pXMLDocInfo)
		delete m_pXMLDocInfo;

	CString* pItemStr;
	for (int nIdx = 0; nIdx <= m_arProfileNameList.GetUpperBound(); nIdx++)
	{
		if (m_arProfileNameList.GetAt(nIdx))
		{
			pItemStr = (CString*)m_arProfileNameList.GetAt(nIdx);
			delete pItemStr;
		}
	}
	m_arProfileNameList.RemoveAll();
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnItemBeginEdit(NMHDR* pNMHDR, LRESULT* pResult) 
{
	TV_DISPINFO*	pDispInfo	= (TV_DISPINFO*)pNMHDR;
	TV_ITEM*		pItem		= &(pDispInfo)->item;

	if (pItem)
		m_strOldProfName = GetItemText(pItem->hItem);
	
	*pResult = 0;
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnItemEndEdit(NMHDR* pNMHDR, LRESULT* pResult) 
{
	TV_DISPINFO* pDispInfo = (TV_DISPINFO*)pNMHDR;
	TV_ITEM* pItem= &(pDispInfo)->item;
	
	if	(
			!pItem->pszText				 ||
			m_strOldProfName.IsEmpty()	 || 
			_tcslen(pItem->pszText) == 0 
		)
	{
		if (!m_strOldProfName.IsEmpty())
			m_strOldProfName.Empty();

		return;
	}

	if (IsPresent(pItem->pszText))
	{
		pItem->pszText = (LPTSTR) ((LPCTSTR)m_strOldProfName);
		AfxMessageBox(_TB("Duplicate name"));
		*pResult = 1;
		return;
	}
	
	m_strNewProfName = pItem->pszText;
	
	if (m_strOldProfName.CompareNoCase(m_strNewProfName) == 0)
	{
		*pResult = 1;
		m_strOldProfName.Empty();
		return;
	}

	CString strProfilePath(*((CString*)GetItemData(pItem->hItem)));

	CFunctionDescription aFuncDescri;
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.RenameExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_nsDoc.ToString());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(strProfilePath);
	aFuncDescri.GetParamValue(szNewName)->Assign(m_strNewProfName);

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
	{
		SetItemText(pItem->hItem, m_strNewProfName);
		ModifyProfileItem(pItem->hItem, aFuncDescri.GetParamValue(szProfilePath)->Str(), TRUE);
	}
	else
		AfxMessageBox(_TB("Impossible to rename the selected profile"));		
	
	m_strOldProfName.Empty();
	*pResult = 0;
}

//-----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::SetDocumentNameSpace(const CTBNamespace& aNameSpace)
{
	m_nsDoc = aNameSpace;

	if (m_nsDoc.IsValid())
	{
		m_pXMLDocInfo = new CXMLDocInfo(m_nsDoc);
		if (m_pXMLDocInfo->LoadAllFiles())
		{
			m_pXMLDefaultInfo = m_pXMLDocInfo->GetDefaultInfo();
				
			if (!m_pXMLDefaultInfo)
			{
				m_pXMLDefaultInfo = new CXMLDefaultInfo(m_nsDoc);
				m_pXMLDocInfo->m_pDefaultInfo = m_pXMLDefaultInfo;
			}
		}
		else
		{
			SAFE_DELETE(m_pXMLDocInfo);
			TRACE("The document description is missing");
			ASSERT(FALSE);
		}
	}
}

//-----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::InitializeImageList()
{
	if (!::IsWindow(m_hWnd))
		return;

	//SetExtendedStyle(GetExtendedStyle() | LVS_EX_TRACKSELECT | LVS_EX_ONECLICKACTIVATE);

	SetBkColor(::GetSysColor(COLOR_WINDOW));
	//SetTextBkColor(::GetSysColor(COLOR_WINDOW));
	SetTextColor(::GetSysColor(COLOR_WINDOWTEXT));

	//SetHoverTime(100);
	
	HICON hIcon[3];
	m_pImageList  = new CImageList;
	m_pImageList->Create(32, 32, ILC_COLOR32, 3, 3);
	m_pImageList->SetBkColor(::GetSysColor(COLOR_WINDOW));
	
	hIcon[0] = AfxGetApp()->LoadIcon(IDI_PROFILE_NORMAL);
	hIcon[1] = AfxGetApp()->LoadIcon(IDI_PROFILE_CUSTOM);
	hIcon[2] = AfxGetApp()->LoadIcon(IDI_PROFILE_PREFERRED);
	
	for(int n = 0 ; n < 3 ; n++)
	{
		m_pImageList->Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}

	SetImageList(m_pImageList,LVSIL_NORMAL);
}


//-----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::AddProfileItems(const CStringArray& aProfileList)
{
	CString  strPrefProf;
	
	if (m_pXMLDefaultInfo)
		strPrefProf = m_pXMLDefaultInfo->GetPreferredProfile();

	for (int i = 0 ; i < aProfileList.GetSize(); i++ )
	{
		//creo le istanze di CXMLProfileInfo relative ai profili caricati e le inserisco
		// nella lista
		if (aProfileList.GetAt(i).IsEmpty())
			continue;

		HTREEITEM hit = InsertProfileItem(aProfileList.GetAt(i));
		if (GetItemText(hit).CompareNoCase(strPrefProf) == 0)
			m_hPrefered = hit;
	}
	HTREEITEM hItem = GetNextItem(TVGN_ROOT, TVGN_FIRSTVISIBLE);
	if (hItem != NULL)
		SelectItem(hItem);
}

//-----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::Fill()
{
	if (!::IsWindow(m_hWnd))
		return;

	DeleteAllItems();

	CStringArray aProfileList;

	::GetAllExportProfiles(m_nsDoc, &aProfileList, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
	AddProfileItems(aProfileList);
}


//-------------------------------------------------------------------------------
BOOL CSimpleProfilesListCtrl::IsPresent(const CString& strProfName) const
{
	HTREEITEM hSib = GetRootItem();
	CString str = GetItemText(hSib);
	if (str.CompareNoCase(strProfName) == 0)
		return TRUE;
	
	while(hSib = GetNextSiblingItem(hSib))
	{
		str = GetItemText(hSib);

		if (str.CompareNoCase(strProfName) == 0)
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnRButtonDown(UINT nFlags, CPoint point)
{
	__super::OnRButtonDown(nFlags, point);

	HTREEITEM hItemToSelect = HitTest(point);
	if (hItemToSelect)
		SelectItem(hItemToSelect);

	ScreenToClient(&point);
	this->GetParent()->ClientToScreen(&point);
	ClientToScreen(&point);

	OnContextMenu(this, point);
}

//-----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnContextMenu(CWnd* /*pWnd*/, CPoint mousePos)
{	
	HTREEITEM	hit	= GetSelectedItem();
	if (!hit)
		return;
	CString strProfileName = GetItemText(hit);
	CString strProfilePath(*(CString*)GetItemData(hit));

	CMenu menu;
	menu.CreatePopupMenu();
	CString strMenuItem = _TB("New");
	menu.AppendMenu(MF_STRING, ID_PROFILE_PROF_NEW, strMenuItem);

	
	menu.AppendMenu(MF_SEPARATOR);                                  
	strMenuItem = _TB("Copy");
	menu.AppendMenu(MF_STRING, ID_PROFILE_PROF_CLONE, strMenuItem);


	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strProfilePath);

	if (ePosType == CPathFinder::USERS)
	{
		strMenuItem = _TB("Rename");
		menu.AppendMenu(MF_STRING, ID_PROFILE_PROF_RENAME, strMenuItem);
		strMenuItem = _TB("Remove");
		menu.AppendMenu(MF_STRING, ID_PROFILE_PROF_REMOVE, strMenuItem);
	}
	
	if (m_pXMLDefaultInfo && m_pXMLDefaultInfo->GetPreferredProfile().CompareNoCase(strProfileName))
	{
		menu.AppendMenu(MF_SEPARATOR);
		m_bSaveDefault = TRUE;
		
		strMenuItem = _TB("Preferred");
		menu.AppendMenu(MF_STRING, ID_PROFILE_PROF_PREFERRED, strMenuItem);
	}

	menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, mousePos.x, mousePos.y, this);		
}


//--------------------------------------------------------------------------------------------------------
HTREEITEM CSimpleProfilesListCtrl::InsertProfileItem(const CString& strProfilePath, BOOL bSelect /*=FALSE*/) 
{
	CString strProfileName = CXMLProfileInfo::GetProfileNameFromPath(strProfilePath);

	if (strProfileName.IsEmpty())
		return NULL;

	HTREEITEM hit = InsertItem(strProfileName);
	if (hit)
	{
		CString* pItemString = new CString(strProfilePath);
		m_arProfileNameList.Add((CObject*)pItemString);
		SetItemData(hit, (DWORD)pItemString);
		SetItemIcon(hit);
		if (bSelect) SelectItem(hit);
	}
	return hit;
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::ModifyProfileItem(HTREEITEM hit,const CString& strProfilePath, BOOL bSelect /*=FALSE*/) 
{
	if (strProfilePath.IsEmpty() || !hit)
		return;

	CString* pOldPathStr = (CString*)GetItemData(hit);
	CString strOldPath(*pOldPathStr);
	if (strProfilePath.CompareNoCase(strOldPath))
	{
		CString* pNewPathStr = new CString(strProfilePath);
		RemoveStringFromArray(pOldPathStr);
		m_arProfileNameList.Add((CObject*)pNewPathStr);
		SetItemData(hit, (DWORD)pNewPathStr);
	}

	SetItemIcon(hit);
	if (bSelect) 
		SelectItem(hit);
	if (GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(CProfiliMngWizPage)))
		 ((CProfiliMngWizPage*)GetParent())->EnableButtons(hit);
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::SetItemIcon(HTREEITEM hit) 
{
	CString strProfileName = GetItemText(hit);
	CString strProfilePath(*((CString*)GetItemData(hit)));

	if (m_pXMLDefaultInfo && !strProfileName.CompareNoCase(m_pXMLDefaultInfo->GetPreferredProfile()))
	{
		SetItemImage(hit, PREFERED_IMAGE_IDX, PREFERED_IMAGE_IDX);
		return;
	}

	if (AfxGetPathFinder()->GetPosTypeFromPath(strProfilePath) == CPathFinder::STANDARD)
		SetItemImage(hit, STANDARD_IMAGE_IDX, STANDARD_IMAGE_IDX);
	else
		SetItemImage(hit, CUSTOM_IMAGE_IDX, CUSTOM_IMAGE_IDX);
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnLButtonDblClk(UINT nFlags, CPoint point) 
{
	HTREEITEM hit = GetSelectedItem();
	CRect rcItem;
	if (GetItemRect(hit, &rcItem, LVIR_BOUNDS))
	{		
		if (rcItem.PtInRect(point))
			OnModifyCurrentProfile();
	}
	else
		OnNewProfile();

	__super::OnLButtonDblClk(nFlags, point);
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::RemoveStringFromArray(CString* pItemStr)
{
	for (int nIdx = 0; nIdx <= m_arProfileNameList.GetUpperBound(); nIdx++)
	{
		if (pItemStr && m_arProfileNameList.GetAt(nIdx) && pItemStr == (CString*)m_arProfileNameList.GetAt(nIdx))
		{
			delete pItemStr;
			pItemStr = NULL;
			m_arProfileNameList.RemoveAt(nIdx);
			break;
		}
	}
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnNewProfile()
{	
	if (!m_strOldProfName.IsEmpty()) //significa che sto rinominando
		return;
	CString strNewName = _TB("New");
	//calcolo il nome da proporre per il nuovo profilo
	CString strProfileName = strNewName;
	int nCount = 0;
	BOOL bFound = TRUE;
	//calcolo il nuovo nome partendo da Nuovo e controllando gli eventuali profili presenti nella directory
	while(bFound)
	{
		bFound = FALSE;
		for (int nIdx = 0; nIdx <= m_arProfileNameList.GetUpperBound(); nIdx++)
		{
			if (bFound = (strProfileName.CompareNoCase(::GetName(*(CString*)m_arProfileNameList.GetAt(nIdx))) == 0))
			{
				strProfileName.Format(_T("%s_%d"), strNewName, ++nCount);
				break;
			}
		}
	}

	CFunctionDescription aFuncDescri;
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.NewExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_nsDoc.ToString());
	aFuncDescri.GetParamValue(szNewProfileName)->Assign(strProfileName);
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)CPathFinder::USERS);
	aFuncDescri.GetParamValue(szUserName)->Assign(AfxGetLoginInfos()->m_strUserName);
	aFuncDescri.GetParamValue(szProfilePath)->Assign(DataStr(_T("")));

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		InsertProfileItem(aFuncDescri.GetParamValue(szProfilePath)->Str(), TRUE);
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnCloneCurrentProfile()
{
	HTREEITEM hit = GetSelectedItem();
	if (!hit)
		return;
	CString strProfilePath(*((CString*)GetItemData(hit)));
	
	if (strProfilePath.IsEmpty())
		return;

	CFunctionDescription aFuncDescri;
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.CloneExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_nsDoc.ToString());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(strProfilePath);
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)(CPathFinder::USERS));
	aFuncDescri.GetParamValue(szUserName)->Assign(AfxGetLoginInfos()->m_strUserName);

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		InsertProfileItem(aFuncDescri.GetParamValue(szProfilePath)->Str());
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnDeleteCurrentProfile()
{
	HTREEITEM hit = GetSelectedItem();
	CString strProfileName = GetItemText(hit);
	CString strProfilePath(*((CString*)GetItemData(hit)));

	if (strProfileName.IsEmpty())
		return;

	if (AfxMessageBox(_TB("Do you need to remove the profile?"), MB_YESNO) == IDYES)
	{
		CFunctionDescription aFuncDescri;
		AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.DeleteExportProfile"), aFuncDescri); 
		aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_nsDoc.ToString());
		aFuncDescri.GetParamValue(szProfilePath)->Assign(DataStr(strProfilePath));

		if (!AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		{
			AfxMessageBox(_TB("Unable to delete the profile. Please check the profile itself or its client documents are not write-protected."));
			return;
		}

		// se il profilo che ho cancellato è uno custom dell'utente allora carico le informazioni del profilo alluser se presente
		// altrimenti lo standard se esiste; se non esiste alcun profilo con quel nome allora lo tolgo dalla lista
		if (!ExistProfile(m_nsDoc, strProfileName))
		{
			RemoveStringFromArray((CString*)GetItemData(hit));
			if (m_hPrefered == hit)
				m_hPrefered = NULL;
			DeleteItem(hit);			
		}
		else
		{	
			CTBNamespace aNsProfile;
			if (GetProfileNamespace(m_nsDoc, strProfileName, aNsProfile))
				ModifyProfileItem(hit, AfxGetPathFinder()->GetFileNameFromNamespace(aNsProfile, AfxGetLoginInfos()->m_strUserName), TRUE);
		}
	}
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnModifyCurrentProfile()
{
	HTREEITEM hit = GetSelectedItem();
	if (!hit) 
		return;
	
	CString strProfilePath(*((CString*)GetItemData(hit)));
	if (strProfilePath.IsEmpty())
		return;

	CFunctionDescription aFuncDescri;
	AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.ModifyExportProfile"), aFuncDescri); 
	aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_nsDoc.ToString());
	aFuncDescri.GetParamValue(szProfilePath)->Assign(strProfilePath);
	((DataInt*)aFuncDescri.GetParamValue(szPosType))->Assign((int)(CPathFinder::USERS));
	aFuncDescri.GetParamValue(szUserName)->Assign(AfxGetLoginInfos()->m_strUserName);

	if (AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0))
		ModifyProfileItem(hit, aFuncDescri.GetParamValue(szProfilePath)->Str(), TRUE);
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnProfilePrefered()
{
	HTREEITEM hit = GetSelectedItem();
	if (!hit) 
		return;

	CString strProfileName = GetItemText(hit);
	if (
			strProfileName.IsEmpty() || 
			!m_pXMLDefaultInfo || m_pXMLDefaultInfo->GetPreferredProfile().CompareNoCase(strProfileName) == 0
		)
		return;

	m_bSaveDefault = TRUE;
	m_pXMLDefaultInfo->SetPreferredProfile(strProfileName);
	if (m_bSaveDefault)
		m_pXMLDefaultInfo->UnParse(CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);				

	if (m_hPrefered)
        SetItemIcon(m_hPrefered);

	m_hPrefered = hit;
	SetItemIcon(hit);
	SelectItem(hit);
}

//----------------------------------------------------------------------------
void CSimpleProfilesListCtrl::OnProfileRename()
{
	//SendMessage(LVM_EDITLABEL, GetNextItem(-1, LVNI_SELECTED), 0);
	SendMessage(TVM_EDITLABEL, 0, (LPARAM)GetSelectedItem());
}
IMPLEMENT_DYNAMIC(CProfiliMngWizPage, CLocalizablePropertyPage)
//----------------------------------------------------------------------------
// CProfiliMngDlg dialog
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProfiliMngWizPage, CLocalizablePropertyPage)
	//{{AFX_MSG_MAP(CProfiliMngWizPage)
	ON_COMMAND	(IDC_PROFMNGWIZ_NEW_PROF, 		OnNewProfile)
	ON_COMMAND	(IDC_PROFMNGWIZ_CLONE_PROF, 	OnCloneCurrentProfile)
	ON_COMMAND	(IDC_PROFMNGWIZ_DEL_PROF,		OnDeleteCurrentProfile)
	ON_COMMAND	(IDC_PROFMNGWIZ_MOD_PROF,		OnModifyCurrentProfile)
	ON_COMMAND	(IDC_PROFMNGWIZ_RENAME_PROF,	OnRenameCurrentProfile)
	ON_NOTIFY	(TVN_SELCHANGED,				IDC_PROFMNGWIZ_LISTCTRL, OnProfListItemStateChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CProfiliMngWizPage::CProfiliMngWizPage(CWnd* pParent /*=NULL*/)
	:
	CLocalizablePropertyPage(IDD_PROFILES_MNG_WIZ_PROPPAGE),
	m_pXMLDocInfo				(NULL)
{
}

//----------------------------------------------------------------------------
CProfiliMngWizPage::CProfiliMngWizPage(const CTBNamespace& aNameSpace, CXMLDocInfo* pXMLDocInfo /*= NULL*/)
	: 
	CLocalizablePropertyPage	(IDD_PROFILES_MNG_WIZ_PROPPAGE),
	m_pXMLDocInfo				(pXMLDocInfo)
{
	m_DocumentNameSpace = aNameSpace;

	if (m_pXMLDocInfo && m_pXMLDocInfo->GetNamespaceDoc() != aNameSpace)
	{
		ASSERT(FALSE);
	}
}

//----------------------------------------------------------------------------
BOOL CProfiliMngWizPage::OnInitDialog() 
{
	CLocalizablePropertyPage::OnInitDialog();

	if (!m_DocumentNameSpace.IsValid())
	{
		ASSERT(FALSE);
		return TRUE;
	}

	VERIFY (m_ProfilesList.SubclassDlgItem(IDC_PROFMNGWIZ_LISTCTRL, this));
	
	m_ProfilesList.InitializeImageList();	
	m_ProfilesList.SetDocumentNameSpace(m_DocumentNameSpace);
	m_ProfilesList.Fill();

	if (AfxGetLoginInfos()->m_bAdmin)
		GetDlgItem(IDC_STATIC_ADMIN_MSG)->ShowWindow(SW_SHOW);
	
	if (m_ProfilesList.GetCount() == 0)
		EnableButtons(FALSE);

	return TRUE; 
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::EnableButtons(BOOL bEnable)
{
	GetDlgItem(IDC_PROFMNGWIZ_MOD_PROF)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNGWIZ_DEL_PROF)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNGWIZ_CLONE_PROF)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNGWIZ_RENAME_PROF)->EnableWindow(bEnable);
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnOK() 
{
	if (
		m_pXMLDocInfo									&& 
		m_pXMLDocInfo->m_pDefaultInfo					&&
		m_ProfilesList.m_pXMLDocInfo					&&
		m_ProfilesList.m_pXMLDocInfo->GetDefaultInfo()	&&
		m_pXMLDocInfo->m_pDefaultInfo != m_ProfilesList.m_pXMLDocInfo->GetDefaultInfo()
		)
	{
		*(m_pXMLDocInfo->m_pDefaultInfo) = *(m_ProfilesList.m_pXMLDocInfo->GetDefaultInfo());
	}
		

	CLocalizablePropertyPage::OnOK();
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnNewProfile()
{
	m_ProfilesList.OnNewProfile();
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnCloneCurrentProfile()
{
	m_ProfilesList.OnCloneCurrentProfile();
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnDeleteCurrentProfile()
{
	m_ProfilesList.OnDeleteCurrentProfile();
	if (m_ProfilesList.GetCount() == 0)
		EnableButtons(FALSE);
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnModifyCurrentProfile()
{
	m_ProfilesList.OnModifyCurrentProfile();
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnRenameCurrentProfile()
{
	m_ProfilesList.SetFocus();
	m_ProfilesList.OnProfileRename();
}

//----------------------------------------------------------------------------
void CProfiliMngWizPage::EnableButtons(HTREEITEM hit)
{
	CString strProfilePath(*(CString*)m_ProfilesList.GetItemData(hit));

	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strProfilePath);
	BOOL bEnable= (!strProfilePath.IsEmpty() && ePosType == CPathFinder::USERS);

	GetDlgItem(IDC_PROFMNGWIZ_RENAME_PROF)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNGWIZ_DEL_PROF)->EnableWindow(bEnable);

	GetDlgItem(IDC_PROFMNGWIZ_CLONE_PROF)->EnableWindow(TRUE);	
	GetDlgItem(IDC_PROFMNGWIZ_MOD_PROF)->EnableWindow(TRUE);
}


//----------------------------------------------------------------------------
void CProfiliMngWizPage::OnProfListItemStateChanged(NMHDR* pNMHDR, LRESULT* pResult) 
{
	LPNMTREEVIEW lpListView = (LPNMTREEVIEW)pNMHDR;

	if (lpListView && lpListView->itemNew.hItem)
		EnableButtons(lpListView->itemNew.hItem);

	*pResult = 0;
}

/////////////////////////////////////////////////////////////////////////////
//		 CProfileWizMasterDlg property sheet implementation
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CProfileWizMasterDlg, CWizardMasterDialog)
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProfileWizMasterDlg, CWizardMasterDialog)
	//{{AFX_MSG_MAP(CProfileWizMasterDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CProfileWizMasterDlg::CProfileWizMasterDlg(CXMLProfileInfo* pXMLProfileInfo, CWnd* pParent /*=NULL*/)
	: 
	CWizardMasterDialog		(IDD_PROFILE_WIZARD_MASTER, pParent),
	m_bConfigXRef			(TRUE),
	m_bConfigUsrCriteria	(TRUE)
{
	if (pXMLProfileInfo && pXMLProfileInfo->m_pExportCriteria)
		m_ProfileSelectionQueryPage.m_pXMLExpCriteria = pXMLProfileInfo->m_pExportCriteria;

	// Add the property pages
	AddPage (&m_ProfileWizPresentationPage);
	AddPage (&m_ProfileExpPropPage);
	AddPage (&m_ProfileMagicDocsPage);
	AddPage (&m_ProfileDBTPage);
	AddPage (&m_ProfileChooseParamPage);
	AddPage (&m_ProfileFieldPage);
	AddPage (&m_ProfileXRefPage);
	AddPage (&m_ProfileSelectionQueryPage);
	AddPage (&m_ProfileFinishPage);

	m_pXMLProfileInfo	= pXMLProfileInfo;
	m_bConfigField = pXMLProfileInfo->m_bUseFieldSel;
}

//---------------------------------------------------------------------------
BOOL CProfileWizMasterDlg::OnInitDialog() 
{
	SetPlaceholderID(IDC_PROF_WIZ_STATIC_SHEET_RECT);

	CWizardMasterDialog::OnInitDialog();

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//---------------------------------------------------------------------------
BOOL CProfileWizMasterDlg::OnWizardFinish() 
{
	
	return TRUE;
}

// apre la dialog per la gestione della query e degli eventuali parametri
//----------------------------------------------------------------------------
void CProfileWizMasterDlg::OnSelections(CXMLProfileInfo* pCurrentProfile)
{
	if (!pCurrentProfile || !pCurrentProfile->GetDBTInfoArray())
		return;
	
	CXMLDBTInfo* pDBTInfo = pCurrentProfile->GetDBTAt(0);
	if (!pDBTInfo)
		return;
	
	if (!pCurrentProfile->m_pExportCriteria)
		pCurrentProfile->m_pExportCriteria = new CXMLExportCriteria(pCurrentProfile);

	CSelectionQueryDlg	aDlg(pCurrentProfile->m_pExportCriteria);
	if (aDlg.DoModal() == IDOK)
		pCurrentProfile->SetModified(TRUE);
}

/////////////////////////////////////////////////////////////////////////////
// CProfileWizardPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CProfileWizardPage, CWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CProfileWizardPage, CWizardPage)
	//{{AFX_MSG_MAP(CProfileWizardPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileWizardPage::CProfileWizardPage(UINT nIDTemplate, CWnd* pParent /*=NULL*/) 
:
	m_pProfileSheet		(NULL),
	CWizardPage			(nIDTemplate, pParent)
{
	m_pToolTip = new CToolTipCtrl;
}

//--------------------------------------------------------------------
CProfileWizardPage::~CProfileWizardPage()
{
	if (m_pToolTip)
	{
		delete m_pToolTip;

		m_pToolTip = NULL;
	}
}

//--------------------------------------------------------------------
BOOL CProfileWizardPage::OnInitDialog()
{
	CWizardPage::OnInitDialog();

	m_pProfileSheet = (CProfileWizMasterDlg*)GetParent();
	ASSERT_KINDOF(CProfileWizMasterDlg, m_pProfileSheet);
	
	if (!m_pToolTip->Create(this))
		ASSERT(FALSE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CProfileWizardPage::OnCreatePage()
{
	if (!m_pParent || !m_pParent->IsKindOf(RUNTIME_CLASS(CProfileWizMasterDlg)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CProfileWizardPage::PreTranslateMessage(MSG* pMsg) 
{
	if (m_pToolTip != NULL)
		m_pToolTip->RelayEvent(pMsg);
	
	return CWizardPage::PreTranslateMessage(pMsg);
}

//--------------------------------------------------------------------
class CTransDocInfo : public CObject
{
public:
	CString		 m_strXSLTName;
	CString		 m_strXSLTTitle;


public:
	CTransDocInfo(const CString& strXSLTName, const CString& strXSLTTitle) 
		:
		m_strXSLTName	(strXSLTName),
		m_strXSLTTitle	(strXSLTTitle)
		{}
};

		
//--------------------------------------------------------------------
// CProfileWizPresentationPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileWizPresentationPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileWizPresentationPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileWizPresentationPage)
	ON_NOTIFY		(UDN_DELTAPOS, IDC_PROFILES_MNG_VERSION_SPIN,		OnDeltaPosSpinVersion)
	ON_BN_CLICKED	(IDC_PROFMNG_TRANSFORM_CHECK,	OnTransformCheck)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileWizPresentationPage::CProfileWizPresentationPage() 
	: 
	CProfileWizardPage(IDD_PROFILE_WIZ_PRESENTATION_PAGE),
	m_Image			  (IDB_PROFILE_WIZARD_PRESENTATION_PAGE),
	m_pProfileInfo	  (NULL),
	m_nProfileVersion	(0)
{
}

//-----------------------------------------------------------------------------
void CProfileWizPresentationPage::DoDataExchange(CDataExchange* pDX)
{
	CWizardPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CGeneralPage)
	DDX_Text (pDX, IDC_PROFMNG_VERSION_EDIT, m_nProfileVersion);
	DDV_MinMaxInt(pDX, m_nProfileVersion, 0, 1000);
	//}}AFX_DATA_MAP
}



//--------------------------------------------------------------------
BOOL CProfileWizPresentationPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_pProfileInfo = m_pProfileSheet->m_pXMLProfileInfo;	
	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_PRESENTATION_PAGE, this);
	
	if (!m_pProfileInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	CWnd* pDlgItem = GetDlgItem(IDC_PROFMNG_PROFILE_NAME_EDIT);
	pDlgItem->SetWindowText(m_pProfileInfo->GetName());
	pDlgItem->EnableWindow(m_pProfileInfo->m_bNewProfile);
	((CEdit*)pDlgItem)->SetLimitText(50);


	pDlgItem = GetDlgItem(IDC_PROFMNG_VERSION_EDIT);
	pDlgItem->SetWindowText(m_pProfileInfo->GetVersion());
	pDlgItem->EnableWindow(!m_pProfileInfo->IsReadOnly());
	
	m_TransDocComboBox.SubclassDlgItem(IDC_PROFMNG_TRANSDOC_COMBO, this);
	GetDlgItem(IDC_PROFMNG_PROFILE_NAME_EDIT);

	LoadTransformInfo();
	if (m_arTransDocs.GetSize() <= 0)
	{
		GetDlgItem(IDC_PROFMNG_TRANSFORM_CHECK)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_PROFMNG_TRANSFORM_GROUP)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_PROFMNG_AVAILABLEXSLT_STATIC)->ShowWindow(SW_HIDE);
		m_TransDocComboBox.ShowWindow(SW_HIDE);
	}
	else
	{
		((CButton*)GetDlgItem(IDC_PROFMNG_TRANSFORM_CHECK))->SetCheck(m_pProfileInfo->m_pHeaderInfo->m_bTransform);
		
		pDlgItem = GetDlgItem(IDC_PROFMNG_AVAILABLEXSLT_STATIC);
		CString strXSLTStatic;
		pDlgItem->GetWindowText(strXSLTStatic);
		strXSLTStatic = cwsprintf(strXSLTStatic, m_pProfileInfo->GetDocumentTitle());
		pDlgItem->SetWindowText(strXSLTStatic);	

		OnTransformCheck();
	}
	return TRUE;
}

//--------------------------------------------------------------------
void CProfileWizPresentationPage::OnActivate()
{	
	CProfileWizardPage::OnActivate();

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(_TB("Export profiles"));
}

//--------------------------------------------------------------------
LRESULT CProfileWizPresentationPage::OnWizardNext()
{
	if (!m_pProfileSheet || !m_pProfileSheet->m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return IDD_PROFILE_WIZ_PRESENTATION_PAGE;
	}
	if (!m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly())
	{
		CString strName;
		GetDlgItem(IDC_PROFMNG_PROFILE_NAME_EDIT)->GetWindowText(strName);
		m_pProfileSheet->m_pXMLProfileInfo->m_strProfileName = strName;
		
		CString strVersion;
		GetDlgItem(IDC_PROFMNG_VERSION_EDIT)->GetWindowText(strVersion);
		m_pProfileSheet->m_pXMLProfileInfo->SetVersion(strVersion);
	}

	int nCurrSel = m_TransDocComboBox.GetCurSel();
	if( ((CButton*)GetDlgItem(IDC_PROFMNG_TRANSFORM_CHECK))->GetCheck() && nCurrSel != CB_ERR)
	{
		m_pProfileInfo->GetHeaderInfo()->m_bTransform = TRUE;
		CTransDocInfo* pTransDocInfo =  (CTransDocInfo*)m_TransDocComboBox.GetItemDataPtr(nCurrSel);
		if (pTransDocInfo)
			m_pProfileInfo->GetHeaderInfo()->m_strTransformXSLT = pTransDocInfo->m_strXSLTName;
	}
	else
	{
		m_pProfileInfo->GetHeaderInfo()->m_bTransform = FALSE;
		m_pProfileInfo->GetHeaderInfo()->m_strTransformXSLT.Empty();
	}

	return CProfileWizardPage::OnWizardNext();
}

//----------------------------------------------------------------------------
void CProfileWizPresentationPage::OnDeltaPosSpinVersion(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_UPDOWN* pNMUpDown = (NM_UPDOWN*)pNMHDR;
	
	CString strVal;
	
	((CEdit*)GetDlgItem(IDC_PROFMNG_VERSION_EDIT))->GetWindowText(strVal);
	
	int i = _ttoi(((TCHAR*)(LPCTSTR)strVal));

	if (pNMUpDown->iDelta < 0)
		i++;
	else
		if (i > 0)
			i--;
		else
			i = 0;

	pNMUpDown->iPos = i;
	
	strVal.Format(_T("%d"), i);

	((CEdit*)GetDlgItem(IDC_PROFMNG_VERSION_EDIT))->SetWindowText(strVal);
	
	*pResult = 0;
}

//--------------------------------------------------------------------
void CProfileWizPresentationPage::LoadTransformInfo()
{
	//Search the xslt files in document folder
	CStringArray arDocumentXSLT;
	CString strXSLTFile, strXSLTDescri;
	CTBNamespace nsTransDoc;

	AfxGetFileSystemManager()->GetFiles(AfxGetPathFinder()->GetDocumentDescriptionPath( m_pProfileInfo->GetNamespaceDoc(), CPathFinder::STANDARD), FileExtension::ANY_XSL(), &arDocumentXSLT);
	if (arDocumentXSLT.GetSize() > 0)
	{
		for(int i = 0 ; i < arDocumentXSLT.GetSize() ; i++)
		{
			strXSLTFile = arDocumentXSLT.GetAt(i);			
			GetXSLTInformation(strXSLTFile, strXSLTDescri, nsTransDoc);
			CTransDocInfo* pTransDocInfo = new CTransDocInfo(::GetName(strXSLTFile), strXSLTDescri);
			m_arTransDocs.Add(pTransDocInfo);			
		}
	}		
}

//--------------------------------------------------------------------
void CProfileWizPresentationPage::OnTransformCheck()
{
	if (((CButton*)GetDlgItem(IDC_PROFMNG_TRANSFORM_CHECK))->GetCheck())
	{	
		m_TransDocComboBox.EnableWindow(TRUE);	
		int nSel = CB_ERR;	
		CTransDocInfo* pTransDocInfo = NULL;
		for (int nIdx = 0; nIdx < m_arTransDocs.GetSize(); nIdx++)
		{
			pTransDocInfo = (CTransDocInfo*)m_arTransDocs.GetAt(nIdx);
			m_TransDocComboBox.InsertString(nIdx, pTransDocInfo->m_strXSLTTitle);
			m_TransDocComboBox.SetItemDataPtr(nIdx, pTransDocInfo);
					
			if (!pTransDocInfo->m_strXSLTName.CompareNoCase(m_pProfileInfo->GetHeaderInfo()->m_strTransformXSLT))
				nSel = nIdx;
		}

		if (nSel == CB_ERR)
			nSel = 0;
		m_TransDocComboBox.SetCurSel(nSel);
	}
	else
	{
		m_TransDocComboBox.ResetContent();
		m_TransDocComboBox.EnableWindow(FALSE);
	}
}


//--------------------------------------------------------------------
// CProfileExpPropPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileExpPropPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileExpPropPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileExpPropPage)
	ON_NOTIFY		(UDN_DELTAPOS, IDC_PROFMNG_MAXFILE_DIM_SPIN,		OnDeltaPosMaxDim)
	ON_NOTIFY		(UDN_DELTAPOS, IDC_PROFILES_MNG_MAX_DOC_SPIN,		OnDeltaPosSpinMaxDoc)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CProfileExpPropPage::DoDataExchange(CDataExchange* pDX)
{
	CWizardPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CGeneralPage)
	DDX_Text (pDX, IDC_PROFMNG_MAXFILE_DIM_EDIT, m_nMaxDim);
	DDV_MinMaxInt(pDX, m_nMaxDim, HEADER_MIN_DOC_DIMENSION, HEADER_MAX_DOC_DIMENSION);
	DDX_Text (pDX, IDC_PROFMNG_MAXDOC_EDIT,	m_nMaxDocument);
	DDV_MinMaxInt(pDX, m_nMaxDocument, HEADER_MIN_DOCUMENT_NUM, HEADER_MAX_DOCUMENT_NUM);
	//}}AFX_DATA_MAP
}

//--------------------------------------------------------------------
CProfileExpPropPage::CProfileExpPropPage() 
	: 
	CProfileWizardPage(IDD_PROFILE_WIZ_EXP_PROP_PAGE),
	m_Image			  (IDB_PROFILE_WIZARD_EXP_PROP),
	m_nMaxDim			(1),
	m_nMaxDocument		(1)
{
}

//--------------------------------------------------------------------
BOOL CProfileExpPropPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_EXP_PROP, this);

	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	m_nMaxDocument	= m_pProfileSheet->m_pXMLProfileInfo->GetMaxDocument();
	m_nMaxDim		= m_pProfileSheet->m_pXMLProfileInfo->GetMaxDimension();

	//url data
	CString strUrlData = m_pProfileSheet->m_pXMLProfileInfo->GetUrlData();
	if (strUrlData.IsEmpty())
		strUrlData = m_pProfileSheet->m_pXMLProfileInfo->GetDocumentName();

	if (strUrlData.Right(4).CompareNoCase(szXmlExt) != 0)
		strUrlData += szXmlExt;

	BOOL bEnable = !m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly();
	CWnd* pWnd = GetDlgItem(IDC_PROFMNG_DATAURL_EDIT);
	pWnd->SetWindowText(strUrlData);
	pWnd->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNG_ENV_EXT_EDIT)->EnableWindow(::AfxGetParameters()->f_UseEnvClassExt && bEnable);
	GetDlgItem(IDC_PROFMNG_MAXFILE_DIM_EDIT)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNG_MAXDOC_EDIT)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFMNG_MAXFILE_DIM_SPIN)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROFILES_MNG_MAX_DOC_SPIN)->EnableWindow(bEnable);

	if (m_pProfileSheet->m_pXMLProfileInfo->m_pHeaderInfo)
	{
		pWnd = GetDlgItem(IDC_WIZ_PROFILE_ENV);
		pWnd->SetWindowText(m_pProfileSheet->m_pXMLProfileInfo->m_pHeaderInfo->m_strEnvClassTitle);
		pWnd = GetDlgItem(IDC_PROFMNG_ENV_EXT_EDIT);
		pWnd->SetWindowText(m_pProfileSheet->m_pXMLProfileInfo->m_pHeaderInfo->GetEnvClassExt());
		((CEdit*)pWnd)->SetLimitText(10);
		pWnd->EnableWindow(bEnable);
	}
	
	UpdateData(FALSE);
	return TRUE;
}

//--------------------------------------------------------------------
void CProfileExpPropPage::OnActivate()
{	
	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export profiles - Export properties");

	if (m_pProfileSheet) m_pProfileSheet->SetTitle(strTitle);
}

//--------------------------------------------------------------------
LRESULT CProfileExpPropPage::OnWizardNext()
{
	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}
	
	if (!m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly())
	{
		if (!UpdateData(TRUE))
			return -1;

		if (m_nMaxDocument > 0)
			m_pProfileSheet->m_pXMLProfileInfo->SetMaxDocument(m_nMaxDocument);
		if (m_nMaxDim > 0)
			m_pProfileSheet->m_pXMLProfileInfo->SetMaxDimension(m_nMaxDim);


		//url data
		CString strWindowText;
		GetDlgItem(IDC_PROFMNG_DATAURL_EDIT)->GetWindowText(strWindowText);

		if (strWindowText.IsEmpty())
			strWindowText = m_pProfileSheet->m_pXMLProfileInfo->GetDocumentName();

		if (strWindowText.Right(4).CompareNoCase(szXmlExt) != 0)
			strWindowText += szXmlExt;

		m_pProfileSheet->m_pXMLProfileInfo->SetUrlData(strWindowText);
		
		GetDlgItem(IDC_PROFMNG_ENV_EXT_EDIT)->GetWindowText(strWindowText);
		
		if (m_pProfileSheet->m_pXMLProfileInfo->m_pHeaderInfo)
			m_pProfileSheet->m_pXMLProfileInfo->m_pHeaderInfo->SetEnvClassExt(strWindowText);
		
	}
	if (AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD) || AfxIsActivated(TBEXT_APP, MAGICLINK_MOD))
		return IDD_PROFILE_WIZ_MAGICDOCS_PAGE;

	if (m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray() && m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetSize() > 1)
		return IDD_PROFILE_WIZ_DBT_PAGE;
	
	return IDD_PROFILE_WIZ_CHOOSE_PARAM_PAGE;
}

//----------------------------------------------------------------------------
void CProfileExpPropPage::OnDeltaPosMaxDim(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_UPDOWN* pNMUpDown = (NM_UPDOWN*)pNMHDR;
	
	CString strVal;
	
	((CEdit*)GetDlgItem(IDC_PROFMNG_MAXFILE_DIM_EDIT))->GetWindowText(strVal);
	
	int i = _ttoi(((TCHAR*)(LPCTSTR)strVal));

	if (pNMUpDown->iDelta < 0)
		i++;
	else
		if (i > 0)
			i--;
		else
			i = 0;

	pNMUpDown->iPos = i;
	
	strVal.Format(_T("%d"), i);

	if (i <= HEADER_MAX_DOC_DIMENSION && i >= HEADER_MIN_DOC_DIMENSION)
		((CEdit*)GetDlgItem(IDC_PROFMNG_MAXFILE_DIM_EDIT))->SetWindowText(strVal);
	
	*pResult = 0;
}

//----------------------------------------------------------------------------
void CProfileExpPropPage::OnDeltaPosSpinMaxDoc(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_UPDOWN* pNMUpDown = (NM_UPDOWN*)pNMHDR;
	
	CString strVal;
	
	((CEdit*)GetDlgItem(IDC_PROFMNG_MAXDOC_EDIT))->GetWindowText(strVal);
	
	int i = _ttoi(((TCHAR*)(LPCTSTR)strVal));

	if (pNMUpDown->iDelta < 0)
		i++;
	else
		if (i > 0)
			i--;
		else
			i = 0;

	pNMUpDown->iPos = i;
	
	strVal.Format(_T("%d"), i);

	if (i <= HEADER_MAX_DOCUMENT_NUM && i >= HEADER_MIN_DOCUMENT_NUM)
		((CEdit*)GetDlgItem(IDC_PROFMNG_MAXDOC_EDIT))->SetWindowText(strVal);
	
	*pResult = 0;
}

//--------------------------------------------------------------------
// CProfileMagicDocsPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileMagicDocsPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileMagicDocsPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileMagicDocsPage)
		ON_BN_CLICKED (IDC_PROFMNG_POSTABLE_CHECK, OnPostableChanged)		
		ON_BN_CLICKED (IDC_PROFMNG_POSTBACK_CHECK, OnPostBackChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileMagicDocsPage::CProfileMagicDocsPage() 
	: 
	CProfileWizardPage(IDD_PROFILE_WIZ_MAGICDOCS_PAGE),
	m_Image			  (IDB_PROFILE_WIZARD_EXP_PROP)
{
}

//--------------------------------------------------------------------
BOOL CProfileMagicDocsPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}
	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_MAGICDOCS, this);
	BOOL bEnable = !m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly();

	CButton* pBtn = ((CButton*)GetDlgItem(IDC_PROFMNG_POSTABLE_CHECK));
	pBtn->SetCheck(m_pProfileSheet->m_pXMLProfileInfo->IsPostable());
	pBtn->EnableWindow(bEnable);
	pBtn =((CButton*)GetDlgItem(IDC_PROFMNG_POSTBACK_CHECK));
	pBtn->SetCheck(m_pProfileSheet->m_pXMLProfileInfo->IsPostBack());
	pBtn->EnableWindow(bEnable);
	pBtn =((CButton*)GetDlgItem(IDC_PROFMNG_NOEXTREF_CHECK));
	pBtn->SetCheck(m_pProfileSheet->m_pXMLProfileInfo->IsNoExtRefPostBack());
	pBtn->EnableWindow(bEnable && m_pProfileSheet->m_pXMLProfileInfo->IsPostBack());

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileMagicDocsPage::OnActivate()
{	
	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export Profiles - Magic Documents integration properties");

	if (m_pProfileSheet) m_pProfileSheet->SetTitle(strTitle);
}

//--------------------------------------------------------------------
void CProfileMagicDocsPage::OnPostableChanged()
{
	BOOL bEnablePB = ((CButton*)GetDlgItem(IDC_PROFMNG_POSTABLE_CHECK))->GetCheck();
	CButton* pBtn = (CButton*)GetDlgItem(IDC_PROFMNG_POSTBACK_CHECK);
	if (!bEnablePB)
		pBtn->SetCheck(FALSE);

	pBtn->EnableWindow(bEnablePB);	
}

//--------------------------------------------------------------------
void CProfileMagicDocsPage::OnPostBackChanged()
{
	BOOL bCheckPB = ((CButton*)GetDlgItem(IDC_PROFMNG_POSTBACK_CHECK))->GetCheck();
	CButton* pBtn = (CButton*)GetDlgItem(IDC_PROFMNG_NOEXTREF_CHECK);
	if (!bCheckPB)
		pBtn->SetCheck(FALSE);

	pBtn->EnableWindow(bCheckPB);	
}

//--------------------------------------------------------------------
LRESULT CProfileMagicDocsPage::OnWizardNext()
{
	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo && !m_pProfileSheet->m_pXMLProfileInfo->m_pHeaderInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	if (!m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly())
	{
		m_pProfileSheet->m_pXMLProfileInfo->SetPostable(((CButton*)GetDlgItem(IDC_PROFMNG_POSTABLE_CHECK))->GetCheck());
		m_pProfileSheet->m_pXMLProfileInfo->SetPostBack(((CButton*)GetDlgItem(IDC_PROFMNG_POSTBACK_CHECK))->GetCheck());
		m_pProfileSheet->m_pXMLProfileInfo->SetNoExtRefPostBack(((CButton*)GetDlgItem(IDC_PROFMNG_NOEXTREF_CHECK))->GetCheck());
	}

	if (m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray() && m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetSize() > 1)
		return CProfileWizardPage::OnWizardNext();
	else
		return IDD_PROFILE_WIZ_CHOOSE_PARAM_PAGE;
}

//--------------------------------------------------------------------
// CProfileDBTPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileDBTPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileDBTPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileDBTPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileDBTPage::CProfileDBTPage() 
	: 
	CProfileWizardPage(IDD_PROFILE_WIZ_DBT_PAGE),
	m_Image			  (IDB_PROFILE_WIZARD_DBT_PAGE)
{
}

//--------------------------------------------------------------------
BOOL CProfileDBTPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	
	if (!m_pProfileSheet || !m_pProfileSheet->m_pXMLProfileInfo || !m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray())
	{
		ASSERT(FALSE);
		return TRUE;
	}

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_DBT_PAGE, this);

	VERIFY (m_DBTList.SubclassDlgItem(IDC_PROFILE_WIZ_DBT_LIST,	this));

	for(int i = 0 ;  i < m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetSize() ; i++)
	{
		if (!m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i))
		{
			ASSERT(FALSE);
			continue;
		}

		if (m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i)->GetType() != CXMLDBTInfo::MASTER_TYPE)
		{
			int nIdx = m_DBTList.AddString(m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i)->GetTitle());
			m_DBTList.SetItemData(nIdx, (DWORD)m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i));
		}
		else
		{
			CString strText = cwsprintf(_TB("However, you need to export the document header ({0-%s}) first."), m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i)->GetTitle());
			GetDlgItem(IDC_PROF_WIZ_DBT_MASTER)->SetWindowText(strText);
		}
	}
	m_DBTList.EnableWindow(!m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly());

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileDBTPage::OnActivate()
{	
	CXMLDBTInfo* pXMLDBTInfo = NULL;
	for(int i = 0; i < m_DBTList.GetCount() ; i++)
	{
		pXMLDBTInfo = (CXMLDBTInfo*)m_DBTList.GetItemData(i);
		if (!pXMLDBTInfo)
		{
			ASSERT(FALSE);
			continue;
		}
		
		if (pXMLDBTInfo->IsToExport())
			m_DBTList.SetCheck(i, 1);
		else
			m_DBTList.SetCheck(i, 0);
	}

	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export profiles - Select bodies to export");

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(strTitle);
}

//--------------------------------------------------------------------
LRESULT CProfileDBTPage::OnWizardNext()
{
	if (m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly())
		return CProfileWizardPage::OnWizardNext();

	CXMLDBTInfo* pXMLDBTInfo = NULL;
	for(int i = 0; i < m_DBTList.GetCount() ; i++)
	{
		pXMLDBTInfo = (CXMLDBTInfo*)m_DBTList.GetItemData(i);
		if (!pXMLDBTInfo)
		{
			ASSERT(FALSE);
			continue;
		}

		if (m_DBTList.GetCheck(i) == 0)
			pXMLDBTInfo->m_bExport = FALSE;
		else
			pXMLDBTInfo->m_bExport = TRUE;
	}

	return CProfileWizardPage::OnWizardNext();
}

//--------------------------------------------------------------------
LRESULT CProfileDBTPage::OnWizardBack()
{
	if (AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD))
		return IDD_PROFILE_WIZ_MAGICDOCS_PAGE;

	return IDD_PROFILE_WIZ_EXP_PROP_PAGE;
}

//--------------------------------------------------------------------
// CProfileChooseParamPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileChooseParamPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileChooseParamPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileChooseParamPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileChooseParamPage::CProfileChooseParamPage() 
	: 
	CProfileWizardPage(IDD_PROFILE_WIZ_CHOOSE_PARAM_PAGE),
	m_Image			  (IDB_PROFILE_WIZARD_CHOOSE_PAGE)
{
}

//--------------------------------------------------------------------
BOOL CProfileChooseParamPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_CHOOSE_PAGE, this);

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileChooseParamPage::OnActivate()
{	
	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export profiles - Select parameteres to set");

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(strTitle);
	
	if (!m_pProfileSheet->m_pXMLProfileInfo)
		return;

	BOOL bEnable = !m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly();
	CButton* pBtn = (CButton*)GetDlgItem(IDC_PROF_WIZ_CONF_USRCRIT_CHK);
	pBtn->SetCheck(m_pProfileSheet->m_bConfigUsrCriteria);
	pBtn->EnableWindow(bEnable);
	pBtn = (CButton*)GetDlgItem(IDC_PROF_WIZ_CONF_FIELD_CHK);
	pBtn->SetCheck(m_pProfileSheet->m_bConfigField);
	pBtn->EnableWindow(bEnable);
	pBtn = (CButton*)GetDlgItem(IDC_PROF_WIZ_CONF_XREF_CHK);
	pBtn->SetCheck(m_pProfileSheet->m_bConfigXRef);
	pBtn->EnableWindow(bEnable);
}

//--------------------------------------------------------------------
LRESULT CProfileChooseParamPage::OnWizardNext()
{
	if (!m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly())
	{
		m_pProfileSheet->m_bConfigUsrCriteria = ((CButton*)GetDlgItem(IDC_PROF_WIZ_CONF_USRCRIT_CHK))->GetCheck();
		m_pProfileSheet->m_bConfigField = ((CButton*)GetDlgItem(IDC_PROF_WIZ_CONF_FIELD_CHK))->GetCheck();
		m_pProfileSheet->m_bConfigXRef = ((CButton*)GetDlgItem(IDC_PROF_WIZ_CONF_XREF_CHK))->GetCheck();
	}

	if (m_pProfileSheet->m_bConfigField)
		return IDD_PROFILE_WIZ_FIELD_PAGE;

	if (m_pProfileSheet->m_bConfigXRef)
		return IDD_PROFILE_WIZ_XREF_PAGE;

	if (m_pProfileSheet->m_bConfigUsrCriteria)
		return IDD_PROFILE_WIZ_CRITERIA_PAGE;

	return IDD_PROFILE_WIZ_FINISH_PAGE;
}

//--------------------------------------------------------------------
LRESULT CProfileChooseParamPage::OnWizardBack()
{
	if (m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray() && m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetSize() > 1)
		return CProfileWizardPage::OnWizardBack();
	else
		return IDD_PROFILE_WIZ_EXP_PROP_PAGE;
}

//--------------------------------------------------------------------
// CProfileFieldPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileFieldPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileFieldPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileFieldPage)
	ON_LBN_SELCHANGE	(IDC_PROF_WIZ_DBT_LIST, OnSelchangeListDBT)
	ON_MESSAGE			(UM_LIST_CHECK_CHANGED, OnFieldCheckChanged)
	ON_BN_CLICKED		(IDC_PROF_WIZ_SELECTALL_BTN, OnSelectAllChanged)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileFieldPage::CProfileFieldPage() 
	: 
	CProfileWizardPage	(IDD_PROFILE_WIZ_FIELD_PAGE),
	m_pCurrentDbtInfo	(NULL),
	m_Image				(IDB_PROFILE_WIZARD_FIELD_PAGE),
	m_bSelectAll		(TRUE)
{
}

//--------------------------------------------------------------------
BOOL CProfileFieldPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_FIELD_PAGE, this);

	VERIFY (m_DBTList.SubclassDlgItem(IDC_PROF_WIZ_DBT_LIST, this));
	VERIFY (m_FieldList.SubclassDlgItem(IDC_PROF_WIZ_FIELD_LIST, this));

	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	CXMLDBTInfoArray* pXMLDBTInfoArray = m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray();
	if (!pXMLDBTInfoArray)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	//se non esiste l'array di field lo creo per ogni dbt
	for(int i = 0 ;  i < pXMLDBTInfoArray->GetSize() ; i++)
	{
		CXMLDBTInfo* pXMLDBTInfo = pXMLDBTInfoArray->GetAt(i);
		if (!pXMLDBTInfo)
		{
			ASSERT(FALSE);
			continue;
		}

		if (i == 0)
			m_pCurrentDbtInfo = pXMLDBTInfo;

		if (!pXMLDBTInfo->m_pXMLFieldInfoArray)
			pXMLDBTInfo->m_pXMLFieldInfoArray = new CXMLFieldInfoArray;
	}
	BOOL bEnable = !m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly();
	m_FieldList.EnableWindow(bEnable);
	GetDlgItem(IDC_PROF_WIZ_SELECTALL_BTN)->EnableWindow(bEnable);
	GetDlgItem(IDC_PROF_WIZ_DIS_EXTREF_CHECK)->EnableWindow(bEnable);

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileFieldPage::OnActivate()
{	
	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo && !m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray())
	{
		ASSERT(FALSE);
		return;
	}

	m_DBTList.ResetContent();
	
	//trucco per bypassare il problema dello scrool orizzontale che non è possibile
	CDC*   pDC		= m_DBTList.GetDC();
	CFont* pFont	= m_DBTList.GetFont();
	CFont* pOldFont	= pDC->SelectObject(pFont);
	
	TEXTMETRIC  tm;
	pDC->GetTextMetrics(&tm); 
	int	dx = 0;
	CSize sz(0);

	CString strDBTTitle;

	for(int i = 0 ;  i < m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetSize() ; i++)
	{
		if (m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i)->IsToExport())
		{
			strDBTTitle = m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i)->GetTitle();
			sz = pDC->GetTextExtent(strDBTTitle);
			sz.cx += tm.tmAveCharWidth;
			if (sz.cx > dx)
				dx = sz.cx;
			int nIns = m_DBTList.AddString(strDBTTitle);		
			m_DBTList.SetItemData(nIns, (DWORD)m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray()->GetAt(i));
		}
	}
	// indica la size del testo più lungo alle scrollbar
	m_DBTList.ReleaseDC			(pDC);
	m_DBTList.SetHorizontalExtent(dx);
	m_DBTList.UpdateWindow		();


	if (m_DBTList.GetCount() > 0)
	{
		m_DBTList.SetCurSel(0);
		OnSelchangeListDBT();
	}

	CString strTitle = _TB("Export profiles - Select fields to export");

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(strTitle);

	CProfileWizardPage::OnActivate();
}

//--------------------------------------------------------------------
void CProfileFieldPage::SetButtonTitle()
{
	((CButton*)GetDlgItem(IDC_PROF_WIZ_SELECTALL_BTN))->SetWindowText((m_bSelectAll) ? _TB("&Unmark all") : _TB("&Mark all"));
}

//--------------------------------------------------------------------
void CProfileFieldPage::OnSelchangeListDBT() 
{
	for(int n = 0; n < m_DBTList.GetCount() ; n++)
	{
		if (m_DBTList.GetSel(n))
		{
			//nuovo dbt selezionato
			CXMLDBTInfo* pDbtInfo = (CXMLDBTInfo*)m_DBTList.GetItemData(n);
			if (!pDbtInfo)
			{
				ASSERT(FALSE);
				return;
			}

			//salvo le modifiche sul precedente
			if (m_pCurrentDbtInfo && m_pCurrentDbtInfo->m_pXMLFieldInfoArray)
			{
				CString strFieldName;
				for(int i = 0; i < m_FieldList.GetCount() ; i++)
				{
					m_FieldList.GetText(i, strFieldName);
					int nCheck = m_FieldList.GetCheck(i);

					CXMLFieldInfo* pXMLFieldInfo = m_pCurrentDbtInfo->m_pXMLFieldInfoArray->GetFieldByName(strFieldName);

					if (!pXMLFieldInfo)
					{
						ASSERT(FALSE);
						continue;
					}

					pXMLFieldInfo->SetExport(nCheck == 1);						
				}
			}
	
			FillFieldListBox(pDbtInfo);
			m_pCurrentDbtInfo = pDbtInfo;
		}
	}
}

//--------------------------------------------------------------------
void CProfileFieldPage::FillFieldListBox(CXMLDBTInfo* pDbtInfo)
{
	m_FieldList.ResetContent();

	if (!pDbtInfo)
	{
		ASSERT(FALSE);
		return;
	}

	if (!pDbtInfo->GetXMLFieldInfoArray())
		pDbtInfo->m_pXMLFieldInfoArray = new CXMLFieldInfoArray();

	SqlTableInfo* pReferencedTblInfo = AfxGetDefaultSqlConnection()->GetTableInfo(pDbtInfo->m_strTableName);
	if (!pReferencedTblInfo)
	{
		ASSERT(FALSE);
		return;
	}

	SqlColumnInfo* pColumn = NULL;
	
	//trucco per bypassare il problema dello scrool orizzontale che non è possibile
	CDC*   pDC		= m_FieldList.GetDC();
	CFont* pFont	= m_FieldList.GetFont();
	CFont* pOldFont	= pDC->SelectObject(pFont);
	
	TEXTMETRIC  tm;
	pDC->GetTextMetrics(&tm); 
	int	dx = 0;
	CSize sz(0);

	const  Array* pColums = pReferencedTblInfo->GetPhysicalColumns();
	for (int i = 0 ; i < pColums->GetSize() ; i++)
	{
		pColumn = (SqlColumnInfo*) pColums->GetAt(i);
		if (!pColumn)
		{
			ASSERT(FALSE);
			m_FieldList.ReleaseDC			(pDC);
			return;
		}

		if (pColumn->m_bVirtual)
			continue;

		CString strFieldName = pColumn->GetColumnName();

		sz = pDC->GetTextExtent(strFieldName);
		sz.cx += tm.tmAveCharWidth;
		if (sz.cx > dx)
			dx = sz.cx;

		int nIns = m_FieldList.AddString(strFieldName);
		m_FieldList.SetCheck(nIns, 1);

		if (pColumn->m_bSpecial || pDbtInfo->IsUniversalKeySegment(strFieldName))
			m_FieldList.Enable(nIns, FALSE);

		CXMLFieldInfo* pXMLFieldInfo = NULL;
		if (pDbtInfo->m_pXMLFieldInfoArray)
		{
			pXMLFieldInfo = pDbtInfo->m_pXMLFieldInfoArray->GetFieldByName(strFieldName);
			if (!pXMLFieldInfo)
			{
				pXMLFieldInfo = new CXMLFieldInfo();
				pXMLFieldInfo->SetFieldName(strFieldName);
				pXMLFieldInfo->SetExport(pColumn->m_bSpecial || pDbtInfo->IsUniversalKeySegment(strFieldName));

				pDbtInfo->m_pXMLFieldInfoArray->Add(pXMLFieldInfo);
			}

			if (!pXMLFieldInfo->IsToExport())
				m_FieldList.SetCheck(nIns, 0);
		}
	}
	// ripristina il vecchio font
	pDC->SelectObject(pOldFont);

	// indica la size del testo più lungo alle scrollbar
	m_FieldList.ReleaseDC			(pDC);
	m_FieldList.SetHorizontalExtent(dx);
	m_FieldList.UpdateWindow		();

	SetSelectAllButton();
}


//--------------------------------------------------------------------------
void CProfileFieldPage::SetSelectAllButton()
{
	//gestione pulsante di marca\smarca tutto
	//se ho tutti i campi abilitati notchecked allora il pulsante diventa &Marca tutto  altrimenti &Smarca tutto
	int nChecked = 0;
	int nEnable = 0;
	for (int nField = 0; nField < m_FieldList.GetCount(); nField++)
	{
		// se è un campo chiave o di UK non faccio niente
		if (!m_FieldList.IsEnabled(nField))
			continue;
		nEnable++;
		if(m_FieldList.GetCheck(nField))
			nChecked++;
	}
	m_bSelectAll = (nChecked == nEnable);
	SetButtonTitle();
}

//--------------------------------------------------------------------------
LRESULT CProfileFieldPage::OnFieldCheckChanged(WPARAM nIdc, LPARAM nMode)
{
	if (!m_pProfileSheet || !m_pProfileSheet->m_pXMLProfileInfo || !m_pProfileSheet->m_pXMLProfileInfo->GetDBTInfoArray())
	{
		ASSERT(FALSE);
		return (LRESULT) 0L;
	}

	CCheckItemData* pCheckItemData = (CCheckItemData*)nMode;
	if (!pCheckItemData)
		return (LRESULT) 0L;

	CString strFieldName;
	m_FieldList.GetText(pCheckItemData->m_nItem, strFieldName);
	if (strFieldName.IsEmpty())
		return (LRESULT) 0L;

	CXMLXRefInfo* pXMLXRefInfo = NULL;
	CXMLXRefInfoArray aXMLXRefInfoArray;
	aXMLXRefInfoArray.SetOwns(FALSE);

	//array di tutti gli xref che hanno il segmento
	if (m_pCurrentDbtInfo->GetXMLXRefInfoArray())
		m_pCurrentDbtInfo->GetXMLXRefInfoArray()->GetXRefArrayByFK(strFieldName, &aXMLXRefInfoArray, FALSE);
	if (aXMLXRefInfoArray.GetSize() == 0)
		return (LRESULT) 0L;

	BOOL bExport = pCheckItemData->m_nNextCheckVal;
	
	if (!bExport && !((CButton*)GetDlgItem(IDC_PROF_WIZ_DIS_EXTREF_CHECK))->GetCheck())
		bExport = (AfxMessageBox(_TB("The selected field is used as segment for one or more external references.\r\nMaking the field unexportable, the related external references will be unused as well.\r\nDo you want to continue?"), MB_YESNO) == IDNO) || bExport;
	//array di tutti gli xref che hanno il segmento
	for (int i = aXMLXRefInfoArray.GetSize() - 1 ; i >= 0 ; i--)
	{
		pXMLXRefInfo = aXMLXRefInfoArray.GetAt(i);		
		if (!pXMLXRefInfo || !pXMLXRefInfo->m_SegmentsArray.GetSize())
			continue;
		BOOL bOldIsToUse = pXMLXRefInfo->IsToUse();
		pXMLXRefInfo->SetUse((bExport) ? pXMLXRefInfo->m_bOldUse : FALSE);
		pXMLXRefInfo->m_bOldUse = bOldIsToUse;		
	}
	m_FieldList.SetCheck(pCheckItemData->m_nItem, bExport);

	return (LRESULT) 0L;
}

//--------------------------------------------------------------------
void CProfileFieldPage::OnSelectAllChanged()
{
	CXMLXRefInfoArray aXMLXRefInfoArray;
	aXMLXRefInfoArray.SetOwns(FALSE);
	CString strFieldName;

	// l'azione da svolgere è il contrario di quella precedente
	m_bSelectAll = !m_bSelectAll;
	for (int nField = 0; nField < m_FieldList.GetCount(); nField++)
	{
		//CCheckItemData* pCheckItemData = (CCheckItemData*)m_FieldList.GetItemData(nField);

		// se è un campo chiave o di UK non faccio niente
		if (!m_FieldList.IsEnabled(nField))
			continue;
		
		if (!m_bSelectAll)
		{
			m_FieldList.GetText(nField, strFieldName);
			if (strFieldName.IsEmpty())
				continue;

			aXMLXRefInfoArray.RemoveAll ();

			//array di tutti gli xref che hanno il segmento
			if (m_pCurrentDbtInfo->GetXMLXRefInfoArray())
				m_pCurrentDbtInfo->GetXMLXRefInfoArray()->GetXRefArrayByFK(strFieldName, &aXMLXRefInfoArray, FALSE);

			// se c'è un segmento di xref non lo deseleziono
			BOOL bOneInUse = FALSE;
			CXMLXRefInfo* pXMLXRefInfo = NULL;
			if (aXMLXRefInfoArray.GetSize() != 0)
			{
				for (int i = aXMLXRefInfoArray.GetSize() - 1 ; i >= 0 ; i--)
				{
					pXMLXRefInfo = aXMLXRefInfoArray.GetAt(i);		
					if (!pXMLXRefInfo || !pXMLXRefInfo->m_SegmentsArray.GetSize())
						continue;
					if (pXMLXRefInfo->IsToUse())
					{
						bOneInUse = TRUE;
						break;
					}
				}
				
				if (bOneInUse)
					continue;
			}
		}

		//problema degli universalkey
		m_FieldList.SetCheck(nField, m_bSelectAll);	
	}
	SetButtonTitle();
}

//--------------------------------------------------------------------
LRESULT CProfileFieldPage::OnWizardNext()
{
	OnSelchangeListDBT();

	for(int n = 0; n < m_DBTList.GetCount() ; n++)
	{
		BOOL bOneFieldToExport = FALSE;

		//nuovo dbt selezionato
		CXMLDBTInfo* pDbtInfo = (CXMLDBTInfo*)m_DBTList.GetItemData(n);
		if (!pDbtInfo)
		{
			ASSERT(FALSE);
			return IDD_PROFILE_WIZ_FIELD_PAGE;
		}

		//salvo le modifiche sul precedente
		if (pDbtInfo && pDbtInfo->m_pXMLFieldInfoArray)
		{
			bOneFieldToExport = FALSE;
			CString strField;
			for(int i = 0; i < pDbtInfo->m_pXMLFieldInfoArray->GetSize() ; i++)
			{
				if (!pDbtInfo->m_pXMLFieldInfoArray->GetAt(i))
				{
					ASSERT(FALSE);
					continue;
				}

				if (pDbtInfo->m_pXMLFieldInfoArray->GetAt(i)->IsToExport())
					bOneFieldToExport = TRUE;
			}
		}

		if (!bOneFieldToExport && !m_pProfileSheet->m_bConfigField)
		{
			delete pDbtInfo->m_pXMLFieldInfoArray;
			pDbtInfo->m_pXMLFieldInfoArray = NULL;
		}
	}

	if (m_pProfileSheet->m_bConfigXRef)
		return IDD_PROFILE_WIZ_XREF_PAGE;

	if (m_pProfileSheet->m_bConfigUsrCriteria)
		return IDD_PROFILE_WIZ_CRITERIA_PAGE;

	return IDD_PROFILE_WIZ_FINISH_PAGE;
}

//--------------------------------------------------------------------
// CProfileSelectionPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileSelectionPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileSelectionPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileSelectionPage)
		ON_BN_CLICKED (IDC_USER_CRITERIA_MNG_BTN_PARAMETERS,ShowAskRules)
		ON_WM_CTLCOLOR()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileSelectionPage::CProfileSelectionPage() 
	: 
	CProfileWizardPage  (IDD_PROFILE_WIZ_CRITERIA_PAGE),
	m_pXMLExpCriteria	(NULL),
	m_pUserCriteria		(NULL),	
	m_pPrgData			(NULL),
	m_pQueryInfo		(NULL),
	m_bPrgDataOwns		(FALSE),
	m_bNewQuery			(FALSE),
	m_pTableInfo		(NULL),
	m_pEdtExpWClause	(NULL),
	m_pEdtOrderBy		(NULL),
	m_Image				(IDB_PROFILE_WIZARD_EXP_PROP)
{
}

//--------------------------------------------------------------------
CProfileSelectionPage::~CProfileSelectionPage() 
{	
	m_brSolidWhite.DeleteObject();

	if (m_bPrgDataOwns && m_pPrgData) 
		delete m_pPrgData;

	SAFE_DELETE(m_pEdtExpWClause);
	SAFE_DELETE(m_pEdtOrderBy);
	SAFE_DELETE(m_pUserCriteria);
}

//--------------------------------------------------------------------
BOOL CProfileSelectionPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_EXP_PROP, this);

	m_pEdtExpWClause= new CExpEdit;
	m_pEdtOrderBy	= new CEqnEdit (NULL, NULL);

	if (!m_pXMLExpCriteria && m_pProfileSheet && m_pProfileSheet->m_pXMLProfileInfo)
		m_pProfileSheet->m_pXMLProfileInfo->m_pExportCriteria = m_pXMLExpCriteria = new CXMLExportCriteria(m_pProfileSheet->m_pXMLProfileInfo);

	if (m_pXMLExpCriteria)
	{
		if (m_pXMLExpCriteria->GetUserExportCriteria())
			m_pUserCriteria = new CUserExportCriteria(*m_pXMLExpCriteria->GetUserExportCriteria());
		else
			m_pUserCriteria = new CUserExportCriteria(m_pXMLExpCriteria);
	}

	m_pQueryInfo	= m_pUserCriteria ? m_pUserCriteria->m_pQueryInfo : NULL;
	m_pPrgData		= m_pQueryInfo ? m_pQueryInfo->m_pPrgData : NULL;
	m_pTableInfo	= m_pUserCriteria ? m_pUserCriteria->GetTableInfo() : NULL;

	// subclass dei controlli di libreria MicroArea
	VERIFY (m_pEdtExpWClause->	SubclassEdit (IDC_USER_CRITERIA_MNG_EDT_WCLAUSE,	this));
	VERIFY (m_pEdtOrderBy->		SubclassEdit (IDC_USER_CRITERIA_MNG_EDT_ORDERBY,	this));

	m_pEdtExpWClause->	SetTableInfo (m_pTableInfo);
	m_pEdtOrderBy->		SetTableInfo (m_pTableInfo);

	InitFields();
	m_pEdtExpWClause->SetFocus ();
	
	m_brSolidWhite.CreateSolidBrush(RGB(255, 255, 255));

	BOOL bEnable = !m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly();
	m_pEdtExpWClause->EnableWindow(bEnable);
	m_pEdtOrderBy->EnableWindow(bEnable);	
	GetDlgItem(IDC_USER_CRITERIA_MNG_BTN_PARAMETERS)->EnableWindow(bEnable);
	GetDlgItem(IDC_USER_CRITERIA_MNG_CKB_SET_NATIVE_SQL)->EnableWindow(bEnable);
	if (AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD))
	{
		CButton* pWnd = (CButton*)GetDlgItem(IDC_USER_CRITERIA_MNG_CKB_OVERRIDE_QUERY);
		pWnd->ShowWindow(SW_SHOW);
		pWnd->EnableWindow(bEnable);
		pWnd->SetCheck(m_pUserCriteria->m_bOverrideDefaultQuery);
	}

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileSelectionPage::OnActivate()
{
	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export profiles - Define selection criteria");

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(strTitle);
}

//--------------------------------------------------------------------
LRESULT CProfileSelectionPage::OnWizardNext()
{
	if (m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly())
		return IDD_PROFILE_WIZ_FINISH_PAGE;

	// Controllo la correttezza sintattica della query
	CString strWClause;
	if (!TestQuery (&strWClause))
		return IDD_PROFILE_WIZ_CRITERIA_PAGE;
    
    CString strOrderByCov;
	CovertStrOrderByInLF (strOrderByCov);
	BOOL bOverride = AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD) && ((CButton*)GetDlgItem(IDC_USER_CRITERIA_MNG_CKB_OVERRIDE_QUERY))->GetCheck();
	
	if (strWClause.IsEmpty() && strOrderByCov.IsEmpty() && !m_pPrgData && !bOverride)
	{
		m_pXMLExpCriteria->SetUserCriteria(NULL);
		return IDD_PROFILE_WIZ_FINISH_PAGE;
	}
	
	if (m_pQueryInfo)
	{
		m_pQueryInfo->m_bNativeExpr	= IsNativeExpr();
		m_pQueryInfo->m_TableInfo.m_strFilter	= strWClause;
		m_pQueryInfo->m_TableInfo.m_strSort	= strOrderByCov;
		m_pQueryInfo->SetProgramData(m_pPrgData);
	}

	m_pUserCriteria->m_bOverrideDefaultQuery = bOverride;
	m_pXMLExpCriteria->SetUserCriteria(m_pUserCriteria);

	return IDD_PROFILE_WIZ_FINISH_PAGE;
}

//--------------------------------------------------------------------
LRESULT CProfileSelectionPage::OnWizardBack()
{
	if (m_pProfileSheet->m_bConfigXRef)
		return IDD_PROFILE_WIZ_XREF_PAGE;

	if (m_pProfileSheet->m_bConfigField)
		return IDD_PROFILE_WIZ_FIELD_PAGE;

	return IDD_PROFILE_WIZ_CHOOSE_PARAM_PAGE;
}

//---------------------------------------------------------------------------
HBRUSH CProfileSelectionPage::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor) 
{
	switch (nCtlColor)
	{	   
		case CTLCOLOR_STATIC:
		case CTLCOLOR_EDIT:
		case CTLCOLOR_LISTBOX:
		case CTLCOLOR_SCROLLBAR:
		case CTLCOLOR_BTN:
			pDC->SetTextColor(RGB(0, 0, 0));
			pDC->SetBkColor(RGB(255, 255, 255));
		case CTLCOLOR_DLG:	    
			return m_brSolidWhite;
	}	
	return m_brSolidWhite;
}

//-----------------------------------------------------------------------------
void CProfileSelectionPage::InitFields()
{

	GetChkBoxNativeExpr()->SetCheck(m_pQueryInfo ? m_pQueryInfo->m_bNativeExpr : 0);

	if (m_pPrgData)
		m_pEdtExpWClause->SetSymbolTable (m_pPrgData->GetSymTable());

	// Conversione delle stringhe per effettuare la corretta visualizzazione
	// a video
	CString strTmp (m_pQueryInfo ? m_pQueryInfo->m_TableInfo.m_strFilter : _T(""));
	ConvertCString(strTmp, LF_TO_CRLF);
	m_pEdtExpWClause-> SetValue (strTmp);

	// Conversione delle stringhe per effettuare la corretta visualizzazione
	// a video
	strTmp = m_pQueryInfo ? m_pQueryInfo->m_TableInfo.m_strSort : _T("");
	ConvertCString(strTmp, LF_TO_CRLF);
	m_pEdtOrderBy-> SetValue (strTmp);
}


// Ritorna TRUE se il controllo sintattico della query ha data esito positivo,
// altrimenti FALSE
//-----------------------------------------------------------------------------
BOOL CProfileSelectionPage::TestQuery (CString* pStrWClause /* NULL */)
{
	if (!m_pUserCriteria->m_pRecord) 
		return FALSE;

	ASSERT (m_pEdtOrderBy);
	ASSERT (m_pEdtExpWClause);

	SqlTable* pTable = new SqlTable(m_pUserCriteria->m_pRecord, AfxGetDefaultSqlSession());
	SqlTableInfoArray aTableInfoArray(m_pTableInfo);

	// Controllo dl Where Clause
	if (!m_pEdtExpWClause->IsEmpty())
	{
	    WClause aWC (AfxGetDefaultSqlConnection(), m_pPrgData ? m_pPrgData->GetSymTable() : NULL, aTableInfoArray);
		aWC.SetNative(IsNativeExpr());
		
		if (!m_pEdtExpWClause->CheckWC(aWC))
		{
			m_pEdtExpWClause->SetCtrlFocus(TRUE);
			return FALSE;
		}

		GetChkBoxNativeExpr()->SetCheck(aWC.IsNative());

		// Se il puntatore è diverso da NULL allora memorizzo la stringa di Filter
		if (pStrWClause)
			*pStrWClause = aWC.ToString(pTable);
	}

	// Se la stringa di order by è vuota, estraggo secondo l'ordine della tabella
	if (!m_pEdtOrderBy->IsEmpty())
	{
		CString	strTmp (m_pEdtOrderBy->GetValue());
		ConvertCString (strTmp, CRLF_TO_LF);
		Parser lex (strTmp);

		// controllo sintattico dell'espressione
		if (!ParseOrderBy(lex, m_pPrgData ? m_pPrgData->GetSymTable() : NULL, &aTableInfoArray, strTmp))
		{
			m_pEdtOrderBy->SetCtrlFocus(TRUE);
			return FALSE;
		}
	}

	delete pTable;

	return TRUE;
}


// Ritorna il Check Box per la gestione della espressione nativa SQL
//-----------------------------------------------------------------------------
CButton* CProfileSelectionPage::GetChkBoxNativeExpr()
{
	return (CButton*)GetDlgItem(IDC_USER_CRITERIA_MNG_CKB_SET_NATIVE_SQL);
}

//-----------------------------------------------------------------------------
void CProfileSelectionPage::CovertStrOrderByInLF (CString& strNewOrderBy)
{
	strNewOrderBy = m_pEdtOrderBy->GetValue();
	ConvertCString (strNewOrderBy, CRLF_TO_LF);
}

// Visualizzazione delle dialog per la costruzione della query parametrica
//-----------------------------------------------------------------------------
void CProfileSelectionPage::ShowAskRules()
{
	//Se non è presente alloco il Program Data
	if (!m_pPrgData)
	{
		m_bPrgDataOwns	= TRUE;
		m_pPrgData		=  new ProgramData(NULL);
	}

	// Visualizzazione della finestra di dialogo per la definizione dei
	// parametri
	CAskRuleDlg	askRuleDlg (m_pPrgData, NULL);
	if (askRuleDlg.DoModal() != IDOK && m_bPrgDataOwns)
	{
		delete m_pPrgData;
		m_pPrgData = NULL;
		return;
	}
	// Occorre settare il flag per evitare di cancellare la program data
	// appena inserito
	m_bPrgDataOwns = FALSE;
	ASSERT (m_pEdtExpWClause);
	ASSERT (m_pPrgData);

	// Associo all'edit della Where Clause la tabella dei simboli appena
	// definita (gestisce i parametri)
	m_pEdtExpWClause->SetSymbolTable(m_pPrgData->GetSymTable());
	m_pEdtExpWClause->SetFocus		();
}


//--------------------------------------------------------------------
// CProfileXRefPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileXRefPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileXRefPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileXRefPage)
	ON_WM_CONTEXTMENU()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileXRefPage::CProfileXRefPage() 
	: 
	CProfileWizardPage	(IDD_PROFILE_WIZ_XREF_PAGE),
	m_Image				(IDB_PROFILE_WIZARD_TREE_PAGE)
{
}

//--------------------------------------------------------------------
BOOL CProfileXRefPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_TREE_PAGE, this);

	VERIFY (m_TreeCtrl.SubclassDlgItem(IDC_PROF_WIZ_XREF_TREE, this));
	m_TreeCtrl.InitializeImageList();
	if (!m_pProfileSheet && !m_pProfileSheet->m_pXMLProfileInfo)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	m_TreeCtrl.SetBkColor(RGB(255,255,255));
	m_TreeCtrl.SetInsertMarkColor(RGB(255,255,255));
	m_TreeCtrl.SetReadOnly(m_pProfileSheet->m_pXMLProfileInfo->IsReadOnly());

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileXRefPage::OnActivate()
{	
	m_TreeCtrl.FillTree(m_pProfileSheet->m_pXMLProfileInfo);

	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export profiles - Define external references");

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(strTitle);
}

//--------------------------------------------------------------------
LRESULT CProfileXRefPage::OnWizardNext()
{
	if (m_pProfileSheet->m_bConfigUsrCriteria)
		return IDD_PROFILE_WIZ_CRITERIA_PAGE;

	return IDD_PROFILE_WIZ_FINISH_PAGE;
}

//--------------------------------------------------------------------
LRESULT CProfileXRefPage::OnWizardBack()
{
	if (m_pProfileSheet->m_bConfigField)
		return IDD_PROFILE_WIZ_FIELD_PAGE;

	return IDD_PROFILE_WIZ_CHOOSE_PARAM_PAGE;
}

//----------------------------------------------------------------------------
void CProfileXRefPage::OnContextMenu(CWnd* pWnd, CPoint point) 
{
	m_TreeCtrl.OnContextMenu(pWnd, point);
}

//--------------------------------------------------------------------
// CProfileFinishPage property page implementation
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CProfileFinishPage, CProfileWizardPage)

BEGIN_MESSAGE_MAP(CProfileFinishPage, CProfileWizardPage)
	//{{AFX_MSG_MAP(CProfileFinishPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CProfileFinishPage::CProfileFinishPage() 
	: 
	CProfileWizardPage(IDD_PROFILE_WIZ_FINISH_PAGE),
	m_Image			  (IDB_PROFILE_WIZARD_FINISH),
	m_pXMLProfileInfo (NULL)
{
	for (int nIdx = 0; nIdx <= AfxGetLoginInfos()->m_CompanyUsers.GetUpperBound(); nIdx++)
        m_arUsers.Add(AfxGetLoginInfos()->m_CompanyUsers.GetAt(nIdx));
}
//--------------------------------------------------------------------
BOOL CProfileFinishPage::OnInitDialog()
{
	CProfileWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_PROFILE_WIZARD_FINISH, this);

	return TRUE;
}

//--------------------------------------------------------------------
void CProfileFinishPage::OnActivate()
{	
	CProfileWizardPage::OnActivate();

	CString strTitle = _TB("Export profiles - Summary");

	if (m_pProfileSheet)
		m_pProfileSheet->SetTitle(strTitle);
	
	m_pXMLProfileInfo = m_pProfileSheet->m_pXMLProfileInfo;

	CWnd* pWnd = GetDlgItem(IDC_PROFMNG_PROFILE_NAME_EDIT);
	pWnd->SetWindowText(m_pXMLProfileInfo->GetName());
	pWnd->EnableWindow(m_pXMLProfileInfo->m_bNewProfile);
	pWnd = GetDlgItem(IDC_PROFMNG_DATAURL_EDIT);
	pWnd->SetWindowText(m_pXMLProfileInfo->GetUrlData());
	pWnd->EnableWindow(!m_pXMLProfileInfo->IsReadOnly());
}

//--------------------------------------------------------------------
LRESULT CProfileFinishPage::OnWizardBack()
{
	if (m_pProfileSheet->m_bConfigUsrCriteria)
		return IDD_PROFILE_WIZ_CRITERIA_PAGE;

	if (m_pProfileSheet->m_bConfigXRef)
		return IDD_PROFILE_WIZ_XREF_PAGE;

	if (m_pProfileSheet->m_bConfigField)
		return IDD_PROFILE_WIZ_FIELD_PAGE;

	return IDD_PROFILE_WIZ_CHOOSE_PARAM_PAGE;
}

//---------------------------------------------------------------------------
BOOL CProfileFinishPage::ProfileAlreadyExists(const CString& strProfileName, CPathFinder::PosType ePosType, const CString& userName) 
{
	if (!m_pXMLProfileInfo || strProfileName.IsEmpty())
		return FALSE;
	
	switch (ePosType)
	{
		case CPathFinder::USERS:
			return ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::USERS, userName, TRUE));
		case CPathFinder::ALL_USERS:
		{	
			//prima controllo in ALL_USERS
			if (!::ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::ALL_USERS)))
			{
				//poi controllo nella STANDARD
				if (!::ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::STANDARD)))
				{
					//poi per tutti gli users
					for (int nIdx = 0; nIdx <= m_arUsers.GetUpperBound(); nIdx++)
					{
						if (::ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::USERS, m_arUsers.GetAt(nIdx)))) 
							return TRUE;
					}
					return FALSE;
				}
			}			
			return TRUE;
		}

		case CPathFinder::STANDARD:
		{
			//prima controllo in STANDARD
			if (!::ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::STANDARD)))
			{
				//poi controllo in ALL_USERS
				if (!::ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::ALL_USERS)))
				{
					//poi per tutti gli users
					for (int nIdx= 0; nIdx <= m_arUsers.GetUpperBound(); nIdx++)
					{
						if (::ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strProfileName, CPathFinder::USERS, m_arUsers.GetAt(nIdx)))) 
							return TRUE;
					}	
					return FALSE;
				}
			}
			return TRUE;
		}
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL CProfileFinishPage::OnWizardFinish() 
{
	if (m_pXMLProfileInfo->IsReadOnly())
		return TRUE;

	CString strNewName;
	CString strNewPath = m_pXMLProfileInfo->m_strDocProfilePath;

	GetDlgItem(IDC_PROFMNG_PROFILE_NAME_EDIT)->GetWindowText(strNewName);	
	
	CString strUserName;
	CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strNewPath);
	if (ePosType == CPathFinder::USERS)
		strUserName = AfxGetPathFinder()->GetUserNameFromPath(strNewPath);

	//Se ho inserito un nuovo profilo controllo che il nome non sia già presente
	if (m_pXMLProfileInfo->m_bNewProfile)
	{	
	
		int nCount = 0;
		CString strOldName = strNewName;
		//calcolo il nuovo nome partendo da Nuovo e controllando gli eventuali profili presenti nella directory
		while(ExistPath(::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strNewName, ePosType, strUserName)))
			strNewName.Format(_T("%s_%d"), strOldName, ++nCount);	

		if (nCount > 0)
			AfxMessageBox(cwsprintf(_TB("The profile {0-%s} already exists. It will be renamed in {1-%s}."), strOldName, strNewName));

		strNewPath = ::GetProfilePath(m_pXMLProfileInfo->GetNamespaceDoc(), strNewName, ePosType, strUserName);
	}

	CString strUrlName;
	GetDlgItem(IDC_PROFMNG_DATAURL_EDIT)->GetWindowText(strUrlName);

	if (strUrlName.IsEmpty())
		strUrlName = strNewName + szXmlExt;
	
	if (strUrlName.Right(4).CompareNoCase(szXmlExt) != 0)
		strUrlName += szXmlExt;

	m_pXMLProfileInfo->SetUrlData(strUrlName);	
	m_pXMLProfileInfo->SetModified();

	BOOL bOk = m_pXMLProfileInfo->SaveProfile(strNewPath, strNewName);
	if (
			bOk &&
			AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD) &&
			!m_pXMLProfileInfo->m_strSchemaFileName.IsEmpty() &&
			!m_pXMLProfileInfo->IsTransformProfile()
		)
	{
		if (
				::ExistFile(m_pXMLProfileInfo->m_strSchemaFileName) &&
				AfxMessageBox(_TB("The profile needs to update the schema XSD for Magic Documents integration.\n It's recommended to update it."), MB_YESNO) == IDYES
			)
		{
			CFunctionDescription aFuncDescri;
			AfxGetTbCmdManager()->GetFunctionDescription(_T("Function.Extensions.XEngine.TBXMLTransfer.CreateSmartXSD"), aFuncDescri); 
			aFuncDescri.GetParamValue(szDocumentNamespace)->Assign(m_pXMLProfileInfo->GetNamespaceDoc().ToString());
			aFuncDescri.GetParamValue(szProfilePath)->Assign(m_pXMLProfileInfo->m_strDocProfilePath);
			BOOL bResult = AfxGetTbCmdManager()->RunFunction(&aFuncDescri, 0);
			AfxMessageBox(bResult ? _TB("Schema XSD successfully created.") : _TB("An error occured creating schema XSD.\nYou can try to re-create the schema using the Export Wizard"));			
		}
	}

	return bOk;
}
