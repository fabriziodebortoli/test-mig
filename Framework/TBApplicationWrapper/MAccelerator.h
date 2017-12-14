#pragma once
class CAcceleratorItemDescription;
using namespace System;
using namespace System::Windows::Forms;
using namespace Microarea::TaskBuilderNet::Core::Localization;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;

namespace Microarea {
	namespace Framework	{
		namespace TBApplicationWrapper
		{
			ref class MAccelerator
			{
				CAcceleratorItemDescription* m_pAcceleratorItem;
			public:
				MAccelerator();
				MAccelerator(CAcceleratorItemDescription* pAcceleratorItem);

				CAcceleratorItemDescription* GetItem() { return m_pAcceleratorItem;  }
				virtual String^ ToString() override;

				/// <summary>
				/// Gets the identifier of the accelerator
				/// </summary>
				[LocalizedCategory("InformationsCategory", EBCategories::typeid)]
				property String^ Id { String^ get(); void set(String^ value); }
				
				/// <summary>
				/// Gets the virtual key of the accelerator
				/// </summary>
				[LocalizedCategory("KeyCategory", EBCategories::typeid)]
				property Keys VirtualKey { Keys get(); void set(Keys value); }
				/// <summary>
				/// Gets the ASCII key of the accelerator
				/// </summary>
				[LocalizedCategory("KeyCategory", EBCategories::typeid)]
				property Char ASCIIKey { Char get(); void set(Char value); }
				
				/// <summary>
				/// The accelerator uses the CTRL modifier
				/// </summary>
				[LocalizedCategory("ModifiersCategory", EBCategories::typeid)]
				property bool Control { bool get(); void set(bool value); }
				/// <summary>
				/// The accelerator uses the ALT modifier
				/// </summary>
				[LocalizedCategory("ModifiersCategory", EBCategories::typeid)]
				property bool Alt { bool get(); void set(bool value); }
				/// <summary>
				/// The accelerator uses the SHIFT modifier
				/// </summary>
				[LocalizedCategory("ModifiersCategory", EBCategories::typeid)]
				property bool Shift { bool get(); void set(bool value); }
			};

		}
	}
}