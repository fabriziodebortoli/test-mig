
#pragma once

#include "PARSEDT.h"

//includere alla fine degli include del .H
#include "beginh.dex"


#define AUTOEXP_EMPTY			0xFF

#define	AUTOEXP_MIN_MONTH_DAY_NUMBER		0x01	// 1
#define	AUTOEXP_MAX_MONTH_DAY_NUMBER		0x1F	// 31
#define AUTOEXP_BEGIN						0x20
#define AUTOEXP_END							0x21
#define AUTOEXP_TODAY						0x22
#define AUTOEXP_YESTERDAY					0x23

#define AUTOEXP_FIRST			0x01
#define AUTOEXP_SECOND			0x02
#define AUTOEXP_THIRD			0x03
#define AUTOEXP_FOURTH			0x04
#define AUTOEXP_LAST			0x05

#define AUTOEXP_MONDAY			0x01
#define AUTOEXP_TUESDAY			0x02
#define AUTOEXP_WEDNESDAY		0x03
#define AUTOEXP_THURSDAY		0x04
#define AUTOEXP_FRIDAY			0x05
#define AUTOEXP_SATURDAY		0x06
#define AUTOEXP_SUNDAY			0x07
#define AUTOEXP_TRIM			0x08
#define AUTOEXP_QUAD			0x09
#define AUTOEXP_SEME			0x0A

#define AUTOEXP_WEEK			0x01
#define AUTOEXP_MONTH			0x02
#define AUTOEXP_YEAR			0x03
#define AUTOEXP_ESER			0x04

#define AUTOEXP_CURR			0x01
#define AUTOEXP_PREV			0x02

TB_EXPORT DataDate	GetAutoExpressionValue		(int, int, int, int, int);
TB_EXPORT BOOL		GetAutoExpressionValue		(const CString&, DataObj*);
TB_EXPORT BOOL		ModifyAutoExpressionString	(CString&, const DataType&);

TB_EXPORT BOOL		IsAutoExpression			(const CString& strExpression);
TB_EXPORT BOOL		IsReadOnlyAutoExpression	(const CString& strExpression);
TB_EXPORT CString	AddAutoExpressionPrefix		(const CString& strExpression, BOOL bValue, BOOL bReadOnly);
TB_EXPORT CString	RemoveAutoExpressionPrefix	(const CString& strExpression);


//=============================================================================
//		AutoExprDlg definition
//=============================================================================
class TB_EXPORT AutoExprDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(AutoExprDlg)
protected:
	CButton		m_btnMonthDay;
	CIntEdit	m_edtDayNum;
	CBCGPComboBox	m_cboBegEnd;
	CBCGPComboBox	m_cboFirstLast;
	CBCGPComboBox	m_cboDaysTrimQuadSeme;
	CBCGPComboBox	m_cboWMYE;
	CBCGPComboBox	m_cboPrevCurr;
	CStatic		m_edtClause;

public:
	CString		m_strAutomaticExpression;

public:
	AutoExprDlg ();

protected:
	void	ParseExpression	();
	void	UpdateClause	();
	void	MakeFirstLastSex(CString& str, BOOL bFem);
	int		GetCurSel		(CComboBox&);
	void	SetCurSel		(CComboBox&, int);
	void	AddItem			(CComboBox&, const CString&, int);

protected:
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();

	//{{AFX_MSG(AliasMngDlg)
	afx_msg		void	OnMonthDayClicked			();
	afx_msg		void	OnDayNumberChange			();
	afx_msg		void	OnSelChanged				();
	afx_msg		void	OnFirstLastDropped			();
	afx_msg		void	OnDaysTrimQuadSemeDropped	();
	afx_msg		void	OnWMYEDropped				();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};


#include "endh.dex"

