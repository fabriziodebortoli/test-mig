#pragma once
#include "../TBVersion.h"

#include "EasyBuilderComponents.h"
#define IMAGES_UITYPEEDITORFULLQUALIFIEDNAME		"Microarea.EasyBuilder.UI.ImagesUITypeEditor, Microarea.EasyBuilder, Version="##STR(TB_PRODUCTVERSION)##", Culture=neutral, PublicKeyToken=null"
#define IMAGE_UITYPEEDITORFULLQUALIFIEDNAME		"Microarea.EasyBuilder.UI.ImageUITypeEditor, Microarea.EasyBuilder, Version="##STR(TB_PRODUCTVERSION)##", Culture=neutral, PublicKeyToken=null"

using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Interfaces::View;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;

class CAbstractFormView;
class CRegisteredParsedCtrl;

namespace Microarea { namespace Framework { namespace TBApplicationWrapper
{
	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class PropertyChangingNotifier
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		static void OnComponentPropertyChanged	(	
													System::IServiceProvider^ provider,
													System::ComponentModel::IComponent^				component,
													System::String^					changingPropertyName,
													System::Object^					oldValue,
													System::Object^					newValue
												);
		/// <summary>
		/// Internal Use
		/// </summary>
		static void OnComponentAdded			(	
													System::ComponentModel::IComponent^		parentComponent,
													System::ComponentModel::IComponent^		component,
													bool			updateSources
												);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class EditorImages
	{	
	public:
		static System::Windows::Forms::ImageList^ ViewModelImages;
		static System::Windows::Forms::ImageList^ DataModelImages;
		//questi enumerativi devono essere allineati a quelli definiti in \TaskBuilderNet\Microarea.EasyBuilder\UI\ImageLists.cs

		enum class ObjectModelImageIndex { Database = 0, Table = 1, Field = 2, DatabaseItem = 3, Document = 4, Root = 5, Tabber = 6, Tab = 7, Master = 8, Slave = 9, SlaveBuffered = 10, HotLink = 11, DataManagerNode = 12, DataManager = 13, None = 14, HotLinks = 15, TabStopField = 16, KeyDatabaseItem = 17, BusinessObject = 18, BusinessObjects = 19, LocalFields = 20, BodyEdit = 21, BodyEditColumn = 22, BodyEditColumnInvisible = 23, DBTS = 24, MVCController = 25, MVCDocument = 26, MVCView = 27, Behaviours = 28, PredefinedBehaviour = 29, UserDefinedBehaviour = 30, InvalidObject = 31, Toolbar = 32, ToolbarItem = 33, TileManager = 34, TileGroup = 35, TileDialog = 36, TilePanel = 37, TilePanelTab = 38 };
		enum class ViewModelImageIndex { Formatter = 0, Class = 1 };
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class ControlClassUITypeEditor : System::Drawing::Design::UITypeEditor
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^					EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	
	private:
		void		listBox_MouseClick(System::Object^ sender, System::Windows::Forms::MouseEventArgs^ e);
		System::Windows::Forms::TreeNode^	GetFamilyNode			(System::Windows::Forms::TreeNode^ rootNode, System::String^ name);
		System::Windows::Forms::TreeView^	BuildControlTree		(
												CRegisteredParsedCtrl* pOldValueCtrl, 
												IControlClass^ oldValue, 
												IControlClassConsumer^ consumer
											);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class FormatterUITypeEditor: System::Drawing::Design::UITypeEditor
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^	EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	
	private:
		void listBox_MouseClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ e);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class DBTOneToManyUITypeEditor : System::Drawing::Design::UITypeEditor
	{
		
	public:
		DBTOneToManyUITypeEditor ();

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	
		/// <summary>
		/// Internal Use
		/// </summary>
		void listBox_MouseClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ e);
	};

	ref class EasyBuilderControl;
	

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class EasyBuilderComponentsEditor : System::ComponentModel::Design::CollectionEditor
	{
	private:
		EasyBuilderComponents^	currentCollection;
		System::Windows::Forms::PropertyGrid^			propertyGrid;
		System::Windows::Forms::Control^				addButton;
		bool					changesCancelled;

	public:
		EasyBuilderComponentsEditor (System::Type^ type);
		~EasyBuilderComponentsEditor ();
		!EasyBuilderComponentsEditor ();

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^						EditValue				(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, System::Object^ value) override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle				GetEditStyle			(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::ComponentModel::Design::CollectionEditor::CollectionForm^ 	CreateCollectionForm	() override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual void								CancelChanges			() override;

	protected:
		virtual bool	CanRemoveInstance	(System::Object^ value) override;
		virtual Object^ CreateInstance		(System::Type^ itemType) override;

		virtual void SelectedObjectChanged	(Object^ sender, System::EventArgs^ e);
				void OnOkCancelClicked		(Object^ sender, System::EventArgs^ e);
				void OnCollectionFormClosing(Object^ sender, System::ComponentModel::CancelEventArgs^ e);
				void AddButtonEnabledChanged(Object^ sender, System::EventArgs^ e);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class QueryUITypeEditor: System::Drawing::Design::UITypeEditor
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^	EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class HotLinkFieldEditor: System::Drawing::Design::UITypeEditor
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^	EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		void listView_MouseClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ e);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class ItemSourcesUITypeEditor : System::ComponentModel::Design::CollectionEditor
	{
	public:
		ItemSourcesUITypeEditor (System::Type^ type);

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle	(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ EditValue (System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Object^ CreateInstance (System::Type^ itemType) override;
	protected:
		virtual System::Type^ CreateCollectionItemType() override;
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class HotLinkSearchesUITypeEditor : EasyBuilderComponentsEditor
	{
	public:
		HotLinkSearchesUITypeEditor (System::Type^ type);

	public:	
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual bool	CanRemoveInstance	(Object^ value) override;
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ EditValue (System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class HotLinkUITypeEditor : System::Drawing::Design::UITypeEditor
	{
	public:
		HotLinkUITypeEditor ();

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	
		/// <summary>
		/// Internal Use
		/// </summary>
		void listBox_MouseClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ e);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class DocumentUITypeEditor : System::Drawing::Design::UITypeEditor
	{
	public:
		DocumentUITypeEditor ();

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;
	};

	//=============================================================================
	/// <remarks/>
	ref class LocalHost;
	ref class MDataObj;
	public ref class DataObjUITypeEditor : System::Drawing::Design::UITypeEditor
	{
		LocalHost^ panel;

		public:
			void OnAttachPropertyEditor(System::Windows::Forms::TextBox^ textBox, MDataObj^ dataObj);
			void OnDetachPropertyEditor(System::Windows::Forms::TextBox^ textBox, MDataObj^ mDataObj);
			void Clear();
		
		internal:
			void TextBox_SizeChanged(System::Object^ sender, System::EventArgs^ e);
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class DataFileUITypeEditor : System::Drawing::Design::UITypeEditor
	{
	public:
		DataFileUITypeEditor ();

	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override; 
		
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^ EditValue	(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;

	private:
		void		BuildTree					(System::Windows::Forms::TreeView^ treeView, Object^ oldValue, bool useCulture);
		System::Windows::Forms::TreeNode^	GetNode						(System::Windows::Forms::TreeNodeCollection^ nodes, System::String^ text, int imageIndex);
		void		treeView_MouseClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ e);

	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class ModuleNamespaceUITypeEditor : System::Drawing::Design::UITypeEditor
	{
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		virtual System::Drawing::Design::UITypeEditorEditStyle	GetEditStyle(System::ComponentModel::ITypeDescriptorContext^ context) override;

		/// <summary>
		/// Internal Use
		/// </summary>
		virtual Object^	EditValue(System::ComponentModel::ITypeDescriptorContext^ context, System::IServiceProvider^ provider, Object^ value) override;

	private:
		void MouseClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ e);
	};

}
}
}

