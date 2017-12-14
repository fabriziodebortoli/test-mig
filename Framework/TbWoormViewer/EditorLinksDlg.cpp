#include "stdafx.h"

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGenlib\TbCommandInterface.h>
#include <TbGenlib\parsCbx.h>
#include <TbGenlibUI\TBExplorer.h>

#include <TbGes\FormMngDlg.h>

#include <TbWoormEngine\RepEngin.h>
#include <TbWoormEngine\RepTable.h>
#include <TbWoormEngine\EdtMng.h>
#include <TbWoormEngine\eqnedit.h>

#include "baseobj.h"
#include "woormdoc.h"
#include "rectobj.h"
#include "Table.h"
#include "Column.h"

#include "EditorLinksDlg.h"


// resources
//#include <TbWoormEngine\eqnedit.hjson> //JSON AUTOMATIC UPDATE

#include "EditorLinksDlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _OUTDATED

#define DESCRTYPE_POS	0
#define NAME_POS		1
#define EXPR_POS		2
#define DATATYPE_POS	3


//------------------------------------------------------------------------------
CString		GetStringConnectionType(WoormLink::WoormLinkType connType)
{
	CString strLinkType;

	switch (connType)
	{
		case WoormLink::ConnectionReport: 
			{
			strLinkType = _TB("LinkReport");
			break;
			}
		case WoormLink::ConnectionForm:
			{
			strLinkType = _TB("LinkForm");
			break;
			}
		case WoormLink::ConnectionFunction: 
			{
			strLinkType = _TB("LinkFunction");
			break;
			}
		case WoormLink::ConnectionURL: 
			{
			strLinkType = _TB("LinkUrl");
			break;
			}
	}
	return strLinkType;
}




//==============================================================================
//          Class CManageLinkDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CManageLinkDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CManageLinkDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CManageLinkDlg)
	ON_BN_CLICKED       (IDC_RADIO_NEWLINKREP,		ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIONEW_LINKFORM,		ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIO_MODLINK,			ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIONEW_LINKFUNCTION,	ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIONEW_LINKURL,		ChangeRadioSelection)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CManageLinkDlg::CManageLinkDlg 
	(
		CString strLabelColumn, 
		CWoormDocMng* pDocument, 
		WORD columnID, 
		CWnd* aParent /*=NULL*/
	)
	:
	CParsedDialog		(IDD_ADD_MOD_LINK, aParent),
	m_pDocument			(pDocument),
	m_nAlias			(columnID),
	m_strLabelColumn	(strLabelColumn),
	m_pConn				(NULL)
{
}


//-----------------------------------------------------------------------------
BOOL CManageLinkDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	//----
	m_cbxLinkUrlType.SubclassDlgItem(IDC_LINKURL_TYPE, this);

	int idx = m_cbxLinkUrlType.AddString(_T("File:"));
	m_cbxLinkUrlType.SetItemData(idx, WoormLink::File);

	idx = m_cbxLinkUrlType.AddString(_T("Url:"));
	m_cbxLinkUrlType.SetItemData(idx, WoormLink::Url);

	idx = m_cbxLinkUrlType.AddString(_T("MailTo:"));
	m_cbxLinkUrlType.SetItemData(idx, WoormLink::MailTo);

	idx = m_cbxLinkUrlType.AddString(_T("CallTo:"));
	m_cbxLinkUrlType.SetItemData(idx, WoormLink::CallTo);

	idx = m_cbxLinkUrlType.AddString(_T("Google Map:"));
	m_cbxLinkUrlType.SetItemData(idx, WoormLink::GoogleMap);

	m_cbxLinkUrlType.SetCurSel(0);
	//----

	m_listCtrlWhenClauses.			SubclassDlgItem (IDC_LISTCTRL_WHENLINK,			this);
	m_listCtrlWhenClauses.InsertColumn	(0,	 _TB("Type"),		LVCFMT_LEFT, 100, 0);
	m_listCtrlWhenClauses.InsertColumn	(1,	 _TB("Link to"),	LVCFMT_LEFT, 300, 0);
	m_listCtrlWhenClauses.InsertColumn	(2,	 _TB("Condition"),	LVCFMT_LEFT, 430, 0);

	m_listCtrlWhenClauses.SetExtendedStyle(m_listCtrlWhenClauses.GetExtendedStyle()|LVS_EX_FULLROWSELECT);
	m_listCtrlWhenClauses.EnableWindow(FALSE);

	//building the "column name to display" string
	if (m_strLabelColumn.IsEmpty())
	{
		SymField* rp =  m_pDocument->m_pEditorManager->GetSymTable()->GetFieldByID (m_nAlias);
		m_strLabelColumn = _T("(");
		m_strLabelColumn.Append(rp->GetName()); 
		m_strLabelColumn.Append(_T(")"));
	}

	//building the dialog caption string 
	CString dialogCaption;
	this->GetWindowText(dialogCaption);
	dialogCaption.Append(_T(" - "));
	dialogCaption.Append(m_strLabelColumn);
	this->SetWindowText(dialogCaption);
	

	// loading of old connections
	if (m_pDocument->ThereIsWoormLink(m_nAlias))
	{
		SendDlgItemMessage(IDC_RADIO_MODLINK, BM_SETCHECK, TRUE, 0);
		m_listCtrlWhenClauses.EnableWindow(TRUE);
		GetDlgItem(IDC_CHECK_VARIABLE)->EnableWindow(FALSE);

		int idxInsert = 0;
		//load when clause into list control	
		for (int i = 0; i < m_pDocument->m_arWoormLinks.GetSize(); i++)
		{
			m_pConn = m_pDocument->m_arWoormLinks.GetAt(i);
			ASSERT (m_pConn);
			if (m_pConn->m_nAlias == m_nAlias)
			{
				CString strLinkType, strName, strExpr;
				GetLinkDescription(strLinkType, strName, strExpr);
			
				m_listCtrlWhenClauses.InsertItem (idxInsert, strLinkType);
				m_listCtrlWhenClauses.SetItemText(idxInsert, 1, strName);
				m_listCtrlWhenClauses.SetItemText(idxInsert, 2, strExpr);
				m_listCtrlWhenClauses.SetItemData(idxInsert,i); //i is the position in the connection array
				idxInsert++;
			}
		}
		m_listCtrlWhenClauses.SetItemState(0, LVIS_FOCUSED|LVIS_SELECTED, LVIS_FOCUSED|LVIS_SELECTED);		
		m_listCtrlWhenClauses.SetFocus();
	}
	else
	{
		(/*(CButton*)*/ GetDlgItem(IDC_RADIO_MODLINK))->EnableWindow(FALSE);
		SendDlgItemMessage(IDC_RADIO_NEWLINKREP, BM_SETCHECK, TRUE, 0);
		m_listCtrlWhenClauses.EnableWindow(FALSE);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CManageLinkDlg::OnOK()
{
	WoormLink::WoormLinkType connType;
	WoormLink::WoormLinkSubType subType = WoormLink::File;

	if (IsDlgButtonChecked(IDC_RADIO_MODLINK))
	{
		int linkId;
		POSITION pos = m_listCtrlWhenClauses.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			int posCtrlList = m_listCtrlWhenClauses.GetNextSelectedItem(pos);
			linkId = m_listCtrlWhenClauses.GetItemData(posCtrlList);
		}
		else
		{
			AfxMessageBox(_TB("Select a link"), MB_OK);
			m_listCtrlWhenClauses.SetRedraw(TRUE);
			return;
		}

		WoormLink* pConn = m_pDocument->m_arWoormLinks.GetAt(linkId);

		CParamLinkDlg dialogModLink(m_pDocument, pConn->m_LinkType, pConn->m_SubType, MODIFY, m_nAlias, m_strLabelColumn, pConn->m_bLinkTargetByField, linkId);
		dialogModLink.DoModal();	
	}
	else
	{
		BOOL isLinkByVariable =	FALSE;
		isLinkByVariable =	IsDlgButtonChecked(IDC_CHECK_VARIABLE);

		if (IsDlgButtonChecked(IDC_RADIO_NEWLINKREP)) 
		{
			connType = WoormLink::ConnectionReport;
		}
		else if (IsDlgButtonChecked(IDC_RADIONEW_LINKFORM)) 
		{
			connType = WoormLink::ConnectionForm;
		}
		else if (IsDlgButtonChecked(IDC_RADIONEW_LINKFUNCTION))
		{
			connType = WoormLink::ConnectionFunction;
		}
		else if (IsDlgButtonChecked(IDC_RADIONEW_LINKURL))
		{
			connType = WoormLink::ConnectionURL;

			int idx = m_cbxLinkUrlType.GetCurSel();
			if (idx >= 0)
				subType = (WoormLink::WoormLinkSubType) this->m_cbxLinkUrlType.GetItemData(idx);
		}
		else 
			EndDialog(IDCANCEL);

		CParamLinkDlg dialogNewLink (m_pDocument, connType, subType, NEWCONN, m_nAlias, m_strLabelColumn, isLinkByVariable);
		dialogNewLink.DoModal();
	}
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CManageLinkDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CManageLinkDlg::ChangeRadioSelection()
{
  if (
	      IsDlgButtonChecked(IDC_RADIO_NEWLINKREP)
	   || IsDlgButtonChecked(IDC_RADIONEW_LINKFORM) 
	   || IsDlgButtonChecked(IDC_RADIONEW_LINKFUNCTION) 
	   || IsDlgButtonChecked(IDC_RADIONEW_LINKURL) 
	 ) 
  {
	GetDlgItem(IDC_CHECK_VARIABLE)->EnableWindow(TRUE);
	m_listCtrlWhenClauses.EnableWindow(FALSE);
  }
  else  //old link selection enabled
  {
	GetDlgItem(IDC_CHECK_VARIABLE)->EnableWindow(FALSE);
	m_listCtrlWhenClauses.EnableWindow(TRUE);
	
	m_listCtrlWhenClauses.SetItemState(0, LVIS_FOCUSED|LVIS_SELECTED, LVIS_FOCUSED|LVIS_SELECTED);
	m_listCtrlWhenClauses.SetFocus();
  }

  m_cbxLinkUrlType.EnableWindow(IsDlgButtonChecked(IDC_RADIONEW_LINKURL));
}
  
//-----------------------------------------------------------------------------
void CManageLinkDlg::GetLinkDescription(CString& linkType, CString& strTarget, CString& strExpr)
{
	linkType = GetStringConnectionType(m_pConn->m_LinkType);

	strTarget = m_pConn->m_strTarget;
	if (m_pConn->m_SubType == WoormLink::MailTo)
		strTarget += _T(" (MailTo:)");
	else if (m_pConn->m_SubType == WoormLink::CallTo)
		strTarget += _T(" (CallTo:)");
	else if (m_pConn->m_SubType == WoormLink::GoogleMap)
		strTarget += _T(" (Google Map:)");

	if (m_pConn->m_pEnableLinkWhenExpr)
	{
		strExpr = m_pConn->m_pEnableLinkWhenExpr->ToString();
		strExpr.Replace(_T("\r\n"), _T(" "));
		strExpr.Replace(_T("\r"), _T(" "));
		strExpr.Replace(_T("\n"), _T(" "));
	}
}


//==============================================================================
//          Class CParamLinkDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CParamLinkDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CParamLinkDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CParamLinkDlg)
		ON_BN_CLICKED       (IDC_BTN_DELLINK,									DeleteLink)
		ON_BN_CLICKED		(IDC_BTN_TBEXPL,									OpenNsSelectionDlg)
		ON_BN_CLICKED		(IDC_BTN_LOADPARAM,									LoadConnectionParams)
		ON_BN_CLICKED		(IDC_BTN_ADD_PARAM,									AddConnectionParam)
		ON_BN_CLICKED		(IDC_BTN_REMOVE_PARAM,								RemoveConnectionParam)

		ON_BN_CLICKED		(IDC_BTN_APPLY_ITEMEXPR,							ApplyExprParam)
		ON_NOTIFY			(NM_DBLCLK,					IDC_LIST_PARAMEXPR,		OnSelectParam)
		ON_NOTIFY			(LVN_ITEMCHANGED,			IDC_LIST_PARAMEXPR,		OnSelectParam)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CParamLinkDlg::CParamLinkDlg 
	(
		CWoormDocMng* pDocument, 
		WoormLink::WoormLinkType connType, 
		WoormLink::WoormLinkSubType subType, 
		Mode mode, 
		WORD nAlias, 
		CString strLabelColumn, 
		BOOL bIsLinkByVariable /*= FALSE*/, 
		int idxLink /*= -1*/, 
		CWnd* aParent /*=NULL*/
	)
	:
	CParsedDialog		(IDD_LINK_PARAM, aParent),
	m_pDocument			(pDocument),
	m_idxLink			(idxLink),
	m_connType			(connType),
	m_subType			(subType),
	m_bLinkTargetByField(bIsLinkByVariable),
	m_mode				(mode),
	m_nAlias			(nAlias),
	m_strLabelColumn	(strLabelColumn),
	m_pConn				(NULL),
	m_pRepSymTable		(NULL),
	m_CalledReportVarSymTable (WoormTable::SimpleTable_RDEVIEW, pDocument)
{
}

//-----------------------------------------------------------------------------
BOOL CParamLinkDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
		
	//graphics objects associatons
	m_editLinkOn.				SubclassDlgItem	(IDC_EDIT_LINK_ON,	        	this);
	m_editLinkName.				SubclassDlgItem (IDC_EDIT_NSLINK,				this);
	m_editLinkWhen.				SubclassEdit	(IDC_EDIT_LINK_WHEN,			this);
	m_editLinkBefore.			SubclassEdit	(IDC_EDIT_LINK_BEFORE,			this);
	m_editLinkAfter.			SubclassEdit	(IDC_EDIT_LINK_AFTER,			this);
	m_btnExplorer.              SubclassDlgItem (IDC_BTN_TBEXPL,				this);
	m_btnDeleteLink. 			SubclassDlgItem (IDC_BTN_DELLINK,				this);
	m_btnLoadParam.				SubclassDlgItem (IDC_BTN_LOADPARAM,				this);
	m_btnAddParam.				SubclassDlgItem	(IDC_BTN_ADD_PARAM,				this);
	m_btnRemoveParam.			SubclassDlgItem	(IDC_BTN_REMOVE_PARAM,				this);
	m_btnApply.					SubclassDlgItem (IDC_BTN_APPLY_ITEMEXPR,		this);
	m_editParamExpression.		SubclassEdit	(IDC_EDIT_ITEMEXPR,				this);
	m_listCtrlParams.			SubclassDlgItem (IDC_LIST_PARAMEXPR,			this);
	m_varInEdit.				SubclassDlgItem	(IDC_EDIT_VAR,					this);

	//building the dialog caption string 
	CString dialogCaption = GetStringConnectionType(m_connType) + _T(" ") +_TB("Parameters Management");
	this->SetWindowText(dialogCaption);

	if (m_bLinkTargetByField)
	{
		GetDlgItem(IDC_STATIC_NS)->SetWindowText(_TB("Variable contains link value:"));
	}
	else if (m_connType == WoormLink::ConnectionURL)
	{
		if (m_subType == WoormLink::File)
			GetDlgItem(IDC_STATIC_NS)->SetWindowText(_TB("Path name:"));
		else if (m_subType == WoormLink::Url)
			GetDlgItem(IDC_STATIC_NS)->SetWindowText(_TB("Url:"));
		else if (m_subType == WoormLink::MailTo)
		{
			GetDlgItem(IDC_STATIC_NS)->SetWindowText(_TB("Email address:"));

		}
		else if (m_subType == WoormLink::CallTo)
			GetDlgItem(IDC_STATIC_NS)->SetWindowText(_TB("Telephone number:"));
		else if (m_subType == WoormLink::GoogleMap)
		{
			GetDlgItem(IDC_STATIC_NS)->SetWindowText(_TB("Street address:"));
			m_editLinkName.SetWindowTextW(_T("http://maps.google.it/maps" ));
		}
	}

	//column with type description (localized)
	m_listCtrlParams.InsertColumn	(DESCRTYPE_POS,	 _TB("Type"),		LVCFMT_LEFT, 250, 0);
	m_listCtrlParams.InsertColumn	(NAME_POS,	 _TB("Name"),		LVCFMT_LEFT, 230, 0);
	m_listCtrlParams.InsertColumn	(EXPR_POS,	 _TB("Expression"),	LVCFMT_LEFT, 430, 0);
	//hidden column for datatype
	m_listCtrlParams.InsertColumn(DATATYPE_POS, _T(""), LVCFMT_LEFT, 0, 0); 

	m_listCtrlParams.SetExtendedStyle(m_listCtrlParams.GetExtendedStyle()|LVS_EX_FULLROWSELECT);
	m_idxItem = -1; //no parameter selected

	if (m_connType == WoormLink::ConnectionURL)
		m_btnExplorer.EnableWindow(FALSE);
	
	if (m_bLinkTargetByField || m_connType == WoormLink::ConnectionURL)
	{
		m_btnLoadParam.EnableWindow(FALSE);
	}
	else
	{
		m_btnAddParam.EnableWindow(FALSE); m_btnRemoveParam.EnableWindow(FALSE);
	}
	
	if (m_connType == WoormLink::ConnectionFunction)
	{
		m_btnLoadParam.EnableWindow(FALSE);
		m_btnAddParam.EnableWindow(FALSE); m_btnRemoveParam.EnableWindow(FALSE);
	}

	if (
		(m_connType == WoormLink::ConnectionURL)
		&& 
		(m_subType == WoormLink::MailTo || m_subType == WoormLink::CallTo || m_subType == WoormLink::GoogleMap)
		)
	{
		m_btnAddParam.EnableWindow(FALSE); m_btnRemoveParam.EnableWindow(FALSE);
		if (m_subType != WoormLink::MailTo) 
		{
			m_btnLoadParam.EnableWindow(TRUE);  
		}
	}
	
	EnableParameterControls(FALSE);

	//load sym table into edit expression field
	m_pRepSymTable = m_pDocument->m_pEditorManager->GetSymTable();

	m_editLinkWhen.SetSymbolTable(m_pRepSymTable);
	m_editParamExpression.SetSymbolTable(m_pRepSymTable);
	
	if (m_mode == MODIFY)
	{
		if (m_connType == WoormLink::ConnectionFunction)
		{
			m_btnLoadParam.EnableWindow(TRUE);
		}

		ASSERT (m_idxLink >= 0);
		//loading selected connection
		m_pConn = m_pDocument->m_arWoormLinks.GetAt ( m_idxLink );
		ASSERT (m_pConn);
		
		//showing connection details
		m_editLinkOn.SetWindowText(m_pConn->m_strLinkOwner);
		m_editLinkName.SetWindowText(m_pConn->m_strTarget);
		m_strDocNamespace = m_pConn->m_strTarget;

		EnableSelectionReport(FALSE);

		//Clear space, new line characth from when expression
		if (m_pConn->m_pEnableLinkWhenExpr)
		{
			CString strWhenExpr = m_pConn->m_pEnableLinkWhenExpr->ToString();
			strWhenExpr.Replace(_T("\r\n"), _T(" "));
			strWhenExpr.Replace(_T("\r"), _T(" "));
			strWhenExpr.Replace(_T("\n"), _T(" "));
			m_editLinkWhen.SetWindowText(strWhenExpr);
		}
	
		// loading of report's or form's parameters already used in the link	
		LoadOldLinkParam();

		if  (m_connType == WoormLink::ConnectionReport && m_mode == MODIFY)
		{
			//	load report's symTable	
			LoadReportSymTable();
		}

		if 	(m_connType == WoormLink::ConnectionForm && m_mode == MODIFY)	 
		{
			//documents(form) have a fixed number of parameters, so it isn't necessary select them
			m_btnLoadParam.EnableWindow(FALSE);
		}	
	}
	else //new linkform or new linkreport
	{
		if 	(
				m_connType != WoormLink::ConnectionReport
				&& 
				m_connType != WoormLink::ConnectionForm
				&&
				!(
					(m_connType == WoormLink::ConnectionURL)
					&& 
					(m_subType == WoormLink::CallTo || m_subType == WoormLink::GoogleMap)
				)
			)	
			m_btnLoadParam.EnableWindow(FALSE);

		m_editLinkOn.SetWindowText(m_strLabelColumn);
		m_btnDeleteLink.EnableWindow(FALSE);
		//creo nuova connessione
		m_pConn = new WoormLink(&(m_pDocument->m_ViewSymbolTable));
	}

	ASSERT(m_pConn && m_pConn->m_pLocalSymbolTable && m_pConn->m_pBeforeLink && m_pConn->m_pAfterLink);

	m_editLinkBefore.SetSymbolTable(m_pConn->m_pLocalSymbolTable);
	if (!m_pConn->m_pBeforeLink->IsEmpty())
		m_editLinkBefore.SetWindowText(m_pConn->m_pBeforeLink->Unparse());
	else
		m_editLinkBefore.SetWindowText(_T("Begin\r\nEnd"));

	m_editLinkAfter.SetSymbolTable(m_pConn->m_pLocalSymbolTable);
	if (!m_pConn->m_pAfterLink->IsEmpty())
		m_editLinkAfter.SetWindowText(m_pConn->m_pAfterLink->Unparse());
	else
		m_editLinkAfter.SetWindowText(_T("Begin\r\nEnd"));

	return TRUE;
}

//-----------------------------------------------------------------------------
void ShowWrongNamespaceMsg()
{
	AfxMessageBox(_TB("Invalid Namespace"),	MB_OK | MB_ICONEXCLAMATION);
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::OnOK()
{
	CString strlinkName, strFilterExpr, strLinkOn;
			
	//check there are no connection parameters without expression
	if (!CheckParametersExpression())
		 return;

	m_editLinkName.GetWindowText	(strlinkName);
	if (strlinkName.IsEmpty())
	{
		AfxMessageBox(_TB("Select a namespace"), MB_OK);
		return;
	}

	if (m_connType == WoormLink::ConnectionForm && !this->m_bLinkTargetByField)
	{
		CTBNamespace ns(CTBNamespace::DOCUMENT , strlinkName);
		if (!CheckNamespace(ns))
		{	
			ShowWrongNamespaceMsg();	
			return;
		}
	}
	else if (m_connType == WoormLink::ConnectionReport && !this->m_bLinkTargetByField)
	{
		CTBNamespace ns(CTBNamespace::REPORT,strlinkName );
		if (!CheckNamespace(ns))
		{	
			ShowWrongNamespaceMsg();	
			return;
		}
	}
	else if (m_connType == WoormLink::ConnectionFunction && !this->m_bLinkTargetByField)
	{
		CTBNamespace ns(CTBNamespace::FUNCTION, strlinkName);
		if (!CheckNamespace(ns))
		{	
			ShowWrongNamespaceMsg();	
			return;
		}
		
		CFunctionDescription aFunctionDescription;
		BOOL bOk = AfxGetTbCmdManager()->GetFunctionDescription(ns, aFunctionDescription, FALSE);
		if (!bOk)
			return;

		WoormField* pField = m_pConn->m_pLocalSymbolTable->GetFieldByID(SpecialReportField::ID.FUNCTION_RETURN_VALUE);
		if (!pField)
			return;
		pField->SetDataType(aFunctionDescription.GetReturnValueDataType());
	}

	m_editLinkWhen.GetWindowText (strFilterExpr);
	
	SymField* rp = m_pRepSymTable->GetFieldByID (m_nAlias);
	     
	m_pConn->m_strLinkOwner = rp->GetName();
	m_pConn->m_strTarget = strlinkName;
		
	m_pConn->m_nAlias = m_nAlias;

	if (!strFilterExpr.IsEmpty())
	{
		Parser localLex(strFilterExpr);
		Expression* filterExpr = new Expression(&(m_pDocument->m_ViewSymbolTable));
		if (!filterExpr->Parse(localLex, DataType::Bool, TRUE))
		{
			delete filterExpr;
			return;
		}    
		delete m_pConn->m_pEnableLinkWhenExpr;
		m_pConn->m_pEnableLinkWhenExpr = filterExpr;
	}

	if (m_mode == NEWCONN)
		AddConnection();  //add connection of correct type in case of creating new connection
	
	//deleting old saved connection parameters and load the new from de list control
	//preserve first two item because they are LinkedDocumentID and _ReturnValue special predef local ident
	if (m_pConn->m_pLocalSymbolTable->GetUpperBound() > 1)
		m_pConn->m_pLocalSymbolTable->RemoveAt(2, m_pConn->m_pLocalSymbolTable->GetUpperBound() - 1);
	else
		m_pConn->m_pLocalSymbolTable->RemoveAt(1, m_pConn->m_pLocalSymbolTable->GetUpperBound());

	for (int i = 0; i < m_listCtrlParams.GetItemCount(); i++)
	{
		DataType dt = DataType(m_listCtrlParams.GetItemText(i, DATATYPE_POS));
		CString varName = m_listCtrlParams.GetItemText(i, NAME_POS); 
		CString strExpr = m_listCtrlParams.GetItemText(i, EXPR_POS);
		
		m_pConn->AddLinkParam(varName, dt, strExpr);
	}
   
	//---- Before Block 
	CString strBlock;

	m_editLinkBefore.GetWindowText(strBlock); strBlock.Trim();
    Block*	pBeforeBlock = new Block(NULL, m_pConn->m_pLocalSymbolTable, NULL, FALSE);
	pBeforeBlock->SetForceBeginEnd();
	if (!strBlock.IsEmpty() && pBeforeBlock->Parse(strBlock))
	{
		SAFE_DELETE (m_pConn->m_pBeforeLink);
		m_pConn->m_pBeforeLink = pBeforeBlock;
	}

	m_editLinkAfter.GetWindowText(strBlock); strBlock.Trim();
 	Block*	pAfterBlock = new Block(NULL, m_pConn->m_pLocalSymbolTable, NULL, FALSE);
	pAfterBlock->SetForceBeginEnd();
	if (!strBlock.IsEmpty() && pAfterBlock->Parse(strBlock))
	{
		SAFE_DELETE (m_pConn->m_pAfterLink);
		m_pConn->m_pAfterLink = pAfterBlock;
	}

	m_pDocument->SetModifiedFlag();
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::OnCancel()
{	
	//se ero su nuovo, delete della connessione creata
	if ( m_mode == NEWCONN )
		SAFE_DELETE(m_pConn);
	
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::OpenNsSelectionDlg()
{
	if (m_bLinkTargetByField)
	{
		SelectObjectByVariable();
		return;
	}

	switch (m_connType)
	{
		case WoormLink::ConnectionReport:
			{
				SelectReportByExplorer();
				break;
			}
		case WoormLink::ConnectionForm:
			{
				SelectDocumentByExplorer();
				break;
			}
		case WoormLink::ConnectionFunction:
			{
				SelectFunctionByDialog();
				break;
			}
		case WoormLink::ConnectionURL:
		{
			//TODO
			//SelectObjectByVariable();
			break;
		}
	}
}

//-----------------------------------------------------------------------------
void	CParamLinkDlg::AddConnectionParam()
{
  //open dialog to insert manually type and name odf parameter
  CAddParamDlg dialogAddParam;

  if (dialogAddParam.DoModal() == IDOK)
  {
	InsertIntoCtrlList(dialogAddParam.m_dtParam, dialogAddParam.m_strNameParam);
  }
}

//-----------------------------------------------------------------------------
void	CParamLinkDlg::RemoveConnectionParam()
{
	if (m_idxItem < 0) 
		return;
	m_listCtrlParams.DeleteItem(m_idxItem);
	m_idxItem = -1;
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::LoadConnectionParams()
{
	//controllo ridondante, bottone disabilitato se non e' di questi 2 tipi
	if (m_connType == WoormLink::ConnectionReport)
	{
		LoadReportParameters();
	}
	else if (m_connType == WoormLink::ConnectionForm)
	{
		LoadFormParameters();
	}
	else if (m_connType == WoormLink::ConnectionFunction)
	{
		LoadFunctionParameters();
	}
	else if (m_connType == WoormLink::ConnectionURL)
	{
		for (int i = m_listCtrlParams.GetItemCount()-1; i >= 0 ; i--)
		{
			m_listCtrlParams.DeleteItem(i);
		}

		if (m_subType == WoormLink::CallTo)
		{
			InsertIntoCtrlList(DataType::String,_NS_WRMVAR("TelephonePrefix"));
			InsertIntoCtrlList(DataType::String,_NS_WRMVAR("ISOCountryCode"));
		}
		else if (m_subType == WoormLink::GoogleMap)
		{
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("AddressType"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("Address"), NULL, m_pConn->m_strTarget);
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("StreetNumber"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("City"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("ZipCode"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("County"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("Country"));
			//InsertIntoCtrlList(DataType::String, _NS_WRMVAR("ISOCountryCode"));
			//InsertIntoCtrlList(DataType::String, _NS_WRMVAR("District"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("FederalState"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("Latitude"));
			InsertIntoCtrlList(DataType::String, _NS_WRMVAR("Longitude"));
		}
	}
}

//-----------------------------------------------------------------------------------------
BOOL CParamLinkDlg::LoadFormParameters ()
{
	for (int i = m_listCtrlParams.GetItemCount()-1; i >= 0 ; i--)
	{
		m_listCtrlParams.DeleteItem(i);
	}
	
	m_editLinkName.GetWindowText(m_strDocNamespace);

	if (m_strDocNamespace.IsEmpty())
		return FALSE;

	CTBNamespace ns(CTBNamespace::DOCUMENT, m_strDocNamespace);

	
	if (!CheckNamespace(ns))
	{	
		ShowWrongNamespaceMsg();	
		return FALSE;
	}

	CLocalizableXMLDocument aXMLDBTDoc(ns, AfxGetPathFinder());
	aXMLDBTDoc.EnableMsgMode(FALSE);	

	if (aXMLDBTDoc.LoadXMLFile(AfxGetPathFinder()->GetDocumentDbtsFullName(ns)))
	{
		CXMLNode* pDBTMasterNode = aXMLDBTDoc.GetRootChildByName(_T("Master")); //XML_DBT_TYPE_MASTER_TAG
		if (pDBTMasterNode == NULL)
			return FALSE;

		CXMLNode* pChildNode = pDBTMasterNode->GetChildByName(_T("Table"));//XML_TABLE_TAG
		if (pChildNode == NULL)
			return FALSE;

		CString strTableName;
		pChildNode->GetText(strTableName);
		if(strTableName.IsEmpty())
			return FALSE;

		const SqlCatalogEntry* pEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(strTableName);
        if (pEntry == NULL) 
			return FALSE;
		SqlRecord* pRec = pEntry->CreateRecord();
		if (pRec == NULL) 
			return FALSE;

		for (int i=0; i < pRec->GetNumberSpecialColumns(); i++)
		{
			SqlRecordItem* pField = pRec->GetSpecialColumn(i);

			CString sName = pField->GetColumnName();
			DataType dt = pField->GetDataObj()->GetDataType();
			
			InsertIntoCtrlList(dt, sName);
		}
		delete pRec;
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------------------
BOOL CParamLinkDlg::LoadFunctionParameters ()
{
	CFunctionDescription aFunc;
	if (!AfxGetTbCmdManager()->GetFunctionDescription((LPCTSTR)m_strDocNamespace, aFunc))
		return FALSE;

	return LoadFunctionParameters(&aFunc);
}

//-----------------------------------------------------------------------------------------
BOOL CParamLinkDlg::LoadFunctionParameters (CFunctionDescription* pFunc)
{
	for (int i = m_listCtrlParams.GetItemCount()-1; i >= 0 ; i--)
	{
		m_listCtrlParams.DeleteItem(i);
	}
	if (!pFunc)
		return FALSE;
	
	for (int i = 0; i < pFunc->GetParameters().GetSize(); i++)
	{
		CDataObjDescription* pDescr = pFunc->GetParamDescription(i);

		CString sName = pDescr->GetName();
		DataType dt = pDescr->GetDataType();
		
		InsertIntoCtrlList(dt, sName);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------------------
void	CParamLinkDlg::LoadReportParameters		()
{
  	//loading symbol table, from a report path file
	m_editLinkName.GetWindowText(m_strDocNamespace);
	if (m_strDocNamespace.IsEmpty())
		return;

	CTBNamespace ns(CTBNamespace::REPORT, m_strDocNamespace);
	if (!CheckNamespace(ns))
	{	
		ShowWrongNamespaceMsg();	
		return;
	}
	CString	strReportPath = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
	
	m_CalledReportVarSymTable.RemoveAll();
	CString sError; int nLine = -1;
	BOOL bOk = m_CalledReportVarSymTable.ParseVariables(strReportPath, sError, nLine); //search T_VAR	
	if (!bOk)
	{
		//TODO show sError, nLine
		return;
	}

	//load report parameters,
	//allow user to select  a subset of them and put this subset into the listbox control
	
	//old variables already selected
	CStringArray arVariablesSelected;
	for (int i = 0; i < m_listCtrlParams.GetItemCount(); i++)
		arVariablesSelected.Add(m_listCtrlParams.GetItemText(i,NAME_POS));

	//open selection dialog
	CSelectReportParamDlg dialogSelectReportParam ( m_strDocNamespace, &m_CalledReportVarSymTable, &arVariablesSelected );

	if (dialogSelectReportParam.DoModal() == IDOK)
	{
		int num_selected_var = dialogSelectReportParam.m_arStrSelectedVariables.GetSize();
		if (num_selected_var > 0)
			EnableSelectionReport (FALSE);
		
		for (int i = 0; i < num_selected_var ; i++)
		{
			CString variableName = dialogSelectReportParam.m_arStrSelectedVariables.GetAt(i);
			SymField* pF = m_CalledReportVarSymTable.GetField(variableName);
		
			InsertIntoCtrlList(pF->GetDataType(), variableName);
		}
	}
}

//-----------------------------------------------------------------------------------------
void	CParamLinkDlg::ApplyExprParam()
{
	Expression* paramExpr = new Expression(&(m_pDocument->m_ViewSymbolTable));
	DataType dtExpr;
	
	if (m_idxItem >= 0)
	{
		dtExpr = DataType(m_listCtrlParams.GetItemText(m_idxItem, DATATYPE_POS));
	    
		//Se expr vuota cancello il paramentro
		CString strExp;
		m_editParamExpression.GetWindowText(strExp);
		if ( !strExp.IsEmpty() )
		{
			if (m_editParamExpression.CheckExp(*paramExpr, dtExpr))
			{
				m_listCtrlParams.SetItemText(m_idxItem, EXPR_POS, paramExpr->ToString());
				delete paramExpr;
			}
			else
			{			
				delete paramExpr;		//nuova espressione non valida, la elimino e non modifico la vecchia
				return;
			}
		}
		else 
		{	
			if (m_connType != WoormLink::ConnectionForm)
			{	//cancella il paramentro (e la param Expr creata prima)
				delete paramExpr;
				m_listCtrlParams.DeleteItem(m_idxItem);
			}
			else //non si puo cancellare
			  AfxMessageBox(_TB("Cannot delete connection parameter in a LinkForm"), MB_OK);  
		}
		//ricarico i parametri
	
		m_idxItem = -1;						//deselezione parametri connessione
		EnableParameterControls(FALSE);		//disabilito tasto apply, si riabilitera' alla prox selezione
	}
}

//-----------------------------------------------------------------------------
void	CParamLinkDlg::DeleteLink()
{	
	if (AfxMessageBox(_TB("Do you want remove the link?"), MB_YESNO) == IDYES)
		m_pDocument->m_arWoormLinks.RemoveAt( m_idxLink );

	m_pDocument->SetModifiedFlag();
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void	CParamLinkDlg::LoadOldLinkParam()
{
	m_listCtrlParams.DeleteAllItems();
	
	ASSERT (m_pConn);
	for (int i = 0; i <= m_pConn->m_pLocalSymbolTable->GetUpperBound(); i++)
	{
		WoormField* pItem = m_pConn->m_pLocalSymbolTable->GetAt(i);
		if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
			continue;

		InsertIntoCtrlList(pItem->GetDataType(), pItem->GetName(), pItem->GetInitExpression());
	}
}

//-----------------------------------------------------------------------------
void	CParamLinkDlg::LoadReportSymTable()
{
	CString strNsReport = m_pConn->m_strTarget;
	CTBNamespace nsReport(CTBNamespace::REPORT, m_pConn->m_strTarget);

	CString strReportPath = AfxGetPathFinder()->GetReportFullName(nsReport, AfxGetLoginInfos()->m_strUserName);
    
	//loading symbol table, from a report path file		
	m_CalledReportVarSymTable.RemoveAll();

	CString sError; int nLine = -1;
	BOOL bOk = m_CalledReportVarSymTable.ParseVariables(strReportPath, sError, nLine); //search T_VAR	
	if (!bOk)
	{
		//TODO show sError, nLine
		return;
	}
}

//-----------------------------------------------------------------------------
void	CParamLinkDlg::OnSelectParam(NMHDR* /*pNMHDR*/, LRESULT* /*pResult*/)
{
	POSITION pos = m_listCtrlParams.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		m_idxItem = m_listCtrlParams.GetNextSelectedItem(pos);
		CString strExpr = m_listCtrlParams.GetItemText(m_idxItem, EXPR_POS);
		m_editParamExpression.SetWindowText(strExpr);
		m_varInEdit.SetWindowText(m_listCtrlParams.GetItemText(m_idxItem, NAME_POS));
		EnableParameterControls(TRUE);
	}
}
//-----------------------------------------------------------------------------
void	CParamLinkDlg::EnableParameterControls (BOOL bEnable)
{
	m_btnApply.EnableWindow(bEnable);
	m_editParamExpression.EnableWindow(bEnable);
	if (!bEnable)
	{
		m_editParamExpression.SetWindowText(_T(""));
		m_varInEdit.SetWindowText(_T(""));
	}
}

//-----------------------------------------------------------------------------
void    CParamLinkDlg::EnableSelectionReport	(BOOL bEnable)
{
	//disabled report name and explorer button (in modify it cannot change them )
	m_editLinkName.EnableWindow	(bEnable);
	m_btnExplorer.EnableWindow	(bEnable);
}

//-----------------------------------------------------------------------------
BOOL    CParamLinkDlg::CheckParametersExpression ()
{
	BOOL emptyExprDetected = FALSE;
	
	for (int i = 0; i < m_listCtrlParams.GetItemCount(); i++)
	{
		CString strExp = m_listCtrlParams.GetItemText(i, EXPR_POS);
		if (strExp.IsEmpty() )
		{
			emptyExprDetected = TRUE;
			break;
		}
	}
	if (emptyExprDetected)//there are some expression empty
	{
		if (AfxMessageBox(_TB("There were detected variables without expression. Do you want remove them?"), MB_YESNO) == IDYES)    
		{
			for (int i = 0; i < m_listCtrlParams.GetItemCount(); i++)
			{
				CString strExp = m_listCtrlParams.GetItemText(i, EXPR_POS);
				if (strExp.IsEmpty() )
					m_listCtrlParams.DeleteItem(i);
			} 
			return TRUE;
		}
		else 
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void    CParamLinkDlg::SelectReportByExplorer()
{       
	//load report
	CTBNamespace aNamespace (m_pDocument->GetNamespace());
	aNamespace.SetType(CTBNamespace::REPORT);
	aNamespace.SetObjectName(_T(""));

	CTBExplorer aExplorer(CTBExplorer::OPEN, aNamespace);
	aExplorer.SetCanLink();
	if (!aExplorer.Open ())
		return;
	
	CString strPath;	
	aExplorer.GetSelPathElement(strPath);
	
	if (!strPath.IsEmpty())
	{
		CTBNamespace selNamespace;
		aExplorer.GetSelNameSpace(selNamespace);
		
		m_strDocNamespace = selNamespace.ToString();
		m_editLinkName.SetWindowText(m_strDocNamespace);		
	}
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::SelectDocumentByExplorer()
{
	CString sNs;
	CBaseDocumentExplorerDlg* pDocExplorer = AfxGetTBExplorerFactory()->CreateDocumentExplorerDlg();
	if (pDocExplorer->DoModal() == IDOK)
	{
		if (pDocExplorer->m_FullNameSpace.IsEmpty())
			ShowWrongNamespaceMsg();
		else
		{
			m_strDocNamespace = pDocExplorer->m_FullNameSpace;
			m_editLinkName.SetWindowText(m_strDocNamespace);
			m_listCtrlParams.DeleteAllItems();
			LoadFormParameters();
		}
	}
	SAFE_DELETE(pDocExplorer); 
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::SelectFunctionByDialog()
{
	CString sNs;
	CSelExtFunctDlg dlgFunct;
	if (dlgFunct.DoModal() == IDOK)
	{
		m_strDocNamespace = dlgFunct.m_sNamespace;
		CFunctionDescription* pFunc = dlgFunct.m_pFunc;

		m_editLinkName.SetWindowText(m_strDocNamespace);
		m_listCtrlParams.DeleteAllItems();
		LoadFunctionParameters(pFunc);
	}
}

//-----------------------------------------------------------------------------
void    CParamLinkDlg::SelectObjectByVariable()
{
	// loading string variables of report (variable with namespace)
	
	CSelectVarNsDlg dialog(m_pRepSymTable);
	if (dialog.DoModal() == IDOK)
	{
		m_strDocNamespace = dialog.GetSelectedVar();
		m_editLinkName.SetWindowText(m_strDocNamespace);
	}
}

//-----------------------------------------------------------------------------
BOOL CParamLinkDlg::InsertIntoCtrlList (DataType itemType, CString itemName, Expression* itemExpr /* = NULL*/, const CString& sVal)
{
	CString strDataType = cwsprintf(_T("%d"), MAKELONG(itemType.m_wType, itemType.m_wTag));
	CString strDescrType = FromDataTypeToDescr(itemType);

	int idxInsert = m_listCtrlParams.GetItemCount(); 
	m_listCtrlParams.InsertItem	(idxInsert, strDescrType);
	m_listCtrlParams.SetItemText(idxInsert, NAME_POS, itemName);
	
	if (itemExpr)
	{
		CString	strExpr	= itemExpr->ToString();
		strExpr.Replace(_T("\r\n"), _T(" "));
		strExpr.Replace(_T("\r"), _T(" "));
		strExpr.Replace(_T("\n"), _T(" "));
		m_listCtrlParams.SetItemText(idxInsert, EXPR_POS, strExpr);
	}
	else if (!sVal.IsEmpty())
		m_listCtrlParams.SetItemText(idxInsert, EXPR_POS, sVal);

	m_listCtrlParams.SetItemText(idxInsert, DATATYPE_POS, strDataType);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CParamLinkDlg::AddConnection ()
{ 
    //aggiungere connessione.
	ASSERT_VALID(m_pConn);

	m_pConn->m_LinkType = m_connType;
	m_pConn->m_SubType = m_subType;
	m_pConn->m_bLinkTargetByField = this->m_bLinkTargetByField;

	m_pDocument->m_arWoormLinks.Add(m_pConn);
}

//-----------------------------------------------------------------------------
BOOL	CParamLinkDlg::CheckNamespace(CTBNamespace ns)
{
	if (!ns.IsValid())
		return FALSE;

	if (ns.GetType() == CTBNamespace::DOCUMENT)
	{
		AddOnModule* pAddOnMod = AfxGetAddOnModule(ns);
		if (!pAddOnMod)
			return FALSE;

		const CDocumentDescription* pDocInfo = AfxGetDocumentDescription(ns);
		if (!pDocInfo)
			return FALSE;
	}
	else if (ns.GetType() == CTBNamespace::FUNCTION)
	{
		return TRUE; //lo verifica dopo
	}
	else if (ns.GetType() == CTBNamespace::REPORT)
	{
		CString strFileName = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
		if (!ExistFile(strFileName))
			return FALSE;
	}
	return TRUE;
}

//==============================================================================
//          Class CSelectReportParamDlg implementation
//==============================================================================

IMPLEMENT_DYNAMIC(CSelectReportParamDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CSelectReportParamDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CSelectReportParamDlg)
	 ON_BN_CLICKED		(IDC_BTN_PARAM_ADD,					AddSelection)
	 ON_BN_CLICKED		(IDC_BTN_PARAM_REMOVE,				RemoveSelection)
	 ON_LBN_DBLCLK		(IDC_LIST_PARAM,					AddSelection)
	 ON_LBN_DBLCLK	    (IDC_LIST_SELECTED_PARAM,			RemoveSelection)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CSelectReportParamDlg::CSelectReportParamDlg (CString strReportNS, WoormTable* pVarSymTable, CStringArray* parVariablesOldSelected, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog					(IDD_PARAM_SEL, aParent),
	m_strReportNS					(strReportNS),
	m_pVarSymTable					(pVarSymTable),
	m_parVariablesOldSelected		(parVariablesOldSelected)
{

}

//-----------------------------------------------------------------------------
BOOL	CSelectReportParamDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	this->SetWindowText(m_strReportNS);

	m_lbReportVariables.		SubclassDlgItem		(IDC_LIST_PARAM,			this);
	m_lbSelectedVariables.		SubclassDlgItem		(IDC_LIST_SELECTED_PARAM,	this);
	m_addParam.					SubclassDlgItem		(IDC_BTN_PARAM_ADD,			this);
	m_removeParam.				SubclassDlgItem		(IDC_BTN_PARAM_REMOVE,		this);

	//load all variables in SymbolTable
	for (int i = 0; i < m_pVarSymTable->GetSize(); i++ )
	{
		WoormField* pF = (WoormField*) m_pVarSymTable->GetAt(i);
		m_lbReportVariables.AddString( pF->GetName());
		
	}

	//show variables already selected
	for (int i = 0; i < m_parVariablesOldSelected->GetSize() ; i++)
		m_lbSelectedVariables.AddString(m_parVariablesOldSelected->GetAt(i));
	
	return TRUE;
}


//-----------------------------------------------------------------------------
void CSelectReportParamDlg::OnOK()
{
	CString strVar;
	m_arStrSelectedVariables.RemoveAll();
	for (int i = 0; i < m_lbSelectedVariables.GetCount(); i++)
	{
		m_lbSelectedVariables.GetText(i, strVar);
		if (!ParamAlreadySelected(strVar))   //strVar not already selected previously
			m_arStrSelectedVariables.Add(strVar);
	}
	if ((m_arStrSelectedVariables.GetSize() < 1)&& (m_parVariablesOldSelected->GetSize() < 1))
		AfxMessageBox(_TB("Select one or more variables"), MB_OK | MB_ICONEXCLAMATION);
	else
		EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void	CSelectReportParamDlg::OnCancel()
{

	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CSelectReportParamDlg::AddSelection()
{
	int nCount = m_lbReportVariables.GetSelCount();

	CArray<int,int> arSelections;
	arSelections.SetSize(nCount);
	m_lbReportVariables.GetSelItems(nCount, arSelections.GetData());

	CStringArray arSelected;
	CString s;
	for (int i=0; i <= arSelections.GetUpperBound(); i++)
	{
		m_lbReportVariables.GetText(arSelections.GetAt(i), s);
		arSelected.Add(s);
	}

	CheckSelectionsToAdd (arSelected); //delete variables already selected


	for (int i=0; i <= arSelected.GetUpperBound(); i++)
		m_lbSelectedVariables.AddString(arSelected.GetAt(i));
}

//-----------------------------------------------------------------------------
void CSelectReportParamDlg::CheckSelectionsToAdd (CStringArray& arSelections)
{
	// elimino le tabelle che sono già presenti nelle selezionate
	CString sSel;
	for (int i = arSelections.GetUpperBound(); i >= 0; i--)
	{
		sSel = arSelections.GetAt(i);
		if (m_lbSelectedVariables.FindStringExact(0, sSel) >= 0)
			arSelections.RemoveAt(i);
	}
}


//-----------------------------------------------------------------------------
void CSelectReportParamDlg::RemoveSelection()
{
	int nCount = m_lbSelectedVariables.GetSelCount();
	CArray<int,int> arSelections;
	arSelections.SetSize(nCount);
	m_lbSelectedVariables.GetSelItems(nCount, arSelections.GetData());

	for (int i = arSelections.GetUpperBound(); i >= 0; i--)
	{	
	  CString varToDelete;
	  m_lbSelectedVariables.GetText(arSelections.GetAt(i), varToDelete);
	
	  if (!ParamAlreadySelected(varToDelete))
			 m_lbSelectedVariables.DeleteString(arSelections.GetAt(i));
	}
}

//-----------------------------------------------------------------------------------------
BOOL CSelectReportParamDlg::ParamAlreadySelected (CString strVarName)
{
	//ciclo sugli elemnti del listbox..se trovo la variabile return true
	for (int i = 0; i < m_parVariablesOldSelected->GetSize() ; i++)
		if (strVarName.Compare(m_parVariablesOldSelected->GetAt(i)) == 0 )
			return TRUE;

	return FALSE;
}


//==============================================================================
//          Class CSelectVarNsDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CSelectVarNsDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CSelectVarNsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CSelectVarNsDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CSelectVarNsDlg::CSelectVarNsDlg (WoormTable*	pRepSymTable, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog					(IDD_DLG_NS_VAR, aParent),
	m_pRepSymTable					(pRepSymTable)
{

}

//-----------------------------------------------------------------------------
BOOL	CSelectVarNsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_lbReportStringVariables.		SubclassDlgItem		(IDC_LIST_STR_NS_VAR,			this);
	
	//load all string variables from SymbolTable
	for (int i = 0; i < m_pRepSymTable->GetSize(); i++ )
	{
		WoormField* pF = (WoormField*) m_pRepSymTable->GetAt(i);
		if ( pF->GetDataType() == DataType::String)
			m_lbReportStringVariables.AddString( pF->GetName() );
	}	
	return TRUE;
}

//-----------------------------------------------------------------------------
void	CSelectVarNsDlg::OnOK()
{
	int idxSel = m_lbReportStringVariables.GetCurSel();
	if (idxSel < 0)
	{
		AfxMessageBox(_TB("Select a variable"), MB_OK);
		return;
	}
	else
	   m_lbReportStringVariables.GetText(idxSel, m_strSelectedVar);
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void	CSelectVarNsDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
CString CSelectVarNsDlg::GetSelectedVar()
{
	return m_strSelectedVar;
}



//==============================================================================
//          Class CAddParamDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CAddParamDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CAddParamDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAddParamDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CAddParamDlg::CAddParamDlg ( CWnd* aParent /*=NULL*/)
	:
	CParsedDialog					(IDD_DLG_ADD_PARAM, aParent)
{

}

//-----------------------------------------------------------------------------
BOOL	CAddParamDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	m_edtParamName.				SubclassEdit	(IDC_EDIT_PARAM_NAME,			 this);
	m_cbDataObjTypes.			SubclassEdit	(IDC_CBX_PARAM_TYPE,			 this);

	m_cbDataObjTypes.FillListBox();
	m_cbDataObjTypes.SetTypeValue(DATA_STR_TYPE);
	return TRUE;
}

//-----------------------------------------------------------------------------
void	CAddParamDlg::OnOK()
{
	m_edtParamName.GetWindowText(m_strNameParam);
	m_dtParam = m_cbDataObjTypes.GetTypeValue();
	
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void	CAddParamDlg::OnCancel()
{

	EndDialog(IDCANCEL);
}


//==============================================================================
//          Class CAllLinksColsDlg implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CAllLinksColsDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CAllLinksColsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAllLinksColsDlg)
		ON_LBN_DBLCLK       (IDC_LIST_LINKS,			EditColumnLinks)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CAllLinksColsDlg::CAllLinksColsDlg (CColumnArray* pArrayColumnWithLink, CWoormDocMng*	pDocument, Table* pTable, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog			(IDD_LIST_LINKS, aParent),
	m_pArrayColumnWithLink	(pArrayColumnWithLink),
	m_pDocument				(pDocument),
	m_pTable				(pTable)
{

}


//-----------------------------------------------------------------------------
BOOL CAllLinksColsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
   	CString labelColumn;

	m_lbColumns.SubclassDlgItem(IDC_LIST_LINKS	,	this);
	
	for (int nCol= 0; nCol <= m_pArrayColumnWithLink->GetUpperBound(); nCol++)
	{
		TableColumn* col = (TableColumn*)m_pArrayColumnWithLink->GetAt(nCol);
		labelColumn = col->GetTitle();
		if (labelColumn.IsEmpty())
		{
			SymField* rp =  m_pDocument->m_pEditorManager->GetSymTable()->GetFieldByID (col->GetInternalID());
			
			labelColumn = _T("(");
			labelColumn.Append(rp->GetName()); 
			labelColumn.Append(_T(")"));
		}
		m_lbColumns.AddString(labelColumn);
	 }
	
    return TRUE;
}


//-----------------------------------------------------------------------------
void CAllLinksColsDlg::OnOK()
{
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CAllLinksColsDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CAllLinksColsDlg::EditColumnLinks()
{
	int selected = m_lbColumns.GetCurSel();
	CString selectedString;
	m_lbColumns.GetText(selected, selectedString);
	
	for (int nCol = 0; nCol <= m_pTable->LastColumn(); nCol++)
	{
		if (selectedString.CompareNoCase((m_pTable->GetColumn(nCol))->GetTitle()) == 0)
		{
			m_pTable->OnColumnManageLink(nCol);
			return;
		}
		selectedString.Trim(_T("()"));

		SymField* rp = m_pDocument->m_pEditorManager->GetSymTable()->GetFieldByID ((m_pTable->GetColumn(nCol))->GetInternalID());  
		if (selectedString.CompareNoCase(rp->GetName()) == 0)
		{
			m_pTable->OnColumnManageLink(nCol);
			return;
		}
	}
}
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CSelExtFunctDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CSelExtFunctDlg, CParsedDialog)
	ON_LBN_SELCHANGE	(IDC_EQNEDIT_EXT_FUNCTSLB,	SelChangeExtFunct)
	ON_LBN_DBLCLK		(IDC_EQNEDIT_EXT_FUNCTSLB,	SelExtFunct)
	ON_CBN_SELCHANGE	(IDC_EQNEDIT_EXT_COMBOAPP,	OnComboAppChanged)
	ON_CBN_SELCHANGE	(IDC_EQNEDIT_EXT_COMBOMOD,	OnComboModChanged)
	//ON_LBN_DBLCLK	(IDC_EQNEDIT_EXT_FUNCTSLB,	SelExtFunct)

END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CSelExtFunctDlg::CSelExtFunctDlg()
	:
	CParsedDialog	(IDD_DLG_SEL_EXTFUNCT, NULL),
	m_pFunc         (NULL)
{
}

//----------------------------------------------------------------------------
BOOL CSelExtFunctDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	EnableToolTips();

	m_lbExtFuncts.		SubclassDlgItem	(IDC_EQNEDIT_EXT_FUNCTSLB,	this);
	m_cbSelApplication.	SubclassDlgItem	(IDC_EQNEDIT_EXT_COMBOAPP,	this);
	m_cbSelModule.		SubclassDlgItem	(IDC_EQNEDIT_EXT_COMBOMOD,	this);
	m_edtShowDescr.		SubclassEdit	(IDC_EQNEDIT_EXT_EDITDESCR,	this);

	CEqnEditDlg::FillAppCombo(m_cbSelApplication, m_cbSelModule, m_arAppItemLoc);

	CEqnEditDlg::ShowModules(m_cbSelApplication, m_cbSelModule, m_lbExtFuncts, m_arModItemLoc);

	return TRUE;
}

//----------------------------------------------------------------------------
void CSelExtFunctDlg::OnComboAppChanged()
{
	m_edtShowDescr.SetWindowText(_T(""));

	CEqnEditDlg::ShowModules(m_cbSelApplication, m_cbSelModule, m_lbExtFuncts, m_arModItemLoc);
}

//----------------------------------------------------------------------------
void CSelExtFunctDlg::OnComboModChanged()
{
	m_edtShowDescr.SetWindowText(_T(""));

	CEqnEditDlg::ShowFunctions(m_cbSelApplication, m_cbSelModule, m_lbExtFuncts);
}

//----------------------------------------------------------------------------
void CSelExtFunctDlg::SelChangeExtFunct()
{
	m_edtShowDescr.SetWindowText(CEqnEditDlg::SelChangeExtFunct(m_lbExtFuncts));
}	

//----------------------------------------------------------------------------
void CSelExtFunctDlg::SelExtFunct ()
{

}

//----------------------------------------------------------------------------
void CSelExtFunctDlg::OnCancel ()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CSelExtFunctDlg::OnOK ()
{
	int idx = m_lbExtFuncts.GetCurSel();
	if (idx >= 0)
	{
		m_pFunc = (CFunctionDescription*) m_lbExtFuncts.GetItemDataPtr(idx);
		if (m_pFunc)
		{
			m_sNamespace = m_pFunc->GetNamespace().ToString();

			EndDialog(IDOK);
		}
	}
}

#endif