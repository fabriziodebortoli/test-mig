#pragma once


#include <TbGes\HeaderStrip.h>

#include "MParsedControls.h"
#include "MTileGroup.h"

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			/// <summary>
			/// Internal Use
			/// </summary>
			//================================================================================
			public ref class HeaderStripSerializer : EasyBuilderControlSerializer
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
			[System::ComponentModel::Design::Serialization::DesignerSerializer(HeaderStripSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
			public ref class MHeaderStrip : public MTileGroup
			{
				CHeaderStrip* m_pHeader = NULL;
			public:
				/// <summary>
				/// Constructor:
				/// </summary>
				/// <param name="handleWndPtr">is the c++ handle of the CWnd which the control refers to</param>
				MHeaderStrip(System::IntPtr handleWndPtr);

				/// <summary>
				/// Constructor: 
				/// </summary>
				/// <param name="parentWindow">wrapper of the parent window</param>
				/// <param name="name">the name of the control</param>
				/// <param name="controlClass">the specific class name for the control</param>
				/// <param name="location">the location of the control</param>
				/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
				MHeaderStrip(IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

				virtual ~MHeaderStrip();
				!MHeaderStrip();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool Create(IWindowWrapperContainer^ parentWindow, System::Drawing::Point location, System::String^ className) override;

				/// <summary>
				/// Gets or Sets the data source for the current control
				/// </summary>
				[LocalizedCategory("DataBindingCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerStatic), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), EditorAttribute("Microarea.EasyBuilder.UI.DataSourceUITypeEditor, Microarea.EasyBuilder", UITypeEditor::typeid)]
				property  System::String^ DataSource {  virtual System::String^ get();  virtual void set(System::String^ value); }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::String^ SerializedName { virtual System::String^ get() override; }

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::String^ SerializedType { virtual System::String^ get() override; }

				/// <summary>
				/// Internal use
				/// </summary>
				[Browsable(false)]
				property System::String^ Name { virtual System::String^ get() override { return __super::Name; } virtual void set(System::String^ value) override { __super::Name = value; } }

				/// <summary>
				/// Internal Use 
				/// </summary>
				virtual int	GetNamespaceType() override;

				/// <summary>
				/// Internal Use 
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnAfterCreateComponents() override;

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool CanDropTarget(System::Type^ droppedType) override { return false; }

			};
		}
	}
}