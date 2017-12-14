#pragma once

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	//=========================================================================
	/// <summary>
	/// TableNameAttribute
	/// </summary>
	[System::AttributeUsage(System::AttributeTargets::Class, Inherited=true, AllowMultiple=false)]
	public ref struct TableAttribute : System::Attribute
	{
	private:
		System::String^ tableName;

	public:
		property System::String^ TableName { System::String^ get(); void set (System::String^ value);	}

		TableAttribute(System::String^ tableName);

		static System::String^ GetTableName(System::Type^ recordType);
	};

	//=========================================================================
	/// <summary>
	/// TBBindableAttribute
	/// </summary>
	[System::AttributeUsage(System::AttributeTargets::Property, Inherited=true, AllowMultiple=false)]
	public ref struct TBBindableAttribute : System::Attribute
	{
	private:
		bool bindable;

	public:
		property bool Bindable { bool get(); void set (bool value);	}

		TBBindableAttribute(bool bindable);

	};

	
	//=========================================================================
	/// <summary>
	/// Describes the form type of a user control.
	/// </summary>
	public ref struct FormTypeAttribute : System::Attribute
	{
	private:
		System::Type^ formType;

	public:
		property System::Type^ FormType { System::Type^ get(); void set (System::Type^ value);	}

		FormTypeAttribute(System::Type^ formType);

	};

	//=========================================================================
	/// <summary>
	/// TableNameAttribute
	/// </summary>
	[System::AttributeUsage(System::AttributeTargets::Method | System::AttributeTargets::Property, Inherited=true, AllowMultiple=false)]
	public ref struct ADMGateExposed : System::Attribute
	{
	private:
		bool exposed;

	public:
		property bool Exposed { bool get(); void set (bool value);	}

		ADMGateExposed();
		ADMGateExposed(bool exposed);
	};
}}}