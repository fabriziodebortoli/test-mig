#include "stdafx.h"

#include <TbGenlib\BarCode.h>

#include "CDDMS.h"
#include "CommonObjects.h"
#include "TbRepositoryManager.h"
#include "UIPaperyDlg.h"

#include "EasyAttachment\JsonForms\UIPaperyDlg\IDD_PAPERY.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace System::Collections::Generic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::EasyAttachment::Core;
using namespace Microarea::TBPicComponents;

/////////////////////////////////////////////////////////////////////////////
//					class CPaperyParsedDlg Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CPaperyParsedDlg, CParsedDialogWithTiles)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPaperyParsedDlg, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP(CPaperyParsedDlg)
	ON_EN_VALUE_CHANGED(IDC_PAPERY_BARCODE_VALUE, OnBarcodeValueChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPaperyParsedDlg::CPaperyParsedDlg(CWnd* pParent, CDDMS* pDMSClientDoc)
	:
	CParsedDialogWithTiles(IDD_PAPERY_VIEW, pParent, _NS_DLG("Extensions.TbDMS.Documents.PaperyDlg")),
	m_pDMSClientDoc(pDMSClientDoc)
{
	AttachDocument(m_pDMSClientDoc->GetServerDoc());
}

//-----------------------------------------------------------------------------
BOOL CPaperyParsedDlg::OnInitDialog()
{
	CParsedDialogWithTiles::OnInitDialog();

	SetToolbarStyle(ToolbarStyle::BOTTOM, 32, TRUE, TRUE);

	m_pDMSClientDoc->m_sPaperyBarcodeValue.Clear();
	m_pDMSClientDoc->m_sPaperyNotes.Clear();
	m_pDMSClientDoc->m_sPaperyBarcodeValue.SetAlwaysEditable();
	m_pDMSClientDoc->m_sPaperyNotes.SetAlwaysEditable();

	// Posiziono la dialog al centro dello schermo
	CenterWindow();

	OnUpdateControls();
	return TRUE;
}

//-----------------------------------------------------------------------------
void CPaperyParsedDlg::OnOK()
{
	CheckForm();

	if (!AfxGetTbRepositoryManager()->IsValidEABarcodeValue(m_pDMSClientDoc->m_sPaperyBarcodeValue))
	{
		m_pDMSClientDoc->m_sPaperyBarcodeValue.Clear();
		OnUpdateControls();
		return;
	}

	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CPaperyParsedDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CPaperyParsedDlg::OnBarcodeValueChanged()
{
	OnUpdateControls();
}