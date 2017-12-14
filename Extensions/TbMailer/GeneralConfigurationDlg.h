#pragma once

#include <TbGenlib\Parsobj.h>
#include <TbGenlib\Parscbx.h>

//includere alla fine degli include del .H
#include "beginh.dex"
/////////////////////////////////////////////////////////////////////////////

//===========================================================================
class CGeneralConfigurationDlg : public CParsedDialog
{
public:
	CGeneralConfigurationDlg(CWnd* pParent = NULL);   // standard constructor

	CBCGPEdit	m_ctrlOutlookProfile;
	CString m_sOutlookProfile;

	BOOL	m_bSupportOutlookExpress;

	BOOL	m_bMailCompress;

	BOOL	m_bCryptFile;

	CBCGPEdit	m_ctrlPassword;
	CString	m_sPassword;

	/*MailConnectorParams::MailProtocol*/ int m_nProtocol;

	BOOL	m_bRequestDeliveryNotification;
	BOOL	m_bRequestReadNotification;

	CString	m_sReplyToAddress;

	CString	m_sTrackingAddress;
	CString	m_sPrinterTemplate;
	CString	m_sFaxFormatTemplate;

	CComboBoxExt	m_PrintersCombo;

protected:
	BOOL	OnInitDialog	();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	void OnChooseProtocol ();
	void OnChooseReplyToAddress ();
	void OnChooseCryptFile ();
	void OnChooseTrackingAddress ();
		
	void LoadPrinters	();

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CGeneralConfigurationDlg)
};

//////////////////////////////////////////////////////////////////////////////////////
#include "endh.dex"

