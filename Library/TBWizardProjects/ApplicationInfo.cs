using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;

namespace Microarea.Library.TBWizardProjects
{
	#region WizardApplicationInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardApplicationInfo.
	/// </summary>
	public class WizardApplicationInfo : IDisposable
	{
		public const int ShortNameLength = 4;
		public enum SolutionEdition : ushort
		{
			Standard,
			Professional,
			Enterprise, 
			Undefined
		}
		public enum AppSolutionType : ushort
		{
			AddOn,
			StandAlone,
			Embedded, 
			Undefined
		}

		public static System.Drawing.Font DefaultFont = new Font(DefaultFontFamilyName, DefaultFontSizeInPoints, GraphicsUnit.Point);
		public const string	DefaultVersion = "1.0.0";

		#region WizardApplicationInfo private data members

		private const string	DefaultFontFamilyName = "Verdana";
		private const float		DefaultFontSizeInPoints = 9.0f;

		private string			name = String.Empty;
		private string			title = String.Empty; // il titolo per esteso di un'applicazione, che viene utilizzato ad es. nei menù
		private ApplicationType type = ApplicationType.TaskBuilderApplication;
		private string			producer = String.Empty;
		private string			dbSignature = String.Empty;
		private string			version = DefaultVersion;
		private string			shortName = String.Empty;
		private SolutionEdition edition = SolutionEdition.Undefined;
		private AppSolutionType	solutionType = AppSolutionType.Undefined;
		private string			cultureName = Generics.GetInstalledUICultureName();
		private Font			font = DefaultFont;

		private System.Guid guid = System.Guid.Empty;

		private WizardModuleInfoCollection	modules = null;
		private ArrayList references = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardApplicationInfo(string aApplicationName, bool isReadOnly, bool isReferenced)
		{
			Name = aApplicationName;
			guid = System.Guid.NewGuid();
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardApplicationInfo(string aApplicationName, bool isReadOnly) : this(aApplicationName, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardApplicationInfo(string aApplicationName) : this(aApplicationName, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardApplicationInfo(WizardApplicationInfo aApplicationInfo)
		{
			name = (aApplicationInfo != null) ? aApplicationInfo.Name : String.Empty;
			title = (aApplicationInfo != null) ? aApplicationInfo.Title : String.Empty;
			type = (aApplicationInfo != null) ? aApplicationInfo.Type : ApplicationType.TaskBuilderApplication;
			producer = (aApplicationInfo != null) ? aApplicationInfo.Producer : String.Empty;
			dbSignature = (aApplicationInfo != null) ? aApplicationInfo.DbSignature : String.Empty;
			version = (aApplicationInfo != null) ? aApplicationInfo.Version : String.Empty;
			shortName = (aApplicationInfo != null) ? aApplicationInfo.ShortName : String.Empty;
			edition = (aApplicationInfo != null) ? aApplicationInfo.Edition : SolutionEdition.Undefined;
			solutionType = (aApplicationInfo != null) ? aApplicationInfo.SolutionType : AppSolutionType.Undefined;
			cultureName = (aApplicationInfo != null) ? aApplicationInfo.CultureName : String.Empty;
			font = (aApplicationInfo != null) ? aApplicationInfo.Font : DefaultFont;
			guid = (aApplicationInfo != null) ? aApplicationInfo.Guid : System.Guid.Empty;
			readOnly = (aApplicationInfo != null) ? aApplicationInfo.ReadOnly : false;
			referenced = (aApplicationInfo != null) ? aApplicationInfo.IsReferenced : false;

			if (aApplicationInfo != null)
			{
				if (aApplicationInfo.ModulesInfo != null && aApplicationInfo.ModulesCount > 0)
				{
					foreach(WizardModuleInfo aModuleInfo in aApplicationInfo.ModulesInfo)
						this.AddModuleInfo(new WizardModuleInfo(aModuleInfo));
				}

				string[] referencesToAdd = aApplicationInfo.ReferencedApplications;
				if (referencesToAdd != null && referencesToAdd.Length > 0)
				{
					foreach(string aReferencedApplicationName in referencesToAdd)
						AddReference(aReferencedApplicationName);
				}
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				if (modules != null)
					modules.Clear();

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardApplicationInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardApplicationInfo)obj).Name) == 0 &&
				String.Compare(title, ((WizardApplicationInfo)obj).Title) == 0 &&
				type == ((WizardApplicationInfo)obj).Type &&
				String.Compare(producer, ((WizardApplicationInfo)obj).Producer) == 0 &&
				String.Compare(dbSignature, ((WizardApplicationInfo)obj).DbSignature) == 0 &&
				String.Compare(version, ((WizardApplicationInfo)obj).Version) == 0 &&
				String.Compare(shortName, ((WizardApplicationInfo)obj).ShortName) == 0 &&
				edition == ((WizardApplicationInfo)obj).Edition &&
				solutionType == ((WizardApplicationInfo)obj).SolutionType &&
				String.Compare(cultureName, ((WizardApplicationInfo)obj).CultureName) == 0 &&
				Font.Equals(((WizardApplicationInfo)obj).Font) &&
				Guid.Equals(((WizardApplicationInfo)obj).Guid) &&
				WizardModuleInfoCollection.Equals(modules, ((WizardApplicationInfo)obj).ModulesInfo) &&
				HasSameReferencesAs((WizardApplicationInfo)obj)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		#region WizardApplicationInfo public properties

		//---------------------------------------------------------------------------
		public string Name { get { return name; } set { if (Generics.IsValidApplicationName(value)) name = value; } }
		//---------------------------------------------------------------------------
		public string Title { get { return title; } set { title = value; } }
		//---------------------------------------------------------------------------
		public ApplicationType Type { get { return type; } set { type = value; } }
		//---------------------------------------------------------------------------
		public string Producer { get { return producer; } set { producer = value; } }
		//---------------------------------------------------------------------------
		public string DbSignature 
		{
			get 
			{
				if (dbSignature != null && dbSignature.Trim().Length > 0)
					return dbSignature.Trim(); 

				return name;
			}
			set 
			{ 
				if (value == null)
				{
					dbSignature = String.Empty;
					return;
				}
				
				string newDbSignature = value.Trim();
				if (newDbSignature.Length > Generics.ApplicationDbSignatureMaxLength)
					dbSignature =  newDbSignature.Substring(0, Generics.ApplicationDbSignatureMaxLength).Trim(); 
				else
					dbSignature = newDbSignature;
			}
		}
		//---------------------------------------------------------------------------
		public string Version { get { return version; } set { version = value; } }
		//---------------------------------------------------------------------------
		public string ShortName 
		{ 
			get { return shortName; }
			set 
			{ 
				if (value == null || value.Trim().Length == 0)
				{
					shortName = String.Empty;
				}
				if (value.Length > ShortNameLength)
					shortName = value.Substring(0, ShortNameLength).ToUpper();
				else
					shortName = value.ToUpper();
			} 
		}

		//---------------------------------------------------------------------------
		public SolutionEdition Edition { get { return edition; } set { edition = value; } }
		//---------------------------------------------------------------------------
		public AppSolutionType SolutionType { get { return solutionType; } set { solutionType = value; } }
		//---------------------------------------------------------------------------
		public System.Guid Guid { get { return guid; } set { guid = value; } }
		//---------------------------------------------------------------------------
		public string CultureName 
		{
			get { return cultureName; } 
			set 
			{ 
				if (value != null && value.Length > 0)
				{
					if (
						String.Compare(cultureName, value) == 0 ||
						!Generics.IsValidCultureName(value)
						)
						return;
				}

				cultureName = value;
			} 
		}
		
		//---------------------------------------------------------------------------
		public string CultureDisplayName { get { return Generics.GetCultureDisplayName(cultureName); } }
		//---------------------------------------------------------------------------
		public int CultureLCID { get { return Generics.GetCultureLCID(cultureName); } }

		//---------------------------------------------------------------------------
		public System.Drawing.Font Font { get { return font; } set { font = value; }}
		//---------------------------------------------------------------------------
		public string FontFamilyName { get { return (font != null && font.FontFamily != null) ? font.FontFamily.Name : DefaultFontFamilyName; }}
		//---------------------------------------------------------------------------
		public float FontSizeInPoints { get { return (font != null) ? font.SizeInPoints : DefaultFontSizeInPoints; }}
		//---------------------------------------------------------------------------
		public bool FontBold { get { return (font != null) ? font.Bold : false; }}
		//---------------------------------------------------------------------------
		public bool FontItalic { get { return (font != null) ? font.Italic : false; }}

		//---------------------------------------------------------------------------
		public WizardModuleInfoCollection	ModulesInfo	{ get { return modules; } }
		
		//---------------------------------------------------------------------------
		public int ModulesCount{ get { return (modules != null) ? modules.Count : 0; } }

		//---------------------------------------------------------------------------
		public string[] ReferencedApplications { get { return (references != null && references.Count > 0) ? (string[])references.ToArray(typeof(string)) : null; } }

		//---------------------------------------------------------------------------
		public bool HasLibraries
		{
			get
			{
				if (modules == null || modules.Count == 0)
					return false;
		
				foreach(WizardModuleInfo aModuleInfo in modules)
				{
					if (aModuleInfo.LibrariesCount > 0)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasNotReadOnlyLibraries
		{
			get
			{
				if (modules == null || modules.Count == 0)
					return false;
		
				foreach(WizardModuleInfo aModuleInfo in modules)
				{
					if (aModuleInfo.HasNotReadOnlyLibraries)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public int TablesCount
		{
			get
			{
				if (modules == null || modules.Count == 0)
					return 0;
		
				int tablesCount = 0;
				foreach(WizardModuleInfo aModuleInfo in modules)
					tablesCount += aModuleInfo.TablesCount;

				return tablesCount;
			}
		}
		//---------------------------------------------------------------------------
		public bool HasTables
		{
			get
			{
				if (modules == null || modules.Count == 0)
					return false;
		
				foreach(WizardModuleInfo aModuleInfo in modules)
				{
					if (aModuleInfo.HasTables)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasDocuments
		{
			get
			{
				if (modules == null || modules.Count == 0)
					return false;
		
				foreach(WizardModuleInfo aModuleInfo in modules)
				{
					if (aModuleInfo.HasDocuments)
						return true;
				}
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasEnums
		{
			get
			{
				if (modules == null || modules.Count == 0)
					return false;
		
				foreach(WizardModuleInfo aModuleInfo in modules)
				{
					if (aModuleInfo.EnumsCount > 0)
						return true;
				}
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool HasReferences { get { return (references != null && references.Count > 0); } }

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)

		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 

		#endregion

		#region WizardApplicationInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public WizardModuleInfo GetModuleInfoByName(string aModuleName)
		{
			return (modules != null) ? modules.GetModuleInfoByName(aModuleName) : null;
		}

		//---------------------------------------------------------------------------
		public WizardModuleInfo GetModuleInfoByDbSignature(string aDbSignature)
		{
			return (modules != null) ? modules.GetModuleInfoByDbSignature(aDbSignature) : null;
		}

		//---------------------------------------------------------------------------
		public int AddModuleInfo(WizardModuleInfo aModuleInfo, bool refreshColumnDefaultConstraintNames)
		{
			if (aModuleInfo == null || aModuleInfo.Name == null || aModuleInfo.Name.Length == 0)
				return -1;

			WizardModuleInfo alreadyExistingModule = GetModuleInfoByName(aModuleInfo.Name);
			if (alreadyExistingModule != null)
				return -1;

			if (modules == null)
				modules = new WizardModuleInfoCollection();

			aModuleInfo.SetApplication(this);

			int addedModuleIndex = modules.Add(aModuleInfo);

			if (addedModuleIndex >= 0 && refreshColumnDefaultConstraintNames)
				aModuleInfo.RefreshTablesConstraintsNames();

			return addedModuleIndex;
		}
	
		//---------------------------------------------------------------------------
		public int AddModuleInfo(WizardModuleInfo aModuleInfo)
		{
			return AddModuleInfo(aModuleInfo, true);
		}
		
		//---------------------------------------------------------------------------
		public void RemoveModule(string aModuleName)
		{
			if (modules == null || modules.Count == 0 || aModuleName == null || aModuleName.Length == 0)
				return;

			WizardModuleInfo moduleToRemove = GetModuleInfoByName(aModuleName);
			if (moduleToRemove == null)
				return;

			moduleToRemove.SetApplication(null);
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetLibraryInfoByName(string aLibraryName)
		{
			if (modules == null || modules.Count == 0 || aLibraryName == null || aLibraryName.Length == 0)
				return null;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				WizardLibraryInfo libraryFound = aModuleInfo.GetLibraryInfoByName(aLibraryName);
				if (libraryFound != null)
					return libraryFound;
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetLibraryInfoByNameSpace(NameSpace aNameSpace)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 || 
				aNameSpace == null || 
				aNameSpace.Application == null || 
				aNameSpace.Application.Length == 0 ||
				String.Compare(aNameSpace.Application, name) != 0 ||
				aNameSpace.Module == null || 
				aNameSpace.Module.Length == 0 ||
				aNameSpace.Library == null || 
				aNameSpace.Library.Length == 0
				)
				return null;

			WizardModuleInfo aModuleInfo = GetModuleInfoByName(aNameSpace.Module);
			if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
				return null;

			foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
			{
				if (String.Compare(aLibraryInfo.SourceFolder, aNameSpace.Library) == 0)
					return aLibraryInfo;
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public bool LibraryExists(string aLibraryName)
		{
			return (GetLibraryInfoByName(aLibraryName) != null);
		}
		
		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfoByName(string aTableName)
		{
			if (modules == null || modules.Count == 0 || aTableName == null || aTableName.Length == 0)
				return null;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				WizardTableInfo tableFound = aModuleInfo.GetTableInfoByName(aTableName);
				if (tableFound != null)
					return tableFound;
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfoCollection GetAllTables()
		{
			if (modules == null || modules.Count == 0)
				return null;

			WizardTableInfoCollection tables = new WizardTableInfoCollection();

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.TablesCount > 0)
					tables.AddRange(aModuleInfo.GetAllTables());
			}

			return (tables.Count > 0) ? tables : null;
		}

		//---------------------------------------------------------------------------
		public bool TableExists(string aTableName)
		{
			return (GetTableInfoByName(aTableName) != null);
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsDBTReferredToTable(string aTableName)
		{
			if (modules == null || modules.Count == 0 || aTableName == null || aTableName.Length == 0)
				return false;

			WizardTableInfo tableInfo = GetTableInfoByName(aTableName);
			if (tableInfo == null)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.ExistsDBTReferredToTable(aTableName))
						return true;
				}
			}
			return false;
		}
	
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTsReferredToTable(WizardTableInfo aTableInfo)
		{
			if (aTableInfo == null)
				return null;

			WizardDBTInfoCollection dbtsReferredToTable = new WizardDBTInfoCollection();
			
			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					dbtsReferredToTable.AddRange(aLibraryInfo.GetDBTsReferredToTable(aTableInfo.Name));
			}
			return dbtsReferredToTable;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTsReferredToTable(string aTableName)
		{
			return GetDBTsReferredToTable(GetTableInfoByName(aTableName));
		}

		//---------------------------------------------------------------------------
		public void CheckDBTsReferredToTable(string aTableName)
		{
			WizardTableInfo tableInfo = GetTableInfoByName(aTableName);
			if (tableInfo == null)
				return;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					aLibraryInfo.CheckDBTsReferredToTable(tableInfo);
			}
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTSlavesAttachedToMasterTable(string aMasterTableName)
		{
			if (modules == null || modules.Count == 0 || aMasterTableName == null || aMasterTableName.Length == 0)
				return null;

			aMasterTableName = aMasterTableName.Trim();
			if (aMasterTableName.Length == 0 || !Generics.IsValidTableName(aMasterTableName))
				return null;

			WizardDBTInfoCollection attachedSlaves = new WizardDBTInfoCollection();

			foreach(WizardModuleInfo aModuleInfo in modules)
				attachedSlaves.AddRange(aModuleInfo.GetDBTSlavesAttachedToMasterTable(aMasterTableName));

			return (attachedSlaves.Count > 0) ? attachedSlaves : null;
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsForeignKeysReferredToTable(string aTableNameSpace)
		{
			if (modules == null || modules.Count == 0 || aTableNameSpace == null || aTableNameSpace.Length == 0)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.ExistsForeignKeysReferredToTable(aTableNameSpace))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfoCollection GetForeignKeysReferencedToTable(string aTableNameSpace)
		{
			if (modules == null || modules.Count == 0 || aTableNameSpace == null || aTableNameSpace.Length == 0)
				return null;

			NameSpace tmpNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!tmpNameSpace.IsValid())
				return null;

			WizardForeignKeyInfoCollection foreignKeysRelatedToTable = new WizardForeignKeyInfoCollection();

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;
				
				foreignKeysRelatedToTable.AddRange(aModuleInfo.GetForeignKeysReferencedToTable(aTableNameSpace));
			}
			return (foreignKeysRelatedToTable.Count > 0) ? foreignKeysRelatedToTable : null;
		}
	
		//---------------------------------------------------------------------------
		public bool IsColumnInvolvedInForeignKeyDefinition(string aTableNameSpace, string aColumnName)
		{
			if (aTableNameSpace == null || aTableNameSpace.Length == 0 || aColumnName == null || aColumnName.Length == 0)
				return false;

			NameSpace referencedTableNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!referencedTableNameSpace.IsValid())
				return false;
			
			WizardForeignKeyInfoCollection foreignKeysRelatedToTable = GetForeignKeysReferencedToTable(aTableNameSpace);
			if (foreignKeysRelatedToTable == null || foreignKeysRelatedToTable.Count == 0)
				return false;

			foreach (WizardForeignKeyInfo aForeignKeyInfo in foreignKeysRelatedToTable)
			{
				if (aForeignKeyInfo.GetKeySegmentInfoByReferencedColumnName(aColumnName) != null)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTsUsingForeignKey(WizardTableInfo aTableInfo, WizardForeignKeyInfo aForeignKeyInfo)
		{
			if (aTableInfo == null || aForeignKeyInfo == null)
				return null;

			WizardDBTInfoCollection allDBTsReferredToTable = GetDBTsReferredToTable(aTableInfo);
			if (allDBTsReferredToTable == null || allDBTsReferredToTable.Count == 0)
				return null;

			WizardDBTInfoCollection allDBTsUsingForeignKey = new WizardDBTInfoCollection();

			foreach(WizardDBTInfo aDBTInfo in allDBTsReferredToTable)
			{
				if ((!aDBTInfo.IsSlave && !aDBTInfo.IsSlaveBuffered))
					continue;

				WizardForeignKeyInfo otherForeignKeyInfo = aDBTInfo.GetForeignKeyInfo();
				if 
					(
					otherForeignKeyInfo != null &&
					otherForeignKeyInfo.SegmentsCount > 0 &&
					String.Compare(otherForeignKeyInfo.ReferencedTableNameSpace, aForeignKeyInfo.ReferencedTableNameSpace) == 0 &&
					otherForeignKeyInfo.HasSameSegments(aForeignKeyInfo.Segments)
					)
					allDBTsUsingForeignKey.Add(aDBTInfo);
			}
			
			return (allDBTsUsingForeignKey.Count > 0) ? allDBTsUsingForeignKey : null;
		}

		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (modules == null || modules.Count == 0 || aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public void RemoveTable(string aTableName)
		{
			if (aTableName == null || aTableName.Length == 0)
				return;

			WizardTableInfo tableToRemove = GetTableInfoByName(aTableName);

			if (tableToRemove == null || tableToRemove.Library == null)
				return;

			tableToRemove.Library.RemoveTable(aTableName);
		}

		//---------------------------------------------------------------------------
		public void RefreshTablesConstraintsNames(bool forceReset)
		{
			if (modules == null || modules.Count == 0)
				return;
			
			foreach(WizardModuleInfo aModuleInfo in modules)
				aModuleInfo.RefreshTablesConstraintsNames(forceReset);
		}

		//---------------------------------------------------------------------------
		public void RefreshTablesConstraintsNames()
		{
			RefreshTablesConstraintsNames(false);
		}
		
		//---------------------------------------------------------------------------
		public WizardLibraryInfoCollection GetLibraryAvailableDependencies
			(
			WizardLibraryInfo				aLibraryInfo,
			WizardApplicationInfoCollection referencedApplications
			)
		{
			if (modules == null || modules.Count == 0 || aLibraryInfo == null)
				return null;

			WizardLibraryInfoCollection dependenciesList = new WizardLibraryInfoCollection();
			
			foreach(WizardModuleInfo aApplicationModuleInfo in modules)
			{
				if (aApplicationModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aModuleLibraryInfo in aApplicationModuleInfo.LibrariesInfo)
				{
					if 
						(
						aModuleLibraryInfo.Module != null &&
						String.Compare(aApplicationModuleInfo.Name, aModuleLibraryInfo.Module.Name) == 0 &&
						String.Compare(aModuleLibraryInfo.Name, aLibraryInfo.Name) == 0
						)
						continue; // si tratta della stessa libreria

					if (aModuleLibraryInfo.DependsOn(aLibraryInfo))
						continue; // non si possono avere dipendenze incrociate !!!

					dependenciesList.Add(aModuleLibraryInfo);
				}
			}

			if (referencedApplications != null && referencedApplications.Count > 0)
			{
				foreach(WizardApplicationInfo aReferencedApplication in referencedApplications)
				{
					if (aReferencedApplication == null || aReferencedApplication.ModulesCount == 0)
						continue;

					foreach(WizardModuleInfo aReferencedModule in aReferencedApplication.ModulesInfo)
					{
						if (aReferencedModule == null || aReferencedModule.LibrariesCount == 0)
							continue;

						foreach(WizardLibraryInfo aReferencedLibrary in aReferencedModule.LibrariesInfo)
						{
							if (aReferencedLibrary.DependsOn(aLibraryInfo))
								continue; // non si possono avere dipendenze incrociate !!!

							dependenciesList.Add(aReferencedLibrary);
						}
					}
				}
			}

			return dependenciesList;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfoCollection GetLibraryAvailableDependencies(WizardLibraryInfo aLibraryInfo)
		{
			return GetLibraryAvailableDependencies(aLibraryInfo, null);
		}
		
		//---------------------------------------------------------------------------
		public WizardLibraryInfoCollection GetLibrariesBuildOrderedList()
		{
			if (modules == null || modules.Count == 0)
				return null;
	
			WizardLibraryInfoCollection buildOrderedList = new WizardLibraryInfoCollection();

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if 
					(
					aModuleInfo.LibrariesInfo == null || 
					aModuleInfo.LibrariesCount == 0
					)
					continue;
				
				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					aLibraryInfo.FillLibrariesBuildOrderedList(ref buildOrderedList);
			}
			return buildOrderedList;
		}

		//---------------------------------------------------------------------------
		public bool ExistsDocumentUsingDBT(WizardDBTInfo aDBTToSearch)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				aDBTToSearch == null || 
				aDBTToSearch.Library == null || 
				aDBTToSearch.Library.Application != this
				)
				return false;

			// Devo controllare se la libreria in cui è definito il DBT 
			// contiene un documento che utilizza il DBT in questione.
			// Se così non è, occorre anche verificare se in almeno una delle  
			// librerie che dipendono da essa risulta definito un simile
			// documento
			if (aDBTToSearch.Library.ExistsDocumentUsingDBT(aDBTToSearch.Name))
				return true;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.DependsOn(aDBTToSearch.Library) && aLibraryInfo.ExistsDocumentUsingDBT(aDBTToSearch.Name))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection GetDocumentsUsingDBT(WizardDBTInfo aDBTToSearch)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				aDBTToSearch == null || 
				aDBTToSearch.Library == null || 
				aDBTToSearch.Library.Application != this
				)
				return null;

			WizardDocumentInfoCollection documentsFound = new WizardDocumentInfoCollection();
			
			// Prima carico gli eventuali documenti definiti nella libreria 
			// in cui è definito il DBT che utilizzano il DBT in questione.
			// Poi, occorre anche caricare gli eventuali documenti appartenenti   
			// a librerie che dipendono da essa.
			documentsFound.AddRange(aDBTToSearch.Library.GetDocumentsUsingDBT(aDBTToSearch.Name));

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.DependsOn(aDBTToSearch.Library))
						documentsFound.AddRange(aLibraryInfo.GetDocumentsUsingDBT(aDBTToSearch.Name));
				}
			}
			return (documentsFound != null && documentsFound.Count > 0) ? documentsFound : null;
		}

		//---------------------------------------------------------------------------
		public bool ExistsClientDocumentAttachedToDocument(WizardDocumentInfo aServerDocumentToSearch)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				aServerDocumentToSearch == null || 
				aServerDocumentToSearch.Library == null || 
				aServerDocumentToSearch.Library.Application == null
				)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.ExistsClientDocumentAttachedToDocument(aServerDocumentToSearch))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfoCollection GetClientDocumentsUsingDBT(WizardDBTInfo aDBTToSearch)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				aDBTToSearch == null || 
				aDBTToSearch.Library == null || 
				aDBTToSearch.Library.Application != this
				)
				return null;

			WizardClientDocumentInfoCollection clientDocumentsFound = new WizardClientDocumentInfoCollection();
			
			// Prima carico gli eventuali client document definiti nella libreria 
			// in cui è definito il DBT che utilizzano il DBT in questione.
			// Poi, occorre anche caricare gli eventuali client document appartenenti   
			// a librerie che dipendono da essa.
			clientDocumentsFound.AddRange(aDBTToSearch.Library.GetClientDocumentsUsingDBT(aDBTToSearch.Name));

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.DependsOn(aDBTToSearch.Library))
						clientDocumentsFound.AddRange(aLibraryInfo.GetClientDocumentsUsingDBT(aDBTToSearch.Name));
				}
			}
			return (clientDocumentsFound != null && clientDocumentsFound.Count > 0) ? clientDocumentsFound : null;
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsAttachedDBTSlaves(WizardDBTInfo aDBTMaster)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				aDBTMaster == null || 
				!aDBTMaster.IsMaster ||
				aDBTMaster.Library == null || 
				aDBTMaster.Library.Application != this
				)
				return false;

			// Devo controllare se la libreria in cui è definito il DBT 
			// contiene uno slave riferito al DBT Master in questione.
			// Se così non è, occorre anche verificare se in almeno una delle  
			// librerie che dipendono da essa risulta definito un DBT simile
			if (aDBTMaster.Library.ExistsAttachedDBTSlaves(aDBTMaster))
				return true;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.DependsOn(aDBTMaster.Library) && aLibraryInfo.ExistsAttachedDBTSlaves(aDBTMaster))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetAttachedDBTSlaves(WizardDBTInfo aDBTMaster)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				aDBTMaster == null || 
				!aDBTMaster.IsMaster ||
				aDBTMaster.Library == null || 
				aDBTMaster.Library.Application != this
				)
				return null;

			WizardDBTInfoCollection attachedSlaves = new WizardDBTInfoCollection();

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo == aDBTMaster.Library || aLibraryInfo.DependsOn(aDBTMaster.Library))
						attachedSlaves.AddRange(aLibraryInfo.GetAttachedDBTSlaves(aDBTMaster));
				}
			}
			return (attachedSlaves.Count > 0) ? attachedSlaves : null;
		}

		//---------------------------------------------------------------------------
		public bool ExistLibraryDependencies(WizardLibraryInfo aLibraryToSearch)
		{
			if 
				(
				modules == null ||
				modules.Count == 0 ||
				aLibraryToSearch == null || 
				aLibraryToSearch.Application != this
				)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo != aLibraryToSearch && aLibraryInfo.DependsOn(aLibraryToSearch))
						return true;
				}
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsTableUsingEnum(WizardEnumInfo aEnumInfo)
		{
			if (aEnumInfo == null || modules == null || modules.Count == 0)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.ExistsTableUsingEnum(aEnumInfo))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByName(string aEnumName)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				!Generics.IsValidEnumName(aEnumName)
				)
				return null;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.EnumsCount == 0)
					continue;

				WizardEnumInfo enumFound = aModuleInfo.GetEnumInfoByName(aEnumName);
				if (enumFound != null)
					return enumFound;
			}
		
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardModuleInfo GetModuleContainingEnum(string aEnumName)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				!Generics.IsValidEnumName(aEnumName)
				)
				return null;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.EnumsCount == 0)
					continue;

				WizardEnumInfo enumFound = aModuleInfo.GetEnumInfoByName(aEnumName);
				if (enumFound != null)
					return aModuleInfo;
			}
		
			return null;
		}

		//---------------------------------------------------------------------------
		public bool ExistEnumInfo(string aEnumName)
		{
			return (GetEnumInfoByName(aEnumName) != null);
		}
		
		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByValue(ushort aEnumValue)
		{
			if 
				(
				modules == null || 
				modules.Count == 0 ||
				!Generics.IsValidEnumValue(aEnumValue)
				)
				return null;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.EnumsCount == 0)
					continue;

				WizardEnumInfo enumFound = aModuleInfo.GetEnumInfoByValue(aEnumValue);
				if (enumFound != null)
					return enumFound;
			}
			
			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByItemStoredValue(uint aEnumItemStoredValue)
		{
			return GetEnumInfoByValue((ushort)(aEnumItemStoredValue / 65536));
		}

		//---------------------------------------------------------------------------
		public bool ExistEnumInfo(ushort aEnumValue)
		{
			return (GetEnumInfoByValue(aEnumValue) != null);
		}
		
		//---------------------------------------------------------------------------
		public ushort GetMaxEnumValue()
		{
			ushort maxEnumValue = Generics.GetInstalledApplicationsEnumsTagMaximum(name);

			if (modules != null && modules.Count > 0)
			{
				foreach(WizardModuleInfo aModuleInfo in modules)
				{
					if (aModuleInfo.EnumsCount == 0)
						continue;

					ushort moduleMaxEnumValue = aModuleInfo.GetMaxEnumValue();
					if (maxEnumValue < moduleMaxEnumValue)
						maxEnumValue = moduleMaxEnumValue;
				}
			}

			return maxEnumValue;
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfoCollection GetEnums(bool sortByName)
		{
			if (modules == null || modules.Count == 0)
				return null;

			WizardEnumInfoCollection allEnums = new WizardEnumInfoCollection();

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.EnumsCount == 0)
					continue;

				allEnums.AddRange(aModuleInfo.EnumsInfo);
			}
		
			if (sortByName)
				allEnums.SortByName();

			return allEnums;
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfoCollection GetEnums()
		{
			return GetEnums(false);
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsDocumentsUsingExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			if (aExtraAddedColumnsInfo == null || modules == null || modules.Count == 0)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.ExistsDocumentsUsingExtraAddedColumnsInfo(aExtraAddedColumnsInfo))
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public string GetCultureLanguageIdentifierText()
		{
			return Generics.GetCultureLanguageIdentifierText(cultureName);
		}
		
		//---------------------------------------------------------------------------
		public string GetCultureSubLanguageIdentifierText()
		{
			return Generics.GetCultureSubLanguageIdentifierText(cultureName);
		}

		//---------------------------------------------------------------------------
		public string GetCodeSolutionPath(IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || name == null || name.Length == 0)
				return String.Empty;

			string applicationPath = aPathFinder.GetStandardApplicationContainerPath(type);
			if (applicationPath == null || applicationPath.Length == 0)
				return String.Empty;

			applicationPath += Path.DirectorySeparatorChar;
			applicationPath += name;

			return applicationPath + Path.DirectorySeparatorChar + NameSolverStrings.Solutions;
		}
		
		//---------------------------------------------------------------------------
		public string GetCodeSolutionFileName(IBasePathFinder aPathFinder)
		{
			string solutionsPath = GetCodeSolutionPath(aPathFinder);
			if (solutionsPath == null || solutionsPath.Length == 0)
				return String.Empty;

			return solutionsPath + Path.DirectorySeparatorChar + name + NameSolverStrings.SolutionExtension;
		}

		//---------------------------------------------------------------------------
		public string GetCodeSolutionModulesPath(IBasePathFinder aPathFinder)
		{
			string solutionsPath = GetCodeSolutionPath(aPathFinder);
			if (solutionsPath == null || solutionsPath.Length == 0)
				return String.Empty;

			return solutionsPath + Path.DirectorySeparatorChar + NameSolverStrings.Modules;
		}

		
		//---------------------------------------------------------------------------
		public string GetCodeNetSolutionFileName(IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || name == null || name.Length == 0)
				return String.Empty;

			string applicationPath = aPathFinder.GetStandardApplicationContainerPath(type);
			if (applicationPath == null || applicationPath.Length == 0)
				return String.Empty;

			applicationPath += Path.DirectorySeparatorChar;
			applicationPath += name;

			return applicationPath + Path.DirectorySeparatorChar + name + Generics.NetSolutionExtension;
		}

		//---------------------------------------------------------------------------
		public string GetLocalizerSolutionFileName(IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || name == null || name.Length == 0)
				return String.Empty;

			string applicationPath = aPathFinder.GetStandardApplicationContainerPath(type);
			if (applicationPath == null || applicationPath.Length == 0)
				return String.Empty;

			applicationPath += Path.DirectorySeparatorChar;
			applicationPath += name;

			return applicationPath + Path.DirectorySeparatorChar + name + Generics.TBLocalizerSolutionExtension;
		}

		//---------------------------------------------------------------------------
		public bool ExistsCodeNetSolutionFile(IBasePathFinder aPathFinder)
		{
			if (aPathFinder == null || name == null || name.Length == 0)
				return false;

			string solutionFileName = this.GetCodeNetSolutionFileName(aPathFinder);
			if (solutionFileName == null || solutionFileName.Length == 0)
				return false;

			return File.Exists(solutionFileName);
		}
		
		//---------------------------------------------------------------------------
		public bool CanRunActivationWizard(IBasePathFinder aPathFinder)
		{
			string solutionFileName = GetCodeSolutionFileName(aPathFinder);
			if (solutionFileName == null || solutionFileName.Length == 0 || !File.Exists(solutionFileName))
				return false;

			// Controllo che per ogni SalesModule elencato nel file di solution
			// esista il corrispondente file nella sottocartella Modules
			try
			{
				XmlDocument solutionFileDocument = new XmlDocument();
				solutionFileDocument.Load(solutionFileName);

				if (solutionFileDocument.DocumentElement == null)
					return false;

				XmlNodeList salesModuleNames = solutionFileDocument.DocumentElement.SelectNodes("SalesModule");
				if (salesModuleNames != null && salesModuleNames.Count > 0)
				{
					string modulesPath = GetCodeSolutionModulesPath(aPathFinder);
					foreach (XmlNode aSalesModuleNode in salesModuleNames)
					{
						if 
							(
							aSalesModuleNode == null || 
							!(aSalesModuleNode is XmlElement) || 
							!((XmlElement)aSalesModuleNode).HasAttribute("name")
							)
							continue;
						string salesModuleName = ((XmlElement)aSalesModuleNode).GetAttribute("name");
						if (salesModuleName == null || salesModuleName.Trim().Length == 0)
							continue;
				
						string filename = Path.Combine(modulesPath, salesModuleName.Trim() + NameSolverStrings.XmlExtension);
						if (!File.Exists(filename))
							return false;
					}
				}

				return true;
			}
			catch(XmlException)
			{
				return false;
			}
		}
		
		//---------------------------------------------------------------------------
		public void SetFont(string aFontFamilyName, float aFontSizeInPoints, bool isBold, bool isItalic)
		{
			if (aFontFamilyName == null || aFontFamilyName.Length == 0)
				aFontFamilyName = DefaultFontFamilyName;

			if (aFontSizeInPoints == 0)
				aFontSizeInPoints = DefaultFontSizeInPoints;

			FontStyle fontStyle = FontStyle.Regular;
			if (isBold)
				fontStyle |= FontStyle.Bold;
			if (isItalic)
				fontStyle |= FontStyle.Italic;

			font = new System.Drawing.Font(aFontFamilyName, aFontSizeInPoints, fontStyle, GraphicsUnit.Point);
		}

		//---------------------------------------------------------------------------
		public void AddReference(string aApplicationName)
		{
			if 
				(
				aApplicationName == null || 
				aApplicationName.Length == 0 ||
				String.Compare(aApplicationName, name) == 0
				)
				return;

			if (References(aApplicationName))
				return;

			if (references == null)
				references = new ArrayList();

			references.Add(aApplicationName);

			references.Sort();
		}
		
		//---------------------------------------------------------------------------
		public bool RemoveReference(string applicationNameToRemove)
		{
			if (references == null || references.Count == 0 || applicationNameToRemove == null || applicationNameToRemove.Length == 0)
				return false;

			for(int i=(references.Count-1); i >= 0; i--)
			{
				string aReferencedApplicationName = (string)references[i];
				
				int lexicalRelationship = String.Compare(aReferencedApplicationName, applicationNameToRemove);

				// Se aReferencedApplicationName è minore di applicationNameToRemove (viene prima in ordine
				// alfabetico) posso uscire, visto che la lista è ordinata alfabeticamente e la sto scorrendo
				// in senso contrario (dall'ultimo elemento verso il primo)
				if (lexicalRelationship < 0) 
					break;
		
				if (lexicalRelationship == 0) // ho trovato applicationNameToRemove
				{
					if (!IsReferenceRemoveable(aReferencedApplicationName))
						return false;
					
					references.RemoveAt(i);
					return true;
				}			
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public void ClearReferences()
		{
			if 
				(
				references == null ||
				references.Count == 0
				)
				return;

			for(int i=(references.Count-1); i >= 0; i--)
			{
				if (IsReferenceRemoveable((string)references[i]))
					references.RemoveAt(i);
			}
		}
		
		//---------------------------------------------------------------------------
		public void SetReferences(string[] applicationNames)
		{
			ClearReferences();

			if 
				(
				applicationNames == null || 
				applicationNames.Length == 0 
				)
				return;

			foreach(string aApplicationName in applicationNames)
				AddReference(aApplicationName);
		}
		
		//---------------------------------------------------------------------------
		public bool References(string anotherApplicationName)
		{
			if 
				(
				anotherApplicationName == null || 
				anotherApplicationName.Length == 0 ||
				String.Compare(anotherApplicationName, name) == 0 ||
				references == null ||
				references.Count == 0
				)
				return false;

			foreach(string aReferencedApplicationName in references)
			{
				int lexicalRelationship = String.Compare(aReferencedApplicationName, anotherApplicationName);
				if (lexicalRelationship == 0)
					return true;
				
				// Se aReferencedApplicationName è maggiore di anotherApplicationName (viene dopo in ordine
				// alfabetico) posso uscire, visto che la lista è ordinata alfabeticamente
				if (lexicalRelationship > 0) 
					return false;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasSameReferencesAs(WizardApplicationInfo anotherApplicationInfo)
		{
			if (anotherApplicationInfo == null)
				return false;
			
			if (anotherApplicationInfo == this)
				return true;

			string[] otherReferences = anotherApplicationInfo.ReferencedApplications;
			if (references == null || references.Count == 0)
				return (otherReferences == null || otherReferences.Length == 0);

			if (otherReferences == null || references.Count != otherReferences.Length)
				return false;

			for(int i = 0; i < references.Count; i++)
			{
				if (String.Compare((string)references[i], otherReferences[i]) != 0)
					return false;
			}
			return true;
		}
		
		//---------------------------------------------------------------------------
		public bool IsReferenceRemoveable(string aReferencedApplicationName)
		{
			return !References(aReferencedApplicationName) || !HasClientDocumentsReferredToApplication(aReferencedApplicationName);
		}
		
		//---------------------------------------------------------------------------
		public bool HasClientDocumentsReferredToApplication(string aReferencedApplicationName)
		{
			if (modules == null || modules.Count == 0 || aReferencedApplicationName == null || aReferencedApplicationName.Length == 0)
				return false;

			foreach(WizardModuleInfo aModuleInfo in modules)
			{
				if (aModuleInfo.LibrariesCount == 0)
					continue;

				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo.HasClientDocumentsReferredToApplication(aReferencedApplicationName))
						return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection GetAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if (aDBTInfo == null || modules == null || modules.Count == 0)
				return null;
 
			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = new WizardDocumentTabbedPaneInfoCollection();
			
			foreach(WizardModuleInfo aModuleInfo in modules)
				dbtTabbedPanes.AddRange(aModuleInfo.GetAllDBTTabbedPanes(aDBTInfo));

			return (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0) ? dbtTabbedPanes : null;
		}
		
		#endregion
	}

	#endregion // WizardApplicationInfo class

	#region WizardApplicationInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardApplicationInfoCollection.
	/// </summary>
	public class WizardApplicationInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardApplicationInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardApplicationInfo))
					throw new NotSupportedException();

				this[index] = (WizardApplicationInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardApplicationInfo))
				throw new NotSupportedException();

			return this.Contains((WizardApplicationInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardApplicationInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardApplicationInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardApplicationInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardApplicationInfo))
				throw new NotSupportedException();

			Insert(index, (WizardApplicationInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardApplicationInfo))
				throw new NotSupportedException();

			Remove((WizardApplicationInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardApplicationInfo this[int index]
		{
			get {  return (WizardApplicationInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardApplicationInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardApplicationInfo[] ToArray()
		{
			return (WizardApplicationInfo[])InnerList.ToArray(typeof(WizardApplicationInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardApplicationInfo aApplicationToAdd)
		{
			if (Contains(aApplicationToAdd))
				return IndexOf(aApplicationToAdd);

			return InnerList.Add(aApplicationToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardApplicationInfoCollection aApplicationsCollectionToAdd)
		{
			if (aApplicationsCollectionToAdd == null || aApplicationsCollectionToAdd.Count == 0)
				return;

			foreach (WizardApplicationInfo aApplicationToAdd in aApplicationsCollectionToAdd)
				Add(aApplicationToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardApplicationInfo aApplicationToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aApplicationToInsert))
				return;

			InnerList.Insert(index, aApplicationToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardApplicationInfo beforeApplication, WizardApplicationInfo aApplicationToInsert)
		{
			if (beforeApplication == null)
				Add(aApplicationToInsert);

			if (!Contains(beforeApplication))
				return;

			if (Contains(aApplicationToInsert))
				return;

			Insert(IndexOf(beforeApplication), aApplicationToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardApplicationInfo aApplicationToRemove)
		{
			if (!Contains(aApplicationToRemove))
				return;

			InnerList.Remove(aApplicationToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardApplicationInfo aApplicationToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aApplicationToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool Contains(string aApplicationNameToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem is WizardApplicationInfo && String.Compare(((WizardApplicationInfo)aItem).Name, aApplicationNameToSearch) == 0)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardApplicationInfo aApplicationToSearch)
		{
			if (!Contains(aApplicationToSearch))
				return -1;
			
			return InnerList.IndexOf(aApplicationToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardApplicationInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardApplicationInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardApplicationInfo.Equals(this[i], ((WizardApplicationInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	
		//---------------------------------------------------------------------------
		public WizardApplicationInfo GetApplicationInfoByName(string aApplicationName)
		{
			if (this.Count == 0 || !Generics.IsValidApplicationName(aApplicationName))
				return null;

			foreach(WizardApplicationInfo aApplicationInfo in InnerList)
			{
				if (String.Compare(aApplicationName, aApplicationInfo.Name) == 0)
					return aApplicationInfo;
			}
			return null;
		}
		
	}

	#endregion // WizardApplicationInfoCollection class

	#region WizardModuleInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardModuleInfo.
	/// </summary>
	public class WizardModuleInfo : IDisposable
	{
		#region WizardModuleInfo private data members

		private WizardApplicationInfo application = null; // parent

		private string		name = String.Empty;
		private string		title = String.Empty; // il titolo per esteso di un modulo, che viene utilizzato ad es. nei menù
		private string		dbSignature = String.Empty;
		private uint		dbReleaseNumber = 1;
        private System.Guid guid = System.Guid.Empty;
		
		private WizardLibraryInfoCollection	libraries = null;
		private WizardEnumInfoCollection enums = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardModuleInfo(string aModuleName, bool isReadOnly, bool isReferenced)
		{
			Name = aModuleName;
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardModuleInfo(string aModuleName, bool isReadOnly) : this(aModuleName, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardModuleInfo(string aModuleName) : this(aModuleName, false)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardModuleInfo(WizardModuleInfo aModuleInfo)
		{
			application = (aModuleInfo != null) ? aModuleInfo.Application : null;
			name = (aModuleInfo != null) ? aModuleInfo.Name : String.Empty;
			title = (aModuleInfo != null) ? aModuleInfo.Title : String.Empty;
			dbSignature = (aModuleInfo != null) ? aModuleInfo.DbSignature : String.Empty;
			dbReleaseNumber = (aModuleInfo != null) ? aModuleInfo.DbReleaseNumber : 1;
            guid = (aModuleInfo != null) ? aModuleInfo.Guid : System.Guid.Empty;
            readOnly = (aModuleInfo != null) ? aModuleInfo.ReadOnly : false;
			referenced = (aModuleInfo != null) ? aModuleInfo.IsReferenced : false;

			if (aModuleInfo != null && aModuleInfo.LibrariesInfo != null && aModuleInfo.LibrariesCount > 0)
			{
				foreach(WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					this.AddLibraryInfo(new WizardLibraryInfo(aLibraryInfo));
			}
			
			if (aModuleInfo != null && aModuleInfo.EnumsInfo != null && aModuleInfo.EnumsCount > 0)
			{
				foreach(WizardEnumInfo aEnumInfo in aModuleInfo.EnumsInfo)
					this.AddEnumInfo(new WizardEnumInfo(aEnumInfo));
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardModuleInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardModuleInfo)obj).Name) == 0 &&
				String.Compare(title, ((WizardModuleInfo)obj).Title) == 0 &&
				String.Compare(dbSignature, ((WizardModuleInfo)obj).DbSignature) == 0 &&
				dbReleaseNumber == ((WizardModuleInfo)obj).DbReleaseNumber &&
                Guid.Equals(((WizardModuleInfo)obj).Guid) &&
                WizardLibraryInfoCollection.Equals(libraries, ((WizardModuleInfo)obj).LibrariesInfo) &&
				WizardEnumInfoCollection.Equals(enums, ((WizardModuleInfo)obj).EnumsInfo)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetApplication(WizardApplicationInfo aApplicationInfo)
		{
			if (application == aApplicationInfo)
				return;

			if (application != null && application.ModulesInfo.Contains(this))
				application.ModulesInfo.Remove(this);

            if (aApplicationInfo != null && guid.Equals(System.Guid.Empty))
                guid = System.Guid.NewGuid();

            application = aApplicationInfo;
		}
		
		#region WizardModuleInfo public properties

		//---------------------------------------------------------------------------
		public WizardApplicationInfo Application { get { return application; } }
		//---------------------------------------------------------------------------
		public string Name 
		{ 
			get { return name; } 
			set 
			{ 
				if (String.Compare(name, value) == 0)
					return;
					
				if (!Generics.IsValidModuleName(value))
					return;

				if (application != null) 
				{
					WizardModuleInfo existingModule = Application.GetModuleInfoByName(value);
					if (existingModule != null && existingModule != this)
						return;
				}
				
				name = value; 
			} 
		}
		//---------------------------------------------------------------------------
		public string Title { get { return title; } set { title = value; } }
		//---------------------------------------------------------------------------
		public string DbSignature 
		{
			get
			{
				if (dbSignature != null && dbSignature.Trim().Length > 0)
					return dbSignature.Trim(); 

				return name;
			}
			set 
			{ 
				if (value == null)
				{
					dbSignature = String.Empty;
					return;
				}
				
				string newDbSignature = value.Trim();
				if (newDbSignature.Length > Generics.ModuleDbSignatureMaxLength)
					dbSignature =  newDbSignature.Substring(0, Generics.ModuleDbSignatureMaxLength).Trim(); 
				else
					dbSignature = newDbSignature;
			}
		}

		//---------------------------------------------------------------------------
		public uint DbReleaseNumber { get { return dbReleaseNumber; } set { dbReleaseNumber = value; } }
        //---------------------------------------------------------------------------
        public System.Guid Guid { get { return guid; } set { guid = value; } }
        //---------------------------------------------------------------------------
		public WizardLibraryInfoCollection	LibrariesInfo	{ get { return libraries; } }
		//---------------------------------------------------------------------------
		public int LibrariesCount { get { return (libraries != null) ? libraries.Count : 0; } }
		//---------------------------------------------------------------------------
		public WizardEnumInfoCollection	EnumsInfo { get { return enums; } }
		//---------------------------------------------------------------------------
		public int EnumsCount { get { return (enums != null) ? enums.Count : 0; } }
		
		//---------------------------------------------------------------------------
		public int TablesCount
		{
			get
			{
				if (libraries == null || libraries.Count == 0)
					return 0;

				int tablesCount = 0;
				foreach(WizardLibraryInfo aLibraryInfo in libraries)
					tablesCount += aLibraryInfo.TablesCount;

				return tablesCount;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasTables
		{
			get
			{
				if (libraries == null || libraries.Count == 0)
					return false;

				foreach(WizardLibraryInfo aLibraryInfo in libraries)
				{
					if (aLibraryInfo.TablesCount > 0)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasDocuments
		{
			get
			{
				if (libraries == null || libraries.Count == 0)
					return false;

				foreach(WizardLibraryInfo aLibraryInfo in libraries)
				{
					if (aLibraryInfo.DocumentsCount > 0)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasClientDocuments
		{
			get
			{
				if (libraries == null || libraries.Count == 0)
					return false;

				foreach(WizardLibraryInfo aLibraryInfo in libraries)
				{
					if (aLibraryInfo.ClientDocumentsCount > 0)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasExtraAddedColumns
		{
			get
			{
				if (libraries == null || libraries.Count == 0)
					return false;

				foreach(WizardLibraryInfo aLibraryInfo in libraries)
				{
					if (aLibraryInfo.ExtraAddedColumnsCount > 0)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool HasNotReadOnlyLibraries
		{
			get
			{
				if (libraries == null || libraries.Count == 0)
					return false;
		
				foreach(WizardLibraryInfo aLibraryInfo in libraries)
				{
					if (!aLibraryInfo.ReadOnly)
						return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 
		
		#endregion
	
		#region WizardModuleInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public string GetNameSpace()
		{
			if 
				(
				name == null || 
				name.Trim().Length == 0 ||
				Application == null || 
				Application.Name == null || 
				Application.Name.Trim().Length == 0
				)
				return String.Empty;

			return Application.Name.Trim() + "." + name;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetLibraryInfoByName(string aLibraryName)
		{
			if (libraries == null || libraries.Count == 0 || aLibraryName == null || aLibraryName.Length == 0)
				return null;

			return libraries.GetLibraryInfoByName(aLibraryName);
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetLibraryInfoBySourceFolder(string aLibrarySourceFolder)
		{
			if (libraries == null || libraries.Count == 0 || aLibrarySourceFolder == null)
				return null;

			aLibrarySourceFolder = aLibrarySourceFolder.Trim();

			if (aLibrarySourceFolder.Length == 0 || !Generics.IsValidPathName(aLibrarySourceFolder))
				return null;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (String.Compare(aLibrarySourceFolder, aLibraryInfo.SourceFolder, true) == 0)
					return aLibraryInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public int AddLibraryInfo(WizardLibraryInfo aLibraryInfo, bool refreshColumnDefaultConstraintNames)
		{
			if (aLibraryInfo == null || aLibraryInfo.Name == null || aLibraryInfo.Name.Length == 0)
				return -1;

			WizardLibraryInfo alreadyExistingLibrary = GetLibraryInfoByName(aLibraryInfo.Name);
			if (alreadyExistingLibrary != null)
				return -1;

			if (libraries == null)
				libraries = new WizardLibraryInfoCollection();

			aLibraryInfo.SetModule(this);

			int addedLibraryIndex = libraries.Add(aLibraryInfo);

			if (addedLibraryIndex >= 0 && !aLibraryInfo.IsReferenced && !aLibraryInfo.ReadOnly && refreshColumnDefaultConstraintNames)
				aLibraryInfo.RefreshTablesConstraintsNames();

			return addedLibraryIndex;
		}
	
		//---------------------------------------------------------------------------
		public int AddLibraryInfo(WizardLibraryInfo aLibraryInfo)
		{
			return AddLibraryInfo(aLibraryInfo, true);
		}

		//---------------------------------------------------------------------------
		public void RemoveLibrary(string aLibraryName)
		{
			if (libraries == null || libraries.Count == 0 || aLibraryName == null || aLibraryName.Length == 0)
				return;

			WizardLibraryInfo libraryToRemove = GetLibraryInfoByName(aLibraryName);
			if (libraryToRemove == null)
				return;

			libraryToRemove.SetModule(null);
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo MoveLibraryInfo
			(
			WizardLibraryInfo		aLibraryInfoToMove, 
			WizardModuleInfo		aDestinationModule
			)
		{
			if 
				(
				application == null ||
				!application.ModulesInfo.Contains(this) ||
				aLibraryInfoToMove == null || 
				aDestinationModule == null ||
				aDestinationModule == this ||
				aLibraryInfoToMove.Name == null || 
				aLibraryInfoToMove.Name.Length == 0 ||
				!this.LibrariesInfo.Contains(aLibraryInfoToMove) ||
				String.Compare(aDestinationModule.Name, this.Name, true) == 0 ||
				aDestinationModule.GetLibraryInfoByName(aLibraryInfoToMove.Name) != null
				)
				return null;

			// Rimuovo la libreria dal modulo corrente, al quale risulta appartenere
			RemoveLibrary(aLibraryInfoToMove.Name);

			// Aggiungo la libreria al modulo di destinazione
			// Nel modulo di destinazione non può essere già presente una libreria 
			// con lo stesso nome (è escluso nella if precedente) e quindi ne viene
			// necessariamente aggiunta un'atra (cioè addedInfoIdx != -1)
			int addedInfoIdx = aDestinationModule.AddLibraryInfo(aLibraryInfoToMove);

			return aDestinationModule.LibrariesInfo[addedInfoIdx];
		}

		//---------------------------------------------------------------------------
		public string GetNewDefaultLibraryName()
		{
			int libraryNumber = 0;
			string defaultName = null;
			
			do
			{
				defaultName = String.Format(TBWizardProjectsStrings.DefaultLibraryNameFmtText, (++libraryNumber).ToString());
			}
			while(GetLibraryInfoByName(defaultName) != null);

			return defaultName;
		}
		
		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfoByName(string aTableName)
		{
			if (libraries == null || libraries.Count == 0 || aTableName == null || aTableName.Length == 0)
				return null;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				WizardTableInfo tableFound = aLibraryInfo.GetTableInfoByName(aTableName);
				if (tableFound != null)
					return tableFound;
			}

			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardTableInfoCollection GetAllTables()
		{
			if (libraries == null || libraries.Count == 0)
				return null;

			WizardTableInfoCollection tables = new WizardTableInfoCollection();

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.TablesCount > 0)
					tables.AddRange(aLibraryInfo.TablesInfo);
			}

			return (tables.Count > 0) ? tables : null;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTSlavesAttachedToMasterTable(string aMasterTableName)
		{
			if (libraries == null || libraries.Count == 0 || aMasterTableName == null)
				return null;

			aMasterTableName = aMasterTableName.Trim();
			if (aMasterTableName.Length == 0 || !Generics.IsValidTableName(aMasterTableName))
				return null;

			WizardDBTInfoCollection attachedSlaves = new WizardDBTInfoCollection();

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.DBTsCount > 0)
					attachedSlaves.AddRange(aLibraryInfo.GetDBTSlavesAttachedToMasterTable(aMasterTableName));
			}

			return (attachedSlaves.Count > 0) ? attachedSlaves : null;
		}

		//---------------------------------------------------------------------------
		public bool DocumentExists(string aDocumentName)
		{
			if (libraries == null || libraries.Count == 0 || aDocumentName == null || aDocumentName.Length == 0)
				return false;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.DocumentExists(aDocumentName))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByName(string aEnumName)
		{
			if (enums == null || enums.Count == 0 || !Generics.IsValidEnumName(aEnumName))
				return null;

			return enums.GetEnumInfoByName(aEnumName);
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByValue(ushort aEnumValue)
		{
			if (enums == null || enums.Count == 0 || !Generics.IsValidEnumValue(aEnumValue))
				return null;

			return enums.GetEnumInfoByValue(aEnumValue);
		}

		//---------------------------------------------------------------------------
		public ushort GetMaxEnumValue()
		{
			if (enums == null || enums.Count == 0)
				return 0;
			
			ushort maxEnumValue = 0;
			foreach(WizardEnumInfo aEnumInfo in enums)
			{
				if (maxEnumValue < aEnumInfo.Value)
					maxEnumValue = aEnumInfo.Value;
			}
			
			return maxEnumValue;
		}
		
		//---------------------------------------------------------------------
		public ushort GetNextValidEnumValue()
		{
			ushort defaultEnumValue = (ushort)(((application != null) ? application.GetMaxEnumValue() : this.GetMaxEnumValue()) + 1);
		
			if (application == null || !application.ExistEnumInfo(defaultEnumValue))
				return defaultEnumValue;
				
			if (!Generics.IsValidEnumValue(defaultEnumValue))
				return 0;

			return defaultEnumValue;
		}

		//---------------------------------------------------------------------------
		public int AddEnumInfo(WizardEnumInfo aEnumInfo)
		{
			if (aEnumInfo == null || aEnumInfo.Name == null || aEnumInfo.Name.Length == 0)
				return -1;

			WizardEnumInfo alreadyExistingEnum = GetEnumInfoByName(aEnumInfo.Name);
			if (alreadyExistingEnum != null)
				return -1;

			if (enums == null)
				enums = new WizardEnumInfoCollection();

			aEnumInfo.SetModule(this);

			return enums.Add(aEnumInfo);
		}
	
		//---------------------------------------------------------------------------
		public void RemoveEnum(string aEnumName)
		{
			if (enums == null || enums.Count == 0 || aEnumName == null || aEnumName.Length == 0)
				return;

			WizardEnumInfo enumToRemove = GetEnumInfoByName(aEnumName);
			if (enumToRemove == null)
				return;

			enumToRemove.SetModule(null);
		}

		//---------------------------------------------------------------------------
		public uint GetMaximumDbReleaseNumberUsed()
		{
			if (libraries == null || libraries.Count == 0)
				return 1; // Nessuna tabella (=> 1 è il numero di partenza)

			uint maxDbReleaseNumber = 1;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				uint libraryMaxDbReleaseNumber = aLibraryInfo.GetMaximumDbReleaseNumberUsed();
				if (libraryMaxDbReleaseNumber > maxDbReleaseNumber)
					maxDbReleaseNumber = libraryMaxDbReleaseNumber;
			}

			return maxDbReleaseNumber;
		}

		//---------------------------------------------------------------------------
		public uint GetMinimumDbReleaseNumberUsed()
		{
			if (libraries == null || libraries.Count == 0)
				return 0;

			uint minDbReleaseNumber = dbReleaseNumber;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				uint libraryMinDbReleaseNumber = aLibraryInfo.GetMinimumDbReleaseNumberUsed();
				if (libraryMinDbReleaseNumber < minDbReleaseNumber)
					minDbReleaseNumber = libraryMinDbReleaseNumber;
			}

			return minDbReleaseNumber;
		}

		//---------------------------------------------------------------------------
		public bool MustApplyDbReleaseNumberChanges(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 1 || aDbReleaseNumber > dbReleaseNumber || libraries == null || libraries.Count == 0)
				return false;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.MustApplyDbReleaseNumberChanges(aDbReleaseNumber))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool MustApplyDbReleaseNumberChanges()
		{
			if (dbReleaseNumber <= 1)
				return false;

			for(uint i = 1; i <= dbReleaseNumber; i++)
			{
				if (MustApplyDbReleaseNumberChanges(i))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasTablesToUpgrade(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 1 || aDbReleaseNumber > dbReleaseNumber || libraries == null || libraries.Count == 0)
				return false;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.HasTablesToUpgrade(aDbReleaseNumber))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasTablesToUpgradeAfter(uint aStartDbReleaseNumber)
		{
			if (aStartDbReleaseNumber < 1)
				return HasTables;

			if (aStartDbReleaseNumber >= dbReleaseNumber || libraries == null || libraries.Count == 0)
				return false;
			
			for(uint relNumber = aStartDbReleaseNumber; relNumber <= dbReleaseNumber; relNumber++)
			{
				if (HasTablesToUpgrade(relNumber))
					return true;
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool CanRollbackToDbRelease(uint aDbReleaseNumber)
		{
			if (!HasTablesToUpgradeAfter(aDbReleaseNumber))
				return true;

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.TablesCount == 0)
					continue;
			
				foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
				{
					WizardDBTInfoCollection dbtsReferredToTable = (application != null) ? application.GetDBTsReferredToTable(aTableInfo.Name) : null;
					if (aTableInfo.CreationDbReleaseNumber > aDbReleaseNumber)
					{
						// Non posso tornare indietro con la release di database e cancellare la
						// definizione di tabelle inserite dopo, se queste tabelle sono riferite
						// da dei DBT dell'applicazione!!!
						if (dbtsReferredToTable != null && dbtsReferredToTable.Count > 0)
							return false;
					}
					else 
					{
						if (dbtsReferredToTable != null && dbtsReferredToTable.Count > 0)
						{
							WizardTableInfo historicTableInfo = new WizardTableInfo(aTableInfo);
							historicTableInfo.RollbackToDbRelease(aDbReleaseNumber);
							
							foreach(WizardDBTInfo aDBTInfo in dbtsReferredToTable)
							{
								if (aDBTInfo.ColumnsCount == 0 || aDBTInfo.IsMaster)
									continue;

								foreach(WizardDBTColumnInfo aDBTColumnInfo in aDBTInfo.ColumnsInfo)
								{
									if (!aDBTColumnInfo.ForeignKeySegment)
										continue;

									// Non posso tornare indietro a prima che una colonna venisse creata
									// qualora questa colonna serva da aggancio alla tabella del master
									if (historicTableInfo.GetColumnInfoByName(aDBTColumnInfo.ColumnName) == null)
										return false;
								}
							}
						}
					}
				}
			}

			return true;
		}
		
		//---------------------------------------------------------------------------
		public bool RollbackToDbRelease(uint aDbReleaseNumber)
		{
			if (HasTablesToUpgradeAfter(aDbReleaseNumber))
			{
				foreach(WizardLibraryInfo aLibraryInfo in libraries)
				{
					if (aLibraryInfo.TablesCount == 0)
						continue;
				
					for(int i = (aLibraryInfo.TablesCount - 1); i >= 0; i--)
					{
						if (aLibraryInfo.TablesInfo[i].CreationDbReleaseNumber > aDbReleaseNumber)
						{
							// Non posso tornare indietro con la release di database e cancellare la
							// definizione di tabelle inserite dopo, se queste tabelle sono riferite
							// da dei DBT dell'applicazione!!!
							if (application != null && application.ExistsDBTReferredToTable(aLibraryInfo.TablesInfo[i].Name))
								return false;

							aLibraryInfo.RemoveTable(aLibraryInfo.TablesInfo[i].Name);
							continue;
						}

						if (!aLibraryInfo.TablesInfo[i].RollbackToDbRelease(aDbReleaseNumber))
							return false;
					}
				}
			}
			dbReleaseNumber = aDbReleaseNumber;
			
			if (dbReleaseNumber == 1)
				this.RefreshTablesConstraintsNames(true);

			return true;
		}

		//---------------------------------------------------------------------------
		public void RefreshTablesConstraintsNames(bool forceReset)
		{
			if (readOnly || referenced)
				return;

			if (libraries == null || libraries.Count == 0)
				return;
			
			foreach(WizardLibraryInfo aLibraryInfo in libraries)
				aLibraryInfo.RefreshTablesConstraintsNames(forceReset);
		}

		//---------------------------------------------------------------------------
		public void RefreshTablesConstraintsNames()
		{
			RefreshTablesConstraintsNames(false);
		}
		
		//---------------------------------------------------------------------------
		public WizardForeignKeyInfoCollection GetForeignKeysReferencedToTable(string aTableNameSpace)
		{
			if (libraries == null || libraries.Count == 0 || aTableNameSpace == null || aTableNameSpace.Length == 0)
				return null;

			NameSpace tmpNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!tmpNameSpace.IsValid())
				return null;

			WizardForeignKeyInfoCollection foreignKeysRelatedToTable = new WizardForeignKeyInfoCollection();

			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.TablesCount == 0)
					continue;
				
				foreignKeysRelatedToTable.AddRange(aLibraryInfo.GetForeignKeysReferencedToTable(aTableNameSpace));
			}
			return (foreignKeysRelatedToTable.Count > 0) ? foreignKeysRelatedToTable : null;
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotLinkNameAlreadyUsed(string aHotLinkName)
		{
			if (!Generics.IsValidHotLinkName(aHotLinkName))
				throw new TBWizardException(TBWizardProjectsStrings.InvalidObjectNameExceptionErrorMsg);

			if (libraries == null || libraries.Count == 0)
				return false;

			aHotLinkName = aHotLinkName.Trim();
	
			foreach(WizardLibraryInfo aLibraryInfo in libraries)
			{
				if (aLibraryInfo.IsHotLinkNameAlreadyUsed(aHotLinkName, false))
					return true;
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection GetAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if (aDBTInfo == null || libraries == null || libraries.Count == 0)
				return null;
 
			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = new WizardDocumentTabbedPaneInfoCollection();
			
			foreach(WizardLibraryInfo aLibraryInfo in libraries)
				dbtTabbedPanes.AddRange(aLibraryInfo.GetAllDBTTabbedPanes(aDBTInfo));

			return (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0) ? dbtTabbedPanes : null;
		}
		
		#endregion // WizardModuleInfo public methods
	}

	#endregion // WizardModuleInfo class

	#region WizardModuleInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardModuleInfoCollection.
	/// </summary>
	public class WizardModuleInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardModuleInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardModuleInfo))
					throw new NotSupportedException();

				this[index] = (WizardModuleInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardModuleInfo))
				throw new NotSupportedException();

			return this.Contains((WizardModuleInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardModuleInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardModuleInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardModuleInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardModuleInfo))
				throw new NotSupportedException();

			Insert(index, (WizardModuleInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardModuleInfo))
				throw new NotSupportedException();

			Remove((WizardModuleInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardModuleInfo this[int index]
		{
			get {  return (WizardModuleInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardModuleInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardModuleInfo[] ToArray()
		{
			return (WizardModuleInfo[])InnerList.ToArray(typeof(WizardModuleInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardModuleInfo aModuleToAdd)
		{
			if (Contains(aModuleToAdd))
				return IndexOf(aModuleToAdd);

			return InnerList.Add(aModuleToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardModuleInfoCollection aModulesCollectionToAdd)
		{
			if (aModulesCollectionToAdd == null || aModulesCollectionToAdd.Count == 0)
				return;

			foreach (WizardModuleInfo aModuleToAdd in aModulesCollectionToAdd)
				Add(aModuleToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardModuleInfo aModuleToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aModuleToInsert))
				return;

			InnerList.Insert(index, aModuleToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardModuleInfo beforeModule, WizardModuleInfo aModuleToInsert)
		{
			if (beforeModule == null)
				Add(aModuleToInsert);

			if (!Contains(beforeModule))
				return;

			if (Contains(aModuleToInsert))
				return;

			Insert(IndexOf(beforeModule), aModuleToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardModuleInfo aModuleToRemove)
		{
			if (!Contains(aModuleToRemove))
				return;

			InnerList.Remove(aModuleToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardModuleInfo aModuleToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aModuleToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardModuleInfo aModuleToSearch)
		{
			if (!Contains(aModuleToSearch))
				return -1;
			
			return InnerList.IndexOf(aModuleToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardModuleInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardModuleInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardModuleInfo.Equals(this[i], ((WizardModuleInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	
		//---------------------------------------------------------------------------
		public WizardModuleInfo GetModuleInfoByName(string aModuleName)
		{
			if (this.Count == 0 || !Generics.IsValidModuleName(aModuleName))
				return null;

			foreach(WizardModuleInfo aModuleInfo in InnerList)
			{
				if (String.Compare(aModuleName, aModuleInfo.Name) == 0)
					return aModuleInfo;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardModuleInfo GetModuleInfoByDbSignature(string aDbSignature)
		{
			if (this.Count == 0 || aDbSignature == null || aDbSignature.Trim().Length == 0)
				return null;

			foreach(WizardModuleInfo aModuleInfo in InnerList)
			{
				if (String.Compare(aDbSignature.Trim(), aModuleInfo.DbSignature, true) == 0)
					return aModuleInfo;
			}
			
			return null;
		}
	}

	#endregion // WizardModuleInfoCollection class

	#region WizardLibraryInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardLibraryInfo.
	/// </summary>
	public class WizardLibraryInfo : IDisposable
	{
		#region WizardLibraryInfo private data members

		private WizardModuleInfo module = null; // parent

		private string name = String.Empty;
        private string aggregateName = String.Empty; // nome della dll in caso la libreria sia stata aggregata ad altre
        private string sourceFolder = String.Empty; // indica il nome della eventuale sotto-cartella
		// che contiene la libreria. Il nome cartella è 
		// inteso relativo alla cartella del modulo
		private string menuTitle = String.Empty;

		private System.Guid guid = System.Guid.Empty;

		private ushort firstResourceId = Generics.FirstValidResourceId;	// First symbol value that will be used for a 
		// dialog resource, menu resource, and so on. 
		// The valid range for resource symbol values 
		// is 1 to 0x6FFF (=28671)		
		private ushort reservedResourceIdsRange = Generics.DefaultReservedResourceIdsRange;

		private ushort firstControlId = Generics.FirstValidControlId;	// First symbol value that will be used for a 
		// dialog control. The valid range for dialog 
		// control symbol values is 8 to 0xDFFF (=57343).
		private ushort reservedControlIdsRange = Generics.DefaultReservedControlIdsRange;

		private ushort firstCommandId = Generics.FirstValidCommandId;	// First symbol value that will be used for a 
		// command identification. The valid range for 
		// command symbol values is 0x8000 (=32768) to 
		// 0xDFFF(=57343).
		private ushort reservedCommandIdsRange = Generics.DefaultReservedCommandIdsRange;

		private ushort firstSymedId = Generics.FirstValidSymedId;		// First symbol value that will be issued when 
		// you manually assign a symbol value using the 
		// New command in the Symbol Browser
		private ushort reservedSymedIdsRange = Generics.DefaultReservedSymedIdsRange;

		private bool trapDSNChangedEvent = false;
		private bool trapApplicationDateChangedEvent = false;

		private WizardTableInfoCollection tables = null;
		private WizardDocumentInfoCollection documents = null;
		private WizardDBTInfoCollection dbts = null;
		private WizardClientDocumentInfoCollection clientDocuments = null;
		private WizardHotKeyLinkInfoCollection extraHotLinks = null;
		private WizardLibraryInfoCollection dependencies = null;
		private WizardExtraAddedColumnsInfoCollection extraAddedColumns = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardLibraryInfo(string aLibraryName, bool isReadOnly, bool isReferenced)
		{
			Name = aLibraryName;
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo(string aLibraryName, bool isReadOnly) : this(aLibraryName, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo(string aLibraryName) : this(aLibraryName, false)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardLibraryInfo(WizardLibraryInfo aLibraryInfo, bool setTableColumnDefaultConstraintNames)
		{
			module = (aLibraryInfo != null) ? aLibraryInfo.Module : null;
			
			name = (aLibraryInfo != null) ? aLibraryInfo.Name : String.Empty;
            aggregateName = (aLibraryInfo != null) ? aLibraryInfo.aggregateName : String.Empty;
			sourceFolder = (aLibraryInfo != null) ? aLibraryInfo.SourceFolder : String.Empty;
			menuTitle = (aLibraryInfo != null) ? aLibraryInfo.MenuTitle : String.Empty;

			guid = (aLibraryInfo != null) ? aLibraryInfo.Guid : System.Guid.Empty;

			firstResourceId = (aLibraryInfo != null) ? aLibraryInfo.FirstResourceId : Generics.FirstValidResourceId;
			reservedResourceIdsRange = (aLibraryInfo != null) ? aLibraryInfo.ReservedResourceIdsRange : Generics.DefaultReservedResourceIdsRange;

			firstControlId = (aLibraryInfo != null) ? aLibraryInfo.FirstControlId : Generics.FirstValidControlId;
			reservedControlIdsRange = (aLibraryInfo != null) ? aLibraryInfo.ReservedControlIdsRange : Generics.DefaultReservedControlIdsRange;

			firstCommandId = (aLibraryInfo != null) ? aLibraryInfo.FirstCommandId : Generics.FirstValidCommandId;
			reservedCommandIdsRange = (aLibraryInfo != null) ? aLibraryInfo.ReservedCommandIdsRange : Generics.DefaultReservedCommandIdsRange;

			firstSymedId = (aLibraryInfo != null) ? aLibraryInfo.FirstSymedId : Generics.FirstValidSymedId;
			reservedSymedIdsRange = (aLibraryInfo != null) ? aLibraryInfo.ReservedSymedIdsRange : Generics.DefaultReservedSymedIdsRange;
			
			trapDSNChangedEvent = (aLibraryInfo != null) ? aLibraryInfo.TrapDSNChangedEvent : false;
			trapApplicationDateChangedEvent = (aLibraryInfo != null) ? aLibraryInfo.TrapApplicationDateChangedEvent : false;
				
			readOnly = (aLibraryInfo != null) ? aLibraryInfo.ReadOnly : false;
			referenced = (aLibraryInfo != null) ? aLibraryInfo.IsReferenced : false;
			
			if (aLibraryInfo != null && aLibraryInfo.Dependencies != null && aLibraryInfo.Dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in aLibraryInfo.Dependencies)
					this.AddDependency(new WizardLibraryInfo(aDependency, false));
			}
			if (aLibraryInfo != null && aLibraryInfo.TablesCount > 0)
			{
				foreach(WizardTableInfo aTableInfo in aLibraryInfo.TablesInfo)
					this.AddTableInfo(new WizardTableInfo(aTableInfo, setTableColumnDefaultConstraintNames), false, setTableColumnDefaultConstraintNames);
			}
			if (aLibraryInfo != null && aLibraryInfo.DBTsCount > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in aLibraryInfo.DBTsInfo)
					this.AddDBTInfo(new WizardDBTInfo(aDBTInfo));
			}
			if (aLibraryInfo != null && aLibraryInfo.DocumentsCount > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in aLibraryInfo.DocumentsInfo)
					this.AddDocumentInfo(new WizardDocumentInfo(aDocumentInfo));
			}

			if (aLibraryInfo != null && aLibraryInfo.ExtraAddedColumnsCount > 0)
			{
				foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in aLibraryInfo.ExtraAddedColumnsInfo)
					AddExtraAddedColumnsInfo(new WizardExtraAddedColumnsInfo(aExtraAddedColumnsInfo));
			}
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo(WizardLibraryInfo aLibraryInfo) : this(aLibraryInfo, true)
		{
		}
		
		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardLibraryInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardLibraryInfo)obj).Name) == 0 &&
                String.Compare(aggregateName, ((WizardLibraryInfo)obj).aggregateName) == 0 &&
                String.Compare(sourceFolder, ((WizardLibraryInfo)obj).SourceFolder) == 0 &&
				String.Compare(menuTitle, ((WizardLibraryInfo)obj).MenuTitle) == 0 &&
				Guid.Equals(((WizardLibraryInfo)obj).Guid) &&
				firstResourceId == ((WizardLibraryInfo)obj).FirstResourceId &&
				reservedResourceIdsRange == ((WizardLibraryInfo)obj).ReservedResourceIdsRange &&
				firstControlId == ((WizardLibraryInfo)obj).FirstControlId &&
				reservedControlIdsRange == ((WizardLibraryInfo)obj).ReservedControlIdsRange &&
				firstCommandId == ((WizardLibraryInfo)obj).FirstCommandId &&
				reservedCommandIdsRange == ((WizardLibraryInfo)obj).ReservedCommandIdsRange &&
				firstSymedId == ((WizardLibraryInfo)obj).FirstSymedId &&
				reservedSymedIdsRange == ((WizardLibraryInfo)obj).ReservedSymedIdsRange &&
				trapDSNChangedEvent == ((WizardLibraryInfo)obj).TrapDSNChangedEvent &&
				trapApplicationDateChangedEvent == ((WizardLibraryInfo)obj).TrapApplicationDateChangedEvent &&
				WizardTableInfoCollection.Equals(tables, ((WizardLibraryInfo)obj).TablesInfo) &&
				WizardDocumentInfoCollection.Equals(documents, ((WizardLibraryInfo)obj).DocumentsInfo) &&
				WizardDBTInfoCollection.Equals(dbts, ((WizardLibraryInfo)obj).DBTsInfo) &&
				WizardClientDocumentInfoCollection.Equals(clientDocuments, ((WizardLibraryInfo)obj).ClientDocumentsInfo) &&
				WizardHotKeyLinkInfoCollection.Equals(extraHotLinks, ((WizardLibraryInfo)obj).ExtraHotLinksInfo) &&
				WizardLibraryInfoCollection.Equals(dependencies, ((WizardLibraryInfo)obj).Dependencies) &&
				WizardExtraAddedColumnsInfoCollection.Equals(extraAddedColumns, ((WizardLibraryInfo)obj).ExtraAddedColumnsInfo)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetModule(WizardModuleInfo aModuleInfo)
		{
			if (module == aModuleInfo)
				return;

			if (module != null && module.LibrariesInfo.Contains(this))
				module.LibrariesInfo.Remove(this);

			if (aModuleInfo != null && guid.Equals(System.Guid.Empty))
				guid = System.Guid.NewGuid();

			module = aModuleInfo;
			
			InitTablesCreationDbReleaseNumber((module != null) ? (uint)1 : 0);
		}

		//---------------------------------------------------------------------------
		internal void InitTablesCreationDbReleaseNumber(uint aReleaseNumber)
		{
			if (tables == null || tables.Count == 0)
				return;

			foreach(WizardTableInfo aTableInfo in tables)
			{
				if (aReleaseNumber == 0)
					aTableInfo.SetCreationDbReleaseNumber(0);
				else if (aTableInfo.CreationDbReleaseNumber == 0)
					aTableInfo.SetCreationDbReleaseNumber(aReleaseNumber);
			}
		}
		
		#region WizardLibraryInfo private methods

		//---------------------------------------------------------------------------
		private void InitReservedResourceIds(WizardApplicationInfo aApplicationInfo)
		{
			if (aApplicationInfo == null || aApplicationInfo.ModulesCount == 0)
				return;

			ushort	firstId = Generics.FirstValidResourceId;
			ushort	range = Generics.DefaultReservedResourceIdsRange;

			foreach (WizardModuleInfo aModuleInfo in aApplicationInfo.ModulesInfo)
			{
				if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
					continue;
				
				foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo == this)
						continue;

					ushort nextValidFirstId = (ushort)(aLibraryInfo.FirstResourceId + aLibraryInfo.ReservedResourceIdsRange + 1);
					if (firstId < nextValidFirstId)
					{
						firstId = nextValidFirstId;
						range = Math.Min(range, (ushort)(Generics.MaximumResourceId - firstId + 1));
					}
				}
			}
			
			this.FirstResourceId = firstId;
			this.ReservedResourceIdsRange = range;
		}

		//---------------------------------------------------------------------------
		private void InitReservedControlIds(WizardApplicationInfo aApplicationInfo)
		{
			if (aApplicationInfo == null || aApplicationInfo.ModulesCount == 0)
				return;

			ushort	firstId = Generics.FirstValidControlId;
			ushort	range = Generics.DefaultReservedControlIdsRange;

			foreach (WizardModuleInfo aModuleInfo in aApplicationInfo.ModulesInfo)
			{
				if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
					continue;
				
				foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo == this)
						continue;

					ushort nextValidFirstId = (ushort)(aLibraryInfo.FirstControlId + aLibraryInfo.ReservedControlIdsRange + 1);
					if (firstId < nextValidFirstId)
					{
						firstId = nextValidFirstId;
						range = Math.Min(range, (ushort)(Generics.MaximumControlId - firstId + 1));
					}
				}
			}
			
			this.FirstControlId = firstId;
			this.ReservedControlIdsRange = range;
		}
		
		//---------------------------------------------------------------------------
		private void InitReservedCommandIds(WizardApplicationInfo aApplicationInfo)
		{
			if (aApplicationInfo == null || aApplicationInfo.ModulesCount == 0)
				return;

			ushort	firstId = Generics.FirstValidCommandId;
			ushort	range = Generics.DefaultReservedCommandIdsRange;

			foreach (WizardModuleInfo aModuleInfo in aApplicationInfo.ModulesInfo)
			{
				if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
					continue;
				
				foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo == this)
						continue;

					ushort nextValidFirstId = (ushort)(aLibraryInfo.FirstCommandId + aLibraryInfo.ReservedCommandIdsRange + 1);
					if (firstId < nextValidFirstId)
					{
						firstId = nextValidFirstId;
						range = Math.Min(range, (ushort)(Generics.MaximumCommandId - firstId + 1));
					}
				}
			}
			
			this.FirstCommandId = firstId;
			this.ReservedCommandIdsRange = range;
		}
		
		//---------------------------------------------------------------------------
		private void InitReservedSymedIds(WizardApplicationInfo aApplicationInfo)
		{
			if (aApplicationInfo == null || aApplicationInfo.ModulesCount == 0)
				return;

			ushort	firstId = Generics.FirstValidSymedId;
			ushort	range = Generics.DefaultReservedSymedIdsRange;

			foreach (WizardModuleInfo aModuleInfo in aApplicationInfo.ModulesInfo)
			{
				if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
					continue;
				
				foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
				{
					if (aLibraryInfo == this)
						continue;

					ushort nextValidFirstId = (ushort)(aLibraryInfo.FirstSymedId + aLibraryInfo.ReservedSymedIdsRange + 1);
					if (firstId < nextValidFirstId)
					{
						firstId = nextValidFirstId;
						range = Math.Min(range, (ushort)(Generics.MaximumSymedId - firstId + 1));
					}
				}
			}
			
			this.FirstSymedId = firstId;
			this.ReservedSymedIdsRange = range;
		}

		#endregion
		
		#region WizardLibraryInfo public properties

		//---------------------------------------------------------------------------
		public WizardModuleInfo Module { get { return module; } }
		//---------------------------------------------------------------------------
		public WizardApplicationInfo Application { get { return (module != null) ? module.Application : null; } }
		//---------------------------------------------------------------------------
		public string Name 
		{ 
			get { return name; } 
			set 
			{ 
				if (String.Compare(name, value) == 0)
					return;
					
				if (!Generics.IsValidLibraryName(value))
					return;

				if (Application != null) 
				{
					WizardLibraryInfo existingLibrary = Application.GetLibraryInfoByName(value);
					if (existingLibrary != null && existingLibrary != this)
						return;
				}
				
				name = value; 
			} 
		}

        public string AggregateName
        {
            get { return (aggregateName == string.Empty) ?  Name : aggregateName; }
            set { aggregateName = value; }
        }
		
        //---------------------------------------------------------------------------
        public string SourceFolder { get { return sourceFolder; } set { if (Generics.IsValidPathName(value)) sourceFolder = value; } }
		//---------------------------------------------------------------------------
		public string MenuTitle { get { return menuTitle; } set { menuTitle = value; } }
		//---------------------------------------------------------------------------
		public System.Guid Guid { get { return guid; } set { guid = value; } }
		//---------------------------------------------------------------------------
		public ushort FirstResourceId { get { return firstResourceId; } set { if (Generics.IsValidResourceId(value)) firstResourceId = value; } }
		//---------------------------------------------------------------------------
		public ushort ReservedResourceIdsRange { get { return reservedResourceIdsRange; } set { if (Generics.IsValidResourceIdsRange(value, firstResourceId)) reservedResourceIdsRange = value; } }
		//---------------------------------------------------------------------------
		public ushort FirstControlId { get { return firstControlId; } set { if (Generics.IsValidControlId(value)) firstControlId = value; } }
		//---------------------------------------------------------------------------
		public ushort ReservedControlIdsRange { get { return reservedControlIdsRange; } set { if (Generics.IsValidControlIdsRange(value, firstControlId)) reservedControlIdsRange = value; } }
		//---------------------------------------------------------------------------
		public ushort FirstCommandId { get { return firstCommandId; } set { if (Generics.IsValidCommandId(value)) firstCommandId = value; } }
		//---------------------------------------------------------------------------
		public ushort ReservedCommandIdsRange { get { return reservedCommandIdsRange; } set { if (Generics.IsValidCommandIdsRange(value, firstCommandId)) reservedCommandIdsRange = value; } }
		//---------------------------------------------------------------------------
		public ushort FirstSymedId { get { return firstSymedId; } set { if (Generics.IsValidSymedId(value)) firstSymedId = value; } }
		//---------------------------------------------------------------------------
		public ushort ReservedSymedIdsRange { get { return reservedSymedIdsRange; } set { if (Generics.IsValidSymedIdsRange(value, firstSymedId)) reservedSymedIdsRange = value; } }
		//---------------------------------------------------------------------------
		public bool TrapDSNChangedEvent { get { return trapDSNChangedEvent; } set { trapDSNChangedEvent = value; } }
		//---------------------------------------------------------------------------
		public bool TrapApplicationDateChangedEvent { get { return trapApplicationDateChangedEvent; } set { trapApplicationDateChangedEvent = value; } }
		//---------------------------------------------------------------------------
		public WizardTableInfoCollection TablesInfo { get { return tables; } }
		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection DocumentsInfo { get { return documents; } }
		//---------------------------------------------------------------------------
		public WizardClientDocumentInfoCollection ClientDocumentsInfo { get { return clientDocuments; } }
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfoCollection ExtraHotLinksInfo { get { return extraHotLinks; } }
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection DBTsInfo { get { return dbts; } }
		//---------------------------------------------------------------------------
		public int TablesCount { get { return (tables != null) ? tables.Count : 0; } }
		//---------------------------------------------------------------------------
		public int DocumentsCount { get { return (documents != null) ? documents.Count : 0; } }
		//---------------------------------------------------------------------------
		public int ClientDocumentsCount { get { return (clientDocuments != null) ? clientDocuments.Count : 0; } }
		//---------------------------------------------------------------------------
		public int DBTsCount { get { return (dbts != null) ? dbts.Count : 0; } }
		//---------------------------------------------------------------------------
		public WizardLibraryInfoCollection Dependencies { get { return dependencies; } }
		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfoCollection ExtraAddedColumnsInfo { get { return extraAddedColumns; } }
		//---------------------------------------------------------------------------
		public int ExtraAddedColumnsCount { get { return (extraAddedColumns != null) ? extraAddedColumns.Count : 0; } }
		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 

		#endregion

		#region WizardLibraryInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public string GetNameSpace()
		{
			if (module == null || sourceFolder == null || sourceFolder.Trim().Length == 0)
				return String.Empty;

			string moduleNamespace = module.GetNameSpace();
			if (moduleNamespace == null || moduleNamespace.Trim().Length == 0)
				return String.Empty;

			return moduleNamespace + "." + sourceFolder;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfoByName(string aTableName, bool searchDependencies)
		{
			if ((!searchDependencies && (tables == null || tables.Count == 0)) || aTableName == null)
				return null;

			aTableName = aTableName.Trim();
			if (aTableName.Length == 0 || !Generics.IsValidTableName(aTableName))
				return null;

			if (tables != null && tables.Count > 0)
			{
				foreach(WizardTableInfo aTableInfo in tables)
				{
					if (String.Compare(aTableName, aTableInfo.Name, true) == 0)
						return aTableInfo;
				}
			}
			
			if (searchDependencies)
			{
				if (dependencies != null && dependencies.Count > 0)
				{
					foreach(WizardLibraryInfo aDependency in dependencies)
					{
						WizardTableInfo tableInfo = aDependency.GetTableInfoByName(aTableName);
						if (tableInfo != null)
							return tableInfo;
					}
				}
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfoByName(string aTableName)
		{
			return GetTableInfoByName(aTableName, false);
		}

		//---------------------------------------------------------------------------
		public int AddTableInfo(WizardTableInfo aTableInfo, bool autoDbRelease, bool refreshColumnDefaultConstraintNames)
		{
			if (aTableInfo == null || aTableInfo.Name == null || aTableInfo.Name.Length == 0)
				return -1;

			WizardTableInfo alreadyExistingTable = GetTableInfoByName(aTableInfo.Name);
			if (alreadyExistingTable != null)
				return -1;

			if (tables == null)
				tables = new WizardTableInfoCollection();

			aTableInfo.SetLibrary(this, autoDbRelease, !aTableInfo.IsReferenced && !aTableInfo.ReadOnly && refreshColumnDefaultConstraintNames);

			return tables.Add(aTableInfo);
		}

		//---------------------------------------------------------------------------
		public int AddTableInfo(WizardTableInfo aTableInfo, bool autoDbRelease)
		{
			return AddTableInfo(aTableInfo, autoDbRelease, true);
		}

		//---------------------------------------------------------------------------
		public int AddTableInfo(WizardTableInfo aTableInfo)
		{
			return AddTableInfo(aTableInfo, true, true);
		}

		//---------------------------------------------------------------------------
		public void RemoveTable(string aTableName)
		{
			if (tables == null || tables.Count == 0 || aTableName == null || aTableName.Length == 0)
				return;

			WizardTableInfo tableToRemove = GetTableInfoByName(aTableName);
			if (tableToRemove == null)
				return;

			// Se ci sono DBT che fanno riferimento alla tabella che si vuole rimuovere
			// l'operazione di cancellazione deve venire annullata
			if (this.Application != null && this.Application.ExistsDBTReferredToTable(aTableName))
				return;

			tableToRemove.SetLibrary(null);
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo MoveTableInfo
			(
			WizardTableInfo		aTableInfoToMove, 
			WizardLibraryInfo	aDestinationLibrary,
			out bool			dependenciesChanged
			)
		{
			dependenciesChanged = false;

			if 
				(
				module == null ||
				!module.LibrariesInfo.Contains(this) ||
				aTableInfoToMove == null || 
				aDestinationLibrary == null ||
				aDestinationLibrary == this ||
				aTableInfoToMove.Name == null || 
				aTableInfoToMove.Name.Length == 0 ||
				!this.TablesInfo.Contains(aTableInfoToMove)
				)
				return null;

			WizardLibraryInfoCollection librariesToAddDependency = new WizardLibraryInfoCollection();

			// Una tabella della libreria può venire effettivamente spostata in
			// un'altra libreria solo se la libreria corrente non contiene DBT 
			// relativi ad essa o se, pur contenendone, dipende dalla libreria 
			// in cui la tabella viene spostata.
			if (ExistsDBTReferredToTable(aTableInfoToMove.Name))
			{
				if (!CanDependOn(aDestinationLibrary))
					return null;
					
				librariesToAddDependency.Add(this);
			}

			// Analogamente, se in un'altra libreria che dipende dalla libreria 
			// corrente risulta implementato un DBT che fa riferimento alla
			// tabella in questione, tale libreria dovrà adesso dipendere anche 
			// dalla libreria in cui la tabella viene spostata.
			if (this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this || aLibraryInfo == aDestinationLibrary)
							continue;

						if (aLibraryInfo.ExistsDBTReferredToTable(aTableInfoToMove.Name))
						{
							if (!CanDependOn(aDestinationLibrary))
								return null;
							
							librariesToAddDependency.Add(aLibraryInfo);
						}
					}
				}
			}
			if (librariesToAddDependency.Count > 0)
			{
				foreach (WizardLibraryInfo aLibraryInfo in librariesToAddDependency)
				{
					if (aLibraryInfo.AddDependency(aDestinationLibrary) != -1)
						dependenciesChanged = true;
				}
			}

			// Rimuovo la tabella dalla libreria corrente, alla quale risulta appartenere
			RemoveTable(aTableInfoToMove.Name);

			// Aggiungo la tabella alla libreria di destinazione
			int addedInfoIdx = aDestinationLibrary.AddTableInfo(aTableInfoToMove, true, false);

			return aDestinationLibrary.TablesInfo[addedInfoIdx];
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo MoveExtraAddedColumnsInfo
			(
			WizardExtraAddedColumnsInfo		aExtraAddedColumnsInfoToMove, 
			WizardLibraryInfo				aDestinationLibrary,
			out bool						dependenciesChanged
			)
		{
			dependenciesChanged = false;

			if 
				(
				module == null ||
				!module.LibrariesInfo.Contains(this) ||
				aExtraAddedColumnsInfoToMove == null || 
				aDestinationLibrary == null ||
				aDestinationLibrary == this ||
				aExtraAddedColumnsInfoToMove.TableNameSpace == null || 
				aExtraAddedColumnsInfoToMove.TableNameSpace.Length == 0 ||
				!this.ExtraAddedColumnsInfo.Contains(aExtraAddedColumnsInfoToMove)
				)
				return null;

			WizardTableInfo tableToExtend = aExtraAddedColumnsInfoToMove.GetOriginalTableInfo();

			WizardLibraryInfoCollection librariesToAddDependency = new WizardLibraryInfoCollection();

			// La direttiva di aggiunta di colonne ad un'altra tabella può venire 
			// effettivamente spostata in un'altra libreria solo se la libreria corrente 
			// non contiene DBT relativi alla tabella da estendere o se, pur contenendone, 
			// dipende dalla libreria in cui la tabella viene spostata.
			if (tableToExtend != null && ExistsDBTReferredToTable(tableToExtend.Name))
			{
				if (!CanDependOn(aDestinationLibrary))
					return null;
					
				librariesToAddDependency.Add(this);
			}

			// Analogamente, se in un'altra libreria che dipende dalla libreria 
			// corrente risulta implementato un DBT che fa riferimento alla
			// tabella in questione, tale libreria dovrà adesso dipendere anche 
			// dalla libreria in cui viene spostata la direttiva di aggiunta di colonne.
			if (tableToExtend != null && this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this || aLibraryInfo == aDestinationLibrary)
							continue;

						if 
							(
							aLibraryInfo.DependsOn(this) && 
							aLibraryInfo.ExistsDBTReferredToTable(tableToExtend.Name)
							)
						{
							if (!CanDependOn(aDestinationLibrary))
								return null;
							
							librariesToAddDependency.Add(aLibraryInfo);
						}
					}
				}
			}
			if (librariesToAddDependency.Count > 0)
			{
				foreach (WizardLibraryInfo aLibraryInfo in librariesToAddDependency)
				{
					if (aLibraryInfo.AddDependency(aDestinationLibrary) != -1)
						dependenciesChanged = true;
				}
			}

			RemoveExtraAddedColumnsInfo(aExtraAddedColumnsInfoToMove);

			return aDestinationLibrary.AddExtraAddedColumnsInfo(aExtraAddedColumnsInfoToMove, true);
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo GetDocumentInfoByName(string aDocumentName)
		{
			if (documents == null || documents.Count == 0 || aDocumentName == null || aDocumentName.Length == 0)
				return null;

			foreach(WizardDocumentInfo aDocumentInfo in documents)
			{
				if (String.Compare(aDocumentName, aDocumentInfo.Name, true) == 0)
					return aDocumentInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public bool DocumentExists(string aDocumentName)
		{
			return (GetDocumentInfoByName(aDocumentName) != null);
		}
		
		//---------------------------------------------------------------------------
		public int AddDocumentInfo(WizardDocumentInfo aDocumentInfo)
		{
			if (aDocumentInfo == null || aDocumentInfo.Name == null || aDocumentInfo.Name.Length == 0)
				return -1;

			WizardDocumentInfo existingDocumentInfo = GetDocumentInfoByName(aDocumentInfo.Name);
			if (existingDocumentInfo != null)
				return -1;

			if (documents == null)
				documents = new WizardDocumentInfoCollection();

			aDocumentInfo.SetLibrary(this);

			return documents.Add(aDocumentInfo);
		}
	
		//---------------------------------------------------------------------------
		public void RemoveDocument(string aDocumentName)
		{
			if (documents == null || documents.Count == 0 || aDocumentName == null || aDocumentName.Length == 0)
				return;

			if (!IsDocumentRemoveable(aDocumentName))
				return;

			WizardDocumentInfo documentToRemove = GetDocumentInfoByName(aDocumentName);
			if (documentToRemove == null)
				return;

			documentToRemove.SetLibrary(null);
		}
		
		//---------------------------------------------------------------------------
		public bool IsDocumentRemoveable(string aDocumentName)
		{
			if 
				(
				documents == null ||
				documents.Count == 0 ||
				aDocumentName == null || 
				aDocumentName.Length == 0
				)
				return false;

			WizardDocumentInfo documentToRemove = GetDocumentInfoByName(aDocumentName);
			if (documentToRemove == null)
				return false;

			return (this.Application == null || !this.Application.ExistsClientDocumentAttachedToDocument(documentToRemove));
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsDocumentUsingDBT(string aDBTName, bool searchDependencies)
		{
			if (aDBTName == null || aDBTName.Length == 0)
				return false;

			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.GetDBTInfoByName(aDBTName) != null)
						return true;
				}
			}
			
			if (clientDocuments != null && clientDocuments.Count > 0)
			{
				foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
				{
					if (aClientDocumentInfo.GetDBTInfoByName(aDBTName) != null)
						return true;
				}
			}
			
			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.ExistsDocumentUsingDBT(aDBTName, false))
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsDocumentUsingDBT(string aDBTName)
		{
			return ExistsDocumentUsingDBT(aDBTName, false);
		}
	
		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection GetDocumentsUsingDBT(string aDBTName)
		{
			if (aDBTName == null || aDBTName.Length == 0 || documents == null || documents.Count == 0)
				return null;

			WizardDocumentInfoCollection documentsFound = new WizardDocumentInfoCollection();

			foreach(WizardDocumentInfo aDocumentInfo in documents)
			{
				if (aDocumentInfo.GetDBTInfoByName(aDBTName) != null)
					documentsFound.Add(aDocumentInfo);
			}

			return (documentsFound != null && documentsFound.Count > 0) ? documentsFound : null;
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfoCollection GetClientDocumentsUsingDBT(string aDBTName)
		{
			if (aDBTName == null || aDBTName.Length == 0 || clientDocuments == null || clientDocuments.Count == 0)
				return null;

			WizardClientDocumentInfoCollection clientDocumentsFound = new WizardClientDocumentInfoCollection();

			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (aClientDocumentInfo.GetDBTInfoByName(aDBTName) != null)
					clientDocumentsFound.Add(aClientDocumentInfo);
			}

			return (clientDocumentsFound != null && clientDocumentsFound.Count > 0) ? clientDocumentsFound : null;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetAttachedDBTSlaves(WizardDBTInfo aDBTMaster)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 ||
				aDBTMaster == null || 
				!aDBTMaster.IsMaster
				)
				return null;

			WizardDBTInfoCollection attachedSlaves = new WizardDBTInfoCollection();

			foreach (WizardDBTInfo aDBTInfo in dbts)
			{
				if (aDBTInfo.IsRelatedTo(aDBTMaster))
					attachedSlaves.Add(aDBTInfo);
			}
			
			return (attachedSlaves.Count > 0) ? attachedSlaves : null;
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTSlavesAttachedToMasterTable(string aMasterTableName)
		{
			if (dbts == null || dbts.Count == 0 || aMasterTableName == null)
				return null;

			aMasterTableName = aMasterTableName.Trim();
			if (aMasterTableName.Length == 0 || !Generics.IsValidTableName(aMasterTableName))
				return null;

			WizardDBTInfoCollection attachedSlaves = new WizardDBTInfoCollection();

			foreach (WizardDBTInfo aDBTInfo in dbts)
			{
				if ((!aDBTInfo.IsSlave && !aDBTInfo.IsSlaveBuffered) || aDBTInfo.RelatedDBTMaster == null)
					continue;

				if (String.Compare(aDBTInfo.RelatedDBTMaster.TableName, aMasterTableName, true) == 0)
					attachedSlaves.Add(aDBTInfo);
			}
			
			return (attachedSlaves.Count > 0) ? attachedSlaves : null;
		}

		//---------------------------------------------------------------------------
		public bool ExistsAttachedDBTSlaves(WizardDBTInfo aDBTMaster)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 ||
				aDBTMaster == null || 
				!aDBTMaster.IsMaster
				)
				return false;

			foreach (WizardDBTInfo aDBTInfo in dbts)
			{
				if (aDBTInfo.IsRelatedTo(aDBTMaster))
					return true;
			}
			
			return false;
		}

		//---------------------------------------------------------------------------
		public int AddDBTInfo(WizardDBTInfo aDBTInfo, bool updateDependencies, bool forceReplace)
		{
			if 
				(
				aDBTInfo == null || 
				aDBTInfo.Name == null || 
				aDBTInfo.Name.Length == 0 ||
				aDBTInfo.TableName == null || 
				aDBTInfo.TableName.Length == 0 ||
				(dbts != null && dbts.Contains(aDBTInfo))
				)
				return -1;

			WizardDBTInfo existingDBT = GetDBTInfoByName(aDBTInfo.Name);
			if (existingDBT != null)
			{
				if (forceReplace)
				{
					int existingDBTIndex = dbts.IndexOf(existingDBT);
					dbts.RemoveAt(existingDBTIndex);
                    if (existingDBTIndex < dbts.Count)
                        dbts.Insert(existingDBTIndex, aDBTInfo);
                    else
                        dbts.Add(aDBTInfo);
                    return existingDBTIndex;
                }
				return -1;
			}

			if (dbts == null)
				dbts = new WizardDBTInfoCollection();

			aDBTInfo.SetLibrary(this, updateDependencies);

			return dbts.Add(aDBTInfo);
		}
	
		//---------------------------------------------------------------------------
		public int AddDBTInfo(WizardDBTInfo aDBTInfo, bool updateDependencies)
		{
			return AddDBTInfo(aDBTInfo, updateDependencies, false);
		}

		//---------------------------------------------------------------------------
		public int AddDBTInfo(WizardDBTInfo aDBTInfo)
		{
			return AddDBTInfo(aDBTInfo, false);
		}
		
		//---------------------------------------------------------------------------
		public void RemoveDBT(string aDBTName)
		{
			if (dbts == null || dbts.Count == 0 || aDBTName == null || aDBTName.Length == 0)
				return;

			WizardDBTInfo dbtToRemove = GetDBTInfoByName(aDBTName);
			if (dbtToRemove == null)
				return;

			// Se ci sono documenti che utilizzano il DBT che si vuole rimuovere
			// l'operazione di cancellazione deve venire annullata
			if (this.Application != null && this.Application.ExistsDocumentUsingDBT(dbtToRemove))
				return;

			dbtToRemove.SetLibrary(null);
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo GetDBTInfoByName(string aDBTName, bool searchDependencies)
		{
			if (aDBTName == null || aDBTName.Length == 0)
				return null;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (String.Compare(aDBTInfo.Name, aDBTName) == 0)
						return aDBTInfo;
				}
			}

			if (searchDependencies)
			{
				if (dependencies != null && dependencies.Count > 0)
				{
					foreach (WizardLibraryInfo aDependency in dependencies)
					{
						WizardDBTInfo aDBTInfo = aDependency.GetDBTInfoByName(aDBTName, false);
						if (aDBTInfo != null)
							return aDBTInfo;
					}
				}

				if (clientDocuments!= null && clientDocuments.Count > 0)
				{
					foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
					{
						if (aClientDocumentInfo.DBTMaster != null && String.Compare(aDBTName, aClientDocumentInfo.DBTMaster.Name) == 0)
							return aClientDocumentInfo.DBTMaster;
					}
				}
			}
			
			return null;
		}
	
		//---------------------------------------------------------------------------
		public WizardDBTInfo GetDBTInfoByName(string aDBTName)
		{
			return GetDBTInfoByName(aDBTName, false);
		}

		//---------------------------------------------------------------------------
		public bool DBTExists(string aDBTName)
		{
			return (GetDBTInfoByName(aDBTName, true) != null);
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTInfo MoveDBTInfo
			(
			WizardDBTInfo		aDBTInfoToMove, 
			WizardLibraryInfo	aDestinationLibrary,
			out bool			dependenciesChanged
			)
		{
			dependenciesChanged = false;

			if 
				(
				module == null ||
				!module.LibrariesInfo.Contains(this) ||
				aDBTInfoToMove == null || 
				aDestinationLibrary == null ||
				aDestinationLibrary == this ||
				aDBTInfoToMove.Name == null || 
				aDBTInfoToMove.Name.Length == 0 ||
				!this.DBTsInfo.Contains(aDBTInfoToMove)
				)
				return null;

			WizardLibraryInfoCollection librariesToAddDependency = new WizardLibraryInfoCollection();

			// Un DBT della libreria può venire effettivamente spostato in un'altra 
			// libreria solo se la libreria corrente non contiene documenti che lo 
			// utilizzano o se, pur contenendone, dipende dalla libreria 
			// in cui il DBT viene spostato.
			if (ExistsDocumentUsingDBT(aDBTInfoToMove.Name))
			{
				if (!CanDependOn(aDestinationLibrary))
					return null;
					
				librariesToAddDependency.Add(this);
			}

			// Analogamente, se in un'altra libreria che dipende dalla libreria 
			// corrente risulta implementato un documento che utilizza il DBT in
			// questione, tale libreria dovrà adesso dipendere anche dalla libreria 
			// in cui il DBT viene spostato.
			if (this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;

					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this || aLibraryInfo == aDestinationLibrary)
							continue;

						if (aLibraryInfo.ExistsDocumentUsingDBT(aDBTInfoToMove.Name))
						{
							if (!CanDependOn(aDestinationLibrary))
								return null;
							
							librariesToAddDependency.Add(aLibraryInfo);
						}
					}
				}
			}
			if (librariesToAddDependency.Count > 0)
			{
				foreach (WizardLibraryInfo aLibraryInfo in librariesToAddDependency)
				{
					if (aLibraryInfo.AddDependency(aDestinationLibrary) != -1)
						dependenciesChanged = true;
				}
			}

			// Rimuovo il DBT dalla libreria corrente, alla quale risulta appartenere
			RemoveDBT(aDBTInfoToMove.Name);

			// Aggiungo il DBT alla libreria di destinazione
			int addedInfoIdx = aDestinationLibrary.AddDBTInfo(aDBTInfoToMove);

			return aDestinationLibrary.DBTsInfo[addedInfoIdx];
		}

		//---------------------------------------------------------------------------
		public bool ExistsDBTReferredToTable(string aTableName, bool searchDependencies)
		{
			if (aTableName == null || aTableName.Length == 0)
				return false;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (String.Compare(aTableName, aDBTInfo.TableName, true) == 0)
						return true;
				}
			}

			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.ExistsDBTReferredToTable(aTableName, false))
						return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool ExistsDBTReferredToTable(string aDBTName)
		{
			return ExistsDBTReferredToTable(aDBTName, false);
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsAdditionalColumnsReferredToTable(string aTableNameSpace, bool searchDependencies)
		{
			if (aTableNameSpace == null || aTableNameSpace.Length == 0)
				return false;

			NameSpace tmpNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!tmpNameSpace.IsValid())
				return false;

			if (extraAddedColumns != null && extraAddedColumns.Count > 0 && IsTableAvailable(tmpNameSpace.Table))
			{
				foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in extraAddedColumns)
				{
					if (String.Compare(aTableNameSpace, aExtraAddedColumnsInfo.TableNameSpace) == 0)
						return true;
				}
			}

			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.ExistsAdditionalColumnsReferredToTable(aTableNameSpace, false))
						return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool ExistsAdditionalColumnsReferredToTable(string aTableNameSpace)
		{
			return ExistsAdditionalColumnsReferredToTable(aTableNameSpace, false);
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsForeignKeysReferredToTable(string aTableNameSpace)
		{
			if (tables == null || tables.Count == 0 || aTableNameSpace == null || aTableNameSpace.Length == 0)
				return false;

			NameSpace tmpNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!tmpNameSpace.IsValid())
				return false;

			foreach(WizardTableInfo aTableInfo in tables)
			{				
				if (aTableInfo.HasForeignKeysReferencedToTable(aTableNameSpace))
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfoCollection GetForeignKeysReferencedToTable(string aTableNameSpace)
		{
			if (tables == null || tables.Count == 0 || aTableNameSpace == null || aTableNameSpace.Length == 0)
				return null;

			NameSpace tmpNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!tmpNameSpace.IsValid())
				return null;

			WizardForeignKeyInfoCollection foreignKeysRelatedToTable = new WizardForeignKeyInfoCollection();

			foreach(WizardTableInfo aTableInfo in tables)
				foreignKeysRelatedToTable.AddRange(aTableInfo.GetForeignKeysReferencedToTable(aTableNameSpace));

			return (foreignKeysRelatedToTable.Count > 0) ? foreignKeysRelatedToTable : null;
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetDBTsReferredToTable(string aTableName)
		{
			if (dbts == null || dbts.Count == 0 || aTableName == null || aTableName.Length == 0)
				return null;

			WizardDBTInfoCollection dbtsReferredToTable = new WizardDBTInfoCollection();

			foreach (WizardDBTInfo aDBTInfo in dbts)
			{
				if (String.Compare(aTableName, aDBTInfo.TableName, true) == 0)
					dbtsReferredToTable.Add(aDBTInfo);
			}

			return dbtsReferredToTable;
		}

		//---------------------------------------------------------------------------
		public void CheckDBTsReferredToTable(WizardTableInfo aTableInfo)
		{
			if (dbts == null || dbts.Count == 0 || aTableInfo == null || aTableInfo.Name == null || aTableInfo.Name.Length == 0)
				return;

			foreach (WizardDBTInfo aDBTInfo in dbts)
			{
				if (String.Compare(aTableInfo.Name, aDBTInfo.TableName, true) == 0)
					aDBTInfo.CheckTableColumns();
			}
		}

		//---------------------------------------------------------------------------
		public bool IsTableAvailable(string aTableName)
		{
			if (aTableName == null || aTableName.Length == 0)
				return false;
			
			return (GetTableInfoByName(aTableName, true) != null);
		}

		//---------------------------------------------------------------------------
		public bool AreTablesAvailable()
		{
			if (tables != null && tables.Count > 0)
				return true;

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.AreTablesAvailable())
						return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfoCollection GetAllAvailableTables()
		{
			if (!AreTablesAvailable())
				return null;

			WizardTableInfoCollection availableTables = new WizardTableInfoCollection();

			availableTables.AddRange(tables);

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
					availableTables.AddRange(aDependency.TablesInfo);
			}

			return (availableTables.Count > 0) ? availableTables : null;
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsTableUsingEnum(WizardEnumInfo aEnumInfo)
		{
			if (aEnumInfo == null || tables == null || tables.Count == 0)
				return false;

			foreach(WizardTableInfo aTableInfo in tables)
			{
				if (aTableInfo.IsUsingEnum(aEnumInfo))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool ExistsDocumentsUsingExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			if (aExtraAddedColumnsInfo == null || ((documents == null || documents.Count == 0) && (clientDocuments == null || clientDocuments.Count == 0)))
				return false;

			if (documents != null && documents.Count > 0)
			{
				foreach (WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.UsesExtraAddedColumnsInfo(aExtraAddedColumnsInfo))
						return true;
				}
			}
			
			if (clientDocuments != null && clientDocuments.Count > 0)
			{
				foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
				{
					if (aClientDocumentInfo.UsesExtraAddedColumnsInfo(aExtraAddedColumnsInfo))
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool AreDBTsAvailable()
		{
			if (dbts != null && dbts.Count > 0)
				return true;

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.AreDBTsAvailable())
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetAllAvailableDBTs()
		{
			WizardDBTInfoCollection availableDBTs = new WizardDBTInfoCollection();

			availableDBTs.AddRange(dbts);

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
					availableDBTs.AddRange(aDependency.DBTsInfo);
			}

			return (availableDBTs.Count > 0) ? availableDBTs : null;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetAllAvailableDBTMasters()
		{
			WizardDBTInfoCollection availableDBTMasters = new WizardDBTInfoCollection();

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (aDBTInfo.IsMaster)
						availableDBTMasters.Add(aDBTInfo);
				}
			}
			if (dependencies != null && dependencies.Count > 0)
			{
				foreach (WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.DBTsCount > 0)
					{
						foreach (WizardDBTInfo aDBTInfo in aDependency.DBTsInfo)
						{
							if (aDBTInfo.IsMaster)
								availableDBTMasters.Add(aDBTInfo);
						}
					}
				}
			}

			return (availableDBTMasters.Count > 0) ? availableDBTMasters : null;
		}

		//---------------------------------------------------------------------------
		public bool IsDBTAvailable(WizardDBTInfo aDBTInfo)
		{
			if (aDBTInfo == null || aDBTInfo.Name == null || aDBTInfo.Name.Length == 0)
				return false;

			if (aDBTInfo.Library == null || String.Compare(aDBTInfo.Library.GetNameSpace(), GetNameSpace()) == 0)
			{
				if (dbts != null && dbts.Count > 0)
				{
					foreach (WizardDBTInfo dbt in dbts)
					{
						if (String.Compare(dbt.Name, aDBTInfo.Name) == 0)
							return true;
					}
				}
				return false;
			}

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDBTInfo.Library == null || String.Compare(aDBTInfo.Library.GetNameSpace(), aDependency.GetNameSpace()) == 0)
					{
						if (aDependency.DBTsCount > 0)
						{
							foreach (WizardDBTInfo dbt in aDependency.DBTsInfo)
							{
								if (String.Compare(dbt.Name, aDBTInfo.Name) == 0)
									return true;
							}
						}
					}
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection GetAllAvailableDBTSlaves(WizardDBTInfo aMasterDBTInfo)
		{
			if (aMasterDBTInfo != null && !aMasterDBTInfo.IsMaster)
				return null;

			WizardDBTInfoCollection availableDBTSlaves = new WizardDBTInfoCollection();

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if 
						(
						(aDBTInfo.IsSlave || aDBTInfo.IsSlaveBuffered) &&
						(aMasterDBTInfo == null || aMasterDBTInfo.Equals(aDBTInfo.RelatedDBTMaster))
						)
						availableDBTSlaves.Add(aDBTInfo);
				}
			}
			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.DBTsCount > 0)
					{
						foreach (WizardDBTInfo aDBTInfo in aDependency.DBTsInfo)
						{
							if 
								(
								(aDBTInfo.IsSlave || aDBTInfo.IsSlaveBuffered) &&
								(aMasterDBTInfo == null || aDBTInfo.RelatedDBTMaster == aMasterDBTInfo)
								)
								availableDBTSlaves.Add(aDBTInfo);
						}
					}
				}
			}

			return (availableDBTSlaves.Count > 0) ? availableDBTSlaves : null;
		}

		//---------------------------------------------------------------------------
		public bool AreDBTSlavesAvailable(WizardDBTInfo aMasterDBTInfo)
		{
			WizardDBTInfoCollection availableSlaves = GetAllAvailableDBTSlaves(aMasterDBTInfo);

			return (availableSlaves != null && availableSlaves.Count > 0);
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo GetClientDocumentInfoByName(string aClientDocumentName)
		{
			if (clientDocuments == null || clientDocuments.Count == 0 || aClientDocumentName == null || aClientDocumentName.Length == 0)
				return null;

			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (String.Compare(aClientDocumentName, aClientDocumentInfo.Name, true) == 0)
					return aClientDocumentInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public bool ClientDocumentExists(string aClientDocumentInfo)
		{
			return (GetClientDocumentInfoByName(aClientDocumentInfo) != null);
		}
		
		//---------------------------------------------------------------------------
		public int AddClientDocumentInfo(WizardClientDocumentInfo aClientDocumentInfo)
		{
			if (aClientDocumentInfo == null || aClientDocumentInfo.Name == null || aClientDocumentInfo.Name.Length == 0)
				return -1;

			WizardClientDocumentInfo existingClientDocumentInfo = GetClientDocumentInfoByName(aClientDocumentInfo.Name);
			if (existingClientDocumentInfo != null)
				return -1;

			if (clientDocuments == null)
				clientDocuments = new WizardClientDocumentInfoCollection();

			aClientDocumentInfo.SetLibrary(this);

			return clientDocuments.Add(aClientDocumentInfo);
		}
	
		//---------------------------------------------------------------------------
		public void RemoveClientDocument(string aClientDocumentName)
		{
			if (clientDocuments == null || clientDocuments.Count == 0 || aClientDocumentName == null || aClientDocumentName.Length == 0)
				return;

			WizardClientDocumentInfo clientDocumentToRemove = GetClientDocumentInfoByName(aClientDocumentName);
			if (clientDocumentToRemove == null)
				return;

			clientDocumentToRemove.SetLibrary(null);
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsClientDocumentUsingDBT(string aDBTName, bool searchDependencies)
		{
			if (aDBTName == null || aDBTName.Length == 0)
				return false;

			if (clientDocuments != null && clientDocuments.Count > 0)
			{
				foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
				{
					if (aClientDocumentInfo.GetDBTInfoByName(aDBTName) != null)
						return true;
				}
			}
			
			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.ExistsClientDocumentUsingDBT(aDBTName, false))
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsClientDocumentUsingDBT(string aDBTName)
		{
			return ExistsClientDocumentUsingDBT(aDBTName, false);
		}
		
		//---------------------------------------------------------------------------
		public bool ExistsClientDocumentAttachedToDocument(WizardDocumentInfo aServerDocumentToSearch)
		{
			if 
				(
				clientDocuments == null || 
				clientDocuments.Count == 0 ||
				aServerDocumentToSearch == null || 
				aServerDocumentToSearch.Library == null || 
				aServerDocumentToSearch.Library.Application == null
				)
				return false;

			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (aServerDocumentToSearch.Equals(aClientDocumentInfo.ServerDocumentInfo))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool AreDirectClientDocumentsDefined() 
		{
			if (clientDocuments == null || clientDocuments.Count == 0)
				return false;
 
			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (!aClientDocumentInfo.AttachToFamily)
					return true;
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool AreFamilyClientDocumentsDefined() 
		{
			if (clientDocuments == null || clientDocuments.Count == 0)
				return false;
 
			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (aClientDocumentInfo.AttachToFamily)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection GetServerDocuments(bool onlyDirectServers) 
		{
			if (clientDocuments == null || clientDocuments.Count == 0)
				return null;
 
			WizardDocumentInfoCollection serverDocuments = new WizardDocumentInfoCollection();

			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (aClientDocumentInfo.ServerDocumentInfo == null || (onlyDirectServers && aClientDocumentInfo.AttachToFamily))
					continue;

				if (!serverDocuments.ContainsDocumentWithSameNamespace(aClientDocumentInfo.ServerDocumentInfo))
					serverDocuments.Add(aClientDocumentInfo.ServerDocumentInfo);
			}
			return (serverDocuments != null && serverDocuments.Count > 0) ? serverDocuments : null;
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection GetServerDocuments() 
		{
			return GetServerDocuments(false);
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection GetDirectServerDocuments() 
		{
			return GetServerDocuments(true);
		}

		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfoCollection GetAllAvailableHotKeyLinks(bool searchDependencies)
		{
			WizardHotKeyLinkInfoCollection availableHotKeyLinks = new WizardHotKeyLinkInfoCollection();

			if (tables != null && tables.Count > 0)
			{
				foreach (WizardTableInfo aTableInfo in tables)
				{
					if (aTableInfo.IsHKLDefined)
						availableHotKeyLinks.Add(aTableInfo.HotKeyLink);
				}
			}
			if (documents != null && documents.Count > 0)
			{
				foreach (WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.IsHKLDefined)
						availableHotKeyLinks.Add(aDocumentInfo.HotKeyLink);
				}
			}
			if (extraHotLinks != null && extraHotLinks.Count > 0)
				availableHotKeyLinks.AddRange(extraHotLinks);

			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach (WizardLibraryInfo aDependency in dependencies)
				{
					WizardHotKeyLinkInfoCollection dependencyHotKeyLinks = aDependency.GetAllAvailableHotKeyLinks(false);
					if (dependencyHotKeyLinks != null && dependencyHotKeyLinks.Count > 0)
						availableHotKeyLinks.AddRange(dependencyHotKeyLinks);
				}
			}

			return (availableHotKeyLinks.Count > 0) ? availableHotKeyLinks : null;
		}
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfoCollection GetAllAvailableHotKeyLinks()
		{
			return GetAllAvailableHotKeyLinks(true);
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkAvailable(WizardHotKeyLinkInfo aHotKeyLinkInfo, bool searchDependencies)
		{
			if (aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			if (tables != null && tables.Count > 0)
			{
				foreach (WizardTableInfo aTableInfo in tables)
				{
					if (aTableInfo.IsHKLDefined && aHotKeyLinkInfo.Equals(aTableInfo.HotKeyLink))
						return true;
				}
			}
			if (documents != null && documents.Count > 0)
			{
				foreach (WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.IsHKLDefined && aHotKeyLinkInfo.Equals(aDocumentInfo.HotKeyLink))
						return true;
				}
			}

			if (extraHotLinks != null && extraHotLinks.Count > 0)
			{
				foreach (WizardHotKeyLinkInfo aHotLink in extraHotLinks)
				{
					if (aHotKeyLinkInfo.Equals(aHotLink))
						return true;
				}
			}
			
			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach (WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.IsHotKeyLinkAvailable(aHotKeyLinkInfo, false))
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkAvailable(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			return IsHotKeyLinkAvailable(aHotKeyLinkInfo, true);
		}
		
		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetHotKeyLinkLibrary(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return null;

			if (tables != null && tables.Count > 0)
			{
				foreach (WizardTableInfo aTableInfo in tables)
				{
					if (aTableInfo.IsHKLDefined && aHotKeyLinkInfo.Equals(aTableInfo.HotKeyLink))
						return this;
				}
			}
			if (documents != null && documents.Count > 0)
			{
				foreach (WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.IsHKLDefined && aHotKeyLinkInfo.Equals(aDocumentInfo.HotKeyLink))
						return this;
				}
			}

			if (extraHotLinks != null && extraHotLinks.Count > 0)
			{
				foreach (WizardHotKeyLinkInfo aHotLink in extraHotLinks)
				{
					if (aHotKeyLinkInfo.Equals(aHotLink))
						return this;
				}
			}

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach (WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.IsHotKeyLinkAvailable(aHotKeyLinkInfo, false))
						return aDependency;
				}
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public object GetHotKeyLinkParent(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return null;

			if (tables != null && tables.Count > 0)
			{
				foreach (WizardTableInfo aTableInfo in tables)
				{
					if (aTableInfo.IsHKLDefined && aHotKeyLinkInfo.Equals(aTableInfo.HotKeyLink))
						return aTableInfo;
				}
			}
			if (documents != null && documents.Count > 0)
			{
				foreach (WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.IsHKLDefined && aHotKeyLinkInfo.Equals(aDocumentInfo.HotKeyLink))
						return aDocumentInfo;
				}
			}

			if (extraHotLinks != null && extraHotLinks.Count > 0)
			{
				foreach (WizardHotKeyLinkInfo aHotLink in extraHotLinks)
				{
					if (aHotKeyLinkInfo.Equals(aHotLink))
						return null;
				}
			}
			
			if (dependencies != null && dependencies.Count > 0)
			{
				foreach (WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.IsHotKeyLinkAvailable(aHotKeyLinkInfo, false))
						return aDependency.GetHotKeyLinkParent(aHotKeyLinkInfo);
				}
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public string GetHotKeyLinkNamespace(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{ 
			if (aHotKeyLinkInfo== null || !aHotKeyLinkInfo.IsDefined)
				return String.Empty;

			if (aHotKeyLinkInfo.IsReferenced)
				return aHotKeyLinkInfo.ReferencedNameSpace;

			string libraryNamespace = GetNameSpace();
			if (libraryNamespace == null || libraryNamespace.Length == 0)
				return String.Empty;

			WizardHotKeyLinkInfoCollection ownedHotKeyLinks = GetAllAvailableHotKeyLinks(false);
			if (!ownedHotKeyLinks.Contains(aHotKeyLinkInfo))
				return String.Empty;

			return libraryNamespace + "." + aHotKeyLinkInfo.Name; 
		}
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo GetHotKeyLinkFromClassName(string aClassName)
		{
			if (aClassName == null || aClassName.Length == 0 || !Generics.IsValidClassName(aClassName))
				return null;

			WizardHotKeyLinkInfoCollection availableHotKeyLinks = GetAllAvailableHotKeyLinks();

			if (availableHotKeyLinks == null || availableHotKeyLinks.Count == 0)
				return null;

			foreach(WizardHotKeyLinkInfo aHotKeyLinkInfo in availableHotKeyLinks)
			{
				if (String.Compare(aHotKeyLinkInfo.ClassName, aClassName) == 0)
					return aHotKeyLinkInfo;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo GetExtraHotLinkByName(string aHotKeyLinkName)
		{
			if (extraHotLinks == null || extraHotLinks.Count == 0 || aHotKeyLinkName == null || aHotKeyLinkName.Length == 0)
				return null;
		
			foreach (WizardHotKeyLinkInfo aHotLink in extraHotLinks)
			{
				if (String.Compare(aHotKeyLinkName, aHotLink.Name) == 0)
					return aHotLink;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public int AddExtraHotLinkInfo(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (aHotKeyLinkInfo == null || aHotKeyLinkInfo.Name == null || aHotKeyLinkInfo.Name.Length == 0)
				return -1;

			if (aHotKeyLinkInfo.Table != null && !this.IsTableAvailable(aHotKeyLinkInfo.Table.Name))
				return -1;

			WizardHotKeyLinkInfo alreadyExistingExtraHotLink = GetExtraHotLinkByName(aHotKeyLinkInfo.Name);
			if (alreadyExistingExtraHotLink != null)
				return -1;

			if (extraHotLinks == null)
				extraHotLinks = new WizardHotKeyLinkInfoCollection();

			return extraHotLinks.Add(aHotKeyLinkInfo);
		}

		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (aDBTInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}

			if (documents != null && documents.Count > 0)
			{
				foreach (WizardDocumentInfo aDocumentInfo in documents)
				{
					if (aDocumentInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}

			if (clientDocuments != null && clientDocuments.Count > 0)
			{
				foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
				{
					if (aClientDocumentInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetDirectDependency(string aModuleName, string aLibraryName)
		{
			if 
				(
				dependencies == null ||
				dependencies.Count == 0 ||
				aModuleName == null || 
				aModuleName.Length == 0 ||
				aLibraryName == null || 
				aLibraryName.Length == 0
				)
				return null;

			foreach(WizardLibraryInfo aDependency in dependencies)
			{
				if (aDependency == null || aDependency.Module == null)
					continue;

				if (
					String.Compare(aDependency.Name, aLibraryName, true) == 0 &&
					String.Compare(aDependency.Module.Name, aModuleName, true) == 0 
					)
					return aDependency;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetDirectDependency(WizardLibraryInfo aDependencyToSearch)
		{
			if 
				(
				dependencies == null ||
				dependencies.Count == 0 ||
				aDependencyToSearch == null || 
				aDependencyToSearch.Module == null
				)
				return null;

			if (dependencies.Contains(aDependencyToSearch))
				return aDependencyToSearch;

			return GetDirectDependency(aDependencyToSearch.Module.Name, aDependencyToSearch.Name);
		}

		//---------------------------------------------------------------------------
		public bool CanDependOn(WizardLibraryInfo aLibraryInfo)
		{
			return
				(
				aLibraryInfo != null &&
				aLibraryInfo != this &&			// si tratta della stessa libreria
				!aLibraryInfo.DependsOn(this)	// non si possono avere dipendenze incrociate !!!
				);
		}

		//---------------------------------------------------------------------------
		public int AddDependency(WizardLibraryInfo aDependency)
		{
			if 
				(
				aDependency == null || 
				aDependency.Module == null || 
				aDependency == this ||
				!CanDependOn(aDependency)
				)
				return -1;

			WizardLibraryInfo alreadyExistingDependency = GetDirectDependency(aDependency);
			if (alreadyExistingDependency != null)
				return -1;

			if (dependencies == null)
				dependencies = new WizardLibraryInfoCollection();

			return dependencies.Add(aDependency);
		}
	
		//---------------------------------------------------------------------------
		public bool IsDependencyRemoveable(string aModuleName, string aLibraryName)
		{
			if 
				(
				dependencies == null ||
				dependencies.Count == 0 ||
				aModuleName == null || 
				aModuleName.Length == 0 ||
				aLibraryName == null || 
				aLibraryName.Length == 0
				)
				return false;

			return IsDependencyRemoveable(GetDirectDependency(aModuleName, aLibraryName));
		}
		
		//---------------------------------------------------------------------------
		public bool IsDependencyRemoveable(WizardLibraryInfo aDependency)
		{
			if 
				(
				dependencies == null ||
				dependencies.Count == 0 || 
				aDependency == null ||
				!dependencies.Contains(aDependency)
				)
				return true;

			if (aDependency.TablesCount > 0)
			{
				foreach (WizardTableInfo aTableInfo in aDependency.TablesInfo)
				{
					if (ExistsDBTReferredToTable(aTableInfo.Name, false))
						return false;

					if (ExistsAdditionalColumnsReferredToTable(aTableInfo.GetNameSpace(), false))
						return false;
				}
			}
			if (aDependency.DBTsCount > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in aDependency.DBTsInfo)
				{
					if (ExistsDocumentUsingDBT(aDBTInfo.Name, false))
						return false;
				}
			}
			return true;
		}
		
		//---------------------------------------------------------------------------
		public void RemoveDependency(WizardLibraryInfo aDependency)
		{
			if (dependencies == null || dependencies.Count == 0 || aDependency == null)
				return;

			WizardLibraryInfo dependencyToRemove = GetDirectDependency(aDependency);
			if (dependencyToRemove == null)
				return;

			if (!IsDependencyRemoveable(aDependency))
				return;

			dependencies.Remove(dependencyToRemove);
		}

		//---------------------------------------------------------------------------
		public bool DependsOn(WizardLibraryInfo aLibraryInfo)
		{
			if 
				(
				dependencies == null ||
				dependencies.Count == 0 ||
				aLibraryInfo == null || 
				aLibraryInfo.Module == null ||
				aLibraryInfo == this
				)
				return false;

			// cerco la libreria aLibraryInfo nelle dipendenze dirette !!!
			WizardLibraryInfo alreadyExistingDirectDependency = GetDirectDependency(aLibraryInfo);
			if (alreadyExistingDirectDependency != null)
				return true;

			// per ciascuna dipendenza controllo se dipende essa stessa da aLibraryInfo (dipendenza indiretta)
			foreach(WizardLibraryInfo aDependency in dependencies)
			{
				if (aDependency.DependsOn(aLibraryInfo))
					return true;
			}
	
			return false;
		}

		//---------------------------------------------------------------------------
		public void FillLibrariesBuildOrderedList(ref WizardLibraryInfoCollection buildOrderedList)
		{
			if (buildOrderedList == null)
				buildOrderedList = new WizardLibraryInfoCollection();
			else if (buildOrderedList.Contains(this))
				return;
			
			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (buildOrderedList.Contains(aDependency))
						continue;

					if (aDependency.Dependencies == null || aDependency.Dependencies.Count == 0)
					{
						// Se la libreria non ha dipendenze la metto in cimma alla lista
						buildOrderedList.Insert(0, aDependency);
						continue;
					}
				
					aDependency.FillLibrariesBuildOrderedList(ref buildOrderedList);
				}
			}
			
			buildOrderedList.Add(this);
		}
		
		//---------------------------------------------------------------------------
		public bool IsClassNameAlreadyUsed(string aClassName, bool searchDependencies)
		{
			if (!Generics.IsValidClassName(aClassName))
				throw new TBWizardException(TBWizardProjectsStrings.InvalidClassNameExceptionErrorMsg);

			aClassName = aClassName.Trim();

			if (tables != null && tables.Count > 0)
			{
				foreach(WizardTableInfo aTableInfo in tables)
				{
					if (String.Compare(aTableInfo.ClassName, aClassName) == 0)
						return true;

					if (aTableInfo.IsHKLDefined && String.Compare(aTableInfo.HKLClassName, aClassName) == 0)
						return true;
				}
			}

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (String.Compare(aDBTInfo.ClassName, aClassName) == 0)
						return true;
				}
			}
			
			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					if (String.Compare(aDocumentInfo.ClassName, aClassName) == 0)
						return true;
					
					if (aDocumentInfo.IsHKLDefined && String.Compare(aDocumentInfo.HKLClassName, aClassName) == 0)
						return true;
				}
			}

			if (extraAddedColumns != null && extraAddedColumns.Count > 0)
			{
				foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in extraAddedColumns)
				{
					if (String.Compare(aExtraAddedColumnsInfo.ClassName, aClassName) == 0)
						return true;
				}
			}

			//@@TODO Oggetti di interfaccia

			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.IsClassNameAlreadyUsed(aClassName, true))
						return true;
				}
			}

			return false;
		}
	
		//---------------------------------------------------------------------------
		public bool IsClassNameAlreadyUsed(string aClassName)
		{
			return IsClassNameAlreadyUsed(aClassName, false);
		}

		//---------------------------------------------------------------------------
		public bool AreClassNamesConflicting(WizardLibraryInfo anotherLibrary, ref string conflictDescription)
		{
			conflictDescription = String.Empty;

			if (anotherLibrary == null || this.Equals(anotherLibrary))
				return false;

			if (tables != null && tables.Count > 0)
			{
				foreach(WizardTableInfo aTableInfo in tables)
				{
					if 
						(
						aTableInfo.ClassName != null && 
						aTableInfo.ClassName.Length > 0 &&
						anotherLibrary.IsClassNameAlreadyUsed(aTableInfo.ClassName, false)
						)
					{
						conflictDescription = String.Format(TBWizardProjectsStrings.ClassNamesConflictFmtMessage, aTableInfo.ClassName, name, anotherLibrary.Name);
						return true;
					}
					if 
						(
						aTableInfo.HKLClassName != null && 
						aTableInfo.HKLClassName.Length > 0 &&
						anotherLibrary.IsClassNameAlreadyUsed(aTableInfo.HKLClassName, false))
					{
						conflictDescription = String.Format(TBWizardProjectsStrings.ClassNamesConflictFmtMessage, aTableInfo.HKLClassName, name, anotherLibrary.Name);
						return true;
					}
				}
			}

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if 
						(
						aDBTInfo.ClassName != null && 
						aDBTInfo.ClassName.Length > 0 &&
						anotherLibrary.IsClassNameAlreadyUsed(aDBTInfo.ClassName, false)
						)
					{
						conflictDescription = String.Format(TBWizardProjectsStrings.ClassNamesConflictFmtMessage, aDBTInfo.ClassName, name, anotherLibrary.Name);
						return true;
					}
				}
			}
			
			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					if 
						(
						aDocumentInfo.ClassName != null && 
						aDocumentInfo.ClassName.Length > 0 &&
						anotherLibrary.IsClassNameAlreadyUsed(aDocumentInfo.ClassName, false)
						)
					{
						conflictDescription = String.Format(TBWizardProjectsStrings.ClassNamesConflictFmtMessage, aDocumentInfo.ClassName, name, anotherLibrary.Name);
						return true;
					}
				}
			}

			if (dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (aDependency.AreClassNamesConflicting(anotherLibrary, ref conflictDescription))
						return true;
				}
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool AreClassNamesConflicting(WizardLibraryInfo anotherLibrary)
		{
			string conflictDescription = String.Empty;
			return AreClassNamesConflicting(anotherLibrary, ref conflictDescription);
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotLinkNameAlreadyUsed(string aHotLinkName, bool checkValidName)
		{
			if (checkValidName)
			{
				if (!Generics.IsValidHotLinkName(aHotLinkName))
					throw new TBWizardException(TBWizardProjectsStrings.InvalidObjectNameExceptionErrorMsg);

				aHotLinkName = aHotLinkName.Trim();
			}

			if (tables != null && tables.Count > 0)
			{
				foreach(WizardTableInfo aTableInfo in tables)
				{
					if 
						(
						aTableInfo.IsHKLDefined &&
						String.Compare(aTableInfo.HKLName, aHotLinkName) == 0
						)
						return true;
				}
			}

			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					if 
						(
						aDocumentInfo.IsHKLDefined &&
						String.Compare(aDocumentInfo.HKLName, aHotLinkName) == 0
						)
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotLinkNameAlreadyUsed(string aHotLinkName)
		{
			return IsHotLinkNameAlreadyUsed(aHotLinkName, true);
		}
		
		//---------------------------------------------------------------------------
		public void InitReservedIds(WizardApplicationInfo aApplicationInfo)
		{
			InitReservedResourceIds(aApplicationInfo);
			InitReservedControlIds(aApplicationInfo);
			InitReservedCommandIds(aApplicationInfo);
			InitReservedSymedIds(aApplicationInfo);
		}
		
		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableResourceId()
		{
			ushort firstAvailableResourceId = firstResourceId;

			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					ushort firstDocumentAvailableId = aDocumentInfo.GetFirstAvailableResourceId();
					if (firstDocumentAvailableId > firstAvailableResourceId)
					{
						firstAvailableResourceId = firstDocumentAvailableId;
					}
				}
			}
			
			return firstAvailableResourceId; 
		}
		
		//---------------------------------------------------------------------------
		public bool IsReservedResourceIdsRangeValid(ushort firstId, ushort range, out bool overlappingOtherLibraryValues)
		{
			overlappingOtherLibraryValues = false;

			if 
				(
				!Generics.IsValidResourceId(firstId) ||
				range == 0 ||
				(firstId + range - 1) > Generics.MaximumResourceId
				)
				return false;

			if (this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;
					
					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this)
							continue;

						if 
							(
							(
							firstId <= aLibraryInfo.FirstResourceId &&
							firstId + range >= aLibraryInfo.FirstResourceId
							)||
							(
							firstId >= aLibraryInfo.FirstResourceId &&
							firstId <= aLibraryInfo.FirstResourceId + aLibraryInfo.ReservedResourceIdsRange
							)
							)
						{
							overlappingOtherLibraryValues = true;
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetDocumentFirstResourceId(WizardDocumentInfo aDocumentInfo)
		{
			if (aDocumentInfo == null)
				throw new ArgumentNullException();
			if (documents == null || documents.Count == 0 || !documents.Contains(aDocumentInfo))
				throw new NotSupportedException();

			ushort firstDocumentResourceId = firstResourceId;

			for (int i=0; i < documents.IndexOf(aDocumentInfo); i++)
				firstDocumentResourceId += documents[i].GetUsedResourceIdsCount();

			return firstDocumentResourceId;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableControlId()
		{
			ushort firstAvailableControlId = firstControlId;

			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					ushort firstDocumentAvailableId = aDocumentInfo.GetFirstAvailableControlId();
					if (firstDocumentAvailableId > firstAvailableControlId)
					{
						firstAvailableControlId = firstDocumentAvailableId;
					}
				}
			}
			
			return firstAvailableControlId; 
		}
	
		//---------------------------------------------------------------------------
		public bool IsReservedControlIdsRangeValid(ushort firstId, ushort range, out bool overlappingOtherLibraryValues)
		{
			overlappingOtherLibraryValues = false;

			if 
				(
				!Generics.IsValidControlId(firstId) ||
				range == 0 ||
				(firstId + range - 1) > Generics.MaximumControlId
				)
				return false;

			if (this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;
					
					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this)
							continue;

						if 
							(
							(
							firstId <= aLibraryInfo.FirstControlId &&
							firstId + range >= aLibraryInfo.FirstControlId
							)||
							(
							firstId >= aLibraryInfo.FirstControlId &&
							firstId <= aLibraryInfo.FirstControlId + aLibraryInfo.ReservedControlIdsRange
							)
							)
						{
							overlappingOtherLibraryValues = true;
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetDocumentFirstControlId(WizardDocumentInfo aDocumentInfo)
		{
			if (aDocumentInfo == null)
				throw new ArgumentNullException();
			if (documents == null || documents.Count == 0 || !documents.Contains(aDocumentInfo))
				throw new NotSupportedException();

			ushort firstDocumentControlId = firstControlId;

			for (int i=0; i < documents.IndexOf(aDocumentInfo); i++)
				firstDocumentControlId += documents[i].GetUsedControlIdsCount();

			return firstDocumentControlId;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableCommandId()
		{
			ushort firstAvailableCommandId = firstCommandId;

			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					ushort firstDocumentAvailableId = aDocumentInfo.GetFirstAvailableCommandId();
					if (firstDocumentAvailableId > firstAvailableCommandId)
					{
						firstAvailableCommandId = firstDocumentAvailableId;
					}
				}
			}

			return firstAvailableCommandId; 
		}

		//---------------------------------------------------------------------------
		public bool IsReservedCommandIdsRangeValid(ushort firstId, ushort range, out bool overlappingOtherLibraryValues)
		{
			overlappingOtherLibraryValues = false;
			if 
				(
				!Generics.IsValidCommandId(firstId) ||
				range == 0 ||
				(firstId + range - 1) > Generics.MaximumCommandId
				)
				return false;

			if (this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;
					
					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this)
							continue;

						if 
							(
							(
							firstId <= aLibraryInfo.FirstCommandId &&
							firstId + range >= aLibraryInfo.FirstCommandId
							)||
							(
							firstId >= aLibraryInfo.FirstCommandId &&
							firstId <= aLibraryInfo.FirstCommandId + aLibraryInfo.ReservedCommandIdsRange
							)
							)
						{
							overlappingOtherLibraryValues = true;
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetDocumentFirstCommandId(WizardDocumentInfo aDocumentInfo)
		{
			if (aDocumentInfo == null)
				throw new ArgumentNullException();
			if (documents == null || documents.Count == 0 || !documents.Contains(aDocumentInfo))
				throw new NotSupportedException();

			ushort firstDocumentCommandId = firstCommandId;

			for (int i=0; i < documents.IndexOf(aDocumentInfo); i++)
				firstDocumentCommandId += documents[i].GetUsedCommandIdsCount();

			return firstDocumentCommandId;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableSymedId()
		{
			ushort firstAvailableSymedId = firstSymedId;

			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
				{
					ushort firstDocumentAvailableId = aDocumentInfo.GetFirstAvailableSymedId();
					if (firstDocumentAvailableId > firstAvailableSymedId)
					{
						firstAvailableSymedId = firstDocumentAvailableId;
					}
				}
			}

			return firstAvailableSymedId; 
		}

		//---------------------------------------------------------------------------
		public bool IsReservedSymedIdsRangeValid(ushort firstId, ushort range, out bool overlappingOtherLibraryValues)
		{
			overlappingOtherLibraryValues = false;
			if 
				(
				!Generics.IsValidSymedId(firstId) ||
				range == 0 ||
				(firstId + range - 1) > Generics.MaximumSymedId
				)
				return false;

			if (this.Application != null && this.Application.ModulesCount > 0)
			{
				foreach (WizardModuleInfo aModuleInfo in this.Application.ModulesInfo)
				{
					if (aModuleInfo == null || aModuleInfo.LibrariesCount == 0)
						continue;
					
					foreach (WizardLibraryInfo aLibraryInfo in aModuleInfo.LibrariesInfo)
					{
						if (aLibraryInfo == this)
							continue;

						if 
							(
							(
							firstId <= aLibraryInfo.FirstSymedId &&
							firstId + range >= aLibraryInfo.FirstSymedId
							)||
							(
							firstId >= aLibraryInfo.FirstSymedId &&
							firstId <= aLibraryInfo.FirstSymedId + aLibraryInfo.ReservedSymedIdsRange
							)
							)
						{
							overlappingOtherLibraryValues = true;
							return false;
						}
					}
				}
			}
			
			return true;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetDocumentFirstSymedId(WizardDocumentInfo aDocumentInfo)
		{
			if (aDocumentInfo == null)
				throw new ArgumentNullException();
			if (documents == null || documents.Count == 0 || !documents.Contains(aDocumentInfo))
				throw new NotSupportedException();

			ushort firstDocumentSymedId = firstSymedId;

			for (int i=0; i < documents.IndexOf(aDocumentInfo); i++)
				firstDocumentSymedId += documents[i].GetUsedSymedIdsCount();

			return firstDocumentSymedId;
		}

		//---------------------------------------------------------------------------
		public ushort GetClientDocumentFirstResourceId(WizardClientDocumentInfo aClientDocumentInfo)
		{
			if (aClientDocumentInfo == null)
				throw new ArgumentNullException();

			if (clientDocuments == null || clientDocuments.Count == 0 || !clientDocuments.Contains(aClientDocumentInfo))
				throw new NotSupportedException();

			ushort firstClientDocumentResourceId = GetFirstAvailableResourceId();

			for (int i=0; i < clientDocuments.IndexOf(aClientDocumentInfo); i++)
				firstClientDocumentResourceId += clientDocuments[i].GetUsedResourceIdsCount();

			return firstClientDocumentResourceId;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetClientDocumentFirstControlId(WizardClientDocumentInfo aClientDocumentInfo)
		{
			if (aClientDocumentInfo == null)
				throw new ArgumentNullException();
			if (clientDocuments == null || clientDocuments.Count == 0 || !clientDocuments.Contains(aClientDocumentInfo))
				throw new NotSupportedException();

			ushort firstClientDocumentControlId = GetFirstAvailableControlId();

			for (int i=0; i < clientDocuments.IndexOf(aClientDocumentInfo); i++)
				firstClientDocumentControlId += clientDocuments[i].GetUsedControlIdsCount();

			return firstClientDocumentControlId;
		}

		//---------------------------------------------------------------------------
		public ushort GetClientDocumentFirstCommandId(WizardClientDocumentInfo aClientDocumentInfo)
		{
			if (aClientDocumentInfo == null)
				throw new ArgumentNullException();
			if (clientDocuments == null || clientDocuments.Count == 0 || !clientDocuments.Contains(aClientDocumentInfo))
				throw new NotSupportedException();

			ushort firstClientDocumentCommandId = GetFirstAvailableCommandId();

			for (int i=0; i < clientDocuments.IndexOf(aClientDocumentInfo); i++)
				firstClientDocumentCommandId += clientDocuments[i].GetUsedCommandIdsCount();

			return firstClientDocumentCommandId;
		}

		//---------------------------------------------------------------------------
		public ushort GetClientDocumentFirstSymedId(WizardClientDocumentInfo aClientDocumentInfo)
		{
			if (aClientDocumentInfo == null)
				throw new ArgumentNullException();
			if (clientDocuments == null || clientDocuments.Count == 0 || !clientDocuments.Contains(aClientDocumentInfo))
				throw new NotSupportedException();

			ushort firstClientDocumentSymedId = GetFirstAvailableSymedId();

			for (int i=0; i < clientDocuments.IndexOf(aClientDocumentInfo); i++)
				firstClientDocumentSymedId += clientDocuments[i].GetUsedSymedIdsCount();

			return firstClientDocumentSymedId;
		}

		//---------------------------------------------------------------------------
		public bool HasClientDocumentsReferredToApplication(string aReferencedApplicationName)
		{
			if (clientDocuments == null || clientDocuments.Count == 0 || aReferencedApplicationName == null || aReferencedApplicationName.Length == 0)
				return false;

			foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
			{
				if (
					aClientDocumentInfo.ServerDocumentInfo == null || 
					aClientDocumentInfo.ServerDocumentInfo.Library == null ||
					aClientDocumentInfo.ServerDocumentInfo.Library.Application == null
					)
					continue;

				if (String.Compare(aClientDocumentInfo.ServerDocumentInfo.Library.Application.Name, aReferencedApplicationName) == 0)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public uint GetMaximumDbReleaseNumberUsed()
		{
			if (tables == null || tables.Count == 0)
				return 1; // Nessuna tabella (=> 1 è il numero di partenza)

			uint maxDbReleaseNumber = 1;

			foreach(WizardTableInfo aTableInfo in tables)
			{
				uint tableMaxDbReleaseNumber = aTableInfo.GetMaximumDbReleaseNumberUsed();
				if (tableMaxDbReleaseNumber > maxDbReleaseNumber)
					maxDbReleaseNumber = tableMaxDbReleaseNumber;
			}

			return maxDbReleaseNumber;
		}

		//---------------------------------------------------------------------------
		public uint GetMinimumDbReleaseNumberUsed()
		{
			if (tables == null || tables.Count == 0)
				return 0; // Nessuna tabella

			uint minDbReleaseNumber = (module != null) ? module.DbReleaseNumber : UInt32.MaxValue;

			foreach(WizardTableInfo aTableInfo in tables)
			{
				if (aTableInfo.CreationDbReleaseNumber < minDbReleaseNumber)
					minDbReleaseNumber = aTableInfo.CreationDbReleaseNumber;
			}

			return minDbReleaseNumber;
		}

		//---------------------------------------------------------------------------
		public bool MustApplyDbReleaseNumberChanges(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 1)
				return false;

			if (tables != null && tables.Count > 0)
			{
				foreach(WizardTableInfo aTableInfo in tables)
				{
					if (
						aTableInfo.CreationDbReleaseNumber == aDbReleaseNumber ||
						aTableInfo.IsToUpgrade(aDbReleaseNumber)
						)
						return true;
				}
			}

			if (extraAddedColumns != null && extraAddedColumns.Count > 0)
			{
				foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in extraAddedColumns)
				{
					if (
						aExtraAddedColumnsInfo.CreationDbReleaseNumber == aDbReleaseNumber ||
						aExtraAddedColumnsInfo.IsToUpgrade(aDbReleaseNumber)
						)
						return true;
				}
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasTablesToUpgrade(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 1)
				return false;

			if (tables != null && tables.Count > 0)
			{
				foreach(WizardTableInfo aTableInfo in tables)
				{
					if (aTableInfo.IsToUpgrade(aDbReleaseNumber))
						return true;
				}
			}

			if (extraAddedColumns != null && extraAddedColumns.Count > 0)
			{
				foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in extraAddedColumns)
				{
					if (aExtraAddedColumnsInfo.IsToUpgrade(aDbReleaseNumber))
						return true;
				}
			}
			
			return false;
		}

		//---------------------------------------------------------------------------
		public void RefreshTablesConstraintsNames(bool forceReset)
		{
			if (readOnly || referenced)
				return;
			
			if (tables == null || tables.Count == 0)
				return;
			
			foreach(WizardTableInfo aTableInfo in tables)
				aTableInfo.RefreshConstraintsNames(forceReset);
		}

		//---------------------------------------------------------------------------
		public void RefreshTablesConstraintsNames()
		{
			RefreshTablesConstraintsNames(false);
		}
		
		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo GetExtraAddedColumnInfo(string aTableNameSpace, string aColumnName, bool searchDependencies)
		{
			if 
				(
				(!searchDependencies && (extraAddedColumns == null || extraAddedColumns.Count == 0)) || 
				aTableNameSpace == null || 
				aTableNameSpace.Trim().Length == 0
				)
				return null;

			if (extraAddedColumns != null && extraAddedColumns.Count > 0)
			{
				foreach(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo in extraAddedColumns)
				{
					if 
						(
						String.Compare(aTableNameSpace.Trim(), aExtraAddedColumnsInfo.TableNameSpace, true) == 0 &&
						(
						(aColumnName == null || aColumnName.Trim().Length == 0) ||
						(aExtraAddedColumnsInfo.ColumnsCount > 0 && aExtraAddedColumnsInfo.GetColumnInfoByName(aColumnName) != null)
						)
						)
						return aExtraAddedColumnsInfo;
				}
			}

			if (!searchDependencies || dependencies == null || dependencies.Count == 0)
				return null;

			foreach(WizardLibraryInfo aDependency in dependencies)
			{
				WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo = aDependency.GetExtraAddedColumnInfo(aTableNameSpace, aColumnName, false);
				if (aExtraAddedColumnsInfo != null)
					return aExtraAddedColumnsInfo;
			}
		
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo GetExtraAddedColumnInfo(string aTableNameSpace, string aColumnName)
		{
			return GetExtraAddedColumnInfo(aTableNameSpace, aColumnName, true);
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo GetExtraAddedColumnInfo(string aTableNameSpace, bool searchDependencies)
		{
			return GetExtraAddedColumnInfo(aTableNameSpace, null, searchDependencies);
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo GetExtraAddedColumnInfo(string aTableNameSpace)
		{
			return GetExtraAddedColumnInfo(aTableNameSpace, null, true);
		}

		//---------------------------------------------------------------------------
		public int AddExtraAddedColumnInfo(string aTableNameSpace, WizardTableColumnInfo aColumnInfo, bool isReadOnly, bool isReferenced, bool checkTableAvailability)
		{
			if 
				(
				aTableNameSpace == null || 
				aTableNameSpace.Length == 0 ||
				aColumnInfo == null
				)
				return -1;

			if (checkTableAvailability)
			{
				NameSpace tmpNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
				if (!tmpNameSpace.IsValid() || !IsTableAvailable(tmpNameSpace.Table))
					return -1;
			}

			WizardExtraAddedColumnsInfo existingExtraAddedColumnInfo = GetExtraAddedColumnInfo(aTableNameSpace, false);
			if (existingExtraAddedColumnInfo != null)
			{
				if (existingExtraAddedColumnInfo.GetColumnInfoByName(aColumnInfo.Name) != null)
					return -1;

				existingExtraAddedColumnInfo.AddColumnInfo(aColumnInfo);

				return extraAddedColumns.IndexOf(existingExtraAddedColumnInfo);
			}

			if (extraAddedColumns == null)
				extraAddedColumns = new WizardExtraAddedColumnsInfoCollection();

			WizardExtraAddedColumnsInfo newExtraAddedColumnInfo = new WizardExtraAddedColumnsInfo(aTableNameSpace, isReadOnly, isReferenced);

			newExtraAddedColumnInfo.AddColumnInfo(aColumnInfo);

			newExtraAddedColumnInfo.SetLibrary(this);

			return extraAddedColumns.Add(newExtraAddedColumnInfo);
		}

		//---------------------------------------------------------------------------
		public int AddExtraAddedColumnInfo(string aTableNameSpace, WizardTableColumnInfo aColumnInfo)
		{
			return AddExtraAddedColumnInfo(aTableNameSpace, aColumnInfo, readOnly, referenced, true);
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo AddExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo, bool autoDbRelease)
		{
			if 
				(
				aExtraAddedColumnsInfo == null ||
				aExtraAddedColumnsInfo.TableNameSpace == null || 
				aExtraAddedColumnsInfo.TableNameSpace.Length == 0 ||
				aExtraAddedColumnsInfo.ColumnsCount == 0
				)
				return null;

			NameSpace tmpNameSpace = new NameSpace(aExtraAddedColumnsInfo.TableNameSpace, NameSpaceObjectType.Table);
			if (!tmpNameSpace.IsValid() || !IsTableAvailable(tmpNameSpace.Table))
				return null;
	
			WizardExtraAddedColumnsInfo existingExtraAddedColumnInfo = GetExtraAddedColumnInfo(aExtraAddedColumnsInfo.TableNameSpace, false);
			if (existingExtraAddedColumnInfo != null)
			{
				foreach (WizardTableColumnInfo addedColumnInfo in aExtraAddedColumnsInfo.ColumnsInfo)
				{
					if (existingExtraAddedColumnInfo.GetColumnInfoByName(addedColumnInfo.Name) == null)
						existingExtraAddedColumnInfo.AddColumnInfo(addedColumnInfo);
				}
				return existingExtraAddedColumnInfo;
			}

			if (extraAddedColumns == null)
				extraAddedColumns = new WizardExtraAddedColumnsInfoCollection();

			WizardExtraAddedColumnsInfo newExtraAddedColumnsInfo = new WizardExtraAddedColumnsInfo(aExtraAddedColumnsInfo);

			newExtraAddedColumnsInfo.SetLibrary(this, autoDbRelease);

			extraAddedColumns.Add(newExtraAddedColumnsInfo);
		
			return newExtraAddedColumnsInfo;
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo AddExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			return AddExtraAddedColumnsInfo(aExtraAddedColumnsInfo, false);
		}
		
		//---------------------------------------------------------------------------
		public void RemoveExtraAddedColumnInfo(string aTableNameSpace, string aColumnName)
		{
			if 
				(
				extraAddedColumns == null || 
				extraAddedColumns.Count == 0 || 
				aTableNameSpace == null || 
				aTableNameSpace.Length == 0 ||
				aColumnName == null || 
				aColumnName.Length == 0
				)
				return;

			WizardExtraAddedColumnsInfo extraAddedColumnInfoToRemove = GetExtraAddedColumnInfo(aTableNameSpace, aColumnName, false);
			if (extraAddedColumnInfoToRemove == null)
				return;

			extraAddedColumnInfoToRemove.RemoveColumn(aColumnName);
		}
		
		//---------------------------------------------------------------------------
		public void RemoveExtraAddedColumnsInfo(string aTableNameSpace)
		{
			if 
				(
				extraAddedColumns == null || 
				extraAddedColumns.Count == 0 || 
				aTableNameSpace == null || 
				aTableNameSpace.Length == 0
				)
				return;

			WizardExtraAddedColumnsInfo extraAddedColumnInfoToRemove = GetExtraAddedColumnInfo(aTableNameSpace, false);
			if (extraAddedColumnInfoToRemove == null)
				return;

			extraAddedColumnInfoToRemove.SetLibrary(null);
		}

		//---------------------------------------------------------------------------
		public void RemoveExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnInfoToRemove)
		{
			if 
				(
				extraAddedColumns == null || 
				extraAddedColumns.Count == 0 || 
				aExtraAddedColumnInfoToRemove == null || 
				aExtraAddedColumnInfoToRemove.TableNameSpace == null || 
				aExtraAddedColumnInfoToRemove.TableNameSpace.Length == 0
				)
				return;

			RemoveExtraAddedColumnsInfo(aExtraAddedColumnInfoToRemove.TableNameSpace);
		}
		
		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfoCollection GetAllAvailableTableAdditionalColumnsInfo(string aTableNameSpace, bool searchDependencies, bool checkTableAvailability)
		{
			if (aTableNameSpace == null || aTableNameSpace.Length == 0)
				return null;

			NameSpace tableNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (tableNameSpace == null || !tableNameSpace.IsValid())
				return null;

			if (checkTableAvailability && GetTableInfoByName(tableNameSpace.Table, true) == null)
				return null;

			WizardExtraAddedColumnsInfoCollection allAddedColumnsInfo = new WizardExtraAddedColumnsInfoCollection();

			WizardExtraAddedColumnsInfo addedColumnsInfo = GetExtraAddedColumnInfo(aTableNameSpace, false);
			if (addedColumnsInfo != null && addedColumnsInfo.ColumnsCount > 0)
				allAddedColumnsInfo.Add(addedColumnsInfo);

			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
				{
					if (checkTableAvailability && aDependency.GetTableInfoByName(tableNameSpace.Table, true) == null)
						continue;
					
					addedColumnsInfo = aDependency.GetExtraAddedColumnInfo(aTableNameSpace);
					if (addedColumnsInfo != null && addedColumnsInfo.ColumnsCount > 0)
						allAddedColumnsInfo.Add(addedColumnsInfo);
				}
			}

			return (allAddedColumnsInfo.Count > 0) ? allAddedColumnsInfo : null;
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfoCollection GetAllAvailableTableAdditionalColumnsInfo(string aTableNameSpace, bool checkTableAvailability)
		{
			return GetAllAvailableTableAdditionalColumnsInfo(aTableNameSpace, true, checkTableAvailability);
		}
		
		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfoCollection GetAllAvailableTableAdditionalColumnsInfo(string aTableNameSpace)
		{
			return GetAllAvailableTableAdditionalColumnsInfo(aTableNameSpace, true, true);
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection GetAllAvailableExtraAddedColumns(string aTableNameSpace, bool searchDependencies, bool checkTableAvailability)
		{
			if (aTableNameSpace == null || aTableNameSpace.Length == 0)
				return null;

			NameSpace tableNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (tableNameSpace == null || !tableNameSpace.IsValid())
				return null;

			if (checkTableAvailability && GetTableInfoByName(tableNameSpace.Table, true) == null)
				return null;

			WizardTableColumnInfoCollection allAddedColumns = new WizardTableColumnInfoCollection();

			WizardExtraAddedColumnsInfo addedColumnsInfo = GetExtraAddedColumnInfo(aTableNameSpace);
			if (addedColumnsInfo != null && addedColumnsInfo.ColumnsCount > 0)
				allAddedColumns.AddRange(addedColumnsInfo.ColumnsInfo);

			if (searchDependencies && dependencies != null && dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in dependencies)
					allAddedColumns.AddRange(aDependency.GetAllAvailableExtraAddedColumns(aTableNameSpace, false, checkTableAvailability));
			}

			return (allAddedColumns.Count > 0) ? allAddedColumns : null;
		}
		
		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection GetAllAvailableExtraAddedColumns(string aTableNameSpace, bool checkTableAvailability)
		{
			return GetAllAvailableExtraAddedColumns(aTableNameSpace, true, checkTableAvailability);
		}
		
		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection GetAllAvailableExtraAddedColumns(string aTableNameSpace)
		{
			return GetAllAvailableExtraAddedColumns(aTableNameSpace, true, true);
		}
		
		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection GetAllAvailableExtraAddedColumns(WizardTableInfo aTableInfo)
		{
			if 
				(
				aTableInfo == null || 
				aTableInfo.Name == null ||
				aTableInfo.Name.Length == 0 ||
				!IsTableAvailable(aTableInfo.Name)
				)
				return null;

			return GetAllAvailableExtraAddedColumns(aTableInfo.GetNameSpace(), false);
		}
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection GetAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if (aDBTInfo == null || !IsDBTAvailable(aDBTInfo))
				return null;
 
			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = new WizardDocumentTabbedPaneInfoCollection();
			
			if (documents != null && documents.Count > 0)
			{
				foreach(WizardDocumentInfo aDocumentInfo in documents)
					dbtTabbedPanes.AddRange(aDocumentInfo.GetAllDBTTabbedPanes(aDBTInfo));
			}
			
			if (clientDocuments != null && clientDocuments.Count > 0)
			{
				foreach(WizardClientDocumentInfo aClientDocumentInfo in clientDocuments)
					dbtTabbedPanes.AddRange(aClientDocumentInfo.GetAllDBTTabbedPanes(aDBTInfo));
			}

			return (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0) ? dbtTabbedPanes : null;
		}

		#endregion // WizardLibraryInfo public methods
	}

	#endregion // WizardLibraryInfo class

	#region WizardLibraryInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardLibraryInfoCollection.
	/// </summary>
	public class WizardLibraryInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardLibraryInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardLibraryInfo))
					throw new NotSupportedException();

				this[index] = (WizardLibraryInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardLibraryInfo))
				throw new NotSupportedException();

			return this.Contains((WizardLibraryInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardLibraryInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardLibraryInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardLibraryInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardLibraryInfo))
				throw new NotSupportedException();

			Insert(index, (WizardLibraryInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardLibraryInfo))
				throw new NotSupportedException();

			Remove((WizardLibraryInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardLibraryInfo this[int index]
		{
			get {  return (WizardLibraryInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardLibraryInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo[] ToArray()
		{
			return (WizardLibraryInfo[])InnerList.ToArray(typeof(WizardLibraryInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardLibraryInfo aLibraryToAdd)
		{
			if (Contains(aLibraryToAdd))
				return IndexOf(aLibraryToAdd);

			return InnerList.Add(aLibraryToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardLibraryInfoCollection aLibrariesCollectionToAdd)
		{
			if (aLibrariesCollectionToAdd == null || aLibrariesCollectionToAdd.Count == 0)
				return;

			foreach (WizardLibraryInfo aLibraryToAdd in aLibrariesCollectionToAdd)
				Add(aLibraryToAdd);
		}

		//---------------------------------------------------------------------------
		public int AddRange(WizardLibraryInfo aLibraryToAdd)
		{
			if (Contains(aLibraryToAdd))
				return IndexOf(aLibraryToAdd);

			return InnerList.Add(aLibraryToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardLibraryInfo aLibraryToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aLibraryToInsert))
				return;

			InnerList.Insert(index, aLibraryToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardLibraryInfo beforeLibrary, WizardLibraryInfo aLibraryToInsert)
		{
			if (beforeLibrary == null)
				Add(aLibraryToInsert);

			if (!Contains(beforeLibrary))
				return;

			if (Contains(aLibraryToInsert))
				return;

			Insert(IndexOf(beforeLibrary), aLibraryToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardLibraryInfo aLibraryToRemove)
		{
			if (!Contains(aLibraryToRemove))
				return;

			InnerList.Remove(aLibraryToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardLibraryInfo aLibraryToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aLibraryToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardLibraryInfo aLibraryToSearch)
		{
			if (!Contains(aLibraryToSearch))
				return -1;
			
            return InnerList.IndexOf(aLibraryToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardLibraryInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardLibraryInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardLibraryInfo.Equals(this[i], ((WizardLibraryInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetLibraryInfoByName(string aLibraryName)
		{
			if (this.Count == 0 || aLibraryName == null || aLibraryName.Length == 0)
				return null;

			foreach(WizardLibraryInfo aLibraryInfo in InnerList)
			{
				if (String.Compare(aLibraryName, aLibraryInfo.Name, true) == 0)
					return aLibraryInfo;
			}
			
			return null;
		}
	}

	#endregion // WizardLibraryInfoCollection class
	
	#region WizardDocumentInfo class
	//=================================================================================
	/// <summary>
	/// Summary description for WizardDocumentInfo.
	/// </summary>
	public class WizardDocumentInfo : IDisposable
	{
		#region WizardDocumentInfo private data members

		public enum DocumentType : ushort
		{
			Undefined	= 0x0000,
			DataEntry	= 0x0001,
			Batch		= 0x0002,
			Finder		= 0x0003
		}
		
		private WizardLibraryInfo library = null;

		private string	name = String.Empty;
		private string	className = String.Empty;
		private string	classHierarchy = String.Empty;
		private string	title = String.Empty;
		
		private DocumentType defaultType = DocumentType.DataEntry;

		private WizardDBTInfoCollection dbts = null;

		private WizardHotKeyLinkInfo hotKeyLink = null;

		private WizardDocumentTabbedPaneInfoCollection tabbedPanes = null;
		
		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;
		private bool disposed = false;

		private int width;
		private int height;
		private Position tabberSize;
		private List<LabelInfo> labelInfoCollection;
		#endregion

		public Position TabberSize { get { return tabberSize; } set { tabberSize = value; } }
		public int Width { get { return width; } set { width = value; }	}
		public int Height {	get { return height; } set { height = value; } }
		public List<LabelInfo> LabelInfoCollection { get { return labelInfoCollection; } set { labelInfoCollection = value; } }

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(string aDocumentName, WizardLibraryInfo aLibraryInfo, bool isReadOnly, bool isReferenced)
		{
			library = aLibraryInfo;	
			Name = aDocumentName;
			readOnly = isReadOnly;
			referenced = isReferenced;
			width = height = 0;
			TabberSize = new Position();
			labelInfoCollection = new List<LabelInfo>();
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(string aDocumentName, WizardLibraryInfo aLibraryInfo, bool isReadOnly) : this(aDocumentName, aLibraryInfo, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(string aDocumentName, WizardLibraryInfo aLibraryInfo) : this(aDocumentName, aLibraryInfo, false, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(string aDocumentName, bool isReadOnly, bool isReferenced) : this(aDocumentName, null, isReadOnly, isReferenced)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(string aDocumentName, bool isReadOnly) : this(aDocumentName, null, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(string aDocumentName) : this(aDocumentName, null)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo(WizardDocumentInfo aDocumentInfo)
		{
			library = (aDocumentInfo != null) ? aDocumentInfo.Library : null;

			name = (aDocumentInfo != null) ? aDocumentInfo.Name : String.Empty;
			className = (aDocumentInfo != null) ? aDocumentInfo.ClassName : String.Empty;
			classHierarchy = (aDocumentInfo != null) ? aDocumentInfo.ClassHierarchy : String.Empty;

			defaultType = (aDocumentInfo != null) ? aDocumentInfo.DefaultType : DocumentType.Undefined;

			title = (aDocumentInfo != null) ? aDocumentInfo.Title : String.Empty;
		
			hotKeyLink = (aDocumentInfo != null && aDocumentInfo.HotKeyLink != null) ? new WizardHotKeyLinkInfo(aDocumentInfo.HotKeyLink) : null;
			
			readOnly = (aDocumentInfo != null) ? aDocumentInfo.ReadOnly : false;
			referenced = (aDocumentInfo != null) ? aDocumentInfo.IsReferenced : false;
			
			if (aDocumentInfo != null && aDocumentInfo.DBTsCount > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in aDocumentInfo.DBTsInfo)
					this.AddDBTInfo(new WizardDBTInfo(aDBTInfo));
			}
			
			if (aDocumentInfo != null && aDocumentInfo.TabbedPanesCount > 0)
			{
				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in aDocumentInfo.TabbedPanes)
					this.AddTabbedPane(new WizardDocumentTabbedPaneInfo(aTabbedPaneInfo));
			}

			width = aDocumentInfo.width;
			height = aDocumentInfo.height;

			if (aDocumentInfo.tabberSize.isSet)
				tabberSize = new Position(aDocumentInfo.tabberSize);
			else
				tabberSize = new Position();

			labelInfoCollection = new List<LabelInfo>();
			aDocumentInfo.labelInfoCollection.ForEach(l => {
				labelInfoCollection.Add(new LabelInfo(l));
			});
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDocumentInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name,((WizardDocumentInfo)obj).Name) == 0 &&
				String.Compare(className,((WizardDocumentInfo)obj).ClassName) == 0 &&
				String.Compare(classHierarchy,((WizardDocumentInfo)obj).ClassHierarchy) == 0 &&
				defaultType == ((WizardDocumentInfo)obj).DefaultType &&
				String.Compare(title,((WizardDocumentInfo)obj).Title) == 0 &&
				WizardHotKeyLinkInfo.Equals(hotKeyLink, ((WizardDocumentInfo)obj).HotKeyLink) &&
				WizardDBTInfoCollection.Equals(dbts,((WizardDocumentInfo)obj).DBTsInfo) &&
				WizardDocumentTabbedPaneInfoCollection.Equals(tabbedPanes,((WizardDocumentInfo)obj).TabbedPanes)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo)
		{
			if (library == aLibraryInfo)
				return;

			if (library != null && library.DocumentsInfo.Contains(this))
				library.DocumentsInfo.Remove(this);

			if (aLibraryInfo != null)
			{
				// Se aggiungo ad una libreria un documento devo controllare che i DBT in esso utilizzati
				// e le tabelle alle quali essi fanno riferimento siano visibili, cioè che siano definiti
				// in librerie dalle quali dipende la libreria corrente. In caso contrario, inserisco
				// tali librerie nelle dipendenze
				if (dbts != null && dbts.Count > 0)
				{
					foreach (WizardDBTInfo aDBTInfo in dbts)
					{
						if (aDBTInfo.Library != null)
							aLibraryInfo.AddDependency(aDBTInfo.Library);

						if (aDBTInfo.TableName == null || aDBTInfo.TableName.Length == 0 || aLibraryInfo.IsTableAvailable(aDBTInfo.TableName))
							continue;
					
						if (aLibraryInfo.Application != null)
						{
							WizardTableInfo dbtTableInfo = aLibraryInfo.Application.GetTableInfoByName(aDBTInfo.TableName);
							if (dbtTableInfo != null && dbtTableInfo.Library != null)
								aLibraryInfo.AddDependency(dbtTableInfo.Library);
						}
					}
				}
			}

			library = aLibraryInfo;
		}


		#region WizardDocumentInfo public properties

		//---------------------------------------------------------------------------
		public WizardLibraryInfo Library { get { return library; } }
		
		//---------------------------------------------------------------------------
		public string Name
		{ 
			get { return name; } 
			set 
			{ 
				if (Generics.IsValidDocumentName(value)) 
				{
					name = value; 
					if (className == null || className.Length == 0)
						className = GetDefaultDocumentClassName(name, defaultType);
				}
			} 
		}
		
		//---------------------------------------------------------------------------
		public string ClassName
		{
			get { return className; } 
			set 
			{ 
				if (value == null || !Generics.IsValidClassName(value))
					className = GetDefaultDocumentClassName(name, defaultType);
				else
					className = value.Trim(); 
			}
		}

		//---------------------------------------------------------------------------
		public string ClassHierarchy
		{
			get { return classHierarchy; } 
			set 
			{ 
				string hierarchyToSet = String.Empty;

				if (value != null && value.Trim().Length > 0)
				{
					string[] classes = value.Trim().Split('.');
					if (classes != null && classes.Length > 0)
					{
						for(int i=0; i < classes.Length; i++)
						{
							string aClass = classes[i].Trim();
							
							if (aClass.Length == 0 || !Generics.IsValidClassName(aClass))
								throw new ArgumentException();

							if (hierarchyToSet.Length > 0)
								hierarchyToSet += '.';

							hierarchyToSet += aClass;
						}
					}
				}

				classHierarchy = hierarchyToSet;
			}
		}

		//---------------------------------------------------------------------------
		public DocumentType DefaultType { get { return defaultType; } set { defaultType = value; } }
		
		//---------------------------------------------------------------------------
		public bool DefaultViewIsDataEntry 
		{
			get { return (defaultType == DocumentType.Undefined || defaultType == DocumentType.DataEntry); } 
			set { defaultType = DocumentType.DataEntry; }
		}
		
		//---------------------------------------------------------------------------
		public bool DefaultViewIsBatch 
		{
			get { return (defaultType == DocumentType.Batch); } 
			set { defaultType = DocumentType.Batch; }
		}
		
		//---------------------------------------------------------------------------
		public bool DefaultViewIsDataFinder
		{
			get { return (defaultType == DocumentType.Finder); } 
			set { defaultType = DocumentType.Finder; }
		}

		//---------------------------------------------------------------------------
		public string Title { get { return title; } set { title = value; } }
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection DBTsInfo { get { return dbts; } }

		//---------------------------------------------------------------------------
		public int DBTsCount { get { return (dbts != null) ? dbts.Count : 0; } }

		//---------------------------------------------------------------------------
		public WizardDBTInfo DBTMaster
		{
			get
			{
				if (dbts == null || dbts.Count == 0)
					return null;

				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (aDBTInfo.IsMaster)
						return aDBTInfo;
				}
				return null;				
			}
			set
			{
				if 
					(
					library == null || 
					(value != null && (value.Name == null || value.Name.Length == 0 || !value.IsMaster))
					)
					return;

				WizardDBTInfo currentDBTMaster = this.DBTMaster;
				if (currentDBTMaster == value)
					return;

				if (value != null && !library.IsDBTAvailable(value))
					return; // il DBT non è stato trovato fra quelli della libreria e nemmeno nelle dipendenze

				if (currentDBTMaster != null)
					RemoveDBT(currentDBTMaster);

				AddDBTInfo(value);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsDBTMasterDefined { get { return (DBTMaster != null); } }

		//---------------------------------------------------------------------------
		public WizardTableInfo DBTMasterTable
		{
			get
			{
				WizardDBTInfo masterInfo = this.DBTMaster;
				if (masterInfo == null)
					return null;

				return masterInfo.GetTableInfo();				
			}
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection DBTsSlaves
		{
			get
			{
				if (dbts == null || dbts.Count == 0)
					return null;

				WizardDBTInfoCollection slaves = null;
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (aDBTInfo.IsSlave || aDBTInfo.IsSlaveBuffered)
					{
						if (slaves == null)
							slaves = new WizardDBTInfoCollection();

						slaves.Add(aDBTInfo);
					}
				}
				return slaves;				
			}
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection TabbedPanes { get { return tabbedPanes; } }

		//---------------------------------------------------------------------------
		public int TabbedPanesCount { get { return (tabbedPanes != null) ? tabbedPanes.Count : 0; } }

		//---------------------------------------------------------------------------
		public ushort FirstResourceId
		{
			get 
			{ 
				return (library != null) ? library.GetDocumentFirstResourceId(this) : Generics.FirstValidResourceId; 
			} 
		}

		//---------------------------------------------------------------------------
		public ushort FirstControlId
		{
			get 
			{ 
				return (library != null) ? library.GetDocumentFirstControlId(this) : Generics.FirstValidControlId; 
			} 
		}

		//---------------------------------------------------------------------------
		public ushort FirstCommandId
		{
			get 
			{ 
				return (library != null) ? library.GetDocumentFirstCommandId(this) : Generics.FirstValidCommandId; 
			} 
		}

		//---------------------------------------------------------------------------
		public ushort FirstSymedId
		{
			get 
			{ 
				return (library != null) ? library.GetDocumentFirstSymedId(this) : Generics.FirstValidSymedId; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo HotKeyLink 
		{
			get { return hotKeyLink; } 
		}

		//---------------------------------------------------------------------------
		public bool IsHKLDefined 
		{ 
			get 
			{
				return 
					(
					hotKeyLink != null && 
					hotKeyLink.IsDefined &&
					hotKeyLink.CodeColumn != null &&
					hotKeyLink.DescriptionColumn != null
					); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public string HKLName
		{ 
			get 
			{ 
				if (!IsHKLDefined)
					return String.Empty;

				return hotKeyLink.Name; 
			} 
			set
			{
				if (IsHKLDefined)
					hotKeyLink.Name = value;
			}
		}

		//---------------------------------------------------------------------------
		public string HKLClassName
		{ 
			get 
			{ 
				if (!IsHKLDefined)
					return String.Empty;

				return hotKeyLink.ClassName; 
			} 
			set
			{
				if (IsHKLDefined)
					hotKeyLink.ClassName = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public string DefaultHKLName
		{ 
			get 
			{ 
				return GetDefaultDocumentHKLName(name); 
			} 
		}

		//---------------------------------------------------------------------------
		public string DefaultHKLClassName
		{ 
			get 
			{ 
				return GetDefaultDocumentHKLClassName(name); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public string HKLNamespace
		{ 
			get 
			{ 
				if (!IsHKLDefined)
					return String.Empty;

				if (hotKeyLink.IsReferenced)
					return hotKeyLink.ReferencedNameSpace;

				if (library == null)
					return String.Empty;

				string libraryNamespace = library.GetNameSpace();
				if (libraryNamespace == null || libraryNamespace.Length == 0)
					return String.Empty;

				return libraryNamespace + "." + HKLClassName; 
			} 
		}

		//---------------------------------------------------------------------------
		public string HKLCodeColumnName 
		{
			get { return (hotKeyLink != null) ? hotKeyLink.CodeColumnName : String.Empty; } 
		}
		
		//---------------------------------------------------------------------------
		public WizardTableColumnInfo HKLCodeColumn 
		{
			get 
			{ 
				WizardDBTInfo master = this.DBTMaster;
				if (master == null)
					return null;

				return master.GetTableColumnInfoByName(this.HKLCodeColumnName); 
			} 
			set 
			{ 
				WizardDBTInfo master = this.DBTMaster;
				if (master == null)
					return;
				
				WizardTableInfo masterTable = master.GetTableInfo();

				if 
					(
					masterTable == null || 
					masterTable.ColumnsInfo == null ||
					masterTable.ColumnsCount == 0 || 
					(value != null && masterTable.GetColumnInfoByName(value.Name) == null)
					)
					return;

				if (hotKeyLink == null)
				{
					hotKeyLink = new WizardHotKeyLinkInfo(masterTable);
					if (name != null && name.Length > 0) 
						hotKeyLink.Name = name;
					if (title != null && title.Length > 0) 
						hotKeyLink.Title = title;
					hotKeyLink.ClassName = DefaultHKLClassName;
				}

				hotKeyLink.CodeColumn = value;
			} 
		}


		//---------------------------------------------------------------------------
		public bool IsHKLCodeColumnTextual 
		{
			get 
			{
				WizardTableColumnInfo hklCodeColumn = this.HKLCodeColumn;
				if (hklCodeColumn == null)
					return false;
 
				return hklCodeColumn.DataType.IsTextual;
			} 
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo HKLDescriptionColumn 
		{
			get 
			{
				WizardDBTInfo master = this.DBTMaster;
				if (master == null)
					return null;

				return master.GetTableColumnInfoByName(this.HKLDescriptionColumnName); 
			} 
			set 
			{ 
				WizardDBTInfo master = this.DBTMaster;
				if (master == null)
					return;
				
				WizardTableInfo masterTable = master.GetTableInfo();

				if 
					(
					masterTable == null || 
					masterTable.ColumnsInfo == null ||
					masterTable.ColumnsCount == 0 || 
					(value != null && masterTable.GetColumnInfoByName(value.Name) == null)
					)
					return;

				if (hotKeyLink == null)
				{
					hotKeyLink = new WizardHotKeyLinkInfo(masterTable);
					if (name != null && name.Length > 0) 
						hotKeyLink.Name = name;
					if (title != null && title.Length > 0) 
						hotKeyLink.Title = title;
					hotKeyLink.ClassName = DefaultHKLClassName;
				}

				hotKeyLink.DescriptionColumn = value;
			} 
		}
		
		//---------------------------------------------------------------------------
		public string HKLDescriptionColumnName 
		{
			get { return (hotKeyLink != null) ? hotKeyLink.DescriptionColumnName : String.Empty; } 
		}

		//---------------------------------------------------------------------------
		public bool IsHKLDescriptionColumnTextual 
		{
			get 
			{
				WizardTableColumnInfo hklDescriptionColumn = this.HKLDescriptionColumn;
				if (hklDescriptionColumn == null)
					return false;
 
				return hklDescriptionColumn.DataType.IsTextual;
			} 
		}

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)

		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 
		
		#endregion

		#region WizardDocumentInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo GetDBTInfoByName(string aDBTName)
		{
			if (aDBTName == null || aDBTName.Length == 0)
				return null;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (String.Compare(aDBTInfo.Name, aDBTName) == 0)
						return aDBTInfo;
				}
			}

			return null;
		}
	
		//---------------------------------------------------------------------------
		public int AddDBTInfo(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				library == null ||
				aDBTInfo == null || 
				aDBTInfo.Name == null || 
				aDBTInfo.Name.Length == 0 || 
				aDBTInfo.TableName == null || 
				aDBTInfo.TableName.Length == 0 ||
				(dbts != null && dbts.Contains(aDBTInfo))
				)
				return -1;

			WizardDBTInfo existingDBT = GetDBTInfoByName(aDBTInfo.Name);
			if (existingDBT != null)
				return -1;

			if (!library.IsDBTAvailable(aDBTInfo))
				return -1; // il DBT non è stato trovato fra quelli della libreria e nemmeno nelle dipendenze

			if (library.Application != null && !library.IsTableAvailable(aDBTInfo.TableName))
			{
				WizardTableInfo dbtTableInfo = library.Application.GetTableInfoByName(aDBTInfo.TableName);
				if (dbtTableInfo != null && dbtTableInfo.Library != null)
					library.AddDependency(dbtTableInfo.Library);
			}

			if (aDBTInfo.IsMaster && this.IsDBTMasterDefined) 
				return -1; // in un documento ci può essere un solo master
			
			if (dbts == null)
				dbts = new WizardDBTInfoCollection();

			if (aDBTInfo.IsMaster && dbts.Count > 0) // Metto sempre il master per primo
			{
				dbts.Insert(0, aDBTInfo);
				return 0;
			}

			int addedIdx = dbts.Add(aDBTInfo);
			if (addedIdx >= 0 && !aDBTInfo.IsMaster)
				AddDBTTabbedPane(aDBTInfo);
			return addedIdx;
		}
	
		//---------------------------------------------------------------------------
		public void RemoveDBT(WizardDBTInfo aDBTInfoToRemove)
		{
			if (dbts == null || dbts.Count == 0 || aDBTInfoToRemove == null || !dbts.Contains(aDBTInfoToRemove))
				return;

			RemoveAllDBTTabbedPanes(aDBTInfoToRemove);
			dbts.Remove(aDBTInfoToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllDBTs()
		{
			if (dbts == null || dbts.Count == 0)
				return;

			RemoveAllTabbedPanes();

			dbts.Clear();
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo GetOriginalDBTTabbedPane(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				aDBTInfo == null || 
				aDBTInfo.IsMaster ||
				!dbts.Contains(aDBTInfo)
				)
				return null;
	
			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
			{
				if 
					(
					aTabbedPaneInfo != null &&
					String.Compare(aTabbedPaneInfo.DBTName, aDBTInfo.Name) == 0 &&
					aTabbedPaneInfo.ManagedColumnsCount == 0
					)
					return aTabbedPaneInfo;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection GetAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				aDBTInfo == null || 
				!dbts.Contains(aDBTInfo)
				)
				return null;
			
			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = new WizardDocumentTabbedPaneInfoCollection();

			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
			{
				if (aTabbedPaneInfo == null || String.Compare(aTabbedPaneInfo.DBTName, aDBTInfo.Name) != 0)
					continue;
				dbtTabbedPanes.Add(aTabbedPaneInfo);
			}

			return (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0) ? dbtTabbedPanes : null;
		}

		//---------------------------------------------------------------------------
		public int AddTabbedPane(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if 
				(
				library == null ||
				aTabbedPaneInfo == null || 
				aTabbedPaneInfo.DBTInfo == null || 
				dbts == null || 
				dbts.Count == 0 || 
				!dbts.Contains(aTabbedPaneInfo.DBTInfo) || 
				aTabbedPaneInfo.TableName == null || 
				aTabbedPaneInfo.TableName.Length == 0 ||
				(tabbedPanes != null && tabbedPanes.Contains(aTabbedPaneInfo))
				)
				return -1;

			if (GetDBTInfoByName(aTabbedPaneInfo.DBTName) == null)
				return -1; // il DBT non è stato trovato fra quelli gestiti dal documento

			if (tabbedPanes != null)
			{
				// Se si tratta di una scheda di un DBT (cioè sprovvista della specifica di sue
				// managedColumns) e ho già caricato le informazioni relative alla scheda equivalente
				// (che viene, infatti, generata automaticamente per ciascun DBT aggiunto al documento), 
				// elimino quest'ultima in modo da garantire l'ordine corretto di tutte le schede...
				if (aTabbedPaneInfo.ManagedColumnsCount == 0)
				{
					WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = GetAllDBTTabbedPanes(aTabbedPaneInfo.DBTInfo);
					if (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0)
					{
						foreach (WizardDocumentTabbedPaneInfo sameDBTTabbedPane in dbtTabbedPanes)
						{
							if (sameDBTTabbedPane.ManagedColumnsCount == 0)
								tabbedPanes.Remove(sameDBTTabbedPane);
						}
					}		
				}
			}
			else
				tabbedPanes = new WizardDocumentTabbedPaneInfoCollection();

			return tabbedPanes.Add(aTabbedPaneInfo);
		}
	
		//---------------------------------------------------------------------------
		public int AddDBTTabbedPane(WizardDBTInfo aDBTInfo)
		{
			return AddTabbedPane(new WizardDocumentTabbedPaneInfo(aDBTInfo));
		}
		
		//---------------------------------------------------------------------------
		public void InsertTabbedPane(int index, WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if 
				(
				library == null ||
				aTabbedPaneInfo == null || 
				aTabbedPaneInfo.DBTInfo == null || 
				dbts == null || 
				dbts.Count == 0 || 
				!dbts.Contains(aTabbedPaneInfo.DBTInfo) || 
				aTabbedPaneInfo.TableName == null || 
				aTabbedPaneInfo.TableName.Length == 0 ||
				(tabbedPanes != null && tabbedPanes.Contains(aTabbedPaneInfo)) ||
				index < 0 || 
				(tabbedPanes != null && index > tabbedPanes.Count - 1) ||
				(tabbedPanes == null && index > 0)
				)
				return;

			if (GetDBTInfoByName(aTabbedPaneInfo.DBTName) == null)
				return; // il DBT non è stato trovato fra quelli gestiti dal documento

			if (tabbedPanes == null)
				tabbedPanes = new WizardDocumentTabbedPaneInfoCollection();

            if (index < tabbedPanes.Count)
                tabbedPanes.Insert(index, aTabbedPaneInfo);
            else
                tabbedPanes.Add(aTabbedPaneInfo);
        }
	
		//---------------------------------------------------------------------------
		public void InsertDBTTabbedPane(int index, WizardDBTInfo aDBTInfo)
		{
			InsertTabbedPane(index, new WizardDocumentTabbedPaneInfo(aDBTInfo));
		}

		//---------------------------------------------------------------------------
		public void RemoveTabbedPane(WizardDocumentTabbedPaneInfo aTabbedPaneToRemove)
		{
			if (tabbedPanes == null || tabbedPanes.Count == 0 || aTabbedPaneToRemove == null || !tabbedPanes.Contains(aTabbedPaneToRemove))
				return;

			tabbedPanes.Remove(aTabbedPaneToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				aDBTInfo == null || 
				!dbts.Contains(aDBTInfo)
				)
				return;

			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = GetAllDBTTabbedPanes(aDBTInfo);
			if (dbtTabbedPanes == null || dbtTabbedPanes.Count == 0)
				return;

			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in dbtTabbedPanes)
				tabbedPanes.Remove(aTabbedPaneInfo);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllTabbedPanes()
		{
			if (tabbedPanes == null || tabbedPanes.Count == 0)
				return;

			tabbedPanes.Clear();
		}

		//---------------------------------------------------------------------------
		public string GetNameSpace()
		{
			if (library == null)
				return String.Empty;

			string libraryNameSpace = library.GetNameSpace();
			if (libraryNameSpace == null || libraryNameSpace.Length == 0)
				return String.Empty;

			return libraryNameSpace + "." + name;
		}
	
		//---------------------------------------------------------------------------
		public static string GetDefaultDocumentClassName(string aDocumentName, DocumentType aDocumentDefaultType)
		{
			if (aDocumentName == null || aDocumentName.Trim().Length == 0)
				return String.Empty;

			string documentClassPrefix = String.Empty;
			if (aDocumentDefaultType == DocumentType.Batch)
				documentClassPrefix = "BD";
			else 
				documentClassPrefix = "D";

			return documentClassPrefix + Generics.SubstitueInvalidCharacterInIdentifier(aDocumentName.Trim().Replace(' ', '_'));
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultDocumentClassName(string aDocumentName)
		{
			return GetDefaultDocumentClassName(aDocumentName, DocumentType.DataEntry);
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultDocumentHKLName(string aDocumentName)
		{
			return WizardHotKeyLinkInfo.GetDefaultHKLName(aDocumentName);
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultDocumentHKLClassName(string aDocumentName)
		{
			return WizardHotKeyLinkInfo.GetDefaultHKLClassName(aDocumentName);
		}

		//---------------------------------------------------------------------------
		public bool UsesExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			if 
				(
				aExtraAddedColumnsInfo == null || 
				aExtraAddedColumnsInfo.TableName == null ||
				aExtraAddedColumnsInfo.TableName.Length == 0 ||
				(this.DBTsCount == 0 && this.TabbedPanesCount == 0)
				)
				return false;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if 
						(
						aDBTInfo != null &&
						String.Compare(aDBTInfo.TableName, aExtraAddedColumnsInfo.TableName) == 0 && 
						aDBTInfo.HasVisibleAdditionalColumns(aExtraAddedColumnsInfo)
						)
						return true;
				}
			}

			if (tabbedPanes != null && tabbedPanes.Count > 0)
			{
				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
				{
					if 
						(
						aTabbedPaneInfo != null &&
						String.Compare(aTabbedPaneInfo.TableName, aExtraAddedColumnsInfo.TableName) == 0 && 
						aTabbedPaneInfo.HasVisibleAdditionalColumns(aExtraAddedColumnsInfo)
						)
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableResourceId()
		{
			return (ushort)(FirstResourceId + GetUsedResourceIdsCount());
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedResourceIdsCount()
		{
			if (tabbedPanes == null || tabbedPanes.Count == 0)
				return (ushort)1; // solo l'identificatore della dialog

			int dialogsCount = tabbedPanes.Count;
			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
			{
				if (aTabbedPaneInfo.CreateRowForm)
					dialogsCount++;
			}

			return (ushort)(1 + dialogsCount);
		}

		//---------------------------------------------------------------------------
		public int GetDocumentFormId()
		{
			return this.FirstResourceId;
		}
		
		//---------------------------------------------------------------------------
		public int GetTabbedPaneFormId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (aTabbedPaneInfo == null)
				return -1;

			WizardDocumentTabbedPaneInfoCollection tabbedPanes = this.TabbedPanes;
			
			if (tabbedPanes == null || tabbedPanes.Count == 0 || !tabbedPanes.Contains(aTabbedPaneInfo))
				return -1;

			int dialogsCount = 0;
			for (int tabbedPaneIdx = 0; tabbedPaneIdx < tabbedPanes.IndexOf(aTabbedPaneInfo); tabbedPaneIdx++)
			{
				dialogsCount++;
				if (tabbedPanes[tabbedPaneIdx].DBTInfo != null && tabbedPanes[tabbedPaneIdx].DBTInfo.CreateRowForm)
					dialogsCount++;
			}

			return FirstResourceId + dialogsCount + 1;
		}

		//---------------------------------------------------------------------------
		public int GetTabbedPaneRowFormId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (aTabbedPaneInfo == null || !aTabbedPaneInfo.CreateRowForm)
				return -1;
			
			WizardDocumentTabbedPaneInfoCollection tabbedPanes = this.TabbedPanes;
			
			if (tabbedPanes == null || tabbedPanes.Count == 0 || !tabbedPanes.Contains(aTabbedPaneInfo))
				return -1;

			return GetTabbedPaneFormId(aTabbedPaneInfo) + 1;
		}

		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableControlId()
		{
			return (ushort)(FirstControlId + GetUsedControlIdsCount());
		}

		//---------------------------------------------------------------------------
		public ushort GetUsedControlIdsCount()
		{
			if (dbts == null || dbts.Count == 0)
				return 0;

			ushort controlsCount = 0;

			WizardDBTInfo master = this.DBTMaster;
			if (master != null)
				controlsCount += master.GetUsedControlIdsCount();

			if (IsTabberToCreate())
			{
				controlsCount++;

				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
					controlsCount += aTabbedPaneInfo.GetUsedControlIdsCount();
			}

			return controlsCount; 
		}

		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableCommandId()
		{
			return (ushort)(FirstCommandId + GetUsedCommandIdsCount());
		}
	
		//---------------------------------------------------------------------------
		public int GetFirstDBTControlId(WizardDBTInfo aDBTInfo, bool isBodyEdit)
		{
			if (aDBTInfo == null || dbts == null || dbts.Count == 0 || !dbts.Contains(aDBTInfo))
				return -1;

			WizardDBTInfo master = this.DBTMaster;
			if (master == null)
				return -1;

			if (aDBTInfo == master)
				return FirstControlId;

			WizardDBTInfoCollection slaves = this.DBTsSlaves;
			
			if (slaves == null || slaves.Count == 0 || !slaves.Contains(aDBTInfo))
				return -1;

			WizardDocumentTabbedPaneInfo dbtTabbedPane = GetOriginalDBTTabbedPane(aDBTInfo);
			if (dbtTabbedPane == null)
				return -1;

			int firstDBTControlId = FirstControlId + master.GetUsedControlIdsCount();

			if (IsTabberToCreate())
				firstDBTControlId++;
			
			for (int i = 0; i < tabbedPanes.IndexOf(dbtTabbedPane); i++)
				firstDBTControlId += tabbedPanes[i].GetUsedControlIdsCount();

			return (!aDBTInfo.IsSlaveBuffered || isBodyEdit) ? firstDBTControlId : (firstDBTControlId + 1); 
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstDBTControlId(WizardDBTInfo aDBTInfo)
		{
			return GetFirstDBTControlId(aDBTInfo, false);
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstTabbedPaneControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo, bool isBodyEdit)
		{
			if (aTabbedPaneInfo == null || tabbedPanes == null || tabbedPanes.Count == 0 || !tabbedPanes.Contains(aTabbedPaneInfo))
				return -1;
		
			WizardDBTInfo master = this.DBTMaster;
			if (master == null)
				return -1;
			
			int firstDBTControlId = FirstControlId + master.GetUsedControlIdsCount();

			if (IsTabberToCreate())
				firstDBTControlId++;
			
			for (int i = 0; i < tabbedPanes.IndexOf(aTabbedPaneInfo); i++)
				firstDBTControlId += tabbedPanes[i].GetUsedControlIdsCount();

			return (!aTabbedPaneInfo.DBTInfo.IsSlaveBuffered || isBodyEdit) ? firstDBTControlId : (firstDBTControlId + 1); 
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstTabbedPaneControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			return GetFirstTabbedPaneControlId(aTabbedPaneInfo, false);
		}
		
		//---------------------------------------------------------------------------
		public int GetBodyEditControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (aTabbedPaneInfo == null || aTabbedPaneInfo.DBTInfo == null || !aTabbedPaneInfo.DBTInfo.IsSlaveBuffered)
				return -1;

			return GetFirstTabbedPaneControlId(aTabbedPaneInfo, true);
		}

		//---------------------------------------------------------------------------
		public int GetBodyEditControlId(WizardDBTInfo aDBTInfo)
		{
			return GetBodyEditControlId(GetOriginalDBTTabbedPane(aDBTInfo));
		}

		//---------------------------------------------------------------------------
		public int GetFirstBodyEditRowFormControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if 
				(
				aTabbedPaneInfo == null || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				!tabbedPanes.Contains(aTabbedPaneInfo)||
				!aTabbedPaneInfo.CreateRowForm
				)
				return -1;

			int firstBodyEditDBTControlId = GetFirstTabbedPaneControlId(aTabbedPaneInfo, true);

			foreach(WizardDBTColumnInfo aColumnInfo in aTabbedPaneInfo.ColumnsInfo)
			{
				if (!aColumnInfo.Visible)
					continue;
					
				firstBodyEditDBTControlId++;
		
				if (aColumnInfo.ShowHotKeyLinkDescription)
					firstBodyEditDBTControlId++;
			}

			return firstBodyEditDBTControlId + 1;
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstBodyEditRowFormControlId(WizardDBTInfo aDBTInfo)
		{
			return GetFirstBodyEditRowFormControlId(GetOriginalDBTTabbedPane(aDBTInfo));
		}
		
		//----------------------------------------------------------------------------
		public bool IsTabberToCreate()
		{
			if (this.TabbedPanesCount > 0)
				return true;

			WizardDBTInfoCollection slaves = this.DBTsSlaves;
			if (slaves == null || slaves.Count == 0)
				return false;

			foreach (WizardDBTInfo aDBTSlaveInfo in slaves)
			{
				if (aDBTSlaveInfo.HasVisibleColums())
					return true;
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public int GetTabberControlId()
		{
			if (!IsTabberToCreate())
				return -1;

			WizardDBTInfo master = this.DBTMaster;
			if (master == null)
				return FirstControlId;

			return FirstControlId + master.GetUsedControlIdsCount();
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedCommandIdsCount()
		{
			return (ushort)0; //@@TODO
		}

		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableSymedId()
		{
			return (ushort)(FirstSymedId + GetUsedSymedIdsCount());
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedSymedIdsCount()
		{
			return 0; //@@TODO
		}

		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (aDBTInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}
			if (tabbedPanes != null && tabbedPanes.Count > 0)
			{
				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
				{
					if (aTabbedPaneInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}
			return false;
		}
		
		#endregion // WizardDocumentInfo public methods
	}

	#endregion

	#region WizardDocumentInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardDocumentInfoCollection.
	/// </summary>
	public class WizardDocumentInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardDocumentInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardDocumentInfo))
					throw new NotSupportedException();

				this[index] = (WizardDocumentInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDocumentInfo))
				throw new NotSupportedException();

			return this.Contains((WizardDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDocumentInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDocumentInfo))
				throw new NotSupportedException();

			Insert(index, (WizardDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardDocumentInfo))
				throw new NotSupportedException();

			Remove((WizardDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardDocumentInfo this[int index]
		{
			get {  return (WizardDocumentInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardDocumentInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardDocumentInfo[] ToArray()
		{
			return (WizardDocumentInfo[])InnerList.ToArray(typeof(WizardDocumentInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardDocumentInfo aDocumentToAdd)
		{
			if (Contains(aDocumentToAdd))
				return IndexOf(aDocumentToAdd);

			return InnerList.Add(aDocumentToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardDocumentInfoCollection aDocumentsCollectionToAdd)
		{
			if (aDocumentsCollectionToAdd == null || aDocumentsCollectionToAdd.Count == 0)
				return;

			foreach (WizardDocumentInfo aDocumentToAdd in aDocumentsCollectionToAdd)
				Add(aDocumentToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardDocumentInfo aDocumentToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aDocumentToInsert))
				return;

			InnerList.Insert(index, aDocumentToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardDocumentInfo beforeDocument, WizardDocumentInfo aDocumentToInsert)
		{
			if (beforeDocument == null)
				Add(aDocumentToInsert);

			if (!Contains(beforeDocument))
				return;

			if (Contains(aDocumentToInsert))
				return;

			Insert(IndexOf(beforeDocument), aDocumentToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardDocumentInfo aDocumentToRemove)
		{
			if (!Contains(aDocumentToRemove))
				return;

			InnerList.Remove(aDocumentToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardDocumentInfo aDocumentToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aDocumentToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardDocumentInfo aDocumentToSearch)
		{
			if (!Contains(aDocumentToSearch))
				return -1;
			
			return InnerList.IndexOf(aDocumentToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDocumentInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardDocumentInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardDocumentInfo.Equals(this[i], ((WizardDocumentInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		public bool ContainsDocumentWithSameNamespace(WizardDocumentInfo aDocumentToSearch)
		{
			if (aDocumentToSearch == null)
				return false;

			if (Contains(aDocumentToSearch))
				return true;
				
			for (int i = 0; i < this.Count; i++)
			{
				if (String.Compare(this[i].GetNameSpace(), aDocumentToSearch.GetNameSpace()) == 0)
					return true;
			}

			return false;
		}
	}

	#endregion
	
	#region WizardDBTInfo Class

	//=================================================================================
	/// <summary>
	/// Summary description for WizardDBTInfo.
	/// </summary>
	public class WizardDBTInfo : IDisposable
	{
		public enum DBTType : ushort
		{
			Undefined		= 0x0000,
			Master			= 0x0001,
			Slave			= 0x0002,
			SlaveBuffered	= 0x0003
		}
		
		#region WizardTableColumnInfo private data members

		private WizardLibraryInfo library = null;

		private string name = String.Empty;
		private string className = String.Empty;
		private string tableName = String.Empty;

		private WizardDBTColumnInfoCollection columns = null;

		private DBTType type = DBTType.Undefined;

		private WizardDBTInfo relatedDBTMaster = null;
		private string slaveTabTitle = String.Empty;
		private bool createRowForm = false;

		private string	referencedTableIncludeFile = String.Empty;
		private bool	onlyForClientDocumentAvailable = false;
		private string	masterTableIncludeFile = String.Empty;
		private string	serverDocumentNamespace = String.Empty;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;
		private Position bodyEditPosition;
		#endregion

		public Position BodyEditPosition { get { return bodyEditPosition; } set { bodyEditPosition = value; } }

		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, WizardLibraryInfo aLibraryInfo, string aTableName, DBTType aType, bool isReadOnly, bool isReferenced)
		{
			library = aLibraryInfo;
			tableName = aTableName;
			type = aType;
			readOnly = isReadOnly;
			referenced = isReferenced;
			bodyEditPosition = new Position();
	
			this.Name = aName;

			if (
				library != null &&
				library.Module != null &&
				library.Module.Application != null &&
				tableName != null &&
				tableName.Length > 0 &&
				library.IsTableAvailable(tableName)
				) 
			{
				WizardTableInfo tableInfo = GetTableInfo(tableName);
				if (tableInfo != null && tableInfo.ColumnsInfo != null && tableInfo.ColumnsCount > 0)
				{
					foreach(WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
						this.AddColumnInfo(new WizardDBTColumnInfo(aColumnInfo));
					
					WizardTableColumnInfoCollection additionalColumnsInfo = library.GetAllAvailableExtraAddedColumns(tableInfo);
					if (additionalColumnsInfo != null && additionalColumnsInfo.Count > 0)
					{
						foreach(WizardTableColumnInfo anAdditionalColumnInfo in additionalColumnsInfo)
							this.AddColumnInfo(new WizardDBTColumnInfo(anAdditionalColumnInfo));
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, WizardLibraryInfo aLibraryInfo, string aTableName, DBTType aType, bool isReadOnly) : this(aName, aLibraryInfo, aTableName, aType, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, WizardLibraryInfo aLibraryInfo, string aTableName, DBTType aType) : this(aName, aLibraryInfo, aTableName, aType, false)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, WizardLibraryInfo aLibraryInfo) : this(aName, aLibraryInfo, String.Empty, DBTType.Undefined)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, string aTableName, DBTType aType, bool isReadOnly, bool isReferenced) : this(aName, null, aTableName, aType, isReadOnly, isReferenced)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, string aTableName, DBTType aType, bool isReadOnly) : this(aName, aTableName, aType, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo(string aName, string aTableName, DBTType aType) : this(aName, aTableName, aType, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo(WizardDBTInfo aDBTInfo)
		{
			library = (aDBTInfo != null) ? aDBTInfo.Library : null;

			name = (aDBTInfo != null) ? aDBTInfo.Name : String.Empty;
			className = (aDBTInfo != null) ? aDBTInfo.ClassName : String.Empty;
			tableName = (aDBTInfo != null) ? aDBTInfo.TableName : String.Empty;
			type = (aDBTInfo != null) ? aDBTInfo.Type : DBTType.Undefined;
			relatedDBTMaster = (aDBTInfo != null) ? aDBTInfo.RelatedDBTMaster : null;
			slaveTabTitle = (aDBTInfo != null) ? aDBTInfo.SlaveTabTitle : String.Empty;
			createRowForm = (aDBTInfo != null) ? aDBTInfo.CreateRowForm : false;
			referencedTableIncludeFile = (aDBTInfo != null) ? aDBTInfo.ReferencedTableIncludeFile : String.Empty;
			onlyForClientDocumentAvailable = (aDBTInfo != null) ? aDBTInfo.OnlyForClientDocumentAvailable : false;
			masterTableIncludeFile = (aDBTInfo != null) ? aDBTInfo.MasterTableIncludeFile : String.Empty;
			serverDocumentNamespace = (aDBTInfo != null) ? aDBTInfo.ServerDocumentNamespace : String.Empty;
			readOnly = (aDBTInfo != null) ? aDBTInfo.ReadOnly : false;
			referenced = (aDBTInfo != null) ? aDBTInfo.IsReferenced : false;
			bodyEditPosition = new Position(aDBTInfo.bodyEditPosition);

			if (aDBTInfo != null && aDBTInfo.ColumnsCount > 0)
			{
				foreach(WizardDBTColumnInfo aColumnInfo in aDBTInfo.ColumnsInfo)
					this.AddColumnInfo(new WizardDBTColumnInfo(aColumnInfo));
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDBTInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardDBTInfo)obj).Name) == 0 &&
				String.Compare(className, ((WizardDBTInfo)obj).ClassName) == 0 &&
				String.Compare(tableName, ((WizardDBTInfo)obj).TableName) == 0 &&
				type == ((WizardDBTInfo)obj).Type &&
				WizardDBTInfo.Equals(relatedDBTMaster, ((WizardDBTInfo)obj).RelatedDBTMaster) &&
				String.Compare(slaveTabTitle, ((WizardDBTInfo)obj).SlaveTabTitle) == 0 &&
				createRowForm == ((WizardDBTInfo)obj).CreateRowForm &&
				String.Compare(referencedTableIncludeFile,((WizardDBTInfo)obj).ReferencedTableIncludeFile, true) == 0 &&
				onlyForClientDocumentAvailable == ((WizardDBTInfo)obj).OnlyForClientDocumentAvailable &&
				String.Compare(masterTableIncludeFile,((WizardDBTInfo)obj).MasterTableIncludeFile, true) == 0 &&
				String.Compare(serverDocumentNamespace, ((WizardDBTInfo)obj).ServerDocumentNamespace) == 0 &&
				WizardDBTColumnInfoCollection.Equals(columns, ((WizardDBTInfo)obj).ColumnsInfo)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo, bool updateDependencies)
		{
			if (library == aLibraryInfo)
				return;

			if (library != null && library.DBTsInfo.Contains(this))
				library.DBTsInfo.Remove(this);

			if 
				(
				library == null && 
				tableName != null && 
				tableName.Length > 0 && 
				aLibraryInfo.Module != null && 
				aLibraryInfo.Module.Application != null
				)
			{
				if (columns != null)
					columns.Clear();
				
				WizardTableInfo tableInfo = aLibraryInfo.Module.Application.GetTableInfoByName(tableName);
				if (tableInfo == null)
				{
					tableName = String.Empty;
				}
				else 
				{
					if (updateDependencies)
					{
						if (!aLibraryInfo.Equals(tableInfo.Library))
							aLibraryInfo.AddDependency(tableInfo.Library);
					}
					if (tableInfo.ColumnsInfo != null && tableInfo.ColumnsCount > 0)
					{
						foreach(WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
							this.AddColumnInfo(new WizardDBTColumnInfo(aColumnInfo));
					}
					WizardTableColumnInfoCollection additionalColumnsInfo = aLibraryInfo.GetAllAvailableExtraAddedColumns(tableInfo);
					if (additionalColumnsInfo != null && additionalColumnsInfo.Count > 0)
					{
						foreach(WizardTableColumnInfo anAdditionalColumnInfo in additionalColumnsInfo)
							this.AddColumnInfo(new WizardDBTColumnInfo(anAdditionalColumnInfo));
					}
				}
			}

			library = aLibraryInfo;
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo)
		{
			SetLibrary(aLibraryInfo, false);
		}

		#region WizardDBTInfo public properties

		//---------------------------------------------------------------------------
		public WizardLibraryInfo Library { get { return library; } }
		
		//---------------------------------------------------------------------------
		public string Name 
		{
			get { return name; } 
			set 
			{ 
				name = value; 
				
				if (!referenced &&(className == null || className.Length == 0))
					className = GetDefaultDBTClassName(name);
			}
		} 
		
		//---------------------------------------------------------------------------
		public string ClassName
		{
			get { return className; } 
			set 
			{ 
				if (!referenced && (value == null || !Generics.IsValidClassName(value)))
					className = GetDefaultDBTClassName(name);
				else
					className = (value != null) ? value.Trim() : String.Empty; 
			}
		}

		//---------------------------------------------------------------------------
		public string TableName 
		{
			get { return tableName; } 
			set 
			{ 
				if (
					library == null ||
					library.Module == null ||
					library.Module.Application == null ||
					value == null ||
					value.Length == 0
					) 
				{
					tableName = String.Empty;
					if (columns != null)
						columns.Clear();
					return;
				}

				if (
					String.Compare(value, tableName) == 0 ||
					!library.IsTableAvailable(value)
					)
					return;

				tableName = value; 
				
				CheckTableColumns();
			} 
		}
		
		//---------------------------------------------------------------------------
		public DBTType Type { get { return type; } }

		//---------------------------------------------------------------------------
		public bool IsMaster 
		{
			get { return type == DBTType.Master; } 
			set 
			{
				if (value)
					type = DBTType.Master; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool IsSlave 
		{
			get { return type == DBTType.Slave; } 
			set 
			{
				if (value)
					type = DBTType.Slave; 
			} 
		}

		//---------------------------------------------------------------------------
		public bool IsSlaveBuffered 
		{
			get { return type == DBTType.SlaveBuffered; } 
			set 
			{
				if (value)
					type = DBTType.SlaveBuffered; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool OnlyForClientDocumentAvailable 
		{
			get { return onlyForClientDocumentAvailable; } 
			set { onlyForClientDocumentAvailable = value; } 
		}

		//---------------------------------------------------------------------------
		public string ReferencedTableIncludeFile
		{
			get 
			{ 
				WizardTableInfo tableInfo = GetTableInfo();
				if (tableInfo == null || !tableInfo.IsReferenced)
					return String.Empty;

				if (referencedTableIncludeFile != null && referencedTableIncludeFile.Length > 0)
					return referencedTableIncludeFile; 

				string libraryPath = WizardCodeGenerator.GetStandardLibraryPath(tableInfo.Library);
				if (libraryPath == null || libraryPath.Length == 0)
					return String.Empty;

				return libraryPath + Path.DirectorySeparatorChar + tableInfo.ClassName + Generics.CppHeaderExtension;
			}
			set
			{
				WizardTableInfo tableInfo = GetTableInfo();
				if (tableInfo == null || !tableInfo.IsReferenced || value == null)
				{
					referencedTableIncludeFile = String.Empty;
					return;
				}

				string includeFile = value.Trim();

				referencedTableIncludeFile = (includeFile.Length > 0 && Generics.IsValidFullPathName(includeFile)) ? includeFile : String.Empty; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public string MasterTableIncludeFile 
		{
			get 
			{ 
				if (OnlyForClientDocumentAvailable && masterTableIncludeFile != null && masterTableIncludeFile.Length > 0)
					return masterTableIncludeFile; 

				WizardTableInfo masterTableInfo = GetRelatedTableInfo(); 
				if (masterTableInfo == null || masterTableInfo.Library == null)
					return String.Empty;

				string libraryPath = WizardCodeGenerator.GetStandardLibraryPath(masterTableInfo.Library);
				if (libraryPath == null || libraryPath.Length == 0)
					return String.Empty;

				return libraryPath + Path.DirectorySeparatorChar + masterTableInfo.ClassName + Generics.CppHeaderExtension;
			}
			set
			{
				if (!OnlyForClientDocumentAvailable || value == null)
				{
					masterTableIncludeFile = String.Empty;
					return;
				}

				string includeFile = value.Trim();

				masterTableIncludeFile = (includeFile.Length > 0 && Generics.IsValidFullPathName(includeFile)) ? includeFile : String.Empty; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public string ServerDocumentNamespace
		{
			get { return serverDocumentNamespace; } 
			set { serverDocumentNamespace = value; } 
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo RelatedDBTMaster
		{
			get { return relatedDBTMaster; } 
			set 
			{
				if (relatedDBTMaster == value)
					return;
 
				if 
					(
					(!this.IsSlave && !this.IsSlaveBuffered)||
					(value != null && !value.IsMaster)
					)
				{
					relatedDBTMaster = null;
					return;
				}
				
				if 
					(
					columns != null && 
					columns.Count > 0 &&
					relatedDBTMaster != null && 
					(value == null || String.Compare(relatedDBTMaster.TableName, value.TableName) != 0)
					)
				{
					foreach(WizardDBTColumnInfo aColumnInfo in columns)
					{
						aColumnInfo.ForeignKeySegment = false;
						aColumnInfo.ForeignKeyRelatedColumn = String.Empty;
					}
				}
				relatedDBTMaster = value; 
			} 
		}

		//---------------------------------------------------------------------------
		public string SlaveTabTitle
		{
			get 
			{
				if (!this.IsSlave && !this.IsSlaveBuffered)
					return String.Empty; 
				
				if (slaveTabTitle == null || slaveTabTitle.Length == 0)
					return tableName;

				return slaveTabTitle; 
			} 
			set 
			{ 
				if (!this.IsSlave && !this.IsSlaveBuffered)
				{
					slaveTabTitle = String.Empty;
					return;
				}
				
				slaveTabTitle = value;
			}
		}

		//---------------------------------------------------------------------------
		public bool CreateRowForm
		{
			get 
			{
				if (!this.IsSlaveBuffered)
					return false; 
				
				return createRowForm; 
			} 
			set 
			{ 
				if (!this.IsSlaveBuffered)
				{
					createRowForm = false;
					return;
				}
				
				createRowForm = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool ShowsAdditionalColumns
		{
			get 
			{
				if (library == null || columns == null || columns.Count == 0)
					return false;

				WizardTableInfo tableInfo = GetTableInfo();
				if (tableInfo == null)
					return false;

				WizardTableColumnInfoCollection additionalColumnsInfo = library.GetAllAvailableExtraAddedColumns(tableInfo);
				if (additionalColumnsInfo == null || additionalColumnsInfo.Count == 0)
					return false;
			
				foreach(WizardTableColumnInfo anAdditionalColumnInfo in additionalColumnsInfo)
				{
					WizardDBTColumnInfo additionalColumnInfo = GetColumnInfoByName(anAdditionalColumnInfo.Name);
					if (additionalColumnInfo != null && additionalColumnInfo.Visible)
						return true;
				}

				return false; 
			}
		} 
		
		//---------------------------------------------------------------------------
		public WizardDBTColumnInfoCollection ColumnsInfo { get { return columns; } }
		
		//---------------------------------------------------------------------------
		public int ColumnsCount { get { return (columns != null) ? columns.Count : 0; } }

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)

		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 

		#endregion

		#region WizardDBTInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo GetColumnInfoByName(string aColumnName)
		{
			if (columns == null || columns.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return null;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (String.Compare(aColumnName, aColumnInfo.ColumnName, true) == 0)
					return aColumnInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfo(string aTableName)
		{
			if 
				(
				aTableName == null || 
				aTableName.Length == 0 ||
				library == null ||
				library.Module == null ||
				library.Module.Application == null
				)
				return null;

			WizardTableInfo tableInfo = library.GetTableInfoByName(aTableName);
			if (tableInfo != null)
				return tableInfo;
		
			if (library.Dependencies != null && library.Dependencies.Count > 0)
			{
				foreach(WizardLibraryInfo aDependency in library.Dependencies)
				{
					tableInfo = aDependency.GetTableInfoByName(tableName);
					if (tableInfo != null)
						return tableInfo;
				}
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfo()
		{
			return GetTableInfo(tableName);
		}
		
		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetTableColumnInfoByName(string aColumnName, ref int columnIndex)
		{
			WizardTableInfo tableInfo = GetTableInfo();
			if (tableInfo == null)
				return null;

			WizardTableColumnInfo tableColumnInfo = tableInfo.GetColumnInfoByName(aColumnName, ref columnIndex);
			if (tableColumnInfo == null && library != null)
			{
				WizardExtraAddedColumnsInfo addedColumnsInfo = library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace());
				if (addedColumnsInfo != null && addedColumnsInfo.ColumnsCount > 0)
					tableColumnInfo = addedColumnsInfo.ColumnsInfo.GetColumnInfoByName(aColumnName, ref columnIndex);
				if (tableColumnInfo != null && tableInfo.ColumnsInfo.Count > 0)
					columnIndex += tableInfo.ColumnsInfo.Count;
			}
			return tableColumnInfo;
		}
		
		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetTableColumnInfoByName(string aColumnName)
		{
			WizardTableInfo tableInfo = GetTableInfo();
			if (tableInfo == null)
				return null;

			WizardTableColumnInfo tableColumnInfo = tableInfo.GetColumnInfoByName(aColumnName);
			if (tableColumnInfo != null || library == null)
				return tableColumnInfo;
			
			WizardTableColumnInfoCollection additionalColumnsInfo = library.GetAllAvailableExtraAddedColumns(tableInfo.GetNameSpace());
			
			if (additionalColumnsInfo != null && additionalColumnsInfo.Count > 0)
				return additionalColumnsInfo.GetColumnInfoByName(aColumnName);

			return null;
		}

		//---------------------------------------------------------------------------
		public int AddColumnInfo(WizardDBTColumnInfo aColumnInfo)
		{
			if 
				(
				aColumnInfo == null || 
				aColumnInfo.ColumnName == null || 
				aColumnInfo.ColumnName.Length == 0
				)
				return -1;

			if (tableName != null && tableName.Length > 0)
			{
				if 
					(
					library != null &&
					library.Module != null &&
					library.Module.Application != null
					)
				{
					WizardTableInfo tableInfo = GetTableInfo(tableName);
					if
						(
						tableInfo == null ||
						(
						tableInfo.GetColumnInfoByName(aColumnInfo.ColumnName) == null && 
						library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace(), aColumnInfo.ColumnName, true) == null
						)
						)
						return -1;
				}
			}

			WizardDBTColumnInfo alreadyExistingColumn = GetColumnInfoByName(aColumnInfo.ColumnName);
			if (alreadyExistingColumn != null)
				return -1;

			if (columns == null)
				columns = new WizardDBTColumnInfoCollection();

			return columns.Add(aColumnInfo);
		}
	
		//---------------------------------------------------------------------------
		public bool SetColumnInfo(WizardDBTColumnInfo aColumnInfo)
		{
			if (aColumnInfo == null || aColumnInfo.ColumnName == null || aColumnInfo.ColumnName.Length == 0)
				return false;

			if (tableName != null && tableName.Length > 0)
			{
				if 
					(
					library != null &&
					library.Module != null &&
					library.Module.Application != null
					)
				{
					WizardTableInfo tableInfo = GetTableInfo(tableName);
					if (tableInfo == null)
						return false;
					WizardTableColumnInfo tableColumnInfo = tableInfo.GetColumnInfoByName(aColumnInfo.ColumnName);
					if (tableColumnInfo == null)
					{
						WizardExtraAddedColumnsInfo addedColumnsInfo = library.GetExtraAddedColumnInfo(tableInfo.GetNameSpace());
						if (addedColumnsInfo != null && addedColumnsInfo.ColumnsCount > 0)
							tableColumnInfo = addedColumnsInfo.ColumnsInfo.GetColumnInfoByName(aColumnInfo.ColumnName);
					}
					if (tableColumnInfo == null)
						return false;
				}
			}

			WizardDBTColumnInfo alreadyExistingColumn = GetColumnInfoByName(aColumnInfo.ColumnName);
			if (alreadyExistingColumn == null)
				return (AddColumnInfo(aColumnInfo) != -1);

			alreadyExistingColumn.Title = aColumnInfo.Title;
			alreadyExistingColumn.Visible = aColumnInfo.Visible;
			alreadyExistingColumn.Findable = aColumnInfo.Findable;
			alreadyExistingColumn.ForeignKeySegment = aColumnInfo.ForeignKeySegment;
			alreadyExistingColumn.ForeignKeyRelatedColumn = aColumnInfo.ForeignKeyRelatedColumn;
			alreadyExistingColumn.HotKeyLink = aColumnInfo.HotKeyLink;
			alreadyExistingColumn.ShowHotKeyLinkDescription = aColumnInfo.ShowHotKeyLinkDescription;
			alreadyExistingColumn.LabelAdded = aColumnInfo.LabelAdded;
			alreadyExistingColumn.Position.Left = aColumnInfo.Position.Left;
			alreadyExistingColumn.Position.Top = aColumnInfo.Position.Top;
			alreadyExistingColumn.Position.Width = aColumnInfo.Position.Width;
			alreadyExistingColumn.Position.Height = aColumnInfo.Position.Height;
			return true;
		}

		//---------------------------------------------------------------------------
		public void RemoveColumn(string aColumnName)
		{
			if (columns == null || columns.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return;

			WizardDBTColumnInfo columnToRemove = GetColumnInfoByName(aColumnName);
			if (columnToRemove == null)
				return;

			columns.Remove(columnToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllColumns()
		{
			if (columns == null || columns.Count == 0)
				return;

			columns.Clear();
		}
		
		//---------------------------------------------------------------------------
		public WizardTableInfo GetRelatedTableInfo()
		{
			if 
				(
				(!this.IsSlave && !this.IsSlaveBuffered)|| 
				relatedDBTMaster == null ||
				relatedDBTMaster.TableName == null ||
				relatedDBTMaster.TableName.Length == 0
				)
				return null;

			return relatedDBTMaster.GetTableInfo();
		}
		
		//---------------------------------------------------------------------------
		public bool SetForeignKeySegment(string foreignKeySegmentName, string relatedColumnName)
		{
			if (
				foreignKeySegmentName == null ||
				foreignKeySegmentName.Length == 0 ||
				relatedColumnName == null ||
				relatedColumnName.Trim().Length == 0 ||
				(!this.IsSlave && !this.IsSlaveBuffered)|| 
				relatedDBTMaster == null ||
				relatedDBTMaster.TableName == null ||
				relatedDBTMaster.TableName.Length == 0
				)
				return false;

			WizardTableColumnInfo foreignKeySegmentInfo = GetTableColumnInfoByName(foreignKeySegmentName);
			if (foreignKeySegmentInfo == null)
				return false;

			WizardTableColumnInfo relatedTableColumnInfo = relatedDBTMaster.GetTableColumnInfoByName(relatedColumnName);
			if 
				(
				relatedTableColumnInfo == null || 
				relatedTableColumnInfo.DataType.Type != foreignKeySegmentInfo.DataType.Type ||
				relatedTableColumnInfo.DataLength != foreignKeySegmentInfo.DataLength
				)
				return false;

			WizardDBTColumnInfo foreignKeySegment = GetColumnInfoByName(foreignKeySegmentName);
			if (foreignKeySegment == null)
				return false;

			foreignKeySegment.ForeignKeySegment = true;
			foreignKeySegment.ForeignKeyRelatedColumn = relatedColumnName.Trim();

			return true;
		}

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfo GetForeignKeyInfo()
		{
			if (
				(!this.IsSlave && !this.IsSlaveBuffered)|| 
				relatedDBTMaster == null ||
				relatedDBTMaster.TableName == null ||
				relatedDBTMaster.TableName.Length == 0
				)
				return null;

			WizardTableInfo tableInfo = GetTableInfo();
			if (tableInfo == null)
				return null;
			
			WizardTableInfo relatedTableInfo = this.GetRelatedTableInfo();
			if (relatedTableInfo == null)
				return null;

			WizardForeignKeyInfo foreignKeyInfo = new WizardForeignKeyInfo(relatedTableInfo.GetNameSpace());

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if 
					(
					!aColumnInfo.ForeignKeySegment || 
					aColumnInfo.ForeignKeyRelatedColumn == null || 
					aColumnInfo.ForeignKeyRelatedColumn.Trim().Length == 0 ||
					tableInfo.GetColumnInfoByName(aColumnInfo.ColumnName) == null ||
					relatedDBTMaster.GetTableColumnInfoByName(aColumnInfo.ForeignKeyRelatedColumn)== null
					)
					continue;

				foreignKeyInfo.AddKeySegment(new WizardForeignKeyInfo.KeySegment(aColumnInfo.ColumnName, aColumnInfo.ForeignKeyRelatedColumn));
			}

			return (foreignKeyInfo.SegmentsCount > 0) ? foreignKeyInfo : null;
		}

		//---------------------------------------------------------------------------
		public bool IsRelatedTo(WizardDBTInfo aDBTMaster)
		{
			if 
				(
				!(IsSlave || IsSlaveBuffered) ||
				relatedDBTMaster == null || 
				aDBTMaster == null || 
				!aDBTMaster.IsMaster
				)
				return false;

			return relatedDBTMaster.Equals(aDBTMaster);
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkAvailable(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			return (library != null && library.IsHotKeyLinkAvailable(aHotKeyLinkInfo));
		}
		
		//---------------------------------------------------------------------------
		public bool SetHotKeyLink(string aColumnName, WizardHotKeyLinkInfo hotKeyLinkInfo, bool showDescription)
		{
			if (
				aColumnName == null ||
				aColumnName.Length == 0 ||
				hotKeyLinkInfo == null ||
				!hotKeyLinkInfo.IsDefined ||
				!IsHotKeyLinkAvailable(hotKeyLinkInfo)
				)
				return false;

			WizardTableColumnInfo tableColumnInfo = GetTableColumnInfoByName(aColumnName);
			if 
				(
				tableColumnInfo == null ||
				tableColumnInfo.DataType.Type != hotKeyLinkInfo.CodeColumn.DataType.Type ||
				tableColumnInfo.DataLength != hotKeyLinkInfo.CodeColumn.DataLength
				)
				return false;
			
			WizardDBTColumnInfo columnInfo = GetColumnInfoByName(aColumnName);
			if (columnInfo == null)
				return false;

			columnInfo.HotKeyLink = hotKeyLinkInfo;
			columnInfo.ShowHotKeyLinkDescription = showDescription;

			return true;
		}

		//---------------------------------------------------------------------------
		public WizardLibraryInfo GetHotKeyLinkLibrary(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (library == null || aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return null;

			return library.GetHotKeyLinkLibrary(aHotKeyLinkInfo);
		}

		//---------------------------------------------------------------------------
		public object GetHotKeyLinkParent(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (library == null || aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return null;

			return library.GetHotKeyLinkParent(aHotKeyLinkInfo);
		}

		//---------------------------------------------------------------------------
		public bool HasHotLinkColumnsWithDescription()
		{
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.ShowHotKeyLinkDescription)
					return true;
			}
			
			return false;
		}
		
		//---------------------------------------------------------------------------
		public static string GetDefaultDBTClassName(string aDBTName)
		{
			if (aDBTName == null || aDBTName.Trim().Length == 0)
				return String.Empty;

			return "DBT" + Generics.SubstitueInvalidCharacterInIdentifier(aDBTName.Trim().Replace(' ', '_'));
		}
		
		//---------------------------------------------------------------------------
		public bool HasVisibleColums()
		{
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.Visible)
					return true;
			}
			
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasFindableColums()
		{
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.Findable)
					return true;
			}
			
			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool HasHKLDefinedColumns()
		{ 
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.IsHKLDefined)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (columns == null || columns.Count == 0 || aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			return columns.IsHotKeyLinkUsed(aHotKeyLinkInfo);
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedControlIdsCount()
		{
			if (columns == null || columns.Count == 0)
				return 0;

			// Se il DBT è di tipo SlaveBuffered si deve anche tenere conto del bodyEdit
			ushort controlsCount = (ushort)(IsSlaveBuffered ? 1 : 0);

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.Visible)
				{
					controlsCount++;
					if (aColumnInfo.ShowHotKeyLinkDescription)
						controlsCount++;
				}
				
				// Se si tratta di uno SlaveBuffered e viene anche creata la finestra di 
				// dettaglio sulla riga devo anche contare i control contenuti in tale
				// finestra: in essa vengono gestiti tutti i campi della tabella alla 
				// quale è riferito il DBT (compresi quelli che non sono visibili, cioè 
				// per i quali non è stata inserita alcuna colonna corrispondente nel
				// BodyEdit), ma vengono comunque scartati i segmenti di chiave esterna 
				// utilizzati dal questo DBT per "agganciarsi" alla tabella master
				if (CreateRowForm && !aColumnInfo.ForeignKeySegment)
					controlsCount++;
			}

			return controlsCount; 
		}
		
		//---------------------------------------------------------------------------
		public void CheckTableColumns()
		{
			if (
				library == null ||
				library.Module == null ||
				library.Module.Application == null ||
				tableName == null ||
				tableName.Length == 0				)
				return;

			WizardTableInfo tableInfo = GetTableInfo(tableName);
			if (tableInfo == null || tableInfo.ColumnsCount == 0)
			{
				if (columns != null)
					columns.Clear();
				return;
			}

			WizardTableColumnInfoCollection additionalColumnsInfo = library.GetAllAvailableExtraAddedColumns(tableInfo);
			
			// Se il DBT contiene delle impostazioni relative a colonne della tabella
			// verifico che tali impostazioni siano effettivamente riferite a colonne
			// tuttora esistenti, altrimenti le cancello. Infatti, dopo aver definito
			// il DBT alcune colonne della tabella potrebbero anche state poi rimosse.
			if (columns != null && columns.Count > 0)
			{
				// Prima controllo se nella tabella sono state eliminate delle colonne
				ArrayList invalidColumns = new ArrayList();
				foreach(WizardDBTColumnInfo aColumnInfo in columns)
				{
					WizardTableColumnInfo tableColumn = tableInfo.GetColumnInfoByName(aColumnInfo.ColumnName);
					if (tableColumn == null && additionalColumnsInfo != null && additionalColumnsInfo.Count > 0)
							tableColumn = additionalColumnsInfo.GetColumnInfoByName(aColumnInfo.ColumnName);

					if (tableColumn == null)
						invalidColumns.Add(aColumnInfo.ColumnName);
				}
				if (invalidColumns.Count > 0)
				{
					foreach(string aColumnNameToRemove in invalidColumns)
						this.RemoveColumn(aColumnNameToRemove);
				}
			}

			// il metodo AddColumnInfo aggiunge le impostazioni per una colonna
			// se e soltanto se queste non sono già presenti. Pertanto, richiamandolo
			// su tutte le colonne della tabella non rischio di cancellare delle
			// impostazioni inserite precedentemente.
			foreach(WizardTableColumnInfo aColumnInfo in tableInfo.ColumnsInfo)
				this.AddColumnInfo(new WizardDBTColumnInfo(aColumnInfo));

			if (additionalColumnsInfo != null && additionalColumnsInfo.Count > 0)
			{
				foreach(WizardTableColumnInfo anAdditionalColumnInfo in additionalColumnsInfo)
					this.AddColumnInfo(new WizardDBTColumnInfo(anAdditionalColumnInfo));
			}
		}

		//---------------------------------------------------------------------------
		public bool HasVisibleAdditionalColumns(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			if 
				(
				library == null || 
				columns == null || 
				columns.Count == 0 ||
				aExtraAddedColumnsInfo == null ||
				aExtraAddedColumnsInfo.Library == null ||
				(
				String.Compare(aExtraAddedColumnsInfo.Library.GetNameSpace(), library.GetNameSpace()) != 0 && 
				!library.DependsOn(aExtraAddedColumnsInfo.Library)
				) ||
				aExtraAddedColumnsInfo.ColumnsCount == 0 ||
				String.Compare(aExtraAddedColumnsInfo.TableName, tableName, true) != 0
				)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (!aColumnInfo.Visible)
					continue;

				WizardTableColumnInfo tableColumn = aExtraAddedColumnsInfo.GetColumnInfoByName(aColumnInfo.ColumnName);
				if (tableColumn != null)
					return true;
			}
			
			return false; 
		} 
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection GetAllTabbedPanes()
		{
			if (library == null)
				return null;

			if (library.Module == null)
				return library.GetAllDBTTabbedPanes(this);

			if (library.Module.Application == null)
				return library.Module.GetAllDBTTabbedPanes(this);

			return library.Module.Application.GetAllDBTTabbedPanes(this);
		}
		
		#endregion
	}

	#endregion

	#region WizardDBTInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardDBTInfoCollection.
	/// </summary>
	public class WizardDBTInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardDBTInfo))
					throw new NotSupportedException();

				this[index] = (WizardDBTInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDBTInfo))
				throw new NotSupportedException();

			return this.Contains((WizardDBTInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardDBTInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDBTInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardDBTInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDBTInfo))
				throw new NotSupportedException();

			Insert(index, (WizardDBTInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardDBTInfo))
				throw new NotSupportedException();

			Remove((WizardDBTInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public WizardDBTInfo this[int index]
		{
			get {  return (WizardDBTInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardDBTInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo[] ToArray()
		{
			return (WizardDBTInfo[])InnerList.ToArray(typeof(WizardDBTInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardDBTInfo aDBTToAdd)
		{
			if (Contains(aDBTToAdd))
				return IndexOf(aDBTToAdd);

			return InnerList.Add(aDBTToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardDBTInfoCollection aColumnsCollectionToAdd)
		{
			if (aColumnsCollectionToAdd == null || aColumnsCollectionToAdd.Count == 0)
				return;

			foreach (WizardDBTInfo aDBTToAdd in aColumnsCollectionToAdd)
				Add(aDBTToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardDBTInfo aDBTToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aDBTToInsert))
				return;

			InnerList.Insert(index, aDBTToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardDBTInfo beforeDBT, WizardDBTInfo aDBTToInsert)
		{
			if (beforeDBT == null)
				Add(aDBTToInsert);

			if (!Contains(beforeDBT))
				return;

			if (Contains(aDBTToInsert))
				return;

			Insert(IndexOf(beforeDBT), aDBTToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardDBTInfo aDBTToRemove)
		{
			if (!Contains(aDBTToRemove))
				return;

			InnerList.Remove(aDBTToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardDBTInfo aDBTToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aDBTToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardDBTInfo aDBTToSearch)
		{
			if (!Contains(aDBTToSearch))
				return -1;
			
			return InnerList.IndexOf(aDBTToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDBTInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardDBTInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardDBTInfo.Equals(this[i], ((WizardDBTInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion

	#region WizardDBTColumnInfo class

	//=================================================================================
	/// <summary>
	/// Position of controls 
	/// </summary>
	public class Position
	{
		private bool setTop;
		private bool setLeft;
		private bool setWidth;
		private bool setHeight;

		private int top;
		private int left;
		private int width;
		private int height;

		public int Top { get { return top; } set { setTop = true;  top = value; } }
		public int Left { get { return left; } set { setLeft = true; left = value; } }
		public int Width { get { return width; } set { setWidth = true; width = value; } }
		public int Height { get { return height; } set { setHeight = true; height = value; } }
		public bool isSet { get { return (setTop && setLeft && setWidth && setHeight && width > 0 && height > 0); } }
		
		//---------------------------------------------------------------------------
		public Position()
		{
			this.top = this.left = this.Width = this.Height = -1;
			setTop = setLeft = setWidth = setHeight = false;
		}

		//---------------------------------------------------------------------------
		public Position(int left, int top, int width, int height)
			: this()
		{
			this.setTop = this.setLeft = this.setWidth = this.setHeight = true;
			this.top = top;
			this.left = left;
			this.width = width;
			this.height = height;
		}

		//---------------------------------------------------------------------------
		public Position(Position position)
		{
			this.setTop = this.setLeft = this.setWidth = this.setHeight = true;
			this.top = position.Top;
			this.left = position.Left;
			this.width = position.Width;
			this.height = position.Height;
		}

		//---------------------------------------------------------------------------
		public void SetPosition(int left, int top, int width, int height)
		{
			this.setTop = this.setLeft = this.setWidth = this.setHeight = true;
			this.top = top;
			this.left = left;
			this.width = width;
			this.height = height;
		}
	}

	//=================================================================================
	public class LabelInfo
	{
		private string label;
		private Position position;
		private string guidId;
		private WizardDBTColumnInfo dbtColumInfo;

		public string Label { get { return label; } set { label = value; } }
		public WizardDBTColumnInfo GetDbtColumInfo { get { return dbtColumInfo; } }
		public Position Position {	get { return position; }set { position = value; }}
		public string GuidID { get { return guidId; }}

		//---------------------------------------------------------------------------
		public LabelInfo(string label, Position pos)
		{
			this.label = label;
			this.dbtColumInfo = null;
			this.guidId = Guid.NewGuid().ToString();
			this.position = new Position(pos);
		}

		//---------------------------------------------------------------------------
		public LabelInfo(string label, int left, int top, int width, int height)
		{
			this.label = label;
			this.dbtColumInfo = null;
			this.guidId = Guid.NewGuid().ToString();
			this.position = new Position(left, top, width, height);
		}

		//---------------------------------------------------------------------------
		public LabelInfo(WizardDBTColumnInfo dbtColumInfo, int top, int left, int width, int height) :
			this(dbtColumInfo.Title, left, top, width, height)
		{
			this.dbtColumInfo = dbtColumInfo;
		}

		//---------------------------------------------------------------------------
		public LabelInfo(LabelInfo labelInfo)
		{
			this.label = labelInfo.label;
			this.dbtColumInfo = labelInfo.dbtColumInfo;
			this.guidId = labelInfo.guidId;
			this.position = new Position(labelInfo.position);
		}
	}

	//=================================================================================
	/// <summary>
	/// Summary description for WizardDBTColumnInfo.
	/// </summary>
	public class WizardDBTColumnInfo : IDisposable
	{
		public const uint DefaultStringColumnLength = 10;

		#region WizardDBTColumnInfo private data members

		private WizardTableColumnInfo	tableColumn = null;
		private string					title = String.Empty;
		private bool					visible = false;
		private bool					findable = false;
		private bool					foreignKeySegment = false;
		private string					foreignKeyRelatedColumn = String.Empty;
		private WizardHotKeyLinkInfo	hotKeyLink = null;
		private bool					showHotKeyLinkDescription = false;
		
		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool disposed = false;
		private bool labelAdded = false;

		private Position fieldPosition;
		private const int maxControlVisibleCharNumber = 36;
		#endregion

		public Position Position { get { return fieldPosition; } set { fieldPosition = value; } }
		public bool LabelAdded { get { return labelAdded; }	set { labelAdded = value; }	}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo(WizardTableColumnInfo aTableColumn, bool isReadOnly)
		{
			labelAdded = false;
			tableColumn = aTableColumn;
			readOnly = isReadOnly;
			fieldPosition = new Position();
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo(WizardTableColumnInfo aTableColumn) : this(aTableColumn, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo(WizardDBTColumnInfo aColumnInfo)
		{
			tableColumn = (aColumnInfo != null) ? aColumnInfo.TableColumn : null;
			title = (aColumnInfo != null) ? aColumnInfo.Title : String.Empty;
			visible = (aColumnInfo != null) ? aColumnInfo.Visible : false;
			findable = (aColumnInfo != null) ? aColumnInfo.Findable : false;
			foreignKeySegment = (aColumnInfo != null) ? aColumnInfo.ForeignKeySegment : false;
			foreignKeyRelatedColumn = (aColumnInfo != null) ? aColumnInfo.ForeignKeyRelatedColumn : String.Empty;
			hotKeyLink = (aColumnInfo != null && aColumnInfo.HotKeyLink != null) ? new WizardHotKeyLinkInfo(aColumnInfo.HotKeyLink) : null;
			showHotKeyLinkDescription =  (aColumnInfo != null ) ? aColumnInfo.ShowHotKeyLinkDescription : false;
			readOnly = (aColumnInfo != null) ? aColumnInfo.ReadOnly : false;
			labelAdded = aColumnInfo.LabelAdded;
			if (aColumnInfo.Position.isSet)
				Position = new Position(aColumnInfo.Position);
			else
				Position = new Position();
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDBTColumnInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				WizardTableColumnInfo.Equals(tableColumn, ((WizardDBTColumnInfo)obj).TableColumn) &&
				String.Compare(title, ((WizardDBTColumnInfo)obj).Title) == 0 &&
				visible == ((WizardDBTColumnInfo)obj).Visible &&
				findable == ((WizardDBTColumnInfo)obj).Findable &&
				foreignKeySegment == ((WizardDBTColumnInfo)obj).ForeignKeySegment &&
				String.Compare(foreignKeyRelatedColumn, ((WizardDBTColumnInfo)obj).ForeignKeyRelatedColumn) == 0 &&
				showHotKeyLinkDescription == ((WizardDBTColumnInfo)obj).ShowHotKeyLinkDescription &&
				WizardHotKeyLinkInfo.Equals(hotKeyLink, ((WizardDBTColumnInfo)obj).HotKeyLink) 
				);
		}
		
		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		#region WizardDBTColumnInfo public properties

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo TableColumn { get { return tableColumn; } }
		//---------------------------------------------------------------------------
		public string ColumnName { get { return (tableColumn != null) ? tableColumn.Name : String.Empty; } }
		//---------------------------------------------------------------------------
		public WizardTableColumnDataType ColumnDataType { get { return (tableColumn != null) ? tableColumn.DataType : null; } }
		//---------------------------------------------------------------------------
		public string Title { get { return title; } set { title = value; } }
		//---------------------------------------------------------------------------
		public bool CanBeVisible 
		{
			get 
			{ 
				return (tableColumn != null);
			} 
		}

		//---------------------------------------------------------------------------
		public bool Visible 
		{
			get 
			{ 
				if (!CanBeVisible)
					return false;

				return this.visible; 
			} 
			set 
			{ 
				if (!CanBeVisible)
				{
					visible = false; 
					return;
				}
				
				visible = value; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool Findable { get { return findable; } set { findable = value; } }
		//---------------------------------------------------------------------------
		public bool ForeignKeySegment { get { return foreignKeySegment; } set { foreignKeySegment = value; } }
		//---------------------------------------------------------------------------
		public string ForeignKeyRelatedColumn 
		{ 
			get { return foreignKeySegment ? foreignKeyRelatedColumn : String.Empty; } 
			set 
			{ 
				if (!foreignKeySegment)
				{
					foreignKeyRelatedColumn = String.Empty;
					return;
				}

				foreignKeyRelatedColumn = value; 
			} 
		}

		//---------------------------------------------------------------------
		public bool IsLabelVisible
		{
			get 
			{
				return 
					(
					tableColumn != null && 
					tableColumn.DataType.Type != WizardTableColumnDataType.DataType.Undefined &&
					tableColumn.DataType.Type != WizardTableColumnDataType.DataType.Boolean // CheckBox
					);
			}
		}
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo HotKeyLink 
		{
			get 
			{
				return CanUseHKL ? hotKeyLink : null; 
			} 
			set 
			{ 
				if (!CanUseHKL)
				{
					hotKeyLink = null;
					return;
				}

				if (hotKeyLink == value)
					return;

				hotKeyLink = value;
			}
		}

		//---------------------------------------------------------------------------
		public bool CanUseHKL
		{ 
			get 
			{
				// Non voglio consentire l'associazione di un HotLink qualora il 
				// campo sia di tipo enumerativo o non specificato
				return 
					(
					tableColumn != null &&
					tableColumn.DataType.Type != WizardTableColumnDataType.DataType.Undefined &&
					tableColumn.DataType.Type != WizardTableColumnDataType.DataType.Enum 
					); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool CanShowHotKeyLinkDescription 
		{ 
			get 
			{
				return 
					(
					CanUseHKL && 
					hotKeyLink != null && 
					!hotKeyLink.IsReferenced && 
					hotKeyLink.IsDefined
					); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool ShowHotKeyLinkDescription 
		{ 
			get 
			{
				return (CanShowHotKeyLinkDescription && showHotKeyLinkDescription); 
			} 
			set 
			{ 
				showHotKeyLinkDescription = CanShowHotKeyLinkDescription ? value : false;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsHKLDefined 
		{ 
			get 
			{
				return (CanUseHKL && hotKeyLink != null && hotKeyLink.IsDefined); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public string HKLExternalIncludeFile 
		{ 
			get 
			{
				return IsHKLDefined ? hotKeyLink.ExternalIncludeFile : String.Empty; 
			} 
		}

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		#endregion

		#region WizardDBTColumnInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return ColumnName;
		}

		//---------------------------------------------------------------------
		public string GetDataObjClassName()
		{
			if (tableColumn == null)
				return String.Empty;

			return tableColumn.GetDataObjClassName();
		}

		//---------------------------------------------------------------------
		public string GetColumnControlName()
		{
			if (tableColumn == null)
				return String.Empty;

			return WizardTableColumnDataType.GetDataTypeControlName(tableColumn.DataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetColumnParsedControlClassName()
		{
			if (tableColumn == null)
				return String.Empty;

			return WizardTableColumnDataType.GetDataTypeParsedControlClassName(tableColumn.DataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetColumnParsedStaticClassName()
		{
			if (tableColumn == null)
				return String.Empty;

			return WizardTableColumnDataType.GetDataTypeParsedStaticClassName(tableColumn.DataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetColumnControlStyles()
		{
			if (tableColumn == null)
				return String.Empty;

			bool setMultilineStyle =
				(
				(tableColumn.DataType.Type == WizardTableColumnDataType.DataType.String && tableColumn.DataLength > maxControlVisibleCharNumber) ||
				tableColumn.DataType.Type == WizardTableColumnDataType.DataType.Text ||
				tableColumn.DataType.Type == WizardTableColumnDataType.DataType.NText
				);

			return WizardTableColumnDataType.GetColumnControlStyles(tableColumn.DataType.Type, setMultilineStyle);
		}

		//---------------------------------------------------------------------
		public static System.Drawing.Size GetStringColumnControlDefaultSize(WizardTableColumnInfo aTableColumnInfo, string fontFamilyName, float fontEmSize)
		{
			if (aTableColumnInfo == null || !aTableColumnInfo.DataType.IsTextual)
				return Size.Empty;
		
			StringBuilder maxInputStringBuilder = new StringBuilder();
			maxInputStringBuilder.Append('W', maxControlVisibleCharNumber);
			int maxDefaultControlWidth = Generics.GDI32.GetTextDisplaySize(maxInputStringBuilder.ToString(), fontFamilyName, fontEmSize).Width;

			System.Drawing.Size defaultSize = Size.Empty;
			string controlText = String.Empty;
			
			StringBuilder sb = new StringBuilder();
			sb.Append('W', (int)aTableColumnInfo.GetWoormDefaultDataLength());
			controlText = sb.ToString();
			defaultSize = Generics.GDI32.GetTextDisplaySize(controlText, fontFamilyName, fontEmSize);

			if (defaultSize.Width > maxDefaultControlWidth)
			{
				defaultSize.Width = maxDefaultControlWidth;

                if (aTableColumnInfo.DataType.Type == WizardTableColumnDataType.DataType.String)
                {
                    defaultSize.Height  = (int)(defaultSize.Height * ((aTableColumnInfo.DataLength / maxControlVisibleCharNumber) + 1)); // su più righe
                    defaultSize.Height += (int)(2 * (aTableColumnInfo.DataLength / maxControlVisibleCharNumber)); 
                }
                else
                    defaultSize.Height = (int)(defaultSize.Height * 2 + 2); // su due righe
			}

			defaultSize.Height += 2;

			return defaultSize;
		}
	
		//---------------------------------------------------------------------
		public System.Drawing.Size GetColumnControlDefaultSize(string fontFamilyName, float fontEmSize)
		{
			if (tableColumn == null || tableColumn.DataType.Type == WizardTableColumnDataType.DataType.Undefined)
				return Size.Empty;

			if (tableColumn.DataType.IsTextual)
				return GetStringColumnControlDefaultSize(tableColumn, fontFamilyName, fontEmSize);
		
			StringBuilder maxInputStringBuilder = new StringBuilder();
			maxInputStringBuilder.Append('W', maxControlVisibleCharNumber);
			int maxDefaultControlWidth = Generics.GDI32.GetTextDisplaySize(maxInputStringBuilder.ToString(), fontFamilyName, fontEmSize).Width;

			System.Drawing.Size defaultSize = Size.Empty;
			if (tableColumn.DataType.Type == WizardTableColumnDataType.DataType.Boolean)
			{
				defaultSize = Generics.GDI32.GetTextDisplaySize(title, fontFamilyName, fontEmSize);
			
				defaultSize.Width += 8; // devo aggiungere lo spazio per il quadratino della checkbox
				if (defaultSize.Width > maxDefaultControlWidth)
					defaultSize.Width = maxDefaultControlWidth;
			}
			else if (tableColumn.DataType.Type == WizardTableColumnDataType.DataType.Enum)
			{
				if (tableColumn.EnumInfo != null)
				{
					// Prendo la stringa più lunga
					string maxEnumItemName = String.Empty;
					foreach(WizardEnumItemInfo enumItem in tableColumn.EnumInfo.ItemsInfo)
					{
						if (enumItem.Name.Length > maxEnumItemName.Length)
							maxEnumItemName = enumItem.Name;
					}
					defaultSize = Generics.GDI32.GetTextDisplaySize(maxEnumItemName.ToUpper(), fontFamilyName, fontEmSize);
			
					defaultSize.Width += 8; // devo aggiungere lo spazio per il pulsante della combobox
					if (defaultSize.Width > maxDefaultControlWidth)
						defaultSize.Width = maxDefaultControlWidth;

					defaultSize.Height += 4;
				}
			}
			else // TextBox
			{
				string controlText = String.Empty;
				
				StringBuilder sb = new StringBuilder();
				sb.Append('W', (int)tableColumn.GetWoormDefaultDataLength());
				controlText = sb.ToString();
				defaultSize = Generics.GDI32.GetTextDisplaySize(controlText, fontFamilyName, fontEmSize);

				if (defaultSize.Width > maxDefaultControlWidth)
				{
					defaultSize.Width = maxDefaultControlWidth;

					if (tableColumn.DataType.Type == WizardTableColumnDataType.DataType.Text || tableColumn.DataType.Type == WizardTableColumnDataType.DataType.NText)
						defaultSize.Height = defaultSize.Height * 2 + 2; // su due righe
				}

				defaultSize.Height += 2;
			}

			return defaultSize;
		}

		//---------------------------------------------------------------------
		public int GetDefaultDataLength()
		{
			if (tableColumn == null || tableColumn.DataType.Type == WizardTableColumnDataType.DataType.Undefined)
				return -1;
			return (int)tableColumn.GetWoormDefaultDataLength();
		}
	
		//---------------------------------------------------------------------
		public int GetColumnControlDefaultWidth(string fontFamilyName, float fontEmSize)
		{
			return GetColumnControlDefaultSize(fontFamilyName, fontEmSize).Width;
		}
	
		//---------------------------------------------------------------------
		public int GetColumnControlDefaultHeight(string fontFamilyName, float fontEmSize)
		{
			return GetColumnControlDefaultSize(fontFamilyName, fontEmSize).Height;
		}

		#endregion

	}
	
	#endregion

	#region WizardDBTColumnInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardDBTColumnInfoCollection.
	/// </summary>
	public class WizardDBTColumnInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardDBTColumnInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardDBTColumnInfo))
					throw new NotSupportedException();

				this[index] = (WizardDBTColumnInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDBTColumnInfo))
				throw new NotSupportedException();

			return this.Contains((WizardDBTColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardDBTColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDBTColumnInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardDBTColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDBTColumnInfo))
				throw new NotSupportedException();

			Insert(index, (WizardDBTColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardDBTColumnInfo))
				throw new NotSupportedException();

			Remove((WizardDBTColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo this[int index]
		{
			get {  return (WizardDBTColumnInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardDBTColumnInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo[] ToArray()
		{
			return (WizardDBTColumnInfo[])InnerList.ToArray(typeof(WizardDBTColumnInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardDBTColumnInfo aColumnToAdd)
		{
			if (Contains(aColumnToAdd))
				return IndexOf(aColumnToAdd);

			return InnerList.Add(aColumnToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardDBTColumnInfoCollection aColumnsCollectionToAdd)
		{
			if (aColumnsCollectionToAdd == null || aColumnsCollectionToAdd.Count == 0)
				return;

			foreach (WizardDBTColumnInfo aColumnToAdd in aColumnsCollectionToAdd)
				Add(aColumnToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardDBTColumnInfo aColumnToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aColumnToInsert))
				return;

			InnerList.Insert(index, aColumnToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardDBTColumnInfo beforeColumn, WizardDBTColumnInfo aColumnToInsert)
		{
			if (beforeColumn == null)
				Add(aColumnToInsert);

			if (!Contains(beforeColumn))
				return;

			if (Contains(aColumnToInsert))
				return;

			Insert(IndexOf(beforeColumn), aColumnToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardDBTColumnInfo aColumnToRemove)
		{
			if (!Contains(aColumnToRemove))
				return;

			InnerList.Remove(aColumnToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardDBTColumnInfo aColumnToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aColumnToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardDBTColumnInfo aColumnToSearch)
		{
			if (!Contains(aColumnToSearch))
				return -1;
			
			return InnerList.IndexOf(aColumnToSearch);
		}
	
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDBTColumnInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardDBTColumnInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardDBTColumnInfo.Equals(this[i], ((WizardDBTColumnInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	
		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo GetColumnInfoByName(string aColumnName, ref int columnIndex)
		{
			if (this.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return null;

			aColumnName = aColumnName.Trim();
			if (aColumnName.Length == 0 || !Generics.IsValidTableColumnName(aColumnName))
				return null;

			for(int i=0; i < this.Count; i++)
			{
				if (String.Compare(aColumnName, this[i].ColumnName, true) == 0)
				{
					columnIndex = i;
					return this[i];
				}
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo GetColumnInfoByName(string aColumnName)
		{
			int columnIndex = -1;
			return GetColumnInfoByName(aColumnName, ref columnIndex);
		}
	
		//---------------------------------------------------------------------------
		public bool HasSameColumnNames(WizardDBTColumnInfoCollection columnsToCompare)
		{
			if (columnsToCompare == null)
				return (this.Count == 0);

			if (this.Count != columnsToCompare.Count)
				return false;

			for (int i = 0; i < this.Count; i++)
			{
				if (columnsToCompare.GetColumnInfoByName(this[i].ColumnName) == null)
					return false; // non c'è una colonna con lo stesso nome
			}

			return true;
		}
	
		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (this.Count == 0 || aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			for(int i=0; i < this.Count; i++)
			{
				if (this[i].IsHKLDefined && WizardHotKeyLinkInfo.Equals(this[i].HotKeyLink,aHotKeyLinkInfo))
					return true;
			}

			return false;
		}
	}

	#endregion

	#region WizardEnumInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardEnumInfo.
	/// </summary>
	public class WizardEnumInfo : IDisposable
	{
		#region WizardEnumInfo private data members

		private WizardModuleInfo module = null; // parent
		
		private string	name = String.Empty;
		private ushort	enumValue = 0;

		private WizardEnumItemInfoCollection items = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		#endregion

		//============================================================================
		internal class WizardEnumInfoNameSorter : IComparer
		{
			public int Compare(object x, object y)
			{
				Debug.Assert(x != null && x is WizardEnumInfo);
				Debug.Assert(y != null && y is WizardEnumInfo);

				WizardEnumInfo enum1 = (WizardEnumInfo)x;
				WizardEnumInfo enum2 = (WizardEnumInfo)y;

				return String.Compare(enum1.Name,enum2.Name);
			}
		}
		
		//============================================================================
		internal class WizardEnumInfoValueSorter : IComparer
		{
			public int Compare(object x, object y)
			{
				Debug.Assert(x != null && x is WizardEnumInfo);
				Debug.Assert(y != null && y is WizardEnumInfo);

				WizardEnumInfo enum1 = (WizardEnumInfo)x;
				WizardEnumInfo enum2 = (WizardEnumInfo)y;

				return (int)enum1.Value - (int)enum2.Value;
			}
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo(string aEnumName, ushort aEnumValue, bool isReadOnly, bool isReferenced)
		{
			name = aEnumName;
			enumValue = aEnumValue;
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo(string aEnumName, ushort aEnumValue, bool isReadOnly) : this(aEnumName, aEnumValue, isReadOnly, false)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardEnumInfo(string aEnumName, ushort aEnumValue) : this(aEnumName, aEnumValue, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo(WizardEnumInfo aEnumInfo)
		{
			name = (aEnumInfo != null) ? aEnumInfo.Name : String.Empty;
			enumValue = (aEnumInfo != null) ? aEnumInfo.Value : (ushort)0;
			readOnly = (aEnumInfo != null) ? aEnumInfo.ReadOnly : false;
			referenced = (aEnumInfo != null) ? aEnumInfo.IsReferenced : false;

			if (aEnumInfo != null && aEnumInfo.ItemsCount > 0)
			{
				foreach(WizardEnumItemInfo aItemInfo in aEnumInfo.ItemsInfo)
					this.AddItemInfo(new WizardEnumItemInfo(aItemInfo));
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardEnumInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardEnumInfo)obj).Name) == 0 &&
				enumValue == ((WizardEnumInfo)obj).Value &&
				WizardEnumItemInfoCollection.Equals(items,((WizardEnumInfo)obj).ItemsInfo)
				);
		}
		
		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetModule(WizardModuleInfo aModuleInfo)
		{
			if (module == aModuleInfo)
				return;

			if (module != null && module.EnumsInfo.Contains(this))
				module.EnumsInfo.Remove(this);

			module = aModuleInfo;
		}
		
		#region WizardEnumInfo public properties

		//---------------------------------------------------------------------------
		public WizardModuleInfo Module { get { return module; } }
		//---------------------------------------------------------------------------
		public WizardApplicationInfo Application { get { return (module != null) ? module.Application : null; } }
		
		//---------------------------------------------------------------------------
		public string Name { get { return name; } set { if (Generics.IsValidEnumName(value)) name = value; } }
		//---------------------------------------------------------------------------
		public ushort Value { get { return enumValue; } set { if (Generics.IsValidEnumValue(value)) enumValue = value; } }
		//---------------------------------------------------------------------------
		public WizardEnumItemInfoCollection ItemsInfo { get { return items; } }
		//---------------------------------------------------------------------------
		public int ItemsCount { get { return items.Count; } }

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo DefaultItem
		{
			get 
			{ 
				if (items == null || items.Count == 0)
					return null;

				foreach(WizardEnumItemInfo aItemInfo in items)
				{
					if (aItemInfo.IsDefaultItem)
						return aItemInfo;
				}
				return items[0];
			} 
			set 
			{ 
				if (items == null || items.Count == 0)
					return;

				foreach(WizardEnumItemInfo aItemInfo in items)
				{
					aItemInfo.IsDefaultItem = aItemInfo.Equals(value);
				}
			} 
		}

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 
		
		#endregion

		#region WizardEnumInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return name;
		}

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo GetItemInfoByName(string aItemName)
		{
			if (items == null || items.Count == 0 || !Generics.IsValidEnumItemName(aItemName))
				return null;

			foreach(WizardEnumItemInfo aItemInfo in items)
			{
				if (String.Compare(aItemName, aItemInfo.Name) == 0)
					return aItemInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo GetItemInfoByValue(ushort aItemValue)
		{
			if (items == null || items.Count == 0 || !Generics.IsValidEnumItemValue(aItemValue))
				return null;

			foreach(WizardEnumItemInfo aItemInfo in items)
			{
				if (aItemValue == aItemInfo.Value)
					return aItemInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public uint GetItemStoredValue(WizardEnumItemInfo aItem)
		{
			if (items == null || items.Count == 0 || aItem == null || !items.Contains(aItem))
				return 0;

			return (uint)enumValue * 65536 + aItem.Value;
		}
		
		//---------------------------------------------------------------------------
		public uint GetItemStoredValue(string aItemName)
		{
			return GetItemStoredValue(GetItemInfoByName(aItemName));
		}

		//---------------------------------------------------------------------------
		public int AddItemInfo(WizardEnumItemInfo aItemInfo)
		{
			if 
				(
				aItemInfo == null || 
				aItemInfo.Name == null || 
				aItemInfo.Name.Length == 0
				)
				return -1;

			WizardEnumItemInfo alreadyExistingItem = GetItemInfoByName(aItemInfo.Name);
			if (alreadyExistingItem != null)
				return -1;

			if (items == null)
				items = new WizardEnumItemInfoCollection();

			return items.Add(aItemInfo);
		}
	
		//---------------------------------------------------------------------------
		public void RemoveItem(string aItemName)
		{
			if (items == null || items.Count == 0 || aItemName == null || aItemName.Length == 0)
				return;

			WizardEnumItemInfo itemToRemove = GetItemInfoByName(aItemName);
			if (itemToRemove == null)
				return;

			items.Remove(itemToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllItems()
		{
			if (items == null || items.Count == 0)
				return;

			items.Clear();
		}

		//---------------------------------------------------------------------------
		public ushort GetMaxItemValue()
		{
			if (items == null || items.Count == 0)
				return 0;
			
			ushort maxItemValue = 0;
			foreach(WizardEnumItemInfo aItemInfo in items)
			{
				if (maxItemValue < aItemInfo.Value)
					maxItemValue = aItemInfo.Value;
			}

			return maxItemValue;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetNextValidItemValue()
		{
			if (items == null || items.Count == 0)
				return 0;
			
			return (ushort)(GetMaxItemValue() + 1);
		}

		//---------------------------------------------------------------------------
		public bool HasSameItems(WizardEnumItemInfoCollection itemsToCompare)
		{
			if (items == null)
				return (itemsToCompare == null || itemsToCompare.Count == 0);

			if (itemsToCompare == null)
				return (items.Count == 0);

			if (items.Count != itemsToCompare.Count)
				return false;

			for (int i = 0; i < items.Count; i++)
			{
				int j = 0;
				// Cerco nella seconda collection un elemento con lo stesso valore:
				for (j = 0; j < itemsToCompare.Count; j++)
				{
					if (items[i].Value == itemsToCompare[j].Value)
					{
						if 
							(
							items[i].IsDefaultItem != itemsToCompare[j].IsDefaultItem ||
							String.Compare(items[i].Name, itemsToCompare[j].Name) != 0
							)
							return false; // l'elemento non coincide
						
						break;
					}
				}
				if (j == itemsToCompare.Count)
					return false; // non c'è un elemento con lo stesso nome e valore
			}

			return true;
		}

		#endregion
	}
	
	#endregion

	#region WizardEnumInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardEnumInfoCollection.
	/// </summary>
	public class WizardEnumInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardEnumInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardEnumInfo))
					throw new NotSupportedException();

				this[index] = (WizardEnumInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardEnumInfo))
				throw new NotSupportedException();

			return this.Contains((WizardEnumInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardEnumInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardEnumInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardEnumInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardEnumInfo))
				throw new NotSupportedException();

			Insert(index, (WizardEnumInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardEnumInfo))
				throw new NotSupportedException();

			Remove((WizardEnumInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public WizardEnumInfo this[int index]
		{
			get {  return (WizardEnumInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardEnumInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo[] ToArray()
		{
			return (WizardEnumInfo[])InnerList.ToArray(typeof(WizardEnumInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardEnumInfo aEnumToAdd)
		{
			if (Contains(aEnumToAdd))
				return IndexOf(aEnumToAdd);

			return InnerList.Add(aEnumToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardEnumInfoCollection aEnumsCollectionToAdd)
		{
			if (aEnumsCollectionToAdd == null || aEnumsCollectionToAdd.Count == 0)
				return;

			foreach (WizardEnumInfo aEnumToAdd in aEnumsCollectionToAdd)
				Add(aEnumToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardEnumInfo aEnumToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aEnumToInsert))
				return;

			InnerList.Insert(index, aEnumToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardEnumInfo beforeEnum, WizardEnumInfo aEnumToInsert)
		{
			if (beforeEnum == null)
				Add(aEnumToInsert);

			if (!Contains(beforeEnum))
				return;

			if (Contains(aEnumToInsert))
				return;

			Insert(IndexOf(beforeEnum), aEnumToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardEnumInfo aEnumToRemove)
		{
			if (!Contains(aEnumToRemove))
				return;

			InnerList.Remove(aEnumToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardEnumInfo aEnumToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aEnumToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardEnumInfo aEnumToSearch)
		{
			if (!Contains(aEnumToSearch))
				return -1;
			
			return InnerList.IndexOf(aEnumToSearch);
		}
	
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardEnumInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardEnumInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardEnumInfo.Equals(this[i], ((WizardEnumInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByName(string aEnumName)
		{
			if (this.Count == 0 || !Generics.IsValidEnumName(aEnumName))
				return null;

			foreach(WizardEnumInfo aEnumInfo in InnerList)
			{
				if (String.Compare(aEnumName, aEnumInfo.Name) == 0)
					return aEnumInfo;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public WizardEnumInfo GetEnumInfoByValue(ushort aEnumValue)
		{
			if (this.Count == 0 || !Generics.IsValidEnumValue(aEnumValue))
				return null;

			foreach(WizardEnumInfo aEnumInfo in InnerList)
			{
				if (aEnumValue == aEnumInfo.Value)
					return aEnumInfo;
			}
			return null;
		}
		
		//---------------------------------------------------------------------------
		public void SortByName()
		{
			if (this.Count == 0)
				return;

			InnerList.Sort(new WizardEnumInfo.WizardEnumInfoNameSorter());
		}

	}

	#endregion

	#region WizardEnumItemInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardEnumItemInfo.
	/// </summary>
	public class WizardEnumItemInfo : IDisposable
	{
		#region WizardEnumItemInfo private data members

		private string	name = String.Empty;
		private ushort	itemValue = 0;
		private bool	isDefaultItem = false;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo(string aItemName, ushort aItemValue, bool isReadOnly)
		{
			name = aItemName;
			itemValue = aItemValue;
			readOnly = isReadOnly;
		}

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo(string aItemName, ushort aItemValue) : this(aItemName, aItemValue, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo(WizardEnumItemInfo aItemInfo)
		{
			name = (aItemInfo != null) ? aItemInfo.Name : String.Empty;
			itemValue = (aItemInfo != null) ? aItemInfo.Value : (ushort)0;
			isDefaultItem = (aItemInfo != null) ? aItemInfo.IsDefaultItem : false;
			readOnly = (aItemInfo != null) ? aItemInfo.ReadOnly : false;
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardEnumItemInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardEnumItemInfo)obj).Name) == 0 &&
				itemValue == ((WizardEnumItemInfo)obj).Value &&
				isDefaultItem == ((WizardEnumItemInfo)obj).IsDefaultItem
				);
		}
		
		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		#region WizardEnumItemInfo public properties

		//---------------------------------------------------------------------------
		public string Name { get { return name; } set { if (Generics.IsValidEnumItemName(value)) name = value; } }
		//---------------------------------------------------------------------------
		public ushort Value { get { return itemValue; } set { if (Generics.IsValidEnumItemValue(value)) itemValue = value; } }
		//---------------------------------------------------------------------------
		public bool IsDefaultItem { get { return isDefaultItem; } set { isDefaultItem = value; } }
		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		#endregion

		#region WizardEnumItemInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return name;
		}

		#endregion

	}
	
	#endregion

	#region WizardEnumItemInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardEnumItemInfoCollection.
	/// </summary>
	public class WizardEnumItemInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardEnumItemInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardEnumItemInfo))
					throw new NotSupportedException();

				this[index] = (WizardEnumItemInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardEnumItemInfo))
				throw new NotSupportedException();

			return this.Contains((WizardEnumItemInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardEnumItemInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardEnumItemInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardEnumItemInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardEnumItemInfo))
				throw new NotSupportedException();

			Insert(index, (WizardEnumItemInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardEnumItemInfo))
				throw new NotSupportedException();

			Remove((WizardEnumItemInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public WizardEnumItemInfo this[int index]
		{
			get {  return (WizardEnumItemInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardEnumItemInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardEnumItemInfo[] ToArray()
		{
			return (WizardEnumItemInfo[])InnerList.ToArray(typeof(WizardEnumItemInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardEnumItemInfo aEnumItemToAdd)
		{
			if (Contains(aEnumItemToAdd))
				return IndexOf(aEnumItemToAdd);

			return InnerList.Add(aEnumItemToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardEnumItemInfoCollection aEnumItemsCollectionToAdd)
		{
			if (aEnumItemsCollectionToAdd == null || aEnumItemsCollectionToAdd.Count == 0)
				return;

			foreach (WizardEnumItemInfo aEnumItemToAdd in aEnumItemsCollectionToAdd)
				Add(aEnumItemToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardEnumItemInfo aEnumItemToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aEnumItemToInsert))
				return;

			InnerList.Insert(index, aEnumItemToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardEnumItemInfo beforeItem, WizardEnumItemInfo aEnumItemToInsert)
		{
			if (beforeItem == null)
				Add(aEnumItemToInsert);

			if (!Contains(beforeItem))
				return;

			if (Contains(aEnumItemToInsert))
				return;

			Insert(IndexOf(beforeItem), aEnumItemToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardEnumItemInfo aEnumItemToRemove)
		{
			if (!Contains(aEnumItemToRemove))
				return;

			InnerList.Remove(aEnumItemToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardEnumItemInfo aEnumItemToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aEnumItemToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardEnumItemInfo aEnumItemToSearch)
		{
			if (!Contains(aEnumItemToSearch))
				return -1;
			
			return InnerList.IndexOf(aEnumItemToSearch);
		}
	
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardEnumItemInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardEnumItemInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardEnumItemInfo.Equals(this[i], ((WizardEnumItemInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion

	#region WizardHotKeyLinkInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardHotKeyLinkInfo.
	/// </summary>
	public class WizardHotKeyLinkInfo : IDisposable
	{
		#region WizardHotKeyLinkInfo private data members

		private string name = String.Empty;
		private string title = String.Empty;
		private string className = String.Empty;

		private WizardTableInfo table = null;

		private string codeColumnName = null;
		private string descriptionColumnName = null;

		private bool showCombo = true;

		private string referencedNameSpace = String.Empty;
		private string externalIncludeFile = String.Empty; 

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo(WizardTableInfo aTableInfo, bool isReadOnly, bool isReferenced)
		{
			this.Table = aTableInfo;

			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo(WizardTableInfo aTableInfo, bool isReadOnly) : this(aTableInfo, isReadOnly, false)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo(WizardTableInfo aTableInfo) : this(aTableInfo, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			table = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.Table : null;
			codeColumnName = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.CodeColumnName : String.Empty;
			descriptionColumnName = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.DescriptionColumnName : String.Empty;
			showCombo = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.ShowCombo : true;
			name = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.Name : String.Empty;
			title = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.Title : String.Empty;
			className = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.ClassName : String.Empty;
			externalIncludeFile = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.ExternalIncludeFile : String.Empty;
			referencedNameSpace = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.ReferencedNameSpace : String.Empty;
			readOnly = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.ReadOnly : false;
			referenced = (aHotKeyLinkInfo != null) ? aHotKeyLinkInfo.IsReferenced : false;
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return this.Name;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardHotKeyLinkInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name, ((WizardHotKeyLinkInfo)obj).Name) == 0 &&
				String.Compare(title, ((WizardHotKeyLinkInfo)obj).Title) == 0 &&
				String.Compare(className, ((WizardHotKeyLinkInfo)obj).ClassName) == 0 &&
				String.Compare(codeColumnName, ((WizardHotKeyLinkInfo)obj).CodeColumnName) == 0 &&
				String.Compare(descriptionColumnName, ((WizardHotKeyLinkInfo)obj).DescriptionColumnName) == 0 &&
				showCombo == ((WizardHotKeyLinkInfo)obj).ShowCombo &&
				(
				(table == null && ((WizardHotKeyLinkInfo)obj).Table == null) ||
				(
				table != null && ((WizardHotKeyLinkInfo)obj).Table != null &&
				String.Compare(table.Name, ((WizardHotKeyLinkInfo)obj).Table.Name) == 0
				)
				) &&
				String.Compare(referencedNameSpace, ((WizardHotKeyLinkInfo)obj).ReferencedNameSpace) == 0 &&
				String.Compare(externalIncludeFile, ((WizardHotKeyLinkInfo)obj).ExternalIncludeFile) == 0
				);
		}
		
		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		#region WizardHotKeyLinkInfo public properties

		//---------------------------------------------------------------------------
		public WizardTableInfo Table
		{
			get { return table; }
			set
			{
				if (WizardTableInfo.Equals(table, value))
					return;

				table = value;

				if (table != null && (name == null || name.Length == 0))
					name = table.Name;

				CodeColumnName = codeColumnName;
				DescriptionColumnName = descriptionColumnName;
			}
		}

		//---------------------------------------------------------------------------
		public string CodeColumnName 
		{ 
			get { return codeColumnName; } 
			set 
			{ 
				if 
					(
					table == null ||
					value == null ||
					value.Length == 0 ||
					table.GetColumnInfoByName(value) == null
					)
				{
					codeColumnName = String.Empty;
					return;
				}
				codeColumnName = value; 
			} 
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo CodeColumn 
		{
			get { return (table != null) ? table.GetColumnInfoByName(codeColumnName) : null; } 
			set 
			{ 
				if 
					(
					table == null || 
					table.ColumnsInfo == null || 
					(value != null && table.GetColumnInfoByName(value.Name) == null)
					)
					return;

				codeColumnName = (value != null) ? value.Name : String.Empty;
			} 
		}

		//---------------------------------------------------------------------------
		public string DescriptionColumnName 
		{ 
			get { return descriptionColumnName; } 
			set 
			{ 
				if 
					(
					table == null ||
					value == null ||
					value.Length == 0 ||
					table.GetColumnInfoByName(value) == null
					)
				{
					descriptionColumnName = String.Empty;
					return;
				}
				descriptionColumnName = value; 
			} 
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo DescriptionColumn 
		{
			get { return (table != null) ? table.GetColumnInfoByName(descriptionColumnName) : null; } 
			set 
			{ 
				if 
					(
					table == null || 
					table.ColumnsInfo == null || 
					(value != null && table.GetColumnInfoByName(value.Name) == null)
					)
					return;

				descriptionColumnName = (value != null) ? value.Name : String.Empty;
			} 
		}

		//---------------------------------------------------------------------------
		public bool IsDefined 
		{ 
			get 
			{ 
				return 
					(
					name != null &&
					name.Length > 0 &&
					className != null &&
					className.Length > 0 &&
					table != null &&
					codeColumnName != null &&
					codeColumnName.Length > 0 &&
					(referenced || (descriptionColumnName != null && descriptionColumnName.Length > 0))
					); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public string Name
		{
			get { return name; } 
			set 
			{
				if (value != null && !Generics.IsValidHotLinkName(value.Trim()))
					return;

				name = value.Trim(); 
			
				if (name != null && name.Length > 0 && (className == null || className.Length == 0))
					className = GetDefaultHKLClassName(name);
			} 
		}
		
		//---------------------------------------------------------------------------
		public string Title
		{
			get { return (title != null && title.Length > 0) ? title : name; } 
			set { title = value; } 
		}

		//---------------------------------------------------------------------------
		public string ClassName
		{
			get { return className; } 
			set 
			{ 
				if (value == null || !Generics.IsValidClassName(value))
				{
					if (name != null && name.Length > 0)
						className = GetDefaultHKLClassName(name);
					else if (table != null)
						className = GetDefaultHKLClassName(table.Name);
					else
						className = String.Empty;
				}
				else
					className = value.Trim(); 
			}
		}
		
		//---------------------------------------------------------------------------
		public bool ShowCombo
		{
			get { return showCombo; } 
			set { showCombo = value; }
		}

		//---------------------------------------------------------------------------
		public string ReferencedNameSpace
		{
			get { return referenced ? referencedNameSpace : String.Empty; } 
			set 
			{
				if (!referenced)
				{
					referencedNameSpace = String.Empty;
					return;
				}

				referencedNameSpace = value; 
			} 
		}


		//---------------------------------------------------------------------------
		public string ExternalIncludeFile 
		{
			get { return referenced ? externalIncludeFile : String.Empty; } 
			set 
			{
				if (!referenced)
				{
					externalIncludeFile = String.Empty;
					return;
				}

				if (value == null || value.Trim().Length == 0 || Generics.IsValidFullPathName(value))
					externalIncludeFile = value.Trim(); 
			} 
		}

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 
		
		#endregion

		#region WizardHotKeyLinkInfo public methods

		//---------------------------------------------------------------------------
		public static string GetDefaultHKLName(string aHKLName)
		{
			if (aHKLName == null || aHKLName.Trim().Length == 0)
				return String.Empty;

			return "HKL" + aHKLName.Trim().Replace(' ', '_');
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultHKLClassName(string aHKLName)
		{
			if (aHKLName == null || aHKLName.Trim().Length == 0)
				return String.Empty;

			return "HKL" + Generics.SubstitueInvalidCharacterInIdentifier(aHKLName.Trim().Replace(' ', '_'));
		}

		#endregion

	}
	
	#endregion

	#region WizardHotKeyLinkInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardHotKeyLinkInfoCollection.
	/// </summary>
	public class WizardHotKeyLinkInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardHotKeyLinkInfo))
					throw new NotSupportedException();

				this[index] = (WizardHotKeyLinkInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardHotKeyLinkInfo))
				throw new NotSupportedException();

			return this.Contains((WizardHotKeyLinkInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardHotKeyLinkInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardHotKeyLinkInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardHotKeyLinkInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardHotKeyLinkInfo))
				throw new NotSupportedException();

			Insert(index, (WizardHotKeyLinkInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardHotKeyLinkInfo))
				throw new NotSupportedException();

			Remove((WizardHotKeyLinkInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo this[int index]
		{
			get {  return (WizardHotKeyLinkInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardHotKeyLinkInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo[] ToArray()
		{
			return (WizardHotKeyLinkInfo[])InnerList.ToArray(typeof(WizardHotKeyLinkInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardHotKeyLinkInfo aHotKeyLinkToAdd)
		{
			if (Contains(aHotKeyLinkToAdd))
				return IndexOf(aHotKeyLinkToAdd);

			return InnerList.Add(aHotKeyLinkToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardHotKeyLinkInfoCollection aHotKeyLinksCollectionToAdd)
		{
			if (aHotKeyLinksCollectionToAdd == null || aHotKeyLinksCollectionToAdd.Count == 0)
				return;

			foreach (WizardHotKeyLinkInfo aHotKeyLinkToAdd in aHotKeyLinksCollectionToAdd)
				Add(aHotKeyLinkToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardHotKeyLinkInfo aHotKeyLinkToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aHotKeyLinkToInsert))
				return;

			InnerList.Insert(index, aHotKeyLinkToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardHotKeyLinkInfo beforeHotKeyLink, WizardHotKeyLinkInfo aHotKeyLinkToInsert)
		{
			if (beforeHotKeyLink == null)
				Add(aHotKeyLinkToInsert);

			if (!Contains(beforeHotKeyLink))
				return;

			if (Contains(aHotKeyLinkToInsert))
				return;

			Insert(IndexOf(beforeHotKeyLink), aHotKeyLinkToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardHotKeyLinkInfo aHotKeyLinkToRemove)
		{
			if (!Contains(aHotKeyLinkToRemove))
				return;

			InnerList.Remove(aHotKeyLinkToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardHotKeyLinkInfo aHotKeyLinkToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aHotKeyLinkToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardHotKeyLinkInfo aHotKeyLinkToSearch)
		{
			if (!Contains(aHotKeyLinkToSearch))
				return -1;
			
			return InnerList.IndexOf(aHotKeyLinkToSearch);
		}
	
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardHotKeyLinkInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardHotKeyLinkInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardHotKeyLinkInfo.Equals(this[i], ((WizardHotKeyLinkInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion

	#region WizardClientDocumentInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardClientDocumentInfo.
	/// </summary>
	public class WizardClientDocumentInfo : IDisposable
	{
		[Flags]
			public enum EventsRoutingMode : ushort
		{
			Undefined	= 0x0000,
			Before		= 0x0001,
			After		= 0x0002,
			Default		= Before,
			Both		= Before | After
		}

		#region WizardClientDocumentInfo private data members

		private WizardLibraryInfo library = null;
	
		private WizardDocumentInfo serverDocumentInfo = null;
		private string familyToAttachClassName = String.Empty;

		private string	name = String.Empty;
		private string	className = String.Empty;
		private string	title = String.Empty;

		private EventsRoutingMode routing = EventsRoutingMode.Default;
		private bool excludeUnattendedMode = false;
		private bool excludeBatchMode = false;

		private ArrayList serverHeaderFilesToinclude = null;

		private bool createSlaveFormView = false;
		private bool addTabDialogs = true;

		private WizardDocumentTabbedPaneInfoCollection tabbedPanes = null;
		
		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private WizardDBTInfoCollection dbts = null;

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName, WizardLibraryInfo aLibraryInfo, WizardDocumentInfo aServerDocumentInfo, bool isReadOnly, bool isReferenced)
		{
			library = aLibraryInfo;	
			serverDocumentInfo = aServerDocumentInfo;

			Name = aClientDocumentName;
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName, WizardLibraryInfo aLibraryInfo, WizardDocumentInfo aServerDocumentInfo, bool isReadOnly) : this(aClientDocumentName, aLibraryInfo, aServerDocumentInfo, isReadOnly, false)
		{
		}
		
		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName, WizardLibraryInfo aLibraryInfo, WizardDocumentInfo aServerDocumentInfo) : this(aClientDocumentName, aLibraryInfo, aServerDocumentInfo, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName, WizardDocumentInfo aServerDocumentInfo, bool isReadOnly, bool isReferenced) : this(aClientDocumentName, null, aServerDocumentInfo, isReadOnly, isReferenced)
		{
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName, WizardDocumentInfo aServerDocumentInfo, bool isReadOnly) : this(aClientDocumentName, aServerDocumentInfo, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName, WizardDocumentInfo aServerDocumentInfo) : this(aClientDocumentName, null, aServerDocumentInfo)
		{
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(string aClientDocumentName) : this(aClientDocumentName, null, null)
		{
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo(WizardClientDocumentInfo aClientDocumentInfo)
		{
			library = (aClientDocumentInfo != null) ? aClientDocumentInfo.Library : null;
			serverDocumentInfo = (aClientDocumentInfo != null) ? aClientDocumentInfo.ServerDocumentInfo : null;;
			familyToAttachClassName = (aClientDocumentInfo != null) ? aClientDocumentInfo.FamilyToAttachClassName : String.Empty;;

			name = (aClientDocumentInfo != null) ? aClientDocumentInfo.Name : String.Empty;
			className = (aClientDocumentInfo != null) ? aClientDocumentInfo.ClassName : String.Empty;

			title = (aClientDocumentInfo != null) ? aClientDocumentInfo.Title : String.Empty;
		
			routing = (aClientDocumentInfo != null) ? aClientDocumentInfo.EventsRouting : EventsRoutingMode.Undefined;

			excludeUnattendedMode = (aClientDocumentInfo != null) ? aClientDocumentInfo.ExcludeUnattendedMode : false;
			excludeBatchMode = (aClientDocumentInfo != null) ? aClientDocumentInfo.ExcludeUnattendedMode : false;
		
			readOnly = (aClientDocumentInfo != null) ? aClientDocumentInfo.ReadOnly : false;
			referenced = (aClientDocumentInfo != null) ? aClientDocumentInfo.IsReferenced : false;
		
			if (aClientDocumentInfo != null && aClientDocumentInfo.ServerHeaderFilesToincludeCount > 0)
			{
				if (serverHeaderFilesToinclude == null)
					serverHeaderFilesToinclude = new ArrayList();
				serverHeaderFilesToinclude.AddRange(aClientDocumentInfo.ServerHeaderFilesToinclude);
			}

			createSlaveFormView = (aClientDocumentInfo != null) ? aClientDocumentInfo.CreateSlaveFormView : false;
			addTabDialogs = (aClientDocumentInfo != null) ? aClientDocumentInfo.AddTabDialogs : true;

			if (aClientDocumentInfo != null && aClientDocumentInfo.DBTsCount > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in aClientDocumentInfo.DBTsInfo)
					this.AddDBTInfo(new WizardDBTInfo(aDBTInfo));
			}
	
			if (aClientDocumentInfo != null && aClientDocumentInfo.TabbedPanesCount > 0)
			{
				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in aClientDocumentInfo.TabbedPanes)
					this.AddTabbedPane(new WizardDocumentTabbedPaneInfo(aTabbedPaneInfo));
			}
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardClientDocumentInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				String.Compare(name,((WizardClientDocumentInfo)obj).Name) == 0 &&
				String.Compare(className,((WizardClientDocumentInfo)obj).ClassName) == 0 &&
				String.Compare(title,((WizardClientDocumentInfo)obj).Title) == 0 &&
				routing == ((WizardClientDocumentInfo)obj).EventsRouting &&
				excludeUnattendedMode == ((WizardClientDocumentInfo)obj).ExcludeUnattendedMode &&
				excludeBatchMode == ((WizardClientDocumentInfo)obj).ExcludeBatchMode &&
				serverDocumentInfo.Equals(((WizardClientDocumentInfo)obj).ServerDocumentInfo) &&
				String.Compare(familyToAttachClassName,((WizardClientDocumentInfo)obj).FamilyToAttachClassName) == 0 &&
				Array.Equals((string[])serverHeaderFilesToinclude.ToArray(typeof(string)), ((WizardClientDocumentInfo)obj).ServerHeaderFilesToinclude) &&
				createSlaveFormView == ((WizardClientDocumentInfo)obj).CreateSlaveFormView &&
				addTabDialogs == ((WizardClientDocumentInfo)obj).AddTabDialogs &&
				WizardDBTInfoCollection.Equals(dbts,((WizardClientDocumentInfo)obj).DBTsInfo) &&
				WizardDocumentTabbedPaneInfoCollection.Equals(tabbedPanes,((WizardClientDocumentInfo)obj).TabbedPanes)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo)
		{
			if (library == aLibraryInfo)
				return;

			if (library != null && library.ClientDocumentsInfo.Contains(this))
				library.ClientDocumentsInfo.Remove(this);

			if (aLibraryInfo != null)
			{
				// Se aggiungo ad una libreria un documento devo controllare che i DBT in esso utilizzati
				// e le tabelle alle quali essi fanno riferimento siano visibili, cioè che siano definiti
				// in librerie dalle quali dipende la libreria corrente. In caso contrario, inserisco
				// tali librerie nelle dipendenze
				if (dbts != null && dbts.Count > 0)
				{
					foreach (WizardDBTInfo aDBTInfo in dbts)
					{
						if (aDBTInfo.Library != null)
							aLibraryInfo.AddDependency(aDBTInfo.Library);

						if (aDBTInfo.TableName == null || aDBTInfo.TableName.Length == 0 || aLibraryInfo.IsTableAvailable(aDBTInfo.TableName))
							continue;
					
						if (aLibraryInfo.Application != null)
						{
							WizardTableInfo dbtTableInfo = aLibraryInfo.Application.GetTableInfoByName(aDBTInfo.TableName);
							if (dbtTableInfo != null && dbtTableInfo.Library != null)
								aLibraryInfo.AddDependency(dbtTableInfo.Library);
						}
					}
				}
			}

			library = aLibraryInfo;
		}


		#region WizardClientDocumentInfo public properties

		//---------------------------------------------------------------------------
		public WizardLibraryInfo Library { get { return library; } }
		
		//---------------------------------------------------------------------
		public WizardDocumentInfo ServerDocumentInfo { get { return serverDocumentInfo; } }
		
		//---------------------------------------------------------------------
		public string FamilyToAttachClassName 
		{
			get 
			{
				if 
					(
					serverDocumentInfo == null || 
					serverDocumentInfo.ClassHierarchy == null || 
					serverDocumentInfo.ClassHierarchy.Length == 0
					)
					return String.Empty;
				
				return familyToAttachClassName;
			} 
			set
			{
				string serverDocAscendantClass = String.Empty;

				if 
					(
					value != null &&
					value.Trim().Length > 0 &&
					serverDocumentInfo != null && 
					serverDocumentInfo.ClassHierarchy != null && 
					serverDocumentInfo.ClassHierarchy.Length > 0
					)
				{
					string[] serverDocClasses = serverDocumentInfo.ClassHierarchy.Split('.');
					if (serverDocClasses != null && serverDocClasses.Length > 0)
					{
						for(int i=0; i < serverDocClasses.Length; i++)
						{
							string aClass = serverDocClasses[i].Trim();
							
							if (String.Compare(aClass, value.Trim()) == 0)
							{
								serverDocAscendantClass = aClass;
								break;
							}
						}
					}
				}

				familyToAttachClassName = serverDocAscendantClass;
			}
		}

		//---------------------------------------------------------------------
		public bool AttachToFamily 
		{
			get
			{
				return
					(
					serverDocumentInfo != null && 
					serverDocumentInfo.ClassHierarchy != null &&
					serverDocumentInfo.ClassHierarchy.Length > 0 &&
					familyToAttachClassName != null &&
					familyToAttachClassName.Length > 0
					);
			}
		}
		
		//---------------------------------------------------------------------------
		public string Name
		{ 
			get { return name; } 
			set 
			{ 
				if (Generics.IsValidDocumentName(value)) 
				{
					name = value; 
					if (className == null || className.Length == 0)
						className = GetDefaultClientDocumentClassName(name);
				}
			} 
		}
		
		//---------------------------------------------------------------------------
		public string ClassName
		{
			get { return className; } 
			set 
			{ 
				if (value == null || !Generics.IsValidClassName(value))
					className = GetDefaultClientDocumentClassName(name);
				else
					className = value.Trim(); 
			}
		}

		//---------------------------------------------------------------------------
		public string Title { get { return title; } set { title = value; } }

		//---------------------------------------------------------------------------
		public EventsRoutingMode EventsRouting { get { return routing; } set { routing = value; } }

		//---------------------------------------------------------------------------
		public bool AreEventsRoutedBefore
		{
			get { return (routing & EventsRoutingMode.Before) == EventsRoutingMode.Before; } 
			set
			{
				if (value)
					routing |= EventsRoutingMode.Before;
				else
					routing &= ~EventsRoutingMode.Before;
			}
		}

		//---------------------------------------------------------------------------
		public bool AreEventsRoutedAfter
		{
			get { return (routing & EventsRoutingMode.After) == EventsRoutingMode.After; } 
			set
			{
				if (value)
					routing |= EventsRoutingMode.After;
				else
					routing &= ~EventsRoutingMode.After;
			}
		}

		//---------------------------------------------------------------------------
		public  bool ExcludeUnattendedMode { get { return excludeUnattendedMode; } set { excludeUnattendedMode = value; } }

		//---------------------------------------------------------------------------
		public  bool ExcludeBatchMode { get { return excludeBatchMode; } set { excludeBatchMode = value; } }

		//---------------------------------------------------------------------------
		public WizardDBTInfoCollection DBTsInfo { get { return dbts; } }

		//---------------------------------------------------------------------------
		public string[] ServerHeaderFilesToinclude
		{
			get
			{
				// se il server document fa parte dell'applicazione corrente devo solamente includere 
				// il file contenente la sua dichiarazione
				if (
					library != null &&
					serverDocumentInfo != null && 
					serverDocumentInfo.Library != null && 
					serverDocumentInfo.Library.Application != null &&
					library.Application != null &&
					String.Compare(library.Application.Name, serverDocumentInfo.Library.Application.Name) == 0
					)
				{
					string libraryPath = WizardCodeGenerator.GetStandardLibraryPath(serverDocumentInfo.Library);
					if (libraryPath != null && libraryPath.Length > 0)
					{
						return new string[] { libraryPath + Path.DirectorySeparatorChar + serverDocumentInfo.ClassName + Generics.CppHeaderExtension};
					}
				}
				
				return (serverHeaderFilesToinclude != null && serverHeaderFilesToinclude.Count > 0) ? (string[])serverHeaderFilesToinclude.ToArray(typeof(string)) : null;
			}
		}

		//---------------------------------------------------------------------------
		public int ServerHeaderFilesToincludeCount
		{
			get
			{
				if (
					library != null &&
					serverDocumentInfo != null && 
					serverDocumentInfo.Library != null && 
					serverDocumentInfo.Library.Application != null &&
					library.Application != null &&
					String.Compare(library.Application.Name, serverDocumentInfo.Library.Application.Name) == 0
					)
					return 1;
					
				return (serverHeaderFilesToinclude != null) ? serverHeaderFilesToinclude.Count : 0;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsInterfacePresent
		{
			get 
			{
				if (tabbedPanes != null && tabbedPanes.Count > 0)
				{
					foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
					{
						if (aTabbedPaneInfo.HasVisibleColums())
							return true;
					}
				}

				if (dbts == null || dbts.Count == 0)
					return false;

				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (aDBTInfo.HasVisibleColums())
						return true;
				}
				return false; 
			}
		}
		
		//---------------------------------------------------------------------------
		public bool CreateSlaveFormView 
		{
			get { return (dbts != null && dbts.Count > 0 && createSlaveFormView); }
 
			set { createSlaveFormView = value; }
		}

		//---------------------------------------------------------------------------
		public bool AddTabDialogs 
		{
			get { return (tabbedPanes != null && tabbedPanes.Count > 0 && addTabDialogs); }
 
			set { addTabDialogs = value; }
		}

		//---------------------------------------------------------------------------
		public int DBTsCount { get { return (dbts != null) ? dbts.Count : 0; } }

		//---------------------------------------------------------------------------
		public WizardDBTInfo DBTMaster
		{
			get
			{
				return (serverDocumentInfo != null) ? serverDocumentInfo.DBTMaster : null;
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo DBTMasterTable
		{
			get
			{
				WizardDBTInfo masterInfo = this.DBTMaster;
				if (masterInfo == null)
					return null;

				return masterInfo.GetTableInfo();				
			}
		}
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection TabbedPanes { get { return tabbedPanes; } }

		//---------------------------------------------------------------------------
		public int TabbedPanesCount { get { return (tabbedPanes != null) ? tabbedPanes.Count : 0; } }

		//---------------------------------------------------------------------------
		public ushort FirstResourceId
		{
			get 
			{ 
				return (library != null) ? library.GetClientDocumentFirstResourceId(this) : Generics.FirstValidResourceId; 
			} 
		}

		//---------------------------------------------------------------------------
		public ushort FirstControlId
		{
			get 
			{ 
				return (library != null) ? library.GetClientDocumentFirstControlId(this) : Generics.FirstValidControlId; 
			} 
		}

		//---------------------------------------------------------------------------
		public ushort FirstCommandId
		{
			get 
			{ 
				return (library != null) ? library.GetClientDocumentFirstCommandId(this) : Generics.FirstValidCommandId; 
			} 
		}

		//---------------------------------------------------------------------------
		public ushort FirstSymedId
		{
			get 
			{ 
				return (library != null) ? library.GetClientDocumentFirstSymedId(this) : Generics.FirstValidSymedId; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } } 
		
		#endregion

		#region WizardClientDocumentInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public bool AddServerHeaderFile(string aHeaderFile)
		{
			if (aHeaderFile == null)
				return false;

			string headerFileToAdd = aHeaderFile.Trim();

			if (headerFileToAdd.Length == 0 || !Generics.IsValidFullPathName(headerFileToAdd))
				return false;

			if (serverHeaderFilesToinclude != null)
			{
				foreach(string aAddedHeaderFile in serverHeaderFilesToinclude)
				{
					if (String.Compare(headerFileToAdd, aAddedHeaderFile, true) == 0)
						return false; // already inserted
				}
			}
			else
				serverHeaderFilesToinclude = new ArrayList();

			serverHeaderFilesToinclude.Add(headerFileToAdd);

			return true;
		}
		
		//---------------------------------------------------------------------------
		public void AddServerHeaderFiles(string[] aHeaderFilesList)
		{
			if (aHeaderFilesList == null || aHeaderFilesList.Length == 0)
				return;
	
			foreach(string aHeaderFile in aHeaderFilesList)
				AddServerHeaderFile(aHeaderFile);
		}
		
		//---------------------------------------------------------------------------
		public void SetServerHeaderFiles(string[] aHeaderFilesList)
		{
			if (serverHeaderFilesToinclude != null)
				serverHeaderFilesToinclude.Clear();

			AddServerHeaderFiles(aHeaderFilesList);
		}

		//---------------------------------------------------------------------------
		public WizardDBTInfo GetDBTInfoByName(string aDBTName)
		{
			if (aDBTName == null || aDBTName.Length == 0)
				return null;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if (String.Compare(aDBTInfo.Name, aDBTName) == 0)
						return aDBTInfo;
				}
			}

			return null;
		}
	
		//---------------------------------------------------------------------------
		public int AddDBTInfo(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				library == null ||
				aDBTInfo == null || 
				aDBTInfo.Name == null || 
				aDBTInfo.Name.Length == 0 || 
				aDBTInfo.TableName == null || 
				aDBTInfo.TableName.Length == 0 ||
				aDBTInfo.IsMaster ||
				(dbts != null && dbts.Contains(aDBTInfo))
				)
				return -1;

			WizardDBTInfo existingDBT = GetDBTInfoByName(aDBTInfo.Name);
			if (existingDBT != null)
				return -1;

			if (!library.IsDBTAvailable(aDBTInfo))
				return -1; // il DBT non è stato trovato fra quelli della libreria e nemmeno nelle dipendenze

			if (library.Application != null && !library.IsTableAvailable(aDBTInfo.TableName))
			{
				WizardTableInfo dbtTableInfo = library.Application.GetTableInfoByName(aDBTInfo.TableName);
				if (dbtTableInfo != null && dbtTableInfo.Library != null)
					library.AddDependency(dbtTableInfo.Library);
			}

			if (dbts == null)
				dbts = new WizardDBTInfoCollection();

			string serverDocumentNamespace = (this.ServerDocumentInfo != null) ? this.ServerDocumentInfo.GetNameSpace() : String.Empty;
			if (serverDocumentNamespace != null && serverDocumentNamespace.Length > 0)
			{
				if 
					(
					aDBTInfo.ServerDocumentNamespace != null &&
					aDBTInfo.ServerDocumentNamespace.Length > 0 &&
					String.Compare(serverDocumentNamespace, aDBTInfo.ServerDocumentNamespace) != 0
					)
					return -1;

				aDBTInfo.ServerDocumentNamespace = serverDocumentNamespace;
			}
			
			int addedIdx = dbts.Add(aDBTInfo);
			if (addedIdx >= 0 && !aDBTInfo.IsMaster)
				AddDBTTabbedPane(aDBTInfo);
			return addedIdx;
		}
	
		//---------------------------------------------------------------------------
		public void RemoveDBT(WizardDBTInfo aDBTInfoToRemove)
		{
			if (dbts == null || dbts.Count == 0 || aDBTInfoToRemove == null || !dbts.Contains(aDBTInfoToRemove))
				return;

			RemoveAllDBTTabbedPanes(aDBTInfoToRemove);
			
			dbts.Remove(aDBTInfoToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllDBTs()
		{
			if (dbts == null || dbts.Count == 0)
				return;

			RemoveAllTabbedPanes();
			
			dbts.Clear();
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo GetOriginalDBTTabbedPane(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				aDBTInfo == null || 
				aDBTInfo.IsMaster ||
				!dbts.Contains(aDBTInfo)
				)
				return null;
	
			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
			{
				if 
					(
					aTabbedPaneInfo != null &&
					String.Compare(aTabbedPaneInfo.DBTName, aDBTInfo.Name) == 0 &&
					aTabbedPaneInfo.ManagedColumnsCount == 0
					)
					return aTabbedPaneInfo;
			}
			return null;
		}
		
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection GetAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				aDBTInfo == null || 
				!dbts.Contains(aDBTInfo)
				)
				return null;
			
			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = new WizardDocumentTabbedPaneInfoCollection();

			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
			{
				if (aTabbedPaneInfo == null || String.Compare(aTabbedPaneInfo.DBTName, aDBTInfo.Name) != 0)
					continue;
				dbtTabbedPanes.Add(aTabbedPaneInfo);
			}

			return (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0) ? dbtTabbedPanes : null;
		}

		//---------------------------------------------------------------------------
		public int AddTabbedPane(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if 
				(
				library == null ||
				aTabbedPaneInfo == null || 
				aTabbedPaneInfo.DBTInfo == null || 
				aTabbedPaneInfo.TableName == null || 
				aTabbedPaneInfo.TableName.Length == 0 ||
				(tabbedPanes != null && tabbedPanes.Contains(aTabbedPaneInfo))
				)
				return -1;

			if
				(
				!WizardDBTInfo.Equals(this.DBTMaster, aTabbedPaneInfo.DBTInfo) &&
				GetDBTInfoByName(aTabbedPaneInfo.DBTName) == null
				)
				return -1; // il DBT non è stato trovato fra quelli gestiti dal documento

			if (tabbedPanes != null)
			{
				// Se si tratta di una scheda di un DBT (cioè sprovvista della specifica di sue
				// managedColumns) e ho già caricato le informazioni relative alla scheda equivalente
				// (che viene, infatti, generata automaticamente per ciascun DBT aggiunto al documento), 
				// elimino quest'ultima in modo da garantire l'ordine corretto di tutte le schede...
				if (aTabbedPaneInfo.ManagedColumnsCount == 0)
				{
					WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = GetAllDBTTabbedPanes(aTabbedPaneInfo.DBTInfo);
					if (dbtTabbedPanes != null && dbtTabbedPanes.Count > 0)
					{
						foreach (WizardDocumentTabbedPaneInfo sameDBTTabbedPane in dbtTabbedPanes)
						{
							if (sameDBTTabbedPane.ManagedColumnsCount == 0)
								tabbedPanes.Remove(sameDBTTabbedPane);
						}
					}		
				}
			}
			else
				tabbedPanes = new WizardDocumentTabbedPaneInfoCollection();

			return tabbedPanes.Add(aTabbedPaneInfo);
		}
	
		//---------------------------------------------------------------------------
		public int AddDBTTabbedPane(WizardDBTInfo aDBTInfo)
		{
			return AddTabbedPane(new WizardDocumentTabbedPaneInfo(aDBTInfo));
		}
		
		//---------------------------------------------------------------------------
		public void InsertTabbedPane(int index, WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if 
				(
				library == null ||
				aTabbedPaneInfo == null || 
				aTabbedPaneInfo.DBTInfo == null || 
				dbts == null || 
				dbts.Count == 0 || 
				!dbts.Contains(aTabbedPaneInfo.DBTInfo) || 
				aTabbedPaneInfo.TableName == null || 
				aTabbedPaneInfo.TableName.Length == 0 ||
				(tabbedPanes != null && tabbedPanes.Contains(aTabbedPaneInfo)) ||
				index < 0 || 
				(tabbedPanes != null && index > tabbedPanes.Count - 1) ||
				(tabbedPanes == null && index > 0)
				)
				return;

			if (GetDBTInfoByName(aTabbedPaneInfo.DBTName) == null)
				return; // il DBT non è stato trovato fra quelli gestiti dal documento

			if (tabbedPanes == null)
				tabbedPanes = new WizardDocumentTabbedPaneInfoCollection();

            if (index < tabbedPanes.Count)
                tabbedPanes.Insert(index, aTabbedPaneInfo);
            else
                tabbedPanes.Add(aTabbedPaneInfo);
        }
	
		//---------------------------------------------------------------------------
		public void InsertDBTTabbedPane(int index, WizardDBTInfo aDBTInfo)
		{
			InsertTabbedPane(index, new WizardDocumentTabbedPaneInfo(aDBTInfo));
		}

		//---------------------------------------------------------------------------
		public void RemoveTabbedPane(WizardDocumentTabbedPaneInfo aTabbedPaneToRemove)
		{
			if (tabbedPanes == null || tabbedPanes.Count == 0 || aTabbedPaneToRemove == null || !tabbedPanes.Contains(aTabbedPaneToRemove))
				return;

			tabbedPanes.Remove(aTabbedPaneToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllDBTTabbedPanes(WizardDBTInfo aDBTInfo)
		{
			if 
				(
				dbts == null || 
				dbts.Count == 0 || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				aDBTInfo == null || 
				!dbts.Contains(aDBTInfo)
				)
				return;

			WizardDocumentTabbedPaneInfoCollection dbtTabbedPanes = GetAllDBTTabbedPanes(aDBTInfo);
			if (dbtTabbedPanes == null || dbtTabbedPanes.Count == 0)
				return;

			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in dbtTabbedPanes)
				tabbedPanes.Remove(aTabbedPaneInfo);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllTabbedPanes()
		{
			if (tabbedPanes == null || tabbedPanes.Count == 0)
				return;

			tabbedPanes.Clear();
		}

		//---------------------------------------------------------------------------
		public string GetNameSpace()
		{
			if (library == null)
				return String.Empty;

			string libraryNameSpace = library.GetNameSpace();
			if (libraryNameSpace == null || libraryNameSpace.Length == 0)
				return String.Empty;

			return libraryNameSpace + "." + name;
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultClientDocumentClassName(string aClientDocumentName)
		{
			if (aClientDocumentName == null || aClientDocumentName.Trim().Length == 0)
				return String.Empty;

			return "CD" + Generics.SubstitueInvalidCharacterInIdentifier(aClientDocumentName.Trim().Replace(' ', '_'));
		}
		
		//---------------------------------------------------------------------------
		public bool UsesExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			if 
				(
				aExtraAddedColumnsInfo == null || 
				aExtraAddedColumnsInfo.TableName == null ||
				aExtraAddedColumnsInfo.TableName.Length == 0 ||
				(this.DBTsCount == 0 && this.TabbedPanesCount == 0)
				)
				return false;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTInfo in dbts)
				{
					if 
						(
						aDBTInfo != null &&
						String.Compare(aDBTInfo.TableName, aExtraAddedColumnsInfo.TableName) == 0 && 
						aDBTInfo.HasVisibleAdditionalColumns(aExtraAddedColumnsInfo)
						)
						return true;
				}
			}

			if (tabbedPanes != null && tabbedPanes.Count > 0)
			{
				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
				{
					if 
						(
						aTabbedPaneInfo != null &&
						String.Compare(aTabbedPaneInfo.TableName, aExtraAddedColumnsInfo.TableName) == 0 && 
						aTabbedPaneInfo.HasVisibleAdditionalColumns(aExtraAddedColumnsInfo)
						)
						return true;
				}
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableResourceId()
		{
			return (ushort)(FirstResourceId + GetUsedResourceIdsCount());
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedResourceIdsCount()
		{
			ushort usedResourceIdsCount = 0;
			if (CreateSlaveFormView)
				usedResourceIdsCount += 5; // id della SlaveFormView e delle 4 bitmap dei bottoni della toolbar

			if (tabbedPanes == null || tabbedPanes.Count == 0)
				return usedResourceIdsCount; 
			
			usedResourceIdsCount += (ushort)tabbedPanes.Count;
			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
			{
				if (aTabbedPaneInfo.CreateRowForm)
					usedResourceIdsCount++;
			}
			
			return usedResourceIdsCount;
		}

		//---------------------------------------------------------------------------
		public int GetSlaveFormViewId()
		{
			return CreateSlaveFormView ? this.FirstResourceId : -1;
		}

		//---------------------------------------------------------------------------
		public int GetSlaveFormViewCommandId()
		{
			return CreateSlaveFormView ? this.FirstCommandId : -1;
		}
		
		//---------------------------------------------------------------------------
		public int GetToolBarButtonLargeId()
		{
			return CreateSlaveFormView ? (this.FirstResourceId + 2) : -1;
		}

		//---------------------------------------------------------------------------
		public int GetToolBarButtonSmallId()
		{
			return CreateSlaveFormView ? (this.FirstResourceId + 3) : -1;
		}

		//---------------------------------------------------------------------------
		public int GetToolBarButtonLargeDisabledId()
		{
			return CreateSlaveFormView ? (this.FirstResourceId + 4) : -1;
		}

		//---------------------------------------------------------------------------
		public int GetToolBarButtonSmallDisabledId()
		{
			return CreateSlaveFormView ? (this.FirstResourceId + 5) : -1;
		}

		//---------------------------------------------------------------------------
		public int GetTabbedPaneFormId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (aTabbedPaneInfo == null)
				return -1;

			WizardDocumentTabbedPaneInfoCollection tabbedPanes = this.TabbedPanes;
			
			if (tabbedPanes == null || tabbedPanes.Count == 0 || !tabbedPanes.Contains(aTabbedPaneInfo))
				return -1;

            int dialogsCount = createSlaveFormView ? 1 : 0;
            for (int tabbedPaneIdx = 0; tabbedPaneIdx < tabbedPanes.IndexOf(aTabbedPaneInfo); tabbedPaneIdx++)
            {
                dialogsCount++;
                if (tabbedPanes[tabbedPaneIdx].DBTInfo != null && tabbedPanes[tabbedPaneIdx].DBTInfo.CreateRowForm)
                    dialogsCount++;
            }

            return FirstResourceId + dialogsCount;
        }

		//---------------------------------------------------------------------------
		public int GetTabbedPaneRowFormId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (aTabbedPaneInfo == null || !aTabbedPaneInfo.CreateRowForm)
				return -1;
			
			WizardDocumentTabbedPaneInfoCollection tabbedPanes = this.TabbedPanes;
			
			if (tabbedPanes == null || tabbedPanes.Count == 0 || !tabbedPanes.Contains(aTabbedPaneInfo))
				return -1;

			return GetTabbedPaneFormId(aTabbedPaneInfo) + 1;
		}

		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableControlId()
		{
			return (ushort)(FirstControlId + GetUsedControlIdsCount());
		}

		//---------------------------------------------------------------------------
		public ushort GetUsedControlIdsCount()
		{
			if (dbts == null || dbts.Count == 0)
				return 0;

			ushort controlsCount = 0;

			if (IsTabberToCreate())
			{
				controlsCount++;

				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
					controlsCount += aTabbedPaneInfo.GetUsedControlIdsCount();
			}

			return controlsCount; 
		}

		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableCommandId()
		{
			return (ushort)(FirstCommandId + GetUsedCommandIdsCount());
		}
	
		//---------------------------------------------------------------------------
		public int GetFirstDBTControlId(WizardDBTInfo aDBTInfo, bool isBodyEdit)
		{
			if (aDBTInfo == null || dbts == null || dbts.Count == 0 || !dbts.Contains(aDBTInfo))
				return -1;

			WizardDocumentTabbedPaneInfo dbtTabbedPane = GetOriginalDBTTabbedPane(aDBTInfo);
			if (dbtTabbedPane == null)
				return -1;

			int firstDBTControlId = FirstControlId;

			if (IsTabberToCreate())
				firstDBTControlId++;
			
			for (int i = 0; i < tabbedPanes.IndexOf(dbtTabbedPane); i++)
				firstDBTControlId += tabbedPanes[i].GetUsedControlIdsCount();

			return (!aDBTInfo.IsSlaveBuffered || isBodyEdit) ? firstDBTControlId : (firstDBTControlId + 1); 
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstDBTControlId(WizardDBTInfo aDBTInfo)
		{
			return GetFirstDBTControlId(aDBTInfo, false);
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstTabbedPaneControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo, bool isBodyEdit)
		{
			if (aTabbedPaneInfo == null || tabbedPanes == null || tabbedPanes.Count == 0 || !tabbedPanes.Contains(aTabbedPaneInfo))
				return -1;
		
			int firstDBTControlId = FirstControlId;

			if (IsTabberToCreate())
				firstDBTControlId++;
			
			for (int i = 0; i < tabbedPanes.IndexOf(aTabbedPaneInfo); i++)
				firstDBTControlId += tabbedPanes[i].GetUsedControlIdsCount();

			return (!aTabbedPaneInfo.DBTInfo.IsSlaveBuffered || isBodyEdit) ? firstDBTControlId : (firstDBTControlId + 1); 
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstTabbedPaneControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			return GetFirstTabbedPaneControlId(aTabbedPaneInfo, false);
		}
		
		//---------------------------------------------------------------------------
		public int GetBodyEditControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if (aTabbedPaneInfo == null || aTabbedPaneInfo.DBTInfo == null || !aTabbedPaneInfo.DBTInfo.IsSlaveBuffered)
				return -1;

			return GetFirstTabbedPaneControlId(aTabbedPaneInfo, true);
		}

		//---------------------------------------------------------------------------
		public int GetBodyEditControlId(WizardDBTInfo aDBTInfo)
		{
			return GetBodyEditControlId(GetOriginalDBTTabbedPane(aDBTInfo));
		}

		//---------------------------------------------------------------------------
		public int GetFirstBodyEditRowFormControlId(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			if 
				(
				aTabbedPaneInfo == null || 
				tabbedPanes == null || 
				tabbedPanes.Count == 0 || 
				!tabbedPanes.Contains(aTabbedPaneInfo)||
				!aTabbedPaneInfo.CreateRowForm
				)
				return -1;

			int firstBodyEditDBTControlId = GetFirstTabbedPaneControlId(aTabbedPaneInfo, true);

			foreach(WizardDBTColumnInfo aColumnInfo in aTabbedPaneInfo.ColumnsInfo)
			{
				if (!aColumnInfo.Visible)
					continue;
					
				firstBodyEditDBTControlId++;
		
				if (aColumnInfo.ShowHotKeyLinkDescription)
					firstBodyEditDBTControlId++;
			}

			return firstBodyEditDBTControlId + 1;
		}
		
		//---------------------------------------------------------------------------
		public int GetFirstBodyEditRowFormControlId(WizardDBTInfo aDBTInfo)
		{
			return GetFirstBodyEditRowFormControlId(GetOriginalDBTTabbedPane(aDBTInfo));
		}
		
		//----------------------------------------------------------------------------
		public bool IsTabberToCreate()
		{
			if (this.TabbedPanesCount > 0)
				return true;

			if (!CreateSlaveFormView)
				return false;

			foreach (WizardDBTInfo aDBTSlaveInfo in dbts)
			{
				if (aDBTSlaveInfo.HasVisibleColums())
					return true;
			}

			return false;
		}
		
		//---------------------------------------------------------------------------
		public int GetTabberControlId()
		{
			if (!IsTabberToCreate())
				return -1;

			return FirstControlId;
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedCommandIdsCount()
		{
			return (ushort)(CreateSlaveFormView ? 1 : 0); 
		}

		//---------------------------------------------------------------------------
		public ushort GetFirstAvailableSymedId()
		{
			return (ushort)(FirstSymedId + GetUsedSymedIdsCount());
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedSymedIdsCount()
		{
			return 0; //@@TODO
		}

		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			if (aHotKeyLinkInfo == null || !aHotKeyLinkInfo.IsDefined)
				return false;

			if (dbts != null && dbts.Count > 0)
			{
				foreach (WizardDBTInfo aDBTSlaveInfo in dbts)
				{
					if (aDBTSlaveInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}
			if (tabbedPanes != null && tabbedPanes.Count > 0)
			{
				foreach (WizardDocumentTabbedPaneInfo aTabbedPaneInfo in tabbedPanes)
				{
					if (aTabbedPaneInfo.IsHotKeyLinkUsed(aHotKeyLinkInfo))
						return true;
				}
			}
			return false;
		}
		
		#endregion
	}

	#endregion

	#region WizardClientDocumentInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardClientDocumentInfoCollection.
	/// </summary>
	public class WizardClientDocumentInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardClientDocumentInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardClientDocumentInfo))
					throw new NotSupportedException();

				this[index] = (WizardClientDocumentInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardClientDocumentInfo))
				throw new NotSupportedException();

			return this.Contains((WizardClientDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardClientDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardClientDocumentInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardClientDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardClientDocumentInfo))
				throw new NotSupportedException();

			Insert(index, (WizardClientDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardClientDocumentInfo))
				throw new NotSupportedException();

			Remove((WizardClientDocumentInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo this[int index]
		{
			get {  return (WizardClientDocumentInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardClientDocumentInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardClientDocumentInfo[] ToArray()
		{
			return (WizardClientDocumentInfo[])InnerList.ToArray(typeof(WizardClientDocumentInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardClientDocumentInfo aClientDocumentToAdd)
		{
			if (Contains(aClientDocumentToAdd))
				return IndexOf(aClientDocumentToAdd);

			return InnerList.Add(aClientDocumentToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardClientDocumentInfoCollection aClientDocumentsCollectionToAdd)
		{
			if (aClientDocumentsCollectionToAdd == null || aClientDocumentsCollectionToAdd.Count == 0)
				return;

			foreach (WizardClientDocumentInfo aClientDocumentToAdd in aClientDocumentsCollectionToAdd)
				Add(aClientDocumentToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardClientDocumentInfo aClientDocumentToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aClientDocumentToInsert))
				return;

			InnerList.Insert(index, aClientDocumentToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardClientDocumentInfo beforeClientDocument, WizardClientDocumentInfo aClientDocumentToInsert)
		{
			if (beforeClientDocument == null)
				Add(aClientDocumentToInsert);

			if (!Contains(beforeClientDocument))
				return;

			if (Contains(aClientDocumentToInsert))
				return;

			Insert(IndexOf(beforeClientDocument), aClientDocumentToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardClientDocumentInfo aClientDocumentToRemove)
		{
			if (!Contains(aClientDocumentToRemove))
				return;

			InnerList.Remove(aClientDocumentToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardClientDocumentInfo aClientDocumentToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aClientDocumentToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardClientDocumentInfo aClientDocumentToSearch)
		{
			if (!Contains(aClientDocumentToSearch))
				return -1;
			
			return InnerList.IndexOf(aClientDocumentToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardClientDocumentInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardClientDocumentInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardClientDocumentInfo.Equals(this[i], ((WizardClientDocumentInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion

	#region WizardDocumentTabbedPaneInfo class
	
	//=================================================================================
	/// <summary>
	/// Summary description for WizardDocumentTabbedPaneInfo.
	/// </summary>
	public class WizardDocumentTabbedPaneInfo : IDisposable
	{
		#region WizardDocumentTabbedPaneInfo private data members

		private WizardDBTInfo					dbtInfo = null;
		private string							title = String.Empty;
		private WizardDBTColumnInfoCollection	managedColumns = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool disposed = false;
		private int width;
		private int height;
		private List<LabelInfo> labelInfoCollection;
		#endregion

		public int Width { get { return width; } set { width = value; } }
		public int Height { get { return height; } set { height = value; } }
		public List<LabelInfo> LabelInfoCollection	{ get { return labelInfoCollection; } set { labelInfoCollection = value; } }

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo(WizardDBTInfo aDBTInfo, bool isReadOnly)
		{
			dbtInfo = aDBTInfo;
			readOnly = isReadOnly;
			width = height = 0;
			labelInfoCollection = new List<LabelInfo>();
			if (dbtInfo != null)
			{
				title = dbtInfo.SlaveTabTitle;
				if (title == null || title.Length == 0)
					title = dbtInfo.Name;
			}
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo(WizardDBTInfo aDBTInfo) : this(aDBTInfo, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo() : this((WizardDBTInfo)null)
		{
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo(WizardDocumentTabbedPaneInfo aTabbedPaneInfo)
		{
			dbtInfo = (aTabbedPaneInfo != null) ? aTabbedPaneInfo.DBTInfo : null;
		
			title = (aTabbedPaneInfo != null) ? aTabbedPaneInfo.Title : String.Empty;

			readOnly = (aTabbedPaneInfo != null) ? aTabbedPaneInfo.ReadOnly : false;
	
			if (aTabbedPaneInfo != null && aTabbedPaneInfo.ManagedColumnsCount > 0)
			{
				foreach(WizardDBTColumnInfo aColumnInfo in aTabbedPaneInfo.ManagedColumns)
					this.AddManagedColumn(new WizardDBTColumnInfo(aColumnInfo));
			}

			width = aTabbedPaneInfo.width;
			height = aTabbedPaneInfo.height;

			aTabbedPaneInfo.labelInfoCollection.ForEach( l => {
				labelInfoCollection.Add(new LabelInfo(l));
			});
			
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this.disposed)
			{
				//@@TODO

				if (disposing)
					GC.SuppressFinalize(this);
			}
			disposed = true;         
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDocumentTabbedPaneInfo))
				return false;

			if (obj == this)
				return true;

			return 
				(
				WizardDBTInfo.Equals(dbtInfo, ((WizardDocumentTabbedPaneInfo)obj).DBTInfo) &&
				String.Compare(title, ((WizardDocumentTabbedPaneInfo)obj).Title) == 0 &&
				WizardDBTColumnInfoCollection.Equals(managedColumns, ((WizardDocumentTabbedPaneInfo)obj).ManagedColumns)
				);
		}
		
		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		#region WizardDocumentTabbedPaneInfo public properties

		//---------------------------------------------------------------------------
		public WizardDBTInfo DBTInfo 
		{
			get { return dbtInfo; } 

			set 
			{
				if (WizardDBTInfo.Equals(dbtInfo, value))
					return;

				dbtInfo = value;

				RemoveAllManagedColumns();
			}
		}

		//---------------------------------------------------------------------------
		public string Title { get { return title; } set { title = value; } }

		//---------------------------------------------------------------------------
		public string DBTName { get { return (dbtInfo != null) ? dbtInfo.Name : String.Empty; } }

		//---------------------------------------------------------------------------
		public string TableName { get { return (dbtInfo != null) ? dbtInfo.TableName : String.Empty; } }

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfoCollection ManagedColumns { get { return managedColumns; } }

		//---------------------------------------------------------------------------
		public int ManagedColumnsCount { get { return (managedColumns != null) ? managedColumns.Count : 0; } }

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfoCollection ColumnsInfo 
		{
			get 
			{ 
				if (dbtInfo == null)
					return null;

				return (managedColumns != null) ? managedColumns : dbtInfo.ColumnsInfo; 
			} 
		}
		
		//---------------------------------------------------------------------------
		public int ColumnsCount
		{
			get 
			{ 
				if (dbtInfo == null)
					return 0;

				return (managedColumns != null) ? managedColumns.Count : dbtInfo.ColumnsCount; 
			} 
		}

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		
		//---------------------------------------------------------------------------
		public bool CreateRowForm
		{
			get 
			{
				if (this.ManagedColumnsCount > 0)
					return false; 
				
				return (dbtInfo != null && dbtInfo.CreateRowForm); 
			} 
		}
		
		//---------------------------------------------------------------------------
		public bool ShowsAdditionalColumns
		{
			get 
			{
				if (dbtInfo == null || dbtInfo.Library == null)
					return false;

				WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
				if (columns == null || columns.Count == 0)
					return false;

				WizardTableInfo tableInfo = GetTableInfo();
				if (tableInfo == null)
					return false;

				WizardTableColumnInfoCollection additionalColumnsInfo = dbtInfo.Library.GetAllAvailableExtraAddedColumns(tableInfo);
				if (additionalColumnsInfo == null || additionalColumnsInfo.Count == 0)
					return false;
			
				foreach(WizardTableColumnInfo anAdditionalColumnInfo in additionalColumnsInfo)
				{
					WizardDBTColumnInfo additionalColumnInfo = GetColumnInfoByName(anAdditionalColumnInfo.Name);
					if (additionalColumnInfo != null && additionalColumnInfo.Visible)
						return true;
				}

				return false; 
			}
		}
		
		#endregion

		#region WizardDocumentTabbedPaneInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			string tabbedPaneDescription = this.Title.Trim();
			if (tabbedPaneDescription == null || tabbedPaneDescription.Length == 0)
				tabbedPaneDescription = "?";

			if (dbtInfo != null && dbtInfo.Name != null && dbtInfo.Name.Length > 0)
				tabbedPaneDescription += " (" + dbtInfo.Name + ")";

			return tabbedPaneDescription;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableInfo()
		{
			return (dbtInfo != null) ? dbtInfo.GetTableInfo() : null;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetTableColumnInfoByName(string aColumnName)
		{
			return (dbtInfo != null) ? dbtInfo.GetTableColumnInfoByName(aColumnName) : null;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetTableColumnInfoByName(string aColumnName, ref int columnIndex)
		{
			return (dbtInfo != null) ? dbtInfo.GetTableColumnInfoByName(aColumnName, ref columnIndex) : null;
		}
		
		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkAvailable(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			return (dbtInfo != null && dbtInfo.Library != null && dbtInfo.Library.IsHotKeyLinkAvailable(aHotKeyLinkInfo));
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo GetColumnInfoByName(string aColumnName)
		{
			if (dbtInfo == null || aColumnName == null || aColumnName.Length == 0)
				return null;
			
			return (this.ManagedColumnsCount > 0) ? GetManagedColumnInfoByName(aColumnName) : dbtInfo.GetColumnInfoByName(aColumnName);
		}
		
		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo GetManagedColumnInfoByName(string aColumnName)
		{
			if (managedColumns == null || managedColumns.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return null;

			foreach(WizardDBTColumnInfo aColumnInfo in managedColumns)
			{
				if (String.Compare(aColumnName, aColumnInfo.ColumnName, true) == 0)
					return aColumnInfo;
			}
			
			return null;
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo AddManagedColumn(string aColumnName, WizardHotKeyLinkInfo hotKeyLinkInfo, bool showDescription)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return null;

			WizardTableColumnInfo tableColumnInfo = GetTableColumnInfoByName(aColumnName);
			if (tableColumnInfo == null)
				return null;
			
			WizardDBTColumnInfo columnInfo = GetManagedColumnInfoByName(aColumnName);
			if (columnInfo == null)
			{
				columnInfo = new WizardDBTColumnInfo(tableColumnInfo);

				if (managedColumns == null)
					managedColumns = new WizardDBTColumnInfoCollection();

				if (managedColumns.Add(columnInfo) < 0)
					return null;
			}

			if (
				hotKeyLinkInfo != null &&
				hotKeyLinkInfo.IsDefined &&
				IsHotKeyLinkAvailable(hotKeyLinkInfo) &&
				tableColumnInfo.DataType.Type == hotKeyLinkInfo.CodeColumn.DataType.Type &&
				tableColumnInfo.DataLength == hotKeyLinkInfo.CodeColumn.DataLength
				)
			{
				columnInfo.HotKeyLink = hotKeyLinkInfo;
				columnInfo.ShowHotKeyLinkDescription = showDescription;
			}

			return columnInfo;
		}
		
		//---------------------------------------------------------------------------
		public bool AddManagedColumn(WizardDBTColumnInfo aColumnInfo)
		{
			if (aColumnInfo == null || aColumnInfo.ColumnName == null || aColumnInfo.ColumnName.Length == 0)
				return false;

			WizardTableColumnInfo tableColumnInfo = GetTableColumnInfoByName(aColumnInfo.ColumnName);
			if (tableColumnInfo == null)
				return false;
			
			if (GetManagedColumnInfoByName(aColumnInfo.ColumnName) != null)
				return false;
			
			if (managedColumns == null)
				managedColumns = new WizardDBTColumnInfoCollection();

			int addedIdx = managedColumns.Add(aColumnInfo);

			return (addedIdx >= 0);
		}

		//---------------------------------------------------------------------------
		public WizardDBTColumnInfo AddManagedColumn(string aColumnName)
		{
			return AddManagedColumn(aColumnName, null, false);
		}
		
		//---------------------------------------------------------------------------
		public void RemoveAllManagedColumns()
		{
			if (managedColumns != null)
				managedColumns.Clear();
		}
		
		//---------------------------------------------------------------------------
		public ushort GetUsedControlIdsCount()
		{
			if (dbtInfo == null)
				return 0;

			WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
			if (columns == null || columns.Count == 0)
				return 0;

			// Se il DBT è di tipo SlaveBuffered si deve anche tenere conto del bodyEdit
			ushort controlsCount = (ushort)(dbtInfo.IsSlaveBuffered ? 1 : 0);

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.Visible)
				{
					controlsCount++;
					if (aColumnInfo.ShowHotKeyLinkDescription)
						controlsCount++;
				}
				
				// Se si tratta di uno SlaveBuffered e viene anche creata la finestra di 
				// dettaglio sulla riga devo anche contare i control contenuti in tale
				// finestra: in essa vengono gestiti tutti i campi della tabella alla 
				// quale è riferito il DBT (compresi quelli che non sono visibili, cioè 
				// per i quali non è stata inserita alcuna colonna corrispondente nel
				// BodyEdit), ma vengono comunque scartati i segmenti di chiave esterna 
				// utilizzati dal questo DBT per "agganciarsi" alla tabella master
				if (this.CreateRowForm && !aColumnInfo.ForeignKeySegment)
					controlsCount++;
			}

			return controlsCount; 
		}

		//---------------------------------------------------------------------------
		public bool HasVisibleColums()
		{
			WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.Visible)
					return true;
			}
			
			return false;
		}

		//---------------------------------------------------------------------------
		public bool HasFindableColums()
		{
			WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.Findable)
					return true;
			}
			
			return false;
		}
		
		//---------------------------------------------------------------------------
		public bool HasHKLDefinedColumns()
		{ 
			WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.IsHKLDefined)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool IsHotKeyLinkUsed(WizardHotKeyLinkInfo aHotKeyLinkInfo)
		{
			WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
			if (columns == null || columns.Count == 0)
				return false;

			return columns.IsHotKeyLinkUsed(aHotKeyLinkInfo);
		}
		
		//---------------------------------------------------------------------------
		public bool HasVisibleAdditionalColumns(WizardExtraAddedColumnsInfo aExtraAddedColumnsInfo)
		{
			if 
				(
				dbtInfo == null || 
				dbtInfo.Library == null || 
				aExtraAddedColumnsInfo == null ||
				aExtraAddedColumnsInfo.Library == null ||
				(
				String.Compare(aExtraAddedColumnsInfo.Library.GetNameSpace(), dbtInfo.Library.GetNameSpace()) != 0 && 
				!dbtInfo.Library.DependsOn(aExtraAddedColumnsInfo.Library)
				) ||
				aExtraAddedColumnsInfo.ColumnsCount == 0 ||
				String.Compare(aExtraAddedColumnsInfo.TableName, this.TableName, true) != 0
				)
				return false;

			WizardDBTColumnInfoCollection columns = this.ColumnsInfo;
			if (columns == null || columns.Count == 0)
				return false;

			foreach(WizardDBTColumnInfo aColumnInfo in columns)
			{
				if (!aColumnInfo.Visible)
					continue;

				WizardTableColumnInfo tableColumn = aExtraAddedColumnsInfo.GetColumnInfoByName(aColumnInfo.ColumnName);
				if (tableColumn != null)
					return true;
			}
			
			return false; 
		} 
		
		//---------------------------------------------------------------------------
		public string GetInternalName(object tabbedPaneContext)
		{
			if (dbtInfo == null) 
				return String.Empty;
			
			if (this.ManagedColumnsCount == 0)
				return this.DBTName;

			if (tabbedPaneContext == null)
				return String.Empty;

			int tabbedPaneIndex = -1;

			if (tabbedPaneContext is WizardDocumentInfo)
			{
				if (((WizardDocumentInfo)tabbedPaneContext).TabbedPanesCount > 0)
					tabbedPaneIndex = ((WizardDocumentInfo)tabbedPaneContext).TabbedPanes.IndexOf(this);
			}
			else if (tabbedPaneContext is WizardClientDocumentInfo)
			{
				if (((WizardClientDocumentInfo)tabbedPaneContext).TabbedPanesCount > 0)
					tabbedPaneIndex = ((WizardClientDocumentInfo)tabbedPaneContext).TabbedPanes.IndexOf(this);
			}
			if (tabbedPaneIndex < 0)
				return String.Empty;
	
			return this.DBTName + tabbedPaneIndex.ToString();
		}
		
		//---------------------------------------------------------------------------
		public bool SetHotKeyLink(string aColumnName, WizardHotKeyLinkInfo hotKeyLinkInfo, bool showDescription)
		{
			if (
				aColumnName == null ||
				aColumnName.Length == 0 ||
				hotKeyLinkInfo == null ||
				!hotKeyLinkInfo.IsDefined ||
				!IsHotKeyLinkAvailable(hotKeyLinkInfo)
				)
				return false;

			WizardTableColumnInfo tableColumnInfo = this.GetTableColumnInfoByName(aColumnName);
			if 
				(
				tableColumnInfo == null ||
				tableColumnInfo.DataType.Type != hotKeyLinkInfo.CodeColumn.DataType.Type ||
				tableColumnInfo.DataLength != hotKeyLinkInfo.CodeColumn.DataLength
				)
				return false;
			
			WizardDBTColumnInfo columnInfo = GetColumnInfoByName(aColumnName);
			if (columnInfo == null)
				return false;

			columnInfo.HotKeyLink = hotKeyLinkInfo;
			columnInfo.ShowHotKeyLinkDescription = showDescription;

			return true;
		}

		#endregion

	}
	
	#endregion

	#region WizardDocumentTabbedPaneInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardDocumentTabbedPaneInfoCollection.
	/// </summary>
	public class WizardDocumentTabbedPaneInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is WizardDocumentTabbedPaneInfo))
					throw new NotSupportedException();

				this[index] = (WizardDocumentTabbedPaneInfo)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDocumentTabbedPaneInfo))
				throw new NotSupportedException();

			return this.Contains((WizardDocumentTabbedPaneInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardDocumentTabbedPaneInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDocumentTabbedPaneInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardDocumentTabbedPaneInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is WizardDocumentTabbedPaneInfo))
				throw new NotSupportedException();

			Insert(index, (WizardDocumentTabbedPaneInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardDocumentTabbedPaneInfo))
				throw new NotSupportedException();

			Remove((WizardDocumentTabbedPaneInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion
		
		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo this[int index]
		{
			get {  return (WizardDocumentTabbedPaneInfo)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (WizardDocumentTabbedPaneInfo)value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardDocumentTabbedPaneInfo[] ToArray()
		{
			return (WizardDocumentTabbedPaneInfo[])InnerList.ToArray(typeof(WizardDocumentTabbedPaneInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardDocumentTabbedPaneInfo aTabbedPaneToAdd)
		{
			if (Contains(aTabbedPaneToAdd))
				return IndexOf(aTabbedPaneToAdd);

			return InnerList.Add(aTabbedPaneToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardDocumentTabbedPaneInfoCollection aTabsCollectionToAdd)
		{
			if (aTabsCollectionToAdd == null || aTabsCollectionToAdd.Count == 0)
				return;

			foreach (WizardDocumentTabbedPaneInfo aTabbedPaneToAdd in aTabsCollectionToAdd)
				Add(aTabbedPaneToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardDocumentTabbedPaneInfo aTabbedPaneToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aTabbedPaneToInsert))
				return;

			InnerList.Insert(index, aTabbedPaneToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardDocumentTabbedPaneInfo beforeTab, WizardDocumentTabbedPaneInfo aTabbedPaneToInsert)
		{
			if (beforeTab == null)
				Add(aTabbedPaneToInsert);

			if (!Contains(beforeTab))
				return;

			if (Contains(aTabbedPaneToInsert))
				return;

			Insert(IndexOf(beforeTab), aTabbedPaneToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardDocumentTabbedPaneInfo aTabbedPaneToRemove)
		{
			if (!Contains(aTabbedPaneToRemove))
				return;

			InnerList.Remove(aTabbedPaneToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(WizardDocumentTabbedPaneInfo aTabbedPaneToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aTabbedPaneToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardDocumentTabbedPaneInfo aTabbedPaneToSearch)
		{
			if (!Contains(aTabbedPaneToSearch))
				return -1;
			
			return InnerList.IndexOf(aTabbedPaneToSearch);
		}
	
		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardDocumentTabbedPaneInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardDocumentTabbedPaneInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardDocumentTabbedPaneInfo.Equals(this[i], ((WizardDocumentTabbedPaneInfoCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}
	}

	#endregion

}
