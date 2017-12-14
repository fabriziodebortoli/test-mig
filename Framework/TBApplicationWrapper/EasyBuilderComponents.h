#pragma once

using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;

namespace Microarea { namespace Framework { namespace TBApplicationWrapper
{
//=============================================================================
public ref class EasyBuilderComponents : public System::Collections::Generic::List<EasyBuilderComponent^>
{
private:
	EasyBuilderComponent^			parent;
	bool							canBeExtendedByUI;
	bool							canBeReducedByUI;
	bool							canBeModifiedByUI;
	
public:
	EasyBuilderComponents (EasyBuilderComponent^ parent);
	
public:
	/// <summary>
	/// Gets parent of collection
	/// </summary>
	[System::ComponentModel::Browsable(false),System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
	property EasyBuilderComponent^	Parent	{ EasyBuilderComponent^ get (); }
		
	/// <summary>
	/// Gets original collection
	/// </summary>
	[System::ComponentModel::Browsable(false),System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
	property System::ComponentModel::ComponentCollection^ OriginalCollection { virtual System::ComponentModel::ComponentCollection^ get (); }
	
	/// <summary>
	/// Gets and sets if collection can be extended or not
	/// </summary>
	[System::ComponentModel::Browsable(false),System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
	property bool	CanBeExtendedByUI	{ bool get (); void set(bool value); }

	/// <summary>
	/// Gets and sets if collection can be reduced or not
	/// </summary>
	[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
	property bool	CanBeReducedByUI	{ bool get (); void set(bool value); }

	/// <summary>
	/// Gets and sets if collection can be modified or not
	/// </summary>
	[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
	property bool	CanBeModifiedByUI { bool get(); void set(bool value); }


public:
		
	/// <summary>
	/// Internal Use
	/// </summary>
	virtual EasyBuilderComponent^ CreateNewInstance ();
		
	/// <summary>
	/// Internal Use
	/// </summary>
	virtual void InitializeForUI ();

	/// <summary>
	/// Internal Use
	/// </summary>
	virtual void ApplyChanges ();
		
	/// <summary>
	/// Internal Use
	/// </summary>
	virtual bool HasChanged	();
		
	/// <summary>
	/// Internal Use
	/// </summary>
	virtual bool IsEditable ();
		
	/// <summary>
	/// Internal Use
	/// </summary>
	virtual EasyBuilderComponents^ Clone ();
	/// <summary>
	/// Internal Use
	/// </summary>
	virtual System::String^ GetEditableError ();
};
}
}
}