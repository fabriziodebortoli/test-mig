#pragma once

#include "PostaliteTables.h"
#include "PostaLiteSettings.h"
#include "PostaLite.hjson" //JSON AUTOMATIC UPDATE
#include <TbGenLibManaged\GlobalFunctions.h>
#include <TbClientCore\GlobalFunctions.h>
#include <TbClientCore\ClientObjects.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\CMapi.h>
#include <TbGenlib\Parsobj.h>
#include <TbGenlib\PARSCBX.H>
#include <TbGenlib\PARSEDT.H>
#include <TbGenlib\PARSCTRL.H>
#include <TbGenlib\MESSAGES.H>
#include <TbGenlibUI\SettingsTableManager.h>
#include <TbOleDb\Sqltable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

TB_EXPORT CCompanyAddressInfo* LoadCompanyData();

/////////////////////////////////////////////////////////////////////////////
// CDeliveryTypeCombo dialog
/////////////////////////////////////////////////////////////////////////////
class CDeliveryTypeCombo : public CEnumCombo
{
public:
	CDeliveryTypeCombo();
	CDeliveryTypeCombo(UINT nBtnIDBmp, DataEnum* = NULL);

protected:
	BOOL IsValidItemListBox (const DataObj& aDataObj);
};


/////////////////////////////////////////////////////////////////////////////
// CPostaLiteSettingsDialog dialog
/////////////////////////////////////////////////////////////////////////////
class CPostaLiteSettingsDialog : public CParsedDialog
{
private:
	DataBool m_bIsFax;
	DataBool m_bIsDirty;

	DataBool m_bPreviewAddresserData;
	DataStr  m_strPreviewAddresserData;
	
	CBoolButton m_EnabledCtrl;
	CDeliveryTypeCombo	m_cbxDeliveryType; 
    CEnumCombo	m_cbxPrintType;
	CPushButton m_UpdateSettingsButton;
	CXmlCombo	m_cbxDefaultCountry; 
   
	CIntEdit	m_nMarginTopCtrl;
	CIntEdit	m_nMarginLeftCtrl;
	CIntEdit	m_nMarginRightCtrl;
	CIntEdit	m_nMarginBottomCtrl;
	CIntEdit	m_nDeliveryIntervalCtrl;
	CTimeEdit	m_DeliveryTimeCtrl;
	CStrEdit	m_AdviceOfReturnEmailCtrl;
	CMoneyEdit	m_NotifyOnLowCreditCtrl;

	CStrEdit	m_PreviewAddresserDataCtrl;

	CEnumButton m_AddresserTypeCtrl;
public:
  

public:
	CPostaLiteSettingsDialog(CWnd* pParent = NULL);   
	~CPostaLiteSettingsDialog();
private:
	void UpdateControls();
	void AddresserChanged();
	void OnValueChanged();
	void OnAddresserChanged();

public:
	void OnUpdate();

protected:

    virtual BOOL OnInitDialog	();
    virtual void OnSubclassEdit	();
	virtual	void OnCancel		();

	
	void OnDeliveryTypeChanged();
	void OnCountryChanged();
	
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg BOOL OnHelpInfo	(HELPINFO* pHelpInfo);

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CPostaLiteSettingsDialog)
};

//////////////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
