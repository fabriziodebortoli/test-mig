#pragma once

namespace Microarea { namespace Framework { namespace TBApplicationWrapper
{
	/// <summary>
	/// Describes information about an EasyBuilder typed formatter.
	/// </summary>
	//=============================================================================
	public ref class TypedFormatterInfo : Microarea::TaskBuilderNet::Core::EasyBuilder::ExpandiblePropertyItem
	{	
	protected:
		Formatter*	m_pFormatter;
	
	public:
		/// <summary>
		/// Returns the name of the style of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ StyleName { System::String^ get (); }

		/// <summary>
		/// Returns the Title of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Title { System::String^ get (); }

		/// <summary>
		/// Returns the Head of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Head { System::String^ get (); }

		/// <summary>
		/// Returns the Tail of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Tail { System::String^ get (); }

		/// <summary>
		/// Returns the PaddedLength of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int PaddedLength { int get (); }

		/// <summary>
		/// Returns the Alignment of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Alignment { System::String^ get (); }

		/// <summary>
		/// Returns the type of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IDataType^	CompatibleType { IDataType^ get (); }

		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ Format	{ virtual System::String^ get (); }

		/// <summary>
		/// Returns true if the formatter is zero padded
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool IsZeroPadded { bool get (); }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of NumericFormatterInfo
		/// </summary>
		TypedFormatterInfo (Formatter* pFormatter);

		/// <summary>
		/// Converts into the formatted System::String 
		/// </summary>
		virtual System::String^ ToString() override;

	internal:
		Formatter* GetFormatter();
	};

	//=============================================================================
	public ref class NumericFormatterInfo : TypedFormatterInfo
	{	
	public:
		/// <summary>
		/// Returns the thousand separator (in case of long or double) for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ ThousandSeparator { System::String^ get (); }

		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format	{ virtual System::String^ get () override; }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of NumericFormatterInfo
		/// </summary>
		NumericFormatterInfo (Formatter* pFormatter);
	};

	/// <summary>
	/// Describes information about an EasyBuilder float formatter.
	/// </summary>
	//=============================================================================
	public ref class FloatFormatterInfo : TypedFormatterInfo
	{	
	public:
		/// <summary>
		/// Returns the decimal separator System::String for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ DecimalSeparator { System::String^ get (); }

		/// <summary>
		/// Returns the thousand separator (in case of long or double) for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ ThousandSeparator { System::String^ get (); }

		/// <summary>
		/// Gets the number of decimals for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int NumberOfDecimals { int get (); }

		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format	{ virtual System::String^ get () override; }
	
	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of FloatFormatterInfo
		/// </summary>
		FloatFormatterInfo (Formatter* pFormatter);
	};

	/// <summary>
	/// Describes information about an EasyBuilder date formatter.
	/// </summary>
	//=============================================================================
	public ref class DateFormatterInfo : TypedFormatterInfo
	{	
	public:
		/// <summary>
		/// Returns the System::String formatted for the entire date
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ DateFormat { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for the entire date
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ DateFormatForEdit { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for the day
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ DayFormat { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for the month
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ MonthFormat { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for the year
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ YearFormat { System::String^ get (); }
		
		/// <summary>
		/// Returns the System::String for the first separator of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ FirstSeparator	{ System::String^ get (); }

		/// <summary>
		/// Returns the System::String for the second separator of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SecondSeparator { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for AM
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ AMString { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for PM
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ PMString { System::String^ get (); }

		/// <summary>
		/// Returns the System::String formatted for the time only
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^	TimeFormat		{ System::String^ get (); }

		/// <summary>
		/// Returns the time separator for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ TimeSeparator { System::String^ get (); }

		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format	{ virtual System::String^ get () override; }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of DateFormatterInfo
		/// </summary>
		DateFormatterInfo (Formatter* pFormatter);
	};

	/// <summary>
	/// Describes information about an EasyBuilder logical formatter.
	/// </summary>
	//=============================================================================
	public ref class LogicalFormatterInfo : TypedFormatterInfo
	{	
	public:
		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format	{ virtual System::String^ get () override; }

		/// <summary>
		/// Returns the true System::String
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ TrueString	{ System::String^ get (); }

		/// <summary>
		/// Returns the false System::String
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ FalseString { System::String^ get (); }

		/// <summary>
		/// Returns the boolean as zero
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool AsZero { bool get (); }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of LogicalFormatterInfo
		/// </summary>
		LogicalFormatterInfo (Formatter* pFormatter);
	};

	/// <summary>
	/// Describes information about an EasyBuilder text formatter.
	/// </summary>
	//=============================================================================
	public ref class TextFormatterInfo : TypedFormatterInfo
	{	
	public:
		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format	{ virtual System::String^ get () override; }

		/// <summary>
		/// Returns the capitalization for the System::String
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Capitalization	{ System::String^ get (); }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of TextFormatterInfo
		/// </summary>
		TextFormatterInfo (Formatter* pFormatter);
	};

	/// <summary>
	/// Describes information about an EasyBuilder elapsed time formatter.
	/// </summary>
	//=============================================================================
	public ref class ElapsedTimeFormatterInfo : TypedFormatterInfo
	{	
	public:
		/// <summary>
		/// Returns the decimal separator for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ DecimalSeparator { System::String^ get (); }

		/// <summary>
		/// Returns the number of decimals for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int NumberOfDecimals { int get (); }
	
		/// <summary>
		/// Returns the time separator for the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ TimeSeparator { System::String^ get (); }

		/// <summary>
		/// Returns the caption position for the System::String of the formatter
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int CaptionPosition { int get (); }

		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format	{ virtual System::String^ get () override; }

	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of ElapsedTimeFormatterInfo
		/// </summary>
		ElapsedTimeFormatterInfo (Formatter* pFormatter);
	};

	/// <summary>
	/// Describes information about the style to use for an EasyBuilder formatter.
	/// </summary>
	[System::ComponentModel::DefaultPropertyAttribute("FormatterName")]
	[System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
	//=============================================================================
	public ref class FormatterStyle : Microarea::TaskBuilderNet::Core::EasyBuilder::ExpandiblePropertyItem
	{	
	private:
		TypedFormatterInfo^	typedInfo;

	public:
		/// <summary>
		/// Initializes a new instance of FormatterStyle
		/// </summary>
		FormatterStyle ();
		/// <summary>
		/// Initializes a new instance of FormatterStyle
		/// </summary>
		FormatterStyle (Formatter* pFormatter);

        /// <summary>
		/// Returns the name of the Style
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ StyleName { System::String^ get (); void set (System::String^ value); }

		/// <summary>
		/// Returns the TypedFormatterInfo type for the formatter
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DisplayName("Typed Info"), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property TypedFormatterInfo^ TypedInfo { TypedFormatterInfo^ get (); }

		/// <summary>
		/// Returns the title of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Title { System::String^ get (); }

		/// <summary>
		/// Returns the Head of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Head { System::String^ get (); }

		/// <summary>
		/// Returns the Tail of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Tail { System::String^ get (); }

		/// <summary>
		/// Returns the PaddedLength of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int PaddedLength { int get (); }

		/// <summary>
		/// Returns the formatted System::String 
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Format { System::String^ get (); }

		/// <summary>
		/// Converts into the formatted System::String 
		/// </summary>
		virtual System::String^ ToString() override;
		
		/// <summary>
		/// Override for Equals, compares two formatters StyleNames
		/// </summary>
		virtual bool Equals(Object^ object) override;

        /// <summary>
		/// Returns the type of the formatter
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IDataType^	CompatibleType	{ IDataType^ get (); }

	internal:
		void SetFormatter (Formatter* pFormatter);
		Formatter* GetFormatter () {return typedInfo->GetFormatter(); };
	};
}
}
}


