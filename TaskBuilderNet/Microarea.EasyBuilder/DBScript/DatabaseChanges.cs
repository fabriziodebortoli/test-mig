using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Microarea.EasyBuilder.Packager;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder.DBScript
{
	//================================================================================
	/// <remarks/>
	[Serializable]
	[XmlInclude(typeof(AddedField))]
	[XmlInclude(typeof(AddedRecord))]
	public class DBScriptInfo
	{
		private const string fileName = "EBDBInfo.xml";

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public static DBScriptInfo Load()
		{
			return Load(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal static DBScriptInfo Load(IEasyBuilderApp easyBuilderApp)
		{
			string basePath = easyBuilderApp.BasePath;
			if (string.IsNullOrWhiteSpace(basePath))
				return null;
			string path = Path.Combine(basePath, fileName);
			if (!File.Exists(path))
				return null;
			try
			{
				using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					XmlSerializer x = new XmlSerializer(typeof(DBScriptInfo), new Type[] { typeof(ImportedRecord), typeof(ImportedField) });
					DBScriptInfo info = (DBScriptInfo)x.Deserialize(stream);

					//aggiusto la lista delle chiavi primarie, che non viene serializzata
					//aggiusto anche la creation release in base a easyBuilderApp
					int creationRelease = DatabaseChanges.GetDatabaseRelease(easyBuilderApp); 

					foreach (AddedRecord rec in info.NewTables)
					{
						rec.CreationRelease = creationRelease;
						foreach (AddedField field in rec.Fields)
						{
							field.CreationRelease = creationRelease;
							if (field.IsSegmentKey)
								rec.PrimaryKeyFields.Add(field);
						}
					}

					return info;
				}
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return null;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void Save()
		{
			Save(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		internal void Save(IEasyBuilderApp easyBuilderApp)
		{
			string path = Path.Combine(easyBuilderApp.BasePath, fileName);
			try
			{
				using (Stream stream = File.Open(path, FileMode.Create))
				{
					XmlSerializer x = new XmlSerializer(typeof(DBScriptInfo), new Type[] { typeof(ImportedRecord), typeof(ImportedField) });
					
					x.Serialize(stream, this);
				}
			}
			catch (UnauthorizedAccessException uaExc)
			{
				throw new IOException(String.Format("File {0} is read-only", path), uaExc);
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
			}
		}
		
		List<AddedRecord> newTables = new List<AddedRecord>();
		List<AddedRecord> newAddonFields = new List<AddedRecord>();

		/// <summary>
		/// Tutte le tabelle aggiunte dalla mia customizzazione nella corrente release
		/// </summary>
		//--------------------------------------------------------------------------------
		public List<AddedRecord> NewTables
		{
			get { return newTables; }
		}
		
		/// <summary>
		/// Tutte i campi, suddivisi per tabella, aggiunti dalla mia customizzazione nella corrente release
		/// </summary>
		//--------------------------------------------------------------------------------
		public List<AddedRecord> NewAddOnFields
		{
			get
			{
				return newAddonFields;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public int DatabaseRelease { get; set; }

		//--------------------------------------------------------------------------------
		internal void Add(NameSpace tableNamespace, IRecordField addedField)
		{
			AddedRecord list = null;
			foreach (AddedRecord rec in newAddonFields)
				if (rec.NameSpace == tableNamespace)
				{
					list = rec;
					break;
				}
			if (list == null)
			{
				list = new AddedRecord(tableNamespace.Leaf);
				list.NameSpace = tableNamespace;
				newAddonFields.Add(list);
			}
			list.Fields.Add(addedField);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public bool OnlyMyItems { get; set; }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public bool IsEmpty { get { return newTables.Count == 0 && newAddonFields.Count == 0; } }
	}
	//================================================================================
	class DatabaseChanges
	{
		public static int GetDatabaseRelease(IEasyBuilderApp easyBuilderApp)
		{
			if (easyBuilderApp == null)
				return 0;

			if (easyBuilderApp.ModuleInfo == null)
				return 0;

			return easyBuilderApp.ModuleInfo.CurrentDBRelease + 1;
		}
		
		public List<IRecord> NewTables = new List<IRecord>();
		private Dictionary<string, List<IRecordField>> newAddonFields = new Dictionary<string, List<IRecordField>>();

		public bool IsEmpty { get { return newAddonFields.Count == 0 && NewTables.Count == 0; } }

		//--------------------------------------------------------------------------------
		public Dictionary<string, List<IRecordField>> NewAddonFields
		{
			get { return newAddonFields; }
		}
		//--------------------------------------------------------------------------------
		internal void AddAddonField(NameSpace tableNameSpace, IRecordField addedField)
		{
			List<IRecordField> fields;
			if (!newAddonFields.TryGetValue(tableNameSpace, out fields))
			{
				fields = new List<IRecordField>();
				newAddonFields[tableNameSpace] = fields;
			}
			fields.Add(addedField);
		}

		//--------------------------------------------------------------------------------
		internal DatabaseChangesCurrentRelease GetCurrentReleaseChanges(IEasyBuilderApp easyBuilderApp)
		{
			DatabaseChangesCurrentRelease currentChanges = new DatabaseChangesCurrentRelease();
			int databaseRelease = DatabaseChanges.GetDatabaseRelease(easyBuilderApp);
			//clono le informazioni delle tabelle prima di rimuovere le vecchie dal catalog per evitare crash
			foreach (IRecord record in NewTables)
			{
				if (HasReleaseChange(record, easyBuilderApp))
					currentChanges.AddedTables.Add(
						record is ImportedRecord
						? new ImportedRecord(record)
						: new AddedRecord(record, databaseRelease, true)
						);
			}

			//clono le informazioni dei campi prima di rimuovere le vecchie dal catalog per evitare crash
			foreach (string tableNamespace in NewAddonFields.Keys)
			{
				AddedRecord rec = null;//un solo record clonato per tutti i campi della stessa tabella
				foreach (IRecordField field in NewAddonFields[tableNamespace])
				{
					if (rec == null)
						rec = new AddedRecord(field.Record, databaseRelease, true);
					if (field.CreationRelease == databaseRelease)
					{
						AddedField f = new AddedField(rec, field);
						currentChanges.AddedFields.Add(f);
					}
				}
			}
			return currentChanges;
		}
		
		//--------------------------------------------------------------------------------
		private bool HasReleaseChange(IRecord record, IEasyBuilderApp easyBuilderApp)
		{
			int databaseRelease = DatabaseChanges.GetDatabaseRelease(easyBuilderApp);
			if (record.CreationRelease == databaseRelease)
				return true;
			foreach (IRecordField f in record.Fields)
				if (f.CreationRelease == databaseRelease)
					return true;
			return false;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	//================================================================================
	public class DatabaseChangesCurrentRelease
	{
		//-----------------------------------------------------------------------------
		internal List<AddedRecord> AddedTables = new List<AddedRecord>();
		//-----------------------------------------------------------------------------
		internal List<AddedField> AddedFields = new List<AddedField>();

		private static volatile bool catalogUpdated;

		//-----------------------------------------------------------------------------
		internal bool IsEmpty { get { return AddedTables.Count == 0 && AddedFields.Count == 0; } }

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public static void UpdateCatalogIfNeeded()
		{
			if (catalogUpdated)
				return;

			lock (typeof(DatabaseChangesCurrentRelease))
			{
				if (catalogUpdated)//lo ritesto perché prima del lock potrebbe essere cambiato
					return;

				DatabaseChangesCurrentRelease changes = new DatabaseChangesCurrentRelease();
				foreach (IEasyBuilderApp item in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
				{
					changes.ReadAndApplyToObjectCatalog(item);
				}
				catalogUpdated = true;
			}
		}

		//-----------------------------------------------------------------------------
		internal void ReadAndApplyToObjectCatalog(IEasyBuilderApp ownerApp)
		{
			if (ownerApp == null)
                return;

			DBScriptInfo info = DBScriptInfo.Load(ownerApp);
			if (info == null ||
                info.IsEmpty ||
				info.DatabaseRelease <= ownerApp.ModuleInfo.CurrentDBRelease)
				return;

			foreach (AddedRecord record in info.NewTables)
				AddedTables.Add(record);

			foreach (AddedRecord dummyRecord in info.NewAddOnFields)
				foreach (AddedField field in dummyRecord.Fields)
				{
					field.Record = dummyRecord;//aggiusto il parent
					AddedFields.Add(field);
				}

			MSqlCatalog catalog = new MSqlCatalog();
			ApplyChangesToObjectModel(catalog, true, ownerApp);
		}

		//-----------------------------------------------------------------------------
		internal void ApplyChangesToObjectModel(MSqlCatalog catalog, bool isVirtual)
		{
			ApplyChangesToObjectModel(catalog, isVirtual, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
		}

		//-----------------------------------------------------------------------------
		internal void ApplyChangesToObjectModel(MSqlCatalog catalog, bool isVirtual, IEasyBuilderApp easyBuilderApp)
		{
			//devo applicare anche se currentChanges è vuoto, infatti devo almeno pulire il catalog
			//con la RemoveObjectsOfRelease
			INameSpace modNs = easyBuilderApp.ModuleInfo.NameSpace;
			int databaseRelease = DatabaseChanges.GetDatabaseRelease(easyBuilderApp);
			//adesso pulisco il catalog e le strutture c++
			catalog.RemoveObjectsOfRelease(databaseRelease, modNs);
			 
			//quindi aggiungo i dati che mi sono messo da parte
			foreach (IRecord record in AddedTables)
			{
				//devo controllare la release della tabella: potrebbe essere presente solo perché contiene campi
				//agiunti nella corrente release (ma lei non deve essere aggiunta perché già presente)
				if (record.CreationRelease == databaseRelease)
                {
                    AddedRecord addedRec = record as AddedRecord;
                    catalog.AddTable(record, isVirtual, addedRec == null ? false : addedRec.IsMasterTable);
                }

				foreach (IRecordField field in record.Fields)
					catalog.AddField(record, field, modNs, isVirtual);
			}
			foreach (IRecordField field in AddedFields)
				catalog.AddField(field.Record, field, modNs, isVirtual);

		}
	}
}
