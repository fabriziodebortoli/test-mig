#pragma once
#include "MParsedControls.h"
#include "WindowWrapperContainer.h"
using namespace Microarea::Framework::TBApplicationWrapper;

namespace Microarea {
	namespace Framework	{
		namespace TBApplicationWrapper
		{
			ref class MAccelerator;
			public ref class MPanel : WindowWrapperContainer
			{
				NameSpace^	nameSpace;
			public:
				/// <summary>
				/// Constructor: 
				/// </summary>
				MPanel(System::IntPtr wrappedObject);
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an original taskbuilder object</param>
				MPanel(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, bool hasCodeBehind);

				//----------------------------------------------------------------------------
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				~MPanel();

				//----------------------------------------------------------------------------
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				!MPanel();

				/// <summary>
				/// true if the window has a thin-line border
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Border {
						virtual bool get() override;
						virtual void set(bool value) override;
				}

				/// <summary>
				/// true if the window is a child window
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Child {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// Creates a dialog box with a modal dialog-box frame that can be combined with a title bar and window menu
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool ModalFrame {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// Centers the dialog box in the working area of the monitor that contains the owner window
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool Center {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// Centers the dialog box on the mouse cursor
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool CenterMouse {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// Creates a dialog box that works well as a child window of another dialog box, much like a page in a property sheet
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool UserControl {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// The window has a title bar
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Caption {
					virtual bool get() ;
					virtual void set(bool value) ;
				}
				/// <summary>
				/// The window has a window menu on its title bar
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool SystemMenu {
					virtual bool get() ;
					virtual void set(bool value) ;
				}
				/// <summary>
				/// Excludes the area occupied by child windows when drawing occurs within the parent window
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property bool ClipChildren {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool ClipSiblings {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool DialogFrame {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// The window has a maximize button
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool MaximizeBox {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// The window has a minimize button
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool MinimizeBox {
					virtual bool get();
					virtual void set(bool value);
				}

				/// <summary>
				/// The window is an overlapped window. An overlapped window has a title bar and a border
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Overlapped {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// The window has a sizing border
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool ThickFrame {
					virtual bool get() ;
					virtual void set(bool value) ;
				}

				/// <summary>
				/// Gets or the namespace of the current control
				/// </summary>
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property INameSpace^	Namespace			{ virtual INameSpace^ get() override { return nameSpace; } }

				/// <summary>
				/// Sets the Parent of the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property IWindowWrapperContainer^ Parent { virtual void set(IWindowWrapperContainer^ value) override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;
			
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;
				private:
				void InitializeName(IWindowWrapperContainer^ parent);	
			};
			
			public enum class EPanelType
			{
				Panel = CWndObjDescription::Panel,
				Tile = CWndObjDescription::Tile
			};

			public enum class ETileDialogStyle { 
				None =		TDS_NONE, 
				Normal =	TDS_NORMAL, 
				Filter =	TDS_FILTER, 
				Header =	TDS_HEADER, 
				Footer =	TDS_FOOTER, 
				Wizard =	TDS_WIZARD, 
				Parameters = TDS_PARAMETERS
			}; 

		/*	public enum class ETileDialogSize { 
				Mini =		TILE_MINI, 
				Standard =	TILE_STANDARD, 
				Wide =		TILE_WIDE, 
				AutoFill =	TILE_AUTOFILL
			};*/

			///classe utilizzata per il designer json; alcuni comportamenti sono diversi perché non siamo a runtime
			public ref class MEasyStudioPanel : MPanel
			{
			public:
				/// <summary>
				/// Constructor: 
				/// </summary>
				MEasyStudioPanel(System::IntPtr wrappedObject);

				/// <summary>
				/// Gets or sets the property indicating if the window is a child window
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				property bool Child  { virtual bool get() override; virtual void set(bool value)override; }
			
				/// <summary>
				/// Gets or sets the property indicating if the tile has a title
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property EBool HasTitle  { EBool get(); 	void set(EBool value); }
				
				/// <summary>
				/// Gets or Sets the tile dialog style
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property ETileDialogStyle TileDialogStyle  { ETileDialogStyle get(); void set(ETileDialogStyle value);  }

				/// <summary>
				/// Gets or Sets the tile dialog size
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property ETileDialogSize TileDialogSize  { ETileDialogSize get(); void set(ETileDialogSize value);  }

				/// <summary>
				/// Gets or Sets the property indicating indicates the amount of space this component will take up in its parent container
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property int Flex { int get() override; void set(int value) override; }
				
				/// <summary>
				/// Gets or Sets the property indicating indicates the left margin of controls with 'COL2' Anchor
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property int Col2Margin { int get(); void set(int value); }

				/// <summary>
				/// Gets or sets the property indicating if the tile is collapsible
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property EBool Collapsible { EBool get(); void set(EBool value); }

				/// <summary>
				/// Gets or sets the property indicating if the tile is collapsed when created
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property bool Collapsed { bool get(); 	void set(bool value); }

				/// <summary>
				/// Gets or sets the property indicating if the tile is pinnable
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool Pinnable  { EBool get(); void set(EBool value); }

				/// <summary>
				/// Gets or sets the property indicating if the tile is pinned when created
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool Pinned  { bool get(); 	void set(bool value); }

				/// <summary>
				/// Gets or sets the property indicating if the tile rearrange its content when stretched
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool WrapTileParts { bool get(); 	void set(bool value); }

				/// <summary>
				/// true if the tile creates automatically its static areas
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool HasStaticArea { bool get(); 	void set(bool value); }

				/// <summary>
				/// Gets or Sets the location for the current control, in logical units
				/// </summary>
				[DisplayName("Location"), System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Drawing::Point LocationLU { virtual System::Drawing::Point get() override; virtual void set(System::Drawing::Point value) override; }

				/// <summary>
				/// Gets or Sets the name of the Tabber
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::ReadOnly(true), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Id { virtual System::String^ get() override;  virtual void set(System::String^ value) override; }

				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { virtual System::String^ get() override;  virtual void set(System::String^ value) override;  }

				/// <summary>
				/// Gets or sets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property EPanelType PanelType { EPanelType get();  void set(EPanelType value);  }

				/// <summary>
				/// Internal use 
				/// </summary>
				[Browsable(false)]
				property  bool CanBeDeleted { virtual bool get() override { return false; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property EditingMode DesignerMovable { virtual EditingMode get() override; }

				/// <summary>
				/// Gets or sets the property indicating the key accelerators for this form
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid), System::ComponentModel::Browsable(true)]
				property cli::array<MAccelerator^>^ Accelerators { cli::array<MAccelerator^>^ get(); void set(cli::array<MAccelerator^>^); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;


				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedObject) override;
			};
		}
	}
}