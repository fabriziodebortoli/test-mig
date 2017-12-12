#pragma once

#include "MParsedControls.h"
#include "MPanel.h"

class CAbstractFormFrame;
class CTBNamespace;
class CEBToolbarItem;
class CEBToolbar;
class CInfoOSL;
class CInfoOSLButton;

namespace Microarea { namespace Framework { namespace TBApplicationWrapper
{
	//===================================================================================
	public ref class MToolbarSerializer : EasyBuilderControlSerializer
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Statement^ GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ dbt) override;
	};

	/// <summary>
	/// Class that wraps toolbar objects
	/// </summary>
	//=============================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(MToolbarSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MToolbar : WindowWrapperContainer
	{
	private:	
		CTBToolBar*					m_pTBToolbar;
		bool						isUsingLargeButtons;
		BYTE						m_bBefore;
		IWindowWrapperContainer^	parent;
		
	internal:
		CInfoOSL*	m_pInfoOSL;
	

	public:
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MToolbar (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="handle">handle of C++ window</param>
		MToolbar (System::IntPtr handle);

		/// <summary>
		/// Distructor
		/// </summary>
		~MToolbar ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MToolbar ();

	public:
		/// <summary>
		/// Gets the namespace of the window toolbar
		/// </summary>
		property INameSpace^ Namespace { virtual INameSpace^ get () override; }

		/// <summary>
		/// Gets the name of the toolbar
		/// </summary>
		property System::String^ Name { virtual System::String^ get () override; }

		/// <summary>
		/// Gets the Rectangle of the control in screen coordinates
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Drawing::Rectangle	Rectangle { virtual System::Drawing::Rectangle get () override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::String^ SerializedName { virtual System::String^ get () override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType { virtual System::String^ get () override; }

		/// <summary>
		/// Gets or sets the index in the tab order of this control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int TabOrder { virtual int get() override { return __super::TabOrder; }  virtual void set(int value) override { __super::TabOrder = value; } }
		/// <summary>
		/// true if this control can be activated pressing the tab key
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool TabStop { virtual bool get() override { return false; } ; virtual void set(bool value) override { } }

		/// <summary>
		/// Gets or Sets the location for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Drawing::Point Location	{ virtual System::Drawing::Point get () override { return __super::Location; } virtual void set (System::Drawing::Point value) override { __super::Location = value; } }

			/// <summary>
		/// Gets or Sets the text for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property System::String^ Text	{ virtual System::String^ get () override; virtual void set (System::String^ value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual bool CanUpdateTarget (System::Type^ droppedType) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void UpdateTargetFromDrop (System::Type^ droppedType) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property bool CanBeDeleted { virtual bool get() override { return false; } }
		
		/// <summary>
		/// Gets or Sets if toolbar uses large buttons
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool LargeButtons	{ bool get (); void set (bool value); }
	
		/// <summary>
		/// Gets or sets the enabled attribute for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property bool Enabled { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget (System::Type^ droppedObject) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void NextTBPosAddWidth(int addWidth);


public:
		/// <summary>
		/// Defines if can create toolbar
		/// </summary>
		virtual bool CanCreate () override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int GetNamespaceType () override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add (System::ComponentModel::IComponent^ component) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void			OnBuildingSecurityTree(System::IntPtr tree, System::IntPtr infoTreeItems) override;

	internal:
		property CTBToolBar* TBToolBar	{ CTBToolBar* get() { return m_pTBToolbar; } }
		
		void                    PreTranslateMsgKey      (UINT Msg, WPARAM wParam, LPARAM lParam);
		int						GetButtonSize			();
		int						GetButtonHeight			();
		void					MoveTo					(System::String^ name, System::Windows::Forms::ToolStripItem^ item, int index);
		void					CallEvent				(HWND hWnd, int commandId, UINT nEvent);
	
	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool Create (IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;		

	private:
		CString GetToolbarName();
	};

	/// <summary>
	/// Class that wraps toolbar item objects
	/// </summary>
	//=============================================================================
	public ref class MToolbarItem :  BaseWindowWrapper
	{
	private:
		System::Windows::Forms::ToolStripItem^	        item;
		System::Windows::Forms::ToolStripDropDown^		itemDropDown;
		System::String^					image;
		System::Windows::Forms::Shortcut				ShortcutValue;
		System::String^					tooltip;
		System::Drawing::Size	itemSize;
		System::Drawing::Size	itemSizeStart;

	internal:
		CInfoOSL*		m_pInfoOSL;
		MToolbar^		toolBar;
		int				tabOrder;
		
	public:
		static int ItemEasyLookCode = 100;

	public:
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MToolbarItem (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="handle">handle of C++ window</param>
		MToolbarItem (System::IntPtr handle);

		/// <summary>
		/// Distructor
		/// </summary>
		~MToolbarItem ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MToolbarItem ();	
	public:
		/// <summary>
		/// Gets the namespace of the window toolbar item
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
		property INameSpace^ Namespace { virtual INameSpace^ get () override; }

		/// <summary>
		/// Gets the name of the toolbar item
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
		property System::String^ Name { virtual System::String^ get () override; virtual void set(System::String^ value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::String^ SerializedName { virtual System::String^ get () override; }
	
		/// <summary>
		/// Gets or sets the enabled attribute for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property bool Enabled { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// Gets or Sets the location for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Drawing::Point Location	{ virtual System::Drawing::Point get () override { return __super::Location; } virtual void set (System::Drawing::Point value) override { __super::Location = value; } }

		/// <summary>
		/// Gets or Sets the size for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::Drawing::Size Size	{ virtual System::Drawing::Size get () override { return __super::Size; } ; virtual void set (System::Drawing::Size value) override { __super::Size = value; } }

		/// <summary>
		/// Gets or Sets the visible for the current control
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool Visible	{ virtual bool get () override; virtual void set (bool value) override; }

		/// <summary>
		/// DropDown button enable / disable
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool DropDown	{ virtual bool get (); virtual void set (bool value); }

		/// <summary>
		/// Gets or Sets the text for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property System::String^ Text	{ virtual System::String^ get () override; virtual void set (System::String^ value) override; }

		/// <summary>
		/// Gets or Sets the tooltip for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
		property System::String^ Tooltip	{ System::String^ get (); void set (System::String^ value); }

		/// <summary>
		/// Gets or Sets the Shortcut
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::EditorAttribute(System::Windows::Forms::Design::ShortcutKeysEditor::typeid, System::Drawing::Design::UITypeEditor::typeid)]
		property System::Windows::Forms::Shortcut ShortcutKey   { System::Windows::Forms::Shortcut get (); void set (System::Windows::Forms::Shortcut value); }

		/// <summary>
		/// Gets or sets the image of the button (can be a file path or an image namespace)
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::EditorAttribute(IMAGE_UITYPEEDITORFULLQUALIFIEDNAME, System::Drawing::Design::UITypeEditor::typeid)]
		property System::String^ Image { System::String^ get (); void set(System::String^ value); }

		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^ ClassName { virtual System::String^ get () override; }

		/// <summary>
		/// Add item in ToolStripDropDownButton, ToolStripComboBox
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ CurrentItem { System::String^ get (); };

		/// <summary>
		/// true if this control can be activated pressing the tab key
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool TabStop { virtual bool get() override { return true; } ; virtual void set(bool value) override {  } }

		/// <summary>
		/// Gets or sets the index in the tab order of this control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int TabOrder { virtual int get() override { return tabOrder; }  virtual void set(int value) override; }

		/// <summary>
		/// alignment of item
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property System::Windows::Forms::ToolStripItemAlignment Alignment { System::Windows::Forms::ToolStripItemAlignment get(); void set(System::Windows::Forms::ToolStripItemAlignment value);  }

		/// <summary>
		/// Gets or sets the background color attribute for the current control
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property System::Drawing::Color BackColor	{ System::Drawing::Color get (); void set (System::Drawing::Color value); }

		/// <summary>
		/// Gets or sets the foreground color for the current control
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property System::Drawing::Color ForeColor	{ System::Drawing::Color get (); void set (System::Drawing::Color value); }

		/// <summary>
		/// Gets or sets the display style for the current control
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property System::Windows::Forms::ToolStripItemDisplayStyle DisplayStyle {System::Windows::Forms::ToolStripItemDisplayStyle get (); void set (System::Windows::Forms::ToolStripItemDisplayStyle value); }

		/// <summary>
		/// Gets or Sets the checked property for the current control
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::ComponentState)]
		property bool Checked	{ bool get (); void set (bool value); }

		/// <summary>
		/// Gets item ClientRectangle
		/// </summary>
		[LocalizedCategory("Appearance", EBCategories::typeid)]
		property System::Drawing::Rectangle ClientRectangle { virtual System::Drawing::Rectangle get () override; }

	public:
		/// <summary>
		/// Defines if can create toolbar item
		/// </summary>
		virtual bool CanCreate () override;

		/// <summary>
		/// Add item in ToolStripDropDownButton, ToolStripComboBox
		/// </summary>
		virtual void AddItem(Object^ item);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int GetNamespaceType () override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		void onKeyDown(System::String^ sShort);

		/// <summary>
		/// Gets the iten size
		/// </summary>
		[ExcludeFromIntellisense]
		System::Drawing::Size GetItemSize();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanChangeProperty	(System::String^ propertyName) override;

		/// <summary>
		/// Event raised when the Click message is raised by the control
		/// </summary>
		MESSAGE_HANDLER_EVENT(Click, SelectedItemEventArgs, "Occurs when toolbar button is clicked.");

	internal:
		static System::String^	TypeToControlClass		(System::Type^ type);
		static System::Type^	ControlClassToType		(System::String^ className);
		void			OnButtonClick			(Object^ sender, System::EventArgs^ e);
		void			OnButtonClick			(Object^ sender, int nDropDownID);
		void			ShortcutToAccel			(ACCEL* pAccel);
		System::Windows::Forms::ToolStripItem^  MakeToolStripButton		(bool dropDown);
		void			ProcessOnDropDown		();

		property System::Type^					 ItemType		{ System::Type^ get () { return item->GetType(); } }
		property System::Drawing::Image^ ImageBuffer	{ System::Drawing::Image^ get () { return item ? item->Image : nullptr; } }
		property System::String^				 FullTooltip	{ System::String^ get () { return item->ToolTipText; } }
		
protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool Create (IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;
	
	private:
		CTBNamespace CreateNamespaceFromParent		(MToolbar^ toolbar);
		System::String^		GetFullPathImageFromNamespace	(System::String^ INamespace);
		System::String^		ShortcutTooltip					();
		void		OnItemSelected					(Object^ sender, System::Windows::Forms::ToolStripItemClickedEventArgs^ e);
		void		OnSelectedIndexChanged			(Object^ sender, System::EventArgs^ e);
		void		DeregisterEvents				();
	};
} 
} 
}
