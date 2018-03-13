#pragma once

#include "MParsedControls.h"
class ColumnInfo;
class CBodyEdit;
class CWndBodyColumnDescription;

namespace Microarea {
	namespace Framework	{
		namespace TBApplicationWrapper
		{
			ref class MBodyEdit;
			ref class MBodyEditColumn;
			ref class MParsedControl;

			//================================================================================
			/// <summary>
			/// Internal use: serializes a body edit
			/// </summary>
			public ref class BodyEditSerializer : EasyBuilderControlSerializer
			{
			public:
				/// <summary>
				/// Internal use
				/// </summary>
				virtual Statement^ GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;

				/// <summary>
				/// Internal use
				/// </summary>
				virtual TypeDeclaration^ SerializeClass(SyntaxTree^ syntaxTree, IComponent^ component) override;

			};

			//================================================================================
			/// <summary>
			/// Internal use: serializes a body edit column
			/// </summary>
			public ref class BodyEditColumnSerializer : EasyBuilderControlSerializer
			{
			public:
				/// <summary>
				/// Internal use
				/// </summary>
				virtual Statement^ GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;
			};

			/// <summary>
			/// Class for managing the columns of the Grid control
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(BodyEditColumnSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid), ExcludeFromIntellisense]
			[System::ComponentModel::PropertyTabAttribute(System::Windows::Forms::Design::EventsTab::typeid, System::ComponentModel::PropertyTabScope::Component)]
			[EasyBuilderComponentExtender(MStateObject::typeid, "StateButton")]
			[EasyBuilderComponentExtender(MFileItemsSource::typeid, "FileItemsSource")]
			public ref class MBodyEditColumn : EasyBuilderControl, IDataBindingConsumer, IControlClassConsumer, IItemsSourceConsumer, IEasyBuilderComponentExtendable, IWindowWrapper
			{
			private:
				ColumnInfo*		m_pColumnInfo;
				MParsedControl^	control;
				MBodyEdit^		bodyEdit;
				
				static COLORREF DefaultColor = (0xFF << 24);

			public:

				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="colInfoPtr">is the c++ handle of the column which the control refers to</param>
				MBodyEditColumn(System::IntPtr colInfoPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent bodyedit</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MBodyEditColumn(MBodyEdit^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Constructor: 
				/// </summary>
				MBodyEditColumn(MBodyEdit^ parentWindow, System::Type^ controlType);

				/// <summary>
				/// Destructor
				/// </summary>
				~MBodyEditColumn();

				/// <summary>
				/// Finalizer
				/// </summary>
				!MBodyEditColumn();

			public:

				///<summary>
				///Updates nedeed attributes for json serialization
				///</summary>
				CWndObjDescription * UpdateAttrForJson(CWndBodyColumnDescription* pColumnDescription);

				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ SerializedPropertyAccessorName { virtual System::String^ get() override { return SerializedName; } }

				/// <summary>
				/// Gets the namespace for the current column
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), 
					System::ComponentModel::ReadOnly(true)]
				property INameSpace^ Namespace { virtual INameSpace^ get() override; }

				/// <summary>
				/// Gets or Sets the name for the current column
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

				/// <summary>
				/// Gets the identifier of the current column
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Id { virtual System::String^ get() override;  virtual void set(System::String^ value);  }

				/// <summary>
				/// Expression used to decide if this column has to be created; may be a logical combination (even using parenthesis) of atoms similar to:
				/// APP.MODULE (to test the activation)
				/// VARIABLE_NAME (test the state of a boolean variable in the document)
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property System::String^ Activation { virtual System::String^ get();  virtual void set(System::String^ value);  }

				/// <summary>
				/// Gets or Sets the title for the current column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
				property System::String^ ColumnTitle { virtual System::String^ get(); virtual void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the Visible attribute for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property bool Visible { virtual bool get() override; virtual void set(bool visible) override; }

				/// <summary>
				/// Gets or Sets the class type for the current column
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::EditorAttribute(ControlClassUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
				property IControlClass^ ClassType { virtual IControlClass^ get(); virtual void set(IControlClass^ value); }

				/// <summary>
				/// Gets or Sets the data binding for the current column
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property  IDataBinding^	DataBinding { virtual  IDataBinding^ get(); virtual void set(IDataBinding^ dataBinding); }
				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.DataSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ DataSource {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Gets or Sets the back color for the title of the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Color TitleBackColor { virtual System::Drawing::Color get(); virtual void set(System::Drawing::Color value); }

				/// <summary>
				/// Gets or Sets the fore color for the title of the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::Drawing::Color TitleTextColor { virtual System::Drawing::Color get(); virtual void set(System::Drawing::Color value); }

				/// <summary>
				/// Gets or Sets the Locked attribute for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
				property bool Locked { bool get(); void set(bool value); }

				/// <summary>
				/// Gets or Sets the number of decimals for the current column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::RealTypes)]
				property int NumberOfDecimal { virtual int get(); virtual void set(int nDec); }

				/// <summary>
				/// Gets or Sets the size  for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::DesignerRuntime)]
				property System::Drawing::Size SizeInChars { virtual System::Drawing::Size get(); void set(System::Drawing::Size value); }

				/// <summary>
				/// Gets or Sets the formatter for the current column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::EditorAttribute(FormatterUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property FormatterStyle^ Formatter { FormatterStyle^ get(); void set(FormatterStyle^ value); }

				/// <summary>
				/// Gets or Sets the Tooltip for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property System::String^ Tooltip { virtual System::String^ get(); virtual void set(System::String^ tooltip); }

				/// <summary>
				/// Gets the class name for the current column
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ ClassName { virtual System::String^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::IntPtr ColumnInfoPtr { virtual System::IntPtr get(); }

				/// <summary>
				/// Gets or Sets the Width for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property int Width { virtual int get(); void set(int value); }

				/// <summary>
				/// Gets or sets the hotlink for the current column
				/// </summary>
				[System::ComponentModel::Browsable(true), LocalizedCategory("DataBindingCategory", EBCategories::typeid), ObjectReference, TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime), System::ComponentModel::EditorAttribute(HotLinkUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				property MHotLink^ HotLink { MHotLink^ get(); void set(MHotLink^ value); }
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
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property IDataType^ CompatibleType { virtual IDataType^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataType^ FilteredDataType { virtual IDataType^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataType^ CompatibleDataType { virtual IDataType^ get() { return CompatibleType; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool HasFamilyClassChangeable { virtual bool get() { return true; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property bool CanAutoFillFromDataBinding { virtual bool get() { return false; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime), ExcludeFromIntellisense, LocalizedCategory("InformationsCategory", EBCategories::typeid)]
				property int ColPos { int get(); void set(int value); }

				/// <summary>
				/// Gets or Sets the Chars attribute for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
				property int Chars { virtual int get(); void set(int value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				virtual property System::String^ SerializedName { virtual System::String^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::Type^ ExcludedBindParentType { virtual System::Type^ get() { return nullptr; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property IDataManager^ FixedDataManager { virtual IDataManager^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property EditingMode DesignerMovable { virtual EditingMode get() override { return EditingMode::ResizingMidRight; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property MBodyEdit^ BodyEdit { MBodyEdit^ get(); void set(MBodyEdit^ value); }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsGrayed { bool get(); void set(bool value); }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsHidden { bool get(); void set(bool value); }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsNoChange_Grayed { bool get(); void set(bool value); }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsNoChange_Hidden { bool get(); void set(bool value); }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsLocked { bool get(); void set(bool value); }
				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsSortedDes { bool get(); void set(bool value); }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BEColumnStatusCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property bool IsSortedAsc { bool get(); void set(bool value); }

				/// <summary>
				/// Gets or sets the index in the tab order of this control
				/// </summary>
				[DisplayName("ColPos"), LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property int TabOrder { virtual int get() override; virtual void set(int value) override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::ComponentModel::ISite^ Site { virtual System::ComponentModel::ISite^ get() override; virtual void set(System::ComponentModel::ISite^ tooltip) override; }

				/// <summary>
				/// true if this control can be activated pressing the tab key
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::Browsable(false)]
				property bool TabStop { virtual bool get() override; }

				/// <summary>
				/// Gets or Sets the Footer attribute for the current column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool ColumnHasFooter { bool get(); void set(bool value); }

				/// <summary>
				/// If true, the user can enlarge this column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool AllowEnlarge { bool get(); void set(bool value); }

				/// <summary>
				/// Gets or Sets the Sort attribute for the current column
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
				property bool Sort { bool get(); void set(bool value); }

				/// <summary>
				/// Allows to show or hide the hotlink button for the current control
				/// </summary>
				[System::ComponentModel::Browsable(true), LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
				property bool ShowHotLinkButton { virtual bool get();  virtual void set(bool value); }

				/// <summary>
				/// Gets or sets the item collection associated to the combo
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				[System::ComponentModel::EditorAttribute(ItemSourcesUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Content)]
				[TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)]
				property System::Collections::IList^	ItemsSource { virtual System::Collections::IList^ get(); virtual void set(System::Collections::IList^ value); }

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

				[System::ComponentModel::Browsable(false)]
				property bool	IsItemsSourceEditable { virtual bool get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::ComponentModel::ComponentCollection^ Components { virtual  System::ComponentModel::ComponentCollection^ get(); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property IEasyBuilderComponentExtenders^ Extensions { virtual  IEasyBuilderComponentExtenders^ get(); }
				
		
				/// <summary>
				/// Gets control extended styles
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				property MParsedControl^ Control { MParsedControl^ get(); void set(MParsedControl^ value); }

			private:
				property bool	HasItemsSource { bool get(); }

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanChangeProperty(System::String^ propertyName) override;

				/// <summary>
				/// Event Raised after a change has occurred to the value of the dataobj
				/// </summary>
				virtual event System::EventHandler<EasyBuilderEventArgs^>^ ValueChanged;

				/// <summary>
				/// OnValueChanged
				/// </summary>
				virtual void OnValueChanged(Object^ sender, EasyBuilderEventArgs^ args);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component) { };

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component, System::String^ name) { };

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component, bool isChanged) { };

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Remove(System::ComponentModel::IComponent^ component) { };

			public:
				/// <summary>
				/// Internal Use
				/// </summary> 
				[ExcludeFromIntellisense]
				static System::String^	GetSerializedName(System::String^ columnName);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void AutoFillFromDataBinding(IDataBinding^ dataBinding, bool overrideExisting) {}

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void RefreshContentByDataType();

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void AddOrRemoveStatus(int status, bool value);


			public:

				[System::ComponentModel::Browsable(false)]
				property System::IntPtr Handle {
					virtual System::IntPtr get();
				}
				[System::ComponentModel::Browsable(false)]
				property System::Drawing::Point Location
				{
					virtual System::Drawing::Point get();
					virtual void set(System::Drawing::Point value);
				}
				
				[System::ComponentModel::Browsable(false)]
				property System::Drawing::Size Size
				{
					virtual System::Drawing::Size get();
					virtual void set(System::Drawing::Size value);
				}
				[System::ComponentModel::Browsable(false)]
				property System::Drawing::Rectangle Rectangle
				{
					virtual System::Drawing::Rectangle get();					
				}
				
				/// <summary>
				/// Activates the control
				/// </summary>
				virtual void Activate() override;

				virtual void UpdateWindow(void) 
				{
				}
				virtual void Invalidate(void)  
				{
				}
				virtual void Invalidate(System::Drawing::Rectangle)  
				{
				}
				virtual void ScreenToClient(System::Drawing::Rectangle %)  
				{
				}
				virtual void ClientToScreen(System::Drawing::Rectangle %) 
				{
				}
				virtual void ScreenToClient(System::Drawing::Point %)  
				{
				}
				virtual void ClientToScreen(System::Drawing::Point %)  
				{
				}
				virtual System::IntPtr SendMessage(int,System::IntPtr,System::IntPtr)  
				{
					return System::IntPtr::Zero;
				}
#pragma push_macro("PostMessage")
#undef PostMessage
				virtual bool PostMessage(int,System::IntPtr,System::IntPtr)   
#pragma pop_macro("PostMessage")
				{
					return false;
				}
				virtual System::IntPtr GetWndPtr(void)   
				{
					return System::IntPtr::Zero;
				}

				void		NotifyBodyEditChanged();
				ColumnInfo* GetColumnInfo() { return m_pColumnInfo; }
				CWndBodyColumnDescription* GetWndObjDescription();

			private:
				void InitializeDefaultControlClass(IDataType^ type);
				void InitializeDefaultFormatter(IDataType^ type);
				void SetControl(MParsedControl^ control);
			};

			
			/// <summary>
			/// Class for managing the Editable columns of the Grid control
			/// </summary>
			//================================================================================
			[ExcludeFromIntellisense]
			[System::ComponentModel::Design::Serialization::DesignerSerializer(BodyEditSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			public ref class MBodyEdit : BaseWindowWrapper, IEasyBuilderContainer, IDataBindingConsumer, IDesignerTarget
			{
			private:
				CBodyEdit*				m_pBodyEdit;
				IDataBinding^			dataBinding;
				System::Collections::Generic::List<MBodyEditColumn^>^	components;
				bool					isSerialized;
				
			public:
				///<summary>
				///Updates needed attributes for json serialization 
				///</summary>
				virtual void UpdateAttributesForJson(CWndObjDescription* pParentDescription) override;

				///<summary>
				///Generate json for children
				///</summary>
				virtual void GenerateJsonForChildren(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization) override;

				///<summary>
				///Generates serialization for the class
				///</summary>
				virtual void GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization) override;

				/// <summary>
				/// Enables or disables the insertion of new rows
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool EnableInsertRow { bool get(); void set(bool value); }

				/// <summary>
				/// Enables or disables the addition of new rows
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool EnableAddRow { bool get(); void set(bool value); }

				/// <summary>
				/// Enables or disables the deletion of rows
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool EnableDeleteRow { bool get(); void set(bool value); }

				/// <summary>
				/// Enables or disables the opening of row form view
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property bool EnableFormViewCall { bool get(); void set(bool value); }

				/// <summary>
				/// Gets the namespace for the bodyedit
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property INameSpace^ Namespace { virtual INameSpace^ get() override; }

				/// <summary>
				/// Gets or Sets the name for the bodyedit
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

				/// <summary>
				/// Gets or Sets the text for the bodyedit
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), System::ComponentModel::Localizable(true)]
				property System::String^ Text { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

				/// <summary>
				/// Gets  the class type for the bodyedit
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::EditorAttribute(ControlClassUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property ControlClass^ ClassType { virtual ControlClass^ get(); }

				/// <summary>
				/// Gets or Sets the BottomStretch property for the bodyedit
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)
				]
				property bool BottomStretch { virtual bool get() override; virtual void set(bool value) override; }

				/// <summary>
				/// Gets or Sets the RightStretch property for the bodyedit
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
					TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)
				]
				property bool RightStretch { virtual bool get() override; virtual void set(bool value) override; }

				/// <summary>
				/// Gets or Sets the AutoFill property for the bodyedit
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid),
				TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)
				]
				property bool AutoFill { virtual bool get() override; virtual void set(bool value) override; }

				/// <summary>
				/// Gets or Sets the Data binding for the bodyedit
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved| TBPropertyFilters::DesignerRuntime), 
					System::ComponentModel::EditorAttribute(DBTOneToManyUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid), 
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property  IDataBinding^	DataBinding { virtual IDataBinding^ get(); virtual void set(IDataBinding^ dataBinding); }

				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.DataSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ DataSource {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Gets the class name for the bodyedit
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::String^ ClassName { virtual System::String^ get() override; }

				/// <summary>
				///  Gets or Sets the anchor criteria for the current control
				/// </summary>
				[System::ComponentModel::Browsable(false)]
				property System::String^ Anchor { virtual System::String^ get() override { return ""; } virtual void set(System::String^ value) override {}}

				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("BehaviourCategory", EBCategories::typeid),
					TBPropertyFilter(TBPropertyFilters::DesignerStatic),
					System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
					EditorAttribute("Microarea.EasyBuilder.UI.RowViewUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ RowView {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property bool MouseDownTarget { virtual bool get() override { return true; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				property System::Int32 TbHandle { virtual System::Int32 get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::ComponentModel::ComponentCollection^ Components { virtual  System::ComponentModel::ComponentCollection^ get() { return gcnew System::ComponentModel::ComponentCollection(components->ToArray()); } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property bool CanAutoFillFromDataBinding { virtual bool get() { return true; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				virtual property System::String^ SerializedType	{ virtual System::String^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::Type^ ExcludedBindParentType { virtual System::Type^ get() { return nullptr; } }

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
				property IDataType^ CompatibleDataType { virtual IDataType^ get() { return nullptr; } }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property System::Collections::Generic::List<MBodyEditColumn^>^ ColumnsCollection { System::Collections::Generic::List<MBodyEditColumn^>^ get() { return components; } }

				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool AllowCallDialog	{ EBool get(); void set(EBool value); }
	
				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool AllowOrdering	{ EBool get(); void set(EBool value); }
				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool AllowOrderingOnBrowse { EBool get(); void set(EBool value); }
				/// <summary>
				/// 
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool AllowOrderingOnEdit { EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowDelete	{ EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowInsert	{ EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowCopy	{ EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowDrop { EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowPaste { EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowMultipleSel { EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowRemoveColumnInteractive { EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowDrag { EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowDragReadOnlyDoc { EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowColumnLock				{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowColumnLockInteractive { EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowCustomize { EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	AllowSearch	{ EBool get(); void set(EBool value); }

				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ChangeColor	{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	EnlargeAllStringColumns{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	EnlargeCustom { EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	EnlargeLastColumn{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	EnlargeLastStringColumn	{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowColumnHeaders			{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowVertScrollbar			{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowDataTip { EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowStatusBar { EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowHorizScrollbar			{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowFooterToolbar			{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowHeaderToolbar			{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowHorizLines				{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowVertLines				{ EBool get(); void set(EBool value); }
				/// <summary>
				///  
				/// </summary>
				[LocalizedCategory("BodyEditStylesCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic)]
				property EBool 	ShowBorders					{ EBool get(); void set(EBool value); }

				/// <summary>
				/// Gets or Sets the Visible attribute for the current column
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerStatic), ExcludeFromIntellisense]
				property bool Visible { virtual bool get() override; virtual void set(bool visible) override; }

				/// <summary>
				/// Number of rows in the column title
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property double TitleRows { double get(); void set(double value); }

				/// <summary>
				/// Maximum number of records in the grid
				/// </summary>
				[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
				property int MaxRecords { int get(); void set(int value); }


				MESSAGE_HANDLER_EVENT(RowChanged, EasyBuilderEventArgs, "Occurs when row is changed.");

			internal:
				[System::ComponentModel::Browsable(false)]
				property bool IsSerialized { bool get() { return isSerialized; } void set(bool value) { isSerialized = value; }  }

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the bodyedit which the control refers wraps to</param>
				MBodyEdit(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MBodyEdit(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				!MBodyEdit();

				/// <summary>
				/// Finalizer
				/// </summary>
				~MBodyEdit();

			protected:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual int GetNamespaceType() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanCreate() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnMouseDown(System::Drawing::Point p) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnDesignerControlCreated() override;
				
				/// <summary>
				/// Internal Use
				/// </summary>
				void GetChildrenFromPos(
					System::Drawing::Point screenPoint,
					System::IntPtr handleToSkip,
					System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren
					);
				/// <summary>
				/// Internal Use
				/// </summary>
				IWindowWrapper^ GetControl(System::IntPtr handle);
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedType) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
				virtual bool CanUpdateTarget(System::Type^ droppedType) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void UpdateTargetFromDrop(System::Type^ droppedType) override;
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;
				/// <summary>
				/// Returns the column of the bodyedit
				/// </summary>
				/// <param name="name">the name of the column to search</param>
				MBodyEditColumn^ GetColumnByName(System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void CreateComponents();

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
				virtual void ApplyResources();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnAfterCreateComponents() { }

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ClearComponents();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual	bool WndProc(System::Windows::Forms::Message% m) override;

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
				virtual void Add(System::ComponentModel::IComponent^ component, bool isChanged);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Remove(System::ComponentModel::IComponent^ component);

				/// <summary>
				/// Insert a MBodyEditColumn into the bodyedit at the specified position
				/// </summary>
				/// <param name="nPos">the position where the column will be placed</param>
				/// <param name="column">the column to add to the bodyedit</param>
				virtual void Insert(int nPos, MBodyEditColumn^ column);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Initialize() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void AutoFillFromDataBinding(IDataBinding^ dataBinding, bool overrideExisting);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropData(IDataBinding^ dataBinding) override;

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
				/// Internal use
				/// </summary>
				[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
				[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property bool	StandardTabOrder { bool get(); void set(bool value); }

			private:
				bool				IsDataBindingCompatibleForAutoFill(IDataBinding^ dataBinding);
				MBodyEditColumn^	GenerateColumnFormField(IRecordField^ field, bool isDroppingAllDbt);
				MBodyEditColumn^	GetColumnByInfo(ColumnInfo* pInfo);
				bool				CreateColumns(bool skipDuplicates);
			};
		}
	}
}
