#pragma once

#include <TbGenlib\Parsobj.h>
#include <TbGenlib\Parsedt.h>

//includere alla fine degli include del .H
#include "beginh.dex"
/////////////////////////////////////////////////////////////////////////////


class CMailConfigurationDlg : public CParsedDialog
{
public:
	CMailConfigurationDlg(CWnd* pParent = NULL);   // standard constructor

//	CComboBox	m_ctrlIPAddresses;
	CBCGPEdit	m_ctrlUsername;
	CBCGPEdit	m_ctrlPassword;
	CString	m_sAddress;
	CString	m_sHost;
	CString	m_sName;

	int		m_nPort;
	CComboBox m_cbxPortNumber;

	CString	m_sPassword;
	CString	m_sUsername;
	BOOL	m_bMime;
	CString	m_sReplyToAddress;
	CString	m_sReplyToName;
 	int		m_nTimeout;
	DWORD	m_Priority;

	BOOL	m_bHTML;
	CButton	m_btnHtml;

	BOOL	m_bExplicitSSL;
 	BOOL	m_bImplicitSSL;
	CButton	m_btnExplicitSSL;
	CButton	m_btnImplicitSSL;

 	BOOL	m_bAccountDefault;
 	BOOL	m_bAccountCertified;
	CButton	m_btnAccountDefault;
	CButton	m_btnAccountCertified;

	CLabelStatic m_LineSep;

	CString	m_sAuthenticationType;
	CComboBox m_cbxAuthenticationType;

	int			m_nSecurityProtocolType;
	CString		m_sSecurityProtocolType;
	CComboBox	m_cbxSecurityProtocolType;
	
	CString	m_sConfiguration;
	short	m_nConfiguration;
	CComboBox m_cbxConfiguration;

	SmtpMailConnectorParams m_settings;

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	afx_msg void OnHtml();
	afx_msg void OnExplicitSSL();
	afx_msg void OnImplicitSSL();
	afx_msg void OnChooseSenderAddress();
	afx_msg void OnChooseReplyToAddress();
	afx_msg void OnClickAccount();

	virtual BOOL	OnInitDialog		();
	virtual void	OnOK				();
	afx_msg void	OnApply				();
	afx_msg void	OnTest				();

	void LoadSettings(BOOL bUpdate);
	void SaveSettings();

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CMailConfigurationDlg)
};

//////////////////////////////////////////////////////////////////////////////////////
#include "endh.dex"

