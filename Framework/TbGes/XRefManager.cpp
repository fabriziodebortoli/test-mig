
#include "stdafx.h"

#include <TbGenlib\generic.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\baseapp.h>

#include <TbParser\symtable.h>

#include <TbGenlibUI\TBExplorer.h>

#include <TbOledb\sqltable.h>
#include <TbOledb\sqlcatalog.h>
#include <TbOleDb\oledbmng.h>

#include <TbWoormEngine\rpsymtbl.h>

#include "xmlgesinfo.h"
#include "xrefmanager.h"

//resource
#include "xrefmanager.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimoo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define PROF_READ		0
#define PROF_MODIFY		1
#define PROF_NEW		2

#define XREF_MNG_ITEM_TYPE_ADDONAPP		0
#define XREF_MNG_ITEM_TYPE_MODULE		1
#define XREF_MNG_ITEM_TYPE_DOCUMENT		2
#define XREF_MNG_TREE_ITEM_TYPE_PROFILE	3

/////////////////////////////////////////////////////////////////////////////
// CWizardInformation implementatio
/////////////////////////////////////////////////////////////////////////////
//
CWizardInformation::CWizardInformation()
	:
	m_pXMLXRefInfo	(NULL),
	m_bDeleteFkPk	(FALSE)
{
}

//---------------------------------------------------------------------------
CWizardInformation::~CWizardInformation()
{
}



/////////////////////////////////////////////////////////////////////////////
// CXRefWizardPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CXRefWizardPage, CWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXRefWizardPage, CWizardPage)
	//{{AFX_MSG_MAP(CXRefWizardPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CXRefWizardPage::CXRefWizardPage(UINT nIDTemplate, CWnd* pParent /*=NULL*/) 
:
	m_pXRefSheet(NULL),
	CWizardPage	(nIDTemplate, pParent)
{
	m_pToolTip = new CToolTipCtrl;
}

//--------------------------------------------------------------------
CXRefWizardPage::~CXRefWizardPage()
{
	if(m_pToolTip)
	{
		delete m_pToolTip;

		m_pToolTip = NULL;
	}
}

//--------------------------------------------------------------------
BOOL CXRefWizardPage::OnInitDialog()
{
	CWizardPage::OnInitDialog();

	m_pXRefSheet = (CXRefWizMasterDlg*)GetParent();
	ASSERT_KINDOF(CXRefWizMasterDlg, m_pXRefSheet);
	
	if(!m_pToolTip->Create(this))
		ASSERT(FALSE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXRefWizardPage::OnCreatePage()
{
	if (!m_pParent || !m_pParent->IsKindOf(RUNTIME_CLASS(CXRefWizMasterDlg)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXRefWizardPage::PreTranslateMessage(MSG* pMsg) 
{
	if(m_pToolTip != NULL)
		m_pToolTip->RelayEvent(pMsg);
	
	return __super::PreTranslateMessage(pMsg);
}

/////////////////////////////////////////////////////////////////////////////
// CXRefFieldsSelectPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CXRefWizPresentationPage, CXRefWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXRefWizPresentationPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefWizPresentationPage)
	ON_BN_CLICKED	(IDC_XREF_EXPORT, OnExportCheck)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CXRefWizPresentationPage::CXRefWizPresentationPage() 
	: 
	CXRefWizardPage	(IDD_XREF_WIZ_PRESENTATION),
	m_Image			(IDB_XREF_WIZARD_PRESENTATION_PAGE)
{
}

//--------------------------------------------------------------------
BOOL CXRefWizPresentationPage::OnInitDialog()
{
	CXRefWizardPage::OnInitDialog();

	 m_Image.SubclassDlgItem(IDC_XREF_WIZARD_PRESENTATION_PAGE, this);
 
	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName.IsEmpty())
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName = _TB("New");

	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati.IsEmpty())
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName + szXmlExt;
	
	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati.Right(4).CompareNoCase(szXmlExt) != 0)
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati += szXmlExt;

	VERIFY (m_edName.SubclassDlgItem(IDC_XREF_NAME_EDIT, this));
	VERIFY (m_edUrl.SubclassDlgItem(IDC_XREF_DATAURL_EDIT, this));
	
	m_edName.SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName);
	m_edUrl.SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati);

	m_edName.EnableWindow(!m_pXRefSheet->m_bReadOnly);
	m_edUrl.EnableWindow(!m_pXRefSheet->m_pDocObjectInfo->IsReadOnly());
	return TRUE;
}

//--------------------------------------------------------------------
void CXRefWizPresentationPage::OnActivate()
{	
	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - External Reference Creation Wizard"));

	CXRefWizardPage::OnActivate();
}

//--------------------------------------------------------------------
void CXRefWizPresentationPage::OnExportCheck()
{
	if(m_pXRefSheet->m_bReadOnly && !m_pXRefSheet->m_bDescription)
	{
		if(!m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->IsToUse())
		{
			m_pXRefSheet->EnableNextBtn(FALSE);
			m_pXRefSheet->EnableFinishBtn();
		}
	}
}

//--------------------------------------------------------------------
LRESULT CXRefWizPresentationPage::OnWizardNext()
{
	m_edName.GetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName);
	
	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName.IsEmpty())
	{
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName = _TB("New");

		m_edName.SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName);
	}

	m_edUrl.GetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati);

	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati.IsEmpty())
	{
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName + szXmlExt;

		m_edName.SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName);
	}

	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati.Right(4).CompareNoCase(szXmlExt) != 0)
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati += szXmlExt;

	if(m_pXRefSheet->m_bReadOnly)
		return IDD_XREF_WIZ_DOC_SEL;

	return CXRefWizardPage::OnWizardNext();
}

/////////////////////////////////////////////////////////////////////////////
// CXRefFieldsSelectPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CXRefFieldsSelectPage, CXRefWizardPage)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXRefFieldsSelectPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefFieldsSelectPage)
	ON_BN_CLICKED		(IDC_ADD_TO_LIST			, OnAddSegment)
	ON_BN_CLICKED		(IDC_REMOVE_FROM_LIST		, OnRemoveSegment)
	ON_LBN_DBLCLK		(IDC_XREF_FK_LIST			, OnDblClickListAdd)
	ON_LBN_DBLCLK		(IDC_XREF_FK_SELECTED_LIST	, OnDblClickListRemove)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CXRefFieldsSelectPage::CXRefFieldsSelectPage() 
	: 
	CXRefWizardPage	(IDD_XREF_WIZ_FK_LIST),
	m_Image			(IDB_XREF_WIZARD_SEL_FK)
{
}

//--------------------------------------------------------------------
BOOL CXRefFieldsSelectPage::OnInitDialog()
{
	CXRefWizardPage::OnInitDialog();
	
	m_Image.SubclassDlgItem(IDC_XREF_WIZARD_SEL_FK, this);

	if(!m_pXRefSheet)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	VERIFY (m_FieldListBox.SubclassDlgItem(IDC_XREF_FK_LIST, this));
	VERIFY (m_FieldListBoxSelected.SubclassDlgItem(IDC_XREF_FK_SELECTED_LIST, this));
	
	FillFieldListBox();

	GetDlgItem(IDC_XREF_TABLE_NAME)->SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetTableName());
	
	m_FieldListBox.EnableWindow(!m_pXRefSheet->m_bReadOnly);
	m_FieldListBoxSelected.EnableWindow(!m_pXRefSheet->m_bReadOnly);
	GetDlgItem(IDC_ADD_TO_LIST)->EnableWindow(!m_pXRefSheet->m_bReadOnly);
	GetDlgItem(IDC_REMOVE_FROM_LIST)->EnableWindow(!m_pXRefSheet->m_bReadOnly);
	
	return TRUE;
}

//--------------------------------------------------------------------
void CXRefFieldsSelectPage::FillFieldListBox()
{
	if (!m_pXRefSheet) 
	{
		ASSERT(FALSE);
		return;
	}
	
	m_FieldListBox.ResetContent();

	if(!m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo)
	{
		ASSERT(FALSE);
		return;
	}

	SqlTableInfo* pTblInfo = AfxGetDefaultSqlConnection()->GetTableInfo(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetTableName());

	if(!pTblInfo)
	{
		ASSERT(FALSE);
		return;
	}

	const  Array* pColums = pTblInfo->GetPhysicalColumns();
	int i = 0;
	
	BOOL bExportAllFields = !m_pXRefSheet->m_pXMLFieldInfoArray;
	for(i = 0 ; i < pColums->GetSize() ; i++)
	{
		SqlColumnInfo* pInfo = (SqlColumnInfo*) pColums->GetAt(i);
		if(!pInfo)
			continue;

		CString strColName = pInfo->GetColumnName();

		BOOL bToExport = TRUE;
		CXMLFieldInfo* pXMLFieldInfo = NULL;

		if	(m_pXRefSheet->m_pXMLFieldInfoArray)
		{	
			pXMLFieldInfo = m_pXRefSheet->m_pXMLFieldInfoArray->GetFieldByName(strColName);
			// PK always exported
			if (pTblInfo->GetAt(i)->m_bSpecial)
			{
				m_FieldListBox.AddString(strColName);
				continue;
			}
			if (
					!pXMLFieldInfo || 
					(!bExportAllFields && !pXMLFieldInfo->IsToExport())
				)
				bToExport = FALSE;
		}

		if(bToExport)
			m_FieldListBox.AddString(strColName);
	}

	for(int n = 0 ;  n < m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_SegmentsArray.GetSize() ; n++)
	{
		CString strFK = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_SegmentsArray.GetAt(n)->GetFKSegment();

		for(i = 0 ; i < m_FieldListBox.GetCount() ; i++)
		{
			CString strTmp;
			m_FieldListBox.GetText(i, strTmp);
			if(strFK == strTmp)
				m_FieldListBox.DeleteString(i);
		}

		m_FieldListBoxSelected.AddString(strFK);
	}
}

//--------------------------------------------------------------------
void CXRefFieldsSelectPage::OnActivate()
{	
	if (!m_pXRefSheet)
	{
		ASSERT(FALSE);
		CXRefWizardPage::OnActivate();
		return;
	}

	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - Field Selection"));

	CXRefWizardPage::OnActivate();
}

//---------------------------------------------------------------------------------
void CXRefFieldsSelectPage::OnDblClickListAdd()
{
	OnAddSegment();
}

//---------------------------------------------------------------------------------
void CXRefFieldsSelectPage::OnDblClickListRemove()
{
	OnRemoveSegment();
}

//---------------------------------------------------------------------------------
void CXRefFieldsSelectPage::OnAddSegment()
{
	CString strText;
	for(int i = m_FieldListBox.GetCount() - 1 ; i >= 0  ; i--)
	{
		if(m_FieldListBox.GetSel(i) > 0)
		{
			m_FieldListBox.GetText(i, strText);
			
			m_FieldListBoxSelected.AddString(strText);

			m_FieldListBox.DeleteString(i);
		}
	}
}

//---------------------------------------------------------------------------------
void CXRefFieldsSelectPage::OnRemoveSegment()
{
	CString strText;
	int size = m_FieldListBoxSelected.GetCount();
	for(int i = size - 1 ; i >= 0 ; i--)
	{
		if(m_FieldListBoxSelected.GetSel(i) > 0)
		{
			m_FieldListBoxSelected.GetText(i, strText);
			m_FieldListBoxSelected.DeleteString(i);
			m_FieldListBox.AddString(strText);

			m_pXRefSheet->m_WizardInfo.m_bDeleteFkPk = TRUE;
		}
	}
}

//--------------------------------------------------------------------
LRESULT CXRefFieldsSelectPage::OnWizardNext()
{
	if (!m_pXRefSheet)
	{
		ASSERT(FALSE);
		return IDD_XREF_WIZ_FK_LIST;
	}

	m_pXRefSheet->m_WizardInfo.m_FieldXRefAr.RemoveAll();

	if(!m_FieldListBoxSelected.GetCount())
	{
		AfxMessageBox(_TB("To continue it is necessary to select at least one field."));
		return IDD_XREF_WIZ_FK_LIST;
	}

	for(int i = 0 ; i < m_FieldListBoxSelected.GetCount() ; i++)
	{
		CString strText;
		m_FieldListBoxSelected.GetText(i, strText);
		m_pXRefSheet->m_WizardInfo.m_FieldXRefAr.Add(strText);
	}

	return CXRefWizardPage::OnWizardNext();
}

/////////////////////////////////////////////////////////////////////////////
// CXRefFieldsPropPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CXRefFieldsPropPage, CXRefWizardPage)

//--------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXRefFieldsPropPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefFieldsPropPage)
	ON_BN_CLICKED	(IDC_XREF_SUBJECT_TO_CHKBOX, OnChangeSubjectTo)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//--------------------------------------------------------------------
CXRefFieldsPropPage::CXRefFieldsPropPage() 
	: 
	CXRefWizardPage		(IDD_XREF_WIZ_FIELD_PROP),
	m_bCreateSymTable	(FALSE),
	m_Image				(IDB_XREF_WIZARD_OPTION)
{
}

//--------------------------------------------------------------------
BOOL CXRefFieldsPropPage::OnInitDialog()
{
	CXRefWizardPage::OnInitDialog();
	
	m_Image.SubclassDlgItem(IDC_XREF_WIZARD_OPTION, this);

	VERIFY (m_bnExist.SubclassDlgItem(IDC_XREF_EXIST_CHKBOX, this));
	VERIFY (m_bnNull.SubclassDlgItem(IDC_XREF_NULL_CHKBOX, this));
	VERIFY (m_bnIndip.SubclassDlgItem(IDC_XREF_NOT_DOC_QUERY_CHKBOX, this));
	VERIFY (m_bnSubjectTo.SubclassDlgItem(IDC_XREF_SUBJECT_TO_CHKBOX, this));
	VERIFY (m_EdtExpression.SubclassDlgItem(IDC_XREF_EXPRESSION, this));

	m_bnExist.SetCheck(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bMustExist);
	m_bnNull.SetCheck(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bCanbeNull);
	m_bnIndip.SetCheck(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bNoDocQuery);
	
	m_bnSubjectTo.SetCheck(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bSubjectTo);
	m_strExpression = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strExpression;
	m_EdtExpression.Attach(&m_strExpression);

	m_EdtExpression.UpdateCtrlView();

	OnChangeSubjectTo();

	if (m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
	{
		m_bnExist.EnableWindow(FALSE);
		m_bnNull.EnableWindow(FALSE);
		m_bnIndip.EnableWindow(FALSE);
		m_bnSubjectTo.EnableWindow(FALSE);
		m_EdtExpression.EnableWindow(FALSE);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void CXRefFieldsPropPage::OnActivate()
{
	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - Data Properties"));

	CXRefWizardPage::OnActivate();
}

//--------------------------------------------------------------------
void CXRefFieldsPropPage::OnChangeSubjectTo()
{
	m_EdtExpression.EnableWindow(m_bnSubjectTo.GetCheck());
	
	if (!m_bnSubjectTo.GetCheck())
		m_EdtExpression.ClearCtrl();
	else
		if (!m_bCreateSymTable)
		{
			m_EdtExpression.SetSymbolTable(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pSymTable);
			m_EdtExpression.SetTableInfo(AfxGetDefaultSqlConnection()->GetTableInfo(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetTableName()));
			m_bCreateSymTable = FALSE;
		}
}

//--------------------------------------------------------------------
LRESULT CXRefFieldsPropPage::OnWizardNext()
{
	CString strErrMess;
	if (
			m_bnSubjectTo.GetCheck() &&
			!m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->CheckExpressionSintax(m_strExpression, strErrMess)
		)
	{
		AfxMessageBox(strErrMess);
		return IDD_XREF_WIZ_FIELD_PROP;	
	}
	
	
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bMustExist	= m_bnExist.GetCheck();
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bCanbeNull	= m_bnNull.GetCheck();
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bNoDocQuery = m_bnIndip.GetCheck();
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_bSubjectTo = m_bnSubjectTo.GetCheck();
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strExpression = m_strExpression;
	
	if(m_pXRefSheet->m_bReadOnly || m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
	{
		if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
			return IDD_XREF_WIZ_UNIVERSAL_KEY;
		else
			return IDD_XREF_WIZ_FINISH_PAGE;
	}

	return CXRefWizardPage::OnWizardNext();
}

//--------------------------------------------------------------------
LRESULT CXRefFieldsPropPage::OnWizardBack()
{
	if (m_pXRefSheet->m_bReadOnly || m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
		return IDD_XREF_WIZ_DOC_SEL;
	
	return CXRefWizardPage::OnWizardBack();
}

/////////////////////////////////////////////////////////////////////////////
// CXRefDocSelPropPage dialog implementation
/////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP(CXRefDocSelPropPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefDocSelPropPage)
	ON_NOTIFY		(TVN_SELCHANGED, IDC_XREF_SEL_APP_DOC_TREECTRL, OnAppDocSelChanged)
	ON_BN_CLICKED	(IDC_XREF_PREF_PROF_CHECK,			OnPrefProfChek)
	ON_BN_CLICKED	(IDC_XREF_NO_PREF_PROF_CHECK,		OnNoPrefProfChek)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//----------------------------------------------------------------------------
CXRefDocSelPropPage::CXRefDocSelPropPage()
	: 
	CXRefWizardPage		(IDD_XREF_WIZ_DOC_SEL),
	m_Image				(IDB_XREF_WIZARD_FINISH)
{
}

//----------------------------------------------------------------------------
CXRefDocSelPropPage::~CXRefDocSelPropPage()
{

} 

//----------------------------------------------------------------------------
BOOL CXRefDocSelPropPage::OnInitDialog() 
{
	CXRefWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_XREF_WIZARD_FINISH, this);

	VERIFY(m_ProfileCombo.SubclassDlgItem(IDC_XREF_SELPROFILE_COMBO, this));
	VERIFY(m_AppDocTreeCtrl.SubclassDlgItem(IDC_XREF_SEL_APP_DOC_TREECTRL, this));
	m_AppDocTreeCtrl.SetBkColor(CLR_WHITE);
	m_AppDocTreeCtrl.SetInsertMarkColor(CLR_WHITE);

	m_AppDocTreeCtrl.FillTree(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetDocumentNamespace());
	m_AppDocTreeCtrl.SetReadOnly(m_pXRefSheet->m_bReadOnly);
	
	if(m_pXRefSheet->m_bDescription || !m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetDocumentNamespace().IsValid())
	{
		((CButton*)GetDlgItem(IDC_XREF_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
		((CButton*)GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
		((CButton*)GetDlgItem(IDC_XREF_SELPROFILE_COMBO))->ShowWindow(SW_HIDE);
	}
	else
		if (m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
		{
			GetDlgItem(IDC_XREF_PREF_PROF_CHECK)->EnableWindow(FALSE);
			GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK)->EnableWindow(FALSE);
			GetDlgItem(IDC_XREF_SELPROFILE_COMBO)->EnableWindow(FALSE);
		}
	
	return TRUE; 
}

// Seleziona documento da usare con l'external reference
//----------------------------------------------------------------------------
void CXRefDocSelPropPage::OnAppDocSelChanged(NMHDR* /*pNMHDR*/, LRESULT* /*pResult*/) 
{
	SelectCurrentAppDoc();
}

//----------------------------------------------------------------------------
void CXRefDocSelPropPage::OnActivate()
{
	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - Document Selection"));

	CXRefWizardPage::OnActivate();
}

//----------------------------------------------------------------------------
void CXRefDocSelPropPage::OnPrefProfChek()
{
	if (AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD))
		AfxMessageBox(_TB("If you are creating this profile for Magic Document, for this external reference it would be better to choose a specific profile and not the preferential one"), MB_OK | MB_ICONINFORMATION);			

	m_nProfCheck = 0;
	
	m_ProfileCombo.EnableWindow(FALSE);

	m_ProfileCombo.ResetContent();
}

//----------------------------------------------------------------------------
void CXRefDocSelPropPage::OnNoPrefProfChek()
{
	CDocumentDescription* pDocModInfo = m_AppDocTreeCtrl.GetCurrentDocInfo();

	if(!pDocModInfo)
		return;

	m_ProfileCombo.EnableWindow(TRUE);
	m_ProfileCombo.ResetContent();

	CStringArray aProfileList;
	GetProfileList(pDocModInfo->GetNamespace(), aProfileList);

	if (aProfileList.GetSize() > 0)
	{
		for(int i = 0 ; i < aProfileList.GetSize() ; i++)
			m_ProfileCombo.InsertString(i, aProfileList.GetAt(i));

		m_ProfileCombo.SelectString(-1, aProfileList.GetAt(0));
	}
	
	m_nProfCheck = 1;
}

//----------------------------------------------------------------------------
LRESULT CXRefDocSelPropPage::OnWizardNext()
{
	if (m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
		return IDD_XREF_WIZ_FIELD_PROP;

	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strProfile = m_nProfCheck ? m_ProfileCombo.GetSelectedProfile() : _T("");	
	CString strReferencedTableName;
	CTBNamespace nsrReferencedDBT;
	if(	m_pXRefSheet && m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo)
	{
		strReferencedTableName = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetReferencedTableName();
		nsrReferencedDBT = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetReferencedDBTNs();
	}

	if(strReferencedTableName.IsEmpty())
	{
		AfxMessageBox(_TB("The description of the selected document is not available."));
		return IDD_XREF_WIZ_DOC_SEL;
	}

	CTBNamespace aDocRefNS = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetDocumentNamespace();

	//istanzio il SqlRecord: serve per avere le informazioni legate al tipo dei dataobj
	const SqlCatalogEntry* pRefEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(strReferencedTableName);
		
	//istanzio il sqlRecord per creare il SqlTableInfo e aggiornare i dati sul tipo del dataobj
	SqlRecord* pRecord = pRefEntry->CreateRecord();
	if (pRecord)
		delete pRecord;
	
	SqlTableInfo* pReferencedTblInfo = (pRefEntry) ? pRefEntry->m_pTableInfo : NULL;
	if(!pReferencedTblInfo)
	{
		AfxMessageBox(_TB("Select a document"));
		return IDD_XREF_WIZ_DOC_SEL;
	}

	CXMLDocObjectInfo aXMLDocInfo(aDocRefNS);
	aXMLDocInfo.LoadAllFiles();
	CXMLDBTInfo* pXMLDbtMasterInfo = aXMLDocInfo.GetDBTMaster();
	//if (nsrReferencedDBT.IsEmpty())
	//	pXMLDbtMasterInfo = aXMLDocInfo.GetDBTMaster();
	//else
	//	pXMLDbtMasterInfo = aXMLDocInfo.GetDBTFromNamespace(nsrReferencedDBT);

	if(!pXMLDbtMasterInfo)
	{
		ASSERT(FALSE);
		return IDD_XREF_WIZ_DOC_SEL;
	}

	if(pXMLDbtMasterInfo->GetXMLUniversalKeyGroup())
	{
		if(!m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
			m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup();

		BOOL bExp = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->IsExportData();

		*(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup) = *(pXMLDbtMasterInfo->GetXMLUniversalKeyGroup());
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->SetTableName(pXMLDbtMasterInfo->GetTableName());
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->SetExportData(bExp);
	}
	else
	{
		if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
		{
			delete m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup;
			m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup = NULL;
		}
	}

	if(m_pXRefSheet->m_bReadOnly )
		return IDD_XREF_WIZ_FIELD_PROP;

	return CXRefWizardPage::OnWizardNext();
}

//----------------------------------------------------------------------------
LRESULT CXRefDocSelPropPage::OnWizardBack()
{
	if(m_pXRefSheet->m_bReadOnly || m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
		return IDD_XREF_WIZ_PRESENTATION;

	return CXRefWizardPage::OnWizardBack();

}

//----------------------------------------------------------------------------
void CXRefDocSelPropPage::GetProfileList(const CTBNamespace& aDocNamespace, CStringArray& aProfileList)
{
	if (AfxIsActivated(TBEXT_APP, XENGINE_ACT))
	{
		CFunctionDescription aFunctionDescription;
		if (!AfxGetTbCmdManager()->GetFunctionDescription(_NS_WEB("Extensions.XEngine.TbXmlTransfer.GetExportProfileList"), aFunctionDescription))
			return;
		
		aFunctionDescription.GetParamValue(_T("documentNamespace"))->Assign(aDocNamespace.ToString());
		((DataInt*)aFunctionDescription.GetParamValue(_T("posType")))->Assign((int)m_pXRefSheet->m_pDocObjectInfo->m_ePosType);
		aFunctionDescription.GetParamValue(_T("userName"))->Assign(m_pXRefSheet->m_pDocObjectInfo->m_strUserName);	
			
		AfxGetTbCmdManager()->RunFunction(&aFunctionDescription, 0);

		CDataObjDescription& aDOD = aFunctionDescription.GetReturnValueDescription();
		DataArray *pValues = (DataArray *) aDOD.GetValue();
		if (pValues)
		{
			//viene restituita la path del profilo e non il solo nome
			for (int i = 0 ; i < pValues->GetSize() ; i++)
				aProfileList.Add(::GetName(pValues->GetAt(i)->Str()));
		}
	}
}
//----------------------------------------------------------------------------
void CXRefDocSelPropPage::SelectCurrentAppDoc() 
{
	CDocumentDescription* pDocModInfo = m_AppDocTreeCtrl.GetCurrentDocInfo();
			
	if(pDocModInfo)
	{	
		if (m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetDocumentNamespace() != pDocModInfo->GetNamespace())
		{
			m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->SetDocumentNamespace(pDocModInfo->GetNamespace());
			m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->RemoveAllSegments();
		}

		if(m_pXRefSheet->m_bDescription)
		{
			((CButton*)GetDlgItem(IDC_XREF_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
			((CButton*)GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
			m_ProfileCombo.ShowWindow(SW_HIDE);
		}
		else
		{
			CStringArray aProfileList;
			GetProfileList(pDocModInfo->GetNamespace(), aProfileList);

			if(aProfileList.GetSize() == 0)
			{
				((CButton*)GetDlgItem(IDC_XREF_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
				((CButton*)GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
				m_ProfileCombo.ShowWindow(SW_HIDE);
			}
			else
			{
				((CButton*)GetDlgItem(IDC_XREF_PREF_PROF_CHECK))->ShowWindow(SW_SHOW);
				((CButton*)GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK))->ShowWindow(SW_SHOW);
				m_ProfileCombo.ShowWindow(SW_SHOW);
				m_ProfileCombo.EnableWindow(TRUE);
				m_ProfileCombo.SetDocumentNameSpace(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetDocumentNamespace(), aProfileList);

				//non e' stato selezionato alcun profilo => seleziono il preferenziale se non sono in MagicDocument
				// altrimenti seleziono il primo profilo della lista
				BOOL isMagicDoc = AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD);
				BOOL bSetProfileList = isMagicDoc || !m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strProfile.IsEmpty();
				((CButton*)GetDlgItem(IDC_XREF_PREF_PROF_CHECK))->SetCheck(!bSetProfileList);
				((CButton*)GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK))->SetCheck(bSetProfileList);

				if (bSetProfileList)
				{
					if (m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strProfile.IsEmpty())
						m_ProfileCombo.SetCurSel(0);
					else
						m_ProfileCombo.SelectProfile(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strProfile);
				}
				m_ProfileCombo.EnableWindow(bSetProfileList);
			}
		}
	}
	else
	{
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->SetDocumentNamespace(CTBNamespace());
		m_ProfileCombo.SetDocumentNameSpace(CTBNamespace(), CStringArray());
		m_ProfileCombo.ShowWindow(SW_HIDE);
		((CButton*)GetDlgItem(IDC_XREF_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
		((CButton*)GetDlgItem(IDC_XREF_NO_PREF_PROF_CHECK))->ShowWindow(SW_HIDE);
	}
}

/////////////////////////////////////////////////////////////////////////////
//class CXRefSegmentsPage implementation
/////////////////////////////////////////////////////////////////////////////
//
BEGIN_MESSAGE_MAP(CXRefSegmentsPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefSegmentsPage)
	ON_NOTIFY			(LVN_ITEMCHANGED, IDC_XREFSEG_SEGMENTS_LISTCTRL, OnSegmentStateChanged)
	ON_CBN_SELCHANGE	(IDC_XREFSEG_FK_CBO,	OnFKSegmentSelChange)
	ON_CBN_SELCHANGE	(IDC_XREFSEG_REF_CBO,	OnPKSegmentSelChange)
	ON_CBN_SELCHANGE	(IDC_XREF_DBTS,			OnDBTSegmentSelChange)
	ON_EN_CHANGE		(IDC_XREFSEG_FK_FIXED_EDIT, OnModifySegment)
	ON_BN_CLICKED		(IDC_XREFSEG_MODIFY_SEGMENT_BTN	, OnModifySegment)
	ON_BN_CLICKED		(IDC_XREFSEG_ADD_SEGMENT_BTN	, OnAddSegment)
	ON_BN_CLICKED		(IDC_XREFSEG_REMOVE_SEGMENT_BTN	, OnRemoveSegment)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//---------------------------------------------------------------------------------
CXRefSegmentsPage::CXRefSegmentsPage()
	:
	CXRefWizardPage	(IDD_XREF_WIZ_SEGMENT),
	m_Image			(IDB_XREF_WIZARD_SEL_DOC)
{
	
}

//---------------------------------------------------------------------------------
CXRefSegmentsPage::~CXRefSegmentsPage()
{
	CXMLDBTData* pData;
	for (int i=m_RefDBTs.GetUpperBound(); i >= 0; i--)
	{
		pData = (CXMLDBTData*) m_RefDBTs.GetAt(i);
		if (pData)
			delete pData;
	}
	
	m_RefDBTs.RemoveAll();
}

//--------------------------------------------------------------------
BOOL CXRefSegmentsPage::IsFkSelected(const CString& strColName)
{
	if(strColName.IsEmpty() || !m_pXRefSheet)
		return FALSE;

	for(int i = 0 ; i < m_pXRefSheet->m_WizardInfo.m_FieldXRefAr.GetSize() ; i++)
	{
		if(m_pXRefSheet->m_WizardInfo.m_FieldXRefAr.GetAt(i).CompareNoCase(strColName) == 0)
			return TRUE;
	}

	return FALSE;
}

//--------------------------------------------------------------------
void CXRefSegmentsPage::OnActivate()
{
	OnDBTSegmentSelChange();

	CString strTableName = m_pXRefInfo ? m_pXRefInfo->GetTableName() : _T("");
	SetDlgItemText(IDC_XREFSEG_FK_TABLE_STATIC, (LPCTSTR)strTableName);

	CString strReferencedTableName = m_pXRefInfo ? m_pXRefInfo->GetReferencedTableName() : _T("");
	SetDlgItemText(IDC_XREFSEG_REF_TABLE_STATIC, (LPCTSTR)strReferencedTableName);

	SqlTableInfo* pTblInfoFK = AfxGetDefaultSqlConnection()->GetTableInfo(strTableName);

	if(!pTblInfoFK)
	{
		AfxMessageBox(cwsprintf(_TB("The table {0-%s} does not exist."), strTableName));
		return;
	}

	ASSERT(!strReferencedTableName.IsEmpty());	
	SqlTableInfo* pReferencedTblInfo = AfxGetDefaultSqlConnection()->GetTableInfo(strReferencedTableName);
	if(!pReferencedTblInfo)
	{
		if(strReferencedTableName.IsEmpty())
			AfxMessageBox(_TB("Select a document to export"));
		else
			AfxMessageBox(cwsprintf(_TB("The table {0-%s} does not exist."), strReferencedTableName));

		return;
	}
	
	if(m_pXRefSheet->m_WizardInfo.m_bDeleteFkPk)
	{
		m_pXRefSheet->m_WizardInfo.m_bDeleteFkPk = FALSE;
		
		int nSegNum = m_pXRefInfo->GetSegmentsNum();

		for(int n = nSegNum - 1 ; n >= 0 ; n--)
			m_pXRefInfo->RemoveSegmentAt(n);
	}

	m_SegmentsListCtrl.AttachXRef(m_pXRefInfo);

	m_FKCombo.ResetContent();

	const  Array* pColums = pTblInfoFK->GetPhysicalColumns();
	for (int i = 0 ; i < pColums->GetSize() ; i++)
	{
		SqlColumnInfo* pInfo = (SqlColumnInfo*) pColums->GetAt(i);
		if(!pInfo)
			continue;

		if(IsFkSelected(pInfo->m_strColumnName) && FkAsCompatiblePk(pInfo, pReferencedTblInfo))
		{
			int nIdx = m_FKCombo.InsertString(-1, pInfo->m_strColumnName);
			m_FKCombo.SetItemData(nIdx, (DWORD)pInfo);
		}
	}

	if(m_FKCombo.GetCount() == 0)
		AfxMessageBox(_TB("No selected key segment is compatible with the linked document"));

	m_ReferencedCombo.ResetContent();

	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - Document Link Creation"));

	CXRefWizardPage::OnActivate();
}

//---------------------------------------------------------------------------------
BOOL CXRefSegmentsPage::OnInitDialog()
{
	CXRefWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_XREF_WIZARD_SEL_DOC, this);

	m_pXRefInfo = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo;
	
	m_cbxDBTs.SubclassDlgItem(IDC_XREF_DBTS, this);
	
	FillExtRefDBTs ();
	
	m_SegmentsListCtrl.SetCurrDBT (m_pXRefInfo ->GetReferencedDBTNs().ToString());

	m_FKCombo.SubclassDlgItem(IDC_XREFSEG_FK_CBO, this);
	m_edFKFixedValue.SubclassDlgItem(IDC_XREFSEG_FK_FIXED_EDIT, this);
	
	m_ReferencedCombo.SubclassDlgItem(IDC_XREFSEG_REF_CBO, this);
	
	m_SegmentsListCtrl.SubclassDlgItem(IDC_XREFSEG_SEGMENTS_LISTCTRL, this);

	if(!m_pToolTip->AddTool(&m_FKCombo, _TB("Field of the source table used as start point of the External Reference relation.")))
		ASSERT(FALSE);

	if(!m_pToolTip->AddTool(&m_ReferencedCombo, _TB("Field of the target table.")))
		ASSERT(FALSE);

	if(!m_pToolTip->AddTool(&m_SegmentsListCtrl, _TB("List of the inserted key segments.")))
		ASSERT(FALSE);

	m_pToolTip->Activate(TRUE);

	m_FKCombo.EnableWindow(!m_pXRefSheet->m_bReadOnly);
	m_ReferencedCombo.EnableWindow(!m_pXRefSheet->m_bReadOnly);
	m_SegmentsListCtrl.EnableWindow(!m_pXRefSheet->m_bReadOnly);
	
	GetDlgItem(IDC_XREFSEG_MODIFY_SEGMENT_BTN)->EnableWindow(FALSE);
	GetDlgItem(IDC_XREFSEG_ADD_SEGMENT_BTN)->EnableWindow(FALSE);
	GetDlgItem(IDC_XREFSEG_REMOVE_SEGMENT_BTN)->EnableWindow(FALSE);
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CXRefSegmentsPage::OnSegmentStateChanged(NMHDR* pNMHDR, LRESULT* /*pResult*/) 
{
	LPNMLISTVIEW lpListView = (LPNMLISTVIEW)pNMHDR;

	if (!lpListView || (lpListView->uChanged & LVIF_STATE) == 0)
		return;

	BOOL bItemSel = lpListView->iItem >= 0 &&
					lpListView->iItem < m_SegmentsListCtrl.GetRowCount() &&
					!(lpListView->uOldState & LVIS_SELECTED) &&
					lpListView->uNewState & LVIS_SELECTED;

	GetDlgItem(IDC_XREFSEG_MODIFY_SEGMENT_BTN)->EnableWindow(bItemSel);
	GetDlgItem(IDC_XREFSEG_REMOVE_SEGMENT_BTN)->EnableWindow(bItemSel);

	if (!bItemSel)
	{
		m_FKCombo.SetCurSel(-1);
		m_ReferencedCombo.SetCurSel(-1);
		m_ReferencedCombo.ResetContent();
		m_ReferencedCombo.EnableWindow(FALSE);
		return;
	}

	CString strFKSegment = m_SegmentsListCtrl.GetFKSegmentAt(lpListView->iItem);
	int nCurrFKIdx = m_FKCombo.FindStringExact(-1, (LPCTSTR) strFKSegment) ;
	ASSERT(nCurrFKIdx != CB_ERR );
	m_FKCombo.SetCurSel(nCurrFKIdx);
	DoFKSegmentSelChange();
	
	CString strReferencedSegment = m_SegmentsListCtrl.GetReferencedSegmentAt(lpListView->iItem);
	int nCurrRefIdx = m_ReferencedCombo.FindStringExact(-1, (LPCTSTR) strReferencedSegment) ;
	ASSERT(nCurrRefIdx != CB_ERR );
	m_ReferencedCombo.SetCurSel(nCurrRefIdx);

	CString strFKFixedValue = m_SegmentsListCtrl.GetFKFixedValueAt(lpListView->iItem);
	m_edFKFixedValue.SetWindowText(strFKFixedValue);
}

//---------------------------------------------------------------------------------------
BOOL CXRefSegmentsPage::FkAsCompatiblePk(SqlColumnInfo* pFKColInfo, SqlTableInfo* pReferencedTblInfo)
{
	if (!pFKColInfo || !pReferencedTblInfo)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	const  Array* pColums = pReferencedTblInfo->GetPhysicalColumns();
	for (int nRefIdx = 0 ; nRefIdx < pColums->GetSize() ; nRefIdx++)
	{
		SqlColumnInfo* pRefColInfo = (SqlColumnInfo*) pColums->GetAt(nRefIdx);
		if(!pRefColInfo)
			continue;
		if
			(
				pFKColInfo->GetDataObjType() == pRefColInfo->GetDataObjType()	&&
				pFKColInfo->GetColumnLength() == pRefColInfo->GetColumnLength()	&&
				(
					m_pXRefInfo->IsNotDocQueryToUse() ||
					(!m_pXRefInfo->IsNotDocQueryToUse() && pRefColInfo->m_bSpecial)
				)
			)
			return TRUE;
	}

	return FALSE;
}

//---------------------------------------------------------------------------------
// La combo dei segmenti riferiti m_ReferencedCombo è disabilitata finchè non si 
// sceglie un campo nella combo m_FKCombo dei segmenti di chiave esterna.
// Appena ciò accade la combo m_ReferencedCombo viene riempita solo con i campi
// della tabella riferita che sono dello stesso tipo e della stessa lunghezza del
// segmento di chiave esterna selezionato in m_FKCombo
//---------------------------------------------------------------------------------
void CXRefSegmentsPage::DoFKSegmentSelChange()
{
	m_ReferencedCombo.ResetContent();
	m_ReferencedCombo.EnableWindow(FALSE);

	if (!m_pXRefInfo)
		return;
	
	int nFKSelIdx = m_FKCombo.GetCurSel();
	if (nFKSelIdx == CB_ERR)
		return;

	SqlColumnInfo* pFKColInfo = (SqlColumnInfo*) m_FKCombo.GetItemData(nFKSelIdx);
	SqlTableInfo* pReferencedTblInfo = AfxGetDefaultSqlConnection()->GetTableInfo(m_pXRefInfo->GetReferencedTableName());

	if (!pFKColInfo || !pReferencedTblInfo)
	{
		ASSERT(FALSE);
		return;
	}

	const  Array* pColums = pReferencedTblInfo->GetPhysicalColumns();
	for (int nRefIdx = 0 ; nRefIdx < pColums->GetSize() ; nRefIdx++)
	{
		SqlColumnInfo* pRefColInfo = (SqlColumnInfo*) pColums->GetAt(nRefIdx);
		if(!pRefColInfo)
			continue;
		if
			(
				pFKColInfo->GetDataObjType() == pRefColInfo->GetDataObjType()	&&
				pFKColInfo->GetColumnLength() == pRefColInfo->GetColumnLength()	&&
				(
					m_pXRefInfo->IsNotDocQueryToUse() ||
					(!m_pXRefInfo->IsNotDocQueryToUse())
				)
			)
		{
			int nIdx = m_ReferencedCombo.InsertString(-1, pRefColInfo->m_strColumnName);
			m_ReferencedCombo.SetItemData(nIdx, (DWORD)pRefColInfo);
		}
	}
	if (m_ReferencedCombo.GetCount() == 0)
	{
		AfxMessageBox(cwsprintf(_TB("The table {0-%s} does not contain columns compatible with the field {1-%s}."), m_pXRefInfo->GetReferencedTableName(), pFKColInfo->m_strColumnName));
		return;
	}
	else
		m_ReferencedCombo.SetCurSel(0);

	m_ReferencedCombo.EnableWindow(TRUE);

	CString strComboFKSegment;
	m_FKCombo.GetLBText(nFKSelIdx, strComboFKSegment);

	int nSelGrid = m_SegmentsListCtrl.GetSelectedRowIdx();
	if(nSelGrid < 0)
	{
		BOOL bFound = FALSE;

		//controllo che la fk non sia gia' stata inserita
		for(int i = 0 ; i < m_SegmentsListCtrl.GetRowCount() ; i++)
		{
			CString strFKGridSegment = m_SegmentsListCtrl.GetFKSegmentAt(i);	
			if(strFKGridSegment.CompareNoCase(strComboFKSegment) == 0)
				bFound = TRUE;
		}	
		
		GetDlgItem(IDC_XREFSEG_ADD_SEGMENT_BTN)->EnableWindow(!bFound);
	}
	else
	{
		GetDlgItem(IDC_XREFSEG_ADD_SEGMENT_BTN)->EnableWindow(FALSE);
	}
}

//---------------------------------------------------------------------------------
void CXRefSegmentsPage::OnFKSegmentSelChange()
{
	DoFKSegmentSelChange ();
	int nSelGrid = m_SegmentsListCtrl.GetSelectedRowIdx();
	if(nSelGrid >= 0)
		OnModifySegment();
}

//---------------------------------------------------------------------------------
void CXRefSegmentsPage::OnPKSegmentSelChange()
{
	OnModifySegment();
}

//---------------------------------------------------------------------------------
void CXRefSegmentsPage::OnDBTSegmentSelChange ()
{
	int nIdx = m_cbxDBTs.GetCurSel();
	
	if (nIdx < 0)
		return;

	CXMLDBTData* pDBTData = (CXMLDBTData*)m_cbxDBTs.GetItemData (nIdx);
	
	if (
			m_SegmentsListCtrl.GetRowCount() > 0 && 
			(pDBTData->m_strNs.CompareNoCase(m_pXRefInfo->GetReferencedDBTNs().ToString()))
		)
		AfxMessageBox(_TB("You selected a different table.\nYou cannot add new segments to the list if you do not delete the previous inserted."));
	else
	{
		AfxGetTbCmdManager()->LoadNeededLibraries(pDBTData->m_strTableNs);

		m_pXRefInfo->SetReferencedDBTNs(pDBTData->m_strNs);
		m_pXRefInfo->SetReferencedTableNs(pDBTData->m_strTableNs);

		//I have to instance SqlRecord to bind data types
		const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(m_pXRefInfo->GetReferencedTableName());
		if (pEntry)
		{
			SqlRecord* pRecord = pEntry->CreateRecord();
			if (pRecord)
				delete pRecord;
		}
	}

	m_ReferencedCombo.SetCurSel(-1);
	m_ReferencedCombo.ResetContent();
}

//---------------------------------------------------------------------------------
BOOL CXRefSegmentsPage::CanAddSegment()
{
	int nIdx = m_cbxDBTs.GetCurSel();

	if (nIdx == CB_ERR)
	{
		AfxMessageBox(_TB("Please select a DBT relation for the external reference."));
		return FALSE;
	}

	CXMLDBTData* pData = (CXMLDBTData*) m_cbxDBTs.GetItemData(nIdx);
	if (!pData)
		return FALSE;

	BOOL bOk = m_SegmentsListCtrl.GetRowCount() == 0 || 
				pData->m_strNs.CompareNoCase(m_pXRefInfo->GetReferencedDBTNs().ToString()) == 0;
	if (!bOk)
		AfxMessageBox(_TB("You selected a different table for the current key segment.\nYou cannot add it to the list if you do not delete the previous inserted."));

	return bOk;
}

//---------------------------------------------------------------------------------
void CXRefSegmentsPage::OnModifySegment()
{
	if (!CanAddSegment())
		return;

	int nFKSelIdx = m_FKCombo.GetCurSel();
	int nPKSelIdx = m_ReferencedCombo.GetCurSel();

	if (nFKSelIdx == CB_ERR || nPKSelIdx == CB_ERR)
	{
		AfxMessageBox(_TB("Incomplete data."));
		return;
	}
	
	int nSelectedSegIdx = m_SegmentsListCtrl.GetSelectedRowIdx();

	if(nSelectedSegIdx < 0)
		return;

	SqlColumnInfo* pFKColInfo = (SqlColumnInfo*) m_FKCombo.GetItemData(nFKSelIdx);
	ASSERT (pFKColInfo);
	CString strNewFKSeg = pFKColInfo ? pFKColInfo->GetColumnName() : _T("");
	
	SqlColumnInfo* pRefColInfo = (SqlColumnInfo*) m_ReferencedCombo.GetItemData(nPKSelIdx);
	ASSERT (pRefColInfo);
	CString strNewRefSeg = pRefColInfo ? pRefColInfo->GetColumnName() : _T("");

	CString strFKFixedValue;
	m_edFKFixedValue.GetWindowText(strFKFixedValue);

	m_SegmentsListCtrl.ModifySegmentAt(nSelectedSegIdx, (LPCTSTR)strNewFKSeg, (LPCTSTR)strNewRefSeg, (LPCTSTR)strFKFixedValue);
}

//---------------------------------------------------------------------------------
void CXRefSegmentsPage::OnAddSegment()
{
	if (!CanAddSegment())
		return;

	int nDbtSelIdx = m_cbxDBTs.GetCurSel();
	int nFKSelIdx = m_FKCombo.GetCurSel();
	int nPKSelIdx = m_ReferencedCombo.GetCurSel();

	if (nDbtSelIdx == CB_ERR || nFKSelIdx == CB_ERR || nPKSelIdx == CB_ERR)
	{
		AfxMessageBox(_TB("Incomplete data."));
		return;
	}

	SqlColumnInfo* pFKColInfo = (SqlColumnInfo*) m_FKCombo.GetItemData(nFKSelIdx);
	SqlColumnInfo* pRefColInfo = (SqlColumnInfo*) m_ReferencedCombo.GetItemData(nPKSelIdx);
	
	const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(pRefColInfo->GetTableName());
	if (pEntry)
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->SetReferencedTableNs(pEntry->GetNamespace().ToString());
	
	if
		(
			!pFKColInfo || 
			!pRefColInfo ||
			pFKColInfo->GetDataObjType() != pRefColInfo->GetDataObjType() 
		)
	{
		AfxMessageBox(_TB("The field types of the selected segment pair are different."));
		return;
	}
	if (pFKColInfo->GetDataObjType().m_wType == DATA_STR_TYPE && pFKColInfo->GetColumnLength() != pRefColInfo->GetColumnLength())
	{
		AfxMessageBox(_TB("The selected segments refer to fields of type String having different lengths."));
		return;
	}

	CString strFKFixedValue;
	m_edFKFixedValue.GetWindowText(strFKFixedValue);

	m_SegmentsListCtrl.InsertNewSegment(pFKColInfo->GetColumnName(), pRefColInfo->GetColumnName(), (LPCTSTR)strFKFixedValue);
	GetDlgItem(IDC_XREFSEG_ADD_SEGMENT_BTN)->EnableWindow(FALSE);
}

//---------------------------------------------------------------------------------
void CXRefSegmentsPage::OnRemoveSegment()
{
	m_SegmentsListCtrl.RemoveSelectedRow();
}

//----------------------------------------------------------------------------
LRESULT CXRefSegmentsPage::OnWizardNext()
{
	if(m_SegmentsListCtrl.GetRowCount() <= 0)
	{
		AfxMessageBox(_TB("To continue you must add at least one key segment."));
		return IDD_XREF_WIZ_SEGMENT;
	}
	
	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
		return CXRefWizardPage::OnWizardNext();

	return IDD_XREF_WIZ_FINISH_PAGE;
}

//--------------------------------------------------------------------
void CXRefSegmentsPage::FillExtRefDBTs ()
{
	CString nsDbt = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetReferencedDBTNs().ToString();

	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetReferencedDBTList(m_RefDBTs);
	
	int nIdx = 0, nCurrentSelIdx = 0;
	
	CXMLDBTData* pData;
	for (int i=0; i <= m_RefDBTs.GetUpperBound(); i++)
	{
		pData = (CXMLDBTData*) m_RefDBTs.GetAt(i);
		if (!pData)
			continue;
		
		nIdx = m_cbxDBTs.AddString(pData->m_strTitle.IsEmpty () ? pData->m_strNs: pData->m_strTitle);
		m_cbxDBTs.SetItemData (nIdx, (DWORD) pData);

		if (!nsDbt.IsEmpty() && pData->m_strNs.CompareNoCase(nsDbt) == 0)
			nCurrentSelIdx = nIdx;
	}

	// if not current, i select the master
	if (nCurrentSelIdx >= 0)
		m_cbxDBTs.SetCurSel(nCurrentSelIdx);
	else
		m_cbxDBTs.SetCurSel(0);
}

/////////////////////////////////////////////////////////////////////////////
// CXRefUniversalKeyPage property page implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CXRefUniversalKeyPage, CXRefWizardPage)

BEGIN_MESSAGE_MAP(CXRefUniversalKeyPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefUniversalKeyPage)
	ON_LBN_SELCHANGE(IDC_XREF_WIZ_UK_LIST, FillUkSegmentListBox)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CXRefUniversalKeyPage::CXRefUniversalKeyPage() 
	: 
	CXRefWizardPage	(IDD_XREF_WIZ_UNIVERSAL_KEY),
	m_Image			(IDB_XREF_WIZARD_UK)
{
}

//--------------------------------------------------------------------
BOOL CXRefUniversalKeyPage::OnInitDialog()
{
	CXRefWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_XREF_WIZARD_UK, this);

	if(!m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	VERIFY (m_UkListBox.SubclassDlgItem(IDC_XREF_WIZ_UK_LIST, this));
	VERIFY (m_UkSegmentListBox.SubclassDlgItem(IDC_XREF_WIZ_UK_SEGMENT, this));
	
	((CButton*)GetDlgItem(IDC_XREF_WIZ_UK_EXPORT))->SetCheck(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->IsExportData());
	GetDlgItem(IDC_XREF_WIZ_UK_FUNC_NAME)->SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->GetFunctionName());
	GetDlgItem(IDC_XREF_WIZ_UK_TABLE_NAME)->SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->GetTableName());


	if (m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
	{
		m_UkListBox.EnableWindow(FALSE);
		m_UkSegmentListBox.EnableWindow(FALSE);
		GetDlgItem(IDC_XREF_WIZ_UK_EXPORT)->EnableWindow(FALSE);
		GetDlgItem(IDC_XREF_WIZ_UK_FUNC_NAME)->EnableWindow(FALSE);
		GetDlgItem(IDC_XREF_WIZ_UK_TABLE_NAME)->EnableWindow(FALSE);
	}
	
	return TRUE;
}

//--------------------------------------------------------------------
void CXRefUniversalKeyPage::FillUkListBox()
{
	CXMLUniversalKeyGroup* pUKGroup = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup;
	if(!pUKGroup)
	{
		ASSERT(FALSE);
		return;
	}

	m_UkListBox.ResetContent();
	m_UkSegmentListBox.ResetContent();

	for(int i = 0; i < pUKGroup->GetSize() ; i++)
	{
		CXMLUniversalKey* pUk = pUKGroup->GetAt(i);
		if(!pUk)
		{
			ASSERT(FALSE);
			continue;
		}

		int nInsIdx = m_UkListBox.AddString(pUk->GetName());
		m_UkListBox.SetItemData(nInsIdx, (DWORD)pUk);
	}
}

//--------------------------------------------------------------------
void CXRefUniversalKeyPage::FillUkSegmentListBox()
{	
	m_UkSegmentListBox.ResetContent();

	int nSel = -1;
	for(int i = 0; i < m_UkListBox.GetCount() ; i++)
	{
		if(m_UkListBox.GetSel(i) ==1)
		{
			nSel = i;
			break;
		}
	}

	if(nSel == -1)
		return;

	CXMLUniversalKey* pUK = (CXMLUniversalKey*)m_UkListBox.GetItemData(nSel);
	if(!pUK)
	{
		ASSERT(FALSE);
		return;
	}

	for(int n = 0 ; n < pUK->GetSegmentNumber() ; n++)
		m_UkSegmentListBox.AddString(pUK->GetSegmentAt(n));
}

//--------------------------------------------------------------------
void CXRefUniversalKeyPage::OnActivate()
{	
	CXRefWizardPage::OnActivate();

	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - Universal Key"));

	FillUkListBox();
}

//--------------------------------------------------------------------
LRESULT CXRefUniversalKeyPage::OnWizardNext()
{
	int nCheck = ((CButton*)GetDlgItem(IDC_XREF_WIZ_UK_EXPORT))->GetCheck();
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup->SetExportData(nCheck > 0);

	return CXRefWizardPage::OnWizardNext();
}

//--------------------------------------------------------------------
LRESULT CXRefUniversalKeyPage::OnWizardBack()
{
	if(m_pXRefSheet->m_bReadOnly)
		return IDD_XREF_WIZ_FIELD_PROP;

	return CXRefWizardPage::OnWizardBack();
}

//--------------------------------------------------------------------
// CXRefWizFinishPage property page implementation
//--------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXRefWizFinishPage, CXRefWizardPage)
	//{{AFX_MSG_MAP(CXRefWizFinishPage)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------
CXRefWizFinishPage::CXRefWizFinishPage() 
	: 
	CXRefWizardPage	(IDD_XREF_WIZ_FINISH_PAGE),
	m_Image			(IDB_XREF_WIZARD_SEGMENT)
{
}

//--------------------------------------------------------------------
BOOL CXRefWizFinishPage::OnInitDialog()
{
	CXRefWizardPage::OnInitDialog();

	m_Image.SubclassDlgItem(IDC_XREF_WIZARD_SEGMENT, this);

	return TRUE;
}

//--------------------------------------------------------------------
void CXRefWizFinishPage::OnActivate()
{	
	CXRefWizardPage::OnActivate();

	m_pXRefSheet->SetWindowText(_TB("Wizard External Reference - Summary"));

	CWnd* pWnd = GetDlgItem(IDC_XREF_NAME_EDIT);
	pWnd->SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetName());
	pWnd->EnableWindow(!m_pXRefSheet->m_bReadOnly && !m_pXRefSheet->m_pDocObjectInfo->IsReadOnly());

	pWnd = GetDlgItem(IDC_XREF_DATAURL_EDIT);
	pWnd->SetWindowText(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->GetUrlDati());
	pWnd->EnableWindow(!m_pXRefSheet->m_pDocObjectInfo->IsReadOnly());
}

//---------------------------------------------------------------------------
BOOL CXRefWizFinishPage::OnWizardFinish() 
{
	if (m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
		return TRUE;

	CString strName;
	GetDlgItem(IDC_XREF_NAME_EDIT)->GetWindowText(strName);
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName = strName;

	GetDlgItem(IDC_XREF_DATAURL_EDIT)->GetWindowText(strName);
	m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati = strName;

	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati.IsEmpty())
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati = m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strName + szXmlExt;
	
	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati.Right(4).CompareNoCase(szXmlExt) != 0)
		m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_strUrlDati += szXmlExt;

	return TRUE;
}

//---------------------------------------------------------------------------
LRESULT CXRefWizFinishPage::OnWizardBack() 
{
	if(!m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->IsToUse())
		return IDD_XREF_WIZ_PRESENTATION;

	if(m_pXRefSheet->m_bReadOnly || m_pXRefSheet->m_pDocObjectInfo->IsReadOnly())
	{
		if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
			return IDD_XREF_WIZ_UNIVERSAL_KEY;
		else
			return IDD_XREF_WIZ_FIELD_PROP;
	}

	if(m_pXRefSheet->m_WizardInfo.m_pXMLXRefInfo->m_pXMLUniversalKeyGroup)
		return CXRefWizardPage::OnWizardBack();

	return IDD_XREF_WIZ_SEGMENT;
}

/////////////////////////////////////////////////////////////////////////////
//		 CXRefWizMasterDlg property sheet implementation
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CXRefWizMasterDlg, CWizardMasterDialog)
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXRefWizMasterDlg, CWizardMasterDialog)
	//{{AFX_MSG_MAP(CXRefWizMasterDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//---------------------------------------------------------------------------
CXRefWizMasterDlg::CXRefWizMasterDlg(CXMLXRefInfo* pXMLXRefInfo, CXMLDocObjectInfo* pDocObjectInfo,  CXMLFieldInfoArray* pXMLFieldInfoArray/*= NULL*/, BOOL bDescription /*TRUE*/, CWnd* pParent /*=NULL*/)
	: 
	CWizardMasterDialog		(IDD_XREF_WIZARD_MASTER, pParent),
	m_pXMLFieldInfoArray	(NULL),
	m_pDocObjectInfo		(pDocObjectInfo)

{
	if(pXMLXRefInfo && pDocObjectInfo)
	{	
		// Add the property pages
		AddPage (&m_XRefPresentationPage);
		AddPage (&m_XRefFieldsSelectPage);
		AddPage (&m_XRefDocumentSelectPage);
		AddPage (&m_XRefFieldsPropPage);
		AddPage (&m_XRefSegmentSelectPage);
		AddPage (&m_XRefUniversalKeyPage);
		AddPage (&m_XRefWizFinishPage);

		m_bDescription				= bDescription;
		m_bReadOnly					= !bDescription && pXMLXRefInfo->IsOwnedByDoc();
		m_WizardInfo.m_pXMLXRefInfo = pXMLXRefInfo;
		m_pXMLFieldInfoArray		= pXMLFieldInfoArray;
	}
	else
		ASSERT(FALSE);
}

//---------------------------------------------------------------------------
// CXRefWizMasterDlg message handlers
//---------------------------------------------------------------------------
BOOL CXRefWizMasterDlg::OnInitDialog() 
{
	SetPlaceholderID(IDC_XREF_WIZ_STATIC_SHEET_RECT);

	CWizardMasterDialog::OnInitDialog();
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//---------------------------------------------------------------------------
BOOL CXRefWizMasterDlg::OnWizardFinish() 
{
	
	return TRUE;
}
