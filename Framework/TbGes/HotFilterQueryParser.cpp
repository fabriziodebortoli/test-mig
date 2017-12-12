#include "stdafx.h"

#include <TbWoormEngine\ActionsRepEngin.H>
#include <TbWoormEngine\PRGDATA.H>

#include "BARQUERY.H"

#include "HotFilter.h"

#include "HotFilterQueryParser.h"

//#include "UIHotFilterQueryParser.hrc"
//#include <OFM\Core\CoreEnums.h>

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//==============================================================================================
//									CQuerySaveAsDlg
//==============================================================================================
class CQuerySaveAsDlg : public CParsedDialog
{
public:
	CQuerySaveAsDlg
		(
					CWnd*		pParent,
			const	CString&	strDefaultName
		);

public:
	const DataStr& GetQueryName()		{ return m_QueryName; }

protected:
	virtual BOOL	OnInitDialog();

private:
	DataStr		m_QueryName;
	CStrEdit	m_QueryNameCtrl;

private:
	void	UpdateControlStatus	(BOOL bEnable = TRUE);

protected:
//{{AFX_MSG( CQuerySaveAsDlg )  
	afx_msg			void OnSave	();
//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// 				class CQuerySaveAsDlg implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CQuerySaveAsDlg, CParsedDialog)
	//{{AFX_MSG_MAP( CQuerySaveAsDlg )
		ON_BN_CLICKED		(IDC_SAVE_QUERY,		OnSave)
		ON_BN_CLICKED		(IDOK,					OnSave)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CQuerySaveAsDlg::CQuerySaveAsDlg
	(
				CWnd*		pParent,
		const	CString&	strDefaultName
	)
	:
	CParsedDialog	(IDD_NEW_QUERY_NAME, pParent),
	m_QueryNameCtrl	(0, &m_QueryName)
{
	m_QueryName = strDefaultName;
}

//----------------------------------------------------------------------------
BOOL CQuerySaveAsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_QueryNameCtrl.SubclassEdit(IDC_NEW_QUERY_NAME, this);

	m_QueryNameCtrl.UpdateCtrlView();

	return TRUE;
}

//----------------------------------------------------------------------------
void CQuerySaveAsDlg::UpdateControlStatus(BOOL bEnable /* = TRUE*/)
{
	m_QueryNameCtrl				.EnableWindow(bEnable); 
	GetDlgItem(IDC_SAVE_QUERY)	->EnableWindow(bEnable);
	GetDlgItem(IDCANCEL)		->EnableWindow(bEnable);
}

//----------------------------------------------------------------------------
void CQuerySaveAsDlg::OnSave()
{
	m_QueryNameCtrl.DoKillFocus(GetDlgItem(IDC_SAVE_QUERY));
	if (m_QueryName.IsEmpty())
	{
		MessageBox(_TB("Set a name for the query."));
		return;
	}

	if (m_QueryName.GetString().CompareNoCase(PredefinedQuery::Default()) == 0)
	{
		MessageBox(_TB("Name not allowed."));
		return;
	}

	EndDialog(IDOK);
}


//==============================================================================================
//									CQueriesCombo
//==============================================================================================

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CQueriesCombo, CStrCombo)

//-----------------------------------------------------------------------------
CQueriesCombo::CQueriesCombo()
	:
	CStrCombo		(),
	m_pQueryParser	(NULL)
{
	m_bNoResetAssociations = TRUE;
}

//-----------------------------------------------------------------------------
CQueriesCombo::CQueriesCombo(UINT nBtnIDBmp, DataStr* pData)
	:
	CStrCombo		(nBtnIDBmp, pData),
	m_pQueryParser	(NULL)
{
	m_bNoResetAssociations = TRUE;
}

//-----------------------------------------------------------------------------
void CQueriesCombo::OnFillListBox()
{
	if (!m_pQueryParser)
		return;

	m_pQueryParser->FillAvailableQueries(this);
}

//==============================================================================================
//									HotFilterQueries
//==============================================================================================

static const TCHAR szHotFilters[]		= _T("HotFilters");
static const TCHAR szExtHFLQueries[]	= _T(".HFLQry");

// specialization of AllQueries only to change path and file name
//=============================================================================
class HotFilterQueries : public AllQueries
{
public:
	HotFilterQueries	(HotFilterRange* pHotFilter);

public:
	virtual CString GetQueryDir				();
	virtual CString	GetQueryFileName		();
	virtual CString GetQueryFileFullPathName(BOOL bFromSave = FALSE);

private:
	const CString GetDocumentHotFilterPath(const CTBNamespace& aNamespace, CPathFinder::PosType pos, const CString& sUserRole = _T(""), BOOL bCreateDir = FALSE) const;

private:
	CString	m_strFilename;
};

//-----------------------------------------------------------------------------
HotFilterQueries::HotFilterQueries(HotFilterRange* pHotFilter)
{
	m_strFilename =  pHotFilter->GetRuntimeClass()->m_lpszClassName;
}

//-----------------------------------------------------------------------------
const CString HotFilterQueries::GetDocumentHotFilterPath (const CTBNamespace& aNamespace, CPathFinder::PosType pos, const CString& sUserRole, BOOL bCreateDir) const
{
	CString sPath = AfxGetPathFinder()->GetDocumentPath(aNamespace, pos, bCreateDir) + SLASH_CHAR + szHotFilters;
	if (bCreateDir && !ExistPath (sPath))	CreateDirectory (sPath);

	return AfxGetPathFinder()->ToPosDirectory(sPath, pos, sUserRole, bCreateDir);
}

//-----------------------------------------------------------------------------
CString HotFilterQueries::GetQueryFileFullPathName (BOOL bFromSave /*FALSE*/)
{
	if (GetCustomizeNamespace().IsEmpty() || !GetCustomizeNamespace().IsValid())
		return _T("");

	CString strHFLQueryFilePath = GetDocumentHotFilterPath (GetCustomizeNamespace(), CPathFinder::ALL_USERS, _T(""), bFromSave);

	// lettura || scrittura
	if (!bFromSave || ExistPath (strHFLQueryFilePath))
		return MakeFilePath (strHFLQueryFilePath, GetQueryFileName (), szExtHFLQueries);

	AfxMessageBox (cwsprintf (_TB("Unable to create the directory:\r\n'{0-%s}'\r\nto save the query configuration files.\r\nMake sure you have the required access rights."), strHFLQueryFilePath), MB_OK | MB_ICONSTOP);
	return _T("");
}

//-----------------------------------------------------------------------------
CString HotFilterQueries::GetQueryDir()
{
	if (GetCustomizeNamespace().IsEmpty() || !GetCustomizeNamespace().IsValid())
		return _T("");
	return GetDocumentHotFilterPath (GetCustomizeNamespace(), CPathFinder::ALL_USERS);
}

//-----------------------------------------------------------------------------
CString HotFilterQueries::GetQueryFileName ()
{
	return m_strFilename;
}

//==============================================================================================
//									HotFilterQueryParser
//==============================================================================================

//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(HotFilterQueryParser, CObject)

//----------------------------------------------------------------------------------------------
HotFilterQueryParser::HotFilterQueryParser(HotFilterRange* pHotFilter)
	:
	m_pHotFilter	(pHotFilter),
	m_pHFLQueries	(NULL),
	m_pRec			(NULL),
	m_pTbl			(NULL)
{
	m_pHFLQueries	= new HotFilterQueries(pHotFilter);
	m_pRec			= (SqlRecord*)m_pHotFilter->GetRecordClass()->CreateObject();
	m_pTbl			= new SqlTable(m_pRec, m_pHotFilter->GetDocument()->GetReadOnlySqlSession(),  m_pHotFilter->GetDocument());

	//m_pHFLQueries->SetCustomizeNamespace(m_pHotFilter->GetDocument()->GetNamespace());
	m_pHFLQueries->SetCustomizeNamespace(m_pHotFilter->m_pHKLRangeFrom->GetAddOnFly());
	m_pHFLQueries->Attach(m_pTbl, FALSE);

	pHotFilter->BindQueryParameters(&m_QueryParams);
}

//----------------------------------------------------------------------------------------------
HotFilterQueryParser::~HotFilterQueryParser()
{
	SAFE_DELETE(m_pTbl);
	SAFE_DELETE(m_pRec);
	SAFE_DELETE(m_pHFLQueries);
}

//----------------------------------------------------------------------------------------------
void HotFilterQueryParser::FillAvailableQueries(CQueriesCombo* pQueryCombo)
{
	DataStr aQueryName;
	pQueryCombo->ResetAssociations(TRUE);
	for (int i = 0; i <= m_pHFLQueries->GetUpperBound(); i++)
	{
		QueryObj* pQueryObj = m_pHFLQueries->GetQueryObj(i);
		if (pQueryObj)
		{
			aQueryName = pQueryObj->m_strQueryName;
			pQueryCombo->CParsedCombo::AddAssociation(pQueryObj->m_strQueryName, aQueryName);
		}
	}
}

//----------------------------------------------------------------------------------------------
CString UnparseDataObj(const DataObj& aDataObj)
{
	CString expr = _T("");
	switch (aDataObj.GetDataType().m_wType)
	{
		case DATA_STR_TYPE:
			expr = cwsprintf(_T("\"{0-%s}\""), aDataObj.FormatData());
		break;

		case DATA_INT_TYPE:
		case DATA_LNG_TYPE:
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
			expr = cwsprintf(_T("{0-%s}"), aDataObj.FormatData());
		break;

		case DATA_DATE_TYPE:
		{
			DataDate& aDataDate = (DataDate&)aDataObj;
			expr = cwsprintf(_T("{d \" {0-%4d}{1-%2d}{2-%2d} \" }"), aDataDate.Year(), aDataDate.Month(), aDataDate.Day());
		}
		break;

		case DATA_BOOL_TYPE:
		{
			Token t = ((DataBool&)aDataObj) == TRUE ? T_TRUE : T_FALSE;
			expr = cwsprintf(_T("{0-%s}"), AfxGetTokensTable()->ToString(t));
		}
		break;

		case DATA_ENUM_TYPE:
			expr = cwsprintf(_T("{ {0-%d} : {1-%d} }"), ((DataEnum&)aDataObj).GetTagValue(), ((DataEnum&)aDataObj).GetItemValue());
		break;

		default:
			ASSERT_TRACE(FALSE,"No unparse allowed for this data type");
		break;
	}

	return expr;
}

//----------------------------------------------------------------------------------------------
ProgramData* HotFilterQueryParser::PrepareProgramData()
{
	ProgramData* pPrgData = new ProgramData(NULL);

	ASSERT_TRACE(!m_QueryParams.IsEmpty(),"No query paramaters bound, impossible to save it.");

	//params.Add(new DataStr(_T("001")));
	//params.Add(new DataInt(1));
	//params.Add(new DataLng(2));
	//params.Add(new DataDbl(3.3));
	//params.Add(new DataMon(4.4));
	//params.Add(new DataPerc(5.5));
	//params.Add(new DataDate(15,11,2013));
	//params.Add(new DataBool(TRUE));
	//params.Add(new DataEnum(E_ARITHMETIC_OPERATION_GREATEREQUAL));

	DataObj*	pDataObj;
	CString		strParamName;
	POSITION p = m_QueryParams.GetStartPosition();
	while (p != NULL)
	{
		m_QueryParams.GetNextAssoc(p, strParamName, pDataObj);

		WoormField* wf = new WoormField(strParamName, WoormField::FIELD_INPUT, pDataObj->GetDataType()); 
		if (pDataObj->GetDataType() == DataType::String)
			wf->SetLen(15);
		
		wf->SetInitExpression(UnparseDataObj(*pDataObj), pDataObj->GetDataType());
		
		pPrgData->GetSymTable()->Add(wf);
	}

	return pPrgData;
}

//----------------------------------------------------------------------------------------------
void HotFilterQueryParser::SaveNewQuery(const CString& strQueryName)
{
	SqlTableStruct sts(m_pHFLQueries->m_TableInfo);

	ProgramData* pPrgData= PrepareProgramData();
	QueryInfo* pQueryInfo = new QueryInfo (m_pHotFilter->GetDocument()->m_pSqlConnection, FALSE, &sts, pPrgData);

	// Aggiungo alla struttura in memoria la nuova query
	m_pHFLQueries->AddNewQuery (strQueryName, pQueryInfo, FALSE);
}

//----------------------------------------------------------------------------------------------
void HotFilterQueryParser::UpdateExistingQuery(const CString& strQueryName)
{
	QueryObj* pQuery;
	if (!FindQuery(strQueryName, pQuery))
	{
		ASSERT_TRACE1(FALSE, "Query not found: %s", strQueryName);
		return;
	}

	ProgramData* pPrgData= PrepareProgramData();
	m_pHFLQueries->UpdateQuery(pQuery, FALSE, _T(""), _T(""), FALSE, pPrgData);
}

//----------------------------------------------------------------------------------------------
void HotFilterQueryParser::Save(CWnd* pParentFrame, DataStr& strQueryName)
{
	if	(
			strQueryName.IsEmpty() ||
			strQueryName == PredefinedQuery::Default()
		)
		SaveAs(pParentFrame, strQueryName);
	else
		UpdateExistingQuery(strQueryName);
}

//----------------------------------------------------------------------------------------------
void HotFilterQueryParser::SaveAs(CWnd* pParentFrame, DataStr& strQueryName)
{
	CString strNewQueryName;
	while (TRUE)
	{
		if (!AskQuerySaveAs(pParentFrame, strNewQueryName))
			break;

		QueryObj* pQuery;
		if (FindQuery(strNewQueryName, pQuery))
		{
			if (AfxMessageBox(_TB("A query with this name is already present, overwrite?"), MB_OKCANCEL | MB_ICONQUESTION) == IDOK)
			{
				UpdateExistingQuery(strNewQueryName);
				strQueryName = strNewQueryName;
				break;
			}
		}
		else
		{
			SaveNewQuery(strNewQueryName);
			strQueryName = strNewQueryName;
			break;
		}
	}
}

//----------------------------------------------------------------------------------------------
BOOL HotFilterQueryParser::FindQuery(const CString& strQueryName, QueryObj*& pQuery)
{
	for (int i = 0; i <= m_pHFLQueries->GetUpperBound(); i++) 
	{
		if (m_pHFLQueries->GetAt(i)->m_strQueryName == strQueryName)
		{
			pQuery = m_pHFLQueries->GetAt(i);
			return TRUE;
		}
	}
	pQuery = NULL;
	return FALSE;
}

//----------------------------------------------------------------------------------------------
BOOL HotFilterQueryParser::Delete(DataStr& strQueryName)
{
	QueryObj*	pQuery;
	BOOL		bOk = FALSE;

	if (FindQuery(strQueryName, pQuery))
	{
		if (m_pHFLQueries->DeleteQuery(pQuery))
		{
			strQueryName = PredefinedQuery::Default();
			bOk = TRUE;
		}
	}
	else
	{
		ASSERT_TRACE1(FALSE, "Query not found: %s", strQueryName.FormatData());
	}

	return bOk;
}

//----------------------------------------------------------------------------------------------
void HotFilterQueryParser::Load(const CString& strQueryName)
{
	QueryObj*	pQuery;

	if (!FindQuery(strQueryName, pQuery))
	{
		ASSERT_TRACE1(FALSE, "Query not found: %s", (LPCTSTR)strQueryName);
		return;
	}

	QueryInfo* pQueryInfo = m_pHFLQueries->GetQueryInfo (pQuery);
	
	WoormTable* pSymTable = pQueryInfo->m_pPrgData->GetSymTable();
	for (int i = 0; i<= pSymTable->GetUpperBound(); i++)
	{
		WoormField* wf = pSymTable->GetAt(i);
		DataObj* pDataObj;
		if (m_QueryParams.Lookup(wf->GetName(), pDataObj))
		{
			Expression* e = wf->GetInitExpression();
			e->Eval(pDataObj);
		}
	}
}

//----------------------------------------------------------------------------------------------
BOOL HotFilterQueryParser::AskQuerySaveAs(CWnd* pParent, CString& strQueryName)
{
	CQuerySaveAsDlg aDlg(pParent, strQueryName);
	if (aDlg.DoModal() != IDOK)
		return FALSE;

	strQueryName	= aDlg.GetQueryName();

	return TRUE;
}



