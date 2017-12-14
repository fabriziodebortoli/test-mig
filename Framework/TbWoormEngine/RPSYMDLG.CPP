

#include "stdafx.h"

#include <TbNameSolver\TBNamespaces.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\FunProto.h>

#include <TbParser\TokensTable.h>

#include "ActionsRepEngin.h"
#include "edtmng.h"
#include "prgdata.h"
#include "repdata.h"
#include "rpsymtbl.h"
#include "rpsymdlg.h"
#include "report.h"


#include "INPUTMNG.hjson" //JSON AUTOMATIC UPDATE
#include "rpsymdlg.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//============================================================================
// CExpressionEdit
//============================================================================
BEGIN_MESSAGE_MAP(CExpressionEdit, CExpEdit)
	//{{AFX_MSG_MAP(CExpressionEdit)
	ON_WM_LBUTTONDBLCLK	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CExpressionEdit::CExpressionEdit(WoormTable* pSymTab, RuleDataArray* pRuleData) 
	:
	CExpEdit	(pSymTab, pRuleData)
{
	m_pLex		= NULL;
	m_pRepField	= NULL;		// attached from owner
	m_bMustCheck= TRUE;
}

//------------------------------------------------------------------------------
CExpressionEdit::~CExpressionEdit()
{
	if (m_pLex)
	{
		m_pLex->ClearError();
		delete m_pLex;
		m_pLex = NULL;
	}
}

//------------------------------------------------------------------------------
void CExpressionEdit::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	m_bMustCheck = FALSE;
	
	CExpEdit::OnLButtonDblClk(nFlags, point);
	
	m_bMustCheck = TRUE;
}

//------------------------------------------------------------------------------
BOOL CExpressionEdit::ErrorMessage()
{
	if (m_pLex)
	{
		delete m_pLex;
		m_pLex = NULL;
	}
	
	SetModifyFlag(TRUE);
	SetCtrlFocus(TRUE);
	
	return TRUE;
}

//------------------------------------------------------------------------------
void CExpressionEdit::Attach(WoormField* pRepField)
{
	m_pRepField = pRepField;
}

//============================================================================
// CFuncEdit
//============================================================================
BEGIN_MESSAGE_MAP(CFuncEdit, CExpressionEdit)
	//{{AFX_MSG_MAP(CFuncEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CFuncEdit::CFuncEdit(WoormTable* pSymTab, RuleDataArray* pRuleData) 
	:
	CExpressionEdit	(pSymTab, pRuleData)
{}

//----------------------------------------------------------------------------
BOOL CFuncEdit::IsValid()
{
	if (!CExpressionEdit::IsValid()) 
		return FALSE;

	if (!m_bMustCheck) 
		return TRUE;
	
	if (!GetModifyFlag()) 
		return TRUE;
	
	if (!m_pRepField) 
		return FALSE;
	
	EventFunction* pFunc = new EventFunction(m_pSymTable, *m_pRepField);

	if (!CheckFunctionData(pFunc))
	{
		delete pFunc;

		SetError(_TB("Syntax error"));
		return FALSE;
	}
	else
	{
		if (pFunc->IsEmpty())
		{
			delete pFunc;
				
			SAFE_DELETE(m_pRepField->GetEventFunction());
		}
		else
		{
			if (m_pRepField->GetEventFunction())
			{
				// se è una cella allora non è possibile cambiare il nome del campo su cui si fa 
				// la valutazione per non fare ad esempio SubTotal o ColTotal su una colonno diversa da quella
				// su cui si fa il display del campo (anomalia 3155)
				if (m_pRepField->IsTableRuleField())
					pFunc->SetPublicName(m_pRepField->GetEventFunction()->GetPublicName());
				delete m_pRepField->GetEventFunction();
			}
		
			m_pRepField->GetEventFunction() = pFunc;
		}

		return TRUE;
	}
}

//----------------------------------------------------------------------------
BOOL CFuncEdit::CheckFunctionData(EventFunction* pFunc)
{
	if (m_pLex) 
		return FALSE;

	if (!m_pRepField) 
		return FALSE;
	
	CString		strFunction;
	GetWindowText(strFunction);

	BOOL bTotal = 	m_pRepField->IsSubTotal()	||
					m_pRepField->IsColTotal();

	Parser* pLex = new Parser(strFunction);

	if (strFunction.IsEmpty())
	{
		pFunc->Reset();
	
		if (bTotal)
		{
			m_pLex = pLex;
			return m_pLex->SetError(_TB("No function is associated to the field Total"), m_pRepField->GetName());
		}
	}
	else
	{
		if (!pFunc->Parse(*pLex))
		{
			m_pLex = pLex;
			return FALSE;
		}

		if	(
				bTotal && !pFunc->IsAnExpression() &&
				pFunc->GetPublicName().CompareNoCase(m_pRepField->GetEventFunction()->GetPublicName())
			)
		{
			m_pLex = pLex;
			return m_pLex->SetError(_TB("Field Total with different function parameter and column name"), pFunc->GetPublicName());
		}
	}

	delete pLex;
	return TRUE;
}

//============================================================================
// CInitEdit
//============================================================================
BEGIN_MESSAGE_MAP(CInitEdit, CExpressionEdit)
	//{{AFX_MSG_MAP(CInitEdit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CInitEdit::CInitEdit(WoormTable* pSymTab, RuleDataArray* pRuleData) 
	:
	CExpressionEdit	(pSymTab, pRuleData)
{}

//----------------------------------------------------------------------------
BOOL CInitEdit::IsValid()
{
	if (m_pLex) return FALSE;

	if (!CExpressionEdit::IsValid()) return FALSE;

	if (!m_bMustCheck) return TRUE;

	if (!GetModifyFlag()) return TRUE;
	
	if (!m_pRepField) return FALSE;

	CString		strExpr;
	GetWindowText(strExpr);

	Parser* pLex = new Parser(strExpr);

	Expression*	initValue = new Expression (m_pSymTable);

	if (strExpr.IsEmpty() && m_pRepField->GetInitExpression())
	{
		m_pRepField->GetInitExpression()->Reset();

		delete pLex;
		return TRUE;
	}

	if (!CheckExp(*pLex, *initValue, m_pRepField->GetDataType()))
	{
		m_pLex = pLex;

		SetError(_TB("Syntax error"));
		delete initValue;
		return FALSE;
	}
		
	m_pRepField->SetInitExpression(initValue); initValue = NULL;

	delete pLex;
	return TRUE;
}


//============================================================================
// CReportSymTableDlg
//============================================================================

IMPLEMENT_DYNAMIC(CReportSymTableDlg, CParsedDialog)
	
BEGIN_MESSAGE_MAP(CReportSymTableDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CReportSymTableDlg)
	ON_LBN_SELCHANGE	(IDC_RPSYMDLG_LIST,		SelectRepFieldAction)
	ON_LBN_DBLCLK		(IDC_RPSYMDLG_LIST,		ModifyRule)
	ON_BN_CLICKED		(IDC_RPSYMDLG_OPEN_RULE,ModifyRule)	

	ON_EN_KILLFOCUS		(IDC_RPSYMDLG_LEN,		FieldLenChangedAction)
	ON_EN_KILLFOCUS		(IDC_RPSYMDLG_PRECISION,FieldPrecChangedAction)
	
	ON_EN_VALUE_CHANGED	(IDC_RPSYMDLG_XML_ALIAS,FieldXmlAliasChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_NO_XML,	FieldXmlAliasChangedAction)

    ON_BN_CLICKED		(IDC_RPSYMDLG_NORMAL,	FieldTypeChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_COLUMN,	FieldTypeChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_INPUT,	FieldTypeChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_VISIBLE,	FieldTypeChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_HIDDEN,	FieldTypeChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_ALL,		FieldTypeChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_DEL_FIELD,DeleteFieldAction)

	ON_BN_CLICKED		(IDC_RPSYMDLG_FLAG_INPUT,	FieldSetInputChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_FLAG_REINIT,	FieldSetReinitChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_FLAG_STATIC,	FieldSetStaticChangedAction)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CReportSymTableDlg::CReportSymTableDlg(EditorManager* pEdtMng)
	:
	CParsedDialog		(IDD_RPSYMTABLE),
	m_pSymTable			(pEdtMng->GetSymTable()),
	m_pEdtMng			(pEdtMng),
	m_bFieldNormal		(FALSE),
	m_bFieldColumn		(FALSE),
	m_bFieldInput		(FALSE),
	m_bFieldVisible		(FALSE),
	m_bFieldHidden		(FALSE),
	m_bFieldAll			(TRUE),
	m_edtRepFieldFunc	(pEdtMng->GetSymTable(), pEdtMng->GetRuleData()),
	m_edtRepFieldInit	(pEdtMng->GetSymTable(), pEdtMng->GetRuleData())
{}

//----------------------------------------------------------------------------
BOOL CReportSymTableDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	if (m_pSymTable->IsEmpty())
	{
		AfxMessageBox(_TB("No field defined!"), MB_OK | MB_ICONEXCLAMATION);
		EndDialog(IDCANCEL);
		return FALSE;
	}

	m_lbRepField.		SubclassDlgItem	(IDC_RPSYMDLG_LIST,		this);
	m_edtRepFieldLen.	SubclassEdit	(IDC_RPSYMDLG_LEN,		this);
	m_edtRepFieldPrec.	SubclassEdit	(IDC_RPSYMDLG_PRECISION,this);
	m_edtRepFieldFunc.	SubclassEdit	(IDC_RPSYMDLG_FUNC,		this);
	m_edtRepFieldInit.	SubclassEdit	(IDC_RPSYMDLG_INIT,		this);
	m_cbDataObjTypes.	SubclassEdit	(IDC_RPSYMDLG_TYPE,		this);
	m_cbDataObjTypes.	EnableWindow	(FALSE);

	m_edtRepFieldAliasName.SubclassEdit (IDC_RPSYMDLG_XML_ALIAS, this);
	m_edtRepFieldAliasName.SetCtrlStyle(m_edtRepFieldAliasName.GetCtrlStyle() && ~IDE_STYLE_NO_EMPTY);

	CheckRadioButton(IDC_RPSYMDLG_NORMAL, IDC_RPSYMDLG_ALL, IDC_RPSYMDLG_ALL);
	UpdateFieldsList();
	return TRUE;
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::DeleteFieldAction()
{
	CString	strCurrName;
	int		nCurrPos = m_lbRepField.GetCurSel();

	m_lbRepField.GetText(nCurrPos, strCurrName);

	WoormField* pRepField = m_pSymTable->GetField(strCurrName);
	
	CString strMess = cwsprintf(_TB("Are you sure you want to delete the {0-%s} field?"), (LPCTSTR) strCurrName);
	if (pRepField->IsSubTotal())
		strMess += CString("\r\n\r\n") + _TB("WARNING!: only actions directly referencing the above-mentioned field -- but no correlated ones -- will be deleted from the correctly defined events.\r\nCheck the result of the operationf or consistency.");
	
	if (AfxMessageBox(strMess, MB_OKCANCEL | MB_ICONQUESTION) != IDOK)
		return;

	m_pEdtMng->GetPrgData()->DeleteField( pRepField->GetName() );

	m_lbRepField.DeleteString(nCurrPos);

	if (nCurrPos >= m_lbRepField.GetCount())
		nCurrPos--;

	m_lbRepField.SetCurSel(nCurrPos);

	SelectRepFieldAction();
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldTypeChangedAction()
{
	m_bFieldNormal	= IsDlgButtonChecked(IDC_RPSYMDLG_NORMAL);
	m_bFieldColumn	= IsDlgButtonChecked(IDC_RPSYMDLG_COLUMN);
	m_bFieldInput	= IsDlgButtonChecked(IDC_RPSYMDLG_INPUT);
	m_bFieldVisible	= IsDlgButtonChecked(IDC_RPSYMDLG_VISIBLE);
	m_bFieldHidden	= IsDlgButtonChecked(IDC_RPSYMDLG_HIDDEN);
	m_bFieldAll		= IsDlgButtonChecked(IDC_RPSYMDLG_ALL);

	UpdateFieldsList();
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldLenChangedAction()
{
	CString	strCurrFieldName;

	m_lbRepField.GetText(m_lbRepField.GetCurSel(), strCurrFieldName);

	WoormField* m_pRepField = m_pSymTable->GetField(strCurrFieldName);
	if (m_pRepField)
		m_pRepField->SetLen(m_edtRepFieldLen.GetValue());
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldPrecChangedAction()
{
	CString	strCurrFieldName;

	m_lbRepField.GetText(m_lbRepField.GetCurSel(), strCurrFieldName);

	WoormField*	pRepField = m_pSymTable->GetField(strCurrFieldName);
	if (pRepField)
		pRepField->SetNumDec(m_edtRepFieldPrec.GetValue());
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldXmlAliasChangedAction()
{
	if (m_edtRepFieldAliasName.IsWindowEnabled())
	{
		CString sXAlias;
		m_edtRepFieldAliasName.GetWindowText(sXAlias);
		if (m_pSymTable->ExistField(sXAlias))
		{
			m_edtRepFieldAliasName.SetError(_TB("Field already defined!"));
			m_edtRepFieldAliasName.SetWindowText(sXAlias + _T("_1"));
			return;
		}

		int idx = m_lbRepField.GetCurSel();
		if (idx >= 0)
		{
			WoormField*	pRepField = (WoormField*) m_lbRepField.GetItemDataPtr(idx);
			pRepField->SetXmlName(sXAlias);
			pRepField->SetSkipXml ( (IsDlgButtonChecked(IDC_RPSYMDLG_NO_XML) == 1));
		}
		else
			ASSERT(FALSE);
	}
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldSetInputChangedAction()
{
	if (GetDlgItem(IDC_RPSYMDLG_FLAG_INPUT)->IsWindowEnabled())
	{
		int idx = m_lbRepField.GetCurSel();
		if (idx >= 0)
		{
			WoormField* pRepField = (WoormField*) m_lbRepField.GetItemDataPtr(idx);
	
			BOOL bInput = IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_INPUT) == 1;

			pRepField->SetFieldType (bInput ? WoormField::FIELD_INPUT : WoormField::FIELD_NORMAL);

			CButton* pCBReinit = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT);
			pCBReinit->EnableWindow(bInput);
			CButton* pCBStatic = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC);
			pCBStatic->EnableWindow(bInput);

			if (!bInput)
			{
				pCBReinit->SetCheck(0);
				pCBStatic->SetCheck(0);
			}
		}
		else
			ASSERT(FALSE);
	}
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldSetReinitChangedAction()
{
	if (GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT)->IsWindowEnabled())
	{
		int idx = m_lbRepField.GetCurSel();
		if (idx >= 0)
		{
			WoormField* pRepField = (WoormField*) m_lbRepField.GetItemDataPtr(idx);
	
			pRepField->SetReInit (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_REINIT) == 1);

			((CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC))->SetCheck(0);
		}
		else
			ASSERT(FALSE);
	}
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::FieldSetStaticChangedAction()
{
	if (GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC)->IsWindowEnabled())
	{
		int idx = m_lbRepField.GetCurSel();
		if (idx >= 0)
		{
			WoormField* pRepField = (WoormField*) m_lbRepField.GetItemDataPtr(idx);
	
			pRepField->SetStatic (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_STATIC) == 1);
			((CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT))->SetCheck(0);
		}
		else
			ASSERT(FALSE);
	}
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::UpdateFieldsList()
{
	m_lbRepField.ResetContent();

	int nItems = m_pSymTable->GetSize();
	for (int i = 0; i < nItems; i++)
	{
		WoormField* pF = (WoormField*) (*m_pSymTable)[i];
		if (pF->IsPredefinedField()) 
			continue;

		if	(
				(m_bFieldHidden		&& pF->IsHidden() && !pF->IsAsk()) ||
				(m_bFieldVisible	&& !pF->IsHidden())						||
                (m_bFieldNormal		&& pF->IsNormal())	||
				(m_bFieldColumn		&& pF->IsColumn()) ||
				(m_bFieldInput		&& pF->IsAsk())	||
                (m_bFieldAll)
			)
		{
			int idx = m_lbRepField.AddString(pF->GetName());
			m_lbRepField.SetItemDataPtr(idx, pF);
		}
	}

	if (m_lbRepField.GetCount() > 0)
    {
		m_lbRepField.SetCurSel(0);

		SelectRepFieldAction();
		return;
	}
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::ShowFieldData(WoormField* pRepField)
{
	if (pRepField == NULL)
		return;

	if (pRepField->GetLen() == 0)
		m_edtRepFieldLen.SetWindowText(_T(""));
	else
		m_edtRepFieldLen.SetValue(pRepField->GetLen());

	if (pRepField->GetNumDec() == 0)
		m_edtRepFieldPrec.SetWindowText(_T(""));
	else
		m_edtRepFieldPrec.SetValue(pRepField->GetNumDec());

	CString s; 
	if (pRepField->GetInitExpression()) 
		s = pRepField->GetInitExpression()->ToString();
	m_edtRepFieldInit.SetWindowText(s);

	DataType dt = pRepField->GetData()->GetDataType();
	if (dt == DataType::Array)
	{
		((CButton*)GetDlgItem(IDC_RPSYMDLG_ARRAY))->SetCheck(1);
		m_cbDataObjTypes.SetTypeValue(((DataArray*)(pRepField->GetData()))->GetBaseDataType());

		m_edtRepFieldInit.EnableWindow(FALSE);
	}
	else
	{
		((CButton*)GetDlgItem(IDC_RPSYMDLG_ARRAY))->SetCheck(0);
		m_cbDataObjTypes.SetTypeValue(dt);

		m_edtRepFieldInit.EnableWindow(TRUE);

		ShowFunctionData(m_edtRepFieldFunc, pRepField->GetEventFunction());
	}
	
	GetDlgItem(IDC_RPSYMDLG_PRECISION)->EnableWindow(m_cbDataObjTypes.IsPrecisionEnabled());

	//----
	CButton* pCBInput = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_INPUT);
	CButton* pCBReinit = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT);
	CButton* pCBStatic = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC);

	pCBInput->EnableWindow(FALSE);
	pCBReinit->EnableWindow(FALSE);
	pCBStatic->EnableWindow(FALSE);

	pCBInput->SetCheck(pRepField->IsInput() ? 1 : 0);
	pCBReinit->SetCheck(0);
	pCBStatic->SetCheck(0);

	if (pRepField->IsHidden() && !pRepField->IsAsk())
	{
		pCBInput->EnableWindow(TRUE);
	}
	if (pRepField->IsInput())
	{
		pCBReinit->EnableWindow(TRUE);
		pCBStatic->EnableWindow(TRUE);
				
		if (pRepField->IsReInit())
			pCBReinit->SetCheck(1);
		if (pRepField->IsStatic())
			pCBStatic->SetCheck(1);
	}
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::ShowFunctionData(CEdit& cedit, EventFunction* pFunc)
{
	if (pFunc == NULL)
	{
		cedit.SetWindowText(_T(""));
		return;
	}

	if (pFunc->IsEmpty())
		return;

	CString strTmp;
	if (pFunc->IsAnExpression())
		strTmp = pFunc->ToString();
	else
	{
		strTmp += cwsprintf(pFunc->GetFunctionType());
		strTmp += cwsprintf(T_ROUNDOPEN);
		strTmp += pFunc->GetPublicName();
		strTmp += cwsprintf(T_ROUNDCLOSE);
	}

	cedit.SetWindowText(strTmp);
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::SelectRepFieldAction()
{
	GetDlgItem(IDC_RPSYMDLG_DESCRIPTION)->SetWindowText(_T(""));
	BOOL bEnable = FALSE;
	BOOL bEnableDelete = FALSE;
	WoormField*	pRepField = NULL;
	int idx = m_lbRepField.GetCurSel();
	if (idx >= 0)
	{
		pRepField = (WoormField*) m_lbRepField.GetItemDataPtr(idx);

		ShowFieldData	(pRepField);
		
		m_edtRepFieldFunc.Attach(pRepField);
		m_edtRepFieldInit.Attach(pRepField);
		
		bEnable = (!pRepField->IsInput() && pRepField->GetDataType() != DataType::Array);
	
		CString sLog;
		bEnableDelete =	(!pRepField->IsInput()) &&
						(pRepField->IsHidden() || pRepField->IsSubTotal()) &&
						m_pEdtMng->GetPrgData()->CanDeleteField(pRepField->GetName(), sLog);
		
		//----
		GetDlgItem(IDC_RPSYMDLG_OPEN_RULE)->EnableWindow(m_pEdtMng->GetRuleData()->GetRuleData(pRepField->GetId()) != NULL);

		//----
		CString sDescr;
		if (pRepField->IsHidden() && !pRepField->IsAsk())
			sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Hidden");
		else if (!pRepField->IsHidden())
			sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Visible");

		if (pRepField->IsAsk())
			sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Request");

		if (pRepField->IsColumn())
			sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Column");
		if (pRepField->IsSubTotal())
			sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Column Subtotal");
		if (pRepField->IsColTotal())
			sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Column Total");

		//Già gestite in altro modo
		//if (pRepField->HasAFunction())
		//	sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Function");
		//if (m_pEdtMng->GetRuleData()->GetRuleData(pRepField->GetId()))
		//	sDescr += (sDescr.IsEmpty() ? _T("") : _T(", ")) + _TB("Data Rule");
		
		//non è ancora gestito il logging completo della CanDeleteField
		//if (!sLog.IsEmpty())
		//	sDescr += _T("\r\n") + _TB("It is used by an other object") + _T("\r\n") + sLog;
		//----
		BOOL bXml = (pRepField->IsColumn() || pRepField->IsNormal());
		m_edtRepFieldAliasName.SetWindowText(pRepField->GetXmlName());
		CheckDlgButton(IDC_RPSYMDLG_NO_XML, pRepField->GetSkipXml() ? 1 : 0);
		GetDlgItem(IDC_RPSYMDLG_XML_ALIAS)->EnableWindow(bXml);
		GetDlgItem(IDC_RPSYMDLG_NO_XML)->EnableWindow(bXml);
		//----
		GetDlgItem(IDC_RPSYMDLG_DESCRIPTION)->SetWindowText(sDescr);
	}

	m_edtRepFieldFunc.EnableWindow(bEnable && (pRepField ? pRepField->HasAFunction() : TRUE));

	GetDlgItem(IDC_RPSYMDLG_FUNC_CAPTION)->EnableWindow(bEnable);
	GetDlgItem(IDC_RPSYMDLG_DEL_FIELD)->EnableWindow(bEnableDelete);
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::ModifyRule()
{
	int idx = m_lbRepField.GetCurSel();
	if (idx < 0)
	{
		return;
	}

	WoormField*	pRepField = (WoormField*) m_lbRepField.GetItemDataPtr(idx);

	if (m_pEdtMng->GetRuleData()->GetRuleData(pRepField->GetId()))
		m_pEdtMng->ModifyField(pRepField->GetId(), pRepField->IsHidden());
		
	SelectRepFieldAction();
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::OnOK()
{             
	if (!CheckForm())
		return;
	
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CReportSymTableDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//============================================================================
// CVarFieldDlg
//============================================================================
IMPLEMENT_DYNAMIC(CVarFieldDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CVarFieldDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CVarFieldDlg)
	ON_EN_VALUE_CHANGED	(IDC_VARFIELD_NAME,	FieldNameChanged)
	ON_EN_VALUE_CHANGED	(IDC_VARFIELD_TYPE,	DataTypeChanged)
	ON_COMMAND	(IDC_VARFIELD_ARRAY,	OnChkArrayChanged)

	ON_BN_CLICKED		(IDC_VARFIELD_HIDDEN,		FieldSetHiddenChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_FLAG_INPUT,	FieldSetInputChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_FLAG_REINIT,	FieldSetReinitChangedAction)
	ON_BN_CLICKED		(IDC_RPSYMDLG_FLAG_STATIC,	FieldSetStaticChangedAction)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CVarFieldDlg::CVarFieldDlg
	(
		WoormTable*	pSymTable,
		RuleDataArray*		pRuleData,
		WoormField*	pRepField,
	    WoormField*	pColumnRepField,
	    BOOL			bNew,
		DispTableInfo*	pDispTableInfo /* = NULL */
	)
	:
	CParsedDialog		(IDD_VARFIELD),
	m_pSymTable			(pSymTable),
	m_pRepField			(pRepField),
	m_pColRepField		(pColumnRepField),
	m_bIsNewField		(bNew),
	m_edtRepFieldFunc	(pSymTable, pRuleData),
	m_edtRepFieldInit	(pSymTable, pRuleData),
	m_edtRepFieldName	(pSymTable, pRepField),
	m_pDispTableInfo	(pDispTableInfo)
{
	if (m_bIsNewField)
	{
		// must be inserted into symbol table
		m_pSymTable->Add (m_pRepField);
		
		switch (m_pRepField->GetFieldType())
		{
			case WoormField::FIELD_SUBTOTAL:
				if (m_pColRepField)
					m_pRepField->SetName(cwsprintf
						(
							_TB("SubTot{0-%s}"),
							(LPCTSTR) m_pColRepField->GetName()
						));
				break;

			case WoormField::FIELD_COLTOTAL:
				if (m_pColRepField)
					m_pRepField->SetName(cwsprintf	
						(
							_TB("Tot{0-%s}"),
							(LPCTSTR) m_pColRepField->GetName()
						));
				break;
				
			default :
				m_pRepField->SetName(cwsprintf
						(
							_TB("Field_{0-%d}"),
							m_pRepField->GetId()
						));
		}

		if (m_pColRepField)
        {
			m_pRepField->SetDataType	(m_pColRepField->GetDataType());
        	m_pRepField->SetLen			(m_pColRepField->GetLen());
			m_pRepField->SetNumDec		(m_pColRepField->GetNumDec());
		}
	}
	
	m_edtRepFieldFunc.Attach(m_pRepField);
	m_edtRepFieldInit.Attach(m_pRepField);
}

//----------------------------------------------------------------------------
BOOL CVarFieldDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_edtRepFieldName.	SubclassEdit	(IDC_VARFIELD_NAME, 		this);
	m_edtRepFieldLen.	SubclassEdit	(IDC_VARFIELD_LEN,			this);
	m_edtRepFieldPrec.	SubclassEdit	(IDC_VARFIELD_PRECISION,	this);
	m_edtRepFieldInit.	SubclassEdit	(IDC_VARFIELD_INIT,			this);
	m_edtRepFieldFunc.	SubclassEdit	(IDC_VARFIELD_FUNCTION,		this);
	m_btnHiddenField.	SubclassDlgItem	(IDC_VARFIELD_HIDDEN,		this);
	m_cbDataObjTypes.	SubclassEdit	(IDC_VARFIELD_TYPE, 		this);
	m_chkArray.			SubclassEdit	(IDC_VARFIELD_ARRAY,		this);

	if (m_pDispTableInfo)
		m_pDispTableInfo->OnInitDialog(this);

	ShowFieldData();
	return TRUE;
}

//----------------------------------------------------------------------------
void CVarFieldDlg::FieldNameChanged()
{
	if (m_bIsNewField)
	{
		CString strName; m_edtRepFieldName.GetValue(strName);
		if (AfxGetTokensTable()->GetKeywordsToken(strName) != T_NOTOKEN)
		{
			m_edtRepFieldName.SetError(_TB("The name collides with a reserved word of TaskBuilder"));
			return;
		}
	
		m_pRepField->SetName(strName);
	}
}

//----------------------------------------------------------------------------
void CVarFieldDlg::OnChkArrayChanged()
{
	if (m_chkArray.GetValue())
	{
		m_btnHiddenField.SetCheck(1);
		m_btnHiddenField.EnableWindow(FALSE);

		m_edtRepFieldInit.SetWindowText(_T(""));
		m_edtRepFieldFunc.SetWindowText(_T(""));
		m_edtRepFieldInit.EnableWindow(FALSE);
		m_edtRepFieldFunc.EnableWindow(FALSE);
	}
	else
	{
		m_btnHiddenField.EnableWindow(TRUE);

		m_edtRepFieldInit.EnableWindow(TRUE);
		m_edtRepFieldFunc.EnableWindow(TRUE);
	}

	DataTypeChanged();
}

//----------------------------------------------------------------------------
void CVarFieldDlg::DataTypeChanged()
{
	DataType curDataType = m_cbDataObjTypes.GetTypeValue();
	if (curDataType == DataType::Null)
	{
		m_edtRepFieldName.SetError(_TB("Data type is mandatory"));
		return;
	}
	CTBNamespace* pContext =	m_pSymTable->GetDocument() ? 
								&m_pSymTable->GetDocument()->GetNamespace() :
								NULL;
	m_edtRepFieldLen.SetValue(AfxGetFormatStyleTable()->GetOutputCharLen(curDataType, pContext));

	GetDlgItem(IDC_VARFIELD_PRECISION)->	EnableWindow(m_cbDataObjTypes.IsPrecisionEnabled());

	m_pRepField->SetDataType(curDataType, m_chkArray.GetValue());
}

//----------------------------------------------------------------------------
void CVarFieldDlg::ShowFieldData()
{
	CString	strLen;
	CString	strPrecision;

	if (m_pRepField->GetLen() != 0)
		m_edtRepFieldLen.SetValue(m_pRepField->GetLen());

	if (m_pRepField->GetNumDec() != 0)
		m_edtRepFieldPrec.SetValue(m_pRepField->GetNumDec());

	m_edtRepFieldName.	SetValue(m_pRepField->GetName());

	CString s; 
	if (m_pRepField->GetInitExpression()) 
		s = m_pRepField->GetInitExpression()->ToString();
	m_edtRepFieldInit.SetWindowText(s);

	m_cbDataObjTypes.FillListBox();

	if (!m_bIsNewField)
    {
		m_edtRepFieldName.EnableWindow(FALSE);                  

		m_btnHiddenField.SetCheck(m_pRepField->IsHidden());
		m_btnHiddenField.EnableWindow(FALSE);

		DataObj* v = m_pRepField->GetData();
		m_chkArray.SetValue (v && v->IsKindOf(RUNTIME_CLASS(DataArray)));
		if (m_chkArray.GetValue())
			m_cbDataObjTypes.SetTypeValue(((DataArray*)v)->GetBaseDataType());
		else
			m_cbDataObjTypes.SetTypeValue(v->GetDataType());
		m_chkArray.EnableWindow(FALSE);
		
		m_cbDataObjTypes.EnableWindow(FALSE);
		
		ShowFunctionData(m_edtRepFieldFunc, m_pRepField->GetEventFunction());
		
		GetDlgItem(IDC_VARFIELD_PRECISION)->	EnableWindow(m_cbDataObjTypes.IsPrecisionEnabled());	}
	else
    {
		// se viene chiamata con field hidden non puo` essere modificato
		m_btnHiddenField.SetCheck(m_pRepField->IsHidden());
		m_btnHiddenField.EnableWindow(!m_pRepField->IsHidden());

		m_chkArray.SetValue (FALSE);
		m_cbDataObjTypes.SetTypeValue(DATA_STR_TYPE);

		DataTypeChanged();
	}
}

//----------------------------------------------------------------------------
void CVarFieldDlg::ShowFunctionData(CEdit& cedit, EventFunction* pFunc)
{
	if (pFunc == NULL)
	{
		cedit.SetWindowText(_T(""));
		return;
	}

	if (pFunc->IsEmpty())
		return;

	CString strTmp;
	if (pFunc->IsAnExpression())
		strTmp = pFunc->ToString();
	else
	{
		strTmp += cwsprintf(pFunc->GetFunctionType());
		strTmp += cwsprintf(T_ROUNDOPEN);
		strTmp += pFunc->GetPublicName();
		strTmp += cwsprintf(T_ROUNDCLOSE);
	}
	cedit.SetWindowText(strTmp);
}

//----------------------------------------------------------------------------
void CVarFieldDlg::OnOK()
{
	if (m_bIsNewField && m_edtRepFieldName.IsValid())
	{
		m_pRepField->SetName(m_edtRepFieldName.GetValue());

		if (m_btnHiddenField.GetCheck())
		{
			m_pRepField->SetHidden(TRUE);
			if (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_INPUT) == 1)
			{
				m_pRepField->SetFieldType (WoormField::FIELD_INPUT);

				if (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_STATIC) == 1)
					m_pRepField->SetStatic(TRUE);
				else if (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_REINIT) == 1)
					m_pRepField->SetReInit(TRUE);
			}
		}
	}

	// this statement must be here after field setting (if new)
	//
	if (!CheckForm())
		return;
		
	if (m_pDispTableInfo && !m_pDispTableInfo->CheckControls())
		return;

	m_pRepField->SetLen(m_edtRepFieldLen.GetValue());
	m_pRepField->SetNumDec(m_edtRepFieldPrec.GetValue());
	
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CVarFieldDlg::OnCancel()
{
	if (m_bIsNewField)
		m_pSymTable->DelField(m_pRepField->GetName());

	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CVarFieldDlg::FieldSetHiddenChangedAction()
{
	if (GetDlgItem(IDC_VARFIELD_HIDDEN)->IsWindowEnabled())
	{	
		BOOL bHidden = IsDlgButtonChecked(IDC_VARFIELD_HIDDEN) == 1;
		CButton* pCBInput = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_INPUT);
		if (!bHidden)
		{
			pCBInput->SetCheck(0);

			CButton* pCBReinit = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT);
			pCBReinit->EnableWindow(FALSE);
			pCBReinit->SetCheck(0);

			CButton* pCBStatic = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC);
			pCBStatic->EnableWindow(FALSE);
			pCBStatic->SetCheck(0);

		}
		pCBInput->EnableWindow(bHidden);
	}
}

//----------------------------------------------------------------------------
void CVarFieldDlg::FieldSetInputChangedAction()
{
	if (GetDlgItem(IDC_RPSYMDLG_FLAG_INPUT)->IsWindowEnabled())
	{
		BOOL bInput = IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_INPUT) == 1;

		m_pRepField->SetFieldType (bInput ? WoormField::FIELD_INPUT : WoormField::FIELD_NORMAL);

		CButton* pCBReinit = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT);
		pCBReinit->EnableWindow(bInput);
		CButton* pCBStatic = (CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC);
		pCBStatic->EnableWindow(bInput);

		if (!bInput)
		{
			pCBReinit->SetCheck(0);
			pCBStatic->SetCheck(0);
		}
	}
}

//----------------------------------------------------------------------------
void CVarFieldDlg::FieldSetReinitChangedAction()
{
	if (GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT)->IsWindowEnabled())
	{	
		m_pRepField->SetReInit (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_REINIT == 1));

		((CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC))->SetCheck(0);
	}
}

//----------------------------------------------------------------------------
void CVarFieldDlg::FieldSetStaticChangedAction()
{
	if (GetDlgItem(IDC_RPSYMDLG_FLAG_STATIC)->IsWindowEnabled())
	{
		m_pRepField->SetStatic (IsDlgButtonChecked(IDC_RPSYMDLG_FLAG_STATIC == 1));
		((CButton*) GetDlgItem(IDC_RPSYMDLG_FLAG_REINIT))->SetCheck(0);
	}
}

//============================================================================
// CSelFieldDlg
//============================================================================
IMPLEMENT_DYNAMIC(CSelFieldDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CSelFieldDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CSelFieldDlg)
    ON_LBN_DBLCLK		(IDC_SELFIELD_LIST,	SelectField)
    
    ON_BN_CLICKED		(IDC_SELFIELD_ALL,	FieldTypeChangedAction)
    ON_BN_CLICKED		(IDC_SELFIELD_ASK,	FieldTypeChangedAction)
    ON_BN_CLICKED		(IDC_SELFIELD_FUNC,	FieldTypeChangedAction)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CSelFieldDlg::CSelFieldDlg
	(
		WoormTable*	pSymTable,
		CWordArray&		IdArray,
		BOOL			bOneField,
		DispTableInfo*	pDispTableInfo /* = NULL */
	)
	:
	CParsedDialog	(IDD_SELFIELD),
	m_pSymTable		(pSymTable),
	m_IdArray		(IdArray),
	m_bOnlyOneField	(bOneField),
    m_mlbFields		(IDB_CHECK),
	m_pDispTableInfo(pDispTableInfo)
{}

//----------------------------------------------------------------------------
BOOL CSelFieldDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_mlbFields.SubclassDlgItem	(IDC_SELFIELD_LIST,	this);
	m_mlbFields.SetStyle(CMultiListBox::SORT_ON_FIRST_STRING);
    
	ShowFieldList();
	GetDlgItem(IDOK)->EnableWindow(FALSE);
	CheckRadioButton(IDC_SELFIELD_ALL, IDC_SELFIELD_FUNC, IDC_SELFIELD_ALL);
	
	if (m_pDispTableInfo)
		m_pDispTableInfo->OnInitDialog(this);

	return TRUE;
}

//----------------------------------------------------------------------------
void CSelFieldDlg::FieldTypeChangedAction()
{
	ShowFieldList();
}

//----------------------------------------------------------------------------
void CSelFieldDlg::ShowFieldList()
{
	BOOL bAllHiddenField	= IsDlgButtonChecked(IDC_SELFIELD_ALL);
	BOOL bOnlyAskField		= IsDlgButtonChecked(IDC_SELFIELD_ASK);
	BOOL bOnlyFuncField		= IsDlgButtonChecked(IDC_SELFIELD_FUNC);

    m_mlbFields.SetRedraw(FALSE);
	m_mlbFields.ResetContent();	
	for (int i = 0; i < m_pSymTable->GetSize(); i++)
	{
		WoormField* pRepField = (WoormField*) m_pSymTable->GetAt(i);
		if (pRepField->IsPredefinedField()) 
			continue;
		if (pRepField->GetDataType() == DataType::Array) 
			continue;

		if (pRepField->IsHidden())
		{
			if (bOnlyAskField)
			{
			 	if (pRepField->IsInput())
					m_mlbFields.AddString(pRepField->GetName(), _T(""), CMultiListBox::UNCHECKED);
				continue;
			}
			
			if (bOnlyFuncField)
			{
				if (pRepField->HasAFunction())
					m_mlbFields.AddString(pRepField->GetName(), _T(""), CMultiListBox::UNCHECKED);
				continue;
			}
			
			m_mlbFields.AddString(pRepField->GetName(), _T(""), CMultiListBox::UNCHECKED);
		}
	}
	
    m_mlbFields.SetRedraw(TRUE);
    m_mlbFields.Invalidate(FALSE);
    m_mlbFields.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CSelFieldDlg::SelectField()
{
	int nCurrIdx = m_mlbFields.GetCurSel();

	if (nCurrIdx >= 0)
	{
		if (m_bOnlyOneField)
			m_mlbFields.SetAllFlags(CMultiListBox::UNCHECKED);	
		
		m_mlbFields.SetFlag(nCurrIdx, CMultiListBox::CHECK_ONE);
		GetDlgItem(IDOK)->EnableWindow(TRUE);
	}		
}

//----------------------------------------------------------------------------
void CSelFieldDlg::OnOK()
{
	if (m_pDispTableInfo && !m_pDispTableInfo->CheckControls())
		return;

	CString	strFieldName;
	int		nItems = m_mlbFields.GetCount();

	for (int i = 0; i < nItems; i++)
	{
		if (m_mlbFields.GetFlag(i))
		{
			strFieldName = m_mlbFields.GetString1(i);
			SymField* pRepField = m_pSymTable->GetField(strFieldName);
			if (pRepField)
			{
				m_IdArray.Add(pRepField->GetId());
				if (m_bOnlyOneField)
					break;
			}				
		}

	}

	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CSelFieldDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}
