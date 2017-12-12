using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for SQLField.
	/// </summary>
	//================================================================================
	public class SQLField
	{
		private bool mOracle = false;

		IRecordField field;

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public SQLField(IRecordField field)
		{
			this.field = field;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool Oracle { set { mOracle = value; } }

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Type { get { return GetFieldType(); } }

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string DefaultValue { get { return "(" + GetDefaultValueLiteral() + ")"; } }
	
		//--------------------------------------------------------------------------------
		private string GetUpdateScriptOracle(string tableName, string fieldName)
		{
			return "UPDATE \"" + tableName.ToUpper() + "\" SET \"" + fieldName.ToUpper() +
				"\" = " + GetDefaultValueLiteral() + " WHERE \"" + fieldName.ToUpper() + "\" IS NULL";
		}

		//--------------------------------------------------------------------------------
		private string GetUpdateScript(string tableName, string fieldName)
		{
			return "UPDATE [dbo].[" + tableName + "] SET [dbo].[" + tableName + "].[" +
								 fieldName + "] = " + GetDefaultValueLiteral() + " WHERE [dbo].[" + tableName + "].[" + fieldName + "] IS NULL"; ;
		}

		//--------------------------------------------------------------------------------
		private string GetDefaultValueLiteral()
		{
			string s = GetTypeString();

			if (s == DataType.DataTypeStrings.Money)
			{
				return GetFieldDefaultValue().Replace(",", ".");
			}
			
			bool addApex = s == DataType.DataTypeStrings.String ||
				s == DataType.DataTypeStrings.Text ||
				s == DataType.DataTypeStrings.Uuid ||
				s == DataType.DataTypeStrings.Bool;

			return addApex ? string.Format("'{0}'", GetFieldDefaultValue()) : GetFieldDefaultValue();
		}

		//--------------------------------------------------------------------------------
		private string GetFieldType()
		{
			string sType = GetTypeString();
			switch (sType)
			{
                case DataType.DataTypeStrings.Uuid:
                    if (mOracle)
                        return "CHAR(38)";
                    else
                        return "[uniqueidentifier]";
                case DataType.DataTypeStrings.Bool:
					if (mOracle)
						return "CHAR (1)";
					else
						return "[char] (1)";

				case DataType.DataTypeStrings.Date:
				case DataType.DataTypeStrings.DateTime:
				case DataType.DataTypeStrings.Time:
					if (mOracle)
						return "DATE";
					else
						return "[datetime]";

				case DataType.DataTypeStrings.Float:
				case DataType.DataTypeStrings.Double:
					if (mOracle)
						return "FLOAT (126)";
					else
						return "[float]";

				case DataType.DataTypeStrings.Enum:
					if (mOracle)
						return "NUMBER (10)";
					else
						return "[int]";

				case DataType.DataTypeStrings.Integer:
					if (mOracle)
						return "NUMBER (6)";
					else
						return "[smallint]"
							;
				case DataType.DataTypeStrings.ElapsedTime:
				case DataType.DataTypeStrings.Long:
					if (mOracle)
						return "NUMBER (10)";
					else
						return "[int]";

				case DataType.DataTypeStrings.Money:
					if (mOracle)
						return "FLOAT (126)";
					else
						return "[float]";

				case DataType.DataTypeStrings.Percent:
					if (mOracle)
						return "FLOAT (126)";
					else
						return "[float]";
				case DataType.DataTypeStrings.Quantity:
					if (mOracle)
						return "FLOAT (126)";
					else
						return "[float]";

				case DataType.DataTypeStrings.String:
					if (mOracle)
						return "VARCHAR2 (" + field.Length.ToString() + ")";
					else
						return "[varchar] (" + field.Length.ToString() + ")";
				case DataType.DataTypeStrings.Text:
					if (mOracle)
						return "clob";
					else
						return "[text]";
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------
		private string GetTypeString()
		{
			return field.DataObjType.IsEnum ? DataType.DataTypeStrings.Enum : field.DataObjType.ToString();
		}

		//--------------------------------------------------------------------------------
		private string GetFieldDefaultValue()
		{
			//allineo il campo del valore con quello del default
			//il campo del valore lo utilizzo per comodità, per non dover istanziare un altro dataobj
			((MDataObj)field.DataObj).StringValue = field.DefaultValue;
			switch (GetTypeString())
			{
				case DataType.DataTypeStrings.Bool:
					{
						MDataBool aBool = (MDataBool)field.DataObj;
						return (aBool ? "1" : "0");
					}
				case DataType.DataTypeStrings.Date:
				case DataType.DataTypeStrings.DateTime:
				case DataType.DataTypeStrings.Time:
					{
						MDataDate aDate = (MDataDate)field.DataObj;
						string aGG = aDate.Value.Day.ToString("00");
						string aMM = aDate.Value.Month.ToString("00");
						string aAA = aDate.Value.Year.ToString("0000");

						if (mOracle)
							return "TO_DATE('" + aGG + "-" + aMM + "-" + aAA + "','DD-MM-YYYY')";
						else
							return "'" + aAA + aMM + aGG + "'";
					}
				default:
					return field.DataObj.Value.ToString();
			}
		}


		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string UpdateScript
		{
			get
			{
				return mOracle
					? GetUpdateScriptOracle(field.Record.Name, field.Name)
					: GetUpdateScript(field.Record.Name, field.Name);
			}
		}
	}
}
