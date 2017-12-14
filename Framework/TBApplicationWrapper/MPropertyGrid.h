#pragma once


#include <TbGenlib\TBPropertyGrid.h>

#include "MParsedControls.h"

using namespace System::Collections::ObjectModel;
using namespace System::Collections::Specialized;

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			//================================================================================
			ref class MPropertyGrid;
			public ref class MPropertyItem : public EasyBuilderComponent, IControlClassConsumer
			{		
			internal:
				CTBProperty* m_pProperty = NULL;
				CTBPropertyGrid* m_pGrid = NULL;
				System::String^ id;
				ObservableCollection<MPropertyItem^>^ items = gcnew ObservableCollection<MPropertyItem^>;
			public:
				MPropertyItem();
				MPropertyItem(CTBProperty* pProperty, CTBPropertyGrid* pGrid);
				~MPropertyItem();
				!MPropertyItem();

				virtual System::String^ ToString() override { return Name; }

				
				/// <summary>
				/// Gets the identifier of the current column
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Id { System::String^ get() {return id;}  void set(System::String^ value) {	id = value;} }

				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ value) override; }
				
				/// <summary>
				/// Expression used to decide if this column has to be created; may be a logical combination (even using parenthesis) of atoms similar to:
				/// APP.MODULE (to test the activation)
				/// VARIABLE_NAME (test the state of a boolean variable in the document)
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Activation { virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Gets the text of the item
				/// </summary>
				[Browsable(true), LocalizedCategory("InformationsCategory", EBCategories::typeid), 
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Text {  System::String^ get();    void set(System::String^ value); }
				
				/// <summary>
				/// Gets or sets the hint text for this item
				/// </summary>
				[Browsable(true), LocalizedCategory("InformationsCategory", EBCategories::typeid), 
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Hint {  System::String^ get();  void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the class type for the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), 
					System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid), 
					TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), 
					System::ComponentModel::EditorAttribute(ControlClassUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid), 
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IControlClass^ ClassType { virtual IControlClass^ get(); virtual void set(IControlClass^ value); }

				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.DataSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid),
					TBPropertyFilter(TBPropertyFilters::DesignerStatic)	]
				property  System::String^ DataSource {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Gets or sets the hotlink name for the current control
				/// </summary>
				[System::ComponentModel::Browsable(true),
					TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic),
					LocalizedCategory("DataBindingCategory", EBCategories::typeid)]
				property System::String^ HotLinkName { System::String^ get();  void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.HotlinkUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ HotLinkNs {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool HasFamilyClassChangeable { virtual bool get() { return true; } }

				/// <summary>
				/// Gets or sets the property indicating if the tile is collapsed when created
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property bool Collapsed { bool get(); 	void set(bool value); }

				/// <summary>
				/// Gets or sets''' the property that automatically scrolls text up one page when the user presses the ENTER key on the last line
				/// </summary>
				[LocalizedCategory("Appearance", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool AutoVScroll { bool get(); void set(bool value);  }

				/// <summary>
				/// Gets or Sets the Sort attribute for the current column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property bool Sort { bool get(); void set(bool value); }

				/// <summary>
				/// Gets or Sets the Chars attribute for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic | TBPropertyFilters::ComponentState)]
				property int Chars { virtual int get(); void set(int value); }


				/// <summary>
				/// Gets the property items
				/// </summary> 
				[Browsable(true), LocalizedCategory("InformationsCategory", EBCategories::typeid), 
					EditorAttribute("Microarea.EasyBuilder.UI.EasyBuilderComponentCollectionEditor, Microarea.EasyBuilder", UITypeEditor::typeid),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property ObservableCollection<MPropertyItem^>^ Items
				{
					ObservableCollection<MPropertyItem^>^  get();
				}

				/// <summary>
				/// Gets or sets the item source name for the current control
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::DesignerStatic), LocalizedCategory("DataBindingCategory", EBCategories::typeid)]
				property System::String^ ItemSourceName { System::String^ get();  void set(System::String^ value); }
				
				/// <summary>
				/// Gets or Sets the item source namespace for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.ItemSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ ItemSourceNs {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;

			internal:
				void CreateItem(EasyBuilderComponent^ parent);

			private:
				CWndPropertyGridItemDescription* GetWndObjDescription();
				void OnCollectionChanged(Object^ sender, NotifyCollectionChangedEventArgs^ args);
			};

			//================================================================================
			public ref class MPropertyGrid : public BaseWindowWrapper
			{

				CTBPropertyGrid* m_pGrid = NULL;
				ObservableCollection<MPropertyItem^>^ items = gcnew ObservableCollection<MPropertyItem^>;
				List<System::String^>^ itemsId = gcnew List<System::String^>();
			public:
				/// <summary>
				/// Constructor:
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MPropertyGrid(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MPropertyGrid(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				virtual ~MPropertyGrid();
				!MPropertyGrid();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;

				/// <summary>
				/// Gets the identifier of the current column
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Id { 
					virtual System::String^ get() override { return __super::Id; }  virtual void set(System::String^ value) override {__super::Id = value;} }

				/// <summary>
				/// Gets the identifier of the current control
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { 
					virtual System::String^ get() override;  virtual void set(System::String^ value) override;  }
			
				/// <summary>
				///  Gets or Sets the anchor criteria for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::String^ Anchor { virtual System::String^ get() override { return ""; } virtual void set(System::String^ value) override {}}

				/// <summary>
				/// Gets the property items
				/// </summary>
				[Browsable(true), LocalizedCategory("InformationsCategory", EBCategories::typeid), 
					EditorAttribute("Microarea.EasyBuilder.UI.EasyBuilderComponentCollectionEditor, Microarea.EasyBuilder", UITypeEditor::typeid),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property ObservableCollection<MPropertyItem^>^ Items
				{
					ObservableCollection<MPropertyItem^>^  get();
				}
			private:

				void OnCollectionChanged(Object^ sender, NotifyCollectionChangedEventArgs^ args);
				void UpdateAllItems(CWndObjDescription* pParentDescr);
				void UpdateItemsOrder(CWndObjDescription* pParentDescr);
				void CalcItemsIds(ObservableCollection<MPropertyItem^>^ propItems);
			public:
				void UpdateAllPropertyGrid();

			};

		}
	}
}