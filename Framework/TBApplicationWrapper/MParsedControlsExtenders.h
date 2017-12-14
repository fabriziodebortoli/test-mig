#pragma once

class CStateCtrlObj;
class CDataFileElementFieldType;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper 
{
	//=============================================================================
	public ref class MManagedControl : MParsedControl
	{
		static System::String^ TheControlPropertyName = "TheControl";
	public:
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
		MManagedControl (System::IntPtr handleWndPtr);
		
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MManagedControl (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

	public:
		/// <summary>
		/// Gets or Sets the formatter for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property FormatterStyle^ Formatter { FormatterStyle^ get () { return __super::Formatter; }  void set (FormatterStyle^ value) { __super::Formatter = value; } }

		/// <summary>
		/// Gets or Sets the class type for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IControlClass^ ClassType { virtual IControlClass^ get () override { return __super::ClassType; }  virtual void set(IControlClass^ value) override { __super::ClassType = value; } }

		/// <summary>
		/// Sets the desired location for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Drawing::Point Location	{ virtual System::Drawing::Point get() override { return Ctrl->Location; } virtual void set (System::Drawing::Point value) override { Ctrl->Location = value; } }
		
		/// <summary>
		/// Gets or Sets the visible attribute for the control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool Visible {  virtual bool get() override { return Ctrl->Visible; } virtual void set (bool value) override { Ctrl->Visible = value; }  }
		
		/// <summary>
		/// Gets or sets the size of the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Drawing::Size Size { virtual  System::Drawing::Size get() override { return Ctrl->Size; } virtual void set (System::Drawing::Size value) override { Ctrl->Size = value; } }	

		/// <summary>
		/// Gets or sets the Enabled
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool Enabled { virtual bool get() override { return Ctrl->Enabled; } virtual void set (bool value) override { Ctrl->Enabled = value; } }	
	
		/// <summary>
		/// Gets or sets the max lenght for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int MaxLength { virtual int get () override { return __super::MaxLength; } virtual void set (int length) override { __super::MaxLength = length; } }

		/// <summary>
		/// Gets or sets the index in the tab order of this control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int TabOrder { virtual int get() override { return Ctrl->TabIndex; } virtual void set(int value) override { Ctrl->TabIndex = value; } }
		/// <summary>
		/// true if this control can be activated pressing the tab key
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool TabStop { virtual bool get() override { return Ctrl->TabStop; } virtual void set(bool value) override { Ctrl->TabStop = value; } }

		/// <summary>
		/// Set sites
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::ComponentModel::ISite^ Site { virtual void set(System::ComponentModel::ISite^ site) override; }

	protected:
		/// <summary>
		/// Gets real object
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Windows::Forms::Control^ Ctrl { System::Windows::Forms::Control^ get (); }		
	};

	//=============================================================================
	public ref class MTextBox : MManagedControl
	{
	public:
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
		MTextBox (System::IntPtr handleWndPtr);
		
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTextBox (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

	public:
		/// <summary>
		/// Gets real object
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Content)]
		property System::Windows::Forms::TextBox^ TheControl { System::Windows::Forms::TextBox^ get (); void set (System::Windows::Forms::TextBox^ value) {} }	
	};

	/// <summary>
	/// Provides information about of MStateObject events.
	/// </summary>
	//=============================================================================
	public ref class MStateObjectEventArgs : EasyBuilderEventArgs
	{
	private:
		MDataObj^	dataObj;

	public:
		/// <summary>
		/// Initializes a new instance of MStateObjectEventArgs
		/// </summary>
		MStateObjectEventArgs(MDataObj^ dataObj);

		/// <summary>
		/// Gets the name used for serializing the object 
		/// </summary>
		property MDataBool^ StateData	{ MDataBool^ get (); }

	};

	/// <summary>
	/// Internal Use
	/// </summary>
	[ExcludeFromIntellisense]
	[System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid)]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(EasyBuilderSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::PropertyTabAttribute(System::Windows::Forms::Design::EventsTab::typeid, System::ComponentModel::PropertyTabScope::Component)]
	//================================================================================
	public ref class MStateObject : EasyBuilderComponentExtender, IDataBindingConsumer
	{
	public:
		enum class ButtonStyle 
		{
			NoButton,
			AutoManual,
			Arrow,
			Calendar,
			Outlook,
			Color,
			Internet
		};

		enum class EditableStyle 
		{
			Default,
			Always,
			WhenTrue,
			WhenFalse
		};

	private:
		CStateCtrlObj*		m_pStateCtrl;
		MDataObj^			privateDataObj;
		BaseWindowWrapper^	ui;
		FieldDataBinding^	dataBinding;
		ButtonStyle			style;
	
	public:
		MStateObject  (IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);
		~MStateObject  ();
		!MStateObject  ();

	public:
	
		/// <summary>
		/// Boolean Data needed to manage and switch auto/manual state button
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("DataBindingCategory", EBCategories::typeid), 
			TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime), ExcludeFromIntellisense]
		property IDataBinding^ DataBinding{ virtual IDataBinding^ get (); virtual void set(IDataBinding^ value); }
		/// <summary>
		/// Gets or Sets the data source for the current control
		/// </summary>
		[LocalizedCategory("DataBindingCategory", EBCategories::typeid),
			TBPropertyFilter(TBPropertyFilters::DesignerStatic),
			System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),
			EditorAttribute("Microarea.EasyBuilder.UI.DataSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
		property  System::String^ DataSource {  virtual System::String^ get();  virtual void set(System::String^ value); }
		/// <summary>
		/// Switches the meaning (manual/automatic) of the boolean value attached as datasource of this button 
		/// </summary>
		[LocalizedCategory("BehaviourCategory", EBCategories::typeid),
			TBPropertyFilter(TBPropertyFilters::DesignerStatic),
			System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property  bool InvertState {  bool get();  void set(bool value); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Type^ ExcludedBindParentType { virtual System::Type^ get (); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDataManager^ FixedDataManager { virtual IDataManager^ get (); }

		/// <summary>
		/// Gets or sets when control is always disabled (TODO usare l'enabled??)
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("GraphicsCategory", EBCategories::typeid), 
			TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
		property bool ControlAlwaysDisabled{ bool get (); void set(bool value); }

		/// <summary>
		/// Gets or sets if control is enabled or disabled when document is in edit mode 
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("DocumentStatusCategory", EBCategories::typeid), 
			TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
		property bool ControlEnabledInEditMode{ bool get (); void set(bool value); }

		/// <summary>
		/// Gets or sets when control is always disabled (TODO usare l'enabled??)
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("GraphicsCategory", EBCategories::typeid), 
			TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
		property EditableStyle ControlEditableStyle{ EditableStyle get (); void set(EditableStyle value); }

		/// <summary>
		/// Gets or sets if button is always disabled 
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("GraphicsCategory", EBCategories::typeid), 
			TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
		property bool ButtonAlwaysDisabled{ bool get (); void set(bool value); }

		/// <summary>
		/// Gets or sets if button is enabled or disabled when document is in edit mode 
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("DocumentStatusCategory", EBCategories::typeid), 
			TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
		property bool ButtonEnabledInEditMode{ bool get (); void set(bool value); }

		/// <summary>
		/// Gets if instance is empty
		/// </summary>
		property bool EmptyComponent { virtual bool get () override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool CanAutoFillFromDataBinding { virtual bool get () { return false; } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void AutoFillFromDataBinding(IDataBinding^ dataBinding, bool overrideExisting) {};

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDataType^ CompatibleDataType { virtual IDataType^ get (); }

		/// <summary>
		/// Gets or sets button style
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property ButtonStyle Style { ButtonStyle get (); void set(ButtonStyle value); }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
        property MDataObj^ DataObj { virtual MDataObj^ get(); }

		/// <summary>
		/// Get/Set State Object Management
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), LocalizedCategory("GraphicsCategory", EBCategories::typeid), ObjectReference, TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property MStateObject^ Accessor { MStateObject^ get () { return this; } void set (MStateObject^ value) {}; }

		/// <summary> 
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ AccessorPropertyName { virtual System::String^ get () override { return "Accessor"; } }

		/// <summary>
		/// Event raised when user state changes
		/// </summary>
		MESSAGE_HANDLER_EVENT(StateButtonClicked, MStateObjectEventArgs, "Occurs when button associated with control is clicked.")

		virtual System::String^ ToString() override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool CanExtendObject (IEasyBuilderComponentExtendable^ o) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanChangeProperty	(System::String^ propertyName) override;

	private:
		[System::ComponentModel::Browsable(true)]
		property MParsedControl^ Control { MParsedControl^ get () { return (MParsedControl^) ExtendedObject; } }
		StateData* GetStateDescription();
		void ClearStateDescription();
		void	UpdateUIWrapping		();
		CString	GetAutoImage			();
		CString	GetManualImage			();
		void	AdjustState				(int nState, CStateCtrlState* pState, bool isControlToEnabled);
		void	InitExistingStyle		();
		void	AttachExistingStateCtrl	();
		void	AttachNewStateCtrl		(CParsedCtrl* pCtrl, ::DataObj* pDataObj);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class MFileItemsSourceFieldSerializer : EasyBuilderSerializer
	{
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, Object^ current) override;
	};

	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(MFileItemsSourceFieldSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	//================================================================================
	public ref class MFileItemsSourceField : EasyBuilderComponent, IDataBindingConsumer
	{
		IDataBinding^	dataBinding;
		System::String^			name;
		IDataType^		dataType;

	public:
		MFileItemsSourceField   (EasyBuilderComponent^ parent, System::String^ name, bool hasCodeBehind);
		~MFileItemsSourceField  ();
		!MFileItemsSourceField  ();
	
	public:
		/// <summary>
		/// Data needed to receive file field
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), ExcludeFromIntellisense]
		property IDataBinding^ DataBinding{ virtual IDataBinding^ get (); virtual void set(IDataBinding^ value); }

		/// <summary>
		/// Gets and sets name of field
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, LocalizedCategory("InformationsCategory", EBCategories::typeid)]
		property System::String^ Name { virtual System::String^ get () override; virtual void set(System::String^ value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool CanAutoFillFromDataBinding { virtual bool get () { return false; } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void AutoFillFromDataBinding(IDataBinding^ dataBinding, bool overrideExisting) {};

		/// <summary>
		/// Gets and Sets compatible data type for this field
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDataType^ CompatibleDataType { virtual IDataType^ get (); void set(IDataType^ value); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Type^ ExcludedBindParentType { virtual System::Type^ get (); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property IDataManager^ FixedDataManager { virtual IDataManager^ get () { return nullptr; } }

		void AdjustSite();
	};

	//================================================================================
	public ref class MFileItemsSourceFields : EasyBuilderComponents
	{
	public:
		MFileItemsSourceFields (EasyBuilderComponent^ parent);
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool HasChanged() override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void InitializeForUI () override;
	
	internal:
		void AdjustSites ();
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(EasyBuilderSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	//================================================================================
	public ref class MFileItemsSource : EasyBuilderComponentExtender, System::ComponentModel::IContainer
	{
		MFileItemsSourceFields^	components;
		bool					useCountry;
		MParsedControl^			ownerControl;
	
	public:
		MFileItemsSource  (MParsedCombo^ owner, EasyBuilderComponent^ parentComponent, System::String^ name);
		~MFileItemsSource  ();
		!MFileItemsSource  ();

	public:
		/// <summary>
		/// Gets/set file namespace for combo box
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::EditorAttribute(DataFileUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
		property System::String^ FileNamespace	{ System::String^ get (); void set(System::String^ nameSpace); }

		/// <summary>
		/// Gets/set file if file manages Culture
		/// </summary>
		[System::ComponentModel::Browsable(true), LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property bool UseCountry	{ bool get (); void set(bool value); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::ComponentModel::ComponentCollection^ Components { virtual  System::ComponentModel::ComponentCollection^ get () { return gcnew System::ComponentModel::ComponentCollection(components->ToArray()); } }

		/// <summary>
		/// Array of Bindable Fields contained into Xml file 
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Content), ExcludeFromIntellisense]
		[System::ComponentModel::EditorAttribute(EasyBuilderComponentsEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
		property MFileItemsSourceFields^ OtherBindings { MFileItemsSourceFields^ get () { return components; }  void set (MFileItemsSourceFields^ value) { components = value; } }
		
		/// <summary> 
		/// Get/Sets File Source management
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::ComponentState)]
		property MFileItemsSource^ Accessor { MFileItemsSource^ get () { return this; } void set (MFileItemsSource^ value) {} }

		/// <summary> 
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ AccessorPropertyName { virtual System::String^ get () override { return "Accessor"; } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add (System::ComponentModel::IComponent^ component);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add (System::ComponentModel::IComponent^ component, System::String^ name);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Remove (System::ComponentModel::IComponent^ component);

		virtual System::String^ ToString() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool CanExtendObject (IEasyBuilderComponentExtendable^ e) override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void AdjustSite	() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanChangeProperty	(System::String^ propertyName) override;

	internal:
		CXmlCombo*	GetXmlCombo();
	};
}
}
}

