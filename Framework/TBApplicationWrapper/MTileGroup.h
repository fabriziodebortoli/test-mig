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
#include "MTilePanel.h"

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
	public ref class TileGroupSerializer : EasyBuilderControlSerializer
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
	[System::ComponentModel::Design::Serialization::DesignerSerializer(TileGroupSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTileGroup : WindowWrapperContainer
	{
	private:
		TileGroupInfoItem* m_pInfo;
		CTabManager* m_pTileManager;
	protected:
		CBaseTileGroup* m_pTileGroup;

	public:

		/// <summary>
		/// Gets the parent tabber of the tab (as IWindowWrapperContainer)
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property IWindowWrapperContainer^ Parent { virtual void set(IWindowWrapperContainer^ value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual HWND GetControlHandle(const CTBNamespace& aNamespace) override;

		/// <summary>
		/// Gets or sets the collection of MTabs for the tabber control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property CBaseTileGroup* TileGroup {virtual CBaseTileGroup* get(); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property int TileGroupID { virtual int get(); }

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
		/// Gets the MinSize for the current control, in Logical Units
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property virtual System::Drawing::Size MinSize { System::Drawing::Size get() override { return __super::minSize; }  }

		/// <summary>
		/// 
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property bool GroupCollapsible {
			virtual bool get();
			void virtual set(bool value);
		}
		
		/// <summary>
		/// Gets or sets the index in the tab order of this control
		/// </summary>
		[System::ComponentModel::Browsable(true)]
		property int TabOrder {
			virtual int get() override;
			virtual void set(int value) override;
		}
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
		/// Gets or Sets the Text (title) of the Tab
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), System::ComponentModel::Localizable(true)]
		property System::String^ Text { virtual System::String^ get() override; virtual void set(System::String^ value) override; }

		/// <summary>
		/// Gets or Sets the name of the Tab
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved|TBPropertyFilters::DesignerRuntime)]
		property System::String^ ParentName { virtual System::String^ get();   }

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
		System::IntPtr GetPtr(){ return (System::IntPtr) m_pTileGroup; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property EditingMode DesignerMovable { virtual EditingMode get() override; }

		/// <summary>
		/// true if this control can be activated pressing the tab key
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool TabStop {
			virtual bool get() override { return __super::TabStop; }
			virtual void set(bool value)override { __super::TabStop = value; }
		}

		/// <summary>
		/// Gets or Sets the location for the current control
		/// </summary>
		[DisplayName("Location"), System::ComponentModel::Browsable(false)]
		property System::Drawing::Point LocationLU {
			virtual System::Drawing::Point get() override { return __super::LocationLU; }
			virtual void set(System::Drawing::Point value) override { __super::LocationLU = value; }
		}

		/// <summary>
		/// Gets or Sets the size for the current control
		/// </summary>
		[DisplayName("Size")]
		property System::Drawing::Size SizeLU {
			virtual System::Drawing::Size get() override { return __super::SizeLU; }
			virtual void set(System::Drawing::Size value) override { __super::SizeLU = value; }
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property bool AreComponentsLoaded {  virtual bool get() override; }

		private:
			void LayoutChanged();
			void TabOrderSetForTileManager(int value);
			void TabOrderSetForView(int value);
	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MTileGroup(System::IntPtr infoPtr);

		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTileGroup(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Distructor
		/// </summary>
		~MTileGroup();

		/// <summary>
		/// Finalizer
		/// </summary>
		!MTileGroup();

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
		virtual void Initialize() override;

		/// <summary>
		/// Activates and select the tab
		/// </summary>
		virtual void Activate() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		void SyncTileGroup();

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		[ExcludeFromIntellisense]
		property System::IntPtr Handle { virtual System::IntPtr get() override; }

		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals(Object^ obj) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanCallCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropData(IDataBinding^ dataBinding) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget(System::Type^ droppedObject) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void AfterTargetDrop(System::Type^ droppedType) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void GetChildrenFromPos(System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int	GetNamespaceType() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void AfterOwnEditingSwitching(bool onwEditingOn, bool IAmInEditing) override;

		void AttachTileManager(CTileManager* pManager, TileGroupInfoItem* pInfo);

		int		GetTileIndex(MTileDialog^ tileDialog);
		void	MoveTile(MTileDialog^ tileDialog, int nToIndex);
	private:
		void SetGroupInDesignMode(CBaseTileGroup* pTileGroup, int nFlex);
	};

}
}
}