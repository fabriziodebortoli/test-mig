using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microarea.Common.Applications;

using TaskBuilderNetCore.Interfaces.Model;

namespace Microarea.Common.CoreTypes
{
	//=========================================================================
	//[CLSCompliant(false)]//l'uso di ushort rende non CLS compliant.
	public struct DataType : IComparable, IComparable<DataType>, IDataType
	{
		public static readonly DataType Null		= new DataType(0, 0);
		public static readonly DataType Void		= new DataType(0, (int)DataStati.TbVoid);
		public static readonly DataType String		= new DataType(1, 0);
		public static readonly DataType Integer		= new DataType(2, 0);
		public static readonly DataType Long		= new DataType(3, 0);
		public static readonly DataType ElapsedTime = new DataType(3, (int)DataStati.Time);
		public static readonly DataType Object		= new DataType(3, (int)DataStati.TbHandle);
		public static readonly DataType Double		= new DataType(4, 0);
		public static readonly DataType Money		= new DataType(5, 0);
		public static readonly DataType Quantity	= new DataType(6, 0);
		public static readonly DataType Percent		= new DataType(7, 0);
		public static readonly DataType Date		= new DataType(8, 0);
		public static readonly DataType DateTime	= new DataType(8, (int)DataStati.FullDate);
		public static readonly DataType Time		= new DataType(8, (int)(DataStati.FullDate | DataStati.Time));
		public static readonly DataType Bool		= new DataType(9, 0);
		public static readonly DataType Enum		= new DataType(10, 0);
		public static readonly DataType Array		= new DataType(11, 0);
		public static readonly DataType Guid		= new DataType(12, 0);
		public static readonly DataType Text		= new DataType(13, 0);
		public static readonly DataType Variant		= new DataType(14, 0);
        public static readonly DataType Record      = new DataType(15, 0);  //TODO
        public static readonly DataType SqlRecord   = new DataType(16, 0);  //TODO
        public static readonly DataType Blob		= new DataType(17, 0);

		private static Enums enums;
		//---------------------------------------------------------------------
		public static Enums Enums 
		{
			get
			{
				if (enums == null)
				{
					lock (typeof(DataType))
					{
						if (enums == null)
						{ 
							enums = new Enums();
							try { enums.LoadXml(); } catch  { }
						}
					}
				}

				return enums;
			}
		}

		private static DataType[] enumTypes;
		//---------------------------------------------------------------------
		public static DataType[] EnumTypes
		{
			get
			{
				if (enumTypes == null)
				{
					lock (typeof(DataType))
					{
						if (enumTypes == null)
							enumTypes = LoadEnumTypes();
					}
				}
				return enumTypes;
			}
		}

		//---------------------------------------------------------------------
		private static DataType[] LoadEnumTypes()
		{
			List<DataType> list = new List<DataType>();
			foreach (EnumTag tag in Enums.Tags)
			{
				list.Add(new DataType(10, tag.Value));
			}
			return list.ToArray();
		}

		//---------------------------------------------------------------------
		public static void RefreshEnumTypes()
		{
			enumTypes = LoadEnumTypes();
		}

		//====================================================================================
		public class DataTypeStrings
		{
			//non ha senso crearne un'istanza
			private DataTypeStrings() { }

			public const string Void = "void"; 
			public const string String = "string";
			public const string Integer = "integer";
			public const string Long = "long";
			public const string Float = "float";
			public const string Double = "double";
			public const string Money = "money";
			public const string Quantity = "quantity";
			public const string Percent = "percent";
			public const string Bool = "bool";
			public const string Uuid = "uuid";
			public const string Date = "date";
			public const string Time = "time";
			public const string DateTime = "dateTime";
			public const string Enum = "enum";
			public const string ElapsedTime = "elapsedTime";
			public const string Text = "text";
			public const string Blob = "blob";
			public const string Array = "array";
            public const string EnumType = "10:";
		}
		
		private int type;
		private int tag;

		//---------------------------------------------------------------------
		public bool IsEnum { get { return Type == 10; } }
		//---------------------------------------------------------------------
		public int Type
		{
			get { return type; }
			set { type = value; }
		}

		//---------------------------------------------------------------------
		public int Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		//---------------------------------------------------------------------
		public DataType(int type, int tag)
		{
			this.type = type;
			this.tag = tag;
		}

		//---------------------------------------------------------------------
		public DataType(DataType dataType)
		{
			type = dataType.type;
			tag = dataType.tag;
		}

		//---------------------------------------------------------------------
		/// <param name="aStringDataType">
		/// The numeric datatype value as Int32.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// aStringDataType is null.
		/// </exception>
		/// <exception cref="FormatException">
		/// aStringDataType is not of the correct format.
		/// </exception>
		/// <exception cref="OverflowException">
		/// represents a number less than Int32.MinValue or greater than Int32.MaxValue.
		/// </exception>
		/// <exception cref="ArgumentException"> represents a number lesser than 0.</exception>
		public DataType(string dataType)
		{
			Int32 aInt32 = Int32.Parse(
				dataType,
				System.Globalization.CultureInfo.InvariantCulture
				);

			if (aInt32 < 0)
				throw new ArgumentException(CoreTypeStrings.CannotUseNegativeValues);

			FromInt32(aInt32, out type, out tag);
		}

		//---------------------------------------------------------------------
		public DataType(Int32 dataType)
		{
			if (dataType < 0)
				throw new ArgumentException(CoreTypeStrings.CannotUseNegativeValues);

			FromInt32(dataType, out type, out tag);
		}

		//---------------------------------------------------------------------
		public DataType(UInt32 dataType)
		{
			FromUInt32(dataType, out type, out tag);
		}

		//---------------------------------------------------------------------
		public static explicit operator Int32(DataType t) { return (t.Tag << 16) + t.Type; }
		//---------------------------------------------------------------------
		public static explicit operator DataType(Int32 i) { return new DataType(i); }
		
		//---------------------------------------------------------------------
		private static void FromInt32(Int32 aInt32, out Int32 type, out Int32 tag)
		{
			type = Convert.ToInt32(aInt32 & 0xF);//prendo i 16 bit più bassi e converto tutto a int.
			tag = Convert.ToInt32(aInt32 >> 16);// prendo i 16 bit più alti e converto tutto a int.
		}

		//---------------------------------------------------------------------
		private static void FromUInt32(UInt32 aInt32, out Int32 type, out Int32 tag)
		{
			type = (int)Convert.ToUInt32(aInt32 & 0xF);//prendo i 16 bit più bassi e converto tutto a int.
			tag = (int)Convert.ToUInt32(aInt32 >> 16);// prendo i 16 bit più alti e converto tutto a int.
		}

		//---------------------------------------------------------------------
		public bool IsFullDate()
		{
			return
				(type == DataType.DateTime.type) &&
				((tag & (int)DataStati.FullDate) == (int)DataStati.FullDate);
		}

		//---------------------------------------------------------------------
		public void SetFullDate(bool fullDate)
		{
			Debug.Assert(type == DataType.Date.type);

			tag = Convert.ToUInt16(
				fullDate
				? (tag | (int)DataStati.FullDate)
				: (tag & ~(int)DataStati.FullDate)
				);
		}

		//---------------------------------------------------------------------
		public void SetFullDate()
		{
			SetFullDate(true);
		}

		//---------------------------------------------------------------------
		public bool IsATime()
		{
			return
				(type == DataType.Date.type || type == DataType.Long.type) &&	//@@ElapsedTime
				((tag & (int)DataStati.Time) == (int)DataStati.Time);
		}

		//---------------------------------------------------------------------
		public void SetAsTime(bool isTime)
		{
			Debug.Assert(type == DataType.Date.type || type == DataType.Long.type);	//@@ElapsedTime

			tag = Convert.ToUInt16(
				isTime
				? (tag | (int)DataStati.Time)
				: (tag & ~(int)DataStati.Time)
				);

			if (isTime && type == DataType.Date.type)
				SetFullDate(true);
		}

		//---------------------------------------------------------------------
		public void SetAsTime()
		{
			SetAsTime(true);
		}

		//---------------------------------------------------------------------
		public static bool operator ==(DataType t1, DataType t2)
		{
			return t1.CompareTo(t2) == 0;
		}

		//---------------------------------------------------------------------
		public static bool operator ==(DataType t1, int t2)
		{
			return t1.type == t2;
		}

		//---------------------------------------------------------------------
		public static bool operator !=(DataType t1, DataType t2)
		{
			return !(t1 == t2);
		}

		//---------------------------------------------------------------------
		public static bool operator !=(DataType t1, int t2)
		{
			return !(t1 == t2);
		}

		//---------------------------------------------------------------------
		public static bool operator <(DataType t1, DataType t2)
		{
			return t1.CompareTo(t2) < 0;
		}

		//---------------------------------------------------------------------
		public static bool operator >(DataType t1, DataType t2)
		{
			return t1.CompareTo(t2) > 0;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return this.CompareTo(obj) == 0;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (this == DataType.String) return DataTypeStrings.String;
			else if (this == DataType.Integer) return DataTypeStrings.Integer;
			else if (this == DataType.Long) return DataTypeStrings.Long;
			else if (this == DataType.Double) return DataTypeStrings.Double;
			else if (this == DataType.Money) return DataTypeStrings.Money;
			else if (this == DataType.Quantity) return DataTypeStrings.Quantity;
			else if (this == DataType.Percent) return DataTypeStrings.Percent;
			else if (this == DataType.Bool) return DataTypeStrings.Bool;
			else if (this == DataType.Guid) return DataTypeStrings.Uuid;
			else if (this == DataType.Date) return DataTypeStrings.Date;
			else if (this == DataType.Time) return DataTypeStrings.Time;
			else if (this == DataType.DateTime) return DataTypeStrings.DateTime;
			else if (this == DataType.Enum) return DataTypeStrings.Enum;
			else if (this == DataType.ElapsedTime) return DataTypeStrings.ElapsedTime;
			else if (this == DataType.Text) return DataTypeStrings.Text;
			else if (this == DataType.Blob) return DataTypeStrings.Blob;
			else if (this == DataType.Array) return DataTypeStrings.Array;
			else if (this == DataType.Void) return DataTypeStrings.Void;
			else if (this == DataType.Null) return string.Empty;
			else if (this.Type == DataType.Enum.Type)
			{
				EnumTag tag = Enums.Tags.GetTag((ushort)this.Tag);
				return tag == null ? string.Empty : tag.LocalizedName;
			}
			
			Debug.Assert(false);
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return ((tag << 16) + type);
		}

		//----------------------------------------------------------------------------------------------
		static public DataType StringToDataType(string type)
		{
			if (string.Compare(type, DataTypeStrings.String, true) == 0) return DataType.String;
			else if (string.Compare(type, DataTypeStrings.Integer, true) == 0) return DataType.Integer;
			else if (string.Compare(type, DataTypeStrings.Long, true) == 0) return DataType.Long;
			else if (string.Compare(type, DataTypeStrings.Double, true) == 0) return DataType.Double;
			else if (string.Compare(type, DataTypeStrings.Money, true) == 0) return DataType.Money;
			else if (string.Compare(type, DataTypeStrings.Quantity, true) == 0) return DataType.Quantity;
			else if (string.Compare(type, DataTypeStrings.Percent, true) == 0) return DataType.Percent;
			else if (string.Compare(type, DataTypeStrings.Bool, true) == 0) return DataType.Bool;
			else if (string.Compare(type, DataTypeStrings.Uuid, true) == 0) return DataType.Guid;
			else if (string.Compare(type, DataTypeStrings.Date, true) == 0) return DataType.Date;
			else if (string.Compare(type, DataTypeStrings.Time, true) == 0) return DataType.Time;
			else if (string.Compare(type, DataTypeStrings.DateTime, true) == 0) return DataType.DateTime;
			else if (string.Compare(type, DataTypeStrings.ElapsedTime, true) == 0) return DataType.ElapsedTime;
            else if (string.Compare(type, DataTypeStrings.Enum, true) == 0) return DataType.Enum;
            else if (string.Compare(type, DataTypeStrings.Text, true) == 0) return DataType.Text;
            else if (string.Compare(type, DataTypeStrings.Blob, true) == 0) return DataType.Blob;
            else
                if (type.StartsWith(DataTypeStrings.EnumType))
                {
                    string tag = type.Substring(3);
                    DataType newType = new DataType();
                    newType.Type = DataType.Enum.Type;
                    newType.Tag = int.Parse(tag);
                    return newType;
                }
	
			Debug.Assert(false);
			return DataType.Null;
		}

        //---------------------------------------------------------------------
        public string DataTypeToString()
        {
            if (this == DataType.String) return DataTypeStrings.String;
            else if (this == DataType.Integer) return DataTypeStrings.Integer;
            else if (this == DataType.Long) return DataTypeStrings.Long;
            else if (this == DataType.Double) return DataTypeStrings.Double;
            else if (this == DataType.Money) return DataTypeStrings.Money;
            else if (this == DataType.Quantity) return DataTypeStrings.Quantity;
            else if (this == DataType.Percent) return DataTypeStrings.Percent;
            else if (this == DataType.Bool) return DataTypeStrings.Bool;
            else if (this == DataType.Guid) return DataTypeStrings.Uuid;
            else if (this == DataType.Date) return DataTypeStrings.Date;
            else if (this == DataType.Time) return DataTypeStrings.Time;
            else if (this == DataType.DateTime) return DataTypeStrings.DateTime;
            else if (this == DataType.Enum) return DataTypeStrings.Enum;
            else if (this == DataType.ElapsedTime) return DataTypeStrings.ElapsedTime;
            else if (this == DataType.Text) return DataTypeStrings.Text;
            else if (this == DataType.Blob) return DataTypeStrings.Blob;
            else if (this == DataType.Array) return DataTypeStrings.Array;
            else if (this == DataType.Void) return DataTypeStrings.Void;
            else if (this == DataType.Null) return string.Empty;
            else if (this.Type == DataType.Enum.Type)
                return string.Format("10:{0}", this.Tag.ToString());

            Debug.Assert(false);
            return string.Empty;
        }

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (obj.GetType() != typeof(DataType))
				throw new ArgumentException(CoreTypeStrings.InvalidArgType);

			DataType aDataType = (DataType)obj;

			return (this.tag != aDataType.tag)
				? this.tag.CompareTo(aDataType.tag)
				: this.type.CompareTo(aDataType.type);
		}

		#endregion

		#region IComparable<DataType> Members

		//---------------------------------------------------------------------
		public int CompareTo(DataType other)
		{
			return (this.tag != other.tag)
				? this.tag.CompareTo(other.tag)
				: this.type.CompareTo(other.type);
		}

		#endregion
	}
}
