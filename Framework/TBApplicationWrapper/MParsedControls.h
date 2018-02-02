#pragma once

class CParsedCtrl;
class CBaseDocument;
class DlgInfoItem;
class DataObj;
class SqlRecord;
class Formatter;

#include "TbGenlib\PARSEDT.H"
#include "UITypeEditors.h"
#include "TBParsedControlHost.h"
#include "MHotLink.h"
#include "MFormatters.h"
#include "EBEventArgs.h"
#include "MHotLink.h"
#include "TBParsedControlHost.h"
#include "Utility.h"
#include "MControlsExtenders.h"


using namespace System::ComponentModel::Design::Serialization;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace System::Resources;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System::Drawing::Design;
using namespace System::ComponentModel;
using namespace System::ComponentModel::Design::Serialization;

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Interfaces::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Localization;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::View;


typedef ICSharpCode::NRefactory::CSharp::Expression AstExpression;

class CRegisteredParsedCtrl;
HWND GetChildWindow(HWND hParent, CString& strControlClass, System::Drawing::Point clientPosition, CMap<CString, LPCTSTR, HWND, HWND>& m_HWNDPositionsMap);
void SaveChildWindowPos(HWND hwndChild, System::Drawing::Point clientPosition, CMap<CString, LPCTSTR, HWND, HWND>& m_HWNDPositionsMap);

#define SET_STYLE_PARAMS(style) (value ? 0 : style), (value ? style : 0)

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			ref class MStateObject;
			ref class MFileItemsSource;
			ref class WindowWrapperContainer;

			public enum class EBool { Undefined = Bool3::B_UNDEFINED, True = Bool3::B_TRUE, False = Bool3::B_FALSE };
			public enum class EResizableControl { None = ::ResizableControl::R_NONE, Vertical = ::ResizableControl::R_VERTICAL, Horizontal = ::ResizableControl::R_HORIZONTAL, All = ::ResizableControl::R_ALL }; //edit

			[System::Flags]
			public enum class ELinePos {
				NONE = CLabelStatic::LP_NONE,
				TOP = CLabelStatic::LP_TOP,
				VCENTER = CLabelStatic::LP_VCENTER,
				BOTTOM = CLabelStatic::LP_BOTTOM,
				LEFT = CLabelStatic::LP_LEFT,
				RIGHT = CLabelStatic::LP_RIGHT,
				HCENTER = CLabelStatic::LP_HCENTER
			};

			[System::Flags]
			public enum class EControlStyle {
				NONE = CS_NONE,
				RESET_DEFAULTS = CS_RESET_DEFAULTS,
				NUMBERS = CS_NUMBERS,
				LETTERS = CS_LETTERS,
				UPPERCASE = CS_UPPERCASE,
				OTHERCHARS = CS_OTHERCHARS,
				ALLCHARS = CS_ALLCHARS,
				NO_EMPTY = CS_NO_EMPTY,
				BLANKS = CS_BLANKS,
				AS_PATH = CS_PATH_STYLE_AS_PATH,
				NO_CHECK_EXIST = CS_PATH_STYLE_NO_CHECK_EXIST,
				ENABLE_DEL = CS_COMBO_DROPDOWNLIST_STYLE_ENABLE_DEL
			};

			/// <summary>
			/// Describes an EasyBuilder action dispatched via post message or send message.
			/// </summary>
			/// <value></value>
			//================================================================================
			public enum EasyBuilderAction
			{
				/// <summary>
				/// No action, default value.
				/// </summary>
				None = 0,
				/// <summary>
				/// A row is changed, used for all grid controls.
				/// </summary>
				RowChanged = 1,
				/// <summary>
				/// A button was clicked.
				/// </summary>
				Clicked = 2,
				/// <summary>
				/// A value changed.
				/// </summary>
				ValueChanged = 3,
				/// <summary>
				/// State Button Clicked
				/// </summary>
				StateButtonClicked = 4
			};


			/// <summary>
			/// Root class for all EasyBuilder control wrappers
			/// </summary>
			//================================================================================
			[ExcludeFromIntellisense]
			public ref class EasyBuilderControl abstract : EasyBuilderComponent, System::IDisposable, IDesignerTarget
			{
			protected:
				bool						visible;
				IWindowWrapperContainer^	parent;

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property IDataType^ CompatibleType { virtual IDataType^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property EditingMode DesignerMovable { virtual EditingMode get() { return EditingMode::All; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool MouseDownTarget { virtual bool get() { return false; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual	void OnMouseDown(System::Drawing::Point p) { /*Non propaga il mouse down perchè non ha figli.*/ };

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property int AncestorCount
				{
					virtual int get()
					{
						if (Parent == nullptr || !EasyBuilderControl::typeid->IsInstanceOfType(Parent))
							return 0;

						return ((EasyBuilderControl^)Parent)->AncestorCount + 1;
					}
				}

				[System::ComponentModel::Browsable(false)]
				property IWindowWrapperContainer^ Parent
				{
					virtual IWindowWrapperContainer^ get() { return parent; }
					virtual void set(IWindowWrapperContainer^ value) { parent = value; ParentComponent = (EasyBuilderComponent^)value; }
				}


				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property System::String^ ClassName { virtual System::String^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::String^ SerializedName { virtual System::String^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::String^ SerializedType { virtual System::String^ get() override; }

				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Id { virtual System::String^ get() { return System::String::Empty; } }


				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ FullId { virtual System::String^ get() { return System::String::Empty; } }

				/// <summary>
				/// Gets or Sets the visible attribute for the control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool Visible { virtual bool get(); virtual void set(bool value); }

				/// <summary>
				/// Gets or sets the index in the tab order of this control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property int TabOrder { virtual int get(); virtual void set(int value); }
				/// <summary>
				/// true if this control can be activated pressing the tab key
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool TabStop { virtual bool get(); virtual void set(bool value); }

			protected:
				/// <summary>
				/// Constructor
				/// </summary>
				EasyBuilderControl();

			internal:
				/// <summary>
				/// Returns true if the given object is contained in a DbtSlaveBuffered
				/// </summary>
				/// <param name="obj">the object to check against the containers</param>
				bool IsInListContainer(Object^ obj);

			public:
				/// <summary>
				/// Activates the control
				/// </summary>
				virtual void Activate() { /*la default non fa nulla*/ };

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void SaveCurrentStatus(IDesignerCurrentStatus^ status) { /*la default non fa nulla*/ };

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ApplyCurrentStatus(IDesignerCurrentStatus^ status) { /*la default non fa nulla*/ };

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnDesignerControlCreated() { /*la default non fa nulla*/ }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedType) { return CanUpdateTarget(droppedType); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual void AfterTargetDrop(System::Type^ droppedType) {};

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual bool CanUpdateTarget(System::Type^ droppedType) { return false; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual void UpdateTargetFromDrop(System::Type^ droppedType) { }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropData(IDataBinding^ dataBinding);
			};

			/// <summary>
			/// Internal Use
			/// </summary>
			//================================================================================
			public ref class EasyBuilderControlSerializer : EasyBuilderSerializer
			{
			public:
				static System::String^ VisiblePropertyName = "Visible";
				static System::String^ SitePropertyName = "Site";
				static System::String^ ControlNamePropertyName = "Name";
				static System::String^ ViewClassName = "DocumentView";
				static System::String^ GetInfoPtrMethodName = "GetInfoPtr";
				static System::String^ GetTabberHandleMethodName = "GetTabberHandle";

				static System::String^ ControlNameVariableName = "controlName";
				static System::String^ ControlClassVariableName = "controlClass";
				static System::String^ HasCodeBehindVariableName = "hasCodeBehind";
				static System::String^ LocationVariableName = "location";
				static System::String^ ParentWindowVariableName = "parentWindow";
				static System::String^ WrappedObjectVariableName = "wrappedObject";
				static System::String^ DocumentPropertyName = "Document";
				static System::String^ FrameClassName = "DocumentFrame";

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual Object^ Serialize(System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, Object^ current) override;
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void SerializePropertiesAndAddMethod(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl, System::Collections::Generic::IList<Statement^>^ collection);

				virtual bool IsSerializable(EasyBuilderComponent^ ebComponent) override;

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				AstExpression^	GetParentWindowReference();

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual Statement^ GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl);

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual System::Drawing::Point GetLocationToSerialize(EasyBuilderControl^ ebControl);
			};

			/// <summary>
			/// Internal Use
			/// </summary>
			//================================================================================
			public ref class MParsedControlSerializer : EasyBuilderControlSerializer
			{
			public:
				static System::String^ ClassTypePropertyName = "ClassType";
				static System::String^ ContextPropertyName = "Context";
				static System::String^ RecordString = "Record";
				static System::String^ FormatterPropertyName = "Formatter";
				static System::String^ HotLinkPropertyName = "HotLink";
				static System::String^ HotLinkNsPropName = "HotLinkNs";
				static System::String^ ShowHotLinkButtonPropertyName = "ShowHotLinkButton";
				static System::String^ ItemsSourcePropertyName = "ItemsSource";
				static array<System::String^>^ BEStatusPropertynames = gcnew array<System::String^>{
					"IsGrayed",
						"IsHidden",
						"IsNoChange_Grayed",
						"IsNoChange_Hidden",
						"IsLocked",
						"IsSortedDes",
						"IsSortedAsc"		};
			};


			/// <summary>
			/// Derived from EasyBuilder control, is the base wrapper for all CWnd windows used in EasyBuilder
			/// </summary>
			[ExcludeFromIntellisense]
			[System::ComponentModel::Design::Serialization::DesignerSerializer(EasyBuilderControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[System::ComponentModel::PropertyTabAttribute(System::Windows::Forms::Design::EventsTab::typeid, System::ComponentModel::PropertyTabScope::Component)]
			//=============================================================================
			public ref class BaseWindowWrapper : EasyBuilderControl, IWindowWrapper, IEasyBuilderComponentExtendable
			{
				System::Windows::Forms::NativeWindow^ nativeWindow;
				EasyBuilderComponentExtenders^	extensions;
				
			private:
				int autoSizeCtrl = -1;

			public:
				static System::String^	staticAreaID = "IDC_STATIC_AREA";
				static System::String^	staticArea1ID = "IDC_STATIC_AREA_1";
				static System::String^	staticArea2ID = "IDC_STATIC_AREA_2";
				static System::String^	staticAreaName = "Static Area";
				static System::String^	staticArea1Name = "Static Area 1";
				static System::String^	staticArea2Name = "Static Area 2";

				bool EndCreation = false;
				CWndObjDescription* jsonDescription = NULL;

			protected:
				System::Drawing::Color			borderColor;
				System::Drawing::Size			minSize;//OKKIO: in logical units
				System::String^					idPrefix = "IDC_";
				System::String^					name;
				System::String^					id;
				Point							partAnchor;
				Point							originalLocation;

			protected:

				///<summary>
				///Calculate left side brother. Used for json serialization
				///</summary>
				CString GetHorizontalIdAnchor();

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property bool DesignerVisible { virtual bool get(); }

			public:

				//----------------------------------------------------------------------------	
#pragma region BrowsableFalse properties
				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property IEasyBuilderComponentExtenders^ Extensions { virtual  IEasyBuilderComponentExtenders^ get() { return extensions; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::ComponentModel::ISite^ Site { virtual void set(System::ComponentModel::ISite^ site) override; }

				/// <summary>
				/// Gets or Sets the Handle for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::IntPtr	Handle { virtual System::IntPtr get(); virtual void set(System::IntPtr handle); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property bool DesignMode { virtual  bool get() override; }

				/// <summary>
				/// Gets the Rectangle of the control in screen coordinates
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::Drawing::Rectangle	Rectangle { virtual System::Drawing::Rectangle get(); }


				/// <summary>
				/// Gets the MinSize for the current control, in Logical Units
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::Browsable(false)]
				property System::Drawing::Size MinSize { System::Drawing::Size get() { return minSize; }  }

				/// <summary>
				/// Gets or Sets the label for the current control
				/// </summary>
				[System::ComponentModel::Localizable(true), System::ComponentModel::Browsable(false),
					TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::BoolTypes)]
				property System::String^ ControlLabel { virtual System::String^ get(); }

				/// <summary>
				/// Gets or Sets the border color for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::Drawing::Color BorderColor { virtual System::Drawing::Color get(); virtual void set(System::Drawing::Color value); }


				/// <summary>
				/// Gets the Rectangle of the control in client coordinates
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::Drawing::Rectangle	ClientRectangle { virtual System::Drawing::Rectangle get(); }

				/// <summary>
				/// true if the window should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Browsable(false)]
				property bool Transparent { virtual bool get(); virtual void set(bool value); 			}

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property EditingMode DesignerMovable { virtual EditingMode get() override; }

#pragma endregion
				//----------------------------------------------------------------------------	
#pragma region DesignStatic properties

	/// <summary>
	/// Gets or sets the property that it's true if the window accepts drag-drop files
	/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool AcceptFiles {
					virtual bool get();
					virtual void set(bool value);
				}


				/// <summary>
				/// Gets the margin left for the current control, in Logical Units
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property int MarginLeft { virtual int get(); virtual void set(int value); }
				/// <summary>
				/// Gets the margin top for the current control, in Logical Units
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property int MarginTop { virtual int get(); virtual void set(int value); }
				/// <summary>
				/// Gets the margin bottom for the current control, in Logical Units
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property int MarginBottom { virtual int get(); virtual void set(int value); }

				/// <summary>
				/// Gets the caption width for the current control, in Logical Units
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property int CaptionWidth { virtual int get(); virtual void set(int value); }

				/// <summary>
				/// true if the window has a horizontal scroll bar
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool HScroll {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// Gets or sets the property that it's true if the window has a vertical scroll bar
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool VScroll {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// Gets or sets the activation string. It's the expression used to decide if this control has to be created; may be a logical combination (even using parenthesis) of atoms similar to:
				/// APP.MODULE (to test the activation)
				/// VARIABLE_NAME (test the state of a boolean variable in the document)
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Activation { virtual System::String^ get();  virtual void set(System::String^ value);  }

				/// <summary>
				/// Gets or Sets the location for the current control, in logical units
				/// </summary>
				[DisplayName("Location"), LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic | TBPropertyFilters::ComponentState),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Drawing::Point LocationLU { virtual System::Drawing::Point get(); virtual void set(System::Drawing::Point value); }

				/// <summary>
				/// Gets or Sets the size for the current control, in logical units
				/// </summary>
				[DisplayName("Size"), LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property System::Drawing::Size SizeLU { virtual System::Drawing::Size get(); virtual void set(System::Drawing::Size value); }

				/// <summary>
				/// Gets or Sets the anchor criteria for the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.AnchorTypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property System::String^ Anchor { virtual System::String^ get(); virtual void set(System::String^ value); }

				/// <summary>
				/// true if the window is the first control of a group of controls
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Group { virtual bool get(); virtual void set(bool value); }

#pragma endregion 
				//----------------------------------------------------------------------------	
#pragma region DesignRunTime properties

/// <summary>
/// Gets or Sets the location for the current control
/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime | TBPropertyFilters::ComponentState),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Drawing::Point Location { virtual System::Drawing::Point get(); virtual void set(System::Drawing::Point value); }

				/// <summary>
				/// Gets or Sets the size for the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
				property System::Drawing::Size Size { virtual System::Drawing::Size get(); virtual void set(System::Drawing::Size value); }

				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ FullId { virtual System::String^ get() override; }


				/// <summary>
				/// Gets the namespace of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
				property INameSpace^ Namespace { virtual INameSpace^ get(); }

#pragma endregion
				//--------------BOTH----------------------------------------------------------	

				/// <summary>
				/// Gets the type of the control (as System::String)
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ WindowType { System::String^ get() { return GetType()->ToString(); } }

				/// <summary>
				/// Gets or Sets the visible attribute for the control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool Visible { virtual bool get() override; virtual void set(bool value)override; }

				/// <summary>
				/// Gets the name of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { virtual System::String^ get() override; }

				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Id { virtual System::String^ get() override;  virtual void set(System::String^ value);  }

				/// <summary>
				/// Gets or sets the index in the tab order of this control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property int TabOrder { virtual int get() override; virtual void set(int value) override; }

				/// <summary>
				/// true if this control can be activated pressing the tab key
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool TabStop { virtual bool get() override; virtual void set(bool value) override; }

				/// <summary>
				/// Gets or sets the enabled attribute for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool Enabled { virtual bool get(); virtual void set(bool value); }

				/// <summary>
				/// true if the window has a thin-line border
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Border { virtual bool get(); virtual void set(bool value); }

				/// <summary>
				/// Gets or Sets the text for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
				property System::String^ Text { virtual System::String^ get(); virtual void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the current anchor value to part
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), 
					TBPropertyFilter(TBPropertyFilters::DesignerRuntime | TBPropertyFilters::ComponentState)
				]
				property Point PartAnchor { Point get(); void set(Point value); }

				/// <summary>
				/// Gets or Sets the AutoStretch property for the bodyedit - only for historical compatibility reasons
				/// </summary>
				[System::ComponentModel::Browsable(false),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					TBPropertyFilterAttribute(TBPropertyFilters::Stretchable)
				]
				property bool AutoStretch { virtual void set(bool value); }

				/// <summary>
				/// Gets or Sets the BottomStretch property for stretchable controls
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime | TBPropertyFilters::Stretchable)
				]
				property bool BottomStretch { virtual bool get(); virtual void set(bool value); }

				/// <summary>
				/// Gets or Sets the RightStretch property for stretchable controls
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime | TBPropertyFilters::Stretchable)
				]
				property bool RightStretch { virtual bool get(); virtual void set(bool value); }

				/// <summary>
				/// Gets or Sets the AutoFill property for stretchable controls
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime | TBPropertyFilters::Stretchable)
				]
				property bool AutoFill { virtual bool get(); virtual void set(bool value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;


			public:
				bool SaveSerialization(const CString& fileName, CWndObjDescription* pDescription);

				///<summary>
				///Updates needed attributes for json serialization 
				///</summary>
				virtual CWndObjDescription* UpdateAttributesForJson(CWndObjDescription* pParentDescription);

				/// <summary>
				/// Event raised when the user click on the active control
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				virtual event System::EventHandler<System::Windows::Forms::MouseEventArgs^>^ MouseDown;

				/// <summary>
				/// Event raised when the user release the click on the active control
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				virtual event System::EventHandler<System::Windows::Forms::MouseEventArgs^>^ MouseUp;

				/// <summary>
				/// Event raised when the paint message is raised by the application
				/// </summary>
				MESSAGE_HANDLER_EVENT(Paint, EasyBuilderEventArgs, "Occurs during paint.");

				/// <summary>
				/// Event raised when the size of the window changes
				/// </summary>
				MESSAGE_HANDLER_EVENT(SizeChanged, EasyBuilderEventArgs, "Occurs when window size is changed.");
				/// <summary>
				/// Event raised when the location of the window changes
				/// </summary>
				MESSAGE_HANDLER_EVENT(LocationChanged, EasyBuilderEventArgs, "Occurs when window location is changed.");

				/// <summary>
				/// Event raised when the SetFocus message is raised by the application
				/// </summary>
				MESSAGE_HANDLER_EVENT(SetFocus, EasyBuilderEventArgs, "Occurs when window takes focus.");

			public:
				/// <summary>
				/// Constructor:
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				BaseWindowWrapper(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				BaseWindowWrapper(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				virtual ~BaseWindowWrapper();

				/// <summary>
				/// Finalizer
				/// </summary>
				!BaseWindowWrapper();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) { return false; }

				System::Drawing::Size	AdjustMinSizeOnParent(BaseWindowWrapper^ control, IWindowWrapperContainer^ parentWindow);
			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				HWND GetHandle();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::String^ GetInternalName();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				System::String^ CreateNamespaceFromParent(IEasyBuilderContainer^ parentContainer);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				System::String^ CalcIdFromName(System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				System::String^ CalcNameFromId(System::String^ id);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual int	GetNamespaceType();
			public:

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual TbResourceType GetTbResourceType();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void SwitchVisibility(bool visible);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				bool IsWindowVisible();

				/// <summary>
				/// Sets the focus on the current control
				/// </summary>
				virtual	void Focus();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	void AfterWndProc(System::Windows::Forms::Message% m);

				/// <summary>
				/// Updates the client area of the specified window
				/// </summary>
				virtual void UpdateWindow();

				/// <summary>
				/// Forces the refresh of the current window in the specified rectangle area
				/// </summary>
				virtual void Invalidate(System::Drawing::Rectangle screenCoordRect);

				/// <summary>
				/// Forces the refresh of the current window
				/// </summary>
				virtual void Invalidate();

				/// <summary>
				/// Converts the given rectanglo from screen to client coordinates
				/// </summary>
				virtual void ScreenToClient(System::Drawing::Rectangle% rect);

				/// <summary>
				/// Converts the given rectanglo from client to screen coordinates
				/// </summary>
				virtual void ClientToScreen(System::Drawing::Rectangle% rect);

				/// <summary>
				/// Converts the given rectanglo from screen to client coordinates
				/// </summary>
				virtual void ScreenToClient(System::Drawing::Point% point);

				/// <summary>
				/// Converts the given rectangle from client to screen coordinates
				/// </summary>
				virtual void ClientToScreen(System::Drawing::Point% point);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnDesignerControlCreated() override;

				/// <summary>
				/// Performs a syncronous call 
				/// </summary>
#pragma push_macro("SendMessage")
#undef SendMessage
				virtual System::IntPtr SendMessage(int msg, System::IntPtr wParam, System::IntPtr lParam);
#pragma pop_macro("SendMessage")
				/// <summary>
				/// Performs an asyncronous call 
				/// </summary>
#pragma push_macro("PostMessage")
#undef PostMessage
				virtual bool PostMessage(int msg, System::IntPtr wParam, System::IntPtr lParam);
#pragma pop_macro("PostMessage")
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::IntPtr GetWndPtr();

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual CWnd* GetWnd();
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				CWndObjDescription* GetWndObjDescription();
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) {}


				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				static IWindowWrapper^ Create(System::IntPtr handle);
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				static bool CanBeWrapped(System::IntPtr handle);

				/// <summary>
				/// Activates the control
				/// </summary>
				virtual void Activate() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				void UpdateViewOutlineOrder(BaseWindowWrapper^ targetNode);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnBuildingSecurityTree(System::IntPtr tree, System::IntPtr infoTreeItems) {};

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				static System::String^ GetIdFormID(UINT nID, TbResourceType aType, BOOL full);

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanCreate();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				void InvalidateInternal(CWnd* pWnd, const CRect& screenRect);
				[ExcludeFromIntellisense]
				void CreateUniqueName(IEasyBuilderContainer^ parentContainer);
			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				void CreateUniqueNameAndId(WindowWrapperContainer^ parentContainer, System::String^ nameSeed, System::String^ idSeed);
			internal:
				bool HasStyle(DWORD s);
				virtual void SetStyle(DWORD dwRemove, DWORD dwAdd);
				bool HasExStyle(DWORD s);
				virtual void SetExStyle(DWORD dwRemove, DWORD dwAdd);
				CString FullIdToResourceKey(System::String^ fullId);
				CBaseTileDialog* GetParentTileDialog();
				void CalculatePartAnchor(CPoint pt);
				virtual void DelayedPartsAnchor();

			private:
				void DoSetSize(System::Drawing::Size size);
				void DoSetLocation(System::Drawing::Point point);
			};

			/// <summary>
			/// Internal Use
			/// </summary>
			[ExcludeFromIntellisense]
			[System::ComponentModel::DefaultPropertyAttribute("ControlClassName")]
			[System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
			//================================================================================
			public ref class ControlClass : Microarea::TaskBuilderNet::Core::EasyBuilder::ExpandiblePropertyItem, IControlClass
			{
			private:
				BaseWindowWrapper^		control;
				CRegisteredParsedCtrl*	m_pRegInfo;

			public:
				ControlClass(BaseWindowWrapper^ control);
				~ControlClass();
				!ControlClass();

			internal:
				ControlClass(CRegisteredParsedCtrl* pRegInfo);

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				property System::String^ ClassName { virtual System::String^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				property System::String^ FamilyName { virtual System::String^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				property System::String^ ClassDescription { System::String^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				property System::String^ CompatibleTypeName { System::String^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property IDataType^	CompatibleType { virtual IDataType^ get(); }

				virtual System::String^ ToString() override;

			internal:

				CRegisteredParsedCtrl*	GetRegInfoPtr();
				void	SetRegInfoPtr(CRegisteredParsedCtrl*);
			};



			/// <summary>
			/// Derive da BaseWindowWrapper: is the base class for all EasyBuilder parsedcontrols
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			public ref class MParsedControl : BaseWindowWrapper, IDataBindingConsumer, IControlClassConsumer, System::ComponentModel::IContainer, IEasyBuilderDataTypeProperties
			{
				CFont*				m_pFont;
			protected:
				MHotLink^			hotLink;
				bool				showHotLinkButton;
				CParsedCtrl*		m_pControl;
				IDataBinding^		dataBinding;
				ControlClass^		controlClass;

				System::Collections::Generic::List<System::ComponentModel::IComponent^>^				components;

			public:

#pragma region BrowsableFalse properties
				/// <summary>
				/// Gets or Sets the label for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::Localizable(true),
					TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::BoolTypes)]
				property System::String^ ControlLabel { virtual System::String^ get() override;  }


				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataType^ CompatibleType { virtual IDataType^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataType^ FilteredDataType { virtual IDataType^ get() { return CompatibleType; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property bool HasFamilyClassChangeable { virtual bool get() { return false; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property bool CanAutoFillFromDataBinding { virtual bool get() { return false; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Drawing::Font^ Font { virtual void set(System::Drawing::Font^ value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Type^ ExcludedBindParentType { virtual System::Type^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataManager^ FixedDataManager { virtual IDataManager^ get() { return nullptr; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataType^ CompatibleDataType { virtual IDataType^ get() { return CompatibleType; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::ComponentModel::ComponentCollection^ Components { virtual  System::ComponentModel::ComponentCollection^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property System::String^ ClassName { virtual System::String^ get() override; }

				/// <summary>
				/// Gets the SqlRecord associated to the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property MSqlRecord^ SqlRecord { MSqlRecord^ get() { return (m_pControl) ? gcnew MSqlRecord((System::IntPtr)m_pControl->m_pSqlRecord) : nullptr; }}

				/// <summary>
				/// Gets the dataobj associated to the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property MDataObj^ DataObj { MDataObj^ get() { return (m_pControl) ? MDataObj::Create((m_pControl->GetCtrlData())) : nullptr; }}

#pragma endregion
				//----------------------------------------------------------------------------	
#pragma region DesignStatic properties
/// <summary>
/// Gets or Sets the data source for the current control
/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.DataSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ DataSource {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.HotlinkUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ HotLinkNs {  virtual System::String^ get();  virtual void set(System::String^ value); }


				/// <summary>
				/// Gets or sets the hotlink name for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic), LocalizedCategory("DataBindingCategory", EBCategories::typeid)]
				property System::String^ HotLinkName { System::String^ get();  void set(System::String^ value); }

				/// <summary>
				/// Gets or sets the min value for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true),
					TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property System::String^ MinValueStr {   System::String^ get();  void set(System::String^ minValue); }

				/// <summary>
				/// Gets or sets the max value for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true),
					TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property System::String^ MaxValueStr {   System::String^ get();  void set(System::String^ maxValue); }


				/// <summary>
				/// Gets or Sets the caption vertical alignment for the current control 
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property EVerticalAlign CaptionVerticalAlign { virtual EVerticalAlign get(); virtual void set(EVerticalAlign value);  }
				/// <summary>
				/// Gets or Sets the caption horizontal alignment for the current control 
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property ETextAlign CaptionAlign { virtual ETextAlign get(); virtual void set(ETextAlign value);  }

#pragma endregion 
				//----------------------------------------------------------------------------	
#pragma region DesignRunTime properties
/// <summary>
/// Gets or Sets the formatter for the current control
/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::EditorAttribute(FormatterUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property FormatterStyle^ Formatter { FormatterStyle^ get(); void set(FormatterStyle^ value); }

				/// <summary>
				/// Gets or sets the hotlink for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), ObjectReference, System::ComponentModel::EditorAttribute(HotLinkUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid),
					TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)]
				property MHotLink^ HotLink { MHotLink^ get() { return hotLink; }  virtual void set(MHotLink^ value); }

				/// <summary>
				/// Gets or sets the min value for the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::Browsable(true),
					TBPropertyFilter(TBPropertyFilters::RealTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::DesignerRuntime)]
				property Object^ MinValue { virtual Object^ get(); virtual void set(Object^ minValue); }

				/// <summary>
				/// Gets or sets the max value for the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::Browsable(true),
					TBPropertyFilter(TBPropertyFilters::RealTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::DesignerRuntime)]
				property Object^ MaxValue { virtual Object^ get(); virtual void set(Object^ maxValue); }

				/// <summary>
				/// Gets or Sets the data binding for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property  IDataBinding^ DataBinding {  virtual IDataBinding^ get();  virtual void set(IDataBinding^ dataBinding); }

				/// <summary>
				/// Gets or sets the max lenght for the current control
				/// </summary>
				[System::ComponentModel::Browsable(true), LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::DesignerRuntime)]
				property int MaxLength { virtual int get(); virtual void set(int length); }


				/// <summary>
				/// Gets the namespace of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property INameSpace^ Namespace { virtual INameSpace^ get() override; }

#pragma endregion

				//-----------BOTH----------------------------------------------------------	
								/// <summary>
								/// Gets or Sets the name for the current control
								/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ name) override; }

				/// <summary>
				/// Sets the visible attribute for the control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool Visible { virtual void set(bool value) override; }

				/// <summary>
				/// Gets or Sets the class type for the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid),
					TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::EditorAttribute(ControlClassUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IControlClass^ ClassType { virtual IControlClass^ get(); virtual void set(IControlClass^ value); }


				/// <summary>
				/// Gets or Sets the text for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true), System::ComponentModel::Browsable(true)]
				property System::String^ Text { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

				/// <summary>
				/// Gets or Sets the caption for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
				property System::String^ Caption { virtual System::String^ get(); virtual void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the caption font for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property System::Drawing::Font^ CaptionFont { virtual System::Drawing::Font^ get(); virtual void set(System::Drawing::Font^ value); }

				/// <summary>
				/// Sets the desired location for the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Drawing::Point Location { virtual void set(System::Drawing::Point value) override; }

				/// <summary>
				/// Gets or sets the size of the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Size Size { virtual void set(System::Drawing::Size value) override; }

				/// <summary>
				/// Gets or sets the property that allows to show or hide the hotlink button for the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
				property bool ShowHotLinkButton { virtual bool get();  virtual void set(bool value); }

				/// <summary>
				/// Gets or sets the number of decimal for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Browsable(true), TBPropertyFilter(TBPropertyFilters::RealTypes)]
				property int NumberOfDecimal { virtual int get(); virtual void set(int nDec); }
				/// <summary>
				/// Gets or sets the style for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.ControlStyleUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property EControlStyle ControlStyle { EControlStyle get(); void set(EControlStyle style); }

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedControl(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedControl(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				~MParsedControl();

				/// <summary>
				/// Finalizer
				/// </summary>
				!MParsedControl();

				/// <summary>
				/// Event raised when the value changed message is raised by the control
				/// </summary>
				MESSAGE_HANDLER_EVENT(ValueChanged, EasyBuilderEventArgs, "Occurs when control value is changed.");

			public:

				///<summary>
				///Updates needed attributes for json serialization 
				///</summary>
				virtual CWndObjDescription* UpdateAttributesForJson(CWndObjDescription* pParentDescription) override;

				/// <summary>
				/// Override of the equals method, true if the compared tabs are the same
				/// </summary>
				virtual bool Equals(Object^ obj) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual CWnd* GetWnd() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanCreate() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ReSetCtrlCaption(CString caption);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				static System::Type^ GetDefaultControlType(IDataType^ dataType, bool readOnly, System::String^% controlClass);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void AutoFillFromDataBinding(IDataBinding^ dataBinding, bool overrideExisting) {};

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				bool Create(System::Windows::Forms::Control^ parent, System::Drawing::Rectangle rect, System::String^ controlName, System::String^ className);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component, System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Remove(System::ComponentModel::IComponent^ component);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				void SetParent(IWindowWrapperContainer^	parent);


			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void MoveAuxControl(CWnd* pWndToMove, const CSize& offset);

				/// <summary>
				/// Internal Use
				/// </summary>
				//[ExcludeFromIntellisense]		
				//virtual void MoveHyperLink(CWnd* pWndToMove, Point positionToMove, CRect parsedControlOriginalRect);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ResizeAuxButton(CWnd* pWndToMove, CRect parsedControlOriginalRect, CRect newParsedControlRect);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ResizeHyperLink(CWnd* pWndToMove, System::Drawing::Size sz);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::String^ GetInternalName() override;
			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedObject) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;
			internal:
				virtual void SetStyle(DWORD dwRemove, DWORD dwAdd) override;
				virtual void SetExStyle(DWORD dwRemove, DWORD dwAdd) override;
				virtual void DelayedPartsAnchor() override;
			private:
				void ChangeClassType(System::String^ name);
				bool IsFromMyParentView(System::Windows::Forms::Message% m);
				bool IsFromMyParentViewRecursive(IWindowWrapperContainer^ windowWrapper, int hashCode);

				void SetMinMaxStr(System::String^ value, bool isMinValue);
			};

			/// <summary>
			/// Derived from MParsedControl: is the wrapper to the CParsedEdit Class
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MStateObject::typeid, "StateButton")]
			[EasyBuilderComponentExtender(MEditProperties::typeid, "EditProperties")]
			public ref class MParsedEdit : MParsedControl
			{
			public:
				/// <summary>
				/// Gets or sets the back color for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerRuntime), LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Color BackColor { virtual System::Drawing::Color get(); void set(System::Drawing::Color value); }

				/// <summary>
				/// Gets or sets the fore color for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerRuntime), LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Color ForeColor { virtual System::Drawing::Color get(); void set(System::Drawing::Color value); }

				/// <summary>
				/// Gets or Sets the Chars attribute for the current control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property int Chars { virtual int get(); void set(int value); }

				/// <summary>
				/// Gets or Sets the Chars attribute for the current control
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
				property EResizableControl Resizable { EResizableControl get(); void set(EResizableControl value);  }

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedEdit(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedEdit(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual bool CanUpdateTarget(System::Type^ droppedType) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual void UpdateTargetFromDrop(System::Type^ droppedType) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;
			};

			/// <summary>
			/// Derived from MParsedControl: is the wrapper to the CParsedStatic Class
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MLabelProperties::typeid, "MLabelProperties")]
			public ref class MParsedStatic : MParsedControl
			{
			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedStatic(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedStatic(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

			public:

				/// <summary>
				/// Gets or sets the hotlink for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property MHotLink^ HotLink {
					MHotLink^ get() { return __super::HotLink; }
					virtual void set(MHotLink^ value) override { __super::HotLink = value; }
				}

				/// <summary>
				/// Allows to show or hide the hotlink button for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool ShowHotLinkButton {
					virtual bool get() override { return __super::ShowHotLinkButton; }
					virtual void set(bool value) override { __super::ShowHotLinkButton = value; }
				}

				/// <summary>
				/// Gets or sets the line position for border
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic), EditorAttribute("Microarea.EasyBuilder.UI.LinePositionUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid),
					LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property ELinePos LinePosition { virtual ELinePos get(); void set(ELinePos value); }

				/// <summary>
				/// Gets or sets the back color for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerRuntime), LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Color BackColor { virtual System::Drawing::Color get(); void set(System::Drawing::Color value); }

				/// <summary>
				/// Gets or sets the fore color for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerRuntime), LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Color ForeColor { virtual System::Drawing::Color get(); void set(System::Drawing::Color value); }


				virtual bool CanChangeProperty(System::String^ propertyName) override;
			};

			/// <summary>
			/// Derived from MParsedControl: is the wrapper to the CParsedCombo Class
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MStateObject::typeid, "StateButton")]
			[EasyBuilderComponentExtender(MFileItemsSource::typeid, "FileItemsSource")]
			[EasyBuilderComponentExtender(MComboBoxProperties::typeid, "MComboBoxProperties")]
			public ref class MParsedCombo : MParsedControl, IItemsSourceConsumer
			{
			private:
				System::Collections::IList^	itemsSource;
				System::Delegate^					fillComboCallBack;
				System::Runtime::InteropServices::GCHandle					fillComboHandle;


			public:
				/// <summary>
				/// Gets or sets the max number of rows for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::DesignerRuntime)]
				property int MaxRowsNumber { int get(); void set(int nRows); }

				/// <summary>
				/// Gets or sets the max number of items for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::DesignerRuntime)]
				property int MaxItemsNumber { int get(); void set(int nRows); }

				/// <summary>
				/// Gets or sets the item collection associated to the combo
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid)]
				[System::ComponentModel::EditorAttribute(ItemSourcesUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Content)]
				[TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::DesignerRuntime)]
				property System::Collections::IList^	ItemsSource { virtual System::Collections::IList^ get(); virtual void set(System::Collections::IList^ value); }

				/// <summary>
				/// Gets or sets the hotlink for the current control
				/// </summary>
				[System::ComponentModel::Browsable(true), LocalizedCategory("DataBindingCategory", EBCategories::typeid), ObjectReference,
					TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState),
					System::ComponentModel::EditorAttribute(HotLinkUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property MHotLink^ HotLink { virtual void set(MHotLink^ value) override; }

				/// <summary>
				/// Gets or Sets the item source namespace for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.ItemSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ ItemSourceNs {  virtual System::String^ get();  virtual void set(System::String^ value); }


				/// <summary>
				/// Gets or sets the item source name for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerStatic), LocalizedCategory("DataBindingCategory", EBCategories::typeid)]
				property System::String^ ItemSourceName { System::String^ get();  void set(System::String^ value); }

				[System::ComponentModel::Browsable(false)]
				property bool	IsItemsSourceEditable { virtual bool get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual bool CanUpdateTarget(System::Type^ droppedType) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual void UpdateTargetFromDrop(System::Type^ droppedType) override;

			public:
				/// <summary>
				/// Event raised when the SelectedIndexChanged message is raised by the control
				/// </summary>
				MESSAGE_HANDLER_EVENT(SelectedIndexChanged, EasyBuilderEventArgs, "Occurs when combo box selected index is changed.");

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedCombo(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedCombo(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				~MParsedCombo();

				/// <summary>
				/// Distructor
				/// </summary>
				!MParsedCombo();



			internal:
				delegate bool	FillComboCallBack();
				bool			OnFillCombo();

			private:
				void			AttachCallBackFunction();

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void ResizeHyperLink(CWnd* pWndToMove, System::Drawing::Size sz) override;

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void RefreshContentByDataType();
			};

			/// <summary>
			/// Derived from MParsedControl: is the wrapper to the CParsedListBox Class
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MListBoxProperties::typeid, "MListBoxProperties")]
			public ref class MParsedListBox : MParsedControl, IItemsSourceConsumer
			{
			private:
				System::Collections::IList^	itemsSource;
				System::Delegate^					fillListBoxCallBack;
				System::Runtime::InteropServices::GCHandle					fillListBoxHandle;

			public:
				/// <summary>
				/// Event raised when the SelectedIndexChanged message is raised by the control
				/// </summary>
				MESSAGE_HANDLER_EVENT(SelectedIndexChanged, EasyBuilderEventArgs, "Occurs when list box selected index is changed.");

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedListBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedListBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				~MParsedListBox();

				/// <summary>
				/// Gets or sets the item collection associated to the listbox
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				[System::ComponentModel::EditorAttribute(ItemSourcesUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Content)]
				[TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::DesignerRuntime)]
				property System::Collections::IList^	ItemsSource { virtual System::Collections::IList^ get(); virtual void set(System::Collections::IList^ value); }
				/// <summary>
				/// Gets or Sets the item source namespace for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.ItemSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ ItemSourceNs {  virtual System::String^ get();  virtual void set(System::String^ value); }


				/// <summary>
				/// Gets or sets the item source name for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerStatic), LocalizedCategory("DataBindingCategory", EBCategories::typeid)]
				property System::String^ ItemSourceName { System::String^ get();  void set(System::String^ value); }

				/// <summary>
				/// Gets or sets the hotlink for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property MHotLink^ HotLink { MHotLink^ get() { return __super::HotLink; }  virtual void set(MHotLink^ value) override { __super::HotLink = value; } }

				/// <summary>
				/// Allows to show or hide the hotlink button for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool ShowHotLinkButton { virtual bool get() override { return __super::ShowHotLinkButton; } virtual void set(bool value) override { __super::ShowHotLinkButton = value; }  }

				[System::ComponentModel::Browsable(false)]
				property bool	IsItemsSourceEditable { virtual bool get(); }

				[System::ComponentModel::Browsable(false)]
				property IWindowWrapperContainer^ Parent { virtual IWindowWrapperContainer^ get() override { return __super::Parent; } virtual void set(IWindowWrapperContainer^ value) override; }

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void Initialize() override;

			internal:
				delegate bool	FillListBoxCallBack();
				bool			OnFillListBox();

			private:
				void			AttachCallBackFunction();
				void			ResumeLayout(Object^ sender, EasyBuilderEventArgs^ e);

			public:
				/// <summary>
				/// Refresh content of listbox
				/// </summary>
				void Refresh();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void RefreshContentByDataType();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;
			};

			/// <summary>
			/// Derived from MParsedControl: is the wrapper to the CParsedButton Class
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			public ref class MParsedButton : MParsedControl
			{

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedButton(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Gets or sets the caption associated to the button
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
				property System::String^ Caption { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

				/// <summary>
				/// Gets or Sets the caption font for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
				property System::Drawing::Font^ CaptionFont {
					virtual System::Drawing::Font^ get() override { return nullptr; }
					virtual void set(System::Drawing::Font^ value) override {}; }

				/// <summary>
				/// Specifies vertical alignment
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
				property EVerticalAlign CaptionVerticalAlign {
					virtual EVerticalAlign get() override { return EVerticalAlign::Center; }
					virtual void set(EVerticalAlign value) override {};  }

				/// <summary>
				/// Gets or sets the hotlink for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
				property MHotLink^ HotLink {
					MHotLink^ get() { return __super::HotLink; }
					virtual void set(MHotLink^ value) override { __super::HotLink = value; }
				}

				/// <summary>
				/// Allows to show or hide the hotlink button for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool ShowHotLinkButton {
					virtual bool get() override { return __super::ShowHotLinkButton; }
					virtual void set(bool value) override { __super::ShowHotLinkButton = value; }
				}

				/// <summary>
				/// Gets or Sets the text for the current control
				/// </summary>
				[System::ComponentModel::Localizable(true), System::ComponentModel::EditorAttribute(System::ComponentModel::Design::MultilineStringEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property System::String^	Text {
					virtual System::String^ get() override { return __super::Text; }
					virtual void set(System::String^ value) override { __super::Text = value; }
				}

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;

			};

			/// <summary>
			/// Derived from MParsedControl: is the wrapper to the CPushBotton Class
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MCheckBoxProperties::typeid, "MCheckBoxProperties")]
			public ref class MCheckBox : MParsedButton
			{
			private:
				bool checkedCheck;

			public:
				/// <summary>
				/// Event raised when the status of the checkbox is changed
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				virtual event System::EventHandler<EasyBuilderEventArgs^>^ CheckedChanged;

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MCheckBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MCheckBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

			public:
				/// <summary>
				/// Gets or Sets the checked attribute for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::ReadOnly(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
				property bool Checked { virtual bool get(); virtual void set(bool checked); }

				/// <summary>
				/// Gets or Sets the property that it's true if the window is the first control of a group of controls
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime | TBPropertyFilters::DesignerStatic)]
				property bool Group {
					virtual bool get() override;
					virtual void set(bool value) override;
				}

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			};

			/// <summary>
			/// Derived from MParsedButton: is the wrapper for the radio button control
			/// </summary>
			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MRadioButtonProperties::typeid, "MRadioButtonProperties")]
			public ref class MRadioButton : MParsedButton
			{
			private:
				bool checkedRadio;

			public:
				/// <summary>
				/// Event raised when the status of the checkbox is changed
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				virtual event System::EventHandler<EasyBuilderEventArgs^>^ CheckedChanged;

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				MRadioButton(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MRadioButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

			public:
				/// <summary>
				/// Gets or Sets the checked attribute for the current control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::ReadOnly(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Visible)]
				property bool Checked { virtual bool get(); virtual void set(bool checked); }

				/// <summary>
				/// true if the window is the first control of a group of controls
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime | TBPropertyFilters::DesignerStatic)]
				property bool Group {
					virtual bool get() override;
					virtual void set(bool value) override;
				}

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			};

			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MPushButtonProperties::typeid, "MPushButtonProperties")]
			public ref class MPushButton : MParsedButton
			{
				/// <summary>
				/// Event raised when the Click message is raised by the control
				/// </summary>
				MESSAGE_HANDLER_EVENT(Click, EasyBuilderEventArgs, "Occurs when button is clicked");

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MPushButton(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MPushButton(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;
			};

			//=============================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			[EasyBuilderComponentExtender(MPushButtonProperties::typeid, "MPushButtonProperties")]
			public ref class MParsedGroupBox : MParsedButton
			{
			public:
				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MParsedGroupBox(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MParsedGroupBox(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnCreateStyles(DWORD& styles, DWORD& exStyles) override;
			};

			//================================================================================
			public ref class LocalHost : TBParsedControlHost
			{
			private:
				System::Windows::Forms::TextBox^ textBox;
				MParsedControl^ wrapper;

			public:
				property MParsedControl^ Wrapper { MParsedControl^ get(); void set(MParsedControl^ value); }

				LocalHost(System::Windows::Forms::TextBox^ textBox);
				~LocalHost();
				!LocalHost();

				void TextBox_SizeChanged(Object^ sender, System::EventArgs^ e);

			private:
				void SetPanelSize(System::Windows::Forms::TextBox^ textBox);
			};


		}
	}
}
