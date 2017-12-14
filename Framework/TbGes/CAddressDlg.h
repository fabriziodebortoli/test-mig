#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\SettingsTable.h>

#include <TBGenlib\ParsObj.h>
#include <TBGenlib\BaseTileDialog.h>
#include <TBGenlib\ParsBtn.h>
#include <TBGenlib\PARSLBX.H>
#include <TBGenlib\SettingsTableManager.h>

// risorse
#include <TBGenlib\AddressEdit.hjson> //JSON AUTOMATIC UPDATE
#include <TBGenlib\AddressEdit.h>

#include "CAddressDlg.hjson" //JSON AUTOMATIC UPDATE
#include "TileDialog.h"
#include "TileManager.h"

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//	class CAddressDlg definition
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CAddressDlg : public CParsedDialogWithTiles, public CAddressDlgObj
{
	DECLARE_DYNCREATE(CAddressDlg)

	friend class CAddressTileDlg;
public: // constructor
	
	CAddressDlg();

	virtual void AttachDocument(CBaseDocument* pDoc) { CParsedDialogWithTiles::AttachDocument(pDoc); }
	virtual void SetAddress(CGeocoder*	pGeocoder,
		CString		aAddress,
		CString		aStreetNumber,
		CString		aCity,
		CString		aCounty,
		CString		aRegion,
		CString		aFederalState,
		CString		aCountry,
		CString		aZipCode,
		CString		aLatitude,
		CString		aLongitude,
		CString	aAddressType = _T(""));

	CAbstractFormDoc* m_pCallerDoc;


protected:
	DataStr m_AddressType;
	DataStr m_Address;
	DataStr m_StreetNumber;
	DataStr m_City;
	DataStr m_County;
	DataStr m_Region;
	DataStr m_FederalState;
	DataStr m_Country;
	DataStr m_ZipCode;
	DataStr m_Latitude;
	DataStr m_Longitude;


protected:
	CStrStatic m_AddressControl;
	CStrStatic m_StreetNumberControl;
	CStrStatic m_CityControl;
	CStrStatic m_CountyControl;
	CStrStatic m_RegionControl;
	CStrStatic m_FederalStateControl;
	CStrStatic m_CountryControl;
	CStrStatic m_ZipCodeControl;
	CStrStatic m_LatitudeControl;
	CStrStatic m_LongitudeControl;


private:
	CGeocoder* m_pGeocoder;

private:
	void UpdateAllLocalCtrl();

public:
	virtual int  ShowDialog();

protected:
	virtual BOOL OnInitDialog		();
	virtual void OnCustomizeToolbar	();

	//{{AFX_MSG(CAddressDlg)  
	afx_msg virtual	void OnOK();
	afx_msg virtual	void OnCancel();
	afx_msg virtual	void OnShowAddress();
	//}}AFX_MSG

public:
	CString GetGoogleWebLink();

	DECLARE_MESSAGE_MAP()
};

//====================================================================
//				 CAddressTileGrp 
//====================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT CAddressTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAddressTileGrp)

protected:
	virtual void Customize();
};

/////////////////////////////////////////////////////////////////////////////
//			Class CAddressTileDlg Declaration
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class CAddressTileDlg : public CTileDialog
{
	DECLARE_DYNCREATE(CAddressTileDlg)
		
public:
	CAddressDlg* GetParentParsedDlg();

public:
	CAddressTileDlg	();
	CAddressTileDlg	(const CString& sName, int nIDD, CWnd* pParent = NULL) : CTileDialog(sName, nIDD, pParent = NULL) {}

public:
	virtual	void BuildDataControlLinks	();
};

///////////////////////////////////////////////////////////////////
class CAddressListBox : public CStrListBox
{
	DECLARE_DYNCREATE(CAddressListBox)

public:
	CAddressListBox();
	CAddressListBox(UINT nBtnIDBmp,
		DataStr* = NULL,
		CSelectAddressDlg* pDlg = NULL);

private:
	CSelectAddressDlg* m_pDlg;

protected:
	virtual BOOL OnInitCtrl();
	virtual	void OnFillListBox();
};


/////////////////////////////////////////////////////////////////////////////
//	class CSelectAddressDlg definition
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSelectAddressDlg : public CParsedDialogWithTiles, public CSelectAddressDlgObj
{
	DECLARE_DYNCREATE(CSelectAddressDlg)

private:
	CSelectAddressDlg();

public: // constructor
	
protected:
	DataStr			m_Address;

	CStringArray	m_AddressArray;
	CStringArray	m_LatitudeArray;
	CStringArray	m_LongitudeArray;

public:
	virtual void	SetSelectAddressDlg(CGeocoder* pGeocoder);
	virtual void	AddAddress(CString aAddress, CString aLatitude,	CString aLongitude);
	virtual CString	GetAddress()			{ return m_Address.GetString(); }
	CStringArray&	GetAddresses()			{ return m_AddressArray; }
	virtual void	AttachDocument(CBaseDocument* pDoc) { CParsedDialogWithTiles::AttachDocument(pDoc); }
	CString			GetGoogleWebLink();
	virtual			int ShowDialog();
	void			DisableShowButton();

protected:
	CAddressListBox	m_AddressControl;

private:
	CGeocoder* m_pGeocoder;

private:
	void UpdateAllLocalCtrl();

protected:
	virtual BOOL OnInitDialog();

	//{{AFX_MSG(CSelectAddressDlg)  
	afx_msg virtual	void OnOK();
	afx_msg virtual	void OnCancel();
	afx_msg void OnLButtonDblClk();
	afx_msg void OnLButtonClk();
	afx_msg virtual	void OnShowAddress();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};
