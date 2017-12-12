using System;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ICSharpCode.NRefactory.CSharp;
using Microarea.EasyBuilder.Packager;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;


namespace Microarea.EasyBuilder
{
	/// <summary>
	/// Describes the type of change occurred.
	/// </summary>
	/// <value></value>
	//--------------------------------------------------------------------------------
	public enum ChangeType
	{ 
		/// <summary>
		/// No changes occurred, it is the default value.
		/// </summary>
		None,

		/// <summary>
		/// A method has been added to a class.
		/// </summary>
		MethodAdded,

		/// <summary>
		/// A method has been removed from a class.
		/// </summary>
		MethodRemoved,

		/// <summary>
		/// Something in the source code is changed, no matter what changed.
		/// </summary>
		CodeChanged
	}

	/// <summary>
	/// Describes the type of document restarting action needed.
	/// </summary>
	//--------------------------------------------------------------------------------
	public enum RestartAction
	{
		/// <summary>
		/// The document is to be restarted loading all customizations.
		/// It is the default value.
		/// </summary>
		RestartAndLoadAll,

		/// <summary>
		/// The document is to be restarted and, once it is loaded,
		/// EasyBuilder starts a customization session.
		/// </summary>
		RestartAndGoInEdit
	}

	/// <summary>
	/// Describes the type of reference action between EasyBuilder object model objects.
	/// </summary>
	//--------------------------------------------------------------------------------
	public enum ReferenceBehaviour
	{
		/// <summary>
		/// No references, it is the default value.
		/// </summary>
		None,
		/// <summary>
		/// A reference to an object is added.
		/// </summary>
		Added,
		/// <summary>
		/// A reference to an object is removed.
		/// </summary>
		Removed
	}

	/// <summary>
	/// Describes the events type affecting the entire document life cycle.
	/// </summary>
	/// <remarks>
	/// There is a one to one mapping between TaskBuilder C++ document events and
	/// ManagedClientDocEvent values.
	/// </remarks>
	//--------------------------------------------------------------------------------
	public enum ManagedClientDocEvent
	{
		/// <summary>
		/// Called before deleting a document.
		/// </summary>
		OnOkDelete,

		/// <summary>
		/// Called before entering the edit document state.
		/// </summary>
		OnOkEdit,

		/// <summary>
		///  Called before entering the new document state.
		/// </summary>
		OnOkNewRecord,

		/// <summary>
		///  Called before deleting a record.
		/// </summary>
		CanDoDeleteRecord,

		/// <summary>
		/// Called before entering the edit state for a record.
		/// </summary>
		CanDoEditRecord,

		/// <summary>
		/// Called before entering the new state for a record.
		/// </summary>
		CanDoNewRecord,

		/// <summary>
		/// Called before deleting a record.
		/// </summary>
		OnBeforeDeleteRecord,

		/// <summary>
		/// Called before entering the edit state for a record.
		/// </summary>
		OnBeforeEditRecord,

		/// <summary>
		/// Called before entering the new state for a record.
		/// </summary>
		OnBeforeNewRecord,

		/// <summary>
		/// Called before starting the transaction to persist document data.
		/// </summary>
		OnBeforeOkTransaction,

		/// <summary>
		/// Called after the transaction to persist document data is finished
		/// </summary>
		OnOkTransaction,

		/// <summary>
		/// Called before standard primary transaction commit performed in order to persist a document. This event allows to update auxiliary data.
		/// If e.Cancel is setted, the primary transaction will be RolledBack.
		/// </summary>
		OnBeforeTransaction,

		/// <summary>
		/// Called after standard primary transaction but before commit operation is performed in order to persist a document. This event allows to update auxiliary data.
		/// If e.Cancel is setted, the primary transaction will be RolledBack.
		/// </summary>
		OnTransaction,

		/// <summary>
		/// Called after document primary transaction is committed and is called in order to perform extra transactions. This event allows to update auxiliary data after the document is saved using StartTransaction, Commit, Rollback commands.
		/// If e.Cancel is setted, the extra transactions will be RolledBack, but not the primary one.
		/// </summary>
		OnExtraTransaction,

		/// <summary>
		/// Called when document is locked in New,Edit,Delete operation. It allows to lock data involved in primary and extra transaction operations.
		/// </summary>
		OnLockDocument,

		/// <summary>
		/// Called before a batch operation is executed
		/// </summary>
		OnBeforeBatchExecute,
	
		/// <summary>
		/// Called after the execution of a batch operation.
		/// </summary>
		OnAfterBatchExecute,

		/// <summary>
		/// Called after all DBT are loaded by the document.
		/// </summary>
		OnAttachData,

		/// <summary>
		/// Called to notify all views to set up auxiliary data (e.g.:HotLink are loaded during this phase).
		/// </summary>
		OnPrepareAuxData,

		/// <summary>
		/// Called to notify all views to set up auxiliary data (e.g.:HotLink are loaded during this phase).
		/// </summary>
		OnInitAuxData,

		/// <summary>
		/// 
		/// </summary>
		OnExistTables,

		/// <summary>
		/// Called after the document has been initialized.
		/// </summary>
		OnInitDocument,

	
		/// <summary>
		/// Called before a document is closed.
		/// </summary>
		OnBeforeCloseDocument,

		/// <summary>
		/// Called after a document has been closed.
		/// </summary>
		OnCloseDocument,

		/// <summary>
		/// Called before a record is browsed
		/// </summary>
		OnBeforeBrowseRecord,

		/// <summary>
		/// Called before entering the browse state for a record.
		/// </summary>
		OnGoInBrowseMode,
		
		/// <summary>
		/// Called before all controls of the current document's view were
		/// disabled when the document enter the batch state.
		/// </summary>
		OnDisableControlsForBatch,

		/// <summary>
		/// Called before all controls of the current document's view were
		/// disabled when the document enter the new state.
		/// </summary>
		OnDisableControlsForAddNew,

		/// <summary>
		/// Called before all controls of the current document's view were
		/// disabled when the document enter the edit state.
		/// </summary>
		OnDisableControlsForEdit,

		/// <summary>
		/// Called before all controls of the current document's view were
		/// disabled when the document enter the find state.
		/// </summary>
		OnEnableControlsForFind,

		/// <summary>
		/// Called before all controls of the current document's view were
		/// disabled when the document enter the new, the
		/// edit or the find state.
		/// </summary>
		OnDisableControlsAlways,
		/// <summary>
		/// Called after document has been created and loaded
		/// </summary>
		OnDocumentCreated
	}

	/// <summary>
	/// Provides data for the ExceptionRaised event.
	/// </summary>
	/// <remarks>
	/// Instances of this clss are used by the controller to signal an exception occurred
	/// in the customization code.
	/// </remarks>
	//================================================================================
	public class ExceptionRaisedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the DirtyChangedEventArgs with the given exception.
		/// </summary>
		/// <param name="exc">Instance of the occurred exception</param>
		/// <seealso cref="System.Exception"/>
		//--------------------------------------------------------------------------------
		public ExceptionRaisedEventArgs(Exception exc)
		{
			this.RaisedException = exc;
		}

		/// <summary>
		/// Gets or sets the occurred exception.
		/// </summary>
		//--------------------------------------------------------------------------------
		public Exception RaisedException { get; set; }
	}

	/// <summary>
	/// Provides data for the ReferenceUpdated event.
	/// </summary>
	//================================================================================
	public class ReferenceUpdatedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the ReferenceUpdatedEventArgs with the given
		/// ReferenceBehaviour and Assembly.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ReferenceBehaviour"/>
		/// <seealso cref="System.Reflection.Assembly"/>
		//--------------------------------------------------------------------------------
		public ReferenceUpdatedEventArgs(ReferenceBehaviour referenceBehaviour, Assembly assembly)
		{
			this.ReferenceBehaviour = referenceBehaviour;
			this.Assembly = assembly;
		}

		/// <summary>
		/// Gets or sets the ReferenceBehaviour.
		/// </summary>
		//--------------------------------------------------------------------------------
		public ReferenceBehaviour ReferenceBehaviour { get; set; }

		/// <summary>
		/// Gets or sets the Assembly added as a reference.
		/// </summary>
		//--------------------------------------------------------------------------------
		public Assembly Assembly { get; set; }
	}

	/// <summary>
	/// Provides data for the BuildCompleted event.
	/// </summary>
	//================================================================================
	public class BuildEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the ReferenceUpdatedEventArgs with the given
		/// CompilerResults.
		/// </summary>
		/// <seealso cref="System.CodeDom.Compiler.CompilerResults"/>
		//-----------------------------------------------------------------------------
		public BuildEventArgs(EBCompilerResults results, bool temporary)
		{
			this.Results = results;
			this.Temporary = temporary;
		}
		/// <summary>
		/// Gets or sets the CompilerResults for the just completed build.
		/// </summary>
		//-----------------------------------------------------------------------------
		public EBCompilerResults Results { get; set; }

		/// <summary>
		/// Gets or sets a boolean that indicates if the build is temporary (for intellisense) or not
		/// </summary>
		public bool Temporary { get; set; }
	}

	/// <summary>
	/// Provides data for the CodeChanged event.
	/// </summary>
	//================================================================================
	public class CodeChangedEventArgs : EventArgs
	{
		private MethodDeclaration method;
		private ChangeType changeType;

		/// <summary>
		/// Initializes a new instance of the ReferenceUpdatedEventArgs with the given
		/// ChangeType.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ChangeType"/>
		//-----------------------------------------------------------------------------
		public CodeChangedEventArgs(ChangeType changeType)
			: this (changeType, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the ReferenceUpdatedEventArgs with the given
		/// ChangeType and MethodDeclaration.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.ChangeType"/>
		//-----------------------------------------------------------------------------
		public CodeChangedEventArgs(ChangeType changeType, MethodDeclaration method)
		{
			this.changeType = changeType;
			this.method = method;
		}

		/// <summary>
		/// Gets or sets the changed MethodDeclaration.
		/// </summary>
		//-----------------------------------------------------------------------------
		public MethodDeclaration Method { get { return method; } set { method = value; } }

		/// <summary>
		/// Gets or sets the ChangeType.
		/// </summary>
		//-----------------------------------------------------------------------------
		public ChangeType ChangeType { get { return changeType; } set { changeType = value; } }
	}

	/// <summary>
	/// Provides data for the AddField event.
	/// </summary>
	//================================================================================
	internal class AddFieldEventArgs : EventArgs
	{
		private IDataManager dataManager;
		private bool local;

		//-----------------------------------------------------------------------------
		public bool Local
		{
			get { return local; }
		}

		//-----------------------------------------------------------------------------
		public IDataManager DataManager
		{
			get { return dataManager; }
		}

		//-----------------------------------------------------------------------------
		public AddFieldEventArgs(IDataManager dataManager, bool local)
		{
			this.dataManager = dataManager;
			this.local = local;
		}
	}

	/// <summary>
	/// Provides data for the CodeMethodEdited event.
	/// </summary>
	//=============================================================================
	public class CodeMethodEditedEventArgs : EventArgs
	{
		private MethodDeclaration codeMemberMethod;
		private string oldMemberName;

		/// <summary>
		/// Gets or sets the OldMemberName.
		/// </summary>
		//-----------------------------------------------------------------------------
		public string OldMemberName { get { return oldMemberName; } set { oldMemberName = value; } }

		/// <summary>
		/// Gets or sets the edited MethodDeclaration.
		/// </summary>
		//-----------------------------------------------------------------------------
		public MethodDeclaration MethodDeclaration
		{
			get { return codeMemberMethod; }
			set { codeMemberMethod = value; }
		}

		/// <summary>
		/// Initializes a new instance of the CodeMethodEditedEventArgs with the given
		/// MethodDeclaration and oldMemberName.
		/// </summary>
		//-----------------------------------------------------------------------------
		public CodeMethodEditedEventArgs(MethodDeclaration codeMemberMethod, string oldMemberName)
		{
			this.codeMemberMethod = codeMemberMethod;
			this.oldMemberName = oldMemberName;
		}
	}

	/// <summary>
	/// Provides data for the CodeMethodAdded event.
	/// </summary>
	//=============================================================================
	public class CodeMethodAddedEventArgs : EventArgs
	{
		private MethodDeclaration codeMemberMethod;

		/// <summary>
		/// Gets or sets the added MethodDeclaration.
		/// </summary>
		//-----------------------------------------------------------------------------
		public MethodDeclaration MethodDeclaration
		{
			get { return codeMemberMethod; }
			set { codeMemberMethod = value; }
		}

		/// <summary>
		/// Initializes a new instance of the CodeMethodAddedEventArgs with the given
		/// MethodDeclaration.
		/// </summary>
		//-----------------------------------------------------------------------------
		public CodeMethodAddedEventArgs(MethodDeclaration codeMemberMethod)
		{
			this.codeMemberMethod = codeMemberMethod;
		}
	}

	/// <summary>
	/// Provides data for the SelectedObjectChanged event.
	/// </summary>
	//=============================================================================
	public class SelectedObjectEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the SelectedObjectEventArgs with the given
		/// selectedObject.
		/// </summary>
		//--------------------------------------------------------------------------------
		public SelectedObjectEventArgs(object selectedObject)
		{
			this.SelectedObject = selectedObject;
		}

		/// <summary>
		/// Gets or sets the selected object.
		/// </summary>
		//--------------------------------------------------------------------------------
		public object SelectedObject { get; set; }
	}

	/// <summary>
	/// Provides data for the SelectedObjectPropertyChanged event.
	/// </summary>
	//=============================================================================
	public class SelectedObjectPropertyChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the SelectedObjectPropertyChangedEventArgs with the given
		/// selectedObject and the name of the changed property.
		/// </summary>
		//--------------------------------------------------------------------------------
		public SelectedObjectPropertyChangedEventArgs(object selectedObject, string propertyChanged)
		{
			this.SelectedObject = selectedObject;
			this.PropertyChanged = propertyChanged;
		}

		/// <summary>
		/// Gets or sets the selected object.
		/// </summary>
		//--------------------------------------------------------------------------------
		public object SelectedObject { get; set; }

		/// <summary>
		/// Gets or sets the name of the changed property.
		/// </summary>
		//--------------------------------------------------------------------------------
		public string PropertyChanged { get; set; }
	}
	
	/// <summary>
	/// Provides data for the CodeMethodEdit event.
	/// </summary>
	//=============================================================================
	public class CodeMethodEditorEventArgs : EventArgs
	{
		MethodDeclaration method;

		/// <summary>
		/// Gets the MethodDeclaration associated to this event
		/// </summary>
		//--------------------------------------------------------------------------------
		public MethodDeclaration Method { get { return method; } }

		/// <summary>
		/// Gets a value indicating i MethodDeclaration associated to this event
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool IgnoreChangesOnFormClosing { get; set; }

		/// <summary>
		/// Initializes a new instance of the CodeMethodEditorEventArgs with the given
		/// method.
		/// </summary>
		//--------------------------------------------------------------------------------
		public CodeMethodEditorEventArgs(MethodDeclaration method, bool ignoreChangesOnFormClosing = false)
		{
			this.method = method;
			this.IgnoreChangesOnFormClosing = ignoreChangesOnFormClosing;
		}
	}

	/// <summary>
	/// Provides information about the OpenSourceCode event.
	/// </summary>
	//=============================================================================
	public class CodeSourceEditorEventArgs : EventArgs
	{
		string source;
		int line;
		int column;

		/// <summary>
		/// Gets the line where the CodeEditor caret will be set.
		/// </summary>
		//--------------------------------------------------------------------------------
		public int Line { get { return line; } }

		/// <summary>
		/// Gets the columns where the CodeEditor caret will be set.
		/// </summary>
		//--------------------------------------------------------------------------------
		public int Column { get { return column; } }

		/// <summary>
		/// Gets the source code that will be displayed by the CodeEditor.
		/// </summary>
		//--------------------------------------------------------------------------------
		public string Source { get { return source; } }

		/// <summary>
		/// Initializes a new instance of the CodeMethodEditorEventArgs with the given
		/// source code, line and column.
		/// </summary>
		//--------------------------------------------------------------------------------
		public CodeSourceEditorEventArgs(string source, int line, int column)
		{
			this.source = source;
			this.line = line;
			this.column = column; 
		}
	}

	/// <summary>
	/// Provides information about the RestartDocument event.
	/// </summary>
	//=============================================================================
	public class RestartEventArgs : EventArgs
	{
		INameSpace customizationNameSpace = null;
		INameSpace documentNameSpace = null;
		RestartAction action = RestartAction.RestartAndLoadAll;
		bool isServerDocument;

		/// <summary>
		/// Gets the INameSpace object for the current customization.
		/// </summary>
		//--------------------------------------------------------------------------------
		public INameSpace CustomizationNamespace { get { return customizationNameSpace; } }

		/// <summary>
		/// Gets the INameSpace object for the current document.
		/// </summary>
		//--------------------------------------------------------------------------------
		public INameSpace DocumentNamespace { get { return documentNameSpace; } }

		/// <summary>
		/// Gets the RestartAction required.
		/// </summary>
		//--------------------------------------------------------------------------------
		public RestartAction Action { get { return action; } }

		/// <summary>
		/// Gets a value indicating if the document to be restarted is a server document or a client document.
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool IsServerDocument { get { return isServerDocument; } }

		/// <summary>
		/// Initializes a new instance of the CodeMethodEditorEventArgs with the given
		/// customization namespace, document namespace and restart action.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Interfaces.INameSpace"/>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.Generic.NameSpace"/>
		/// <seealso cref="Microarea.EasyBuilder.RestartAction"/>
		//--------------------------------------------------------------------------------
		public RestartEventArgs(
			INameSpace customizationNameSpace,
			INameSpace documentNameSpace,
			RestartAction action,
			bool isServerDocument = false
			)
		{
			this.customizationNameSpace = customizationNameSpace;
			this.documentNameSpace = documentNameSpace; 
			this.action = action;
			this.isServerDocument = isServerDocument;
		}
	}

	/// <summary>
	/// Provides information about the PropertyChanging event.
	/// </summary>
	//================================================================================
	public class MyPropertyChangingArgs : EventArgs
	{
		private string propertyName;
		private object newValue;

		/// <summary>
		/// Gets or sets the property name.
		/// </summary>
		//--------------------------------------------------------------------------------
		public string PropertyName
		{
			get { return propertyName; }
			set { propertyName = value; }
		}
		
		/// <summary>
		/// Gets or sets the new value fot the changing property.
		/// </summary>
		//--------------------------------------------------------------------------------
		public object NewValue
		{
			get { return newValue; }
			set { newValue = value; }
		}

		/// <summary>
		/// Initializes a new instance of the MyPropertyChangingArgs with default values.
		/// </summary>
		//--------------------------------------------------------------------------------
		public MyPropertyChangingArgs()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the MyPropertyChangingArgs with the given name and value.
		/// </summary>
		//--------------------------------------------------------------------------------
		public MyPropertyChangingArgs(string name, object newValue)
		{
			this.propertyName = name;
			this.newValue = newValue;
		}
	}

	/// <summary>
	/// Provides information about the DeleteObject event.
	/// </summary>
	//================================================================================
	public class DeleteObjectEventArgs : EventArgs
	{
		private EasyBuilderComponent component = null;
		
		/// <summary>
		/// Gets or sets the deleted EasyBuilderComponent.
		/// </summary>
		//--------------------------------------------------------------------------------
		public EasyBuilderComponent Component { get { return component; } set { component = value; } }

		/// <summary>
		/// Initializes a new instance of the DeleteObjectEventArgs with the given EasyBuilderComponent.
		/// </summary>
		/// <seealso cref="Microarea.TaskBuilderNet.Core.EasyBuilder.EasyBuilderComponent"/>
		//--------------------------------------------------------------------------------
		public DeleteObjectEventArgs(EasyBuilderComponent ebComponent)
		{
			this.component = ebComponent;
		}
	}

	/// <summary>
	/// Provides information about the AddDbt event.
	/// </summary>
	//================================================================================
	public class AddDataManagerEventArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public enum DataManagerRequestType
		{
			/// <summary>
			/// 
			/// </summary>
			FromTable
		};


		private string tableName = null;
		MDBTObject dbt;
		MDataManager dataManager;
		DataManagerRequestType requestType;

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public DataManagerRequestType RequestType
		{
			get { return requestType; }
			private set { requestType = value; }
		}

		/// <summary>
		/// Gets the DBT Object upon which the Data Manager is added (if any).
		/// </summary>
		//--------------------------------------------------------------------------------
		public MDBTObject Dbt { get { return dbt; } }
		/// <summary>
		/// Gets the name of the table upon which the Data Manager is added.
		/// </summary>
		//--------------------------------------------------------------------------------
		public string TableName { get { return tableName; } }

		/// <summary>
		/// Gets or Sets the data manager.
		/// </summary>
		//--------------------------------------------------------------------------------
		public MDataManager NewDataManager { get { return dataManager; } set { dataManager = value; } }

		/// <summary>
		/// Initializes a new instance of the AddDataManagerEventArgs with the given
		/// table name.
		/// </summary>
		//--------------------------------------------------------------------------------
		public AddDataManagerEventArgs(string tableName, MDBTObject dbt, DataManagerRequestType requestType = DataManagerRequestType.FromTable)
		{
			this.tableName = tableName;
			this.dbt = dbt;
			RequestType = requestType;
		}
	}

	/// <summary>
	/// Provides information about the DeclareComponent event.
	/// </summary>
	//================================================================================
	public class DeclareComponentEventArgs : EventArgs
	{
		private ComponentDeclarationRequest request;

		/// <summary>
		/// It represents the request action
		/// </summary>
		//--------------------------------------------------------------------------------
		public ComponentDeclarationRequest Request { get { return request; } }

		/// <summary>
		/// Constructor
		/// </summary>
		//--------------------------------------------------------------------------------
		public DeclareComponentEventArgs(ComponentDeclarationRequest request)
		{
			this.request = request;
		}
	};
	/// <summary>
	/// Provides information about the AddHotLink event.
	/// </summary>
	//================================================================================
	public class AddHotLinkEventArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public enum RequestType
		{
			/// <summary>
			/// 
			/// </summary>
			FromTable, 	
			/// <summary>
			/// 
			/// </summary>
			FromTemplate,
			/// <summary>
			/// 
			/// </summary>
			FromDocument
		};

		private RequestType request;
		private string source;
		private MHotLink newHotlink;

		/// <summary>
		/// 
		/// </summary>
		public RequestType Request { get { return request; } }
		/// <summary>
		/// 
		/// </summary>
		public string Source { get { return source; } }
		/// <summary>
		/// 
		/// </summary>
		public MHotLink NewHotlink { get { return newHotlink; } set { newHotlink = value; } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="source"></param>
		public AddHotLinkEventArgs(RequestType request, string source)
		{
			this.request = request;
			this.source = source;
		}

	}

	/// <summary>
	/// Provides information about the CustomListItemAdded event.
	/// </summary>
	//================================================================================
	internal class CustomListItemAddedEventArgs : EventArgs
	{
		private string fileFullPath = string.Empty;
		private IEasyBuilderApp customization = null; 
		private string publishedUser = string.Empty;
		private bool isActiveDocument = false;

		/// <summary>
		/// Gets the full path of the file added to the custom list
		/// </summary>
		//--------------------------------------------------------------------------------
		public string FileFullPath { get { return fileFullPath; } }

		/// <summary>
		/// Gets or sets the current Customization
		/// </summary>
		//--------------------------------------------------------------------------------
		public IEasyBuilderApp Customization { get { return customization; } set { customization = value; } }

		/// <summary>
		/// Gets or sets the user for whom the file is published.
		/// </summary>
		//--------------------------------------------------------------------------------
		public string PublishedUser { get { return publishedUser; } set { publishedUser = value; } }

		/// <summary>
		/// Gets or sets a boolean value indicating if the current documenti sthe active document.
		/// </summary>
		//--------------------------------------------------------------------------------
		public bool IsActiveDocument { get { return isActiveDocument; } set { isActiveDocument = value; } }


		/// <summary>
		/// Initializes a new instance of the CustomListItemAddedEventArgs with the given
		/// file path, customization, user and setting whether it is for the active
		/// document or not.
		/// </summary>
		/// <seealso cref="Microarea.EasyBuilder.Packager.Customization"/>
		//--------------------------------------------------------------------------------
		public CustomListItemAddedEventArgs(
			string fileFullPath,
			IEasyBuilderApp customization,
			string publishedUser,
			bool isActiveDocument
			)
		{
			this.fileFullPath	= fileFullPath;
			this.customization = customization;
			this.publishedUser = publishedUser;
			this.isActiveDocument = isActiveDocument;
		}
	}
	


	/// <summary>
	/// Provides data for all the events raised by a DocumentController.
	/// </summary>
	//=============================================================================
	public class ControllerEventArgs : EasyBuilderEventArgs
	{
		/// <summary>
		/// Gets or sets the Cancel boolean.
		/// </summary>
		/// <remarks>
		/// The cancel boolean is used as a return value in almost all events 
		/// needing a value to be checked in order to state if going on with the operation or not.
		/// E.g.: the DocumentController.DataForTransactionChecked event tests
		/// the cancel boolean to know if an EasyBuilder customization allows to save data or not.
		/// </remarks>
		//--------------------------------------------------------------------------------
		public bool Cancel { get; set; }
	}
	//=============================================================================
	/// <summary>
	/// Provides data for WOORM report events
	/// </summary>
	public class WoormEventArgs : ControllerEventArgs
	{
		MWoormInfo woormInfo;

		/// <summary>
		/// Creates a new WoormEventArgs object
		/// </summary>
		public WoormEventArgs(MWoormInfo info) { woormInfo = info; }
		/// <summary>
		/// A bag that encapsulates information for running a report
		/// </summary>
		public MWoormInfo WoormInfo { get { return woormInfo; } }
	};

	//=============================================================================
	/// <summary>
	/// Provides data for batch procedure events
	/// </summary>
	public class BatchEventArgs : ControllerEventArgs
	{
		MSqlRecord record;

		/// <summary>
		/// Creates a new BatchEventArgs object
		/// </summary>
		public BatchEventArgs(MSqlRecord record) { this.record = record; }
		/// <summary>
		/// The record currently being processed when running a batch
		/// </summary>
		public MSqlRecord Record { get { return record; } }
	};
	/// <summary>
	/// Provides data for all events raised by a managed client document.
	/// </summary>
	//=============================================================================
	public class ControllerEventManagerArgs : ControllerEventArgs
	{
		string eventName;

		/// <summary>
		/// The name of the event raised by the managed client document
		/// </summary>
		//--------------------------------------------------------------------------------
		public string EventName { get { return eventName; } set { eventName = value; } }

		/// <summary>
		/// Data associated to the event
		/// </summary>
		//--------------------------------------------------------------------------------
		public string Data { get; set; }

		/// <summary>
		/// Initializes a new instance of the ControllerEventManagerArgs with the given
		/// eventName.
		/// </summary>
		//--------------------------------------------------------------------------------
		public ControllerEventManagerArgs(string eventName)
		{
			this.eventName = eventName;
		}
	}

	/// <summary>
	/// Provides data for all the events raised by a Transaction.
	/// </summary>
	//=============================================================================
	public class TransactionEventArgs : ControllerEventArgs
	{
		private TransactionMode transactionMode = TransactionMode.New;
		/// <summary>
		/// Gets transaction type
		/// </summary>
		/// <remarks>
		/// </remarks>
		public enum TransactionMode
		{
			/// <summary>
			/// </summary>
			New,
			/// <summary>
			/// </summary>
			Edit,
			/// <summary>
			/// </summary>
			Delete
		};
		/// <summary>
		/// Gets transaction type
		/// </summary>
		/// <remarks>
		/// </remarks>
		//--------------------------------------------------------------------------------
		public TransactionMode Mode { get { return transactionMode; } }

		/// <summary>
		/// Constructs a TransactionEventArgs
		/// </summary>
		/// <remarks>
		/// </remarks>
		//--------------------------------------------------------------------------------
		public TransactionEventArgs(TransactionMode mode)
		{
			this.transactionMode = mode;
		}
	}

	/// <summary>
	/// Provides data for all the events raised by a Transaction.
	/// </summary>
	//=============================================================================
	public class LockEventArgs : TransactionEventArgs
	{
		private LockResult lockResult = LockResult.Locked;
		/// <summary>
		/// Gets transaction type
		/// </summary>
		/// <remarks>
		/// </remarks>
		//--------------------------------------------------------------------------------
		public LockResult Result { get { return lockResult; } set { lockResult = value; Cancel = lockResult != LockResult.Locked; } }

		/// <summary>
		/// </summary>
		/// <remarks>
		/// </remarks>
		//--------------------------------------------------------------------------------
		public LockEventArgs(TransactionMode transactionMode)
			:
			base(transactionMode)
		{
		}
	}


	/// <summary>
	/// CulturesEventArgs
	/// </summary>
	//=============================================================================
	public class CulturesEventArgs : EventArgs
	{
		List<CultureInfo> cultures;

		///<remarks/>
		public List<CultureInfo> Cultures
		{
			get { return cultures; }
			set { cultures = value; }
		}

		internal CulturesEventArgs(List<CultureInfo> cultures)
		{
			this.cultures = cultures;
		}

		internal CulturesEventArgs()
		{
			cultures = new List<CultureInfo>();
		}

	}


	//=============================================================================
	/// <summary>
	/// SetDirtyBoolArg
	/// </summary>
	public class SetDirtyBoolArg : EventArgs
	{
		bool dirty;

		///<remarks/>
		public bool Dirty
		{
			get { return dirty; }
			set { dirty = value; }
		}

		internal SetDirtyBoolArg(bool dirty)
		{
			this.dirty = dirty;
		}

	}

}
