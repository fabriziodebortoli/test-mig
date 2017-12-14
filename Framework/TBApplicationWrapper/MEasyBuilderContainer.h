#pragma once

using namespace  Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace  Microarea::TaskBuilderNet::Interfaces::EasyBuilder;
using namespace  Microarea::TaskBuilderNet::Interfaces::View;

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			public ref class MEasyBuilderContainer : public EasyBuilderComponent, IEasyBuilderContainer
			{
			protected:
				System::Collections::Generic::List<System::ComponentModel::IComponent^>^	components;

			public:
				/// <summary>
				/// Constructor
				/// </summary>
				MEasyBuilderContainer(void);

				/// <summary>
				/// Destructor
				/// </summary>
				~MEasyBuilderContainer();

				/// <summary>
				/// Finalizer
				/// </summary>
				!MEasyBuilderContainer();

				/// <summary>
				/// Internal Use
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property System::ComponentModel::ComponentCollection^ Components { virtual System::ComponentModel::ComponentCollection^ get(); }

				/// <summary>
				/// Get the name of the dbt
				/// </summary>
				[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
				property bool IsReferenceableType { virtual bool get() override; }

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
				virtual void CreateComponents();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ClearComponents();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void ApplyResources();

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual bool HasComponent(System::String^ controlName);
				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual System::ComponentModel::IComponent^ GetComponent(System::String^ controlName);

				/// <summary>
				/// Add EasyBuilder components of a specified type to the array. The function is recursive
				/// </summary>
				/// <returns>void</returns>
				/// <param name="requestedTypes">The EasyBuilderComponent type to add.</param>
				/// <param name="components">The array of components to populate.</param>
				[ExcludeFromIntellisense]
				virtual void GetEasyBuilderComponents(System::Collections::Generic::List<System::Type^>^ requestedTypes, System::Collections::Generic::List<EasyBuilderComponent^>^ components);

				/// <summary>
				/// Internal Use
				/// </summary>
				[ExcludeFromIntellisense]
				virtual void OnAfterCreateComponents() {/*does nothing*/ }
			};


		}
	}
}
