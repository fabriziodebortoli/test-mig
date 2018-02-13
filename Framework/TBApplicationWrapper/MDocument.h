#pragma once

#include <TbGes\EXTDOC.H>
#include "MDBTObjects.h"
#include "MHotLink.h"
#include "MExpression.h"

#include <TBGes\ExtDoc.h>


using namespace System::Collections::Generic;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;


namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	public enum class LockResult { Locked = CAbstractFormDoc::ALL_LOCKED, NoDataToLock = CAbstractFormDoc::NO_AUX_DATA, LockFailed = CAbstractFormDoc::LOCK_FAILED }; 

	/// <summary>
	/// Internal use: serializes a document
	/// </summary>
	//================================================================================
	public ref class DocumentSerializer : EasyBuilderSerializer
	{
	public:
		static System::String^ GetFieldPtrMethodName = "GetFieldPtr";

	public:
		/// <remarks />
		virtual void GenerateFields(IContainer^ container, TypeDeclaration^ classStructure) override;
		/// <remarks />
		virtual System::Object^ Serialize (System::ComponentModel::Design::Serialization::IDesignerSerializationManager^ manager, System::Object^ current) override;
		/// <remarks />
		virtual TypeDeclaration^ SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ component) override;
		/// <remarks />
		virtual IList<Statement^>^  GetAdditionalCreateComponentsStatements(IContainer^ container, IList<System::String^>^ memberDeclaration) override;
		
	protected:
		property System::Type^		ComponentSerializedAs	{ virtual System::Type^ get (); }
	};

	/// <summary>
	/// Wrapper class to the original c++ document context
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	public ref class DocumentContext
	{
	private:
		bool hasCodeBehind;

	private:
		CTBContext*		m_pContext;

	public:
		DocumentContext(System::IntPtr sqlConnectionPtr);
		~DocumentContext();
		!DocumentContext();
	
	internal:
		DocumentContext (CTBContext* pContext);

	internal:
		CTBContext* GetContext();

	public:
		/// <summary>
		/// Return the context readable sql session
		/// </summary>
		virtual System::IntPtr GetReadOnlySessionPtr ();

		/// <summary>
		/// Return the context updatable sql session
		/// </summary>
		virtual System::IntPtr GetUpdatableSessionPtr ();

		/// <summary>
		/// Begins a new local transaction for the active session
		/// </summary>
		bool				StartTransaction();
		
		/// <summary>
		/// Commits the local transaction
		/// </summary>
		void				Commit();

		/// <summary>
		/// Rollback the local transaction
		/// </summary>
		void				Rollback();
	};

	
	/// <summary>
	/// Internal Use
	/// </summary>
	//================================================================================
	public ref class MXMLVariable
	{
	private:
		CXMLVariable* m_pXMLVariable;
	
	public:
		MXMLVariable(CXMLVariable*);

		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^	Name	{ System::String^	get(); }
		property MDataObj^			DataObj	{ MDataObj^			get(); }	
	};

	/// <summary>
	/// Internal Use
	/// </summary>
	//=============================================================================
	public ref class MXMLVariableArray 
	{	
	private:
		CXMLVariableArray* m_pXMLVariableArray;		

	public:
		MXMLVariableArray(CXMLVariableArray* pXMLVariableArray);

		/// <summary>
		/// Internal Use
		/// </summary>
		property System::Collections::Generic::List<MXMLVariable^>^ XMLVariables { System::Collections::Generic::List<MXMLVariable^>^ get(); }
	
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		MXMLVariable^	GetVariable(System::String^ name);
	};
	/// <summary>
	/// Wrapper class to the original c++ document
	/// </summary>
	//================================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(DocumentSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::PropertyTabAttribute(System::Windows::Forms::Design::EventsTab::typeid, System::ComponentModel::PropertyTabScope::Component)]
	public ref class MDocument : MEasyBuilderContainer, IDocumentDataManager
	{
	protected:
		AbstractFormDocPtr*					m_ppDocument;
		DocumentContext^					context;
	
	private:
		NameSpace^							nameSpace;
		MDBTMaster^							master;
		bool								componentsCreated;
		bool								wrapExistingObjectsInRunning;
		CStringArray*						m_pInvalidDBTs;

				generic<class T> where T : MDocument
	static T Create(System::String^ docNamespace, bool unattendedMode, bool invisible, DocumentContext^ context, bool isExposing);
		
	public:
		/// <summary>
		/// Internal Use
		/// </summary>
		generic<class T> where T : MDocument
		[ExcludeFromIntellisense]
		static T Create(System::String^ documentNamespace);

		/// <summary>
		/// Internal Use, effettua anche la create component per valorizzare master e slave
		/// </summary>
		generic<class T> where T : MDocument
			[ExcludeFromIntellisense]
		static T Create(System::IntPtr documentPtr);

		/// <summary>
		/// Internal Use
		/// </summary>
		generic<class T> where T : MDocument
		[ExcludeFromIntellisense]
		static T Create(System::String^ documentNamespace, DocumentContext^ context);
		/// <summary>
		/// Internal Use
		/// </summary>
		generic<class T> where T : MDocument
		[ExcludeFromIntellisense]
		static T CreateUnattended(System::String^ documentNamespace, DocumentContext^ context);
		/// <summary>
		/// Internal Use
		/// </summary>
		generic<class T> where T : MDocument
		[ExcludeFromIntellisense]
		static T CreateHidden(System::String^ documentNamespace, DocumentContext^ context);
		/// <summary>
		/// Internal Use
		/// </summary>
		generic<class T> where T : MDocument
		[ExcludeFromIntellisense]
		static T CreateForSerialization(System::String^ documentNamespace);

	public:
		SymTable* GetSymTable();
		/// <summary>
		/// Gets or sets the Namespace of the current Document
		/// </summary>
		property INameSpace^ Namespace { virtual INameSpace^ get ();  virtual void set (INameSpace^);}
		
		/// <summary>
		/// Gets the Master of the current Document
		/// </summary>
		property IDocumentMasterDataManager^ Master { virtual IDocumentMasterDataManager^ get (); }
	    
		/// <summary>
		/// Gets the current FormMode for the Document (None, Browse, New, Edit, Find)
		/// </summary>
		property FormModeType FormMode { virtual FormModeType get(); void set (FormModeType value); }


		/// <summary>
		/// Get/Set document modified state
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	Modified { virtual bool get(); virtual void set(bool value); }
		
		/// <summary>
		/// Returns true if this document is a batch
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	Batch { virtual bool get(); }
		
		/// <summary>
		/// Returns true if the batch procedure has been aborted by the user
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	BatchAborted { bool get() { return GetDocument() ? GetDocument()->m_BatchScheduler.IsAborted() == TRUE : false; }}

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property bool DesignMode { virtual  bool get () override; }

		/// <summary>
		/// Returns true if the batch procedure has been aborted by the user
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	WrapExistingObjectsInRunning { bool get() { return wrapExistingObjectsInRunning; } void set(bool bValue ) { wrapExistingObjectsInRunning =  bValue; }}

		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ ControllerType { virtual System::String^ get () override; }

		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool AutoValueChanged { virtual bool get (){ return false; } virtual void set (bool value) {/*non fa nulla*/}}

		[System::ComponentModel::Browsable(false), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::Int32 TbHandle { virtual System::Int32 get() override; }

		/// <summary>
		/// Get document CanClose state
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	CanClose { bool get(); }

		/// <summary>
		/// Get document BatchRunning 
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	BatchRunning { bool get(); }
		
		/// <summary>
		/// Get/Set document BatchCloseAfterExecution state
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	BatchCloseAfterExecution { virtual bool get(); virtual void set(bool value); }

		/// <summary>
		/// true if the underlying document is alive
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool	IsAlive { virtual bool get(){ return GetDocument()!= NULL; } }

		/// <summary>
		/// Gets the Document Context used to manage diagnostic session and transaction 
		/// </summary>
		property DocumentContext^ Context { virtual DocumentContext^ get (); }

	public:
		/// <summary>
		/// Internal Use: Constructor
		/// </summary>
		MDocument (System::IntPtr pDocument);
		
		/// <summary>
		/// Distructor
		/// </summary>
		~MDocument();
		
		/// <summary>
		/// Finalizer
		/// </summary>
		!MDocument();
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		static void AddDynamicDocumentObject(INameSpace^ docNamespace, INameSpace^ templateDocNamespace, System::String^ title, bool isBatch);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		CAbstractFormDoc* GetDocument() { return (m_ppDocument && m_ppDocument->operator CAbstractFormDoc*()) ? m_ppDocument->operator->() : NULL; }
		//CAbstractFormDoc* GetDocument() { return m_ppDocument ? m_ppDocument->operator->() : NULL; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::IntPtr GetDocumentPtr() { return (System::IntPtr)GetDocument(); }

		/// <summary>
		/// Internal Use 
		/// </summary>
		[ExcludeFromIntellisense]
		virtual System::IntPtr GetFieldPtr(System::String^ name);

		/// <summary>
		/// Internal Use
		/// </summary>
		void	StartDuringInitDocument();
		void	EndDuringInitDocument();

		/// <summary>
		/// Gets the desired dbt
		/// </summary>
		/// <param name="nameSpace">the namespace of the desired dbt</param>
		MDBTObject^	GetDBT (INameSpace^ nameSpace);
		/// <summary>
		/// Gets the desired dbt
		/// </summary>
		/// <param name="name">the name of the desired dbt</param>
		MDBTObject^	GetDBT (System::String^ name);
		
		/// <summary>
		/// Gets the desired dbt
		/// </summary>
		/// <param name="dbtPtr">the pointer of the desired dbt</param>
		MDBTObject^	GetDBT (System::IntPtr dbtPtr);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		DocumentMessageProvider^ GetMessageProvider();//usato per lo scripting
			
		/// <summary>
		/// Forces the update of data on the View
		/// </summary>
		void UpdateDataView() { if (GetDocument()) GetDocument()->UpdateDataView(); }
	
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		bool ValidCurrentRecord() { return GetDocument() ? (GetDocument()->ValidCurrentRecord() == TRUE) : FALSE; }
		
		/// <summary>
		/// Browse document for the selected Record:
		/// </summary>
		/// <param name="primaryKey">the record format must be: "segmName1:value1;segmName2:value2"</param>
		void BrowseRecord(System::String^ primaryKey);

		/// <summary>
		/// Browse document for the selected Record:
		/// </summary>
		virtual void BrowseRecord();
		/// <summary>
		/// Browse document and execute default query
		/// </summary>
		virtual void Browse();

		/// <summary>
		/// Ask the document to perform new record
		/// </summary>
		virtual bool EnterInNewRecord ();

		/// <summary>
		/// Ask the document to perform edit record
		/// </summary>
		virtual bool EditCurrentRecord ();

		/// <summary>
		/// Ask the document to save record
		/// </summary>
		virtual bool SaveCurrentRecord ();

		/// <summary>
		/// Ask the document to delete record
		/// </summary>
		virtual bool DeleteCurrentRecord ();

		/// <summary>
		/// Ask the document to go in browse mode
		/// </summary>
		virtual void GoInBrowseMode ();
	
		/// <summary>
		/// Ask the document to go in find mode
		/// </summary>
		virtual void GoInFindMode ();

		/// <summary>
		/// Gets the hotlink with the given name
		/// </summary>
		MHotLink^ GetHotLink (System::String^ name);

		/// <summary>
		/// Attach the given hotlink
		/// </summary>
		void AttachHotLink(MHotLink^ mHotLink);
		
		/// <summary>
		/// Gets the data manager with the given name
		/// </summary>
		MDataManager^ GetDataManager (System::String^ name);
		
		/// <summary>
		/// True if the current diagnostic session contains errors
		/// </summary>
		bool ErrorFound ();
		
		/// <summary>
		/// True if the current diagnostic session contains warnings
		/// </summary>
		bool WarningFound ();
		
		/// <summary>
		/// True if the current diagnostic session contains info
		/// </summary>
		bool InfoFound ();
		
		/// <summary>
		/// True if the current diagnostic session contains Messages
		/// </summary>
		bool MessageFound ();		

		/// <summary>
		/// Starts a new Message session
		/// </summary>
		void StartMessageSession(System::String^ openingBanner); 
		
		/// <summary>
		/// Ends a new Message session
		/// </summary>
		void EndMessageSession (System::String^ closingBanner);
		
		/// <summary>
		/// Adds a new Message to the current session
		/// </summary>
		/// <param name="message">the message to add to the diagnostic</param>
		/// <param name="type">the type of the message to add (Info, warning, error)</param>
		void AddMessage	(System::String^ message, DiagnosticType type);
		
		/// <summary>
		/// Shows all the messages of the current session
		/// </summary>
		/// <param name="clearMessages">true if you want to empty the diagnostic after showing the messages</param>
		bool ShowMessage (bool clearMessages);

		// da usarsi nella intercettazione del messaggio EN_VALUE_CHANGED
		/// <summary>
		/// Sets the current DataObj in a "bad" state with the specified message
		/// (e.g.: "the current date is not in a correct format")
		/// </summary>
		/// <param name="data">the dataobj on which apply the desired message</param>
		/// <param name="message">the desired message</param>
		void SetBadData (MDataObj^ data, System::String^ message);
		
		/// <summary>
		/// Sets the error message for the entire document
		/// </summary>
		/// <param name="message">the error to add to the diagnostic</param>
		void SetError (System::String^ message);
		
		/// <summary>
		/// Sets the warning message for the entire document
		/// </summary>
		/// <param name="message">the warning to add to the diagnostic</param>
		void SetWarning (System::String^ message);

		/// <summary>
		/// Returns in a string list all document's messages 
		/// </summary>
		System::Collections::Generic::List<System::String^>^ GetAllMessages();


		/// <summary>
		/// Begins a new local transaction for the active session
		/// </summary>
		bool				StartTransaction();
		
		/// <summary>
		/// Commits the local transaction
		/// </summary>
		void				Commit();

		/// <summary>
		/// Rollback the local transaction
		/// </summary>
		void				Rollback();

		/// <summary>
		/// Executes the batch
		/// </summary>
		void				ExecuteBatch();
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		bool PostMessageUM (int msg, System::IntPtr wParam, System::IntPtr lParam);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		long SendMessageUM (int msg, System::IntPtr wParam, System::IntPtr lParam);

		/// <summary>
		/// Return the document readable sql session
		/// </summary>
		virtual System::IntPtr GetReadOnlySessionPtr ();

		/// <summary>
		/// Return the document updatable sql session
		/// </summary>
		virtual System::IntPtr GetUpdatableSessionPtr ();


		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::Collections::Generic::List<MXMLSearchBookmark^>^ GetXMLSearchBookmark(INameSpace^ nameSpace, [System::Runtime::InteropServices::Out] int% version); 

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		int GetFiscalYear();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::String^ GetSosSuffix();
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::String^ MDocument::GetSosDocumentType();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::String^ MDocument::GetCompanyName();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::String^ MDocument::GetTaxIdNumber();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::String^ MDocument::GetFiscalCode();

		/// <summary>
		/// unlocks all data locked by the document
		/// </summary>
		void UnlockAll ();

		/// <summary>
		/// True if the current document is UnattendedMode
		/// </summary>
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool InUnattendedMode { virtual bool get(); }
		
		/// <summary>
		/// True if you can have only one instance of this document in the database
		/// </summary>
		[System::ComponentModel::Browsable(true)]
		property bool OnlyOneRecord { bool get(); void set(bool value); }
		

		/// <summary>
		/// Alias NameSpace of a copied document
		/// </summary>
		[System::ComponentModel::Browsable(true), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property NameSpace^ TemplateNamespace { NameSpace^ get();  }

		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		virtual property System::String^ SerializedName	{ System::String^ get () override; }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property System::String^ SerializedType	{ virtual System::String^ get () override; }


		/// <summary>
		/// Internal Use
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property MXMLVariableArray^ XMLVariableArray { MXMLVariableArray^ get(); }

		/// <summary>
		/// Internal Use for DMS
		/// </summary>
		[System::ComponentModel::Browsable(false), ExcludeFromIntellisense]
		property MXMLVariableArray^ BookmarkXMLVariables { MXMLVariableArray^ get(); }


		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void Remove(System::ComponentModel::IComponent^ component) override;
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void AttachMaster(IDocumentMasterDataManager^ dbtMaster);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void AttachSlave(IDocumentSlaveDataManager^ dbtSlave);
		
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void CallCreateComponents() override;

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		virtual void OnAfterCreateComponents() override;

		/// <summary>
		/// Gets or sets the title of the current document
		/// </summary>
		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ Title { virtual System::String^ get(); protected: void set(System::String^ value);}
		
	internal:
		IDataManager^	GetDataManager		(SqlRecord* pRecord);
		bool			IsHotLinkWrapped	(HotKeyLink* pHotKeyLink);

		/// <summary>
		/// Manages invaild DBTs objects
		/// </summary>
		void 	AddInvalidDBT(CString dbtNameSpace);
		void 	RemoveInvalidDBT(CString dbtNameSpace);
		bool 	IsInvalidDBT(CString dbtNameSpace);
	private:
		int 	GetInvalidDBTIdx(CString dbtNameSpace);

public:
		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		System::Collections::Generic::SortedDictionary<System::String^, System::String^>^	GetUnWrappedHotLinks();

		/// <summary>
		/// Internal Use
		/// </summary>
		MHotLink^		GetWrappedHotLink	(System::IntPtr hotKeyLinkPtr);
	
		/// <summary>
		/// Closes a document
		/// </summary>
		virtual void Close();

		/// <summary>
		/// Internal Use
		/// </summary>
		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false)]
		property EDesignMode DesignModeType { virtual  EDesignMode get() override; }

	};


	/// <summary>
	/// Internal use: serializes a business object
	/// </summary>
	//================================================================================
	public ref class BusinessObjectSerializer : DocumentSerializer
	{
	public:
		static System::String^ BusinessObjectVariableName   = "businessObject";
		static System::String^ CreateMethodName				= "Create";
		static System::String^ CreateUnattendedMethodName	= "CreateUnattended";
		static System::String^ CreateHiddenMethodName		= "CreateHidden";

	public:
		virtual TypeDeclaration^ SerializeClass	(SyntaxTree^ syntaxTree, IComponent^ component) override;
	
	protected:
		property System::Type^		ComponentSerializedAs	{ virtual System::Type^ get () override; }
	};
	
	//================================================================================
	public enum class BOEvent
	{
			OnPrepareAuxData,
			OnGoInBrowseMode,
			OnBeforeCloseDocument, 
			OnAfterBatchExecute
	};

	//================================================================================
	[ExcludeFromIntellisense]
	[System::ComponentModel::Design::Serialization::DesignerSerializer(BusinessObjectSerializer::typeid, System::ComponentModel::Design::Serialization::CodeDomSerializer::typeid)]
	[System::ComponentModel::PropertyTabAttribute(System::Windows::Forms::Design::EventsTab::typeid, System::ComponentModel::PropertyTabScope::Component)]
	/// <summary>
	/// It wraps a business object
	/// </summary>
	public ref class BusinessObject : MDocument
	{
	private:
		bool	autoValueChanged;

	public:
		/// <summary>
		/// Constructor
		/// </summary>
		[ExcludeFromIntellisense]
		BusinessObject (System::IntPtr wrappedObject);

		/// <summary>
		/// Destructor/Dispose
		/// </summary>
		~BusinessObject ();

		/// <summary>
		/// Finalize
		/// </summary>
		!BusinessObject ();

	public:
		[System::ComponentModel::Browsable(false), System::ComponentModel:: DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
		property System::Collections::Generic::List<System::String^>^ InternalClasses { virtual System::Collections::Generic::List<System::String^>^ get() override; }

		[ExcludeFromIntellisense]
		[System::ComponentModel::Browsable(false), System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property System::String^ ControllerType { virtual System::String^ get () override; }

		[System::ComponentModel::DesignerSerializationVisibility(System::ComponentModel::DesignerSerializationVisibility::Hidden)]
		property bool AutoValueChanged { virtual bool get () override; virtual void set(bool value) override; }


	public:
		GENERIC_HANDLER_EVENT(Saved, EasyBuilderEventArgs, "Actions", EBCategories, "Occurs when business objects has saved and returned in browse mode");
		GENERIC_HANDLER_EVENT(Closing, EasyBuilderEventArgs, "Actions", EBCategories, "Occurs when business objects is closing");
		GENERIC_HANDLER_EVENT(DataLoaded, EasyBuilderEventArgs, "Actions", EBCategories, "Occurs when business objects loads data");
		GENERIC_HANDLER_EVENT(BatchExecuted, EasyBuilderEventArgs, "Actions", EBCategories, "Occurs when batch business objects terminates their execution");

		void OnSaved		();
		void OnClosing		();
		void OnDataLoaded	();
		void OnBatchExecuted();

		void DispatchEvent	(BOEvent evnt);
	public:

		/// <summary>
		/// Browse document for the selected Record:
		/// </summary>
		virtual void BrowseRecord() override;

		/// <summary>
		/// Ask the document to perform new record
		/// </summary>
		virtual bool EnterInNewRecord () override;

		/// <summary>
		/// Ask the document to perform edit record
		/// </summary>
		virtual bool EditCurrentRecord () override;

		/// <summary>
		/// Ask the document to save record
		/// </summary>
		virtual bool SaveCurrentRecord () override;

		/// <summary>
		/// Ask the document to delete record
		/// </summary>
		virtual bool DeleteCurrentRecord () override;

		/// <summary>
		/// Ask the document to go in browse mode
		/// </summary>
		virtual void GoInBrowseMode () override;



		/// <summary>
		/// Add EasyBuilder components of a specified type to the array. The function is recursive
		/// </summary>
		/// <returns>void</returns>
		/// <param name="requestedTypes">The EasyBuilderComponent type to add.</param>
		/// <param name="components">The array of components to populate.</param>
		[ExcludeFromIntellisense]
		virtual void GetEasyBuilderComponents(System::Collections::Generic::List<System::Type^>^ requestedTypes, System::Collections::Generic::List<EasyBuilderComponent^>^ components) override;

	};

	////================================================================================
	//[ExcludeFromIntellisense]
	///// <summary>
	///// Internal Use
	///// </summary>
	//public ref class MClientDocBag
	//{
	//private:
	//	const CDocumentDescription*		m_pDocumentInfo;
	//	const CServerDocDescription*	m_pServerInfo;
	//	const CClientDocDescription*	m_pClientDocInfo;

	//public:
	//	/// <summary>
	//	/// Internal Use
	//	/// </summary>
	//	[ExcludeFromIntellisense]
	//	property System::String^ DocumentAssemblyFullName { System::String^ get(); }
	//	/// <summary>
	//	/// Internal Use
	//	/// </summary>
	//	[ExcludeFromIntellisense]
	//	property System::String^ ClientDocAssemblyFullName { System::String^ get(); }
	//	/// <summary>
	//	/// Internal Use
	//	/// </summary>
	//	[ExcludeFromIntellisense]
	//	property System::String^ ServerDocument	{ System::String^ get(); }

	//	/// <summary>
	//	/// Internal Use
	//	/// </summary>
	//	[ExcludeFromIntellisense]
	//	property NameSpace^ ServerDocumentNameSpace	{ NameSpace^ get(); }

	//	/// <summary>
	//	/// Internal Use
	//	/// </summary>
	//	[ExcludeFromIntellisense]
	//	property System::String^ Controller		{ System::String^ get(); }

	//public:
	//	MClientDocBag (const CDocumentDescription* pDocInfo, const CServerDocDescription* pServerInfo, const CClientDocDescription* pClientDocInfo);
	//
	//private:
	//	System::String^ GetAssemblyName(const CTBNamespace& sNameSpace, BOOL bIsManaged);
	//};

	////================================================================================
	//[ExcludeFromIntellisense]
	///// <summary>
	///// It wraps a business object
	///// </summary>
	//public ref class MClientDoc : MDocument
	//{
	//private:
	//	MDocument^	serverDocument;

	//public:
	//	MClientDoc(MDocument^ serverDocument);

	//	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility::Hidden), ExcludeFromIntellisense]
	//	property MDocument^ ServerDocument { MDocument^ get(); void set (MDocument^); }
	//};
}
}
}
