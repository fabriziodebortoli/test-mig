#include "stdafx.h"

#include <TbGeneric\globals.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DataObj.h>

#include <TbParser\Lexan.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\funproto.h>

#include <TbOledb\sqlcatalog.h>	
#include "ActionsRepEngin.h"

#include "edtcmm.h"
#include "reptable.h"
#include "ruledata.h"
#include "eqnedit.h"
#include "generic.h"
#include "multilayout.h"

#include "INPUTMNG.HJSON" //JSON AUTOMATIC UPDATE
#include "eqnedit.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Nome della sezione, nel file .INI dell'applicazione, in cui viene salvata
// l'attuale configurazione grafica 
//-----------------------------------------------------------------------------
static const TCHAR szEquationEditorFull	[] = _T("WP_EquationEditorFull");
static const TCHAR szEquationEditor		[] = _T("WP_EquationEditor");

//----------------------------------------------------------------------------
#define EQNEDIT_SELECT_ALL_FIELDS			0
#define EQNEDIT_SELECT_ONLY_TBLRULE_FIELDS	1
#define EQNEDIT_SELECT_ONLY_EXPRULE_FIELDS	2
#define EQNEDIT_SELECT_ONLY_FUNC_FIELDS		3
#define EQNEDIT_SELECT_ONLY_ASK_FIELDS		4
		
const UINT BASED_CODE eqnedit_indicators[] =
{
	ID_EQNEDIT_FIELD_NAME,
	ID_EQNEDIT_MESSAGE
};

//============================================================================
//		CEqnItemNoLoc implementation
//			per localizzazione
//============================================================================
CEqnItemNoLoc::CEqnItemNoLoc(CString strName)
{
	m_strName = strName;
};

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CEqnEditDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CEqnEditDlg, CParsedDialog)
	//{{AFX_MSG_MAP( CEqnEditDlg )
	ON_CBN_SELCHANGE(IDC_EQNEDIT_SEL_FIELDS,		ShowVars)
	ON_CBN_SELCHANGE(IDC_EQNEDIT_SEL_TABLE,			ShowVars)
	ON_CBN_SELCHANGE(IDC_EQNEDIT_FILTER_FIELDS,		ShowVars)
	ON_CBN_EDITCHANGE(IDC_EQNEDIT_FILTER_FIELDS,	ShowVars)

	ON_CBN_SELCHANGE(IDC_EQNEDIT_TAG_NAME,		TagNameChanged)

	ON_LBN_SELCHANGE(IDC_EQNEDIT_FUNCTSLB,		SelChangeFunct)
	ON_LBN_SELCHANGE(IDC_EQNEDIT_EXT_FUNCTSLB,	SelChangeExtFunct)
	ON_LBN_SELCHANGE(IDC_EQNEDIT_PHISIC_FIELDS,	SelChangePhysicalFields)
    ON_LBN_SELCHANGE(IDC_EQNEDIT_VARSLB,       	VarFieldChanged)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_EQNEDIT_ITEM_NAME, EnumSetItemChanged)
    ON_LBN_SELCHANGE(IDC_EQNEDIT_LST_EVENTACTIONS,		SelChangeActionEvents)
	ON_LBN_SELCHANGE(IDC_EQNEDIT_FUNCTSLB,		SelChangeActionEvents)

	ON_LBN_DBLCLK	(IDC_EQNEDIT_FUNCTSLB,		SelFunct)
	ON_LBN_DBLCLK	(IDC_EQNEDIT_EXT_FUNCTSLB,	SelExtFunct)
	ON_LBN_DBLCLK	(IDC_EQNEDIT_VARSLB,		SelVar)
	ON_LBN_DBLCLK	(IDC_EQNEDIT_PHISIC_FIELDS,	SelPhysicalFields)
	ON_NOTIFY		(NM_DBLCLK, IDC_EQNEDIT_ITEM_NAME, SelEnumSetItem)
	ON_LBN_DBLCLK	(IDC_EQNEDIT_LST_EVENTACTIONS,	SelActionEvents)

	ON_BN_CLICKED	(IDC_CHECK_LONG,			OnClickedCheckLong)
	ON_BN_CLICKED	(IDC_EQNEDIT_ENUMSVIEWER,	OnRunEnumsViewer)

	ON_CBN_SELCHANGE	(IDC_EQNEDIT_EXT_COMBOAPP,	OnComboAppChanged)
	ON_CBN_SELCHANGE	(IDC_EQNEDIT_EXT_COMBOMOD,	OnComboModChanged)

	ON_WM_SIZE ()

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CEqnEditDlg::CEqnEditDlg
	(
		CWnd*			pParent, 
		SymTable*		pSymTable, 
		CString& 		strToEdit,
		DWORD			dwPosSel,
		RuleDataArray*		pRuleData,
		TblRuleData*	pTblRule,
		SqlTableInfoArray arSqlTableInfo
	) 
	:
	CParsedDialog	(pSymTable ? IDD_EQNEDIT : IDD_EQNEDIT_NO_PUB_NAMES, pParent),
	m_strExpr		(strToEdit),
	m_pSymTable		(pSymTable),
	m_dwStartPos	(dwPosSel),
	m_pTblRule		(pTblRule),
    m_arSqlTableInfo (arSqlTableInfo),
	m_lbPhisicFields(IDB_KEY),
	m_bOnlyPubNames (FALSE) 
{
	if (pRuleData)           
		for (int i = 0; i < pRuleData->GetSize(); i++)
		{
			RuleDataObj* pRule = (RuleDataObj*) (pRuleData->GetAt(i));
			if (pRule->IsARule() == RULE_DATA_TABLE)
				m_Rules.Add(pRule);
		}
}

//----------------------------------------------------------------------------
CEqnEditDlg::CEqnEditDlg
	(
		CWnd*			pParent, 
		SymTable*		pSymTable, 
		CString& 		strToEdit,
		DWORD			dwPosSel,
		RuleDataArray*		pRuleData,
		TblRuleData*	pTblRule,
		SqlTableInfo*	pSqlTableInfo
	) 
	:
	CParsedDialog	(pSymTable ? IDD_EQNEDIT : IDD_EQNEDIT_NO_PUB_NAMES, pParent),
	m_strExpr		(strToEdit),
	m_pSymTable		(pSymTable),
	m_dwStartPos	(dwPosSel),
	m_pTblRule		(pTblRule),
	m_lbPhisicFields(IDB_KEY),
	m_bOnlyPubNames (FALSE) 
{
	m_arSqlTableInfo.Add(pSqlTableInfo);
	if (pRuleData)           
		for (int i = 0; i < pRuleData->GetSize(); i++)
		{
			RuleDataObj* pRule = (RuleDataObj*) (pRuleData->GetAt(i));
			if (pRule->IsARule() == RULE_DATA_TABLE)
				m_Rules.Add(pRule);
		}
}

//------------------------------------------------------------------------------
void CEqnEditDlg::OnSize (UINT nType, int cx, int cy)
{
	CParsedDialog::OnSize (nType, cx, cy); 
	
	RecalcLayout();
}

//----------------------------------------------------------------------------
BOOL CEqnEditDlg::BelongsToDataTable(WoormField* pRfd)
{
	int nIdxRule = -1;
	int nIdxTableInfo = -1;
	int numRules = m_Rules.GetSize();
	
	if (numRules == 0)
		return FALSE;
		
	CString selectedTable;
	m_cbSelTable.GetLBText(m_cbSelTable.GetCurSel(), selectedTable);

	if (selectedTable.Find('[') != -1 && selectedTable.Find(']') != -1) 
		selectedTable = selectedTable.Mid(selectedTable.Find(']') + 1 );

	for (int i = 0; i < numRules; i++)
	{
		CStringArray arTableNames;
		((TblRuleData*)m_Rules[i])->GetTableNames(arTableNames);
		
		for (int j = 0; j < arTableNames.GetSize(); j++) 
			if (arTableNames.GetAt(j).CompareNoCase(selectedTable) == 0)
			{
				nIdxRule = i;
				nIdxTableInfo = j;
			}
	}
	
	if (nIdxRule == -1)
		return FALSE;

	TblRuleData* pTblRule = (TblRuleData*) m_Rules[nIdxRule];
	
	if (pTblRule == NULL)
	{
		ASSERT(FALSE);
		return FALSE;
	}
		
	if (pTblRule-> ExistPublicNameInTableInfo(pRfd->GetName(), nIdxTableInfo))
			return TRUE;

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CEqnEditDlg::FieldInCurTableRule(WoormField* pRfd)
{
	return m_pTblRule && m_pTblRule->ExistPublicName(pRfd->GetName());
}

//----------------------------------------------------------------------------
BOOL CEqnEditDlg::IsFieldApplicable(SymField* pF)
{
	if (m_pSymTable == NULL || !m_pSymTable->IsKindOf(RUNTIME_CLASS(WoormTable)))
		return TRUE;

	if (!pF->IsKindOf(RUNTIME_CLASS(WoormField)))
		return TRUE;
		
	WoormField* pRfd = dynamic_cast<WoormField*>(pF);

	int nFieldType		= m_cbSelFields.GetCurSel();
	int nIdxSelTable	= m_cbSelTable.GetCurSel();

	// if the field appartain to current table rule then it musn't be showed
	//
	if (FieldInCurTableRule(pRfd))
		return FALSE;

	switch (nFieldType)
	{
		case EQNEDIT_SELECT_ALL_FIELDS:
		{
			if (nIdxSelTable > 0)
				return BelongsToDataTable(pRfd);

			// se non si sta editando una where clause o un order by si inserisce
			// la variabile di stato predefinita
			if (m_pTblRule && pRfd->IsPredefinedField())
				return FALSE;
					
			return TRUE;
		}

		case EQNEDIT_SELECT_ONLY_TBLRULE_FIELDS:
			if (pRfd->IsTableRuleField())
			{
				if (nIdxSelTable > 0)
					return BelongsToDataTable(pRfd);				

				return TRUE;
			}
			break;

		case EQNEDIT_SELECT_ONLY_EXPRULE_FIELDS:
			return (pRfd->IsExprRuleField());
			
		case EQNEDIT_SELECT_ONLY_FUNC_FIELDS:
			return (pRfd->HasAFunction());
		
		case EQNEDIT_SELECT_ONLY_ASK_FIELDS:
			return (pRfd->IsInput());
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SetCaptionButtons()
{
	GetDlgItem(IDC_EQNEDIT_ISNULL)		->SetWindowText(_T("Is Null"));
	GetDlgItem(IDC_EQNEDIT_CLOSEG)		->SetWindowText(_T("}"));
	GetDlgItem(IDC_EQNEDIT_OPENG)		->SetWindowText(_T("{"));
	GetDlgItem(IDC_EQNEDIT_CLOSEROUND)	->SetWindowText(_T(" ) "));
	GetDlgItem(IDC_EQNEDIT_OPENROUND)	->SetWindowText(_T(" ( "));
	GetDlgItem(IDC_EQNEDIT_PLUS)		->SetWindowText(_T(" + "));
	GetDlgItem(IDC_EQNEDIT_MINUS)		->SetWindowText(_T(" - "));
	GetDlgItem(IDC_EQNEDIT_TIMES)		->SetWindowText(_T(" * "));
	GetDlgItem(IDC_EQNEDIT_SLASH)		->SetWindowText(_T(" / "));
	GetDlgItem(IDC_EQNEDIT_ASSIGN)		->SetWindowText(_T("="));
	GetDlgItem(IDC_EQNEDIT_THEN)		->SetWindowText(_T("Then"));
	GetDlgItem(IDC_EQNEDIT_ELSE)		->SetWindowText(_T("Else"));
	GetDlgItem(IDC_EQNEDIT_IF)			->SetWindowText(_T("If"));
	GetDlgItem(IDC_EQNEDIT_BETWEEN)		->SetWindowText(_T("Between "));
	GetDlgItem(IDC_EQNEDIT_LIKE)		->SetWindowText(_T("Like "));
	GetDlgItem(IDC_EQNEDIT_NOT)			->SetWindowText(_T(" Not "));
	GetDlgItem(IDC_EQNEDIT_OR)			->SetWindowText(_T(" Or "));
	GetDlgItem(IDC_EQNEDIT_AND)			->SetWindowText(_T(" And "));
	GetDlgItem(IDC_EQNEDIT_NE)			->SetWindowText(_T(" != "));
	GetDlgItem(IDC_EQNEDIT_GT)			->SetWindowText(_T(" > "));
	GetDlgItem(IDC_EQNEDIT_GE)			->SetWindowText(_T(" >= "));
	GetDlgItem(IDC_EQNEDIT_LE)			->SetWindowText(_T(" <= "));
	GetDlgItem(IDC_EQNEDIT_LT)			->SetWindowText(_T(" < "));
	GetDlgItem(IDC_EQNEDIT_EQ)			->SetWindowText(_T(" == "));
	GetDlgItem(IDC_EQNEDIT_AT)			->SetWindowText(_T(";"));
	GetDlgItem(IDC_EQNEDIT_DOT)			->SetWindowText(_T("."));
	GetDlgItem(IDC_EQNEDIT_COMMA)		->SetWindowText(_T(", "));
	GetDlgItem(IDC_EQNEDIT_PERC)		->SetWindowText(_T("%"));
	GetDlgItem(IDC_EQNEDIT_DQUOTE)		->SetWindowText(_T("\""));
	GetDlgItem(IDC_EQNEDIT_QUOTE)		->SetWindowText(_T("'"));
}

//----------------------------------------------------------------------------
BOOL CEqnEditDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	EnableToolTips();
	SetCaptionButtons();

	m_edtExpr.SubclassEdit(IDC_EQNEDIT_EDIT,	this);

	if (m_pSymTable)
	{
		m_cbSelFields.	SubclassDlgItem(IDC_EQNEDIT_SEL_FIELDS,	this);
		m_cbSelTable.	SubclassDlgItem(IDC_EQNEDIT_SEL_TABLE,	this);
		m_lbVars.		SubclassDlgItem(IDC_EQNEDIT_VARSLB,		this);
		m_lbExtFuncts.	SubclassDlgItem(IDC_EQNEDIT_EXT_FUNCTSLB,	this);
		m_cbSelApplication.	SubclassDlgItem	(IDC_EQNEDIT_EXT_COMBOAPP,	this);
		m_cbSelModule.		SubclassDlgItem	(IDC_EQNEDIT_EXT_COMBOMOD,	this);

		m_cbFilterSelFields.SubclassDlgItem	(IDC_EQNEDIT_FILTER_FIELDS,	this);

		if (!m_bOnlyPubNames)
			m_edtShowDescr.		SubclassEdit	(IDC_EQNEDIT_EXT_EDITDESCR,	this);
	}

	m_cbTagName.		SubclassEdit(IDC_EQNEDIT_TAG_NAME,		this);
	m_lbItemName.		SubclassDlgItem(IDC_EQNEDIT_ITEM_NAME,	this);
	m_lbItemName.InsertColumn(0, _TB("Name"), LVCFMT_LEFT, 130, 0);
	m_lbItemName.InsertColumn(1, _TB("Value"), LVCFMT_LEFT, 0, 0);
		
	VERIFY(m_ck_viewLong.SubclassDlgItem(IDC_CHECK_LONG, this));
	m_ck_viewLong.SetCheck(FALSE);

	m_lbFuncts.			SubclassDlgItem	(IDC_EQNEDIT_FUNCTSLB,		this);
	m_lbEventActions.	SubclassDlgItem	(IDC_EQNEDIT_LST_EVENTACTIONS,	this);
	m_lbPhisicFields.	SubclassDlgItem	(IDC_EQNEDIT_PHISIC_FIELDS,	this);

	m_lbPhisicFields.	SetTabStops(16);
	m_lbPhisicFields.	SetStyle(CMultiListBox::SORT_ON_FIRST_STRING);

	ShowPhysicalNames	();

	TypeChecked			();

	ShowComboBoxes		();

	CTBNamespace Ns;
	ShowBaseFunctions	();
	CEqnEditDlg::ShowFunctions		(m_cbSelApplication, m_cbSelModule, m_lbExtFuncts);

	ShowVars			();

	ShowEventActions	();

	m_edtExpr.SetTabStops(8);
	m_edtExpr.SetWindowText(m_strExpr);
	m_edtExpr.PositionCursor(m_dwStartPos);

	InitStatusBar	();	
	RecalcLayout	();

	// Controllo dell'esistenza di un precedente salvataggio della posizione
	// e dimensione della finestra principale
	WINDOWPLACEMENT wp;
	wp.length = sizeof wp;
	if (LoadWindowPlacement(m_nID == IDD_EQNEDIT ? szEquationEditorFull : szEquationEditor, wp))
	{
		// Lettura dal file .INI dell'applicazione 
		SetWindowPlacement	(&wp);
	}

	return FALSE;
}

//------------------------------------------------------------------------------
void CEqnEditDlg::InitStatusBar()
{
	UINT	nID, nStyle;
	int		cxWidth;
	
	if	(
			!m_sbInfo.Create(this) || 
			!m_sbInfo.SetIndicators(eqnedit_indicators, sizeof(eqnedit_indicators) / sizeof(UINT))
		)
	{
		TRACE("Failed to create status bar\n");
		return;
	}

	m_sbInfo.GetPaneInfo( 0, nID, nStyle, cxWidth);
	m_sbInfo.GetPaneInfo( 1, nID, nStyle, cxWidth);

	m_sbInfo.SetPaneInfo( 0, nID, SBPS_STRETCH | SBPS_NORMAL, cxWidth);
	m_sbInfo.SetPaneInfo( 1, nID, SBPS_NORMAL, cxWidth);
	
	m_sbInfo.SetPaneText(0, NULL);
	m_sbInfo.SetPaneText(1, NULL);
}

//----------------------------------------------------------------------------
void CEqnEditDlg::RecalcLayout()
{
	RepositionBars(0, 0xFFFF, AFX_IDW_PANE_FIRST);
}

//----------------------------------------------------------------------------
void CEqnEditDlg::ShowVars()
{
	if (m_pSymTable == NULL)
	{
		if (::IsWindow(m_cbFilterSelFields.m_hWnd))
			m_cbFilterSelFields.ShowWindow(SW_HIDE);
		if (::IsWindow(m_lbVars.m_hWnd))
			m_lbVars.EnableWindow(FALSE);
		return;
	}
		
	int nFieldType	= m_cbSelFields.GetCurSel();
	if (nFieldType == EQNEDIT_SELECT_ONLY_TBLRULE_FIELDS && m_Rules.GetSize() > 0)
	{
		m_cbSelTable.ShowWindow(SW_SHOW);
		GetDlgItem(IDC_EQNEDIT_TBL_CAPTION)->ShowWindow(SW_SHOW);
	}
	else
	{
		m_cbSelTable.SetCurSel(0);
		GetDlgItem(IDC_EQNEDIT_TBL_CAPTION)->ShowWindow(SW_HIDE);
		m_cbSelTable.ShowWindow(SW_HIDE);
	}		
	
	if (m_sbInfo.m_hWnd)
	{
		m_sbInfo.SetPaneText(0, L"");
		m_sbInfo.SetPaneText(1, L"");
	}
	m_lbVars.SetRedraw(FALSE);
	m_lbVars.ResetContent();
		
	CString sMatch;
	BOOL bFirst = m_cbFilterSelFields.GetCount() == 0;
	if (bFirst)
	{
		m_cbFilterSelFields.AddString(L"");
	}
	else
	{
		int idx = m_cbFilterSelFields.GetCurSel();
		if (idx > -1)
			m_cbFilterSelFields.GetLBText(idx, sMatch);
		else
			m_cbFilterSelFields.GetWindowText(sMatch);
	}

	for (int i = 0; i <= m_pSymTable->GetUpperBound(); i ++)
	{
		SymField* pF = m_pSymTable->GetAt(i);

		if (IsFieldApplicable(pF))
		{
			CString s = pF->GetName();
			if (bFirst)
			{
				int i = s.ReverseFind('.');
				if (i > 0)
				{
					CString pref = s.Left(i+1);
					if (m_cbFilterSelFields.SelectString(-1, pref) < 0)
						m_cbFilterSelFields.AddString(pref);
				}
			}

			if (sMatch.IsEmpty() || ::WildcardMatch(s, sMatch + '*'))
				m_lbVars.AddString(s);
		}
	}
	if (bFirst)
		m_cbFilterSelFields.SetCurSel(0);

	m_lbVars.CalcHorizontalExtent();
	m_lbVars.SetRedraw(TRUE);
	m_lbVars.Invalidate(FALSE);

	if (sMatch.IsEmpty())
		m_edtExpr.SetFocus();
}

//----------------------------------------------------------------------------
void CEqnEditDlg::ShowBaseFunctions()
{
	m_lbFuncts.SetRedraw(FALSE);
	m_lbFuncts.ResetContent();

	for (Token tk = T_FUNCTION_START; tk <= T_FUNCTION_PUBLIC_END; tk = (Token)(int(tk)+1))
	{
		//if (
		//		tk == T_FCONVERT || tk == T_FTYPEOF || tk == T_FADDRESSOF || 
		//		tk == T_FEXECUTESCRIPT || tk == T_FGETTITLE ||
		//		tk == T_FCOLUMN_GETAT || tk == T_FCOLUMN_SIZE || tk == T_FCOLUMN_SUM || tk == T_FCOLUMN_FIND ||
		//		tk == T_FRECORD_GETFIELD || tk == T_FOBJECT_GETFIELD
		//	)
		//	continue;

		m_lbFuncts.AddString( cwsprintf(tk) );
	}

	m_lbFuncts.CalcHorizontalExtent();
	m_lbFuncts.SetRedraw(TRUE);
	m_lbFuncts.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
/*static */void CEqnEditDlg::ShowFunctions(CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CResizableListBox& lbExtFuncts)
{
	if (!cbSelApplication.m_hWnd)
		return;

	CString sApp = _T("");
	CString sMod = _T("");
	GetAppModSel (cbSelApplication, cbSelModule, sApp, sMod);

	lbExtFuncts.SetRedraw(FALSE);
	lbExtFuncts.ResetContent();

	CObArray arFunctions;
	// itero su tutte le AddOnApplication per scoprire si è registrato all'evento
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOnApp);

		if (sApp.IsEmpty())
		{
			for (int j = 0; j <= pAddOnApp->m_pAddOnModules->GetUpperBound(); j++) 
			{
				AddOnModule* pAddOnModApp = pAddOnApp->m_pAddOnModules->GetAt(j);
				FillExtFuncList(lbExtFuncts, pAddOnModApp);
			}
		}
		else
		{
			for (int j = 0; j <= pAddOnApp->m_pAddOnModules->GetUpperBound(); j++) 
			{
				AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(j);
				if (pAddOnApp->m_strAddOnAppName.CompareNoCase(sApp) == 0)
				{
					if (sMod.IsEmpty())
						FillExtFuncList(lbExtFuncts, pAddOnMod);
					else
					{
						if (pAddOnMod->GetModuleName().CompareNoCase(sMod) == 0)
							FillExtFuncList(lbExtFuncts, pAddOnMod);
					}
				}				
			}
		}
	}

	lbExtFuncts.CalcHorizontalExtent();
	lbExtFuncts.SetRedraw(TRUE);
	lbExtFuncts.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
/*static */void CEqnEditDlg::FillExtFuncList(CResizableListBox& lbExtFuncts, AddOnModule* pAddOnMod)
{
	ASSERT(pAddOnMod);

	const CBaseDescriptionArray &arFunctions = pAddOnMod->m_XmlDescription.GetFunctionsInfo().GetFunctions();
	for (int k = 0; k <= arFunctions.GetUpperBound(); k++)
	{
		CFunctionDescription* pFun = (CFunctionDescription*) arFunctions.GetAt(k);
		if (!pFun->IsPublished())
			continue;
		if (AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
		{
			int nIdx = lbExtFuncts.AddString(pFun->GetName());
			lbExtFuncts.SetItemDataPtr(nIdx, pFun);
		}
	}
}

//----------------------------------------------------------------------------
/*static */BOOL CEqnEditDlg::ExistExtFuncList(AddOnModule* pAddOnMod)
{
	ASSERT(pAddOnMod);

	const CBaseDescriptionArray &arFunctions = pAddOnMod->m_XmlDescription.GetFunctionsInfo().GetFunctions();
	for (int k = 0; k <= arFunctions.GetUpperBound(); k++)
	{
		CFunctionDescription* pFun = (CFunctionDescription*) arFunctions.GetAt(k);
		if (!pFun->IsPublished())
			continue;
		if (AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName()))
		{
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CEqnEditDlg::ShowPhysicalNames()
{
	CString tablesName;
	if (m_arSqlTableInfo.GetSize() == 0)
	{
		m_lbPhisicFields.ResetContent();
		m_lbPhisicFields.EnableWindow(FALSE);
		return;
	}

	m_lbPhisicFields.SetRedraw(FALSE);	
	for (int i = 0; i < m_arSqlTableInfo.GetSize(); i++)
	{   
		// non deve far vedere i campi virtuali definiti dal programmatore
		// ed inseriti nel TableInfo dalla prima istanza di record derivato 
		// che definisce eventuali campi virtuali
		const SqlTableInfo* pTableInfo = m_arSqlTableInfo.GetAt(i);
		const Array* pColums = pTableInfo->GetPhysicalColumns();
		for (int j = 0; j < pColums->GetSize(); j++)
		{ 
			SqlColumnInfo* pSqlColumnInfo = (SqlColumnInfo*) pColums->GetAt(j);
			ASSERT (pSqlColumnInfo);
			ASSERT(!pSqlColumnInfo->m_bVirtual);

			int nIdx = m_lbPhisicFields.AddString
				(
					pSqlColumnInfo->GetColumnName(), 
					_T(""),
					pSqlColumnInfo->m_bSpecial
						? CMultiListBox::CHECK_ONE
						: CMultiListBox::UNCHECKED
				);
			m_lbPhisicFields.SetItemDataPtr(nIdx, pSqlColumnInfo);
		}

		//building string with involved table name 
		tablesName += " [";
		tablesName += m_arSqlTableInfo.GetAt(i)->GetTableName();
		tablesName += "] ";
	}
	m_lbPhisicFields.SetRedraw(TRUE);	
	m_lbPhisicFields.Invalidate(FALSE);	
	SetWindowText (cwsprintf(_TB("Query generator on tables {0-%s}"), (LPCTSTR) tablesName));
}

//----------------------------------------------------------------------------
void CEqnEditDlg::ShowComboBoxes()
{
	if (m_pSymTable == NULL)
		return;
		
	//	Fill Selection fields comboBox
	//
	m_cbSelFields.AddString(_TB("All fields"));
	m_cbSelFields.AddString(_TB("Only fields from archive"));
	m_cbSelFields.AddString(_TB("Only expression fields"));
	m_cbSelFields.AddString(_TB("Only function fields"));
	m_cbSelFields.AddString(_TB("Only request fields"));
	m_cbSelFields.SetCurSel(0);
	
	//	Fill DataTable names comboBox
	//
	m_cbSelTable.AddString(_TB("All archives"));
	for (int i = 0; i < m_Rules.GetSize(); i++)
	{
		TblRuleData* pTRD = (TblRuleData*) m_Rules[i];
		if (pTRD == m_pTblRule) continue;
		
		CStringArray tableNames;
		pTRD->GetTableNames(tableNames);
		for (int j = 0; j < tableNames.GetSize(); j++)
		{
			m_cbSelTable.AddString(cwsprintf(_T("[%d]%s"),i, tableNames.GetAt(j)));
		}
	}
	m_cbSelTable.SetCurSel(0);

	//Fill selection Application
	if (m_pSymTable)
	{
		FillAppCombo(m_cbSelApplication, m_cbSelModule, m_arAppItemLoc);
	}
}

//----------------------------------------------------------------------------
/*static */void CEqnEditDlg::FillAppCombo (CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, Array& arAppItemLoc)
{
	if (!cbSelApplication.m_hWnd)
		return;

	cbSelApplication.ResetContent();
	cbSelApplication.AddString(_TB("<All applications>"));

	CStringArray	aApplications;
	CString			strApps;
	int				nIdx;
	CEqnItemNoLoc*	pAppItemNoLoc	= NULL;

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pApp = AfxGetAddOnAppsTable()->GetAt(i);
		strApps = pApp->m_strAddOnAppName;

		nIdx			= cbSelApplication.AddString(AfxGetAddOnApp(strApps)->GetTitle());
		pAppItemNoLoc	= new CEqnItemNoLoc(strApps);
		arAppItemLoc.Add(pAppItemNoLoc);
		cbSelApplication.SetItemData(nIdx, (DWORD) pAppItemNoLoc);
	}

	cbSelApplication.SetCurSel(0);

	cbSelModule.AddString(_TB("<All modules>"));
	cbSelModule.SetCurSel(0);
	cbSelModule.EnableWindow(FALSE);
}

//----------------------------------------------------------------------------
void CEqnEditDlg::ShowEventActions()
{
	if (m_lbEventActions.m_hWnd == 0)
		return;
	CStringArray arKeywords;
	AddEventActionsKeywords(arKeywords);
	for (int i =0; i < arKeywords.GetCount(); i++)
		m_lbEventActions.AddString(arKeywords[i]);
//	
}

//--------------------------------------------------------------------------
/*static */void CEqnEditDlg::FillModuleCombo(CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CResizableListBox& lbExtFuncts, Array& arModItemLoc, const CString& strLabel/*= _TB("")*/)
{
	CStringArray	aModules;
	CTBNamespace	Ns;
	CEqnItemNoLoc*	pModItemNoLoc	= NULL;
	CString			strMods;
	int				nIdx;

	cbSelModule.ResetContent();
	cbSelModule.AddString(_TB("<All modules>"));
	arModItemLoc.RemoveAll();
	
	AddOnApplication* pAddOnApplication =  AfxGetAddOnApp(strLabel);
	if (pAddOnApplication)
		for (int a = 0; a <= pAddOnApplication->m_pAddOnModules->GetUpperBound(); a++)
		{
			AddOnModule* pAddOnMod = pAddOnApplication->m_pAddOnModules->GetAt(a);
			if (!CEqnEditDlg::ExistExtFuncList(pAddOnMod))
				continue;

			nIdx = cbSelModule.AddString(pAddOnMod->GetModuleTitle());
			pModItemNoLoc = new CEqnItemNoLoc(pAddOnMod->GetModuleName());
			arModItemLoc.Add(pModItemNoLoc);
			cbSelModule.SetItemData(nIdx, (DWORD) pModItemNoLoc);	
		}	
	cbSelModule.SetCurSel(0);

	CEqnEditDlg::ShowFunctions (cbSelApplication, cbSelModule, lbExtFuncts);
}

//----------------------------------------------------------------------------
/*static */void CEqnEditDlg::GetAppModSel (CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CString& sApplication, CString& sModule)
{
	CEqnItemNoLoc* pItemNoLoc = NULL;

	int nCurSel = cbSelApplication.GetCurSel();
	if (nCurSel >= 0)
	{
		pItemNoLoc = (CEqnItemNoLoc*) cbSelApplication.GetItemData(nCurSel);
		if (pItemNoLoc) sApplication = pItemNoLoc->m_strName;
	}
	else
		return;

	nCurSel = cbSelModule.GetCurSel();
	if (nCurSel >= 0)
	{
		pItemNoLoc = (CEqnItemNoLoc*) cbSelModule.GetItemData(nCurSel);
		if (pItemNoLoc) sModule = pItemNoLoc->m_strName;
	}
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelChangeActionEvents()
{
	m_sbInfo.SetPaneText(0, _T(""));
	m_sbInfo.SetPaneText(1, _T(""));
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelActionEvents()
{
	CString cs;	
	m_lbEventActions.GetText( m_lbEventActions.GetCurSel(), cs );
	if (!cs.IsEmpty())
	{
		::CEditReplaceSelection(m_edtExpr, cs);
	}		
}

//----------------------------------------------------------------------------
void CEqnEditDlg::TypeChecked()
{
	m_cbTagName.GetDataTypes().RemoveAll();

	m_cbTagName.GetDataTypes().Add(DATA_ENUM_TYPE);

	m_cbTagName.SetTypeValue(DATA_NULL_TYPE);

	m_cbTagName.FillListBox();

	if (m_cbTagName.GetCount() > 0)
	{
		// visualizzo il primo elemento
		m_cbTagName.SetCurSel(0);
		m_cbTagName.SetModifyFlag(TRUE);
		m_cbTagName.UpdateCtrlData(TRUE);
	}

	TagNameChanged();
}

//----------------------------------------------------------------------------
void CEqnEditDlg::TagNameChanged()
{
	m_lbItemName.DeleteAllItems();
	if (m_cbTagName.GetCurSel() < 0 || m_cbTagName.GetTypeValue().m_wType == DATA_NULL_TYPE)
		return;
	
	WORD wTagValue = m_cbTagName.GetTypeValue().m_wTag;
	const EnumItemArray* pEnumItemArray = AfxGetEnumsTable()->GetEnumItems(wTagValue);

	if (pEnumItemArray == NULL)
		return;

	for (int i = 0; i <= pEnumItemArray->GetUpperBound(); i++)
	{
		EnumItem* pItem = pEnumItemArray->GetAt(i);
		//int j = m_lbItemName.AddString(pItem->GetTitle()/*cwsprintf(_T("{0-%05d} ({1-%08ld})"), pItem->GetItemValue(), GET_TI_VALUE(pItem->GetItemValue(), wTagValue))*/);
		int j = m_lbItemName.InsertItem(i, (LPCTSTR)pItem->GetTitle());
		m_lbItemName.SetItemText(i, 1, cwsprintf(_T("{0-%05d} ({1-%08ld})"), pItem->GetItemValue(), GET_TI_VALUE(pItem->GetItemValue(), wTagValue)));
		m_lbItemName.SetItemData(j, GET_TI_VALUE(pItem->GetItemValue(), wTagValue));
	}
}

//----------------------------------------------------------------------------
void CEqnEditDlg::EnumSetItemChanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (m_cbTagName.GetCurSel() < 0 || /*m_lbItemName.GetSelCount() < 0*/m_lbItemName.GetSelectedCount() < 1)
		return;

	//	non si può avere la multiselezione per gli enumerativi e visto che i Set
	//	saranno eliminati, è stato impostato che il CListCtrl sia senza multisel
	CString strItem;

	POSITION	Pos		= m_lbItemName.GetFirstSelectedItemPosition();
	int			nItem	= 0;
	
	while (Pos)
		nItem = m_lbItemName.GetNextSelectedItem(Pos);

	strItem = m_lbItemName.GetItemText(nItem, 0);
	m_sbInfo.SetPaneText(0, strItem);
	m_sbInfo.SetPaneText(1, _T(""));
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelEnumSetItem(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (m_cbTagName.GetCurSel() < 0 || m_lbItemName.GetSelectedCount() < 1/*m_lbItemName.GetSelCount() <= 0*/)
		return;
	
	DataObj*	pDataObj = NULL;
	int			nSel = m_lbItemName.GetSelectedCount();
	int*		idxs = new int[nSel];
	
	POSITION	Pos	  = m_lbItemName.GetFirstSelectedItemPosition();
	int			nItem = 0;
	
	while (Pos)
		nItem	= m_lbItemName.GetNextSelectedItem(Pos);

	pDataObj = new DataEnum(m_lbItemName.GetItemData(nItem));

	delete []idxs;

	// compone la stringa
	::CEditReplaceSel(m_edtExpr, ExpUnparse::UnparseData(*pDataObj));
	
	delete pDataObj;
}

//----------------------------------------------------------------------------
BOOL CEqnEditDlg::SelDataType(DataType dt)
{
	for (int i = 0; i < m_cbTagName.GetCount(); i++)
	{
		DataObj* pObj = m_cbTagName.GetDataObjFromIdx(i);
		if (!pObj) continue;
		if (!pObj->IsKindOf(RUNTIME_CLASS(DataLng))) continue;
		long w = (long) *((DataLng*)pObj);
		if (HIWORD(w) == dt.m_wTag)
		{
			m_cbTagName.SetCurSel(i);
			m_cbTagName.SetModifyFlag(TRUE);
			m_cbTagName.UpdateCtrlData(TRUE);

			TagNameChanged();

			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelFunct()
{
	CString cs;
	int 	nStart, nStop, nLenFunc;
	
	m_lbFuncts.GetText( m_lbFuncts.GetCurSel(), cs );
	if (!cs.IsEmpty())
	{
		::CEditGetSel(m_edtExpr,nStart,nStop);
		nLenFunc = cs.GetLength();
		
		cs += "( ) ";
		::CEditReplaceSel(m_edtExpr, cs);
		m_edtExpr.SetSel(nStart+nLenFunc+1, nStart+nLenFunc+1);
	}		
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelExtFunct()
{
	int 	nStart, nStop, nLenFunc;
	
	CFunctionDescription* pFunc = (CFunctionDescription*) m_lbExtFuncts.GetItemDataPtr(m_lbExtFuncts.GetCurSel());
		
	if (pFunc)
	{
		CString cs = pFunc->GetNamespace().ToUnparsedString();

		::CEditGetSel(m_edtExpr,nStart,nStop);
		nLenFunc = cs.GetLength();
		
		cs += "( ) ";
		::CEditReplaceSel(m_edtExpr, cs);
		m_edtExpr.SetSel(nStart+nLenFunc+1, nStart+nLenFunc+1);
	}		
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelVar()
{
	if (m_pSymTable == NULL)
		return;
	CString cs;
	if (m_lbVars.GetCurSel() >= 0 )
	{
		m_lbVars.GetText(m_lbVars.GetCurSel(), cs);
		cs += " ";
		::CEditReplaceSel(m_edtExpr, cs);
	}
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelPhysicalFields()
{
	CString cs;
	int		nIdx;

	cs += GetTableName();
	cs += ".";
	
	nIdx = m_lbPhisicFields.GetCurSel();
	if (nIdx >= 0)
	{
		cs += m_lbPhisicFields.GetString1(nIdx);
		cs += " ";
		::CEditReplaceSel(m_edtExpr, cs);
	}
}

//----------------------------------------------------------------------------
void CEqnEditDlg::VarFieldChanged()
{
	if (m_pSymTable == NULL)
		return;
		
	CString			strFieldName;
    CString    		strType;
	int				nCurIdx = m_lbVars.GetCurSel();

    if (nCurIdx >= 0)
	{
		m_lbVars.GetText(nCurIdx, strFieldName);//controlla che non sia -1  

		SymField* pFld = m_pSymTable->GetField(strFieldName);

		if (pFld)
		{	
			strType = FromDataTypeToDescr(pFld->GetDataType());

			if (pFld->GetDataType() == DATA_STR_TYPE)
			{
        		strType += cwsprintf(_T("(%d)"), pFld->GetLen());
			}
			else  if (pFld->GetDataType() == DATA_ARRAY_TYPE)
			{
				DataArray* par = dynamic_cast<DataArray*>(pFld->GetData());
				DataType dt = par->GetBaseDataType();
        		strType += cwsprintf(_T("[%s]"), FromDataTypeToDescr(dt));
				if (dt == DATA_ENUM_TYPE)
					SelDataType(dt);
			}
			else if (pFld->GetDataType() == DATA_ENUM_TYPE)
			{
				SelDataType(pFld->GetDataType());
			}
			else if (pFld->GetDataType() == DataType::Object)
			{
				strType += L"{object}";
			}
			else  if (pFld->GetDataType() == DATA_RECORD_TYPE)
			{
				DataTRecord* par = dynamic_cast<DataTRecord*>(pFld->GetData());
				//TODO
				ASSERT(FALSE);
			}
			else 

			if (pFld->GetProvider())
				strType += L"[*]";

			CString sTitle = pFld->GetTitle();
			if (!sTitle.IsEmpty())
				strFieldName += L" - " + sTitle;
		}                	
	                
		m_sbInfo.SetPaneText(0, strFieldName);
		m_sbInfo.SetPaneText(1, strType);
	}
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelChangeExtFunct()
{
	m_edtShowDescr.SetWindowText(CEqnEditDlg::SelChangeExtFunct(m_lbExtFuncts));
}	

//----------------------------------------------------------------------------
/*static */CString CEqnEditDlg::SelChangeExtFunct(CResizableListBox& lbExtFuncts)
{
	int	nIdx = lbExtFuncts.GetCurSel();
	if (nIdx >= 0)
	{
		CString strHelp;
		CFunctionDescription* pFunc = (CFunctionDescription*) lbExtFuncts.GetItemDataPtr(nIdx);

		if(pFunc)
		{	
			BOOL bIsRunReport = pFunc->GetName().CompareNoCase(_T("RunReport")) == 0;
			strHelp = pFunc->GetHelpSignature();
			
			if (bIsRunReport)
			{
				strHelp += _T("\r\n");
				strHelp += _TB("RunReport function supports optional parameters: it allows parametrization of called report");
				strHelp += _T("\r\n");
				strHelp += _TB("Optional parameters must be pairs: the first is the field name of called report to valorize with the value of the second parameter");
			}
			
			return (strHelp);
		}
	}
	return _T("");
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelChangeFunct()
{
	//TODO
}

//----------------------------------------------------------------------------
void CEqnEditDlg::SelChangePhysicalFields()
{
	int nIdx = m_lbPhisicFields.GetCurSel();
	if (nIdx >= 0)
	{
		CString cs = AfxLoadDatabaseString(GetTableName(),GetTableName()/*m_strTableName, m_strTableName*/);
		cs += ".";
		cs += AfxLoadDatabaseString(m_lbPhisicFields.GetString1(nIdx), GetTableName()/*m_strTableName*/);

		CString strBuffer;
		SqlColumnInfo* pCol = (SqlColumnInfo*) m_lbPhisicFields.GetItemDataPtr(nIdx);
		if(pCol)
		{
			//ASSERT_KINDOF(SqlColumnInfo, pCol);
			strBuffer = FromDataTypeToDescr(pCol->GetDataObjType());
			if (pCol->GetDataObjType() == DATA_STR_TYPE)
        		strBuffer += cwsprintf(_T("(%d)"), pCol->GetColumnLength());
			else if (pCol->GetDataObjType() == DATA_ENUM_TYPE)
			{
				SelDataType(pCol->GetDataObjType());
			}
		}

		m_sbInfo.SetPaneText(0, cs);
		m_sbInfo.SetPaneText(1, strBuffer);
	}
}

//----------------------------------------------------------------------------
BOOL CEqnEditDlg::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nCtlId, nCode, hCtl);

	if (nCtlId == IDC_EQNEDIT_ASSIGN ||
		nCtlId == IDC_EQNEDIT_IF ||
		nCtlId == IDC_EQNEDIT_THEN ||
		nCtlId == IDC_EQNEDIT_ELSE ||
		nCtlId == IDC_EQNEDIT_ISNULL ||
		nCtlId == IDC_EQNEDIT_OPENG ||
		nCtlId == IDC_EQNEDIT_CLOSEG ||
		nCtlId == IDC_EQNEDIT_PLUS ||
		nCtlId == IDC_EQNEDIT_MINUS ||
		nCtlId == IDC_EQNEDIT_TIMES ||
		nCtlId == IDC_EQNEDIT_SLASH ||
		nCtlId == IDC_EQNEDIT_OPENROUND ||
		nCtlId == IDC_EQNEDIT_CLOSEROUND ||
		nCtlId == IDC_EQNEDIT_QUOTE ||
		nCtlId == IDC_EQNEDIT_DQUOTE ||
		nCtlId == IDC_EQNEDIT_PERC ||
		nCtlId == IDC_EQNEDIT_DOT ||
		nCtlId == IDC_EQNEDIT_COMMA ||
		nCtlId == IDC_EQNEDIT_AT ||
		nCtlId == IDC_EQNEDIT_EQ ||
		nCtlId == IDC_EQNEDIT_NE ||
		nCtlId == IDC_EQNEDIT_GE ||
		nCtlId == IDC_EQNEDIT_LE ||
		nCtlId == IDC_EQNEDIT_GT ||
		nCtlId == IDC_EQNEDIT_LT ||
		nCtlId == IDC_EQNEDIT_AND ||
		nCtlId == IDC_EQNEDIT_OR ||
		nCtlId == IDC_EQNEDIT_NOT ||
		nCtlId == IDC_EQNEDIT_LIKE ||
		nCtlId == IDC_EQNEDIT_BETWEEN)
	{
		CString	cs;

		FromHandle(hCtl)->GetWindowText(cs);
		::CEditReplaceSel(m_edtExpr, cs);
		return TRUE;
	}

	return CParsedDialog::OnCommand(wParam, lParam);
}

//----------------------------------------------------------------------------
void CEqnEditDlg::OnOK()
{
	m_edtExpr.GetWindowText(m_strExpr);

	WINDOWPLACEMENT wp;
	wp.length = sizeof wp;
	if (GetWindowPlacement(&wp))
	{
		WriteWindowPlacement(m_nID == IDD_EQNEDIT ? szEquationEditorFull : szEquationEditor, wp);
	}

	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CEqnEditDlg::OnClickedCheckLong()
{
	int nCheck = m_ck_viewLong.GetCheck();
	if (nCheck)
	{
		m_lbItemName.SetColumnWidth(0, 85);
		m_lbItemName.SetColumnWidth(1, 130);	
	}
	else
	{
		m_lbItemName.SetColumnWidth(0, 130);
		m_lbItemName.SetColumnWidth(1, 0);
	}
}

//----------------------------------------------------------------------------
void CEqnEditDlg::OnRunEnumsViewer ()
{
	AfxRunEnumsViewer ();
}

//----------------------------------------------------------------------------
void CEqnEditDlg::OnComboAppChanged()
{
	m_edtShowDescr.SetWindowText(_T(""));
	CEqnEditDlg::ShowModules(m_cbSelApplication, m_cbSelModule, m_lbExtFuncts, m_arModItemLoc);
}

//----------------------------------------------------------------------------
/*static */void CEqnEditDlg::ShowModules (CResizableComboBox& cbSelApplication, CResizableComboBox& cbSelModule, CResizableListBox& lbExtFuncts, Array& arModItemLoc)
{
	BOOL			bFind			= FALSE;
	int				nCurSel			= cbSelApplication.GetCurSel();
	
	if (nCurSel >= 0)
		cbSelModule.EnableWindow();
	else
	{
		cbSelModule.EnableWindow(FALSE);		
		cbSelModule.SetCurSel(0);

		CEqnEditDlg::ShowFunctions(cbSelApplication, cbSelModule, lbExtFuncts);
		return;
	}
	
	cbSelModule.SetCurSel(0);

	CEqnItemNoLoc* pItemNoLoc = (CEqnItemNoLoc*) cbSelApplication.GetItemData(nCurSel);
	cbSelModule.ResetContent();

	CString sLabel = pItemNoLoc ? pItemNoLoc->m_strName : _T("");

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		CString strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
		if (strApps == sLabel)
			bFind = TRUE; 
	}

	CEqnEditDlg::FillModuleCombo(cbSelApplication, cbSelModule, lbExtFuncts, arModItemLoc, sLabel);
}

//----------------------------------------------------------------------------
void CEqnEditDlg::OnComboModChanged()
{
	m_edtShowDescr.SetWindowText(_T(""));

	CEqnEditDlg::ShowFunctions(m_cbSelApplication, m_cbSelModule, m_lbExtFuncts);
}

//----------------------------------------------------------------------------
INT_PTR CEqnEditDlg::OnToolHitTest(CPoint point, TOOLINFO* pTI) const
{
	CString strTableName;
	strTableName = GetTableName();
	if (::OnToolHitTest
			(
				this,
				m_lbPhisicFields, 
				point, 
				pTI, 
				strTableName
			)
		)
	{
		return IDC_EQNEDIT_PHISIC_FIELDS;
	}

	return __super::OnToolHitTest(point, pTI);
}

//----------------------------------------------------------------------------
CString CEqnEditDlg::GetTableName() const
{
	int nIdx = m_lbPhisicFields.GetCurSel();
    if (nIdx >= 0)
	{
	 SqlColumnInfo* pCol = (SqlColumnInfo*) m_lbPhisicFields.GetItemDataPtr(nIdx);
	 return pCol->GetTableName();
	}
	if (m_arSqlTableInfo.GetSize() != 0)
	    return  (m_arSqlTableInfo.GetAt(0))->GetTableName();
	return CString("");
}

