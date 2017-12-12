
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include "parsobj.h"
#include "parsctrl.h"
#include "baseapp.h"
#include "AutoExprDlg.h"


#include "AutoExprDlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#define EXPRESSION_FORMATTER	_T("%02x%02x%02x%02x%02x")
#define AUTO_EXPRESSION_PREFIX	_T("E")
#define AUTO_VALUE_PREFIX		_T("V")

//----------------------------------------------------------------------------
BOOL IsAutoExpression(const CString& strExpression)
{
	return !strExpression.IsEmpty() && 
			strExpression.Left(1).CompareNoCase(AUTO_EXPRESSION_PREFIX) == 0;
}

//----------------------------------------------------------------------------
BOOL IsReadOnlyAutoExpression(const CString& strExpression)
{
	if (strExpression.IsEmpty())
		return FALSE;
	
	CString strX = strExpression.Left(1);
	
	CString strLow(strX);
	strLow.MakeLower();
	
	return strLow == strX;
}

//----------------------------------------------------------------------------
CString	AddAutoExpressionPrefix (const CString& strExpression, BOOL bValue, BOOL bReadOnly)
{
	CString strPrefix = bValue ? AUTO_VALUE_PREFIX : AUTO_EXPRESSION_PREFIX;
	if (bReadOnly)
		strPrefix.MakeLower();

	return strPrefix + strExpression;
}

//----------------------------------------------------------------------------
CString	RemoveAutoExpressionPrefix(const CString& strExpression)
{
	if (strExpression.IsEmpty())
		return strExpression;
	
	CString strPrefix = strExpression.Left(1);
	strPrefix.MakeUpper();

	if (strPrefix == AUTO_VALUE_PREFIX || strPrefix == AUTO_EXPRESSION_PREFIX)
		return strExpression.Mid(1);

	return strExpression;
}

//----------------------------------------------------------------------------
DataDate GetAutoExpressionValue(int nBegEnd, int nFirstLast, int nDaysTrimQuadSeme, int nWMYE, int nPrevCurr)
{
	DataDate aDataDate;

	switch (nBegEnd)
	{
		case AUTOEXP_EMPTY:
		{
			if (nPrevCurr != AUTOEXP_PREV && nPrevCurr != AUTOEXP_CURR)
				return DataDate::NULLDATE;

			if (nDaysTrimQuadSeme < AUTOEXP_MONDAY || nDaysTrimQuadSeme > AUTOEXP_SUNDAY)
				return DataDate::NULLDATE;

			if (nFirstLast == AUTOEXP_EMPTY)
			{
				if (nWMYE != AUTOEXP_WEEK)
					return DataDate::NULLDATE;

				aDataDate.SetTodayDate();
				aDataDate -= WeekDay(aDataDate);
				if (nPrevCurr == AUTOEXP_PREV)
					aDataDate -= 7;
				aDataDate += (nDaysTrimQuadSeme - 1);

				return aDataDate;
			}

			if (nWMYE != AUTOEXP_MONTH)
				return DataDate::NULLDATE;

			WORD wMonth	= TodayMonth();
			WORD wYear	= TodayYear();
			if (nPrevCurr == AUTOEXP_PREV)
			{
				wMonth--;
				if (wMonth == 0)
				{
					wYear--;
					wMonth = 12;
				}
			}

			if (nFirstLast != AUTOEXP_LAST)
				aDataDate.SetDate(1, wMonth, wYear);
			else
				aDataDate.SetDate((WORD)(MonthDays(wMonth, wYear) - 6), wMonth, wYear);

			aDataDate += (7 - (WeekDay(aDataDate) - (nDaysTrimQuadSeme - 1))) % 7;
				
			if (nFirstLast != AUTOEXP_LAST)
				aDataDate += (nFirstLast - 1) * 7;

			return aDataDate;
		}
		case AUTOEXP_BEGIN:
		case AUTOEXP_END:
		{
			if (nPrevCurr != AUTOEXP_PREV && nPrevCurr != AUTOEXP_CURR)
				return DataDate::NULLDATE;

			if (nFirstLast == AUTOEXP_EMPTY)
				switch (nDaysTrimQuadSeme)
				{
					case AUTOEXP_EMPTY :
					{
						switch (nWMYE)
						{
							case AUTOEXP_WEEK :
							{
								if (nBegEnd == AUTOEXP_BEGIN)
									return GetAutoExpressionValue(AUTOEXP_EMPTY, AUTOEXP_EMPTY, AUTOEXP_MONDAY, nWMYE, nPrevCurr);

								return GetAutoExpressionValue(AUTOEXP_EMPTY, AUTOEXP_EMPTY, AUTOEXP_SUNDAY, nWMYE, nPrevCurr);
							}
							case AUTOEXP_MONTH :
							{
								WORD wMonth	= TodayMonth();
								WORD wYear	= TodayYear();
								if (nPrevCurr == AUTOEXP_PREV)
								{
									wMonth--;
									if (wMonth == 0)
									{
										wYear--;
										wMonth = 12;
									}
								}

								if (nBegEnd == AUTOEXP_BEGIN)
									aDataDate.SetDate(1, wMonth, wYear);
								else
									aDataDate.SetDate(MonthDays(wMonth, wYear), wMonth, wYear);

								return aDataDate;
							}
							case AUTOEXP_YEAR :
							{
								WORD wYear	= TodayYear();
								if (nPrevCurr == AUTOEXP_PREV)
									wYear--;

								if (nBegEnd == AUTOEXP_BEGIN)
									aDataDate.SetDate(1, 1, wYear);
								else
									aDataDate.SetDate(31, 12, wYear);

								return aDataDate;
							}
							// Tb lo chiede alle applicazioni
							case AUTOEXP_ESER :
							{
								CString sEvent;
								if (nBegEnd == AUTOEXP_BEGIN)
									if (nPrevCurr == AUTOEXP_PREV)
										sEvent = szPrevAccPeriodBeginDate; 
									else
										sEvent = szAccPeriodBeginDate;
								else
									if (nPrevCurr == AUTOEXP_PREV)
										sEvent = szPrevAccPeriodEndDate;
									else
										sEvent = szAccPeriodEndDate;
								
								if (sEvent.IsEmpty())
									return DataDate::NULLDATE;

								FailedInvokeCode aError = InvkNoError;
								FunctionDataInterface aFDI;
								AfxGetTbCmdManager()->FireEvent(sEvent, &aFDI,&aError);

								if (aError != InvkNoError)
									return DataDate::NULLDATE;
								
								// verifico che mi sia stato ritornato
								DataObj* pData = aFDI.GetReturnValue();
								if (pData && pData->GetDataType() == DataType::Date)
									return *((DataDate*) pData);

								return DataDate::NULLDATE;
							}
							default:
								return DataDate::NULLDATE;
						}

						break;
					}
					case AUTOEXP_TRIM:
					{
						if (nWMYE != AUTOEXP_EMPTY)
							return DataDate::NULLDATE;

						int nTrim = int((TodayMonth() - 1) / 3 + 1);
						if (nPrevCurr == AUTOEXP_PREV)
						{
							nTrim--;
							if (nTrim == 0)
								nTrim = 4;
							else
								nPrevCurr = AUTOEXP_CURR;
						}

						return GetAutoExpressionValue(nBegEnd, nTrim, nDaysTrimQuadSeme, AUTOEXP_YEAR, nPrevCurr);
					}
					case AUTOEXP_QUAD:
					{
						if (nWMYE != AUTOEXP_EMPTY)
							return DataDate::NULLDATE;

						int nQuad = int((TodayMonth() - 1) / 4 + 1);
						if (nPrevCurr == AUTOEXP_PREV)
						{
							nQuad--;
							if (nQuad == 0)
								nQuad = 3;
							else
								nPrevCurr = AUTOEXP_CURR;
						}

						return GetAutoExpressionValue(nBegEnd, nQuad, nDaysTrimQuadSeme, AUTOEXP_YEAR, nPrevCurr);
					}
					case AUTOEXP_SEME:
					{
						if (nWMYE != AUTOEXP_EMPTY)
							return DataDate::NULLDATE;

						int nSeme = int((TodayMonth() - 1) / 6 + 1);
						if (nPrevCurr == AUTOEXP_PREV)
						{
							nSeme--;
							if (nSeme == 0)
								nSeme = 2;
							else
								nPrevCurr = AUTOEXP_CURR;
						}

						return GetAutoExpressionValue(nBegEnd, nSeme, nDaysTrimQuadSeme, AUTOEXP_YEAR, nPrevCurr);
					}
					default:
						return DataDate::NULLDATE;
				}
			else
			{
				if (nWMYE != AUTOEXP_YEAR && nWMYE != AUTOEXP_ESER)
					return DataDate::NULLDATE;

				WORD wYear;
				WORD wMonth = 0;

				if (nWMYE == AUTOEXP_YEAR)
				{
					wYear = TodayYear();
					if (nPrevCurr == AUTOEXP_PREV)
						wYear--;
				}
				else
				{
					CString sEvent;
					if (nPrevCurr == AUTOEXP_PREV)
						sEvent = szPrevAccPeriodBeginDate; 
					else
						sEvent = szAccPeriodBeginDate;
						
					if (!sEvent.IsEmpty())
					{
						FailedInvokeCode aError = InvkNoError;
						FunctionDataInterface aFDI;
						AfxGetTbCmdManager()->FireEvent(sEvent, &aFDI,&aError);

						if (aError == InvkNoError)
						{										
							// verifico che mi sia stato ritornato
							DataObj* pData = aFDI.GetReturnValue();
							if (pData && pData->GetDataType() == DataType::Date)
								aDataDate = *((DataDate*) pData);
						}
					}

					wMonth = (WORD)(aDataDate.Month() - 1);
					wYear = aDataDate.Year();
				}

				switch (nDaysTrimQuadSeme)
				{
					case AUTOEXP_TRIM:
					{
						wMonth += nBegEnd == AUTOEXP_BEGIN
							? (nFirstLast - 1) * 3 + 1
							: nFirstLast * 3;
						break;
					}
					case AUTOEXP_QUAD:
					{
						wMonth += nBegEnd == AUTOEXP_BEGIN
							? (nFirstLast - 1) * 4 + 1
							: nFirstLast * 4;
						break;
					}
					case AUTOEXP_SEME:
					{
						wMonth += nBegEnd == AUTOEXP_BEGIN
							? (nFirstLast - 1) * 6 + 1
							: nFirstLast * 6;
						break;
					}
					default:
						return DataDate::NULLDATE;
				}

				if (nWMYE == AUTOEXP_ESER && wMonth > 12)
				{
					wMonth -= 12;
					wYear++;
				}

				if (nBegEnd == AUTOEXP_BEGIN)
					aDataDate.SetDate(1, wMonth, wYear);
				else
					aDataDate.SetDate(MonthDays(wMonth, wYear), wMonth, wYear);

				return aDataDate;
			}
		}
		case AUTOEXP_TODAY:
		{
			aDataDate.SetTodayDate();
			return aDataDate;
		}
		case AUTOEXP_YESTERDAY:
		{
			aDataDate.SetTodayDate();
			return aDataDate.GiulianDate() - 1;
		}
		default:	// 1-31
		{
			if (nWMYE != AUTOEXP_MONTH || (nPrevCurr != AUTOEXP_PREV && nPrevCurr != AUTOEXP_CURR))
				return DataDate::NULLDATE;

			WORD wMonth	= TodayMonth();
			WORD wYear	= TodayYear();
			if (nPrevCurr == AUTOEXP_PREV)
			{
				wMonth--;
				if (wMonth == 0)
				{
					wYear--;
					wMonth = 12;
				}
			}

			aDataDate.SetDate(min(nBegEnd, MonthDays(wMonth, wYear)), wMonth, wYear);
			return aDataDate;
		}
	}

	return DataDate::NULLDATE;
}

//-----------------------------------------------------------------------------
BOOL GetAutoExpressionValue(const CString& strAutomaticExpression, DataObj* pDataObj) 
{
	if (!pDataObj || pDataObj->GetDataType() != DATA_DATE_TYPE)
		return FALSE;

	int nBegEnd;
	int nFirstLast;
	int nDaysTrimQuadSeme;
	int nWMYE;
	int nPrevCurr;
	_stscanf_s
		(
			RemoveAutoExpressionPrefix(strAutomaticExpression), EXPRESSION_FORMATTER,
			&nBegEnd, &nFirstLast, &nDaysTrimQuadSeme, &nWMYE, &nPrevCurr
		);

	DataDate aDataDate = ::GetAutoExpressionValue(nBegEnd, nFirstLast, nDaysTrimQuadSeme, nWMYE, nPrevCurr);
	if (aDataDate.IsEmpty())
		return FALSE;

	((DataDate*)pDataObj)->Assign(aDataDate.Str(0,0));
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL ModifyAutoExpressionString	(CString& strAutomaticExpression, const DataType& aDataType) 
{
	if (aDataType != DATA_DATE_TYPE)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	AutoExprDlg dlg;

	dlg.m_strAutomaticExpression = strAutomaticExpression;

	if (dlg.DoModal() != IDOK)
		return FALSE;

	strAutomaticExpression = dlg.m_strAutomaticExpression;
	return TRUE;
}

//=============================================================================
//		AutoExprDlg implementation
//=============================================================================
/******************************************************************************

				Editatore Espressioni Data
				==========================

					Frasi previste
					--------------

1) "Oggi/Ieri"
2) "#Giorno" "Mese" "Precedente/Corrente"
3) "Primo/.../Quarto/Ultimo" "Lunedi/.../Domenica" "Mese" "Corrente/Precedente"
4) "Lunedi/.../Domenica" "Settimana" "Corrente/Precedente"
5) "Inizio/Fine" "Settimana/Mese/Anno/Esercizio" "Corrente/Precedente"
6) "Inizio/Fine" "Trimestre/Quadrimestre/Semestre" "Corrente/Precedente"
7) "Inizio/Fine" "Primo/.../Quarto"  "Trimestre/Quadrimestre/Semestre" "Anno/Esercizio" "Corrente/Precedente"

******************************************************************************/

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(AutoExprDlg, CParsedDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(AutoExprDlg, CParsedDialog)
	//{{AFX_MSG_MAP( AutoExprDlg )
	ON_EN_CHANGE		(IDC_AUTOEXPR_EDIT_DAY_NUMBER,			OnDayNumberChange)
	ON_EN_VALUE_CHANGED	(IDC_AUTOEXPR_EDIT_DAY_NUMBER,			OnDayNumberChange)
	ON_BN_CLICKED		(IDC_AUTOEXPR_CHECK_MONTH_DAY,			OnMonthDayClicked)
	ON_CBN_SELCHANGE	(IDC_AUTOEXPR_COMBO_BEGIN_END,			OnSelChanged)
	ON_CBN_SELCHANGE	(IDC_AUTOEXPR_COMBO_FIRST_LAST,			OnSelChanged)
	ON_CBN_SELCHANGE	(IDC_AUTOEXPR_COMBO_DAYS_TRI_QUA_SEM,	OnSelChanged)
	ON_CBN_SELCHANGE	(IDC_AUTOEXPR_COMBO_WMYE,				OnSelChanged)
	ON_CBN_SELCHANGE	(IDC_AUTOEXPR_COMBO_PREV_CURR,			OnSelChanged)
	ON_CBN_DROPDOWN		(IDC_AUTOEXPR_COMBO_FIRST_LAST,			OnFirstLastDropped)
	ON_CBN_DROPDOWN		(IDC_AUTOEXPR_COMBO_DAYS_TRI_QUA_SEM,	OnDaysTrimQuadSemeDropped)
	ON_CBN_DROPDOWN		(IDC_AUTOEXPR_COMBO_WMYE,				OnWMYEDropped)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//=============================================================================
//		AutoExprDlg implementation
//=============================================================================
//-----------------------------------------------------------------------------
AutoExprDlg::AutoExprDlg ()
	:
	CParsedDialog	(IDD_AUTOEXPR_EDIT_DLG, NULL)
{}

//-----------------------------------------------------------------------------
void AutoExprDlg::ParseExpression()
{
	if (m_strAutomaticExpression.IsEmpty())
	{
		SetCurSel(m_cboBegEnd, AUTOEXP_TODAY);
		return;
	}

	int nBegEnd;
	int nFirstLast;
	int nDaysTrimQuadSeme;
	int nWMYE;
	int nPrevCurr;
	// Cambiata la format string con 0 padding => per retrocompatibilità con il valori
	// già in uso dai clienti sostituisco il blank con lo 0
	m_strAutomaticExpression.Replace(_T(' '), _T('0'));
	_stscanf_s
		(
			m_strAutomaticExpression, EXPRESSION_FORMATTER,
			&nBegEnd, &nFirstLast, &nDaysTrimQuadSeme, &nWMYE, &nPrevCurr
		);

	SetCurSel			(m_cboBegEnd, nBegEnd);

	OnFirstLastDropped	();
	SetCurSel			(m_cboFirstLast, nFirstLast);

	OnDaysTrimQuadSemeDropped	();
	SetCurSel					(m_cboDaysTrimQuadSeme, nDaysTrimQuadSeme);

	OnWMYEDropped	();
	SetCurSel		(m_cboWMYE, nWMYE);

	SetCurSel		(m_cboPrevCurr, nPrevCurr);
}

//-----------------------------------------------------------------------------
void AutoExprDlg::UpdateClause()
{
	int nBegEnd				= (int)GetCurSel(m_cboBegEnd);
	int nFirstLast			= (int)GetCurSel(m_cboFirstLast);
	int nDaysTrimQuadSeme	= (int)GetCurSel(m_cboDaysTrimQuadSeme);
	int nWMYE				= (int)GetCurSel(m_cboWMYE);
	int nPrevCurr			= (int)GetCurSel(m_cboPrevCurr);

	CString strClause;
	
	DataDate aDataDate = GetAutoExpressionValue
		(
			nBegEnd, nFirstLast, nDaysTrimQuadSeme, nWMYE, nPrevCurr
		);

	CString strDate;
	if (aDataDate.IsEmpty())
		strDate = _TB("*** Invalid date ***");
	else
		strDate = aDataDate.Str(1);
	
	strClause += _T("\r\n");
	strClause += cwsprintf(_TB("Example: today's date the defined day is: {0-%s}"), (LPCTSTR) strDate);

	m_edtClause.SetWindowText(strClause);
}

//----------------------------------------------------------------------------
void AutoExprDlg::MakeFirstLastSex(CString& str, BOOL bFem)
{
	if (str.IsEmpty()) return;

	int nNewLen = str.GetLength() - 2;
	TCHAR* pStr = str.GetBufferSetLength(nNewLen);
	if (bFem) pStr[nNewLen - 1] = _T('a');

	str.ReleaseBuffer();
}

//-----------------------------------------------------------------------------
int AutoExprDlg::GetCurSel(CComboBox& aComboBox)
{
	if	(
			aComboBox.GetDlgCtrlID() == IDC_AUTOEXPR_COMBO_BEGIN_END &&
			m_btnMonthDay.GetCheck()
		)
	{
		int nVal = m_edtDayNum.GetValue();
		return nVal < 1 || nVal > 31 ? AUTOEXP_EMPTY : nVal;
	}
		
	if (aComboBox.GetCurSel() < 0)
		return AUTOEXP_EMPTY;

	return aComboBox.GetItemData(aComboBox.GetCurSel());
}

//-----------------------------------------------------------------------------
void AutoExprDlg::SetCurSel(CComboBox& aComboBox, int nSel)
{
	if (aComboBox.GetDlgCtrlID() == IDC_AUTOEXPR_COMBO_BEGIN_END)
	{
		if (nSel >= 1 && nSel <= 31)
		{
			m_cboBegEnd.ShowWindow(SW_HIDE);
			m_edtDayNum.ShowCtrl(SW_SHOW);
			m_edtDayNum.SetValue(nSel);
			m_btnMonthDay.SetCheck(TRUE);
			return;
		}

		m_cboBegEnd.ShowWindow(SW_SHOW);
		m_edtDayNum.ShowCtrl(SW_HIDE);
		m_edtDayNum.SetValue(0);
		m_btnMonthDay.SetCheck(FALSE);
	}

	for (int i = 0; i < aComboBox.GetCount(); i++)
		if ((int) aComboBox.GetItemData(i) == nSel)
		{
			aComboBox.SetCurSel(i);
			return;
		}

	aComboBox.SetCurSel(-1);
}

//-----------------------------------------------------------------------------
void AutoExprDlg::AddItem(CComboBox& aComboBox, const CString &str, int nVal)
{
	aComboBox.SetItemData(aComboBox.AddString(str), nVal);
}

//-----------------------------------------------------------------------------
BOOL AutoExprDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_edtDayNum.CParsedCtrl::Attach	(BTN_SPIN_ID);
	m_edtDayNum.SetRange			(1, 31);
	m_edtDayNum.SubclassEdit		(IDC_AUTOEXPR_EDIT_DAY_NUMBER, this);
	m_edtDayNum.SetCtrlMaxLen		(2);

	VERIFY(m_btnMonthDay.			SubclassDlgItem	(IDC_AUTOEXPR_CHECK_MONTH_DAY,			this));
	VERIFY(m_cboBegEnd.				SubclassDlgItem	(IDC_AUTOEXPR_COMBO_BEGIN_END,			this));
	VERIFY(m_cboFirstLast.			SubclassDlgItem	(IDC_AUTOEXPR_COMBO_FIRST_LAST,			this));
	VERIFY(m_cboDaysTrimQuadSeme.	SubclassDlgItem	(IDC_AUTOEXPR_COMBO_DAYS_TRI_QUA_SEM,	this));
	VERIFY(m_cboWMYE.				SubclassDlgItem	(IDC_AUTOEXPR_COMBO_WMYE,				this));
	VERIFY(m_cboPrevCurr.			SubclassDlgItem	(IDC_AUTOEXPR_COMBO_PREV_CURR,			this));
	VERIFY(m_edtClause.				SubclassDlgItem	(IDC_AUTOEXPR_CLAUSE,					this));

	m_cboBegEnd.			SetExtendedUI();
	m_cboFirstLast.			SetExtendedUI();
	m_cboDaysTrimQuadSeme.	SetExtendedUI();
	m_cboWMYE.				SetExtendedUI();
	m_cboPrevCurr.			SetExtendedUI();

	AddItem(m_cboBegEnd, _T(""),		AUTOEXP_EMPTY);
	AddItem(m_cboBegEnd, _TB("Start Date"),	AUTOEXP_BEGIN);
	AddItem(m_cboBegEnd, _TB("End Date"),	AUTOEXP_END);
	AddItem(m_cboBegEnd, _TB("Today"),	AUTOEXP_TODAY);
	AddItem(m_cboBegEnd, _TB("Yesterday"),	AUTOEXP_YESTERDAY);

	AddItem(m_cboPrevCurr, _TB("Current"), AUTOEXP_CURR);
	AddItem(m_cboPrevCurr, _TB("Previous"), AUTOEXP_PREV);

	ParseExpression();
	OnSelChanged();

	return TRUE;
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnOK()
{
	int nBegEnd				= (int)GetCurSel(m_cboBegEnd);
	int nFirstLast			= (int)GetCurSel(m_cboFirstLast);
	int nDaysTrimQuadSeme	= (int)GetCurSel(m_cboDaysTrimQuadSeme);
	int nWMYE				= (int)GetCurSel(m_cboWMYE);
	int nPrevCurr			= (int)GetCurSel(m_cboPrevCurr);

	DataDate aDataDate = GetAutoExpressionValue
		(
			nBegEnd, nFirstLast, nDaysTrimQuadSeme, nWMYE, nPrevCurr
		);

	if (aDataDate.IsEmpty())
		return;

	m_strAutomaticExpression.Format
		(	  
			EXPRESSION_FORMATTER,
			nBegEnd, nFirstLast, nDaysTrimQuadSeme, nWMYE, nPrevCurr
		);

	if (nBegEnd <= 15)
		m_strAutomaticExpression = _T("0") + m_strAutomaticExpression.Trim();

	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnMonthDayClicked()
{
	if (m_btnMonthDay.GetCheck())
	{
		int nDay = m_edtDayNum.GetValue();
		if (nDay < 1 || nDay > 31)
			m_edtDayNum.SetValue(1);

		m_cboBegEnd.ShowWindow(SW_HIDE);
		m_edtDayNum.ShowCtrl(SW_SHOW);
	}
	else
	{
		m_edtDayNum.ShowCtrl(SW_HIDE);
		m_cboBegEnd.ShowWindow(SW_SHOW);
	}

	OnSelChanged();
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnDayNumberChange()
{
	if (!IsTBWindowVisible(&m_edtDayNum))
		return;

	int nDay = m_edtDayNum.GetValue();
	if (nDay >= 1 && nDay <= 31)
		UpdateClause();
	else
		MessageBeep((UINT)-1);
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnSelChanged()
{
	switch (GetCurSel(m_cboBegEnd))
	{
		case AUTOEXP_TODAY:
		case AUTOEXP_YESTERDAY:
		{
			m_cboFirstLast.			EnableWindow(FALSE);
			m_cboDaysTrimQuadSeme.	EnableWindow(FALSE);
			m_cboWMYE.				EnableWindow(FALSE);
			m_cboPrevCurr.			EnableWindow(FALSE);

			m_cboFirstLast.			SetCurSel(-1);
			m_cboDaysTrimQuadSeme.	SetCurSel(-1);
			m_cboWMYE.				SetCurSel(-1);
			m_cboPrevCurr.			SetCurSel(-1);
			break;
		}
		case AUTOEXP_EMPTY:
		{
			m_cboFirstLast.			EnableWindow(TRUE);
			m_cboDaysTrimQuadSeme.	EnableWindow(TRUE);
			m_cboWMYE.				EnableWindow(TRUE);
			m_cboPrevCurr.			EnableWindow(TRUE);

			int nSelDaysTrimQuadSeme = GetCurSel(m_cboDaysTrimQuadSeme);

			if (!(nSelDaysTrimQuadSeme >= AUTOEXP_MONDAY && nSelDaysTrimQuadSeme <= AUTOEXP_SUNDAY))
				m_cboDaysTrimQuadSeme.SetCurSel(-1);

			m_cboWMYE.ResetContent();
			if	(
					nSelDaysTrimQuadSeme < AUTOEXP_MONDAY	||
					nSelDaysTrimQuadSeme > AUTOEXP_SUNDAY
				)
			{
				m_cboWMYE.SetCurSel(-1);
				break;
			}

			if (GetCurSel(m_cboFirstLast) == AUTOEXP_EMPTY)
			{
				AddItem		(m_cboWMYE, _TB("Week"), AUTOEXP_WEEK);
				SetCurSel	(m_cboWMYE, AUTOEXP_WEEK);
			}
			else
			{
				AddItem		(m_cboWMYE, _TB("Month"), AUTOEXP_MONTH);
				SetCurSel	(m_cboWMYE, AUTOEXP_MONTH);
			}

			break;
		}
		case AUTOEXP_BEGIN:
		case AUTOEXP_END:
		{
			m_cboFirstLast.			EnableWindow(TRUE);
			m_cboDaysTrimQuadSeme.	EnableWindow(TRUE);
			m_cboWMYE.				EnableWindow(TRUE);
			m_cboPrevCurr.			EnableWindow(TRUE);

			int nSelFirstLast = GetCurSel(m_cboFirstLast);
			switch(nSelFirstLast)
			{
				case AUTOEXP_EMPTY:
				{
					int nSelDaysTrimQuadSeme = GetCurSel(m_cboDaysTrimQuadSeme);

					if	(
							nSelDaysTrimQuadSeme == AUTOEXP_TRIM ||
							nSelDaysTrimQuadSeme == AUTOEXP_QUAD ||
							nSelDaysTrimQuadSeme == AUTOEXP_SEME
						)
					{
						m_cboWMYE.EnableWindow(FALSE);
						m_cboWMYE.SetCurSel(-1);
						break;
					}

					if (nSelDaysTrimQuadSeme >= AUTOEXP_MONDAY && nSelDaysTrimQuadSeme <= AUTOEXP_SUNDAY)
						m_cboDaysTrimQuadSeme.SetCurSel(-1);
	
					break;
				}
				case AUTOEXP_LAST:
				{
					m_cboFirstLast.			SetCurSel(-1);
					m_cboDaysTrimQuadSeme.	SetCurSel(-1);
					m_cboWMYE.				SetCurSel(-1);
					break;
				}
				default:
				{
					int nSelDaysTrimQuadSeme = GetCurSel(m_cboDaysTrimQuadSeme);

					if	(
							nSelDaysTrimQuadSeme >= AUTOEXP_MONDAY && nSelDaysTrimQuadSeme <= AUTOEXP_SUNDAY	||
							nSelFirstLast == AUTOEXP_THIRD && nSelDaysTrimQuadSeme == AUTOEXP_SEME
						)
						m_cboDaysTrimQuadSeme.SetCurSel(-1);
					else
						if (nSelFirstLast == AUTOEXP_FOURTH && nSelDaysTrimQuadSeme != AUTOEXP_TRIM)
						{
							m_cboDaysTrimQuadSeme.ResetContent();
							AddItem		(m_cboDaysTrimQuadSeme, _TB("Quarter"), AUTOEXP_TRIM);
							SetCurSel	(m_cboDaysTrimQuadSeme, AUTOEXP_TRIM);
						}

					int nSelWMYE = GetCurSel(m_cboWMYE);

					if (nSelWMYE != AUTOEXP_YEAR && nSelWMYE != AUTOEXP_ESER)
						m_cboWMYE.SetCurSel(-1);

					break;
				}
			}
			break;
		}
		default: //1-31
		{
			m_cboFirstLast.			EnableWindow(FALSE);
			m_cboDaysTrimQuadSeme.	EnableWindow(FALSE);
			m_cboWMYE.				EnableWindow(TRUE);
			m_cboPrevCurr.			EnableWindow(TRUE);

			m_cboFirstLast.			SetCurSel(-1);
			m_cboDaysTrimQuadSeme.	SetCurSel(-1);

			if (GetCurSel(m_cboWMYE) != AUTOEXP_MONTH)
			{
				m_cboWMYE.ResetContent	();
				AddItem					(m_cboWMYE, _TB("Month"), AUTOEXP_MONTH);
				SetCurSel				(m_cboWMYE, AUTOEXP_MONTH);
			}
		}
	}

	UpdateClause();
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnFirstLastDropped()
{
	int nSel = GetCurSel(m_cboFirstLast);
	m_cboFirstLast.ResetContent();

	AddItem(m_cboFirstLast, 0,					AUTOEXP_EMPTY);
	AddItem(m_cboFirstLast, _TB("First"),	AUTOEXP_FIRST);
	AddItem(m_cboFirstLast, _TB("Second"),	AUTOEXP_SECOND);
	AddItem(m_cboFirstLast, _TB("Third"),	AUTOEXP_THIRD);
	AddItem(m_cboFirstLast, _TB("Fourth"),	AUTOEXP_FOURTH);

	if (GetCurSel(m_cboBegEnd) == AUTOEXP_EMPTY)
		AddItem(m_cboFirstLast, _TB("Last"), AUTOEXP_LAST);

	SetCurSel(m_cboFirstLast, nSel);
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnDaysTrimQuadSemeDropped()
{
	int nSel = GetCurSel(m_cboDaysTrimQuadSeme);
	m_cboDaysTrimQuadSeme.ResetContent();

	if (GetCurSel(m_cboBegEnd) == AUTOEXP_EMPTY)
	{
		AddItem(m_cboDaysTrimQuadSeme, _TB("Monday"),		AUTOEXP_MONDAY);
		AddItem(m_cboDaysTrimQuadSeme, _TB("Tuesday"),		AUTOEXP_TUESDAY);
		AddItem(m_cboDaysTrimQuadSeme, _TB("Wednesday"),	AUTOEXP_WEDNESDAY);
		AddItem(m_cboDaysTrimQuadSeme, _TB("Thursday"),	AUTOEXP_THURSDAY);
		AddItem(m_cboDaysTrimQuadSeme, _TB("Friday"),		AUTOEXP_FRIDAY);
		AddItem(m_cboDaysTrimQuadSeme, _TB("Saturday"),	AUTOEXP_SATURDAY);
		AddItem(m_cboDaysTrimQuadSeme, _TB("Sunday"),		AUTOEXP_SUNDAY);
	}
	else
	{
		switch (GetCurSel(m_cboFirstLast))
		{
			case AUTOEXP_EMPTY:
				AddItem(m_cboDaysTrimQuadSeme, 0, AUTOEXP_EMPTY);
				// continua nel case seguente
			case AUTOEXP_FIRST:
			case AUTOEXP_SECOND:
				AddItem(m_cboDaysTrimQuadSeme, _TB("Quarter"), AUTOEXP_TRIM);
				AddItem(m_cboDaysTrimQuadSeme, _TB("Four-month period"), AUTOEXP_QUAD);
				AddItem(m_cboDaysTrimQuadSeme, _TB("Half Year"), AUTOEXP_SEME);
				break;

			case AUTOEXP_THIRD:
				AddItem(m_cboDaysTrimQuadSeme, _TB("Quarter"), AUTOEXP_TRIM);
				AddItem(m_cboDaysTrimQuadSeme, _TB("Four-month period"), AUTOEXP_QUAD);
				break;

			case AUTOEXP_FOURTH:
				AddItem(m_cboDaysTrimQuadSeme, _TB("Quarter"), AUTOEXP_TRIM);
				break;
		}
	}

	SetCurSel(m_cboDaysTrimQuadSeme, nSel);
}

//-----------------------------------------------------------------------------
void AutoExprDlg::OnWMYEDropped()
{
	int nSel = GetCurSel(m_cboWMYE);
	m_cboWMYE.ResetContent();

	int nSelNumsBegEnd = GetCurSel(m_cboBegEnd);
	int nSelFirstLast = GetCurSel(m_cboFirstLast);
	int nSelDaysTrimQuadSeme = GetCurSel(m_cboDaysTrimQuadSeme);

	if	(
			(
				(
					nSelNumsBegEnd == AUTOEXP_BEGIN	||
					nSelNumsBegEnd == AUTOEXP_END
				)								&&
				nSelFirstLast == AUTOEXP_EMPTY	&&
				nSelDaysTrimQuadSeme == AUTOEXP_EMPTY
			)	||
			(
				nSelNumsBegEnd == AUTOEXP_EMPTY	&&
				nSelFirstLast == AUTOEXP_EMPTY	&&
				(
					nSelDaysTrimQuadSeme >= AUTOEXP_MONDAY	&&
					nSelDaysTrimQuadSeme <= AUTOEXP_SUNDAY
				)
			)
		)
		AddItem(m_cboWMYE, _TB("Week"), AUTOEXP_WEEK);

	if	(
			nSelNumsBegEnd >= 1 && nSelNumsBegEnd <= 31 ||
			(
				(
					nSelNumsBegEnd == AUTOEXP_BEGIN	||
					nSelNumsBegEnd == AUTOEXP_END
				)								&&
				nSelFirstLast == AUTOEXP_EMPTY	&&
				nSelDaysTrimQuadSeme == AUTOEXP_EMPTY
			)	||
			nSelNumsBegEnd == AUTOEXP_EMPTY	&&
			nSelFirstLast != AUTOEXP_EMPTY	&&
			(
				nSelDaysTrimQuadSeme >= AUTOEXP_MONDAY &&
				nSelDaysTrimQuadSeme <= AUTOEXP_SUNDAY
			)
		)
		AddItem(m_cboWMYE, _TB("Month"), AUTOEXP_MONTH);

	if	(
			(
				nSelNumsBegEnd == AUTOEXP_BEGIN ||
				nSelNumsBegEnd == AUTOEXP_END
			) &&
			(
				nSelFirstLast == AUTOEXP_FIRST	||
				nSelFirstLast == AUTOEXP_SECOND	||
				nSelFirstLast == AUTOEXP_THIRD	||
				nSelFirstLast == AUTOEXP_FOURTH ||
				(
					nSelFirstLast == AUTOEXP_EMPTY &&
					nSelDaysTrimQuadSeme == AUTOEXP_EMPTY
				)
			)
		)
	{
		AddItem(m_cboWMYE, _TB("Year"), AUTOEXP_YEAR);
		AddItem(m_cboWMYE, _TB("Fiscal year"), AUTOEXP_ESER);
	}	

	SetCurSel(m_cboWMYE, nSel);
}
