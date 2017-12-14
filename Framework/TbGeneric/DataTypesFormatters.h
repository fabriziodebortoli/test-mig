#pragma once

#include <TbGeneric\FormatsHelpers.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\JsonTags.h>

#include <TbGeneric\FormatsTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
// gestione lunghezza primo segmento dell'elapsedtime
// caso base: precisione PRECISION_ZERO
#define	FMT_ELAPSED_TIME_D_LEN	 5
#define	FMT_ELAPSED_TIME_H_LEN   6
#define	FMT_ELAPSED_TIME_M_LEN	 8
#define	FMT_ELAPSED_TIME_S_LEN  10

//----------------------------------------------------------------------------
static const int	nDefDecimals		= 2;
static const TCHAR	szDefDateSep[]		=_T("/");
static const TCHAR	szDefDecSeparator[]	=_T(",");
static const TCHAR	szDefTimeSep[]		=_T(":");
static const TCHAR	szDefLng1000Sep[]	=_T("");
static const TCHAR	szDefDbl1000Sep[]	=_T(".");
static const TCHAR	szDefSetSep[]		=_T(",");
static const TCHAR	szZeroAsDash[]		=_T("--");
static const TCHAR  szDefTimeAM[]		=_T("AM");
static const TCHAR  szDefTimePM[]		=_T("PM");

static const TCHAR	szDefaultTrueTag[]	=_T("Si");	//da non localizzare (usati come default a prescindere dalla lingua)
static const TCHAR	szDefaultFalseTag[]	=_T("No");	//da non localizzare (usati come default a prescindere dalla lingua)

static const TCHAR szYearChar = _T('Y');
static const TCHAR szNumberChar = _T('#');
static const TCHAR szSuffixChar = _T('?');
static const TCHAR szSiteCodeChar = _T('*');
static const TCHAR szNonEditableSuffix = _T('-');
static const TCHAR szCommaChar = _T(',');
static const TCHAR szNoPaddingNo = _T('N');
//----------------------------------------------------------------------------

// General Functions
//============================================================================
TB_EXPORT Formatter* PolyNewFormatter	(
											const DataType&			aDataType, 
											const Formatter::FormatStyleSource& aSource = Formatter::FROM_STANDARD,
											const CTBNamespace&		aOwner = CTBNamespace(),
											LPCTSTR					aName = NULL
										);

//============================================================================
class TB_EXPORT CFormatMask
{
	BOOL	m_bEnabled;
	CString	m_strMask;
	CString	m_sSiteCode;
	
	// current mask features
	BOOL	m_bIsIrrilevant;
	int		m_nEditableZoneStart;
	int		m_nEditableZoneEnd;
	int		m_nSuffixStart;
	int		m_HowManyOfYear;
	int		m_nNumberStartAt;
	int		m_HowManyOfNumber;
	int		m_HowManyOfDecimal;
	int		m_HowManyOfSiteCode;


public:
	CFormatMask();

public:
	const CString&	GetMask				() const { return m_strMask; }
	const BOOL		IsEnabled			() const { return m_bEnabled; }
	const BOOL		IsEmpty				() const { return m_bIsIrrilevant; }
	const int&		GetEditableZoneStart() const { return m_nEditableZoneStart; }
	const int&		GetSuffixStart		() const { return m_nSuffixStart; }

	CString ApplyMask	(const CString str, BOOL bZeroPadded, int nYear = 0) const;

	long	GetNumberFromMask	(const CString& sText) const;
	CString	GetSuffixFromMask	(const CString& sText) const;
	void	SetSiteCode			(const CString& sSiteCode);
	void	SetMask				(const CString& sMask);
	void	SetEnabled			(const BOOL& bValue);

	// operators
	int					operator==	(const CFormatMask& mask) const	{ return !Compare(mask); }
	int					operator!=	(const CFormatMask& mask) const	{ return Compare(mask); }
	const CFormatMask&	operator = (const CFormatMask& mask) ;

	void Assign	(const CFormatMask& mask);
	int  Compare(const CFormatMask& mask) const;

private:
	bool	IsToApply		(const CString& sValue) const;
	void	InitMaskInfo	();
	void	ParseMaskInfo	();
};

//============================================================================
class TB_EXPORT CLongFormatter : public Formatter
{
	friend class CIntFormatDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CLongFormatter)

public:
	enum FormatTag 
	{
		NUMERIC			= 0x0000,
		LETTER			= 0x0001,
		ENCODED			= 0x0002,
		ZERO_AS_DASH	= 0x0099
	};

	enum SignTag 
		{
			ABSOLUTEVAL		= 0x0000, 
			MINUSPREFIX		= 0x0001, 
			MINUSPOSTFIX	= 0x0002, 
			ROUNDS			= 0x0003, 
			SIGNPREFIX		= 0x0004, 
			SIGNPOSTFIX		= 0x0005 
		};

private:
	FormatTag	m_FormatType;

protected:
	SignTag		m_Sign;
	CString		m_strXTable;
	BOOL		m_bIs1000SeparatorDefault;
	CString		m_str1000Separator;
	CString		m_strAsZeroValue;


public:
	CLongFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	FormatTag		GetFormat		()	{return m_FormatType;}
	void			SetFormat		(FormatTag format){m_FormatType = format;}
	virtual BOOL	IsSameAsDefault	() const;

	//	const void* = const long*
	virtual void 	Format			(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual CString	UnFormat		(const CString&) const;
	virtual void	RecalcWidths	();
	virtual AlignType GetDefaultAlign	() const;
	virtual	void	Assign			(const Formatter& Fmt);
	virtual	int		Compare			(const Formatter& Fmt) const;

	virtual void		SetToLocale			();
	virtual const CSize	GetInputWidth		(CDC* pDC, int nCols = -1, CFont* = NULL);

	virtual CString	GetDefaultInputString(DataObj* pDataObj = NULL);
	virtual void SerializeJson(CJsonSerializer& strJson) const;
public:
 	const CString&	Get1000Separator	() const	{ return m_str1000Separator;}


protected:
};

//============================================================================
class TB_EXPORT CIntFormatter : public CLongFormatter
{
	friend class CIntFormatDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CIntFormatter)

public:
	CIntFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual BOOL	IsSameAsDefault	() const;

	virtual CString	GetDefaultInputString(DataObj* pDataObj = NULL);

	//	const void* = const int*
	virtual void 		Format			(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;
	virtual void		RecalcWidths	();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
};

//============================================================================
class TB_EXPORT CStringFormatter : public Formatter
{
	friend class CStringFormatDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CStringFormatter)

public:
	enum FormatTag 
	{
		ASIS =			0x0000,
		UPPERCASE =		0x0001,
		LOWERCASE =		0x0002,
		CAPITALIZED =	0x0003,
		EXPANDED =		0x0004,
		MASKED =		0x0005,
	};

private:
	FormatTag	m_FormatType;
	CFormatMask m_FormatMask;

protected:
	CString		m_strInterChars;

public:
	CStringFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	FormatTag		GetFormat		()		{ return m_FormatType;}	
	void			SetFormat				(FormatTag format){m_FormatType = format;}
	
	CFormatMask&	GetFormatMask	() { return m_FormatMask;}	
	void			SetMask			(const CString& sMask);

	virtual BOOL	IsSameAsDefault	() const;

	//	const void* = const char*
	virtual void 	Format			(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
	virtual AlignType	GetDefaultAlign		() const;
	virtual	void		Assign				(const Formatter& Fmt);
	virtual	int			Compare				(const Formatter& Fmt)	const;
	virtual const CSize	GetInputWidth		(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual BOOL		CanAttachToData	() const { return TRUE; }
	virtual BOOL		ZeroPaddedHasUI	() const;
};

//============================================================================
class TB_EXPORT CTextFormatter : public CStringFormatter
{
	friend class CStringFormatDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CTextFormatter)

public:
	CTextFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual BOOL	IsSameAsDefault	() const;
	virtual void	Assign(const Formatter& Fmt);
};

//============================================================================
class TB_EXPORT CDblFormatter : public Formatter
{
	friend class CFormatDlgRoot;
	friend class CDblFormatDlg;
	friend class CPrompterDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CDblFormatter)

public:
	enum FormatTag 
	{
		FIXED =				0x0000,
		EXPONENTIAL =		0x0001,
		ENGINEER =			0x0002,
		ENCODED =			0x0003,
		LETTER =			0x0004,
		ZERO_AS_DASH =		0x0099
	};
 
	enum RoundingTag
	{

		ROUND_NONE	= 0x0000, 
		ROUND_ABS	= 0x0001, 
		ROUND_SIGNED= 0x0002, 
		ROUND_ZERO	= 0x0003, 
		ROUND_INF	= 0x0004
	};
		
	enum SignTag
	{
		ABSOLUTEVAL	= 0x0000, 
		MINUSPREFIX	= 0x0001, 
		MINUSPOSTFIX= 0x0002, 
		ROUNDS		= 0x0003, 
		SIGNPREFIX	= 0x0004, 
		SIGNPOSTFIX	= 0x0005
	};

private:
	FormatTag	m_FormatType;

protected:
	RoundingTag	m_Rounding;
	SignTag		m_Sign;
	double		m_nQuantum;
	BOOL		m_bIs1000SeparatorDefault;
	CString		m_str1000Separator;
	BOOL		m_bIsDecSeparatorDefault;
	CString		m_strDecSeparator;
	BOOL		m_bShowMSZero;
	BOOL		m_bShowLSZero;
	CString		m_strXTable;
	int			m_nDecNumber;
	CString		m_strAsZeroValue;

public:
	CDblFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

private:
	CString ValueToAppend(double aValue, FormatTag format )const;

public:
	FormatTag		GetFormat		()	{return m_FormatType;}
	void			SetFormat		(FormatTag format){m_FormatType = format;}
	void			SetDecSeparator	(CString decSeparator){m_strDecSeparator = decSeparator;}
	virtual BOOL	IsSameAsDefault	() const;

	virtual CString GetDefaultInputString(DataObj* pDataObj = NULL);

	//	const void* = const double*
	virtual void 	Format			(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual CString	UnFormat		(const CString&) const;
	virtual void	RecalcWidths	();
	virtual AlignType GetDefaultAlign	() const;
	virtual	void	Assign			(const Formatter& Fmt);
	virtual	int		Compare			(const Formatter& Fmt)	const;

	virtual void		SetToLocale		();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);

public:
	int				SetDecNumber		(int nDN)	{
														int nOldDN = m_nDecNumber;
														m_nDecNumber = nDN;
														return nOldDN;
													}
	int				GetDecNumber		() const	{ return m_nDecNumber;		}
	const CString&	Get1000Separator	() const	{ return m_str1000Separator;}
 	const CString&	GetDecSeparator		() const	{ return m_strDecSeparator;	}
 	const CString&	GetAsZeroValue		() const	{ return m_strAsZeroValue;	}
	BOOL			IsShowLSZero		() const	{ return m_bShowLSZero;	}
	void			ShowLSZero			(BOOL bShow){ m_bShowLSZero = bShow;}
	BOOL			IsShowMSZero		() const	{ return m_bShowMSZero;	}
	void			ShowMSZero			(BOOL bShow){ m_bShowMSZero = bShow;}

	double			GetRoundedValue		(double dValue, double dQuantum) const;
	double			GetRoundedValue		(double) const;
	RoundingTag		SetRounding			(RoundingTag nRound){ 
														RoundingTag nOldR = m_Rounding;
														m_Rounding = nRound;
														return nOldR;
													}
	RoundingTag		GetRounding				() const	{ return m_Rounding;	}
	double			GetQuantumFromDecNumber	(const int& nDec) const;
	virtual void SerializeJson(CJsonSerializer& strJson) const;
};

//============================================================================
class TB_EXPORT CMonFormatter : public CDblFormatter
{
	friend class CFormatDlgRoot;
	friend class CMonFormatDlg;
	friend class CPrompterDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CMonFormatter)

public:
	CMonFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual BOOL	IsSameAsDefault	() const;
};

//============================================================================
class TB_EXPORT CQtaFormatter : public CDblFormatter
{
	friend class CFormatDlgRoot;
	friend class CQtaFormatDlg;
	friend class CPrompterDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CQtaFormatter)

public:
	CQtaFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual BOOL		IsSameAsDefault	() const;
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
	virtual void		RecalcWidths	();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
};

//============================================================================
class TB_EXPORT CPercFormatter : public CDblFormatter
{
	friend class CFormatDlgRoot;
	friend class CPercFormatDlg;
	friend class CPrompterDlg;
	friend class FormatsParser;
	
	DECLARE_DYNCREATE (CPercFormatter)

public:
	CPercFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual BOOL	IsSameAsDefault	() const;

	virtual void		RecalcWidths	();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
};

//============================================================================
class TB_EXPORT CDateFormatter : public Formatter
{
	friend class CDateTimeFormatDlg;
	friend class FormatsParser;
	
	DECLARE_DYNCREATE (CDateFormatter)

public:

private:
	BOOL							m_bIsFormatTypeDefault;
	BOOL							m_bIsFormatYearDefault;
	CDateFormatHelper::FormatTag	m_FormatType;

protected:	
	BOOL	m_bIsTimeFormatDefault;
	CString	m_strFirstSeparator;
	CString	m_strSecondSeparator;

	int		m_nInputDateLen;
	CString	m_strTimeSeparator;
	CString	m_strTimeAM;
	CString	m_strTimePM;

	CDateFormatHelper::TimeFormatTag		m_TimeFormat;
	CDateFormatHelper::WeekdayFormatTag		m_WeekdayFormat;
	CDateFormatHelper::DayFormatTag			m_DayFormat;
	CDateFormatHelper::MonthFormatTag		m_MonthFormat;
	CDateFormatHelper::YearFormatTag		m_YearFormat;

private:
	void AppendFormatString (
								CDateFormatHelper::DayFormatTag format, 
								CString& strWork, 
								BOOL bTypeShort
							) const;
	void AppendFormatString (
								CDateFormatHelper::MonthFormatTag format, 
								CString& strWork, 
								BOOL bTypeShort, 
								const DataDate &aDate
							) const;
	void AppendFormatString (
								CDateFormatHelper::YearFormatTag format, 
								CString &strWork,
								const int& nCurrentYear
							) const;

public:
	CDateFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());
    
public:
	virtual BOOL	IsSameAsDefault	() const;

	//	const void* = const DBTIMESTAMP*
	virtual void 	Format			(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual void		RecalcWidths	();
	virtual	int			GetInputDateLen	()	const	{ return m_nInputDateLen; }
	virtual AlignType	GetDefaultAlign	()	const;
	virtual	void		Assign			(const Formatter& Fmt);
	virtual	int			Compare			(const Formatter& Fmt)	const;
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
	virtual void	SetToLocale			();
	virtual void SerializeJson(CJsonSerializer& strJson) const;

public:
	CDateFormatHelper::FormatTag GetFormat		()	{return m_FormatType;}
    void	SetFormat		(CDateFormatHelper::FormatTag format){m_FormatType = format;}

	CDateFormatHelper::WeekdayFormatTag	GetWeekdayFormat	() 	const 	{ return m_WeekdayFormat;}
	CDateFormatHelper::DayFormatTag		GetDayFormat		()	const	{ return m_DayFormat;}
	CDateFormatHelper::MonthFormatTag	GetMonthFormat		()	const	{ return m_MonthFormat;}
	CDateFormatHelper::YearFormatTag	GetYearFormat		()	const	{ return m_YearFormat;}
	CDateFormatHelper::TimeFormatTag	GetTimeFormat		()	const	{ return m_TimeFormat; }
 	
	const CString&	GetFirstSeparator	()	const	{ return m_strFirstSeparator;}
 	const CString&	GetSecondSeparator	()	const	{ return m_strSecondSeparator;}

	BOOL			IsFullDateTimeFormat()	const	{ return m_OwnType.IsFullDate() && m_TimeFormat != CDateFormatHelper::TIME_NONE; }
 	const CString&	GetTimeSeparator	()	const	{ return m_strTimeSeparator;}
	const CString&	GetTimeAMString		()	const	{ return m_strTimeAM; }
	const CString&	GetTimePMString		()	const	{ return m_strTimePM; }
	BOOL			IsTimeAMPMFormat	()	const	{ return m_TimeFormat == CDateFormatHelper::TIME_AMPM; }
};

//============================================================================
class TB_EXPORT CDateTimeFormatter : public CDateFormatter
{
	DECLARE_DYNCREATE (CDateTimeFormatter)

public:
	CDateTimeFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

protected:
	virtual BOOL	IsSameAsDefault	() const;

	virtual void	SetToLocale			();

	virtual void		RecalcWidths	();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
};

//============================================================================
class TB_EXPORT CTimeFormatter : public CDateFormatter
{
	DECLARE_DYNCREATE (CTimeFormatter)

public:
	CTimeFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

protected:
	virtual BOOL	IsSameAsDefault	() const;

	virtual void		RecalcWidths	();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
};

//============================================================================
class TB_EXPORT CElapsedTimeFormatter : public Formatter	//@@ElapsedTime
{
	friend class CElapsedTimeFormatDlg;
	friend class FormatsParser;
	
	DECLARE_DYNCREATE (CElapsedTimeFormatter)

private:
	CElapsedTimeFormatHelper::FormatTag	m_FormatType;

protected:
	CString		m_strTimeSeparator;
	BOOL		m_bIsDecSeparatorDefault;
	CString		m_strDecSeparator;
	int			m_nDecNumber;
	int			m_nCaptionPos;

public:
	CElapsedTimeFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	CElapsedTimeFormatHelper::FormatTag	GetFormat	()	{return m_FormatType;}
	void	SetFormat	(CElapsedTimeFormatHelper::FormatTag format){m_FormatType = format;}
	
	virtual BOOL	IsSameAsDefault	() const;

	//	const void* = const long*
	virtual void 	Format			(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual void	RecalcWidths	();
    virtual AlignType GetDefaultAlign	() const;
	virtual	void	Assign			(const Formatter& Fmt);
	virtual	int		Compare			(const Formatter& Fmt) const;

	virtual void		SetToLocale		();
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
	virtual void SerializeJson(CJsonSerializer& strJson) const;

public:
	CString			GetShortDescription	  ()	const;
	CString			GetNumberFormatString ()	const;
 	const CString&	GetTimeSeparator	()	const	{ return m_strTimeSeparator;}
 	const CString&	GetDecSeparator		()	const	{ return m_strDecSeparator;}
	int				SetDecNumber		(int nDN)	{
														int nOldDN = m_nDecNumber;
														m_nDecNumber = nDN;
														return nOldDN;
													}
	int				GetDecNumber		() const	{ return m_nDecNumber;	}
	int				GetCaptionPos		() const	{ return m_nCaptionPos;	}
};

//============================================================================
class TB_EXPORT CBoolFormatter : public Formatter
{
	friend class CBoolFormatDlg;
	friend class FormatsParser;
	
	DECLARE_DYNCREATE (CBoolFormatter)

public:
	enum FormatTag { AS_ZERO = 0, AS_CHAR = 1};

private:
	FormatTag m_FormatType;

	CString	m_strFalseTag;
	CString	m_strTrueTag;

public:

	CBoolFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	CString			GetTrueTag		() const	{ return m_strTrueTag == szDefaultTrueTag ? DataObj::Strings::YES() : m_strTrueTag; } 
	CString			GetFalseTag		() const	{ return m_strFalseTag == szDefaultFalseTag ? DataObj::Strings::NO() : m_strFalseTag; } 

	FormatTag		GetFormat		()	{return m_FormatType;}
	void			SetFormat		(FormatTag format){m_FormatType = format;}
	virtual BOOL	IsSameAsDefault	() const;

	//	const void* = const BOOL*
	virtual void 	Format		(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual void		RecalcWidths();
	virtual	void		Assign			(const Formatter& Fmt);
	virtual	int			Compare			(const Formatter& Fmt)	const;
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
};

//============================================================================
class TB_EXPORT CEnumFormatter : public CStringFormatter
{
	friend class CEnumFormatDlg;
	friend class FormatsParser;
	
	DECLARE_DYNCREATE (CEnumFormatter)

public:
	enum FormatTag
	{
		ASIS =			0x0000,
		CAPITALIZED =	0x0001,
		FIRSTLETTER =	0x0002,
		UPPERCASE =		0x0003,
		LOWERCASE =		0x0004
	};

private:
	FormatTag	m_FormatType;

public:
	CEnumFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	FormatTag		GetFormat		()	{return m_FormatType;}
	void			SetFormat		(FormatTag format);
	virtual BOOL	IsSameAsDefault	() const;

	//	const void* = const DWORD*
	virtual void 	Format				(const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;

	virtual	void	Assign				(const Formatter& Fmt);
	virtual	int		Compare				(const Formatter& Fmt)	const;
	virtual const CSize	GetInputWidth	(CDC* pDC, int nCols = -1, CFont* = NULL);
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
};

//============================================================================
class TB_EXPORT CGuidFormatter : public CStringFormatter
{
	friend class CStringFormatDlg;
	friend class FormatsParser;

	DECLARE_DYNCREATE (CGuidFormatter)

public:
	CGuidFormatter	(const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual void  Format (const void* pIn, CString& strOut, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;
};

//============================================================================
class TB_EXPORT CTickTimeFormatter : public CElapsedTimeFormatter
{
public:
	CTickTimeFormatter();

public:
	CString FormatTime(DWORD);
};

//////////////////////////////////////////////////////////////////////////////
//						 CMonthNameFormatter
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CMonthNameFormatter : public CIntFormatter
{
	DECLARE_DYNCREATE (CMonthNameFormatter)

public:
	CMonthNameFormatter (const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual void Format	(const void* Address, CString& Str, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;
	virtual void	RecalcWidths	();
		
};

//////////////////////////////////////////////////////////////////////////////
//						 CWeekDayNameFormatter
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CWeekDayNameFormatter : public CIntFormatter
{
	DECLARE_DYNCREATE (CWeekDayNameFormatter)

public:
	CWeekDayNameFormatter (const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual void Format	(const void* Address, CString& Str, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;
	virtual void	RecalcWidths	();
};


//////////////////////////////////////////////////////////////////////////////
//						 CPrivacyFormatter
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CPrivacyFormatter : public Formatter
{
	DECLARE_DYNCREATE (CPrivacyFormatter)

public:
	CPrivacyFormatter (const CString& name = _T(""), const FormatStyleSource aSource = FROM_STANDARD, const CTBNamespace& aOwner = CTBNamespace());

public:
	virtual void Format	(const void* data, CString& Str, BOOL bPaddingEnabled = TRUE, BOOL bCollateCultureSensitive = FALSE) const;
	virtual CString		GetDefaultInputString(DataObj* pDataObj = NULL);
};



#include "endh.dex"
