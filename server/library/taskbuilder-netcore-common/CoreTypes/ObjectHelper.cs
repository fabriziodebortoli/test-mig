using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces.Model;
using TaskBuilderNetCore.Data;
using Microarea.Common.Hotlink;


namespace Microarea.Common.CoreTypes
{

    //================================================================================
    public class DefaultFormat
	{
		public const string Logico		= "Bool";
		public const string Intero		= "Integer";
		public const string Esteso		= "Long";
		public const string Reale		= "Double";
		public const string Testo		= "Text";
		public const string Uuid		= "Uuid";
		public const string DataOra		= "DateTime";
        public const string Ora         = "Time";
		public const string Enumerativo	= "Enum";
		public const string None		= "";
	}

	//================================================================================
	public class DefaultFont
	{
		public const string Testo				= "Text";			//FNT_TEXT			
		public const string Normale				= "Normal";			//FNT_STANDARD		
		public const string CellaStringa		= "CellText";		//FNT_CELL_STRING		
		public const string CellaNumerica		= "CellNumber";		//FNT_CELL_NUM		
		public const string TotaleStringa		= "TotalText";		//FNT_TOTAL_STRING	
		public const string TotaleNumerico		= "TotalNumber";	//FNT_TOTAL_NUM		
		public const string SubTotaleStringa	= "SubTotalText";	//FNT_SUBTOTAL_STRING	
		public const string SubTotaleNumerico	= "SubTotalNumber";	//FNT_SUBTOTAL_NUM	
		public const string TitoloTabella		= "TableTitle";		//FNT_TABLE_TITLE		
		public const string Descrizione			= "Description";	//FNT_LABEL			
		public const string TitoloColonna		= "ColumnTitle";	//FNT_COLUMN_TITLE			
		public const string Radar				= "Radar";				
		public const string Monospaziato		= "MonoSpaced";
		public const string None				= "";
	}
	
	///=============================================================================
	public class FrameworkType
	{
		///=============================================================================
		public class Microsoft
		{
			public const string Boolean		= "Boolean";
			public const string Byte		= "Byte";
			public const string Int16		= "Int16";	
			public const string Int32		= "Int32";
			public const string Int64		= "Int64";
			public const string Single		= "Single";
			public const string Decimal		= "Decimal";
			public const string Double		= "Double";
			public const string String		= "String";
			public const string Guid		= "Guid";	
			public const string DateTime	= "DateTime";
			public const string DataEnum	= "DataEnum";
			public const string DataArray	= "DataArray";
		}
	
		///=============================================================================
		public class Microarea
		{
			public const string Tbstring		= "string";
			public const string TbicString		= "icString";
			public const string Tblongstring	= "longstring";
			public const string Tbinteger		= "integer";
			public const string Tblong			= "long";	
			public const string Tbdouble		= "double";
			public const string Tbpercent		= "percent";
			public const string Tbquantity		= "quantity";
			public const string Tbmoney			= "money";
			public const string Tbuuid			= "uuid";
			public const string Tbdate			= "date";	
			public const string Tbtime			= "time";
			public const string Tbdatetime		= "datetime";
			public const string Tbbool			= "bool";
			public const string Tbenum			= "enum";
			public const string Tbelapsedtime	= "elapsedtime";
			public const string Tbblob			= "blob";
			public const string Tbfloat			= "float";
			public const string Tbarray			= "array";
		}
	}

	///=============================================================================
	public class ObjectHelperException : Exception
	{
		public ObjectHelperException(string message) : base(message) {}
		public ObjectHelperException(string message, Exception inner) : base(message, inner) {}
	}	

	/// <summary>
	/// Descrizione di riepilogo per ObjectHelper.
	/// </summary>
	///=============================================================================
	public sealed class ObjectHelper
	{
		// limite superiore per la richiesta dei limiti
		public static string MaxString = "zzzzz";
		public static string MinString = "";

		public static string TrueString = "1";
		public static string FalseString = "0";

		// limitazione di SqlServer sulla gestione delle date (ALLINEATI A QUELLI di TbGeneric\DataObj.cpp)
		public static DateTime NullTbDateTime = new DateTime(1799, 12, 31, 0, 0, 0);
		public static DateTime MaxDateTime = new DateTime(2199, 12, 31);
		public static DateTime MinDateTime = new DateTime(1800, 1, 1);

		public static double DataMonEpsilon		= 10E-7;
		public static double DataQtyEpsilon		= 10E-7;
		public static double DataPercEpsilon	= 10E-7;
		public static double DataDblEpsilon		= 10E-7;

		//-----------------------------------------------------------------------------
		private ObjectHelper()
		{
		}

		//-----------------------------------------------------------------------------
		public static string GetMaxString(string cultureMaxString)
		{
			return (cultureMaxString == null || cultureMaxString.Length == 0) ? MaxString : cultureMaxString;
		}
		//-----------------------------------------------------------------------------
		public static string GetMaxString(string str, string cultureMaxString, int len)
		{
			return str + (new string((GetMaxString(cultureMaxString))[0], Math.Max(0, len - str.Length)));
		}
		//-----------------------------------------------------------------------------
		public static string TrimMaxString(string s, string cultureMaxString)
		{
			int j = 0;
			for (j = s.Length - 1; j >= 0 && s[j] == cultureMaxString[0]; j--);
			return s.Substring(0, j + 1);
		}

		//-----------------------------------------------------------------------------
		public static object CreateObject(string type) { return CreateObject(type, "", 0, 0); }
		public static object CreateObject(string type, string baseType, ushort tag, ushort item)
		{
			if (type == null || type.Length == 0)
				return null;

            int idx = type.IndexOf('.');
            if (idx > -1)   //"System."
            {
                type = type.Mid(idx);
            }
            
			switch (type.ToLower())
			{
 				case "string"	: string s = ""; return s;

                case "money":
                case "quantity": 
                case "percent":
				case "double"	: double d = 0.0; return d;

                case "bool"     :
				case "boolean"	: bool b = false; return b;

				case "datetime"	: DateTime dt = DateTime.Now; return dt;
                case "date"     : DateTime dt1 = DateTime.Today; return dt1;

                case "enum": 
                case "dataenum" : return new DataEnum(tag, item);

                case "integer"  : 
				case "int16"	: short i16 = 0; return i16;

                case "int":
                case "long":
                case "int32"    : int i = 0; return i;

				case "int64"	: long l = 0; return l;

                case "uuid" :
 				case "guid"		: return new Guid();

                case "array"    : 
				case "dataarray": return new DataArray(baseType);

				case "byte"		: byte i8 = 0; return i8;
				case "decimal"	: decimal dm = 0.0M; return dm;
				case "single"	: float f = 0.0F; return f;

				case "object"	: object o = new object(); return o;

                case "void"     :
				case "variant"	:
                                    return null;
			}

			throw(new ObjectHelperException("CreateObject: illegal data type " + type));
		}

		//-----------------------------------------------------------------------------
		public static bool CheckType(object o1, object o2)
		{
			if (o1 == null && o2 == null)
				return true;

			if (o1 != null && o2 == null)
			{
				o1 = null;//ITRI per me nn serve perchè non è by ref???
				return false;
			}

			if (o1 == null && o2 != null)
			{
				Debug.WriteLine(CoreTypeStrings.VariableNull);
				return false;
			}

			if (!Compatible(o1, o2))
			{
				Debug.WriteLine(CoreTypeStrings.VariableNotCompatible);
				return false;
			}

			return true;
		}

        //-----------------------------------------------------------------------------
        public static bool IsATime(DateTime d)
		{
            return d.Year == DateTimeFunctions.MinTimeYear 
                    && 
                    d.Month == DateTimeFunctions.MinTimeMonth 
                    && 
                    d.Day == DateTimeFunctions.MinTimeDay;
		}

		//-----------------------------------------------------------------------------
		public static bool IsDblEquals(double d1, double d2)
		{
			return System.Math.Abs(d1 - d2) < DataDblEpsilon;
		}
		//-----------------------------------------------------------------------------
		public static bool IsEquals(object o1, object o2)
		{
			if (!CheckType(o1, o2)) 
                return false;

            if (o1 == null && o2 == null)
                return true;
            if (o1 == null || o2 == null)
                return false;

            //if (o1 is IDataObj)
            //o1 = ((IDataObj)o1).Value;
            //if (o2 is IDataObj)
            //o2 = ((IDataObj)o2).Value;
            try { 
                switch (o1.GetType().Name)
			    {
				    case "String"	: { return (string) o1 == (string) o2; }

				    case "Double"	: 
					    {
						    return IsDblEquals((double)o1, (double)o2);
						    //return (double) o1 == (double) o2; 
					    }

				    case "DateTime"	: { return (DateTime) o1 == (DateTime) o2; }
				    case "Boolean"	: { return (bool) o1 == (bool) o2; }
				    case "Int16"	: { return (short) o1 == (short) o2; }
				    case "Int32"	: { return (int) o1 == (int) o2; }
				    case "Int64"	: { return (long) o1 == (long) o2; }
				    case "DataEnum"	: { return (DataEnum) o1 == (DataEnum)o2; }
				    case "Guid"		: { return (Guid) o1 == (Guid) o2; }
				    case "Array"    :
				    case "DataArray": { return (DataArray) o1 == (DataArray)o2; }
				    case "Byte"		: { return (byte) o1 == (byte) o2; }
				    case "Decimal"	: { return (decimal) o1 == (decimal) o2; }
				    case "Single"	: { return (float) o1 == (float) o2; }
			    }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            throw (new ObjectHelperException(CoreTypeStrings.IllegalDataType + " " + o2));
		}

		//-----------------------------------------------------------------------------
		public static bool IsDblGreater(double d1, double d2)
		{
			return d1 > d2 && System.Math.Abs(d1 - d2) >= DataDblEpsilon;
		}

		//-----------------------------------------------------------------------------
		public static bool IsGreater(object o1, object o2)
		{
			if (!CheckType(o1, o2)) 
                return false;

            if (o1 == null && o2 == null)
                return false;

            //if (o1 is IDataObj)
                //o1 = ((IDataObj)o1).Value;
            //if (o2 is IDataObj)
                //o2 = ((IDataObj)o2).Value;

            switch (o1.GetType().Name)
			{
				case "String"	: { return string.Compare((string) o1,(string) o2) > 0; }
				case "Int16"	: { return (short) o1 > (short) o2; }
				case "Int32"	: { return (int) o1 > (int) o2; }
				case "Int64"	: { return (long) o1 > (long) o2; }
				case "Double"	: 
						{
							return IsDblGreater((double)o1, (double)o2);
							//return (double) o1 > (double) o2; 
						}
				case "DateTime"	: { return (DateTime) o1 > (DateTime) o2; }
				case "DataEnum"	: { return (DataEnum) o1 > (DataEnum)o2; }
				case "Boolean"	: { return (bool) o1 != (bool) o2; }
				case "Guid"		: { return ((Guid) o1).CompareTo((Guid)o2) > 0;}
				case "Byte"		: { return (byte) o1 > (byte) o2; }
				case "Single"	: { return (float) o1 > (float) o2; }
				case "Decimal"	: { return (decimal) o1 > (decimal) o2; }
////				case "Array":
//				case "DataArray":
//								{ return (DataArray) o1 > (DataArray)o2; }
			}

			throw (new ObjectHelperException(CoreTypeStrings.IllegalDataType + o2));
		}

		//-----------------------------------------------------------------------------
		public static bool IsDblLess(double d1, double d2)
		{
			return d1 < d2 && System.Math.Abs(d1 - d2) >= DataDblEpsilon;
		}
		//-----------------------------------------------------------------------------
		public static bool IsLess(object o1, object o2)
		{
			if (!CheckType(o1, o2)) 
                return false;

            if (o1 == null && o2 == null)
                return false;

            //if (o1 is IDataObj)
                //o1 = ((IDataObj)o1).Value;
            //if (o2 is IDataObj)
                //o2 = ((IDataObj)o2).Value;

			switch (o1.GetType().Name)
			{
				case "String": { return string.Compare((string)o1, (string)o2) < 0; }
				case "Int16": { return (short)o1 < (short)o2; }
				case "Int32": { return (int)o1 < (int)o2; }
				case "Int64": { return (long)o1 < (long)o2; }
				case "Double": 
						{
							return IsDblLess((double)o1, (double)o2);
							//return (double)o1 < (double)o2; 
						}
				case "DateTime": { return (DateTime)o1 < (DateTime)o2; }
				case "DataEnum": { return (DataEnum)o1 < (DataEnum)o2; }
				case "Boolean": { return (bool)o1 != (bool)o2; }
				case "Guid": { return ((Guid)o1).CompareTo((Guid)o2) < 0; }
				case "Byte": { return (byte)o1 < (byte)o2; }
				case "Single": { return (float)o1 < (float)o2; }
				case "Decimal": { return (decimal)o1 < (decimal)o2; }
				////				case "Array":
				//				case "DataArray":
				//								{ return (DataArray) o1 < (DataArray)o2; }
			}

			throw (new ObjectHelperException(CoreTypeStrings.IllegalDataType + o2));
		}

		//-----------------------------------------------------------------------------
		public static bool Compatible(object from, object to)
		{
			string fromType = DataType(from);
			string toType = DataType(to);

			return Compatible(fromType, toType);
		}

        //-----------------------------------------------------------------------------
        public static bool Compatible(object from, string toType)
        {
            string fromType = DataType(from);
 
            return Compatible(fromType, toType);
        }

		//-----------------------------------------------------------------------------
		public static bool Compatible(string fromType, string toType)
		{
			if (fromType == toType) return true;

            if (fromType == "") return true;//(Variant) late binding (lo saprò solo in esecuzione se sono compatibili)
            if (fromType == "Variant") return true;//late binding (lo saprò solo in esecuzione se sono compatibili)
			if (fromType == "Object") return true;//late binding (lo saprò solo in esecuzione se sono compatibili)
            if (fromType == "String") return true;//posso convertirlo

            switch (toType)
			{
				case "Int16": return
					(fromType == "Byte");

				case "Int32": return
					(fromType == "Byte")	||
                    (fromType == "Int16")   ||
                    (fromType == "Int64");  

				case "Int64": return
					(fromType == "Byte")	||
					(fromType == "Int16")	||
					(fromType == "Int32");

				case "Decimal": return
					(fromType == "Byte")	||
					(fromType == "Int16")	||
					(fromType == "Int32")	||
					(fromType == "Int64")	||
					(fromType == "DateTime")||
					(fromType == "Single")  ||
					(fromType == "Double");


				case "Double": return
					(fromType == "Boolean")	||
					(fromType == "Byte")	||
					(fromType == "Int16")	||
					(fromType == "Int32")	||
					(fromType == "Int64")	||
					(fromType == "Decimal")	||
					(fromType == "Single")	||
					(fromType == "DateTime");
					
				case "Boolean": return
					(fromType == "Byte")	||
					(fromType == "Int16")	||
					(fromType == "Int32")	||
					(fromType == "Boolean");

				case "Guid": return
					(fromType == "String") ||
					(fromType == "Guid");

				case "DataEnum": return
					(fromType == "Int16")	||
					(fromType == "Int32")	||
					(fromType == "Int64");

				case "Array": return (fromType == "DataArray");
				case "DataArray": return (fromType == "Array");

				case "Object": return true;
                case "Variant": return true;
                case "": return true;
            }
			return false;
		}

		//-----------------------------------------------------------------------------
		public static string DataType(object o) 
		{
            if (o is DataEnum)
                return o.GetType().Name;

			if (o is DataArray)
				return "Array";

            if (o is IDataObj)
                o = ((IDataObj)o).Value;

			return o.GetType().Name; 
		}

		//-----------------------------------------------------------------------------
		public static void Clear(ref object d)
		{
            if (d is IDataObj)
            {
                 ((IDataObj)d).Clear();
                return;
            }

			switch (d.GetType().Name)
			{
				case "String":		d = (object)"";					break;
				case "Double":		d = (object)0.0;				break;
				case "DateTime":	d = (object)NullTbDateTime;		break;
				case "DataEnum":	((DataEnum) d).Clear();			break;
				case "Boolean":		d = (object)false;				break;
				case "Int32":		d = (object)((int)0);			break;
				case "Int16":		d = (object)((short)0);			break;
				case "Int64":		d = (object)((long)0);			break;
				case "Guid":		d = (object)(new Guid());		break;
				case "DataArray":	
//				case "Array":	
					((DataArray) d).Clear();		break;
				case "Decimal":		d = (object)((decimal)0.0M);	break;
				case "Single":		d = (object)0.0F;				break;
				case "Byte":		d = (object)((byte)0);			break;
				default:
					Debug.WriteLine(CoreTypeStrings.ClearError);
					break;
			}
		}

		//-----------------------------------------------------------------------------
		public static object Clone(object from)
		{
			object clone = CreateObject(DataType(from));
			Assign (ref clone, from);
			return clone;
		}

		//-----------------------------------------------------------------------------
		public static void Assign(ref object to, object from)
		{
			if (!CheckType(from, to))
				throw (new ObjectHelperException(CoreTypeStrings.IncompatibleType));
            if (from == null || to == null)
                throw (new ObjectHelperException(CoreTypeStrings.UnsupportedType));

			switch (DataType(to))
			{
				case "String"	: to = CastString (from);	return;
                case "Long": 
				case "Int32"	: to = CastInt (from);		return;
				case "Double"	: to = CastDouble (from);	return;
                case "Integer": 
				case "Int16"	: to = CastShort (from);	return;
				case "DateTime"	: to = CastDateTime(from);	return;
				case "DataEnum"	: to = CastDataEnum (from);	return;
				case "Bool"	: 
                case "Boolean": to = CastBool(from); return;
                case "Int64": to = CastLong(from); return;
                case "Uuid": 
                case "Guid"		: to = CastGuid (from);		return;
				case "DataArray": 
				case "Array":	
								to = CastDataArray (from);	return;
				case "Byte"		: to = CastByte (from);		return;
				case "Decimal"	: to = CastDecimal (from);	return;
				case "Single"	: to = CastFloat (from);	return;
				case "Object"	: to = from;				return;
			}
			throw (new ObjectHelperException(CoreTypeStrings.IllegalType));
		}

		//-----------------------------------------------------------------------------
		public static bool CastBool(object d)
		{
			if (d is bool) return (bool)d;
			if (d is byte) 
			{
				byte i = (byte)d;
				return i != 0;
			}
			if (d is short) 
			{
				short i = (short)d;
				return i != 0;
			}

			if (d is int) 
			{
				int i = (int)d;
				return i != 0;
			}

			if (d is string)
			{
				string b = (string)d;
				return b == TrueString || b.CompareNoCase("true");
			}

			//Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastBool");
			return false;
		}

		//-----------------------------------------------------------------------------
		public static string CastString(object d)
		{
			if (d is DateTime)
			{
                DateTime dateTime = (DateTime)d;
                if (IsATime((dateTime)))
                {
                    string timePattern = "hh:mm:ss";
                    if (CultureInfo.DefaultThreadCurrentCulture != null)
                    {
                        timePattern = CultureInfo.DefaultThreadCurrentCulture.DateTimeFormat.ToString(); // TODO rsweb Thread.CurrentThread.CurrentCulture.DateTimeFormat.FullDateTimePattern;
                    }

                    return dateTime.ToString(timePattern, null);
                }
                else
                {
                    string shortDatePattern = "yyyy-MM-dd"; //Thh:mm:ss
                    if (CultureInfo.DefaultThreadCurrentCulture != null)
                    {
                        shortDatePattern = CultureInfo.DefaultThreadCurrentCulture.DateTimeFormat.ToString(); // TODO rsweb Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    }
                    
                    return dateTime.ToString(shortDatePattern, null);
                }
			}
			
			return d.ToString();
		}

		//-----------------------------------------------------------------------------
		public static Guid CastGuid(object d)
		{
			if (d is Guid)		return (Guid)d;
			if (d is string)	
                return new Guid((string)d);

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastGuid");
			return Guid.Empty;
		}

		//-----------------------------------------------------------------------------
		public static int CastByte(object d)
		{
			if (d is byte)	return (byte)d;

			Debug.WriteLine("Error in ObjectHelper.CastByte");
			return 0;
		}

		//-----------------------------------------------------------------------------
		public static short CastShort(object d)
		{
			if (d is short)		return (short)d;
            if (d is int) 
            {
                int i = (int) d;
                if (i <= short.MaxValue)
                    return (short)i;
            }
			if (d is byte)		return (byte)d;
			if (d is decimal)	return Decimal.ToInt16((decimal)d);
            if (d is string)
            {
                short l;
                if (short.TryParse(d as string, out l))
                    return l;
            }

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastShort");
			return 0;
		}

        //-----------------------------------------------------------------------------
        public static ushort CastUShort(object d)
        {
            if (d is short) return (ushort)d;
            if (d is int)
            {
                int i = (int)d;
                if (i <= 2*short.MaxValue)
                    return (ushort)i;
            }
            if (d is byte) return (byte)d;
            if (d is decimal) return Decimal.ToUInt16((decimal)d);
            if (d is string)
            {
                ushort l;
                if (ushort.TryParse(d as string, out l))
                    return l;
            }

            Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastShort");
            return 0;
        }

		//-----------------------------------------------------------------------------
		public static uint CastUInt(object d)
		{
			if (d is int) return (uint)(int)d;
			if (d is long) return (uint)(long)d;
			if (d is short) return (uint)(short)d;
			if (d is byte) return (byte)d;
			if (d is decimal) return Decimal.ToUInt32((decimal)d);
			if (d is string)
			{
				uint l;
				if (UInt32.TryParse(d as string, out l))
					return l;
			}
			if (d is DataEnum) return (uint)(DataEnum)d;

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastUInt");
			return 0;
		}

		//-----------------------------------------------------------------------------
		public static int CastInt(object d)
		{
			if (d is uint)		return (int)(uint)d;
			if (d is int)		return (int)d;
			if (d is short)		return (short)d;
            if (d is long)      return (int)(long)d;
            if (d is byte)      return (byte)d;
			if (d is double)	return (int)(double)d;
            if (d is decimal) return Decimal.ToInt32((decimal)d);
            if (d is string)
            {
                int i;
                if (int.TryParse(d as string, out i))
                    return i;
            }
			if (d is DataEnum)	
                                return (int)(uint)(DataEnum)d;

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastInt");
			return 0;
		}

		//-----------------------------------------------------------------------------
		public static long CastLong(object d)
		{
			if (d is long)		return (long)d;
			if (d is short)		return (short)d;
			if (d is int)		return (int)d;
			if (d is DateTime)	return DateTimeFunctions.GiulianDate((DateTime)d);
			if (d is DataEnum)	return (long)(uint)((DataEnum) d);
			if (d is byte)		return (byte)d;
            if (d is double)    return (long)(double)d;
            if (d is decimal)	return Decimal.ToInt64((decimal)d);
            if (d is string)
            {
                long l;
                if (long.TryParse(d as string, out l))
                    return l;
            }
            if (d is uint) return (long)(uint)d;

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastLong");
			return 0L;
		}

		//-----------------------------------------------------------------------------
		public static decimal CastDecimal(object d)
		{
			if (d is decimal)	return (decimal)d;
			if (d is long)		return (long)d;
			if (d is byte)		return (byte)d;
			if (d is short)		return (short)d;
			if (d is int)		return (int)d;

			try
			{
				if (d is double)	return new decimal((double)d);
				if (d is float)		return new decimal((float)d);
			}
			catch (OverflowException)
			{
				decimal dd = ((double)d) >= 0 ? decimal.MaxValue : decimal.MinValue;
				return dd;
			}

			if (d is DateTime)	return DateTimeFunctions.GiulianDate((DateTime)d);
			if (d is DataEnum)	return (uint)((DataEnum) d);

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastDecimal");
			return 0;
		}
		
		//-----------------------------------------------------------------------------
		public static double CastDouble(object d)
		{
			if (d is double)	return (double)d;
			if (d is short)		return (short)d;
			if (d is int)		return (int)d;
			if (d is long)		return (long)d;
			if (d is float)		return (float)d;
			if (d is decimal)	return Decimal.ToDouble((decimal)d);
			if (d is byte)		return (byte)d;
            if (d is string)
            {
                double dbl;
                if (double.TryParse(d as string, out dbl))
                    return dbl;
            }

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastDouble");
			return 0.0;
		}

		//-----------------------------------------------------------------------------
		public static float CastFloat(object d)
		{
			if (d is float)		return (float)d;
			if (d is decimal)	return Decimal.ToSingle((decimal)d);
			if (d is byte)		return (byte)d;
			if (d is short)		return (short)d;
			if (d is int)		return (int)d;
			if (d is long)		return (long)d;

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastFloat");
			return 0.0F;
		}

		//-----------------------------------------------------------------------------
		public static DataEnum CastDataEnum(object d)
		{
			if (d is DataEnum)	return (DataEnum)d;
			if (d is int || d is long || d is decimal)	return new DataEnum(CastUInt(d));
            if (d is string)
            {
                uint l;
                if (uint.TryParse(d as string, out l))
                    return new DataEnum(l);
            }

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastDataEnum");

			return new DataEnum(0);
		}

		//-----------------------------------------------------------------------------
		public static DateTime CastDateTime(object d)
		{
			if (d is DateTime) return (DateTime)d;

            if (d is String) return DateTime.Parse((string)d);
			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastDateTime");
			return NullTbDateTime;
		}

		//-----------------------------------------------------------------------------
		public static DataArray CastDataArray(object d)
		{
			if (d is DataArray) return (DataArray)d;

			Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.CastDataArray");
			return new DataArray();
		}

		// per compatibilità con TBC++ i booleani sono rappresentati
		// dal carattere "1" per true e "0" per false.
		//-----------------------------------------------------------------------------
		public static string CastToDBBool(bool o)
		{
			return (bool)o ? TrueString : FalseString;
		}

		//-----------------------------------------------------------------------------
		public static object CastToDBData(object o)
		{
			if (o == null) return null;
			switch (o.GetType().Name)
			{
				case "Boolean"	:	return CastToDBBool((bool) o);
				case "DataEnum"	:	return CastInt((DataEnum) o); //Occhio! il database si aspetta un int, non uint!
			}
			
			return o;
		}

		//-----------------------------------------------------------------------------
		public static object CastFromDBData(object from, object to, Variable field = null)
		{
			if (from is System.DBNull)
			{
				Clear(ref to);
				return to;
			}

            if (from is System.Data.SqlClient.SqlParameter)
            {
                return CastFromDBData((from as System.Data.SqlClient.SqlParameter).Value, to, field); 
            }

            string stype = field != null ? field.WoormType.ToLower() : to.GetType().Name.ToLower();
            stype.RemovePrefix("system.");
            int posTag = stype.IndexOf('[');
            if (posTag > -1)
            {
                stype = stype.Left(posTag);
            }

            switch (stype)
			{
               case "string"	: return CastString(from);
				case "boolean"	: return CastBool(from);
                case "bool"     : return CastBool(from);
 				case "int16"	: return CastShort(from);
                case "integer"  : return CastShort(from);
                case "int"      : return CastInt(from);
                case "int32"	: return CastInt(from);
				case "int64"	: return CastLong(from);
                case "long"     : return CastLong(from);
                case "double"	: return CastDouble(from);
                case "money"    : return CastDouble(from);
                case "quantity" : return CastDouble(from);
                case "percent"  : return CastDouble(from);
 				case "datetime"	: return CastDateTime(from);
				case "dataenum"	: return CastDataEnum(from);
                case "enum"     : return CastDataEnum(from);
                case "uuid": 
                case "guid"		: return CastGuid(from);
				case "decimal"	: return CastDecimal(from);
				case "single"	: return CastFloat(from);
                case "byte"		: return CastByte(from);
                case "text"     : return CastString(from);

                default: return CastString(from);
            }

            throw (new ObjectHelperException(CoreTypeStrings.IllegalType));
		}

		// evita l'eccezione
		//-----------------------------------------------------------------------------
		public static object Parse(string from, object to)
		{
			return Parse(from, to, false);
		}

		
		//-----------------------------------------------------------------------------
		public static object Parse(string from, object to, bool throwException)
		{
			if (from == null) return to;
			object retObject = Parse(from, to.GetType().Name, throwException); 
			if (retObject == null) 
				retObject = to;
			return retObject;
			
		}

		//-----------------------------------------------------------------------------
		public static object Parse(string from, Type destinationType, bool throwException)
		{
			if (from == null) return null;
			object retObject = Parse(from, destinationType.Name, throwException);

			return retObject;
		}
		
		// Attenzione per parsare una stringa contenente un DataEnum in forma estesa({"colore":"rosso"})
		// si deve usare usare: Enums.Parse(from, to)
		//-----------------------------------------------------------------------------
		public static object Parse(string from, string typeName, bool throwException)
		{
			switch (typeName)
			{
				case "Boolean"	: 
					return from == TrueString || from.CompareNoCase("true");

				case "Byte"		: 
				{ 
					byte b = 0; 
					try { if (from.Length > 0) b = Byte.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return b;
				}
				case "Int16"	: 
				{
					short s = 0;
					try { if (from.Length > 0) s = Int16.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return s;
				}
				case "Int32"	:
				{
					int i = 0;
					try { if (from.Length > 0) i = Int32.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return i;
				}
				case "Int64"	:
				{
					long l = 0;
					try { if (from.Length > 0) l = Int64.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return l;
				}
				case "Decimal"	:
				{
					decimal d = 0.0M;
					try { if (from.Length > 0) d = Decimal.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return d;
				}
				case "Single"	:	
				{
					float f = 0.0F;
					try { if (from.Length > 0) f = Single.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return f;
				}
				case "Double"	:
				{
					double d = 0.0;
					try { if (from.Length > 0) d = Double.Parse(from); }
					catch(Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return d;
				}

				case "String"	:
					return from;

				case "Guid"		:
					return new Guid(from);

				case "DateTime"	:
				{
					DateTime d = NullTbDateTime;
                        try
                        {
                            if (from.Length > 0)
                                d = DateTime.Parse(from);
                        }
                        catch (Exception e)
                        {
                            if (throwException)
                                throw (new ObjectHelperException(e.Message));
                            else Debug.WriteLine(e.Message);
                        }
                        finally
                        {
                            
                        }
                        return d;
				};

				case "DataEnum"	:
				{
					// in questo caso "from" deve contenere un int valido che è la rappresentazione del DataEnum
					DataEnum de = new DataEnum(0);
					try { de = DataEnum.Parse(from); }
					catch (Exception e) { if (throwException) throw(new ObjectHelperException(e.Message)); else Debug.WriteLine(e.Message); }
					return de;
				}
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public static bool IsCleared(object d)
		{
			if (d == null)
				return false;

			switch (d.GetType().Name)
			{
				case "Boolean"	: return (bool) d == false;
				case "Byte"		: return (byte) d == 0;
				case "Int16"	: return (short) d == 0;
				case "Int32"	: return (int) d == 0;
				case "Int64"	: return (long) d == 0;
				case "Decimal"	: return (decimal) d == 0.0M;
				case "Single"	: return (float) d == 0.0F;
				case "Double"	: return (double) d == 0.0;
				case "String"	: return (string) d == "";
				case "Guid"		: return false;
				case "DateTime"	: return (DateTime) d == NullTbDateTime;
				case "DataEnum"	: return false;
				case "DataArray": 
//				case "Array": 
								  return ((DataArray)d).Count == 0;
			}
			throw (new ObjectHelperException(CoreTypeStrings.IllegalType));
		}

		//-----------------------------------------------------------------------------
		public static bool IsUpperValue(object d, string cultureMaxString)
		{
			if (d == null)
				return false;

			switch (d.GetType().Name)
			{
				case "Boolean"	: return (bool) d == true;
				case "Byte"		: return (byte) d == 0;
				case "Int16"	: return (short) d == short.MaxValue;
				case "Int32"	: return (int) d == int.MaxValue;
				case "Int64"	: return (long) d == long.MaxValue;
				case "Decimal"	: return (decimal) d == decimal.MaxValue;
				case "Single"	: return (float) d == float.MaxValue;
				case "Double"	: return (double) d == double.MaxValue;
				case "String"	: return (string) d == GetMaxString(string.Empty, cultureMaxString, ((string)d).Length);
				case "Guid"		: return false;
				case "DateTime"	: return (DateTime) d == MaxDateTime;
				case "DataEnum"	: return false;
				case "DataArray": return false;
			}

			throw (new ObjectHelperException(CoreTypeStrings.IllegalType));
		}

		//-----------------------------------------------------------------------------
		public static bool IsLowerValue(object d)
		{
			if (d == null)
				return false;

			switch (d.GetType().Name)
			{
				case "Boolean"	: return (bool) d == false;
				case "Byte"		: return (byte) d == 0;
				case "Int16"	: return (short) d == short.MinValue;
				case "Int32"	: return (int) d == int.MinValue;
				case "Int64"	: return (long) d == long.MinValue;
				case "Decimal"	: return (decimal) d == decimal.MinValue;
				case "Single"	: return (float) d == float.MinValue;
				case "Double"	: return (double) d == double.MinValue;
				case "String"	: return (string) d == MinString;
				case "Guid"		: return false;
				case "DateTime"	: return (DateTime) d == MinDateTime;
				case "DataEnum"	: return false;
				case "DataArray": return false;
			}

			throw (new ObjectHelperException(CoreTypeStrings.IllegalType));
		}

		//-----------------------------------------------------------------------------
		public static object SetUpperLimit(object d, string cultureMaxString, int len)
		{
			switch (d.GetType().Name)
			{
				case "Boolean":		return (object)true;
				case "Byte":		return (object)byte.MaxValue;
				case "Int16":		return (object)short.MaxValue;
				case "Int32":		return (object)int.MaxValue;
				case "Int64":		return (object)long.MaxValue;
				case "Decimal":		return (object)decimal.MaxValue;
				case "Single":		return (object)float.MaxValue;
				case "Double":		return (object)double.MaxValue;
				case "String":		return (object)GetMaxString((string)d, cultureMaxString, len);
				case "Guid":		return (object)(new Guid(""));
				case "DateTime":	return (object)MaxDateTime;
				case "DataEnum":	((DataEnum) d).Clear(); return d;
//				case "DataArray":	((DataArray) d).Clear(); return d;
				default:
					Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.SetUpperLimit");
					return null;
			}
		}

		//-----------------------------------------------------------------------------
		public static object SetLowerLimit(object d)
		{
			switch (d.GetType().Name)
			{
				case "Boolean":		return (object)false;
				case "Byte":		return (object)byte.MinValue;
				case "Int16":		return (object)short.MinValue;
				case "Int32":		return (object)int.MinValue;
				case "Int64":		return (object)long.MinValue;
				case "Decimal":		return (object)decimal.MinValue;
				case "Single":		return (object)float.MinValue;
				case "Double":		return (object)double.MinValue;
				case "String":		return ((d as string).IsNullOrEmpty() ? MinString : d);
				case "Guid":		return (object)(new Guid(""));
				case "DateTime":	return (object)MinDateTime;
				case "DataEnum":	((DataEnum) d).Clear();	return d;
//				case "DataArray":	((DataArray) d).Clear(); return d;
				default:
					Debug.WriteLine(CoreTypeStrings.ErrorIn + " ObjectHelper.SetUpperLimit");
					return null;
			}
		}

		//-----------------------------------------------------------------------------
		public static string DefaultFormatStyleName(object d)
		{
			switch (d.GetType().Name)
			{
				case "Boolean":		return DefaultFormat.Logico;
				case "Byte":		return DefaultFormat.Intero;
				case "Int16":		return DefaultFormat.Intero;
				case "Int32":		return DefaultFormat.Intero;
				case "Int64":		return DefaultFormat.Esteso;
				case "Decimal":		return DefaultFormat.Esteso;
				case "Single":		return DefaultFormat.Reale;
				case "Double":		return DefaultFormat.Reale;
				case "String":		return DefaultFormat.Testo;
				case "Guid":		return DefaultFormat.Uuid;
                case "DateTime":
                    {
                        if (IsATime((DateTime)d))
                             return DefaultFormat.Ora;

                        return DefaultFormat.DataOra;
                    }
				case "DataEnum":	return DefaultFormat.Enumerativo;
			}

			return DefaultFormat.None;
		}

		// dice se posso tagliuzzare la stringa per farla entrare dentro ad una cella di dimensioni prefissate
		//-----------------------------------------------------------------------------
		public static bool AllowOverflow(string type)
		{
			return
				type == "Boolean"	||
				type == "String"	||
				type == "Guid"		||
				type == "DataEnum";
		}

		// serve per gli allineamenti a sinistra per le stringhe a destra per il resto
		//-----------------------------------------------------------------------------
		public static bool IsLeftAligned(string type)
		{
			return
				type == "Boolean"	||
				type == "String"	||
				type == "Guid"		||
				type == "DateTime"  ||
				type == "DataEnum";
		}

		// implementa il match di tipo tra i tipi TB definiti in XMLTags.h e quelli C#
		//-----------------------------------------------------------------------------
		public static Type TBTypeToType(string type)
		{
			switch (type.ToLower())
			{
				case "string": return typeof(string);
				case "cistring": return typeof(String);
				case "longstring": return typeof(String);
				case "integer": return typeof(Int32);
				case "long": return typeof(Int64);
				case "double": return typeof(Double);
				case "percent": return typeof(Double);
				case "quantity": return typeof(Double);
				case "money": return typeof(Double);
				case "uuid": return typeof(String);
				case "date": return typeof(DateTime);
				case "time": return typeof(DateTime);
				case "datetime": return typeof(DateTime);
				case "bool": return typeof(Boolean);
				case "enum": return typeof(DataEnum);
				case "elapsedtime": return typeof(Int64); // TB trasforma in long memorizzando millisecondi
				case "array": return typeof(DataArray);
				case "void": return typeof(void);
				case "var": return typeof(Object);
                //----
                case "boolean": return typeof(Boolean);
                case "dataenum": return typeof(DataEnum);
                case "dataarray": return typeof(DataArray);
                case "int": return typeof(Int32);
                case "int32": return typeof(Int32);
                case "int64": return typeof(Int64);
                case "blob": return typeof(object);
				case "float": return typeof(Single);
			}

			throw (new ObjectHelperException(CoreTypeStrings.UnsupportedType + " " + type));
		}

		// implementa il match di tipo tra i tipi TB definiti in XMLTags.h e quelli C#
		//-----------------------------------------------------------------------------
		public static string FromTBType(string type)
		{
			if (type == "blob")
				return "Blob";
			
			return TBTypeToType(type).Name;
		}

		//-----------------------------------------------------------------------------
		public static PropertyInfo GetProperty(object obj, string propName)
		{
			return GetPropertyFromType(obj.GetType(), propName);
			
		}

		//-----------------------------------------------------------------------------
		private static PropertyInfo GetPropertyFromType(Type type, string propName)
		{
			//prima provo con la ricerca nel tipo specifico
			PropertyInfo mi = type.GetProperty(
				propName,
				BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.DeclaredOnly
				);
			//poi anche nei tipi base
			if (mi == null)
			{
                type = type.GetTypeInfo().BaseType;
				if (type != null)
					return GetPropertyFromType(type, propName);
			}
				

			return mi;
		}

		//-----------------------------------------------------------------------------
		public static FieldInfo GetField(object obj, string propName)
		{
			//prima provo con la ricerca nel tipo specifico
			FieldInfo mi = obj.GetType().GetField(
				propName,
				BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.DeclaredOnly
				);
			//poi anche nei tipi base
			if (mi == null)
					mi = obj.GetType().GetField(
				   propName,
				   BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance
				   );
			return mi;
		}
		//-----------------------------------------------------------------------------
		public static MethodInfo GetMethod(object obj, string methodName, Type[] types)
		{
			//prima provo con la ricerca nel tipo specifico
			MethodInfo mi = obj.GetType().GetMethod(
				methodName, 
				//BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				//Type.DefaultBinder,
				types 
				//null
				);
			//poi anche nei tipi base
			//if (mi == null)
			//	mi = obj.GetType().GetMethod(
			//		methodName,
			//		BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance,
			//		Type.DefaultBinder,
			//		types,
			//		null
			//		);
			return mi;
		}

		// implementa il match di tipo tra i tipi Schema Microsoft e quelli C#
		//-----------------------------------------------------------------------------
		public static string FromSchemaType(string type)
		{
			switch (type.ToLower())
			{
				case "xs:string"		: return "String";
				case "xs:int"			: return "Int32";
				case "xs:integer"		: return "Int64";
				case "xs:double"		: return "Double";
				case "xs:float"			: return "Float";
				case "xs:short"			: return "Short";
				case "xs:unsignedint"	: return "Int32";
				case "xs:date"			: return "DateTime";
				case "xs:time"			: return "DateTime";
				case "xs:datetime"		: return "DateTime";
				case "xs:boolean"		: return "Boolean";			
			}

			throw (new ObjectHelperException(CoreTypeStrings.UnsupportedSchemaType + " " + type));
		}

		/// <summary>
		/// Converte un oggetto nel tipo specificato
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object ConvertType(object source, Type type)
		{
			if (source is IConvertible)
				return Convert.ChangeType(source, type);
			if (source is IntPtr)
				return Convert.ChangeType((long)(IntPtr)source, type);

			return source;
		}

        public static string GetDotNetType(string type, Provider.DBType dbType)
        {
            switch (type)
            {
                case "bigint": return "Int64";
                case "varbinary":
                case "binary": return "Byte[]";
                case "date":
                case "smalldatetime":
                case "datetime":
                case "datetime2": return "DateTime";
                case "datetimeoffset": return "DateTimeOffset";
                case "numeric":
                case "smallmoney":
                case "decimal": return "Decimal";
                case "money":
                case "float": return "Double";
                case "int": return "Int32";
                case "char":
                case "nvarchar":
                case "varchar":
                case "text":
                case "ntext":
                case "nchar": return "String";
                case "real": return "Single";
                case "smallint": return "Int16";
                case "time": return "TimeSpan";
                case "tinyint": return "Byte";
                case "uniqueidentifier": return "Guid";
                case "xml": return "Xml";
            }
            return "";
        }
    }
}
