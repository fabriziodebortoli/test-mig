#pragma once

class CParsedCtrl;
class CBaseDocument;
class DlgInfoItem;
class DataObj;
class SqlRecord;
class Formatter;

#include "MDataObj.h"
#include "MSqlRecord.h"
#include "MFormatters.h"
#include "MDocument.h"
#include "UITypeEditors.h"
#include "MParsedControls.h"
#include "MPanel.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::ComponentModel::Design::Serialization;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace System::Resources;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System::Drawing::Design;
using namespace System::ComponentModel;
using namespace System::Windows::Forms;
using namespace System::Windows::Forms::Design;
using namespace System::ComponentModel::Design::Serialization;

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::Localization;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::View;

class CRegisteredParsedCtrl;


//----------------------------------------------------------------------------
class CMyCrackingDialog : public CParsedDialog
{
public:
	CTileManager* AddBaseTileManagerCracked(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE)
	{
		return (CTileManager*)AddBaseTileManager(nIDC, pClass, sName, bCallOnInitialUpdate);
	};

	CBaseTabManager* AddBaseTabManagerCracked(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE)
	{
		return AddBaseTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);
	};
};

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class TabSerializer : EasyBuilderControlSerializer
	{
	protected:
		virtual Statement^ GetConstructor (IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass (SyntaxTree^ syntaxTree, IComponent^ control) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void SerializePropertiesAndAddMethod (IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl, System::Collections::Generic::IList<Statement^>^ collection) override;

	};
	
	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class TabberSerializer : EasyBuilderControlSerializer
	{
	protected:
		virtual Statement^ GetConstructor	(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool IsSerializable (EasyBuilderComponent^ ebComponent) override { return true; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass (SyntaxTree^ syntaxTree, IComponent^ control) override;

	};

	/// <summary>
	/// Class that wraps the CTabDialog taskbuilder object: is a container for MParsedControls
	/// and GenericControls
	/// </summary>
	//=============================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(TabSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTab : WindowWrapperContainer
	{
	public:
		static int	NoActiveTabIndex = -1;

	private:
		DlgInfoItem*		m_pInfo;
		CTabManager*		m_pTabManager;

	public:
		/// <summary>
		/// Gets or Sets the Text (title) of the Tab
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
		property System::String^ Text { virtual System::String^ get () override; virtual void set (System::String^ value) override; }

		/// <summary>
		/// Gets or Sets the name of the Tab
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property System::String^ Name { virtual System::String^ get () override; virtual void set (System::String^ value) override;  }

		/// <summary>
		/// Gets the namespace (as INamespace) of the Tab
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime)]
		property INameSpace^ Namespace { virtual INameSpace^ get () override;}

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property System::IntPtr Handle { virtual System::IntPtr get () override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property EditingMode DesignerMovable { virtual EditingMode get() override { return EditingMode::None; } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::IntPtr	DocumentPtr	{ virtual  System::IntPtr get () override; }
		
		/// <summary>
		/// Gets or sets the background image of the tab (can be a file path or an image namespace)
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
			System::ComponentModel::EditorAttribute(IMAGE_UITYPEEDITORFULLQUALIFIEDNAME, System::Drawing::Design::UITypeEditor::typeid)]
		property System::String^ BackgroundImage { System::String^ get (); void set(System::String^ value); }
		
		/// <summary>
		/// Returns true if the tab is active in design mode (in runtime mode only the current tab is alive)
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool IsAlive { virtual bool get () { return Handle != System::IntPtr::Zero; } }
				
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanCallCreateComponents() override;

		/// <summary>
		/// Gets or sets the visible attribute of the tab
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::Browsable(false)]
		property bool Visible { virtual bool get () override; virtual void set (bool value) override; }

		/// <summary>
		/// Gets or sets the enabled attribute of the tab
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property bool Enabled { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// Gets the tab index of the tab
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property int TabIndex { virtual int get (); }

		/// <summary>
		/// Gets the parent tabber of the tab (as IWindowWrapperContainer)
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IWindowWrapperContainer^ Parent { virtual void set(IWindowWrapperContainer^ value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName { virtual System::String^ get () override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType { virtual System::String^ get () override; }

		/// <summary>
		/// Gets or Sets the size for the current control, in logical units
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
		property System::Drawing::Size Size {  virtual void set(System::Drawing::Size value) override; }
	
		/// <summary>
		/// override, tells if tilegroup is alive and loaded when in tilemanager 
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property bool AreComponentsLoaded {  virtual bool get() override; }

	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MTab (System::IntPtr infoPtr);
		
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTab (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MTab ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MTab ();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetPtr() { return (System::IntPtr) m_pInfo; }

		/// <summary>
		/// Events raised when the tab becomes active
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		virtual event System::EventHandler<EasyBuilderEventArgs^>^		Displayed;
		
		/// <summary>
		/// Events raised when the tab controls became active
		/// </summary>
		[LocalizedCategory("DocumentStatusCategory", EBCategories::typeid)]
		virtual event System::EventHandler<EasyBuilderEventArgs^>^ ControlsEnabled;
		
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		void OnControlsEnabled();

	public:
		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals (Object^ obj) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int	 GetNamespaceType () override;
		
		/// <summary>
		/// Activates and select the tab
		/// </summary>
		virtual void Activate () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget (System::Type^ droppedObject) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void CallCreateComponents () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual System::IntPtr GetChildFromOriginalPos (System::Drawing::Point clientPosition, System::String^ controlClass) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void SaveChildOriginalPos (System::IntPtr hwndChild, System::Drawing::Point clientPosition) override;
		
	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanCreate () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool Create (IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;
	};


	/// <summary>
	/// Class that wraps the CTabManager taskbuilder object: is a container for the MTab 
	/// </summary>
	//================================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(TabberSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTabber : WindowWrapperContainer
	{
		//----------------------------------------------------------------------------
		ref class TabCurrentStatus : IDesignerCurrentStatusObject
		{
		private:
			int nActiveTab;
	
		internal:
			TabCurrentStatus (int nActiveTab) { this->nActiveTab = nActiveTab; }
			
			property int ActiveTab { int get () { return nActiveTab; } }
		};
	
		BaseWindowWrapper^	defaultItem;
		
	protected:
		CTabManager* m_pTabber;
		int			 m_nDefaultItemPos;
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MTabber (System::IntPtr handleWndPtr);

		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="className">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTabber (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, bool hasCodeBehind);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MTabber ();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MTabber ();

	public:
		/// <summary>
		/// Gets or Sets the Text (title) of the Tabber
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden),  System::ComponentModel::Localizable(true)]
		property System::String^ Text { virtual System::String^ get () override; virtual void set (System::String^ value) override; }

		/// <summary>
		/// Gets or Sets the name of the Tabber
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Name { virtual System::String^ get () override; virtual void set (System::String^ value) override; }

		/// <summary>
		/// Gets the namespace (as INamespace) of the Tabber
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
			System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property INameSpace^ Namespace { virtual INameSpace^ get () override; }

		/// <summary>
		/// Gets or sets the visible attribute of the tab
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool Visible { virtual bool get () override { return __super::Visible; } virtual void set(bool value) override { __super::Visible = value; } }

		/// <summary>
		/// Gets the active tab of the tabber
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::ReadOnly(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property MTab^ CurrentTab { virtual MTab^ get (); }
		
		/// <summary>
		/// Gets or Sets the AutoStretch property for the tabber
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property bool AutoStretch { virtual void set(bool value) override; }

		/// <summary>
		/// Gets or Sets the FillParent property for the bodyedit
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid)]
		property bool AutoFill { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// Gets or Sets the size for the tabber
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property System::Drawing::Size Size	{ virtual System::Drawing::Size get () override; virtual void set (System::Drawing::Size value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool KeepTabsAlive { bool get (); void set (bool value); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property EditingMode DesignerMovable { virtual EditingMode get() override { return EditingMode::All; } }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property bool MouseDownTarget { virtual bool get () override { return true;} }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName { virtual System::String^ get () override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType { virtual System::String^ get () override; }

		/// <summary>
		/// Gets or Sets the property indicating indicates the amount of space this component will take up in its parent container
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
		[TBPropertyFilter(TBPropertyFilters::ComponentState)]
		property int Flex {
			virtual int get() override;
			void virtual set(int value)override;
		}

		[System::ComponentModel::Browsable(false)]
		property BaseWindowWrapper^ DefaultItem { BaseWindowWrapper^ get() { return defaultItem; } void set(BaseWindowWrapper^ item) { defaultItem = item; } }

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual	bool WndProc (System::Windows::Forms::Message% m) override;
		
		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals (Object^ obj) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int GetNamespaceType () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnMouseDown (System::Drawing::Point p) override;
		
		/// <summary>
		/// Get the control specified by the desired namespace
		/// </summary>
		virtual IWindowWrapper^	GetControl (INameSpace^ nameSpace) override;
		
		/// <summary>
		/// Gets the  tab specified by the name
		/// </summary>
		MTab^ GetTabByName (System::String^ name);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetPtr (){ return (System::IntPtr) m_pTabber; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetInfoPtr (System::String^ name);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		CTabManager* GetInnerTabManager () { return m_pTabber; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual HWND GetControlHandle (const CTBNamespace& aNamespace) override;
				
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		int MouseHitTestOnTab (System::Drawing::Point^ mousePosition);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		MTab^ GetTabByPoint(System::Drawing::Point^ mousePosition);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void SaveCurrentStatus (IDesignerCurrentStatus^ status) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void ApplyCurrentStatus (IDesignerCurrentStatus^ status) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnDesignerControlCreated() override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget (System::Type^ droppedObject) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropData (IDataBinding^ dataBinding) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void CallCreateComponents () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void SwitchVisibility (bool bVisible) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CreateWrappers (array<System::IntPtr>^ handlesToSkip) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void GetChildrenFromPos (System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren) override;

		[ExcludeFromIntellisense]
		void AfterTargetDrop(System::Type^ droppedType) override;

		[ExcludeFromIntellisense]
		void ResumeDefaultLayout();

		int GetActiveItemPos();

	protected:
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void Initialize () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanCreate () override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool Create (IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;
	};


} 
} 
}
