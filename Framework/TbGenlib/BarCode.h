#pragma once

#include "ParsCtrl.h"
#include "BarCodeEnums.h"

//includere alla fine degli include del .H
#include "beginh.dex"
class Parser;
class Unparser;
class CBarCodeSettingsDlg;


//barcode EAN128
static const TCHAR szChecksumModule103_Optional[] = _T("Optional Checksum");	//1
static const TCHAR szChecksumModule10103_Default[] = _T("Default (none or embedded)");	//0

//2D Barcode Data Matrix
//Encoding type
static const TCHAR sz2DBCDataMatrixEncodingMode_ASCII[] = _T("ASCII");		//0
static const TCHAR sz2DBCDataMatrixEncodingMode_C40[] = _T("C40");		//1
static const TCHAR sz2DBCDataMatrixEncodingMode_Text[] = _T("Text");		//2
static const TCHAR sz2DBCDataMatrixEncodingMode_X12[] = _T("X12");		//3
static const TCHAR sz2DBCDataMatrixEncodingMode_EDIFACT[] = _T("EDIFACT");	//4
static const TCHAR sz2DBCDataMatrixEncodingMode_Base256[] = _T("Base 256");	//5
//version
//static const TCHAR sz2DBCDataMatrixVersion_Auto[] = _T("Auto");		//0


//2D Barcode QR
static const TCHAR sz2DBCQREncodingMode_Numeric[] = _T("Numeric");		//0
static const TCHAR sz2DBCQREncodingMode_Alphanumeric[] = _T("Alphanumeric");	//1
static const TCHAR sz2DBCQREncodingMode_Byte[] = _T("Byte");			//2
static const TCHAR sz2DBCQREncodingMode_Kanji[] = _T("Kanji");			//3

	
//2D Barcode PDF417
static const TCHAR sz2DBCPDF417EncodingMode_Text[] = _T("Text");		//0
static const TCHAR sz2DBCPDF417EncodingMode_Byte[] = _T("Byte");		//1
static const TCHAR sz2DBCPDF417EncodingMode_Numeric[] = _T("Numeric");	//2


class TB_EXPORT CBarCodeTypes
{
public:
	typedef struct struct_BarCodeTypes { int m_nType; LPCTSTR m_sName; DWORD m_eBc; int m_nStdLength; } SBarCodeTypes;

public:
	typedef struct struct_BarCodeDataMatrixVersions { int m_nType; LPCTSTR m_sName; } SBarCodeDataMatrixVersions;


static const int	BARCODE_TYPES_NUM	= 26;
static const int	BC_DEFAULT			= 0;
static const int	BC_EAN128			= 22;

static const int	BARCODE_DM_VERSIONS_NUM = 31;

static const TCHAR	BC_SEP				= '\01';

static int				s_bcTypes			[BARCODE_TYPES_NUM];

static SBarCodeTypes	s_arBarCodeTypes	[BARCODE_TYPES_NUM];
static SBarCodeDataMatrixVersions	s_arBarCodeDataMatrixVersions[BARCODE_DM_VERSIONS_NUM];

	
static CString	BarCodeDescription	(int nBarcCodeType);
static int	BarCodeStandardLength(int nBarcCodeType);

static CString	BarCodeDMVersionDescription(int nBarCodeVersion);

static int		BarCodeType			(const CString& sName, BOOL bDefaultERR = TRUE);
static int		BarCodeType			(DWORD eBc, BOOL bDefaultERR = TRUE);
static DWORD	BarCodeEnum			(int nType, BOOL bDefaultERR = TRUE);
static DWORD	BarCodeEnum			(const CString& sName, BOOL bDefaultERR = TRUE);

static int		BarCodeDMVersion (const CString& sName, BOOL bDefaultERR = TRUE);


static CString	TypedBarCode(const CString& strCode, int nBarCodeType, int nChkSum = 0, const CString& sHumanReadable = _T(""), int nNarrowBar = -1, int nBarHeight = -1, const CString& sVersion = _T(""), int nErrCorrLevel = -2);
static BOOL		Is2DBarcodeType(DWORD nType);
static BOOL		ReadPDF417Version(CString& sVersion, int& nRows, int&nColumns);
static BOOL		CheckAndCompleteBCString(CString& barcode, int nBarCodeType, CString& errMsg);
static BOOL		AddCheckDigit(CString& barcode, int nBarCodeType);
static char		Get_UPC_EAN_CheckDigit(CString barcode);
static char		Get_CODE39_CheckDigit(CString barcode);
static char		Get_Mod10_CheckDigit(CString barcode);
static char		Get_CODABAR_CheckDigit(CString barcode);
static CString	UPCE_to_UPCA(CString strUPCE);

};

//Immagine usata per disegnare il BarCode, scrivendo direttamente sul suo device context
//==============================================================================
class TB_EXPORT CStaticImage : public CStatic
{
private:
		CBarCodeSettingsDlg* m_pBarCodeDlg;
public:
	void SetOwnerDialog(CBarCodeSettingsDlg* pBarCodeDlg){ m_pBarCodeDlg = pBarCodeDlg;}
protected:
		//{{AFX_MSG(CStaticImage)
			afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
		//}}AFX_MSG
		DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CBarCode : public CObject
{
	DECLARE_DYNAMIC(CBarCode)
	
public:    
	int			m_nBarCodeType;
	int			m_nBCDefaultType;
	int			m_nNarrowBar;				// thickness of a narrow bar, the default is 1
	BOOL		m_bVertical;				// the default is horizontal (FALSE)
	BOOL		m_bShowLabel;				// the default is TRUE
	int			m_nCheckSumType;			// checsum type value
	CString		m_sCheckEncodeFieldName;	//name of field that contains encoding type/check digit
	int			m_nCustomBarHeight;			// customizable bar height in logical units 
	int			m_nHumanTextAlias;			// human text is contained into the associated alias 
	int			m_nBarCodeTypeAlias;		// bar code type is contained into the associated alias 
	int			m_nNarrowBarAlias;			//TODO NarrowBar thickness is contained into the associated alias 
	BOOL		m_bCheckSize;
	int			m_n2DVersion;				// version of barcode 2D
	CString		m_s2DVersionFieldName;		//name of field that contains barcode version
	int			m_nErrCorrLevel;			// error Correction level of barcode 2D
	CString		m_sErrCorrLevelFieldName;	//name of field that contains error Correction level
	int			m_nRowsNo;					// Number of rows of PDF417 barcode
	int			m_nColumnsNo;				// Number of columns of PDF417 barcode


public:    
	CBarCode ();
	CBarCode (const CBarCode&);

public:    
	BOOL 	DrawBarCode		(CDC&, CRect, const LOGFONT&, const CString& sBarCode, COLORREF clrText, COLORREF clrBkg, CString& sErr,
							CBaseDocument* = NULL, CString strHumanReadeable = _T(""), BOOL bPreview = FALSE,
							int align = DT_CENTER, int vAlign = DT_CENTER);

	BOOL	PrepareBCParameters(CDC& DC, 
								CString &strValue,
								int &nBarCodeType,
								CString& strHumanReadeable,
								int &nEncodingMode,
								int &nVersion,
								int &nErrCorrLevel,
								int &nNarrowBar,
								int &nHeight,
								int &nRows,
								int &nColumns,
								BOOL bPreview);

	BOOL 	Parse			(Parser&);
	void	Unparse			(Unparser& ofile, BOOL bNewline);

	const BOOL Is2DBarcode();
	const BOOL IsCheckEncodigEnabled();

};

//==============================================================================
class TB_EXPORT CBarCodeAttrsDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CBarCodeAttrsDlg)
// Dialog Data
	//{{AFX_DATA(CBarCodeAttrsDlg)
	//}}AFX_DATA
public:
	CBarCode*	m_pBarCode;

	CIntEdit	m_NarrowBarEdit;
	CBCGPComboBox	m_TypeCombo;
	CButton		m_VertRadioBtn;
	CButton		m_HorzRadioBtn;
	CButton		m_ShowLabCheckBtn;
	CBCGPComboBox	m_CheckSumType;
	CButton		m_CustomBarHeight;
	CIntEdit	m_Height;
	
	CButton		m_HT_CustomAliasNumber;
	CBCGPComboBox	m_HT_AliasNumber;

	CButton		m_chkCustomType;
	CBCGPComboBox	m_cbxCustomType;

	SymTable*	m_pSymTable;
	int			m_nDefaultBarCode;

	CStrEdit	m_ValueEdit;
	CStatic		m_TestStatic;

	CString	m_strBCValue;
	//BOOL		m_bEnableValueEdit;
	//BOOL		m_bEnableTypeCbo;

public:
	CBarCodeAttrsDlg	(CBarCode*, SymTable* pSymTable = NULL, CWnd* = NULL);

private:
	void LoadDefaultBarCode				();
	void LoadBarcodeTypesCombo			();
	void LoadCheckSumModulesCombo		();
	void SetCheckSumModulesCombo		();
	void SetCustomBarHeightControls		();
	void SetCustomAliasNumberControls	();
	void LoadAliasTable					();
	BOOL UseDynamicText					();

protected:
		void 	DrawBarCode	(CDC&, CRect&, LOGFONT); 
		void 	DrawStaticBarCode	(); 

	//{{AFX_MSG(CBarCodeAttrsDlg)
		virtual BOOL    OnInitDialog				();
		virtual void    OnOK						();
		virtual void    OnCancel					();

		afx_msg void	OnTypeBarCodeChanged		();
		afx_msg void	OnCustomBarsHeightChanged	();
		afx_msg void	OnCustomAliasNumberChanged	();
		afx_msg void	OnShowTextChanged			();
		afx_msg void	OnCustomTypeChanged			();

		afx_msg void 	OnPaint						();
		afx_msg void 	OnBCValueChanged			();
		afx_msg void 	OnDrawStaticBarCode			(); 
		
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//==============================================================================
class TB_EXPORT CBarCodeSettingsDlg : public CParsedDialog
{
	friend class CStaticImage;
	DECLARE_DYNAMIC(CBarCodeSettingsDlg)
public:
	CString&			m_strBCValue;
	CBarCode&			m_BarCode;
	CStrEdit			m_ValueEdit;
	CStaticImage		m_TestStatic;
	CIntEdit			m_NarrowBarEdit;
	CBCGPComboBox		m_TypeCombo;
	CButton				m_VertRadioBtn;
	CButton				m_HorzRadioBtn;
	CButton				m_ShowLabCheckBtn;
	BOOL				m_bEnableValueEdit;
	BOOL				m_bEnableTypeCbo;
	CBCGPComboBox		m_CheckSumType;
	CButton				m_CustomBarHeight;
	CIntEdit			m_Height;
	int					m_nDefaultBarCode;

	CBarCodeSettingsDlg	(CString&, CBarCode&, BOOL = TRUE, BOOL = TRUE, CWnd* = NULL);

private:
	void LoadDefaultBarCode				();
	void LoadCheckSumModulesCombo		();
	void SetCheckSumModulesCombo		();
	void SetCustomBarHeightControls		();
	//void SetCustomAliasNumberControls	();

protected:

	virtual BOOL    OnInitDialog();
	virtual void    OnOK		();
	virtual void    OnCancel	();
	
	void 	DrawBarCode	(CDC&, CRect&, LOGFONT); 
	void 	DrawStaticBarCode	(); 

	//{{AFX_MSG(CBarCodeSettingsDlg) 
	afx_msg void 	OnPaint					();
	afx_msg void 	OnBCValueChanged		();
	afx_msg void 	OnDrawStaticBarCode		(); 
	afx_msg void	OnTypeBarCodeChanged	();
	afx_msg void	OnCustomBarsHeightChanged();
	afx_msg void	OnShowTextChanged			();
		//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
