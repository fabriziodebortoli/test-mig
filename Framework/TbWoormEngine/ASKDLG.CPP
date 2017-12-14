
#include "stdafx.h"

#include <stdio.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\EnumsTable.h>

#include <TbParser\TokensTable.h>

#include <TbGenlib\Baseapp.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\const.h>
#include <TbGenlib\Hlinkobj.h>

#include "ActionsRepEngin.h"
#include "inputmng.h"
#include "askdata.h"
#include "prgdata.h"
#include "rpsymtbl.h"
#include "askdlg.h"
#include "report.h"
#include "edtmng.h"

//............................... Resources
#include "askdlg.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define DEFAULT_CAPTION_POS		0
#define LEFT_CAPTION_POS		1
#define TOP_CAPTION_POS			2

static const TCHAR szCheckBoxStyle	[] = _T("Check Box");
static const TCHAR szComboStyle		[] = _T("Combo");
static const TCHAR szEditStyle		[] = _T("Edit");
static const TCHAR szRadioStyle		[] = _T("Radio Button");


//============================================================================
// 	class CAskRuleDlg
//============================================================================

IMPLEMENT_DYNAMIC(CAskRuleDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CAskRuleDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAskRuleDlg)
	ON_BN_CLICKED		(IDC_BTN_ADD_DIALOG,	OnBtnAddDialog)
	ON_BN_CLICKED		(IDC_BTN_MOD_DIALOG,	OnBtnModDialog)
	ON_BN_CLICKED		(IDC_BTN_DEL_DIALOG,	OnBtnDelDialog)
	ON_BN_CLICKED		(IDC_BTN_DIALOG_UP,		OnBtnDialogUp)
	ON_BN_CLICKED		(IDC_BTN_DIALOG_DOWN,	OnBtnDialogDown)

	ON_BN_CLICKED		(IDC_BTN_ADD_GROUP,		OnBtnAddGroup)
	ON_BN_CLICKED		(IDC_BTN_MOD_GROUP,		OnBtnModGroup)
	ON_BN_CLICKED		(IDC_BTN_DEL_GROUP,		OnBtnDelGroup)
	ON_BN_CLICKED		(IDC_BTN_GROUP_UP,		OnBtnGroupUp)
	ON_BN_CLICKED		(IDC_BTN_GROUP_DOWN,	OnBtnGroupDown)

	ON_BN_CLICKED		(IDC_BTN_ADD_FIELD,		OnBtnAddField)
	ON_BN_CLICKED		(IDC_BTN_MOD_FIELD,		OnBtnModField)
	ON_BN_CLICKED		(IDC_BTN_DEL_FIELD,		OnBtnDelField)
	ON_BN_CLICKED		(IDC_BTN_FIELD_UP,		OnBtnFieldUp)
	ON_BN_CLICKED		(IDC_BTN_FIELD_DOWN,	OnBtnFieldDown)

	ON_BN_CLICKED		(IDC_BTN_PREVIEW,		OnBtnPreView)

	ON_LBN_SELCHANGE	(IDC_LIST_DIALOG,		OnLbnDialog)
	ON_LBN_SELCHANGE	(IDC_LIST_GROUP,		OnLbnGroup)
	ON_LBN_SELCHANGE	(IDC_LIST_FIELD,		OnLbnField)

	ON_LBN_DBLCLK		(IDC_LIST_DIALOG,		OnBtnModDialog)
	ON_LBN_DBLCLK		(IDC_LIST_GROUP,		OnBtnModGroup)
	ON_LBN_DBLCLK		(IDC_LIST_FIELD,		OnBtnModField)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAskRuleDlg::CAskRuleDlg (ProgramData* pPrgData, CBaseDocument *pDocument)
	:
	CParsedDialog	(IDD_ASKRULE),
	m_pPrgData		(pPrgData),
	m_pCurAskDialog	(NULL),
	m_pCurAskGroup	(NULL),
	m_pCurAskField	(NULL),
	m_bModified		(FALSE),
	m_pDocument		(pDocument)
{
	if (m_pPrgData)
    {
		m_pAskRule	= m_pPrgData->GetAskRuleData();
		m_pSymTable	= m_pPrgData->GetSymTable();
	}
	else
    {
		m_pAskRule	= NULL;
		m_pSymTable	= NULL;
	}
}

//----------------------------------------------------------------------------
BOOL CAskRuleDlg::OnInitDialog()
{           
	ASSERT(m_pAskRule && m_pSymTable);
	           
	CParsedDialog::OnInitDialog();
	
	m_lbDialog.		SubclassDlgItem(IDC_LIST_DIALOG,	this);
	m_lbGroup.		SubclassDlgItem(IDC_LIST_GROUP,		this);
	m_lbField.		SubclassDlgItem(IDC_LIST_FIELD,		this);

	//m_btnAddDialog.	SubclassDlgItem(IDC_BTN_ADD_DIALOG,	this);
	//m_btnModDialog.	SubclassDlgItem(IDC_BTN_MOD_DIALOG,	this);
	//m_btnDelDialog.	SubclassDlgItem(IDC_BTN_DEL_DIALOG,	this);

	//m_btnAddGroup.	SubclassDlgItem(IDC_BTN_ADD_GROUP,	this);
	//m_btnModGroup.	SubclassDlgItem(IDC_BTN_MOD_GROUP,	this);
	//m_btnDelGroup.	SubclassDlgItem(IDC_BTN_DEL_GROUP,	this);

	//m_btnAddField.	SubclassDlgItem	(IDC_BTN_ADD_FIELD,	this);
	//m_btnModField.	SubclassDlgItem	(IDC_BTN_MOD_FIELD,	this);
	//m_btnDelField.	SubclassDlgItem	(IDC_BTN_DEL_FIELD,	this);
	m_btnPreView.	SubclassDlgItem	(IDC_BTN_PREVIEW,	this);
	
	if (m_pAskRule->m_AskDialogs.GetSize() > 0)
	{
		ShowAskDialog();
		ShowAskGroup();
    	ShowAskField();
	}

	EnableBtnPreView();

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CAskRuleDlg::AskConfirm(const CString& string)
{
	return 
		AfxMessageBox
		(
			cwsprintf(_TB("Are you sure you want to delete {0-%s} ?"), (LPCTSTR) string), 
			MB_YESNO | MB_ICONQUESTION
		)
		==	IDYES;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::EnableBtnPreView()
{
	if (m_pCurAskDialog && m_pCurAskDialog->m_AskGroups.GetSize() > 0)
		for (int nIdxGrp = 0; nIdxGrp < m_pCurAskDialog->m_AskGroups.GetSize(); nIdxGrp++)
		{
			AskGroupData* pTmpAskGroup = m_pCurAskDialog->GetAskGroup(nIdxGrp);
			if (pTmpAskGroup && !pTmpAskGroup->IsEmpty())
			{
				m_btnPreView.EnableWindow(TRUE);
				return;
			}
		}
		
	m_btnPreView.EnableWindow(FALSE);
}

//----------------------------------------------------------------------------
void CAskRuleDlg::AddDefaultDialog()
{
	AskDialogData*	pNewAskDialog = new AskDialogData(*m_pSymTable, m_pDocument);

	pNewAskDialog->m_strName = m_pAskRule->GetAdviseName(NULL);
	pNewAskDialog->m_strTitle = AskDialogData::GetEmptyTitle();
	m_pAskRule->m_AskDialogs.Add(pNewAskDialog);
	m_lbDialog.AddString(pNewAskDialog->m_strName);
	m_lbDialog.SetCurSel(m_lbDialog.GetCount() - 1);
    m_lbGroup.ResetContent();
	m_lbField.ResetContent();

	m_pCurAskDialog	= pNewAskDialog;
	m_pCurAskGroup	= NULL;
    m_pCurAskField	= NULL;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::AddDefaultGroup()
{
	AskGroupData*	pNewAskGroup = new AskGroupData(*m_pSymTable, m_pDocument);

    pNewAskGroup->m_strTitle = _TB("Group Without Title");
	m_pCurAskDialog->m_AskGroups.Add(pNewAskGroup);
    m_lbGroup.AddString(pNewAskGroup->m_strTitle);
	m_lbGroup.SetCurSel(m_lbGroup.GetCount() - 1);
	m_lbField.ResetContent();

	m_pCurAskGroup		= pNewAskGroup;
    m_pCurAskField		= NULL;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::ShowAskDialog()
{
	m_lbDialog.ResetContent();

	for (int i = 0; i < m_pAskRule->m_AskDialogs.GetSize(); i++)
	{
		AskDialogData* pAskDlg = m_pAskRule->GetAskDialog(i);
        m_lbDialog.AddString(pAskDlg->GetName());
	}

    m_pCurAskDialog = m_pAskRule->GetAskDialog(0);
	m_lbDialog.SetCurSel(0);
}


//----------------------------------------------------------------------------
void CAskRuleDlg::ShowAskGroup()
{
	m_lbGroup.ResetContent();
	if (m_pCurAskDialog == NULL) return;

	for (int i = 0; i < m_pCurAskDialog->m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroup = m_pCurAskDialog->GetAskGroup(i);
		m_lbGroup.AddString(pAskGroup->GetTitle());
	}

	m_pCurAskGroup = m_pCurAskDialog->GetAskGroup(0);
    m_lbGroup.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CAskRuleDlg::ShowAskField()
{
	m_lbField.ResetContent();
	if (m_pCurAskGroup == NULL) return;

	for (int i = 0; i < m_pCurAskGroup->m_AskFields.GetSize(); i++)
	{
		AskFieldData* pAskField = m_pCurAskGroup->GetAskField(i);
        m_lbField.AddString(pAskField->m_strPublicName);
	}

	m_pCurAskField = m_pCurAskGroup->GetAskField(0);
	m_lbField.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnLbnDialog()
{
	int nCurPos = m_lbDialog.GetCurSel();

	if (nCurPos >= 0)
    {
		m_pCurAskDialog = m_pAskRule->GetAskDialog(nCurPos);
		ShowAskGroup();
		ShowAskField();

		EnableBtnPreView();
	}
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnLbnGroup()
{
	int nCurPos = m_lbGroup.GetCurSel();

	if (nCurPos >= 0 && m_pCurAskDialog != NULL)
    {
		m_pCurAskGroup = m_pCurAskDialog->GetAskGroup(nCurPos);
		ShowAskField();
	}
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnLbnField()
{
	int nCurPos = m_lbField.GetCurSel();

	if (nCurPos >= 0 && m_pCurAskGroup != NULL)
		m_pCurAskField = m_pCurAskGroup->GetAskField(nCurPos);
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnHelpFilePath()
{
	CString strFile(m_pPrgData->GetHelpFile());
	
	m_pPrgData->SetHelpFile(strFile);
}
		
//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnPreView()
{
	AskDialogInputMng aAskDialogInputMng(m_pAskRule, m_pSymTable, m_pDocument);
	aAskDialogInputMng.ExecAsk(this, m_pCurAskDialog);
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnAddDialog()
{
	AskDialogData*	pNewAskDialog = new AskDialogData(*m_pSymTable, m_pDocument);
	CAskDialogDlg	pAskDlg(pNewAskDialog, m_pAskRule, TRUE);

	if (pAskDlg.DoModal() != IDOK)
	{
		delete pNewAskDialog;
        return;
	}


    m_pAskRule->m_AskDialogs.Add(pNewAskDialog);
    m_lbDialog.AddString(pNewAskDialog->m_strName);
    m_lbDialog.SetCurSel(m_lbDialog.GetCount() - 1);
    m_lbGroup.ResetContent();
	m_lbField.ResetContent();

	m_pCurAskDialog	= pNewAskDialog;
	m_pCurAskGroup	= NULL;
    m_pCurAskField	= NULL;
    
	EnableBtnPreView();
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnModDialog()
{
	if (m_pCurAskDialog == NULL)	
		return;
	
	CAskDialogDlg pAskDlg(m_pCurAskDialog, m_pAskRule, FALSE);

	if (pAskDlg.DoModal() == IDOK)
	{
		int nCurPos = m_lbDialog.GetCurSel();
		m_lbDialog.DeleteString(nCurPos);
		m_lbDialog.InsertString(nCurPos, m_pCurAskDialog->m_strName);
		m_lbDialog.SetCurSel   (nCurPos);
	}
	
	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnDelDialog()
{
	int nCurPos = m_lbDialog.GetCurSel();
	if (nCurPos < 0)
		return;

	CString	strTmp;

    m_lbDialog.GetText(nCurPos, strTmp);          
    
	if (CanDeleteDialog(m_pCurAskDialog) && AskConfirm(strTmp))
	{
    	m_lbDialog.DeleteString(nCurPos);
		m_pAskRule->DelAskDialog(nCurPos, TRUE);
		
		if (m_lbDialog.GetCount() > 0)
		{
			m_pCurAskDialog = m_pAskRule->GetAskDialog(0);
			m_lbDialog.SetCurSel(0);
			ShowAskGroup();
            ShowAskField();
		}
		else
		{
			m_pCurAskDialog		= NULL;
			m_pCurAskGroup		= NULL;
			m_pCurAskField		= NULL;
			
            m_lbDialog.ResetContent();
			m_lbGroup.ResetContent();
			m_lbField.ResetContent();
		}
    
	    EnableBtnPreView();
		m_bModified = TRUE;
	}
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnDialogUp()
{
	int nCurPos = m_lbDialog.GetCurSel();

	if (m_pCurAskDialog == NULL || nCurPos <= 0)	
		return;
	
	AskDialogData* pAskDialog = m_pAskRule->GetAskDialog(nCurPos);
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pAskRule->m_AskDialogs.SetAt(nCurPos, NULL);
	m_pAskRule->m_AskDialogs.RemoveAt(nCurPos);
	m_pAskRule->m_AskDialogs.InsertAt(nCurPos - 1, pAskDialog);

	m_lbDialog.DeleteString(nCurPos);
	m_lbDialog.InsertString(nCurPos - 1, m_pCurAskDialog->m_strName);
	m_lbDialog.SetCurSel   (nCurPos - 1);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnDialogDown()
{
	int nCurPos = m_lbDialog.GetCurSel();

	if (m_pCurAskDialog == NULL || nCurPos < 0 || nCurPos == (m_lbDialog.GetCount() - 1))
		return;
	
	AskDialogData* pAskDialog = m_pAskRule->GetAskDialog(nCurPos);
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pAskRule->m_AskDialogs.SetAt(nCurPos, NULL);
	m_pAskRule->m_AskDialogs.RemoveAt(nCurPos);
	m_pAskRule->m_AskDialogs.InsertAt(nCurPos + 1, pAskDialog);

	m_lbDialog.DeleteString(nCurPos);
	m_lbDialog.InsertString(nCurPos + 1, m_pCurAskDialog->m_strName);
	m_lbDialog.SetCurSel   (nCurPos + 1);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
BOOL CAskRuleDlg::CanDeleteDialog(AskDialogData* pAskDialog)
{
	return 	pAskDialog->CanDelete();
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnAddGroup()
{
	if (m_pCurAskDialog == NULL) 
		AddDefaultDialog();

	AskGroupData*	pNewAskGroup = new AskGroupData(*m_pSymTable, m_pDocument);
	CAskGroupDlg	pAskDlg(pNewAskGroup, m_pCurAskDialog, m_pPrgData->GetRuleData(), m_pSymTable, TRUE);

	if (pAskDlg.DoModal() != IDOK)
	{
		delete pNewAskGroup;
        return;
	}

	m_pCurAskDialog->m_AskGroups.Add(pNewAskGroup);
    m_lbGroup.AddString(pNewAskGroup->m_strTitle);
	m_lbGroup.SetCurSel(m_lbGroup.GetCount() - 1);
	m_lbField.ResetContent();

	m_pCurAskGroup		= pNewAskGroup;
    m_pCurAskField		= NULL;

	EnableBtnPreView();
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnModGroup()
{
	if (m_pCurAskGroup == NULL)
		return;
	
	CAskGroupDlg pAskDlg(m_pCurAskGroup, m_pCurAskDialog, m_pPrgData->GetRuleData(), m_pSymTable, FALSE);

	if (pAskDlg.DoModal() == IDOK)
	{
		int nCurPos = m_lbGroup.GetCurSel();
		m_lbGroup.DeleteString(nCurPos);
		m_lbGroup.InsertString(nCurPos, m_pCurAskGroup->m_strTitle);
		m_lbGroup.SetCurSel   (nCurPos);
	}

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnDelGroup()
{
	if (m_pCurAskGroup == NULL)
		return;
	
	CString	strTmp;

	int nCurPos = m_lbGroup.GetCurSel();
	if (nCurPos < 0)
		return;

    m_lbGroup.GetText(nCurPos, strTmp);

	if (CanDeleteGroup(m_pCurAskGroup) && AskConfirm(strTmp))
    {
		m_lbGroup.DeleteString(nCurPos);
		m_pCurAskDialog->DelAskGroup(nCurPos, TRUE);

		if (m_lbGroup.GetCount() > 0)
		{
			m_pCurAskGroup = m_pCurAskDialog->GetAskGroup(0);
			m_lbGroup.SetCurSel(0);
			ShowAskField();
		}
		else
		{
			m_pCurAskGroup		= NULL;
			m_pCurAskField		= NULL;
			m_lbGroup.ResetContent();
			m_lbField.ResetContent();
		}
    
	    EnableBtnPreView();

		m_bModified = TRUE;
	}
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnGroupUp()
{
	int nCurPos = m_lbGroup.GetCurSel();

	if (m_pCurAskGroup == NULL || nCurPos <= 0)	
		return;
	
	AskGroupData* pAskGroup = m_pCurAskDialog->GetAskGroup(nCurPos);
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pCurAskDialog->m_AskGroups.SetAt(nCurPos, NULL);
	m_pCurAskDialog->m_AskGroups.RemoveAt(nCurPos);
	m_pCurAskDialog->m_AskGroups.InsertAt(nCurPos - 1, pAskGroup);

	m_lbGroup.DeleteString(nCurPos);
	m_lbGroup.InsertString(nCurPos - 1, m_pCurAskGroup->m_strTitle);
	m_lbGroup.SetCurSel   (nCurPos - 1);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnGroupDown()
{
	int nCurPos = m_lbGroup.GetCurSel();

	if (m_pCurAskGroup == NULL ||  nCurPos < 0 || nCurPos == (m_lbGroup.GetCount() - 1))
		return;
	
	AskGroupData* pAskGroup = m_pCurAskDialog->GetAskGroup(nCurPos);
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pCurAskDialog->m_AskGroups.SetAt(nCurPos, NULL);
	m_pCurAskDialog->m_AskGroups.RemoveAt(nCurPos);
	m_pCurAskDialog->m_AskGroups.InsertAt(nCurPos + 1, pAskGroup);

	m_lbGroup.DeleteString(nCurPos);
	m_lbGroup.InsertString(nCurPos + 1, m_pCurAskGroup->m_strTitle);
	m_lbGroup.SetCurSel   (nCurPos + 1);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
BOOL CAskRuleDlg::CanDeleteGroup(AskGroupData* pAskGroup)
{
	return pAskGroup->CanDelete();
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnAddField()
{
	if (m_pCurAskDialog == NULL) 
		AddDefaultDialog();
		
	if (m_pCurAskGroup == NULL)
		AddDefaultGroup();

	AskFieldData*	pNewAskField = new AskFieldData(m_pCurAskGroup);
	CAskFieldDlg	pAskDlg(pNewAskField, m_pSymTable, m_pPrgData->GetRuleData(), TRUE);

	if (pAskDlg.DoModal() != IDOK)
	{
		delete pNewAskField;
        return;
	}

	m_pCurAskGroup->m_AskFields.Add(pNewAskField);

	m_lbField.AddString(pNewAskField->m_strPublicName);
	m_lbField.SetCurSel(m_lbField.GetCount() - 1);

	//aggiorno view symbolTable con nouova askField inserito
	if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		((CWoormDoc*)m_pDocument)->SyncronizeViewSymbolTable(m_pSymTable);
	}
	
	m_pCurAskField	= pNewAskField;
	EnableBtnPreView();
	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnModField()
{
	if (m_pCurAskField == NULL)	return;

	CAskFieldDlg pAskDlg(m_pCurAskField, m_pSymTable, m_pPrgData->GetRuleData(), FALSE);

	if (pAskDlg.DoModal() == IDOK)
	{
		int nCurPos = m_lbField.GetCurSel();
		m_lbField.DeleteString(nCurPos);
		m_lbField.InsertString(nCurPos, m_pCurAskField->m_strPublicName);
		m_lbField.SetCurSel   (nCurPos);
	}

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnDelField()
{
	CString	strTmp;

	int nCurPos = m_lbField.GetCurSel();
	if (nCurPos < 0)
		return;

    m_lbField.GetText(nCurPos, strTmp);
    
	if (CanDeleteField(m_pCurAskField) && AskConfirm(strTmp))
    {
		m_lbField.DeleteString(nCurPos);
		m_pCurAskGroup->DelAskField(nCurPos, TRUE);

		if (m_lbField.GetCount() > 0)
		{
			m_pCurAskField = m_pCurAskGroup->GetAskField(0);
			m_lbField.SetCurSel(0);
		}
		else
		{
			m_pCurAskField	= NULL;
			m_lbField.ResetContent();
		}
    
		EnableBtnPreView();
		m_bModified = TRUE;
	}
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnFieldUp()
{
	int nCurPos = m_lbField.GetCurSel();

	if (m_pCurAskField == NULL || nCurPos <= 0)	
		return;
	
	AskFieldData* pAskField = m_pCurAskGroup->GetAskField(nCurPos);
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pCurAskGroup->m_AskFields.SetAt(nCurPos, NULL);
	m_pCurAskGroup->m_AskFields.RemoveAt(nCurPos);
	m_pCurAskGroup->m_AskFields.InsertAt(nCurPos - 1, pAskField);

	m_lbField.DeleteString(nCurPos);
	m_lbField.InsertString(nCurPos - 1, m_pCurAskField->m_strPublicName);
	m_lbField.SetCurSel   (nCurPos - 1);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnBtnFieldDown()
{
	int nCurPos = m_lbField.GetCurSel();

	if (m_pCurAskField == NULL || nCurPos < 0 || nCurPos == (m_lbField.GetCount() - 1))
		return;
	
	AskFieldData* pAskField = m_pCurAskGroup->GetAskField(nCurPos);
	// serve per non far cancellare l'oggetto dalla RemoveAt
	m_pCurAskGroup->m_AskFields.SetAt(nCurPos, NULL);
	m_pCurAskGroup->m_AskFields.RemoveAt(nCurPos);
	m_pCurAskGroup->m_AskFields.InsertAt(nCurPos + 1, pAskField);

	m_lbField.DeleteString(nCurPos);
	m_lbField.InsertString(nCurPos + 1, m_pCurAskField->m_strPublicName);
	m_lbField.SetCurSel   (nCurPos + 1);

	m_bModified = TRUE;
}

//----------------------------------------------------------------------------
BOOL CAskRuleDlg::CanDeleteField(AskFieldData* pAskField)
{
	return pAskField->CanDelete();
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnOK()
{
	int nIdxDlg = 0;
	
	while (nIdxDlg < m_pAskRule->m_AskDialogs.GetSize())
	{
		AskDialogData* pAskDlg = m_pAskRule->GetAskDialog(nIdxDlg);

		if (pAskDlg)
		{
			int nIdxGrp = 0;
			
			while (nIdxGrp < pAskDlg->m_AskGroups.GetSize())
			{
				AskGroupData* pAskGroup = pAskDlg->GetAskGroup(nIdxGrp);
				
				if (pAskGroup && pAskGroup->IsEmpty())
				{
					pAskDlg->DelAskGroup(nIdxGrp, TRUE);
					
					// the AskGroups array has been shrinked then the current nIdxGrp is valid
					continue;
				}

				nIdxGrp++;
			}

			if (pAskDlg->IsEmpty())
			{
				m_pAskRule->DelAskDialog(nIdxDlg, TRUE);
					
				// the AskDialogs array has been shrinked then the current nIdxDlg is valid
				continue;
			}

			nIdxDlg++;
		}
	}
	
	EndDialog(m_bModified);
}

//----------------------------------------------------------------------------
void CAskRuleDlg::OnCancel()
{
	OnOK();
}

//============================================================================
// CAskDialogDlg
//============================================================================
IMPLEMENT_DYNAMIC(CAskDialogDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CAskDialogDlg, CParsedDialog)
	ON_BN_CLICKED	(IDC_ASK_DLG_ONASK,		OnAskClicked)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAskDialogDlg::CAskDialogDlg
	(
		AskDialogData*	pAskDialogData,
		AskRuleData*	pAskRuleData,
		BOOL			bNewDialog
	)
	:
	CParsedDialog	(IDD_ASKDIALOG),
	m_pAskDialog	(pAskDialogData),
	m_pAskRule		(pAskRuleData),
	m_pSymTable		(&pAskRuleData->m_SymTable),
	m_bIsNewDialog	(bNewDialog),
	m_edtWhenExpr	(&pAskRuleData->m_SymTable),
	m_edtOnExpr	(&pAskRuleData->m_SymTable),
	m_edtAbortExpr	(&pAskRuleData->m_SymTable),
	m_edtBeforeBlock	(&pAskRuleData->m_SymTable),
	m_edtAfterBlock	(&pAskRuleData->m_SymTable)
{}

//----------------------------------------------------------------------------
void CAskDialogDlg::OnAskClicked()
{
	CString strName = m_edtDialogName.GetValue();

	BOOL bIsOnAsk = m_btnOnAsk.GetCheck() == 1;
	m_edtBeforeBlock.	EnableWindow(!bIsOnAsk);
	m_edtWhenExpr.		EnableWindow(!bIsOnAsk);
	m_edtAfterBlock.	EnableWindow(!bIsOnAsk);
	m_edtOnExpr.		EnableWindow(!bIsOnAsk);
	m_edtAbortExpr.		EnableWindow(!bIsOnAsk);

	strName.Replace(L"NoName", _T("1"));
	if (_ttoi(strName) > 0 && bIsOnAsk)
	{
		m_edtDialogName.SetValue(_T(""));
		m_edtDialogName.SetCtrlFocus();
	}
}

//----------------------------------------------------------------------------
BOOL CAskDialogDlg::OnInitDialog()
{                        
	CParsedDialog::OnInitDialog();

	m_edtDialogName.	SubclassEdit	(IDC_ASK_DLG_NAME,	this);
	m_edtDialogName.	SetCtrlStyle	(m_edtDialogName.GetCtrlStyle() | IDE_STYLE_NO_EMPTY);
	m_edtDialogTitle.	SubclassDlgItem	(IDC_ASK_DLG_TITLE,	this);
	m_edtWhenExpr.		SubclassEdit	(IDC_WHEN_EXPR,		this);
	m_edtOnExpr.		SubclassEdit	(IDC_ON_EXPR,		this);
	m_edtAbortExpr.		SubclassEdit	(IDC_ABORT_EXPR,	this);
	m_edtBeforeBlock.	SubclassEdit	(IDC_BEFORE_BLOCK,	this);
	m_edtAfterBlock.	SubclassEdit	(IDC_AFTER_BLOCK,	this);
	
	m_cbCaptionPos.		SubclassDlgItem	(IDC_ASK_CAPTION_POSITION,	this);

	m_cbCaptionPos.		AddString(AskDlgStrings::DEFAULT());
	m_cbCaptionPos.		AddString(AskDlgStrings::LEFT());
	m_cbCaptionPos.		AddString(AskDlgStrings::UP());

	m_btnOnAsk.			SubclassDlgItem	(IDC_ASK_DLG_ONASK,	this);

	m_edtWhenExpr	.SetWindowText(m_pAskDialog->m_pWhenExpr		? (m_pAskDialog->m_pWhenExpr->ToString()) : _T(""));
	m_edtOnExpr		.SetWindowText(m_pAskDialog->m_pOnExpr		? (m_pAskDialog->m_pOnExpr->ToString()) : _T(""));
	m_edtAbortExpr	.SetWindowText(m_pAskDialog->m_pAbortExpr	? (m_pAskDialog->m_pAbortExpr->ToString()) : _T(""));

	//unparsing Before/After Block
	if (m_pAskDialog->m_pBeforeBlock)
	{
		m_edtBeforeBlock.SetWindowText(m_pAskDialog->m_pBeforeBlock->Unparse());
	}
	else m_edtBeforeBlock.SetWindowText(_T("Begin\r\nEnd"));

	if (m_pAskDialog->m_pAfterBlock)
	{
		m_edtAfterBlock.SetWindowText(m_pAskDialog->m_pAfterBlock->Unparse());
	}
	else m_edtAfterBlock.SetWindowText(_T("Begin\r\nEnd"));

	if (m_bIsNewDialog)
	{
		m_edtDialogTitle.SetWindowText(AskDialogData::GetEmptyTitle());

	  // Building of name displayed in the listbox
	    CString strAdvisedName = m_pAskRule->GetAdviseName(m_bIsNewDialog ? NULL : m_pAskDialog);

		m_edtDialogName.SetWindowText(strAdvisedName);
		
		m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);
	}
	else
	{
		m_edtDialogTitle.SetWindowText(m_pAskDialog->m_strTitle);
		m_edtDialogName.SetWindowText(m_pAskDialog->m_strName);

		switch (m_pAskDialog->m_nFieldsCaptionPos)
		{
			case T_DEFAULT	: m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);	break;
			case T_LEFT		: m_cbCaptionPos.SetCurSel(LEFT_CAPTION_POS);		break;
			case T_TOP		: m_cbCaptionPos.SetCurSel(TOP_CAPTION_POS);		break;
		}
	}

 	if (m_pAskDialog->IsOnAsk())
	{
    	m_btnOnAsk.SetCheck(1);

		m_edtWhenExpr.EnableWindow(FALSE);
		m_edtOnExpr.EnableWindow(FALSE);
		m_edtAbortExpr.EnableWindow(FALSE);
		m_edtBeforeBlock.EnableWindow(FALSE);
		m_edtAfterBlock.EnableWindow(FALSE);
	}
   	
	return TRUE;
}

//----------------------------------------------------------------------------
void CAskDialogDlg::OnOK()
{
	if (!CParsedForm::CheckForm())
		return;
		
	CString	strTmp;
	CString strName;
	CString strDefaultTitle;
    
    strDefaultTitle = _TB("Dialog Without Title");
    
	m_edtDialogTitle.GetWindowText(strTmp);
	strTmp.TrimRight();
	if (strTmp.IsEmpty()) 
		strTmp = strDefaultTitle;

	strName = m_edtDialogName.GetValue();	
  
	if	(
			/*m_btnOnAsk.GetCheck() == 1 &&*/
			m_pAskRule->ExistDialog(strName, (m_bIsNewDialog ? NULL : m_pAskDialog))
		)
	{
		AfxMessageBox(_TB("Dialog box name already defined!"), MB_OK | MB_ICONEXCLAMATION);
		return;
	}

	if	(m_btnOnAsk.GetCheck() == 1)
	{
		m_pAskDialog->m_bIsOnAsk = TRUE;

		m_edtWhenExpr.SetWindowText(_T(""));
		m_edtOnExpr.SetWindowText(_T(""));
		m_edtAbortExpr.SetWindowText(_T(""));
		m_edtBeforeBlock.SetWindowText(_T(""));
		m_edtAfterBlock.SetWindowText(_T(""));
	}
	else
		m_pAskDialog->m_bIsOnAsk = FALSE;

	m_pAskDialog->m_strName		= strName;
	m_pAskDialog->m_strTitle	= strTmp;

	switch (m_cbCaptionPos.GetCurSel())
	{
		case DEFAULT_CAPTION_POS:m_pAskDialog->m_nFieldsCaptionPos = T_DEFAULT;	break;
		case LEFT_CAPTION_POS	:m_pAskDialog->m_nFieldsCaptionPos = T_LEFT;	break;
		case TOP_CAPTION_POS	:m_pAskDialog->m_nFieldsCaptionPos = T_TOP;		break;
	}
	
	//---- WHEN 
    Expression*	pWhenExpr = new Expression(m_pSymTable);
	if (!m_edtWhenExpr.CheckExp(*pWhenExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtWhenExpr.SetCtrlFocus(TRUE);
		delete pWhenExpr;
		return;
	}
	if (pWhenExpr->IsEmpty())
	{
		SAFE_DELETE(pWhenExpr);
	}
	SAFE_DELETE (m_pAskDialog->m_pWhenExpr);
	m_pAskDialog->m_pWhenExpr = pWhenExpr;

	//---- On Check-Expr Abort Msg  
    Expression*	pOnExpr = new Expression(m_pSymTable);
	if (!m_edtOnExpr.CheckExp(*pOnExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtOnExpr.SetCtrlFocus(TRUE);
		delete pOnExpr;
		return;
	}
	if (pOnExpr->IsEmpty())
	{
		delete pOnExpr;
		pOnExpr = NULL;
		m_edtAbortExpr.SetWindowText(_T(""));
	}
	if (m_pAskDialog->m_pOnExpr)
	{
		delete m_pAskDialog->m_pOnExpr;
		m_pAskDialog->m_pOnExpr = NULL;
	}
	m_pAskDialog->m_pOnExpr = pOnExpr;

	//---- Abort-Message 
    Expression*	pAbortExpr = new Expression(m_pSymTable);
	if (!m_edtAbortExpr.CheckExp(*pAbortExpr, DATA_STR_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtAbortExpr.SetCtrlFocus(TRUE);
		delete pAbortExpr;
		return;
	}
	if (pAbortExpr->IsEmpty())
	{
		delete pAbortExpr;
		pAbortExpr = NULL;
		if (m_pAskDialog->m_pOnExpr && !m_pAskDialog->m_pOnExpr->IsEmpty())
		{
			m_edtAbortExpr.SetWindowText(_TB("The field is mandatory!"));
			m_edtAbortExpr.SetCtrlFocus(TRUE);
			return;
		}
	}
	if (m_pAskDialog->m_pAbortExpr)
	{
		delete m_pAskDialog->m_pAbortExpr;
		m_pAskDialog->m_pAbortExpr = NULL;
	}
	m_pAskDialog->m_pAbortExpr = pAbortExpr;

	//---- Before Block 
	CString strBlock;
	m_edtBeforeBlock.GetWindowText(strBlock);

    Block*	pBeforeBlock = new Block(NULL, m_pSymTable, NULL, FALSE);
	if (strBlock.IsEmpty() || !pBeforeBlock->Parse(strBlock) || pBeforeBlock->IsEmpty())
	{
		delete pBeforeBlock;
		pBeforeBlock = NULL;
	}
	if (m_pAskDialog->m_pBeforeBlock)
	{
		delete m_pAskDialog->m_pBeforeBlock;
		m_pAskDialog->m_pBeforeBlock = NULL;
	}
	m_pAskDialog->m_pBeforeBlock = pBeforeBlock;

	//---- After Block 
 	m_edtAfterBlock.GetWindowText(strBlock);
   
	Block*	pAfterBlock = new Block(NULL, m_pSymTable, NULL, FALSE);
	if (strBlock.IsEmpty() || !pAfterBlock->Parse(strBlock) || pAfterBlock->IsEmpty())
	{
		delete pAfterBlock;
		pAfterBlock = NULL;
	}
	if (m_pAskDialog->m_pAfterBlock)
	{
		delete m_pAskDialog->m_pAfterBlock;
		m_pAskDialog->m_pAfterBlock = NULL;
	}
	m_pAskDialog->m_pAfterBlock = pAfterBlock;
	//----

	EndDialog(TRUE);
}

//----------------------------------------------------------------------------
void CAskDialogDlg::OnCancel()
{
	EndDialog(FALSE);
}


//============================================================================
// CAskGroupDlg
//============================================================================
IMPLEMENT_DYNAMIC(CAskGroupDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CAskGroupDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAskGroupDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAskGroupDlg::CAskGroupDlg
	(
		AskGroupData*	bAskGroupData,
		AskDialogData*	pAskDialogData,
		RuleDataArray*		pRuleData,
		WoormTable* pSymTable,
		BOOL			bNewGroup
	)
	:
	CParsedDialog	(IDD_ASKGROUP),
	m_pAskGroup		(bAskGroupData),
	m_pAskDialog	(pAskDialogData),
	m_pSymTable		(pSymTable),
    m_bIsNewGroup	(bNewGroup),
	m_edtAskGroupWhen (pSymTable, pRuleData)
{
}

//----------------------------------------------------------------------------
BOOL CAskGroupDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_edtGroupTitle.	SubclassDlgItem (IDC_GROUP_TITLE, this);
	
	m_cbCaptionPos.		SubclassDlgItem	(IDC_ASK_CAPTION_POSITION,	this);
	m_cbCaptionPos.		AddString(AskDlgStrings::DEFAULT());
	m_cbCaptionPos.		AddString(AskDlgStrings::LEFT());
	m_cbCaptionPos.		AddString(AskDlgStrings::UP());

	CString strDefaultTitle;
	if (m_bIsNewGroup)
	{
		strDefaultTitle = _TB("Group Without Title");

		m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);
	}
	else
	{
		strDefaultTitle = m_pAskGroup->m_strTitle;

		switch (m_pAskGroup->m_nFieldsCaptionPos)
		{
			case T_DEFAULT	: m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);	break;
			case T_LEFT		: m_cbCaptionPos.SetCurSel(LEFT_CAPTION_POS);		break;
			case T_TOP		: m_cbCaptionPos.SetCurSel(TOP_CAPTION_POS);		break;
		}
	}

	m_edtGroupTitle.SetWindowText(strDefaultTitle);

	if (!m_pAskGroup->m_bHiddenTitle)
    	CheckDlgButton(IDC_GROUP_BTN_SHOW_TITLE, TRUE);

	m_edtAskGroupWhen.SubclassEdit (IDC_ASK_HIDDEN,	this);
	if (!m_bIsNewGroup)
	{
		CString strWhenExpr;
		if (m_pAskGroup->m_pWhenExpr)
			strWhenExpr	= m_pAskGroup->m_pWhenExpr->ToString();
		m_edtAskGroupWhen.SetWindowText(strWhenExpr);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CAskGroupDlg::OnOK()
{
	CString	strTmp;
	CString strDefaultTitle;
    
    strDefaultTitle = _TB("Group Without Title");

	Expression*	pWhenExpr = new Expression(m_pSymTable);
	if (!m_edtAskGroupWhen.CheckExp(*pWhenExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtAskGroupWhen.SetCtrlFocus(TRUE);
		delete pWhenExpr;
		return;
	}
	if (pWhenExpr->IsEmpty())
	{
		delete pWhenExpr;
		pWhenExpr = NULL;
	}
    
	m_edtGroupTitle.GetWindowText(strTmp);
	strTmp.TrimRight();
	if (strTmp.IsEmpty()) 
		strTmp = strDefaultTitle;

	m_pAskGroup->m_strTitle = strTmp;
	m_pAskGroup->m_bHiddenTitle = !IsDlgButtonChecked(IDC_GROUP_BTN_SHOW_TITLE);

	switch (m_cbCaptionPos.GetCurSel())
	{
		case DEFAULT_CAPTION_POS:m_pAskGroup->m_nFieldsCaptionPos = T_DEFAULT;	break;
		case LEFT_CAPTION_POS	:m_pAskGroup->m_nFieldsCaptionPos = T_LEFT;		break;
		case TOP_CAPTION_POS	:m_pAskGroup->m_nFieldsCaptionPos = T_TOP;		break;
	}

	if (m_pAskGroup->m_pWhenExpr)
		delete m_pAskGroup->m_pWhenExpr;

	m_pAskGroup->m_pWhenExpr = pWhenExpr;

	EndDialog(TRUE);
}

//----------------------------------------------------------------------------
void CAskGroupDlg::OnCancel()
{
	EndDialog(FALSE);
}


//============================================================================
// CAskFieldDlg
//============================================================================
IMPLEMENT_DYNAMIC(CAskFieldDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CAskFieldDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAskFieldDlg)
	ON_EN_VALUE_CHANGED	(IDC_ASK_DATA_TYPE,		DataTypeChanged)
	ON_BN_CLICKED		(IDC_ASK_LOWER_LIMIT,	LowerLimitChanged)
	ON_BN_CLICKED		(IDC_ASK_UPPER_LIMIT,	UpperLimitChanged)
	ON_CBN_SELCHANGE	(IDC_ASK_BOOL_CTRL_STYLE, CtrlStyleChanged)
	ON_EN_VALUE_CHANGED	(IDC_ASK_FIELD_NAME,	PublicNameChanged)
	ON_EN_VALUE_CHANGED	(IDC_ASK_DATA_LEN,		OnDataLengthChanged)
	ON_CBN_SELCHANGE	(IDC_ASK_HKL_GROUPS,	HotLinkGroupChanged)
	ON_CBN_SELCHANGE	(IDC_ASK_HKL_NAMES,		HotLinkNameChanged)
	ON_BN_CLICKED		(IDC_ASK_BTN_STATIC_CAPTION,	OnBtnDynamicCaption)
	ON_BN_CLICKED		(IDC_ASK_BTN_DYNAMIC_CAPTION,	OnBtnDynamicCaption)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAskFieldDlg::CAskFieldDlg
	(
		AskFieldData*	pAskField,
		WoormTable* pSymTable,
		RuleDataArray*		pRuleData,
		BOOL			bNewField
	)
	:
	CParsedDialog				(IDD_ASKFIELD),
	m_pAskField					(pAskField),
	m_pSymTable					(pSymTable),
	m_bIsNewField				(bNewField),
	m_edtAskFieldDefault		(pSymTable, pRuleData),
	m_edtAskFieldReadOnly		(pSymTable, pRuleData),
	m_edtAskFieldWhen			(pSymTable, pRuleData),
	m_edtAskFieldName			(pSymTable),
    m_edtHotLinkParams			(pSymTable, pRuleData),
	m_edtAskFieldDynamicCaption	(pSymTable, pRuleData),
	m_pHLAddOnModule			(NULL),
	m_bHLHasCombo				(FALSE)
{
}

//----------------------------------------------------------------------------
BOOL CAskFieldDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_edtAskFieldName.		SubclassEdit	(IDC_ASK_FIELD_NAME,	this);
	m_edtAskFieldName.		SetCtrlStyle	(m_edtAskFieldName.GetCtrlStyle() | IDE_STYLE_NO_EMPTY);

	m_edtAskFieldCaption.			SubclassDlgItem	(IDC_ASK_FIELD_LABEL,			this);
	m_edtAskFieldDynamicCaption.	SubclassEdit	(IDC_ASK_FIELD_DYNAMIC_CAPTION,	this);
	m_btnAskFieldDynamicCaption.	SubclassDlgItem	(IDC_ASK_BTN_DYNAMIC_CAPTION,	this);
	m_btnAskFieldStaticCaption.		SubclassDlgItem	(IDC_ASK_BTN_STATIC_CAPTION,	this);
	
	m_cbCaptionPos.			SubclassDlgItem	(IDC_ASK_CAPTION_POSITION,	this);
	m_cbCaptionPos.			AddString(AskDlgStrings::DEFAULT());
	m_cbCaptionPos.			AddString(AskDlgStrings::LEFT());
	m_cbCaptionPos.			AddString(AskDlgStrings::UP());

	m_edtAskFieldLen.		SubclassEdit	(IDC_ASK_DATA_LEN,		this);
	m_edtAskFieldPrec.		SubclassEdit	(IDC_ASK_DATA_PREC,		this);
	m_cbCtrlStyle.			SubclassDlgItem	(IDC_ASK_BOOL_CTRL_STYLE,this);
	m_btnAskFieldLowerLimit.SubclassDlgItem	(IDC_ASK_LOWER_LIMIT,	this);
	m_btnAskFieldUpperLimit.SubclassDlgItem	(IDC_ASK_UPPER_LIMIT,	this);
	m_cbDataObjTypes.		SubclassEdit	(IDC_ASK_DATA_TYPE,		this);
	m_edtAskFieldDefault.	SubclassEdit	(IDC_ASK_FIELD_DEF,		this);
	m_edtAskFieldReadOnly.	SubclassEdit	(IDC_ASK_FIELD_READONLY,this);
	m_edtAskFieldWhen.		SubclassEdit	(IDC_ASK_HIDDEN,	this);
	m_btnAskFieldDynamicHidden.SubclassDlgItem	(IDC_ASK_DYNAMIC_HIDDEN,	this);

	m_btnAskFieldCheckRadioOnLeft.SubclassDlgItem		(IDC_ASK_CHECK_RADIO_ON_LEFT, this);
	m_btnAskFieldCheckRadioTextOnLeft.SubclassDlgItem	(IDC_ASK_CHECK_RADIO_TEXT_ON_LEFT, this);

	m_cbHotLinkGroups.		SubclassDlgItem	(IDC_ASK_HKL_GROUPS,	this);
	m_cbHotLinkNames.		SubclassDlgItem	(IDC_ASK_HKL_NAMES,		this);
	m_edtHotLinkParams.		SubclassEdit	(IDC_ASK_HKL_PARAMS,	this);
	m_btnShowHotLinkDescription.SubclassDlgItem(IDC_ASK_SHOW_HKL_DESCRIPTION, this);

	m_btnAskFieldAlwaysReInit.	SubclassDlgItem	(IDC_ASK_FIELD_ALWAYS_REINIT,	this);
	m_btnAskFieldNeverReInit.SubclassDlgItem(IDC_ASK_FIELD_NEVER_REINIT, this);

	m_btnAskUseMultiSelectionCombo.SubclassDlgItem(IDC_ASK_FIELD_USE_MULTISELECTION_COMBO, this);
	m_btnAskUseDescriptionCombo.SubclassDlgItem(IDC_ASK_FIELD_USE_DESCRIPTION_COMBO, this);

	if (!ShowAskFieldData())
	{
		EndDialog(IDCANCEL);
		return FALSE;
    }
	return TRUE;
}

//----------------------------------------------------------------------------
void CAskFieldDlg::PublicNameChanged()
{
	if (m_bIsNewField)
	{
		CString strName; m_edtAskFieldName.GetValue(strName);
		if (AfxGetTokensTable()->GetKeywordsToken(strName) != T_NOTOKEN)
			m_edtAskFieldName.SetError(_TB("The name collides with a reserved word of TaskBuilder"));
	}
}

//----------------------------------------------------------------------------
void CAskFieldDlg::EnableControls()
{
	DataType curDataType = m_cbDataObjTypes.GetTypeValue();
	BOOL bLenNotNull	=	m_edtAskFieldLen.GetValue() != 0;
	BOOL bIsBool		=	curDataType == DATA_BOOL_TYPE;
	BOOL bIsEnumSet		=	curDataType == DATA_ENUM_TYPE;
	BOOL bEnablePrec	=	bLenNotNull	&&
							(
								m_cbDataObjTypes.IsPrecisionEnabled()	||
								curDataType == DATA_STR_TYPE			||
								bIsEnumSet
							);

	m_edtAskFieldPrec.					EnableWindow(bEnablePrec);
	GetDlgItem(IDC_ASK_DATA_PREC_TAG)->	EnableWindow(bEnablePrec);

	CString sSelected;
	m_cbCtrlStyle.GetLBText(m_cbCtrlStyle.GetCurSel(), sSelected);
	DoCtrlStyleChanged();
	m_cbCtrlStyle.SelectString(0, sSelected);

	BOOL bEnablePosCaption = !bIsBool || sSelected.CompareNoCase(szEditStyle) == 0;

	m_edtAskFieldCaption.	EnableWindow(bLenNotNull);

	GetDlgItem(IDC_ASK_CAPTION_POSITION_TAG)->	EnableWindow(bLenNotNull && bEnablePosCaption);
	m_cbCaptionPos.EnableWindow(bLenNotNull && bEnablePosCaption);
	m_btnAskFieldCheckRadioOnLeft.EnableWindow(bLenNotNull && bIsBool && !bEnablePosCaption);
	m_btnAskFieldCheckRadioTextOnLeft.EnableWindow(bLenNotNull && bIsBool && !bEnablePosCaption);

	m_btnAskFieldLowerLimit.EnableWindow(bLenNotNull && !bIsBool && !bIsEnumSet);
	m_btnAskFieldUpperLimit.EnableWindow(bLenNotNull && !bIsBool && !bIsEnumSet);

	m_edtAskFieldDefault.		EnableWindow(TRUE);
	m_edtAskFieldReadOnly.		EnableWindow(bLenNotNull);
	m_edtAskFieldWhen.			EnableWindow(bLenNotNull);
	m_btnAskFieldDynamicHidden.	EnableWindow(bLenNotNull);

	HotLinkGroupChanged	();

	// non posso chiamare il changed altrimenti mi fa perdere
	// l'hotlink correntemente selezionato solo perchè si è
	// modificato la grandezza!
	if (bLenNotNull)
	{
		m_cbHotLinkNames.		EnableWindow(bLenNotNull);
		m_cbHotLinkGroups.		EnableWindow(bLenNotNull);
	}
	else
		HotLinkGroupChanged		();
}

//-----------------------------------------------------------------------------
void CAskFieldDlg::OnDataLengthChanged()
{
	if (m_edtAskFieldLen.GetValue() < 1)
        m_edtAskFieldLen.SetError(_TB("Field length is less than 1"));
}

//----------------------------------------------------------------------------
void CAskFieldDlg::SetCurrentCtrlStyle (const int& nCtrlStyle)
{
	switch (nCtrlStyle)
	{
	case AskFieldData::COMBO_BOX:
	case AskFieldData::EDIT_HKL:
		if (m_nsHotLink.IsValid() && m_pHLAddOnModule)
		{
			m_pAskField->m_CtrlStyle = m_bHLHasCombo ? (AskFieldData::CtrlStyle) nCtrlStyle : AskFieldData::EDIT_HKL;
			m_cbCtrlStyle.SelectString(0, m_bHLHasCombo && ((AskFieldData::CtrlStyle) nCtrlStyle == AskFieldData::COMBO_BOX) ? szComboStyle : szEditStyle);
			break;
		}

		m_cbCtrlStyle.SelectString(0, szEditStyle);
		m_pAskField->m_CtrlStyle = AskFieldData::EDIT;
		break;
	case AskFieldData::CHECK_BOX:
		m_cbCtrlStyle.SelectString(0, szCheckBoxStyle);
		m_pAskField->m_CtrlStyle = AskFieldData::CHECK_BOX;
		break;
	case AskFieldData::RADIO_BUTTON:
		m_cbCtrlStyle.SelectString(0, szRadioStyle);
		m_pAskField->m_CtrlStyle = AskFieldData::RADIO_BUTTON;
		break;
	default:
		m_cbCtrlStyle.SelectString(0, szEditStyle);
		m_pAskField->m_CtrlStyle = AskFieldData::EDIT;
	}
}

//----------------------------------------------------------------------------
void CAskFieldDlg::DoCtrlStyleChanged ()
{
	BOOL bIsBool	 = m_cbDataObjTypes.GetTypeValue() == DATA_BOOL_TYPE;
	BOOL bLenNotNull = m_edtAskFieldLen.GetValue() != 0;
	BOOL bEnable = FALSE;
	
	m_cbCtrlStyle.ResetContent();
	if (bIsBool)
	{
		m_cbCtrlStyle.AddString(szCheckBoxStyle);
		m_cbCtrlStyle.AddString(szRadioStyle);
		bEnable = TRUE;
	}
	else if (m_nsHotLink.IsValid() && m_pHLAddOnModule && m_bHLHasCombo)
	{
		m_cbCtrlStyle.AddString(szComboStyle);
		bEnable = TRUE;
	}

	m_cbCtrlStyle.AddString(szEditStyle);

	m_cbCtrlStyle.SetCurSel(0);

	m_cbCtrlStyle.EnableWindow(bEnable);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::OnBtnDynamicCaption()
{
	if (m_btnAskFieldStaticCaption.GetCheck() == 1)
	{
		m_btnAskFieldDynamicCaption.SetCheck(0);

		m_edtAskFieldCaption.SetWindowText(m_pAskField->m_strCaption);
		m_edtAskFieldCaption.EnableWindow(TRUE);
			
		m_edtAskFieldDynamicCaption.EnableWindow(FALSE);
		m_edtAskFieldDynamicCaption.SetWindowText(NULL);
	}
	else
	{
		m_btnAskFieldStaticCaption.SetCheck(0);

		m_edtAskFieldCaption.EnableWindow(FALSE);
		m_edtAskFieldCaption.SetWindowText(NULL);
		m_edtAskFieldDynamicCaption.EnableWindow(TRUE);

		if (m_pAskField->m_pCaptionExpr)
		{
			CString strCaption = m_pAskField->m_pCaptionExpr->ToString();
			m_edtAskFieldDynamicCaption.SetWindowText(strCaption);
		}
	}
}

//----------------------------------------------------------------------------
void CAskFieldDlg::DataTypeChanged()
{
	DataType curDataType = m_cbDataObjTypes.GetTypeValue();

	CString		strValue, strReadOnly, strHide, strCaption;
	int			nDynamicHidden = 0;
	int			nLen, nPrec;

	if (m_bIsNewField)
	{
		nLen	= AfxGetFormatStyleTable()->GetOutputCharLen(curDataType, NULL);
		nPrec	= 0;

		switch (curDataType.m_wType)
		{
			case DATA_TXT_TYPE	: 
			case DATA_STR_TYPE	: 
				strValue = _T("\"\"");
				nPrec	= 1;
				break; 
			case DATA_DATE_TYPE	:
			{
				if (curDataType.IsFullDate())
					if (curDataType.IsATime())
						strValue = cwsprintf(T_FTIME) + _T("()");
					else
						strValue = cwsprintf(T_FDATETIME) + _T("()");
				else
					strValue = cwsprintf(T_FDATE) + _T("()");
				break;
			}
			case DATA_INT_TYPE	:
			case DATA_LNG_TYPE	:	//@@ElapsedTime @@TODOTIME
			case DATA_DBL_TYPE	:
			case DATA_MON_TYPE	:
			case DATA_QTA_TYPE	:
			case DATA_PERC_TYPE	:
			{
				strValue = "0";
				break;
			}
			case DATA_BOOL_TYPE	:
			{
				strValue = "TRUE";
				m_cbCtrlStyle.SelectString(0, szCheckBoxStyle);
				m_btnAskFieldCheckRadioOnLeft.SetCheck(FALSE);
				m_btnAskFieldCheckRadioTextOnLeft.SetCheck(FALSE);
				break;
			}
			case DATA_ENUM_TYPE :
			{
				int nItemVal = AfxGetEnumsTable()->GetEnumDefaultItemValue(curDataType.m_wTag);
				DWORD dwEnumVal = MAKELONG(nItemVal, curDataType.m_wTag);
				strValue = Unparser::UnparseEnumItem(dwEnumVal);
				nPrec	= 1;
				break;
			}

			default	: ASSERT(FALSE); return;
		}
	}
	else
	{
		WoormField* pRepField =  m_pSymTable->GetField(m_pAskField->m_strPublicName);
		ASSERT_VALID(pRepField);
		if (!pRepField)
			return;

		nLen		= pRepField->GetLen();
		nPrec		= pRepField->GetNumDec();
		strValue	= pRepField->GetInitExpression() ? pRepField->GetInitExpression()->ToString() : _T("");	//unparse expression

		m_btnAskFieldAlwaysReInit.SetCheck (pRepField->IsReInit() ? 1 : 0);	
		m_btnAskFieldNeverReInit.SetCheck  (pRepField->IsStatic() ? 1 : 0);

		m_btnAskUseMultiSelectionCombo.SetCheck(m_pAskField->m_bMultiSelectionCombo ? 1 : 0);
		m_btnAskUseDescriptionCombo.SetCheck(m_pAskField->m_bDescriptionCombo ? 1 : 0);

		m_btnShowHotLinkDescription.SetCheck(m_pAskField->m_bShowHotLinkDescription ? 1 : 0);

		if (m_pAskField->m_pReadOnlyExpr)
			strReadOnly	= m_pAskField->m_pReadOnlyExpr->ToString();
		if (m_pAskField->m_pWhenExpr)
			strHide	= m_pAskField->m_pWhenExpr->ToString();
		if (m_pAskField->m_bDynamicHidden)
			nDynamicHidden	= 1;
		if (m_pAskField->m_pCaptionExpr)
			strCaption	= m_pAskField->m_pCaptionExpr->ToString();

		if (curDataType == DATA_STR_TYPE || curDataType == DATA_TXT_TYPE)
		{
			if (nLen == 0) nLen = 1;
		}
		else if (curDataType == DATA_BOOL_TYPE)
		{
			if (m_pAskField->m_CtrlStyle == AskFieldData::CHECK_BOX)
				m_cbCtrlStyle.SelectString(0, szCheckBoxStyle);
			else if (m_pAskField->m_CtrlStyle == AskFieldData::RADIO_BUTTON)
				m_cbCtrlStyle.SelectString(0, szRadioStyle);
			else 
				m_cbCtrlStyle.SelectString(0, szEditStyle);

			m_btnAskFieldCheckRadioOnLeft.SetCheck(m_pAskField->m_bLeftBoolAlign);
			m_btnAskFieldCheckRadioTextOnLeft.SetCheck(m_pAskField->m_bLeftTextBool);
		}
	}

	m_edtAskFieldLen.SetValue(m_pAskField->m_bHiddenInput ? 0 : nLen);
	m_edtAskFieldPrec.SetValue(nPrec);

	m_edtAskFieldDefault.SetWindowText(strValue);
	m_edtAskFieldReadOnly.SetWindowText(strReadOnly);
	m_edtAskFieldWhen.SetWindowText(strHide);
	m_btnAskFieldDynamicHidden.SetCheck(nDynamicHidden);
	m_edtAskFieldDynamicCaption.SetWindowText(strCaption);

	if (curDataType == DATA_STR_TYPE || curDataType == DATA_ENUM_TYPE)
		GetDlgItem(IDC_ASK_DATA_PREC_TAG)->SetWindowText(_TB("Li&nes"));
	else
		GetDlgItem(IDC_ASK_DATA_PREC_TAG)->SetWindowText(_TB("&Decimals"));
		
	DoCtrlStyleChanged();
	
	BOOL bIsBool = curDataType == DATA_BOOL_TYPE;
	m_btnAskFieldLowerLimit.					ShowWindow(bIsBool ? SW_HIDE : SW_SHOW);
	m_btnAskFieldUpperLimit.					ShowWindow(bIsBool ? SW_HIDE : SW_SHOW);
	GetDlgItem(IDC_ASK_CAPTION_POSITION_TAG)->	ShowWindow(bIsBool ? SW_HIDE : SW_SHOW);
	m_cbCaptionPos.								ShowWindow(bIsBool ? SW_HIDE : SW_SHOW);
	m_btnAskFieldCheckRadioOnLeft.				ShowWindow(bIsBool ? SW_SHOW : SW_HIDE);
	m_btnAskFieldCheckRadioTextOnLeft.			ShowWindow(bIsBool ? SW_SHOW : SW_HIDE);

	int nPos = m_cbCtrlStyle.GetCurSel();
	if (nPos >= 0)
	{
		CString sSelected;
		m_cbCtrlStyle.GetLBText(nPos, sSelected);
		if (bIsBool && sSelected.CompareNoCase(szEditStyle) == 0)
			m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);
	}

	EnableControls		();
	HotLinkGroupChanged	();
}

//----------------------------------------------------------------------------
void CAskFieldDlg::LowerLimitChanged()
{
	BOOL bLowerLimit = m_btnAskFieldLowerLimit.GetCheck();
	
	if (bLowerLimit)
		m_btnAskFieldUpperLimit.SetCheck(FALSE);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::UpperLimitChanged()
{
	BOOL bUpperLimit = m_btnAskFieldUpperLimit.GetCheck();
	
	if (bUpperLimit)
		m_btnAskFieldLowerLimit.SetCheck(FALSE);
}
//----------------------------------------------------------------------------
void CAskFieldDlg::CtrlStyleChanged()
{
	int nPos = m_cbCtrlStyle.GetCurSel();
	int nCurrentStyle = AskFieldData::EDIT;
	if (nPos >= 0)
	{
		CString sSelected;
		m_cbCtrlStyle.GetLBText(nPos, sSelected);
		if (sSelected.CompareNoCase(szRadioStyle) == 0)
			nCurrentStyle = AskFieldData::RADIO_BUTTON;
		else if(sSelected.CompareNoCase(szCheckBoxStyle) == 0)
			nCurrentStyle = AskFieldData::CHECK_BOX;
		else if(sSelected.CompareNoCase(szComboStyle) == 0)
			nCurrentStyle = AskFieldData::COMBO_BOX;
		else if(sSelected.CompareNoCase(szEditStyle) == 0 && m_nsHotLink.IsValid() && m_pHLAddOnModule && m_bHLHasCombo)
			nCurrentStyle = AskFieldData::EDIT_HKL;
	}

	BOOL bEnablePosCaption = (nCurrentStyle == AskFieldData::EDIT || nCurrentStyle == AskFieldData::COMBO_BOX || nCurrentStyle == AskFieldData::EDIT_HKL);
	if (!bEnablePosCaption)
		m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);

	GetDlgItem(IDC_ASK_CAPTION_POSITION_TAG)->EnableWindow(bEnablePosCaption);
	m_cbCaptionPos.EnableWindow(bEnablePosCaption);
	m_btnAskFieldCheckRadioOnLeft.EnableWindow(!bEnablePosCaption);
	m_btnAskFieldCheckRadioTextOnLeft.EnableWindow(!bEnablePosCaption);

	SetCurrentCtrlStyle(nCurrentStyle);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::HotLinkGroupChanged()
{
	m_edtHotLinkParams.SetCtrlStyle(m_edtHotLinkParams.GetCtrlStyle() & ~STR_STYLE_NO_EMPTY);
	m_edtHotLinkParams.ClearCtrl();
	m_edtHotLinkParams.EnableWindow(FALSE);

	m_cbHotLinkNames.SetRedraw(FALSE);
	m_cbHotLinkNames.ResetContent();

	BOOL bLenNotNull = m_edtAskFieldLen.GetValue() != 0;
	BOOL bFound = FALSE;

	if (m_cbHotLinkGroups.GetCurSel() < 0)
	{
		m_cbHotLinkNames.EnableWindow(FALSE);
		m_cbHotLinkNames.SetCurSel(-1);
		m_cbHotLinkNames.SetRedraw(TRUE);
		m_cbHotLinkNames.Invalidate(FALSE);
		DoCtrlStyleChanged();

		m_cbHotLinkGroups.EnableWindow(bLenNotNull && bFound);
		return;
	}

	// si posiziona per default su nessun query component selezionato
	int idx = m_cbHotLinkNames.AddString(Strings::NO_GROUP());
	m_cbHotLinkNames.SetItemDataPtr(idx, NULL);
	m_cbHotLinkNames.SetCurSel(idx);

	CTBNamespace* pGroup = (CTBNamespace*) m_cbHotLinkGroups.GetItemData(m_cbHotLinkGroups.GetCurSel());
	if (!pGroup || pGroup->GetObjectName() == Strings::NO_GROUP())
		return;

	AddOnModule* pAddOnMod = AfxGetAddOnModule(*pGroup);
	if (!pAddOnMod || !pAddOnMod->m_bIsValid)
	{
		m_cbHotLinkNames.EnableWindow(FALSE);
		m_cbHotLinkNames.SetCurSel(-1);
		m_cbHotLinkNames.SetRedraw(TRUE);
		m_cbHotLinkNames.Invalidate(FALSE);
		return;
	}

	const CBaseDescriptionArray& aObjects = pAddOnMod->m_XmlDescription.GetReferencesInfo().GetHotLinks();
	m_cbHotLinkNames.EnableWindow(aObjects.GetSize());

	for (int o=0; o <= aObjects.GetUpperBound(); o++)
	{
		CFunctionDescription* pObject = (CFunctionDescription*) aObjects.GetAt(o);
		if	(pObject->IsPublished() && m_cbHotLinkNames.FindString(0, pObject->GetNamespace().ToString()) < 0)
		{
			idx = m_cbHotLinkNames.AddString(pObject->GetTitle().IsEmpty() ? pObject->GetNamespace().GetObjectName() : pObject->GetTitle());
			m_cbHotLinkNames.SetItemDataPtr(idx,pObject);
		}
	}

	m_cbHotLinkNames.SetRedraw(TRUE);
	m_cbHotLinkNames.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::HotLinkNameChanged()
{
	DWORD dwStyle = m_edtHotLinkParams.GetCtrlStyle();
	int nIdx = m_cbHotLinkNames.GetCurSel();
	GetDlgItem(IDC_ASK_NAMESPACE)->SetWindowText(_T(""));
	
	if (nIdx >= 0)
	{
		CBaseDescription* pInfo = (CBaseDescription*) m_cbHotLinkNames.GetItemDataPtr(nIdx);
		CFunctionDescription* pDecri;

		// ItemData == NULL e` quello relativo a <Nessuno>
		if (pInfo)
		{
			if (!SetHotLinkNamespace(pInfo->GetNamespace()))
				return;

			pDecri = (CFunctionDescription*) m_pHLAddOnModule->m_XmlDescription.GetParamObjectInfo(pInfo->GetNamespace());
			if (!pDecri)
				return;

			GetDlgItem(IDC_ASK_NAMESPACE)->SetWindowText(m_nsHotLink.ToUnparsedString());

			if (pDecri->GetParameters().GetSize())
			{
				CString strProto;
				for (int i = 0; i <= pDecri->GetParameters().GetUpperBound(); i++)
				{
					strProto += _T("<") + pDecri->GetParamDescription(i)->GetTitle() + _T(">");
					if (i < pDecri->GetParameters().GetUpperBound())
						strProto += cwsprintf(T_SEP) + _T(" ");
				}

				m_edtHotLinkParams.SetCtrlStyle(dwStyle | STR_STYLE_NO_EMPTY);
				m_edtHotLinkParams.SetValue(strProto);
				m_edtHotLinkParams.EnableWindow(m_edtAskFieldLen.GetValue() != 0);
			}
			else
			{
				m_edtHotLinkParams.SetCtrlStyle(dwStyle & ~STR_STYLE_NO_EMPTY);
				m_edtHotLinkParams.ClearCtrl();
				m_edtHotLinkParams.EnableWindow(FALSE);
			}

			DoCtrlStyleChanged();

			if (m_bHLHasCombo)
				SetCurrentCtrlStyle(AskFieldData::COMBO_BOX);
			else
				SetCurrentCtrlStyle(AskFieldData::EDIT_HKL);

			return;
		}
		else
		{
			m_nsHotLink.Clear();
			m_bHLHasCombo = FALSE;
			m_pHLAddOnModule = NULL;

			SetCurrentCtrlStyle(AskFieldData::EDIT);
		}

		DoCtrlStyleChanged();
	}

	m_edtHotLinkParams.SetCtrlStyle(dwStyle & ~STR_STYLE_NO_EMPTY);
	m_edtHotLinkParams.ClearCtrl();
	m_edtHotLinkParams.EnableWindow(FALSE);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::ShowHotlinkGroups ()
{
	m_cbHotLinkGroups.ResetContent();

	CString sTitle;
	// ciclo per le AddOnApplications e gli AddOnModules che possiedono la definizione
	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOnApp);

		if (!pAddOnApp)
			continue;

		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound() ; n++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			ASSERT(pAddOnMod);

			sTitle = cwsprintf(_T("%s (%s)"), pAddOnMod->GetModuleTitle(), pAddOnApp->GetTitle());

			if (
					!pAddOnMod || !pAddOnMod->m_bIsValid || !pAddOnMod->HasHotlinks() ||
					m_cbHotLinkGroups.FindStringExact(0, sTitle) >= 0 
					||
					!AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName())
				)
				continue;
			
			int nIdx = m_cbHotLinkGroups.AddString(sTitle);
			m_cbHotLinkGroups.SetItemDataPtr(nIdx, &pAddOnMod->m_Namespace);
		}
	}

	if (m_cbHotLinkGroups.GetCount())
		m_cbHotLinkGroups.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::ShowCurrentHotlinkGroup()
{
	if (!m_pAskField->m_nsHotLink.IsValid())
	{
		m_nsHotLink.Clear();
		m_bHLHasCombo = FALSE;
		m_pHLAddOnModule = NULL;
		return;
	}
	
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_pAskField->m_nsHotLink.GetApplicationName());
	if (!pAddOnApp)
		return;

	if (!SetHotLinkNamespace(m_pAskField->m_nsHotLink))
		return;

	CString sTitle = cwsprintf(_T("%s (%s)"), m_pHLAddOnModule->GetModuleTitle(), pAddOnApp->GetTitle());

	int nIdx = m_cbHotLinkGroups.FindStringExact(-1, sTitle);
	if (nIdx >= 0)
		m_cbHotLinkGroups.SetCurSel(nIdx);
}

//----------------------------------------------------------------------------
BOOL CAskFieldDlg::SetHotLinkNamespace(const CTBNamespace& nsHotLink)
{
	m_nsHotLink = nsHotLink;
	m_pHLAddOnModule = AfxGetAddOnModule(m_nsHotLink);

	if (m_pHLAddOnModule)
	{
		HotKeyLinkObj* pHL = AfxGetTbCmdManager()->RunHotlink(m_nsHotLink, NULL, NULL);
		if (pHL)
		{
			pHL->InitNamespace();
			m_bHLHasCombo = pHL->IsFillListBoxEnabled();
			delete pHL;

			return TRUE;
		}

		m_pHLAddOnModule = NULL;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void CAskFieldDlg::ShowCurrentHotlink()
{
	// mi posiziono sull' item corrente cercando nella combo la descrizione
	if (m_pHLAddOnModule)
	{
		CBaseDescription* pInfo = m_pHLAddOnModule->m_XmlDescription.GetParamObjectInfo(m_nsHotLink);
		if (pInfo)
			GetDlgItem(IDC_ASK_NAMESPACE)->SetWindowText(m_nsHotLink.ToUnparsedString());
		
		CString sTitle;
		if (pInfo)
		{
			if (pInfo->GetTitle().IsEmpty())
				sTitle = pInfo->GetNamespace().GetObjectName();
			else
				sTitle = pInfo->GetTitle();
		}

		m_cbHotLinkNames.SetCurSel(m_cbHotLinkNames.FindStringExact(-1, sTitle));
	}

	CString strParams, strParam;
	for (int p = 0; p <= m_pAskField->m_HotLinkParamsExpr.GetUpperBound(); p++)
	{
		if (m_pAskField->m_HotLinkParamsExpr[p])
		{
			strParam = ((Expression*)m_pAskField->m_HotLinkParamsExpr[p])->ToString();
			strParam.Trim();
			strParam.Replace (_T("\t\n"), _T(""));
			strParams += strParam;
		}
		if (p < m_pAskField->m_HotLinkParamsExpr.GetUpperBound())
			strParams += cwsprintf(T_SEP) + _T(" ");
	}

	strParams.Trim();
	if (!strParams.IsEmpty())
	{
		m_edtHotLinkParams.SetValue(strParams);
		m_edtHotLinkParams.EnableWindow(m_edtAskFieldLen.GetValue() != 0);
	}
}

//----------------------------------------------------------------------------
BOOL CAskFieldDlg::ShowAskFieldData()
{
	m_cbDataObjTypes.FillListBox();

	ShowHotlinkGroups();

	if (m_bIsNewField)
	{
		m_cbDataObjTypes.SetTypeValue(DATA_STR_TYPE);
		m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);

		DataTypeChanged();

		m_pAskField->m_strPublicName = cwsprintf(_TB("Field_{0-%d}"), m_pSymTable->GetCurId() + 1);
	}
	else
	{	
		SymField* pRepField = m_pSymTable->GetField(m_pAskField->m_strPublicName);
		if (pRepField == NULL)
		{
			AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD), MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}
		DataObj* pRepData = pRepField->GetData();
		if (pRepData == NULL)
		{
			AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD), MB_OK | MB_ICONEXCLAMATION);
			return FALSE;
		}

		m_cbDataObjTypes.	SetTypeValue(pRepData->GetDataType());
		m_cbDataObjTypes.	EnableWindow(FALSE);
		m_edtAskFieldName.	EnableWindow(FALSE);

		switch (m_pAskField->m_nCaptionPos)
		{
			case T_DEFAULT	: m_cbCaptionPos.SetCurSel(DEFAULT_CAPTION_POS);	break;
			case T_LEFT		: m_cbCaptionPos.SetCurSel(LEFT_CAPTION_POS);		break;
			case T_TOP		: m_cbCaptionPos.SetCurSel(TOP_CAPTION_POS);		break;
		}

		ShowCurrentHotlinkGroup();

		// setup del resto sulla base del data type (verranno chiamate anche
		// le funzioni di riempimento dei query component basato anche sui
		// gruppi selezionati dalle precedenti ShowCurrentHotlinkGroup,
		// Di default non viene pero` selezionato alcun componente
		DataTypeChanged();

		// Se al field erano associati dei componenti, ora e` possibile
		// selezionare quelli giusti
		ShowCurrentHotlink();

		// solo dopo aver validato l'hotlink posso settare lo stile
		SetCurrentCtrlStyle(m_pAskField->m_CtrlStyle);
	}

	m_edtAskFieldName.SetWindowText(m_pAskField->m_strPublicName);
	if (m_bIsNewField || !m_pAskField->m_pCaptionExpr)
	{
		m_edtAskFieldCaption.SetWindowText(m_pAskField->m_strCaption);
			
		m_btnAskFieldStaticCaption.SetCheck(1);
		m_btnAskFieldDynamicCaption.SetCheck(0);

		m_edtAskFieldCaption.EnableWindow(TRUE);
		m_edtAskFieldDynamicCaption.EnableWindow(FALSE);
	}
	else
	{
		m_btnAskFieldStaticCaption.SetCheck(0);
		m_btnAskFieldDynamicCaption.SetCheck(1);

		m_edtAskFieldCaption.EnableWindow(FALSE);
		m_edtAskFieldDynamicCaption.EnableWindow(TRUE);
	}

	m_btnAskFieldLowerLimit.SetCheck(m_pAskField->m_nInputLimit == T_LOWER_LIMIT);
	m_btnAskFieldUpperLimit.SetCheck(m_pAskField->m_nInputLimit == T_UPPER_LIMIT);

	// ridimensiona la dialog
	if (m_cbHotLinkGroups.GetCount() <= 0)
	{
		CRect rectDlg; GetWindowRect(rectDlg);
		CRect rectLeft; GetDlgItem(IDC_HKL_GROUP)->GetWindowRect(rectLeft);

		rectDlg.right = rectLeft.left;

		SetWindowPos
			(
				NULL, -1, -1, rectDlg.Width(), rectDlg.Height(),
				SWP_NOMOVE|SWP_NOZORDER|SWP_NOREDRAW|SWP_NOACTIVATE
			);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CAskFieldDlg::ReadHotlink()
{
	m_pAskField->m_CtrlStyle = AskFieldData::EDIT;

	CFunctionDescription* pDescri = NULL;
	int nIdx = m_cbHotLinkNames.GetCurSel();

	// recupera la descrizione registrata
	if (nIdx >= 0)
	{
		CBaseDescription* pInfo = (CBaseDescription*) m_cbHotLinkNames.GetItemDataPtr(nIdx);

		// ItemData == NULL e` quello relativo a <Nessuno>
		if(pInfo && SetHotLinkNamespace(pInfo->GetNamespace()))
		{
			pDescri = (CFunctionDescription*) m_pHLAddOnModule->m_XmlDescription.GetParamObjectInfo(pInfo->GetNamespace());

			CtrlStyleChanged();
		}
	}

	if (pDescri)
	{
		m_pAskField->m_nsHotLink = pDescri->GetNamespace();

		if (m_pAskField->m_bMultiSelectionCombo)
		{
			if (m_cbDataObjTypes.GetTypeValue() != DataType::String)
			{
				AfxMessageBox (_TB("The data type is incompatible with the hotlink return value. "));
				return FALSE;
			}
		}
		else if (m_cbDataObjTypes.GetTypeValue() != pDescri->GetReturnValueDescription().GetDataType())
		{
			AfxMessageBox (_TB("The data type is incompatible with the hotlink return value. "));
			return FALSE;
		}

		CString sParams (m_edtHotLinkParams.GetValue());
		sParams.Trim();
		sParams.Replace (_T("\t\n"), _T(""));
		Parser lex(sParams);
		if	(!m_pAskField->ParseHotlinkParam (lex, pDescri, *m_pSymTable, FALSE)	)
		{   
			// reselect edit field and focus it                 
			m_edtHotLinkParams.SetCtrlFocus(TRUE);
			AfxMessageBox (_TB("HotLink's parameters expression is wrong!"));
			return FALSE;
		}

		return TRUE;
	}

	m_pAskField->m_nsHotLink.Clear();
	m_pAskField->m_HotLinkParamsExpr.RemoveAll();

	return TRUE;
}

//----------------------------------------------------------------------------
void CAskFieldDlg::OnOK()
{                        
	if (!CheckForm())
		return;

	DataType	dataType = m_cbDataObjTypes.GetTypeValue();

	Expression*	tmpInitExpr = new Expression(m_pSymTable);
	
	if (!m_edtAskFieldDefault.CheckExp(*tmpInitExpr, dataType, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtAskFieldDefault.SetCtrlFocus(TRUE);

		SAFE_DELETE(tmpInitExpr);
		return;
	}
	
    Expression*	pReadOnlyExpr = new Expression(m_pSymTable);
	if (!m_edtAskFieldReadOnly.CheckExp(*pReadOnlyExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtAskFieldReadOnly.SetCtrlFocus(TRUE);

		SAFE_DELETE(pReadOnlyExpr);
		SAFE_DELETE(tmpInitExpr);
		return;
	}
	if (pReadOnlyExpr->IsEmpty())
	{
		SAFE_DELETE(pReadOnlyExpr);
	}

	Expression*	pWhenExpr = new Expression(m_pSymTable);
	if (!m_edtAskFieldWhen.CheckExp(*pWhenExpr, DATA_BOOL_TYPE, TRUE))
	{   
		// reselect edit field and focus it                 
		m_edtAskFieldWhen.SetCtrlFocus(TRUE);

		SAFE_DELETE(pWhenExpr);
		SAFE_DELETE(pReadOnlyExpr);
		SAFE_DELETE(tmpInitExpr);
		return;
	}
	if (pWhenExpr->IsEmpty())
	{
		SAFE_DELETE(pWhenExpr);
	}

	CString	strName = m_edtAskFieldName.GetValue();
		
	WoormField* pRepField = NULL;

	if (m_bIsNewField)
	{
		DataType dataType = m_cbDataObjTypes.GetTypeValue();

		pRepField = m_pSymTable->GetField(strName);
		if (pRepField)
		{
			{
				AfxMessageBox(_TB("A field with the same name already exists"), MB_OK | MB_ICONERROR);

				SAFE_DELETE(pReadOnlyExpr);
				SAFE_DELETE(tmpInitExpr);
				SAFE_DELETE(pWhenExpr);
				return;
			}
			//if (pRepField->GetDataType() != dataType)
			//{
			//	AfxMessageBox(_TB("An other field with the same name already exists, but it has a different data type"), MB_OK | MB_ICONERROR);
			//	if (pReadOnlyExpr)
			//		delete pReadOnlyExpr;
			//	delete tmpInitExpr;
			//	return;
			//}
		}
		else
		{
			pRepField = new WoormField
				(
					strName,	
					WoormField::FIELD_INPUT
				);

			pRepField->SetDataType(dataType);
			pRepField->SetHidden	(TRUE);
		}
	}
	else
	{
		pRepField =  m_pSymTable->GetField(strName);
		if (pRepField == NULL)
		{
			AfxMessageBox(Expression::FormatMessage(Expression::UNKNOWN_FIELD), MB_OK | MB_ICONEXCLAMATION);

			SAFE_DELETE(pReadOnlyExpr);
			SAFE_DELETE(tmpInitExpr);
			SAFE_DELETE(pWhenExpr);
			return;
		}
	}

	if (m_btnAskFieldAlwaysReInit.GetCheck() == 1)
		pRepField->SetReInit (TRUE);
	else if (m_btnAskFieldNeverReInit.GetCheck() == 1)
		pRepField->SetStatic(TRUE);
	else
	{
		pRepField->SetReInit(FALSE);
		pRepField->SetStatic(FALSE);
	}

	m_pAskField->m_strPublicName	= strName;

	if (m_btnAskFieldDynamicCaption.GetCheck() == 1)
	{
		Expression*	pCaptionExpr = new Expression(m_pSymTable);
		if (!m_edtAskFieldDynamicCaption.CheckExp(*pCaptionExpr, DATA_STR_TYPE, TRUE))
		{   
			// reselect edit field and focus it                 
			m_edtAskFieldDynamicCaption.SetCtrlFocus(TRUE);
			delete pCaptionExpr;
			delete tmpInitExpr;
			return;
		}
		if (pCaptionExpr->IsEmpty())
		{
			m_edtAskFieldDynamicCaption.SetCtrlFocus(TRUE);
			delete pCaptionExpr;
			delete tmpInitExpr;
			return;
		}

		SAFE_DELETE(m_pAskField->m_pCaptionExpr);
		m_pAskField->m_pCaptionExpr	= pCaptionExpr;
		m_pAskField->m_strCaption.Empty();
	}
	else
	{
		CString strCaption; m_edtAskFieldCaption.GetWindowText(strCaption);
		m_pAskField->m_strCaption	= strCaption;

		SAFE_DELETE(m_pAskField->m_pCaptionExpr);
	}

	switch (m_cbCaptionPos.GetCurSel())
	{
		case DEFAULT_CAPTION_POS: m_pAskField->m_nCaptionPos = T_DEFAULT;	break;
		case LEFT_CAPTION_POS	: m_pAskField->m_nCaptionPos = T_LEFT;		break;
		case TOP_CAPTION_POS	: m_pAskField->m_nCaptionPos = T_TOP;		break;
	}

	if (pRepField->GetDataType() != DATA_BOOL_TYPE)
	{
		if (m_btnAskFieldLowerLimit.GetCheck())
			m_pAskField->m_nInputLimit = T_LOWER_LIMIT;
		else
			if (m_btnAskFieldUpperLimit.GetCheck())
				m_pAskField->m_nInputLimit = T_UPPER_LIMIT;
			else
				m_pAskField->m_nInputLimit = T_NOTOKEN;
	}
	else
	{
		m_pAskField->m_nInputLimit = T_NOTOKEN;
		int nStyleIdx = m_cbCtrlStyle.GetCurSel();
		if (nStyleIdx >= 0)
		{
			CString sSelection;
			m_cbCtrlStyle.GetLBText(nStyleIdx, sSelection);
			if (sSelection.CompareNoCase(szRadioStyle) == 0)
				m_pAskField->m_CtrlStyle = AskFieldData::RADIO_BUTTON;
			else if (sSelection.CompareNoCase(szEditStyle) == 0)
				m_pAskField->m_CtrlStyle = AskFieldData::EDIT;
			else
				m_pAskField->m_CtrlStyle = AskFieldData::CHECK_BOX;
		}
		if (m_pAskField->m_CtrlStyle != AskFieldData::EDIT)
		{
			m_pAskField->m_nCaptionPos = T_DEFAULT;
			m_pAskField->m_bLeftBoolAlign = m_btnAskFieldCheckRadioOnLeft.GetCheck();
			m_pAskField->m_bLeftTextBool = m_btnAskFieldCheckRadioTextOnLeft.GetCheck();
		}
	}

	if (m_pAskField->m_pReadOnlyExpr)
		delete m_pAskField->m_pReadOnlyExpr;
	if (m_pAskField->m_pWhenExpr)
		delete m_pAskField->m_pWhenExpr;

	m_pAskField->m_pReadOnlyExpr	= pReadOnlyExpr;
	m_pAskField->m_pWhenExpr		= pWhenExpr;
	if (pWhenExpr)
		m_pAskField->m_bDynamicHidden = m_btnAskFieldDynamicHidden.GetCheck() == 1;
	else
		m_pAskField->m_bDynamicHidden = FALSE;

	m_pAskField->m_bHiddenInput		= m_edtAskFieldLen.GetValue() == 0;

	pRepField->SetLen		(m_edtAskFieldLen.GetValue());
	pRepField->SetNumDec	(m_edtAskFieldPrec.GetValue());
	
	pRepField->SetInitExpression(tmpInitExpr); tmpInitExpr = NULL;

	m_pAskField->m_bMultiSelectionCombo	= (m_btnAskUseMultiSelectionCombo.GetCheck() == 1);
	m_pAskField->m_bDescriptionCombo	= (m_btnAskUseDescriptionCombo.GetCheck() == 1);
	m_pAskField->m_bShowHotLinkDescription = (m_btnShowHotLinkDescription.GetCheck() == 1);

	// il boolean e l'enum non hanno hotlinks
	if (dataType != DATA_BOOL_TYPE && dataType != DATA_ENUM_TYPE && !ReadHotlink())
	{
		if (m_bIsNewField)
			delete pRepField;

		return;
	}

	if (m_bIsNewField)
		m_pSymTable->Add (pRepField);

	if (m_pAskField->m_nsHotLink.IsValid())
		AfxGetTbCmdManager()->LoadNeededLibraries(m_pAskField->m_nsHotLink);

	EndDialog(TRUE);
}

//----------------------------------------------------------------------------
void CAskFieldDlg::OnCancel()
{
	EndDialog(FALSE);
}

