#pragma once

#include "MParsedControls.h"
#include "UITypeEditors.h"

using namespace Aga::Controls::Tree;
using namespace Microarea::TaskBuilderNet::Interfaces;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	ref class MTreeView;
	ref class MParsedControl;


	/// <summary>
	/// Class for managing the  tree view control
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(MParsedControlSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	public ref class MTreeView : BaseWindowWrapper
	{
		MESSAGE_HANDLER_EVENT(SelectionChanged, EasyBuilderEventArgs, "Occurs when tree view selection is changed.");
		MESSAGE_HANDLER_EVENT(ContextMenuItemClick, EasyBuilderEventArgs, "Occurs when context menu item is clicked.");

	private:

		TreeViewAdv^			treeViewUC;		//puntatore al tree c#, usato per eventi e chiamate a funzioni
		CTreeViewAdvCtrl*		m_pTreeView;	//usato per ottenere il namespace
		System::IntPtr handle;
		System::Collections::Generic::List<System::String^>^		images;
		void InitializeTree();
		System::String^ GetImageKeyFromIndex(int index);
	
	public:
		/// <summary>
		/// Gets the namespace for the treeview
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::DesignerRuntime),
			System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property INameSpace^ Namespace { virtual INameSpace^ get () override; }
	
		/// <summary>
		/// Gets or Sets the name for the treeview
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Name { virtual System::String^ get () override; virtual void set (System::String^ value) override; }

		/// <summary>
		/// Gets  the class type for the treeview
		/// </summary>
		[LocalizedCategory("InformationsCategory", EBCategories::typeid), System::ComponentModel::TypeConverter(System::ComponentModel::ExpandableObjectConverter::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved), System::ComponentModel::EditorAttribute(ControlClassUITypeEditor::typeid, System::Drawing::Design::UITypeEditor::typeid),  System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property ControlClass^ ClassType { virtual ControlClass^ get (); }
	
		/// <summary>
		/// Gets the class name for the treeview
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::String^ ClassName { virtual System::String^ get () override; }
		/// <summary>
		/// Gets or Sets the Handle for the current control
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property System::IntPtr	Handle { virtual System::IntPtr get () override {return handle;} virtual void set (System::IntPtr handle) override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property System::Int32 TbHandle { virtual System::Int32 get() override; }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		virtual property System::String^ SerializedType	{ virtual System::String^ get () override; }

		/// <summary>
		/// Gets or Sets the size for the treeview
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid)]
		property System::Drawing::Size Size	{ virtual System::Drawing::Size get () override; virtual void set (System::Drawing::Size value) override; }

		/// <summary>
		/// Sets the desired location for the current control
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::Drawing::Point Location { virtual void set(System::Drawing::Point value) override; }


		/// <summary>
		/// Gets or Sets the AutoStretch property for the treeview
		/// </summary>
		[System::ComponentModel::Browsable(false),
			System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)
		]
		property bool AutoStretch { virtual void set (bool value) override; }

		/// <summary>
		/// Gets or Sets the BottomStretch property for the bodyedit
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid),
			TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)
		]
		property bool BottomStretch { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// Gets or Sets the RightStretch property for the bodyedit
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid),
			TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)
		]
		property bool RightStretch { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// Gets or Sets the AutoFill property for the bodyedit
		/// </summary>
		[LocalizedCategory("GeneralCategory", EBCategories::typeid),
			TBPropertyFilterAttribute(TBPropertyFilters::ComponentState | TBPropertyFilters::DesignerRuntime)
		]
		property bool AutoFill { virtual bool get() override; virtual void set(bool value) override; }

		/// <summary>
		/// true if the window has a thin-line border
		/// </summary>
		[System::ComponentModel::Browsable(false)]
		property bool Border {
				virtual bool get() override;
				virtual void set(bool value) override;
		}

		/// <summary>
		/// Gets or Sets the text for the current control
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Text { virtual System::String^ get () override {return nullptr;}  virtual void set (System::String^ value)override{}  }

		/// <summary>
		/// Gets or Sets the image list that contains the image objects that are used by the tree nodes
		/// </summary>
		[LocalizedCategory("GraphicsCategory", EBCategories::typeid), TBPropertyFilter(TBPropertyFilters::IsCodeBehindInvolved | TBPropertyFilters::DesignerRuntime), 
			System::ComponentModel::EditorAttribute(IMAGES_UITYPEEDITORFULLQUALIFIEDNAME, System::Drawing::Design::UITypeEditor::typeid), 
			System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Content)]
		property System::Collections::Generic::List<System::String^>^ Images { System::Collections::Generic::List<System::String^>^ get ();  void set (System::Collections::Generic::List<System::String^>^ value); }


	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		/// <param name="handleWndPtr">is the c++ handle of the bodyedit which the control refers wraps to</param>
		MTreeView (System::IntPtr handleWndPtr);
		
		/// <summary>
		/// Constructor: 
		/// </summary>
		/// <param name="parentWindow">wrapper of the parent window</param>
		/// <param name="name">the name of the control</param>
		/// <param name="controlClass">the specific class name for the control</param>
		/// <param name="location">the location of the control</param>
		/// <param name="hasCodeBehind">true indicates that the wrapper refers to an origina taskbuilder object</param>
		MTreeView (IWindowWrapperContainer^ parentWindow, System::String^ name, System::String^ controlClass, System::Drawing::Point location, bool hasCodeBehind);

		/// <summary>
		/// Distructor
		/// </summary>
		!MTreeView();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		~MTreeView();

		/// <summary>
		/// Add node to the current TreeView (to the selected/current node)
		/// </summary>
		void AddNode(System::String^  sText, System::String^  sNodeKey, System::String^  sImage);


		/// <summary>
		/// Add node to the current TreeView (to the selected/current node)
		/// </summary>
		void AddNode(System::String^ sText, System::String^ sNodeKey, int imageIndex);	

		/// <summary>
		/// Clear all nodes
		/// </summary>
		void ClearTree();

		/// <summary>
		/// Select the specified node 
		/// </summary>
		void SelectNode (System::String^ sNodeKey);
	
		/// <summary>
		/// Expand/Collapse the specified node
		/// </summary>
		void ToggleNode(System::String^ sNodeKey);

		/// <summary>
		/// Returns the key of the selected node
		/// </summary>
		System::String^ MTreeView::GetSelectedNodeKey();

		/// <summary>
		/// Insert a child node in the node with key passed as first argument
		/// </summary>
		void InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, System::String^ image);

		/// <summary>
		/// Insert a child node with tooltip in the node with key passed as first argument
		/// </summary>
		void InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, System::String^ image, System::String^ toolTip);

		/// <summary>
		/// Insert a child node in the node with key passed as first argument
		/// </summary>
		void InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, int imageIndex);

				/// <summary>
		/// Insert a child node in the node with key passed as first argument
		/// </summary>
		void InsertChild(System::String^ parentKey, System::String^ text, System::String^ nodeKey, int imageIndex, System::String^ toolTip);

		/// <summary>
		/// Insert a child node in the node with key passed as first argument
		/// </summary>
		void AddContextMenuItem(System::String^ menuItem);

		/// <summary>
		/// Check the specified node
		/// </summary>
		void SetMenuItemCheck(System::String^ itemMenu, bool check);

		/// <summary>
		/// Add a submenu to specified node
		/// </summary>
		void AddContextSubMenuItem(System::String^ itemMenu, array<System::String^>^ subItems);

		/// <summary>
		/// Enable/disable the specified node
		/// </summary>
		void SetMenuItemEnable(System::String^ itemMenu, bool enabled);

		/// <summary>
		/// Set specified node as selected node
		/// </summary>
		bool	SetNodeAsSelected				(System::String^ sNodeKey);
	
		/// <summary>
		/// Retrieve true if a node exists, false otherwise
		/// </summary>
		bool	ExistsNode						(System::String^ sNodeKey);

		/// <summary>
		/// Get the parent key of the specified node
		/// </summary>
		System::String^ GetParentKey					(System::String^ sNodeKey);
		
		/// <summary>
		/// Expand all nodes in the tree
		/// </summary>
		void	ExpandAll						();

		/// <summary>
		/// Collapse all nodes in the tree
		/// </summary>
		void	CollapseAll						();

		/// <summary>
		/// Add a separator to the context menu
		/// </summary>
		void	AddContextMenuSeparator			();

		/// <summary>
		/// Get the text of context menu item clicked
		/// </summary>
		System::String^	GetTextContextMenuItemClicked	();

		/// <summary>
		/// Add an image to the tree
		/// </summary>
		void AddImage(System::String^ imageKey);

		/// <summary>
		/// Delete the specified node
		/// </summary>
		void	DeleteNode						(System::String^ sNodeKey);

		/// <summary>
		/// Enable the specified node
		/// </summary>
		void	Enable							(bool bValue);		

		/// <summary>
		/// Event raised on node selection changed
		/// </summary>
		void ManagedSelectionChanged (System::Object^ sender, System::EventArgs^ args);

		/// <summary>
		/// Event raised on context menu item click
		/// </summary>
		void ManagedContextMenuItemClick (System::Object^ sender, System::EventArgs^ args);
		
		
		/// <summary>
		/// Returns the image keys of tree control
		/// </summary>
		System::Collections::Generic::List<System::String^>^ GetImageKeys();
		
		/// <summary>
		///Internal use
		/// </summary>
		void OnWndProc (System::Object^ sender, WndProcEventArgs^ args);
		/// <summary>
		///Internal use
		/// </summary>
		void OnAfterWndProc (System::Object^ sender, WndProcEventArgs^ args);
	protected:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual int GetNamespaceType () override;

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
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Initialize () override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::ComponentModel::ComponentCollection^ Components { virtual  System::ComponentModel::ComponentCollection^ get () { return nullptr; /*TODO SILVANO*/ } }

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual bool HasComponent (System::String^ controlName) { return false; /*TODO SILVANO*/ }
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual System::ComponentModel::IComponent^ GetComponent (System::String^ controlName) { return nullptr;  /*TODO SILVANO*/ }

	};
}}}
