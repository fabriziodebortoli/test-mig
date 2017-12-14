#pragma once
#include "TbGeneric\WndObjDescription.h"

using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Localization;
using namespace Microarea::TaskBuilderNet::Interfaces::EasyBuilder;
namespace Microarea {
	namespace Framework	{
		namespace TBApplicationWrapper {

			public enum class EListCtrlAlign	{ Top = ::ListCtrlAlign::LC_TOP,		Left = ::ListCtrlAlign::LC_LEFT					};
			public enum class ETextAlign		{ Left = ::TextAlignment::TALEFT,		Center = ::TextAlignment::TACENTER,				Right = ::TextAlignment::TARIGHT				};			//edit
			public enum class EOwnerDrawType	{ None = ::OwnerDrawType::NO,			Fixed = ::OwnerDrawType::ODFIXED,				Variable = ::OwnerDrawType::ODVARIABLE			};			//combo, list
			public enum class EComboType		{ Simple = ::ComboType::SIMPLE,			DropDown = ::ComboType::DROPDOWN,				DropDownList = ::ComboType::DROPDOWNLIST		};			//combo
			public enum class EVerticalAlign	{ Top = ::VerticalAlignment::VATOP,		Center = ::VerticalAlignment::VACENTER,			Bottom = ::VerticalAlignment::VABOTTOM			};		
			public enum class ESelectionType	{ Single = ::SelectionType::SINGLE,		None = ::SelectionType::NOSEL,					Multiple = ::SelectionType::MULTIPLESEL,		Extended = ::SelectionType::EXTENDEDSEL				}; //list
			public enum class EEtchedFrame		{ None = ::EtchedFrameType::EFNO,		All = ::EtchedFrameType::EFALL,					Horz = ::EtchedFrameType::EFHORZ,				Vert = ::EtchedFrameType::EFVERT			};
			public enum class EListCtrlViewMode { Icon = ::ListCtrlViewMode::LC_ICON,	SmallIcon = ::ListCtrlViewMode::LC_SMALLICON,	List = ::ListCtrlViewMode::LC_LIST,				Report = ::ListCtrlViewMode::LC_REPORT		};


			ref class BaseWindowWrapper;
			//--------------------------------------------------------------------------------
			public ref class MBaseWindowWrapperProperties : EasyBuilderComponentExtender
			{
			public:
				MBaseWindowWrapperProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);
			protected:
				property BaseWindowWrapper^ ExtendedWindow { BaseWindowWrapper^ get(); }
			};
			
			//--------------------------------------------------------------------------------
			public ref class MEditProperties : MBaseWindowWrapperProperties
			{
			public:
				MEditProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual bool CanExtendObject(IEasyBuilderComponentExtendable^ o) override { return true; }

				/// <summary>
				/// Gets or sets the property that automatically scrolls text to the right by 10 characters when the user types a character at the end of the line
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool AutoHScroll { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that automatically scrolls text up one page when the user presses the ENTER key on the last line
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool AutoVScroll { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Negates the default behavior that hides the selection when the control loses the input focus and inverts the selection when the control receives the input focus
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool NoHideSelection { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Displays an asterisk (*) for each character typed
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Password { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Prevents the user from typing or editing text
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool ReadOnly { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Converts all characters to uppercase as they are typed
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool UpperCase { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Converts all characters to lowercase as they are typed
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool LowerCase { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Allows only digits to be entered into the edit control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Number { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that a carriage return be inserted when the user presses the ENTER key while entering text into a multiline edit control in a dialog box
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				property bool WantReturn { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Designates a multiline edit control. The default is single-line edit control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool Multiline { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies text alignment
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property ETextAlign TextAlignment   { ETextAlign get(); void set(ETextAlign value);  }

			};

			//--------------------------------------------------------------------------------
			public ref class MComboBoxProperties : MBaseWindowWrapperProperties
			{
			public:
				MComboBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual bool CanExtendObject(IEasyBuilderComponentExtendable^ o) override { return true; }

				/// <summary>
				/// Gets or sets the property that Converts all characters to uppercase as they are typed
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool UpperCase { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Automatically sorts strings added to the list box
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				property bool Sort { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Converts text entered in the combo box edit control from the Windows character set to the OEM character set and then back to the Windows character set
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool OemConvert { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that the size of the combo box is exactly the size specified by the application when it created the combo box
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool NoIntegralHeight { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that automatically scrolls the text in an edit control to the right when the user types a character at the end of the line
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool AutoHScroll { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies the ComboBox style.
				/// SIMPLE: Displays the list box at all times.
				/// DROPDOWN: Similar to SIMPLE, except that the list box is not displayed unless the user selects an icon next to the edit control.
				/// DROPDOWNlIST: Similar to DROPDOWN, except that the edit control is replaced by a static text item that displays the current selection in the list box.
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property EComboType ComboType  { EComboType get(); void set(EComboType value);  }

				/// <summary>
				/// Gets or sets the property that Specifies the owner responsibilities and the list items height
				/// FIXED: Specifies that the owner of the list box is responsible for drawing its contents and that the items in the list box are all the same height.
				/// VARIABLE: Specifies that the owner of the list box is responsible for drawing its contents and that the items in the list box are variable in height.
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property EOwnerDrawType ComboOwnerDrawType   { EOwnerDrawType get(); void set(EOwnerDrawType value);  }




			};

			//--------------------------------------------------------------------------------
			public ref class MLabelProperties : MBaseWindowWrapperProperties
			{
				CFont* m_pCustomFont = NULL;
			public:
				MLabelProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);
				~MLabelProperties();
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual bool CanExtendObject(IEasyBuilderComponentExtendable^ o) override { return true; }

				/// <summary>
				/// Gets or sets the property indicating if the box with a frame is drawn with the same color as the screen background (desktop)	
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool GrayFrame { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the box with a frame is drawn in the same color as the window frames
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool BlackFrame { bool get(); void set(bool value);  }		
				
				/// <summary>
				/// Gets or sets the property indicating if if the box with a frame is drawn with the same color as the window background
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool WhiteFrame { bool get(); void set(bool value);  }
				
				/// <summary>
				/// Gets or sets the property indicating if the rectangle is filled with the current screen background color
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool GrayRect { bool get(); void set(bool value);  }
				
				/// <summary>
				/// Gets or sets the property indicating if the rectangle is filled with the current window background color
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool WhiteRect { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the sunken attribute for the current control
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Sunken { bool get(); void set(bool value); }			


				/// <summary>
				/// Gets or sets the frame or border drawing
				/// All : Draws the frame of the static control
				/// Horz : Draws the top and bottom edges of the static control
				/// Vert : Draws the left and right edges of the static control
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property EEtchedFrame EtchedFrame   { EEtchedFrame get(); void set(EEtchedFrame value);  }

				/// <summary>
				/// Gets or sets the property indicating if the rectangle is filled with the current window frame color
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool BlackRect { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the static control duplicates the text-displaying characteristics of a multiline edit control
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool EditControl { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the control is a simple rectangle and left-aligns the text in the rectangle. Tabs are expanded, but words are not wrapped
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool LeftNoWrap { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that prevents interpretation of any ampersand (&amp;) characters in the control's text as accelerator prefix characters
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool NoPrefix { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the owner of the static control is responsible for drawing the control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool OwnerDraw { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that replaces characters in the middle of the string with ellipses so that the result fits in the specified rectangle
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool PathEllipsis { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the simple rectangle displays a single line of left-aligned text in the rectangle
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Simple { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that allows an image displayed on a Label
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Bitmap { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the bitmap is centered in the static control that contains it
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool CenterImage { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the rectangle truncates any word that does not fit and adds ellipses
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool WordEllipsis { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that sends to the parent window CLICKED, DBLCLK, DISABLE, and ENABLE notification codes when the user clicks or double-clicks the control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Notify { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the end of a string does not fit in the rectangle, it is truncated and ellipses are added
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool EndEllipsis { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if the icon will be displayed in the dialog box	
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Icon { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the Font for the text of the control
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property  System::Drawing::Font^ Font {  virtual System::Drawing::Font^ get();  virtual void set(System::Drawing::Font^ value); }


				/// <summary>
				/// Gets or sets the property that Specifies text alignment
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property ETextAlign TextAlignment    { ETextAlign get(); void set(ETextAlign value);  }

			

			};

			//--------------------------------------------------------------------------------
			public ref class MListBoxProperties : MBaseWindowWrapperProperties
			{
			public:
				MListBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual bool CanExtendObject(IEasyBuilderComponentExtendable^ o) override { return true; }

				/// <summary>
				/// Gets or sets the property that Shows a disabled horizontal or vertical scroll bar when the list box does not contain enough items to scroll
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool DisableNoScroll { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Sorts strings in the list box alphabetically
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				property bool Sort { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that the owner of the list box receives WM_VKEYTOITEM messages whenever the user presses a key and the list box has the input focus
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				property bool WantKeyInput { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that a list box contains items consisting of strings
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool HasStrings { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that the size of the list box is exactly the size specified by the application when it created the list box
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool NoIntegralHeight { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies a multi-column list box that is scrolled horizontally
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool MultiColumn { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Causes the list box to send a notification code to the parent window whenever the user clicks a list box item (LBN_SELCHANGE), double-clicks an item (LBN_DBLCLK), or cancels the selection (LBN_SELCANCEL)
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Notify { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies the owner responsibilities and the list items height
				/// FIXED: Specifies that the owner of the list box is responsible for drawing its contents and that the items in the list box are the same height. 
				/// VARIABLE: Specifies that the owner of the list box is responsible for drawing its contents and that the items in the list box are variable in height
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property EOwnerDrawType OwnerDrawType     { EOwnerDrawType get(); void set(EOwnerDrawType value);  }

				/// <summary>
				/// Gets or sets the property that specifies the selection type
				/// NIGLE: Specifies that only one item in the list of values can be selected at a time.
				/// NOSEL: Specifies that the list box contains items that can be viewed but not selected. 
				/// MULTIPLESEL: Turns string selection on or off each time the user clicks or double - clicks a string in the list box. 
				/// EXTENDEDSEL: Allows multiple items to be selected by using the SHIFT key and the mouse or special key combinations
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property ESelectionType SelectionType     { ESelectionType get(); void set(ESelectionType value);  }

			};

			//--------------------------------------------------------------------------------
			public ref class MButtonProperties : MBaseWindowWrapperProperties
			{
			public:
				MButtonProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual bool CanExtendObject(IEasyBuilderComponentExtendable^ o) override { return true; }

				/// <summary>
				/// Gets or sets the property that Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool ClipChildren { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that the button displays a bitmap
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Bitmap { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that the button displays an icon
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Icon { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies that the button is two-dimensional; it does not use the default shading to create a 3-D image
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Flat { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property that Specifies text alignment
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property ETextAlign TextAlignment    { ETextAlign get(); void set(ETextAlign value);  }

				/// <summary>
				/// Gets or sets the property that Specifies vertical alignment
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property EVerticalAlign VerticalAlign     { EVerticalAlign get(); void set(EVerticalAlign value);  }

				/// <summary>
				/// Gets or sets the property that Wraps the button text to multiple lines if the text string is too long to fit on a single line in the button rectangle
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool Multiline { bool get(); void set(bool value);  }

			};

			//--------------------------------------------------------------------------------
			public ref class MCheckBoxProperties : MButtonProperties
			{
			public:
				MCheckBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

				/// <summary>
				/// Gets or sets the property that Places text on the left side of the radio button or check box when combined with a radio button or check box style
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool LabelOnLeft  { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or Sets the property indicating if the control is an automatic check box (AUTOCHECKBOX) or a normal one (CHECKBOX)
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Auto { bool get(); void set(bool value);  }

			};

			//--------------------------------------------------------------------------------
			public ref class MRadioButtonProperties : MButtonProperties
			{
			public:
				MRadioButtonProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

				/// <summary>
				/// Gets or sets the property that Places text on the left side of the radio button or check box when combined with a radio button or check box style
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool LabelOnLeft  { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or Sets the property indicating if the control is an automatic radio button (AUTORADIOBUTTON) or a normal one (RADIOBUTTON)
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Auto { bool get(); void set(bool value);  }
			};

			//--------------------------------------------------------------------------------
			public ref class MGroupBoxProperties : MButtonProperties
			{
			public:
				MGroupBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);

			};

			//--------------------------------------------------------------------------------
			public ref class MPushButtonProperties : MButtonProperties
			{
			public:
				MPushButtonProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);
				/// <summary>
				/// Gets or sets the property indicating if the control is a default push button (DEFPUSHBUTTON) or not (PUSHBUTTON)
				/// DEFPUSHBUTTON: Creates a push button that behaves like a BS_PUSHBUTTON style button, but has a distinct appearance. PUSHBUTTON: Creates a push button that posts a WM_COMMAND message to the owner window when the user selects the button
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool Default { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or sets the property indicating if will be created an owner-drawn button
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool OwnerDraw { bool get(); void set(bool value);  }

			};
		}
	}
}