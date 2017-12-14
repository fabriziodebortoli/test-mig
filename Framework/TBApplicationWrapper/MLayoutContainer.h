#pragma once

#include "MParsedControls.h"
#include "MEasyBuilderContainer.h"

using namespace System::Collections::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Interfaces::View;
using namespace System::ComponentModel;


namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{

	public enum class EContainerLayout	{ Stripe = CLayoutContainer::STRIPE, Column = CLayoutContainer::COLUMN, Hbox = CLayoutContainer::HBOX, Vbox = CLayoutContainer::VBOX };
	public enum class ELayoutAlign		{ NoAlign = CLayoutContainer::NO_ALIGN, Begin = CLayoutContainer::BEGIN, Middle = CLayoutContainer::MIDDLE, End = CLayoutContainer::END, Stretch = CLayoutContainer::STRETCH, StretchMax = CLayoutContainer::STRETCHMAX	};


	/// <summary>
	/// Wrapper class to the original c++ document context
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	public ref class MLayoutComponent : public EasyBuilderComponent, ILayoutComponent
	{
	protected:
		LayoutElement*		m_pLayoutElement;

	private:
		IComponent^			linkedComponent;
		INameSpace^			nameSpace;

	public:
		MLayoutComponent (System::IntPtr containerPtr);
	
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property IComponent^ LinkedComponent { virtual IComponent^ get(); virtual void set(IComponent^ value);   }

		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property ILayoutComponent^ LayoutObject { virtual ILayoutComponent^ get() { return this; }  }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::String^ LayoutDescription { virtual  System::String^ get(); }

		System::IntPtr	GetPtr();

		/// <summary>
		/// Gets the namespace of the current control
		/// </summary>
		property INameSpace^ Namespace { virtual INameSpace^ get(); virtual void set(INameSpace^ value);}

		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals(System::Object^ obj) override;

	
		// metodo necessari per accedere bene al C++
		LayoutElement* GetLayoutElement();
		virtual const	LayoutElementArray*	GetContainedElements();

		// metodo static
		static System::String^ GetNameFrom(INameSpace^ ns);
		static MLayoutComponent^ Create(LayoutElement* pElement);

	protected:
		BaseWindowWrapper^		AsBaseWindowWrapper();
	private:
		System::String^ TypeDescription(System::Type^ type);
	};

	/// <summary>
	/// Wrapper class to the original c++ document context
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	public ref class MLayoutContainer : public MLayoutComponent
	{
		CLayoutContainer* m_pLayoutContainer;
	
	public:
		static System::String^ defaultName = "Container";
	
	public:
		MLayoutContainer(System::IntPtr containerPtr);

		[ExcludeFromIntellisense]
		property ELayoutAlign LayoutAlign { virtual ELayoutAlign get(); virtual void set(ELayoutAlign value);   }

		[ExcludeFromIntellisense]
		property EContainerLayout Layout { virtual EContainerLayout get(); virtual void set(EContainerLayout value);   }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::String^ LayoutDescription { virtual  System::String^ get() override; }

		virtual const	LayoutElementArray*	GetContainedElements() override;
	};

	/// <summary>
	/// Wrapper class to the original c++ document context
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	public ref class MLayoutObject : public MEasyBuilderContainer, ILayoutComponent
	{
	private:
		MLayoutComponent^	layoutObject;

	public:
		MLayoutObject(MLayoutComponent^ layoutObject);
	
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property IComponent^ LinkedComponent { virtual IComponent^ get(); virtual void set(IComponent^ value);   }

		/// <summary>
		/// Gets the namespace of the current control
		/// </summary>
		property INameSpace^ Namespace { virtual INameSpace^ get(); virtual void set(INameSpace^ value); }

		/// <summary>
		/// Gets the namespace of the current control
		/// </summary>
		property System::String^ Name { virtual System::String^ get() override; }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::String^ LayoutDescription { virtual  System::String^ get(); }

		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property ILayoutComponent^ LayoutObject { virtual ILayoutComponent^ get() { return layoutObject; }  }

		IComponent^		FindLinkedComponent(INameSpace^ ns, IContainer^ container);
		MLayoutObject^	FindLayoutObjectOn(INameSpace^ ns);
		void			RemoveLayoutObjectOn(INameSpace^ ns);
		void			LayoutChangedFor	(INameSpace^ ns);

		WindowWrapperContainer^ AsWindowWrapperContainer();

		void AddContainer(CLayoutContainer* pContainer, IComponent^ linkedComponent);
		void AddElement	(LayoutElement* pElement, IComponent^ linkedComponent);

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanCallCreateComponents() override;

		virtual void CallCreateComponents() override;
		virtual void OnAfterCreateComponents() override;
		

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Add(System::ComponentModel::IComponent^ component, System::String^ name) override;

		/// <summary>
		/// Override of the equals method, true if the compared tabs are the same
		/// </summary>
		virtual bool Equals(System::Object^ obj) override;
	
	private:
		void RecursiveOnAfterCreateComponent();
		WindowWrapperContainer^ GetParentWindow();
	};
}
}
}
