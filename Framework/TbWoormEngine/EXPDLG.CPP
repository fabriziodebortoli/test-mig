
#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>

#include <TbParser\TokensTable.h>

#include <TbGenlib\FunProto.h>

#include "ActionsRepEngin.h"
#include "prgdata.h"
#include "ruledata.h"
#include "rpsymtbl.h"
#include "expdlg.h"

//............................... Resources
#include "expdlg.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//===========================================================================
// CExpRuleDlg
//===========================================================================
IMPLEMENT_DYNAMIC(CExpRuleDlg, CParsedDialog)

BEGIN_MESSAGE_MAP(CExpRuleDlg, CParsedDialog)
	//{{AFX_MSG_MAP( CExpRuleDlg )
	ON_EN_VALUE_CHANGED	(IDC_EXPDLG_DATA_TYPE,	DataTypeChanged)
	ON_EN_VALUE_CHANGED	(IDC_EXPDLG_FIELDNAME,	PublicNameChanged)
	ON_COMMAND	(IDC_EXPDLG_CHK_ARRAY,	OnChkArrayChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CExpRuleDlg::CExpRuleDlg
	(
		ExpRuleData*	pExpRule,
		WoormTable* pSymTable,
		RuleDataArray*		pRuleData,
		WoormField*	pFieldData,	/* = NULL */
		DispTableInfo*	pDispTableInfo, /* = NULL */
		CBaseDocument*	pDocument  /* = NULL */
	)
	:
	CParsedDialog		(IDD_EXPRULE),
	m_pExpRule			(pExpRule),
	m_pSymTable			(pSymTable),
	m_pRepFieldData		(pFieldData),
	m_edtExpr			(pSymTable, pRuleData),
	m_edtFilter			(pSymTable, pRuleData),
	m_edtRepFieldName	(pSymTable, pFieldData),
	m_pDispTableInfo	(pDispTableInfo),
	m_pDocument			(pDocument)
{
	if (m_pRepFieldData && m_pExpRule->m_strPublicName.IsEmpty())
	{
		// is a new field
		m_bItIsNewExpr	= TRUE;
		m_pSymTable->Add (m_pRepFieldData);
		
		m_pRepFieldData->SetName(cwsprintf(WoormDialogStrings::FIELD(), m_pRepFieldData->GetId()));
		m_pRepFieldData->SetDataType(DATA_STR_TYPE);
		m_pRepFieldData->SetExprRuleField(TRUE);
	}
	else
	{
		m_bItIsNewExpr	= FALSE;
		m_pRepFieldData = (WoormField*) m_pSymTable->GetField(m_pExpRule->m_strPublicName);
	}                                                          
}

//----------------------------------------------------------------------------
BOOL CExpRuleDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_edtRepFieldName.	SubclassEdit	(IDC_EXPDLG_FIELDNAME,		this);
	m_edtRepFieldLen.	SubclassEdit	(IDC_EXPDLG_DATA_LEN,		this);
	m_edtRepFieldPrec.	SubclassEdit	(IDC_EXPDLG_DATA_PREC,		this);
	m_edtExpr.			SubclassEdit	(IDC_EXPDLG_EXPRESSION,		this);
	m_edtFilter.		SubclassEdit	(IDC_EXPDLG_WCLAUSE,		this);
	m_cbDataObjTypes.	SubclassEdit	(IDC_EXPDLG_DATA_TYPE,		this);
	m_chkArray.			SubclassEdit	(IDC_EXPDLG_CHK_ARRAY,		this);

	if (m_pDispTableInfo)
		m_pDispTableInfo->OnInitDialog(this);

	ShowData();
	return TRUE;
}

//----------------------------------------------------------------------------
void CExpRuleDlg::ShowData()
{
	DataType currDataType = m_pRepFieldData->GetData()->GetDataType();

	m_edtRepFieldName.	SetValue(m_pRepFieldData->GetName());
	m_cbDataObjTypes.	FillListBox();
	m_cbDataObjTypes.	SetTypeValue(currDataType);
	m_chkArray.SetValue (m_pRepFieldData->GetData()->IsKindOf(RUNTIME_CLASS(DataArray)));

	if (m_bItIsNewExpr)
	{
		DataTypeChanged();
		return;
	}

	GetDlgItem(IDC_EXPDLG_HIDDEN)->EnableWindow(FALSE);
	m_chkArray.EnableWindow(FALSE);
	m_cbDataObjTypes.EnableWindow(FALSE);
	m_edtRepFieldName.EnableWindow(FALSE);

	if (m_pRepFieldData->GetLen() != 0)
		m_edtRepFieldLen.SetValue(m_pRepFieldData->GetLen());

	if (m_pRepFieldData->GetNumDec() != 0)
		m_edtRepFieldPrec.SetValue(m_pRepFieldData->GetNumDec());

	if (m_pExpRule->m_pValidateWhereExpr)
		m_edtFilter.SetWindowText(m_pExpRule->m_pValidateWhereExpr->ToString());

	if (m_pExpRule->m_TestExpr.IsEmpty())
	{
    	m_edtExpr.SetWindowText(m_pExpRule->m_ThenExpr.ToString());
		return;
	}

	CString strTmp;
	strTmp	+= cwsprintf(T_IF);
	strTmp	+= " ";
	strTmp	+= m_pExpRule->m_TestExpr.ToString();
	strTmp	+= "\r\n\t";
    strTmp	+= cwsprintf(T_THEN);
	strTmp	+= " ";
	strTmp	+= m_pExpRule->m_ThenExpr.ToString();

	if (!m_pExpRule->m_ElseExpr.IsEmpty())
	{
		strTmp	+= "\r\n\t";
		strTmp	+= cwsprintf(T_ELSE);
		strTmp	+= " ";
		strTmp	+= m_pExpRule->m_ElseExpr.ToString();
	}
	
	ConvertCString(strTmp, LF_TO_CRLF);
	m_edtExpr.SetWindowText(strTmp);
}

//----------------------------------------------------------------------------
void CExpRuleDlg::OnChkArrayChanged	()
{
	if (m_chkArray.GetValue())
	{
		CheckDlgButton(IDC_EXPDLG_HIDDEN, 1);
		GetDlgItem(IDC_EXPDLG_HIDDEN)->EnableWindow(FALSE);

		m_edtExpr.SetWindowText(_T(""));
		m_edtFilter.SetWindowText(_T(""));
		m_edtExpr.EnableWindow(FALSE);
		m_edtFilter.EnableWindow(FALSE);
	}
	else
	{
		GetDlgItem(IDC_EXPDLG_HIDDEN)->EnableWindow(TRUE);

		m_edtExpr.EnableWindow(TRUE);
		m_edtFilter.EnableWindow(TRUE);
	}
	DataTypeChanged();
}

//----------------------------------------------------------------------------
void CExpRuleDlg::DataTypeChanged()
{
	DataType curDataType = m_cbDataObjTypes.GetTypeValue();
	int	nDefaultLen	= AfxGetFormatStyleTable()->GetOutputCharLen(curDataType, NULL);
	
	m_edtRepFieldLen.SetValue(nDefaultLen);
	m_pRepFieldData->SetDataType(curDataType, m_chkArray.GetValue());
			
	GetDlgItem(IDC_EXPDLG_DATA_PREC)->EnableWindow(curDataType == DATA_DBL_TYPE);
}

//----------------------------------------------------------------------------
void CExpRuleDlg::PublicNameChanged()
{
	if (m_bItIsNewExpr)
	{
		CString strName; m_edtRepFieldName.GetValue(strName);
		if (AfxGetTokensTable()->GetKeywordsToken(strName) != T_NOTOKEN)
		{
			m_edtRepFieldName.SetError(_TB("The name collides with a reserved word of TaskBuilder"));
			return;
		}
	
		m_pRepFieldData->SetName(strName);
	}
}

//----------------------------------------------------------------------------
BOOL CExpRuleDlg::CheckExprOK()
{
	CString			strExpr;
	Expression		exprTest(m_pSymTable);
	Expression		exprThen(m_pSymTable);
	Expression		exprElse(m_pSymTable);

	m_edtExpr.GetWindowText(strExpr);
	ConvertCString(strExpr, CRLF_TO_LF);
	
	Parser	lex(strExpr);

	if (!lex.LookAhead(T_IF))
	{
		if (m_chkArray.GetValue())
		{
			m_pExpRule->m_ThenExpr.	Reset();
			m_pExpRule->m_TestExpr.	Reset();
			m_pExpRule->m_ElseExpr.	Reset();
            return TRUE;
		}

		if (m_edtExpr.CheckExp(lex, exprThen, m_pRepFieldData->GetDataType()))
		{
			m_pExpRule->m_ThenExpr	= exprThen;

			m_pExpRule->m_TestExpr.	Reset();
			m_pExpRule->m_ElseExpr.	Reset();
            return TRUE;
		}
		
		return FALSE;
	}
	if (!lex.ParseTag(T_IF) || m_chkArray.GetValue())
		return FALSE;

    exprTest.SetStopTokens(T_THEN);
	if (!m_edtExpr.CheckExp(lex, exprTest, DATA_BOOL_TYPE))
		return FALSE;

	if (!lex.ParseTag(T_THEN)) 
		return FALSE;

	exprThen.SetStopTokens(T_ELSE, T_SEP);
	if (!m_edtExpr.CheckExp(lex, exprThen, m_pRepFieldData->GetDataType()))
		return FALSE;

	lex.Matched(T_SEP);

	if (lex.Matched(T_ELSE))
	{
		exprElse.SetStopTokens(T_SEP);
		if (!m_edtExpr.CheckExp(lex, exprElse, m_pRepFieldData->GetDataType()))
			return FALSE;

		lex.Matched(T_SEP);
	}

	m_pExpRule->m_TestExpr	= exprTest;
	m_pExpRule->m_ThenExpr	= exprThen;
	m_pExpRule->m_ElseExpr	= exprElse;
	
	return TRUE;
}

//----------------------------------------------------------------------------
void CExpRuleDlg::OnOK()
{                                            
	if (!CheckForm())
		return;
		
	if (m_pDispTableInfo && !m_pDispTableInfo->CheckControls())
		return;

	Expression tmpFilterExpr(m_pSymTable);

	if (m_bItIsNewExpr)
    {
		m_pRepFieldData->SetName(m_edtRepFieldName.GetValue());
		m_edtRepFieldName.GetValue(m_pExpRule->m_strPublicName);
		
		if (IsDlgButtonChecked(IDC_EXPDLG_HIDDEN))
			m_pRepFieldData->SetHidden(TRUE);
	}

	m_pRepFieldData->SetLen			(m_edtRepFieldLen.GetValue());
	m_pRepFieldData->SetNumDec		(m_edtRepFieldPrec.GetValue());

	if (m_edtFilter.CheckExp(tmpFilterExpr, DATA_BOOL_TYPE, TRUE))
	{
		if (CheckExprOK())
		{
			*m_pExpRule->m_pValidateWhereExpr = tmpFilterExpr;
			EndDialog(IDOK);
	      	return;
		}
	
		// reselect edit field and focus it                 
		m_edtExpr.SetCtrlFocus(TRUE);
	}
	else
		m_edtFilter.SetCtrlFocus(TRUE);
}

//----------------------------------------------------------------------------
void CExpRuleDlg::OnCancel()
{
	if (m_bItIsNewExpr)
	{
		m_pSymTable->DelField( m_pRepFieldData->GetId() );
		delete m_pExpRule;
	}

	EndDialog(IDCANCEL);
}
