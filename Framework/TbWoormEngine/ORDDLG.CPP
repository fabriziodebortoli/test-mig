
#include "stdafx.h"

#include <stdio.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbParser\Parser.h>

#include <TbGenlib\FunProto.h>
#include <TbGenlib\expr.h>
#include <TbGenlib\generic.h>
#include <TBGenlib\baseapp.h>

#include "ActionsRepEngin.h"
#include "rpsymtbl.h"
#include "qrydata.h"
#include "orddlg.h"

//............................... Resources
#include "edtcmm.hjson" //JSON AUTOMATIC UPDATE
#include "orddlg.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
// class CGroupByDlg
//============================================================================

IMPLEMENT_DYNAMIC(CGroupByDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CGroupByDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CGroupByDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CGroupByDlg::CGroupByDlg (QueryData* pQueryData, WoormTable* pSymTable)
	:
	CParsedDialog	(IDD_ORDERBY),
    m_pQueryData	(pQueryData),
	m_pSymTable		(pSymTable),
    m_edtOrderBy	(pSymTable, pQueryData->GetRuleData()),
	m_edtGroupBy	(pSymTable, pQueryData->GetRuleData()),
	m_edtFilter		(pSymTable, pQueryData->GetRuleData()),
	m_edtActions	(pSymTable, pQueryData->GetRuleData())
{
	m_pGroupBy	= m_pQueryData->m_pGroupBy;
}

//----------------------------------------------------------------------------
CGroupByDlg::~CGroupByDlg()
{}

//----------------------------------------------------------------------------
BOOL CGroupByDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	if	(
//@@TODO			!m_edtOrderBy.	SubclassEdit	(IDC_ORDERBY,		this)	||
//@@TODO			!m_btnDescending.SubclassDlgItem(IDC_DESCENDING,	this)	||
			!m_edtGroupBy.	SubclassEdit	(IDC_GROUPBY,		this)	||
			!m_edtActions.	SubclassEdit	(IDC_GROUPBY_ACTION,this)	||
			!m_edtFilter.	SubclassEdit	(IDC_FILTER,		this)
		)
	{
		EndDialog(IDCANCEL);
		return FALSE;	
	}

	if (m_pGroupBy->m_pHavingTupleFilter == NULL)
		m_pGroupBy->m_pHavingTupleFilter = new Expression(&m_pQueryData->m_pGroupBy->m_SymTable);

	m_edtGroupBy.SetWindowText(m_pQueryData->m_pGroupBy->m_pGroupingTuple->ToString());
	m_edtFilter.SetWindowText(m_pQueryData->m_pGroupBy->m_pHavingTupleFilter->ToString());

	ShowGroupByActions	();

	return TRUE;
}

//----------------------------------------------------------------------------
void CGroupByDlg::ShowGroupByActions()
{
	CLineFile	iFile;
	CString		strTmp;
    CString		strFileName = GetTempName();
	TCHAR		buffer[256];

	Unparser oFile(strFileName);

	int nNumItem = m_pGroupBy->m_ActionsArray.GetSize();
	for (int i = 0; i < nNumItem; i++)
	{
		GroupByAction* pGroupAction = (GroupByAction*) m_pGroupBy->m_ActionsArray[i];
		pGroupAction->Unparse(oFile);
		oFile.UnparseSep(TRUE);
	}
	oFile.Close();

	UINT flags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;

	if (!iFile.Open(strFileName, flags))
	{
		DeleteFile (strFileName);
		return;
	}

	while (iFile.ReadString(buffer, 255))
		strTmp += buffer;

	iFile.Close();
	DeleteFile (strFileName);

	ConvertCString(strTmp, LF_TO_CRLF);
	m_edtActions.SetWindowText(strTmp);
}


//----------------------------------------------------------------------------
BOOL CGroupByDlg::GroupByOK(GroupByData* pGroupBy)
{
	ExpressionWithCheck	exprGroupBy	(m_pSymTable);

	if (!m_edtGroupBy.CheckExp(exprGroupBy, DATA_STR_TYPE, TRUE))
	{
		m_edtGroupBy.SetCtrlFocus(TRUE);
    	return FALSE;
    }
    
	*pGroupBy->m_pGroupingTuple = exprGroupBy;
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CGroupByDlg::GroupByActionOK(GroupByData* pGroupBy)
{
	CString	strAction;
	m_edtActions.GetWindowText(strAction);

	Parser lex(strAction);

	while (!lex.LookAhead(T_EOF))
	{
		GroupByAction* pGroupAction = new GroupByAction;
		if (!pGroupAction->Parse(lex, m_pSymTable))
		{
           	delete pGroupAction;
			return FALSE;
		}
		pGroupBy->m_ActionsArray.Add(pGroupAction);

		if (!lex.ParseSep())
			return FALSE;
	}

	if (pGroupBy->m_pGroupingTuple->IsEmpty() && pGroupBy->m_ActionsArray.GetSize() == 0)
		return TRUE;
		
	if (pGroupBy->m_pGroupingTuple->IsEmpty())
   	{                
   		AfxMessageBox(_TB("Grouping expression missing!"));
   		return FALSE;
   	}

	if (pGroupBy->m_ActionsArray.GetSize() == 0)
	{
		AfxMessageBox(_TB("Grouping actions missing!"));
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void CGroupByDlg::OnOK()
{
	CString		strOrderBy;
	Expression	exprFilter	(m_pSymTable);

/*@@TODO
	Expression	exprOrderBy	(m_pSymTable);

	if (!m_edtOrderBy.IsEmpty())
    {
		if (!m_edtOrderBy.CheckExp(exprOrderBy, DATA_STR_TYPE))
		{
			m_edtOrderBy.SetCtrlFocus(TRUE);
			return;
		}
		
		m_pQueryData->m_bDescendingOrder = m_btnDescending.GetCheck();
	}
*/
	if (!m_edtFilter.CheckExp(exprFilter, DATA_BOOL_TYPE, TRUE))
	{
		m_edtFilter.SetCtrlFocus(TRUE);
		return;
	}

	GroupByData* pGroupBy = new GroupByData(*m_pSymTable);
	if (!GroupByOK(pGroupBy) || !GroupByActionOK(pGroupBy))
	{
		delete pGroupBy;
        return;
	}

	if (!pGroupBy->m_pHavingTupleFilter)
		pGroupBy->m_pHavingTupleFilter = new Expression(&pGroupBy->m_SymTable);

	*pGroupBy->m_pHavingTupleFilter = exprFilter;

	m_pQueryData->SetGroupBy(pGroupBy);
//@@TODO	m_pQueryData->m_OrderBy		= exprOrderBy;

	EndDialog(TRUE);
}

//----------------------------------------------------------------------------
void CGroupByDlg::OnCancel()
{
	EndDialog(FALSE);
}
