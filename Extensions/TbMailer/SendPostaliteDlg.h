
#pragma once

#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\CMapi.h>
#include <TbGes\Hotlink.h>
#include <TbGes\TileManager.h>
#include <TBGenlib\TBStrings.h>
#include <TBGenlib\parsedt.h>

#include "Email.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class DynamicHotKeyLink;
class HotKeyLink;

/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CAddresseeEdit: public CStrEdit
{
public:
	CAddresseeEdit();
	~CAddresseeEdit();

	CString GetRefDocNamespace(HotKeyLink* pHkl = NULL);
protected:

	afx_msg void	OnContextMenu		(CWnd* pWnd, CPoint ptMousePos);
	afx_msg	void	CmdMenuButton		(UINT nID) { DoCmdMenuButton(nID);}

	virtual	BOOL	GetMenuButton		(CMenu*);
	virtual	void	DoCmdMenuButton		(UINT nID);
	virtual CString	GetMenuButtonImageNS();

	DynamicHotKeyLink*	m_pHklCustSuppAddrs;

	DynamicHotKeyLink*	m_pHklContactEmail;
	DynamicHotKeyLink*	m_pHklProspectiveSuppEmail;
	DynamicHotKeyLink*	m_pHklProducerEmail;
	DynamicHotKeyLink*	m_pHklCompanyEmail;
	DynamicHotKeyLink*	m_pHklBankEmail;
	DynamicHotKeyLink*	m_pHklCarriersEmail;
	DynamicHotKeyLink*	m_pHklStoragesEmail;

	afx_msg	LRESULT	OnPushButtonCtrl				(WPARAM wParam, LPARAM lParam);
	
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////
//							CEMailDlg
//---------------------------------------------------------------------------
//
class TB_EXPORT CSendPostaLiteDlg : public CParsedDialogWithTiles
{
	DECLARE_DYNAMIC(CSendPostaLiteDlg)
	friend class CAddresseeEdit;

protected:
	CMapiMessage&	m_Email;
	CBaseDocument*	m_pCallerDoc;

	CPostaLiteAddress m_Addr;

	DataStr			m_dsSubject;
	CStrEdit		m_edtSubject;

	DataStr			m_dsAddressee;
	DataStr			m_dsAddress;
	DataStr			m_dsZipCode;
	DataStr			m_dsFax;
	DataStr			m_dsCity;
	DataStr			m_dsCounty;
	DataStr			m_dsCountry;
	DataStr			m_dsISOCode;

	CAddresseeEdit	m_edtAddressee;
	CStrEdit		m_edtAddress;
	CStrEdit		m_edtZipCode;
	CXmlCombo		m_cbxCity;
	CXmlCombo		m_cbxCounty;
	CXmlCombo		m_cbxCountry;

	CEnumCombo		m_cbxDeliveryType;
	DataEnum		m_deDeliveryType;

	CEnumCombo		m_cbxPrintType;
	DataEnum		m_dePrintType;

	CFont			m_fontZucchetti;

public:
	CSendPostaLiteDlg
		(
			CDocument* pCallerDoc,
			CMapiMessage& msg
		);
	~CSendPostaLiteDlg();

	void UpdateCtrl();

protected:
	virtual	BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual void	OnCustomizeToolbar	();

protected:
	void OnAfterRecordSelected			();
	void OnCityChanged					();
	void OnDeliveryTypeChanged			();

protected:
	//{{AFX_MSG(CSendPostaLiteDlg)  
	afx_msg	LRESULT OnGetWebCommandType (WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
