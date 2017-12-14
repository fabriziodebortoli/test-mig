using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public class MDataType : IComparable, IComparable<MDataType>
	{
		public static readonly MDataType Null = new MDataType(0, 0);
		public static readonly MDataType Void = new MDataType(0, (int)DataStatus.TbVoid);
		public static readonly MDataType String = new MDataType(1, 0);
		public static readonly MDataType Integer = new MDataType(2, 0);
		public static readonly MDataType Long = new MDataType(3, 0);
		public static readonly MDataType ElapsedTime = new MDataType(3, (int)DataStatus.Time);
		public static readonly MDataType Object = new MDataType(3, (int)DataStatus.TbHandle);
		public static readonly MDataType Double = new MDataType(4, 0);
		public static readonly MDataType Money = new MDataType(5, 0);
		public static readonly MDataType Quantity = new MDataType(6, 0);
		public static readonly MDataType Percent = new MDataType(7, 0);
		public static readonly MDataType Date = new MDataType(8, 0);
		public static readonly MDataType DateTime = new MDataType(8, (int)DataStatus.FullDate);
		public static readonly MDataType Time = new MDataType(8, (int)(DataStatus.FullDate | DataStatus.Time));
		public static readonly MDataType Bool = new MDataType(9, 0);
		public static readonly MDataType Enum = new MDataType(10, 0);
		public static readonly MDataType Array = new MDataType(11, 0);
		public static readonly MDataType Guid = new MDataType(12, 0);
		public static readonly MDataType Text = new MDataType(13, 0);
		public static readonly MDataType Variant = new MDataType(14, 0);
		public static readonly MDataType Blob = new MDataType(15, 0);



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
		public MDataType(int type, int tag)
		{
			this.type = type;
			this.tag = tag;
		}

		//---------------------------------------------------------------------
		public MDataType(MDataType dataType)
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
		public MDataType(string dataType)
		{
			Int32 aInt32 = Int32.Parse(
				dataType,
				System.Globalization.CultureInfo.InvariantCulture
				);

			if (aInt32 < 0)
				throw new ArgumentException("Cannot use negative values");

			FromInt32(aInt32, out type, out tag);
		}

		//---------------------------------------------------------------------
		public MDataType(Int32 dataType)
		{
			if (dataType < 0)
				throw new ArgumentException("Cannot use negative values");

			FromInt32(dataType, out type, out tag);
		}

		//---------------------------------------------------------------------
		public MDataType(UInt32 dataType)
		{
			FromUInt32(dataType, out type, out tag);
		}

		//---------------------------------------------------------------------
		public static explicit operator Int32(MDataType t) { return (t.Tag << 16) + t.Type; }
		//---------------------------------------------------------------------
		public static explicit operator MDataType(Int32 i) { return new MDataType(i); }

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
				(type == MDataType.DateTime.type) &&
				((tag & (int)DataStatus.FullDate) == (int)DataStatus.FullDate);
		}

		//---------------------------------------------------------------------
		public void SetFullDate(bool fullDate)
		{
			Debug.Assert(type == MDataType.Date.type);

			tag = Convert.ToUInt16(
				fullDate
				? (tag | (int)DataStatus.FullDate)
				: (tag & ~(int)DataStatus.FullDate)
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
				(type == MDataType.Date.type || type == MDataType.Long.type) &&	//@@ElapsedTime
				((tag & (int)DataStatus.Time) == (int)DataStatus.Time);
		}

		//---------------------------------------------------------------------
		public void SetAsTime(bool isTime)
		{
			Debug.Assert(type == MDataType.Date.type || type == MDataType.Long.type);	//@@ElapsedTime

			tag = Convert.ToUInt16(
				isTime
				? (tag | (int)DataStatus.Time)
				: (tag & ~(int)DataStatus.Time)
				);

			if (isTime && type == MDataType.Date.type)
				SetFullDate(true);
		}

		//---------------------------------------------------------------------
		public void SetAsTime()
		{
			SetAsTime(true);
		}

		//---------------------------------------------------------------------
		public static bool operator ==(MDataType t1, MDataType t2)
		{
			return t1.CompareTo(t2) == 0;
		}

		//---------------------------------------------------------------------
		public static bool operator ==(MDataType t1, int t2)
		{
			return t1.type == t2;
		}

		//---------------------------------------------------------------------
		public static bool operator !=(MDataType t1, MDataType t2)
		{
			return !(t1 == t2);
		}

		//---------------------------------------------------------------------
		public static bool operator !=(MDataType t1, int t2)
		{
			return !(t1 == t2);
		}

		//---------------------------------------------------------------------
		public static bool operator <(MDataType t1, MDataType t2)
		{
			return t1.CompareTo(t2) < 0;
		}

		//---------------------------------------------------------------------
		public static bool operator >(MDataType t1, MDataType t2)
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
			if (this == MDataType.String) return DataTypeStrings.String;
			else if (this == MDataType.Integer) return DataTypeStrings.Integer;
			else if (this == MDataType.Long) return DataTypeStrings.Long;
			else if (this == MDataType.Double) return DataTypeStrings.Double;
			else if (this == MDataType.Money) return DataTypeStrings.Money;
			else if (this == MDataType.Quantity) return DataTypeStrings.Quantity;
			else if (this == MDataType.Percent) return DataTypeStrings.Percent;
			else if (this == MDataType.Bool) return DataTypeStrings.Bool;
			else if (this == MDataType.Guid) return DataTypeStrings.Uuid;
			else if (this == MDataType.Date) return DataTypeStrings.Date;
			else if (this == MDataType.Time) return DataTypeStrings.Time;
			else if (this == MDataType.DateTime) return DataTypeStrings.DateTime;
			else if (this == MDataType.Enum) return DataTypeStrings.Enum;
			else if (this == MDataType.ElapsedTime) return DataTypeStrings.ElapsedTime;
			else if (this == MDataType.Text) return DataTypeStrings.Text;
			else if (this == MDataType.Blob) return DataTypeStrings.Blob;
			else if (this == MDataType.Array) return DataTypeStrings.Array;
			else if (this == MDataType.Void) return DataTypeStrings.Void;
			else if (this == MDataType.Null) return string.Empty;
			/*else if (this.Type == MDataType.Enum.Type)
			{
				EnumTag tag = Enums.Tags.GetTag((ushort)this.Tag);
				return tag == null ? string.Empty : tag.LocalizedName;
			}*/

			Debug.Assert(false);
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return ((tag << 16) + type);
		}

		//----------------------------------------------------------------------------------------------
		static public MDataType StringToDataType(string type)
		{
			if (string.Compare(type, DataTypeStrings.String, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.String;
			else if (string.Compare(type, DataTypeStrings.Integer, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Integer;
			else if (string.Compare(type, DataTypeStrings.Long, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Long;
			else if (string.Compare(type, DataTypeStrings.Double, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Double;
			else if (string.Compare(type, DataTypeStrings.Money, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Money;
			else if (string.Compare(type, DataTypeStrings.Quantity, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Quantity;
			else if (string.Compare(type, DataTypeStrings.Percent, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Percent;
			else if (string.Compare(type, DataTypeStrings.Bool, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Bool;
			else if (string.Compare(type, DataTypeStrings.Uuid, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Guid;
			else if (string.Compare(type, DataTypeStrings.Date, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Date;
			else if (string.Compare(type, DataTypeStrings.Time, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Time;
			else if (string.Compare(type, DataTypeStrings.DateTime, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.DateTime;
			else if (string.Compare(type, DataTypeStrings.ElapsedTime, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.ElapsedTime;
			else if (string.Compare(type, DataTypeStrings.Enum, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Enum;
			else if (string.Compare(type, DataTypeStrings.Text, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Text;
			else if (string.Compare(type, DataTypeStrings.Blob, StringComparison.OrdinalIgnoreCase) == 0) return MDataType.Blob;
			/*else
				if (type.StartsWith(DataTypeStrings.EnumType))
				{
					string tag = type.Substring(3);
					MDataType newType = new MDataType();
					newType.Type = MDataType.Enum.Type;
					newType.Tag = int.Parse(tag);
					return newType;
				}*/

			Debug.Assert(false);
			return MDataType.Null;
		}

		//---------------------------------------------------------------------
		public string DataTypeToString()
		{
			if (this == MDataType.String) return DataTypeStrings.String;
			else if (this == MDataType.Integer) return DataTypeStrings.Integer;
			else if (this == MDataType.Long) return DataTypeStrings.Long;
			else if (this == MDataType.Double) return DataTypeStrings.Double;
			else if (this == MDataType.Money) return DataTypeStrings.Money;
			else if (this == MDataType.Quantity) return DataTypeStrings.Quantity;
			else if (this == MDataType.Percent) return DataTypeStrings.Percent;
			else if (this == MDataType.Bool) return DataTypeStrings.Bool;
			else if (this == MDataType.Guid) return DataTypeStrings.Uuid;
			else if (this == MDataType.Date) return DataTypeStrings.Date;
			else if (this == MDataType.Time) return DataTypeStrings.Time;
			else if (this == MDataType.DateTime) return DataTypeStrings.DateTime;
			else if (this == MDataType.Enum) return DataTypeStrings.Enum;
			else if (this == MDataType.ElapsedTime) return DataTypeStrings.ElapsedTime;
			else if (this == MDataType.Text) return DataTypeStrings.Text;
			else if (this == MDataType.Blob) return DataTypeStrings.Blob;
			else if (this == MDataType.Array) return DataTypeStrings.Array;
			else if (this == MDataType.Void) return DataTypeStrings.Void;
			else if (this == MDataType.Null) return string.Empty;
			else if (this.Type == MDataType.Enum.Type)
				return string.Format("10:{0}", this.Tag.ToString());

			Debug.Assert(false);
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			if (obj.GetType() != typeof(MDataType))
				throw new ArgumentException("Invalid type");

			MDataType aDataType = (MDataType)obj;

			return (this.tag != aDataType.tag)
				? this.tag.CompareTo(aDataType.tag)
				: this.type.CompareTo(aDataType.type);
		}

		//---------------------------------------------------------------------
		public int CompareTo(MDataType other)
		{
			return (this.tag != other.tag)
				? this.tag.CompareTo(other.tag)
				: this.type.CompareTo(other.type);
		}

	}
}
