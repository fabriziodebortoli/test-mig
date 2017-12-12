using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public enum SelectTopType
	{
		Top = 1,
		TopPercent = 2,
		TopWithTies = 3,
	}

	internal enum PredicateType
	{
 		Plain = 0,
		Field = 1,
		Value = 2,
		ForeignKey = 3
	}

	/// <summary>
	/// abstract class to derive from in order to create predicates for customizing query
	/// </summary>
	internal abstract class Predicate
	{
		internal abstract PredicateType ParamType { get; }
		internal abstract JToken ToJSONObject();
	}
	internal class PlainQueryPredicate : Predicate
	{
		public string Query { get; set; }
		internal override PredicateType ParamType
		{
			get { return PredicateType.Plain; }
		}
		internal override JToken ToJSONObject()
		{
			return new JObject(
				 new JProperty("type", ParamType),
				 new JProperty("value", Query)
				);
		}
	}

	internal abstract class ParamPredicate : Predicate
	{
		protected MSqlRecordItem paramField;
		protected string op;

		public ParamPredicate(MSqlRecordItem paramField, string op)
		{
			this.paramField = paramField;
			this.op = op;
		}

		internal override JToken ToJSONObject()
		{
			return new JObject(
				 new JProperty("type", ParamType),
				 new JProperty("field", Field),
				 new JProperty("value", Value),
				 new JProperty("op", op)
				);
		}
		internal virtual string Field { get { return paramField.Name; } }
		internal abstract string Value { get; }
		
	}
	/// <summary>
	/// Predicate that compares a table field to another field, usually belonging to tha master table 
	/// </summary>
	internal class FieldPredicate : ParamPredicate
	{
		protected MSqlRecordItem valueField;
		public FieldPredicate(MSqlRecordItem paramField, MSqlRecordItem valueField, string op="")
			: base(paramField, op)
		{
			this.valueField = valueField;
		}


		internal override PredicateType ParamType
		{
			get { return PredicateType.Field; }
		}

		internal override string Value
		{
			get { return valueField.QualifiedName; }
		}
	}
	/// <summary>
	/// Predicate that define the foreign key thet links two tables
	/// </summary>
	internal class ForeignKeyPredicate : FieldPredicate
	{
		public ForeignKeyPredicate(MSqlRecordItem pkField, MSqlRecordItem fkField)
			: base (pkField, fkField)
		{

		}
		internal override string Value
		{
			get { return valueField.Name; }
		}
		internal override PredicateType ParamType
		{
			get { return PredicateType.ForeignKey; }
		}
	}
	/// <summary>
	/// Predicate that compares a table field to a constant value 
	/// </summary>
	internal class ValuePredicate : ParamPredicate
	{
		protected MDataObj valueField;
		public ValuePredicate(MSqlRecordItem paramField, MDataObj valueField, string op = "")
			: base(paramField, op)
		{
			this.valueField = valueField;
		}
		
		internal override PredicateType ParamType
		{
			get { return PredicateType.Value; }
		}
		internal override string Value
		{
			get { return valueField.FormatDataForXml(); }
		}
	}
	// Summary:
	//     Select object of MSqlTable class
	public class SelectStatement
	{
		public string PlainSelect { get; set; }

		/*public void AddColumn(MDataObj dataObj);
		public void AddColumn(MSqlRecord record, MDataObj dataObj);
		public void AddColumn(string columnName, MDataObj dataObj);
		public void AddColumn(string columnName, MDataObj dataObj, int allocSize);
		public void AddColumn(string columnName, MDataObj dataObj, int allocSize, bool autoIncrement);
		public void AddFunction(string function, MDataObj resDataObj);
		public void AddFunction(string function, MDataObj resDataObj, int allocSize);
		public void AddFunction(string function, MDataObj paramDataObj, MDataObj resDataObj);
		public void AddFunction(string function, MDataObj ParamDataObj, MDataObj resDataObj, int allocSize);
		public void AddFunction(string function, MDataObj ParamDataObj, MDataObj resDataObj, int allocSize, MSqlRecord record);
		public void All();
		public void All(MSqlRecord record);
		public void Distinct();
		public void FromAllTables();
		public void Top(SelectTopType aType, int topValue);*/
	}

	public class FromStatement
	{
		public string PlainFrom { get; set; }

		/*public void AddTable(MSqlRecord record);
		public void AddTable(MSqlRecord record, string alias);
		public void AddTable(string tableName, string alias);
		public void AddTable(string tableName, string alias, MSqlRecord record);*/
	}
	public class WhereStatement
	{
		List<Predicate> predicates = new List<Predicate>();
		PlainQueryPredicate plain = null;
		public void AddCompareColumn(MDataObj paramField, MDataObj valueField, string op = "")
		{
			if (paramField.Owner == null)
				throw new ArgumentException("Parameter does not belong to a MSqlRecord", "paramField");
			Predicate p = valueField.Owner == null
			? (Predicate)new ValuePredicate(paramField.Owner, valueField, op)
			: (Predicate)new FieldPredicate(paramField.Owner, valueField.Owner, op);
			predicates.Add(p);
		}
		public void AddForeignKeyColumn(MDataObj pkField, MDataObj fkField)
		{
			if (pkField.Owner == null)
				throw new ArgumentException("Parameter does not belong to a MSqlRecord", "pkField");
			if (fkField.Owner == null)
				throw new ArgumentException("Parameter does not belong to a MSqlRecord", "fkField");
			predicates.Add(new ForeignKeyPredicate(pkField.Owner, fkField.Owner));
		}

		public string PlainWhere
		{ 
			get { return plain == null ? "" : plain.Query; }
			set { plain = new PlainQueryPredicate { Query = value }; } 
		}
		public bool IsEmpty { get { return predicates.Count == 0 && plain == null; } }
		/*
		public void AddBetweenColumn(MDataObj dataObj);
		public void AddBetweenColumn(string columnName);
		public void AddBetweenColumn(MSqlRecord record, MDataObj dataObj);
		public void AddColumn(MDataObj dataObj);
		public void AddColumn(string columnName);
		public void AddColumn(MDataObj dataObj, string strOperator);
		public void AddColumn(MSqlRecord record, MDataObj dataObj);
		public void AddColumn(string columnName, string strOperator);
		public void AddColumn(MSqlRecord record, MDataObj dataObj, string strOperator);
		public void AddColumnLike(MDataObj dataObj);
		public void AddColumnLike(string columnName);
		public void AddCompareColumn(MDataObj dataObj, MSqlRecord record, MDataObj compareDataObj);
		public void AddCompareColumn(MDataObj dataObj, MSqlRecord record, MDataObj compareDataObj, string strOperator);
		public void AddCompareColumn(MSqlRecord record1, MDataObj compareDataObj1, MSqlRecord record2, MDataObj compareDataObj2);
		public void AddCompareColumn(MSqlRecord record1, MDataObj compareDataObj1, MSqlRecord record2, MDataObj compareDataObj2, string strOperator);
		public void AddParameter(string paramName, MDataObj dataObj, string columnName);
		public void Parameter(string paramName, MDataObj value);
		public void ParameterLike(string paramName, MDataObj dataObj);*/

		internal JToken ToJSONObject()
		{
			JArray parms = new JArray();
			foreach (Predicate p in predicates)
				parms.Add(p.ToJSONObject());
			if (plain != null)
				parms.Add(plain);
			return parms;
		}
	}
	public class GroupByStatement
	{
		public string PlainGroupBy { get; set; }

		/*public void AddColumn(MDataObj dataObj);
		public void AddColumn(string columnName);
		public void AddColumn(MSqlRecord record, MDataObj dataObj);*/
	}

	public class HavingStatement
	{
		public string PlainHaving { get; set; }

		/*public void AddBetweenColumn(MDataObj dataObj);
		public void AddBetweenColumn(string columnName);
		public void AddBetweenColumn(MSqlRecord record, MDataObj dataObj);
		public void AddColumn(MDataObj dataObj);
		public void AddColumn(string columnName);
		public void AddColumn(MDataObj dataObj, string strOperator);
		public void AddColumn(MSqlRecord record, MDataObj dataObj);
		public void AddColumn(string columnName, string strOperator);
		public void AddColumn(MSqlRecord record, MDataObj dataObj, string strOperator);
		public void AddColumnLike(MDataObj dataObj);
		public void AddColumnLike(string columnName);
		public void AddCompareColumn(MDataObj dataObj, MSqlRecord record, MDataObj compareDataObj);
		public void AddCompareColumn(MDataObj dataObj, MSqlRecord record, MDataObj compareDataObj, string strOperator);
		public void AddCompareColumn(MSqlRecord record1, MDataObj compareDataObj1, MSqlRecord record2, MDataObj compareDataObj2);
		public void AddCompareColumn(MSqlRecord record1, MDataObj compareDataObj1, MSqlRecord record2, MDataObj compareDataObj2, string strOperator);*/
	}

	public class OrderByStatement
	{
		public string PlainOrderBy { get; set; }

		/*public void AddColumn(MDataObj dataObj);
		public void AddColumn(string columnName);
		public void AddColumn(MDataObj dataObj, bool descending);
		public void AddColumn(MSqlRecord record, MDataObj dataObj);
		public void AddColumn(string columnName, bool descending);
		public void AddColumn(MSqlRecord record, MDataObj dataObj, bool descending);*/
	}
}
