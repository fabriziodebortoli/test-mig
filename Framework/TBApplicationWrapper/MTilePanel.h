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
#include "MTabber.h"	
#include "MTileDialog.h"

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

namespace Microarea {	namespace Framework	{		namespace TBApplicationWrapper
{

	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class TilePanelSerializer : EasyBuilderControlSerializer
	{
	protected:
		virtual Statement^ GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool IsSerializable(EasyBuilderComponent^ ebComponent) override { return true; }

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control) override;
	};


	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class TilePanelTabSerializer : EasyBuilderControlSerializer
	{
	protected:
		virtual Statement^ GetConstructor(IDesignerSerializationManager^ manager, EasyBuilderControl^ ebControl) override;

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool IsSerializable(EasyBuilderComponent^ ebComponent) override { return true; }

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual TypeDeclaration^ SerializeClass(SyntaxTree^ syntaxTree, IComponent^ control) override;
	};

	/// <summary>
	/// Class that wraps the CTilePanel taskbuilder object.
	/// </summary>
	//=============================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(TilePanelTabSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTilePanelTab : WindowWrapperContainer
	{
	private:

		CTilePanel*		m_pPanelTabOwner;
		CTilePanelTab*	m_pTilePanelTab;

	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MTilePanelTab(System::IntPtr infoPtr);

		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTilePanelTab(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Distructor
		/// </summary>
		~MTilePanelTab();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MTilePanelTab();

		/// <summary>
		/// Gets or Sets the name of the Tab
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ value) override;  }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName { virtual System::String^ get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType { virtual System::String^ get() override; }

		/// <summary>
		/// Gets the namespace of the current control
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
		property INameSpace^ Namespace { virtual INameSpace^ get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property System::IntPtr Handle { virtual System::IntPtr get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing) override;

		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals(Object^ obj) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual HWND GetControlHandle(const CTBNamespace& aNamespace) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetPtr() { return (System::IntPtr) m_pTilePanelTab; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanCreate() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget(System::Type^ droppedObject) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;


		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int	 GetNamespaceType() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		CLayoutContainer* GetLayoutContainer();
	};
	
	/// <summary>
	/// Class that wraps the CTilePanel taskbuilder object.
	/// </summary>
	//=============================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(TilePanelSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTilePanel : WindowWrapperContainer
	{
	private:
		CTilePanel*		m_pTilePanel;
		CBaseTileGroup* m_pParentTileGroup;
		bool			oldVisible;
		bool			inOwnEditing;

	public:

		/// <summary>
		/// Gets or sets the collection of MTabs for the tabber control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property MTilePanelTab^ CurrentTab {MTilePanelTab^ get(); }

		/// <summary>
		/// Gets or Sets the name of the Tab
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property System::String^ Name { virtual System::String^ get() override; virtual void set(System::String^ value) override;  }

		/// <summary>
		/// Gets the namespace of the current control
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
		property INameSpace^ Namespace { virtual INameSpace^ get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedName { virtual System::String^ get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ SerializedType { virtual System::String^ get() override; }

		/// <summary>
		/// Gets or Sets the text for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true), TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::BoolTypes)]
		property System::String^ Text { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property bool HasOwnEditing { virtual bool get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void AfterSelectionChanged(bool IAmSelected) override;

	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MTilePanel(System::IntPtr infoPtr);

		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTilePanel(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Distructor
		/// </summary>
		~MTilePanel();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MTilePanel();

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
		virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int	GetNamespaceType() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual HWND GetControlHandle(const CTBNamespace& aNamespace) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property System::IntPtr Handle { virtual System::IntPtr get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget(System::Type^ droppedObject) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Initialize() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnMouseDown(System::Drawing::Point p) override;

		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals(Object^ obj) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void GetChildrenFromPos(System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetPtr(){ return (System::IntPtr) m_pTilePanel; }

		[ExcludeFromIntellisense]
		virtual void IntegrateLayout(ILayoutComponent^ layoutObject) override;
	};

}
}
}