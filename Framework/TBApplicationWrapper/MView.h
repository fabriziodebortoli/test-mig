#pragma once

using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Interfaces::View;

#include "MLayoutContainer.h"
#include "MTabber.h"
#include "MTileManager.h"
#include "EasyBuilderComponents.h"
#include "MPanel.h"

class CAbstractFormView;

namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			
		
	/// <summary>
	/// Wrapper class for the taskbuilder CAbstractFormView class
	/// </summary>
	//=============================================================================
	[ExcludeFromIntellisense]
	public ref class MFrame : WindowWrapperContainer
	{
		WindowWrapperContainer^ view;

	public:
		MFrame(System::IntPtr framePtr, WindowWrapperContainer^ view);

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual	bool WndProc (System::Windows::Forms::Message% m) override;
	};

	/// <summary>
	/// Wrapper class for the taskbuilder CAbstractFormView class
	/// </summary>
	//=============================================================================
	[ExcludeFromIntellisense]
	public ref class MView : WindowWrapperContainer
	{
		static System::String^ defaultToolbarName = "Toolbar";
		
	private:
		CAbstractFormView*		m_pView;
		MFrame^					frame;
		MLayoutObject^			layoutObject;
		bool					designerVisible;
		bool					bVisible;
		bool					suspendLayout;
		System::String^			pathToSerialize;
				
	public:
		/// <summary>
		/// Internal Use: Initializes a new instance of MDBTObject
		/// </summary>
		MView(System::IntPtr handleViewPtr);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MView(void);
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MView();

	protected:

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool DesignerVisible { virtual bool get() override { return designerVisible; } }


	public:
		/// <summary>
		/// Gets the identifier of the current control
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Id { virtual System::String^ get() override; }

		/// <summary>
		/// Gets a value indicating controller is admitted without view 
		/// </summary>
		//-----------------------------------------------------------------------------
		static System::IntPtr NoView = System::IntPtr::Zero;

		/// <summary>
		/// Next tool bar button pos
		/// </summary>
		//-----------------------------------------------------------------------------
		static System::Drawing::Point  NextTBPos = System::Drawing::Point(0,0);
		/// <summary>
		/// Next tool level
		/// </summary>
		//-----------------------------------------------------------------------------
		static BYTE   toolBarLevel = 0;

		/// <summary>
		/// Gets the name of the view
		/// </summary>
		property System::String^ ViewName { virtual System::String^ get (); }
		
		/// <summary>
		/// Gets the namespace of the view
		/// </summary>
		property INameSpace^ Namespace { virtual INameSpace^ get () override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property bool DesignMode { virtual  bool get() override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property EDesignMode DesignModeType { virtual  EDesignMode get() override; }
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property System::IntPtr DocumentPtr	{ virtual  System::IntPtr get () override; }
			
		/// <summary>
		/// Gets or sets the background image of the view (can be a file path or an image namespace)
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
			System::ComponentModel::EditorAttribute(IMAGE_UITYPEEDITORFULLQUALIFIEDNAME, System::Drawing::Design::UITypeEditor::typeid)]
		property System::String^ BackgroundImage { System::String^ get (); void set(System::String^ value); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense, System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool SuspendLayout { bool get() { return suspendLayout; } void set(bool value) { suspendLayout = value; }}

		/// <summary>
		/// Gets the label for the current control
		/// </summary>
		[System::ComponentModel::Localizable(true), System::ComponentModel::Browsable(false),
			TBPropertyFilter(TBPropertyFilters::StringTypes | TBPropertyFilters::NumericTypes | TBPropertyFilters::RealTypes | TBPropertyFilters::BoolTypes)]
		property System::String^ ControlLabel { virtual System::String^ get() override;  }

 		/// <summary>
		/// Gets the size of the view
		/// </summary>
		property System::Drawing::Size TotalSize { System::Drawing::Size get (); }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void SwitchVisibility(bool visible) override { designerVisible = visible; __super::SwitchVisibility(visible); }

		///<summary>
		///Updates nedeed attributes for json serialization
		///</summary>
		virtual void GenerateJson(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization) override;

		///<summary>
		///Updates needed attributes for json serialization 
		///</summary>
		virtual void UpdateAttributesForJson(CWndObjDescription* pParentDescription) override;

		///<summary>
		///Generates serialization for the class
		///</summary>
		virtual void GenerateSerialization(CWndObjDescription* pParentDescription, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization) override;
		
		///<summary>
		///Set the path to serialize to
		///</summary>
		void SetPathToSerialize(System::String^ path);

		///<summary>
		///Set the newDocument true or false if document is created by ES or not
		///</summary>
		bool IsDynamicDocument();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		property System::Drawing::Rectangle FrameArea { System::Drawing::Rectangle get (); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		property System::IntPtr FrameHandle{ System::IntPtr get (); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property IWindowWrapper^ ToolBar { IWindowWrapper^ get (); }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property MLayoutObject^ LayoutObject { MLayoutObject^ get(); }

	protected:
		void ScrollToPosition (System::Drawing::Point pos);

	private:
		void GetFrameChildrenFromPos(System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren);
		void ResizeFrame();
		void EventsJsonStringDeserialize(const CString& strEvents, CJsonSerializer& jsonSer, int& idx);
		bool ManageSerializations(List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization);
		bool ManageDocumentOutline();
		bool ManageClientForms(List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization);
		bool CreateClientFormChild(CXMLNode* pClientForms, List<System::Tuple<System::String^, System::String^, System::Boolean>^>^ serialization, const CString& sServerId);
		
	public:
		///<summary>
		///Save the json string in the file specified with fileName
		///</summary>
		bool SaveSerialization(const CString& fileName, const CString& sSerialization);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetTabberHandle (System::String^ name);
			
		/// <summary>
		/// Gets the tabber related to the specifed name
		/// </summary>
		MTabber^ GetTabberByName (System::String^ name);

		/// <summary>
		/// PreTranslateMsg
		/// </summary>
		[ExcludeFromIntellisense]
		void PreTranslateMsgKey (UINT Msg, WPARAM wParam, LPARAM lParam);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		void MoveTileGroup(MTileGroup^ tileGroup, int newIndex);

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void AfterTargetDrop(System::Type^ droppedType) override;

		/// <summary>
		/// Gets the IWindowWrapper control related to the specified namespace
		/// </summary>
		virtual IWindowWrapper^	GetControl (INameSpace^ nameSpace) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual HWND GetControlHandle(const CTBNamespace& aNamespace) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual	void AfterWndProc(System::Windows::Forms::Message% m) override;

		/// <summary>
		/// Updates the client area of the specified window
		/// </summary>
		virtual void UpdateWindow() override;

		/// <summary>
		/// Forces the refresh of the current window in the specified rectangle area
		/// </summary>
		virtual void Invalidate(System::Drawing::Rectangle screenCoordRect) override;

		/// <summary>
		/// Forces the refresh of the current window
		/// </summary>
		virtual void Invalidate() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		IWindowWrapper^ GetParsedCtrlLink(INameSpace^ nameSpace);
			
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;


		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		virtual property System::String^ SerializedName	{ virtual System::String^ get () override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property System::String^ SerializedType	{ virtual System::String^ get ()  override; }


		//Sovrascrivo Location e Size ereditate perchè la view non deve essere ne spostata nè ridimensionata né nascosta.
		/// <summary>
		/// Gets or sets the location 
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		virtual property System::Drawing::Point Location	{ virtual System::Drawing::Point get () override ; virtual void set (System::Drawing::Point value) override ; }
		
		/// <summary>
		/// Gets or sets the visible attribute of the tab
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		virtual property bool Visible		
		{
			virtual bool get () override { return bVisible; }
			virtual void set (bool value) override 
			{ 
				bVisible = value;
				CWnd* pWnd = GetWnd();
				if (!pWnd)
					return;
				pWnd->ShowWindow(bVisible ? SW_SHOW : SW_HIDE);
			}
		}
		
		/// <summary>
		/// Gets or Sets the name of the view
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		virtual property System::String^ Name		
		{
			virtual System::String^ get () override { return __super::Name; }
			virtual void set (System::String^ value) override { __super::Name = value;} 
		}
		
		/// <summary>
		/// Gets or Sets the size of the view
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		virtual property System::Drawing::Size Size	{ virtual System::Drawing::Size get () override ; virtual void set (System::Drawing::Size value) override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		virtual void RequestRelayout();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CanDropTarget (System::Type^ droppedObject) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual bool CanUpdateTarget (System::Type^ droppedObject) override { return false; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual void UpdateTargetFromDrop (System::Type^ droppedType) override { }

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

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void GetChildrenFromPos	(System::Drawing::Point p, System::IntPtr handleToSkip, System::Collections::Generic::ICollection<IWindowWrapper^>^ foundChildren) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual	bool FrameWndProc (System::Windows::Forms::Message% m);

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool CreateWrappers (array<System::IntPtr>^ handlesToSkip) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void CallCreateComponents() override;

		void	LayoutChangedFor		(INameSpace^ ns);
		void	RemoveLayoutObjectOn	(INameSpace^ ns);
	};
}
}
}

