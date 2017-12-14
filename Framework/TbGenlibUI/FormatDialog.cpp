#include "stdafx.h"

#include <stdlib.h>
#include <time.h>
#include <float.h>

#include <TbClientCore\ClientObjects.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\dirtreectrl.h>
#include <TbGenlibManaged\HelpManager.h>

#include <TbGenlibUI\ContextSelectionDialog.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "formatdialog.h"

#include "FormatDialog.hjson" //JSON AUTOMATIC UPDATE
#include <TbParser\FormatsParser.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szHelpNamespace[] = _T("Document.Framework.TbGenlibUI.TbGenlibUI.ExecOpenFormatter");

//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
enum IndexIDS
{
	FMT_NULL =0,

	FMT_SHOW_STR_UPPER,
    FMT_SHOW_STR_LOWER,
    FMT_SHOW_STR_CAPIT,
    FMT_SHOW_STR_EXPANDED,
    FMT_SHOW_STR_ASIS,
	FMT_SHOW_STR_MASKED,
	
	FMT_SHOW_INT_NUM,
    FMT_SHOW_INT_LET,
    FMT_SHOW_INT_ENC,
    	
    FMT_SIGN_INT_MINUSPREFIX,
    FMT_SIGN_INT_MINUSPOSTFIX,
    FMT_SIGN_INT_USERND,
    FMT_SIGN_INT_SIGNPREFIX,
    FMT_SIGN_INT_SIGNPOSTFIX,
	FMT_SIGN_INT_NEVER,

	FMT_SHOW_DBL_FIX,
    FMT_SHOW_DBL_EXP,
    FMT_SHOW_DBL_ENG,
    FMT_SHOW_DBL_ENC,
    FMT_SHOW_DBL_LET,

    FMT_ROUND_DBL_NONE,
    FMT_ROUND_DBL_ABS,
    FMT_ROUND_DBL_SIGNED,
    FMT_ROUND_DBL_ZERO,
    FMT_ROUND_DBL_INF,

	FMT_TEMPL_DAT_DF99,
    FMT_TEMPL_DAT_DFB9,
    FMT_TEMPL_DAT_DF9,

	FMT_TEMPL_DAT_MF99,
	FMT_TEMPL_DAT_MFB9,
    FMT_TEMPL_DAT_MF9,
    FMT_TEMPL_DAT_MFS3,
    FMT_TEMPL_DAT_MFSX,

	FMT_TEMPL_DAT_YF99,
    FMT_TEMPL_DAT_YF999,
    FMT_TEMPL_DAT_YF9999,

	FMT_SHOW_DAT_DMY,
    FMT_SHOW_DAT_MDY,
    FMT_SHOW_DAT_YMD,

    FMT_SHOW_DAT_NOWD,
    FMT_SHOW_DAT_WDDATE,
    FMT_SHOW_DAT_DATEWD,

    FMT_TEMPL_TIM_H99,
    FMT_TEMPL_TIM_HB9,
    FMT_TEMPL_TIM_H9,
    FMT_TEMPL_TIM_H99_AMPM,
    FMT_TEMPL_TIM_HB9_AMPM,
    FMT_TEMPL_TIM_H9_AMPM,
    FMT_TEMPL_TIM_H99_NOSEC,
    FMT_TEMPL_TIM_HB9_NOSEC,
    FMT_TEMPL_TIM_H9_NOSEC,
    FMT_TEMPL_TIM_H99_AMPM_NOSEC,
    FMT_TEMPL_TIM_HB9_AMPM_NOSEC,
    FMT_TEMPL_TIM_H9_AMPM_NOSEC,
 
	FMT_ELAPSED_TIME_DHMSF,
	FMT_ELAPSED_TIME_DHMS,		
    FMT_ELAPSED_TIME_DHM ,
    FMT_ELAPSED_TIME_DH,
    FMT_ELAPSED_TIME_D,
    FMT_ELAPSED_TIME_HMSF,
	FMT_ELAPSED_TIME_HMS,
    FMT_ELAPSED_TIME_HM,
    FMT_ELAPSED_TIME_H,
	FMT_ELAPSED_TIME_MSF,
    FMT_ELAPSED_TIME_MS,
    FMT_ELAPSED_TIME_M,
    FMT_ELAPSED_TIME_S,
    FMT_ELAPSED_TIME_SF,
    FMT_ELAPSED_TIME_DHMCM,
    FMT_ELAPSED_TIME_DHCH,
    FMT_ELAPSED_TIME_DCD,
	FMT_ELAPSED_TIME_HMCM,
    FMT_ELAPSED_TIME_HCH,
    FMT_ELAPSED_TIME_MCM,
	FMT_ELAPSED_TIME_CH,

	FMT_UPPER,
    FMT_LOWER,
    FMT_CAPIT,
    FMT_ASIS,
	FMT_FIRSTLETTER
};

struct IndexIDSTag
{
	int			nIndex;
	IndexIDS	enumIDS;
	int			nTag;
};

static const TCHAR szBlank[] = _T(" ");
static const TCHAR szCurrentReport[] = _T("Local styles of current report");

//----------------------------------------------------------------------------
static const CString FormatIndexTag(IndexIDSTag TableRow)
{
	switch(TableRow.enumIDS)
	{
		case FMT_NULL			:		return _T("");

		case FMT_SHOW_STR_UPPER  :		return _TB("CAPITAL LETTER");
		case FMT_SHOW_STR_LOWER  :		return _TB("Lower-case");
		case FMT_SHOW_STR_CAPIT  :		return _TB("Capitalized");
		case FMT_SHOW_STR_EXPANDED :	return _TB("Spaced");
		case FMT_SHOW_STR_ASIS   :		return _TB("As is");
		case FMT_SHOW_STR_MASKED  :		return _TB("Masked");

		case FMT_SHOW_INT_NUM    :		return _TB("Digital");
		case FMT_SHOW_INT_LET    :		return _TB("Literal");
		case FMT_SHOW_INT_ENC    :		return _TB("Coded");
		
		case FMT_SIGN_INT_MINUSPREFIX : return _TB("Prefix");
		case FMT_SIGN_INT_MINUSPOSTFIX:	return _TB("Postfix");
		case FMT_SIGN_INT_USERND :		return _TB("Use ( )");
		case FMT_SIGN_INT_SIGNPREFIX :	return _TB("+/- Prefix");
		case FMT_SIGN_INT_SIGNPOSTFIX : return _TB("Postfix +/-");
		case FMT_SIGN_INT_NEVER  :		return _TB("Never");
	
		case FMT_SHOW_DBL_FIX    :		return _TB("Fixed");
		case FMT_SHOW_DBL_EXP    :		return _TB("Exponential");
		case FMT_SHOW_DBL_ENG    :		return _TB("Technical");
		case FMT_SHOW_DBL_ENC    :		return _TB("Coded");
		case FMT_SHOW_DBL_LET    :		return _TB("Literal");
	
		case FMT_ROUND_DBL_NONE  :		return _TB("None");
		case FMT_ROUND_DBL_ABS   :		return _TB("Mathematical (absolute value)");
		case FMT_ROUND_DBL_SIGNED :		return _TB("Mathematical (with sign)");
		case FMT_ROUND_DBL_ZERO  :		return _TB("Round down");
		case FMT_ROUND_DBL_INF   :		return _TB("Round up");

		case FMT_TEMPL_DAT_DF99  :		return _TB("04");
		case FMT_TEMPL_DAT_DFB9  :		return _TB(" 4");
		case FMT_TEMPL_DAT_DF9   :		return _TB(" 4");
		
		case FMT_TEMPL_DAT_MF99  :		return _TB("01");
		case FMT_TEMPL_DAT_MFB9  :		return _TB(" 1");
		case FMT_TEMPL_DAT_MF9   :		return _TB(" 1");
		case FMT_TEMPL_DAT_MFS3  :		return _TB("Jan");
		case FMT_TEMPL_DAT_MFSX  :		return _TB("January");
		
		case FMT_TEMPL_DAT_YF99  :		return _TB("60");
		case FMT_TEMPL_DAT_YF999 :		return _TB("960");
		case FMT_TEMPL_DAT_YF9999 :		return _TB("1960");
		
		case FMT_SHOW_DAT_DMY    :		return _TB("DDMMYY");
		case FMT_SHOW_DAT_MDY    :		return _TB("MMDDYY");
		case FMT_SHOW_DAT_YMD    :		return _TB("YYMMDD");

		case FMT_SHOW_DAT_NOWD   :		return _TB("Only Date");
		case FMT_SHOW_DAT_WDDATE :		return _TB("Day and then Date");
		case FMT_SHOW_DAT_DATEWD :		return _TB("Date and then Day");

		case FMT_TEMPL_TIM_H99   :		return _TB("hh:mm:ss");
		case FMT_TEMPL_TIM_HB9   :		return _TB(" h:mm:ss");
		case FMT_TEMPL_TIM_H9    :		return _TB("h:mm:ss");
		case FMT_TEMPL_TIM_H99_AMPM :	return _TB("hh:mm:ss tt");
		case FMT_TEMPL_TIM_HB9_AMPM :	return _TB(" h:mm:ss tt");
		case FMT_TEMPL_TIM_H9_AMPM :	return _TB("h:mm:ss tt");
		case FMT_TEMPL_TIM_H99_NOSEC :	return _TB("hh:mm");
		case FMT_TEMPL_TIM_HB9_NOSEC :	return _TB(" h:mm");
		case FMT_TEMPL_TIM_H9_NOSEC :	return _TB("h:mm");
		case FMT_TEMPL_TIM_H99_AMPM_NOSEC : return _TB("hh:mm tt");
		case FMT_TEMPL_TIM_HB9_AMPM_NOSEC : return _TB(" h:mm tt");
		case FMT_TEMPL_TIM_H9_AMPM_NOSEC : return _TB("h:mm tt");

		case FMT_ELAPSED_TIME_DHMSF :	return _TB("Days:Hours:Minutes: Seconds.Fractions of a Second");
		case FMT_ELAPSED_TIME_DHMS :	return _TB("Days:Hours:Seconds");	
		case FMT_ELAPSED_TIME_DHM :		return _TB("Days:Hours:Minutes");;
		case FMT_ELAPSED_TIME_DH :		return _TB("Days:Hours");
		case FMT_ELAPSED_TIME_D  :		return _TB("Days");
		case FMT_ELAPSED_TIME_HMSF :	return _TB("Hours:Minutes:Seconds.Fraction of a Second");
		case FMT_ELAPSED_TIME_HMS :		return _TB("Hours:Minutes:Seconds");
		case FMT_ELAPSED_TIME_HM :		return _TB("Hours:Minutes");
		case FMT_ELAPSED_TIME_H  :		return _TB("Hours");
		case FMT_ELAPSED_TIME_MSF :		return _TB("Minutes:Seconds.Fractions of a Second");
		case FMT_ELAPSED_TIME_MS :		return _TB("Minutes:Seconds");
		case FMT_ELAPSED_TIME_M  :		return _TB("Minutes");
		case FMT_ELAPSED_TIME_S  :		return _TB("Seconds");
		case FMT_ELAPSED_TIME_SF  :		return _TB("Seconds.Fractions of a Second");
		case FMT_ELAPSED_TIME_DHMCM:	return _TB("Days:Hours:Minutes.Fractions of a Minute");
		case FMT_ELAPSED_TIME_DHCH :	return _TB("Days:Hours.Fractions of a Hour");
		case FMT_ELAPSED_TIME_DCD :		return _TB("Days.Fractions of a Day");
		case FMT_ELAPSED_TIME_HMCM :	return _TB("Hours:Minutes.Fractions of a Minute");
		case FMT_ELAPSED_TIME_HCH :		return _TB("Hours.Fractions of a Hour");
		case FMT_ELAPSED_TIME_MCM :		return _TB("Minutes.Fractions of a Minute");
		case FMT_ELAPSED_TIME_CH :		return _TB("Centesimal hour");

		case FMT_UPPER           :		return _TB("CAPITAL LETTER");
		case FMT_LOWER           :		return _TB("Lower-case");
		case FMT_CAPIT           :		return _TB("Capitalized");
		case FMT_ASIS            :		return _TB("As is");
		case FMT_FIRSTLETTER     :		return _TB("Only first letter");
	}

	ASSERT(FALSE);
	return _T("");
}

//----------------------------------------------------------------------------
static void SetupCombo(CComboBox& cbo, const IndexIDSTag Table[])
{
	cbo.ResetContent();
	cbo.SetRedraw(FALSE);
	for (int nIdx = 0; Table[nIdx].nIndex != -1; nIdx++)
	{
		int iPos = cbo.AddString(FormatIndexTag(Table[nIdx]));
		cbo.SetItemData(iPos, Table[nIdx].nTag);
	}
	cbo.SetRedraw(TRUE);
	cbo.Invalidate(FALSE);
}

//----------------------------------------------------------------------------
static void TagToComboCurSel(CComboBox& cbo, DWORD dwTag)
{
	for (int nIndex = 0; nIndex < cbo.GetCount(); nIndex++)
		if (cbo.GetItemData(nIndex) == dwTag)
		{
			cbo.SetCurSel(nIndex);
			return;
		}
}

//----------------------------------------------------------------------------
static int ComboCurSelToTag(CComboBox& cbo)
{         
	return (int)(cbo.GetItemData(cbo.GetCurSel()));
}

//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//============================================================================
//		CTreeItemRef implementation
//			per localizzazione (parte non localizzata)
//============================================================================
CTreeItemRef::CTreeItemRef(CString strName)
{
	m_strName = strName;
};

//============================================================================
class CPrompterDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CPrompterDlg)
protected:
	CString			m_strDefault;
	Formatter*		m_pFormatter;
	DataType		m_DataType;
	DataObj*		m_pDataObj;		
	
	CParsedEdit*	m_pedtIn;
	CBCGPEdit		m_edtOut;
	CBCGPButton			m_btnFormat;

public:
	CPrompterDlg
		(
			Formatter*	PF,
			LPCTSTR		Default,
			CWnd*		Parent
		);
	~CPrompterDlg();

protected:
	BOOL	OnInitDialog	();
	void	OnFormat		();
	void	OnCancel		();

	//{{AFX_MSG(CPrompterDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
IMPLEMENT_DYNAMIC(CPrompterDlg, CParsedDialog)
//============================================================================
class CXLatEdit : public CStrEdit
{
protected:
	virtual	BOOL OnInitCtrl ();
	virtual	BOOL DoOnChar	(UINT nChar);
};

//----------------------------------------------------------------------------
BOOL CXLatEdit::DoOnChar (UINT nChar)
{
	if (CParsedEdit::DoOnChar(nChar))
		return TRUE;
		
	if (_istalnum(nChar) || _istpunct(nChar) || nChar == VK_BACK)
		return FALSE;

	BadInput();
	return TRUE;

}

//----------------------------------------------------------------------------
BOOL CXLatEdit::OnInitCtrl ()
{
	if (!__super::OnInitCtrl())
		return FALSE;

	SetCtrlMaxLen(1);
	return TRUE;
}

//============================================================================
class CIntXlatDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CIntXlatDlg)
public:
	CString m_strXlat;

protected:
	CXLatEdit m_edtDigits[10];

public:
	CIntXlatDlg	(CWnd* Parent);

protected:
	BOOL	OnInitDialog	();
	void	OnOK			();
	void	OnCancel		();

	//{{AFX_MSG(CIntXlatDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

IMPLEMENT_DYNAMIC(CIntXlatDlg, CParsedDialog)

//============================================================================
class CDblXlatDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CDblXlatDlg)
public:
	CString m_strXlat;

protected:                      
	// include also comma
	CXLatEdit m_edtDigits[11];

public:
	CDblXlatDlg	(CWnd* Parent);
	
protected:
	BOOL	OnInitDialog	();
	void	OnOK			();
	void	OnCancel		();

	//{{AFX_MSG(CDblXlatDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

IMPLEMENT_DYNAMIC(CDblXlatDlg, CParsedDialog)
//============================================================================
class CFormatDlgRoot : public CParsedDialog
{
	DECLARE_DYNAMIC(CFormatDlgRoot)
	friend class CFormatDlg;

protected:
	Formatter* 	m_pFormatter;
	Formatter*	m_pNewFormatter;
	CStrEdit	m_edtPrologue;
	CStrEdit	m_edtEpilogue;
	CString		m_strOldPrologue;
	CString		m_strOldEpilogue;

public:
	CString		m_strPrologue;
	CString		m_strEpilogue;

protected:
	CBCGPButton		m_btnTest;
	CIntEdit	m_edtPaddedLen;
	CBCGPButton		m_btnPadLeft;
	CBCGPButton		m_btnPadRight;
	BOOL		m_bModified;

protected:
	CFormatDlgRoot	(Formatter*, UINT id, CWnd*, BOOL);
	~CFormatDlgRoot	();

protected:
	BOOL	OnInitDialog	();
	void	OnOK			();
	void	OnCancel		();

	void	OnLeftClicked	();
	void	OnRightClicked	();
	void	OnWidthUpdate	();

	int		GetIdxGlobalCustomFormat	(Formatter* pFormatter);

	// virtual overridables mixed function
	virtual void	OnTest			();

	virtual void	OnUpdate		(int);
	
	virtual BOOL	SetupControls	();
	virtual	void	FillControls	();
	virtual BOOL	ReadControls	();
	virtual void	CleanupControls	();

public:
	BOOL IsModified () const { return m_bModified; }
	
protected:
	//{{AFX_MSG(CFormatDlgRoot)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

IMPLEMENT_DYNAMIC(CFormatDlgRoot, CParsedDialog)
//============================================================================
class CStringFormatDlg : public CFormatDlgRoot
{
	DECLARE_DYNAMIC(CStringFormatDlg)
private:
	CBCGPComboBox	m_cboFormat;
	CBCGPEdit	m_edtInterChars;
	CBCGPEdit	m_edtMask;
	CBCGPButton	m_btnZeroPadded;

public:
	CStringFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);

protected:
	virtual BOOL	SetupControls	();
	virtual void 	FillControls	();
	virtual BOOL	ReadControls	();

protected:
	//{{AFX_MSG(CStringFormatDlg)
	afx_msg void OnStrFmtSelChange	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

IMPLEMENT_DYNAMIC(CStringFormatDlg, CFormatDlgRoot)
//============================================================================
class CIntFormatDlg : public CFormatDlgRoot
{
private:
	CBCGPComboBox	m_cboFormat;
	CBCGPComboBox	m_cboSign;
	CBCGPButton		m_btnZeroPadded;
	CBCGPEdit		m_edtThouSep;
	CBCGPButton		m_btnXtable;
	CBCGPButton		m_btnShowNullValueAlias;
	CBCGPEdit		m_edtNullValueAlias;

public:
	CIntFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);

protected:
	virtual void	OnUpdate	(int nCtlOriginator);
			
	virtual BOOL	SetupControls	();
	virtual void	FillControls	();
	virtual BOOL	ReadControls	();

protected:
	//{{AFX_MSG(CIntFormatDlg)
	afx_msg void OnIntFmtSelChange			();
	afx_msg void OnIntSignSelChange			();
	afx_msg void OnIntZeroAsDashSelChange	();
	afx_msg void OnIntXlat					();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
class CDblFormatDlg : public CFormatDlgRoot
{
private:
	CBCGPComboBox	m_cboFormat;
	CBCGPComboBox	m_cboRounding;
	CBCGPComboBox	m_cboSign;
	CBCGPEdit	m_edtThouSep;
	CBCGPEdit	m_edtDecSep;
	CBCGPButton		m_btnLeading0;
	CBCGPButton		m_btnTrailing0;
	CBCGPButton		m_btnXtable;
	CDoubleEdit	m_edtQuantum;
	CIntEdit	m_edtDecimals;
	CBCGPButton		m_btnShowNullValueAlias;
	CBCGPEdit	m_edtNullValueAlias;

public:
	CDblFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);

protected:
	virtual	void	OnUpdate	(int nCtlOriginator);
			
	virtual BOOL	SetupControls	();
	virtual void 	FillControls	();
	virtual BOOL	ReadControls	();

protected:
	//{{AFX_MSG(CDblFormatDlg)
	afx_msg void OnFormatSelChange			();
	afx_msg void OnDblXlat					();
	afx_msg void OnDecimalsChanged			();
	afx_msg void OnDblZeroAsDashSelChange	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
class CDateTimeFormatDlg : public CFormatDlgRoot
{
protected:
	CBCGPComboBox	m_cboOrder;
	CBCGPComboBox	m_cboWeekday;
	CBCGPComboBox	m_cboDay;
	CBCGPComboBox	m_cboMth;
	CBCGPComboBox	m_cboYer;
	CBCGPEdit	m_edtFirstSep;
	CBCGPEdit	m_edtSecondSep;

	CBCGPComboBox	m_cboTimeFormat;
	CBCGPEdit	m_edtTimeSeparator;
	CBCGPEdit	m_edtTimeAM;
	CBCGPEdit	m_edtTimePM;

public:
	CDateTimeFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);

protected:
   	void	LoadVariable	(int);

	virtual BOOL	SetupControls	();
	virtual void 	FillControls	();
	virtual BOOL	ReadControls	();
	virtual void	CleanupControls	();

protected:
	//{{AFX_MSG(CDateTimeFormatDlg)
	afx_msg void	OnOrderKillFocus		();
	afx_msg void	OnFormatTimeChanged		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//@@ElapsedTime
//============================================================================
class CElapsedTimeFormatDlg : public CFormatDlgRoot
{
protected:
	CBCGPComboBox	m_cboTimeFormat;
	CBCGPEdit	m_edtTimeSeparator;
	CBCGPEdit	m_edtDecSeparator;
	CIntEdit	m_edtDecimals;
	CBCGPButton		m_btnCaptionShowNever;
	CBCGPButton		m_btnCaptionShowLeft;
	CBCGPButton		m_btnCaptionShowRight;

public:
	CElapsedTimeFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);
	
protected:
	virtual BOOL	SetupControls	();
	virtual void 	FillControls	();
	virtual BOOL	ReadControls	();
	virtual void	CleanupControls	();

protected:
	//{{AFX_MSG(CElapsedTimeFormatDlg)
	afx_msg void	OnFormatTimeChanged		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
class CBoolFormatDlg : public CFormatDlgRoot
{
private:
	CBCGPEdit	m_edtValueF;
	CBCGPEdit	m_edtValueT;
	CBCGPButton	m_btnIsBitmap;

public:
	CBoolFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);

protected:
	virtual BOOL	SetupControls	();
	virtual void 	FillControls	();
	virtual BOOL	ReadControls	();

protected:
	//{{AFX_MSG(CBoolFormatDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
class CEnumFormatDlg : public CFormatDlgRoot
{
private:
	CBCGPComboBox	m_cboFormat;

public:
	CEnumFormatDlg (Formatter* pF, CWnd* Parent, BOOL fSystem);

protected:
	virtual BOOL	SetupControls	();
	virtual void 	FillControls	();
	virtual BOOL	ReadControls	();

protected:
	//{{AFX_MSG(CEnumFormatDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//============================================================================
//		CPrompterDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CPrompterDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CPrompterDlg)
	ON_COMMAND		(IDC_FMTMNG_FORMAT, OnFormat)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CPrompterDlg::CPrompterDlg
	(
		Formatter*	pFormatter,
		LPCTSTR		lpszDefault,
		CWnd*		pParent
	)
	:
	CParsedDialog	(IDD_FMTMNG_PROMPTER, pParent),
	m_strDefault	(lpszDefault),
	m_pFormatter	(pFormatter)
{
	m_DataType = m_pFormatter->GetDataType();

	switch (m_DataType.m_wType)
	{
		case DATA_INT_TYPE:	m_pedtIn = new CIntEdit;		break;
		case DATA_LNG_TYPE:	if (m_DataType.IsATime())	//@@ElapsedTime
								m_pedtIn = new CElapsedTimeEdit;
							else
								m_pedtIn = new CLongEdit;
							break;
		case DATA_STR_TYPE:	m_pedtIn = new CStrEdit;		break;
		case DATA_DBL_TYPE:	m_pedtIn = new CDoubleEdit;		break;
		case DATA_MON_TYPE:	m_pedtIn = new CMoneyEdit;		break;
		case DATA_QTA_TYPE:	m_pedtIn = new CQuantityEdit;	break;
		case DATA_PERC_TYPE:m_pedtIn = new CPercEdit;		break;
		case DATA_DATE_TYPE:if (m_DataType.IsFullDate())
								if (m_DataType.IsATime())
									m_pedtIn = new CTimeEdit;
								else
									m_pedtIn = new CDateTimeEdit;
							else
								m_pedtIn = new CDateEdit;
							break;
		case DATA_BOOL_TYPE:m_pedtIn = new CBoolEdit;		break;
		default:			m_pedtIn = NULL;				break;
	}

	m_pDataObj = DataObj::DataObjCreate (m_DataType);

	if (m_pedtIn) m_pedtIn->CParsedCtrl::Attach(m_pDataObj);

	// Il formattatore viene temporanemente messo nella tabella di applicazione
	// (rimosso dal distruttore) in modo che i meccanismi di formattazione funzionino
	// correttamente per il control instanziato.
	Formatter* pFormTmp = PolyNewFormatter(m_pFormatter->GetDataType(), Formatter::FROM_CUSTOM);
	pFormTmp->Assign(*m_pFormatter);
	AfxGetWritableFormatStyleTable()->AddFormatter(pFormTmp);
	FormatIdx nIdx = AfxGetFormatStyleTable()->GetFormatIdx(pFormTmp->GetName());

	if (m_pedtIn) m_pedtIn->AttachFormatter(nIdx);
}

//----------------------------------------------------------------------------
CPrompterDlg::~CPrompterDlg()
{
	if (m_pedtIn)
	{
		delete m_pedtIn;
		// elimina l'entry della FormatTable di applicazione inserito dal costruttore
		// (per non distruggere anche il formattatore bisogna (temporaneamente) dire
		// all'Array che non possiede gli elementi
		//
		FormatStyleTablePtr ptrFormatStyleTable = AfxGetWritableFormatStyleTable();
		ptrFormatStyleTable->SetOwns			(FALSE);
		ptrFormatStyleTable->DeleteFormatter	(m_pFormatter);
		ptrFormatStyleTable->SetOwns			(TRUE);
	}
	SAFE_DELETE(m_pDataObj);
}

//----------------------------------------------------------------------------
BOOL CPrompterDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();

	CString title;
	GetWindowText(title);

	if (m_pedtIn)
		m_pedtIn->SubclassEdit(IDC_FMTMNG_EDIT_INP,	this);

	m_edtOut.	SubclassDlgItem(IDC_FMTMNG_EDIT_OUT, this);
	m_btnFormat.SubclassDlgItem(IDC_FMTMNG_FORMAT, this);

	if (!m_strDefault.IsEmpty())
		m_pedtIn->SetWindowText(m_strDefault);
		
	if (m_pedtIn)
		m_pedtIn->SetFocus();

	return bInit;
}

//----------------------------------------------------------------------------
void CPrompterDlg::OnFormat()
{
	if (!m_pedtIn || !m_pDataObj) return;

	CString	strOutput;
	
	m_pFormatter->FormatDataObj(*m_pDataObj, strOutput);
	m_edtOut.SetWindowText(strOutput);
}

//----------------------------------------------------------------------------
void CPrompterDlg::OnCancel()
{
	EndDialog(FALSE);
}

//============================================================================
//		CIntXlatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CIntXlatDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CIntXlatDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
        
//----------------------------------------------------------------------------
CIntXlatDlg::CIntXlatDlg(CWnd* Parent)
	:
	CParsedDialog(IDD_FMTMNG_INTXTABLE, Parent)
{
}

//----------------------------------------------------------------------------
BOOL CIntXlatDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();

	m_edtDigits[0].SubclassEdit(IDC_FMTMNG_XLAT0, this);
	m_edtDigits[1].SubclassEdit(IDC_FMTMNG_XLAT1, this);
	m_edtDigits[2].SubclassEdit(IDC_FMTMNG_XLAT2, this);
	m_edtDigits[3].SubclassEdit(IDC_FMTMNG_XLAT3, this);
	m_edtDigits[4].SubclassEdit(IDC_FMTMNG_XLAT4, this);
	m_edtDigits[5].SubclassEdit(IDC_FMTMNG_XLAT5, this);
	m_edtDigits[6].SubclassEdit(IDC_FMTMNG_XLAT6, this);
	m_edtDigits[7].SubclassEdit(IDC_FMTMNG_XLAT7, this);
	m_edtDigits[8].SubclassEdit(IDC_FMTMNG_XLAT8, this);
	m_edtDigits[9].SubclassEdit(IDC_FMTMNG_XLAT9, this);

	for (int i = 0; i < 10; i++)
	{
		m_edtDigits[i].SetValue(m_strXlat.Mid(i,1));		
		m_edtDigits[i].SetCtrlStyle(STR_STYLE_NO_EMPTY);
	}   
	
	return bInit;
}

//----------------------------------------------------------------------------
void CIntXlatDlg::OnOK()
{                         
	CString str;
	m_strXlat.Empty();
	
	for (int i = 0; i < 10; i++)
	{          
		m_edtDigits[i].GetWindowText(str);
		if (str.GetLength())
			m_strXlat += str.Left(1);
		else
			m_strXlat += szBlank;
	}
	
	EndDialog(TRUE);
}

//----------------------------------------------------------------------------
void CIntXlatDlg::OnCancel()
{                 
	m_strXlat.Empty();
	EndDialog(FALSE);
}

//============================================================================
//		CDblXlatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CDblXlatDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CDblXlatDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
        
//----------------------------------------------------------------------------
CDblXlatDlg::CDblXlatDlg(CWnd* Parent)
	:
	CParsedDialog(IDD_FMTMNG_DBLXTABLE, Parent)
{
}

//----------------------------------------------------------------------------
BOOL CDblXlatDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();
      
	m_edtDigits[0].SubclassEdit(IDC_FMTMNG_XLAT0, this);
	m_edtDigits[1].SubclassEdit(IDC_FMTMNG_XLAT1, this);
	m_edtDigits[2].SubclassEdit(IDC_FMTMNG_XLAT2, this);
	m_edtDigits[3].SubclassEdit(IDC_FMTMNG_XLAT3, this);
	m_edtDigits[4].SubclassEdit(IDC_FMTMNG_XLAT4, this);
	m_edtDigits[5].SubclassEdit(IDC_FMTMNG_XLAT5, this);
	m_edtDigits[6].SubclassEdit(IDC_FMTMNG_XLAT6, this);
	m_edtDigits[7].SubclassEdit(IDC_FMTMNG_XLAT7, this);
	m_edtDigits[8].SubclassEdit(IDC_FMTMNG_XLAT8, this);
	m_edtDigits[9].SubclassEdit(IDC_FMTMNG_XLAT9, this);
	m_edtDigits[10].SubclassEdit(IDC_FMTMNG_XLAT10, this);
	
	for (int i = 0; i < 11; i++)
	{
		m_edtDigits[i].SetValue(m_strXlat.Mid(i,1));		
		m_edtDigits[i].SetCtrlStyle(STR_STYLE_NO_EMPTY);
	}   

	return bInit;
}

//----------------------------------------------------------------------------
void CDblXlatDlg::OnOK()
{
	CString str;
	m_strXlat.Empty();
	
	for (int i = 0; i < 11; i++)
	{          
		m_edtDigits[i].GetWindowText(str);
		if (str.GetLength())
			m_strXlat += str.Left(1);
		else
			m_strXlat += szBlank;
	}
	
	EndDialog(TRUE);
}

//----------------------------------------------------------------------------
void CDblXlatDlg::OnCancel()
{
	m_strXlat.Empty();
	EndDialog(FALSE);
}

//============================================================================
//		CFormatDlgRoot implementation
//============================================================================
BEGIN_MESSAGE_MAP(CFormatDlgRoot, CParsedDialog)
	//{{AFX_MSG_MAP(CFormatDlgRoot)
	ON_BN_CLICKED	(IDC_FMTMNG_TEST,		OnTest)
	ON_EN_UPDATE	(IDC_FMTMNG_WIDTH,		OnWidthUpdate)
	ON_BN_CLICKED	(IDC_FMTMNG_PADLEFT,	OnLeftClicked)
	ON_BN_CLICKED	(IDC_FMTMNG_PADRIGHT,	OnRightClicked)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CFormatDlgRoot::CFormatDlgRoot
	(
		Formatter*	pFormatter,
		UINT		nResId,
		CWnd*		pWndParent,
		BOOL		bSystem
	)
	:
	CParsedDialog	(nResId, pWndParent),
	m_bModified		(FALSE)
{
	m_pNewFormatter = pFormatter->Clone();
	m_pFormatter	= pFormatter->Clone();
}

//----------------------------------------------------------------------------
CFormatDlgRoot::~CFormatDlgRoot()
{	
	SAFE_DELETE(m_pNewFormatter);
	SAFE_DELETE(m_pFormatter);
}
//----------------------------------------------------------------------------
BOOL CFormatDlgRoot::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();
	
	if (SetupControls())
	{
		FillControls();
		OnWidthUpdate();

		// change dialog title
		CString title;
		GetWindowText(title);
		title = title + szBlank + m_pFormatter->GetName();
		SetWindowText(title);
	}
	else
	{
		CleanupControls();
		EndDialog(-1);
	}
	return bInit;
}
//----------------------------------------------------------------------------
void CFormatDlgRoot::OnOK()
{
	if (!CheckForm())
		return;

	BOOL bOk = TRUE;
	Formatter* pOldFormSource = m_pFormatter->Clone();

	if (!ReadControls())
	{
		SAFE_DELETE(pOldFormSource);
		EndDialog(IDCANCEL);
		return;
	}

	m_edtEpilogue.GetWindowText(m_strEpilogue);
	m_edtPrologue.GetWindowText(m_strPrologue);

	if (m_pFormatter->m_strHead.CompareNoCase(m_strPrologue) != 0)
		m_pFormatter->m_strHead = m_strPrologue;
	if (m_pFormatter->m_strTail.CompareNoCase(m_strEpilogue) != 0)
		m_pFormatter->m_strTail = m_strEpilogue;

	CleanupControls();

	CFormatDlg* pDlg = (CFormatDlg*) GetParent();

	Formatter* pToModify = (Formatter*) pDlg->m_StyleTable.GetFormatter(pOldFormSource, pOldFormSource->GetSource());
	m_bModified = m_pFormatter->Compare(*m_pNewFormatter); 

	if (pToModify && m_bModified)
	{
		pToModify->SetChanged(m_bModified);
		if (!pDlg->m_StyleTable.IsModified())
			pDlg->m_StyleTable.SetModified(TRUE);
	}

	// non è modificata o è stato premuto Cancel
	if (!m_bModified)
	{
		if (pToModify && pOldFormSource->GetSource() == Formatter::FROM_CUSTOM)
			pToModify->SetChanged(FALSE);
		SAFE_DELETE(pOldFormSource);
		if (!bOk)
			EndDialog(IDCANCEL);
		else
			CParsedDialog::OnOK();
		return;
	}
	
	Formatter* pNewFmt = m_pFormatter->Clone();
	pNewFmt->SetChanged(TRUE);

	if (m_pFormatter->m_FromAndTo == Formatter::FROM_STANDARD)
	{
		if (!pDlg->m_bIgnoreIdx)
		{
			pNewFmt->SetOwner(pDlg->m_NsForWoorm);
			pNewFmt->m_FromAndTo = Formatter::FROM_WOORM;
		}
		else
			pNewFmt->m_FromAndTo = Formatter::FROM_CUSTOM;

		// l' utente può toccare solo i formattatori di report
		if (!AfxGetLoginInfos()->m_bAdmin && pNewFmt->m_FromAndTo != Formatter::FROM_WOORM)
		{
			AfxMessageBox(_TB("Warning, only administrator is enabled to edit this no report style!"), MB_APPLMODAL);
			EndDialog(IDCANCEL);
			pToModify->SetChanged(FALSE);
			SAFE_DELETE(pNewFmt);
			SAFE_DELETE(pOldFormSource);
			bOk = FALSE;
			return;
		}

		Formatter* pFormatLocalStd = (Formatter*) pDlg->m_StyleTable.GetFormatter(pNewFmt, pNewFmt->GetSource());
		if (!pFormatLocalStd)
		{
			Formatter* pForLocal =  PolyNewFormatter(pNewFmt->GetDataType(), pNewFmt->GetSource());
			pForLocal->Assign(*pNewFmt);
			pDlg->m_StyleTable.AddFormatter(pForLocal);
			pDlg->InsertInTree(pForLocal);
			if (pNewFmt->m_FromAndTo == Formatter::FROM_WOORM)
				AfxMessageBox(cwsprintf(_TB("A local formatter report style has been generated\nwith name {0-%s}"), pForLocal->GetTitle()), MB_APPLMODAL);
		}
		else
		{
			if (pFormatLocalStd->m_bDeleted)
			{
				pFormatLocalStd->Assign(*pNewFmt);
				pDlg->InsertInTree(pNewFmt);

				// ripeto la message box visto che per l'utente l'effetto è uguale all'insert
				if (pNewFmt->m_FromAndTo == Formatter::FROM_WOORM)
					AfxMessageBox(cwsprintf(_TB("A local formatter report style has been generated\nwith name {0-%s}"), pNewFmt->GetTitle()), MB_APPLMODAL);
			}
			else
			{
				CString sMsg;
				if (pNewFmt->m_FromAndTo == Formatter::FROM_CUSTOM)
					sMsg = cwsprintf(_TB("Custom formatter style '{0-%s}' already exist.\nDo you want overwrite it?"), pNewFmt->GetName());
				else
					sMsg = cwsprintf(_TB("Report formatter style '{0-%s}' already exist.\nDo you want overwrite it?"), pNewFmt->GetName());

				if (AfxMessageBox(sMsg, MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
					pFormatLocalStd->Assign(*pNewFmt);
				else
					bOk = FALSE;
			}
		}				
	}

	// controllo se la provenienza era da WOORM
	if (m_pFormatter->m_FromAndTo == Formatter::FROM_CUSTOM) 
	{	
		// l' utente può toccare solo i formattatori di report
		if (!AfxGetLoginInfos()->m_bAdmin && pNewFmt->m_FromAndTo != Formatter::FROM_WOORM)
		{
			AfxMessageBox(_TB("Warning, only administrator is enabled to edit this no report style!"), MB_APPLMODAL);
			EndDialog(IDCANCEL);
			pToModify->SetChanged(FALSE);
			SAFE_DELETE(pNewFmt);
			SAFE_DELETE(pOldFormSource);
			bOk = FALSE;
			return;
		}

		if (pOldFormSource->m_FromAndTo == Formatter::FROM_WOORM) 
		{
			if (AfxMessageBox(cwsprintf(_TB("Warning, with this operation will be deleted report formatter style: '{0-%s}'.\nDo you want continue?"), pOldFormSource->GetName()), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
			{
				pNewFmt->m_OwnerModule.SetType(CTBNamespace::MODULE);
				pNewFmt->m_OwnerModule.SetObjectName(CTBNamespace::REPORT, _T(""));
				Formatter* pFormatLocal = (Formatter*) pDlg->m_StyleTable.GetFormatter(pOldFormSource, pOldFormSource->GetSource());
				if (!pFormatLocal)
				{
					Formatter* pForLocal =  PolyNewFormatter(pNewFmt->GetDataType(), Formatter::FROM_CUSTOM);
					pForLocal->Assign(*pNewFmt);
					pDlg->m_StyleTable.AddFormatter(pForLocal);
					pDlg->InsertInTree(pForLocal);
				}
				else
				{
					//FormatIdx nIdxFormat = pDlg->m_StyleTable.GetFormatIdx(pNewFmt, pNewFmt->GetSource());
					Formatter* pFormatExist = (Formatter*) pDlg->m_StyleTable.GetFormatter(pNewFmt, pNewFmt->GetSource());
					if (pFormatExist == NULL)
					{
						pFormatLocal->Assign(*pNewFmt);
						pFormatLocal->SetChanged(TRUE);
					}
					else
						if (AfxMessageBox(cwsprintf(_TB("Warning! Custom formatter style '{0-%s}' already exist.\nDo you want overwrite it?"), pNewFmt->GetName()), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
						{
							pFormatExist->Assign(*pNewFmt);
							pFormatExist->SetChanged(TRUE);
							pFormatLocal->SetDeleted(TRUE);
						}
				}
			}
			else
				bOk = FALSE;
		}

		if (pOldFormSource->m_FromAndTo == Formatter::FROM_CUSTOM)
		{
			Formatter* pFormatCst = (Formatter*) pDlg->m_StyleTable.GetFormatter(pNewFmt, pNewFmt->GetSource());
			if (!pFormatCst)
			{
				SAFE_DELETE(pNewFmt);
				SAFE_DELETE(pOldFormSource);
				return;
			}
			pFormatCst->Assign(*pNewFmt);
		}
	}
	
	if (m_pFormatter->m_FromAndTo == Formatter::FROM_WOORM)
	{
		Formatter* pFormatLocalW = (Formatter*) pDlg->m_StyleTable.GetFormatter(pNewFmt, pNewFmt->GetSource());

		if (!pFormatLocalW)
		{
			Formatter* pForWoorm = PolyNewFormatter(pNewFmt->GetDataType(), Formatter::FROM_CUSTOM);
			pForWoorm->Assign(*pNewFmt);
			pDlg->m_StyleTable.AddFormatter(pForWoorm);
			pDlg->InsertInTree(pForWoorm);

		}
		else		// esiste quello di woorm 
		{
			if (pOldFormSource->m_FromAndTo == Formatter::FROM_WOORM)
			{
				if (pFormatLocalW->m_FromAndTo == Formatter::FROM_WOORM)
					pFormatLocalW->Assign(*pNewFmt);
				else
					ASSERT(pNewFmt);
			}
			else
			{
				if (AfxMessageBox(cwsprintf(_TB("Warning! Custom formatter style '{0-%s}' already exist.\nDo you want overwrite it?"), pFormatLocalW->GetName()), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
					if (pFormatLocalW->m_FromAndTo == Formatter::FROM_WOORM)
						pFormatLocalW->Assign(*pNewFmt);
					else
						ASSERT(pNewFmt);
				else
					bOk = FALSE;
			}
		}

		if (pOldFormSource->m_FromAndTo == Formatter::FROM_CUSTOM && bOk)
		{
			FormatIdx nIdxLocal = pDlg->m_StyleTable.GetFormatIdx(pOldFormSource->GetName());
			if (nIdxLocal >=  0) // altrimenti non esiste
				for (int p = 0; p <= pDlg->m_StyleTable.GetAt(nIdxLocal)->GetFormatters().GetUpperBound(); p++)
				{
					Formatter* pFormatOld = (Formatter*) pDlg->m_StyleTable.GetAt(nIdxLocal)->GetFormatters().GetAt(p);
					if (pFormatOld->m_OwnerModule == pOldFormSource->m_OwnerModule && pFormatOld->m_FromAndTo == pOldFormSource->m_FromAndTo)
					{
						pFormatOld->SetDeleted(TRUE);
						break;			
					}
				}	
		}
	}

	SAFE_DELETE(pOldFormSource);
	SAFE_DELETE(pNewFmt);
	CParsedDialog::OnOK();
}

//-----------------------------------------------------------------------------
int CFormatDlgRoot::GetIdxGlobalCustomFormat(Formatter* pFormatter)
{
	FormatIdx idx = AfxGetFormatStyleTable()->GetFormatIdx(pFormatter->GetName());
	
	if (idx < 0)	//non esiste neppure il gruppo
		return -1;

	for (int n = 0; n <= AfxGetFormatStyleTable()->GetAt(idx)->GetFormatters().GetUpperBound(); n++)
	{
		Formatter* pFormat = (Formatter*) AfxGetFormatStyleTable()->GetAt(idx)->GetFormatters().GetAt(n);
		if (
				pFormat->m_FromAndTo		== Formatter::FROM_CUSTOM &&
				pFormatter->m_OwnerModule	== pFormat->m_OwnerModule
		   )
			return n;		
	}
	return -1;
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::OnCancel()
{
	CleanupControls();
	m_pFormatter->m_strHead = m_strOldPrologue;
	m_pFormatter->m_strTail = m_strOldEpilogue;

	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::OnLeftClicked()
{
	m_btnPadLeft.	SetCheck(m_edtPaddedLen.GetValue() != 0);
	m_btnPadRight.	SetCheck(!m_btnPadLeft.GetCheck());
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::OnRightClicked()
{
	m_btnPadRight.	SetCheck(m_edtPaddedLen.GetValue() != 0);
	m_btnPadLeft.	SetCheck(!m_btnPadRight.GetCheck());
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::OnWidthUpdate()
{
	if (m_edtPaddedLen.GetValue())
	{
		
		m_btnPadLeft.EnableWindow(TRUE);
		m_btnPadRight.EnableWindow(TRUE);
		((CStatic*)GetDlgItem(IDC_STATIC_PAD))->EnableWindow(TRUE);
		if (!m_btnPadLeft.GetCheck() && !m_btnPadRight.GetCheck())
			switch (m_pFormatter->m_Align)
			{
				case Formatter::RIGHT :
					m_btnPadRight.SetCheck(1);
					m_btnPadLeft.SetCheck(0);
					break;
				case Formatter::LEFT	:
					m_btnPadRight.SetCheck(0);
					m_btnPadLeft.SetCheck(1);
					break;
				default:
					m_btnPadRight.SetCheck(0);
					m_btnPadLeft.SetCheck(0);
					break;
			}
	}
	else
	{
		m_btnPadLeft.EnableWindow(FALSE);
		m_btnPadRight.EnableWindow(FALSE);
		((CStatic*)GetDlgItem(IDC_STATIC_PAD))->EnableWindow(FALSE);
		switch (m_pFormatter->m_Align)
		{
			case Formatter::RIGHT :
				m_btnPadRight.SetCheck(1);
				m_btnPadLeft.SetCheck(0);
				break;
			case Formatter::LEFT	:
				m_btnPadRight.SetCheck(0);
				m_btnPadLeft.SetCheck(1);
				break;
			default:
				m_btnPadRight.SetCheck(0);
				m_btnPadLeft.SetCheck(0);
				break;
		}
	}

	OnUpdate(IDC_FMTMNG_WIDTH);
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::OnUpdate(int)
{}

//----------------------------------------------------------------------------
void CFormatDlgRoot::OnTest()
{
	m_edtEpilogue.GetWindowText(m_strEpilogue);
	m_edtPrologue.GetWindowText(m_strPrologue);

	if (m_pFormatter->m_strHead.CompareNoCase(m_strPrologue) != 0)
		m_pFormatter->m_strHead = m_strPrologue;
	if (m_pFormatter->m_strTail.CompareNoCase(m_strEpilogue) != 0)
		m_pFormatter->m_strTail = m_strEpilogue;

	Formatter* pCurrent = m_pFormatter->Clone();
	m_pFormatter->m_strName = m_pFormatter->GetName() + _T(" (**tmp@@@microarea@@@tmp**)");

	if (ReadControls())
	{
		CPrompterDlg dlg(m_pFormatter, NULL, this);
		dlg.DoModal();
	}

	m_pFormatter->Assign(*pCurrent);

	SAFE_DELETE(pCurrent);
}
//----------------------------------------------------------------------------
BOOL CFormatDlgRoot::SetupControls()
{
	m_btnTest.		SubclassDlgItem(IDC_FMTMNG_TEST,		this);
	m_btnPadLeft.	SubclassDlgItem(IDC_FMTMNG_PADLEFT,		this);
	m_btnPadRight.	SubclassDlgItem(IDC_FMTMNG_PADRIGHT,	this);

	m_edtPaddedLen.	SubclassEdit(IDC_FMTMNG_WIDTH, this);
	m_edtPaddedLen.	SetRange	(0, 32767);

	m_edtPrologue.	SubclassEdit(IDC_FMTMNG_PREFIX, this);
	m_edtEpilogue.	SubclassEdit(IDC_FMTMNG_POSTFIX, this);

	m_strOldPrologue = m_strPrologue = m_pFormatter->m_strHead;
	m_strOldEpilogue = m_strEpilogue = m_pFormatter->m_strTail;

	m_edtPrologue.SetWindowText(m_strPrologue);
	m_edtEpilogue.SetWindowText(m_strEpilogue);
		
	return TRUE;
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::FillControls()
{
	m_edtPaddedLen.SetValue(m_pFormatter->m_nPaddedLen);

	switch (m_pFormatter->m_Align)
	{
		case Formatter::NONE:
			m_btnPadLeft.SetCheck(0);
			m_btnPadRight.SetCheck(0);
			break;
		case Formatter::LEFT:
			m_btnPadLeft.SetCheck(1);
			m_btnPadRight.SetCheck(0);
			break;
		case Formatter::RIGHT:
			m_btnPadLeft.SetCheck(0);
			m_btnPadRight.SetCheck(1);
			break;
    }
}

//----------------------------------------------------------------------------
BOOL CFormatDlgRoot::ReadControls()
{
	m_pFormatter->m_nPaddedLen = m_edtPaddedLen.GetValue();

	m_pFormatter->m_Align = Formatter::NONE;

	if (m_btnPadLeft.GetCheck())	m_pFormatter->m_Align = Formatter::LEFT;
	if (m_btnPadRight.GetCheck())	m_pFormatter->m_Align = Formatter::RIGHT;

	m_pFormatter->RecalcWidths();

    return TRUE;
}

//----------------------------------------------------------------------------
void CFormatDlgRoot::CleanupControls()
{}

//----------------------------------------------------------------------------
//String
static const IndexIDSTag StringFormatTable[]={
	{ 0, FMT_SHOW_STR_ASIS, CStringFormatter::ASIS },
	{ 1, FMT_SHOW_STR_UPPER, CStringFormatter::UPPERCASE },
	{ 2, FMT_SHOW_STR_LOWER, CStringFormatter::LOWERCASE },
	{ 3, FMT_SHOW_STR_CAPIT, CStringFormatter::CAPITALIZED },
	{ 4, FMT_SHOW_STR_EXPANDED, CStringFormatter::EXPANDED },
	{ 5, FMT_SHOW_STR_MASKED, CStringFormatter::MASKED},
	{ -1, FMT_NULL, 0 }
};


//============================================================================
//		CStringFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CStringFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CStringFormatDlg)
	ON_CBN_SELCHANGE(IDC_FMTMNG_STR_FORMAT,			OnStrFmtSelChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CStringFormatDlg::CStringFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot(pFormatter, IDD_FMTMNG_STRINGFORMAT, Parent, bSystem)
{
}

//----------------------------------------------------------------------------
void CStringFormatDlg::OnStrFmtSelChange()
{
	int nActualFmt = ComboCurSelToTag(m_cboFormat);
	BOOL bExpanded = nActualFmt == CStringFormatter::EXPANDED;
	BOOL bMasked = nActualFmt == CStringFormatter::MASKED;

	GetDlgItem(IDC_FMTMNG_STR_INTER_STRING_LABEL)->EnableWindow(bExpanded);
	m_edtInterChars.EnableWindow(bExpanded);

	GetDlgItem(IDC_FMTMNG_STR_MASK_EDIT)->EnableWindow(bMasked);
	m_edtMask.EnableWindow(bMasked);
	m_btnZeroPadded.EnableWindow(bMasked);
}

//----------------------------------------------------------------------------
BOOL CStringFormatDlg::SetupControls()
{
	if (CFormatDlgRoot::SetupControls())
	{
		m_cboFormat.SubclassDlgItem(IDC_FMTMNG_STR_FORMAT, this);
		m_edtInterChars.SubclassDlgItem(IDC_FMTMNG_STR_INTER_STRING_EDIT, this);
		m_edtMask.SubclassDlgItem(IDC_FMTMNG_STR_MASK_EDIT, this);
		m_btnZeroPadded.SubclassDlgItem(IDC_FMTMNG_STR_ZERO_PADDED, this);
		SetupCombo(m_cboFormat, StringFormatTable);
		return TRUE;
	}
	else
    	return FALSE;
}

//----------------------------------------------------------------------------
void CStringFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();
	CStringFormatter* pBody = (CStringFormatter*) m_pFormatter;

	TagToComboCurSel(m_cboFormat, pBody->m_FormatType);
	m_edtInterChars.SetWindowText(pBody->m_strInterChars);
	m_edtMask.SetWindowText(pBody->GetFormatMask().GetMask());
	m_btnZeroPadded.SetCheck(pBody->IsZeroPadded());
	OnStrFmtSelChange();
}

//----------------------------------------------------------------------------
BOOL CStringFormatDlg::ReadControls()
{
	CStringFormatter* pBody = (CStringFormatter*) m_pFormatter;

	pBody->SetFormat((CStringFormatter::FormatTag) ComboCurSelToTag(m_cboFormat));
	m_edtInterChars.GetWindowText(pBody->m_strInterChars);
	CString strMask;
	m_edtMask.GetWindowText(strMask);
	pBody->SetMask(strMask);
	pBody->SetZeroPadded(m_btnZeroPadded.GetCheck() != 0);

	return CFormatDlgRoot::ReadControls();
}


//----------------------------------------------------------------------------
//Integer
static const IndexIDSTag IntFormatTable[]={
	{ 0, FMT_SHOW_INT_NUM, CIntFormatter::NUMERIC },
	{ 1, FMT_SHOW_INT_LET, CIntFormatter::LETTER },
	{ 2, FMT_SHOW_INT_ENC, CIntFormatter::ENCODED },
	{-1, FMT_NULL, 0 }
};
	
static const IndexIDSTag IntSignTable[]={
	{ 1, FMT_SIGN_INT_MINUSPREFIX,		CIntFormatter::MINUSPREFIX		},
	{ 2, FMT_SIGN_INT_MINUSPOSTFIX,		CIntFormatter::MINUSPOSTFIX		},
	{ 3, FMT_SIGN_INT_SIGNPREFIX,		CIntFormatter::SIGNPREFIX		},
	{ 4, FMT_SIGN_INT_SIGNPOSTFIX,		CIntFormatter::SIGNPOSTFIX		},
	{ 5, FMT_SIGN_INT_USERND,			CIntFormatter::ROUNDS			},
	{ 0, FMT_SIGN_INT_NEVER,			CIntFormatter::ABSOLUTEVAL		},
	{ -1, FMT_NULL, 0 },
};

//============================================================================
//		CIntFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CIntFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CIntFormatDlg)
	ON_CBN_SELCHANGE(IDC_FMTMNG_INT_FORMAT,			OnIntFmtSelChange)
	ON_CBN_SELCHANGE(IDC_FMTMNG_INT_SIGN,			OnIntSignSelChange)
	ON_BN_CLICKED	(IDC_FMTMNG_INTXLAT,			OnIntXlat)
	ON_BN_CLICKED	(IDC_FMTMNG_INT_ZERO_AS_DASH,	OnIntZeroAsDashSelChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CIntFormatDlg::CIntFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot(pFormatter, IDD_FMTMNG_INTFORMAT, Parent, bSystem)
{
}

//----------------------------------------------------------------------------
void CIntFormatDlg::OnIntFmtSelChange()
{           
	OnUpdate(IDC_FMTMNG_INT_FORMAT);
}

//----------------------------------------------------------------------------
void CIntFormatDlg::OnIntSignSelChange()
{                                   
	OnUpdate(IDC_FMTMNG_INT_SIGN);
}

//----------------------------------------------------------------------------
void CIntFormatDlg::OnIntZeroAsDashSelChange()
{                                   
	OnUpdate(IDC_FMTMNG_INT_ZERO_AS_DASH);
}

//----------------------------------------------------------------------------
void CIntFormatDlg::OnUpdate(int /*nCtlOriginator*/)
{
	m_btnShowNullValueAlias.EnableWindow(ComboCurSelToTag(m_cboFormat) == CIntFormatter::NUMERIC);
	m_btnXtable.EnableWindow(ComboCurSelToTag(m_cboFormat) == CIntFormatter::ENCODED);

	BOOL bNullAliased = m_btnShowNullValueAlias.GetCheck() != 0;
	m_cboFormat.EnableWindow(!bNullAliased);
	m_edtNullValueAlias.EnableWindow(bNullAliased);
	m_edtNullValueAlias.SetWindowText
		(
			bNullAliased
				? ((CIntFormatter*)m_pFormatter)->m_strAsZeroValue
				: _T("")
		);

	m_btnZeroPadded.EnableWindow
		(
			(ComboCurSelToTag(m_cboFormat) == CIntFormatter::NUMERIC) &&
			(ComboCurSelToTag(m_cboSign) == CIntFormatter::ABSOLUTEVAL) &&
			(m_edtPaddedLen.GetValue() != 0)
		);
}       

//----------------------------------------------------------------------------
void CIntFormatDlg::OnIntXlat()
{                         
	if (m_pFormatter->GetDataType() == DATA_INT_TYPE)
	{
		CIntFormatter* pIF = (CIntFormatter*)m_pFormatter;

		CIntXlatDlg dlg(this);
		dlg.m_strXlat = pIF->m_strXTable;
		
		if (dlg.DoModal() == IDOK)
			pIF->m_strXTable = dlg.m_strXlat;
	}
	else
	{
		CLongFormatter* pLF = (CLongFormatter*)m_pFormatter;

		CIntXlatDlg dlg(this);
		dlg.m_strXlat = pLF->m_strXTable;
		
		if (dlg.DoModal() == IDOK)
			pLF->m_strXTable = dlg.m_strXlat;
	}
}

//----------------------------------------------------------------------------
BOOL CIntFormatDlg::SetupControls()
{
	if (CFormatDlgRoot::SetupControls())
	{
		m_cboFormat.	SubclassDlgItem(IDC_FMTMNG_INT_FORMAT, this);
		m_cboSign.		SubclassDlgItem(IDC_FMTMNG_INT_SIGN, this);

		m_edtThouSep.	SubclassDlgItem(IDC_FMTMNG_INT_THOUSEP, this);
		m_edtThouSep.	LimitText(1);

		m_btnZeroPadded.SubclassDlgItem(IDC_FMTMNG_INT_ZEROPAD, this);
		m_btnXtable.	SubclassDlgItem(IDC_FMTMNG_INTXLAT, this);

		m_btnShowNullValueAlias.SubclassDlgItem(IDC_FMTMNG_INT_ZERO_AS_DASH, this);
		m_edtNullValueAlias.	SubclassDlgItem(IDC_FMTMNG_INT_ZERO_ALIAS, this);

    	SetupCombo(m_cboFormat, IntFormatTable);
        SetupCombo(m_cboSign, IntSignTable);

		((CStatic*)GetDlgItem(IDC_STATIC_SEP1000))->EnableWindow(TRUE);		
        
		return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void CIntFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();

	CIntFormatter* pBody = (CIntFormatter*)m_pFormatter;

 	if (pBody->m_FormatType == CIntFormatter::ZERO_AS_DASH)
	{
		TagToComboCurSel(m_cboFormat, CIntFormatter::NUMERIC);
 		m_btnShowNullValueAlias.SetCheck(1);
		m_edtNullValueAlias.SetWindowText(pBody->m_strAsZeroValue);
 	}
 	else
 	{
		TagToComboCurSel(m_cboFormat, pBody->m_FormatType);
 		m_btnShowNullValueAlias.SetCheck(0);
		m_edtNullValueAlias.SetWindowText(_T(""));
 	}

	TagToComboCurSel(m_cboSign, pBody->m_Sign);
	m_btnZeroPadded.SetCheck(pBody->m_bZeroPadded);
	m_edtThouSep.SetWindowText(pBody->m_str1000Separator);
	OnUpdate(0);
}

//----------------------------------------------------------------------------
BOOL CIntFormatDlg::ReadControls()
{
	CIntFormatter* pBody = (CIntFormatter*)m_pFormatter;

 	if (m_btnShowNullValueAlias.GetCheck() == 0)
 		pBody->SetFormat((CIntFormatter::FormatTag) ComboCurSelToTag(m_cboFormat));
 	else
	{
		pBody->SetFormat(CLongFormatter::ZERO_AS_DASH);
		m_edtNullValueAlias.GetWindowText(pBody->m_strAsZeroValue);
	}

	pBody->m_Sign = (CLongFormatter::SignTag)ComboCurSelToTag(m_cboSign);
 	pBody->m_bZeroPadded = m_btnZeroPadded.GetCheck();
	m_edtThouSep.GetWindowText(pBody->m_str1000Separator);

	return CFormatDlgRoot::ReadControls();
}

//----------------------------------------------------------------------------
//Double
static const IndexIDSTag DblFormatTable[]={
	{ 0, FMT_SHOW_DBL_FIX, CDblFormatter::FIXED },
	{ 1, FMT_SHOW_DBL_ENC, CDblFormatter::ENCODED },
	{ 2, FMT_SHOW_DBL_EXP, CDblFormatter::EXPONENTIAL },
	{ 3, FMT_SHOW_DBL_ENG, CDblFormatter::ENGINEER },
	{ 4, FMT_SHOW_DBL_LET, CDblFormatter::LETTER },
	{-1, FMT_NULL, 0 }
};

static const IndexIDSTag DblRoundingTable[]={
	{ 0, FMT_ROUND_DBL_NONE, CDblFormatter::ROUND_NONE },
	{ 1, FMT_ROUND_DBL_INF, CDblFormatter::ROUND_INF },
	{ 2, FMT_ROUND_DBL_ZERO, CDblFormatter::ROUND_ZERO },
	{ 3, FMT_ROUND_DBL_ABS, CDblFormatter::ROUND_ABS },
	{ 4, FMT_ROUND_DBL_SIGNED, CDblFormatter::ROUND_SIGNED },
	{-1, FMT_NULL, 0 }
};

static const IndexIDSTag DblSignTable[]={
	{ 1, FMT_SIGN_INT_MINUSPREFIX, CDblFormatter::MINUSPREFIX },
	{ 2, FMT_SIGN_INT_MINUSPOSTFIX, CDblFormatter::MINUSPOSTFIX },
	{ 3, FMT_SIGN_INT_SIGNPREFIX, CDblFormatter::SIGNPREFIX },
	{ 4, FMT_SIGN_INT_SIGNPOSTFIX, CDblFormatter::SIGNPOSTFIX },
	{ 5, FMT_SIGN_INT_USERND, CDblFormatter::ROUNDS },
	{ 0, FMT_SIGN_INT_NEVER, CDblFormatter::ABSOLUTEVAL },
	{-1, FMT_NULL, 0 }
};

//============================================================================
//		CDblFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CDblFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CDblFormatDlg)
	ON_CBN_SELCHANGE	(IDC_FMTMNG_DBL_FORMAT,			OnFormatSelChange)
	ON_CBN_SELCHANGE	(IDC_FMTMNG_DBL_ROUND,			OnDecimalsChanged)
	ON_EN_VALUE_CHANGED	(IDC_FMTMNG_DBL_DECIMALS,		OnDecimalsChanged)
	ON_BN_CLICKED		(IDC_FMTMNG_DBLXLAT,			OnDblXlat)
	ON_BN_CLICKED		(IDC_FMTMNG_DBL_ZERO_AS_DASH,	OnDblZeroAsDashSelChange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDblFormatDlg::CDblFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot(pFormatter, IDD_FMTMNG_DBLFORMAT, Parent, bSystem)
{}

//----------------------------------------------------------------------------
void CDblFormatDlg::OnFormatSelChange()
{
	OnUpdate(IDC_FMTMNG_DBL_FORMAT);
}

//----------------------------------------------------------------------------
void CDblFormatDlg::OnDecimalsChanged()
{
	if	(
			m_edtQuantum.GetValue() == 0. &&
			ComboCurSelToTag(m_cboRounding) != CDblFormatter::ROUND_NONE
		)
	{
		int nDec = m_edtDecimals.GetValue();
		
		double quantum = 1.;
		for (int i = 0; i < nDec; i++) quantum /= 10.;

		// default quantum
		m_edtQuantum.SetValue(quantum);
		m_edtQuantum.SetCtrlNumDec(nDec);
	}
}

//----------------------------------------------------------------------------
void CDblFormatDlg::OnDblZeroAsDashSelChange()
{                                   
	OnUpdate(IDC_FMTMNG_DBL_ZERO_AS_DASH);
}

//----------------------------------------------------------------------------
void CDblFormatDlg::OnUpdate(int /*nCtlOriginator*/)
{
	m_btnShowNullValueAlias.EnableWindow(ComboCurSelToTag(m_cboFormat) == CDblFormatter::FIXED);
	m_btnXtable.EnableWindow(ComboCurSelToTag(m_cboFormat) == CDblFormatter::ENCODED);

	BOOL bNullAliased = m_btnShowNullValueAlias.GetCheck() != 0;
	m_cboFormat.EnableWindow(!bNullAliased);
	m_edtNullValueAlias.EnableWindow(bNullAliased);
	m_edtNullValueAlias.SetWindowText
		(
			bNullAliased
				? ((CDblFormatter*)m_pFormatter)->m_strAsZeroValue
				: _T("")
		);

}

//----------------------------------------------------------------------------
void CDblFormatDlg::OnDblXlat()
{                         
	CDblFormatter* pDF = (CDblFormatter*)m_pFormatter;
	
	CDblXlatDlg dlg(this);
	
	dlg.m_strXlat = pDF->m_strXTable;
	
	if (dlg.DoModal() == IDOK)
		pDF->m_strXTable = dlg.m_strXlat;
}                             

//----------------------------------------------------------------------------
BOOL CDblFormatDlg::SetupControls()
{
	if (CFormatDlgRoot::SetupControls())
	{
		m_cboFormat.	SubclassDlgItem(IDC_FMTMNG_DBL_FORMAT,	this);
		m_cboRounding.	SubclassDlgItem(IDC_FMTMNG_DBL_ROUND,	this);
		m_cboSign.		SubclassDlgItem(IDC_FMTMNG_DBL_SIGN,	this);

		m_edtThouSep.	SubclassDlgItem(IDC_FMTMNG_DBL_THOUSEP,	this);
		m_edtThouSep.	LimitText(1);

		m_edtDecSep.	SubclassDlgItem(IDC_FMTMNG_DBL_DECSEP,	this);
		m_edtDecSep.	LimitText(1);

		m_btnLeading0.	SubclassDlgItem(IDC_FMTMNG_DBL_LEAD0,	this);
		m_btnTrailing0.	SubclassDlgItem(IDC_FMTMNG_DBL_TRAIL0,	this);
		m_btnXtable.	SubclassDlgItem(IDC_FMTMNG_DBLXLAT,		this);

		m_btnShowNullValueAlias.SubclassDlgItem(IDC_FMTMNG_DBL_ZERO_AS_DASH, this);
		m_edtNullValueAlias.	SubclassDlgItem(IDC_FMTMNG_DBL_ZERO_ALIAS, this);

		m_edtDecimals.	SubclassEdit(IDC_FMTMNG_DBL_DECIMALS,	this);
		m_edtDecimals.	SetRange(0, DBL_DIG);

		m_edtQuantum.	SubclassEdit(IDC_FMTMNG_DBL_QUANTUM,	this);
		m_edtQuantum.	SetCtrlNumDec(((CDblFormatter*)m_pFormatter)->m_nDecNumber);
		
		SetupCombo(m_cboFormat, DblFormatTable);
		SetupCombo(m_cboRounding, DblRoundingTable);                                              
		SetupCombo(m_cboSign, DblSignTable); 

		return TRUE;
    }
	return FALSE;
}

//----------------------------------------------------------------------------
void CDblFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();

	CDblFormatter* pBody = (CDblFormatter*)m_pFormatter;

 	if (pBody->m_FormatType == CDblFormatter::ZERO_AS_DASH)
	{
		TagToComboCurSel(m_cboFormat, CDblFormatter::FIXED);
 		m_btnShowNullValueAlias.SetCheck(1);
		m_edtNullValueAlias.SetWindowText(pBody->m_strAsZeroValue);
 	}
 	else
 	{
		TagToComboCurSel(m_cboFormat, pBody->m_FormatType);
 		m_btnShowNullValueAlias.SetCheck(0);
		m_edtNullValueAlias.SetWindowText(_T(""));
 	}

	TagToComboCurSel(m_cboRounding, pBody->m_Rounding);
	TagToComboCurSel(m_cboSign, pBody->m_Sign);

	m_edtThouSep.	SetWindowText(pBody->m_str1000Separator);
	m_edtDecSep.	SetWindowText(pBody->m_strDecSeparator);

	m_btnLeading0.	SetCheck(pBody->m_bShowMSZero);
	m_btnTrailing0.	SetCheck(pBody->m_bShowLSZero);

	m_edtQuantum.	SetValue(pBody->m_nQuantum);
    m_edtDecimals.	SetValue(pBody->m_nDecNumber);
    
    OnUpdate(0);
}

//----------------------------------------------------------------------------
BOOL CDblFormatDlg::ReadControls()
{
	CString str1000Sep;
	CString strDecSep;

	m_edtDecSep.	GetWindowText(strDecSep);
	m_edtThouSep.	GetWindowText(str1000Sep);

	if (!strDecSep.IsEmpty() && !str1000Sep.IsEmpty() && strDecSep == str1000Sep)
	{
		AfxMessageBox(_TB("The thousands separator must be different from the decimal separator"), MB_APPLMODAL | MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	CDblFormatter* pBody = (CDblFormatter*)m_pFormatter;

 	if (m_btnShowNullValueAlias.GetCheck() == 0)
		pBody->SetFormat ((CDblFormatter::FormatTag) ComboCurSelToTag(m_cboFormat));
 	else
	{
		pBody->SetFormat (CDblFormatter::ZERO_AS_DASH);
		m_edtNullValueAlias.GetWindowText(pBody->m_strAsZeroValue);
	}

	pBody->m_Rounding	= (CDblFormatter::RoundingTag)  ComboCurSelToTag(m_cboRounding);
	pBody->m_Sign		= (CDblFormatter::SignTag) ComboCurSelToTag(m_cboSign);

	pBody->m_str1000Separator	= str1000Sep;
	pBody->m_strDecSeparator	= strDecSep;

	pBody->m_bShowMSZero = m_btnLeading0.GetCheck();
	pBody->m_bShowLSZero = m_btnTrailing0.GetCheck();

	pBody->m_nQuantum	= m_edtQuantum.GetValue();
	pBody->m_nDecNumber = m_edtDecimals.GetValue();

	return CFormatDlgRoot::ReadControls();
}

//----------------------------------------------------------------------------
//Date
static const IndexIDSTag DateDayOptsTable[]={
	{ 0, FMT_TEMPL_DAT_DF99, CDateFormatHelper::DAY99 },
	{ 1, FMT_TEMPL_DAT_DFB9, CDateFormatHelper::DAYB9 },
	{ 2, FMT_TEMPL_DAT_DF9, CDateFormatHelper::DAY9 },
	{ -1, FMT_NULL, 0 }
};

static const IndexIDSTag DateMonthOptsTable[]={
	{ 0, FMT_TEMPL_DAT_MF99, CDateFormatHelper::MONTH99 },
	{ 1, FMT_TEMPL_DAT_MFB9, CDateFormatHelper::MONTHB9 },
	{ 2, FMT_TEMPL_DAT_MF9, CDateFormatHelper::MONTH9 },
	{ 3, FMT_TEMPL_DAT_MFS3, CDateFormatHelper::MONTHS3 }, 
	{ 4, FMT_TEMPL_DAT_MFSX, CDateFormatHelper::MONTHSX },
	{ -1, FMT_NULL, 0 }
};

static const IndexIDSTag DateYearOptsTable[]={
	{ 0, FMT_TEMPL_DAT_YF99, CDateFormatHelper::YEAR99 },
	{ 1, FMT_TEMPL_DAT_YF999, CDateFormatHelper::YEAR999 },
	{ 2, FMT_TEMPL_DAT_YF9999, CDateFormatHelper::YEAR9999 },
	{ -1, FMT_NULL, 0 }
};

static const IndexIDSTag DateOrderTable[]={
	{ 0, FMT_SHOW_DAT_DMY, CDateFormatHelper::DATE_DMY },
	{ 1, FMT_SHOW_DAT_MDY, CDateFormatHelper::DATE_MDY },
	{ 2, FMT_SHOW_DAT_YMD, CDateFormatHelper::DATE_YMD },
	{ -1, FMT_NULL, 0 }
};
	
static const IndexIDSTag DateWeekdayTable[]={
	{ 0, FMT_SHOW_DAT_NOWD, CDateFormatHelper::NOWEEKDAY },
	{ 1, FMT_SHOW_DAT_WDDATE, CDateFormatHelper::PREFIXWEEKDAY },
	{ 2, FMT_SHOW_DAT_DATEWD, CDateFormatHelper::POSTFIXWEEKDAY },
	{ -1, FMT_NULL, 0 }
};

//Time
static const IndexIDSTag TimeFormatTable[]={
	{ 0, FMT_TEMPL_TIM_H99, CDateFormatHelper::TIME_HF99 },
	{ 1, FMT_TEMPL_TIM_HB9, CDateFormatHelper::TIME_HFB9 },
	{ 2, FMT_TEMPL_TIM_H9, CDateFormatHelper::TIME_HF9 },
	{ 3, FMT_TEMPL_TIM_H99_AMPM, CDateFormatHelper::TIME_HF99 | CDateFormatHelper::TIME_AMPM },
	{ 4, FMT_TEMPL_TIM_HB9_AMPM, CDateFormatHelper::TIME_HFB9 | CDateFormatHelper::TIME_AMPM },
	{ 5, FMT_TEMPL_TIM_H9_AMPM, CDateFormatHelper::TIME_HF9 | CDateFormatHelper::TIME_AMPM },
	{ 6, FMT_TEMPL_TIM_H99_NOSEC, CDateFormatHelper::TIME_HF99 | CDateFormatHelper::TIME_NOSEC },
	{ 7, FMT_TEMPL_TIM_HB9_NOSEC, CDateFormatHelper::TIME_HFB9 | CDateFormatHelper::TIME_NOSEC },
	{ 8, FMT_TEMPL_TIM_H9_NOSEC, CDateFormatHelper::TIME_HF9 | CDateFormatHelper::TIME_NOSEC },
	{ 9, FMT_TEMPL_TIM_H99_AMPM_NOSEC, CDateFormatHelper::TIME_HF99 | CDateFormatHelper::TIME_AMPM | CDateFormatHelper::TIME_NOSEC },
	{ 10,FMT_TEMPL_TIM_HB9_AMPM_NOSEC, CDateFormatHelper::TIME_HFB9 | CDateFormatHelper::TIME_AMPM | CDateFormatHelper::TIME_NOSEC },
	{ 11,FMT_TEMPL_TIM_H9_AMPM_NOSEC, CDateFormatHelper::TIME_HF9 | CDateFormatHelper::TIME_AMPM | CDateFormatHelper::TIME_NOSEC },
	{ -1,FMT_NULL, 0 }
};

//============================================================================
//		CDateTimeFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CDateTimeFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CDateTimeFormatDlg)
	ON_CBN_SELCHANGE	(IDC_FMTMNG_DATE_ORDER,		OnOrderKillFocus)
	ON_CBN_SELCHANGE	(IDC_FMTMNG_TIME_FORMAT,	OnFormatTimeChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDateTimeFormatDlg::CDateTimeFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot
		(
			pFormatter,
			pFormatter->GetDataType().IsFullDate()
				? (pFormatter->GetDataType().IsATime() ? IDD_FMTMNG_TIMEFORMAT : IDD_FMTMNG_DATETIMEFORMAT)
				: IDD_FMTMNG_DATEFORMAT,
			Parent,
			bSystem
		)
{}

//----------------------------------------------------------------------------
void CDateTimeFormatDlg::OnFormatTimeChanged()
{
	BOOL bTimeAMPM = (ComboCurSelToTag(m_cboTimeFormat) & CDateFormatHelper::TIME_AMPM) == CDateFormatHelper::TIME_AMPM;

	m_edtTimeAM.EnableWindow(bTimeAMPM);
	m_edtTimePM.EnableWindow(bTimeAMPM);
}

//----------------------------------------------------------------------------
void CDateTimeFormatDlg::OnOrderKillFocus()
{
	CDateFormatter* pBody = (CDateFormatter*)m_pFormatter;

	if (pBody->m_FormatType != ComboCurSelToTag(m_cboOrder))
	{
		ReadControls();

		m_cboDay.ResetContent();
		m_cboMth.ResetContent();
		m_cboYer.ResetContent();

		m_cboDay.Detach();
		m_cboMth.Detach();
		m_cboYer.Detach();

		LoadVariable(pBody->m_FormatType);

		FillControls();
    }
}

//----------------------------------------------------------------------------
void CDateTimeFormatDlg::LoadVariable(int fmt)
{
	UINT id_day, id_mth, id_yer;
	HWND hwnd = NULL;
			
	switch (fmt)
	{
		case CDateFormatHelper::DATE_DMY:
			id_day = IDC_FMTMNG_DATE_1LABEL;
			id_mth = IDC_FMTMNG_DATE_2LABEL;
			id_yer = IDC_FMTMNG_DATE_3LABEL;
			GetDlgItem(IDC_FMTMNG_DATE_1COMBO, &hwnd);
			m_cboDay.Attach(hwnd);
			GetDlgItem(IDC_FMTMNG_DATE_2COMBO, &hwnd);
			m_cboMth.Attach(hwnd);
			GetDlgItem(IDC_FMTMNG_DATE_3COMBO, &hwnd);
			m_cboYer.Attach(hwnd);
			break;
		case CDateFormatHelper::DATE_MDY:
			id_day = IDC_FMTMNG_DATE_2LABEL;
			id_mth = IDC_FMTMNG_DATE_1LABEL;
			id_yer = IDC_FMTMNG_DATE_3LABEL;

			GetDlgItem(IDC_FMTMNG_DATE_2COMBO, &hwnd);
			m_cboDay.Attach(hwnd);
			GetDlgItem(IDC_FMTMNG_DATE_1COMBO, &hwnd);
			m_cboMth.Attach(hwnd);
			GetDlgItem(IDC_FMTMNG_DATE_3COMBO, &hwnd);
			m_cboYer.Attach(hwnd);
			break;
		case CDateFormatHelper::DATE_YMD:
			id_day = IDC_FMTMNG_DATE_3LABEL;
			id_mth = IDC_FMTMNG_DATE_2LABEL;
			id_yer = IDC_FMTMNG_DATE_1LABEL;
			
			GetDlgItem(IDC_FMTMNG_DATE_3COMBO, &hwnd);
			m_cboDay.Attach(hwnd);
			GetDlgItem(IDC_FMTMNG_DATE_2COMBO, &hwnd);
			m_cboMth.Attach(hwnd);
			GetDlgItem(IDC_FMTMNG_DATE_1COMBO, &hwnd);
			m_cboYer.Attach(hwnd);
			break;
	}

	SetDlgItemText(id_day, _TB("Day"));
	SetDlgItemText(id_mth, _TB("Month"));
	SetDlgItemText(id_yer, _TB("Year"));

	SetupCombo(m_cboDay, DateDayOptsTable);
	SetupCombo(m_cboMth, DateMonthOptsTable);
	SetupCombo(m_cboYer, DateYearOptsTable);
}

//----------------------------------------------------------------------------
BOOL CDateTimeFormatDlg::SetupControls()
{
	if (!CFormatDlgRoot::SetupControls())
    	return FALSE;

	CDateFormatter* pBody = (CDateFormatter*)m_pFormatter;

	if (pBody->GetDataType().IsFullDate())
	{
		m_cboTimeFormat.	SubclassDlgItem		(IDC_FMTMNG_TIME_FORMAT, this);
		
		m_edtTimeSeparator.	SubclassDlgItem	(IDC_FMTMNG_TIME_SEP, this);
		m_edtTimeSeparator.	LimitText(1);

		m_edtTimeAM.	SubclassDlgItem			(IDC_FMTMNG_TIME_AM_STR, this);
		m_edtTimePM.	SubclassDlgItem			(IDC_FMTMNG_TIME_PM_STR, this);

		SetupCombo(m_cboTimeFormat, TimeFormatTable);
	}

	// se non e` solo time si attiva la gestione anche della data
	if (!pBody->GetDataType().IsATime())
	{
		m_cboOrder.		SubclassDlgItem	(IDC_FMTMNG_DATE_ORDER, this);
		m_cboWeekday.	SubclassDlgItem	(IDC_FMTMNG_DATE_WEEKDAY, this);

		m_edtFirstSep.	SubclassDlgItem	(IDC_FMTMNG_DATE_1SEP, this);
		m_edtFirstSep.	LimitText(1);

		m_edtSecondSep.	SubclassDlgItem	(IDC_FMTMNG_DATE_2SEP, this);
		m_edtSecondSep.	LimitText(1);

		LoadVariable(pBody->m_FormatType);

		SetupCombo(m_cboOrder,		DateOrderTable);
		SetupCombo(m_cboWeekday,	DateWeekdayTable);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void CDateTimeFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();
	CDateFormatter* pBody = (CDateFormatter*)m_pFormatter;

	if (pBody->GetDataType().IsFullDate())
	{
		TagToComboCurSel(m_cboTimeFormat, pBody->m_TimeFormat & ~CDateFormatHelper::TIME_ONLY);

		m_edtTimeSeparator.SetWindowText(pBody->m_strTimeSeparator);
		m_edtTimeAM.SetWindowText(pBody->m_strTimeAM);
		m_edtTimePM.SetWindowText(pBody->m_strTimePM);

		OnFormatTimeChanged();
	}

	if (!pBody->GetDataType().IsATime())
	{
		TagToComboCurSel(m_cboOrder, pBody->m_FormatType);
		TagToComboCurSel(m_cboWeekday, pBody->m_WeekdayFormat);
		TagToComboCurSel(m_cboDay, pBody->m_DayFormat);
		TagToComboCurSel(m_cboMth, pBody->m_MonthFormat);
		TagToComboCurSel(m_cboYer, pBody->m_YearFormat);

		m_edtFirstSep.SetWindowText(pBody->m_strFirstSeparator);
		m_edtSecondSep.SetWindowText(pBody->m_strSecondSeparator);
	}
}

//----------------------------------------------------------------------------
BOOL CDateTimeFormatDlg::ReadControls()
{
	CDateFormatter* pBody = (CDateFormatter*)m_pFormatter;

	if (pBody->GetDataType().IsFullDate())
	{
		pBody->m_TimeFormat =	(CDateFormatHelper::TimeFormatTag) ComboCurSelToTag(m_cboTimeFormat);
		if (pBody->GetDataType().IsATime())
			pBody->m_TimeFormat = (CDateFormatHelper::TimeFormatTag) (pBody->m_TimeFormat | CDateFormatHelper::TIME_ONLY);

		m_edtTimeSeparator.GetWindowText(pBody->m_strTimeSeparator);
		m_edtTimeAM.GetWindowText(pBody->m_strTimeAM);
		m_edtTimePM.GetWindowText(pBody->m_strTimePM);

	}

	if (!pBody->GetDataType().IsATime())
	{
		pBody->SetFormat ((CDateFormatHelper::FormatTag) ComboCurSelToTag(m_cboOrder));
		pBody->m_WeekdayFormat = (CDateFormatHelper::WeekdayFormatTag) ComboCurSelToTag(m_cboWeekday);
		pBody->m_DayFormat = (CDateFormatHelper::DayFormatTag) ComboCurSelToTag(m_cboDay);
		pBody->m_MonthFormat = (CDateFormatHelper::MonthFormatTag) ComboCurSelToTag(m_cboMth);
		pBody->m_YearFormat = (CDateFormatHelper::YearFormatTag) ComboCurSelToTag(m_cboYer);

		m_edtFirstSep.GetWindowText(pBody->m_strFirstSeparator);
		m_edtSecondSep.GetWindowText(pBody->m_strSecondSeparator);
	}

	return CFormatDlgRoot::ReadControls();
}

//----------------------------------------------------------------------------
void CDateTimeFormatDlg::CleanupControls()
{
	CFormatDlgRoot::CleanupControls();

	if (!m_pFormatter->GetDataType().IsATime())
	{
		m_cboDay.Detach();
		m_cboMth.Detach();
		m_cboYer.Detach();
	}
}

//@@ElapsedTime
static const IndexIDSTag ElapsedTimeFormatTable[]={
	{ 0,	FMT_ELAPSED_TIME_DHMSF,		CElapsedTimeFormatHelper::TIME_DHMSF },
	{ 1,	FMT_ELAPSED_TIME_DHMS,		CElapsedTimeFormatHelper::TIME_DHMS },
	{ 2,	FMT_ELAPSED_TIME_DHM,		CElapsedTimeFormatHelper::TIME_DHM },
	{ 3,	FMT_ELAPSED_TIME_DH,		CElapsedTimeFormatHelper::TIME_DH },
	{ 4,	FMT_ELAPSED_TIME_D,			CElapsedTimeFormatHelper::TIME_D },
	{ 5,	FMT_ELAPSED_TIME_HMSF,		CElapsedTimeFormatHelper::TIME_HMSF },
	{ 6,	FMT_ELAPSED_TIME_HMS,		CElapsedTimeFormatHelper::TIME_HMS },
	{ 7,	FMT_ELAPSED_TIME_HM,		CElapsedTimeFormatHelper::TIME_HM },
	{ 8,	FMT_ELAPSED_TIME_H,			CElapsedTimeFormatHelper::TIME_H },
	{ 9,	FMT_ELAPSED_TIME_MSF,		CElapsedTimeFormatHelper::TIME_MSF },
	{ 10,	FMT_ELAPSED_TIME_MS,		CElapsedTimeFormatHelper::TIME_MSEC },
	{ 11,	FMT_ELAPSED_TIME_M,			CElapsedTimeFormatHelper::TIME_M },
	{ 12,	FMT_ELAPSED_TIME_SF,		CElapsedTimeFormatHelper::TIME_SF },
	{ 13,	FMT_ELAPSED_TIME_S,			CElapsedTimeFormatHelper::TIME_S },
	{ 14,	FMT_ELAPSED_TIME_DHMCM,		CElapsedTimeFormatHelper::TIME_DHMCM },
	{ 15,	FMT_ELAPSED_TIME_DHCH,		CElapsedTimeFormatHelper::TIME_DHCH },
	{ 16,	FMT_ELAPSED_TIME_DCD,		CElapsedTimeFormatHelper::TIME_DCD },
	{ 17,	FMT_ELAPSED_TIME_HMCM,		CElapsedTimeFormatHelper::TIME_HMCM },
	{ 18,	FMT_ELAPSED_TIME_HCH,		CElapsedTimeFormatHelper::TIME_HCH },
	{ 19,	FMT_ELAPSED_TIME_MCM,		CElapsedTimeFormatHelper::TIME_MCM },
	{ 20,	FMT_ELAPSED_TIME_CH,		CElapsedTimeFormatHelper::TIME_CH },	
	{ -1,	FMT_NULL,  0 }
};
//@@ElapsedTime - ridotta per non gestire le frazioni di secondo
// che dipendono dalla precisione scelta per i secondi
static const IndexIDSTag ElapsedTimeLimitedFormatTable[]={
	{ 0, FMT_ELAPSED_TIME_DHMS,		CElapsedTimeFormatHelper::TIME_DHMS },
	{ 1, FMT_ELAPSED_TIME_DHM,		CElapsedTimeFormatHelper::TIME_DHM },
	{ 2, FMT_ELAPSED_TIME_DH,		CElapsedTimeFormatHelper::TIME_DH },
	{ 3, FMT_ELAPSED_TIME_D,		CElapsedTimeFormatHelper::TIME_D },
	{ 4, FMT_ELAPSED_TIME_HMS,		CElapsedTimeFormatHelper::TIME_HMS },
	{ 5, FMT_ELAPSED_TIME_HM,		CElapsedTimeFormatHelper::TIME_HM },
	{ 6, FMT_ELAPSED_TIME_H,		CElapsedTimeFormatHelper::TIME_H },
	{ 7, FMT_ELAPSED_TIME_MS,		CElapsedTimeFormatHelper::TIME_MSEC },
	{ 8, FMT_ELAPSED_TIME_M,		CElapsedTimeFormatHelper::TIME_M },
	{ 9, FMT_ELAPSED_TIME_S,		CElapsedTimeFormatHelper::TIME_S },
	{ 10,FMT_ELAPSED_TIME_DHMCM,	CElapsedTimeFormatHelper::TIME_DHMCM },
	{ 11,FMT_ELAPSED_TIME_DHCH,		CElapsedTimeFormatHelper::TIME_DHCH },
	{ 12,FMT_ELAPSED_TIME_DCD,		CElapsedTimeFormatHelper::TIME_DCD },
	{ 13,FMT_ELAPSED_TIME_HMCM,		CElapsedTimeFormatHelper::TIME_HMCM },
	{ 14,FMT_ELAPSED_TIME_HCH,		CElapsedTimeFormatHelper::TIME_HCH },
	{ 15,FMT_ELAPSED_TIME_MCM,		CElapsedTimeFormatHelper::TIME_MCM },
	{ 16,FMT_ELAPSED_TIME_CH,		CElapsedTimeFormatHelper::TIME_CH },
	{ -1, FMT_NULL, 0 }
};

//============================================================================
//@@ElapsedTime		CElapsedTimeFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CElapsedTimeFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CElapsedTimeFormatDlg)
	ON_CBN_SELCHANGE	(IDC_FMTMNG_TIME_FORMAT,	OnFormatTimeChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CElapsedTimeFormatDlg::CElapsedTimeFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot(pFormatter, IDD_FMTMNG_ELAPSEDTIMEFORMAT, Parent, bSystem)
{}

//----------------------------------------------------------------------------
void CElapsedTimeFormatDlg::OnFormatTimeChanged()
{
	BOOL bCent = (ComboCurSelToTag(m_cboTimeFormat) & CElapsedTimeFormatHelper::TIME_DEC) != 0;
	BOOL bCentHour = ComboCurSelToTag(m_cboTimeFormat) == CElapsedTimeFormatHelper::TIME_CH;

	m_edtDecSeparator.	EnableWindow(bCent);
	m_edtDecimals.		EnableWindow(bCent);
	m_edtTimeSeparator.	EnableWindow(!bCentHour);
	((CStatic*)GetDlgItem(IDC_STATIC_CIFRE))->EnableWindow(bCent);
	((CStatic*)GetDlgItem(IDC_STATIC_SEP))->EnableWindow(bCent);
}

//----------------------------------------------------------------------------
BOOL CElapsedTimeFormatDlg::SetupControls()
{
	if (!CFormatDlgRoot::SetupControls())
    	return FALSE;

	m_cboTimeFormat.	SubclassDlgItem	(IDC_FMTMNG_TIME_FORMAT,		this);

	// se non gestisco la precisione dei secondi carico la tabella limited
	SetupCombo			(m_cboTimeFormat, (DataLng::GetElapsedTimePrecision() == PRECISON_ZERO) 
											? ElapsedTimeLimitedFormatTable 
											: ElapsedTimeFormatTable
						);

	m_edtTimeSeparator.	SubclassDlgItem	(IDC_FMTMNG_TIME_SEP,		this);
	m_edtTimeSeparator.	LimitText(1);

	m_edtDecSeparator.	SubclassDlgItem	(IDC_FMTMNG_TIME_DEC_SEP,	this);
	m_edtDecSeparator.	LimitText		(1);

	m_edtDecimals.	SubclassEdit		(IDC_FMTMNG_TIME_DECIMALS,	this);
	m_edtDecimals.	SetRange			(1, DBL_DIG);

	m_btnCaptionShowNever.	SubclassDlgItem	(IDC_FMT_ELAPSED_TIME_NOSHOW_CAPT,	this);
	m_btnCaptionShowLeft.	SubclassDlgItem	(IDC_FMT_ELAPSED_TIME_LEFT_CAPT,	this);
	m_btnCaptionShowRight.	SubclassDlgItem	(IDC_FMT_ELAPSED_TIME_RIGHT_CAPT,	this);

	return TRUE;
}

//----------------------------------------------------------------------------
void CElapsedTimeFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();
	CElapsedTimeFormatter* pBody = (CElapsedTimeFormatter*)m_pFormatter;

	TagToComboCurSel	(m_cboTimeFormat, pBody->m_FormatType);
	OnFormatTimeChanged	();

	m_edtTimeSeparator.	SetWindowText(pBody->m_strTimeSeparator);

	m_edtDecSeparator.	SetWindowText(pBody->m_strDecSeparator);
    m_edtDecimals.		SetValue(pBody->m_nDecNumber);

	m_btnCaptionShowNever.	SetCheck(pBody->m_nCaptionPos == 0);
	m_btnCaptionShowLeft.	SetCheck(pBody->m_nCaptionPos == T_LEFT);
	m_btnCaptionShowRight.	SetCheck(pBody->m_nCaptionPos == T_RIGHT);
}

//----------------------------------------------------------------------------
BOOL CElapsedTimeFormatDlg::ReadControls()
{
	CElapsedTimeFormatter* pBody = (CElapsedTimeFormatter*)m_pFormatter;

	pBody->SetFormat ((CElapsedTimeFormatHelper::FormatTag)	ComboCurSelToTag(m_cboTimeFormat));

	m_edtTimeSeparator.		GetWindowText(pBody->m_strTimeSeparator);

	m_edtDecSeparator.		GetWindowText(pBody->m_strDecSeparator);
	pBody->m_nDecNumber =	m_edtDecimals.GetValue();

	if (m_btnCaptionShowNever.GetCheck())
		pBody->m_nCaptionPos = 0;

	if (m_btnCaptionShowLeft.GetCheck())
		pBody->m_nCaptionPos = T_LEFT;

	if (m_btnCaptionShowRight.GetCheck())
		pBody->m_nCaptionPos = T_RIGHT;

	return CFormatDlgRoot::ReadControls();
}

//----------------------------------------------------------------------------
void CElapsedTimeFormatDlg::CleanupControls()
{
	CFormatDlgRoot::CleanupControls();
}

//============================================================================
//		CBoolFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CBoolFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CBoolFormatDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CBoolFormatDlg::CBoolFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot(pFormatter, IDD_FMTMNG_BOOLFORMAT, Parent, bSystem)
{
}

//----------------------------------------------------------------------------
BOOL CBoolFormatDlg::SetupControls()
{
	if (CFormatDlgRoot::SetupControls())
	{
		m_edtValueF.SubclassDlgItem(IDC_FMTMNG_BOOL_VALUEF, this);
		m_edtValueT.SubclassDlgItem(IDC_FMTMNG_BOOL_VALUET, this);
		m_btnIsBitmap.SubclassDlgItem(IDC_FMTMNG_BOOL_ISBITMAP, this);

		return TRUE;
	}
	else
    	return FALSE;
}

//----------------------------------------------------------------------------
void CBoolFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();

	CBoolFormatter* pBody = (CBoolFormatter*)m_pFormatter; 

	m_edtValueF.SetWindowText(pBody->GetFalseTag());
	m_edtValueT.SetWindowText(pBody->GetTrueTag());

	m_btnIsBitmap.SetCheck(pBody->m_FormatType);
}

//----------------------------------------------------------------------------
BOOL CBoolFormatDlg::ReadControls()
{
	CBoolFormatter* pBody = (CBoolFormatter*)m_pFormatter;

	CString strTag;
	m_edtValueF.GetWindowText(strTag);
	if (strTag != pBody->GetFalseTag())
		pBody->m_strFalseTag = strTag;

	m_edtValueT.GetWindowText(strTag);
	if (strTag != pBody->GetTrueTag())
		pBody->m_strTrueTag = strTag;

	pBody->SetFormat ((CBoolFormatter::FormatTag) m_btnIsBitmap.GetCheck());

	return CFormatDlgRoot::ReadControls();
}

//Enumerativi
static const IndexIDSTag EnumFormatTable[]={
	{ 0, FMT_ASIS, 			CEnumFormatter::ASIS			},
	{ 1, FMT_FIRSTLETTER,	CEnumFormatter::FIRSTLETTER 	},
	{ 2, FMT_CAPIT, 		CEnumFormatter::CAPITALIZED		},
	{ 3, FMT_UPPER, 		CEnumFormatter::UPPERCASE		},
	{ 4, FMT_LOWER, 		CEnumFormatter::LOWERCASE		},
	{ -1, FMT_NULL, 0 }
};

//============================================================================
//		CEnumFormatDlg implementation
//============================================================================
BEGIN_MESSAGE_MAP(CEnumFormatDlg, CFormatDlgRoot)
	//{{AFX_MSG_MAP(CEnumFormatDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CEnumFormatDlg::CEnumFormatDlg
	(
		Formatter*	pFormatter,
		CWnd*		Parent,
		BOOL		bSystem
	)
	:
	CFormatDlgRoot(pFormatter, IDD_FMTMNG_ENUMFORMAT, Parent, bSystem)
{}
	
//----------------------------------------------------------------------------
BOOL CEnumFormatDlg::SetupControls()
{
	if (CFormatDlgRoot::SetupControls())
	{
		m_cboFormat.SubclassDlgItem(IDC_FMTMNG_ENUM_FORMAT, this);

		SetupCombo(m_cboFormat, EnumFormatTable);
		return TRUE;
	}
	
   	return FALSE;
}

//----------------------------------------------------------------------------
void CEnumFormatDlg::FillControls()
{
	CFormatDlgRoot::FillControls();
	CEnumFormatter* pBody = (CEnumFormatter*) m_pFormatter;

	TagToComboCurSel(m_cboFormat, pBody->m_FormatType);
}

//----------------------------------------------------------------------------
BOOL CEnumFormatDlg::ReadControls()
{
	CEnumFormatter* pBody = (CEnumFormatter*) m_pFormatter;

	pBody->SetFormat ((CEnumFormatter::FormatTag) ComboCurSelToTag(m_cboFormat));

	return CFormatDlgRoot::ReadControls();
}

//==========================================================================
//							CTreeFormatDialog
//==========================================================================
BEGIN_MESSAGE_MAP(CTreeFormatDialog, CTBTreeCtrl)
	ON_WM_KEYDOWN		()	
	ON_WM_KEYUP			()	
	ON_NOTIFY_REFLECT	(TVN_BEGINLABELEDIT,		OnItemBeginEdit)
	ON_NOTIFY_REFLECT	(TVN_ENDLABELEDIT,			OnItemEndEdit)
	ON_COMMAND(ID_OPENFORMAT, OnOpen)
	ON_COMMAND(ID_DELETEFORMAT, OnDelete)
	ON_COMMAND(ID_COPYTBFORMAT, OnCopy)
	ON_COMMAND(ID_PASTETBFORMAT, OnPaste)
	ON_COMMAND(ID_RENAMEFORMAT, OnRename)
	ON_COMMAND(ID_CUTFORMAT, OnCut)
	ON_COMMAND(ID_FORMAT_APPAREA, OnContextArea)
	ON_COMMAND(ID_REFRESHFORMAT, OnRefreshFormatter)
  	
	ON_WM_RBUTTONDOWN	()
	ON_WM_CONTEXTMENU	()
END_MESSAGE_MAP()
//--------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTreeFormatDialog, CTBTreeCtrl)
//--------------------------------------------------------------------------
CTreeFormatDialog::CTreeFormatDialog()
	:
	m_bAfterCtrl	(FALSE)
{}

//---------------------------------------------------------------------------
void CTreeFormatDialog::ExpandAll(HTREEITEM hItem, UINT nCode)
{
	Expand(hItem, nCode);
	HTREEITEM hChildItem = GetChildItem(hItem);
	while (hChildItem) 
	{
		ExpandAll(hChildItem, nCode);
		hChildItem = GetNextSiblingItem(hChildItem);
	}
}

//---------------------------------------------------------------------------
void CTreeFormatDialog::ExpandAll(UINT nCode)
{
	HTREEITEM hItem = GetRootItem();

	while(hItem)
	{
		Expand(hItem, nCode);
		HTREEITEM hChildItem = GetChildItem(hItem);
		while (hChildItem) 
		{
			ExpandAll(hChildItem, nCode);
			hChildItem = GetNextSiblingItem(hChildItem);
		}
		hItem = GetNextSiblingItem(hItem);
	}
}

//---------------------------------------------------------------------------
void CTreeFormatDialog::OnRButtonDown(UINT nFlags, CPoint point) 
{		
	__super::OnRButtonDown(nFlags, point);
	HTREEITEM hItem = HitTest(point);
	SelectItem(hItem);
	OnContextMenu(this, point);
}

//--------------------------------------------------------------------------
void CTreeFormatDialog::OnContextMenu(CWnd* pWnd, CPoint mousePos) 
{
	CRect		rcTree;	
	HTREEITEM	hItemToSelect = GetSelectedItem();
	
	if (hItemToSelect) 
	{
		CLocalizableMenu	menu;
		CMenu*		pPopup	= NULL; 
		CFormatDlg* pDlg	= (CFormatDlg*) GetParent();
		/*
IDR_MENU_FORMATDIALOG MENU
BEGIN
    POPUP "Menu"
    BEGIN
        MENUITEM "Properties",                  ID_OPENFORMAT
        MENUITEM "Apply in ...",                ID_FORMAT_APPAREA
        MENUITEM SEPARATOR
        MENUITEM "Cut   (Ctrl+X)",              ID_CUTFORMAT
        MENUITEM "Copy (Ctrl+C)",               ID_COPYTBFORMAT
        MENUITEM "Paste (Ctrl+V)",              ID_PASTETBFORMAT
        MENUITEM SEPARATOR
        MENUITEM "Delete",                      IDM_DELETEFORMAT
        MENUITEM "Rename",                      IDM_RENAMEFORMAT
        MENUITEM SEPARATOR
        MENUITEM "Refresh",                     IDM_REFRESHFORMAT
    END
END*/
		menu.CreateMenu();
		CMenu popup;
		popup.CreatePopupMenu();
		popup.AppendMenu(MF_STRING, ID_OPENFORMAT, _TB("Properties"));
		popup.AppendMenu(MF_STRING, ID_FORMAT_APPAREA, _TB("Apply in ..."));
		popup.AppendMenu(MF_STRING, ID_CUTFORMAT, _TB("Cut (Ctrl+X)"));
		popup.AppendMenu(MF_STRING, ID_COPYTBFORMAT, _TB("Copy (Ctrl+C)"));
		popup.AppendMenu(MF_STRING, ID_PASTETBFORMAT, _TB("Paste (Ctrl+V)"));
		popup.AppendMenu(MF_SEPARATOR);
		popup.AppendMenu(MF_STRING, ID_DELETEFORMAT, _TB("Delete"));
		popup.AppendMenu(MF_STRING, ID_RENAMEFORMAT, _TB("Rename"));
		popup.AppendMenu(MF_SEPARATOR);
		popup.AppendMenu(MF_STRING, ID_REFRESHFORMAT, _TB("Refresh"));
		menu.AppendMenu(MF_STRING | MF_POPUP, (UINT_PTR)popup.Detach(), _TB("Menu"));
		ASSERT(menu);	
		pPopup = menu.GetSubMenu(0);
		ASSERT(pPopup);		
		if (pPopup)
		{
			if (!pDlg->CanCopy())
				pPopup->EnableMenuItem(ID_COPYTBFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

			if (!pDlg->CanPaste())
				pPopup->EnableMenuItem(ID_PASTETBFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

			if (!pDlg->CanOpen())
				pPopup->EnableMenuItem(ID_OPENFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

			if (!pDlg->CanDelete())
			{
				pPopup->EnableMenuItem(ID_DELETEFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				pPopup->EnableMenuItem(ID_CUTFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				pPopup->EnableMenuItem(ID_FORMAT_APPAREA, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				pPopup->EnableMenuItem(ID_RENAMEFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
			}
			
			if (pDlg->GetItemLevel(hItemToSelect) > 0)
				pPopup->EnableMenuItem(ID_REFRESHFORMAT, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
				
			if (!pDlg->CanApplyContextArea())
				pPopup->EnableMenuItem(ID_FORMAT_APPAREA, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
						
			ClientToScreen(&mousePos);	

			pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON,	mousePos.x, mousePos.y, this);		
		}
	}
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnItemBeginEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 0;
	CFormatDlg* pDlg = (CFormatDlg*) GetParent();
	if (!pDlg->CanCopy())		// se posso copiare allora...
	{
		*pResult = 1;
		return;
	}	

	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnItemEndEdit(NMHDR* pNMHDR, LRESULT* pResult)
{
	*pResult = 1;
	LPNMTVDISPINFO lpDispInfo = (LPNMTVDISPINFO)pNMHDR;
	if (!lpDispInfo || !lpDispInfo->item.hItem)
		return;

	if (!lpDispInfo->item.pszText)
	{
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		SetFocus	();
		return;
	}	

	if (!lpDispInfo->item.pszText[0])
	{
		*pResult = 0;
		SetItemState(lpDispInfo->item.hItem, TVIS_SELECTED, TVIS_SELECTED);
		SetFocus	();
		return;
	}

	//SetItemText(lpDispInfo->item.hItem, lpDispInfo->item.pszText);
		
	CFormatDlg* pDlg = (CFormatDlg*) GetParent();
	pDlg->RenameStyle(lpDispInfo->item.pszText);
	*pResult = 0;
	SelectItem(lpDispInfo->item.hItem);
	SetFocus	();
}

//---------------------------------------------------------------------
void CTreeFormatDialog::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	CFormatDlg* pDlg = (CFormatDlg*) GetParent();

	if (nChar == 113 && pDlg->CanPaste())
		OnRename();

	if (nChar == 46 && pDlg->CanDelete())
		OnDelete();

	if (nChar == VK_CONTROL)
		m_bAfterCtrl = TRUE;
	
	if (nChar == 67 && m_bAfterCtrl)
	{
		OnCopy();
		m_bAfterCtrl = FALSE;
	}

	if (nChar == 86 && m_bAfterCtrl && pDlg->CanPaste())
	{
		OnPaste();
		m_bAfterCtrl = FALSE;
	}

	if (nChar == 88 && m_bAfterCtrl && pDlg->CanDelete())
	{
		OnCut();
		m_bAfterCtrl = FALSE;

	}

	__super::OnKeyDown(nChar, nRepCnt, nFlags);	
}
//---------------------------------------------------------------------
void CTreeFormatDialog::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	m_bAfterCtrl = FALSE;
	__super::OnKeyUp(nChar, nRepCnt, nFlags);
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnContextArea() 
{
	CFormatDlg* pFormatDlg =  (CFormatDlg*) GetParent();
	pFormatDlg->ContextArea();
}


//----------------------------------------------------------------------------
void CTreeFormatDialog::OnRefreshFormatter()
{
	CFormatDlg* pFormatDlg =  (CFormatDlg*) GetParent();

	pFormatDlg->RefreshFormatsTable	();
	pFormatDlg->FillTreeCtrlStyle	();
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnOpen() 
{
	CFormatDlg* pFormatDlg =  (CFormatDlg*) GetParent();
	pFormatDlg->OpenStyle();
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnDelete() 
{
	CFormatDlg* pFormatDlg =  (CFormatDlg*) GetParent();
	pFormatDlg->DeleteStyle();
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnCopy() 
{
	CFormatDlg* pFormatDlg =  (CFormatDlg*) GetParent();
	pFormatDlg->CopyStyle();
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnPaste() 
{
	CFormatDlg* pFormatDlg = (CFormatDlg*) GetParent();
	pFormatDlg->PasteStyle();
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnRename() 
{
	CFormatDlg* pFormatDlg = (CFormatDlg*) GetParent();
	pFormatDlg->OnRemaneLabel();
}

//----------------------------------------------------------------------------
void CTreeFormatDialog::OnCut() 
{
	CFormatDlg* pFormatDlg	= (CFormatDlg*) GetParent();
	pFormatDlg->m_bFormCut	= TRUE;
	pFormatDlg->m_hSelCut	= pFormatDlg->m_treeStyle.GetSelectedItem();
	pFormatDlg->CopyStyle();
}

//============================================================================
//		CFormatDlg implementation
//============================================================================
IMPLEMENT_DYNAMIC(CFormatDlg, CParsedDialog)
	BEGIN_MESSAGE_MAP(CFormatDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CFormatDlg)
	ON_NOTIFY			(NM_DBLCLK,				IDC_TREE_STYLE,	OnNMDblclkTree)
	ON_NOTIFY			(TVN_SELCHANGED,		IDC_TREE_STYLE,	OnTreeSelchanged)	
	ON_BN_CLICKED		(ID_FORMATDLG_SAVE,				OnSaveFormatStyles)
	ON_CBN_SELCHANGE	(IDC_FORMAT_COMBOFILTER,				OnComboStyleChanged)

	ON_BN_CLICKED		(ID_FORMAT_OPEN,		OnOpen)
	ON_BN_CLICKED		(ID_FORMAT_APPLYIN,		OnContextArea)
	ON_BN_CLICKED		(ID_FORMAT_CUT,			OnCut)
	ON_BN_CLICKED		(ID_FORMAT_COPY,		OnCopy)
	ON_BN_CLICKED		(ID_FORMAT_PASTE,		OnPaste)
	ON_BN_CLICKED		(ID_FORMAT_DELETE,		OnDelete)
	ON_BN_CLICKED		(ID_FORMAT_RENAME,		OnRename)
	ON_BN_CLICKED		(ID_FORMAT_FILTERTREE,	OnFilterTree)

	ON_COMMAND			(ID_FORMAT_HELP, 		OnHelp)
	
	ON_WM_CONTEXTMENU()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CFormatDlg::CFormatDlg
	(
		FormatStyleTable&		StyleTable,
		FormatIdx&					nFormatIndex,
		BOOL						bIgnoreIdx,
		CWnd*						pWndParent,
		const CTBNamespace&			NsForWoorm,
		BOOL						bSelezionaAsOk
	)
    :
	CParsedDialog				(IDD_FORMAT_STYLE, pWndParent),
    m_pWndParent				(pWndParent),
	m_SourceStyleTable			(StyleTable),
	m_StyleTable				(StyleTable),
	m_FormatIdx					(nFormatIndex),
	m_bIgnoreIdx				(bIgnoreIdx),
	m_bModified					(TRUE),
	m_bFormCut					(FALSE),
	m_NsForWoorm				(NsForWoorm),
	m_bFromTree					(FALSE),
	m_DefaultSel				(NULL),
	m_bFilterTree				(FALSE),
	m_bSelezionaAsOk			(bSelezionaAsOk),
	m_bFontSave					(FALSE)
{
	// sono in nuovo report
	if (m_NsForWoorm.GetType() == CTBNamespace::DOCUMENT)
		m_NsForWoorm.SetType(CTBNamespace::REPORT);

	m_ItemType			= StyleTable.GetDataType(nFormatIndex);
	if (nFormatIndex == -1 && m_ItemType == DataType::Null)
		m_ItemType = DataType::Variant;
		
	m_FormatCopy		= NULL;
	m_sFilterStyle		= _T("");
}

//------------------------------------------------------------------------------
CFormatDlg::~CFormatDlg()
{
	SAFE_DELETE(m_FormatCopy);
	m_ImageList.DeleteImageList();
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::OnInitDialog()
{
	BOOL bInit = CParsedDialog::OnInitDialog();
	EnableToolTips(TRUE);

	RefreshFormatsTable();

	BOOL bEnableSave = m_StyleTable.IsModified();

	SetToolbarStyle(CParsedDialog::TOP, DEFAULT_TOOLBAR_HEIGHT, FALSE, TRUE);

	// se sono nel caso di woorm
	if (!m_bIgnoreIdx)
	{
		SetWindowText(_TB("Formatter styles Customizations"));
		if (m_bSelezionaAsOk)
			m_pToolBar->SetText(IDOK, _TB("Ok"));
	}
	else
	{
		m_pToolBar->EnableButton(IDOK);
	}

	m_treeStyle	.SubclassDlgItem(IDC_TREE_STYLE,			this);
	m_CmbStyle	.SubclassDlgItem(IDC_FORMAT_COMBOFILTER,	this);

	LoadImageList		();
	FillComboStyle		();
	OnFilterTree		();

	m_treeStyle.ExpandAll(TVE_EXPAND); 	

	EnableDisableToolbar();
	
	m_pToolBar->EnableButton(ID_FORMATDLG_SAVE, FALSE);
	m_bFontSave = FALSE;

	m_treeStyle.SetFocus();
	return FALSE;
}

//----------------------------------------------------------------------------
void CFormatDlg::OnCustomizeToolbar()
{
	// associa bottoni e immagini
	CString sNs = _T("Framework.TbGenlibUI.TbGenlibUI.Format.");

	m_pToolBar->AddButton(ID_FORMAT_FILTERTREE, sNs + _T("FilTerTree"), TBIcon(szIconFilterTree, IconSize::TOOLBAR), _TB("Filter"));
	m_pToolBar->AddButton(ID_FORMAT_OPEN, sNs + _T("Open"), TBIcon(szIconEdit, IconSize::TOOLBAR), _TB("Open"));
	m_pToolBar->AddButton(ID_FORMAT_APPLYIN, sNs + _T("Applyin"), TBIcon(szIconApplyIn, IconSize::TOOLBAR), _TB("Apply in"));
	m_pToolBar->AddButton(ID_FORMAT_CUT, sNs + _T("Cut"), TBIcon(szIconCut, IconSize::TOOLBAR), _TB("Cut"));
	m_pToolBar->AddButton(ID_FORMAT_COPY, sNs + _T("Copy"), TBIcon(szIconCopy, IconSize::TOOLBAR), _TB("Copy"));
	m_pToolBar->AddButton(ID_FORMAT_PASTE, sNs + _T("Paste"), TBIcon(szIconPaste, IconSize::TOOLBAR), _TB("Paste"));
	m_pToolBar->AddButton(ID_FORMAT_DELETE, sNs + _T("Delete"), TBIcon(szIconDelete, IconSize::TOOLBAR), _TB("Delete"));
	m_pToolBar->AddButton(ID_FORMAT_RENAME, sNs + _T("Rename"), TBIcon(szIconRename, IconSize::TOOLBAR), _TB("Rename"));

	m_pToolBar->AddButtonToRight(IDOK, sNs + _T("Select"), TBIcon(szIconOk, IconSize::TOOLBAR), _TB("Select"), _TB("Select (Alt + S)"));
	m_pToolBar->AddButtonToRight(ID_FORMATDLG_SAVE, sNs + _T("Savestyles"), TBIcon(szIconSave, IconSize::TOOLBAR), _TB("Save styles"), _TB("Save styles (Alt + Enter)"));
	m_pToolBar->AddButtonToRight(IDCANCEL, sNs + _T("Cancel"), TBIcon(szIconCancel, IconSize::TOOLBAR), _TB("Cancel"));

	// Append acceleretor
	AppendAccelerator(IDOK, FALT, 0x73);
	AppendAccelerator(ID_FORMATDLG_SAVE, FALT, VK_RETURN);
}

//----------------------------------------------------------------------------
void CFormatDlg::SetDefaultSelection ()
{
	if (m_bIgnoreIdx || !m_DefaultSel)
		return;

	m_treeStyle.SelectItem(m_DefaultSel);
	m_treeStyle.SetItemState(m_DefaultSel, TVIS_SELECTED | TVIS_BOLD , TVIS_SELECTED | TVIS_BOLD);
	m_treeStyle.SetFocus ();

}

//------------------------------------------------------------------------------
void CFormatDlg::OnComboStyleChanged()
{
	BOOL bFind	= FALSE;
	int	nCurSel	= m_CmbStyle.GetCurSel();
	
	if (!nCurSel)
	{
		m_sFilterStyle = _T("");
		FillTreeCtrlStyle();
		return;
	}

	Formatter* pItem = (Formatter*) m_CmbStyle.GetItemData(nCurSel);
	m_sFilterStyle = pItem->m_strName;

	FillTreeCtrlStyle();	
	SetDefaultSelection ();
	return;
}

//----------------------------------------------------------------------------
void CFormatDlg::OnNMDblclkTree(NMHDR *pNMHDR, LRESULT *pResult)
{
	if (CanOpen())
		OpenStyle();
	*pResult = 0;
}

//----------------------------------------------------------------------------
void CFormatDlg::OnTreeSelchanged(NMHDR *pNMHDR, LRESULT *pResult)
{
	HTREEITEM hSel;
	hSel = m_treeStyle.GetSelectedItem();
	
	m_pToolBar->EnableButton(IDOK, FALSE);

	EnableDisableToolbar ();

	if (m_treeStyle.ItemHasChildren(hSel))
		return;

	Formatter*		pTmpFormat	= (Formatter*) m_treeStyle.GetItemData(hSel);
	//CTreeItemRef*	pRef		= (CTreeItemRef*) m_treeStyle.GetItemData(hSel);
	FormatIdx		nIdx		= m_StyleTable.GetFormatIdx(pTmpFormat->GetName());

	if (nIdx < 0)
		return;

	m_pToolBar->EnableButton(IDOK);
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnHelp()
{
	ShowHelp(szHelpNamespace);
}

//----------------------------------------------------------------------------
void CFormatDlg::OnSaveFormatStyles	()
{
	if (AfxMessageBox(cwsprintf(_TB("Are you sure to save changes\nto general formatter styles?\nDo you want continue?")), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
		DoSaveFormatStyles ();		
}

//----------------------------------------------------------------------------
void CFormatDlg::DoSaveFormatStyles ()
{
	FormatStyleTablePtr ptrFormatStyleTable = AfxGetWritableFormatStyleTable();
	if (m_bIgnoreIdx)
		*ptrFormatStyleTable = m_StyleTable;
	else
	{
		*ptrFormatStyleTable = m_StyleTable;
		m_SourceStyleTable = m_StyleTable;
		DeleteWoormFormatter(ptrFormatStyleTable);	
	}
	
	m_pToolBar->EnableButton(ID_FORMATDLG_SAVE, FALSE);
	m_bFontSave = FALSE;
	CTBNamespaceArray aNs;
	ptrFormatStyleTable->CheckFormatTable(aNs);

	if (aNs.GetSize() == 0)
		return;	

	FormatsParser aParser;	
	for (int i = 0; i <= aNs.GetUpperBound(); i++)
	{
		// check se ns valido e correttezza di module
		CTBNamespace* ns = aNs.GetAt(i);
		if (!ns->IsValid() || ns->GetType() != CTBNamespace::MODULE)
			continue;	
		aParser.SaveFormats(*ns, AfxGetPathFinder(), FALSE);
	}	

	// devo farlo per fare il Refresh delle date dei files
	m_StyleTable.CopyFileLoaded(*ptrFormatStyleTable);

	ptrFormatStyleTable->SetModified(FALSE);
}

//----------------------------------------------------------------------------
void CFormatDlg::DeleteWoormFormatter(FormatStyleTablePtr StyleTable)
{
	for (int i = 0; i <= StyleTable->GetUpperBound(); i++)
	{
		FormatterGroup* FormGrp = (FormatterGroup*) StyleTable->GetAt(i);
		for (int n = 0; n <= FormGrp->m_Formatters.GetUpperBound(); n++ )
		{
			Formatter* pFormat = (Formatter*) FormGrp->m_Formatters.GetAt(n);
			if (pFormat->GetSource() == Formatter::FROM_WOORM)
				FormGrp->DeleteFormatter(pFormat);
		}
	}
}

//----------------------------------------------------------------------------
void CFormatDlg::UpdateSourceStyleTable()
{
	DeleteWoormFormatter(FormatStyleTablePtr(&m_SourceStyleTable, FALSE));
	for (int i = 0; i <= m_StyleTable.GetUpperBound(); i++)
	{
		FormatterGroup* FormGrp = (FormatterGroup*) m_StyleTable.GetAt(i);
		for (int n = 0; n <= FormGrp->m_Formatters.GetUpperBound(); n++ )
		{
			Formatter* pFormat = (Formatter*) FormGrp->m_Formatters.GetAt(n);
			if (pFormat->GetSource() ==	 Formatter::FROM_WOORM /*&& !pFormat->m_bDeleted*/)
			{
				Formatter* pNewWoormFormat = PolyNewFormatter(pFormat->GetDataType(), Formatter::FROM_CUSTOM);
				pNewWoormFormat->Assign(*pFormat);
				m_SourceStyleTable.AddFormatter(pNewWoormFormat);
				m_SourceStyleTable.SetModified(TRUE);
			}
		}
	}
}

//----------------------------------------------------------------------------
void CFormatDlg::EnableDisableToolbar ()
{
	if (AfxGetThemeManager()->AutoHideToolBarButton())
	{
		m_pToolBar->HideButton(ID_FORMAT_CUT,		!CanDelete());
		m_pToolBar->HideButton(ID_FORMAT_COPY,		!CanCopy());
		m_pToolBar->HideButton(ID_FORMAT_PASTE,		!CanPaste());
		m_pToolBar->HideButton(ID_FORMAT_OPEN,		!CanOpen());
		m_pToolBar->HideButton(ID_FORMAT_APPLYIN,	!CanApplyContextArea());
		m_pToolBar->HideButton(ID_FORMAT_RENAME,	!CanDelete());
		m_pToolBar->HideButton(ID_FORMAT_DELETE,	!CanDelete());
		m_pToolBar->HideButton(ID_FORMAT_HELP,		FALSE);

		m_pToolBar->RepositionRightButtons();
		m_pToolBar->AdjustSizeImmediate();
		m_pToolBar->AdjustLayout();
		return;
	}

	m_pToolBar->EnableButton(ID_FORMAT_CUT, CanDelete());
	m_pToolBar->EnableButton(ID_FORMAT_COPY, CanCopy());
	m_pToolBar->EnableButton(ID_FORMAT_PASTE, CanPaste());
	m_pToolBar->EnableButton(ID_FORMAT_OPEN, CanOpen());
	m_pToolBar->EnableButton(ID_FORMAT_APPLYIN, CanApplyContextArea());
	m_pToolBar->EnableButton(ID_FORMAT_RENAME, CanDelete());
	m_pToolBar->EnableButton(ID_FORMAT_DELETE, CanDelete());
	m_pToolBar->EnableButton(ID_FORMAT_HELP, TRUE);

	m_pToolBar->RepositionRightButtons();
	m_pToolBar->AdjustSizeImmediate();
	m_pToolBar->AdjustLayout();
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::OnOkFormatForSelect (Formatter* pSelFormat, BOOL bSave)
{
	if (!pSelFormat)
	{
		AfxMessageBox(_TB("This formatter is not utilizable by this report.\nVerify criterions of Apply In..."), MB_APPLMODAL);
		return FALSE;
	}

	// controllo l'usabilità del formattatore
	int nIdx = m_StyleTable.GetFormatIdx(pSelFormat->GetName());
	Formatter* pFormatter = m_StyleTable.GetFormatter(nIdx, &m_NsForWoorm);
	if (!pFormatter)
	{
		AfxMessageBox(_TB("This formatter is not utilizable by this report.\nVerify criterions of Apply In..."), MB_APPLMODAL);
		return FALSE;
	}

	if (pSelFormat->GetDataType() != m_ItemType && m_ItemType != DataType::Variant)
	{
		AfxMessageBox(_TB("Incompatible format type"), MB_APPLMODAL | MB_OK | MB_ICONSTOP);
		return FALSE;
	}
	
	// avviso l'utente che non ha salvato gli stili generali
	if	(bSave && m_bFontSave)
	{
		if (AfxMessageBox(_TB("General application styles has been modified!\nModifications will be automatically saved.\nDo you want continue?"), MB_APPLMODAL | MB_OKCANCEL) == IDCANCEL)
			return FALSE;
		
		DoSaveFormatStyles();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnOK()
{
	if (!IsTreeCtrlEditMessage(VK_RETURN) && !m_bFromTree)
	{
		if (m_bModified)
			AfxGetMainWnd()->PostMessage(UM_FORMAT_STYLE_CHANGED); 

		if (m_bIgnoreIdx || m_bSelezionaAsOk)	
		{
			// avviso l'utente che non ha salvato gli stili generali
			if	(m_bSelezionaAsOk && m_bFontSave)
			{
				if (AfxMessageBox(_TB("General application styles has been modified!\nModifications will be automatically saved.\nDo you want continue?"), MB_APPLMODAL | MB_OKCANCEL) == IDCANCEL)
					return;
				
				DoSaveFormatStyles();
			}

			m_SourceStyleTable = m_StyleTable;
			EndDialog (IDOK);
			return;
		}
		
		HTREEITEM hSel = m_treeStyle.GetSelectedItem();	
		if (!hSel)
		{
			AfxMessageBox(_TB("No formatter selected!"), MB_APPLMODAL);
			return;
		}
		
		Formatter* pSelFormat = (Formatter*) m_treeStyle.GetItemData(hSel);

		if (!OnOkFormatForSelect (pSelFormat, TRUE))
			return;

		m_FormatIdx = m_StyleTable.GetFormatIdx(pSelFormat->GetName());
		m_SourceStyleTable = m_StyleTable;
		EndDialog(IDOK);
	}

	if (m_bFromTree)
	{
		OpenStyle();
		m_bFromTree = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnCancel()
{
	if (m_bFontSave && AfxMessageBox (_TB("Modifications to general styles will be losed!\nAre you sure to exit?"), MB_APPLMODAL | MB_YESNO ) == IDNO)
		return;

	if (!m_bIgnoreIdx)	//devo riportare solo i woorm
	{
		// controllo che non abbiano cambiato le caratteristiche del preesistente
		Formatter* pSelFormat = m_StyleTable.GetFormatter(m_FormatIdx, &m_NsForWoorm);
		if (pSelFormat)
		{
			if (!OnOkFormatForSelect (pSelFormat, FALSE))
				return;

			UpdateSourceStyleTable();
		}
	}
	
	CLocalizableDialog::OnCancel();
}

//-----------------------------------------------------------------------------
void CFormatDlg::DeleteStyle()
{
	HTREEITEM	hSel	= m_treeStyle.GetSelectedItem();
	Formatter*	pFormat = (Formatter*) m_treeStyle.GetItemData(hSel);
	if (RemoveCustomStyle(pFormat))
		m_treeStyle.DeleteItem(hSel);
	if (pFormat->m_FromAndTo == Formatter::FROM_CUSTOM)
	{
		m_pToolBar->EnableButton(ID_FORMATDLG_SAVE);
		m_bFontSave = TRUE;
	}
	// controlla se ne è rimasto almeno uno
	BOOL		bOne	= FALSE;
	FormatIdx	nIdx	= m_StyleTable.GetFormatIdx(pFormat->GetName());
	for (int p = 0; p <= m_StyleTable.GetAt(nIdx)->m_Formatters.GetUpperBound(); p++)
	{
		Formatter* pFormatNotDel = (Formatter*) m_StyleTable.GetAt(nIdx)->m_Formatters.GetAt(p);
		if (!pFormatNotDel->m_bDeleted)
		{
			bOne = TRUE;
			break;			
		}
	}	
	
	FillComboStyle();
	if (!bOne)
		OnComboStyleChanged();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnOpen	()
{
	m_treeStyle.OnOpen();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnDelete()
{
	m_treeStyle.OnDelete();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnCopy()
{
	m_treeStyle.OnCopy();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnPaste()
{
	m_treeStyle.OnPaste();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnRename()
{
	m_treeStyle.OnRename();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnCut()
{
	m_treeStyle.OnCut ();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnContextArea()
{
	m_treeStyle.OnContextArea();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnFilterTree()
{
	m_bFilterTree = !m_bFilterTree;
	m_pToolBar->PressButton(ID_FORMAT_FILTERTREE, m_bFilterTree);

	FillTreeCtrlStyle();
	SetDefaultSelection ();
}

//-----------------------------------------------------------------------------
void CFormatDlg::OnRemaneLabel()
{
	m_treeStyle.EditLabel(m_treeStyle.GetSelectedItem());
	return;
}

//-----------------------------------------------------------------------------
void CFormatDlg::RenameStyle(const CString& strSelItem)
{
	HTREEITEM	hSel				= m_treeStyle.GetSelectedItem();
	Formatter*	pFormat				= (Formatter*) m_treeStyle.GetItemData(hSel);
	CString		strFormatName		= _T("");
	Formatter*	pNewFormat			= pFormat->Clone();

	if (strSelItem.CompareNoCase(pFormat->GetName()) == 0 && pNewFormat->m_FromAndTo == Formatter::FROM_CUSTOM)
	{
		SAFE_DELETE(pNewFormat);
		return;
	}

	if (pNewFormat->m_FromAndTo != Formatter::FROM_WOORM)
		pNewFormat->m_FromAndTo = Formatter::FROM_CUSTOM;
	
	pNewFormat->m_strName	= strSelItem;
	pNewFormat->SetChanged(TRUE); 

	// se il formattatore è programmativo cambio in namespace
	if (pNewFormat->GetOwner().GetType() == CTBNamespace::LIBRARY)
	{
		CTBNamespace aModule(
								CTBNamespace::MODULE, 
								pNewFormat->GetOwner().GetApplicationName()
								+ CTBNamespace::GetSeparator() +
								pNewFormat->GetOwner().GetObjectName(CTBNamespace::MODULE)
							);
		pNewFormat->SetOwner (aModule);
	}

	if (pNewFormat->m_FromAndTo != Formatter::FROM_STANDARD)
	{
		Formatter* pOldFormatCopy = m_FormatCopy;

		m_FormatCopy = pNewFormat;
		m_FormatCopy->SetStandardFormatter(NULL);
		AddNewStyle(&pNewFormat->m_OwnerModule);
		m_FormatCopy = pOldFormatCopy;
	}

	if (RemoveCustomStyle(pFormat, TRUE))
		m_treeStyle.DeleteItem(hSel);

	if (!m_StyleTable.IsModified())
		m_StyleTable.SetModified(TRUE);
	
	if (pNewFormat->m_FromAndTo == Formatter::FROM_CUSTOM)
	{
		m_pToolBar->EnableButton(ID_FORMATDLG_SAVE);
		m_bFontSave = TRUE;
	}

	SAFE_DELETE(pNewFormat);
}

//-----------------------------------------------------------------------------
CString CFormatDlg::GetNewFormatName(Formatter* pFormat)
{
	Formatter* pExistingFormat = m_StyleTable.GetFormatter(pFormat, pFormat->GetSource());
	// ne esiste uno contrassegnato deleted quindi lo riuso
	if (!pExistingFormat || pExistingFormat->IsDeleted())
		return pFormat->m_strName;

	CString strName	= pFormat->GetName();
	CString sCopiaDi= _TB("Copy of");
	CString sDi = _TB("of");
	CString sNewName;

	int	nLast  = -1;
	int	nFirst = strName.Find('(');
	int nCopy  = 0;
	
	// provo a cercare copia di
	BOOL bIsCopiaDi = strName.Left(sCopiaDi.GetLength()).CompareNoCase(sCopiaDi) == 0;

	if (bIsCopiaDi)
		nLast = sCopiaDi.GetLength();
	else
	{
		// c'è anche la parola dopo
		nLast = strName.ReverseFind(')');
		if (nLast >=0)
			nLast += sDi.GetLength() + 1;
	}
	
	if (nLast)
		sNewName = strName.Mid(nLast+1);

	if (nFirst < nLast)
	{
		if (nFirst >= 0 && nLast >= 0)
			strName = strName.Mid(nFirst + 1, nLast - nFirst);
		nCopy = _tstoi(strName);
		if (nCopy <= 0)
			nCopy = 1;
	}
	nCopy++;

	if (nCopy == 1)
	{
		pFormat->m_strName = sCopiaDi;
		sNewName = strName;
	}
	else
		pFormat->m_strName =  cwsprintf(_TB("Copy ({0-%d})") + sDi, nCopy);

	pFormat->m_strName += _T (" ") + sNewName;

	// esiste già quindi provo in ricorsione
	if (m_StyleTable.GetFormatIdx(pFormat, pFormat->GetSource()) >= 0)
		GetNewFormatName(pFormat);
	
	return pFormat->m_strName;
}

//-----------------------------------------------------------------------------
void CFormatDlg::OpenStyle()
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	int			nLev = GetItemLevel(hSel);
	Formatter*  pFormatterSel	= NULL;
	Formatter*  pFormatterOpen	= NULL;

	pFormatterSel = (Formatter*) m_treeStyle.GetItemData(hSel);
		
	pFormatterOpen = PolyNewFormatter(pFormatterSel->GetDataType(), pFormatterSel->m_FromAndTo);
	pFormatterOpen->Assign(*pFormatterSel);
	// se sono nel report di default lo creo come woorm
	BOOL bInWoorm = FALSE;
	if (!m_bIgnoreIdx && pFormatterOpen->GetSource() == Formatter::FROM_STANDARD)
		bInWoorm = TRUE;

	switch(nLev)
	{
		case (0):
		{
			SAFE_DELETE(pFormatterOpen);
			return;
			break;		
		}
		case (1):
		{
			//controllo se il parent è Application
			HTREEITEM		hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeItemRef*	pItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString			strParent	= pItemRef->m_strName;

			if (strParent.CompareNoCase(szCurrentReport) == 0)
				break;

			if (!AfxGetPathFinder()->IsASystemApplication(strParent) || !AfxGetAddOnApp(strParent))
			{
				SAFE_DELETE(pFormatterOpen);
				return;
			}	
			break;		
		}
	}
	
	CFormatDlgRoot* pDlg = NULL;
	switch (pFormatterOpen->GetDataType().m_wType)
	{
		case DATA_LNG_TYPE:
		{
			if (pFormatterOpen->GetDataType().IsATime())	//@@ElapsedTime
				pDlg = new CElapsedTimeFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			else
				pDlg = new CIntFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_INT_TYPE:
		{
			pDlg = new CIntFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_STR_TYPE:
		case DATA_TXT_TYPE:
		{
			pDlg = new CStringFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		{
			pDlg = new CDblFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_DATE_TYPE:
		{
			pDlg = new CDateTimeFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_BOOL_TYPE:
		{
			pDlg = new CBoolFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_ENUM_TYPE:
		{
			pDlg = new CEnumFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		case DATA_GUID_TYPE: 
		{
			pDlg = new CStringFormatDlg(pFormatterOpen, this, m_bIgnoreIdx);
			break;
		}
		default: break;
	}

	if (!pDlg)
		ASSERT(FALSE);

	if (pDlg->DoModal() == IDOK && pDlg->IsModified())
	{
		m_bFontSave = !bInWoorm && pDlg->IsModified();
		m_pToolBar->EnableButton(ID_FORMATDLG_SAVE, m_bFontSave);
	}
	SAFE_DELETE(pDlg);
	SAFE_DELETE(pFormatterOpen);

	if (!bInWoorm)
	{
		m_treeStyle.SelectItem	(hSel);
		m_treeStyle.SetFocus	();
	}
}

//-----------------------------------------------------------------------------
void CFormatDlg::ContextArea()
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	//int			nLev = GetItemLevel(hSel);
	Formatter*  pFormatterSel = (Formatter*) m_treeStyle.GetItemData(hSel);

	if (!pFormatterSel)
		return;
	
	// inizializzo il default
	CStringArray aSelections;
	for (int i=0; i <= pFormatterSel->GetLimitedAreas().GetUpperBound(); i++)
		aSelections.Add(pFormatterSel->GetLimitedAreas().GetAt(i));
	
	// editazione e travaso dei dati
	CContextSelectionDialog aDlg(&aSelections);
	if (aDlg.DoModal() == IDCANCEL)
		return;

	BOOL bModified = aSelections.GetSize() != pFormatterSel->GetLimitedAreas().GetSize();
	if (!bModified)
		for (int i=0; i <= aSelections.GetUpperBound(); i++)
			if (aSelections.GetAt(i).CompareNoCase(pFormatterSel->GetLimitedAreas().GetAt(i)))
			{
				bModified = TRUE;
				break;
			}
	
	if (bModified)
	{
		pFormatterSel->SetLimitedAreas(aSelections);
		pFormatterSel->SetChanged(TRUE);
		m_StyleTable.SetModified(TRUE);
	}

	// applica in è selezionabile solo sui font custom
	m_pToolBar->EnableButton(ID_FORMATDLG_SAVE);
	m_bFontSave = TRUE;
}

//-----------------------------------------------------------------------------
void CFormatDlg::CopyStyle()	//per ora solo formatter (solo un custom)
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	int			nLev = GetItemLevel(hSel);

	HTREEITEM hMod = m_treeStyle.GetParentItem(hSel);
	HTREEITEM hApp = m_treeStyle.GetParentItem(hMod);

	switch (nLev)
	{
		case (2):
		{
			Formatter* pFormatterSel = (Formatter*) m_treeStyle.GetItemData(hSel);

			if (m_FormatCopy)
				SAFE_DELETE(m_FormatCopy);

			m_FormatCopy = PolyNewFormatter(pFormatterSel->GetDataType(), Formatter::FROM_CUSTOM);
			m_FormatCopy->Assign(*pFormatterSel);
			break;
		}
		case (1):
		{
			//controllo se il parent è Application
			HTREEITEM		hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeItemRef*	pItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString			strParent	= pItemRef->m_strName;
			
			if (AfxGetPathFinder()->IsASystemApplication(strParent) || strParent.CompareNoCase(szCurrentReport) == 0)
			{
				Formatter* pFormatSel = (Formatter*) m_treeStyle.GetItemData(hSel);

				if (m_FormatCopy)
					SAFE_DELETE(m_FormatCopy);

				m_FormatCopy = PolyNewFormatter(pFormatSel->GetDataType(), Formatter::FROM_CUSTOM);
				m_FormatCopy->Assign(*pFormatSel);
				break;
			}

			if (!AfxGetAddOnApp(strParent))
				return;

			return;
			break;		
		}
		case (0):
		{
			return;
			break;		
		}
	}
}

//-----------------------------------------------------------------------------
void CFormatDlg::PasteStyle()	//per ora solo formatter (solo un custom)
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	//int			nLev = GetItemLevel(hSel);

	CTreeItemRef*	pTreeItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hSel);
	CString			strSel			= pTreeItemRef->m_strName;
	CString			strParent		= _T("");
	CTBNamespace	NsNew; 

	HTREEITEM hParent  = m_treeStyle.GetParentItem(hSel);
	if (hParent)	// caso del modulo
	{
		pTreeItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hParent);
		strParent		= pTreeItemRef->m_strName;

		NsNew.SetType(CTBNamespace::MODULE);
		NsNew.SetApplicationName(strParent);
		NsNew.SetObjectName(CTBNamespace::MODULE, strSel);
		
		if (strParent.CompareNoCase(szCurrentReport) == 0)
		{
			NsNew.SetType(CTBNamespace::REPORT);
			NsNew.SetObjectName(CTBNamespace::REPORT, m_FormatCopy->m_OwnerModule.GetObjectName(CTBNamespace::REPORT));
		}
	}
	else		// caso application
	{
		if (strSel.CompareNoCase(szCurrentReport) == 0)
			NsNew = m_NsForWoorm;
		else
		{
			NsNew.SetType(CTBNamespace::MODULE);
			NsNew.SetApplicationName(strSel);
			HTREEITEM hChild = m_treeStyle.GetChildItem(hSel);
			if (hChild == NULL)
				hChild = m_treeStyle.GetPrevSiblingItem(hSel);
		
			Formatter* pFormat = (Formatter*) m_treeStyle.GetItemData(hChild);
			CTBNamespace NSfromRenamane;
			CString strMod = pFormat->m_OwnerModule.GetObjectName(CTBNamespace::MODULE);
			NSfromRenamane.SetObjectName(CTBNamespace::MODULE, strMod);
			NsNew.SetObjectName(CTBNamespace::MODULE, strMod);
		}
	} 

	if(!AddNewStyle(&NsNew))
		return;

	if (!m_StyleTable.IsModified())
		m_StyleTable.SetModified(TRUE);

	if (m_bFormCut)
	{
		if (RemoveCustomStyle(m_FormatCopy))
			m_treeStyle.DeleteItem(m_hSelCut);
		if (m_FormatCopy->m_FromAndTo == Formatter::FROM_WOORM)
		{
			m_pToolBar->EnableButton(ID_FORMATDLG_SAVE);
			m_bFontSave = TRUE;
		}

		m_hSelCut = NULL;
		m_bFormCut = FALSE;
	}
	
	if (NsNew.GetType() != CTBNamespace::REPORT)
	{
		m_pToolBar->EnableButton(ID_FORMATDLG_SAVE);
		m_bFontSave = TRUE;
	}

	EnableDisableToolbar();
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::AddNewStyle(CTBNamespace* Ns)
{	
	if (m_FormatCopy == NULL)
		return FALSE;

	Formatter* pNewStyle = m_FormatCopy->Clone();
	pNewStyle->SetChanged(TRUE);
	pNewStyle->m_OwnerModule.SetNamespace(*Ns);

	// limito l'applicabilità di default in caso di copia/incolla,
	// ma solo se c'è un cambio di posizione o se non sono sugli stili Tb
	if (	
			!m_bFormCut && 
			Ns->GetType() != CTBNamespace::REPORT && 
			m_FormatCopy->GetOwner() != *Ns &&
			Ns->GetApplicationName().CompareNoCase(szTaskBuilderApp) 
		)
		pNewStyle->SetLimitedArea(Ns->ToString());

	pNewStyle->m_strName = pNewStyle->m_strName;
	
	if (Ns->GetType() == CTBNamespace::REPORT)
		pNewStyle->m_FromAndTo = Formatter::FROM_WOORM;
	else
		pNewStyle->m_FromAndTo = Formatter::FROM_CUSTOM;

	// mi faccio dare il nuovo nome
	pNewStyle->m_strName = GetNewFormatName(pNewStyle);

	// se esiste già cancellato riattivo il precedente, altrimenti lo aggiungo
    Formatter* pFormSpecLoc = m_StyleTable.GetFormatter(pNewStyle, pNewStyle->GetSource());
	if (pFormSpecLoc)
	{
		BOOL bDeleted = pFormSpecLoc->m_bDeleted;
		if (bDeleted)
		{
			pFormSpecLoc->Assign(*pNewStyle);
			InsertInTree(pFormSpecLoc);
			SAFE_DELETE(pNewStyle);
			return TRUE;
		}
	}
	
	m_StyleTable.AddFormatter(pNewStyle);
	InsertInTree(pNewStyle);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CFormatDlg::InsertInTree(Formatter* pFormatter)
{
	FillComboStyle();

	// se il nome non corrisponde al filtro indicato non lo visualizzo
	if (!m_sFilterStyle.IsEmpty()&& pFormatter->GetName().CompareNoCase(m_sFilterStyle))
		return;

	HTREEITEM		hAppItem = m_treeStyle.GetRootItem();
	CTBNamespace	ns;
	ns.SetNamespace(pFormatter->GetOwner());
	Formatter*		pItemData = m_StyleTable.GetFormatter(pFormatter, pFormatter->GetSource());

	while(hAppItem)
	{
		CString sLev0 = m_treeStyle.GetItemText(hAppItem);
		CTreeItemRef* pItemRef = (CTreeItemRef*) m_treeStyle.GetItemData(hAppItem);
		AddOnApplication* pAddOnApp =   AfxGetAddOnApp(ns.GetApplicationName());
		CString strXml = pAddOnApp ? pAddOnApp->GetTitle() : ns.GetApplicationName();

		// sono nella cartella Report ed i child sono dei fontstyle
		if (pItemRef->m_strName.CompareNoCase(szCurrentReport) == 0 && pFormatter->m_FromAndTo == Formatter::FROM_WOORM)
		{
			HTREEITEM hInsert = m_treeStyle.InsertItem(pFormatter->GetTitle().IsEmpty() ? pFormatter->GetName() : pFormatter->GetTitle(), 1, 1, hAppItem);
			m_treeStyle.EnsureVisible(hInsert);
			BOOL n = m_treeStyle.SetItemData(hInsert, (DWORD_PTR) pItemData);
			m_treeStyle.SelectItem(hInsert);
			m_treeStyle.SetFocus();
			return;
		}

		if (AfxGetPathFinder()->IsASystemApplication(ns.GetApplicationName()))
		{
			if (sLev0.CompareNoCase(strXml) == 0)
			{
				HTREEITEM hInsert = m_treeStyle.InsertItem(pFormatter->GetTitle().IsEmpty() ? pFormatter->GetName() : pFormatter->GetTitle(), 1, 1, hAppItem);
			
				FormatIdx nIdxLocal = m_StyleTable.GetFormatIdx(pFormatter->GetName());
				if (nIdxLocal < 0) // altrimenti non esiste
					ASSERT(FALSE);

				m_treeStyle.EnsureVisible(hInsert);
				BOOL n = m_treeStyle.SetItemData(hInsert, (DWORD_PTR) pItemData);
				m_treeStyle.SelectItem(hInsert);
				m_treeStyle.SetFocus();
				return;
			}
		}
		else
		{
			if (sLev0.CompareNoCase(strXml) == 0)
			{
				HTREEITEM	hModItem	= m_treeStyle.GetChildItem(hAppItem);
				
				while(hModItem)
				{
					CString		sLev1		= m_treeStyle.GetItemText(hModItem);
					CString		strModXml	= AfxGetAddOnModule(ns)->GetModuleTitle();

					if (sLev1.CompareNoCase(strModXml) == 0)
					{
						HTREEITEM hInsertItem = m_treeStyle.InsertItem(pFormatter->GetTitle().IsEmpty() ? pFormatter->GetName() : pFormatter->GetTitle(), 1, 1, hModItem);
						m_treeStyle.EnsureVisible(hInsertItem);
						BOOL n = m_treeStyle.SetItemData(hInsertItem, (DWORD_PTR) pItemData);
						m_treeStyle.SelectItem(hInsertItem);
						m_treeStyle.SetFocus();
						return;
					}
					hModItem = m_treeStyle.GetNextSiblingItem(hModItem);
				}				
			}			
		}

		hAppItem = m_treeStyle.GetNextSiblingItem(hAppItem);
	}
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::RemoveCustomStyle(Formatter* pFormatter, BOOL bFromRename)
{
	if (pFormatter->m_FromAndTo == Formatter::FROM_STANDARD)
		return FALSE;

	Formatter* pFormatOld	= NULL;
	Formatter* pFormatCopy	= pFormatter->Clone();
	pFormatCopy->SetChanged(TRUE);

	FormatIdx nIdxLocal = m_StyleTable.GetFormatIdx(pFormatCopy->GetName());

	 // altrimenti non esiste
	if (nIdxLocal < 0)
		return TRUE;

	for (int i = 0; i <= m_StyleTable.GetAt(nIdxLocal)->m_Formatters.GetUpperBound(); i++)
	{
		pFormatOld = (Formatter*) m_StyleTable.GetAt(nIdxLocal)->m_Formatters.GetAt(i);
		if (pFormatOld->m_OwnerModule == pFormatter->m_OwnerModule && pFormatOld->m_FromAndTo == pFormatter->m_FromAndTo)
		{
			Formatter* pFormToDel = m_StyleTable.GetFormatter(pFormatCopy, pFormatCopy->GetSource());
			if (!pFormToDel)
				return FALSE;

			if (pFormatCopy->m_FromAndTo == Formatter::FROM_WOORM)
			{
				if (!bFromRename)
				{
					if (AfxMessageBox(cwsprintf(_TB("Warning, report formatter style '{0-%s}' will be deleted. Do you want permanently delete  it?"), pFormatCopy->GetName()), MB_APPLMODAL | MB_OKCANCEL | MB_ICONEXCLAMATION | MB_DEFBUTTON2) == IDOK)
					{
						pFormToDel->SetChanged(TRUE);
						pFormToDel->SetDeleted(TRUE);
						if (!m_StyleTable.IsModified())
							m_StyleTable.SetModified(TRUE);
						break;
					}
					else
					{
						return FALSE;
					}	
				}
				else
				{
					pFormToDel->SetChanged(TRUE);
					pFormToDel->SetDeleted(TRUE);
					if (!m_StyleTable.IsModified())
						m_StyleTable.SetModified(TRUE);
					break;
				}
			}
			else
			{
				pFormToDel->SetChanged(TRUE);
				pFormToDel->SetDeleted(TRUE);
				if (!m_StyleTable.IsModified())
					m_StyleTable.SetModified(TRUE);
				break;	
			}
		}
	}
	
	FillComboStyle();
	SAFE_DELETE(pFormatCopy);
	return TRUE;
}

//-----------------------------------------------------------------------------
int CFormatDlg::GetItemLevel(HTREEITEM hItem)
{
	if (!hItem)
		return -1;

	int nLevel = 0;

	HTREEITEM hParent = m_treeStyle.GetParentItem(hItem);
	if (!hParent)
		return nLevel;

	nLevel++;
	hItem = hParent;

	while(hParent = m_treeStyle.GetParentItem(hItem))
	{
		nLevel++;
		hItem = hParent;
	}

	return nLevel;
}

//-----------------------------------------------------------------------------
void CFormatDlg::LoadImageList()
{
	HICON	hIcon[3];
	int		n;

	m_ImageList.Create(20, 20, ILC_COLOR32, 20, 20);
	m_ImageList.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());


	hIcon[0] = TBLoadImage(TBGlyph(szIconFolder));
	hIcon[1] = TBLoadImage(TBGlyph(szIconAllUsers));
	hIcon[2] = TBLoadImage(TBGlyph(szIconStandard));
	
	for (n = 0 ; n < 3 ; n++)
	{
		m_ImageList.Add(hIcon[n]);
		::DeleteObject(hIcon[n]);
	}
	m_treeStyle.SetImageList(&m_ImageList, TVSIL_NORMAL);
}

//-----------------------------------------------------------------------------
void CFormatDlg::FillTreeCtrlStyle()
{
	CTreeItemRef*	pItemRefLocaliz = NULL;
	HTREEITEM 		hItemReport		= 0;
	HTREEITEM 		hItemApp		= 0;
	CString			strApps			= _T("");
	CString			strXml			= _T("");
	BOOL			bIsSystemApp	= FALSE;
	BOOL			bHasFormatters	= FALSE;

	m_DefaultSel = NULL;
	
	//if (!m_treeStyle.GetImageList(TVSIL_NORMAL))
	//	m_treeStyle.SetImageList(&m_ImageList, TVSIL_NORMAL);
	
	BeginWaitCursor();
	m_treeStyle.SetRedraw(FALSE);
	m_treeStyle.DeleteAllItems();
	m_arTreeItemRef.RemoveAll();
	
	//IF FROM WOORM (CONSIDERATA COME APPLICAZIONE)
	if (!m_bIgnoreIdx)
	{
		hItemReport = m_treeStyle.InsertItem(_TB("Local styles of current report"));
		pItemRefLocaliz = new CTreeItemRef(szCurrentReport);
		m_arTreeItemRef.Add(pItemRefLocaliz);
		m_treeStyle.SetItemData(hItemReport,  (DWORD) pItemRefLocaliz);
	}		

	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		strApps = AfxGetAddOnAppsTable()->GetAt(i)->m_strAddOnAppName;
		
		if (!AfxGetAddOnApp(strApps))
			continue;

		strXml = AfxGetAddOnApp(strApps)->GetTitle();

		// se sono in parte sistemistica
		// controllo se esiste almeno un formattatore in quella parte
		bIsSystemApp	= AfxGetPathFinder()->IsASystemApplication(strApps);
		bHasFormatters	= HasApplicationFormatters(strApps, _T(""), m_sFilterStyle) || HasReportFormatters (strApps, _T(""), m_sFilterStyle); 
		if (!bIsSystemApp || bHasFormatters)
		{
			if (bIsSystemApp)
				hItemApp = 0;
			// filtro di visualizzazione
			else if (!bHasFormatters && m_bFilterTree)
				continue;

			hItemApp = m_treeStyle.InsertItem(strXml);
			pItemRefLocaliz = new CTreeItemRef(strApps);
			m_arTreeItemRef.Add(pItemRefLocaliz);
			m_treeStyle.SetItemData(hItemApp, (DWORD) pItemRefLocaliz);

			if (bIsSystemApp)
				FillTreeAddFormats (strApps, _T(""), hItemApp);
			else
				FillTreeAddModules(strApps, hItemApp);
		}
	}

	m_treeStyle.SetRedraw(TRUE);
	m_treeStyle.Invalidate(FALSE);
	m_treeStyle.ExpandAll(TVE_EXPAND);
	
	EnableDisableToolbar ();
	EndWaitCursor();
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::HasApplicationFormatters(const CString& strApp, const CString& strMod, const CString& sFilterStyle)
{
	for (int i = 0; i <= m_StyleTable.GetUpperBound(); i++)
	{
		FormatterGroup* pFormatterGrp = m_StyleTable.GetAt(i);
		for (int n = 0; n <= pFormatterGrp->m_Formatters.GetUpperBound(); n++)
		{
			Formatter* pFormat = (Formatter*) pFormatterGrp->m_Formatters.GetAt(n);
			
			// confronta solo l'applicazione
			if	(
					strMod.IsEmpty()&& 
					pFormat->m_OwnerModule.GetType() != CTBNamespace::REPORT &&
					pFormat->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 && 
					(sFilterStyle.IsEmpty() || pFormat->GetName().CompareNoCase(sFilterStyle) == 0)
				)
				return TRUE;

			// confronta applicazione e modulo
			if	(
					pFormat->m_OwnerModule.GetType() != CTBNamespace::REPORT &&
					pFormat->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 &&
					pFormat->m_OwnerModule.GetObjectName(CTBNamespace::MODULE).CompareNoCase(strMod) == 0 && 
					(sFilterStyle.IsEmpty() || pFormat->GetName().CompareNoCase(sFilterStyle) == 0)
				)
				return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::HasReportFormatters(const CString& strApp, const CString& strMod, const CString& sFilterStyle)
{
	for (int i = 0; i <= m_StyleTable.GetUpperBound(); i++)
	{
		FormatterGroup* pFormatterGrp = m_StyleTable.GetAt(i);
		for (int n = 0; n <= pFormatterGrp->m_Formatters.GetUpperBound(); n++)
		{
			Formatter* pFormat = (Formatter*) pFormatterGrp->m_Formatters.GetAt(n);
			
			// confronta solo l'applicazione
			if	(
					strMod.IsEmpty()&& 
					pFormat->m_OwnerModule.GetType() == CTBNamespace::REPORT &&
					pFormat->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 &&
					(sFilterStyle.IsEmpty() || pFormat->GetName().CompareNoCase(sFilterStyle) == 0)
				)
				return TRUE;

			// confronta applicazione e modulo
			if	(
					pFormat->m_OwnerModule.GetType() == CTBNamespace::REPORT &&
					pFormat->m_OwnerModule.GetApplicationName().CompareNoCase(strApp) == 0 &&
					pFormat->m_OwnerModule.GetObjectName(CTBNamespace::MODULE).CompareNoCase(strMod) == 0 && 
					(sFilterStyle.IsEmpty() || pFormat->GetName().CompareNoCase(sFilterStyle) == 0)
				)
				return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::CanCopy()		//anche per CanCut
{
	HTREEITEM	hSel = m_treeStyle.GetSelectedItem();
	int			nLev = GetItemLevel(hSel);
	
	switch (nLev)
	{
		case (2):
		{
			return TRUE;
			break;
		}
		case (1):
		{
			HTREEITEM		hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeItemRef*	pItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString			strParent	= pItemRef->m_strName;
			
			if (strParent.CompareNoCase(szCurrentReport) == 0)
				return TRUE;

			if (!AfxGetAddOnApp(strParent))
				return FALSE;

			if (AfxGetPathFinder()->IsASystemApplication(strParent))
				return TRUE;
			break;		
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::CanDelete()
{
	// solo se custom
	CTreeItemRef*	pItemRef	= NULL;
	HTREEITEM		hSel		= m_treeStyle.GetSelectedItem();
	HTREEITEM		hParentSel	= NULL;
	Formatter*		Format		= NULL;
	int				nLev		= GetItemLevel(hSel);

	switch (nLev)
	{
		case (2):
		{
			if (!AfxGetLoginInfos()->m_bAdmin)
				return FALSE;
			Format = (Formatter*) m_treeStyle.GetItemData(hSel);
			if (Format->m_FromAndTo ==  Formatter::FROM_CUSTOM)
				return TRUE;
			break;
		}
		case (1):
		{
			hParentSel	= m_treeStyle.GetParentItem(hSel);
			pItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hParentSel);
			CString	strParent	= pItemRef->m_strName;
			
			if (strParent.CompareNoCase(szCurrentReport) == 0)
				return TRUE;

			if (!AfxGetAddOnApp(strParent))
				return FALSE;

			if (AfxGetPathFinder()->IsASystemApplication(strParent))
			{
				if (!AfxGetLoginInfos()->m_bAdmin)
					return FALSE;
				Format = (Formatter*) m_treeStyle.GetItemData(hSel);
				if (Format->m_FromAndTo ==  Formatter::FROM_CUSTOM)
					return TRUE;
			}	
			break;		
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::CanPaste()
{
	HTREEITEM	hSel	= m_treeStyle.GetSelectedItem();
	CString		strApp	= _T("");
	int			nLev	= GetItemLevel(hSel);

	if (!m_FormatCopy)
		return FALSE;

	switch (nLev)
	{
		case (0):
		{
			CTreeItemRef*	pItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hSel);
			strApp = pItemRef->m_strName;

			if (strApp.CompareNoCase(szCurrentReport) == 0)
				return TRUE;
			if (!AfxGetAddOnApp(strApp))
				return FALSE;
			if (AfxGetPathFinder()->IsASystemApplication(strApp))
			{
				if (!AfxGetLoginInfos()->m_bAdmin)
					return FALSE;
				return TRUE;
			}		
			break;
		}
		case (1):
		{
			HTREEITEM		hParentSel	= m_treeStyle.GetParentItem(hSel);
			CTreeItemRef*	pItemRef	= (CTreeItemRef*) m_treeStyle.GetItemData(hParentSel);
			strApp	= pItemRef->m_strName;
			
			if (strApp.CompareNoCase(szCurrentReport) == 0)
				return FALSE;

			if (!AfxGetAddOnApp(strApp))
				return FALSE;
			if (!AfxGetPathFinder()->IsASystemApplication(strApp)) 
			{
				if (!AfxGetLoginInfos()->m_bAdmin)
					return FALSE;
				return TRUE;
			}	
			break;		
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::GetToolTipProperties(CTooltipProperties& tp)
{
	tp.m_strText.Empty();
	if (tp.m_nControlID == ID_FORMAT_CUT)			
		tp.m_strText = _TB("Cut");
	else if (tp.m_nControlID == ID_FORMAT_COPY)		
		tp.m_strText = _TB("Copy");
	else if (tp.m_nControlID == ID_FORMAT_PASTE)		
		tp.m_strText = _TB("Paste");
	else if (tp.m_nControlID == ID_FORMAT_OPEN)		
		tp.m_strText = _TB("Property");
	else if (tp.m_nControlID == ID_FORMAT_APPLYIN)		
		tp.m_strText = _TB("Apply in...");
	else if (tp.m_nControlID == ID_FORMAT_RENAME)		
		tp.m_strText = _TB("Rename");
	else if (tp.m_nControlID == ID_FORMAT_DELETE)		
		tp.m_strText = _TB("Delete");
	else if (tp.m_nControlID == ID_FORMAT_FILTERTREE)
	{
		if (m_bFilterTree)
			tp.m_strText = _TB("Show all modules tree");
		else
			tp.m_strText = _TB("Show only modules with styles");
	}
	else if (tp.m_nControlID == ID_FORMAT_HELP)
		tp.m_strText = _TB("Help on line (F1)");
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFormatDlg::CanApplyContextArea()
{
	if (!AfxGetLoginInfos()->m_bAdmin)
		return FALSE;

	HTREEITEM hSel = m_treeStyle.GetSelectedItem();

	if (hSel == NULL)
		return FALSE;

	int	nLev = GetItemLevel(hSel);

	if (nLev == 0)
		return FALSE;

	Formatter* pFormat = (Formatter*) m_treeStyle.GetItemData(hSel);

	return !pFormat || pFormat->GetSource() == Formatter::FROM_CUSTOM;
}

//-----------------------------------------------------------------------------
void CFormatDlg::FillTreeAddModules (const CString strApps, HTREEITEM hItemApp)
{
	HTREEITEM 		hItemMod	= 0;
	CString			strMods		= _T("");
	CStringArray	aModules;
	CTreeItemRef*	pItemRefLocaliz = NULL;

	AddOnApplication* pAddOnApp = AfxGetAddOnApp(strApps);

	if (!pAddOnApp)
		return;

	AddOnModule* pAddOnMod;
	for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
	{
		pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
		if (!pAddOnMod)
			continue;

		strMods = pAddOnMod->GetModuleName();
		BOOL bAppFormatters = HasApplicationFormatters(strApps, pAddOnMod->GetModuleName(), m_sFilterStyle);
		BOOL bRepFormatters = HasReportFormatters(strApps, pAddOnMod->GetModuleName(), m_sFilterStyle);

		// filtro di visualizzazione
		if (m_bFilterTree && !bAppFormatters && !bRepFormatters)
			continue;

		if (bRepFormatters)
		{
			hItemMod = GetReportTreeItem ();
			FillTreeAddFormats (strApps, pAddOnMod->GetModuleName(), hItemMod);
		}

		if (bAppFormatters || !m_bFilterTree)
		{
			hItemMod = m_treeStyle.InsertItem(pAddOnMod->GetModuleTitle(), 0,0, hItemApp);
			pItemRefLocaliz = new CTreeItemRef(strMods);
			m_arTreeItemRef.Add(pItemRefLocaliz);
			m_treeStyle.SetItemData(hItemMod, (DWORD) pItemRefLocaliz);
			if (bAppFormatters)
				FillTreeAddFormats (strApps, pAddOnMod->GetModuleName(), hItemMod);
		}
	}
}

//-----------------------------------------------------------------------------
void CFormatDlg::FillComboStyle()
{
	// mi salvo la vecchia selezione se esisteva
	int nOldPos = m_CmbStyle.GetCurSel();
	if (nOldPos < 0 || nOldPos >= m_CmbStyle.GetCount())
		nOldPos = 0;

	m_CmbStyle.ResetContent();
	m_CmbStyle.AddString(_TB("<All styles>"));
	for (int n = 0; n <= m_StyleTable.GetUpperBound(); n++)
	{
		FormatterGroup* pFormatterGrp = m_StyleTable.GetAt(n);

		for (int i = 0; i <= pFormatterGrp->m_Formatters.GetUpperBound(); i++)
		{
			Formatter* pFormatter = (Formatter*) pFormatterGrp->m_Formatters.GetAt(i);
			if (pFormatter->m_bDeleted)
				continue;
			
			if (m_CmbStyle.FindStringExact(-1, pFormatter->GetTitle().IsEmpty() ? pFormatter->GetName() : pFormatter->GetTitle()) < 0 )
			{
				int nIdx = m_CmbStyle.AddString(pFormatter->GetTitle().IsEmpty() ? pFormatter->GetName() : pFormatter->GetTitle());
				m_CmbStyle.SetItemData(nIdx, (DWORD_PTR) pFormatter);
			}
		}
	}

	m_CmbStyle.SetCurSel(nOldPos);
}

//-----------------------------------------------------------------------------
void CFormatDlg::FillTreeAddFormats (const CString& strApp, const CString& strMod, HTREEITEM hParentItem)
{
	HTREEITEM 		hItemIni		= 0;
	HTREEITEM		hCurrParent;
	int				nIco			= 1;	// custom

	// default di entrata
	Formatter* pFmtDefault = NULL;

	if (m_DefaultSel == NULL && !m_bIgnoreIdx)
		pFmtDefault	 = m_StyleTable.GetFormatter(m_FormatIdx, !m_bIgnoreIdx ? &m_NsForWoorm : NULL);

	for (int n = 0; n <= m_StyleTable.GetUpperBound(); n++)
	{
		FormatterGroup* pFormatterGrp = m_StyleTable.GetAt(n);

		for (int i = 0; i <= pFormatterGrp->m_Formatters.GetUpperBound(); i++)
		{
			Formatter* pFormat = (Formatter*) pFormatterGrp->m_Formatters.GetAt(i);
			
			// cancellato o non dell'applicazione
			if (pFormat->m_bDeleted || strApp.CompareNoCase(pFormat->GetOwner().GetApplicationName()))
				continue;
	
			// filtro di visualizzazione
			if (!m_sFilterStyle.IsEmpty() && pFormat->GetName().CompareNoCase(m_sFilterStyle))
				continue;

			CString strModFormatTable = pFormat->GetOwner().GetObjectName(CTBNamespace::MODULE);
			
			// modulo se gestito
			if (!strMod.IsEmpty() && strMod.CompareNoCase(strModFormatTable))
				continue;

			if (pFormat->m_FromAndTo == Formatter::FROM_WOORM)
				hCurrParent = GetReportTreeItem();
			else
				hCurrParent = hParentItem;

			if (!hCurrParent)
				continue;

			nIco = pFormat->m_FromAndTo == Formatter::FROM_STANDARD ?  2 : 1;

			hItemIni = m_treeStyle.InsertItem
					(
						pFormat->GetTitle().IsEmpty() ? pFormat->GetName() : pFormat->GetTitle(), 
						nIco, 
						nIco, 
						hCurrParent
					);
			m_treeStyle.SetItemData(hItemIni, (DWORD_PTR) m_StyleTable.GetAt(n)->m_Formatters.GetAt(i));
			
			// se è il default di entrata lo memorizzo		
			Formatter* pFmt = (Formatter*) m_StyleTable.GetAt(n)->m_Formatters.GetAt(i);
			if (pFmtDefault && pFmtDefault == pFmt)
				m_DefaultSel = hItemIni;
		}
	}
}

//-----------------------------------------------------------------------------
HTREEITEM CFormatDlg::GetReportTreeItem()
{
	HTREEITEM hItemReport = m_treeStyle.GetRootItem();
	CTreeItemRef* pItemRef = (CTreeItemRef*) m_treeStyle.GetItemData(hItemReport);
	if (pItemRef->m_strName.CompareNoCase(szCurrentReport) == 0)
		return hItemReport;

	return 0;
}

//--------------------------------------------------------------------------
// If edit control is visible in tree view control, when you send a
// WM_KEYDOWN message to the edit control it will dismiss the edit
// control. When the ENTER key was sent to the edit control, the
// parent window of the tree view control is responsible for updating
// the item's label in TVN_ENDLABELEDIT notification code.
//--------------------------------------------------------------------------
BOOL CFormatDlg::PreTranslateMessage(MSG* pMsg)
{
	if (
		pMsg->message == WM_KEYDOWN &&
		pMsg->wParam == VK_RETURN || pMsg->wParam == VK_ESCAPE 
		)
    {
		CEdit* edit =  m_treeStyle.GetEditControl();
        if (edit)
        {
           edit->SendMessage(WM_KEYDOWN, pMsg->wParam, pMsg->lParam);
           return TRUE;
        }
     }
	
	if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
	{
		m_treeStyle.SendMessage(VK_RETURN);
		m_bFromTree = TRUE;
	}

	return CParsedDialog::PreTranslateMessage(pMsg);
}

//--------------------------------------------------------------------------
// If the edit control of the tree view control has the input focus,
// sending a WM_KEYDOWN message to the edit control will dismiss the
// edit control.  When ENTER key was sent to the edit control, the
// parentwindow of the tree view control is responsible for updating
// the item's label in TVN_ENDLABELEDIT notification code.
//--------------------------------------------------------------------------
BOOL CFormatDlg::IsTreeCtrlEditMessage(WPARAM KeyCode)
{
	BOOL	bValue	= FALSE;
	CWnd*   pWnd	= this;

	if (!pWnd)
		ASSERT(FALSE);

	CTreeFormatDialog* pTreeCtrl = (CTreeFormatDialog*) pWnd->GetDlgItem(IDC_TREE_STYLE);
	if (!pTreeCtrl)
		return bValue;
	
	CWnd*  Focus = GetFocus();
	CEdit* Edit  = pTreeCtrl->GetEditControl();

	if ((CEdit *) Focus == Edit)
	{
		Edit->SendMessage(WM_KEYDOWN, KeyCode); 
		bValue = TRUE;
	}
	return bValue;
}

//----------------------------------------------------------------------------
BOOL CFormatDlg::RefreshFormatsTable ()
{
	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;
	FormatsParser       aParser;
	CStringArray		arModulesRefreshed;

	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp)
			continue;

		for (int n=0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			if (!pAddOnMod)
				continue;

			// lo comunico anche se non dovesse funzionare il , 
			// LoadFonts comunque qualcuno mi ha toccato la tabella.
			if (aParser.RefreshFormats(FormatStyleTablePtr(&m_StyleTable, TRUE), pAddOnMod->m_Namespace, AfxGetPathFinder()))
				arModulesRefreshed.Add(pAddOnApp->GetTitle() + _T(" : ") + pAddOnMod->GetModuleTitle() + _T("\n"));
		}
	}

	if (arModulesRefreshed.GetSize())
	{
		CString sMessage;
		sMessage = _TB("Warning: Formats.ini files in the disk are modified!\nProgram has update tree with new changes.\n\nModules updates are:\n\n");
		for (int i = 0; i <= arModulesRefreshed.GetUpperBound(); i++)
			sMessage += arModulesRefreshed.GetAt(i);
		sMessage += _TB("\n\nVerify customizations before save!");
		AfxMessageBox(sMessage, MB_APPLMODAL);
	}

	return arModulesRefreshed.GetSize();
}
