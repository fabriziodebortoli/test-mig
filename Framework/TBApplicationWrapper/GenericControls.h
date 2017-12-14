#pragma once

#include "MParsedControls.h"
#include "MPanel.h"

namespace Microarea {
	namespace Framework	{
		namespace TBApplicationWrapper
		{
			/// <summary>
			/// Iternal use: serializes a generic window wrapper 
			/// </summary>
			//================================================================================
			public ref class GenericWindowWrapperSerializer : EasyBuilderControlSerializer
			{
			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual Statement^	GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;
			};

			/// <summary>
			/// Root class for all the wrapper controls with no unique taskbuilder Namespace 
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			public  ref class GenericWindowWrapper abstract : BaseWindowWrapper
			{
			protected:
				NameSpace^	nameSpace;
				System::Drawing::Point		originalPosition;

			public:
				/// <summary>
				/// Constructor: handleWndPtr is the c++ handle of the CWnd which the generic control refers to
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericWindowWrapper(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericWindowWrapper(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				~GenericWindowWrapper();

				/// <summary>
				/// Finalizer
				/// </summary>
				!GenericWindowWrapper();

			private:
				void InitializeName(IWindowWrapperContainer^ parent);

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanCreate() override;
				
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedObject) override;

				static System::String^ GetStaticAreaIDFromName(System::String^);

			public:
				/// <summary>
				/// Gets or sets the name of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
				property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ name) override; }

				/// <summary>
				/// Sets the Location of the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Drawing::Point Location	{ virtual void set(System::Drawing::Point value) override; }

				/// <summary>
				/// Gets the ClassName of the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::String^ ClassName { virtual System::String^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property System::Drawing::Point OriginalPosition { virtual System::Drawing::Point get(); virtual void set(System::Drawing::Point value); }

				/// <summary>
				/// Sets the Parent of the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property IWindowWrapperContainer^ Parent { virtual void set(IWindowWrapperContainer^ value) override; }

				/// <summary>
				/// Gets or the namespace of the current control
				/// </summary>
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property INameSpace^	Namespace			{ virtual INameSpace^ get() override; }

			private:
				System::IntPtr GetChildFromScaledArea(WindowWrapperContainer^ parentWindow, System::Drawing::Point originalPosition);

			protected:
				virtual LPCTSTR GetWindowClass() = 0;
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;

			};

			/// <summary>
			/// Wrapper class for the MFC Edit
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			[EasyBuilderComponentExtender(MEditProperties::typeid, "EditProperties")]
			public ref class GenericEdit : GenericWindowWrapper
			{

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericEdit(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericEdit(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("EDIT"); }
			};

			/// <summary>
			/// Wrapper class for the MFC ComboBox
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			[EasyBuilderComponentExtender(MComboBoxProperties::typeid, "MComboBoxProperties")]
			public ref class GenericComboBox : GenericWindowWrapper
			{

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericComboBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericComboBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
				virtual LPCTSTR GetWindowClass() override { return _T("COMBOBOX"); }
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
			};


			/// <summary>
			/// Wrapper class for the MFC Label
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			[EasyBuilderComponentExtender(MLabelProperties::typeid, "MLabelProperties")]
			public ref class MLabel : GenericWindowWrapper
			{
			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MLabel(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MLabel(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;

				/// <summary>
				/// Gets or sets the Text for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true), 
					TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::BoolTypes), 
					System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				[Description("Sets the text for the current control")]
				property System::String^	Text		{
					virtual System::String^ get() override { return __super::Text; };
					virtual void set(System::String^ value) override;
				}

				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("STATIC"); }
			};

			/// <summary>
			/// Wrapper class for the MFC ListBox
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MListBoxProperties::typeid, "MListBoxProperties")]
			public ref class GenericListBox : GenericWindowWrapper
			{
				MESSAGE_HANDLER_EVENT(CheckedChanged, EasyBuilderEventArgs, "Occurs when check value is changed.");





			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericListBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericListBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);


			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("LISTBOX"); }
			};

			/// <summary>
			/// Wrapper class for the MFC Button
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			//	[EasyBuilderComponentExtender(MButtonProperties::typeid, "MButtonProperties")]
			public ref class GenericButton : GenericWindowWrapper
			{
				MESSAGE_HANDLER_EVENT(Click, EasyBuilderEventArgs, "Occurs when button is clicked.");

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericButton(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Gets or sets the property indicating if the button displays text
				/// </summary>
				[System::ComponentModel::Localizable(true), System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property System::String^	Text		{ virtual System::String^ get() override { return __super::Text; } }



			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("BUTTON"); }
			};

			/// <summary>
			/// Wrapper class for the MFC Button
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MPushButtonProperties::typeid, "MPushButtonProperties")]
			public ref class GenericPushButton : GenericButton
			{
				

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericPushButton(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericPushButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				
			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			};

			/// <summary>
			/// Wrapper class for the MFC CheckBox
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MCheckBoxProperties::typeid, "MCheckBoxProperties")]
			public ref class GenericCheckBox : GenericButton
			{
				MESSAGE_HANDLER_EVENT(SelectedIndexChanged, EasyBuilderEventArgs, "Occurs when selected index is changed.");

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericCheckBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericCheckBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Sets the Text for the current control
				/// </summary>
				[System::ComponentModel::Localizable(true), System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property System::String^	Text		{ virtual System::String^ get() override { return __super::Text; } }


			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
				virtual LPCTSTR GetWindowClass() override { return _T("BUTTON"); }
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
			};

			/// <summary>
			/// Wrapper class for the MFC RadioButton
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MRadioButtonProperties::typeid, "MRadioButtonProperties")]
			public ref class GenericRadioButton : GenericButton
			{
				MESSAGE_HANDLER_EVENT(CheckedChanged, EasyBuilderEventArgs, "Occurs when check value is changed.")

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericRadioButton(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericRadioButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Sets the Text for the current control
				/// </summary>
				[System::ComponentModel::Localizable(true), System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property System::String^	Text		{ virtual System::String^ get() override { return __super::Text; } }

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			//protected:
				virtual LPCTSTR GetWindowClass() override { return _T("BUTTON"); }
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
			};

			/// <summary>
			/// Wrapper class for the MFC Groupbox
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MGroupBoxProperties::typeid, "MGroupBoxProperties")]
			public ref class GenericGroupBox : GenericButton
			{
			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericGroupBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericGroupBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedObject) override;


				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;


				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;

				/// <summary>
				/// Specifies that the button displays text
				/// </summary>
				[System::ComponentModel::Localizable(true), TBPropertyFilter(TBPropertyFilters::ComponentState),
					System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property System::String^	Text { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("BUTTON"); }
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
			};

			/// <summary>
			/// Wrapper class for the MFC Button
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			public ref class GenericTreeView : GenericWindowWrapper
			{
				MESSAGE_HANDLER_EVENT(Click, EasyBuilderEventArgs, "Occurs when tree is clicked.");

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericTreeView(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericTreeView(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
			
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;


				/// <summary>
				/// Gets or Sets the text for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::String^ Text { 
					virtual System::String^ get() override { return gcnew System::String(""); } 
					virtual void set(System::String^ value) override {} 
				}


			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("SYSTREEVIEW32"); }
			};

			/// <summary>
			/// Wrapper class for the MFC Button
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			public ref class GenericListCtrl : GenericWindowWrapper
			{
				MESSAGE_HANDLER_EVENT(Click, EasyBuilderEventArgs, "Occurs when list control is clicked.");

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericListCtrl(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericListCtrl(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;

				/// <summary>
				/// Gets or sets the text alignment by the Enum EListCtrlAlign
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property EListCtrlAlign Alignment { EListCtrlAlign get(); void set(EListCtrlAlign value);  }

				/// <summary>
				/// Gets or sets the view mode by the Enum EListCtrlViewMode
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property EListCtrlViewMode View { EListCtrlViewMode get(); void set(EListCtrlViewMode value);  }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

			protected:
				virtual LPCTSTR GetWindowClass() override { return _T("SYSLISTVIEW32"); }
			};

			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(GenericWindowWrapperSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MGroupBoxProperties::typeid, "MGroupBoxProperties")]
			public ref class GenericActiveX : BaseWindowWrapper
			{
			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				GenericActiveX(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="originalPosition">the original position of the control before any resize or location change</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				GenericActiveX(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, System::Drawing::Point originalPosition, bool hasCodeBehind);

			
				/// <summary>
				/// Specifies the ActiveX class id
				/// </summary>
				[System::ComponentModel::Localizable(true), System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property System::String^	ControlClass 
				{
					System::String^ get();
					void set(System::String^ value);
				}

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;
			private:
				bool CreateActiveX(CString sClass, CWnd* pParentWnd, CRect rect);
			};
		}
	}
}
