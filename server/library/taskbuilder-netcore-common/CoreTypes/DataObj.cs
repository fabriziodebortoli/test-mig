using System;
using System.ComponentModel;
using System.Diagnostics;

using TaskBuilderNetCore.Interfaces.Model;

namespace Microarea.Common.CoreTypes
{
	//=========================================================================
	[Serializable]
	public abstract class DataObj : IDataObj, IComponent
	{
		public static readonly IDataObj Null = new NullDataObj();

		private const string dataStatusTag = "DataStati";
		private DataStati dataStatus;
        private bool modified;

		//---------------------------------------------------------------------
		public object Value
		{
			get { return GetValue(); }
			set { SetValue(value); }
		}

        //---------------------------------------------------------------------
        public bool Modified
        {
            get { return this.modified; }
            set { this.modified = value; }
        }

		protected abstract object GetValue();
		protected abstract void SetValue(object value);


		//---------------------------------------------------------------------
		public DataStati DataStatus
		{
			get { return dataStatus; }
		}

		//---------------------------------------------------------------------
		protected void SetStatus(bool set, DataStati statusFlag)
		{
			dataStatus = set ? (dataStatus | statusFlag) : (dataStatus & ~statusFlag);
		}

		//---------------------------------------------------------------------
		protected DataObj()
		{
			//this.dataStatus = 0;inizializzato a zero dal runtime.
		}

		//---------------------------------------------------------------------
		protected DataObj(DataStati dataStatus)
		{
			this.dataStatus = dataStatus;
		}

		//---------------------------------------------------------------------
		public bool IsValid
		{
			get { return ((dataStatus & DataStati.Valid) == DataStati.Valid); }
			set
			{
				OnPropertyChanging("IsValid");
				SetValid(value);
				OnPropertyChanged("IsValid");
			}
		}

		//---------------------------------------------------------------------
		public bool IsModified
		{
			get { return ((dataStatus & DataStati.Modified) == DataStati.Modified); }
			set
			{
				OnPropertyChanging("IsModified");
				SetModified(value);
				OnPropertyChanged("IsModified");
			}
		}

		//---------------------------------------------------------------------
		public bool IsUppercase
		{
			get { return ((dataStatus & DataStati.Uppercase) == DataStati.Uppercase); }
			set
			{
				OnPropertyChanging("IsUppercase");
				SetUppercase(value);
				OnPropertyChanged("IsUppercase");
			}
		}

		//---------------------------------------------------------------------
		public bool IsDBCaseCompliant
		{
			get { return (dataStatus & DataStati.DBCaseCompliant) == DataStati.DBCaseCompliant; }
			set
			{
				OnPropertyChanging("IsDBCaseCompliant");
				SetDBCaseCompliant(value);
				OnPropertyChanged("IsDBCaseCompliant");
			}
		}

		//---------------------------------------------------------------------
		public bool IsValueChanged
		{
			get { return (dataStatus & DataStati.ValueChanged) == DataStati.ValueChanged; }
			set
			{
				OnPropertyChanging("IsValueChanged");
				SetValueChanged(value);
				OnPropertyChanged("IsValueChanged");
			}
		}

		//---------------------------------------------------------------------
		public bool IsDirty
		{
			get { return ((dataStatus & DataStati.Dirty) == DataStati.Dirty); }
			set
			{
				OnPropertyChanging("IsDirty");
				SetDirty(value);
				OnPropertyChanged("IsDirty");
			}
		}

		//---------------------------------------------------------------------
		public bool IsFindable
		{
			get { return ((dataStatus & DataStati.Findable) == DataStati.Findable); }
			set
			{
				OnPropertyChanging("IsFindable");
				SetFindable(value);
				OnPropertyChanged("IsFindable");
			}
		}

		//---------------------------------------------------------------------
		public bool IsFullDate
		{
			get { return ((dataStatus & DataStati.FullDate) == DataStati.FullDate); }
			set
			{
				OnPropertyChanging("IsFullDate");
				SetFullDate(value);
				OnPropertyChanged("IsFullDate");
			}
		}

		//---------------------------------------------------------------------
		public bool IsATime
		{
			get { return ((dataStatus & DataStati.Time) == DataStati.Time); }
			set
			{
				OnPropertyChanging("IsATime");
				SetAsTime(value);
				OnPropertyChanged("IsATime");
			}
		}

		//---------------------------------------------------------------------
		public bool IsUpdateView
		{
			get { return ((dataStatus & DataStati.UpdateView) == DataStati.UpdateView); }
			set
			{
				OnPropertyChanging("IsUpdateView");
				SetUpdateView(value);
				OnPropertyChanged("IsUpdateView");
			}
		}

		//---------------------------------------------------------------------
		public bool IsReadOnly
		{
			get
			{
				return
					(dataStatus & DataStati.ReadOnly) == DataStati.ReadOnly ||
					(dataStatus & DataStati.OslReadOnly) == DataStati.OslReadOnly ||
					(dataStatus & DataStati.AlwaysReadOnly) == DataStati.AlwaysReadOnly;
			}
			set
			{
				OnPropertyChanging("IsReadOnly");
				SetReadOnly(value);
				OnPropertyChanged("IsReadOnly");
			}
		}

		//---------------------------------------------------------------------
		public bool IsHide
		{
			get
			{
				return
					(dataStatus & DataStati.Hide) == DataStati.Hide ||
					(dataStatus & DataStati.OslHide) == DataStati.OslHide;
			}
			set
			{
				OnPropertyChanging("IsHide");
				SetHide(value);
				OnPropertyChanged("IsHide");
			}
		}

		//---------------------------------------------------------------------
		public bool IsStateReadOnly
		{
			get { return (dataStatus & DataStati.ReadOnly) == DataStati.ReadOnly; }
			set
			{
				OnPropertyChanging("IsStateReadOnly");
				SetReadOnly(value);
				OnPropertyChanged("IsStateReadOnly");
			}
		}

		//---------------------------------------------------------------------
		public bool IsStateHide
		{
			get { return ((dataStatus & DataStati.Hide) == DataStati.Hide); }
			set
			{
				OnPropertyChanging("IsStateHide");
				SetHide(value);
				OnPropertyChanged("IsStateHide");
			}
		}

		//---------------------------------------------------------------------
		public bool IsOslReadOnly
		{
			get { return ((dataStatus & DataStati.OslReadOnly) == DataStati.OslReadOnly); }
			set
			{
				OnPropertyChanging("IsOslReadOnly");
				SetOslReadOnly(value);
				OnPropertyChanged("IsOslReadOnly");
			}
		}

		//---------------------------------------------------------------------
		public bool IsOslHide
		{
			get { return ((dataStatus & DataStati.OslHide) == DataStati.OslHide); }
			set
			{
				OnPropertyChanging("IsOslHide");
				SetOslHide(value);
				OnPropertyChanged("IsOslHide");
			}
		}

		//---------------------------------------------------------------------
		public bool IsAlwaysReadOnly
		{
			get { return ((dataStatus & DataStati.AlwaysReadOnly) == DataStati.AlwaysReadOnly); }
			set
			{
				OnPropertyChanging("IsAlwaysReadOnly");
				SetAlwaysReadOnly(value);
				OnPropertyChanged("IsAlwaysReadOnly");
			}
		}

		//---------------------------------------------------------------------
		public bool IsValueLocked
		{
			get { return ((dataStatus & DataStati.ValueLocked) == DataStati.ValueLocked); }
			set
			{
				OnPropertyChanging("IsValueLocked");
				SetValueLocked(value);
				OnPropertyChanged("IsValueLocked");
			}
		}

		//---------------------------------------------------------------------
		public bool IsCollateCultureSensitive
		{
			get { return ((dataStatus & DataStati.CollateCultureSensitive) == DataStati.CollateCultureSensitive); }
			set
			{
				OnPropertyChanging("IsCollateCultureSensitive");
				SetCollateCultureSensitive(value);
				OnPropertyChanged("IsCollateCultureSensitive");
			}
		}

		//---------------------------------------------------------------------
		public void SetValid(bool valid)
		{
			SetStatus(valid, DataStati.Valid);
		}

		//---------------------------------------------------------------------
		public void SetModified(bool modified)
		{
			SetStatus(modified, DataStati.Modified);
		}

		//---------------------------------------------------------------------
		public void SetUppercase(bool uppercase)
		{
			SetStatus(uppercase, DataStati.Uppercase);
			Debug.Assert(DataType.Equals(CoreTypes.DataType.String));
		}

		//---------------------------------------------------------------------
		public void SetDBCaseCompliant(bool dbCaseCompliant)
		{
			SetStatus(dbCaseCompliant, DataStati.DBCaseCompliant);
			Debug.Assert(DataType.Equals(CoreTypes.DataType.String));
		}


		//---------------------------------------------------------------------
		public void SetValueChanged(bool changed)
		{
			SetStatus(changed, DataStati.ValueChanged);
		}

		//---------------------------------------------------------------------
		public void SetDirty(bool dirty)
		{
			SetStatus(dirty, DataStati.Dirty);
		}

		//---------------------------------------------------------------------
		public void SetFullDate(bool fullDate)
		{
			SetStatus(fullDate, DataStati.FullDate);
			Debug.Assert(DataType.Equals(CoreTypes.DataType.Date));
		}

		//---------------------------------------------------------------------
		public void SetUpdateView(bool changed)
		{
			SetStatus(changed, DataStati.UpdateView);
		}

		//---------------------------------------------------------------------
		public void SetHide(bool hide)
		{
			SetStatus(hide, DataStati.Hide);
		}

		//---------------------------------------------------------------------
		public void SetOslHide(bool hide)
		{
			SetStatus(hide, DataStati.OslHide);
		}

		//---------------------------------------------------------------------
		public void SetAlwaysReadOnly(bool readOnly)
		{
			SetStatus(readOnly, DataStati.AlwaysReadOnly);

			if (readOnly)
				SetStatus(false, DataStati.Findable);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public void SetValueLocked(bool valueLocked)
		{
			SetStatus(valueLocked, DataStati.ValueLocked);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public void SetCollateCultureSensitive(bool sensitive)
		{
			SetStatus(sensitive, DataStati.CollateCultureSensitive);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public void SetFindable(bool findable)
		{
			SetStatus(findable, DataStati.Findable);

			if (findable)
			{
				SetStatus(false, DataStati.ReadOnly);
				SetStatus(false, DataStati.AlwaysReadOnly);
			}

			IsModified = true;
		}

		//-----------------------------------------------------------------------------
		public void SetAsTime(bool isTime)
		{
			Debug.Assert(DataType.Equals(CoreTypes.DataType.Date) || DataType.Equals(CoreTypes.DataType.Long));	//@@ElapsedTime

			SetStatus(isTime, DataStati.Time);

			if (isTime && DataType.Equals(CoreTypes.DataType.Date))
				SetFullDate(true);
		}

		//-----------------------------------------------------------------------------
		public void SetReadOnly(bool readOnly)
		{
			SetStatus(readOnly, DataStati.ReadOnly);

			if (readOnly)
				SetStatus(false, DataStati.Findable);

			IsModified = true;
		}

		//-----------------------------------------------------------------------------
		public void SetOslReadOnly(bool readOnly)
		{
			SetStatus(readOnly, DataStati.OslReadOnly);

			if (readOnly)
				SetStatus(false, DataStati.Findable);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			DataObj temp = obj as DataObj;
			if (temp == null)
				return false;

			return (temp.CompareTo(temp) == 0);
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return dataStatus.GetHashCode();
		}

		//RDE parsing e unparsing
		//---------------------------------------------------------------------
		public virtual string ConvertToJson()
		{
            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(GetType());      // todo rsweb
            //MemoryStream ms = new MemoryStream();
            //serializer.WriteObject(ms, this);
            //return Encoding.UTF8.GetString(ms.ToArray());
            return "";
		}

		//---------------------------------------------------------------------
		public static IDataObj JsonToDataObj(string from, Type type)
		{
            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(type);                  //todo rsweb
            //MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(from));
            //return (IDataObj)serializer.ReadObject(ms);
            return null;
        }

		#region IDataObj Members

		//---------------------------------------------------------------------
		// Ignora ReadOnly e Selected status mantenendo i valori originali, cioe` non si 
		// ereditano per copia nelle assegnazioni e nelle copie le caratteristiche estetiche
		// di sola lettura e di selezionato
		public virtual void AssignStatus(IDataObj dataObj)
		{
			DataObj temp = dataObj as DataObj;

			if (temp != null)
			{
				SetValid(temp.IsValid);
				if (temp.IsCollateCultureSensitive)
					IsCollateCultureSensitive = true;
			}
		}

		//---------------------------------------------------------------------
		public virtual void Clear(bool valid)
		{ 
			if (IsValueLocked)
				return;

			IsValid = valid;
			IsModified = true;
			IsDirty = true;

			SetValueChanged(false);
		}

		//---------------------------------------------------------------------
		public virtual void Clear()
		{
			Clear(true);
		}

		//---------------------------------------------------------------------
		public abstract string ToString(int minLen, int maxLen);

		//---------------------------------------------------------------------
		public abstract IDataType DataType { get; }

		//---------------------------------------------------------------------
		public abstract bool IsEmpty();

		//---------------------------------------------------------------------
		public abstract string GetXmlType(bool soapType);

		//---------------------------------------------------------------------
		public abstract string GetXmlType();

		//---------------------------------------------------------------------
		public abstract string FormatDataForXml(bool soapType);

		//---------------------------------------------------------------------
		public abstract void AssignFromXmlString(string xmlFragment);

		//---------------------------------------------------------------------
		public abstract string FormatDataForXml();

		//---------------------------------------------------------------------
		public virtual void SetLowerValue(int value)
		{ }

		//---------------------------------------------------------------------
		public virtual void SetUpperValue(int value)
		{ }

		//---------------------------------------------------------------------
		public virtual bool IsLowerValue()
		{
			return false;
		}

		//---------------------------------------------------------------------
		public virtual bool IsUpperValue()
		{
			return false;
		}

		//---------------------------------------------------------------------
		public abstract bool IsEqual(IDataObj dataObj);

		//---------------------------------------------------------------------
		public abstract bool IsLessThan(IDataObj dataObj);

		//---------------------------------------------------------------------
		public abstract bool IsLessEqualThan(IDataObj dataObj);

		//---------------------------------------------------------------------
		public abstract bool IsGreaterThan(IDataObj dataObj);

		//---------------------------------------------------------------------
		public abstract bool IsGreaterEqualThan(IDataObj dataObj);

		#endregion

		#region IEquatable<IDataObj> Members

		//---------------------------------------------------------------------
		public abstract bool Equals(IDataObj other);

		#endregion

		#region IComparable Members

		//---------------------------------------------------------------------
		public abstract int CompareTo(object obj);

		#endregion

		#region IComparable<IDataObj> Members

		//---------------------------------------------------------------------
		public abstract int CompareTo(IDataObj other);

		#endregion

		#region ICloneable Members

		//---------------------------------------------------------------------
		public abstract object Clone();

		#endregion

		#region INotifyPropertyChanging Members

		public event PropertyChangingEventHandler PropertyChanging;

		//---------------------------------------------------------------------
		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (PropertyChanging != null)
				PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		//---------------------------------------------------------------------
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region IComponent Members

		public event EventHandler Disposed;

		public ISite Site { get; set; }

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		#endregion

		//---------------------------------------------------------------------
		public bool BelongsToPrototypeRecord
		{
			get { return false; }
		}
	}

	//=========================================================================
	class NullDataObj : DataObj
	{
		//---------------------------------------------------------------------
		protected override object GetValue()
		{
			return null;
		}

		protected override void SetValue(object value)
		{
		}

		//---------------------------------------------------------------------
		public override IDataType DataType
		{
			get { return CoreTypes.DataType.Null; }
		}

		//---------------------------------------------------------------------
		public override string ToString(int minLen, int maxLen)
		{
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public override bool IsEmpty()
		{
			return true;
		}

		//---------------------------------------------------------------------
		public override string GetXmlType(bool soapType)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override string GetXmlType()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override string FormatDataForXml(bool soapType)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override void AssignFromXmlString(string xmlFragment)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override string FormatDataForXml()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override bool IsEqual(IDataObj dataObj)
		{
			NullDataObj aNullDataObj = dataObj as NullDataObj;

			return (aNullDataObj != null);
		}

		//---------------------------------------------------------------------
		public override bool IsLessThan(IDataObj dataObj)
		{
			return !Equals(dataObj);
		}

		//---------------------------------------------------------------------
		public override bool IsLessEqualThan(IDataObj dataObj)
		{
			return true;
		}

		//---------------------------------------------------------------------
		public override bool IsGreaterThan(IDataObj dataObj)
		{
			return false;
		}

		//---------------------------------------------------------------------
		public override bool IsGreaterEqualThan(IDataObj dataObj)
		{
			return Equals(dataObj);
		}

		//---------------------------------------------------------------------
		public override bool Equals(IDataObj other)
		{
			return IsEqual(other);
		}

		//---------------------------------------------------------------------
		public override int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			NullDataObj aNullDataObj = obj as NullDataObj;

			if (aNullDataObj == null)
				return -1;

			return 0;
		}

		//---------------------------------------------------------------------
		public override int CompareTo(IDataObj other)
		{
			return CompareTo(other);
		}

		//---------------------------------------------------------------------
		public override object Clone()
		{
			return new NullDataObj();
		}
	}
}
