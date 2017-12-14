using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace TBMobile
{
	public abstract class MDataObj 
	{
		
		public static readonly MDataObj Null = new NullDataObj();

		private const string dataStatusTag = "DataStatus";
		private DataStatus dataStatus;
		private bool modified;
		private MSqlRecordItem owner;

		public MSqlRecordItem Owner
		{
			get { return owner; }
			internal set { owner = value; }
		}


		internal static MDataObj Create(MDataType dataObjType)
		{
			return new MDataStr();
		}
		
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

		public override string ToString()
		{
			object o = GetValue();
			return o == null? "(null)" : o.ToString();
		}
		protected abstract object GetValue();
		protected abstract void SetValue(object value);


		//---------------------------------------------------------------------
		public DataStatus DataStatus
		{
			get { return dataStatus; }
		}

		//---------------------------------------------------------------------
		protected void SetStatus(bool set, DataStatus statusFlag)
		{
			dataStatus = set ? (dataStatus | statusFlag) : (dataStatus & ~statusFlag);
		}

		//---------------------------------------------------------------------
		protected MDataObj()
		{
			//this.dataStatus = 0;inizializzato a zero dal runtime.
		}

		//---------------------------------------------------------------------
		protected MDataObj(DataStatus dataStatus)
		{
			this.dataStatus = dataStatus;
		}

		//---------------------------------------------------------------------
		public bool IsValid
		{
			get { return ((dataStatus & DataStatus.Valid) == DataStatus.Valid); }
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
			get { return ((dataStatus & DataStatus.Modified) == DataStatus.Modified); }
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
			get { return ((dataStatus & DataStatus.Uppercase) == DataStatus.Uppercase); }
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
			get { return (dataStatus & DataStatus.DBCaseCompliant) == DataStatus.DBCaseCompliant; }
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
			get { return (dataStatus & DataStatus.ValueChanged) == DataStatus.ValueChanged; }
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
			get { return ((dataStatus & DataStatus.Dirty) == DataStatus.Dirty); }
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
			get { return ((dataStatus & DataStatus.Findable) == DataStatus.Findable); }
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
			get { return ((dataStatus & DataStatus.FullDate) == DataStatus.FullDate); }
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
			get { return ((dataStatus & DataStatus.Time) == DataStatus.Time); }
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
			get { return ((dataStatus & DataStatus.UpdateView) == DataStatus.UpdateView); }
			set
			{
				OnPropertyChanging("IsUpdateView");
				SetUpdateView(value);
				OnPropertyChanged("IsUpdateView");
			}
		}
		//---------------------------------------------------------------------
		public bool IsEnabled
		{
			get { return !IsReadOnly; } set{}
		}
		//---------------------------------------------------------------------
		public bool IsReadOnly
		{
			get
			{
				return
					(dataStatus & DataStatus.ReadOnly) == DataStatus.ReadOnly ||
					(dataStatus & DataStatus.OslReadOnly) == DataStatus.OslReadOnly ||
					(dataStatus & DataStatus.AlwaysReadOnly) == DataStatus.AlwaysReadOnly;
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
					(dataStatus & DataStatus.Hide) == DataStatus.Hide ||
					(dataStatus & DataStatus.OslHide) == DataStatus.OslHide;
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
			get { return (dataStatus & DataStatus.ReadOnly) == DataStatus.ReadOnly; }
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
			get { return ((dataStatus & DataStatus.Hide) == DataStatus.Hide); }
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
			get { return ((dataStatus & DataStatus.OslReadOnly) == DataStatus.OslReadOnly); }
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
			get { return ((dataStatus & DataStatus.OslHide) == DataStatus.OslHide); }
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
			get { return ((dataStatus & DataStatus.AlwaysReadOnly) == DataStatus.AlwaysReadOnly); }
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
			get { return ((dataStatus & DataStatus.ValueLocked) == DataStatus.ValueLocked); }
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
			get { return ((dataStatus & DataStatus.CollateCultureSensitive) == DataStatus.CollateCultureSensitive); }
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
			SetStatus(valid, DataStatus.Valid);
		}

		//---------------------------------------------------------------------
		public void SetModified(bool modified)
		{
			SetStatus(modified, DataStatus.Modified);
		}

		//---------------------------------------------------------------------
		public void SetUppercase(bool uppercase)
		{
			SetStatus(uppercase, DataStatus.Uppercase);
			Debug.Assert(DataType.Equals(MDataType.String));
		}

		//---------------------------------------------------------------------
		public void SetDBCaseCompliant(bool dbCaseCompliant)
		{
			SetStatus(dbCaseCompliant, DataStatus.DBCaseCompliant);
			Debug.Assert(DataType.Equals(MDataType.String));
		}


		//---------------------------------------------------------------------
		public void SetValueChanged(bool changed)
		{
			SetStatus(changed, DataStatus.ValueChanged);
		}

		//---------------------------------------------------------------------
		public void SetDirty(bool dirty)
		{
			SetStatus(dirty, DataStatus.Dirty);
		}

		//---------------------------------------------------------------------
		public void SetFullDate(bool fullDate)
		{
			SetStatus(fullDate, DataStatus.FullDate);
			Debug.Assert(DataType.Equals(MDataType.Date));
		}

		//---------------------------------------------------------------------
		public void SetUpdateView(bool changed)
		{
			SetStatus(changed, DataStatus.UpdateView);
		}

		//---------------------------------------------------------------------
		public void SetHide(bool hide)
		{
			SetStatus(hide, DataStatus.Hide);
		}

		//---------------------------------------------------------------------
		public void SetOslHide(bool hide)
		{
			SetStatus(hide, DataStatus.OslHide);
		}

		//---------------------------------------------------------------------
		public void SetAlwaysReadOnly(bool readOnly)
		{
			SetStatus(readOnly, DataStatus.AlwaysReadOnly);

			if (readOnly)
				SetStatus(false, DataStatus.Findable);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public void SetValueLocked(bool valueLocked)
		{
			SetStatus(valueLocked, DataStatus.ValueLocked);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public void SetCollateCultureSensitive(bool sensitive)
		{
			SetStatus(sensitive, DataStatus.CollateCultureSensitive);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public void SetFindable(bool findable)
		{
			SetStatus(findable, DataStatus.Findable);

			if (findable)
			{
				SetStatus(false, DataStatus.ReadOnly);
				SetStatus(false, DataStatus.AlwaysReadOnly);
			}

			IsModified = true;
		}

		//-----------------------------------------------------------------------------
		public void SetAsTime(bool isTime)
		{
			Debug.Assert(DataType.Equals(MDataType.Date) || DataType.Equals(MDataType.Long));	//@@ElapsedTime

			SetStatus(isTime, DataStatus.Time);

			if (isTime && DataType.Equals(MDataType.Date))
				SetFullDate(true);
		}

		//-----------------------------------------------------------------------------
		public void SetReadOnly(bool readOnly)
		{
			SetStatus(readOnly, DataStatus.ReadOnly);

			if (readOnly)
				SetStatus(false, DataStatus.Findable);

			IsModified = true;
		}

		//-----------------------------------------------------------------------------
		public void SetOslReadOnly(bool readOnly)
		{
			SetStatus(readOnly, DataStatus.OslReadOnly);

			if (readOnly)
				SetStatus(false, DataStatus.Findable);

			IsModified = true;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			MDataObj temp = obj as MDataObj;
			if (temp == null)
				return false;

			return (temp.CompareTo(temp) == 0);
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return dataStatus.GetHashCode();
		}

		
		//---------------------------------------------------------------------
		// Ignora ReadOnly e Selected status mantenendo i valori originali, cioe` non si 
		// ereditano per copia nelle assegnazioni e nelle copie le caratteristiche estetiche
		// di sola lettura e di selezionato
		public virtual void AssignStatus(MDataObj dataObj)
		{
			MDataObj temp = dataObj as MDataObj;

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
		public abstract MDataType DataType { get; }

		//---------------------------------------------------------------------
		public abstract bool IsEmpty();


		//---------------------------------------------------------------------
		public abstract void Assign(MDataObj mDataObj);
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
		public abstract bool Equals(MDataObj other);

		//---------------------------------------------------------------------
		public abstract int CompareTo(object obj);

		//---------------------------------------------------------------------
		public abstract int CompareTo(MDataObj other);

		//---------------------------------------------------------------------
		public abstract object Clone();

		public event PropertyChangingEventHandler PropertyChanging;

		//---------------------------------------------------------------------
		protected virtual void OnPropertyChanging(string propertyName)
		{
			if (PropertyChanging != null)
				PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		//---------------------------------------------------------------------
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}



		public event EventHandler Disposed;

		public void Dispose()
		{
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}
	}

	//=========================================================================
	class NullDataObj : MDataObj
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
		public override MDataType DataType
		{
			get { return MDataType.Null; }
		}

		//---------------------------------------------------------------------
		public override bool IsEmpty()
		{
			return true;
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
		public override bool Equals(MDataObj other)
		{
			NullDataObj aNullDataObj = other as NullDataObj;

			return (aNullDataObj != null);
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
		public override int CompareTo(MDataObj other)
		{
			return CompareTo(other);
		}

		//---------------------------------------------------------------------
		public override object Clone()
		{
			return new NullDataObj();
		}

		public override void Assign(MDataObj mDataObj)
		{
			throw new NotImplementedException();
		}
	}

	//=========================================================================
	/// <summary>
	/// Available stati for a DataObj instance.
	/// </summary>
	[Flags]
	public enum DataStatus
	{
		// Basato sulla definizione di DataStatus in DataObj.h
		DBCaseCompliant = 0x0001,
		Uppercase = 0x0002,
		FullDate = 0x0004,	// usato solo per DataDate per indicare l'uso del Time
		Time = 0x0008,	// usato per DataDate per indicare che e` un Ora
		// e per DataLng per indicare che e` un tempo
		ReadOnly = 0x00010,	// per gestire il readonly nei controls dipendentemente allo stato del documento
		Hide = 0x00020,	// per gestire il hide/show dei controls
		Findable = 0x00040,	// abilita la ricerca nei documenti
		ValueChanged = 0x00080,	// riservato ed utilizzabile dal programmatore
		Valid = 0x00100,	// usato dal report engine
		Modified = 0x00200,	// riservato dalla gestione interna del documento
		Dirty = 0x00400,	// usato per ottimizzare i/o su database
		UpdateView = 0x00800,	// usato per forzare la rivisualizzazione del dato
		OslReadOnly = 0x01000,	// OSL: per gestire il readonly nei controls
		OslHide = 0x02000,	// OSL: per gestire il hide/show dei controls
		AlwaysReadOnly = 0x04000,	// per gestire il readonly nei controls indipendentemente dallo stato del documento
		ValueLocked = 0x08000,	// per impedire l'assegnazione di un nuovo valore al DataObj
		CollateCultureSensitive = 0x10000,	// indica che il contenuto è collate culture-sensitive
		TbHandle = 0x0004,	// usato come attributo del DATA_LONG_TYPE (DataType::Long) per indicare che il contenuto è un handle (DataType::Object)
		TbVoid = 0x0008		// usato come attributo del DATA_NULL_TYPE (DataType::Null) per indicare un valore di ritorno void (DataType::Void)
	}


	public class MDataStr : MDataObj
	{
		private string _value = "";
		protected override object GetValue()
		{
			return _value;
		}

		protected override void SetValue(object value)
		{
			string s =  (value == null) ? "" : value.ToString();
			if (s.Equals(_value))
				return;
			OnPropertyChanging("Value");
			this._value = s;
			OnPropertyChanged("Value");
		}

	

		public override MDataType DataType
		{
			get { return MDataType.String; }
		}

		public override bool IsEmpty()
		{
			return string.IsNullOrEmpty(_value);
		}

		public override string FormatDataForXml()
		{
			return _value;
		}

		public override void AssignFromXmlString(string xmlFragment)
		{
			SetValue(xmlFragment);
		}

		public override bool Equals(MDataObj other)
		{
			MDataStr ds = other as MDataStr;
			if (ds == null)
				return false;
			return _value.Equals(ds._value);
		}

		public override int CompareTo(object other)
		{
			MDataStr ds = other as MDataStr;
			if (ds == null)
				return 1;
			return _value.CompareTo(ds._value);
		}

		public override int CompareTo(MDataObj other)
		{
			MDataStr ds = other as MDataStr;
			if (ds == null)
				return 1;
			return _value.CompareTo(ds._value);
		}

		public override object Clone()
		{
			throw new NotImplementedException();
		}

		public override void Assign(MDataObj mDataObj)
		{
			SetValue(mDataObj.Value);
		}
	}
}

