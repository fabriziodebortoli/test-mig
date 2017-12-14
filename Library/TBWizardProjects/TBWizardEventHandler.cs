using System;


namespace Microarea.Library.TBWizardProjects
{
	//============================================================================
	public class TBWizardEventArgs : EventArgs
	{
		public enum ActionTaken : ushort
		{
			Undefined				= 0x0001,
			Parsing					= 0x0002,
			ApplicationRenamed		= 0x0003,
			ApplicationInfoChanged	= 0x0004,
			ModuleAdded				= 0x0005,
			ModuleRenamed			= 0x0006,
			ModuleDeleted			= 0x0007,
			ModuleInfoChanged		= 0x0008,
			LibraryAdded			= 0x0009,
			LibraryRenamed			= 0x000A,
			LibraryDeleted			= 0x000B,
			LibraryInfoChanged		= 0x000C,
			LibraryMoved			= 0x000D,
			TableAdded				= 0x000E,
			TableRenamed			= 0x000F,
			TableDeleted			= 0x0010,
			TableInfoChanged		= 0x0011,
			TableMoved				= 0x0012,
			DocumentAdded			= 0x0013,
			DocumentRenamed			= 0x0014,
			DocumentDeleted			= 0x0015,
			DocumentInfoChanged		= 0x0016,
			DependenciesChanged		= 0x0017,
			DBTAdded				= 0x0018,
			DBTRenamed				= 0x0019,
			DBTDeleted				= 0x001A,
			DBTInfoChanged			= 0x001B,
			DBTMoved				= 0x001C,
			DbReleaseNumberChanged	= 0x001D,
			EnumsAdded				= 0x001E,
			EnumDeleted				= 0x0020,
			EnumInfoChanged			= 0x0021,
			ImportingApplication	= 0x0022,
			ImportingModule			= 0x0023,
			ImportingLibrary		= 0x0024,
			ParseTableScript		= 0x0025,
			ReferencesChanged		= 0x0026,
			ClientDocDeleted		= 0x0027,
			ClientDocInfoChanged	= 0x0028,
			ExtraColumnsAdded		= 0x0029,
			ExtraColumnsDeleted		= 0x002A,
			ExtraColumnsChanged		= 0x002B,
			ExtraColumnsMoved		= 0x002C,
			UIDesign                = 0x002D
	}
		
		private object		projectItem = null;
		private ActionTaken action = ActionTaken.Undefined;

		//---------------------------------------------------------------------------
		public TBWizardEventArgs(object aProjectItem, ActionTaken aAction)
		{
			projectItem = aProjectItem;
			action = aAction;
		}

		//---------------------------------------------------------------------------
		public TBWizardEventArgs(object aProjectItem) : this(aProjectItem, ActionTaken.Undefined)
		{
		}

		//---------------------------------------------------------------------------
		public object ProjectItem { get { return projectItem; } } 
		
		//---------------------------------------------------------------------------
		public WizardApplicationInfo ApplicationInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardApplicationInfo)
					return (WizardApplicationInfo)projectItem;

				if (projectItem is WizardModuleInfo)
					return ((WizardModuleInfo)projectItem).Application;

				if (projectItem is WizardLibraryInfo)
					return (((WizardLibraryInfo)projectItem).Module != null) ? ((WizardLibraryInfo)projectItem).Module.Application : null;

				if (projectItem is WizardTableInfo)
					return (((WizardTableInfo)projectItem).Library != null && ((WizardTableInfo)projectItem).Library.Module != null) ? ((WizardTableInfo)projectItem).Library.Module.Application : null;

				if (projectItem is WizardDBTInfo)
					return (((WizardDBTInfo)projectItem).Library != null && ((WizardDBTInfo)projectItem).Library.Module != null) ? ((WizardDBTInfo)projectItem).Library.Module.Application : null;

				if (projectItem is WizardDocumentInfo)
					return (((WizardDocumentInfo)projectItem).Library != null && ((WizardDocumentInfo)projectItem).Library.Module != null) ? ((WizardDocumentInfo)projectItem).Library.Module.Application : null;
				
				if (projectItem is WizardClientDocumentInfo)
					return (((WizardClientDocumentInfo)projectItem).Library != null && ((WizardClientDocumentInfo)projectItem).Library.Module != null) ? ((WizardClientDocumentInfo)projectItem).Library.Module.Application : null;

				if (projectItem is WizardEnumInfo)
					return (((WizardEnumInfo)projectItem).Module != null) ? ((WizardEnumInfo)projectItem).Module.Application : null;

				if (projectItem is WizardExtraAddedColumnsInfo)
					return (((WizardExtraAddedColumnsInfo)projectItem).Library != null && ((WizardExtraAddedColumnsInfo)projectItem).Library.Module != null) ? ((WizardExtraAddedColumnsInfo)projectItem).Library.Module.Application : null;

				return null; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public WizardModuleInfo ModuleInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardModuleInfo)
					return (WizardModuleInfo)projectItem;

				if (projectItem is WizardLibraryInfo)
					return ((WizardLibraryInfo)projectItem).Module;

				if (projectItem is WizardTableInfo)
					return (((WizardTableInfo)projectItem).Library != null) ? ((WizardTableInfo)projectItem).Library.Module : null;

				if (projectItem is WizardDBTInfo)
					return (((WizardDBTInfo)projectItem).Library != null) ? ((WizardDBTInfo)projectItem).Library.Module : null;

				if (projectItem is WizardDocumentInfo)
					return (((WizardDocumentInfo)projectItem).Library != null) ? ((WizardDocumentInfo)projectItem).Library.Module : null;
				
				if (projectItem is WizardClientDocumentInfo)
					return (((WizardClientDocumentInfo)projectItem).Library != null) ? ((WizardClientDocumentInfo)projectItem).Library.Module : null;
				
				if (projectItem is WizardEnumInfo)
					return ((WizardEnumInfo)projectItem).Module;
				
				if (projectItem is WizardExtraAddedColumnsInfo)
					return (((WizardExtraAddedColumnsInfo)projectItem).Library != null) ? ((WizardExtraAddedColumnsInfo)projectItem).Library.Module : null;
				
				return null; 
			} 
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo LibraryInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardLibraryInfo)
					return (WizardLibraryInfo)projectItem;

				if (projectItem is WizardTableInfo)
					return ((WizardTableInfo)projectItem).Library;

				if (projectItem is WizardDBTInfo)
					return ((WizardDBTInfo)projectItem).Library;

				if (projectItem is WizardDocumentInfo)
					return ((WizardDocumentInfo)projectItem).Library;

				if (projectItem is WizardClientDocumentInfo)
					return ((WizardClientDocumentInfo)projectItem).Library;
				
				if (projectItem is WizardExtraAddedColumnsInfo)
					return ((WizardExtraAddedColumnsInfo)projectItem).Library;
				
				return null; 
			} 
		}
	
		//---------------------------------------------------------------------------
		public WizardTableInfo TableInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardTableInfo)
					return (WizardTableInfo)projectItem;

				if (projectItem is WizardDBTInfo)
					return ((WizardDBTInfo)projectItem).GetTableInfo();
				
				if (projectItem is WizardExtraAddedColumnsInfo)
					return ((WizardExtraAddedColumnsInfo)projectItem).GetOriginalTableInfo();
				
				return null; 
			} 
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo DBTInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardDBTInfo)
					return (WizardDBTInfo)projectItem;

				return null; 
			} 
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo DocumentInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardDocumentInfo)
					return (WizardDocumentInfo)projectItem;

				return null; 
			} 
		}
	
		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo ClientDocumentInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardClientDocumentInfo)
					return (WizardClientDocumentInfo)projectItem;

				return null; 
			} 
		}
	
		//---------------------------------------------------------------------------
		public WizardEnumInfo EnumInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardEnumInfo)
					return (WizardEnumInfo)projectItem;

				return null; 
			} 
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo ExtraAddedColumnsInfo 
		{
			get 
			{
				if (projectItem == null) 
					return null; 

				if (projectItem is WizardExtraAddedColumnsInfo)
					return (WizardExtraAddedColumnsInfo)projectItem;
				
				return null; 
			} 
		}

		//---------------------------------------------------------------------------
		public ActionTaken Action { get { return action; } }
	}

	//============================================================================
	public class TBWizardCancelEventArgs : TBWizardEventArgs
	{
		private bool cancel = false;

		//---------------------------------------------------------------------------
		public TBWizardCancelEventArgs(object aProjectItem, ActionTaken aAction) : base(aProjectItem, aAction)
		{
		}

		//---------------------------------------------------------------------------
		public TBWizardCancelEventArgs(object aProjectItem) : this(aProjectItem, ActionTaken.Undefined)
		{
		}

		//---------------------------------------------------------------------------
		public bool Cancel { get { return cancel; } set { cancel = value; } }
	}


	public delegate void TBWizardEventHandler(object sender, TBWizardEventArgs e);
	public delegate void TBWizardCancelEventHandler(object sender, TBWizardCancelEventArgs e);
	public delegate void TBWizardEnumInfoAddedEventHandler(object sender, WizardModuleInfo changedModule, WizardEnumInfo newEnumInfo);


	//============================================================================
	public class TBWizardReferencedApplicationEventArgs : TBWizardEventArgs
	{
		private string referencedApplication = String.Empty;

		//---------------------------------------------------------------------------
		public TBWizardReferencedApplicationEventArgs(string aApplicationName, WizardApplicationInfo aApplication, ActionTaken aAction) : base(aApplication, aAction)
		{
			referencedApplication = aApplicationName;
		}

		//---------------------------------------------------------------------------
		public TBWizardReferencedApplicationEventArgs(string aApplicationName, WizardApplicationInfo aApplication) : this(aApplicationName, aApplication, ActionTaken.Undefined)
		{
		}

		//---------------------------------------------------------------------------
		public string ReferencedApplication { get { return referencedApplication; } }
	}

	//============================================================================
	public class TBWizardReferencedApplicationCancelEventArgs : TBWizardReferencedApplicationEventArgs
	{
		private bool cancel = false;

		//---------------------------------------------------------------------------
		public TBWizardReferencedApplicationCancelEventArgs(string aApplicationName, WizardApplicationInfo aApplication, ActionTaken aAction) : base(aApplicationName, aApplication, aAction)
		{
		}

		//---------------------------------------------------------------------------
		public TBWizardReferencedApplicationCancelEventArgs(string aApplicationName, WizardApplicationInfo aApplication) : this(aApplicationName, aApplication, ActionTaken.Undefined)
		{
		}

		//---------------------------------------------------------------------------
		public bool Cancel { get { return cancel; } set { cancel = value; } }
	}

	public delegate void TBWizardReferencedApplicationEventHandler(object sender, TBWizardReferencedApplicationEventArgs e);
	public delegate void TBWizardReferencedApplicationCancelEventHandler(object sender, TBWizardReferencedApplicationCancelEventArgs e);

	public delegate void InitProgressBarEventHandler(object sender, int progressBarMaximum);
	public delegate void PerformProgressBarStepEventHandler(object sender);
}
