using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBWizardProjects
{
	#region WizardTableInfo class

	//=================================================================================
	/// <summary>
	/// Summary description for WizardTableInfo.
	/// </summary>
	public class WizardTableInfo : IDisposable
	{
		private const string PrimaryKeyConstraintNamePrefix = "PK_";
		private const string ColumnDefaultConstraintNamePrefix = "DF_";
		private const string ForeignKeyConstraintNamePrefix = "FK_";

		#region WizardTableInfo private data members

		private WizardLibraryInfo library = null;

		private string name = String.Empty;

		private string tbNameSpace = String.Empty;

		private string className = String.Empty;

		private uint creationDbReleaseNumber = 0;

		private TableHistoryInfo history = null;

		private WizardTableColumnInfoCollection columns = null;

		private bool addTBGuidColumn = false;

		private WizardHotKeyLinkInfo hotKeyLink = null;

		private string primaryKeyConstraintName = String.Empty;
		private bool primaryKeyClustered = true;
		private WizardForeignKeyInfoCollection foreignKeys = null;

		private IList<WizardTableIndexInfo> indexes = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		// serve per decidere se aggiungere in automatico in coda allo script di generazione della tabella
        // anche le colonne obbligatorie TBCreated, TBModified, TBCreatedID, TBModifiedID
        // (previste solo x il database aziendale e non in quello di sistema)
		private bool addMandatoryColumns = true;

		#endregion

		# region Constructors
		//---------------------------------------------------------------------------
		public WizardTableInfo(string aTableName, bool isReadOnly, bool isReferenced)
		{
			Name = aTableName;
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo(string aTableName, bool isReadOnly)
			: this(aTableName, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo(string aTableName)
			: this(aTableName, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo(WizardTableInfo aTableInfo, bool setDefaultConstraintName)
		{
			library = (aTableInfo != null) ? aTableInfo.Library : null;

			name = (aTableInfo != null) ? aTableInfo.Name : String.Empty;

			className = (aTableInfo != null) ? aTableInfo.ClassName : String.Empty;

			creationDbReleaseNumber = (aTableInfo != null) ? aTableInfo.CreationDbReleaseNumber : 0;

			history = (aTableInfo != null && aTableInfo.History != null) ? new TableHistoryInfo(aTableInfo.History) : null;

			addTBGuidColumn = (aTableInfo != null) ? aTableInfo.AddTBGuidColumn : false;

			hotKeyLink = (aTableInfo != null && aTableInfo.HotKeyLink != null) ? new WizardHotKeyLinkInfo(aTableInfo.HotKeyLink) : null;

			primaryKeyConstraintName = (aTableInfo != null) ? aTableInfo.PrimaryKeyConstraintName : String.Empty;

			readOnly = (aTableInfo != null) ? aTableInfo.ReadOnly : false;
			referenced = (aTableInfo != null) ? aTableInfo.IsReferenced : false;

			if (aTableInfo != null && aTableInfo.ColumnsInfo != null && aTableInfo.ColumnsInfo.Count > 0)
			{
				foreach (WizardTableColumnInfo aColumnInfo in aTableInfo.ColumnsInfo)
					this.AddColumnInfo(new WizardTableColumnInfo(aColumnInfo), setDefaultConstraintName);
			}

			if (aTableInfo != null && aTableInfo.ForeignKeysCount > 0)
			{
				foreach (WizardForeignKeyInfo aForeignKeyInfo in aTableInfo.ForeignKeys)
					this.AddForeignKeyInfo(new WizardForeignKeyInfo(aForeignKeyInfo));
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo(WizardTableInfo aTableInfo)
			: this(aTableInfo, true)
		{
		}
		# endregion

		# region Dispose methods
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
		# endregion

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardTableInfo))
				return false;

			if (obj == this)
				return true;

			return
				(
				String.Compare(name, ((WizardTableInfo)obj).Name) == 0 &&
				String.Compare(className, ((WizardTableInfo)obj).ClassName) == 0 &&
				creationDbReleaseNumber == ((WizardTableInfo)obj).CreationDbReleaseNumber &&
				addTBGuidColumn == ((WizardTableInfo)obj).AddTBGuidColumn &&
				TableHistoryInfo.Equals(history, ((WizardTableInfo)obj).History) &&
				WizardHotKeyLinkInfo.Equals(hotKeyLink, ((WizardTableInfo)obj).HotKeyLink) &&
				WizardTableColumnInfoCollection.Equals(columns, ((WizardTableInfo)obj).ColumnsInfo) &&
				WizardForeignKeyInfoCollection.Equals(foreignKeys, ((WizardTableInfo)obj).ForeignKeys)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo, bool autoDbRelease, bool refreshColumnDefaultConstraintNames)
		{
			if (library == aLibraryInfo)
				return;

			if (library != null && library.TablesInfo.Contains(this))
				library.TablesInfo.Remove(this);

			library = aLibraryInfo;

			if (autoDbRelease)
				SetCreationDbReleaseNumber((library != null && library.Module != null) ? library.Module.DbReleaseNumber : 0);

			RefreshPrimaryKeyConstraintName();

			if (refreshColumnDefaultConstraintNames)
				RefreshColumnDefaultConstraintNames();
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo, bool autoDbRelease)
		{
			SetLibrary(aLibraryInfo, autoDbRelease, true);
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo)
		{
			SetLibrary(aLibraryInfo, true, true);
		}

		//---------------------------------------------------------------------------
		internal void SetCreationDbReleaseNumber(uint aReleaseNumber, bool forceSetting)
		{
			if (creationDbReleaseNumber == aReleaseNumber ||
				(!forceSetting && creationDbReleaseNumber > 0 && aReleaseNumber < creationDbReleaseNumber))
				return;

			creationDbReleaseNumber = aReleaseNumber;

			if (columns == null || columns.Count == 0)
				return;

			foreach (WizardTableColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.CreationDbReleaseNumber != aReleaseNumber &&
					(aColumnInfo.CreationDbReleaseNumber == 0 || aReleaseNumber > aColumnInfo.CreationDbReleaseNumber))
					aColumnInfo.CreationDbReleaseNumber = aReleaseNumber;
			}
		}

		//---------------------------------------------------------------------------
		internal void SetCreationDbReleaseNumber(uint aReleaseNumber)
		{
			SetCreationDbReleaseNumber(aReleaseNumber, false);
		}

		#region WizardTableInfo public properties

		//---------------------------------------------------------------------------
		public WizardLibraryInfo Library { get { return library; } }
		//---------------------------------------------------------------------------
		public string TbNameSpace
		{
			get { return tbNameSpace; }
			set { tbNameSpace = value; }
		}

		//---------------------------------------------------------------------------
		public string Name
		{
			get { return name.Trim(); }
			set
			{
				if (Generics.IsValidTableName(value))
				{
					name = value.Trim();

					if (className == null || className.Length == 0)
						className = GetDefaultTableClassName(name);
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
					className = GetDefaultTableClassName(name);
				else
					className = value.Trim();
			}
		}

		//---------------------------------------------------------------------------
		public uint CreationDbReleaseNumber { get { return creationDbReleaseNumber; } set { creationDbReleaseNumber = value; } }

		//---------------------------------------------------------------------------
		public TableHistoryInfo History { get { return history; } }

		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection ColumnsInfo { get { return columns; } }

		//---------------------------------------------------------------------------
		public int ColumnsCount
		{
			get
			{
				int columnsCount = (columns != null) ? columns.Count : 0;
				if (addTBGuidColumn)
					columnsCount++;
				return columnsCount;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsPrimaryKeyDefined
		{
			get
			{
				if (columns == null || columns.Count == 0)
					return false;

				return columns.IsPrimaryKeyDefined;
			}
		}

        //---------------------------------------------------------------------------
        public bool CanDefineTR
        {
            get
            {
                if (columns == null || columns.Count == 0 || !columns.IsPrimaryKeyDefined)
                    return false;

                bool stringPrimaryKeySegmentAvailable = false;
                foreach (WizardTableColumnInfo aColumnInfo in columns)
                {
                    if (!stringPrimaryKeySegmentAvailable && aColumnInfo.IsPrimaryKeySegment)
                        stringPrimaryKeySegmentAvailable = true;
                }

                return stringPrimaryKeySegmentAvailable;
            }
        }

        //---------------------------------------------------------------------------
        public bool CanDefineHKL
        {
            get
            {
                if (columns == null || columns.Count == 0 || !columns.IsPrimaryKeyDefined)
                    return false;

                bool stringPrimaryKeySegmentAvailable = false;
                bool stringDescriptiveFieldAvailable = false;
                foreach (WizardTableColumnInfo aColumnInfo in columns)
                {
                    if (!stringPrimaryKeySegmentAvailable && aColumnInfo.IsPrimaryKeySegment)
                        stringPrimaryKeySegmentAvailable = true;
                    else if (aColumnInfo.DataType.IsTextual)
                        stringDescriptiveFieldAvailable = true;
                }

                return stringPrimaryKeySegmentAvailable && stringDescriptiveFieldAvailable;
            }
        }

        //---------------------------------------------------------------------------
		public bool AddTBGuidColumn { get { return addTBGuidColumn; } set { addTBGuidColumn = value; } }
		
		//---------------------------------------------------------------------------
		public bool AddMandatoryColumns { get { return addMandatoryColumns; } set { addMandatoryColumns = value; } }

		//---------------------------------------------------------------------------
		public WizardHotKeyLinkInfo HotKeyLink
		{
			get { return hotKeyLink; }
		}

		//---------------------------------------------------------------------------
		public bool IsTRDefined
		{
			get
			{
				return
					(
                        true
					);
			}
		}

        //---------------------------------------------------------------------------
        public string TRClassName
        {
            get
            {
                if (!IsTRDefined)
                    return String.Empty;

                // se il nome della classe inizia con "T" 
                // allora "TR" + mid da 1 in poi della classe
                // altrimenti "TR" + namespace
                if (this.ClassName.Substring(0, 1) == "T")
                    return "TR" + this.ClassName.Substring(1);
                else
                    return "TR" + this.Name;
            }
/*
            set
            {
                if (IsHKLDefined)
                    hotKeyLink.ClassName = value;
            }
*/
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
		public string HKLNamespace
		{
			get
			{
				if (!IsHKLDefined || library == null)
					return String.Empty;

				return library.GetHotKeyLinkNamespace(hotKeyLink);
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
			get { return this.GetColumnInfoByName(this.HKLCodeColumnName); }
			set
			{
				if
					(
					columns == null ||
					columns.Count == 0 ||
					(value != null && this.GetColumnInfoByName(value.Name) == null)
					)
					return;

				if (hotKeyLink == null)
				{
					hotKeyLink = new WizardHotKeyLinkInfo(this);
					hotKeyLink.ClassName = "HKL" + name;
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
			get { return this.GetColumnInfoByName(this.HKLDescriptionColumnName); }
			set
			{
				if
					(
					columns == null ||
					columns.Count == 0 ||
					(value != null && this.GetColumnInfoByName(value.Name) == null)
					)
					return;

				if (hotKeyLink == null)
				{
					hotKeyLink = new WizardHotKeyLinkInfo(this);
					hotKeyLink.ClassName = "HKL" + name;
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
		public string PrimaryKeyConstraintName
		{
			get
			{
				return (primaryKeyConstraintName != null) ? primaryKeyConstraintName.Trim() : String.Empty;
			}
			set
			{
				if (Generics.IsValidDBObjectName(value))
					primaryKeyConstraintName = value.Trim();
			}
		}

		//---------------------------------------------------------------------------
		public bool PrimaryKeyClustered { get { return primaryKeyClustered; } set { primaryKeyClustered = value; } }

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfoCollection ForeignKeys { get { return foreignKeys; } }

		//---------------------------------------------------------------------------
		public int ForeignKeysCount { get { return (foreignKeys != null) ? foreignKeys.Count : 0; } }

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)

		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } }

		//---------------------------------------------------------------------------
		public IList<WizardTableIndexInfo> Indexes { get { return indexes; } set { indexes = value; } }

		#endregion

		#region WizardTableInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetColumnInfoByName(string aColumnName, ref int columnIndex)
		{
			if (aColumnName == null || aColumnName.Length == 0)
				return null;

			if (addTBGuidColumn && String.Compare(aColumnName, Generics.TBGuidColumnName) == 0)
			{
				columnIndex = (columns != null) ? columns.Count : 0;
				return GetTBGuidColumnInfo();
			}

			if (columns == null || columns.Count == 0)
				return null;

			return columns.GetColumnInfoByName(aColumnName, ref columnIndex);
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetColumnInfoByName(string aColumnName)
		{
			int columnIndex = -1;
			return GetColumnInfoByName(aColumnName, ref columnIndex);
		}

		//---------------------------------------------------------------------------
		public int AddColumnInfo(WizardTableColumnInfo aColumnInfo, bool setDefaultConstraintName)
		{
			if (aColumnInfo == null || aColumnInfo.Name == null || aColumnInfo.Name.Length == 0)
				return -1;

			WizardTableColumnInfo alreadyExistingColumn = GetColumnInfoByName(aColumnInfo.Name);
			if (alreadyExistingColumn != null)
				return -1;

			if (columns == null)
				columns = new WizardTableColumnInfoCollection();

			int addedColumnIndex = columns.Add(aColumnInfo);
			if (addedColumnIndex >= 0 && library != null && !library.ReadOnly && !referenced && setDefaultConstraintName && (aColumnInfo.DefaultConstraintName == null || aColumnInfo.DefaultConstraintName.Length == 0))
				columns[addedColumnIndex].DefaultConstraintName = GetColumnDefaultConstraintDefaultName(columns[addedColumnIndex]);

			return addedColumnIndex;
		}

		//---------------------------------------------------------------------------
		public int AddColumnInfo(WizardTableColumnInfo aColumnInfo)
		{
			return AddColumnInfo(aColumnInfo, true);
		}

		//---------------------------------------------------------------------------
		public void RemoveColumn(string aColumnName)
		{
			if (columns == null || columns.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return;

			WizardTableColumnInfo columnToRemove = GetColumnInfoByName(aColumnName);
			if (columnToRemove == null || IsColumnUsedAsForeignKeySegment(columnToRemove))
				return;

			columns.Remove(columnToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllColumns()
		{
			if (columns == null || columns.Count == 0)
				return;

			RemoveAllForeignKeys();

			columns.Clear();
		}

		//---------------------------------------------------------------------------
		public bool IsPrimaryKeySegment(string aColumnName)
		{
			WizardTableColumnInfo columnInfo = GetColumnInfoByName(aColumnName);

			return (columnInfo != null) ? columnInfo.IsPrimaryKeySegment : false;
		}

		//---------------------------------------------------------------------------
		public bool HasSameColumns(WizardTableColumnInfoCollection columnsToCompare)
		{
			if (columns == null)
				return (columnsToCompare == null || columnsToCompare.Count == 0);

			return columns.HasSameColumns(columnsToCompare);
		}

		//---------------------------------------------------------------------------
		public bool ContainsUpperCaseStringColumns()
		{
			return (columns != null) ? columns.ContainsUpperCaseStringColumns() : false;
		}

		//---------------------------------------------------------------------------
		public bool IsUsingEnum(WizardEnumInfo aEnumInfo)
		{
			return (columns != null) ? columns.IsUsingEnum(aEnumInfo) : false;
		}

		//---------------------------------------------------------------------------
		public bool ContainsDataEnumColumns()
		{
			return (columns != null) ? columns.ContainsDataEnumColumns() : false;
		}

		//---------------------------------------------------------------------------
		public void RefreshPrimaryKeyConstraintName(bool forceReset)
		{
			if (forceReset || primaryKeyConstraintName == null || primaryKeyConstraintName.Trim().Length == 0)
				this.PrimaryKeyConstraintName = GetTablePrimaryKeyConstraintDefaultName();
		}

		//---------------------------------------------------------------------------
		public void RefreshPrimaryKeyConstraintName()
		{
			RefreshPrimaryKeyConstraintName(false);
		}

		//---------------------------------------------------------------------------
		public void RefreshColumnDefaultConstraintNames(bool forceReset)
		{
			if (referenced || columns == null || columns.Count == 0 || library == null)
				return;

			foreach (WizardTableColumnInfo aColumnInfo in columns)
			{
				if (forceReset || aColumnInfo.DefaultConstraintName == null || aColumnInfo.DefaultConstraintName.Length == 0)
					aColumnInfo.DefaultConstraintName = GetColumnDefaultConstraintDefaultName(aColumnInfo);
			}
		}

		//---------------------------------------------------------------------------
		public void RefreshColumnDefaultConstraintNames()
		{
			RefreshColumnDefaultConstraintNames(false);
		}

		//---------------------------------------------------------------------------
		public void RefreshConstraintsNames(bool forceReset)
		{
			if (readOnly || referenced)
				return;

			RefreshPrimaryKeyConstraintName(forceReset);

			RefreshColumnDefaultConstraintNames(forceReset);
		}

		//---------------------------------------------------------------------------
		public void RefreshConstraintsNames()
		{
			RefreshConstraintsNames(false);
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultTableClassName(string aTableName)
		{
			if (aTableName == null || aTableName.Trim().Length == 0)
				return String.Empty;

			return "T" + Generics.SubstitueInvalidCharacterInIdentifier(aTableName.Trim().Replace(' ', '_'));
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultHKLName(string aTableName)
		{
			return WizardHotKeyLinkInfo.GetDefaultHKLName(aTableName);
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultHKLClassName(string aTableName)
		{
			return WizardHotKeyLinkInfo.GetDefaultHKLClassName(aTableName);
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
		public uint GetMaximumDbReleaseNumberUsed()
		{
			if (columns == null || columns.Count == 0)
				return creationDbReleaseNumber;

			uint maxDbReleaseNumber = creationDbReleaseNumber;

			foreach (WizardTableColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo.CreationDbReleaseNumber > maxDbReleaseNumber)
					maxDbReleaseNumber = aColumnInfo.CreationDbReleaseNumber;
			}

			if (history != null && history.StepsCount > 0)
			{
				uint historyMaxDbReleaseNumber = history.GetMaximumDbReleaseNumberUsed();
				if (historyMaxDbReleaseNumber > maxDbReleaseNumber)
					maxDbReleaseNumber = historyMaxDbReleaseNumber;
			}

			return maxDbReleaseNumber;
		}

		//---------------------------------------------------------------------------
		public TableHistoryStep GetNearestDbReleaseStep(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || history == null || history.StepsCount == 0)
				return null;

			return history.GetNearestDbReleaseStep(aDbReleaseNumber);
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo GetTableSchemaAtDbRelease(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || creationDbReleaseNumber > aDbReleaseNumber)
				return null;

			if (history == null || history.StepsCount == 0)
				return this;

			WizardTableInfo historicTableInfo = new WizardTableInfo(this);

			return (historicTableInfo.RollbackToDbRelease(aDbReleaseNumber)) ? historicTableInfo : null;
		}

		//---------------------------------------------------------------------------
		public bool RollbackToDbRelease(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || creationDbReleaseNumber > aDbReleaseNumber)
				return false;

			if (history == null || history.StepsCount == 0)
				return true;

			// Devo partire dalla tabella nel suo stato attuale e riprodurre all'incontrario
			// tutti gli step successivi al numero di release passato come argomento
			TableHistoryStepsCollection stepsAfter = history.GetStepsAfterDbRelease(aDbReleaseNumber);

			if (stepsAfter == null || stepsAfter.Count == 0)
				return true;

			for (int i = (stepsAfter.Count - 1); i >= 0; i--)
			{
				TableHistoryStep stepToRollback = stepsAfter[i];

				history.Steps.Remove(stepToRollback);

				if (stepToRollback.EventsCount == 0)
					continue;

				if (stepToRollback.PrimaryKeyConstraintName != null && stepToRollback.PrimaryKeyConstraintName.Length > 0)
					this.PrimaryKeyConstraintName = stepToRollback.PrimaryKeyConstraintName;

				if (stepToRollback.ColumnsEventsCount > 0)
				{
					foreach (ColumnHistoryEvent aColumnEvent in stepToRollback.ColumnsEvents)
					{
						WizardTableColumnInfo currentColumn = aColumnEvent.ColumnInfo;
						WizardTableColumnInfo previousColumn = aColumnEvent.PreviousColumnInfo;
						if (previousColumn == null)
						{
							if (currentColumn == null)
								continue;

							if (aColumnEvent.Type == TableHistoryStep.EventType.AddColumn)
							{
								// La colonna è stata aggiunta durante questo scatto di release
								if (String.Compare(currentColumn.Name, Generics.TBGuidColumnName, true) != 0)
									RemoveColumn(currentColumn.Name);
								else
									addTBGuidColumn = false;
								continue;
							}
							if (aColumnEvent.Type == TableHistoryStep.EventType.DropColumn)
							{
								// La colonna è stata rimossa durante questo scatto di release
								if (String.Compare(currentColumn.Name, Generics.TBGuidColumnName, true) != 0)
									AddColumnInfo(currentColumn);
								else
									addTBGuidColumn = true;
								continue;
							}
						}
						else
						{
							if (aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType ||
								aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue ||
								aColumnEvent.Type == TableHistoryStep.EventType.ModifyPrimaryKey)
							{
								WizardTableColumnInfo columnToAlter = GetColumnInfoByName(currentColumn.Name);
								if (columnToAlter != null)
								{
									if (aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType &&
										previousColumn.DataType != null &&
										previousColumn.DataType.Type != WizardTableColumnDataType.DataType.Undefined)
									{
										columnToAlter.DataType = previousColumn.DataType;
										columnToAlter.DataLength = previousColumn.DataLength;
									}
									columnToAlter.IsPrimaryKeySegment = previousColumn.IsPrimaryKeySegment;

									// Se per la colonna cambia il valore default e si tratta di un 
									// enumerativo devo anche modificare enumInfo
									if (aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue &&
										columnToAlter.DataType.Type == WizardTableColumnDataType.DataType.Enum)
									{
										columnToAlter.EnumInfo = previousColumn.EnumInfo;
									}
									else
										columnToAlter.DefaultValue = previousColumn.DefaultValue;
								}
								continue;
							}
						}
					}

					// Risistemo l'ordine delle colonne
					foreach (ColumnHistoryEvent aColumnEvent in stepToRollback.ColumnsEvents)
					{
						if (aColumnEvent.Type != TableHistoryStep.EventType.ChangeColumnOrder)
							continue;

						if (aColumnEvent.ColumnInfo == null ||
							aColumnEvent.PreviousColumnOrder < 0 ||
							aColumnEvent.ColumnOrder == aColumnEvent.PreviousColumnOrder)
							continue;

						WizardTableColumnInfo columnToMove = GetColumnInfoByName(aColumnEvent.ColumnInfo.Name);
						if (columnToMove != null)
						{
							columns.Remove(columnToMove);
							columns.Insert(aColumnEvent.PreviousColumnOrder, columnToMove);
						}
					}
				}
			}

			// Se è predisposta la generazione di codice di HotLink, controllo che le 
			// impostazioni siano ancora valide:
			if (hotKeyLink != null)
			{
				if
					(
					(
					hotKeyLink.CodeColumnName != null &&
					hotKeyLink.CodeColumnName.Length > 0 &&
					(!WizardTableColumnInfo.Equals(hotKeyLink.CodeColumn, this.GetColumnInfoByName(hotKeyLink.CodeColumnName)) || !hotKeyLink.CodeColumn.DataType.IsTextual)
					) ||
					(
					hotKeyLink.DescriptionColumnName != null &&
					hotKeyLink.DescriptionColumnName.Length > 0 &&
					(!WizardTableColumnInfo.Equals(hotKeyLink.DescriptionColumn, this.GetColumnInfoByName(hotKeyLink.DescriptionColumnName)) || !hotKeyLink.DescriptionColumn.DataType.IsTextual)
					)
					)
					hotKeyLink = null;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public WizardTableIndexInfo GetPrimaryKeyIndex()
		{
			string indexName = this.PrimaryKeyConstraintName;
			if (indexName == null || indexName.Length == 0)
				indexName = this.GetTablePrimaryKeyConstraintDefaultName();
			if (indexName == null || indexName.Length == 0)
				return null;

			WizardTableIndexInfo primaryIndexInfo = new WizardTableIndexInfo(indexName, true);

			foreach (WizardTableColumnInfo aColumnInfo in columns)
			{
				if (aColumnInfo == null || !aColumnInfo.IsPrimaryKeySegment)
					continue;

				primaryIndexInfo.AddSegmentInfo(aColumnInfo);
			}

			return (primaryIndexInfo.SegmentsCount > 0) ? primaryIndexInfo : null;
		}

		//---------------------------------------------------------------------------
		public void ClearHistory()
		{
			if (history != null)
				history.ClearSteps();

			history = null;
		}

		//---------------------------------------------------------------------------
		private TableHistoryStep BuildHistoryStep
			(
			uint aDbReleaseNumber,
			WizardTableColumnInfoCollection newColumnsInfo,
			bool changeTBGuidExistence,
			bool isTBGuidPresent
			)
		{
			if (aDbReleaseNumber == 0 || (!changeTBGuidExistence && (newColumnsInfo == null || newColumnsInfo.Count == 0)))
				return null;

			TableHistoryStep historyStepToAdd = new TableHistoryStep(aDbReleaseNumber);

			historyStepToAdd.PrimaryKeyConstraintName = this.PrimaryKeyConstraintName;

			bool isPrimaryKeyToModify = false;
			for (int i = 0; i < newColumnsInfo.Count; i++)
			{
				WizardTableColumnInfo aColumnInfo = newColumnsInfo[i];
				if (aColumnInfo == null)
					continue;

				WizardTableColumnInfo existingColumnInfo = GetColumnInfoByName(aColumnInfo.Name);
				if (existingColumnInfo != null)
				{
					// La definizione della colonna esiste già e quindi è stata modificata
					// da un punto di vista strutturale
					if (
						!aColumnInfo.DataType.Equals(existingColumnInfo.DataType) ||
						aColumnInfo.DataLength != existingColumnInfo.DataLength
						)
					{
						historyStepToAdd.AddAlterColumnTypeEvent(aColumnInfo, i, existingColumnInfo);
					}
					else if (!aColumnInfo.HasSameDefaultValueAs(existingColumnInfo))
					{
						historyStepToAdd.AddChangeColumnDefaultValueEvent(aColumnInfo, i, existingColumnInfo);
					}

					int previousColumnIndex = columns.IndexOf(existingColumnInfo);
					if (previousColumnIndex != i) // La colonna è stata spostata
						historyStepToAdd.AddChangeColumnOrderEvent(aColumnInfo, i, previousColumnIndex);

					if (existingColumnInfo.IsPrimaryKeySegment != aColumnInfo.IsPrimaryKeySegment)
						isPrimaryKeyToModify = true;
				}
				else // La definizione della colonna è stata aggiunta
				{
					historyStepToAdd.AddAddColumnEvent(aColumnInfo, i);

					// Controllo se è da modificare anche la composizione della chiave 
					// primaria, cioè se la colonna che viene aggiunta ne fa parte
					if (aColumnInfo.IsPrimaryKeySegment)
						isPrimaryKeyToModify = true;
				}
			}

			// Adesso esamino eventuali colonne rimosse
			if (columns != null && columns.Count > 0)
			{
				for (int i = 0; i < columns.Count; i++)
				{
					bool columnToDrop = true;
					for (int j = 0; j < newColumnsInfo.Count; j++)
					{
						if (String.Compare(columns[i].Name, newColumnsInfo[j].Name, true) == 0)
						{
							columnToDrop = false; // la colonna esiste ancora
							break;
						}
					}
					if (!columnToDrop)
						continue;

					// Controllo se è da modificare anche la composizione della chiave 
					// primaria, cioè se la colonna che viene rimossa ne faceva parte
					if (columns[i].IsPrimaryKeySegment)
						isPrimaryKeyToModify = true;

					// Controllo se è stata eliminata una colonna coinvolta nella definizione
					// di una chiave esterna
					if (IsColumnUsedAsForeignKeySegment(columns[i]))
					{
						foreach (WizardForeignKeyInfo aForeignKeyInfo in foreignKeys)
						{
							WizardForeignKeyInfo.KeySegment segment = aForeignKeyInfo.GetKeySegmentInfoByColumnName(columns[i].Name);
							if (segment != null)
								historyStepToAdd.AddDropForeignKeyConstraintEvent(aForeignKeyInfo);
						}
					}

					historyStepToAdd.AddDropColumnEvent(columns[i], i);
				}
			}

			if (isPrimaryKeyToModify)
			{
				string indexName = this.PrimaryKeyConstraintName;
				if (indexName == null || indexName.Length == 0)
					indexName = this.GetTablePrimaryKeyConstraintDefaultName();
				if (indexName != null && indexName.Length > 0)
				{
					WizardTableIndexInfo newPrimaryIndex = new WizardTableIndexInfo(indexName, true);
					foreach (WizardTableColumnInfo aColumnInfo in newColumnsInfo)
					{
						if (aColumnInfo == null || !aColumnInfo.IsPrimaryKeySegment)
							continue;
						newPrimaryIndex.AddSegmentInfo(aColumnInfo);
					}

					historyStepToAdd.AddIndexEvent(newPrimaryIndex, GetPrimaryKeyIndex(), TableHistoryStep.EventType.ModifyPrimaryKey);
				}
			}

			if (changeTBGuidExistence)
				historyStepToAdd.AddChangeTBGuidExistenceColumnEvent(GetTBGuidColumnInfo(), isTBGuidPresent);

			return historyStepToAdd;
		}

		//---------------------------------------------------------------------------
		public int AddHistoryStep(TableHistoryStep aHistoryStep)
		{
			if
				(
				aHistoryStep == null ||
				aHistoryStep.DbReleaseNumber == 0 ||
				aHistoryStep.DbReleaseNumber == creationDbReleaseNumber ||
				aHistoryStep.EventsCount == 0
				)
				return -1;

			if (history == null)
				history = new TableHistoryInfo();

			return history.AddHistoryStep(aHistoryStep);
		}

		//---------------------------------------------------------------------------
		public int AddHistoryStep(uint aDbReleaseNumber, WizardTableColumnInfoCollection newColumnsInfo, bool changeTBGuidExistence, bool isTBGuidPresent)
		{
			return AddHistoryStep(BuildHistoryStep(aDbReleaseNumber, newColumnsInfo, changeTBGuidExistence, isTBGuidPresent));
		}

		//---------------------------------------------------------------------------
		public int AddHistoryStep(uint aDbReleaseNumber, WizardTableColumnInfoCollection newColumnsInfo)
		{
			return AddHistoryStep(BuildHistoryStep(aDbReleaseNumber, newColumnsInfo, false, false));
		}

		//---------------------------------------------------------------------------
		public int AddCreateForeignKeyHistoryStep(uint aDbReleaseNumber, WizardForeignKeyInfo aForeignKeyInfo)
		{
			if (aDbReleaseNumber == 0 || aForeignKeyInfo == null || aForeignKeyInfo.SegmentsCount == 0)
				return -1;

			TableHistoryStep historyStepToAdd = new TableHistoryStep(aDbReleaseNumber);

			if (historyStepToAdd.AddCreateForeignKeyConstraintEvent(aForeignKeyInfo) == -1)
				return -1;

			return AddHistoryStep(historyStepToAdd);
		}

		//---------------------------------------------------------------------------
		public int AddDropForeignKeyHistoryStep(uint aDbReleaseNumber, WizardForeignKeyInfo aForeignKeyInfo)
		{
			if (aDbReleaseNumber == 0 || aForeignKeyInfo == null || aForeignKeyInfo.ConstraintName == null || aForeignKeyInfo.ConstraintName.Length == 0)
				return -1;

			TableHistoryStep historyStepToAdd = new TableHistoryStep(aDbReleaseNumber);

			if (historyStepToAdd.AddDropForeignKeyConstraintEvent(aForeignKeyInfo) == -1)
				return -1;

			return AddHistoryStep(historyStepToAdd);
		}

		//---------------------------------------------------------------------------
		public bool IsToUpgrade(uint aDbReleaseNumber)
		{
			if (
				history == null ||
				history.StepsCount == 0 ||
				aDbReleaseNumber <= creationDbReleaseNumber
				)
				return false;

			return (history.GetDbReleaseStep(aDbReleaseNumber) != null);
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetTBGuidColumnInfo()
		{
			WizardTableColumnInfo guidColumnInfo = new WizardTableColumnInfo(Generics.TBGuidColumnName, true);
			guidColumnInfo.DataType = new WizardTableColumnDataType(WizardTableColumnDataType.DataType.Guid);
			guidColumnInfo.DefaultValue = Guid.Empty;
			guidColumnInfo.DefaultConstraintName = GetColumnDefaultConstraintDefaultName(guidColumnInfo);
			return guidColumnInfo;
		}

		///<summary>
        /// Se previste sono aggiunte in coda le colonne obbligatorie TBCreated, TBModified, TBCreatedID e TBModifiedID
		///</summary>
		//---------------------------------------------------------------------------
		public void AddTbMandatoryColumnInfo()
		{
            // le colonne obbligatorie TBCreated, TBModified, TBCreatedID e TBModifiedID 
            // sono previste solo x il database aziendale
			this.AddColumnInfo(GetMandatoryColumn(Generics.TBCreatedColumnName));
			this.AddColumnInfo(GetMandatoryColumn(Generics.TBModifiedColumnName));
            this.AddColumnInfo(GetMandatoryColumn(Generics.TBCreatedIDColumnName));
            this.AddColumnInfo(GetMandatoryColumn(Generics.TBModifiedIDColumnName));
        }

		///<summary>
        /// Definizione delle colonne obbligatorie TBCreated, TBModified, TBCreatedID e TBModifiedID 
		///</summary>
		//---------------------------------------------------------------------
		private WizardTableColumnInfo GetMandatoryColumn(string colName)
		{
			WizardTableColumnInfo columnInfo = new WizardTableColumnInfo(colName, false, false, true);
			columnInfo.DataType = new WizardTableColumnDataType(WizardTableColumnDataType.DataType.Date);
			columnInfo.DefaultExpressionValue = "getdate()";
			columnInfo.DefaultConstraintName = String.Format("DF_{0}_{1}_000", this.Name, colName);
			columnInfo.IsNullable = false;
			return columnInfo;
		}

		//---------------------------------------------------------------------
		private int GetNameConflictingCharsCount(ref int conflictsCounter, ref int currentConflictIndex)
		{
			conflictsCounter = 0;
			currentConflictIndex = 0;

			if (library == null || library.Application == null)
				return 0;

			WizardTableInfoCollection applicationTables = library.Application.GetAllTables();
			if (applicationTables == null || applicationTables.Count == 0)
				return 0;

			string tableNameToCompare = name.Trim();

			string upperTableNameToCompare = tableNameToCompare.ToUpper();

			int conflictingCharsCount = 0;
			for (int i = 0; i < applicationTables.Count; i++)
			{
				if (String.Compare(tableNameToCompare, applicationTables[i].Name, true) == 0)
				{
					currentConflictIndex = conflictsCounter;
					continue;
				}

				string upperTableName = applicationTables[i].Name.ToUpper();

				int commonCharsCount = 0;
				for (int charIndex = 0; charIndex < Math.Min(tableNameToCompare.Length, applicationTables[i].Name.Length); charIndex++)
				{
					if (upperTableNameToCompare[charIndex] != upperTableName[charIndex])
						break;
					commonCharsCount++;
				}
				if (commonCharsCount > 0)
					conflictsCounter++;

				if (commonCharsCount > conflictingCharsCount)
					conflictingCharsCount = commonCharsCount;
			}

			return conflictingCharsCount;
		}

		//---------------------------------------------------------------------
		private string TruncateToUniqueName(int charactersMaximumNumber, ref bool collisionResolved)
		{
			collisionResolved = false;

			if
				(
				charactersMaximumNumber <= 0 ||
				library == null ||
				library.Application == null ||
				name == null ||
				name.Trim().Length == 0
				)
				return String.Empty;

			string tableNameToUse = name.Trim();

			if (charactersMaximumNumber >= tableNameToUse.Length)
				return tableNameToUse;

			int conflictsCounter = 0;
			int currentConflictIndex = 0;

			int conflictingCharsCount = GetNameConflictingCharsCount(ref conflictsCounter, ref currentConflictIndex);

			if (conflictingCharsCount == 0)
				return tableNameToUse.Substring(0, charactersMaximumNumber);

			if (conflictingCharsCount < charactersMaximumNumber)
				return tableNameToUse.Substring(0, charactersMaximumNumber);

			int digitNum = 0;
			int tmpDigitCounter = conflictsCounter + 1;
			do
			{
				digitNum++;
				tmpDigitCounter = tmpDigitCounter / 10;
			} while (tmpDigitCounter > 0);

			if (digitNum < (charactersMaximumNumber - 1))
			{
				tableNameToUse = tableNameToUse.Substring(0, charactersMaximumNumber - digitNum);
				if (tableNameToUse[charactersMaximumNumber - digitNum - 1] != '_')
					tableNameToUse = tableNameToUse.Substring(0, charactersMaximumNumber - digitNum - 1) + "_";
			}
			else
				tableNameToUse = String.Empty;

			tableNameToUse += (currentConflictIndex + 1).ToString("D" + digitNum.ToString());

			if (tableNameToUse.Length > charactersMaximumNumber)
				return String.Empty;

			collisionResolved = true;

			return tableNameToUse.Substring(0, charactersMaximumNumber);
		}

		//---------------------------------------------------------------------
		public string GetTablePrimaryKeyConstraintDefaultName()
		{
			if
				(
				name == null ||
				name.Trim().Length == 0 ||
				library == null ||
				library.Application == null ||
				referenced ||
				!IsPrimaryKeyDefined
				)
				return String.Empty;

			bool collisionResolved = false;
			string tableNameToUse = TruncateToUniqueName(Generics.DBObjectNameMaximumLength - PrimaryKeyConstraintNamePrefix.Length, ref collisionResolved);

			if (collisionResolved)
			{
				int lastUnderscoreIdx = tableNameToUse.LastIndexOf('_');
				if (lastUnderscoreIdx >= 0 && lastUnderscoreIdx < (tableNameToUse.Length - 1))
				{
					string counterText = tableNameToUse.Substring(lastUnderscoreIdx + 1).Trim();
					if (counterText != null && counterText.Length > 0)
					{
						try
						{
							int usedCounter = Int32.Parse(counterText);

							if (usedCounter > 1)
							{
								WizardTableInfoCollection applicationTables = library.Application.GetAllTables();
								if (applicationTables != null && applicationTables.Count > 0)
								{
									ArrayList countersList = new ArrayList();

									foreach (WizardTableInfo aApplicationTable in applicationTables)
									{
										if (String.Compare(name.Trim(), aApplicationTable.Name, true) == 0)
											continue;

										if (aApplicationTable.PrimaryKeyConstraintName != null && aApplicationTable.PrimaryKeyConstraintName.Length > 0)
										{
											bool appCollisionResolved = false;
											string truncatedTableName = aApplicationTable.TruncateToUniqueName(tableNameToUse.Length, ref appCollisionResolved);
											if
												(
												appCollisionResolved &&
												truncatedTableName != null &&
												truncatedTableName.Length > 0 &&
												String.Compare(tableNameToUse.Substring(0, lastUnderscoreIdx), truncatedTableName.Substring(0, lastUnderscoreIdx), true) == 0 &&
												String.Compare(aApplicationTable.PrimaryKeyConstraintName.Substring(PrimaryKeyConstraintNamePrefix.Length, truncatedTableName.Length), truncatedTableName, true) != 0
												)
											{
												int lastAppTableUnderscoreIdx = truncatedTableName.LastIndexOf('_');
												if (lastAppTableUnderscoreIdx >= 0 && lastAppTableUnderscoreIdx < (truncatedTableName.Length - 1))
													countersList.Add(Int32.Parse(truncatedTableName.Substring(lastAppTableUnderscoreIdx + 1)));
											}
										}
									}
									if (countersList.Count > 0)
									{
										// Uso il primo contatore libero
										int counterToUse = 1;
										while (counterToUse < usedCounter)
										{
											bool isCounterUsed = false;
											foreach (int aUsedAppCounter in countersList)
											{
												if (aUsedAppCounter == counterToUse)
												{
													isCounterUsed = true;
													break;
												}
											}
											if (!isCounterUsed)
												break;
											counterToUse++;
										}
										if (counterToUse != usedCounter)
											tableNameToUse = tableNameToUse.Substring(0, lastUnderscoreIdx + 1) + counterToUse.ToString("D" + counterText.Length.ToString());
									}
								}
							}
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}
			}

			return (tableNameToUse != null && tableNameToUse.Length > 0) ? (PrimaryKeyConstraintNamePrefix + tableNameToUse) : String.Empty;
		}

		//---------------------------------------------------------------------
		public string GetColumnDefaultConstraintDefaultName(WizardTableColumnInfo aColumnInfo, bool checkForExtraColumns)
		{
			if
				(
				name == null ||
				name.Trim().Length == 0 ||
				library == null ||
				library.Application == null ||
				aColumnInfo == null ||
				aColumnInfo.DefaultValue == null ||
				(!checkForExtraColumns &&
				(referenced ||
				(
				String.Compare(aColumnInfo.Name, Generics.TBGuidColumnName) != 0 &&
				(columns == null || !columns.Contains(aColumnInfo))
				)))
				)
				return String.Empty;

			bool collisionResolved = false;
			string tableNameToUse = TruncateToUniqueName(12, ref collisionResolved);

			if (collisionResolved)
			{
				int lastUnderscoreIdx = tableNameToUse.LastIndexOf('_');
				if (lastUnderscoreIdx >= 0 && lastUnderscoreIdx < (tableNameToUse.Length - 1))
				{
					string counterText = tableNameToUse.Substring(lastUnderscoreIdx + 1).Trim();
					if (counterText != null && counterText.Length > 0)
					{
						try
						{
							int usedCounter = Int32.Parse(counterText);

							if (usedCounter > 1)
							{
								WizardTableInfoCollection applicationTables = library.Application.GetAllTables();
								if (applicationTables != null && applicationTables.Count > 0)
								{
									ArrayList countersList = new ArrayList();

									foreach (WizardTableInfo aApplicationTable in applicationTables)
									{
										if (String.Compare(name.Trim(), aApplicationTable.Name, true) == 0)
											continue;

										if (aApplicationTable.ColumnsInfo != null && aApplicationTable.ColumnsInfo.Count > 0)
										{
											bool appCollisionResolved = false;
											string truncatedTableName = aApplicationTable.TruncateToUniqueName(tableNameToUse.Length, ref appCollisionResolved);
											if
												(
												appCollisionResolved &&
												truncatedTableName != null &&
												truncatedTableName.Length > 0 &&
												String.Compare(tableNameToUse.Substring(0, lastUnderscoreIdx), truncatedTableName.Substring(0, lastUnderscoreIdx), true) == 0
												)
											{
												// Per ogni colonna devo vedere il counter usato nel default constraint
												foreach (WizardTableColumnInfo aAppColumnInfo in aApplicationTable.ColumnsInfo)
												{
													if
														(
														aAppColumnInfo.DefaultConstraintName == null ||
														aAppColumnInfo.DefaultConstraintName.Length == 0
														)
														continue;

													int nextUnderscoreIdx = aAppColumnInfo.DefaultConstraintName.IndexOf('_', ColumnDefaultConstraintNamePrefix.Length + truncatedTableName.Length);
													if (nextUnderscoreIdx >= 0 && nextUnderscoreIdx < (aAppColumnInfo.DefaultConstraintName.Length - 1))
													{
														int appTableCounterUnderscoreIdx = ColumnDefaultConstraintNamePrefix.Length + truncatedTableName.LastIndexOf('_');

														if (appTableCounterUnderscoreIdx < nextUnderscoreIdx)
														{
															string tableCounterText = aAppColumnInfo.DefaultConstraintName.Substring(appTableCounterUnderscoreIdx + 1, nextUnderscoreIdx - appTableCounterUnderscoreIdx - 1).Trim();
															if (tableCounterText != null && tableCounterText.Length > 0)
															{
																int columnDefaultConstraintTableCounter = Int32.Parse(tableCounterText);
																if (!countersList.Contains(columnDefaultConstraintTableCounter))
																	countersList.Add(columnDefaultConstraintTableCounter);
															}
														}
													}
												}
											}
										}
									}
									if (countersList.Count > 0)
									{
										// Uso il primo contatore libero
										int counterToUse = 1;
										while (counterToUse < usedCounter)
										{
											bool isCounterUsed = false;
											foreach (int aUsedAppCounter in countersList)
											{
												if (aUsedAppCounter == counterToUse)
												{
													isCounterUsed = true;
													break;
												}
											}
											if (!isCounterUsed)
												break;
											counterToUse++;
										}
										if (counterToUse != usedCounter)
											tableNameToUse = tableNameToUse.Substring(0, lastUnderscoreIdx + 1) + counterToUse.ToString("D" + counterText.Length.ToString());
									}
								}
							}
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}
			}

			string defaultConstraintName = ColumnDefaultConstraintNamePrefix + tableNameToUse + "_" + aColumnInfo.Name;
			if (defaultConstraintName.Length <= Generics.DBObjectNameMaximumLength)
				return defaultConstraintName;

			int digitNum = 0;
			int tmpDigitCounter = this.ColumnsCount;
			do
			{
				digitNum++;
				tmpDigitCounter = tmpDigitCounter / 10;
			} while (tmpDigitCounter > 0);

			defaultConstraintName = defaultConstraintName.Substring(0, Generics.DBObjectNameMaximumLength - digitNum);

			if (defaultConstraintName[Generics.DBObjectNameMaximumLength - digitNum - 1] != '_')
				defaultConstraintName = defaultConstraintName.Substring(0, Generics.DBObjectNameMaximumLength - digitNum - 1) + "_";

			uint constraintCounter = 1;
			int columnIndex = columns.IndexOf(aColumnInfo);
			if (columnIndex > 0)
			{
				for (int i = 0; i < columnIndex; i++)
				{
					string alreadyUsedConstraintName = GetColumnDefaultConstraintDefaultName(columns[i]);
					if (
						alreadyUsedConstraintName != null &&
						alreadyUsedConstraintName.Length > 0 &&
						alreadyUsedConstraintName.StartsWith(defaultConstraintName)
						)
						constraintCounter++;
				}
				if (addTBGuidColumn)
				{
					string tbGuidConstraintName = GetColumnDefaultConstraintDefaultName(Generics.TBGuidColumnName);
					if (
						tbGuidConstraintName != null &&
						tbGuidConstraintName.Length > 0 &&
						tbGuidConstraintName.StartsWith(defaultConstraintName)
						)
						constraintCounter++;
				}
			}
			return defaultConstraintName + constraintCounter.ToString("D" + digitNum.ToString());
		}

		//---------------------------------------------------------------------
		public string GetColumnDefaultConstraintDefaultName(WizardTableColumnInfo aColumnInfo)
		{
			return GetColumnDefaultConstraintDefaultName(aColumnInfo, false);
		}

		//---------------------------------------------------------------------
		public string GetColumnDefaultConstraintDefaultName(string aColumnName)
		{
			return GetColumnDefaultConstraintDefaultName(GetColumnInfoByName(aColumnName));
		}

		//---------------------------------------------------------------------
		public string GetForeignKeyConstraintDefaultName(WizardForeignKeyInfo aForeignKeyInfo)
		{
			if
				(
				name == null ||
				name.Trim().Length == 0 ||
				library == null ||
				library.Application == null ||
				referenced ||
				foreignKeys == null ||
				aForeignKeyInfo == null ||
				!foreignKeys.Contains(aForeignKeyInfo) ||
				aForeignKeyInfo.SegmentsCount == 0
				)
				return String.Empty;

			bool collisionResolved = false;
			string tableNameToUse = TruncateToUniqueName(12, ref collisionResolved);

			if (collisionResolved)
			{
				int lastUnderscoreIdx = tableNameToUse.LastIndexOf('_');
				if (lastUnderscoreIdx >= 0 && lastUnderscoreIdx < (tableNameToUse.Length - 1))
				{
					string counterText = tableNameToUse.Substring(lastUnderscoreIdx + 1).Trim();
					if (counterText != null && counterText.Length > 0)
					{
						try
						{
							int usedCounter = Int32.Parse(counterText);

							if (usedCounter > 1)
							{
								WizardTableInfoCollection applicationTables = library.Application.GetAllTables();
								if (applicationTables != null && applicationTables.Count > 0)
								{
									ArrayList countersList = new ArrayList();

									foreach (WizardTableInfo aApplicationTable in applicationTables)
									{
										if (String.Compare(name.Trim(), aApplicationTable.Name, true) == 0)
											continue;

										if (aApplicationTable.ForeignKeysCount > 0)
										{
											bool appCollisionResolved = false;
											string truncatedTableName = aApplicationTable.TruncateToUniqueName(tableNameToUse.Length, ref appCollisionResolved);
											if
												(
												appCollisionResolved &&
												truncatedTableName != null &&
												truncatedTableName.Length > 0 &&
												String.Compare(tableNameToUse.Substring(0, lastUnderscoreIdx), truncatedTableName.Substring(0, lastUnderscoreIdx), true) == 0
												)
											{
												// Per ogni chiave esterna devo vedere il counter usato nel nome di constraint corrispondente
												foreach (WizardForeignKeyInfo aAppForeignKeyInfo in aApplicationTable.ForeignKeys)
												{
													if
														(
														aAppForeignKeyInfo.ConstraintName == null ||
														aAppForeignKeyInfo.ConstraintName.Length == 0
														)
														continue;

													int nextUnderscoreIdx = aAppForeignKeyInfo.ConstraintName.IndexOf('_', ForeignKeyConstraintNamePrefix.Length + truncatedTableName.Length);
													if (nextUnderscoreIdx >= 0 && nextUnderscoreIdx < (aAppForeignKeyInfo.ConstraintName.Length - 1))
													{
														int appTableCounterUnderscoreIdx = ForeignKeyConstraintNamePrefix.Length + truncatedTableName.LastIndexOf('_');

														if (appTableCounterUnderscoreIdx < nextUnderscoreIdx)
														{
															string tableCounterText = aAppForeignKeyInfo.ConstraintName.Substring(appTableCounterUnderscoreIdx + 1, nextUnderscoreIdx - appTableCounterUnderscoreIdx - 1).Trim();
															if (tableCounterText != null && tableCounterText.Length > 0)
															{
																int foreignKeyConstraintTableCounter = Int32.Parse(tableCounterText);
																if (!countersList.Contains(foreignKeyConstraintTableCounter))
																	countersList.Add(foreignKeyConstraintTableCounter);
															}
														}
													}
												}
											}
										}
									}
									if (countersList.Count > 0)
									{
										// Uso il primo contatore libero
										int counterToUse = 1;
										while (counterToUse < usedCounter)
										{
											bool isCounterUsed = false;
											foreach (int aUsedAppCounter in countersList)
											{
												if (aUsedAppCounter == counterToUse)
												{
													isCounterUsed = true;
													break;
												}
											}
											if (!isCounterUsed)
												break;
											counterToUse++;
										}
										if (counterToUse != usedCounter)
											tableNameToUse = tableNameToUse.Substring(0, lastUnderscoreIdx + 1) + counterToUse.ToString("D" + counterText.Length.ToString());
									}
								}
							}
						}
						catch (FormatException)
						{
						}
						catch (OverflowException)
						{
						}
					}
				}
			}

			string referencedTableNameToUse = aForeignKeyInfo.ReferencedTableName;
			if (referencedTableNameToUse.Length > 12)
			{
				referencedTableNameToUse = aForeignKeyInfo.ReferencedTableName.Substring(0, 12);
				foreach (WizardForeignKeyInfo anotherForeignKeyInfo in foreignKeys)
				{
					if (anotherForeignKeyInfo == aForeignKeyInfo)
						break;
					string anotherTruncatedTableName = (anotherForeignKeyInfo.ReferencedTableName.Length > 12) ? anotherForeignKeyInfo.ReferencedTableName.Substring(0, 12) : anotherForeignKeyInfo.ReferencedTableName;
					if (String.Compare(anotherTruncatedTableName, referencedTableNameToUse, true) != 0)
						continue;

					referencedTableNameToUse = referencedTableNameToUse + (foreignKeys.IndexOf(aForeignKeyInfo) + 1).ToString();
				}
			}

			WizardForeignKeyInfoCollection foreignKeysReferencedToSameTable = GetForeignKeysReferencedToTable(aForeignKeyInfo.ReferencedTableNameSpace);
			if (foreignKeysReferencedToSameTable != null && foreignKeysReferencedToSameTable.Count > 1)
			{
				int referencedTableCounterDigitNum = 0;
				int tmpReferencedTableDigitCounter = foreignKeysReferencedToSameTable.Count;
				do
				{
					referencedTableCounterDigitNum++;
					tmpReferencedTableDigitCounter = tmpReferencedTableDigitCounter / 10;
				} while (tmpReferencedTableDigitCounter > 0);

				referencedTableNameToUse += "_" + (foreignKeysReferencedToSameTable.IndexOf(aForeignKeyInfo) + 1).ToString("D" + referencedTableCounterDigitNum.ToString());
			}

			return ForeignKeyConstraintNamePrefix + tableNameToUse + "_" + referencedTableNameToUse;
		}

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfoCollection GetForeignKeysReferencedToTable(string aTableNameSpace)
		{
			if (foreignKeys == null || foreignKeys.Count == 0 || aTableNameSpace == null || aTableNameSpace.Trim().Length == 0)
				return null;

			NameSpace relatedTableNameSpace = new NameSpace(aTableNameSpace, NameSpaceObjectType.Table);
			if (!relatedTableNameSpace.IsValid())
				return null;

			WizardForeignKeyInfoCollection foreignKeysRelatedToTable = new WizardForeignKeyInfoCollection();

			foreach (WizardForeignKeyInfo aForeignKeyInfo in foreignKeys)
			{
				if (String.Compare(aForeignKeyInfo.ReferencedTableNameSpace, aTableNameSpace) == 0)
					foreignKeysRelatedToTable.Add(aForeignKeyInfo);
			}

			return (foreignKeysRelatedToTable.Count > 0) ? foreignKeysRelatedToTable : null;
		}

		//---------------------------------------------------------------------------
		public bool HasForeignKeysReferencedToTable(string aTableNameSpace)
		{
			if (foreignKeys == null || foreignKeys.Count == 0 || aTableNameSpace == null || aTableNameSpace.Trim().Length == 0)
				return false;

			foreach (WizardForeignKeyInfo aForeignKeyInfo in foreignKeys)
			{
				if (String.Compare(aForeignKeyInfo.ReferencedTableNameSpace, aTableNameSpace) == 0)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfo GetEquivalentForeignKey(WizardForeignKeyInfo aForeignKeyInfoToSearch)
		{
			if
				(
				foreignKeys == null ||
				foreignKeys.Count == 0 ||
				aForeignKeyInfoToSearch == null ||
				aForeignKeyInfoToSearch.SegmentsCount == 0 ||
				aForeignKeyInfoToSearch.ReferencedTableNameSpace == null ||
				aForeignKeyInfoToSearch.ReferencedTableNameSpace.Trim().Length == 0
				)
				return null;

			NameSpace relatedTableNameSpace = new NameSpace(aForeignKeyInfoToSearch.ReferencedTableNameSpace, NameSpaceObjectType.Table);
			if (!relatedTableNameSpace.IsValid())
				return null;

			foreach (WizardForeignKeyInfo aForeignKeyInfo in foreignKeys)
			{
				if
					(
					String.Compare(aForeignKeyInfo.ReferencedTableNameSpace, aForeignKeyInfoToSearch.ReferencedTableNameSpace) == 0 &&
					aForeignKeyInfo.HasSameSegments(aForeignKeyInfoToSearch.Segments)
					)
					return aForeignKeyInfo;
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public bool HasForeignKey(WizardForeignKeyInfo aForeignKeyInfoToSearch)
		{
			return (GetEquivalentForeignKey(aForeignKeyInfoToSearch) != null);
		}

		//---------------------------------------------------------------------------
		public int AddForeignKeyInfo(WizardForeignKeyInfo aForeignKeyToAdd, bool addHistoryEvent)
		{
			if
				(
				aForeignKeyToAdd == null ||
				aForeignKeyToAdd.SegmentsCount == 0 ||
				aForeignKeyToAdd.ReferencedTableNameSpace == null ||
				aForeignKeyToAdd.ReferencedTableNameSpace.Length == 0
				)
				return -1;

			// Controllo che i segmenti della chiave esterna siano tutti riferiti a colonne esistenti
			for (int segmentIdx = (aForeignKeyToAdd.SegmentsCount - 1); segmentIdx >= 0; segmentIdx--)
			{
				if (GetColumnInfoByName(aForeignKeyToAdd.Segments[segmentIdx].ColumnName) == null)
				{
					// rimuovo il segmento perchè non valido
					aForeignKeyToAdd.Segments.RemoveAt(segmentIdx);
				}
			}

			// Testo nuovamente il numero di segmenti perchè potrei averli tolti tutti!
			if (aForeignKeyToAdd.SegmentsCount == 0)
				return -1;

			// Controllo che non esista già una chiave esterna identica a quella che si vuole aggiungere
			if (foreignKeys != null && foreignKeys.Count > 0)
			{
				for (int i = 0; i < foreignKeys.Count; i++)
				{
					WizardForeignKeyInfo aExistingForeignKeyInfo = foreignKeys[i];
					if (aExistingForeignKeyInfo == null)
						continue;
					if
						(
						String.Compare(aExistingForeignKeyInfo.ReferencedTableNameSpace, aForeignKeyToAdd.ReferencedTableNameSpace) == 0 &&
						aExistingForeignKeyInfo.HasSameSegments(aForeignKeyToAdd.Segments)
						)
					{
						if (aExistingForeignKeyInfo.ConstraintName == null || aExistingForeignKeyInfo.ConstraintName.Trim().Length == 0)
							aExistingForeignKeyInfo.ConstraintName = aForeignKeyToAdd.ConstraintName;

						return i;
					}
				}
			}

			if (foreignKeys == null)
				foreignKeys = new WizardForeignKeyInfoCollection();

			int addedIdx = foreignKeys.Add(aForeignKeyToAdd);

			if (addedIdx >= 0)
			{
				if (aForeignKeyToAdd.ConstraintName == null || aForeignKeyToAdd.ConstraintName.Trim().Length == 0)
					aForeignKeyToAdd.ConstraintName = GetForeignKeyConstraintDefaultName(aForeignKeyToAdd);

				if (addHistoryEvent && library != null && library.Module != null)
					AddCreateForeignKeyHistoryStep(library.Module.DbReleaseNumber, aForeignKeyToAdd);
			}

			return addedIdx;
		}

		//---------------------------------------------------------------------------
		public int AddForeignKeyInfo(WizardForeignKeyInfo aForeignKeyToAdd)
		{
			return AddForeignKeyInfo(aForeignKeyToAdd, false);
		}

		//---------------------------------------------------------------------------
		public bool IsColumnUsedAsForeignKeySegment(WizardTableColumnInfo aColumnInfo)
		{
			if
				(
				foreignKeys == null ||
				foreignKeys.Count == 0 ||
				columns == null ||
				aColumnInfo == null ||
				!columns.Contains(aColumnInfo)
				)
				return false;

			foreach (WizardForeignKeyInfo aExistingForeignKeyInfo in foreignKeys)
			{
				if
					(
					aExistingForeignKeyInfo != null &&
					aExistingForeignKeyInfo.GetKeySegmentInfoByColumnName(aColumnInfo.Name) != null
					)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------------
		public bool IsColumnUsedAsForeignKeySegment(string aColumnName)
		{
			if
				(
				foreignKeys == null ||
				foreignKeys.Count == 0 ||
				aColumnName == null ||
				aColumnName.Trim().Length == 0
				)
				return false;

			WizardTableColumnInfo existingColumnInfo = GetColumnInfoByName(aColumnName);

			return (existingColumnInfo != null) ? IsColumnUsedAsForeignKeySegment(existingColumnInfo) : false;
		}

		//---------------------------------------------------------------------------
		public void RemoveAllForeignKeys(bool addHistoryEvents)
		{
			if (foreignKeys == null || foreignKeys.Count == 0)
				return;

			for (int keyIdx = (foreignKeys.Count - 1); keyIdx >= 0; keyIdx--)
				RemoveForeignKey(foreignKeys[keyIdx], addHistoryEvents);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllForeignKeys()
		{
			RemoveAllForeignKeys(false);
		}

		//---------------------------------------------------------------------------
		public void RemoveForeignKey(WizardForeignKeyInfo aForeignKeyToRemove, bool addHistoryEvent)
		{
			if
				(
				aForeignKeyToRemove == null ||
				foreignKeys == null ||
				foreignKeys.Count == 0 ||
				!foreignKeys.Contains(aForeignKeyToRemove)
				)
				return;

			foreignKeys.Remove(aForeignKeyToRemove);

			if (addHistoryEvent && library != null && library.Module != null)
				AddDropForeignKeyHistoryStep(library.Module.DbReleaseNumber, aForeignKeyToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveForeignKey(WizardForeignKeyInfo aForeignKeyToAdd)
		{
			RemoveForeignKey(aForeignKeyToAdd, false);
		}

		#endregion // WizardTableInfo public methods
	}

	#endregion

	#region WizardTableInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardTableInfoCollection.
	/// </summary>
	public class WizardTableInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardTableInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				if (value != null && !(value is WizardTableInfo))
					throw new NotSupportedException();

				this[index] = (WizardTableInfo)value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardTableInfo))
				throw new NotSupportedException();

			return this.Contains((WizardTableInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardTableInfo)item);
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

			if (!(item is WizardTableInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardTableInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardTableInfo))
				throw new NotSupportedException();

			Insert(index, (WizardTableInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardTableInfo))
				throw new NotSupportedException();

			Remove((WizardTableInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardTableInfo this[int index]
		{
			get { return (WizardTableInfo)InnerList[index]; }
			set
			{
				InnerList[index] = (WizardTableInfo)value;
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo[] ToArray()
		{
			return (WizardTableInfo[])InnerList.ToArray(typeof(WizardTableInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardTableInfo aTableToAdd)
		{
			if (Contains(aTableToAdd))
				return IndexOf(aTableToAdd);

			return InnerList.Add(aTableToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardTableInfoCollection aTablesCollectionToAdd)
		{
			if (aTablesCollectionToAdd == null || aTablesCollectionToAdd.Count == 0)
				return;

			foreach (WizardTableInfo aTableToAdd in aTablesCollectionToAdd)
				Add(aTableToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardTableInfo aTableToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aTableToInsert))
				return;

			InnerList.Insert(index, aTableToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardTableInfo beforeTable, WizardTableInfo aTableToInsert)
		{
			if (beforeTable == null)
				Add(aTableToInsert);

			if (!Contains(beforeTable))
				return;

			if (Contains(aTableToInsert))
				return;

			Insert(IndexOf(beforeTable), aTableToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardTableInfo aTableToRemove)
		{
			if (!Contains(aTableToRemove))
				return;

			InnerList.Remove(aTableToRemove);
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
		public bool Contains(WizardTableInfo aTableToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aTableToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardTableInfo aTableToSearch)
		{
			if (!Contains(aTableToSearch))
				return -1;

			return InnerList.IndexOf(aTableToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardTableInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardTableInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardTableInfo.Equals(this[i], ((WizardTableInfoCollection)obj)[i]))
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

	#region WizardTableColumnDataType class

	//=================================================================================
	/// <summary>
	/// Summary description for WizardTableColumnDataType.
	/// </summary>
	public class WizardTableColumnDataType
	{
		public enum DataType : ushort
		{
			Undefined = 0x0000,
			String = 0x0001,
			Short = 0x0002,
			Long = 0x0003,
			Double = 0x0004,
			Monetary = 0x0005,
			Quantity = 0x0006,
			Percent = 0x0007,
			Date = 0x0008,
			Boolean = 0x0009,
			Enum = 0x0010,
			Text = 0x0011,
			Guid = 0x0012,
			NText = 0x0013,
			Bit = 0x0014,
			ElapsedTime = 0x0015,
			DateNoTime = 0x0016,
			Time = 0x0017,
			Identity = 0x0018,
		}

		private DataType type = DataType.Undefined;

		internal const string TB_XML_DATATYPE_STRING_VALUE = "string";
		internal const string TB_XML_DATATYPE_INT_VALUE = "integer";
		internal const string TB_XML_DATATYPE_LONG_VALUE = "long";
		internal const string TB_XML_DATATYPE_DOUBLE_VALUE = "double";
		internal const string TB_XML_DATATYPE_PERC_VALUE = "percent";
		internal const string TB_XML_DATATYPE_QUANTITY_VALUE = "quantity";
		internal const string TB_XML_DATATYPE_MONEY_VALUE = "money";
		internal const string TB_XML_DATATYPE_UUID_VALUE = "uuid";
		internal const string TB_XML_DATATYPE_DATE_VALUE = "date";
		internal const string TB_XML_DATATYPE_TIME_VALUE = "time";
		internal const string TB_XML_DATATYPE_DATETIME_VALUE = "dateTime";
		internal const string TB_XML_DATATYPE_BOOLEAN_VALUE = "bool";
		internal const string TB_XML_DATATYPE_ENUM_VALUE = "enum";
		internal const string TB_XML_DATATYPE_ELAPSEDTIME_VALUE = "elapsedTime";
		internal const string TB_XML_DATATYPE_IDENTITY_VALUE = "identity";
		internal const string TB_XML_DATATYPE_TEXT_VALUE = "text";
		internal const string TB_XML_DATATYPE_ARRAY_VALUE = "array";
		internal const string TB_XML_DATATYPE_VOID_VALUE = "void";

		//---------------------------------------------------------------------
		public WizardTableColumnDataType(DataType aDataType)
		{
			type = aDataType;
		}

		//---------------------------------------------------------------------
		public WizardTableColumnDataType(string aDataType)
		{
			type = Parse(aDataType).Type;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardTableColumnDataType))
				return false;

			if (obj == this)
				return true;

			return (type == ((WizardTableColumnDataType)obj).Type);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return (int)type;
		}

		//---------------------------------------------------------------------
		public DataType Type { get { return type; } }

		//---------------------------------------------------------------------
		public bool IsTextual { get { return IsTextualDataType(type); } }


		//---------------------------------------------------------------------
		public bool IsNumeric { get { return IsNumericDataType(type); } }

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return GetDataTypeDescription(type);
		}

		//---------------------------------------------------------------------
		public static bool IsTextualDataType(DataType aDataType)
		{

			return (aDataType == DataType.String || aDataType == DataType.Text || aDataType == DataType.NText);
		}

		//---------------------------------------------------------------------
		public static bool IsNumericDataType(DataType aDataType)
		{

			return (aDataType == DataType.Short || aDataType == DataType.Long || aDataType == DataType.Double ||
					aDataType == DataType.Monetary || aDataType == DataType.Percent || aDataType == DataType.Quantity);
		}

		//---------------------------------------------------------------------
		public static string GetDataTypeDescription(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.Undefined:
					return TBWizardProjectsStrings.UndefinedDataTypeDescription;

				case DataType.String:
					return TBWizardProjectsStrings.StringDataTypeDescription;

				case DataType.Short:
					return TBWizardProjectsStrings.ShortDataTypeDescription;

				case DataType.Long:
					return TBWizardProjectsStrings.LongDataTypeDescription;

				case DataType.Double:
					return TBWizardProjectsStrings.DoubleDataTypeDescription;

				case DataType.Monetary:
					return TBWizardProjectsStrings.MonetaryDataTypeDescription;

				case DataType.Quantity:
					return TBWizardProjectsStrings.QuantityDataTypeDescription;

				case DataType.Percent:
					return TBWizardProjectsStrings.PercentDataTypeDescription;

				case DataType.Date:
					return TBWizardProjectsStrings.DateDataTypeDescription;

				case DataType.Boolean:
					return TBWizardProjectsStrings.BooleanDataTypeDescription;

				case DataType.Enum:
					return TBWizardProjectsStrings.EnumDataTypeDescription;

				case DataType.Text:
					return TBWizardProjectsStrings.TextDataTypeDescription;

				case DataType.NText:
					return TBWizardProjectsStrings.NTextDataTypeDescription;

				case DataType.Guid:
					return TBWizardProjectsStrings.GuidDataTypeDescription;

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static DataType GetDataTypeFromDescription(string aDataTypeDescription)
		{
			if
				(
				aDataTypeDescription == null ||
				aDataTypeDescription.Length == 0 ||
				String.Compare(aDataTypeDescription, TBWizardProjectsStrings.UndefinedDataTypeDescription) == 0
				)
				return DataType.Undefined;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.StringDataTypeDescription) == 0)
				return DataType.String;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.ShortDataTypeDescription) == 0)
				return DataType.Short;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.LongDataTypeDescription) == 0)
				return DataType.Long;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.DoubleDataTypeDescription) == 0)
				return DataType.Double;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.MonetaryDataTypeDescription) == 0)
				return DataType.Monetary;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.QuantityDataTypeDescription) == 0)
				return DataType.Quantity;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.PercentDataTypeDescription) == 0)
				return DataType.Percent;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.DateDataTypeDescription) == 0)
				return DataType.Date;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.BooleanDataTypeDescription) == 0)
				return DataType.Boolean;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.EnumDataTypeDescription) == 0)
				return DataType.Enum;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.TextDataTypeDescription) == 0)
				return DataType.Text;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.NTextDataTypeDescription) == 0)
				return DataType.NText;

			if (String.Compare(aDataTypeDescription, TBWizardProjectsStrings.GuidDataTypeDescription) == 0)
				return DataType.Guid;

			return DataType.Undefined;
		}

		//---------------------------------------------------------------------
		public static string GetDataObjClassName(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.Undefined:
					return String.Empty;

				case DataType.String:
					return "DataStr";

				case DataType.Short:
					return "DataInt";

				case DataType.Long:
					return "DataLng";

				case DataType.Double:
					return "DataDbl";

				case DataType.Monetary:
					return "DataMon";

				case DataType.Quantity:
					return "DataQta";

				case DataType.Percent:
					return "DataPerc";

				case DataType.Date:
					return "DataDate";

				case DataType.Boolean:
					return "DataBool";

				case DataType.Enum:
					return "DataEnum";

				case DataType.Text:
				case DataType.NText:
					return "DataText";

				case DataType.Guid:
					return "DataGuid";

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string GetWoormDataTypeText(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.Undefined:
					return String.Empty;

				case DataType.String:
					return Language.GetTokenString(Token.STRING);

				case DataType.Short:
					return Language.GetTokenString(Token.INTEGER);

				case DataType.Long:
					return Language.GetTokenString(Token.LONG_INTEGER);

				case DataType.Double:
					return Language.GetTokenString(Token.DOUBLE_PRECISION);

				case DataType.Monetary:
					return Language.GetTokenString(Token.MONEY);

				case DataType.Quantity:
					return Language.GetTokenString(Token.QUANTITY);

				case DataType.Percent:
					return Language.GetTokenString(Token.PERCENT);

				case DataType.Date:
					return Language.GetTokenString(Token.DATE);

				case DataType.Boolean:
					return Language.GetTokenString(Token.BOOLEAN);

				case DataType.Enum:
					return Language.GetTokenString(Token.ENUM);

				case DataType.Text:
				case DataType.NText:
					return Language.GetTokenString(Token.LONG_STRING);

				case DataType.Guid:
					return Language.GetTokenString(Token.UUID);

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static DataType GetDataTypeFromWoormText(string aWoormDataType)
		{
			if (aWoormDataType == null || aWoormDataType.Length == 0)
				return DataType.Undefined;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.STRING)) == 0)
				return DataType.String;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.INTEGER)) == 0)
				return DataType.Short;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.LONG_INTEGER)) == 0)
				return DataType.Long;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.DOUBLE_PRECISION)) == 0)
				return DataType.Double;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.MONEY)) == 0)
				return DataType.Monetary;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.QUANTITY)) == 0)
				return DataType.Quantity;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.PERCENT)) == 0)
				return DataType.Percent;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.DATE)) == 0)
				return DataType.Date;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.BOOLEAN)) == 0)
				return DataType.Boolean;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.ENUM)) == 0)
				return DataType.Enum;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.LONG_STRING)) == 0)
				return DataType.Text;

			if (String.Compare(aWoormDataType, Language.GetTokenString(Token.UUID)) == 0)
				return DataType.Guid;

			return DataType.Undefined;
		}

		//---------------------------------------------------------------------
		public static string GetDefaultFormatStyleName(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.Undefined:
					return String.Empty;

				case DataType.String:
					return "Text";

				case DataType.Short:
					return "Integer";

				case DataType.Long:
					return "Long";

				case DataType.Double:
					return "Double";

				case DataType.Monetary:
					return "Money";

				case DataType.Quantity:
					return "Quantity";

				case DataType.Percent:
					return "Percent";

				case DataType.Date:
					return "Date";

				case DataType.Boolean:
					return "Bool";

				case DataType.Enum:
					return "Enum";

				case DataType.Text:
				case DataType.NText:
					return "LongText";

				case DataType.Guid:
					return "Uuid";

				case DataType.Bit:
					return "Bit";

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static DataType GetDataTypeFromTBXmlValue(string aDataType)
		{
			switch (aDataType)
			{
				case TB_XML_DATATYPE_STRING_VALUE:
					return DataType.String;

				case TB_XML_DATATYPE_TEXT_VALUE:
					return DataType.Text;

				case TB_XML_DATATYPE_INT_VALUE:
					return DataType.Short;

				case TB_XML_DATATYPE_LONG_VALUE:
					return DataType.Long;

				case TB_XML_DATATYPE_DOUBLE_VALUE:
					return DataType.Double;

				case TB_XML_DATATYPE_MONEY_VALUE:
					return DataType.Monetary;

				case TB_XML_DATATYPE_QUANTITY_VALUE:
					return DataType.Quantity;

				case TB_XML_DATATYPE_PERC_VALUE:
					return DataType.Percent;

				case TB_XML_DATATYPE_DATETIME_VALUE:
					return DataType.Date;

				case TB_XML_DATATYPE_DATE_VALUE:
					return DataType.DateNoTime;

				case TB_XML_DATATYPE_TIME_VALUE:
					return DataType.Time;

				case TB_XML_DATATYPE_BOOLEAN_VALUE:
					return DataType.Boolean;

				case TB_XML_DATATYPE_ENUM_VALUE:
					return DataType.Enum;

				case TB_XML_DATATYPE_UUID_VALUE:
					return DataType.Guid;

				case TB_XML_DATATYPE_IDENTITY_VALUE:
					return DataType.Identity;

				case TB_XML_DATATYPE_ELAPSEDTIME_VALUE:
					return DataType.ElapsedTime;

				case "Bit":
					return DataType.Bit;

				default:
					return DataType.Undefined;
			}
		}

		//---------------------------------------------------------------------
		public static string GetDataTypeTBXmlValue(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.Undefined:
					return TB_XML_DATATYPE_VOID_VALUE;

				case DataType.String:
					return TB_XML_DATATYPE_STRING_VALUE;

				case DataType.Text:
				case DataType.NText:
					return TB_XML_DATATYPE_TEXT_VALUE;

				case DataType.Short:
					return TB_XML_DATATYPE_INT_VALUE;

				case DataType.Long:
					return TB_XML_DATATYPE_LONG_VALUE;

				case DataType.Double:
					return TB_XML_DATATYPE_DOUBLE_VALUE;

				case DataType.Monetary:
					return TB_XML_DATATYPE_MONEY_VALUE;

				case DataType.Quantity:
					return TB_XML_DATATYPE_QUANTITY_VALUE;

				case DataType.Percent:
					return TB_XML_DATATYPE_PERC_VALUE;

				case DataType.Date:
					return TB_XML_DATATYPE_DATETIME_VALUE;

				case DataType.DateNoTime:
					return TB_XML_DATATYPE_DATE_VALUE;

				case DataType.Time:
					return TB_XML_DATATYPE_TIME_VALUE;

				case DataType.Boolean:
					return TB_XML_DATATYPE_BOOLEAN_VALUE;

				case DataType.Enum:
					return TB_XML_DATATYPE_ENUM_VALUE;

				case DataType.Guid:
					return TB_XML_DATATYPE_UUID_VALUE;

				case DataType.Identity:
					return TB_XML_DATATYPE_IDENTITY_VALUE;

				case DataType.ElapsedTime:
					return TB_XML_DATATYPE_ELAPSEDTIME_VALUE;

				case DataType.Bit:
					return "Bit";

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static object GetDataTypeDefaultValue(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.String:
				case DataType.Text:
				case DataType.NText:
					return String.Empty;

				case DataType.Short:
					return (Int16)0;

				case DataType.Long:
					return (Int32)0;

				case DataType.Double:
				case DataType.Monetary:
				case DataType.Quantity:
				case DataType.Percent:
					return (double)0.0;

				case DataType.Date:
					return ObjectHelper.NullTbDateTime;

				case DataType.Boolean:
					return false;

				case DataType.Bit:
					return 0;

				case DataType.Guid:
					return Guid.Empty;

				default:
					break;
			}

			return null;
		}

		//---------------------------------------------------------------------
		public static string GetDataTypeControlName(DataType aDataType)
		{
			if (aDataType == DataType.Undefined)
				return String.Empty;

			if (aDataType == DataType.Boolean)
				return "PUSHBUTTON";

			if (aDataType == DataType.Enum)
				return "COMBOBOX";

			return "EDITTEXT";
		}

		//---------------------------------------------------------------------
		public static string GetDataTypeParsedControlClassName(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.String:
					return "CStrEdit";

				case DataType.Text:
				case DataType.NText:
					return "CTextEdit";

				case DataType.Short:
					return "CIntEdit";

				case DataType.Long:
					return "CLongEdit";

				case DataType.Double:
					return "CDoubleEdit";

				case DataType.Monetary:
					return "CMoneyEdit";

				case DataType.Quantity:
					return "CQuantityEdit";

				case DataType.Percent:
					return "CPercEdit";

				case DataType.Date:
					return "CDateEdit";

				case DataType.Boolean:
					return "CBoolButton";

				case DataType.Enum:
					return "CEnumCombo";

				case DataType.Guid:
					return "CGuidEdit";

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string GetDataTypeParsedStaticClassName(DataType aDataType)
		{
			switch (aDataType)
			{
				case DataType.String:
					return "CStrStatic";

				case DataType.Text:
				case DataType.NText:
					return "CTextStatic";

				case DataType.Short:
					return "CIntStatic";

				case DataType.Long:
					return "CLongStatic";

				case DataType.Double:
					return "CDoubleStatic";

				case DataType.Monetary:
					return "CMoneyStatic";

				case DataType.Quantity:
					return "CQuantityStatic";

				case DataType.Percent:
					return "CPercStatic";

				case DataType.Date:
					return "CDateStatic";

				case DataType.Boolean:
					return "CBoolStatic";

				case DataType.Enum:
					return "CEnumStatic";

				case DataType.Guid:
					return "CGuidStatic";

				default:
					break;
			}

			return String.Empty;
		}

		//---------------------------------------------------------------------
		public static string GetColumnControlStyles(DataType aDataType, bool setMultilineStyle)
		{
			if (aDataType == DataType.Undefined)
				return String.Empty;

			if (aDataType == DataType.Boolean)
				return "BS_AUTOCHECKBOX";

			if (aDataType == DataType.Enum)
				return "CBS_DROPDOWNLIST";

			if (setMultilineStyle)
				return "ES_MULTILINE | ES_AUTOVSCROLL | ES_WANTRETURN";

			return "ES_AUTOHSCROLL";
		}

		//---------------------------------------------------------------------
		public static WizardTableColumnDataType Parse(string aTextToParse)
		{
			try
			{
				return new WizardTableColumnDataType((DataType)Enum.Parse(typeof(DataType), aTextToParse, false));
			}
			catch (ArgumentException)
			{
				// se è fallita la Parse ritorniamo il valore Undefined invece di null
				return new WizardTableColumnDataType(DataType.Undefined);
			}
		}

		//---------------------------------------------------------------------
		public static string Unparse(WizardTableColumnDataType aDataTypeToUnparse)
		{
			return aDataTypeToUnparse.type.ToString();
		}

		//---------------------------------------------------------------------
		public static WizardTableColumnDataType.DataType GetFromSystemDataTypeName(string aSystemDataTypeName)
		{
			if (aSystemDataTypeName == null || aSystemDataTypeName.Trim().Length == 0)
				return DataType.Undefined;

			string systemTypeDescription = aSystemDataTypeName.Trim().ToLower();

			if (
				String.Compare(systemTypeDescription, "string") == 0 ||
				String.Compare(systemTypeDescription, "char") == 0
				)
				return DataType.String;

			if (
				String.Compare(systemTypeDescription, "int16") == 0 ||
				String.Compare(systemTypeDescription, "uint16") == 0
				)
				return DataType.Short;

			if (
				String.Compare(systemTypeDescription, "int32") == 0 ||
				String.Compare(systemTypeDescription, "uint32") == 0
				)
				return DataType.Long;

			if
				(
				String.Compare(systemTypeDescription, "float") == 0 ||
				String.Compare(systemTypeDescription, "double") == 0 ||
				String.Compare(systemTypeDescription, "decimal") == 0
				)
				return DataType.Double;

			if (String.Compare(systemTypeDescription, "datetime") == 0)
				return DataType.Date;

			//if (String.Compare(systemTypeDescription, "text") == 0)
			//	return DataType.Text;

			if (String.Compare(systemTypeDescription, "boolean") == 0)
				return DataType.Boolean;

			if (String.Compare(systemTypeDescription, "guid") == 0)
				return DataType.Guid;

			return DataType.Undefined;
		}

		//---------------------------------------------------------------------
		public static WizardTableColumnDataType.DataType GetFromSQLServerDataType(string aSQLDataTypeKeyword)
		{
			if (aSQLDataTypeKeyword == null || aSQLDataTypeKeyword.Trim().Length == 0)
				return DataType.Undefined;

			string sqlTypeDescription = aSQLDataTypeKeyword.Trim().ToLower();

			if (sqlTypeDescription[0] == '[')
			{
				int closedSquareIdx = sqlTypeDescription.LastIndexOf(']');
				if (closedSquareIdx == -1)
					return DataType.Undefined;
				sqlTypeDescription = sqlTypeDescription.Substring(1, closedSquareIdx - 1);
			}

			if (
				String.Compare(sqlTypeDescription, "char") == 0 ||
				String.Compare(sqlTypeDescription, "nchar") == 0 ||
				String.Compare(sqlTypeDescription, "nvarchar") == 0 ||
				String.Compare(sqlTypeDescription, "varchar") == 0
				)
				return DataType.String;

			if (String.Compare(sqlTypeDescription, "smallint") == 0)
				return DataType.Short;

			if (String.Compare(sqlTypeDescription, "int") == 0)
				return DataType.Long;

			if (String.Compare(sqlTypeDescription, "float") == 0)
				return DataType.Double;

			if (String.Compare(sqlTypeDescription, "datetime") == 0)
				return DataType.Date;

			if (String.Compare(sqlTypeDescription, "text") == 0)
				return DataType.Text;

			if (String.Compare(sqlTypeDescription, "ntext") == 0)
				return DataType.NText;

			if (String.Compare(sqlTypeDescription, "bit") == 0)
				return DataType.Bit;

			if (String.Compare(sqlTypeDescription, "uniqueidentifier") == 0)
				return DataType.Guid;

			return DataType.Undefined;
		}

		//---------------------------------------------------------------------
		public static string GetSQLServerTranslation(WizardTableColumnDataType aDataType, uint length)
		{
			switch (aDataType.type)
			{
				case DataType.String:
					return "[varchar] (" + length.ToString() + ")";

				case DataType.Short:
					return "[smallint]";

				case DataType.Long:
				case DataType.Enum:
				case DataType.ElapsedTime:
				case DataType.Identity:
					return "[int]";

				case DataType.Double:
				case DataType.Monetary:
				case DataType.Quantity:
				case DataType.Percent:
					return "[float]";

				case DataType.Date:
				case DataType.DateNoTime:
				case DataType.Time:
					return "[datetime]";

				case DataType.Boolean:
					return "[char] (1)";

				case DataType.Text:
				//					return "[text]"; // considerando che TB non gestisce i text, mettiamo sempre ntext
				case DataType.NText:
					return "[ntext]";

				case DataType.Bit:
					return "[bit]";

				case DataType.Guid:
					return "[uniqueidentifier]";

				default:
					return String.Empty;
			}
		}

		//---------------------------------------------------------------------
		public static string GetOracleTranslation(WizardTableColumnDataType aDataType, uint length)
		{
			switch (aDataType.type)
			{
				case DataType.String:
					return "VARCHAR2 (" + length.ToString() + ")";

				case DataType.Short:
					return "NUMBER(6)";

				case DataType.Long:
				case DataType.Enum:
				case DataType.ElapsedTime:
				case DataType.Identity:
					return "NUMBER(10)";

				case DataType.Double:
				case DataType.Monetary:
				case DataType.Quantity:
				case DataType.Percent:
					return "FLOAT(126)";

				case DataType.Date:
				case DataType.DateNoTime:
				case DataType.Time:
					return "DATE";

				case DataType.Boolean:
					return "CHAR(1)";

				case DataType.Text:
				//					return "CLOB"; // considerando che TB non gestisce i CLOB, mettiamo sempre NCLOB
				case DataType.NText:
					return "NCLOB";

				case DataType.Bit:
					return "NUMBER(1)";

				case DataType.Guid:
					return "CHAR(38)";

				default:
					return String.Empty;
			}
		}

		//---------------------------------------------------------------------
		public static string GetDBValueString(DataType aType, object aValue, DBMSType aDBMSType)
		{
			string myValue = aValue.ToString();

			switch (aType)
			{
				case DataType.String:
				case DataType.Text:
				case DataType.NText:
				case DataType.Boolean:
					return "'" + myValue + "'";

				case DataType.Short:
					Int16 myShort;
					if (Int16.TryParse(myValue, out myShort))
						return myValue;
					return "0";

				case DataType.Long:
				case DataType.ElapsedTime:
				case DataType.Identity:
					Int32 myLong;
					if (Int32.TryParse(myValue, out myLong))
						return myValue;
					return "0";

				case DataType.Enum:
					return (
						(aValue is UInt32)
						? ((UInt32)aValue).ToString(NumberFormatInfo.InvariantInfo)
						: aValue.ToString() // nel caso in cui non sia un uint gli passo direttamente il ToString() del value
						);

				case DataType.Double:
				case DataType.Monetary:
				case DataType.Quantity:
				case DataType.Percent:
					{
						double myDouble;
						if (double.TryParse(myValue, out myDouble) && myDouble != 0)
							return myValue;
						return "0.00";
					}

				case DataType.Date:
				case DataType.DateNoTime:
				case DataType.Time:
					{
						// imposto la data generica (vuota per TB)
						string defaultDate =
							(aDBMSType == DBMSType.SQLSERVER)
							? "'17991231'"
							: (aDBMSType == DBMSType.ORACLE) ? "TO_DATE('31-12-1799','DD-MM-YYYY')" : "''";

						// per la data non possiamo utilizzare la funzione DateTime.TryParse
						// perchè non si riesce ad applicare al default '17991231' 
						// allora utilizziamo la funzione Parse del SqlDateTime e formattiamo a mano la data per lo script
						try
						{
							SqlDateTime dt = SqlDateTime.Parse(myValue);
							DateTime myDateTime = dt.Value;

							if (aDBMSType == DBMSType.SQLSERVER)
								return "'" + myDateTime.ToString("yyyyMMdd") + "'";
							else if (aDBMSType == DBMSType.ORACLE)
								return String.Format("TO_DATE('{0}','DD-MM-YYYY')", myDateTime.ToString("dd-MM-yyyy"));
						}
						catch (FormatException)
						{
							return defaultDate;
						}

						return defaultDate;
					}

				case DataType.Bit:
					{
						if (myValue == "1" || myValue == "0")
							return myValue;
						return "0";
					}

				case DataType.Guid:
					{
						string defaultGuid = (aDBMSType == DBMSType.SQLSERVER)
											? "0x00"
											: (aDBMSType == DBMSType.ORACLE ? ("'" + Guid.Empty.ToString("B") + "'") : "");

						try
						{
							if (string.Compare(myValue, "0x00", StringComparison.InvariantCultureIgnoreCase) != 0)
							{
								Guid myGuid = new Guid(myValue);
								if (aDBMSType == DBMSType.SQLSERVER)
									return myValue;
								else if (aDBMSType == DBMSType.ORACLE)
									return "'" + myGuid.ToString("B").ToUpper() + "'";
							}
							return defaultGuid;
						}
						catch
						{
							return defaultGuid;
						}
					}

				case DataType.Undefined:
				default:
					break;
			}

			return String.Empty;
		}

		[Obsolete("Da adeguare alla nuova gestione dei default per poter utilizzare le funzioni nella Default Expression")]
		//---------------------------------------------------------------------------
		public static object AdjustValueToDataType(DataType aType, object aValueToAdjust, System.IFormatProvider formatProvider)
		{
			if (aValueToAdjust == null || aType == DataType.Undefined)
				return null;

			// If aValueToAdjust is a string, it is parsed using the formatting information 
			// supplied by formatProvider.
			// If formatProvider is a null reference, the current culture is used.

			switch (aType)
			{
				case DataType.String:
				case DataType.Text:
				case DataType.NText:
					try
					{
						return (aValueToAdjust is string) ? aValueToAdjust : Convert.ToString(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Short:
					try
					{
						return (aValueToAdjust is Int16) ? aValueToAdjust : Convert.ToInt16(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Long:
					try
					{
						return (aValueToAdjust is Int32) ? aValueToAdjust : Convert.ToInt32(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Enum:
					try
					{
						return (aValueToAdjust is UInt32) ? aValueToAdjust : Convert.ToUInt32(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Double:
				case DataType.Monetary:
				case DataType.Percent:
				case DataType.Quantity:
					try
					{
						return (aValueToAdjust is double) ? aValueToAdjust : Convert.ToDouble(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Boolean:
					try
					{
						if (aValueToAdjust is string)
						{
							if (String.Compare(((string)aValueToAdjust).Trim(), "0") == 0)
								return false;
							if (String.Compare(((string)aValueToAdjust).Trim(), "1") == 0)
								return true;
						}
						return (aValueToAdjust is bool) ? aValueToAdjust : Convert.ToBoolean(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Date:
					try
					{
						return (aValueToAdjust is DateTime) ? aValueToAdjust : Convert.ToDateTime(aValueToAdjust, formatProvider);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
					break;

				case DataType.Guid:
					try
					{
						return (aValueToAdjust is Guid) ? aValueToAdjust : new Guid(aValueToAdjust.ToString());
					}
					catch (FormatException)
					{
					}
					break;

				case DataType.Bit:
					return aValueToAdjust.ToString();

				default:
					break;
			}

			return null;
		}
		[Obsolete("Da adeguare alla nuova gestione dei default per poter utilizzare le funzioni nella Default Expression")]
		//---------------------------------------------------------------------
		public static object AdjustValueToDataType(DataType aType, object aValueToAdjust)
		{
			return AdjustValueToDataType(aType, aValueToAdjust, null);
		}
		[Obsolete("Da adeguare alla nuova gestione dei default per poter utilizzare le funzioni nella Default Expression")]
		//---------------------------------------------------------------------
		public static object GetValueFromString(DataType aType, string aValueText, System.IFormatProvider formatProvider)
		{
			if (aType == DataType.Undefined || aValueText == null)
				return null;

			if (aType == DataType.String || aType == DataType.Text || aType == DataType.NText)
				return aValueText;

			return AdjustValueToDataType(aType, aValueText, formatProvider);
		}
		[Obsolete("Da adeguare alla nuova gestione dei default per poter utilizzare le funzioni nella Default Expression")]
		//---------------------------------------------------------------------
		public static object GetValueFromString(DataType aType, string aValueText)
		{
			return GetValueFromString(aType, aValueText, null);
		}
	}

	#endregion

	#region WizardTableColumnInfo class

	//=================================================================================
	/// <summary>
	/// Summary description for WizardTableColumnInfo.
	/// </summary>
	public class WizardTableColumnInfo : IDisposable
	{
		public const uint DefaultStringColumnLength = 10;

		#region WizardTableColumnInfo private data members

		private string name = String.Empty;
		private WizardTableColumnDataType dataType = null;
		private WizardEnumInfo enumInfo = null;
		private uint dataLength = 0;
		private object defaultValue = null;
		private string defaultExpressionValue = null;
		private bool isPrimaryKeySegment = false;
		private bool isNullable = true; //def a true perchè sono la maggioranza e perchè prima dell'esistenza dell'attributo era sempre true
		private bool isCollateSensitive = true; //def a true, perchè la maggior parte delle colonne eredita la collate dal db 
		private bool isUpperCaseDataString = false;
		private bool isAutoIncrement = false;
		private int seed = -1;
		private int increment = -1;
		private uint creationDbReleaseNumber = 0;
		private int createStep = 0;
		private string defaultConstraintName = String.Empty;

		// (temporary) property not save ! 
		private string description;
		
		private uint tbenum = 0; // valore dell'enumerativo letto dal c++ (non usiamo il WizardEnumInfo xchè contiene troppe info)

		private bool extraAdded = false;
		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool disposed = false;
		private bool mandatory = false;


		#endregion

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo(string aColumnName, bool isReadOnly, bool isExtraAdded)
		{
			Name = aColumnName;
			description = Name;
			readOnly = isReadOnly;
			extraAdded = isExtraAdded;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo(string aColumnName, bool isReadOnly, bool isExtraAdded, bool mandatory)
		{
			this.mandatory = mandatory;
			Name = aColumnName;
			description = Name;
			readOnly = isReadOnly;
			extraAdded = isExtraAdded;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo(string aColumnName, bool isExtraAdded)
			: this(aColumnName, false, isExtraAdded)
		{
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo(string aColumnName)
			: this(aColumnName, false, false)
		{
		}

		//---------------------------------------------------------------------------
		public string Description
		{
			get 
			{ 
				return description; 
			}

			set 
			{ 
				description = value; 
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo(WizardTableColumnInfo aColumnInfo)
		{
			name = (aColumnInfo != null) ? aColumnInfo.Name : String.Empty;
			dataType = (aColumnInfo != null) ? aColumnInfo.DataType : null;
			enumInfo = (aColumnInfo != null) ? aColumnInfo.EnumInfo : null;
			dataLength = (aColumnInfo != null) ? aColumnInfo.DataLength : 0;
			defaultValue = (aColumnInfo != null) ? aColumnInfo.DefaultValue : null;
			defaultExpressionValue = (aColumnInfo != null) ? aColumnInfo.DefaultExpressionValue : null;
			isPrimaryKeySegment = (aColumnInfo != null) ? aColumnInfo.IsPrimaryKeySegment : false;
			isNullable = (aColumnInfo != null) ? aColumnInfo.IsNullable : true;
			isCollateSensitive = (aColumnInfo != null) ? aColumnInfo.IsCollateSensitive : true;
			isUpperCaseDataString = (aColumnInfo != null) ? aColumnInfo.IsUpperCaseDataString : false;
			creationDbReleaseNumber = (aColumnInfo != null) ? aColumnInfo.CreationDbReleaseNumber : 0;
			createStep = (aColumnInfo != null) ? aColumnInfo.CreateStep : 0;
			defaultConstraintName = (aColumnInfo != null) ? aColumnInfo.DefaultConstraintName : String.Empty;
			extraAdded = (aColumnInfo != null) ? aColumnInfo.ExtraAdded : false;
			readOnly = (aColumnInfo != null) ? aColumnInfo.ReadOnly : false;
			isAutoIncrement = (aColumnInfo != null) ? aColumnInfo.IsAutoIncrement : false;
			seed = (aColumnInfo != null) ? aColumnInfo.Seed : -1;
			increment = (aColumnInfo != null) ? aColumnInfo.Increment : -1;
			tbenum = (aColumnInfo != null) ? aColumnInfo.TbEnum : 0;
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
			if (obj == null || !(obj is WizardTableColumnInfo))
				return false;

			if (obj == this)
				return true;

			return
				(
				String.Compare(name, ((WizardTableColumnInfo)obj).Name) == 0 &&
				WizardTableColumnDataType.Equals(dataType, ((WizardTableColumnInfo)obj).DataType) &&
				WizardEnumInfo.Equals(enumInfo, ((WizardTableColumnInfo)obj).EnumInfo) &&
				dataLength == ((WizardTableColumnInfo)obj).DataLength &&
				HasSameDefaultValueAs((WizardTableColumnInfo)obj) &&
				defaultExpressionValue == ((WizardTableColumnInfo)obj).DefaultExpressionValue &&
				isPrimaryKeySegment == ((WizardTableColumnInfo)obj).IsPrimaryKeySegment &&
				isNullable == ((WizardTableColumnInfo)obj).IsNullable &&
				isCollateSensitive == ((WizardTableColumnInfo)obj).IsCollateSensitive &&
				isUpperCaseDataString == ((WizardTableColumnInfo)obj).IsUpperCaseDataString &&
				creationDbReleaseNumber == ((WizardTableColumnInfo)obj).CreationDbReleaseNumber &&
				createStep == ((WizardTableColumnInfo)obj).CreateStep &&
				extraAdded == ((WizardTableColumnInfo)obj).ExtraAdded &&
				isAutoIncrement == ((WizardTableColumnInfo)obj).IsAutoIncrement &&
				seed == ((WizardTableColumnInfo)obj).Seed &&
				increment == ((WizardTableColumnInfo)obj).Increment &&
				tbenum == ((WizardTableColumnInfo)obj).TbEnum
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region WizardTableColumnInfo public properties
		//---------------------------------------------------------------------------
		public string Name
		{
			get { return name; }
			set
			{
				if (
					mandatory ||
                    Generics.IsValidTableColumnName(value) || //verifica i nomi riservati (tbguid, tbmodified, tbcreated, tbmodifiedid, tbcreatedid)
					String.Compare(value.Trim(), Generics.TBGuidColumnName, true) == 0 // la colonna TBGuid la inserisco cmq!
					)
					name = value.Trim();
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnDataType DataType
		{
			get { return dataType; }
			set
			{
				if (dataType != null && value != null && dataType.Type == value.Type)
					return;

				dataType = value;

				if (dataType != null)
				{
					if (dataType.Type != WizardTableColumnDataType.DataType.String)
					{
						if (dataType.Type != WizardTableColumnDataType.DataType.Enum)
							enumInfo = null;
						dataLength = 0;
					}
					else
					{
						if (dataLength == 0)
							dataLength = DefaultStringColumnLength;
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		public WizardEnumInfo EnumInfo
		{
			get { return (dataType != null && dataType.Type == WizardTableColumnDataType.DataType.Enum) ? enumInfo : null; }
			set
			{
				if (dataType != null && dataType.Type != WizardTableColumnDataType.DataType.Enum)
				{
					enumInfo = null;
					return;
				}

				enumInfo = value;
			}
		}

		//---------------------------------------------------------------------------
		public string EnumTypeName
		{
			get
			{
				if
					(
					dataType == null ||
					dataType.Type != WizardTableColumnDataType.DataType.Enum ||
					enumInfo == null
					)
					return String.Empty;

				return enumInfo.Name;
			}
		}

		//---------------------------------------------------------------------------
		public uint DataLength
		{
			get { return dataLength; }
			set
			{
				if (dataType != null && dataType.Type == WizardTableColumnDataType.DataType.String)
				{
					dataLength = value;

					if (defaultValue != null && (defaultValue is string) && ((string)defaultValue).Length > dataLength)
						defaultValue = ((string)defaultValue).Substring(0, (int)dataLength);
				}
				else
					dataLength = 0;
			}
		}

		//---------------------------------------------------------------------------
		public string DefaultExpressionValue { get { return defaultExpressionValue; } set { defaultExpressionValue = value; } }

		//---------------------------------------------------------------------------
		public bool HasSpecificDefaultValue { get { return (defaultValue != null); } }

		//---------------------------------------------------------------------------
		public object DefaultValue
		{
			get
			{
				if (dataType == null)
					return null;

				// Nel caso di enumerativi il valore di default è deciso dal tipo di enumerativo
				if (dataType.Type == WizardTableColumnDataType.DataType.Enum)
				{
					if (enumInfo != null && enumInfo.DefaultItem != null)
						return enumInfo.GetItemStoredValue(enumInfo.DefaultItem);
				}

				return defaultValue;
			}
			set
			{
				defaultValue = value;
			}
		}

		//---------------------------------------------------------------------------
		public string DefaultValueString
		{
			get
			{
				if (
					dataType == null ||
					dataType.Type == WizardTableColumnDataType.DataType.Undefined ||
					(dataType.Type != WizardTableColumnDataType.DataType.Enum && defaultValue == null)
					)
					return null;


				if (
					dataType.Type == WizardTableColumnDataType.DataType.Enum &&
					enumInfo != null &&
					enumInfo.DefaultItem != null
					)
					return enumInfo.GetItemStoredValue(enumInfo.DefaultItem).ToString(NumberFormatInfo.InvariantInfo);

				return defaultValue.ToString();
			}
		}

		//---------------------------------------------------------------------------
		public uint DatabaseLength
		{
			get
			{
				if (dataType == null)
					return 0;

				if (dataType.Type == WizardTableColumnDataType.DataType.String)
					return dataLength;

				if (dataType.Type == WizardTableColumnDataType.DataType.Boolean)
					return 1;

				return 0;
			}
		}

		//---------------------------------------------------------------------------
		public bool IsPrimaryKeySegment { get { return isPrimaryKeySegment; } set { isPrimaryKeySegment = value; } }

		//---------------------------------------------------------------------------
		public bool IsNullable { get { return isNullable; } set { isNullable = value; } }

		//---------------------------------------------------------------------------
		public bool IsCollateSensitive { get { return isCollateSensitive; } set { isCollateSensitive = value; } }

		//---------------------------------------------------------------------------
		public bool IsAutoIncrement { get { return isAutoIncrement; } set { isAutoIncrement = value; } }

		//---------------------------------------------------------------------------
		public int Seed { get { return seed; } set { seed = value; } }

		//---------------------------------------------------------------------------
		public int Increment { get { return increment; } set { increment = value; } }

		//---------------------------------------------------------------------------
		public uint TbEnum { get { return tbenum; } set { tbenum = value; } }

		//---------------------------------------------------------------------------
		public bool IsUpperCaseDataString
		{
			get { return isUpperCaseDataString && dataType.IsTextual; }
			set { isUpperCaseDataString = value && dataType.IsTextual; }
		}

		//---------------------------------------------------------------------------
		public uint CreationDbReleaseNumber { get { return creationDbReleaseNumber; } set { creationDbReleaseNumber = value; } }

		//---------------------------------------------------------------------------
		public int CreateStep { get { return createStep; } set { createStep = value; } }

		//---------------------------------------------------------------------------
		public string DefaultConstraintName
		{
			get
			{
				return (defaultConstraintName != null) ? defaultConstraintName.Trim() : String.Empty;
			}
			set
			{
				// ATTENZIONE: solo in SQL viene dato un nome al constraint di DEFAULT
				// pertanto deve essere controllato il valore solo in base alle regole di sql
				if (Generics.IsValidSQLServerDBObjectName(value))
					defaultConstraintName = value;
			}
		}

		//---------------------------------------------------------------------------
		public bool ExtraAdded { get { return extraAdded; } set { extraAdded = value; } }
		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)

		#endregion

		#region WizardTableColumnInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------
		public string GetDataObjClassName()
		{
			if (dataType == null)
				return String.Empty;

			return WizardTableColumnDataType.GetDataObjClassName(dataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetWoormBaseDataTypeText()
		{
			if (dataType == null)
				return String.Empty;

			return WizardTableColumnDataType.GetWoormDataTypeText(dataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetWoormDataTypeText()
		{
			string dataTypeText = GetWoormBaseDataTypeText();

			if
				(
				dataTypeText != null &&
				dataTypeText.Length > 0 &&
				dataType.Type == WizardTableColumnDataType.DataType.Enum &&
				enumInfo != null
				)
				dataTypeText += "[" + enumInfo.Value.ToString() + "]";

			return dataTypeText;
		}

		//---------------------------------------------------------------------
		public uint GetWoormDefaultDataLength()
		{
			if (dataType == null)
				return 0;

			switch (dataType.Type)
			{
				case WizardTableColumnDataType.DataType.Undefined:
					return 0;

				case WizardTableColumnDataType.DataType.String:
					return dataLength;

				case WizardTableColumnDataType.DataType.Short:
					return 6;

				case WizardTableColumnDataType.DataType.Long:
					return 10;

				case WizardTableColumnDataType.DataType.Double:
				case WizardTableColumnDataType.DataType.Monetary:
				case WizardTableColumnDataType.DataType.Quantity:
				case WizardTableColumnDataType.DataType.Percent:
					return 15;

				case WizardTableColumnDataType.DataType.Date:
					return 19;

				case WizardTableColumnDataType.DataType.Boolean:
					return 1;

				case WizardTableColumnDataType.DataType.Enum:
					return 10;

				case WizardTableColumnDataType.DataType.Text:
				case WizardTableColumnDataType.DataType.NText:
					return 255;

				case WizardTableColumnDataType.DataType.Guid:
					return 38;

				default:
					break;
			}

			return 0;
		}

		//---------------------------------------------------------------------
		public string GetDefaultFormatStyleName()
		{
			if (dataType == null)
				return String.Empty;

			return WizardTableColumnDataType.GetDefaultFormatStyleName(dataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetDataTypeTBXmlValue()
		{
			if (dataType == null)
				return String.Empty;

			return WizardTableColumnDataType.GetDataTypeTBXmlValue(dataType.Type);
		}

		//---------------------------------------------------------------------
		public bool HasSameDefaultValueAs(WizardTableColumnInfo aColumnInfo)
		{
			if (aColumnInfo == null || dataType == null || !WizardTableColumnDataType.Equals(dataType, aColumnInfo.DataType))
				return false;

			if (aColumnInfo == this || aColumnInfo.DefaultValue == defaultValue)
				return true;

			// Nel caso di enumerativi il valore di default è deciso dal tipo di 
			// enumerativo (si ha defaultValue uguale a null)
			if (dataType.Type == WizardTableColumnDataType.DataType.Enum)
				return WizardEnumInfo.Equals(enumInfo, aColumnInfo.EnumInfo);

			object dataTypeDefaultValue = WizardTableColumnDataType.GetDataTypeDefaultValue(dataType.Type);

			if (defaultValue == null)
			{
				object columnDefaultValue = aColumnInfo.DefaultValue;

				if (columnDefaultValue == null)
					return true;

				switch (dataType.Type)
				{
					case WizardTableColumnDataType.DataType.String:
					case WizardTableColumnDataType.DataType.Text:
					case WizardTableColumnDataType.DataType.NText:
						if (columnDefaultValue is string)
							return String.Compare((string)columnDefaultValue, (string)dataTypeDefaultValue) == 0;
						break;

					case WizardTableColumnDataType.DataType.Short:
						if (columnDefaultValue is Int16)
							return Int16.Equals(columnDefaultValue, dataTypeDefaultValue);
						break;

					case WizardTableColumnDataType.DataType.Long:
						if (columnDefaultValue is Int32)
							return Int32.Equals(columnDefaultValue, dataTypeDefaultValue);
						break;

					case WizardTableColumnDataType.DataType.Double:
					case WizardTableColumnDataType.DataType.Monetary:
					case WizardTableColumnDataType.DataType.Percent:
					case WizardTableColumnDataType.DataType.Quantity:
						if (columnDefaultValue is double)
							return double.Equals(columnDefaultValue, dataTypeDefaultValue);
						break;

					case WizardTableColumnDataType.DataType.Boolean:
						if (columnDefaultValue is bool)
							return bool.Equals(columnDefaultValue, dataTypeDefaultValue);
						break;

					case WizardTableColumnDataType.DataType.Date:
						if (columnDefaultValue is DateTime)
							return DateTime.Equals(columnDefaultValue, dataTypeDefaultValue);

						break;

					case WizardTableColumnDataType.DataType.Guid:
						if (columnDefaultValue is Guid)
							return Guid.Equals(columnDefaultValue, dataTypeDefaultValue);
						break;

					default:
						break;
				}
				return (columnDefaultValue == dataTypeDefaultValue);
			}

			if (aColumnInfo.DefaultValue == null)
				return (defaultValue == dataTypeDefaultValue);

			if (
				dataType.Type == WizardTableColumnDataType.DataType.String ||
				dataType.Type == WizardTableColumnDataType.DataType.Text ||
				dataType.Type == WizardTableColumnDataType.DataType.NText
				)
			{
				if (!(defaultValue is string) || !(aColumnInfo.DefaultValue is string))
					throw new ArgumentException();

				if (((string)defaultValue).Length == 0)
					return (((string)aColumnInfo.DefaultValue).Length == 0);

				return (String.Compare((string)defaultValue, (string)aColumnInfo.DefaultValue) == 0);
			}

			if (dataType.Type == WizardTableColumnDataType.DataType.Short)
			{
				if (!(defaultValue is Int16) || !(aColumnInfo.DefaultValue is Int16))
					throw new ArgumentException();

				return ((Int16)defaultValue == (Int16)aColumnInfo.DefaultValue);
			}

			if (dataType.Type == WizardTableColumnDataType.DataType.Long)
			{
				if (!(defaultValue is Int32) || !(aColumnInfo.DefaultValue is Int32))
					throw new ArgumentException();

				return ((Int32)defaultValue == (Int32)aColumnInfo.DefaultValue);
			}

			if (
				dataType.Type == WizardTableColumnDataType.DataType.Double ||
				dataType.Type == WizardTableColumnDataType.DataType.Monetary ||
				dataType.Type == WizardTableColumnDataType.DataType.Percent ||
				dataType.Type == WizardTableColumnDataType.DataType.Quantity
				)
			{
				if (!(defaultValue is double) || !(aColumnInfo.DefaultValue is double))
					throw new ArgumentException();

				return ((double)defaultValue == (double)aColumnInfo.DefaultValue);
			}

			if (dataType.Type == WizardTableColumnDataType.DataType.Boolean)
			{
				if (!(defaultValue is bool) || !(aColumnInfo.DefaultValue is bool))
					throw new ArgumentException();

				return ((bool)defaultValue == (bool)aColumnInfo.DefaultValue);
			}

			if (dataType.Type == WizardTableColumnDataType.DataType.Date)
			{
				if (!(defaultValue is DateTime) || !(aColumnInfo.DefaultValue is DateTime))
					throw new ArgumentException();

				return ((DateTime)defaultValue == (DateTime)aColumnInfo.DefaultValue);
			}

			if (dataType.Type == WizardTableColumnDataType.DataType.Guid)
			{
				if (!(defaultValue is Guid) || !(aColumnInfo.DefaultValue is Guid))
					throw new ArgumentException();

				return ((Guid)defaultValue == (Guid)aColumnInfo.DefaultValue);
			}

			MethodInfo equalsMethod = defaultValue.GetType().GetMethod("Equals");
			if (equalsMethod != null)
				return (bool)equalsMethod.Invoke(defaultValue, new object[] { aColumnInfo.DefaultValue });

			return false;
		}

		//---------------------------------------------------------------------
		public void SetDefaultValueFromString(string aDefaultValueText)
		{
			if
				(dataType != null &&
				dataType.Type == WizardTableColumnDataType.DataType.String &&
				aDefaultValueText != null &&
				aDefaultValueText.Length > dataLength
				)
				aDefaultValueText = ((string)aDefaultValueText).Substring(0, (int)dataLength);

            defaultValue = WizardTableColumnDataType.GetValueFromString(dataType.Type, aDefaultValueText);
		}

		//---------------------------------------------------------------------
		public string GetDBDefaultValueString(DBMSType aDBMSType)
		{
			if (dataType == null)
				return null;

			return WizardTableColumnDataType.GetDBValueString(dataType.Type, this.DefaultValue, aDBMSType);
		}

		//---------------------------------------------------------------------
		public string GetParsedControlClassName()
		{
			return WizardTableColumnDataType.GetDataTypeParsedControlClassName(dataType.Type);
		}

		//---------------------------------------------------------------------
		public string GetParsedStaticClassName()
		{
			return WizardTableColumnDataType.GetDataTypeParsedStaticClassName(dataType.Type);
		}
		#endregion
	}

	#endregion

	#region WizardTableColumnInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardTableColumnInfoCollection.
	/// </summary>
	public class WizardTableColumnInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				if (value != null && !(value is WizardTableColumnInfo))
					throw new NotSupportedException();

				this[index] = (WizardTableColumnInfo)value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardTableColumnInfo))
				throw new NotSupportedException();

			return this.Contains((WizardTableColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardTableColumnInfo)item);
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

			if (!(item is WizardTableColumnInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardTableColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardTableColumnInfo))
				throw new NotSupportedException();

			Insert(index, (WizardTableColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardTableColumnInfo))
				throw new NotSupportedException();

			Remove((WizardTableColumnInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo this[int index]
		{
			get { return (WizardTableColumnInfo)InnerList[index]; }
			set
			{
				InnerList[index] = (WizardTableColumnInfo)value;
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo[] ToArray()
		{
			return (WizardTableColumnInfo[])InnerList.ToArray(typeof(WizardTableColumnInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardTableColumnInfo aColumnToAdd)
		{
			if (Contains(aColumnToAdd))
				return IndexOf(aColumnToAdd);

			return InnerList.Add(aColumnToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardTableColumnInfoCollection aColumnsCollectionToAdd)
		{
			if (aColumnsCollectionToAdd == null || aColumnsCollectionToAdd.Count == 0)
				return;

			foreach (WizardTableColumnInfo aColumnToAdd in aColumnsCollectionToAdd)
				Add(aColumnToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardTableColumnInfo aColumnToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aColumnToInsert))
				return;

			InnerList.Insert(index, aColumnToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardTableColumnInfo beforeColumn, WizardTableColumnInfo aColumnToInsert)
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
		public void Remove(WizardTableColumnInfo aColumnToRemove)
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
		public bool Contains(WizardTableColumnInfo aColumnToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aColumnToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardTableColumnInfo aColumnToSearch)
		{
			if (!Contains(aColumnToSearch))
				return -1;

			return InnerList.IndexOf(aColumnToSearch);
		}

		//---------------------------------------------------------------------------
		public bool IsPrimaryKeyDefined
		{
			get
			{
				if (this.Count == 0)
					return false;

				for (int i = 0; i < this.Count; i++)
				{
					if (this[i].IsPrimaryKeySegment)
						return true;
				}

				return false;
			}
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardTableColumnInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardTableColumnInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardTableColumnInfo.Equals(this[i], ((WizardTableColumnInfoCollection)obj)[i]))
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
		public WizardTableColumnInfo GetColumnInfoByName(string aColumnName, ref int columnIndex)
		{
			if (this.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return null;

			aColumnName = aColumnName.Trim();
			if (aColumnName.Length == 0 || !Generics.IsValidTableColumnName(aColumnName))
				return null;

			for (int i = 0; i < this.Count; i++)
			{
				if (String.Compare(aColumnName, this[i].Name, true) == 0)
				{
					columnIndex = i;
					return this[i];
				}
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetColumnInfoByName(string aColumnName)
		{
			int columnIndex = -1;
			return GetColumnInfoByName(aColumnName, ref columnIndex);
		}

		//---------------------------------------------------------------------------
		public bool HasSameColumns(WizardTableColumnInfoCollection columnsToCompare)
		{
			if (columnsToCompare == null)
				return (this.Count == 0);

			if (this.Count != columnsToCompare.Count)
				return false;

			for (int i = 0; i < this.Count; i++)
			{
				int j = 0;
				// Cerco nella seconda collection una colonna con lo stesso nome:
				for (j = 0; j < columnsToCompare.Count; j++)
				{
					if (String.Compare(this[i].Name, columnsToCompare[j].Name) == 0)
					{
						if
							(
							this[i].DataType.Type != columnsToCompare[j].DataType.Type ||
							this[i].DataLength != columnsToCompare[j].DataLength ||
							!this[i].HasSameDefaultValueAs(columnsToCompare[j]) ||
							this[i].IsPrimaryKeySegment != columnsToCompare[j].IsPrimaryKeySegment ||
							this[i].IsNullable != columnsToCompare[j].IsNullable ||
							this[i].IsCollateSensitive != columnsToCompare[j].IsCollateSensitive
							)
							return false; // la colonna non coincide
						break;
					}
				}
				if (j == columnsToCompare.Count)
					return false; // non c'è una colonna con lo stesso nome
			}

			return true;
		}
		//---------------------------------------------------------------------------
		public bool ContainsUpperCaseStringColumns()
		{
			if (this.Count == 0)
				return false;

			foreach (WizardTableColumnInfo aColumnInfo in this.InnerList)
			{
				if (aColumnInfo.IsUpperCaseDataString)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool IsUsingEnum(WizardEnumInfo aEnumInfo)
		{
			if (aEnumInfo == null || this.Count == 0)
				return false;

			foreach (WizardTableColumnInfo aColumnInfo in this.InnerList)
			{
				if (aColumnInfo.DataType.Type != WizardTableColumnDataType.DataType.Enum)
					continue;

				if (aEnumInfo.Equals(aColumnInfo.EnumInfo))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public bool ContainsDataEnumColumns()
		{
			if (this.Count == 0)
				return false;

			foreach (WizardTableColumnInfo aColumnInfo in this.InnerList)
			{
				if (aColumnInfo.EnumInfo != null)
					return true;
			}
			return false;
		}
	}

	#endregion

	#region WizardTableIndexInfo class

	//=================================================================================
	/// <summary>
	/// Summary description for WizardTableIndexInfo.
	/// </summary>
	public class WizardTableIndexInfo : IDisposable
	{
		#region WizardTableIndexInfo private data members

		private string name = String.Empty;
		private bool primary = false;
		private bool unique = false;
		private bool nonClustered = true;
		private WizardTableColumnInfoCollection segments = null;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)

		private bool disposed = false;
		private string tableName = string.Empty;

		#endregion

		//---------------------------------------------------------------------------
		public WizardTableIndexInfo(string aIndexName, bool aPrimaryFlag, bool isReadOnly)
		{
			name = aIndexName;
			primary = aPrimaryFlag;
			readOnly = isReadOnly;
		}

		//---------------------------------------------------------------------------
		public WizardTableIndexInfo(string aIndexName, bool aPrimaryFlag)
			: this(aIndexName, aPrimaryFlag, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardTableIndexInfo(string aIndexName)
			: this(aIndexName, false, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardTableIndexInfo(WizardTableIndexInfo aIndexInfo)
		{
			name = (aIndexInfo != null) ? aIndexInfo.Name : String.Empty;
			readOnly = (aIndexInfo != null) ? aIndexInfo.ReadOnly : false;

			if (aIndexInfo != null && aIndexInfo.SegmentsCount > 0)
			{
				foreach (WizardTableColumnInfo aSegmentInfoTo in aIndexInfo.Segments)
					this.AddSegmentInfo(new WizardTableColumnInfo(aSegmentInfoTo));
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
			if (obj == null || !(obj is WizardTableIndexInfo))
				return false;

			if (obj == this)
				return true;

			return
				(
				String.Compare(name, ((WizardTableIndexInfo)obj).Name) == 0 &&
				primary == ((WizardTableIndexInfo)obj).Primary &&
				WizardTableColumnInfoCollection.Equals(segments, ((WizardTableIndexInfo)obj).Segments)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region WizardTableIndexInfo public properties

		//---------------------------------------------------------------------------
		public string Name { get { return name; } set { if (Generics.IsValidEnumName(value)) name = value; } }
		//---------------------------------------------------------------------------
		public bool Primary { get { return primary; } set { primary = value; } }
		//---------------------------------------------------------------------------
		public bool Unique { get { return unique; } set { unique = value; } }
		//---------------------------------------------------------------------------
		public bool NonClustered { get { return nonClustered; } set { nonClustered = value; } }
		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection Segments { get { return segments; } }
		//---------------------------------------------------------------------------
		public int SegmentsCount { get { return (segments != null) ? segments.Count : 0; } }
		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)
		//---------------------------------------------------------------------------
		public string TableName { get { return tableName; } set { tableName = value; } }
		#endregion

		#region WizardTableIndexInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return name;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetSegmentInfoByName(string aColumnName)
		{
			if (segments == null || segments.Count == 0 || !Generics.IsValidTableColumnName(aColumnName))
				return null;

			foreach (WizardTableColumnInfo aSegmentInfoToInfo in segments)
			{
				if (String.Compare(aColumnName, aSegmentInfoToInfo.Name, true) == 0)
					return aSegmentInfoToInfo;
			}

			return null;
		}

		//---------------------------------------------------------------------------
		public int AddSegmentInfo(WizardTableColumnInfo aSegmentInfoToInfo)
		{
			if
				(
				aSegmentInfoToInfo == null ||
				aSegmentInfoToInfo.Name == null ||
				aSegmentInfoToInfo.Name.Length == 0 ||
				(primary && !aSegmentInfoToInfo.IsPrimaryKeySegment)
				)
				return -1;

			WizardTableColumnInfo alreadyExistingSegment = GetSegmentInfoByName(aSegmentInfoToInfo.Name);
			if (alreadyExistingSegment != null)
				return -1;

			if (segments == null)
				segments = new WizardTableColumnInfoCollection();

			return segments.Add(aSegmentInfoToInfo);
		}

		//---------------------------------------------------------------------------
		public void RemoveSegment(string aColumnName)
		{
			if (segments == null || segments.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return;

			WizardTableColumnInfo segmentToRemove = GetSegmentInfoByName(aColumnName);
			if (segmentToRemove == null)
				return;

			segments.Remove(segmentToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAllSegments()
		{
			if (segments == null || segments.Count == 0)
				return;

			segments.Clear();
		}

		//---------------------------------------------------------------------------
		public bool HasSameSegments(WizardTableColumnInfoCollection segmentsToCompare, bool checkSegmentsOrder)
		{
			if (segments == null)
				return (segmentsToCompare == null || segmentsToCompare.Count == 0);

			if (segmentsToCompare == null)
				return (segments.Count == 0);

			if (segments.Count != segmentsToCompare.Count)
				return false;

			for (int i = 0; i < segments.Count; i++)
			{
				if (!checkSegmentsOrder)
				{
					int j = 0;
					// Cerco nella seconda collection un elemento equivalente:
					for (j = 0; j < segmentsToCompare.Count; j++)
					{
						if (WizardTableColumnInfo.Equals(segments[i], segmentsToCompare[j]))
							break;
					}
					if (j == segmentsToCompare.Count)
						return false; // non c'è una colonna con lo stesso nome
				}
				else
				{
					if (!WizardTableColumnInfo.Equals(segments[i], segmentsToCompare[i]))
						return false;
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool IsSameIndexAs(WizardTableIndexInfo aIndexToCompare)
		{
			if (aIndexToCompare == null)
				return false;

			return HasSameSegments(aIndexToCompare.Segments, true);
		}

		#endregion
	}

	#endregion

	#region WizardExtraAddedColumnsInfo class

	/// <summary>
	/// Colonna aggiunta da alter utilizzata nello strumento che parsa sql->xml, 
	/// gestisce namespace della library e nome tabella e namespace Tabella
	/// </summary>
	//=================================================================================
	public class DBObjectsExtraAddedColumnsInfo : WizardExtraAddedColumnsInfo
	{
		protected string tableName = String.Empty;
		protected string libraryNameSpace = String.Empty;
		protected string tbNameSpace = String.Empty;

		//---------------------------------------------------------------------------
		public DBObjectsExtraAddedColumnsInfo(string aTableNameSpace, string aTableName)
			: base(aTableNameSpace)
		{
			tableName = aTableName;
		}

		//---------------------------------------------------------------------------
		public override string TableName { get { return tableName; } }
		//---------------------------------------------------------------------------
		public string LibraryNameSpace { get { return libraryNameSpace; } set { libraryNameSpace = value; } }
		//---------------------------------------------------------------------------
		public string TbNameSpace { get { return tbNameSpace; } set { tbNameSpace = value; } }
	}

	//=================================================================================
	/// <summary>
	/// Summary description for WizardExtraAddedColumnsInfo.
	/// </summary>
	public class WizardExtraAddedColumnsInfo : IDisposable
	{
		#region WizardExtraAddedColumnsInfo private data members

		private WizardLibraryInfo library = null;

		protected string tableNameSpace = String.Empty;

		private WizardTableColumnInfoCollection columns = null;

		private string className = String.Empty;

		private string referencedTableIncludeFile = String.Empty;

		private TableHistoryInfo history = null;

		private uint creationDbReleaseNumber = 0;

		private bool readOnly = false; // struttura non modificabile (caricata da ReverseEngineer)
		private bool referenced = false;

		private bool disposed = false;

		#endregion

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo(string aTableNameSpace, bool isReadOnly, bool isReferenced)
		{
			this.TableNameSpace = aTableNameSpace;
			readOnly = isReadOnly;
			referenced = isReferenced;
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo(string aTableNameSpace, bool isReadOnly)
			: this(aTableNameSpace, isReadOnly, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo(string aTableNameSpace)
			: this(aTableNameSpace, false)
		{
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraColumnInfo, bool setDefaultConstraintName)
		{
			library = (aExtraColumnInfo != null) ? aExtraColumnInfo.Library : null;

			tableNameSpace = (aExtraColumnInfo != null) ? aExtraColumnInfo.TableNameSpace : String.Empty;

			className = (aExtraColumnInfo != null) ? aExtraColumnInfo.ClassName : String.Empty;

			referencedTableIncludeFile = (aExtraColumnInfo != null) ? aExtraColumnInfo.ReferencedTableIncludeFile : String.Empty;

			creationDbReleaseNumber = (aExtraColumnInfo != null) ? aExtraColumnInfo.CreationDbReleaseNumber : 0;

			history = (aExtraColumnInfo != null && aExtraColumnInfo.History != null) ? new TableHistoryInfo(aExtraColumnInfo.History) : null;

			readOnly = (aExtraColumnInfo != null) ? aExtraColumnInfo.ReadOnly : false;
			referenced = (aExtraColumnInfo != null) ? aExtraColumnInfo.IsReferenced : false;

			if (aExtraColumnInfo != null && aExtraColumnInfo.ColumnsInfo != null && aExtraColumnInfo.ColumnsInfo.Count > 0)
			{
				foreach (WizardTableColumnInfo aColumnInfo in aExtraColumnInfo.ColumnsInfo)
					this.AddColumnInfo(new WizardTableColumnInfo(aColumnInfo), setDefaultConstraintName);
			}
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo(WizardExtraAddedColumnsInfo aExtraColumnInfo)
			: this(aExtraColumnInfo, true)
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

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo, bool autoDbRelease, bool refreshColumnDefaultConstraintNames)
		{
			if (library == aLibraryInfo)
				return;

			if (library != null && library.ExtraAddedColumnsInfo.Contains(this))
				library.ExtraAddedColumnsInfo.Remove(this);

			string tableName = this.TableName;
			if
				(
				tableName == null ||
				tableName.Length == 0 ||
				(library != null && !library.IsTableAvailable(tableName))
				)
				return;

			library = aLibraryInfo;

			if (autoDbRelease)
				SetCreationDbReleaseNumber((library != null && library.Module != null) ? library.Module.DbReleaseNumber : 0);

			if (refreshColumnDefaultConstraintNames)
				RefreshColumnDefaultConstraintNames();
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo, bool autoDbRelease)
		{
			SetLibrary(aLibraryInfo, autoDbRelease, true);
		}

		//---------------------------------------------------------------------------
		internal void SetLibrary(WizardLibraryInfo aLibraryInfo)
		{
			SetLibrary(aLibraryInfo, true);
		}

		//---------------------------------------------------------------------------
		internal void SetCreationDbReleaseNumber(uint aReleaseNumber, bool forceSetting)
		{
			if
				(
				creationDbReleaseNumber == aReleaseNumber ||
				(!forceSetting && creationDbReleaseNumber > 0 && aReleaseNumber < creationDbReleaseNumber)
				)
				return;

			creationDbReleaseNumber = aReleaseNumber;

			if (columns == null || columns.Count == 0)
				return;

			foreach (WizardTableColumnInfo aColumnInfo in columns)
			{
				if
					(
					aColumnInfo.CreationDbReleaseNumber != aReleaseNumber &&
					(aColumnInfo.CreationDbReleaseNumber == 0 || aReleaseNumber > aColumnInfo.CreationDbReleaseNumber)
					)
					aColumnInfo.CreationDbReleaseNumber = aReleaseNumber;
			}
		}

		//---------------------------------------------------------------------------
		internal void SetCreationDbReleaseNumber(uint aReleaseNumber)
		{
			SetCreationDbReleaseNumber(aReleaseNumber, false);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardExtraAddedColumnsInfo))
				return false;

			if (obj == this)
				return true;

			return
				(
				String.Compare(tableNameSpace, ((WizardExtraAddedColumnsInfo)obj).TableNameSpace) == 0 &&
				String.Compare(className, ((WizardExtraAddedColumnsInfo)obj).ClassName) == 0 &&
				String.Compare(referencedTableIncludeFile, ((WizardExtraAddedColumnsInfo)obj).ReferencedTableIncludeFile, true) == 0 &&
				creationDbReleaseNumber == ((WizardExtraAddedColumnsInfo)obj).CreationDbReleaseNumber &&
				WizardTableColumnInfoCollection.Equals(columns, ((WizardExtraAddedColumnsInfo)obj).ColumnsInfo) &&
				TableHistoryInfo.Equals(history, ((WizardExtraAddedColumnsInfo)obj).History)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region WizardExtraAddedColumnsInfo public properties

		//---------------------------------------------------------------------------
		public WizardLibraryInfo Library { get { return library; } }

		//---------------------------------------------------------------------------
		public virtual string TableName
		{
			get
			{
				if (tableNameSpace == null || tableNameSpace.Trim().Length == 0)
					return String.Empty;

				NameSpace tmpNameSpace = new NameSpace(tableNameSpace, NameSpaceObjectType.Table);

				return tmpNameSpace.Table;
			}

		}

		//---------------------------------------------------------------------------
		public virtual string TableNameSpace
		{
			get { return tableNameSpace; }

			set
			{
				if (value != null && value.Trim().Length > 0)
				{
					if (String.Compare(tableNameSpace, value.Trim()) == 0)
						return;
					NameSpace tmpNameSpace = new NameSpace(value.Trim(), NameSpaceObjectType.Table);
					tableNameSpace = (tmpNameSpace.IsValid()) ? value.Trim() : String.Empty;
				}
				else
					tableNameSpace = String.Empty;

				RefreshColumnDefaultConstraintNames();
			}
		}

		//---------------------------------------------------------------------------
		public string ClassName
		{
			get { return className; }
			set
			{
				if (value == null || !Generics.IsValidClassName(value))
					className = GetDefaultExtraAddedColumnClassName();
				else
					className = value.Trim();
			}
		}

		//---------------------------------------------------------------------------
		public string ReferencedTableIncludeFile
		{
			get
			{
				WizardTableInfo originalTableInfo = this.GetOriginalTableInfo();
				if (originalTableInfo == null || !originalTableInfo.IsReferenced)
					return String.Empty;

				if (referencedTableIncludeFile != null && referencedTableIncludeFile.Length > 0)
					return referencedTableIncludeFile;

				string libraryPath = WizardCodeGenerator.GetStandardLibraryPath(originalTableInfo.Library);
				if (libraryPath == null || libraryPath.Length == 0)
					return String.Empty;

				return libraryPath + Path.DirectorySeparatorChar + originalTableInfo.ClassName + Generics.CppHeaderExtension;
			}
			set
			{
				WizardTableInfo originalTableInfo = this.GetOriginalTableInfo();
				if (originalTableInfo == null || !originalTableInfo.IsReferenced || value == null)
				{
					referencedTableIncludeFile = String.Empty;
					return;
				}

				string includeFile = value.Trim();

				referencedTableIncludeFile = (includeFile.Length > 0 && Generics.IsValidFullPathName(includeFile)) ? includeFile : String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfoCollection ColumnsInfo { get { return columns; } }

		//---------------------------------------------------------------------------
		public int ColumnsCount { get { return (columns != null) ? columns.Count : 0; } }

		//---------------------------------------------------------------------------
		public uint CreationDbReleaseNumber { get { return creationDbReleaseNumber; } set { creationDbReleaseNumber = value; } }

		//---------------------------------------------------------------------------
		public TableHistoryInfo History { get { return history; } }

		//---------------------------------------------------------------------------
		public bool ReadOnly { get { return readOnly; } } // struttura non modificabile (caricata da ReverseEngineer)

		//---------------------------------------------------------------------------
		public bool IsReferenced { get { return referenced; } }

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo ColumnAtZeroIndex
		{
			get
			{
				if (this.columns == null || this.columns.Count <= 0)
					return null;

				return this.columns[0];
			}
		}
		#endregion

		#region WizardExtraAddedColumnsInfo public methods

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return className;
		}

		//---------------------------------------------------------------------------
		public WizardTableColumnInfo GetColumnInfoByName(string aColumnName)
		{
			if (columns == null || columns.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return null;

			return columns.GetColumnInfoByName(aColumnName);
		}

		//---------------------------------------------------------------------------
		public int AddColumnInfo(WizardTableColumnInfo aColumnInfo, bool setDefaultConstraintName)
		{
			if (aColumnInfo == null || aColumnInfo.Name == null || aColumnInfo.Name.Length == 0)
				return -1;

			WizardTableColumnInfo alreadyExistingColumn = GetColumnInfoByName(aColumnInfo.Name);
			if (alreadyExistingColumn != null)
				return -1;

			if (columns == null)
				columns = new WizardTableColumnInfoCollection();

			aColumnInfo.IsPrimaryKeySegment = false;
			aColumnInfo.ExtraAdded = true;

			int addedColumnIndex = columns.Add(aColumnInfo);
			if (addedColumnIndex >= 0 && library != null && !library.ReadOnly && !referenced && setDefaultConstraintName && (aColumnInfo.DefaultConstraintName == null || aColumnInfo.DefaultConstraintName.Length == 0))
			{
				WizardTableInfo originalTableInfo = GetOriginalTableInfo();
				if (originalTableInfo != null)
					columns[addedColumnIndex].DefaultConstraintName = originalTableInfo.GetColumnDefaultConstraintDefaultName(columns[addedColumnIndex], true);
			}

			return addedColumnIndex;
		}

		//---------------------------------------------------------------------------
		public int AddColumnInfo(WizardTableColumnInfo aColumnInfo)
		{
			return AddColumnInfo(aColumnInfo, true);
		}

		//---------------------------------------------------------------------------
		public void RemoveColumn(string aColumnName)
		{
			if (columns == null || columns.Count == 0 || aColumnName == null || aColumnName.Length == 0)
				return;

			WizardTableColumnInfo columnToRemove = GetColumnInfoByName(aColumnName);
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
		public string GetDefaultExtraAddedColumnClassName()
		{
			if (tableNameSpace == null || tableNameSpace.Trim().Length == 0)
				return String.Empty;

			string tableName = this.TableName;
			if (tableName == null || tableName.Trim().Length == 0)
				return String.Empty;

			return GetDefaultExtraAddedColumnClassName(tableName);
		}

		//---------------------------------------------------------------------------
		public static string GetDefaultExtraAddedColumnClassName(string aTableName)
		{
			if (aTableName == null || aTableName.Trim().Length == 0)
				return String.Empty;

			return "T" + Generics.SubstitueInvalidCharacterInIdentifier(aTableName.Trim().Replace(' ', '_')) + "AdditionalColumns";
		}

		//---------------------------------------------------------------------------
		public WizardTableInfo GetOriginalTableInfo()
		{
			if
				(
				library == null ||
				tableNameSpace == null ||
				tableNameSpace.Trim().Length == 0
				)
				return null;

			string tableName = this.TableName;
			if (tableName == null || tableName.Length == 0)
				return null;

			return library.GetTableInfoByName(tableName, true);
		}

		//---------------------------------------------------------------------------
		public void RefreshColumnDefaultConstraintNames(bool forceReset)
		{
			if (referenced || columns == null || columns.Count == 0 || library == null)
				return;

			WizardTableInfo originalTableInfo = GetOriginalTableInfo();
			if (originalTableInfo == null)
				return;

			foreach (WizardTableColumnInfo aColumnInfo in columns)
			{
				if (forceReset || aColumnInfo.DefaultConstraintName == null || aColumnInfo.DefaultConstraintName.Length == 0)
					aColumnInfo.DefaultConstraintName = originalTableInfo.GetColumnDefaultConstraintDefaultName(aColumnInfo, true);
			}
		}

		//---------------------------------------------------------------------------
		public void RefreshColumnDefaultConstraintNames()
		{
			RefreshColumnDefaultConstraintNames(false);
		}

		//---------------------------------------------------------------------------
		public bool HasSameColumns(WizardTableColumnInfoCollection columnsToCompare)
		{
			if (columns == null)
				return (columnsToCompare == null || columnsToCompare.Count == 0);

			return columns.HasSameColumns(columnsToCompare);
		}

		//---------------------------------------------------------------------------
		public bool IsUsingEnum(WizardEnumInfo aEnumInfo)
		{
			return (columns != null) ? columns.IsUsingEnum(aEnumInfo) : false;
		}

		//---------------------------------------------------------------------------
		public bool ContainsDataEnumColumns()
		{
			return (columns != null) ? columns.ContainsDataEnumColumns() : false;
		}

		//---------------------------------------------------------------------------
		public TableHistoryStep GetNearestDbReleaseStep(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || history == null || history.StepsCount == 0)
				return null;

			return history.GetNearestDbReleaseStep(aDbReleaseNumber);
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo GetAdditionalColumsAtDbRelease(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || creationDbReleaseNumber > aDbReleaseNumber)
				return null;

			if (history == null || history.StepsCount == 0)
				return this;

			WizardExtraAddedColumnsInfo historicAdditionalColumsInfo = new WizardExtraAddedColumnsInfo(this);

			return (historicAdditionalColumsInfo.RollbackToDbRelease(aDbReleaseNumber)) ? historicAdditionalColumsInfo : null;
		}

		//---------------------------------------------------------------------------
		public bool RollbackToDbRelease(uint aDbReleaseNumber)
		{
			if (aDbReleaseNumber <= 0 || creationDbReleaseNumber > aDbReleaseNumber)
				return false;

			if (history == null || history.StepsCount == 0)
				return true;

			// Devo partire dall'insieme di colonne aggiuntive nel suo stato attuale e 
			// riprodurre all'incontrario tutti gli step successivi al numero di release 
			// passato come argomento
			TableHistoryStepsCollection stepsAfter = history.GetStepsAfterDbRelease(aDbReleaseNumber);

			if (stepsAfter == null || stepsAfter.Count == 0)
				return true;

			for (int i = (stepsAfter.Count - 1); i >= 0; i--)
			{
				TableHistoryStep stepToRollback = stepsAfter[i];

				history.Steps.Remove(stepToRollback);

				if (stepToRollback.EventsCount == 0)
					continue;

				if (stepToRollback.ColumnsEventsCount > 0)
				{
					foreach (ColumnHistoryEvent aColumnEvent in stepToRollback.ColumnsEvents)
					{
						WizardTableColumnInfo currentColumn = aColumnEvent.ColumnInfo;
						WizardTableColumnInfo previousColumn = aColumnEvent.PreviousColumnInfo;
						if (previousColumn == null)
						{
							if (currentColumn == null)
								continue;

							if (aColumnEvent.Type == TableHistoryStep.EventType.AddColumn)
							{
								// La colonna è stata aggiunta durante questo scatto di release
								RemoveColumn(currentColumn.Name);
								continue;
							}
							if (aColumnEvent.Type == TableHistoryStep.EventType.DropColumn)
							{
								// La colonna è stata rimossa durante questo scatto di release
								AddColumnInfo(currentColumn);
								continue;
							}
						}
						else
						{
							if
								(
								aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType ||
								aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue
								)
							{
								WizardTableColumnInfo columnToAlter = GetColumnInfoByName(currentColumn.Name);
								if (columnToAlter != null)
								{
									if
										(
										aColumnEvent.Type == TableHistoryStep.EventType.AlterColumnType &&
										previousColumn.DataType != null &&
										previousColumn.DataType.Type != WizardTableColumnDataType.DataType.Undefined
										)
									{
										columnToAlter.DataType = previousColumn.DataType;
										columnToAlter.DataLength = previousColumn.DataLength;
									}
									columnToAlter.IsPrimaryKeySegment = previousColumn.IsPrimaryKeySegment;

									// Se per la colonna cambia il valore default e si tratta di un 
									// enumerativo devo anche modificare enumInfo
									if
										(
										aColumnEvent.Type == TableHistoryStep.EventType.ChangeColumnDefaultValue &&
										columnToAlter.DataType.Type == WizardTableColumnDataType.DataType.Enum
										)
									{
										columnToAlter.EnumInfo = previousColumn.EnumInfo;
									}
									else
										columnToAlter.DefaultValue = previousColumn.DefaultValue;
								}
								continue;
							}
						}
					}

					// Risistemo l'ordine delle colonne
					foreach (ColumnHistoryEvent aColumnEvent in stepToRollback.ColumnsEvents)
					{
						if (aColumnEvent.Type != TableHistoryStep.EventType.ChangeColumnOrder)
							continue;

						if
							(
							aColumnEvent.ColumnInfo == null ||
							aColumnEvent.PreviousColumnOrder < 0 ||
							aColumnEvent.ColumnOrder == aColumnEvent.PreviousColumnOrder
							)
							continue;

						WizardTableColumnInfo columnToMove = GetColumnInfoByName(aColumnEvent.ColumnInfo.Name);
						if (columnToMove != null)
						{
							columns.Remove(columnToMove);
							columns.Insert(aColumnEvent.PreviousColumnOrder, columnToMove);
						}
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public void ClearHistory()
		{
			if (history != null)
				history.ClearSteps();

			history = null;
		}

		//---------------------------------------------------------------------------
		private TableHistoryStep BuildHistoryStep
			(
			uint aDbReleaseNumber,
			WizardTableColumnInfoCollection newColumnsInfo
			)
		{
			if (aDbReleaseNumber == 0 || newColumnsInfo == null || newColumnsInfo.Count == 0)
				return null;

			TableHistoryStep historyStepToAdd = new TableHistoryStep(aDbReleaseNumber);

			for (int i = 0; i < newColumnsInfo.Count; i++)
			{
				WizardTableColumnInfo aColumnInfo = newColumnsInfo[i];
				if (aColumnInfo == null)
					continue;

				WizardTableColumnInfo existingColumnInfo = GetColumnInfoByName(aColumnInfo.Name);
				if (existingColumnInfo != null)
				{
					// La definizione della colonna esiste già e quindi è stata modificata
					// da un punto di vista strutturale
					if (
						!aColumnInfo.DataType.Equals(existingColumnInfo.DataType) ||
						aColumnInfo.DataLength != existingColumnInfo.DataLength
						)
					{
						historyStepToAdd.AddAlterColumnTypeEvent(aColumnInfo, i, existingColumnInfo);
					}
					else if (!aColumnInfo.HasSameDefaultValueAs(existingColumnInfo))
					{
						historyStepToAdd.AddChangeColumnDefaultValueEvent(aColumnInfo, i, existingColumnInfo);
					}

					int previousColumnIndex = columns.IndexOf(existingColumnInfo);
					if (previousColumnIndex != i) // La colonna è stata spostata
						historyStepToAdd.AddChangeColumnOrderEvent(aColumnInfo, i, previousColumnIndex);
				}
				else // La definizione della colonna è stata aggiunta
				{
					historyStepToAdd.AddAddColumnEvent(aColumnInfo, i);
				}
			}

			// Adesso esamino eventuali colonne rimosse
			if (columns != null && columns.Count > 0)
			{
				for (int i = 0; i < columns.Count; i++)
				{
					bool columnToDrop = true;
					for (int j = 0; j < newColumnsInfo.Count; j++)
					{
						if (String.Compare(columns[i].Name, newColumnsInfo[j].Name, true) == 0)
						{
							columnToDrop = false; // la colonna esiste ancora
							break;
						}
					}
					if (!columnToDrop)
						continue;

					historyStepToAdd.AddDropColumnEvent(columns[i], i);
				}
			}

			return historyStepToAdd;
		}

		//---------------------------------------------------------------------------
		public int AddHistoryStep(TableHistoryStep aHistoryStep)
		{
			if
				(
				aHistoryStep == null ||
				aHistoryStep.DbReleaseNumber == 0 ||
				aHistoryStep.DbReleaseNumber == creationDbReleaseNumber ||
				aHistoryStep.EventsCount == 0
				)
				return -1;

			if (history == null)
				history = new TableHistoryInfo();

			return history.AddHistoryStep(aHistoryStep);
		}

		//---------------------------------------------------------------------------
		public int AddHistoryStep(uint aDbReleaseNumber, WizardTableColumnInfoCollection newColumnsInfo)
		{
			return AddHistoryStep(BuildHistoryStep(aDbReleaseNumber, newColumnsInfo));
		}

		//---------------------------------------------------------------------------
		public bool IsToUpgrade(uint aDbReleaseNumber)
		{
			if (
				history == null ||
				history.StepsCount == 0 ||
				aDbReleaseNumber <= creationDbReleaseNumber
				)
				return false;

			return (history.GetDbReleaseStep(aDbReleaseNumber) != null);
		}

		#endregion // WizardExtraAddedColumnsInfo public methods
	}

	#endregion

	#region WizardExtraAddedColumnsInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardExtraAddedColumnsInfoCollection.
	/// </summary>
	public class WizardExtraAddedColumnsInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				if (value != null && !(value is WizardExtraAddedColumnsInfo))
					throw new NotSupportedException();

				this[index] = (WizardExtraAddedColumnsInfo)value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardExtraAddedColumnsInfo))
				throw new NotSupportedException();

			return this.Contains((WizardExtraAddedColumnsInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardExtraAddedColumnsInfo)item);
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

			if (!(item is WizardExtraAddedColumnsInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardExtraAddedColumnsInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardExtraAddedColumnsInfo))
				throw new NotSupportedException();

			Insert(index, (WizardExtraAddedColumnsInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardExtraAddedColumnsInfo))
				throw new NotSupportedException();

			Remove((WizardExtraAddedColumnsInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo this[int index]
		{
			get { return (WizardExtraAddedColumnsInfo)InnerList[index]; }
			set
			{
				InnerList[index] = (WizardExtraAddedColumnsInfo)value;
			}
		}

		//---------------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo[] ToArray()
		{
			return (WizardExtraAddedColumnsInfo[])InnerList.ToArray(typeof(WizardExtraAddedColumnsInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardExtraAddedColumnsInfo aExtraColumnToAdd)
		{
			if (Contains(aExtraColumnToAdd))
				return IndexOf(aExtraColumnToAdd);

			return InnerList.Add(aExtraColumnToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardExtraAddedColumnsInfoCollection aExtraColumnsCollectionToAdd)
		{
			if (aExtraColumnsCollectionToAdd == null || aExtraColumnsCollectionToAdd.Count == 0)
				return;

			foreach (WizardExtraAddedColumnsInfo aExtraColumnToAdd in aExtraColumnsCollectionToAdd)
				Add(aExtraColumnToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardExtraAddedColumnsInfo aExtraColumnToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aExtraColumnToInsert))
				return;

			InnerList.Insert(index, aExtraColumnToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardExtraAddedColumnsInfo beforeExtraColumn, WizardExtraAddedColumnsInfo aExtraColumnToInsert)
		{
			if (beforeExtraColumn == null)
				Add(aExtraColumnToInsert);

			if (!Contains(beforeExtraColumn))
				return;

			if (Contains(aExtraColumnToInsert))
				return;

			Insert(IndexOf(beforeExtraColumn), aExtraColumnToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardExtraAddedColumnsInfo aExtraColumnToRemove)
		{
			if (!Contains(aExtraColumnToRemove))
				return;

			InnerList.Remove(aExtraColumnToRemove);
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
		public bool Contains(WizardExtraAddedColumnsInfo aExtraColumnToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aExtraColumnToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardExtraAddedColumnsInfo aExtraColumnToSearch)
		{
			if (!Contains(aExtraColumnToSearch))
				return -1;

			return InnerList.IndexOf(aExtraColumnToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardExtraAddedColumnsInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardExtraAddedColumnsInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardExtraAddedColumnsInfo.Equals(this[i], ((WizardExtraAddedColumnsInfoCollection)obj)[i]))
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

	#region ColumnNamesPair Class

	//===========================================================================
	public class ColumnNamesPair
	{
		private string previousName = String.Empty;
		private string currentName = String.Empty;

		public ColumnNamesPair(string aPreviousName, string aCurrentName)
		{
			previousName = aPreviousName;
			currentName = aCurrentName;
		}

		public string PreviousName { get { return previousName; } }
		public string CurrentName { get { return currentName; } set { currentName = value; } }
	}

	#endregion

	#region WizardForeignKeyInfo Class

	//===========================================================================
	public class DBObjectsForeignKeyInfo : WizardForeignKeyInfo
	{
		//---------------------------------------------------------------------------
		public override string ReferencedTableNameSpace
		{
			get { return referencedTableNameSpace ?? string.Empty; }

			set { referencedTableNameSpace = value.Trim() ?? String.Empty; }
		}
		//---------------------------------------------------------------------------
		public override string ReferencedTableName
		{
			get { return ReferencedTableNameSpace; }
		}
		//---------------------------------------------------------------------
		public DBObjectsForeignKeyInfo(string aTableNameSpace)
			: base(aTableNameSpace)
		{

		}

	}

	//===========================================================================
	public class WizardForeignKeyInfo : IDisposable
	{

		//===========================================================================
		public class KeySegment
		{
			private string columnName = String.Empty;
			private string referencedColumnName = String.Empty;

			//---------------------------------------------------------------------
			public KeySegment(string aColumnName, string aReferencedColumnName)
			{
				columnName = (aColumnName != null) ? aColumnName.Trim() : String.Empty;
				referencedColumnName = (aReferencedColumnName != null) ? aReferencedColumnName.Trim() : String.Empty;
			}

			//---------------------------------------------------------------------
			public KeySegment(KeySegment aSegmentInfo)
				: this((aSegmentInfo != null) ? aSegmentInfo.ColumnName : String.Empty, (aSegmentInfo != null) ? aSegmentInfo.ReferencedColumnName : String.Empty)
			{
			}

			public string ColumnName { get { return columnName; } }
			public string ReferencedColumnName { get { return referencedColumnName; } set { referencedColumnName = (value != null) ? value.Trim() : String.Empty; } }
		}

		//===========================================================================
		public class KeySegmentsCollection : ReadOnlyCollectionBase, IList
		{
			//---------------------------------------------------------------------------
			public KeySegmentsCollection()
			{
			}

			#region IList implemented members

			//--------------------------------------------------------------------------------------------------------------------------------
			object IList.this[int index]
			{
				get { return this[index]; }
				set
				{
					if (value != null && !(value is KeySegment))
						throw new NotSupportedException();

					this[index] = (KeySegment)value;
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			bool IList.Contains(object item)
			{
				if (item == null)
					throw new ArgumentNullException();

				if (!(item is KeySegment))
					throw new NotSupportedException();

				return this.Contains((KeySegment)item);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			int IList.Add(object item)
			{
				return Add((KeySegment)item);
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

				if (!(item is KeySegment))
					throw new NotSupportedException();

				return this.IndexOf((KeySegment)item);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			void IList.Insert(int index, object item)
			{
				if (item == null)
					throw new ArgumentNullException();

				if (!(item is KeySegment))
					throw new NotSupportedException();

				Insert(index, (KeySegment)item);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			void IList.Remove(object item)
			{
				if (item == null)
					return;

				if (!(item is KeySegment))
					throw new NotSupportedException();

				Remove((KeySegment)item);
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			void IList.RemoveAt(int index)
			{
				RemoveAt(index);
			}

			#endregion

			//---------------------------------------------------------------------------
			public KeySegment this[int index]
			{
				get { return (KeySegment)InnerList[index]; }
				set
				{
					InnerList[index] = (KeySegment)value;
				}
			}

			//---------------------------------------------------------------------------
			public KeySegment[] ToArray()
			{
				return (KeySegment[])InnerList.ToArray(typeof(KeySegment));
			}

			//---------------------------------------------------------------------------
			public int Add(KeySegment aSegmentInfoToAdd)
			{
				if (Contains(aSegmentInfoToAdd))
					return IndexOf(aSegmentInfoToAdd);

				return InnerList.Add(aSegmentInfoToAdd);
			}

			//---------------------------------------------------------------------------
			public void AddRange(KeySegmentsCollection aSegmentsCollectionToAdd)
			{
				if (aSegmentsCollectionToAdd == null || aSegmentsCollectionToAdd.Count == 0)
					return;

				foreach (KeySegment aSegmentInfoToAdd in aSegmentsCollectionToAdd)
					Add(aSegmentInfoToAdd);
			}

			//---------------------------------------------------------------------------
			public void Insert(int index, KeySegment aSegmentInfoToInsert)
			{
				if (index < 0 || index > InnerList.Count - 1)
					return;

				if (Contains(aSegmentInfoToInsert))
					return;

				InnerList.Insert(index, aSegmentInfoToInsert);
			}

			//---------------------------------------------------------------------------
			public void Insert(KeySegment beforeEnum, KeySegment aSegmentInfoToInsert)
			{
				if (beforeEnum == null)
					Add(aSegmentInfoToInsert);

				if (!Contains(beforeEnum))
					return;

				if (Contains(aSegmentInfoToInsert))
					return;

				Insert(IndexOf(beforeEnum), aSegmentInfoToInsert);
			}

			//---------------------------------------------------------------------------
			public void Remove(KeySegment aSegmentInfoToRemove)
			{
				if (!Contains(aSegmentInfoToRemove))
					return;

				InnerList.Remove(aSegmentInfoToRemove);
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
			public bool Contains(KeySegment aSegmentInfoToSearch)
			{
				foreach (object aItem in InnerList)
				{
					if (aItem == aSegmentInfoToSearch)
						return true;
				}
				return false;
			}

			//---------------------------------------------------------------------------
			public int IndexOf(KeySegment aSegmentInfoToSearch)
			{
				if (!Contains(aSegmentInfoToSearch))
					return -1;

				return InnerList.IndexOf(aSegmentInfoToSearch);
			}

			//---------------------------------------------------------------------
			public override bool Equals(object obj)
			{
				if (obj == null || !(obj is KeySegmentsCollection))
					return false;

				if (obj == this)
					return true;

				if (((KeySegmentsCollection)obj).Count != this.Count)
					return false;

				if (this.Count == 0)
					return true;

				for (int i = 0; i < this.Count; i++)
				{
					if (!KeySegment.Equals(this[i], ((KeySegmentsCollection)obj)[i]))
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
			public KeySegment GetKeySegmentInfoByName(string aColumnName)
			{
				if (this.Count == 0 || !Generics.IsValidTableColumnName(aColumnName))
					return null;

				foreach (KeySegment aSegmentInfo in InnerList)
				{
					if (String.Compare(aColumnName, aSegmentInfo.ColumnName) == 0)
						return aSegmentInfo;
				}
				return null;
			}
		}


		#region WizardForeignKeyInfo private data members

		protected string constraintName = String.Empty;
		protected string referencedTableNameSpace = String.Empty;
		protected KeySegmentsCollection segments = null;
		private bool onDeleteCascade = false;
		private bool onUpdateCascade = false;
		protected bool disposed = false;

		#endregion // WizardForeignKeyInfo private data members

		//---------------------------------------------------------------------
		public WizardForeignKeyInfo(string aTableNameSpace)
		{
			this.ReferencedTableNameSpace = aTableNameSpace;
		}

		//---------------------------------------------------------------------
		public WizardForeignKeyInfo(WizardForeignKeyInfo aForeignKeyInfo)
		{
			constraintName = (aForeignKeyInfo != null) ? aForeignKeyInfo.ConstraintName : String.Empty;

			this.ReferencedTableNameSpace = (aForeignKeyInfo != null) ? aForeignKeyInfo.ReferencedTableNameSpace : String.Empty;

			if (aForeignKeyInfo != null && aForeignKeyInfo.SegmentsCount > 0)
			{
				foreach (KeySegment aSegmentInfo in aForeignKeyInfo.Segments)
					AddKeySegment(new KeySegment(aSegmentInfo));
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
			if (obj == null || !(obj is WizardForeignKeyInfo))
				return false;

			if (obj == this)
				return true;

			return
				(
				String.Compare(constraintName, ((WizardForeignKeyInfo)obj).ConstraintName, true) == 0 &&
				String.Compare(referencedTableNameSpace, ((WizardForeignKeyInfo)obj).ReferencedTableNameSpace) == 0 &&
				HasSameSegments(((WizardForeignKeyInfo)obj).Segments)
				);
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region WizardForeignKeyInfo private methods


		#endregion // WizardForeignKeyInfo private methods

		#region WizardForeignKeyInfo public properties

		//---------------------------------------------------------------------------
		public string ConstraintName
		{
			get { return constraintName; }

			set
			{
				constraintName = (value != null) ? value.Trim() : String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		public virtual string ReferencedTableNameSpace
		{
			get { return referencedTableNameSpace; }

			set
			{
				if (value != null && value.Trim().Length > 0)
				{
					if (String.Compare(referencedTableNameSpace, value.Trim()) == 0)
						return;

					NameSpace tmpNameSpace = new NameSpace(value.Trim(), NameSpaceObjectType.Table);
					referencedTableNameSpace = (tmpNameSpace.IsValid()) ? value.Trim() : String.Empty;
				}
				else
					referencedTableNameSpace = String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		public virtual string ReferencedTableName
		{
			get
			{
				if (referencedTableNameSpace == null || referencedTableNameSpace.Trim().Length == 0)
					return String.Empty;

				NameSpace tmpNameSpace = new NameSpace(referencedTableNameSpace, NameSpaceObjectType.Table);

				return (tmpNameSpace.IsValid()) ? tmpNameSpace.Table : String.Empty;
			}

		}

		//---------------------------------------------------------------------------
		public KeySegmentsCollection Segments { get { return segments; } }

		//---------------------------------------------------------------------------
		public int SegmentsCount { get { return (segments != null) ? segments.Count : 0; } }

		//---------------------------------------------------------------------------
		public bool OnDeleteCascade { get { return onDeleteCascade; } set { onDeleteCascade = value; } }

		//---------------------------------------------------------------------------
		public bool OnUpdateCascade { get { return onUpdateCascade; } set { onUpdateCascade = value; } }


		#endregion // WizardForeignKeyInfo public properties

		#region WizardForeignKeyInfo public methods

		//---------------------------------------------------------------------------
		public KeySegment GetKeySegmentInfoByColumnName(string aColumnName)
		{
			if
				(
				segments == null ||
				segments.Count == 0 ||
				aColumnName == null ||
				aColumnName.Length == 0
				)
				return null;

			foreach (KeySegment aSegmentInfo in segments)
			{
				if (String.Compare(aColumnName, aSegmentInfo.ColumnName, true) == 0)
					return aSegmentInfo;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public KeySegment GetKeySegmentInfoByReferencedColumnName(string aReferencedColumnName)
		{
			if
				(
				segments == null ||
				segments.Count == 0 ||
				aReferencedColumnName == null ||
				aReferencedColumnName.Length == 0
				)
				return null;

			foreach (KeySegment aSegmentInfo in segments)
			{
				if (String.Compare(aReferencedColumnName, aSegmentInfo.ReferencedColumnName, true) == 0)
					return aSegmentInfo;
			}
			return null;
		}

		//---------------------------------------------------------------------------
		public int AddKeySegment(KeySegment aSegmentInfo)
		{
			if
				(
				aSegmentInfo == null ||
				aSegmentInfo.ColumnName == null ||
				aSegmentInfo.ColumnName.Length == 0 ||
				aSegmentInfo.ReferencedColumnName == null ||
				aSegmentInfo.ReferencedColumnName.Length == 0
				)
				return -1;

			KeySegment existingSegmentInfo = GetKeySegmentInfoByColumnName(aSegmentInfo.ColumnName);
			if (existingSegmentInfo != null)
			{
				existingSegmentInfo.ReferencedColumnName = aSegmentInfo.ReferencedColumnName;
				return segments.IndexOf(existingSegmentInfo);
			}

			if (segments == null)
				segments = new KeySegmentsCollection();

			return segments.Add(aSegmentInfo);
		}

		//---------------------------------------------------------------------------
		public void RemoveKeySegment(string aColumnName)
		{
			KeySegment segmentToRemove = GetKeySegmentInfoByColumnName(aColumnName);
			if (segmentToRemove == null)
				return;

			segments.Remove(segmentToRemove);

			if (segments.Count == 0)
				segments = null;
		}

		//---------------------------------------------------------------------------
		public bool HasSameSegments(KeySegmentsCollection segmentsToCompare)
		{
			if (segmentsToCompare == null || segmentsToCompare.Count == 0)
				return (this.SegmentsCount == 0);

			if (segments == null || this.SegmentsCount != segmentsToCompare.Count)
				return false;

			for (int i = 0; i < this.SegmentsCount; i++)
			{
				int j = 0;
				// Cerco nella seconda collection un segmento con lo stesso nome:
				for (j = 0; j < segmentsToCompare.Count; j++)
				{
					if (String.Compare(segments[i].ColumnName, segmentsToCompare[j].ColumnName) == 0)
					{
						if (String.Compare(segments[i].ReferencedColumnName, segmentsToCompare[j].ReferencedColumnName) != 0)
							return false; // il segmento non coincide
						break;
					}
				}
				if (j == segmentsToCompare.Count)
					return false; // non c'è un segmento con lo stesso nome
			}

			return true;
		}

		#endregion // WizardForeignKeyInfo public methods
	}

	#endregion // WizardForeignKeyInfo class

	#region WizardForeignKeyInfoCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for WizardForeignKeyInfoCollection.
	/// </summary>
	public class WizardForeignKeyInfoCollection : ReadOnlyCollectionBase, IList
	{
		//---------------------------------------------------------------------------
		public WizardForeignKeyInfoCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				if (value != null && !(value is WizardForeignKeyInfo))
					throw new NotSupportedException();

				this[index] = (WizardForeignKeyInfo)value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardForeignKeyInfo))
				throw new NotSupportedException();

			return this.Contains((WizardForeignKeyInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((WizardForeignKeyInfo)item);
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

			if (!(item is WizardForeignKeyInfo))
				throw new NotSupportedException();

			return this.IndexOf((WizardForeignKeyInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();

			if (!(item is WizardForeignKeyInfo))
				throw new NotSupportedException();

			Insert(index, (WizardForeignKeyInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is WizardForeignKeyInfo))
				throw new NotSupportedException();

			Remove((WizardForeignKeyInfo)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfo this[int index]
		{
			get { return (WizardForeignKeyInfo)InnerList[index]; }
			set
			{
				InnerList[index] = (WizardForeignKeyInfo)value;
			}
		}

		//---------------------------------------------------------------------------
		public WizardForeignKeyInfo[] ToArray()
		{
			return (WizardForeignKeyInfo[])InnerList.ToArray(typeof(WizardForeignKeyInfo));
		}

		//---------------------------------------------------------------------------
		public int Add(WizardForeignKeyInfo aForeignKeyToAdd)
		{
			if (Contains(aForeignKeyToAdd))
				return IndexOf(aForeignKeyToAdd);

			return InnerList.Add(aForeignKeyToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(WizardForeignKeyInfoCollection aForeignKeysCollectionToAdd)
		{
			if (aForeignKeysCollectionToAdd == null || aForeignKeysCollectionToAdd.Count == 0)
				return;

			foreach (WizardForeignKeyInfo aForeignKeyToAdd in aForeignKeysCollectionToAdd)
				Add(aForeignKeyToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, WizardForeignKeyInfo aForeignKeyToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aForeignKeyToInsert))
				return;

			InnerList.Insert(index, aForeignKeyToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(WizardForeignKeyInfo beforeColumn, WizardForeignKeyInfo aForeignKeyToInsert)
		{
			if (beforeColumn == null)
				Add(aForeignKeyToInsert);

			if (!Contains(beforeColumn))
				return;

			if (Contains(aForeignKeyToInsert))
				return;

			Insert(IndexOf(beforeColumn), aForeignKeyToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(WizardForeignKeyInfo aForeignKeyToRemove)
		{
			if (!Contains(aForeignKeyToRemove))
				return;

			InnerList.Remove(aForeignKeyToRemove);
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
		public bool Contains(WizardForeignKeyInfo aForeignKeyToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aForeignKeyToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(WizardForeignKeyInfo aForeignKeyToSearch)
		{
			if (!Contains(aForeignKeyToSearch))
				return -1;

			return InnerList.IndexOf(aForeignKeyToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WizardForeignKeyInfoCollection))
				return false;

			if (obj == this)
				return true;

			if (((WizardForeignKeyInfoCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!WizardForeignKeyInfo.Equals(this[i], ((WizardForeignKeyInfoCollection)obj)[i]))
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
