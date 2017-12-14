using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	/// <summary>
	/// Descrizione di riepilogo per DataEnum.
	/// </summary>
	//=========================================================================
	[DataContract (Name="DataEnum", Namespace="")]
	//DataEnum ha il nome che termina con 'Enum' ma non eredita da un enumerativo.
	[Serializable]
	public class DataEnum : DataObj
	{
		private const string dataEnumTag = "DataEnum";

		private ushort tag;
		private ushort item;

		//---------------------------------------------------------------------
		protected override object GetValue()
		{
			return null;
		}

		protected override void SetValue(object value)
		{
		}

		//---------------------------------------------------------------------
		[DataMember]
		public ushort Tag
		{
			get { return tag; }
			set
			{
				OnPropertyChanging("Tag");

				if (IsValueLocked)
					return;

				tag = value;

				IsModified = true;
				IsDirty = true;

				OnPropertyChanged("Tag");
			}
		}
		//---------------------------------------------------------------------
		[DataMember]
		public ushort Item
		{
			get { return item; }
			set
			{
				OnPropertyChanging("Item");

				if (IsValueLocked)
					return;

				item = value;

				IsModified = true;
				IsDirty = true;

				OnPropertyChanged("Item");
			}
		}

		//---------------------------------------------------------------------
		public DataEnum(ushort tag, ushort item)
		{
			this.tag = tag;
			this.item = item;
		}

		//---------------------------------------------------------------------
		public DataEnum(uint toBeAssigned)
		{
			// inizializzati a 0 dal runtime
			//tag = 0;
			//item = 0;
			Assign(toBeAssigned);
		}

		//---------------------------------------------------------------------
		public void Assign(uint toBeAssigned)
		{
			int reminder;
			int quotient = Math.DivRem((int)toBeAssigned, 65536, out reminder);

			tag = (ushort)quotient;
			item = (ushort)reminder;
		}

		//---------------------------------------------------------------------
		public void Assign(ushort tag, ushort item)
		{
			this.tag = tag;
			this.item = item;
		}

		//---------------------------------------------------------------------
		public void Assign(ushort item)
		{
			this.item = item;
		}

		// a differenza del caso di TB in C++ il valore di default è sempre 0
		// a causa dell'impossibilità in questo punto di conoscere la tabella degli
		// enumerativi (Enums) che genererebbe una dipendenza circolare da Applications
		//---------------------------------------------------------------------
		public override void Clear() 
		{
			Clear(true);
		}

		//---------------------------------------------------------------------
		public override void Clear(bool valid)
		{
			if (IsValueLocked)
				return;

			base.Clear(valid);
		}

		//---------------------------------------------------------------------
		public string ToString(IFormatProvider provider)
		{
			if (provider == null)
				provider = CultureInfo.InvariantCulture;

			uint ui = (uint)this;
			return ui.ToString(provider);
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return ToString(CultureInfo.InvariantCulture);
		}

		//---------------------------------------------------------------------
		public static DataEnum Parse(string from) 
		{
			return new DataEnum(Parse(from, CultureInfo.InvariantCulture));
		}

		//---------------------------------------------------------------------
		public static DataEnum Parse(string from, IFormatProvider provider)
		{
			if (provider == null)
				provider = CultureInfo.InvariantCulture;

			uint ui = uint.Parse(from, provider);
			return new DataEnum(ui);
		}

		// Compatibile al formato Soap (vedi la classe System.Xml.XmlConvert)
		//---------------------------------------------------------------------
		public string XmlConvertToString()
		{ 
			uint ui = (uint)this;
			return XmlConvert.ToString(ui);
		}

		// Compatibile al formato Soap (vedi la classe System.Xml.XmlConvert)
		//---------------------------------------------------------------------
		public static DataEnum XmlConvertToDataEnum(string from) 
		{ 
			uint ui = XmlConvert.ToUInt32(from);
			return new DataEnum(ui);
		}

		//---------------------------------------------------------------------
		public static bool operator !=(DataEnum e1, DataEnum e2)
		{
			return !(e1 == e2);
		}
		//---------------------------------------------------------------------
		public static bool operator ==(DataEnum e1, DataEnum e2)
		{
			if (Object.ReferenceEquals(e1, e2))
				return true;

			if (Object.ReferenceEquals(null, e1) || Object.ReferenceEquals(null, e2))
				return false;

			return e1.Equals(e2);
		}

		//---------------------------------------------------------------------
		public static implicit operator uint(DataEnum dataEnum)
		{
			if (dataEnum == null)
				return 0;

			uint ui = dataEnum.tag * (uint)65536 + dataEnum.item;
			return ui;
		}

		//---------------------------------------------------------------------
		public static explicit operator int(DataEnum dataEnum)
		{
			if (dataEnum == null)
				return 0;

			return (int)(dataEnum.tag * (uint)65536 + dataEnum.item);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj) 
		{
			return Equals(obj as IDataObj);
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return (int) (tag * ushort.MaxValue + item);
		}

		//---------------------------------------------------------------------
		public override object Clone()
		{
			DataEnum op = new DataEnum(tag, item);

			op.Assign(tag, item);

			return op;
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		//solo per uniformità con gli altri tipi di dato
		public override int CompareTo(object obj)
		{
			IDataObj dataObj = obj as IDataObj;
			if (dataObj == null)
				throw new ArgumentException(CoreTypeStrings.InvalidArgType);

			return CompareTo(dataObj);
		}

		#endregion

		//---------------------------------------------------------------------
		public override string ToString(int minLen, int maxLen)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override IDataType DataType
		{
			get { return CoreTypes.DataType.Enum; }
		}

		//---------------------------------------------------------------------
		public override bool IsEmpty()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		public override string GetXmlType(bool soapType)
		{
			if (soapType)
				return XsdSchemaHelper.SchemaXsdDataTypeUintValue;

			return XsdSchemaHelper.SchemaXsdDataTypeStringValue;
		}

		//---------------------------------------------------------------------
		public override string GetXmlType()
		{
			return GetXmlType(false);
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
			if (Object.ReferenceEquals(null, dataObj))
				return false;

			return (CompareTo(dataObj) == 0);
		}

		//---------------------------------------------------------------------
		public override bool IsLessThan(IDataObj dataObj)
		{
			if (Object.ReferenceEquals(null, dataObj))
				return false;

			return (CompareTo(dataObj) <= 0);
		}

		//---------------------------------------------------------------------
		public override bool IsLessEqualThan(IDataObj dataObj)
		{
			if (Object.ReferenceEquals(null, dataObj))
				return false;

			return (CompareTo(dataObj) <= 0);
		}

		//---------------------------------------------------------------------
		public override bool IsGreaterThan(IDataObj dataObj)
		{
			if (Object.ReferenceEquals(null, dataObj))
				return true;

			return (CompareTo(dataObj) > 0);
		}

		//---------------------------------------------------------------------
		public override bool IsGreaterEqualThan(IDataObj dataObj)
		{
			if (Object.ReferenceEquals(null, dataObj))
				return true;

			return (CompareTo(dataObj) >= 0);
		}

		//---------------------------------------------------------------------
		public override bool Equals(IDataObj other)
		{
			if (Object.ReferenceEquals(null, other))
				return false;

			return CompareTo(other) == 0;
		}

		//---------------------------------------------------------------------
		public override int CompareTo(IDataObj other)
		{
			DataEnum dataEnum = other as DataEnum;
			if (Object.ReferenceEquals(null, dataEnum))
				return 1;

			return ((uint)this).CompareTo((uint)dataEnum);
		}

	}
}
