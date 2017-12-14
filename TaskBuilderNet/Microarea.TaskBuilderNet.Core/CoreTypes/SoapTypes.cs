using System;
using System.Globalization;
using System.Xml;


namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	///=============================================================================
	public class SoapTypes
	{
		//-----------------------------------------------------------------------------
		public static string	ToSoapBoolean	(bool d)		{ return XmlConvert.ToString(d).ToLower(CultureInfo.InvariantCulture); }
		public static string	ToSoapByte		(byte d)		{ return XmlConvert.ToString(d); }
		public static string	ToSoapShort		(short d)		{ return XmlConvert.ToString(d); }
		public static string	ToSoapInt		(int d)			{ return XmlConvert.ToString(d); }
		public static string	ToSoapLong		(long d)		{ return XmlConvert.ToString(d); }
		public static string	ToSoapFloat		(float d)		{ return XmlConvert.ToString(d); }
		public static string	ToSoapDouble	(double d)		{ return XmlConvert.ToString(d); }
		public static string	ToSoapString	(string s)		{ return System.Web.HttpUtility.HtmlEncode(s); }
		public static string	ToSoapGuid		(Guid d)		{ return XmlConvert.ToString(d); }
		public static string	ToSoapDateTime	(DateTime d)	{ return XmlConvert.ToString(d, @"yyyy-MM-ddTHH\:mm\:ss"); }
        public static string    ToSoapDate      (DateTime d)    { return XmlConvert.ToString(d, @"yyyy-MM-dd"); }
		public static string	ToSoapDataEnum	(DataEnum d)	{ return d.XmlConvertToString();}
		public static string	ToSoapDataArray	(DataArray d)	{ return d.XmlConvertToString();}
		public static string	ToSoapTimeSpan	(TimeSpan d)	{ return XmlConvert.ToString(d); }

		//-----------------------------------------------------------------------------
		public static bool		FromSoapBoolean	(string s)	{ return XmlConvert.ToBoolean(s.ToLower(CultureInfo.InvariantCulture)); }
		public static byte		FromSoapByte	(string s)	{ return XmlConvert.ToByte(s); }
		public static short		FromSoapShort	(string s)	{ return XmlConvert.ToInt16(s); }
		public static int		FromSoapInt		(string s)	{ return XmlConvert.ToInt32(s); }
		public static long		FromSoapLong	(string s)	{ return XmlConvert.ToInt64(s); }
		public static float		FromSoapFloat	(string s)	{ return XmlConvert.ToSingle(s); }
		public static double	FromSoapDouble	(string s)	{ return XmlConvert.ToDouble(s); }
		public static string	FromSoapString	(string s)	{ return System.Web.HttpUtility.HtmlDecode(s); }
		public static Guid		FromSoapGuid	(string s)	{ return XmlConvert.ToGuid(s); }
		public static DateTime FromSoapDateTime(string s) { return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Unspecified); }
		public static DataEnum	FromSoapDataEnum(string s)	{ return DataEnum.XmlConvertToDataEnum(s); }
		public static DataArray	FromSoapDataArray(string s)	{ return DataArray.XmlConvertToDataArray(s); }
		public static TimeSpan	FromSoapTimeSpan(string s)	{ return XmlConvert.ToTimeSpan(s); }

		//-----------------------------------------------------------------------------
		public static string To(object data)
		{
			string toType = data.GetType().Name;
			try
			{
				switch (toType)
				{
					case "String"	: return SoapTypes.ToSoapString		((string)data);
					case "Int64"	: return SoapTypes.ToSoapLong		((long)data);
					case "Int16"	: return SoapTypes.ToSoapShort		((short)data);
					case "Int32"	: return SoapTypes.ToSoapInt		((int)data);
					case "Double"	: return SoapTypes.ToSoapDouble		((double)data);
					case "DateTime"	: return SoapTypes.ToSoapDateTime	((DateTime)data);
					case "DataEnum"	: return SoapTypes.ToSoapDataEnum	((DataEnum)data);
					case "Boolean"	: return SoapTypes.ToSoapBoolean	((bool)data);
					case "Guid"		: return SoapTypes.ToSoapGuid		((Guid)data);
					case "DataArray": return SoapTypes.ToSoapDataArray	((DataArray)data);
					case "Single"	: return SoapTypes.ToSoapFloat		((float)data);
					case "Byte"		: return SoapTypes.ToSoapByte		((byte)data);
				}
			}
			catch (FormatException e)
			{
				throw(new SoapClientException(e.Message));
			}
			throw (new SoapClientException(string.Format(CoreTypeStrings.IllegalType, toType)));
		}

		//-----------------------------------------------------------------------------
		public static object From(string data, string fromType)
		{
			try
			{
				switch (fromType)
				{
					case "String"	: return SoapTypes.FromSoapString(data);
					case "Int64"	: return SoapTypes.FromSoapLong(data);
					case "Int16"	: return SoapTypes.FromSoapShort(data);
					case "Int32"	: return SoapTypes.FromSoapInt(data);
					case "Double"	: return SoapTypes.FromSoapDouble(data);
					case "DateTime"	: return SoapTypes.FromSoapDateTime(data);
					case "DataEnum"	: return SoapTypes.FromSoapDataEnum(data);
					case "Boolean"	: return SoapTypes.FromSoapBoolean(data);
					case "Guid"		: return SoapTypes.FromSoapGuid(data);
					case "DataArray": 
					case "Array"	: 
									return SoapTypes.FromSoapDataArray(data);
					case "Single"	: return SoapTypes.FromSoapFloat(data);
					case "Byte"		: return SoapTypes.FromSoapByte(data);
				}
			}
			catch (FormatException e)
			{
				throw(new SoapClientException(e.Message));
			}

			throw (new SoapClientException(string.Format(CoreTypeStrings.IllegalType, fromType)));
		}
	}

	///=============================================================================
	public class WcfTypes
	{
		//-----------------------------------------------------------------------------
		private static object[] To(DataArray data)
		{ 
			object[] ar = new object[data.Elements.Count];
			for (int i = 0; i < data.Elements.Count; i++)
				ar[i] = To(data.Elements[i]);
			return ar;
			
		}

		//-----------------------------------------------------------------------------
		private static DataArray From(object[] data, string fromBaseType)
		{
			object[] ar = new object[data.Length];
			for (int i = 0; i < data.Length; i++)
				ar[i] = From(data[i], fromBaseType, null);
			DataArray dar = new DataArray(fromBaseType);
			dar.Elements.AddRange(ar);
			return dar;
		}
		//-----------------------------------------------------------------------------
		public static object To(object data)
		{
			string toType = data.GetType().Name;
			try
			{
				switch (toType)
				{
					case "String": 
					case "Int16":
					case "Int64": 
					case "Double":
                    case "Boolean": 
					case "Int32": 
                        return data;

                    case "DateTime": 
                        return XmlConvert.ToString((DateTime)data, @"yyyy-MM-ddTHH\:mm\:ss"); ;

					case "DataEnum": 
                        return (uint)((DataEnum)data);
					
					case "Guid": 
                        return data.ToString();

					case "DataArray": 
                        return To((DataArray)data);

					case "Byte": 
					case "Single": 
                        return data;
				}
			}
			catch (FormatException e)
			{
				throw (new SoapClientException(e.Message));
			}
			throw (new SoapClientException(string.Format(CoreTypeStrings.IllegalType, toType)));
		}

		//-----------------------------------------------------------------------------
		public static object From(object data, string fromType, string fromBaseType)
		{
			try
			{
				switch (fromType)
				{
					case "String":
						return Convert.ToString(data);
					case "Double":
						return Convert.ToDouble(data);
					case "Int64":
						return Convert.ToInt64(data);
					case "Int32":
						return Convert.ToInt32(data);
					case "Int16":
						return Convert.ToInt16(data);
					case "DateTime": return DateTime.Parse((string)data);
                    case "DataEnum": return new DataEnum(ObjectHelper.CastUInt(data));
					case "Boolean":
						return Convert.ToBoolean(data);
					case "Guid": return new Guid((string)(data));
					case "DataArray":
					case "Array":
						return WcfTypes.From((object[])data, fromBaseType);
					case "Byte":
						return Convert.ToByte(data);
					case "Single":
						return Convert.ToSingle(data);
					case "void":
					case "Void":
						return null;
				}
			}
			catch (FormatException e)
			{
				throw (new SoapClientException(e.Message));
			}

			throw (new SoapClientException(string.Format(CoreTypeStrings.IllegalType, fromType)));
		}
	}
	///=============================================================================
	public class SoapClientException : ApplicationException
	{
		public SoapClientException(string message) : base(message) {}
		public SoapClientException(string message, Exception inner) : base(message, inner) {}
	}
}