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
#include "MTileGroup.h"

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

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			/// <summary>
			/// Internal Use
			/// </summary>
			//================================================================================
			public ref class TileManagerSerializer : EasyBuilderControlSerializer
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
			/// Class that wraps the CTabManager taskbuilder object: is a container for the MTileManager 
			/// </summary>
			//================================================================================
			[System::ComponentModel::Design::Serialization::DesignerSerializer(TileManagerSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			public ref class MTileManager : MTabber
			{
			public:

				/// <summary>
				/// Gets the active tab of the tabber
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::ReadOnly(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
				property MTileGroup^ CurrentTileGroup { virtual MTileGroup^ get(); }

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

			public:
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				inline CTileManager* GetTileManager() { return (CTileManager*)m_pTabber; }

			public:
				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				MTileManager(System::IntPtr handleWndPtr);

				/// <summary>
				/// Internal Use: Constructor
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="className">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MTileManager(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ className, System::Drawing::Point location, bool hasCodeBehind);

				/// <summary>
				/// Distructor
				/// </summary>
				~MTileManager();

				/// <summary>
				/// Finalizer
				/// </summary>
				!MTileManager();

				///<summary>
				///Updates needed attributes for json serialization 
				///</summary>
				virtual void UpdateAttributesForJson(CWndObjDescription* pParentDescription) override;

				///<summary>
				///Generates serialization for the class
				///</summary>
				virtual void GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void Initialize() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;

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
				virtual bool CreateWrappers(array<System::IntPtr>^ handlesToSkip) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnAfterCreateComponents() override;

				/// <summary>
				/// Override of the equals method, true if the compared tabs are the same
				/// </summary>
				virtual bool Equals(Object^ obj) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				System::IntPtr GetPtr() { return (System::IntPtr) GetTileManager(); }
	
				/// <summary>
				/// Internal Use
				/// </summary>
				virtual void GetChildrenFromPos(System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual int GetNamespaceType() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void CallCreateComponents() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual HWND GetControlHandle(const CTBNamespace& aNamespace) override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				MTileGroup^ GetTabByPoint(Point^ p);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				void MoveTileGroup(MTileGroup^ tileGroup, int newIndex);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				MTileGroup^ GetGroupByName(System::String^ name);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void IntegrateLayout(ILayoutComponent^ layoutObject) override;


				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void Add(System::ComponentModel::IComponent^ component, bool isChanged) override;

				private:
					void LayoutChanged();
			};
		}
	}
}