namespace Microarea.TaskBuilderNet.Model
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using Microarea.Framework.TBApplicationWrapper;
	using Microarea.TaskBuilderNet.Core.CoreTypes;
	using Microarea.TaskBuilderNet.Interfaces.Model;

	//=========================================================================
	/// <summary>
	/// SqlRecord
	/// </summary>
	public class SqlRecord : MSqlRecord
	{
		MDataDate _f_TBCreated;
		MDataDate _f_TBModified;
		MDataLng _f_TBCreatedID;
		MDataLng _f_TBModifiedID;

		IEnumerable<PropertyInfo> mDataObjPropertyInfos;

		//-------------------------------------------------------------------------
		/// <summary>
		/// TBCreated field
		/// </summary>
		public virtual MDataDate f_TBCreated
		{
			get
			{
				return _f_TBCreated;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// TBModified field
		/// </summary>
		public virtual MDataDate f_TBModified
		{
			get
			{
				return _f_TBModified;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// TBCreated id field
		/// </summary>
		public virtual MDataLng f_TBCreatedID
		{
			get
			{
				return _f_TBCreatedID;
			}
		}

		//-------------------------------------------------------------------------
		/// <summary>
		/// TBModified id field
		/// </summary>
		public virtual MDataLng f_TBModifiedID
		{
			get
			{
				return _f_TBModifiedID;
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Dispose
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				DisposeFields();
			}
		}

		//---------------------------------------------------------------------
		private void DisposeFields()
		{
			UnwireEvents();

			if (_f_TBCreated != null)
			{
				if (!_f_TBCreated.IsDisposed)
				{
					_f_TBCreated.Dispose();
				}
				_f_TBCreated = null;
			}
			if (_f_TBModified != null)
			{
				if (!_f_TBModified.IsDisposed)
				{
					_f_TBModified.Dispose();
				}
				_f_TBModified = null;
			}
			if (_f_TBCreatedID != null)
			{
				if (!_f_TBCreatedID.IsDisposed)
				{
					_f_TBCreatedID.Dispose();
				}
				_f_TBCreatedID = null;
			}
			if (_f_TBModifiedID != null)
			{
				if (!_f_TBModifiedID.IsDisposed)
				{
					_f_TBModifiedID.Dispose();
				}
				_f_TBModifiedID = null;
			}
		}

		//---------------------------------------------------------------------
		private void UnwireEvents()
		{
			foreach (PropertyInfo pi in GetType().GetProperties())
			{
				if (typeof(MDataObj).IsAssignableFrom(pi.PropertyType))
				{
					MDataObj dataObj = (MDataObj)pi.GetValue(this, null);
					UnsubscribeDataObjValueEvents(dataObj, pi.Name);
				}
			}
		}

		//---------------------------------------------------------------------
		private void UnsubscribeDataObjValueEvents(MDataObj dataObj, string name)
		{
			EventBag bag = new EventBag(this, name);
			if (dataObj != null)
			{
				dataObj.ValueChanging -= new EventHandler<EasyBuilderEventArgs>(bag.dataObj_ValueChanging);
				dataObj.ValueChanged -= new EventHandler<EasyBuilderEventArgs>(bag.dataObj_ValueChanged);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a new SqlRecord instance
		/// </summary>
		protected SqlRecord(IntPtr wrappedObject)
			: base(wrappedObject)
		{
			EnsureTypeDescriptionProvider();
			Initialize();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Creates a new SqlRecord instance
		/// </summary>
		protected SqlRecord()
			: base(string.Empty)
		{
			EnsureTypeDescriptionProvider();
			Initialize();
		}

		//---------------------------------------------------------------------
		private void EnsureTypeDescriptionProvider()
		{
			Type sqlRecordType = this.GetType();
			object[] attributes = sqlRecordType.GetCustomAttributes(false);
			bool found = false;
			foreach (var attribute in attributes)
			{
				TypeDescriptionProviderAttribute t = attribute as TypeDescriptionProviderAttribute;
				if (t != null)
				{
					found = true;
					break;
				}
			}

			Debug.Assert(found, sqlRecordType.FullName + " class without TypeDescriptionProviderAttribute");
		}

		//---------------------------------------------------------------------
		private void Initialize()
		{
			_f_TBCreated = new MDataDate(this.GetFieldPtr("TBCreated"));
			this.Add(_f_TBCreated);
			_f_TBModified = new MDataDate(this.GetFieldPtr("TBModified"));
			this.Add(_f_TBModified);
			_f_TBCreatedID = new MDataLng(this.GetFieldPtr("TBCreatedID"));
			this.Add(_f_TBCreatedID);
			_f_TBModifiedID = new MDataLng(this.GetFieldPtr("TBModifiedID"));
			this.Add(_f_TBModifiedID);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Add a component
		/// </summary>
		public override void Add(IComponent component)
		{
			base.Add(component);

			MDataObj dataObj = component as MDataObj;
			if (dataObj != null)
			{
				AttachDataObjValueChanged(dataObj);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Add a component
		/// </summary>
		public override void Add(IComponent component, string name)
		{
			base.Add(component, name);

			MDataObj dataObj = component as MDataObj;
			if (dataObj != null)
			{
				AttachDataObjValueChanged(dataObj);
			}
		}

		//---------------------------------------------------------------------
		private void AttachDataObjValueChanged(MDataObj dataObj)
		{
			if (this.mDataObjPropertyInfos == null)
			{
				this.mDataObjPropertyInfos = GetType().GetProperties()
					.Where(pi => typeof(MDataObj).IsAssignableFrom(pi.PropertyType));
			}

			MDataObj currentDataObj = null;
			foreach (PropertyInfo pi in this.mDataObjPropertyInfos)
			{
				currentDataObj = (MDataObj)pi.GetValue(this, null);
				if (Object.ReferenceEquals(currentDataObj, dataObj))
				{
					SubscribeDataObjValueEvents(dataObj, pi.Name);
					break;
				}
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Subscribe the record to the value changing and value changed event
		/// </summary>
		/// <param name="dataObj"></param>
		/// <param name="name"></param>
		public void SubscribeDataObjValueEvents(MDataObj dataObj, string name)
		{
			EventBag bag = new EventBag(this, name);
			if (dataObj != null)
			{
				dataObj.ValueChanging += new EventHandler<EasyBuilderEventArgs>(bag.dataObj_ValueChanging);
				dataObj.ValueChanged += new EventHandler<EasyBuilderEventArgs>(bag.dataObj_ValueChanged);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Add a local field
		/// </summary>
		protected TDataObj CreateLocalField<TDataObj>(
			string name,
			int length = 0
			) where TDataObj : MDataObj
		{
			DataType dataType = GetDataTypeFromPropertyType(typeof(TDataObj));

			int len = length;
			if (dataType.Equals(DataType.Integer))
				len = 4;
			else if (dataType.Equals(DataType.Long) || dataType.Equals(DataType.Enum) || dataType.Equals(DataType.ElapsedTime))
				len = 8;
			else if (dataType.Equals(DataType.Bool))
				len = 1;

			// wrapping di un nuovo local field
			IRecordField field = GetField(name);
			if (field == null)
			{
				MSqlRecordItem item = base.AddLocalField(name, dataType, len);
				MDataObj dataObj = item.DataObj as MDataObj;

				TDataObj returnDataObj = MDataObj.Create(dataObj.DataObjPtr) as TDataObj;
				AttachDataObjValueChanged(returnDataObj);
				return returnDataObj;
			}

			if (
					!(((IDataType)dataType).Equals(field.DataObjType))
					|| field.Length != len
					|| field.Type != DataModelEntityFieldType.Variable
				)
				throw new Exception("Duplicate AddLocalField");

			TDataObj theDataObj = field.DataObj as TDataObj;
			if (theDataObj != null)
				theDataObj.ParentComponent = this;

			// ritorno di quello già esistente
			return theDataObj;
		}

		//---------------------------------------------------------------------
		private DataType GetDataTypeFromPropertyType(Type type)
		{
			if (type == typeof(MDataInt))
			{
				return DataType.Integer;
			}
			if (type == typeof(MDataLng))
			{
				return DataType.Long;
			}
			if (type == typeof(MDataQty))
			{
				return DataType.Quantity;
			}
			if (type == typeof(MDataMon))
			{
				return DataType.Money;
			}
			if (type == typeof(MDataDbl))
			{
				return DataType.Double;
			}
			if (type == typeof(MDataPerc))
			{
				return DataType.Percent;
			}
			if (type == typeof(MDataDate))
			{
				return DataType.Date;
			}
			if (type == typeof(MDataBool))
			{
				return DataType.Bool;
			}
			if (type == typeof(MDataEnum))
			{
				return DataType.Enum;
			}
			if (type == typeof(MDataGuid))
			{
				return DataType.Guid;
			}
			if (type == typeof(MDataStr))
			{
				return DataType.String;
			}
			if (type == typeof(MDataText))
			{
				return DataType.Text;
			}
			throw new ArgumentException("Unrecognized dataobj type: " + type.FullName);
		}
	}


	internal class EventBag
	{
		private string propertyName;
		private SqlRecord sqlRecord;

		public EventBag(SqlRecord sqlRecord, string propertyName)
		{
			this.sqlRecord = sqlRecord;
			this.propertyName = propertyName;
		}

		public void dataObj_ValueChanging(object sender, EasyBuilderEventArgs e)
		{
			sqlRecord.FirePropertyChanging(propertyName);
		}

		public void dataObj_ValueChanged(object sender, EasyBuilderEventArgs e)
		{
			sqlRecord.FirePropertyChanged(propertyName);
		}

	}
}
