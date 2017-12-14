#include "StdAfx.h"

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBResourcesMap.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbParser\SymTable.h>

#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\ParsEdt.h>
#include <TbGenlib\ParsCbx.h>
#include <TbGenlib\ParsBtn.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\TABCORE.H>
#include <TbGenlib\hlinkobj.h>
#include <TbGenlib\HyperLink.h>
#include <TbGenlib\BaseApp.h>

#include <TbOledb\SqlRec.h>

#include <TbGes\dbt.h>
#include <TbGes\Tabber.h>
#include <TbGes\ExtDocView.h>

#include "MParsedControls.h"
#include "MFormatters.h"

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Windows::Forms;

/////////////////////////////////////////////////////////////////////////////
// 				class TypedFormatter Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
TypedFormatterInfo::TypedFormatterInfo (Formatter* pFormatter)
	:
	m_pFormatter(pFormatter)
{
}

//----------------------------------------------------------------------------
Formatter* TypedFormatterInfo::GetFormatter()
{
	return m_pFormatter;
}

//----------------------------------------------------------------------------
System::String^	TypedFormatterInfo::StyleName::get ()
{
	return gcnew System::String(m_pFormatter ? m_pFormatter->GetName() : _T(""));
}

//----------------------------------------------------------------------------
System::String^	TypedFormatterInfo::Title::get ()
{
	return gcnew System::String(m_pFormatter ? m_pFormatter->GetTitle() : _T(""));
}

//----------------------------------------------------------------------------
System::String^	TypedFormatterInfo::Head::get ()
{
	return gcnew System::String(m_pFormatter ? m_pFormatter->GetHead() : _T(""));
}

//----------------------------------------------------------------------------
System::String^	TypedFormatterInfo::Tail::get ()
{
	return gcnew System::String(m_pFormatter ? m_pFormatter->GetTail() : _T(""));
}

//----------------------------------------------------------------------------
int	TypedFormatterInfo::PaddedLength::get ()
{
	return m_pFormatter ? m_pFormatter->GetPaddedLen() : 0;
}

//----------------------------------------------------------------------------
IDataType^ TypedFormatterInfo::CompatibleType::get ()
{
	return m_pFormatter ? 
		gcnew Microarea::TaskBuilderNet::Core::CoreTypes::DataType 
		(
			m_pFormatter->GetDataType().m_wType, 
			m_pFormatter->GetDataType().m_wTag
		) 
		: nullptr;
}

//----------------------------------------------------------------------------
System::String^	TypedFormatterInfo::Alignment::get ()
{
	if (!m_pFormatter)
		return System::String::Empty;
	
	CString sAlign;
	switch (m_pFormatter->GetAlign())
	{
	case Formatter::LEFT:
		sAlign =_TB("Left");
		break;
	case Formatter::RIGHT:
		sAlign =_TB("Right");
		break;
	default:
		sAlign =_TB("None");
	}
	
	return gcnew System::String (sAlign);
}

//----------------------------------------------------------------------------
System::String^	TypedFormatterInfo::Format::get ()
{
	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^ TypedFormatterInfo::ToString()
{
	return CompatibleType->ToString();
}

//----------------------------------------------------------------------------
bool TypedFormatterInfo::IsZeroPadded::get ()
{
	return m_pFormatter ? m_pFormatter->IsZeroPadded() == TRUE : false;
}


/////////////////////////////////////////////////////////////////////////////
// 				class FormatterStyle Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
FormatterStyle::FormatterStyle ()
	:
	typedInfo(nullptr)
{
}

//----------------------------------------------------------------------------
FormatterStyle::FormatterStyle (Formatter* pFormatter)
	:
	typedInfo(nullptr)
{
	SetFormatter(pFormatter);
}

//----------------------------------------------------------------------------
void FormatterStyle::SetFormatter (Formatter* pFormatter)
{
	if (typedInfo != nullptr)
		delete typedInfo;

	if (!pFormatter)
		return;
	
	TypedFormatterInfo^ style = nullptr;
	if (pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)))
		typedInfo = gcnew NumericFormatterInfo(pFormatter);
	else if (pFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)))
		typedInfo = gcnew FloatFormatterInfo(pFormatter);
	else if (pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
		typedInfo = gcnew DateFormatterInfo(pFormatter);
	else if (pFormatter->IsKindOf(RUNTIME_CLASS(CBoolFormatter)))
		typedInfo = gcnew LogicalFormatterInfo(pFormatter);
	else if (pFormatter->IsKindOf(RUNTIME_CLASS(CElapsedTimeFormatter)))
		typedInfo = gcnew ElapsedTimeFormatterInfo(pFormatter);
	else if (pFormatter->IsKindOf(RUNTIME_CLASS(CStringFormatter)))
		typedInfo = gcnew TextFormatterInfo(pFormatter);
	else
		typedInfo = gcnew TypedFormatterInfo(pFormatter);
}

//----------------------------------------------------------------------------
System::String^	FormatterStyle::StyleName::get ()
{
	return typedInfo == nullptr ?  System::String::Empty : typedInfo->StyleName;
}

//----------------------------------------------------------------------------
TypedFormatterInfo^	FormatterStyle::TypedInfo::get ()
{
	return typedInfo;
}

//----------------------------------------------------------------------------
System::String^	FormatterStyle::Title::get ()
{
	return typedInfo == nullptr ?  System::String::Empty : typedInfo->Title;
}

//----------------------------------------------------------------------------
System::String^	FormatterStyle::Head::get ()
{
	return typedInfo == nullptr ?  System::String::Empty : typedInfo->Head;
}

//----------------------------------------------------------------------------
System::String^	FormatterStyle::Tail::get ()
{
	return typedInfo == nullptr ?  System::String::Empty : typedInfo->Tail;
}

//----------------------------------------------------------------------------
int	FormatterStyle::PaddedLength::get ()
{
	return typedInfo == nullptr ? 0 : typedInfo->PaddedLength;
}

//----------------------------------------------------------------------------
void FormatterStyle::StyleName::set (System::String^ name)
{
	Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter (CString(name), NULL);
	if (pFormatter)
		SetFormatter(pFormatter);
}

//----------------------------------------------------------------------------
System::String^	FormatterStyle::Format::get ()
{
	return typedInfo == nullptr ? System::String::Empty : typedInfo->Format;
}

//----------------------------------------------------------------------------
System::String^ FormatterStyle::ToString()
{
	return StyleName;
}

//----------------------------------------------------------------------------
bool FormatterStyle::Equals(Object^ object)
{
	if (object == nullptr)
		return false;

	if (object->GetType() != FormatterStyle::typeid)
		return false;

	return this->StyleName->Equals(((FormatterStyle^) object)->StyleName);
}

//----------------------------------------------------------------------------
IDataType^ FormatterStyle::CompatibleType::get ()
{
	return typedInfo == nullptr ? nullptr : typedInfo->CompatibleType;
}

/////////////////////////////////////////////////////////////////////////////
// 				class NumericFormatterInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
NumericFormatterInfo::NumericFormatterInfo (Formatter* pFormatter)
	:
	TypedFormatterInfo(pFormatter)
{
}

//----------------------------------------------------------------------------
System::String^	NumericFormatterInfo::ThousandSeparator::get ()
{
	if (!m_pFormatter)
		return nullptr;

	if (m_pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)))
		return gcnew System::String(((CLongFormatter*) m_pFormatter)->Get1000Separator());
	else if (m_pFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)))
		return gcnew System::String(((CDblFormatter*) m_pFormatter)->Get1000Separator());
	
	return nullptr;
}

//----------------------------------------------------------------------------
System::String^	NumericFormatterInfo::Format::get ()
{
	if (!m_pFormatter || !m_pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)))
		return System::String::Empty;
	
	CLongFormatter* pLongFmt = (CLongFormatter*) m_pFormatter;
	CString sFormat;
	::DataObj* pDataObj = NULL;
	if (m_pFormatter->IsKindOf(RUNTIME_CLASS(CIntFormatter)))
		pDataObj = new ::DataInt(INT_MIN);
	else
		pDataObj = new ::DataLng(LONG_MIN);
		
	pLongFmt->FormatDataObj(*pDataObj, sFormat, FALSE);
	
	return gcnew System::String (sFormat);				
}

/////////////////////////////////////////////////////////////////////////////
// 				class FloatFormatterInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
FloatFormatterInfo::FloatFormatterInfo (Formatter* pFormatter)
	:
	TypedFormatterInfo(pFormatter)
{
}

//----------------------------------------------------------------------------
System::String^	FloatFormatterInfo::DecimalSeparator::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)))
		return gcnew System::String(((CDblFormatter*) m_pFormatter)->GetDecSeparator());
	
	return nullptr;
}

//----------------------------------------------------------------------------
System::String^	FloatFormatterInfo::ThousandSeparator::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)))
		return gcnew System::String(((CDblFormatter*) m_pFormatter)->Get1000Separator());
	
	return nullptr;
}

//----------------------------------------------------------------------------
int FloatFormatterInfo::NumberOfDecimals::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)) ?
				((CDblFormatter*) m_pFormatter)->GetDecNumber() :
				0;
}

//----------------------------------------------------------------------------
System::String^	FloatFormatterInfo::Format::get ()
{
	if (!m_pFormatter || !m_pFormatter->IsKindOf(RUNTIME_CLASS(CDblFormatter)))
		return System::String::Empty;
	
	CDblFormatter* pDblFmt = (CDblFormatter*) m_pFormatter;
	CString sFormat;
	::DataDbl aDouble(1.0);
	pDblFmt->FormatDataObj(aDouble, sFormat, FALSE);
	
	return gcnew System::String (sFormat);				
}

/////////////////////////////////////////////////////////////////////////////
// 				class DateFormatterInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
DateFormatterInfo::DateFormatterInfo (Formatter* pFormatter)
	:
	TypedFormatterInfo(pFormatter)
{
}


//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::DateFormat::get ()
{
	System::Text::StringBuilder^ format = gcnew System::Text::StringBuilder();

	format->Append(Head);

	CDateFormatHelper::WeekdayFormatTag tag = (((CDateFormatter*) m_pFormatter)->GetWeekdayFormat());
	if (tag == CDateFormatHelper::PREFIXWEEKDAY)
		format->Append("dddd ");

	format->Append(DateFormatForEdit);
	
	if (tag == CDateFormatHelper::POSTFIXWEEKDAY)
		format->Append(" dddd");

	format->Append(Tail);

	return format->ToString();
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::DateFormatForEdit::get ()
{
	if (!m_pFormatter)
		return System::String::Empty;
	
	CDateFormatter* pDateFmt = (CDateFormatter*) m_pFormatter;
	if ((pDateFmt->GetTimeFormat() & CDateFormatHelper::TIME_ONLY) == CDateFormatHelper::TIME_ONLY)
		return gcnew System::String(_TB("None"));
	System::Text::StringBuilder^ format = gcnew System::Text::StringBuilder();
	
	switch (pDateFmt->GetFormat())
	{
		case CDateFormatHelper::DATE_MDY:
			format->Append(MonthFormat);
			format->Append(FirstSeparator);
			format->Append(DayFormat);
			format->Append(SecondSeparator);
			format->Append(YearFormat);
			break;
		case CDateFormatHelper::DATE_YMD:
			format->Append(YearFormat);
			format->Append(FirstSeparator);
			format->Append(MonthFormat);
			format->Append(SecondSeparator);
			format->Append(DayFormat);
			break;
		default:
			format->Append(DayFormat);
			format->Append(FirstSeparator);
			format->Append(MonthFormat);
			format->Append(SecondSeparator);
			format->Append(YearFormat);
	}

	return format->ToString();
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::DayFormat::get ()
{
	if (!m_pFormatter)
		return System::String::Empty;
	
	CDateFormatter* pDateFmt = (CDateFormatter*) m_pFormatter;

	switch (pDateFmt->GetDayFormat())
	{
		case CDateFormatHelper::DAYB9:
			return gcnew System::String(" d");
		case CDateFormatHelper::DAY9:
			return gcnew System::String("d");
		default:
			return gcnew System::String("dd");
	}
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::MonthFormat::get ()
{
	if (!m_pFormatter)
		return System::String::Empty;
	
	CDateFormatter* pDateFmt = (CDateFormatter*) m_pFormatter;

	switch (pDateFmt->GetMonthFormat())
	{
		case CDateFormatHelper::MONTHB9:
			return gcnew System::String(" M");
		case CDateFormatHelper::MONTH9:
			return gcnew System::String("M");
		case CDateFormatHelper::MONTHS3:
			return gcnew System::String("MMM");
		case CDateFormatHelper::MONTHSX:
			return gcnew System::String("MMMMM");
		default:
			return gcnew System::String("MM");
	}
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::YearFormat::get ()
{
	if (!m_pFormatter)
		return System::String::Empty;
	
	CDateFormatter* pDateFmt = (CDateFormatter*) m_pFormatter;

	switch (pDateFmt->GetYearFormat())
	{
		case CDateFormatHelper::YEAR9999:
			return gcnew System::String("yyyy");
		case CDateFormatHelper::YEAR999:
			return gcnew System::String("yyy");
		default:
			return gcnew System::String("yy");
	}
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::FirstSeparator::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
		return gcnew System::String(((CDateFormatter*) m_pFormatter)->GetFirstSeparator());
	
	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::SecondSeparator::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
		return gcnew System::String(((CDateFormatter*) m_pFormatter)->GetSecondSeparator());
	
	return System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::AMString::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
		return gcnew System::String(((CDateFormatter*) m_pFormatter)->GetTimeAMString());
	
	return nullptr;
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::PMString::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
		return gcnew System::String(((CDateFormatter*) m_pFormatter)->GetTimePMString());
	
	return nullptr;
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::TimeFormat::get ()
{
	CDateFormatter* pDateFmt = (CDateFormatter*) m_pFormatter;

	switch (pDateFmt->GetTimeFormat())
	{
		case CDateFormatHelper::HHMMTT:
			return gcnew System::String("hh:mm tt");
		case CDateFormatHelper::BHMMTT:
			return gcnew System::String(" h:mm tt");
		case CDateFormatHelper::HMMTT:
			return gcnew System::String("h:mm tt");
		case CDateFormatHelper::HHMMSSTT:
			return gcnew System::String("hh:mm:ss tt");
		case CDateFormatHelper::BHMMSSTT:
			return gcnew System::String(" h:mm:ss tt");
		case CDateFormatHelper::HMMSSTT:
			return gcnew System::String("h:mm:ss tt");
		case CDateFormatHelper::HHMM:
			return gcnew System::String("hh:mm");
		case CDateFormatHelper::BHMM:
			return gcnew System::String(" h:mm");
		case CDateFormatHelper::HMM:
			return gcnew System::String("h:mm");
		case CDateFormatHelper::HHMMSS:	// case CDateFormatHelper::TIME_HF99:
			return gcnew System::String("hh:mm:ss");
		case CDateFormatHelper::BHMMSS:	// case CDateFormatHelper::TIME_HFB9:
			return gcnew System::String(" h:mm:ss");
		case CDateFormatHelper::HMMSS:	// case CDateFormatHelper::TIME_HF9:
			return gcnew System::String("h:mm:ss");
		case CDateFormatHelper::TIME_AMPM:
			return gcnew System::String("AMPM");
		default:
			return gcnew System::String(_TB("None"));
	}	

	return nullptr;
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::TimeSeparator::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
		return gcnew System::String(((CDateFormatter*) m_pFormatter)->GetTimeSeparator());
	
	return nullptr;
}

//----------------------------------------------------------------------------
System::String^	DateFormatterInfo::Format::get ()
{
	return DateFormat + " ; " + TimeFormat;
}

/////////////////////////////////////////////////////////////////////////////
// 				class LogicalFormatterInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
LogicalFormatterInfo::LogicalFormatterInfo (Formatter* pFormatter)
	:
	TypedFormatterInfo(pFormatter)
{
}

//----------------------------------------------------------------------------
System::String^	LogicalFormatterInfo::Format::get ()
{
	return System::String::Concat (TrueString, "/", FalseString);
}

//----------------------------------------------------------------------------
System::String^	LogicalFormatterInfo::FalseString::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CBoolFormatter)) ?
			gcnew System::String(((CBoolFormatter*) m_pFormatter)->GetFalseTag()) :
			System::String::Empty;
}

//----------------------------------------------------------------------------
System::String^	LogicalFormatterInfo::TrueString::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CBoolFormatter)) ?
			gcnew System::String(((CBoolFormatter*) m_pFormatter)->GetTrueTag()) :
			System::String::Empty;
}

//----------------------------------------------------------------------------
bool LogicalFormatterInfo::AsZero::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CBoolFormatter)) ?
			((CBoolFormatter*) m_pFormatter)->GetFormat() == CBoolFormatter::AS_ZERO :
			false;
}

/////////////////////////////////////////////////////////////////////////////
// 				class LogicalFormatterInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
TextFormatterInfo::TextFormatterInfo (Formatter* pFormatter)
	:
	TypedFormatterInfo(pFormatter)
{
}

//----------------------------------------------------------------------------
System::String^	TextFormatterInfo::Format::get ()
{
	return Capitalization;
}

//----------------------------------------------------------------------------
System::String^	TextFormatterInfo::Capitalization::get ()
{
	if (!m_pFormatter)
		return System::String::Empty;

	if (m_pFormatter->IsKindOf(RUNTIME_CLASS(CStringFormatter)))
	{
		switch (((CStringFormatter*) m_pFormatter)->GetFormat())
		{
		case CStringFormatter::UPPERCASE:
			return gcnew System::String(_TB("Uppercase"));
		case CStringFormatter::LOWERCASE:
			return gcnew System::String(_TB("Lowercase"));
		case CStringFormatter::CAPITALIZED:
			return gcnew System::String(_TB("Capitalized"));
		case CStringFormatter::EXPANDED:
			return gcnew System::String(_TB("Expanded"));
		case CStringFormatter::MASKED:
			return gcnew System::String(cwsprintf(_TB("Masked as {0-%s}"), ((CStringFormatter*) m_pFormatter)->GetFormatMask().GetMask()));
		default:
			return gcnew System::String(_TB("As is"));
		}
	}	
	else if (m_pFormatter->IsKindOf(RUNTIME_CLASS(CEnumFormatter)))
	{
		switch (((CEnumFormatter*) m_pFormatter)->GetFormat())
		{
		case CEnumFormatter::UPPERCASE:
			return gcnew System::String(_TB("Uppercase"));
		case CEnumFormatter::LOWERCASE:
			return gcnew System::String(_TB("Lowercase"));
		case CEnumFormatter::CAPITALIZED:
			return gcnew System::String(_TB("Capitalized"));
		case CEnumFormatter::FIRSTLETTER:
			return gcnew System::String(_TB("First Letter"));
		default:
			return gcnew System::String(_TB("As is"));
		}
	}	

	return System::String::Empty;
}

/////////////////////////////////////////////////////////////////////////////
// 				class ElapsedTimeFormatterInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
ElapsedTimeFormatterInfo::ElapsedTimeFormatterInfo (Formatter* pFormatter)
	:
	TypedFormatterInfo(pFormatter)
{
}

//----------------------------------------------------------------------------
System::String^	ElapsedTimeFormatterInfo::DecimalSeparator::get ()
{
	if (m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CElapsedTimeFormatter)))
		return gcnew System::String(((CElapsedTimeFormatter*) m_pFormatter)->GetDecSeparator());
	
	return nullptr;
}

//----------------------------------------------------------------------------
int ElapsedTimeFormatterInfo::NumberOfDecimals::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CElapsedTimeFormatter)) ?
				((CElapsedTimeFormatter*) m_pFormatter)->GetDecNumber() :
				0;
}

//----------------------------------------------------------------------------
System::String^ ElapsedTimeFormatterInfo::TimeSeparator::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CElapsedTimeFormatter)) ?
				gcnew System::String(((CElapsedTimeFormatter*) m_pFormatter)->GetTimeSeparator()) :
				System::String::Empty;
}

//----------------------------------------------------------------------------
int ElapsedTimeFormatterInfo::CaptionPosition::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CElapsedTimeFormatter)) ?
				((CElapsedTimeFormatter*) m_pFormatter)->GetCaptionPos() :
				0;
}

//----------------------------------------------------------------------------
System::String^	ElapsedTimeFormatterInfo::Format::get ()
{
	return m_pFormatter && m_pFormatter->IsKindOf(RUNTIME_CLASS(CElapsedTimeFormatter)) ?
		gcnew System::String(((CElapsedTimeFormatter*) m_pFormatter)->GetShortDescription()) :
		System::String::Empty;
}