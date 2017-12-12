
#pragma once

#include <TBGENLIB\basedoc.h>
#include <TBGENLIB\parsedt.h>
#include <TBGES\tabber.h>
#include <TBGES\extdoc.h>
#include <TBGES\extdocview.h>
#include <TbGenLibManaged\HelpManager.h>
#include "TbSenderInterface.h"
#include "PostaliteSettings.h"
#include "PostaliteTables.h"

#include <TbGenLibManaged\PostaliteNet.h>
#include <TbGeneric\CMapi.h>
#include <TbClientCore\ClientObjects.h>
#include <TBGENLIB\baseapp.h>
#include <TbWoormViewer\SoapFunctions.h>
#include <TbGenLibManaged\GlobalFunctions.h>
#include <TbGes\Tabber.h>
#include <TbGeneric\VisualStylesXP.h>

//includere alla fine degli include del .H
#include "beginh.dex"

static const TCHAR szHelpNamespace[] = _T("RefGuide-Extensions-TbMailer-TbMailer-PostaLiteAccountManagement");

//--------------------------------------------------------------------------
class CPostaliteAccountManagementWizardDoc : public CWizardFormDoc
{
	DECLARE_DYNCREATE(CPostaliteAccountManagementWizardDoc)

public:
	CPostaliteAccountManagementWizardDoc();
	~CPostaliteAccountManagementWizardDoc(){}

public:
	virtual BOOL OnOpenDocument(LPCTSTR);
	virtual BOOL OnAttachData		();
			BOOL Subscribe();
			void Logout();
			void ViewCredit(DataInt& nCodeState, DataMon& currentCredit, DataDate& expiryDate, DataStr& errorMsg);
			BOOL Login(DataStr& strPassword);
			BOOL CheckSubscribeData();
			BOOL CheckSenderData();
			void ClearSenderData();
			void ClearWireTransferData();
			void SetDefaultAddresser();
			CCompanyAddressInfo* GetCompanyData();
};

//=============================================================================
class TB_EXPORT CPostaliteAccountManagementWizardFrame : public CWizardStepperBatchFrame
{
	DECLARE_DYNCREATE(CPostaliteAccountManagementWizardFrame)
public:	
	CPostaliteAccountManagementWizardFrame();

protected:
	//{{AFX_MSG(CPostaliteAccountManagementWizardFrame)             
	afx_msg void OnGetMinMaxInfo	(MINMAXINFO* lpMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------
class CPostaliteAccountManagementWizardView : public CWizardFormView
{
	DECLARE_DYNCREATE(CPostaliteAccountManagementWizardView)

public:	
	CPostaliteAccountManagementWizardView(){}
	~CPostaliteAccountManagementWizardView();

private:
	CPostaliteAccountManagementWizardDoc* GetPostaliteAccountManagementWizardDoc	() const;
	
public:
	virtual	void CustomizeTabWizard	(CTabManager* pTabManager);
	virtual LRESULT	OnWizardNext	(UINT nIDD);
	virtual void OnInitialUpdate	();

protected:
	//{{AFX_MSG(CPostaliteAccountManagementWizardView)             
		afx_msg	void OnWizardFinish();
		afx_msg	void OnWizardCancel();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardMainPage dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardMainPage : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardMainPage)
private:
	
	// Construction
public:
	CPostaLiteWizardMainPage();   // standard constructor
	~CPostaLiteWizardMainPage();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument();}

private:
	void UpdateControls();
	void OnLogout();
	void OnGoToCreditMng();
	void OnSave();
	void OnShowHelp();

protected:
	virtual LRESULT	OnWizardNext();
	virtual void BuildDataControlLinks();

protected:
	//{{AFX_MSG(CPostaLiteWizardNotLoggedPage)
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardNotLoggedPage dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardNotLoggedPage : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardNotLoggedPage)
private:
	DataBool m_bLoginRadio;
	DataBool m_bSubscribeRadio;

public:
	CPostaLiteWizardNotLoggedPage();   // standard constructor
	~CPostaLiteWizardNotLoggedPage();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument();}

private:
	void UpdateControls();
	protected:
	virtual LRESULT	OnWizardNext();
	virtual void BuildDataControlLinks();

protected:
	//{{AFX_MSG(CPostaLiteWizardNotLoggedPage)
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardLoggedPage dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardLoggedPage : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardLoggedPage)
private:
	DataBool m_bIsDirty;
	DataBool m_bLoginRadio;
	DataBool m_bLogout;
	DataBool m_bGoToCredit;

// Construction
public:
	CPostaLiteWizardLoggedPage();   // standard constructor
	~CPostaLiteWizardLoggedPage();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }

private:
	void UpdateControls();
	void OnLogout();
	void OnGoToCreditMng();
	void OnChanged();

protected:
	virtual LRESULT	OnWizardNext();
	virtual void BuildDataControlLinks();

protected:
	//{{AFX_MSG(CPostaLiteWizardNotLoggedPage)
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardLoginPage dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardLoginPage : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardLoginPage)

private:
	DataStr m_strPassword;

// Construction
public:
	CPostaLiteWizardLoginPage();   // standard constructor
	~CPostaLiteWizardLoginPage();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }

private:
	void UpdateControls();

protected:
	virtual LRESULT	OnWizardNext	();
	virtual void BuildDataControlLinks		();

protected:
	//{{AFX_MSG(CPostaLiteWizardLoginPage)
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage1 dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardSubscribePage1 : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardSubscribePage1)

private:
	DataBool	m_bLoadCompanyDefaultsButton;
	DataBool	m_bGetCadastralCodeButton;
	
private:
	void UpdateControls			();
	BOOL CheckSubscribeData		();
	void OnLoadCompanyDetails	();
	void OnGetCadastralCode		();

// Construction
public:
	CPostaLiteWizardSubscribePage1();   // standard constructor
	~CPostaLiteWizardSubscribePage1();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }

protected:
	virtual LRESULT	OnWizardNext	();
	virtual LRESULT	OnWizardCancel	();
	virtual void BuildDataControlLinks		();

protected:
	//{{AFX_MSG(CPostaLiteWizardSubscribePage1)
			void OnCityChanged();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage2 dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardSubscribePage2 : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardSubscribePage2)
private:
	DataBool m_bPublicEntity;
	DataBool m_bNotPublicEntity;
	DataBool m_bSenderPublicEntity;
	DataBool m_bSenderNotPublicEntity;
	
// Construction
public:
	CPostaLiteWizardSubscribePage2();   // standard constructor
	~CPostaLiteWizardSubscribePage2();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }

private:
	void UpdateControls();

protected:
	virtual LRESULT	OnWizardNext	();
	virtual LRESULT	OnWizardCancel	();
	virtual LRESULT OnWizardBack	();
	virtual void BuildDataControlLinks		();

protected:
	//{{AFX_MSG(CPostaLiteWizardSubscribePage2)
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage3 dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardSubscribePage3 : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardSubscribePage3)

private:
	DataBool m_bLoadSenderCompanyDefaultsButton;
	
private:
	void UpdateControls		();
	BOOL CheckSenderData	();
	void OnLoadSenderCompanyDetails();
// Construction
public:
	CPostaLiteWizardSubscribePage3();   // standard constructor
	~CPostaLiteWizardSubscribePage3();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }

protected:
	virtual LRESULT	OnWizardNext	();
	virtual LRESULT	OnWizardCancel	();
	virtual void BuildDataControlLinks		();

protected:
	//{{AFX_MSG(CPostaLiteWizardSubscribePage3)
			void OnCityChanged();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage4 dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardSubscribePage4 : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardSubscribePage4)

private:
	DataBool m_bAcceptTos;
	DataBool m_bAcceptRestrictiveClauses;
	DataBool m_bAcceptPrivayPolicy;

// Construction
public:
	CPostaLiteWizardSubscribePage4();   // standard constructor
	~CPostaLiteWizardSubscribePage4();

	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }
private:
	void UpdateControls();

protected:
	virtual LRESULT	OnWizardNext	();
	virtual LRESULT	OnWizardCancel	();
	virtual void BuildDataControlLinks		();

protected:
	//{{AFX_MSG(CPostaLiteWizardSubscribePage4)
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MS
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardCreditPage dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteWizardCreditPage : public CWizardTabDialog
{
	DECLARE_DYNCREATE(CPostaLiteWizardCreditPage)

private:
	DataInt			m_nCodeState;
	DataMon			m_TempChargeAmount;
	DataMon			m_CurrentCredit;
	DataStr			m_strFileName;
	DataStr			m_strCodeStateString;
	DataDate		m_CreditExpiryDate;
	DataBool		m_bViewCurrentCredit;
	DataBool		m_bBrowseFile;
	DataBool		m_bChargeFile;
	DataBool		m_bModify;
	DataBool		m_bAcceptRechargeCondition;
	DataBool		m_bGetWireTransferDetails;
	DataBool		m_bResetWireTransferDetails;

public:
	CPostaLiteWizardCreditPage();   
	~CPostaLiteWizardCreditPage();
	CPostaliteAccountManagementWizardDoc* GetWizardDoc() { return (CPostaliteAccountManagementWizardDoc*)GetDocument(); }

private:
	void UpdateControls();
	void ViewCredit();

protected:
	virtual LRESULT	OnWizardNext();
	virtual LRESULT	OnWizardBack();
	virtual void BuildDataControlLinks		();

protected:
	//{{AFX_MSG(CPostaLiteWizardCreditPage)
			void OnViewCredit();
			void OnCharge();
			void OnBrowse();
			void OnGetWireTransferDetails();
			void OnClearWireTransferDetails();
			void OnChargeAmountChanged();

	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo (HELPINFO* pHelpInfo);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
//			Class CPostaLiteMoneyEdit
//=============================================================================
class TB_EXPORT CPostaLiteMoneyEdit  : public CMoneyEdit
{
	DECLARE_DYNCREATE (CPostaLiteMoneyEdit)

public:
	// Construction
	CPostaLiteMoneyEdit();

protected:
	//{{AFX_MSG(CPostaLiteMoneyEdit)
	afx_msg void OnContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};


#include "endh.dex"