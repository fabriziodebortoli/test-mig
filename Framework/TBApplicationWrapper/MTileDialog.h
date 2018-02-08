#pragma once

class CParsedCtrl;
class CBaseDocument;
class DlgInfoItem;
class DataObj;
class SqlRecord;
class Formatter;
class CRegisteredParsedCtrl;
class CTilePanel;
class CTileDialog;

//#include "MDataObj.h"
//#include "MDocument.h"
//#include "UITypeEditors.h"
#include "MParsedControls.h"
#include <TbGenlib\BaseTileDialog.h>
#include <TbGenlib\BaseTileManager.h>
#include <TbGes\TileManager.h>
#include "MPanel.h"
#include "MLayoutContainer.h"
#include "GenericControls.h"

using namespace System::ComponentModel::Design::Serialization;
using namespace System::Collections::Generic;
using namespace System::Drawing;
using namespace System::Resources;
using namespace ICSharpCode::NRefactory::CSharp;
using namespace System::Drawing::Design;
using namespace System::ComponentModel;
using namespace System::ComponentModel::Design::Serialization;

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::Localization;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::View;


namespace Microarea {	namespace Framework	{		namespace TBApplicationWrapper
{
	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class TileDialogSerializer : EasyBuilderControlSerializer
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
	/// Class that wraps the CTabDialog taskbuilder object: is a container for MParsedControls
	/// and GenericControls
	/// </summary>
	//=============================================================================
	[System::ComponentModel::Design::Serialization::DesignerSerializer(TileDialogSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTileDialog : WindowWrapperContainer
	{
	private:
		GenericGroupBox^ m_staticArea2;

	public:
		CBaseTileDialog* m_pTileDialog;
		bool	oldVisible;
		System::String^ tileStyleName;
		bool			inOwnEditing;
	public:
		
		///<summary>
		///Updates needed attributes for json serialization 
		///</summary>
		virtual void UpdateAttributesForJson(CWndObjDescription* pParentDescription) override;

		///<summary>
		///Generates serialization for the class
		///</summary>
		virtual void GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^>^>^ serialization) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual TbResourceType GetTbResourceType() override;

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
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime) ]
		property EContainerLayout ParentContainerLayout { virtual EContainerLayout get(); virtual void set(EContainerLayout value);   }

		/// <summary>
		/// Gets or Sets the size for the current control, in logical units
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
		property System::Drawing::Size Size {  virtual void set(System::Drawing::Size value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime)]
		property ELayoutAlign ParentLayoutAlign { virtual ELayoutAlign get(); virtual void set(ELayoutAlign value);   }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property EditingMode DesignerMovable { virtual EditingMode get() override; }

		[ExcludeFromIntellisense]
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property ETileDialogSize TileDialogType { virtual ETileDialogSize get(); virtual void set(ETileDialogSize size); }


		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime), System::ComponentModel::ReadOnly(true)]
		property System::String^ TileStyleName { virtual System::String^ get(); virtual void set(System::String^ value); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool Collapsible { virtual bool  get(); virtual void set(bool value);   }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property bool Collapsed { virtual bool  get(); virtual void set(bool value);   }

		

		
		/// <summary>
		/// Gets or Sets the property indicating indicates the amount of space this component will take up in its parent container
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
		[TBPropertyFilter(TBPropertyFilters::ComponentState)]
		property int Flex {
			virtual int get() override;
			void virtual set(int value)override;
		}

		
		/// <summary>
		/// 
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property System::Drawing::Size MinSize {
			virtual System::Drawing::Size get() override;
		}

		/// <summary>
		/// 
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property bool GroupCollapsible {
			virtual bool get();
			void virtual set(bool value);
		}
		

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
		property bool Pinnable { virtual bool  get(); virtual void set(bool value);   }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic)]
		property bool Pinned { virtual bool  get(); virtual void set(bool value);   }

		/// <summary>
		/// Gets or sets the index in the tab order of this control
		/// </summary>
		[TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved)]
		property int TabOrder { virtual int get() override; virtual void set(int value) override; }

		/// <summary>
		/// true if this control can be activated pressing the tab key
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool TabStop {
			virtual bool get() override { return __super::TabStop; }
			virtual void set(bool value)override { __super::TabStop = value; }
		}

		/// <summary>
		/// Gets or Sets the text for the current control
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true), TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::BoolTypes)]
		property System::String^ Text { virtual System::String^ get() override; virtual void set(System::String^ value) override; }


		/// <summary>
		/// Gets or Sets the location for the current control
		/// </summary>
		[DisplayName("Location"), System::ComponentModel::Browsable(false)]
		property System::Drawing::Point LocationLU {
			virtual System::Drawing::Point get() override { return __super::LocationLU; }
			virtual void set(System::Drawing::Point value) override { __super::LocationLU = value; }
		}

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

		/// <summary>
		/// Forces the refresh of the current window
		/// </summary>
		virtual void Invalidate() override;

		/// <summary>
		/// Updates the client area of the specified window
		/// </summary>
		virtual void UpdateWindow() override;

	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MTileDialog(System::IntPtr infoPtr);

		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTileDialog(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Distructor
		/// </summary>
		~MTileDialog();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MTileDialog();

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
		void ManageChildren(ETileDialogSize size);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		bool HasBrothers(BaseWindowWrapper^ ctrl);

		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals(Object^ obj) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetPtr() { return (System::IntPtr) m_pTileDialog; }

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
		virtual bool CanDropTarget(System::Type^ droppedObject) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnDesignerControlCreated() override;


		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Initialize() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add(System::ComponentModel::IComponent^ component, System::String^ name) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual HWND GetControlHandle(const CTBNamespace& aNamespace) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void CallCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual	void AfterWndProc(System::Windows::Forms::Message% m) override;
	private:
		void AddStaticArea(MTileDialog^ tileDialog);
		BOOL AddStaticArea(UINT nID, int nLeft);
		bool IsStaticArea(UINT nID);
	internal:
			virtual void DelayedPartsAnchor() override;

	};

}
}
}