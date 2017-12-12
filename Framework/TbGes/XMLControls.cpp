#include "stdafx.h"

#include <TbGenlib\baseapp.h>

#include <TbNameSolver\IFileSystemManager.h>
#include <TbGenlibUI\TBExplorer.h>
#include <TbOleDb\oledbmng.h>

#include <TbWoormEngine\report.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "extdoc.h"
#include "xrefmanager.h"
#include "DBT.H"

#include "XMLControls.h"

//risorse
#include <TbGenlib\basefrm.hjson> //JSON AUTOMATIC UPDATE
#include "XMLControls.hjson" //JSON AUTOMATIC UPDATE


//includere come ulo include all'inizio del cpp
#include "begincpp.dex"

/****************************************************************************
			DISPLAY Driver extensions
****************************************************************************/
#ifndef CAPS1 
    #define CAPS1   94					/* use with GetDeviceCaps() */
#endif /* ifndef CAPS1 */

#ifndef C1_TRANSPARENT 
	#define C1_TRANSPARENT	0x0001
#endif /* ifndef C1_TRANSPARENT */

#ifndef NEWTRANSPARENT
    #define NEWTRANSPARENT  3           /* use with SetBkMode() */
#endif  /* ifndef NEWTRANSPARENT */

static const TCHAR szUKGroupName	[] = _T("Universal Key Group");

#define TYPE_POS 1
#define REPORT_POS 2

//----------------------------------------------------------------------------------------------
//	CDBTFields implementation
//----------------------------------------------------------------------------------------------
//===========================================================================
class CDBTFields : public CObject
{
public:
	CXMLDBTInfo*		m_pDBTInfo;
	BOOL				m_bIsXRefDBT;
	DataTypeNamedArray	m_arRecordFields;

public:
	CDBTFields(CXMLDBTInfo* pDBTInfo, BOOL bIsXRefDBT = FALSE);

public:
	CString			GetTitle() const	{ return m_pDBTInfo->GetTitle(); }
	CString			GetName() const		{ return m_pDBTInfo->GetNamespace().GetObjectNameForTag(); }
	void			AddField(const CString& strName, DataType aDataType) { m_arRecordFields.Add(strName, aDataType); }

public:
	CString			GetFieldName(const CString& strFieldName) const;
};

//===========================================================================
CDBTFields::CDBTFields(CXMLDBTInfo* pDBTInfo, BOOL bIsXRefDBT /*= FALSE*/)
: 
	m_pDBTInfo		(pDBTInfo), 
	m_bIsXRefDBT	(bIsXRefDBT)
{
}
//===========================================================================
CString	CDBTFields::GetFieldName(const CString& strFieldName) const
{
	CString strDBTName = GetName();
	if (m_pDBTInfo->GetType() == CXMLDBTInfo::BUFFERED_TYPE)
		strDBTName += _T("/") + strDBTName + XML_ROW_TAG;
	return cwsprintf(_T("{0-%s}/{1-%s}"), strDBTName, strFieldName);
}

//===========================================================================
class CReportFields : public CObject
{
public:
	DataTypeNamedArray* m_pReportColumns;
	DataTypeNamedArray* m_pReportAskFields;

public:
	CReportFields()
		:
		m_pReportColumns	(new DataTypeNamedArray()),
		m_pReportAskFields	(new DataTypeNamedArray())
	{ }

	~CReportFields()
	{
		delete m_pReportColumns;
		delete m_pReportAskFields;
	}
};

//----------------------------------------------------------------------------------------------
//	CHKLFieldDlg
//----------------------------------------------------------------------------------------------
//
IMPLEMENT_DYNAMIC(CHKLFieldDlg, CLocalizableDialog)
BEGIN_MESSAGE_MAP(CHKLFieldDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CHKLFieldDlg)
	ON_CBN_SELCHANGE(IDC_HKL_DBTS_LIST,		OnDBTChange)
	ON_CBN_SELCHANGE(IDC_HKL_REPFIELDS_LIST,OnDBTChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CHKLFieldDlg::CHKLFieldDlg(CXMLHKLFieldArray::HKLListType eListType, CWnd* pWndParent)
	:
	CLocalizableDialog	(IDD_DBT_HKL_FIELDS, pWndParent),
	m_eListType			(eListType)
{	
}

//----------------------------------------------------------------------------------------------
BOOL CHKLFieldDlg::OnInitDialog()
{
	if (!m_pParentWnd || !m_pParentWnd->IsKindOf(RUNTIME_CLASS(CSingleHKLDlg)))
		return FALSE;

	m_RepFieldsCombo.	SubclassDlgItem	(IDC_HKL_REPFIELDS_LIST,	this);
	m_DBTsCombo.		SubclassDlgItem	(IDC_HKL_DBTS_LIST,			this);
	m_DocFieldsCombo.	SubclassDlgItem	(IDC_HKL_DOCFIELDS_LIST,	this);

	DataTypeNamedArray* pReportFields = (m_eListType ==  CXMLHKLFieldArray::FILTER_TYPE)
						?((CSingleHKLDlg*)m_pParentWnd)->m_pReportFields->m_pReportAskFields
						: ((CSingleHKLDlg*)m_pParentWnd)->m_pReportFields->m_pReportColumns;
	CObArray* pDocumentFields = ((CSingleHKLDlg*)m_pParentWnd)->m_pDocumentFields;
	if(
			(!pReportFields || pReportFields->GetSize() <= 0) ||
			(m_eListType !=  CXMLHKLFieldArray::PREVIEW_TYPE && (!pDocumentFields || pDocumentFields->GetSize() <= 0))		
		)
	{
		AfxMessageBox(_TB("The choosed report or the document do not have useful fields for creating the hotlink."));
		OnCancel();
		return FALSE;

	}
	CString strExplainText;
	switch (m_eListType)
	{
		case CXMLHKLFieldArray::PREVIEW_TYPE:
			strExplainText = _TB("Select the fields using as preview in the Magic Pane"); break;
			
		case CXMLHKLFieldArray::FILTER_TYPE:
			strExplainText = _TB("Select the other filter fields used by Magic Document"); break;
			
		case CXMLHKLFieldArray::RESULT_TYPE:
			strExplainText = _TB("Connect the report returned values with the Magic Document fields"); break;
	}
	GetDlgItem(IDC_HKL_EXPLAIN_STATIC)->SetWindowText(strExplainText);

	InitCombos(pReportFields, pDocumentFields);	

	return CLocalizableDialog::OnInitDialog();;
}

//-----------------------------------------------------------------------------------
void CHKLFieldDlg::InitCombos(DataTypeNamedArray* pReportColumns, CObArray* pDocumentFields)
{
	CXMLHKLField* pHKLField = ((CSingleHKLDlg*)m_pParentWnd)->m_pCurrHKLField;

	for(int i = 0 ; i < pReportColumns->GetSize() ; i++)
	{
		DataTypeNamedArray::DataTypeNamed* pData = pReportColumns->GetAt(i);
		if (!pData)
			continue;

		int nSel = m_RepFieldsCombo.AddString(pData->m_strName);
		m_RepFieldsCombo.SetItemData(nSel, (DWORD)pData);
		if (pData->m_strName.CompareNoCase(pHKLField->GetReportField()) == 0)
			m_RepFieldsCombo.SetCurSel(nSel);
	}

	if (m_RepFieldsCombo.GetCurSel() == -1 && m_RepFieldsCombo.GetCount() > 0)
		m_RepFieldsCombo.SetCurSel(0);
	
	if (m_eListType == CXMLHKLFieldArray::PREVIEW_TYPE)
	{
		GetDlgItem(IDC_HKL_DBT_STATIC)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_HKL_FIELD_STATIC)->ShowWindow(SW_HIDE);
		m_DBTsCombo.ShowWindow(SW_HIDE);
		m_DocFieldsCombo.ShowWindow(SW_HIDE);
		return;
	}

	int nDBT = -1;
	BOOL bSetBanner = TRUE;
	CDBTFields* pFields = NULL;
	for(int i = 0 ; i < pDocumentFields->GetSize() ; i++)
	{
		pFields = (CDBTFields*)pDocumentFields->GetAt(i);
		if (!pFields)
			continue;

		if (pFields->m_bIsXRefDBT && bSetBanner)
		{
			m_DBTsCombo.AddString(_TB("--- External Reference DBTs ---"));
			bSetBanner = FALSE;
		}


		nDBT = m_DBTsCombo.AddString(pFields->GetTitle());
		m_DBTsCombo.SetItemData(nDBT, (DWORD)pFields);
		if (pHKLField->GetDocumentField().Find(pFields->GetName()) >= 0 && (pHKLField->IsXRefField() == pFields->m_bIsXRefDBT))
			m_DBTsCombo.SetCurSel(nDBT);
	}
	
	if (m_DBTsCombo.GetCurSel() == -1 && m_DBTsCombo.GetCount() > 0)
		m_DBTsCombo.SetCurSel(0);

	OnDBTChange();
}

//-----------------------------------------------------------------------------------
void CHKLFieldDlg::OnDBTChange()
{
	if (m_eListType ==  CXMLHKLFieldArray::PREVIEW_TYPE)
		return;
	CXMLHKLField* pHKLField = ((CSingleHKLDlg*)m_pParentWnd)->m_pCurrHKLField;
	CDBTFields* pFields = NULL;
	DataTypeNamedArray::DataTypeNamed* pRepField = 0;
	m_DocFieldsCombo.ResetContent();

	int nDBTSel = m_DBTsCombo.GetCurSel();
	int nRepSel = m_RepFieldsCombo.GetCurSel();

	if (
			(nRepSel < 0 || !(pRepField = (DataTypeNamedArray::DataTypeNamed*)m_RepFieldsCombo.GetItemData(nRepSel)))||
			(m_eListType !=  CXMLHKLFieldArray::PREVIEW_TYPE && (nDBTSel < 0 || !(pFields = (CDBTFields*)m_DBTsCombo.GetItemData(nDBTSel))))
		)
	{
		m_DocFieldsCombo.EnableWindow(FALSE);
		GetDlgItem(IDOK)->EnableWindow(FALSE);
		return;
	}
	
	DataTypeNamedArray::DataTypeNamed* pRecField = NULL;
	int nField = -1;
	if (pFields)
	{
		for (int i = 0; i < pFields->m_arRecordFields.GetSize(); i++)
		{
			pRecField = pFields->m_arRecordFields.GetAt(i);
			if (!pRecField || pRepField->m_DataType != pRecField->m_DataType)
				continue;
			
			nField = m_DocFieldsCombo.AddString(pRecField->m_strName);
			m_DocFieldsCombo.SetItemData(nField, (DWORD)pRecField);
			if (pHKLField->GetDocumentField().Find(URL_SLASH_CHAR + pRecField->m_strName) >= 0)
				m_DocFieldsCombo.SetCurSel(nField);
		}
	}
	if (m_DocFieldsCombo.GetCount() <= 0)
	{
		AfxMessageBox(cwsprintf(
							_TB("In DBT {0-%s} do not exist fields with the same type {1-%s} of the report field {2-%s}"),
							pFields->GetTitle(), ::FromDataTypeToDescr(pRepField->m_DataType), pRepField->m_strName
							));
		m_DocFieldsCombo.EnableWindow(FALSE);
		GetDlgItem(IDOK)->EnableWindow(FALSE);
	}
	else
	{
		if (m_DocFieldsCombo.GetCurSel() == -1)
			m_DocFieldsCombo.SetCurSel(0);
		
		m_DocFieldsCombo.EnableWindow(TRUE);
		GetDlgItem(IDOK)->EnableWindow(TRUE);
	}	
}

//-----------------------------------------------------------------------------------
void CHKLFieldDlg::OnOK()
{
	CXMLHKLField* pHKLField = ((CSingleHKLDlg*)m_pParentWnd)->m_pCurrHKLField;
	int nRepSel = m_RepFieldsCombo.GetCurSel();
	if (nRepSel < 0 )
	{
		AfxMessageBox(_TB("Some fields are empty. No saving action is possible."));
		return;
	}
	pHKLField->SetReportField(((DataTypeNamedArray::DataTypeNamed*)m_RepFieldsCombo.GetItemData(nRepSel))->m_strName);

	if (m_eListType !=  CXMLHKLFieldArray::PREVIEW_TYPE)
	{
		int nDBTSel = m_DBTsCombo.GetCurSel();
		int nDocFieldSel = m_DocFieldsCombo.GetCurSel();
		if (nDBTSel < 0 || nDocFieldSel < 0)
		{
			AfxMessageBox(_TB("Some fields are empty. No saving action is possible."));
			return;
		}
		CDBTFields* pDBTFields = (CDBTFields*)m_DBTsCombo.GetItemData(nDBTSel);
		if (!pDBTFields)
			return;
		CString strDBTName = pDBTFields->GetName();
		pHKLField->SetXRefField(pDBTFields->m_bIsXRefDBT);
		if (pDBTFields->m_pDBTInfo->m_eType == CXMLDBTInfo::BUFFERED_TYPE)
			pHKLField->SetDocumentField(cwsprintf(_T("{0-%s}/{1-%s}/{2-%s}"), 
									strDBTName,
									strDBTName + XML_ROW_TAG,
									((DataTypeNamedArray::DataTypeNamed*)m_DocFieldsCombo.GetItemData(nDocFieldSel))->m_strName
									));
		else
			pHKLField->SetDocumentField(cwsprintf(_T("{0-%s}/{1-%s}"), 
									strDBTName,
									((DataTypeNamedArray::DataTypeNamed*)m_DocFieldsCombo.GetItemData(nDocFieldSel))->m_strName
									));
	}
	
	__super::OnOK();
}

//----------------------------------------------------------------------------------------------
//	CSingleHKLDlg
//----------------------------------------------------------------------------------------------
CSingleHKLDlg::CSingleHKLDlg(CWnd* pWndParent)
	:
	CLocalizableDialog	(IDD_DBT_SINGLE_HOTLINK, pWndParent),
	m_pReportFields		(new CReportFields),
	m_pCurrHKLField		(NULL),
	m_pHKLInfo			(NULL),
	m_pHKLInfoToModify	(NULL),
	m_pDocumentFields	(NULL)
{	
	if (m_pParentWnd && m_pParentWnd->IsKindOf(RUNTIME_CLASS(CHotKeyLinkDlg)))
		m_pHKLInfo = new CXMLHotKeyLink(*((CHotKeyLinkDlg*)m_pParentWnd)->m_pCurrHKLInfo);
}

//----------------------------------------------------------------------------------------------
CSingleHKLDlg::~CSingleHKLDlg()
{
	SAFE_DELETE(m_pHKLInfo);
	SAFE_DELETE(m_pReportFields);
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSingleHKLDlg, CLocalizableDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSingleHKLDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CSingleHKLDlg)
	ON_BN_CLICKED(IDC_HKL_PREVIEW_ADD,		OnAddPreview)
	ON_BN_CLICKED(IDC_HKL_FILTER_ADD,		OnAddFilter)
	ON_BN_CLICKED(IDC_HKL_RESULT_ADD,		OnAddResult)

	ON_BN_CLICKED(IDC_HKL_PREVIEW_REMOVE,	OnRemovePreview)
	ON_BN_CLICKED(IDC_HKL_FILTER_REMOVE,	OnRemoveFilter)
	ON_BN_CLICKED(IDC_HKL_RESULT_REMOVE,	OnRemoveResult)

	ON_NOTIFY	(LVN_ITEMCHANGED,	IDC_HKL_PREVIEWS_ASSOCIATION,	OnSelectPreview)
	ON_NOTIFY	(LVN_ITEMCHANGED,	IDC_HKL_FILTERS_ASSOCIATION,	OnSelectFilter)
	ON_NOTIFY	(LVN_ITEMCHANGED,	IDC_HKL_RESULTS_ASSOCIATION,	OnSelectResult)

	ON_NOTIFY	(NM_DBLCLK,			IDC_HKL_PREVIEWS_ASSOCIATION,	OnModifyPreview)
	ON_NOTIFY	(NM_DBLCLK,			IDC_HKL_FILTERS_ASSOCIATION,	OnModifyFilter)
	ON_NOTIFY	(NM_DBLCLK,			IDC_HKL_RESULTS_ASSOCIATION,	OnModifyResult)
	

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
BOOL CSingleHKLDlg::OnInitDialog()
{
	if (!m_pHKLInfo || m_pHKLInfo->GetReportNamespace().IsEmpty() || !m_pParentWnd || !m_pParentWnd->IsKindOf(RUNTIME_CLASS(CHotKeyLinkDlg)))
		return FALSE;
	m_pDocumentFields = ((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocumentFields;
	CTBNamespace nsReport = m_pHKLInfo->GetReportNamespace();
	if (
			!AfxGetTbCmdManager()->GetSchemaReportVariables(nsReport, *m_pReportFields->m_pReportColumns, *m_pReportFields->m_pReportAskFields)
			||
			!InitCombos()
		)
	{
		ASSERT(FALSE);
		OnCancel();
		return FALSE;
	}

	InitListCtrls();

	return CLocalizableDialog::OnInitDialog();
}

//-----------------------------------------------------------------------------------
void CSingleHKLDlg::InitSingleCombo(CComboBox* pCombo, DataTypeNamedArray* pFieldsArray, const CString& strSelectedField)
{
	//the IDC_HKL_TEXTBOXFILTER combo must be fill with the report's ask dialog variables
	if(!pFieldsArray || pFieldsArray->GetSize() <= 0)
	{
		pCombo->EnableWindow(FALSE);
		return;
	}

	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
	{
		if (!strSelectedField.IsEmpty())
		{
			pCombo->AddString(strSelectedField);
			pCombo->SetCurSel(0);
		}
		pCombo->EnableWindow(FALSE);
		return;
	}

	if (pCombo != &m_DescriptionCombo)
		pCombo->AddString(_T(""));

	for(int i = 0 ; i < pFieldsArray->GetSize() ; i++)
	{
		DataTypeNamedArray::DataTypeNamed* pData = pFieldsArray->GetAt(i);
		if (!pData)
			continue;

		int nSel = pCombo->AddString(pData->m_strName);
		pCombo->SetItemData(nSel, (DWORD)pData);
		if (pData->m_strName.CompareNoCase(strSelectedField) == 0)
			pCombo->SetCurSel(nSel);
	}

}

//-----------------------------------------------------------------------------------
BOOL  CSingleHKLDlg::InitCombos()
{
	if (
			m_pReportFields->m_pReportAskFields->GetCount() == 0 &&
			m_pReportFields->m_pReportColumns->GetCount() == 0 
		)
	{
		AfxMessageBox(_TB("The choosed report or the document do not have useful fields for creating the hotlink."));
		return FALSE;
	}

	m_TextBoxCombo.		SubclassDlgItem	(IDC_HKL_TEXTBOXFILTER,		this);
	m_DescriptionCombo.	SubclassDlgItem	(IDC_HKL_DESCRIPTION,		this);
	m_ImageCombo.		SubclassDlgItem	(IDC_HKL_IMAGE,				this);

	InitSingleCombo(&m_TextBoxCombo, m_pReportFields->m_pReportAskFields, m_pHKLInfo->m_strTextBoxField);
	InitSingleCombo(&m_DescriptionCombo, m_pReportFields->m_pReportColumns, m_pHKLInfo->m_strDescriptionField);
	InitSingleCombo(&m_ImageCombo,m_pReportFields->m_pReportColumns, m_pHKLInfo->m_strImageField);

	return TRUE;
}

//-----------------------------------------------------------------------------------
void CSingleHKLDlg::InitSingleListCtrls(CListCtrl* pListCtrl, CXMLHKLFieldArray* pFieldArray, CXMLHKLFieldArray::HKLListType eListType)
{
	pListCtrl->InsertColumn(0, _TB("Report field"), LVCFMT_LEFT, 200);
	if (eListType != CXMLHKLFieldArray::PREVIEW_TYPE)
		pListCtrl->InsertColumn(1, _TB("Document field"), LVCFMT_LEFT, 400);

	if(!pFieldArray || pFieldArray->GetSize() <= 0)
	{
		pListCtrl->EnableWindow(FALSE);
		return;
	}	

	for(int i = 0 ; i < pFieldArray->GetSize() ; i++)
	{
		CXMLHKLField* pHKLField = pFieldArray->GetAt(i);
		if(!pHKLField)
		{
			ASSERT(FALSE);
			continue;
		}

		int nItem = pListCtrl->InsertItem(pListCtrl->GetItemCount(), pHKLField->GetReportField());
		if (eListType != CXMLHKLFieldArray::PREVIEW_TYPE)
			pListCtrl->SetItemText(nItem, 1, pHKLField->GetDocumentField());
		pListCtrl->SetItemData(nItem, (DWORD)pHKLField);
	}
}

//-----------------------------------------------------------------------------------
void CSingleHKLDlg::InitListCtrls()
{
	m_PreviewFieldsList.SubclassDlgItem	(IDC_HKL_PREVIEWS_ASSOCIATION,	this);
	m_FilterFieldsList.	SubclassDlgItem	(IDC_HKL_FILTERS_ASSOCIATION,	this);
	m_ResultFieldsList.	SubclassDlgItem	(IDC_HKL_RESULTS_ASSOCIATION,	this);
	
	InitSingleListCtrls(&m_PreviewFieldsList,&m_pHKLInfo->m_arPreviewFields, CXMLHKLFieldArray::PREVIEW_TYPE);
	InitSingleListCtrls(&m_FilterFieldsList, &m_pHKLInfo->m_arFilterFields,  CXMLHKLFieldArray::FILTER_TYPE);
	InitSingleListCtrls(&m_ResultFieldsList, &m_pHKLInfo->m_arResultFields,  CXMLHKLFieldArray::RESULT_TYPE);

	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
	{
		GetDlgItem(IDC_HKL_PREVIEW_ADD)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_HKL_FILTER_ADD)->ShowWindow(SW_HIDE);	
		GetDlgItem(IDC_HKL_RESULT_ADD)->ShowWindow(SW_HIDE);	
		GetDlgItem(IDC_HKL_PREVIEW_REMOVE)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_HKL_FILTER_REMOVE)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_HKL_RESULT_REMOVE)->ShowWindow(SW_HIDE);
	}			
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnModifyAssociation(CListCtrl* pListCtrl, CXMLHKLFieldArray* pFieldArray, CXMLHKLFieldArray::HKLListType eListType)
{
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
		return;
	
	int nItem = pListCtrl->GetNextItem(-1, LVNI_SELECTED);
	if (nItem < 0)
		return;

	m_pCurrHKLField = (CXMLHKLField*)pListCtrl->GetItemData(nItem);
	
	CHKLFieldDlg aFieldDlg(eListType, this);
	if (aFieldDlg.DoModal() == IDOK)
	{
		pListCtrl->SetItemText(nItem, 0, m_pCurrHKLField->GetReportField());
		if (eListType != CXMLHKLFieldArray::PREVIEW_TYPE)
			pListCtrl->SetItemText(nItem, 1, m_pCurrHKLField->GetDocumentField());
		
		pListCtrl->SetItemState(nItem, LVIS_SELECTED, LVIS_SELECTED);
		pListCtrl->SetFocus();
	}	
	
	pListCtrl->EnableWindow(pListCtrl->GetItemCount());
}


//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnAddAssociation(CListCtrl* pListCtrl, CXMLHKLFieldArray* pFieldArray, CXMLHKLFieldArray::HKLListType eListType)
{	
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
		return;

	m_pCurrHKLField = new CXMLHKLField();
	
	CHKLFieldDlg aFieldDlg(eListType, this);
	if (aFieldDlg.DoModal() == IDOK)
	{
		int nItem = pListCtrl->InsertItem(pListCtrl->GetItemCount(), m_pCurrHKLField->GetReportField());	
		if (eListType != CXMLHKLFieldArray::PREVIEW_TYPE)
			pListCtrl->SetItemText(nItem, 1, m_pCurrHKLField->GetDocumentField());
		pListCtrl->SetItemData(nItem, (DWORD)m_pCurrHKLField);
		pFieldArray->Add(m_pCurrHKLField);
		pListCtrl->SetItemState(nItem, LVIS_SELECTED, LVIS_SELECTED);
		pListCtrl->SetFocus();
	}	
	else
		SAFE_DELETE(m_pCurrHKLField);

	pListCtrl->EnableWindow(pListCtrl->GetItemCount());
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnAddPreview()
{
	OnAddAssociation(&m_PreviewFieldsList, &m_pHKLInfo->m_arPreviewFields, CXMLHKLFieldArray::PREVIEW_TYPE);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnAddFilter()
{
	OnAddAssociation(&m_FilterFieldsList, &m_pHKLInfo->m_arFilterFields, CXMLHKLFieldArray::FILTER_TYPE);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnAddResult()
{
	OnAddAssociation(&m_ResultFieldsList, &m_pHKLInfo->m_arResultFields, CXMLHKLFieldArray::RESULT_TYPE);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnRemoveAssociation(CListCtrl* pListCtrl, CXMLHKLFieldArray* pFieldArray)
{
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
		return;

	int nItem = pListCtrl->GetNextItem(-1, LVNI_SELECTED);
	if (nItem < 0)
		return;
    
	if(AfxMessageBox(_TB("Are you sure to remove this association?"), MB_OKCANCEL) == IDOK)
	{
		m_pCurrHKLField = (CXMLHKLField*)pListCtrl->GetItemData(nItem);
		pFieldArray->Remove(m_pCurrHKLField);
		m_pCurrHKLField = NULL;
        pListCtrl->DeleteItem(nItem);
	}
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnRemovePreview()
{
	OnRemoveAssociation(&m_PreviewFieldsList, &m_pHKLInfo->m_arPreviewFields);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnRemoveFilter()
{
	OnRemoveAssociation(&m_FilterFieldsList, &m_pHKLInfo->m_arFilterFields);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnRemoveResult()
{
	OnRemoveAssociation(&m_ResultFieldsList, &m_pHKLInfo->m_arResultFields);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnSelectPreview(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
		return;

	GetDlgItem(IDC_HKL_PREVIEW_REMOVE)->EnableWindow(TRUE);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnSelectFilter(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
		return;

	GetDlgItem(IDC_HKL_FILTER_REMOVE)->EnableWindow(TRUE);
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnSelectResult(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
		return;

	GetDlgItem(IDC_HKL_RESULT_REMOVE)->EnableWindow(TRUE);
}	

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnModifyPreview(NMHDR *pNMHDR, LRESULT *pResult)
{
	OnModifyAssociation(&m_PreviewFieldsList, &m_pHKLInfo->m_arPreviewFields, CXMLHKLFieldArray::PREVIEW_TYPE);	
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnModifyFilter(NMHDR *pNMHDR, LRESULT *pResult)
{
	OnModifyAssociation(&m_FilterFieldsList, &m_pHKLInfo->m_arFilterFields, CXMLHKLFieldArray::FILTER_TYPE);	
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnModifyResult(NMHDR *pNMHDR, LRESULT *pResult)
{
	OnModifyAssociation(&m_ResultFieldsList, &m_pHKLInfo->m_arResultFields, CXMLHKLFieldArray::RESULT_TYPE);	
}

//----------------------------------------------------------------------------------------------
void CSingleHKLDlg::OnOK()
{	
	if (((CHotKeyLinkDlg*)m_pParentWnd)->m_pDocObjectInfo->IsReadOnly())
	{
		__super::OnOK();
		return;
	}

	int nTextBoxSel = m_TextBoxCombo.GetCurSel();
	int nDescriSel = m_DescriptionCombo.GetCurSel();
	int nImageSel = m_ImageCombo.GetCurSel();

	(nTextBoxSel < 0) ? _T("") : m_TextBoxCombo.GetWindowText(m_pHKLInfo->m_strTextBoxField);
	if (nDescriSel < 0)
	{
		AfxMessageBox(cwsprintf(_TB("The field Description is compulsory. Please specify it!")));
		return;
	}
		
	m_DescriptionCombo.GetWindowText(m_pHKLInfo->m_strDescriptionField);
	(nImageSel < 0) ? _T("") : m_ImageCombo.GetWindowText(m_pHKLInfo->m_strImageField);
	
	*((CHotKeyLinkDlg*)m_pParentWnd)->m_pCurrHKLInfo = *m_pHKLInfo; 
	__super::OnOK();
}

//----------------------------------------------------------------------------------------------
//	CHotKeyLinkDlg
//----------------------------------------------------------------------------------------------
CHotKeyLinkDlg::CHotKeyLinkDlg(
									CXMLDBTInfo*		pDbtInfo, 
									CXMLDocObjectInfo*	pDocObjectInfo, 
									CXMLXRefInfo*		pXRefInfo /*= NULL*/, 
									CXMLDocObjectInfo*	pExtDocObjectInfo /*= NULL*/, 									
									CWnd*				pWndParent /*= NULL*/
								)
	:
	CLocalizableDialog(IDD_DBT_HOTLINKS, pWndParent),
	m_pRecord			(NULL),
	m_pDbtInfo			(pDbtInfo),
	m_pXRefInfo			(pXRefInfo),
	m_pDocObjectInfo	(pDocObjectInfo),
	m_pExtDocObjectInfo	(pExtDocObjectInfo),
	m_pCurrHKLInfo		(NULL),
	m_pDocumentFields   (NULL)
{	
}

//----------------------------------------------------------------------------------------------
CHotKeyLinkDlg::~CHotKeyLinkDlg()
{
	SAFE_DELETE(m_pRecord);
	SAFE_DELETE(m_pDocumentFields);
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CHotKeyLinkDlg, CLocalizableDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CHotKeyLinkDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CFixedFieldDlg)
	ON_CBN_SELCHANGE(IDC_HKL_FIELD_COMBO,	OnFieldChange)
	ON_CBN_SELCHANGE(IDC_HKL_DBT_COMBO,		OnDBTChange)
	ON_BN_CLICKED	(IDC_EDIT_HKL,			OnEditHKL)
	ON_BN_CLICKED	(IDC_REMOVE_HKL,		OnRemoveHKL)
	ON_BN_CLICKED	(IDC_HKL_ADD_REPORT,	OnAssociateReport)
	ON_BN_CLICKED	(IDC_HKL_XREF_RADIO,	OnHKLTypeChanged)
	ON_BN_CLICKED	(IDC_HKL_DBT_RADIO,	OnHKLTypeChanged)
	ON_BN_CLICKED	(IDC_HKL_FIELD_RADIO,	OnHKLTypeChanged)
	ON_NOTIFY		(LVN_ITEMCHANGED,	IDC_HKL_LIST,	OnSelectHKL)
	ON_NOTIFY		(NM_DBLCLK,			IDC_HKL_LIST,	OnDBLClickHKL)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//----------------------------------------------------------------------------------------------
BOOL CHotKeyLinkDlg::OnInitDialog()
{
	m_DBTsCombo.		SubclassDlgItem	(IDC_HKL_DBT_COMBO,		this);
	m_FieldsCombo.		SubclassDlgItem	(IDC_HKL_FIELD_COMBO,	this);
	m_HotKeyLinkList.	SubclassDlgItem	(IDC_HKL_LIST,			this);
	m_XRefRadioBtn.		SubclassDlgItem	(IDC_HKL_XREF_RADIO,	this);
	m_DbtRadioBtn.		SubclassDlgItem	(IDC_HKL_DBT_RADIO,		this);
	m_FieldRadioBtn.	SubclassDlgItem	(IDC_HKL_FIELD_RADIO,	this);
	
	if (!InitDocumentFields())
		return FALSE;	

	m_HotKeyLinkList.InsertColumn(0, _TB("Name filed"), LVCFMT_LEFT, 300);
	m_HotKeyLinkList.InsertColumn(TYPE_POS, _TB("Type"), LVCFMT_LEFT, 70);
	m_HotKeyLinkList.InsertColumn(REPORT_POS, _TB("Linked report"), LVCFMT_LEFT, 400);

	if (m_pDbtInfo->m_pXMLHotKeyLinkArray == NULL)
		m_pDbtInfo->m_pXMLHotKeyLinkArray = new CXMLHotKeyLinkArray();
	
	BOOL bEnable = !m_pDocObjectInfo->IsReadOnly();
	if (!m_pXRefInfo)
	{	
		m_XRefRadioBtn.EnableWindow(FALSE);
		m_DBTsCombo.EnableWindow(FALSE);	
	}
	else
		m_XRefRadioBtn.EnableWindow(bEnable);	

	m_XRefRadioBtn.SetCheck(m_pXRefInfo != NULL);
	m_DbtRadioBtn.SetCheck(m_pXRefInfo == NULL);
	
	m_FieldRadioBtn.SetCheck(FALSE);

	m_DbtRadioBtn.EnableWindow(bEnable);
	m_FieldRadioBtn.EnableWindow(bEnable);
	GetDlgItem(IDC_HKL_ADD_REPORT)->EnableWindow(bEnable);
	GetDlgItem(IDC_NS_REPORT_STATIC)->EnableWindow(bEnable);

	FillHKLList();
	FillDBTsCombo();
	OnHKLTypeChanged();

	return CLocalizableDialog::OnInitDialog();
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::LoadSingleDBTField(CXMLDBTInfo* pDBTInfo, BOOL bExtRef)
{
	if (!pDBTInfo)
		return;

	CDBTFields* pDBTFields = new CDBTFields(pDBTInfo, bExtRef);
	SqlRecord* pRecord = GetSqlRecord(pDBTInfo->m_strTableName);;
	m_pDocumentFields->Add(pDBTFields);

	if (pRecord)
	{
		for (int nColIdx = 0; nColIdx <= pRecord->GetUpperBound(); nColIdx++)
		{
			SqlRecordItem* pColumn = pRecord->GetAt(nColIdx);
			if (
					(pColumn->GetColumnInfo() && !pColumn->GetColumnInfo()->m_bVirtual) && 
					(!pDBTInfo->m_pXMLFieldInfoArray || pDBTInfo->IsFieldToExport(pColumn->GetColumnName())) &&
					!pDBTInfo->IsFieldInUsedExtRef(pColumn->GetColumnName()) 
				)
				pDBTFields->AddField(pColumn->GetColumnName(), pColumn->GetDataObj()->GetDataType());
		}

		SAFE_DELETE(pRecord);
	}	
}

//----------------------------------------------------------------------------------------------
BOOL CHotKeyLinkDlg::InitDocumentFields()
{
	if(!m_pDocObjectInfo->GetDBTInfoArray() || m_pDocObjectInfo->GetDBTInfoArray()->GetSize() <= 0 || !m_pDbtInfo)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pDocumentFields = new Array;

	for (int nIdx = 0; nIdx < m_pDocObjectInfo->GetDBTInfoArray()->GetSize(); nIdx++)
	{
		CXMLDBTInfo* pDBTInfo = m_pDocObjectInfo ->GetDBTAt(nIdx);
		if (pDBTInfo && pDBTInfo->IsToExport())
			LoadSingleDBTField(pDBTInfo, FALSE);
		
	}
	if (m_pExtDocObjectInfo && m_pExtDocObjectInfo->GetDBTInfoArray())
	{
		for (int nIdx = 0; nIdx < m_pExtDocObjectInfo->GetDBTInfoArray()->GetSize(); nIdx++)
		{
			CXMLDBTInfo* pDBTInfo = m_pExtDocObjectInfo ->GetDBTAt(nIdx);
			if (pDBTInfo && pDBTInfo->IsToExport())
				LoadSingleDBTField(pDBTInfo, TRUE);		
		}
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
SqlRecord* CHotKeyLinkDlg::GetSqlRecord(const CString& strTableName)
{	
	const SqlCatalogEntry* pSqlCatalogEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(strTableName);
	if(!pSqlCatalogEntry)
	{
			ASSERT(FALSE);
			return NULL;
	}
	return pSqlCatalogEntry->CreateRecord();
}

//-----------------------------------------------------------------------------------
void CHotKeyLinkDlg::FillHKLList()
{
	if (!m_pDbtInfo->m_pXMLHotKeyLinkArray)
		return;
		
	CXMLHotKeyLinkArray* pHKLArray = new CXMLHotKeyLinkArray;
	if (m_pXRefInfo)
		m_pDbtInfo->m_pXMLHotKeyLinkArray->GetAllHKLForType(CXMLHotKeyLink::XREF, pHKLArray, m_pXRefInfo->GetName());
	else
	{
		m_pDbtInfo->m_pXMLHotKeyLinkArray->GetAllHKLForType(CXMLHotKeyLink::DBT, pHKLArray);
		m_pDbtInfo->m_pXMLHotKeyLinkArray->GetAllHKLForType(CXMLHotKeyLink::FIELD, pHKLArray);
	}

	CXMLHotKeyLink* pXMLHKL = NULL;
	CString strFieldName;
	for(int i = 0 ; i < pHKLArray->GetSize() ; i++)
	{
		pXMLHKL = pHKLArray->GetAt(i);
	
		if(!pXMLHKL)
			continue;
		
		if (pXMLHKL->GetHKLFieldType() == CXMLHotKeyLink::XREF)
		{
			switch (pXMLHKL->GetHKLSubType())
			{
				case CXMLHotKeyLink::XREF:
					strFieldName = pXMLHKL->GetFieldName(); break;
				case CXMLHotKeyLink::DBT:
				case CXMLHotKeyLink::FIELD:
					{
						int nPos = pXMLHKL->GetFieldName().Find(URL_SLASH_CHAR);
						strFieldName = (nPos >= 0) ? pXMLHKL->GetFieldName().Right(pXMLHKL->GetFieldName().GetLength() - (nPos + 1)) : pXMLHKL->GetFieldName();
						break;
					}
			}
		}
		else
			strFieldName = (pXMLHKL->GetHKLFieldType() == CXMLHotKeyLink::DBT) ? m_pDbtInfo->GetTitle() : pXMLHKL->GetFieldName();

		int nItem = m_HotKeyLinkList.InsertItem(m_HotKeyLinkList.GetItemCount(), strFieldName);
	
		m_HotKeyLinkList.SetItemText(nItem, TYPE_POS, pXMLHKL->GetStrHKLFieldType((m_pXRefInfo) ? pXMLHKL->GetHKLSubType() : pXMLHKL->GetHKLFieldType()));
		m_HotKeyLinkList.SetItemText(nItem, REPORT_POS, pXMLHKL->GetReportNamespace().ToString());
		m_HotKeyLinkList.SetItemData(nItem, (DWORD)new CXMLHotKeyLink(*pXMLHKL));
	}

	delete pHKLArray;
}
//-----------------------------------------------------------------------------------
void CHotKeyLinkDlg::FillDBTsCombo()
{
	if (!m_pXRefInfo)
	{
		FillFieldsCombo(m_pDbtInfo);
		return;
	}

	CDBTFields* pDBTFields = NULL;
	int nItem = -1;
	for (int nIdx = 0; nIdx < m_pDocumentFields->GetSize(); nIdx++)
	{
		pDBTFields = (CDBTFields*)m_pDocumentFields->GetAt(nIdx);
		if (pDBTFields && pDBTFields->m_bIsXRefDBT)
		{
			nItem = m_DBTsCombo.AddString(pDBTFields->GetTitle());
			m_DBTsCombo.SetItemData(nItem, (DWORD)pDBTFields);
		}
	}
	if (m_DBTsCombo.GetCount() > 0)
		m_DBTsCombo.SetCurSel(0);	
}

//-----------------------------------------------------------------------------------
void CHotKeyLinkDlg::FillFieldsCombo(CXMLDBTInfo* pDBTInfo)
{	
	m_FieldsCombo.ResetContent();
	if (!pDBTInfo)
		return;

	CDBTFields* pDBTFields = NULL;
	CDBTFields* pCurrDBTFields = NULL;
	for (int nIdx = 0; nIdx < m_pDocumentFields->GetSize(); nIdx++)
	{
		pDBTFields = (CDBTFields*)m_pDocumentFields->GetAt(nIdx);
		if (pDBTFields && pDBTFields->m_pDBTInfo == pDBTInfo)
		{
			pCurrDBTFields = pDBTFields;
			break;
		}
	}

	DataTypeNamedArray::DataTypeNamed* dataType = NULL;
	for(int i = 0 ; i < pCurrDBTFields->m_arRecordFields.GetSize() ; i++)
	{
		dataType = pCurrDBTFields->m_arRecordFields.GetAt(i);
		if(!dataType)
			continue;
		int nItem = m_FieldsCombo.AddString(dataType->m_strName);
		m_FieldsCombo.SetItemData(nItem, (DWORD)pCurrDBTFields);
	}

	if (m_FieldsCombo.GetCount() > 0)
		m_FieldsCombo.SetCurSel(0);
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnHKLTypeChanged()
{
	CXMLDBTInfo* pCurrDBTInfo = m_pDbtInfo;
	m_FieldsCombo.EnableWindow(m_FieldRadioBtn.GetCheck());
	if (m_pXRefInfo)
	{
		m_DBTsCombo.EnableWindow(!m_XRefRadioBtn.GetCheck());
		pCurrDBTInfo = (m_DBTsCombo.GetCount() > 0) ? ((CDBTFields*)m_DBTsCombo.GetItemData(0))->m_pDBTInfo : NULL;
	}

	if (m_FieldRadioBtn.GetCheck())
	{
		if (!pCurrDBTInfo)
		{
			m_FieldsCombo.ResetContent();
			m_FieldsCombo.EnableWindow(FALSE);
		}
		else
			FillFieldsCombo(pCurrDBTInfo);
	}
	
	OnFieldChange();
}
//-----------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnDBTChange()
{
	int nSel = m_DBTsCombo.GetCurSel();
	if (nSel < 0)
	{
		m_FieldsCombo.ResetContent();
		return;
	}
	if (m_DbtRadioBtn.GetCheck())
		SetDlgItemText(IDC_NS_REPORT_STATIC, _T(""));
	else
	{
		CDBTFields* pDBTFields = (CDBTFields*)m_DBTsCombo.GetItemData(nSel);
		if (pDBTFields)
			FillFieldsCombo(pDBTFields->m_pDBTInfo);
	}
	OnFieldChange();
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnFieldChange()
{
	CString strFieldName, strFieldTitle;
	CXMLHotKeyLink::HKLFieldType eType = CXMLHotKeyLink::FIELD;
	GetFieldInformation(strFieldName, strFieldTitle, eType);
	if (strFieldName.IsEmpty())
		return;

	int nFieldPos = IsFieldPresent(strFieldName, eType);
	if(nFieldPos != -1)
	{
		CString strValue = m_HotKeyLinkList.GetItemText(nFieldPos, REPORT_POS);
		SetDlgItemText(IDC_NS_REPORT_STATIC, strValue);		
		m_HotKeyLinkList.SetItemState(nFieldPos, LVIS_SELECTED, LVIS_SELECTED);
		m_HotKeyLinkList.SetFocus();
	}
	else
	{
		SetDlgItemText(IDC_NS_REPORT_STATIC, _T(""));
		GetDlgItem(IDC_EDIT_HKL)->EnableWindow(FALSE);
		GetDlgItem(IDC_REMOVE_HKL)->EnableWindow(FALSE);
	}
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnAssociateReport()
{
	CTBNamespace nsReport;
	CString strNamespace;
	GetDlgItemText(IDC_NS_REPORT_STATIC, strNamespace);
	if (!strNamespace.IsEmpty())
		nsReport.SetNamespace(strNamespace);		

	if (OpenTBExplorer(nsReport))
	{
		if (!nsReport.IsEmpty() && nsReport.IsValid())
		{
			SetDlgItemText(IDC_NS_REPORT_STATIC, nsReport.ToString());
			OnAddHKL();
		}
	}
	else
		GetDlgItem(IDC_EDIT_HKL)->EnableWindow(FALSE);
}

//----------------------------------------------------------------------------------------------
BOOL CHotKeyLinkDlg::OpenTBExplorer(CTBNamespace& nsReport)
{
	CTBExplorer tbExplorer(CTBExplorer::OPEN, nsReport);
	if (!tbExplorer.Open())
		return FALSE;

	tbExplorer.GetSelNameSpace(nsReport);
	CString strNamespace = nsReport.ToString();
	//elimino l'estensione del file .wrm
	strNamespace.Replace(_T(".wrm"), _T(""));
	nsReport.SetNamespace(strNamespace);
	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::GetFieldInformation(CString& strFieldName, CString& strFieldTitle, CXMLHotKeyLink::HKLFieldType& eType)
{
	BOOL bDBT = FALSE; 
	if (m_XRefRadioBtn.GetCheck())
	{
		strFieldTitle = strFieldName = m_pXRefInfo->GetName();
		eType = CXMLHotKeyLink::XREF;
	}
	else
	{
		int nSel = -1;
		CXMLDBTInfo* pDBTInfo = m_pDbtInfo;
		if (m_DbtRadioBtn.GetCheck())
		{
			if (m_pXRefInfo)
			{
				nSel = m_DBTsCombo.GetCurSel();
				if (nSel < 0)
					return;
				CDBTFields* pDBTFields = (CDBTFields*)m_DBTsCombo.GetItemData(nSel);
				pDBTInfo = pDBTFields->m_pDBTInfo;
				strFieldName = m_pXRefInfo->GetName() + _T("/") + pDBTFields->GetName();

			}
			else
				strFieldName = m_pDbtInfo->GetNamespace().GetObjectNameForTag();
			eType = CXMLHotKeyLink::DBT;
			strFieldTitle = pDBTInfo->GetTitle();
		}
		else
		{
			nSel = m_FieldsCombo.GetCurSel();
			if (nSel < 0)
				return;
			m_FieldsCombo.GetLBText(nSel, strFieldName);
			if (m_pXRefInfo)
			{
				CString strTemp = strFieldName;		
				CDBTFields* pDBTFields = (CDBTFields*)m_FieldsCombo.GetItemData(nSel);
				strFieldName = m_pXRefInfo->GetName() + _T("/") + pDBTFields->GetFieldName(strTemp);
				strFieldTitle = pDBTFields->GetFieldName(strTemp);
			}
			else
				strFieldTitle = strFieldName;
		}
	}
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnAddHKL()
{	
	CString strFieldName, strTitle, strNamespace;
	CXMLHotKeyLink* pOldHKLInfo = NULL;

	CXMLHotKeyLink::HKLFieldType eType = CXMLHotKeyLink::FIELD;
	GetFieldInformation(strFieldName, strTitle, eType);
	if (strFieldName.IsEmpty())
		return;

	int nFieldPos = IsFieldPresent(strFieldName,eType);

	GetDlgItemText(IDC_NS_REPORT_STATIC, strNamespace);

	BOOL bNew = nFieldPos == -1;
	//non era stato ancora inserito
	if(bNew)
	{
		m_pCurrHKLInfo = new CXMLHotKeyLink();
		m_pCurrHKLInfo->SetFieldName(strFieldName);
		m_pCurrHKLInfo->SetReportNamespace(strNamespace);
		if (m_pXRefInfo)
		{
			m_pCurrHKLInfo->SetHKLFieldType(CXMLHotKeyLink::XREF);
			m_pCurrHKLInfo->SetHKLSubType(eType);
		}
		else
			m_pCurrHKLInfo->SetHKLFieldType(eType);
	}
	else
	{
		m_pCurrHKLInfo = (CXMLHotKeyLink*)m_HotKeyLinkList.GetItemData(nFieldPos);
		if (m_pCurrHKLInfo->GetReportNamespace().ToString().CompareNoCase(strNamespace) != 0)
		{
			pOldHKLInfo = m_pCurrHKLInfo;
			m_pCurrHKLInfo = new CXMLHotKeyLink();
			m_pCurrHKLInfo->SetFieldName(pOldHKLInfo->GetFieldName());
			m_pCurrHKLInfo->SetReportNamespace(strNamespace); 
			if (m_pXRefInfo)
			{
				m_pCurrHKLInfo->SetHKLFieldType(CXMLHotKeyLink::XREF);
				m_pCurrHKLInfo->SetHKLSubType(eType);
			}
			else
				m_pCurrHKLInfo->SetHKLFieldType(eType);
		}
	}

	CSingleHKLDlg aSingleHKLDlg(this);
	if (aSingleHKLDlg.DoModal() == IDOK)
	{
		if (bNew)
		{
			nFieldPos = m_HotKeyLinkList.InsertItem(m_HotKeyLinkList.GetItemCount(), strTitle);	
			m_HotKeyLinkList.SetItemText(nFieldPos, TYPE_POS, m_pCurrHKLInfo->GetStrHKLFieldType((m_pXRefInfo) ? m_pCurrHKLInfo->GetHKLSubType() : m_pCurrHKLInfo->GetHKLFieldType()));
			m_HotKeyLinkList.SetItemText(nFieldPos, REPORT_POS, m_pCurrHKLInfo->GetReportNamespace().ToString());			
			m_HotKeyLinkList.SetItemData(nFieldPos, (DWORD)m_pCurrHKLInfo);
		}
		else
		{
			if (pOldHKLInfo)
			{
				m_HotKeyLinkList.SetItemText(nFieldPos, REPORT_POS, m_pCurrHKLInfo->GetReportNamespace().ToString());
				m_HotKeyLinkList.SetItemData(nFieldPos, (DWORD)m_pCurrHKLInfo);
				SAFE_DELETE(pOldHKLInfo);
			}
		}
		m_HotKeyLinkList.SetItemState(nFieldPos, LVIS_SELECTED, LVIS_SELECTED);
		m_HotKeyLinkList.SetFocus();
	}
	else
	{
		if (bNew)
			SAFE_DELETE(m_pCurrHKLInfo);
		if (pOldHKLInfo)
		{
			SAFE_DELETE(m_pCurrHKLInfo);
			m_pCurrHKLInfo = pOldHKLInfo;
			GetDlgItem(IDC_NS_REPORT_STATIC)->SetWindowText(m_pCurrHKLInfo->GetReportNamespace().ToString());
		}
	}
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnSelectHKL(NMHDR *pNMHDR, LRESULT *pResult)
{
	POSITION pos = m_HotKeyLinkList.GetFirstSelectedItemPosition();
	GetDlgItem(IDC_EDIT_HKL)->EnableWindow((pos != NULL));
	GetDlgItem(IDC_REMOVE_HKL)->EnableWindow((pos != NULL));
}	

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnDBLClickHKL(NMHDR *pNMHDR, LRESULT *pResult)
{
	OnEditHKL();
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnEditHKL()
{
	POSITION pos = m_HotKeyLinkList.GetFirstSelectedItemPosition();
	int nItem = -1;
	if (pos != NULL)
	{
		nItem = m_HotKeyLinkList.GetNextSelectedItem(pos);
		m_pCurrHKLInfo = (CXMLHotKeyLink*)m_HotKeyLinkList.GetItemData(nItem);
		
		CSingleHKLDlg aSingleHKLDlg(this);
		if (aSingleHKLDlg.DoModal() == IDOK)
			m_HotKeyLinkList.SetItemText(nItem, REPORT_POS, m_pCurrHKLInfo->GetReportNamespace().ToString());
	}
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnRemoveHKL()
{
	int nSel = m_HotKeyLinkList.GetNextItem(-1, LVNI_SELECTED);
	if (nSel < 0)
		return;

	if(AfxMessageBox(_TB("Are you sure to remove the field-report association?"), MB_OKCANCEL) == IDOK)
	{
		m_HotKeyLinkList.DeleteItem(nSel);
		OnFieldChange();
	}
}

//----------------------------------------------------------------------------------------------
int CHotKeyLinkDlg::IsFieldPresent(const CString& strFieldName, CXMLHotKeyLink::HKLFieldType eType)
{
	for(int i = 0 ; i < m_HotKeyLinkList.GetItemCount() ; i++)
	{
		CXMLHotKeyLink* pHKLInfo = (CXMLHotKeyLink*)m_HotKeyLinkList.GetItemData(i);
		if (!pHKLInfo)
			continue;
		if(pHKLInfo->GetFieldName().CollateNoCase(strFieldName) == 0)
			return i;
	}

	return -1;
}

//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnOK()
{
	if(!m_pDbtInfo || !m_pDbtInfo->m_pXMLHotKeyLinkArray)
	{
		ASSERT(FALSE);
		return;
	}
	if(m_pXRefInfo)
		m_pDbtInfo->m_pXMLHotKeyLinkArray->RemoveOnlyForXRefHKL(m_pXRefInfo->GetName());
	else
		m_pDbtInfo->m_pXMLHotKeyLinkArray->RemoveOnlyForDBTHKL();

	CXMLHotKeyLink* pXMLHKL = NULL;
	for(int i = 0 ; i < m_HotKeyLinkList.GetItemCount() ; i++)
	{
		pXMLHKL = (CXMLHotKeyLink*)m_HotKeyLinkList.GetItemData(i);
		if (!pXMLHKL)
			continue;
		m_pDbtInfo->m_pXMLHotKeyLinkArray->Add(pXMLHKL);
	}

	__super::OnOK();
}
//----------------------------------------------------------------------------------------------
void CHotKeyLinkDlg::OnCancel()
{
	for(int i = m_HotKeyLinkList.GetItemCount()-1 ; i >= 0  ; i--)
		delete((CXMLHotKeyLink*)(m_HotKeyLinkList.GetItemData(i)));
	
	__super::OnCancel();
}

//----------------------------------------------------------------------------------------------
//	CFixedFieldDlg
//----------------------------------------------------------------------------------------------
CFixedFieldDlg::CFixedFieldDlg(CXMLDBTInfo* pDbtMaster, CWnd* pWndParent /*= NULL*/)
	:
	CLocalizableDialog(IDD_DBT_FIXED_FIELDS, pWndParent),
	m_pRecord		(NULL)
{
	m_pDbtMasterInfo = pDbtMaster;
}

//----------------------------------------------------------------------------------------------
CFixedFieldDlg::~CFixedFieldDlg()
{
	if(m_pRecord)
		delete m_pRecord;
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CFixedFieldDlg, CLocalizableDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFixedFieldDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CFixedFieldDlg)
	ON_BN_CLICKED		(IDC_FIXED_FIELD_ADD,		OnAddFixedField)
	ON_CBN_SELCHANGE	(IDC_FIELD_COMBO,			OnFieldChange)
	ON_BN_CLICKED		(IDC_REMOVE_FIXED_FIELD,	OnRemoveFixedField)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
BOOL CFixedFieldDlg::OnInitDialog()
{
	m_FixedFieldsCombo.	SubclassDlgItem	(IDC_FIELD_COMBO,				this);
	m_EnumCombo.		SubclassEdit	(IDC_FIXED_VALUE_COMBO,			this);
	m_BoolCombo.		SubclassDlgItem	(IDC_FIXED_BOOL_VALUE_COMBO,	this);
	m_FixedFieldsList.	SubclassDlgItem	(IDC_FIXED_FIELDS,				this);

	if(!m_pDbtMasterInfo)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	const SqlCatalogEntry* pSqlCatalogEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(m_pDbtMasterInfo->m_strTableName);
	if(!pSqlCatalogEntry)
		return FALSE;
	m_pRecord = pSqlCatalogEntry->CreateRecord();	
	
	m_FixedFieldsList.InsertColumn(0, _TB("Name filed"), LVCFMT_LEFT, 200);
	m_FixedFieldsList.InsertColumn(1, _TB("Value field"), LVCFMT_LEFT, 200);

	FillFieldList();
	FillFieldCombo();

	//metto true e false nella combo bool
	m_BoolCombo.InsertString(0, _T("0"));
	m_BoolCombo.InsertString(1, _T("1"));

	return CLocalizableDialog::OnInitDialog();;
}

//-----------------------------------------------------------------------------------
void CFixedFieldDlg::FillFieldList()
{
	if(!m_pDbtMasterInfo)
	{
		ASSERT(FALSE);
		return;
	}

	if(m_pDbtMasterInfo->m_pXMLFixedKeyArray == NULL)
	{
		m_pDbtMasterInfo->m_pXMLFixedKeyArray = new CXMLFixedKeyArray();
		return;
	}

	for(int i = 0 ; i < m_pDbtMasterInfo->m_pXMLFixedKeyArray->GetSize() ; i++)
	{
		CXMLFixedKey* pXMLFixedKey = m_pDbtMasterInfo->m_pXMLFixedKeyArray->GetAt(i);
		if(!pXMLFixedKey)
		{
			ASSERT(FALSE);
			continue;
		}

		int nItem = m_FixedFieldsList.InsertItem(m_FixedFieldsList.GetItemCount(), pXMLFixedKey->GetName());
	
		//verifico se il campo è di tipo data enum
		DataObj* pDataObj = m_pRecord->GetDataObjFromColumnName(pXMLFixedKey->GetName());
		if(!pDataObj)
		{
			ASSERT(FALSE);
			continue;
		}
			
		if(pDataObj->GetDataType() == DATA_ENUM_TYPE)
		{
			pDataObj->AssignFromXMLString(pXMLFixedKey->GetValue());
			m_FixedFieldsList.SetItemText(nItem, 1, pDataObj->FormatData());
			m_FixedFieldsList.SetItemData(nItem, (DWORD)pDataObj);
		}
		else
		{
			m_FixedFieldsList.SetItemText(nItem, 1, pXMLFixedKey->GetValue());
			m_FixedFieldsList.SetItemData(nItem, 0);
		}
	}
}

//-----------------------------------------------------------------------------------
void CFixedFieldDlg::FillFieldCombo()
{
	if(!m_pRecord)
	{
		ASSERT(FALSE);
		return;
	}

	for(int i = 0 ; i < m_pRecord->GetSize() ; i++)
	{
		if(!m_pRecord->GetAt(i) || !m_pRecord->GetColumnInfo(i))
		{
			ASSERT(FALSE);
			continue;
		}

		//if(!m_pRecord->GetAt(i)->IsSpecial())
		//	continue;

		CString strFieldName = m_pRecord->GetColumnName(i);

		int nSel = m_FixedFieldsCombo.AddString(strFieldName);
		DataObj* pDataObj = m_pRecord->GetAt(i)->GetDataObj();
		if(!pDataObj)
		{
			ASSERT(FALSE);
			continue;
		}

		m_FixedFieldsCombo.SetItemData(nSel, (DWORD)pDataObj);
	}

	if(m_FixedFieldsCombo.GetCount())
	{
		m_FixedFieldsCombo.SetCurSel(0);
		OnFieldChange();
	}
}

//----------------------------------------------------------------------------------------------
void CFixedFieldDlg::OnFieldChange()
{
	CString strFieldName;

	int nSel = m_FixedFieldsCombo.GetCurSel();
	m_FixedFieldsCombo.GetLBText(nSel, strFieldName);
	
	//il campo è un enumerativo
	DataObj* pDataObj = (DataObj*)m_FixedFieldsCombo.GetItemData(nSel);
	if(!pDataObj)
	{
		ASSERT(FALSE);
		return;
	}
	
	BOOL bIsEnum = pDataObj->GetDataType() == DATA_ENUM_TYPE;
	BOOL bIsBool = pDataObj->GetDataType() == DATA_BOOL_TYPE;

	if(bIsEnum)
	{
		m_EnumCombo.ShowWindow(SW_SHOW);
		GetDlgItem(IDC_FIXED_VALUE_EDIT)->ShowWindow(SW_HIDE);
		m_BoolCombo.ShowWindow(SW_HIDE);

		m_EnumCombo.Attach(pDataObj);
		m_EnumCombo.SetValue(*pDataObj);
	}
	else
	{
		m_EnumCombo.ShowWindow(SW_HIDE);
		
		if(bIsBool)
		{
			GetDlgItem(IDC_FIXED_VALUE_EDIT)->ShowWindow(SW_HIDE);
			m_BoolCombo.ShowWindow(SW_SHOW);
		}
		else
		{
			GetDlgItem(IDC_FIXED_VALUE_EDIT)->ShowWindow(SW_SHOW);
			m_BoolCombo.ShowWindow(SW_HIDE);
		}
	}

	int nFieldPos = IsFieldPresent(strFieldName);
	if(nFieldPos != -1)
	{
		CString strValue = m_FixedFieldsList.GetItemText(nFieldPos, 1);
		SetDlgItemText(IDC_FIXED_FIELD_ADD, _TB("Edit"));
		SetDlgItemText(IDC_FIXED_VALUE_EDIT, strValue);
		
		if(!bIsEnum)
			m_EnumCombo.SetValue(strValue);
		
		if(bIsBool)
		{
			if(strValue == "1")
				m_BoolCombo.SetCurSel(1);
			else
				m_BoolCombo.SetCurSel(0);
		}

		m_FixedFieldsList.SetItemState(nFieldPos, LVIS_SELECTED, LVIS_SELECTED);
	}
	else
	{
		SetDlgItemText(IDC_FIXED_FIELD_ADD, _TB("Add"));
		SetDlgItemText(IDC_FIXED_VALUE_EDIT, _T(""));
		m_BoolCombo.SetCurSel(1);
	}
}

//----------------------------------------------------------------------------------------------
void CFixedFieldDlg::OnAddFixedField()
{
	CString strFieldName, strFieldValue;
	
	int nSel = m_FixedFieldsCombo.GetCurSel();
	m_FixedFieldsCombo.GetLBText(nSel, strFieldName);

	//il campo è un enumerativo
	DataObj* pDataObj = (DataObj*)m_FixedFieldsCombo.GetItemData(nSel);
	if(!pDataObj)
	{
		ASSERT(FALSE);
		return;
	}
	
	BOOL bIsEnum = pDataObj->GetDataType() == DATA_ENUM_TYPE;
	BOOL bIsBool = pDataObj->GetDataType() == DATA_BOOL_TYPE;

	if(bIsEnum)
		m_EnumCombo.GetValue(strFieldValue);
	else
		if(bIsBool)
			strFieldValue.Format(_T("%d"), m_BoolCombo.GetCurSel());
		else
			GetDlgItemText(IDC_FIXED_VALUE_EDIT, strFieldValue);

	if(strFieldValue.IsEmpty())
	{
		AfxMessageBox(_TB("No possible insert field with null value."));
		return;
	}

	int nFieldPos = IsFieldPresent(strFieldName);

	//non era stato ancora inserito
	if(nFieldPos == -1)
	{
		int nItem = m_FixedFieldsList.InsertItem(m_FixedFieldsList.GetItemCount(), strFieldName);
	
		m_FixedFieldsList.SetItemText(nItem, 1, strFieldValue);
		m_FixedFieldsList.SetItemData(nItem, (bIsEnum) ? (DWORD)pDataObj : 0);
	}
	else
		m_FixedFieldsList.SetItemText(nFieldPos, 1, strFieldValue);

	SetDlgItemText(IDC_FIXED_FIELD_ADD, _TB("Edit"));
}

//----------------------------------------------------------------------------------------------
void CFixedFieldDlg::OnRemoveFixedField()
{
	int nSel = m_FixedFieldsList.GetNextItem(-1, LVNI_SELECTED);
	if (nSel<0)
		return;

	if(AfxMessageBox(_TB("Do you want remove the segment?"), MB_OKCANCEL) == IDOK)
	{
		m_FixedFieldsList.DeleteItem(nSel);
		OnFieldChange();
	}
}

//----------------------------------------------------------------------------------------------
int CFixedFieldDlg::IsFieldPresent(const CString& strFieldName)
{
	for(int i = 0 ; i < m_FixedFieldsList.GetItemCount() ; i++)
	{
		if(m_FixedFieldsList.GetItemText(i, 0).CompareNoCase(strFieldName) == 0)
			return i;
	}

	return -1;
}

//----------------------------------------------------------------------------------------------
void CFixedFieldDlg::OnOK()
{
	if(!m_pDbtMasterInfo)
	{
		ASSERT(FALSE);
		return;
	}

	if(m_pDbtMasterInfo->m_pXMLFixedKeyArray == NULL)
	{
		ASSERT(FALSE);
		return;
	}

	m_pDbtMasterInfo->m_pXMLFixedKeyArray->RemoveAll();

	for(int i = 0 ; i < m_FixedFieldsList.GetItemCount() ; i++)
	{
		CXMLFixedKey* pXMLFixedKey = new CXMLFixedKey();

		pXMLFixedKey->SetName(m_FixedFieldsList.GetItemText(i, 0));
		
		DataObj* pDataObj = (DataObj*)m_FixedFieldsList.GetItemData(i);

		if(!pDataObj)
			pXMLFixedKey->SetValue(m_FixedFieldsList.GetItemText(i, 1));
		else
			pXMLFixedKey->SetValue(pDataObj->FormatDataForXML());
		
		m_pDbtMasterInfo->m_pXMLFixedKeyArray->Add(pXMLFixedKey);
	}

	__super::OnOK();
}

//----------------------------------------------------------------------------------------------
//	CDBTPropertiesDlg
//----------------------------------------------------------------------------------------------
CDBTPropertiesDlg::CDBTPropertiesDlg(CXMLDBTInfo* pDBT)
	:
	CLocalizableDialog(IDD_DBT_PROPERTIES, NULL)
{
	m_pDBTInfo = pDBT;
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDBTPropertiesDlg, CLocalizableDialog)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDBTPropertiesDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CDBTPropertiesDlg
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
BOOL CDBTPropertiesDlg::OnInitDialog	()
{
	CLocalizableDialog::OnInitDialog();
	SetWindowText(cwsprintf(_TB("Properties of {0-%s}"), m_pDBTInfo->GetTitle()));		

	SetDlgItemText(IDC_DBT_PROPS_TABLENAME_STATIC, m_pDBTInfo->m_strTableName);
	SetDlgItemText(IDC_DBT_PROPS_TYPE_STATIC, m_pDBTInfo->GetStrType());

	((CButton*)GetDlgItem(IDC_DBT_PROPS_UPDATE_CHKBOX))->SetCheck(m_pDBTInfo->GetChooseUpdate());
	
	if (m_pDBTInfo->GetType() == CXMLDBTInfo::MASTER_TYPE)
	{
		CWnd* pChild = GetDlgItem(IDC_DBT_PROPS_GROUP_STATIC);
		
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

		GetDlgItem(IDC_DBT_PROPS_UPDATE_CHKBOX)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_DBT_PROPS_UPDATE_STATIC)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_DBT_PROPS_GROUP_STATIC)->ShowWindow(SW_HIDE);
	}	

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CDBTPropertiesDlg::OnOK	()
{
	m_pDBTInfo->m_bChooseUpdate = ((CButton*)GetDlgItem(IDC_DBT_PROPS_UPDATE_CHKBOX))->GetCheck();
	
	CLocalizableDialog::OnOK();
}

//----------------------------------------------------------------------------------------------
//	CUniversalKeyGroupDlg
//----------------------------------------------------------------------------------------------
CUniversalKeyGroupDlg::CUniversalKeyGroupDlg(CXMLUniversalKeyGroup* pXMLUniversalKeyGroup, BOOL bReadOnly /*FALSE*/)
	:
	CLocalizableDialog	(IDD_DBT_UK_GROUP_PROPERTIES, NULL)
{
	m_bReadOnly = bReadOnly;
	m_pXMLUniversalKeyGroup = pXMLUniversalKeyGroup;
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (CUniversalKeyGroupDlg, CLocalizableDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUniversalKeyGroupDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CUniversalKeyGroupDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
BOOL CUniversalKeyGroupDlg::OnInitDialog	()
{
	CLocalizableDialog::OnInitDialog();

	if(!m_pXMLUniversalKeyGroup)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	m_FuncCombo.SubclassDlgItem(IDC_UK_FUNCTION_COMBO, this);
	
	//riempio combo funzioni
	if(FillUKFuncCombo(m_pXMLUniversalKeyGroup->GetFunctionName()) == 0)
	{
		AfxMessageBox(_T("Il modulo non contiene la dichiarazione di nessuna funzione"));
		GetDlgItem(IDOK)->EnableWindow(FALSE);
	}

	return TRUE;
}

static const TCHAR szLostAndFound[] = _T("LostAndFound");
static const TCHAR szAlternativeSearch[] = _T("AlternativeSearch");

//---------------------------------------------------------------------------
BOOL GetUKCommonFunctionList(CStringArray* pUKFuncList)
{
	if (!pUKFuncList)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bLostAndFound = FALSE, bAlternativeSearch = FALSE;
	CString tempStr;
	for (int i = 0; !(bLostAndFound && bAlternativeSearch) && i<pUKFuncList->GetSize(); i++)
	{
		tempStr = pUKFuncList->GetAt(i);
		if (tempStr == szLostAndFound)
			bLostAndFound = TRUE;
		else if (tempStr == szAlternativeSearch)
			bAlternativeSearch = TRUE;
	}

	if (!bLostAndFound)
		pUKFuncList->Add(szLostAndFound);
	if (!bAlternativeSearch)
		pUKFuncList->Add(szAlternativeSearch);

	return TRUE;
}


//----------------------------------------------------------------------------------------------
int  CUniversalKeyGroupDlg::FillUKFuncCombo(const CString& strFunction)
{
	// se è già stata caricata effettuo solo la selezione sulla stringa strEnvClass
	// se diversa da Empty
	ASSERT(AfxGetAddOnAppsTable());
	int nCurrSel = 0;
	int nPos = 0;
	
	if (m_FuncCombo.GetCount() > 0)
	{
		nPos = m_FuncCombo.SelectString(-1, (LPCTSTR)strFunction);		
		if (nPos == m_FuncCombo.GetCurSel())
			return m_FuncCombo.GetCount();		
		if (nPos != LB_ERR) 
			nCurrSel = nPos;
	}

	CStringArray aUKFuncArray;
	GetUKCommonFunctionList(&aUKFuncArray);
	
	for(int n = 0 ; n < aUKFuncArray.GetSize() ; n++)
	{
		nPos = m_FuncCombo.InsertString(-1, aUKFuncArray.GetAt(n));
		if(aUKFuncArray.GetAt(n).CompareNoCase(strFunction) == 0)
			nCurrSel = nPos;
	}

	m_FuncCombo.SetCurSel(nCurrSel);
	
	return m_FuncCombo.GetCount();
}

//----------------------------------------------------------------------------------------------
void CUniversalKeyGroupDlg::OnOK	()
{
	CString strText;
	int nSel = m_FuncCombo.GetCurSel();
	if(nSel >= 0)
	{
		m_FuncCombo.GetLBText(nSel, strText);
		m_pXMLUniversalKeyGroup->SetFunctionName(strText);
		CLocalizableDialog::OnOK();
	}
}

//----------------------------------------------------------------------------------------------
//	CUniversalKeyDlg
//----------------------------------------------------------------------------------------------
CUniversalKeyDlg::CUniversalKeyDlg(CXMLUniversalKey* pXMLUniversalKey, const CString& strTableName, BOOL bReadOnly /*FALSE*/)
	:
	CLocalizableDialog(IDD_UNIVERSAL_KEY_PROPERTIES, NULL)
{
	m_bReadOnly = bReadOnly;
	m_pXMLUniversalKey = pXMLUniversalKey;
	m_strTableName = strTableName;
}

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CUniversalKeyDlg, CLocalizableDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUniversalKeyDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CUniversalKeyDlg
	ON_BN_CLICKED		(IDC_ADD_UK_SEGMENT,			OnAddSegment)
	ON_BN_CLICKED		(IDC_REMOVE_UK_SEGMENT,			OnRemoveSegment)
	ON_LBN_DBLCLK		(IDC_UNIVERSAL_KEY_FIELD_LIST,	OnAddSegment)
	ON_LBN_DBLCLK		(IDC_UNIVERSAL_KEY_SEGMENT_LIST,OnRemoveSegment)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
BOOL CUniversalKeyDlg::OnInitDialog	()
{
	CLocalizableDialog::OnInitDialog();

	if(!m_pXMLUniversalKey)
	{
		ASSERT(FALSE);
		return TRUE;
	}

	m_FieldList.SubclassDlgItem(IDC_UNIVERSAL_KEY_FIELD_LIST, this);
	m_UkSegmentList.SubclassDlgItem(IDC_UNIVERSAL_KEY_SEGMENT_LIST, this);

	SetDlgItemText(IDC_UK_NAME, m_pXMLUniversalKey->GetName());

	//riempio lista campi
	FillFieldList();

	//riempio lista segmenti
	FillSegmentList();

	return TRUE;
}

//----------------------------------------------------------------------------
void CUniversalKeyDlg::FillFieldList()
{
	//riempio la lista dei field
	SqlTableInfo* pMasterTblInfo = AfxGetDefaultSqlConnection()->GetTableInfo(m_strTableName);	

	if(!pMasterTblInfo)
	{
		ASSERT(FALSE);
		return;
	}
	
	CStringArray arFields;
	const  Array* pColums = pMasterTblInfo->GetPhysicalColumns();
	for(int i = 0 ; i < pColums->GetSize() ; i++)
	{
		SqlColumnInfo* pInfo = (SqlColumnInfo*) pColums->GetAt(i);
		if(!pInfo)
		{
			ASSERT(FALSE);
			continue;
		}

		CString strColName = pInfo->GetColumnName();

		BOOL bFound = FALSE;
		for(int i = 0 ; i < m_pXMLUniversalKey->GetSegmentNumber() ; i++)
		{
			if(m_pXMLUniversalKey->GetSegmentAt(i).CompareNoCase(strColName) == 0)
			{
				bFound = TRUE;
				break;
			}
		}
		if(!bFound)
			m_FieldList.AddString(strColName);
	}
}

//----------------------------------------------------------------------------
void CUniversalKeyDlg::FillSegmentList()
{
	if(!m_pXMLUniversalKey)
	{
		ASSERT(FALSE);
		return;
	}

	m_UkSegmentList.ResetContent();

	for(int i = 0 ; i < m_pXMLUniversalKey->GetSegmentNumber() ; i++)
		m_UkSegmentList.AddString(m_pXMLUniversalKey->GetSegmentAt(i));
}

//---------------------------------------------------------------------------------
void CUniversalKeyDlg::OnAddSegment()
{
	CString strText;
	for(int i = m_FieldList.GetCount() - 1 ; i >= 0  ; i--)
	{
		if(m_FieldList.GetSel(i) > 0)
		{
			m_FieldList.GetText(i, strText);
			
			m_UkSegmentList.AddString(strText);

			m_FieldList.DeleteString(i);
		}
	}
}

//---------------------------------------------------------------------------------
void CUniversalKeyDlg::OnRemoveSegment()
{
	CString strText;
	int size = m_UkSegmentList.GetCount();
	for(int i = size - 1 ; i >= 0 ; i--)
	{
		if(m_UkSegmentList.GetSel(i) > 0)
		{
			m_UkSegmentList.GetText(i, strText);
			m_UkSegmentList.DeleteString(i);
			m_FieldList.AddString(strText);
		}
	}
}

//----------------------------------------------------------------------------------------------
void CUniversalKeyDlg::OnOK	()
{
	if(m_pXMLUniversalKey)
	{
		CString strUkName;
		GetDlgItemText(IDC_UK_NAME, strUkName);

		if(strUkName.IsEmpty())
		{
			AfxMessageBox((LPCTSTR)_TB("Enter a valid Universal Key name"));
			return;
		}

		m_pXMLUniversalKey->SetName(strUkName);

		m_pXMLUniversalKey->RemoveAllSegments();
		CString strTmp;
		for(int i = 0 ; i < m_UkSegmentList.GetCount() ; i++)
		{
			m_UkSegmentList.GetText(i, strTmp);
			m_pXMLUniversalKey->AddSegment(strTmp);
		}
	}

	CLocalizableDialog::OnOK();
}


/////////////////////////////////////////////////////////////////////////////
// CDocDescrTreeCtrl
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDocDescrTreeCtrl, CTBTreeCtrl)

BEGIN_MESSAGE_MAP(CDocDescrTreeCtrl, CTBTreeCtrl)
	//{{AFX_MSG_MAP(CDocDescrTreeCtrl)
	ON_NOTIFY_REFLECT	(TVN_ITEMEXPANDING,						OnItemExpanding)
	ON_NOTIFY_REFLECT	(TVN_BEGINLABELEDIT,					OnItemBeginEdit)
	ON_NOTIFY_REFLECT	(TVN_ENDLABELEDIT,						OnItemEndEdit)
	ON_NOTIFY_REFLECT	(TVN_KEYDOWN,							OnKeydown)
	ON_WM_CONTEXTMENU	()
	ON_NOTIFY_REFLECT	(NM_DBLCLK,								OnDoubleClick)
	
	//menu del nodo dbt
	ON_COMMAND(ID_DOC_TREE_DBT_PROPERTIES, OnDBTProperties)
	ON_COMMAND(ID_DOC_TREE_DBT_NEW_XREF,	OnNewXRef)
	ON_COMMAND(ID_DOC_TREE_DBT_NEW_UK_GROUP, OnNewUniversalKeyGroup)
	ON_COMMAND(ID_DOC_TREE_DBT_FIXED_FIELD, OnFixedField)
	ON_COMMAND(ID_DOC_TREE_DBT_HKL, OnDBTHotKeyLink)
	ON_COMMAND(ID_DOC_TREE_DBT_APPEND_XREF, OnAppendXRefs)

	//menu del nodo XRef
	ON_COMMAND(ID_DOC_TREE_XREF_MODIFY_XREF, OnModifyXRef)
	ON_COMMAND(ID_DOC_TREE_XREF_REMOVE_XREF, OnRemoveXRef)
	ON_COMMAND(ID_DOC_TREE_XREF_HKL, OnXRefHotKeyLink)

	
	//menu del nodo Universal Key Group
	ON_COMMAND(ID_DOC_TREE_UK_GROUP_NEW_UK_ELEMENT, OnNewUniversalKey)
	ON_COMMAND(ID_DOC_TREE_UK_GROUP_MODIFY_GROUP, OnUniversalKeyGroupProperties)
	ON_COMMAND(ID_DOC_TREE_UK_GROUP_REMOVE_GROUP, OnRemoveUniversalKeyGroup)

	//menu del nodo Universal Key Elem
	ON_COMMAND(ID_DOC_TREE_UK_MODIFY_UK_ELEMENT, OnUniversalKeyProperties)
	ON_COMMAND(ID_DOC_TREE_UK_REMOVE_UK_ELEMENT, OnRemoveUniversalKey)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDocDescrTreeCtrl::CDocDescrTreeCtrl()
	:
	m_bEnableDBTNaming	(FALSE),
	m_bReadOnly			(FALSE),
//	m_bModified			(FALSE),
	m_bDescription		(TRUE),
	m_pXMLDocObject		(NULL)
{
}

//-----------------------------------------------------------------------------
CDocDescrTreeCtrl::~CDocDescrTreeCtrl()
{
	m_ImageList.DeleteImageList();
}

//-------------------------------------------------------------------------------
void CDocDescrTreeCtrl::InitializeImageList()
{	
	CString asPaths[16];
	asPaths[0] = TBGlyph(szIconDocument);
	asPaths[1] = TBGlyph(szIconDbtMaster);
	asPaths[2] = TBGlyph(szIconDbtSlave);
	asPaths[3] = TBGlyph(szIconDbtBuffered);
	
	asPaths[4] = TBGlyph(szIconTreeDbt);
	asPaths[5] = TBGlyph(szIconExtRef);
	asPaths[6] = TBGlyph(szIconSegment);
	asPaths[7] = TBGlyph(szIconDbtMastExp);
	asPaths[8] = TBGlyph(szIconDbtSlavExp);
	asPaths[9] = TBGlyph(szIconDbtSlavBufExp);
	asPaths[10] = TBGlyph(szIconXRefCon);
	asPaths[11] = TBGlyph(szIconUniversalKey);
	asPaths[12] = TBGlyph(szIconUniversalKeyGroup);
	asPaths[13] = TBGlyph(szIconXRefUniversalKey);
	asPaths[14] = TBGlyph(szIconXRefCondUK);
	asPaths[15] = TBGlyph(szIconXRefNotExported);

	for (size_t i = 0; i < 16; i++)
	{
		HICON hIcon = TBLoadImage(asPaths[i]);
		if (i == 0)
		{
			CSize iconSize = GetHiconSize(hIcon);
			m_ImageList.Create(iconSize.cx, iconSize.cy, ILC_COLOR32, 16, 16);
			SetBackColor();
		}

		m_ImageList.Add(hIcon);
		::DeleteObject(hIcon);
	}
	SetImageList(&m_ImageList, TVSIL_NORMAL);
}

//-------------------------------------------------------------------------------
void CDocDescrTreeCtrl::SetBackColor()
{
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
}

//-------------------------------------------------------------------------------
HTREEITEM CDocDescrTreeCtrl::InsertDBTItem
									(
										CXMLDBTInfo*	pDBTInfo,
										HTREEITEM		hParentItem	/*= TVI_ROOT*/, 
										HTREEITEM		hInsertAfter/*= TVI_LAST*/
									)
{
	if (!pDBTInfo || !AfxIsValidAddress(pDBTInfo, sizeof(CXMLDBTInfo)))
		return 0;

	HTREEITEM hItem = 0;
	
	int nImage;

	switch (pDBTInfo->m_eType)
	{
		case CXMLDBTInfo::MASTER_TYPE:
			if(pDBTInfo->IsToExport())
				nImage = 7;
			else
				nImage = 1;

			hItem = InsertItem(pDBTInfo->GetTitle(), nImage, nImage, hParentItem, hInsertAfter);
			break;

		case CXMLDBTInfo::SLAVE_TYPE:
			if(pDBTInfo->IsToExport())
				nImage = 8;
			else
				nImage = 2;

			hItem = InsertItem(pDBTInfo->GetTitle(), nImage, nImage, hParentItem, hInsertAfter);
			break;

		case CXMLDBTInfo::BUFFERED_TYPE:
			if(pDBTInfo->IsToExport())
				nImage = 9;
			else
				nImage = 3;

			hItem = InsertItem((LPCTSTR)pDBTInfo->GetTitle(), nImage, nImage, hParentItem, hInsertAfter);
			break;

		default:
			//hItem = InsertItem((LPCTSTR)pDBTInfo->GetTitle(), 4, 4, hParentItem, hInsertAfter);
			break;
	}
	
	if (!hItem)
		return 0;

	SetItemData(hItem, (DWORD) pDBTInfo);
	return hItem;
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::UpdateXRefIcon(HTREEITEM hXRefItem, CXMLXRefInfo* pCurrXRef)
{
	if(!pCurrXRef || !hXRefItem)
	{
		ASSERT(FALSE);
		return;
	}

	SetItemImage(hXRefItem, 5, 5);

	if(!pCurrXRef->IsToUse())
		SetItemImage(hXRefItem, 15, 15);
	else
		if(pCurrXRef->m_bSubjectTo && pCurrXRef->GetXMLUniversalKeyGroup())
			SetItemImage(hXRefItem, 14, 14);
		else
			if(pCurrXRef->GetXMLUniversalKeyGroup())
				SetItemImage(hXRefItem, 13, 13);
			else
				if(pCurrXRef->m_bSubjectTo)
					SetItemImage(hXRefItem, 10, 10);
}

//-------------------------------------------------------------------------------
HTREEITEM CDocDescrTreeCtrl::InsertXRefItem
									(
										CXMLXRefInfo*	pXRefInfo,
										HTREEITEM		hDBTParentItem	/*= TVI_ROOT*/, 
										HTREEITEM		hInsertAfter	/*= TVI_LAST*/
									)
{
	if (!pXRefInfo || !AfxIsValidAddress(pXRefInfo, sizeof(CXMLXRefInfo)))
		return 0;

	int nImage = 5; //xref normale

	HTREEITEM hItem = InsertItem((LPCTSTR)pXRefInfo->GetName(), nImage, nImage, hDBTParentItem, hInsertAfter);
	if (!hItem)
		return 0;

	SetItemData(hItem, (DWORD) pXRefInfo);

	UpdateXRefIcon(hItem, pXRefInfo);

	return hItem;
}

//-------------------------------------------------------------------------------
HTREEITEM CDocDescrTreeCtrl::InsertXRefSegmentItem
									(
										CXMLSegmentInfo*	pSegInfo,
										HTREEITEM			hXRefParentItem	/*= TVI_ROOT*/, 
										HTREEITEM			hInsertAfter	/*= TVI_LAST*/
									)
{
	if (!pSegInfo || !AfxIsValidAddress(pSegInfo, sizeof(CXMLSegmentInfo)))
		return 0;

	HTREEITEM hItem = InsertItem(_T(""), 6, 6, hXRefParentItem, hInsertAfter);
	if (!hItem)
		return 0;

	SetItemData(hItem, (DWORD) pSegInfo);
	SetSegmentItemText(hItem);

	return hItem;
}

//-------------------------------------------------------------------------------
HTREEITEM CDocDescrTreeCtrl::InsertUniversalKeyGroupItem
									(
										CXMLUniversalKeyGroup*	pUKGroupInfo,
										const CString&			strName,
										HTREEITEM				hDBTParentItem	/*= TVI_ROOT*/, 
										HTREEITEM				hInsertAfter	/*= TVI_LAST*/
									)
{
	if (!pUKGroupInfo || !AfxIsValidAddress(pUKGroupInfo, sizeof(CXMLUniversalKeyGroup)))
		return 0;

	int nImage = 12; //uk

	HTREEITEM hItem = InsertItem(strName, nImage, nImage, hDBTParentItem, hInsertAfter);
	if (!hItem)
		return 0;

	SetItemData(hItem, (DWORD) pUKGroupInfo);



	return hItem;
}

//-------------------------------------------------------------------------------
HTREEITEM CDocDescrTreeCtrl::InsertUniversalKeyItem
									(
										CXMLUniversalKey*	pUKInfo,
										const CString&		strName,
										HTREEITEM			hUKGroupItem	/*= TVI_ROOT*/, 
										HTREEITEM			hInsertAfter	/*= TVI_LAST*/
									)
{
	if (!pUKInfo || !AfxIsValidAddress(pUKInfo, sizeof(CXMLUniversalKey)))
		return 0;

	int nImage = 11; //uk

	HTREEITEM hItem = InsertItem(strName, nImage, nImage, hUKGroupItem, hInsertAfter);
	if (!hItem)
		return 0;

	SetItemData(hItem, (DWORD) pUKInfo);
	return hItem;
}

//-------------------------------------------------------------------------------
HTREEITEM CDocDescrTreeCtrl::InsertUniversalKeySegmentItem
									(
										const CString&		strSegment,
										HTREEITEM			hXRefParentItem	/*= TVI_ROOT*/, 
										HTREEITEM			hInsertAfter	/*= TVI_LAST*/
									)
{
	if (strSegment.IsEmpty())
		return 0;

	HTREEITEM hItem = InsertItem(strSegment, 6, 6, hXRefParentItem, hInsertAfter);
	if (!hItem)
		return 0;

	return hItem;
}

//-------------------------------------------------------------------------------
void CDocDescrTreeCtrl::SetXMLDocObjInfo(CXMLDocObjectInfo* pXMLDocObj)
{

	if (!pXMLDocObj)
	{
		ASSERT(FALSE);
		return;
	}

	m_nsDocument = pXMLDocObj->GetNamespaceDoc();
	if (!m_nsDocument.IsValid())
	{
		ASSERT(FALSE);
		return;
	}

	m_pXMLDocObject = pXMLDocObj;
	ASSERT(m_pXMLDocObject);
}

//-------------------------------------------------------------------------------
void CDocDescrTreeCtrl::FillTree(CXMLDocObjectInfo* pOldXMLDocObj)
{
	DeleteAllItems();
	if (!m_pXMLDocObject)
		return;
	
	CString docTile = m_pXMLDocObject->GetDocumentTitle();
	if (!pOldXMLDocObj)
		docTile += _TB(" New!");
	
	HTREEITEM hDocTreeItem = InsertItem(docTile);

	if(!hDocTreeItem)
	{
		ASSERT(FALSE);
		return;
	}

	SetItemData(hDocTreeItem, (DWORD)m_pXMLDocObject);
	
	if (!m_pXMLDocObject->m_pDBTArray || m_pXMLDocObject->m_pDBTArray->GetSize() <= 0)
	{
		ASSERT(FALSE);
		return;
	}
		
	CXMLDBTInfo* pXMLDBTInfo = NULL;
	for (int i = 0; i <m_pXMLDocObject->m_pDBTArray->GetSize(); i++)
	{
		pXMLDBTInfo = m_pXMLDocObject->m_pDBTArray->GetAt(i);
		if (pXMLDBTInfo)
			AddDbtInfo(pXMLDBTInfo, hDocTreeItem);
		else
			ASSERT(FALSE);
	}
/*
	

	for( int nDBTIdx = 0 ; nDBTIdx < pDBTMaster->GetDBTSlaves()->GetSize() ; nDBTIdx++)
	{
		DBTSlave* pDBTSlave = pDBTMaster->GetDBTSlaves()->GetAt(nDBTIdx);
		if(!pDBTSlave)
		{
			ASSERT(FALSE);
			continue;
		}

		if(pDBTSlave->IsDBTOnView())
			continue;

		CXMLDBTInfo* pXMLDBTInfo = pDBTSlave->GetXMLDBTInfo();
		if (!pXMLDBTInfo) 
			pXMLDBTInfo = pDBTSlave->GetClientDocOwner()
						  ? m_pXMLDocObject->UpdateClientDocDBTInfo(pDBTSlave->GetClientDocOwner()->m_Namespace, pDBTSlave)
						  :	m_pXMLDocObject->UpdateDBTInfo(pDBTSlave);

		if(!pXMLDBTInfo || !pXMLDBTInfo->GetNamespace().IsValid())
		{
			ASSERT(FALSE);
			continue;
		}
		
		AddDbtInfo(pXMLDBTInfo, hDocTreeItem);
	}*/
	
	Expand(hDocTreeItem, TVE_EXPAND);
}

//-------------------------------------------------------------------------------
void CDocDescrTreeCtrl::AddDbtInfo(CXMLDBTInfo* pXMLDBTInfo, HTREEITEM hDocTreeItem, CXMLDocObjectInfo* pOldXMLDocObj /*=NULL*/)
{
	if(!pXMLDBTInfo || !hDocTreeItem)
		return;

	HTREEITEM hDBTItem = InsertDBTItem (pXMLDBTInfo, hDocTreeItem);

	if (!hDBTItem)
		return;

	//il documento rispetto alla descrizione xml ha un nuovo DBT
	if (pOldXMLDocObj && !pOldXMLDocObj->GetDBTFromNamespace(pXMLDBTInfo->GetNamespace()))
	{
		CString dbtItemText = pXMLDBTInfo->GetTitle();
		dbtItemText += _TB(" New!");
		SetItemText(hDBTItem, dbtItemText);
	}
	

	if(pXMLDBTInfo->GetType() == CXMLDBTInfo::MASTER_TYPE && pXMLDBTInfo->GetXMLUniversalKeyGroup())
	{
		HTREEITEM hUKGroupItem = InsertUniversalKeyGroupItem(pXMLDBTInfo->GetXMLUniversalKeyGroup(), szUKGroupName, hDBTItem);

		for (int nUKIdx = 0 ; nUKIdx < pXMLDBTInfo->GetXMLUniversalKeyGroup()->GetSize() ; nUKIdx++)
		{
			CXMLUniversalKey* pXMLUniversalKey = pXMLDBTInfo->GetXMLUniversalKeyAt(nUKIdx);
			if(!pXMLUniversalKey)
				continue;
			
			CString strUKName = pXMLUniversalKey->GetName();
			if(strUKName.IsEmpty())
			{
				strUKName.Format(_T("UK_%d"), nUKIdx);
				pXMLUniversalKey->SetName(strUKName);
			}

			HTREEITEM hUKItem = InsertUniversalKeyItem(pXMLUniversalKey, strUKName, hUKGroupItem);
		
			if (!hUKItem)
				continue;

			for(int nSegIdx = 0 ; nSegIdx < pXMLUniversalKey->GetSegmentNumber() ; nSegIdx++)
			{
				CString strSeg = pXMLUniversalKey->GetSegmentAt(nSegIdx);
				if(strSeg.IsEmpty())
					continue;

				InsertUniversalKeySegmentItem(strSeg,	hUKItem);
			}
		}
	}

	if (!pXMLDBTInfo->GetXMLXRefInfoArray()) 
		return;

	for (int nXRefIdx = 0 ; nXRefIdx < pXMLDBTInfo->GetXMLXRefInfoArray()->GetSize() ; nXRefIdx++)
	{
		CXMLXRefInfo* pXRefInfo = pXMLDBTInfo->GetXRefAt(nXRefIdx);
		if(!pXRefInfo)
			continue;
		
		HTREEITEM hXRefItem = InsertXRefItem(pXRefInfo,	hDBTItem);
	
		if (!hXRefItem)
			continue;

		for(int nSegIdx = 0 ; nSegIdx < pXRefInfo->GetSegmentsNum() ; nSegIdx++)
		{
			CXMLSegmentInfo* pSegInfo = pXRefInfo->GetSegmentAt(nSegIdx);
			if(!pSegInfo)
				continue;

			InsertXRefSegmentItem(pSegInfo,	hXRefItem);
		}
	}
}

//----------------------------------------------------------------------------
int	CDocDescrTreeCtrl::GetItemType(HTREEITEM hItem) const
{
	CObject* pItemObject = NULL;

	if (!hItem || !(pItemObject = (CObject*)GetItemData(hItem)))
		return -1;

	ASSERT_VALID(pItemObject);

	if (pItemObject->IsKindOf(RUNTIME_CLASS(CXMLDocObjectInfo)))
		return DOC_DESCR_TREE_ITEM_TYPE_ROOT;
	
	if (pItemObject->IsKindOf(RUNTIME_CLASS(CXMLDBTInfo)))
		return DOC_DESCR_TREE_ITEM_TYPE_DBT;
	
	if (pItemObject->IsKindOf(RUNTIME_CLASS(CXMLXRefInfo)))
		return DOC_DESCR_TREE_ITEM_TYPE_XREF;
	
	if (pItemObject->IsKindOf(RUNTIME_CLASS(CXMLSegmentInfo)))
		return DOC_DESCR_TREE_ITEM_TYPE_SEGMENT;
	
	if (pItemObject->IsKindOf(RUNTIME_CLASS(CXMLUniversalKeyGroup)))
		return DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP;

	if (pItemObject->IsKindOf(RUNTIME_CLASS(CXMLUniversalKey)))
		return DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY;

	return -1;
}

//----------------------------------------------------------------------------
CXMLDBTInfo* CDocDescrTreeCtrl::GetCurrentDBT(HTREEITEM* lphDBTItem /* = NULL*/) const
{
	if (lphDBTItem)
		*lphDBTItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();
	
	if	(
			hSelItem && 
			(	GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_XREF	||
				GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP
			)
		)
		hSelItem = GetParentItem(hSelItem);

	if	(
			hSelItem && 
			(
				GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY	||
				GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_SEGMENT
			)
		)
	{
		HTREEITEM hTmp;
		hTmp = GetParentItem(hSelItem);
		if(!hTmp)
			hSelItem = GetParentItem(hTmp);
	}

	if (!hSelItem || GetItemType(hSelItem) != DOC_DESCR_TREE_ITEM_TYPE_DBT)
		return NULL;

	if (lphDBTItem)
		*lphDBTItem = hSelItem;
	
	return (CXMLDBTInfo*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
CXMLXRefInfo* CDocDescrTreeCtrl::GetCurrentXRef(HTREEITEM* lphXRefItem /* = NULL*/) const
{
	if (lphXRefItem)
		*lphXRefItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();
	if (!hSelItem || GetItemType(hSelItem) != DOC_DESCR_TREE_ITEM_TYPE_XREF)
		return NULL;

	if (lphXRefItem)
		*lphXRefItem = hSelItem;
	
	return (CXMLXRefInfo*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
CXMLUniversalKeyGroup* CDocDescrTreeCtrl::GetCurrentUniversalKeyGroup(HTREEITEM* lphUKGroupItem /* = NULL*/) const
{
	if (lphUKGroupItem)
		*lphUKGroupItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();

	if	(hSelItem && GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY)
		hSelItem = GetParentItem(hSelItem);

	if (!hSelItem || GetItemType(hSelItem) != DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP)
		return NULL;

	if (lphUKGroupItem)
		*lphUKGroupItem = hSelItem;
	
	return (CXMLUniversalKeyGroup*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
CXMLUniversalKey* CDocDescrTreeCtrl::GetCurrentUniversalKey(HTREEITEM* lphUKItem /* = NULL*/) const
{
	if (lphUKItem)
		*lphUKItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();
	if (!hSelItem || GetItemType(hSelItem) != DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY)
		return NULL;

	if (lphUKItem)
		*lphUKItem = hSelItem;
	
	return (CXMLUniversalKey*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
CXMLSegmentInfo* CDocDescrTreeCtrl::GetCurrentSegment(HTREEITEM* lphSegmentItem /* = NULL*/) const
{
	if (lphSegmentItem)
		*lphSegmentItem = NULL;

	HTREEITEM hSelItem = GetSelectedItem();
	if (!hSelItem || GetItemType(hSelItem) != DOC_DESCR_TREE_ITEM_TYPE_SEGMENT)
		return NULL;

	if (lphSegmentItem)
		*lphSegmentItem = hSelItem;
	
	return (CXMLSegmentInfo*)GetItemData(hSelItem);
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::SetSegmentItemText (HTREEITEM hSegmentItem)
{
	if (!hSegmentItem || GetItemType(hSegmentItem) != DOC_DESCR_TREE_ITEM_TYPE_SEGMENT)
		return;

	CXMLSegmentInfo* pSegInfo = (CXMLSegmentInfo*)GetItemData(hSegmentItem);

	SetItemText(hSegmentItem, pSegInfo ? (LPCTSTR)(pSegInfo->GetFKSegment() + _T(" -> ") + pSegInfo->GetReferencedSegment()) : _T(""));
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::RemoveTreeChilds(HTREEITEM hItem)
{
	CString text;

	HTREEITEM hFirst = GetNextItem(hItem, TVGN_CHILD);

	if(!hFirst)
		return;

	HTREEITEM hNode;

	while(hNode = GetNextItem(hFirst, TVGN_NEXT))
		DeleteItem(hNode);

	if(hFirst)
		DeleteItem(hFirst);
}

////----------------------------------------------------------------------------
//CAbstractFormDoc* CDocDescrTreeCtrl::GetDocument() const
//{
//	HTREEITEM hRoot = GetRootItem();
//	if (!hRoot)
//		return NULL;
//	
//	return (CAbstractFormDoc*)GetItemData(hRoot);
//}
BOOL CDocDescrTreeCtrl::LoadMenu(CMenu& menu, int nItemType)
{

	if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_XREF)
	{
		/*
		IDR_DOC_TREE_XREF_CONTEXTMENU MENU
		BEGIN
		POPUP "Menu"
		BEGIN
		MENUITEM "Use",                         ID_DOC_TREE_XREF_EXPORT_XREF, GRAYED
		MENUITEM "Delete",                      ID_DOC_TREE_XREF_REMOVE_XREF
		MENUITEM "Properties...",               ID_DOC_TREE_XREF_MODIFY_XREF
		MENUITEM "Manage HotLink",              ID_DOC_TREE_XREF_HKL
		END
		END
		*/
		menu.CreateMenu();
		CMenu popup;
		popup.CreatePopupMenu();
		popup.AppendMenu(MF_STRING | MF_GRAYED, ID_DOC_TREE_XREF_EXPORT_XREF, _TB("Use"));
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_XREF_REMOVE_XREF, _TB("Delete"));
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_XREF_MODIFY_XREF, _TB("Properties..."));
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_XREF_HKL, _TB("Manage HotLink"));
		menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popup.Detach(), _TB("Menu"));
		return TRUE;
	}
	if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_DBT)
	{
		/*
		IDR_DOC_TREE_DBT_CONTEXTMENU MENU
		BEGIN
		POPUP "Menu"
		BEGIN
		MENUITEM "Properties...",					ID_DOC_TREE_DBT_PROPERTIES
		MENUITEM SEPARATOR
		MENUITEM "New External Reference",			ID_DOC_TREE_DBT_NEW_XREF
		MENUITEM "Append External References",		ID_DOC_TREE_DBT_APPEND_XREF
		MENUITEM "New Universal Key Group",			ID_DOC_TREE_DBT_NEW_UK_GROUP
		MENUITEM "Do Not Use External References",	ID_DOC_TREE_DBT_NO_XREF, GRAYED
		MENUITEM "Use All External References",		ID_DOC_TREE_DBT_ALL_XREF, GRAYED
		MENUITEM "Manage fixed fields",				ID_DOC_TREE_DBT_FIXED_FIELD, GRAYED
		MENUITEM "Manage HotLink",					ID_DOC_TREE_DBT_HKL, GRAYED
		END
		END
		*/
		menu.CreateMenu();
		CMenu popup;
		popup.CreatePopupMenu();
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_DBT_PROPERTIES, _TB("Properties..."));
		popup.AppendMenu(MF_SEPARATOR);
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_DBT_NEW_XREF, _TB("New External Reference"));
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_DBT_APPEND_XREF, _TB("Append External References"));
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_DBT_NEW_UK_GROUP, _TB("New Universal Key Group"));
		popup.AppendMenu(MF_STRING | MF_GRAYED, ID_DOC_TREE_DBT_NO_XREF, _TB("Do Not Use External References"));
		popup.AppendMenu(MF_STRING | MF_GRAYED, ID_DOC_TREE_DBT_ALL_XREF, _TB("Use All External References"));
		popup.AppendMenu(MF_STRING | MF_GRAYED, ID_DOC_TREE_DBT_FIXED_FIELD, _TB("Manage fixed fields"));
		popup.AppendMenu(MF_STRING | MF_GRAYED, ID_DOC_TREE_DBT_HKL, _TB("Manage HotLink"));
		menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popup.Detach(), _TB("Menu"));
		return TRUE;
	}
	if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY)
	{
		/*
		IDR_DOC_TREE_UK_CONTEXTMENU MENU
		BEGIN
		POPUP "Menu"
		BEGIN
		MENUITEM "Delete",                      ID_DOC_TREE_UK_REMOVE_UK_ELEMENT
		MENUITEM SEPARATOR
		MENUITEM "Properties...",               ID_DOC_TREE_UK_MODIFY_UK_ELEMENT
		END
		END
		*/
		menu.CreateMenu();
		CMenu popup;
		popup.CreatePopupMenu();
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_UK_REMOVE_UK_ELEMENT, _TB("Delete"));
		popup.AppendMenu(MF_SEPARATOR);
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_UK_MODIFY_UK_ELEMENT, _TB("Properties..."));
		menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popup.Detach(), _TB("Menu"));
		return TRUE;
	}

	if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP)
	{
		/*
		IDR_DOC_TREE_UK_GROUP_CONTEXTMENU MENU
		BEGIN
		POPUP "Menu"
		BEGIN
		MENUITEM "Delete Universal Key Group",  ID_DOC_TREE_UK_GROUP_REMOVE_GROUP
		MENUITEM SEPARATOR
		MENUITEM "Universal Key Group Properties", ID_DOC_TREE_UK_GROUP_MODIFY_GROUP
		MENUITEM "New Universal Key",           ID_DOC_TREE_UK_GROUP_NEW_UK_ELEMENT
		END
		END
		*/
		menu.CreateMenu();
		CMenu popup;
		popup.CreatePopupMenu();
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_UK_GROUP_REMOVE_GROUP, _TB("Delete Universal Key Group"));
		popup.AppendMenu(MF_SEPARATOR);
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_UK_GROUP_MODIFY_GROUP, _TB("Universal Key Group Properties"));
		popup.AppendMenu(MF_STRING, ID_DOC_TREE_UK_GROUP_NEW_UK_ELEMENT, _TB("New Universal Key"));
		menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)popup.Detach(), _TB("Menu"));
		return TRUE;
	}
	return FALSE;
}
//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	CRect rcTree;
	GetWindowRect(&rcTree);

	if (!rcTree.PtInRect(mousePos))
		return;

	ScreenToClient(&mousePos);
	
	HTREEITEM hItemToSelect = HitTest(mousePos);
	
	if(hItemToSelect && SelectItem(hItemToSelect))
	{
		CLocalizableMenu menu;
		CMenu* pPopup = NULL;
		
		int nItemType = GetItemType(hItemToSelect);

		if (LoadMenu(menu, nItemType))
		{
			pPopup = menu.GetSubMenu(0);
			ASSERT(pPopup);
		}
		if (pPopup)
		{
			if (m_bReadOnly)
			{
				if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_DBT)
				{
					pPopup->EnableMenuItem(ID_DOC_TREE_DBT_NEW_XREF, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					pPopup->EnableMenuItem(ID_DOC_TREE_DBT_NEW_UK_GROUP, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					pPopup->EnableMenuItem(ID_DOC_TREE_DBT_FIXED_FIELD, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					pPopup->EnableMenuItem(ID_DOC_TREE_DBT_HKL, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				}
				
				if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP)
				{
					pPopup->EnableMenuItem(ID_DOC_TREE_UK_GROUP_REMOVE_GROUP, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					pPopup->EnableMenuItem(ID_DOC_TREE_UK_GROUP_NEW_UK_ELEMENT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				}

				if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_XREF)
				{
					pPopup->EnableMenuItem(ID_DOC_TREE_XREF_REMOVE_XREF, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					pPopup->RemoveMenu(0, MF_BYPOSITION);
					pPopup->RemoveMenu(ID_DOC_TREE_XREF_EXPORT_XREF, MF_BYCOMMAND);
					CXMLDBTInfo* pDbtInfo = (CXMLDBTInfo*)GetItemData(GetParentItem(hItemToSelect));
					if (pDbtInfo)
						pPopup->EnableMenuItem(ID_DOC_TREE_XREF_HKL, MF_BYCOMMAND | MF_ENABLED);
				}
			
				if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY)
					pPopup->EnableMenuItem(ID_DOC_TREE_UK_REMOVE_UK_ELEMENT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
			}
			else 
			{
				if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_XREF)
				{
					pPopup->RemoveMenu(ID_DOC_TREE_XREF_EXPORT_XREF, MF_BYCOMMAND);
					CXMLDBTInfo* pDbtInfo = (CXMLDBTInfo*)GetItemData(GetParentItem(hItemToSelect));
					if (pDbtInfo)
						pPopup->EnableMenuItem(ID_DOC_TREE_XREF_HKL, MF_BYCOMMAND | MF_ENABLED);
				}


				if (nItemType == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP)
				{
					CXMLDBTInfo* pDbtInfo = (CXMLDBTInfo*)GetItemData(GetParentItem(hItemToSelect));
					if(pDbtInfo && pDbtInfo->GetType() != CXMLDBTInfo::MASTER_TYPE)
					{
						pPopup->EnableMenuItem(ID_DOC_TREE_UK_GROUP_REMOVE_GROUP, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
						pPopup->EnableMenuItem(ID_DOC_TREE_UK_GROUP_NEW_UK_ELEMENT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					}
				}
				if(nItemType == DOC_DESCR_TREE_ITEM_TYPE_DBT)
				{
					CXMLDBTInfo* pDbtInfo = (CXMLDBTInfo*)GetItemData(hItemToSelect);
					if(	pDbtInfo && 
						(	pDbtInfo->GetType() != CXMLDBTInfo::MASTER_TYPE || 
							pDbtInfo->GetType() == CXMLDBTInfo::MASTER_TYPE &&
							pDbtInfo->m_pXMLUniversalKeyGroup
						)
					  )
					  pPopup->EnableMenuItem(ID_DOC_TREE_DBT_NEW_UK_GROUP, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
					
					//gestione campi fissi
					if(pDbtInfo)
					{
						if (pDbtInfo->GetType() == CXMLDBTInfo::MASTER_TYPE)
							pPopup->EnableMenuItem(ID_DOC_TREE_DBT_FIXED_FIELD, MF_BYCOMMAND | MF_ENABLED);
						pPopup->EnableMenuItem(ID_DOC_TREE_DBT_HKL, MF_BYCOMMAND | MF_ENABLED);
					}
				}
			}
			ClientToScreen(&mousePos);

			pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON,	mousePos.x, mousePos.y, this);		
		}
	}
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnDoubleClick(NMHDR* pNMHDR, LRESULT* pResult) 
{
	*pResult = 0;

	HTREEITEM hSelItem = GetSelectedItem();
	if (hSelItem)
	{
		if (GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_DBT)
		{
			OnDBTProperties();		
			*pResult = 1;
			return;
		}
		
		if (GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_XREF)
		{
			OnModifyXRef();
			*pResult = 1;
			return;
		}
		
		if (GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY_GROUP)
		{
			OnUniversalKeyGroupProperties();
			*pResult = 1;
			return;
		}

		if (GetItemType(hSelItem) == DOC_DESCR_TREE_ITEM_TYPE_UNIVERSAL_KEY)
		{
			OnUniversalKeyProperties();
			*pResult = 1;
			return;
		}
	}
}

//-------------------------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnItemExpanding (NMHDR* pNMHDR, LRESULT* pResult)
{
    LPNMTREEVIEW pnmtv = (LPNMTREEVIEW)pNMHDR;

	if // la radice deve sempre risultare espansa!
	(
		pnmtv &&
		pnmtv->itemNew.hItem == GetRootItem() &&
		pnmtv->action == TVE_COLLAPSE
	)
		*pResult = 1; // Returns TRUE to prevent the list from expanding or collapsing. 
}

//-------------------------------------------------------------------------------------------
void CDocDescrTreeCtrl::EnableDBTNaming(BOOL  bEnableDBTNaming /*= TRUE*/)
{
	m_bEnableDBTNaming = bEnableDBTNaming;
}

//-------------------------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnItemBeginEdit (NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;

	if (m_bReadOnly)
		return;
	
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;
	
	if (
		GetItemType(lpDispInfo->item.hItem) == DOC_DESCR_TREE_ITEM_TYPE_XREF ||
		GetItemType(lpDispInfo->item.hItem) == DOC_DESCR_TREE_ITEM_TYPE_DBT && m_bEnableDBTNaming
		)
		*pResult = 0;
}

//-------------------------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnKeydown(NMHDR* pNMHDR, LRESULT* pResult) 
{
	TV_KEYDOWN* pTVKeyDown = (TV_KEYDOWN*)pNMHDR;
	
	//se ha premuto del elimino il profilo corrente
	if(VK_DELETE == pTVKeyDown->wVKey)
		OnRemoveXRef();

	*pResult = 0;
}

//-------------------------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnItemEndEdit (NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;
	
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	if (!(
		GetItemType(lpDispInfo->item.hItem) == DOC_DESCR_TREE_ITEM_TYPE_XREF ||
		GetItemType(lpDispInfo->item.hItem) == DOC_DESCR_TREE_ITEM_TYPE_DBT
		))
		return;

	// lpDispInfo->item.pszText punta alla stringa modificata ed è NULL se
	// non sono state apportate modifiche
	if (!lpDispInfo->item.pszText)
		return;
	
	if (!lpDispInfo->item.pszText[0])
	{
		AfxMessageBox(cwsprintf(_TB("Invalid external reference name '{0-%s}'."), lpDispInfo->item.pszText));

		*pResult = 0;

		EditLabel(lpDispInfo->item.hItem);
		
		return;
	}
	
	SetItemText(lpDispInfo->item.hItem, lpDispInfo->item.pszText);
	
	if(GetItemType(lpDispInfo->item.hItem) == DOC_DESCR_TREE_ITEM_TYPE_XREF)
	{
		if (IsXRefNameAlreadyUsed(lpDispInfo->item.hItem))
		{
			AfxMessageBox(cwsprintf(_TB("The name '{0-%s}' has already been assigned to another external reference.\r\nYou must change this setting."), (LPCTSTR)lpDispInfo->item.pszText));

			*pResult = 0;

			EditLabel(lpDispInfo->item.hItem);
			
			return;
		}

		CXMLXRefInfo* pCurrXRef = (CXMLXRefInfo*)GetItemData(lpDispInfo->item.hItem);

		pCurrXRef->m_strName = (LPCTSTR)lpDispInfo->item.pszText;
	}
	else
	{
		CXMLDBTInfo* pCurrDBT = (CXMLDBTInfo*)GetItemData(lpDispInfo->item.hItem);

		pCurrDBT->m_strTitle = (LPCTSTR)lpDispInfo->item.pszText;
	}
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnDBTProperties() 
{
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT();
	if(!pCurrDBT)
		return;

	CDBTPropertiesDlg dlg(pCurrDBT);
	dlg.DoModal();
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnNewXRef()
{
	HTREEITEM hDBTItem;
	
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT(&hDBTItem);
	if (!pCurrDBT || !hDBTItem)
		return;

	CXMLXRefInfo* pNewXRef = new CXMLXRefInfo(pCurrDBT->GetTableName());
	pNewXRef->SetOwnedByDoc(m_bDescription);

	CXMLDBTInfo* pXMLDBTInfo = GetCurrentDBT();
	CXMLFieldInfoArray* pXMLFieldInfoArray = NULL;
	if(pXMLDBTInfo) 
		pXMLFieldInfoArray = pXMLDBTInfo->GetXMLFieldInfoArray();

	CString strNewXRefName;
	strNewXRefName = pNewXRef->GetName();
	int nCount = 0;
	while(pCurrDBT->IsXRefPresent(pNewXRef->m_strName))
	{
		nCount++;
		pNewXRef->m_strName.Format(_T("%s%d"), strNewXRefName, nCount);
	}

	CXRefWizMasterDlg dlg(pNewXRef, m_pXMLDocObject, pXMLFieldInfoArray, m_bDescription);
	if(dlg.DoModal() == IDCANCEL)
	{
		if(pNewXRef)
			delete pNewXRef;
		return;
	}
//	m_bModified = TRUE;
	strNewXRefName = pNewXRef->GetName();
	nCount = 0;

	while(pCurrDBT->IsXRefPresent(pNewXRef->m_strName))
	{
		nCount++;
		pNewXRef->m_strName.Format(_T("%s%d"), strNewXRefName, nCount);
	}
	
	pCurrDBT->AddXRef(pNewXRef);
	
	HTREEITEM hNewXRefItem = InsertXRefItem(pNewXRef, hDBTItem);
	for(int i = 0 ; i < pNewXRef->GetSegmentsNum(); i++)
		InsertXRefSegmentItem(pNewXRef->GetSegmentAt(i), hNewXRefItem);

	//controllo che il nuovo xref possegga un nome diverso da quello degli altri xref 
	//presenti nel dbt
	if (IsXRefNameAlreadyUsed(hNewXRefItem))
	{
		AfxMessageBox(cwsprintf(_TB("The name '{0-%s}' has already been assigned to another external reference.\r\nYou must change this setting."), (LPCTSTR)pNewXRef->m_strName));
		Select(hNewXRefItem, TVGN_CARET);
		SetItemText(hNewXRefItem, _T(""));
		EditLabel(hNewXRefItem);
	}
}


//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnAppendXRefs()
{
	HTREEITEM hDBTItem;	
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT(&hDBTItem);
	if (!pCurrDBT || !hDBTItem)
		return;
	
	CStringArray arExistDocNamespaces;
	CStringArray arSelectedDocNamespaces;
	
	if (pCurrDBT->m_pXRefsToAppendArray)
	{
		for(int idx = 0; idx < pCurrDBT->m_pXRefsToAppendArray->GetSize(); idx++)
		{
			if (pCurrDBT->m_pXRefsToAppendArray->GetAt(idx))
				arExistDocNamespaces.Add(pCurrDBT->m_pXRefsToAppendArray->GetAt(idx)->m_strDocNamespace);
		}
	}

	CAppendXRefDialog aAppendXRefDlg(&arExistDocNamespaces, &arSelectedDocNamespaces);
	if (aAppendXRefDlg.DoModal() == IDOK)
	{
		for(int i = 0; i < arSelectedDocNamespaces.GetSize(); i++)
		{
			if (arSelectedDocNamespaces.GetAt(i).IsEmpty())
				continue;

			CXMLXReferencesToAppend* extRefsToAdd = new CXMLXReferencesToAppend();
			if (extRefsToAdd->Parse(arSelectedDocNamespaces.GetAt(i), pCurrDBT->GetTableName()))
			{
				pCurrDBT->AddXReferencesToAppend(extRefsToAdd);
				for (int j = 0; j <= extRefsToAdd->m_pXRefsToAppendArray->GetUpperBound(); j++)
				{
					CXMLXRefInfo* pNewXRef = extRefsToAdd->m_pXRefsToAppendArray->GetAt(j);
					pNewXRef->SetAppended(TRUE);
					pCurrDBT->AddXRef(pNewXRef);	
					if (pNewXRef->m_bSubjectTo)
						extRefsToAdd->SubstituteTableName(pNewXRef, _T("TableName"), pCurrDBT->GetTableName());


					HTREEITEM hNewXRefItem = InsertXRefItem(pNewXRef, hDBTItem);
					for(int k = 0 ; k < pNewXRef->GetSegmentsNum(); k++)
						InsertXRefSegmentItem(pNewXRef->GetSegmentAt(k), hNewXRefItem);
				}
			}				
		}
	}	
	arSelectedDocNamespaces.RemoveAll();
	arExistDocNamespaces.RemoveAll();
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnModifyXRef()
{
	HTREEITEM hXRefItem;
	
	CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);
	if (!pCurrXRef || !hXRefItem)
		return;

	if(!m_bDescription && !pCurrXRef->HasValidRefDoc())
	{
		AfxMessageBox(_TB("Unable to change or use the External Reference since the referenced business object does not exist in the system."));
		return;
	}

	CXMLXRefInfo aCopyCurrXRef(*pCurrXRef);

	CXMLDBTInfo* pXMLDBTInfo = GetCurrentDBT();
	CXMLFieldInfoArray* pXMLFieldInfoArray = NULL;
	if(pXMLDBTInfo) 
		pXMLFieldInfoArray = pXMLDBTInfo->GetXMLFieldInfoArray();
	
	CXRefWizMasterDlg dlg(&aCopyCurrXRef, m_pXMLDocObject, pXMLFieldInfoArray, m_bDescription);
	if(dlg.DoModal() != IDCANCEL)
	{
		//m_bModified = TRUE;
		*pCurrXRef = aCopyCurrXRef;

		SetItemText(hXRefItem, pCurrXRef->m_strName);

		UpdateXRefIcon(hXRefItem, pCurrXRef);

		RemoveTreeChilds(hXRefItem);
	
		for(int i = 0 ; i < pCurrXRef->GetSegmentsNum() ; i++)
			InsertXRefSegmentItem(pCurrXRef->GetSegmentAt(i), hXRefItem);
		
		if (IsXRefNameAlreadyUsed(hXRefItem))
		{
			AfxMessageBox(cwsprintf(_TB("The name '{0-%s}' has already been assigned to another external reference.\r\nYou must change this setting."), (LPCTSTR)pCurrXRef->m_strName));
			Select(hXRefItem, TVGN_CARET);
			SetItemText(hXRefItem, _T(""));
			EditLabel(hXRefItem);
		}
	}
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl:: OnRemoveXRef()
{
	HTREEITEM hXRefItem;
	
	CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);
	if (!pCurrXRef || !hXRefItem)
		return;
	
	if(pCurrXRef->IsOwnedByDoc() && !m_bDescription)
		return;

	CXMLDBTInfo* pCurrDBT = (CXMLDBTInfo*)GetItemData(GetParentItem(hXRefItem));
	if(pCurrDBT)
		pCurrDBT->RemoveXRef(*pCurrXRef);

	DeleteItem(hXRefItem);
	//m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnRemoveUniversalKeyGroup()
{
	HTREEITEM hUKGroupItem;
	
	CXMLUniversalKeyGroup* pXMLUniversalKeyGroup = GetCurrentUniversalKeyGroup(&hUKGroupItem);
	if(!pXMLUniversalKeyGroup)
	{
		ASSERT(FALSE);
		return;
	}
	
	CXMLDBTInfo* pCurrDBT = (CXMLDBTInfo*)GetItemData(GetParentItem(hUKGroupItem));
	if(pCurrDBT && pCurrDBT->GetType() == CXMLDBTInfo::MASTER_TYPE && !m_bReadOnly)
	{
		delete pCurrDBT->m_pXMLUniversalKeyGroup;
		pCurrDBT->m_pXMLUniversalKeyGroup = NULL;
	}

	DeleteItem(hUKGroupItem);
	//m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnUniversalKeyGroupProperties()
{
	HTREEITEM hUKGroupItem, hDBTItem;
	CXMLUniversalKeyGroup* pCurrUKGroup = GetCurrentUniversalKeyGroup(&hUKGroupItem);
	if(hUKGroupItem)
		hDBTItem = GetParentItem(hUKGroupItem);
	
	if (!pCurrUKGroup || !hUKGroupItem || !hDBTItem)
	{
		ASSERT(FALSE);
		return;
	}
	
	CXMLUniversalKeyGroup aCopyXMLUniversalKeyGroup(*pCurrUKGroup);

	CUniversalKeyGroupDlg dlg(&aCopyXMLUniversalKeyGroup);
	if(dlg.DoModal() == IDOK && *pCurrUKGroup != aCopyXMLUniversalKeyGroup)
	{
		*pCurrUKGroup = aCopyXMLUniversalKeyGroup;

		DeleteItem(hUKGroupItem);

		hUKGroupItem = InsertUniversalKeyGroupItem(pCurrUKGroup, szUKGroupName, hDBTItem);

		for (int nUKIdx = 0 ; nUKIdx < pCurrUKGroup->GetSize() ; nUKIdx++)
		{
			CXMLUniversalKey* pXMLUniversalKey = pCurrUKGroup->GetAt(nUKIdx);
			if(!pXMLUniversalKey)
				continue;
			
			HTREEITEM hUKItem = InsertUniversalKeyItem(pXMLUniversalKey, pXMLUniversalKey->GetName(), hUKGroupItem);
		
			if (!hUKItem)
				continue;

			for(int nSegIdx = 0 ; nSegIdx < pXMLUniversalKey->GetSegmentNumber() ; nSegIdx++)
			{
				CString strSeg = pXMLUniversalKey->GetSegmentAt(nSegIdx);
				if(strSeg.IsEmpty())
					continue;

				InsertUniversalKeySegmentItem(strSeg,	hUKItem);
			}
		}

		//m_bModified = TRUE;
	}
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnNewUniversalKeyGroup()
{
	if(!m_nsDocument.IsValid())
	{
		ASSERT(FALSE);
		return;
	}

	HTREEITEM hDBTItem;
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT(&hDBTItem);
	if (!pCurrDBT || !hDBTItem || pCurrDBT->m_pXMLUniversalKeyGroup)
	{
		ASSERT(FALSE);
		return;
	}

	pCurrDBT->m_pXMLUniversalKeyGroup = new CXMLUniversalKeyGroup;

	CUniversalKeyGroupDlg dlg(pCurrDBT->m_pXMLUniversalKeyGroup);
	if(dlg.DoModal() == IDOK)
	{
		InsertUniversalKeyGroupItem(pCurrDBT->m_pXMLUniversalKeyGroup, szUKGroupName, hDBTItem);
		//m_bModified = TRUE;
	}
	else
	{
		delete pCurrDBT->m_pXMLUniversalKeyGroup;
		pCurrDBT->m_pXMLUniversalKeyGroup = NULL;
	}
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnNewUniversalKey()
{
	HTREEITEM hUKGroupItem;
	
	CXMLUniversalKeyGroup* pXMLUniversalKeyGroup = GetCurrentUniversalKeyGroup(&hUKGroupItem);
	if(!pXMLUniversalKeyGroup)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLDBTInfo* pCurrXMLDBTInfo = (CXMLDBTInfo*)(GetItemData(GetParentItem(hUKGroupItem)));
	if(!pCurrXMLDBTInfo)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLUniversalKey* pNewUK = new CXMLUniversalKey();

	CUniversalKeyDlg dlg(pNewUK, pCurrXMLDBTInfo->GetTableName(), m_bDescription);
	if(dlg.DoModal() == IDCANCEL)
	{
		if(pNewUK)
			delete pNewUK;
		return;
	}

	//m_bModified = TRUE;

	CString strUKName = pNewUK->GetName();
	CString strTmpUKName = strUKName;

	int n = 0;
	while(pXMLUniversalKeyGroup->IsPresent(strTmpUKName))
	{
		strTmpUKName.Format(_T("%s_%d"), strUKName, n);
		n++;
	}
	
	pNewUK->SetName(strTmpUKName);
		
	pXMLUniversalKeyGroup->Add(pNewUK);
	
	HTREEITEM hNewUKItem = InsertUniversalKeyItem(pNewUK, strTmpUKName, hUKGroupItem);
	
	for(int i = 0 ; i < pNewUK->GetSegmentNumber(); i++)
		InsertUniversalKeySegmentItem(pNewUK->GetSegmentAt(i), hNewUKItem);
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnUniversalKeyProperties()
{
	HTREEITEM hUKItem, hUKGroupItem;
	
	CXMLUniversalKey* pCurrUK = GetCurrentUniversalKey(&hUKItem);
	if (!pCurrUK || !hUKItem)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLUniversalKey aCopyCurrUK(*pCurrUK);

	CXMLUniversalKeyGroup* pXMLUniversalKeyGroup = GetCurrentUniversalKeyGroup(&hUKGroupItem);
	if(!pXMLUniversalKeyGroup)
	{
		ASSERT(FALSE);
		return;
	}

	CXMLDBTInfo* pCurrXMLDBTInfo = (CXMLDBTInfo*)(GetItemData(GetParentItem(hUKGroupItem)));
	if(!pCurrXMLDBTInfo)
	{
		ASSERT(FALSE);
		return;
	}

	//devo passargli la lista dei campi della tabella del master
	CUniversalKeyDlg dlg(&aCopyCurrUK, pCurrXMLDBTInfo->GetTableName(), m_bReadOnly);
	if(dlg.DoModal() != IDCANCEL && *pCurrUK != aCopyCurrUK)
	{
		//m_bModified = TRUE;
	
		if(aCopyCurrUK.GetName() != pCurrUK->GetName())
		{
			CString strUKName		= aCopyCurrUK.GetName();
			CString strTmpUKName	= aCopyCurrUK.GetName();

			int n = 0;
			while(pXMLUniversalKeyGroup->IsPresent(strTmpUKName))
			{
				strTmpUKName.Format(_T("%s_%d"), strUKName, n);
				n++;
			}

			pCurrUK->SetName(strTmpUKName);
			SetItemText(hUKItem, strTmpUKName);
		}

		*pCurrUK = aCopyCurrUK;
	
		RemoveTreeChilds(hUKItem);

		for(int i = 0 ; i < pCurrUK->GetSegmentNumber() ; i++)
			InsertUniversalKeySegmentItem(pCurrUK->GetSegmentAt(i), hUKItem);
	}
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl:: OnRemoveUniversalKey()
{
	HTREEITEM hUKItem;
	
	CXMLUniversalKey* pCurrUK = GetCurrentUniversalKey(&hUKItem);
	if (!pCurrUK || !hUKItem)
		return;
	
	CXMLUniversalKeyGroup* pXMLUniversalKeyGroup = (CXMLUniversalKeyGroup*)GetItemData(GetParentItem(hUKItem));
	if(pXMLUniversalKeyGroup)
		pXMLUniversalKeyGroup->Remove(pCurrUK);

	DeleteItem(hUKItem);
	//m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnFixedField()
{
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT();
	if(!pCurrDBT)
		return;

	CFixedFieldDlg dlg(pCurrDBT);
	dlg.DoModal();
}
//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnDBTHotKeyLink()
{
	CXMLDBTInfo* pCurrDBT = GetCurrentDBT();
	if(!pCurrDBT)
		return;

	CHotKeyLinkDlg dlg(pCurrDBT, m_pXMLDocObject);
	dlg.DoModal();
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::OnXRefHotKeyLink()
{
	HTREEITEM hXRefItem;
	CXMLXRefInfo* pCurrXRef = GetCurrentXRef(&hXRefItem);
	
	if(!pCurrXRef || !hXRefItem)
		return;

	CXMLDBTInfo* pCurrDBT = (CXMLDBTInfo*)GetItemData(GetParentItem(GetSelectedItem()));
	if (!pCurrDBT)
		return;
	CXMLDocObjectInfo aExtDocInfo(pCurrXRef->GetDocumentNamespace());
	aExtDocInfo.LoadAllFiles();
	CHotKeyLinkDlg dlg(pCurrDBT, m_pXMLDocObject, pCurrXRef, &aExtDocInfo);
	dlg.DoModal();
}


//----------------------------------------------------------------------------
BOOL CDocDescrTreeCtrl::IsXRefNameAlreadyUsed(HTREEITEM hXRefItem) const
{
	if (GetItemType(hXRefItem) != DOC_DESCR_TREE_ITEM_TYPE_XREF)
		return FALSE;

   HTREEITEM hPrevSiblingItem = GetPrevSiblingItem(hXRefItem);
   while (hPrevSiblingItem)
   {
		if (!GetItemText(hPrevSiblingItem).Compare(GetItemText(hXRefItem)))
			return TRUE;
		hPrevSiblingItem = GetPrevSiblingItem(hPrevSiblingItem);
   }


   HTREEITEM hNextSiblingItem = GetNextSiblingItem(hXRefItem);
   while (hNextSiblingItem)
   {
		if (!GetItemText(hNextSiblingItem).Compare(GetItemText(hXRefItem)))
			return TRUE;
		hNextSiblingItem = GetNextSiblingItem(hNextSiblingItem);
   }
   return FALSE;
}

//----------------------------------------------------------------------------
void CDocDescrTreeCtrl::SetReadOnly(BOOL bReadOnly	/*= FALSE*/)
{
	if(m_bReadOnly == bReadOnly)
		return;

	m_bReadOnly = bReadOnly;
}

//---------------------------------------------------------------------------
// CSegmentsGrid
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSegmentsGrid, CBCGPListCtrl)
	//{{AFX_MSG_MAP(CSegmentsGrid)
	ON_NOTIFY_REFLECT	(LVN_KEYDOWN, OnKeyDown)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//---------------------------------------------------------------------------
CSegmentsGrid::CSegmentsGrid(LPCTSTR lpszTableName /*= NULL*/, CXMLXRefInfo* pXRef /*= NULL*/)
	:
	CBCGPListCtrl	(),
	m_pXRef		(pXRef)
{
}

//---------------------------------------------------------------------------
CString	CSegmentsGrid::GetCurrDBT ()
{
	return m_sCurrDBTNs;
}

//---------------------------------------------------------------------------
void CSegmentsGrid::SetCurrDBT (const CString& strNs)
{
	m_sCurrDBTNs = strNs;
}

//---------------------------------------------------------------------------
void CSegmentsGrid::PreSubclassWindow() 
{
	SetExtendedStyle(LVS_REPORT|LVS_EX_FULLROWSELECT);

	CRect rcGrid;
	GetClientRect(&rcGrid);
	int nColWidth = rcGrid.Width() / 2;
	InsertSegmentColumn(0, nColWidth, m_pXRef ? m_pXRef->GetTableName() : (LPCTSTR)_TB("Foreign key"));
	InsertSegmentColumn(1, nColWidth, m_pXRef ? m_pXRef->GetReferencedTableName() : (LPCTSTR)_TB("Referenced key"));
	InsertSegmentColumn(2, nColWidth, (LPCTSTR)_TB("Fixed value"));
	
	SetBkColor(CLR_WHITE);
	SetTextBkColor(CLR_WHITE);

	CBCGPListCtrl::PreSubclassWindow();
}

//-------------------------------------------------------------------------------
void CSegmentsGrid::OnKeyDown(NMHDR* pNMHDR, LRESULT* pResult) 
{
	LPNMLVKEYDOWN lpKeyDown = (LPNMLVKEYDOWN)pNMHDR;
	if (!lpKeyDown)
	{
		ASSERT(FALSE);
		return;
	}
	if (lpKeyDown->wVKey == VK_DELETE)
		RemoveSelectedRow();
	
	*pResult = 0;
}

//----------------------------------------------------------------------------
BOOL CSegmentsGrid::ModifySegmentAt(int nRowIdx, LPCTSTR lpszFKSeg, LPCTSTR lpszReferencedSeg, LPCTSTR lpszFKFixedValue)
{
	if (nRowIdx < 0 || nRowIdx >= GetRowCount())
		return FALSE;
	
	BOOL bFKModified = (GetFKSegmentAt(nRowIdx).CompareNoCase(lpszFKSeg) != 0);
	BOOL bRefModified = (GetReferencedSegmentAt(nRowIdx).CompareNoCase(lpszReferencedSeg) != 0);
	BOOL bValModified = (GetFKFixedValueAt(nRowIdx).CompareNoCase(lpszFKFixedValue) != 0);

	if (!bFKModified && !bRefModified && !bValModified)
		return TRUE;

	for ( int nSegIdx = 0 ; nSegIdx < GetRowCount() ; nSegIdx++)
	{
		CString strFK = GetFKSegmentAt(nSegIdx);
		if(bFKModified && !strFK.CompareNoCase(lpszFKSeg))
		{
			AfxMessageBox(_TB("Referenced key segment already defined"));
			return FALSE;
		}
		CString strRef = GetReferencedSegmentAt(nSegIdx);
		if (bRefModified && !strRef.CompareNoCase(lpszReferencedSeg))
		{
			AfxMessageBox(_TB("Foreign key segment already defined"));
			return FALSE;
		}
	}

	if 
		(
			!SetFKSegmentAt			(nRowIdx, lpszFKSeg) ||
			!SetReferencedSegmentAt	(nRowIdx, lpszReferencedSeg) ||
			!SetFKFixedValueAt		(nRowIdx, lpszFKFixedValue)
		)
		return FALSE;
	
	if (m_pXRef)
	{
		CXMLSegmentInfo* pSeg = m_pXRef->GetSegmentAt(nRowIdx);
		if(pSeg)
			pSeg->SetKeySegments(lpszFKSeg, lpszReferencedSeg, lpszFKFixedValue);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CSegmentsGrid::InsertNewSegment(LPCTSTR lpszFKSeg, LPCTSTR lpszReferencedSeg, LPCTSTR lpszFKFixedValue)
{
	if 
		(
			!lpszFKSeg || !lpszFKSeg[0] ||
			!lpszReferencedSeg || !lpszReferencedSeg[0] ||
			!lpszFKFixedValue 
		)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// se il segmento lpszFKSeg -> lpszReferencedSeg non e' gia' presente 
	// nell'external reference lo inserisce	
	for ( int nSegIdx = 0 ; nSegIdx < GetRowCount() ; nSegIdx++)
	{
		CString strFK = GetFKSegmentAt(nSegIdx);
		if(strFK.CompareNoCase(lpszFKSeg) == 0)
		{
			AfxMessageBox(_TB("Referenced key segment already defined"));
			return FALSE;
		}
		CString strReferencedSeg = GetReferencedSegmentAt(nSegIdx);
		if(strReferencedSeg.CompareNoCase(lpszReferencedSeg) == 0)
		{
			AfxMessageBox(_TB("Foreign key segment already defined"));
			return FALSE;
		}
	}
	
	int nInsIdx = InsertItem(GetRowCount(), lpszFKSeg);
	if 
		(
			nInsIdx < 0 ||
			!SetFKSegmentAt(nInsIdx, lpszFKSeg) ||
			!SetReferencedSegmentAt(nInsIdx, lpszReferencedSeg) ||
			!SetFKFixedValueAt		(nInsIdx, lpszFKFixedValue)
		)
		return FALSE;
	
	
	if (m_pXRef)
	{
		CXMLSegmentInfo* pSeg = new CXMLSegmentInfo(m_pXRef, lpszFKSeg, lpszReferencedSeg, lpszFKFixedValue);
		m_pXRef->AddSegment(pSeg);
	}
	
	return TRUE;
}

//---------------------------------------------------------------------------
void CSegmentsGrid::AttachXRef(CXMLXRefInfo* pXRef)
{
	m_pXRef = pXRef;
	
	DeleteAllItems();

	if(!m_pXRef)
		return;
	
	SetColumnTitle(0, m_pXRef->GetTableName());
	SetColumnTitle(1, m_pXRef->GetReferencedTableName());
	
	if (!m_pXRef->GetSegmentsNum())
		return;

	for (int nSegIdx = 0 ; nSegIdx < m_pXRef->GetSegmentsNum() ; nSegIdx++)
	{
		CXMLSegmentInfo* pSegInfo = m_pXRef->GetSegmentAt(nSegIdx);
		
		if(!pSegInfo)
			continue;

		/*int nInsIdx = */ InsertItem(nSegIdx, pSegInfo->GetFKSegment());
		//SetItemData(nInsIdx, (DWORD)pSegInfo);
		
		SetReferencedSegmentAt	(nSegIdx, pSegInfo->GetReferencedSegment());
		SetFKFixedValueAt		(nSegIdx, pSegInfo->GetFKStrFixedValue());
	}
}

//---------------------------------------------------------------------------
void CSegmentsGrid::SetColumnTitle(int nCol, LPCTSTR lpszTitle)
{
	if(!::IsWindow(m_hWnd) || nCol < 0 || nCol >=  GetColumnCount())
		return;

	LVCOLUMN lvColumn;
	lvColumn.mask = LVCF_TEXT ;

	lvColumn.pszText = (LPTSTR)lpszTitle;
	lvColumn.cchTextMax =  lpszTitle ? _tcslen(lpszTitle) : 1;
	lvColumn.pszText = lpszTitle ? (LPTSTR)lpszTitle : _T("\0");

	SetColumn(nCol, &lvColumn);
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::SetFKSegmentAt(int nIdx, LPCTSTR lpszKey)
{
	if( nIdx > GetItemCount())
		return FALSE;
	
	SetItemText(nIdx, 0, lpszKey);

	return TRUE;
}

//---------------------------------------------------------------------------
CString CSegmentsGrid::GetFKSegmentAt(int nRowIdx) const
{
	if (nRowIdx < 0 || nRowIdx >= GetRowCount())
		return _T("");

	return GetItemText(nRowIdx, 0);
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::SetReferencedSegmentAt(int nIdx, LPCTSTR lpszKey)
{
	if(nIdx > GetItemCount())
		return FALSE;
	
	SetItemText(nIdx, 1, lpszKey);

	return TRUE;
}

//---------------------------------------------------------------------------
CString CSegmentsGrid::GetReferencedSegmentAt(int nRowIdx) const
{
	if (nRowIdx < 0 || nRowIdx >= GetRowCount())
		return _T("");

	return GetItemText(nRowIdx, 1);
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::SetFKFixedValueAt(int nIdx, LPCTSTR lpszVal)
{
	if(nIdx > GetItemCount())
		return FALSE;
	
	SetItemText(nIdx, 2, lpszVal);

	return TRUE;
}

//---------------------------------------------------------------------------
CString CSegmentsGrid::GetFKFixedValueAt(int nRowIdx) const
{
	if (nRowIdx < 0 || nRowIdx >= GetRowCount())
		return _T("");

	return GetItemText(nRowIdx, 2);
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::InsertSegmentColumn(int nCol, int nWidth, LPCTSTR lpszColumnHeading)
{
	return (InsertColumn(nCol, lpszColumnHeading, LVCFMT_LEFT, nWidth) >= 0);
}

//---------------------------------------------------------------------------
int	CSegmentsGrid::GetRowCount() const
{
	return GetItemCount();
}

//---------------------------------------------------------------------------
int	CSegmentsGrid::GetColumnCount()
{
	return GetHeaderCtrl().GetItemCount();
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::SelectSegment(LPCTSTR lpszFKToSel) 
{
	if (!lpszFKToSel || !lpszFKToSel[0])
		return FALSE;

	for ( int nSegIdx = 0 ; nSegIdx < GetRowCount() ; nSegIdx++)
	{
		CString strFK = GetFKSegmentAt(nSegIdx);

		if(strFK.CompareNoCase(lpszFKToSel) == 0)
			return SetItemState(nSegIdx, LVIS_SELECTED, LVIS_SELECTED);
	}
	return FALSE;
}

//---------------------------------------------------------------------------
int	CSegmentsGrid::GetSelectedRowIdx() const
{
	return GetNextItem(-1, LVNI_SELECTED);
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::GetSelectedRowRect(CRect& rcItem) const
{
	int nSelSegmentIdx = GetSelectedRowIdx();
	if(nSelSegmentIdx == -1)
		return FALSE;

	return GetItemRect(nSelSegmentIdx, (LPRECT)&rcItem, LVIR_BOUNDS);
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::RemoveSelectedRow()
{
	return RemoveRowAt(GetSelectedRowIdx());
}

//---------------------------------------------------------------------------
BOOL CSegmentsGrid::RemoveRowAt(int nRowIdx)
{
	if (nRowIdx < 0 || nRowIdx >= GetRowCount())
		return FALSE;

	int nXRefSegIdx = m_pXRef ? m_pXRef->GetSegmentIdx(CXMLSegmentInfo(m_pXRef, (LPCTSTR)GetFKSegmentAt(nRowIdx), (LPCTSTR)GetReferencedSegmentAt(nRowIdx))) : -1;
	//int nXRefSegIdx = m_pXRef ? m_pXRef->GetSegmentIdx(*GetItemData(nRowIdx)) : -1;
	
	if (!DeleteItem(nRowIdx))
		return FALSE;
	
	if (m_pXRef && nXRefSegIdx >= 0)
		m_pXRef->RemoveSegmentAt(nXRefSegIdx);

	return TRUE;
}
//////////////////////////////////////////////////////////////////////////////
//             class CProfileCombo implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE (CProfileCombo, CBCGPComboBox)

//-----------------------------------------------------------------------------
CProfileCombo::CProfileCombo()
{

}

//-----------------------------------------------------------------------------
void CProfileCombo::SetDocumentNameSpace(const CTBNamespace& nsDocument, const CStringArray& aProfileList)
{
	if (nsDocument == m_nsDocument)
		return;

	m_nsDocument = nsDocument;
	ResetContent();
	for (int i = 0 ; i < aProfileList.GetSize() ; i++ )
		AddString((LPCTSTR)aProfileList.GetAt(i));
}

//-----------------------------------------------------------------------------
int CProfileCombo::SelectProfile(LPCTSTR lpszProfileToSel /* = NULL*/)
{
	if (!lpszProfileToSel || !lpszProfileToSel[0])
	{
		SetCurSel(-1);
		return CB_ERR;
	}

	return SelectString(-1, lpszProfileToSel);
}

//-----------------------------------------------------------------------------
CString CProfileCombo::GetSelectedProfile() const
{
	CString strSelectedProfileName;

	strSelectedProfileName.Empty();

	int nProfileIdx = GetCurSel();
	if (nProfileIdx != CB_ERR)
		GetLBText(nProfileIdx, strSelectedProfileName);
	
	return strSelectedProfileName;
}



//----------------------------------------------------------------------------------------------
//	CAppendXRefDialog
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CAppendXRefDialog, CLocalizableDialog)
//----------------------------------------------------------------------------------------------
CAppendXRefDialog::CAppendXRefDialog(CStringArray*	pExistingDocNamespaces, CStringArray* pSelectedDocNamespaces)
	:
	CLocalizableDialog(IDD_DBT_APPEND_XREFS, NULL),
	m_pExistingDocNamespaces(pExistingDocNamespaces),
	m_pSelectedDocNamespaces(pSelectedDocNamespaces)
{
}

//----------------------------------------------------------------------------------------------
BOOL CAppendXRefDialog::OnInitDialog	()
{
	CLocalizableDialog::OnInitDialog();

	//riempio lista campi
	FillListBox();
	return TRUE;
}

//----------------------------------------------------------------------------
void CAppendXRefDialog::FillListBox()
{
	CListBox* listBox = (CListBox*)GetDlgItem(IDC_APPEND_XREFS_DOC_LIST);		

	CString strContainerPath = AfxGetPathFinder()->GetContainerPath(CPathFinder::TB_APPLICATION);
	CStringArray arAppFolders;
	AfxGetFileSystemManager()->GetSubFolders(strContainerPath, &arAppFolders); //application folders list

	if (strContainerPath.Right(1) != SLASH_CHAR) 
		strContainerPath += SLASH_CHAR;

	for (int i=0; i <= arAppFolders.GetUpperBound(); i++)
	{  
		CString strAppName = arAppFolders.GetAt(i);
		CString strAppFolder = strContainerPath + strAppName + SLASH_CHAR; //single application
		CStringArray arModFolders;
		AfxGetFileSystemManager()->GetSubFolders(strAppFolder, &arModFolders); //module folders list
		for (int j=0; j <= arModFolders.GetUpperBound(); j++)
		{
			CString strModName = arModFolders.GetAt(j);		
			CString strModFolder = strAppFolder + strModName + SLASH_CHAR; //single module
			CString strModObjsFolder = strModFolder + _T("ModuleObjects")  + SLASH_CHAR;
			if (AfxGetFileSystemManager()->ExistPath(strModObjsFolder))
			{
				CStringArray arDocuments;
				AfxGetFileSystemManager()->GetSubFolders(strModObjsFolder, &arDocuments); //documents folders list
	
				for (int k=0; k <= arDocuments.GetUpperBound(); k++)
				{
					CString strDocName = arDocuments.GetAt(k);
					CString strAppendXRefFile = strModObjsFolder + strDocName + SLASH_CHAR + "Description\\ExtReferencesToAppend.xml"; //description single document
					if (AfxGetFileSystemManager()->ExistFile(strAppendXRefFile))
					{
						CString docNamespace = strAppName + _T(".") +  strModName + _T(".") + strDocName;
						BOOL bExist = FALSE;
						//prima controllo che non sia già presente nella lista dei namespace già gestiti dal dbt
						for (int nIdx = 0; nIdx <= m_pExistingDocNamespaces->GetUpperBound(); nIdx++)
						{
							if (!m_pExistingDocNamespaces->GetAt(nIdx).IsEmpty() && docNamespace.CompareNoCase(m_pExistingDocNamespaces->GetAt(nIdx)) == 0)
							{
								bExist = TRUE;
								break;
							}
						}

						if (!bExist) listBox->AddString(docNamespace);
					}		
				}
			}			
		}		
	}
}

//----------------------------------------------------------------------------------------------
void CAppendXRefDialog::OnOK()
{
	CListBox* listBox = (CListBox*)GetDlgItem(IDC_APPEND_XREFS_DOC_LIST);		
	CString strEvent;
	CString strDocNamespace;		
	int*	pIndexSel;
	
	int nSelItems = listBox->GetSelCount();
   	if (nSelItems > 0)
	{	
		pIndexSel = new int[nSelItems];
		listBox->GetSelItems (nSelItems, pIndexSel);

		for (int i = nSelItems - 1; i >= 0; i--)
		{
			listBox->GetText(pIndexSel[i], strDocNamespace);
			m_pSelectedDocNamespaces->Add(strDocNamespace);
		}
		delete [] pIndexSel;
	}
	CLocalizableDialog::OnOK();
}
