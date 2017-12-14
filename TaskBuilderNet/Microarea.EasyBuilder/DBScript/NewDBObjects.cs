using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.UI;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Localization;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// 
	/// </summary>
	//================================================================================
	public class AddedRecord : EasyBuilderComponent, IRecord, INotifyPropertyChanged
	{
		/// <remarks/>
		protected string name;
		/// <remarks/>
		protected string tableNamespace;
		/// <remarks/>
		protected bool inCatalog = false;
        /// <remarks/>
        protected int creationRelease = DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);

        /// <remarks/>
        protected bool isMasterTable = false;


        List<AddedField> fields = new List<AddedField>();
		List<AddedField> keyFields = new List<AddedField>();
		internal event EventHandler<MyPropertyChangingArgs> PropertyChanging;

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public bool InCatalog { get { return inCatalog; } }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        [ReadOnly(true)]
        public bool IsMasterTable { get { return isMasterTable; } set { isMasterTable = value; } }


        //-----------------------------------------------------------------------------
        /// <remarks/>
        public AddedRecord()
		{
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public AddedRecord(string name)
		{
			this.name = name;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public AddedRecord(IRecord record, int forRelease, bool excludeTBFields)
		{
			this.name = record.Name;
			//se copio un MSqlrecord, significa che sono presente nel catalog
			this.inCatalog = record is MSqlRecord || (record is AddedRecord && ((AddedRecord)record).InCatalog);
				
			this.tableNamespace = (NameSpace)record.NameSpace;
			this.creationRelease = record.CreationRelease;
            if (record is AddedRecord)
                this.isMasterTable = ((AddedRecord)record).IsMasterTable;
            if (record is MSqlRecord)
                this.isMasterTable = ((MSqlRecord)record).IsMasterTable;

            foreach (IRecordField f in record.Fields)
			{
				if (excludeTBFields && IsTBField(f))
					continue;
				if (forRelease > 0 && f.CreationRelease != forRelease)
					continue;
				AddedField af = new AddedField(this, f);
				Fields.Add(af);
				if (af.IsSegmentKey)
					PrimaryKeyFields.Add(af);
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected bool IsTBField(IRecordField field)
		{
			return
				field.Name.CompareNoCase(DatabaseLayerConsts.TBCreatedColNameForSql) ||
				field.Name.CompareNoCase(DatabaseLayerConsts.TBModifiedColNameForSql) ||
				field.Name.CompareNoCase(DatabaseLayerConsts.TBCreatedIDColNameForSql) ||
				field.Name.CompareNoCase(DatabaseLayerConsts.TBModifiedIDColNameForSql);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[XmlIgnore]
		[ReadOnly(true)]
		public int CreationRelease { get { return creationRelease; } set { creationRelease = value; } }

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		TaskBuilderNet.Interfaces.INameSpace IRecord.NameSpace
		{
			get { return string.IsNullOrEmpty(tableNamespace) ? null : new NameSpace(tableNamespace); }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[XmlIgnore]
		public bool IsRegistered { get { return true; } }

		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		public string NameSpace
		{
			get { return tableNamespace; }
			set { tableNamespace = value; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		[XmlIgnore]
		public DataModelEntityType RecordType
		{
			get { return DataModelEntityType.Table; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		public IList Fields
		{
			get { return fields; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		[XmlIgnore]
		public bool IsValid
		{
			get { return true; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public override string Name
		{
			get { return name; }
			set
			{
				OnPropertyChanging("Name", value);
				name = value;
				OnPropertyChanged("Name");

				if (tableNamespace != null)
				{
					NameSpace ns = new NameSpace(tableNamespace);
					ns.Leaf = name;
					NameSpace = ns.ToString();
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		[XmlIgnore]
		public IList PrimaryKeyFields
		{
			get { return keyFields; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public IRecordField Add(string name, DataModelEntityFieldType type, IDataType dataType, string localizableName, int length)
		{
			Debug.Assert(false);
			throw new NotImplementedException();
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public IRecordField GetField(string name)
		{
			foreach (IRecordField field in fields)
			{
				if (field.Name == name)
					return field;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public IRecordField GetField(IDataObj dataObj)
		{
			Debug.Assert(false);
			throw new NotImplementedException();

		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public IDataObj GetData(string fieldName)
		{
			Debug.Assert(false);
			throw new NotImplementedException();
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public object GetValue(string fieldName)
		{
			Debug.Assert(false);
			throw new NotImplementedException();
		}

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		internal TreeNode TreeNode { get; set; }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public event PropertyChangedEventHandler PropertyChanged;
		
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected virtual void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		[DebuggerStepThrough]
		protected virtual void OnPropertyChanging(string name, object newValue)
		{
			if (PropertyChanging != null)
				PropertyChanging(this, new MyPropertyChangingArgs(name, newValue));
		}


		#region IContainer Members

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component, string name)
		{
			Add(component);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void Add(IComponent component)
		{
			fields.Add((AddedField)component);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[XmlIgnore]
		[Browsable(false)]
		public ComponentCollection Components
		{
			get { return new ComponentCollection(fields.ToArray()); }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void Remove(IComponent component)
		{
			fields.Remove((AddedField)component);
		}

		#endregion
	}

	//-----------------------------------------------------------------------------
	/// <remarks/>
	public class AddedField : EasyBuilderComponent, IRecordField, INotifyPropertyChanged, IDisposable
	{
		string name;
		DataType dataObjType = DataType.Integer;
		int lenght = 0;
		bool isSegmentKey = false;
		bool inCatalog = false;
		/// <remarks/>
		protected IRecord record;
		/// <remarks/>
		protected MDataObj dataObj;
        /// <remarks/>
        protected int creationRelease = DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public event PropertyChangedEventHandler PropertyChanged;
		
		internal event EventHandler<MyPropertyChangingArgs> PropertyChanging;

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public bool InCatalog { get { return inCatalog; } }
		
		//serve per la serializzazione
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public AddedField()
		{
		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public AddedField(IRecord record, string name)
		{
			this.record = record;
			this.name = name;
			AssignDefaultValue();
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public AddedField(IRecord record, IRecordField field)
		{
			this.record = record;
			this.inCatalog = field is MSqlRecordItem || (field is AddedField && ((AddedField)field).InCatalog);
			this.name = field.Name;
			this.dataObjType = (DataType)field.DataObjType;
			this.lenght = field.Length;
			this.isSegmentKey = field.IsSegmentKey;
			this.dataObj = dataObj = MDataObj.Create(dataObjType);
			this.dataObj.StringValue = field.DefaultValue;
			this.creationRelease = field.CreationRelease;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public override void Dispose()
		{
			base.Dispose();
			if (dataObj != null)
				dataObj.Dispose();
		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		[XmlIgnore]
		[ReadOnly(true)]
		public int CreationRelease { get { return creationRelease; } set { creationRelease = value; } }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[XmlIgnore]
		[Browsable(false)]
		public IRecord Record
		{
			get { return record; }
			set { record = value; }
		}
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		[Browsable(false)]
		internal TreeNode TreeNode { get; set; }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public override string Name
		{
			get { return name; }
			set { OnPropertyChanging("Name", value); name = value; OnPropertyChanged("Name"); }
		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		[XmlIgnore]
		public string QualifiedName
		{
			get { return record.Name + "." + name; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[LocalizedCategory("GeneralCategory", typeof(EBCategories)), Editor(typeof(FieldUITypeEditor), typeof(UITypeEditor))]
		public DataType DataObjType
		{
			get { return dataObjType; }
			set
			{
				OnPropertyChanging("DataObjType", value);
				dataObjType = value;
				OnPropertyChanged("DataObjType");
				AssignDefaultValue();
			}
		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		public string DefaultValue
		{
			get { return dataObj.StringValue; }
			set { OnPropertyChanging("DefaultValue", value); dataObj.StringValue = value; OnPropertyChanged("DefaultValue"); }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public virtual int Length
		{
			get { return lenght; }
			[DebuggerStepThrough]
			set
			{

				if (dataObjType.ToString() == DataType.DataTypeStrings.String)
				{
					if (value <= 0)
						throw new ArgumentException("Length");
				}
				else
					return;
				OnPropertyChanging("Length", value);
				lenght = value;
				OnPropertyChanged("Length");
			}
		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		[LocalizedCategory("GeneralCategory", typeof(EBCategories))]
		public virtual bool IsSegmentKey
		{
			get { return isSegmentKey; }
			set { OnPropertyChanging("IsSegmentKey", value); isSegmentKey = value; OnPropertyChanged("IsSegmentKey"); }
		}

		//--------------------------------------------------------------------------------
		private void AssignDefaultValue()
		{
			if (dataObj != null)
				dataObj.Dispose();

			dataObj = MDataObj.Create(dataObjType);
			switch (dataObjType.ToString())
			{
				case DataType.DataTypeStrings.String:
					lenght = 50;
					break;
				case DataType.DataTypeStrings.Bool:
					lenght = 1;
					break;
			}
		}

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		IDataType IRecordField.DataObjType
		{
			get { return dataObjType; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[XmlIgnore,
		Editor(typeof(DataObjUITypeEditor), typeof(UITypeEditor)),
		LocalizedCategory("GeneralCategory", typeof(EBCategories)),
		DisplayName("DefaultValue")]
		public IDataObj DataObj
		{
			get { return dataObj; }
			set { dataObj = (MDataObj)value; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		[XmlIgnore]
		public DataModelEntityFieldType Type
		{
			get { return DataModelEntityFieldType.Column; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		[Browsable(false)]
		[XmlIgnore]
		public object Value
		{
			get
			{
				return DataObj.Value;
			}
			set
			{
				DataObj.Value = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public bool IsCompatibleWith(IRecordField field)
		{
			return false;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected virtual void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		[DebuggerStepThrough]
		protected virtual void OnPropertyChanging(string name, object newValue)
		{
			if (PropertyChanging != null)
				PropertyChanging(this, new MyPropertyChangingArgs(name, newValue));
		}

	}
	/// <remarks/>
	[Serializable]
	public class ImportedRecord : AddedRecord 
	{
		MSqlRecord unregisteredRecord;

		/// <remarks/>
		public MSqlRecord UnregisteredRecord
		{
			get { return unregisteredRecord; }
		}
		/// <remarks/>
		public ImportedRecord()
		{ 
		}
		/// <remarks/>
		public ImportedRecord(IRecord record)
		{
			this.name = record.Name;
			//se copio un MSqlrecord, significa che sono presente nel catalog
			this.inCatalog = true;
            AddedRecord addedRecord = record as AddedRecord;
            if (addedRecord != null)
                this.IsMasterTable = addedRecord.IsMasterTable;

            this.tableNamespace = (NameSpace)record.NameSpace;
			this.unregisteredRecord = record as MSqlRecord;
			foreach (IRecordField f in record.Fields)
			{
				if (IsTBField(f))
					continue;
				ImportedField af = new ImportedField(this, f);
				Fields.Add(af);
				if (af.IsSegmentKey)
					PrimaryKeyFields.Add(af);
			}
		}

		/// <remarks/>
		[ReadOnly(true)]
		public override string Name
		{
			get { return base.Name; }
			set { base.Name = value; }
		}
	}

	/// <remarks/>
	[Serializable]
	public class ImportedField : AddedField
	{
		MSqlRecord unregisteredRecord;

		/// <remarks/>
		public ImportedField()
		{ 
		}

		/// <remarks/>
		public ImportedField(ImportedRecord importedRecord, IRecordField f)
			: base (importedRecord, f)
		{
			creationRelease = DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
			unregisteredRecord = importedRecord.UnregisteredRecord;
		}
		/// <remarks/>
		protected override void OnPropertyChanging(string name, object newValue)
		{
			if (unregisteredRecord != null)
			{
				if (name == "DataObjType")
				{
					List<DataType> types = new List<DataType>();
					unregisteredRecord.GetCompatibleDataTypes(Name, types);
					bool found = false;
					foreach (DataType dt in types)
						if (dt.Equals(newValue))
						{
							found = true;
							break;
						}
					if (!found)
						throw new ApplicationException(Microarea.EasyBuilder.Properties.Resources.DataObjTypeNotCompatibleWithExistingDatabaseColumn);

				}
			}
			base.OnPropertyChanging(name, newValue);
		}
		/// <remarks/>
		[ReadOnly(true)]
		public override string Name
		{
			get { return base.Name; }
			set { base.Name = value; }
		}
		/// <remarks/>
		[ReadOnly(true)]
		public override int Length
		{
			get { return base.Length; }
			set { base.Length = value; }
		}
		/// <remarks/>
		[ReadOnly(true)]
		public override bool IsSegmentKey
		{
			get { return base.IsSegmentKey; }
			set { base.IsSegmentKey = value; }
		}
	}
}
