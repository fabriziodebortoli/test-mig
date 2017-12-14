#include "stdafx.h"

#include "rectobj.h"
#include "table.h"

#include "WoormInfoViewer.h"
#include "WoormInfoViewer.hjson" //JSON AUTOMATIC UPDATE

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
/////////////////////////////////////////////////////////////////////////////
// 				class WoormInfoViewer Implementation
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(WoormInfoViewer, CParsedDialog)
BEGIN_MESSAGE_MAP(WoormInfoViewer, CParsedDialog)
	//{{AFX_MSG_MAP(WoormInfoViewer)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
WoormInfoViewer::WoormInfoViewer(CWoormInfo* pWoormInfo, SymTable* pSymTable)
	:
	CParsedDialog		(IDD_WOORMVIEWERDLG),
	m_pWoormInfo		(pWoormInfo),
	m_pSymTable			(pSymTable)
{
	ASSERT_VALID(pSymTable);
}

//----------------------------------------------------------------------------
WoormInfoViewer::~WoormInfoViewer()
{
}

//----------------------------------------------------------------------------
BOOL WoormInfoViewer::OnInitDialog()
{
	BOOL bRet = CParsedDialog::OnInitDialog();

	VERIFY(m_ReportList.SubclassDlgItem	(IDC_LISTWOORMPARAM, this));

	m_ReportList.InsertColumn(0, _TB("Name"),		LVCFMT_LEFT, 230, 0);
	m_ReportList.InsertColumn(1, _TB("Type"),		LVCFMT_LEFT, 100, 0);
	m_ReportList.InsertColumn(2, _TB("Value"),		LVCFMT_LEFT,  80, 0);
	m_ReportList.InsertColumn(3, _TB("Direction"),	LVCFMT_LEFT,  80, 0);
	if (m_pSymTable)
		m_ReportList.InsertColumn(4, _TB("Binded"), LVCFMT_LEFT, 80, 0);

	FillReportList();
	// Posiziono la dialog al centro dello schermo
	CenterWindow();	

	return bRet;
}

//----------------------------------------------------------------------------
void WoormInfoViewer::FillReportList()
{
	CString			strName	;
	CString			strType	;
	CString			strValue;
	CString			strDir	;
	CString			strBinded;

	m_ReportList.SetRedraw(FALSE);		
	m_ReportList.DeleteAllItems();
	m_ReportList.SetRedraw(TRUE);
	m_ReportList.Invalidate(FALSE);

	m_ReportList.SetRedraw(FALSE);
	//if (!m_ReportList.GetImageList(LVSIL_NORMAL))
	//	m_ReportList.SetImageList(&m_ImageList, LVSIL_SMALL);	
	if (!m_pWoormInfo)
		return;

	for (int i = 0; i <= m_pWoormInfo->GetParameters().GetUpperBound(); i++)
	{
		CDataObjDescription* parName = (CDataObjDescription*) m_pWoormInfo->GetParameters().GetAt(i);
		strName		= parName->GetName();
		strType		= parName->ToString(parName->GetDataType());
		strValue	= parName->GetValue()->Str();

		switch (parName->GetPassedMode())
		{
			case (CDataObjDescription::_IN):
				strDir = _T("In");
				break;
			case (CDataObjDescription::_INOUT):
				strDir = _T("In/Out");
				break;
			case (CDataObjDescription::_OUT):
				strDir = _T("Out");
				break;
		}
		
		m_ReportList.InsertItem	(i, strName);
		m_ReportList.SetItemText(i, 1, strType);

		m_ReportList.SetItemText(i, 2, strValue);
		m_ReportList.SetItemText(i, 3, strDir);

		if (m_pSymTable)
		{
			SymField* pF = m_pSymTable->GetField(strName);

			if (pF)
			{
				strBinded = _TB("yes");

				if (!DataType::IsCompatible(parName->GetDataType(), pF->GetDataType()))
					strBinded = _TB("ERROR:it has incompatible data type");

				m_ReportList.SetItemText(i, 4, strBinded);
			}
		}
	}
	
	m_ReportList.SetRedraw(TRUE);
	m_ReportList.Invalidate(FALSE);
}

/////////////////////////////////////////////////////////////////////////////
// 				class ObjectsList Implementation
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ObjectsList, CParsedDialog)
BEGIN_MESSAGE_MAP(ObjectsList, CParsedDialog)
	//{{AFX_MSG_MAP(ObjectsList)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
ObjectsList::ObjectsList(CWoormDocMng* pWoorm)
	:
	CParsedDialog	(IDD_WOORMVIEWERDLG),
	m_pWoorm		(pWoorm)
{
}

//----------------------------------------------------------------------------
ObjectsList::~ObjectsList()
{
}

//----------------------------------------------------------------------------
BOOL ObjectsList::OnInitDialog()
{
	BOOL bRet = CParsedDialog::OnInitDialog();
	SetWindowText(_TB("Objects list of current layout"));

	VERIFY(m_ReportList.SubclassDlgItem	(IDC_LISTWOORMPARAM, this));

	m_ReportList.InsertColumn(0, _TB("Name"),		LVCFMT_LEFT, 230, 0);
	m_ReportList.InsertColumn(1, _TB("Type"),		LVCFMT_LEFT, 100, 0);
	m_ReportList.InsertColumn(2, _TB("(top, left, bottom, right)"),		LVCFMT_LEFT,  200, 0);
	m_ReportList.InsertColumn(3, _TB("Attributes"),		LVCFMT_LEFT,  100, 0);
	
	FillReportList();
	// Posiziono la dialog al centro dello schermo
	CenterWindow();	

	return bRet;
}

//----------------------------------------------------------------------------
void ObjectsList::FillReportList()
{
	if (!m_pWoorm) return;

	CString			strName;
	CString			strType;
	CString			strPosition;
	CString			strAttributes;

	m_ReportList.SetRedraw(FALSE);		
	m_ReportList.DeleteAllItems();
	m_ReportList.SetRedraw(TRUE);
	m_ReportList.Invalidate(FALSE);

	m_ReportList.SetRedraw(FALSE);

	for (int i = 2; i <= m_pWoorm->GetObjects().GetUpperBound(); i++)
	{
		BaseObj* pObj = m_pWoorm->GetObjects()[i];

		strName		= pObj->GetDescription();
		strType		= pObj->GetRuntimeClass()->m_lpszClassName;
		strPosition	= cwsprintf(
						_T("(%4d,%4d,%4d,%4d)"), 
						pObj->GetBaseRect().top, pObj->GetBaseRect().left,
						pObj->GetBaseRect().bottom, pObj->GetBaseRect().right
						);
		strAttributes.Empty();
		if (pObj->IsTransparent())
			strAttributes += _TB("Trasparent") + ' ';
		if (pObj->IsAlwaysHidden())
			strAttributes += _TB("Hidden") + ' ';

		m_ReportList.InsertItem	(i - 2, strName);
			m_ReportList.SetItemText(i - 2, 1, strType);
			m_ReportList.SetItemText(i - 2, 2, strPosition);
			m_ReportList.SetItemText(i - 2, 3, strAttributes);
	}
	
	m_ReportList.SetRedraw(TRUE);
	m_ReportList.Invalidate(FALSE);
}


