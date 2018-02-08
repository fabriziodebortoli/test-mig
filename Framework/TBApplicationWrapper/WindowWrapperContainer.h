#pragma once
#include "MParsedControls.h"
#include "MLayoutContainer.h"
using namespace Microarea::Framework::TBApplicationWrapper;

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			/// <summary>
			/// Derived from BaseWindowWrapper, is the root class for all CWnd wrappers that hosts other windows
			/// </summary>
			[ExcludeFromIntellisense]
			[System::ComponentModel::Design::Serialization::DesignerSerializer(EasyBuilderControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			//=============================================================================
			public ref class WindowWrapperContainer : BaseWindowWrapper, IWindowWrapperContainer
			{
			protected:
				System::Collections::Generic::List<System::ComponentModel::IComponent^>^	components;

				ELayoutAlign	layoutAlignment;	// properties of Layout Element

				int				flex;				//
				int				maxWidth;			//common properties of tiles and groups 	
				bool			collapsed;			//

			private:
				int						lastEditDPI;

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				WindowWrapperContainer(System::IntPtr wrapperObject);
				/// <summary>
				/// Constructor
				/// </summary>
				WindowWrapperContainer(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				~WindowWrapperContainer();

				/// <summary>
				/// Finalizer
				/// </summary>
				!WindowWrapperContainer();

				/// <summary>
				/// Gets the name of the window wrapper container
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
				property System::String^ Name { virtual System::String^ get() override; }

				/// <summary>
				/// Gets the namespace of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
				property INameSpace^ Namespace { virtual INameSpace^ get() override; }

				/// <summary>
				/// Event raised when the position of the scroll is changed
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				virtual event System::EventHandler<EasyBuilderEventArgs^>^ ScrollChanged;

				/// <summary>
				/// Event raised when the data loading occurs (event linked to client doc "OnPrepareAuxData")
				/// </summary>
				[LocalizedCategory("DataCategory", EBCategories::typeid)]
				virtual event System::EventHandler<EasyBuilderEventArgs^>^ DataLoaded;

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::ComponentModel::ComponentCollection^ Components { virtual  System::ComponentModel::ComponentCollection^ get() { return gcnew System::ComponentModel::ComponentCollection(components->ToArray()); } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::IntPtr	DocumentPtr { virtual  System::IntPtr get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool HorizontalScrollBar { bool get() { return HasStyle(WS_HSCROLL); }}

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool VerticalScrollBar { bool get() { return HasStyle(WS_VSCROLL); }}


				/// <summary>
				/// Gets or Sets the property indicating indicates the amount of space this component will take up in its parent container
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property int Flex {
					virtual int get() { return flex; }
					void virtual set(int value) { flex = value; }
				}

				/// <summary>
				/// Gets the MinSize for the current control, in Logical Units
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::Browsable(false)]
				property virtual System::Drawing::Size MinSize  { System::Drawing::Size get() { return __super::minSize; }  }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool HasOwnEditing { virtual bool get() { return false; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual void AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing) { }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual void AfterSelectionChanged(bool IAmSelected) { }

				/// <summary>
				/// Gets or Sets the anchor criteria for the current control
				/// </summary>
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), Browsable(false)]
				property System::String^ Anchor 
				{
					virtual System::String^ get() override { return __super::get(); }
					virtual void set(System::String^ value) override { __super::set(value); } }

				/// <summary>
				/// Internal Use
				/// </summary>
				MESSAGE_HANDLER_EVENT(SuspendLayout, EasyBuilderEventArgs, "Occurs when layout is suspended.");

				/// <summary>
				/// Internal Use
				/// </summary>
				MESSAGE_HANDLER_EVENT(ResumeLayout, EasyBuilderEventArgs, "Occurs when layout is resumed.");

			public:
				///<summary>
				///Generate json for children
				///</summary>
				virtual void GenerateJsonForChildren(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization) override;

				/// <summary>
				/// Method called when the data loading occurs (event linked to client doc "OnPrepareAuxData")
				/// </summary>
				void OnDataLoaded();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual HWND GetControlHandle(const CTBNamespace& nameSpace);

								/// <summary>
				/// Returns the control associated to the given namespace if exist, NULL otherwise
				/// </summary>
				/// <param name="nameSpace">the namespace of the control to find</param>
				virtual IWindowWrapper^	GetControl(INameSpace^ nameSpace);

				/// <summary>
				/// Returns the control associated to the given name if existing, NULL otherwise
				/// </summary>
				/// <param name="name">the name of the control to find</param>
				virtual IWindowWrapper^	GetControl(System::String^ name);

				/// <summary>
				/// Returns the controls associated to the given id or name 
				/// </summary>
				List<BaseWindowWrapper^>^ GetChildrenByIdOrName(System::String^ id, System::String^ name);

				/// <summary>
				/// Returns true if the wrapper container contains a control that match the given handle
				/// </summary>
				/// <param name="handle">the handle of the control to find</param>
				virtual bool HasControl(System::IntPtr handle);

				/// <summary>
				/// Returns the control associated to the given handle if exist, NULL otherwise
				/// </summary>
				/// <param name="handle">the handle of the control to find</param>
				virtual IWindowWrapper^	GetControl(System::IntPtr handle);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component, bool isChanged);

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
				virtual void RemoveAll();
				
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void CreateComponents() {/*does nothing*/ }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnAfterCreateComponents() {/*does nothing*/ }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void CallCreateComponents();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanCallCreateComponents() { return true; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ApplyResources() {/*does nothing*/ }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void RegisterEventHandlers() {/*does nothing*/ }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual int GetNamespaceType() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ClearComponents();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void SwitchVisibility(bool bVisible) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void SaveCurrentStatus(IDesignerCurrentStatus^ status) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ApplyCurrentStatus(IDesignerCurrentStatus^ status) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedObject) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnScrollChanged(WindowMessageEventArgs^ e);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool HasComponent(System::String^ controlName) { return EasyBuilderComponent::HasComponent(Components, controlName); }
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::ComponentModel::IComponent^ GetComponent(System::String^ controlName) { return EasyBuilderComponent::GetComponent(Components, controlName); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void GetChildrenFromPos(System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::IntPtr GetChildFromOriginalPos(System::Drawing::Point clientPosition, System::String^ controlClass) { return System::IntPtr::Zero; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::IntPtr GetChildFromCtrlID(UINT nID);

				/// <summary>
				/// Returns the scroll position
				/// </summary>
				virtual System::Drawing::Point GetScrollPosition();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void SaveChildOriginalPos(System::IntPtr hwndChild, System::Drawing::Point clientPosition) {/*does nothing*/ }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnBuildingSecurityTree(System::IntPtr tree, System::IntPtr infoTreeItems) override;

				/// <summary>
				/// Converts a point in pixels to logical units
				/// </summary>
				void ToLogicalUnits(Point% px);
				/// <summary>
				/// Converts a size in pixels to logical units
				/// </summary>
				void ToLogicalUnits(System::Drawing::Size% sz);

				/// <summary>
				/// Converts a point in logical units to pixels
				/// </summary>
				void ToPixels(Point% lu);
				/// <summary>
				/// Converts a size in logical units to pixels
				/// </summary>
				void ToPixels(System::Drawing::Size% lu);

				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false)]
				property int			LastEditDPI { int get(); void set(int value); }

				static int				GetCurrentLogPixels();
				virtual void IntegrateLayout(ILayoutComponent^ layoutObject) {}
			

			private:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				void SyncHotLinks();
			};
		}
	}
}