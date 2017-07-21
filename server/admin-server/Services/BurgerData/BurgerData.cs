﻿using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services.BurgerData
{

	public class BurgerData
	{
		string connectionString;

		public BurgerData(string connectionString)
		{
			this.connectionString = connectionString;
		}

		//--------------------------------------------------------------------------------
		public I GetObject<T, I>(string command, ModelTables table)
		{
			return GetObject<T, I>(command, table, null, null);
		}

		public I GetObject<T, I>(string command, ModelTables table, SqlLogicOperators? sqlLogicOperator, params WhereCondition[] conditions)
		{
			IModelObject obj = ModelFactory.CreateModelObject<T, I>();

			IDBManager dbManager = new DBManager(DataProvider.SqlClient, this.connectionString);

			string tableName = SqlScriptManager.GetTableName(table);

			if (String.IsNullOrEmpty(command))
				command = conditions != null ?
					CreateSelectFromSelectParameters(conditions, sqlLogicOperator, tableName) :
					CreateSelectFromSelectParameters(sqlLogicOperator, tableName);

			if (String.IsNullOrEmpty(command))
				return default(I);

			bool found = false;

			try
			{
				dbManager.Open();
				dbManager.ExecuteReader(System.Data.CommandType.Text, command);

				while (dbManager.DataReader.Read())
				{
					found = true;
					obj = obj.Fetch(dbManager.DataReader);
				}
			}
			catch (SqlException)
			{
				// todo: log
			}
			finally
			{
				dbManager.Close();
				dbManager.Dispose();
			}

			return found ? (I)obj : default(I);
		}

		//--------------------------------------------------------------------------------
		public List<I> GetList<T, I>(string command, ModelTables table)
		{
			return GetList<T, I>(command, table, null, null);
		}

		public List<I> GetList<T, I>(string command, ModelTables table, SqlLogicOperators? sqlLogicOperator, params WhereCondition[] conditions)
		{
			List<I> innerList = new List<I>();
			IModelObject obj = ModelFactory.CreateModelObject<T, I>();

			IDBManager dbManager = new DBManager(DataProvider.SqlClient, this.connectionString);
			string tableName = SqlScriptManager.GetTableName(table);

			if (String.IsNullOrEmpty(command))
				command = conditions != null ?
					CreateSelectFromSelectParameters(conditions, sqlLogicOperator, tableName) :
					CreateSelectFromSelectParameters(sqlLogicOperator, tableName);

			if (String.IsNullOrEmpty(command))
				return innerList;

			try
			{
				dbManager.Open();
				dbManager.ExecuteReader(System.Data.CommandType.Text, command);
				while (dbManager.DataReader.Read())
					innerList.Add((I)obj.Fetch(dbManager.DataReader));
			}
			catch (SqlException)
			{
				// todo log
			}
			finally
			{
				dbManager.Close();
				dbManager.Dispose();
			}

			return innerList;
		}

		//--------------------------------------------------------------------------------
		private static string CreateSelectFromSelectParameters(SqlLogicOperators? sqlLogicOperator, string tableName)
		{
			return CreateSelectFromSelectParameters(new WhereCondition[] { }, sqlLogicOperator, tableName);
		}

		//--------------------------------------------------------------------------------
		private static string CreateSelectFromSelectParameters(WhereCondition[] conditions,
			SqlLogicOperators? sqlLogicOperator, string tableName)
		{
			SelectScript selectScript = new SelectScript(tableName);
			selectScript.AddSelectField("*");

			if (sqlLogicOperator != null)
				selectScript.LogicOperatorForAllParameters = (SqlLogicOperators)sqlLogicOperator;

			if (conditions == null)
				return selectScript.ToString();

			foreach (WhereCondition condition in conditions)
				selectScript.AddWhereParameter(
					condition.FieldName,
					condition.Val,
					condition.ComparingOperator,
					condition.IsNumber);

			return selectScript.ToString();
		}
	}

	public class SqlScriptManager
	{
		public static string GetTableName(ModelTables table)
		{
			switch (table)
			{
				case ModelTables.Accounts:
					return "MP_Accounts";
				default:
					return String.Empty;
			}
		}

		//--------------------------------------------------------------------------------
		public static string GetOperatorText(QueryComparingOperators comparingOperator)
		{
			switch (comparingOperator)
			{
				case QueryComparingOperators.IsEqual:
					return " = ";
				case QueryComparingOperators.IsNotEqual:
					return " != ";
				case QueryComparingOperators.IsGreater:
					return " > ";
				case QueryComparingOperators.IsGreaterOrEqual:
					return " >= ";
				case QueryComparingOperators.IsSmaller:
					return " < ";
				case QueryComparingOperators.IsSmallerOrEqual:
					return " <= ";
				case QueryComparingOperators.Like:
					return " LIKE ";
				default:
					break;
			}

			return String.Empty;
		}
	}

	//================================================================================
	public class SelectScript
	{
		List<ParamCouple> whereParameters;
		Hashtable fields;
		string tableName;
		SqlLogicOperators logicOperatorForAllParameters;
		bool useDistinctClause;

		public bool UseDistinctClause
		{
			get { return this.useDistinctClause; }
			set { this.useDistinctClause = value; }
		}

		public SqlLogicOperators LogicOperatorForAllParameters
		{
			get { return this.logicOperatorForAllParameters; }
			set { this.logicOperatorForAllParameters = value; }
		}

		//--------------------------------------------------------------------------------
		public SelectScript(string tableName)
		{
			whereParameters = new List<ParamCouple>();
			fields = new Hashtable();
			this.tableName = tableName;
			this.logicOperatorForAllParameters = SqlLogicOperators.AND;
		}

		//--------------------------------------------------------------------------------
		public void AddWhereParameter(string name, object val, QueryComparingOperators comparingOperator, bool isNumber)
		{
			string mask = isNumber ? "{0}{1}" : "{0}'{1}'";
			whereParameters.Add(new ParamCouple(name, String.Format(mask,
				SqlScriptManager.GetOperatorText(comparingOperator), val)));
		}

		//--------------------------------------------------------------------------------
		public void AddSelectField(string name)
		{
			fields.Add(name, name);
		}

		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder strWhereParams = new StringBuilder();
			StringBuilder strSelectField = new StringBuilder();
			IDictionaryEnumerator enumInterface;
			bool isFirst = true;
			string logicOperator = String.Format(" {0} ", this.logicOperatorForAllParameters.ToString());
			string param;

			whereParameters.ForEach(p =>
			{
				param = String.Concat(p.ParamName, p.ParamValue);

				if (isFirst)
				{
					strWhereParams.Append(param);
					isFirst = false;
				}
				else
					strWhereParams.Append(String.Concat(logicOperator, param));
			});

			enumInterface = fields.GetEnumerator();
			isFirst = true;

			while (enumInterface.MoveNext())
			{
				string field = enumInterface.Value.ToString();

				if (isFirst)
				{
					strSelectField.Append(field);
					isFirst = false;
				}
				else
					strSelectField.Append(String.Concat(",", field));
			}

			string selectMask = useDistinctClause ?
				"SELECT DISTINCT {0} FROM {1} [WHERE] {2}" : "SELECT {0} FROM {1} [WHERE] {2}";

			selectMask = (strWhereParams.Length == 0) ?
				selectMask.Replace("[WHERE]", String.Empty) :
				selectMask.Replace("[WHERE]", "WHERE");

			return String.Format(selectMask,
				strSelectField, tableName, strWhereParams).Trim();
		}
	}

	//================================================================================
	public class ParamCouple
	{
		string paramName;
		string paramValue;
		bool isNumber;

		public string ParamName { get { return this.paramName; } }
		public string ParamValue { get { return this.paramValue; } }
		public bool IsNumber { get { return this.isNumber; } }

		public ParamCouple(string name, string value, bool isNumber)
		{
			this.paramName = name;
			this.paramValue = value;
			this.isNumber = isNumber;
		}

		public ParamCouple(string name, string value) : this(name, value, false)
		{
		}
	}

	public sealed class DBManager : IDBManager, IDisposable
	{
		DataProvider providerType;
		string connectionString;
		IDbConnection connection;
		IDbTransaction transaction = null;
		IDataReader dataReader;
		IDbCommand command;
		IDbDataParameter[] parameters;

		public IDataReader DataReader
		{
			get { return this.dataReader; }
			set { this.dataReader = value; }
		}

		public DataProvider ProviderType
		{
			get { return this.providerType; }
			set { this.providerType = value; }
		}

		public string ConnectionString
		{
			get { return this.connectionString; }
			set { this.connectionString = value; }
		}

		public IDbCommand Command
		{
			get { return this.command; }
		}

		public IDbTransaction Transaction
		{
			get { return this.transaction; }
			set { this.transaction = value; }
		}

		public IDbDataParameter[] Parameters
		{
			get { return this.parameters; }
		}

		public DBManager() { }

		public DBManager(DataProvider providerType)
		{
			this.providerType = providerType;
		}

		public DBManager(DataProvider providerType, string
			connectionString)
		{
			this.providerType = providerType;
			this.connectionString = connectionString;
		}

		public void Open()
		{
			this.connection = DBManagerFactory.GetConn(this.providerType);
			this.connection.ConnectionString = this.connectionString;
			if (connection.State != ConnectionState.Open)
			{
				try
				{
					connection.Open();
				}
				catch
				{
					// Sometimes the attempt to open connection fails stating that
					// "the connection property has not been initialized".
					// It happens when we're closing the application.
					// Here we try to definitively close it and exit.
					connection.Dispose();
					return;
				}
			}
			this.command = DBManagerFactory.GetCommand(this.providerType);
		}

		public void Close()
		{
			if (connection == null)
				return;

			if (connection.State != ConnectionState.Closed)
				connection.Close();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			this.Close();
			this.command = null;
			this.transaction = null;
			this.connection = null;
		}

		public void CreateParameters(int paramCount)
		{
			parameters = new IDbDataParameter[paramCount];
			parameters = DBManagerFactory.GetParameters(this.providerType,
				paramCount);
		}

		public void AddParameters(int index, string paramName, object objValue)
		{
			string pName = paramName;

			if (index < parameters.Length)
			{
				parameters[index].ParameterName = pName;
				parameters[index].Value = objValue;
			}
		}

		public IDataReader ExecuteReader(CommandType commandType,
			string cmdText)
		{
			this.command = DBManagerFactory.GetCommand(this.providerType);
			this.command.Connection = this.connection;
			PrepareCommand(this.command, this.connection, this.transaction,
				commandType, cmdText, this.parameters);
			this.dataReader = command.ExecuteReader();
			this.command.Parameters.Clear();
			return this.dataReader;
		}

		public void CloseReader()
		{
			if (this.DataReader != null)
				this.DataReader.Close();
		}

		static void AttachParameters(IDbCommand command, IDbDataParameter[] commandParameters)
		{
			foreach (IDbDataParameter param in commandParameters)
			{
				if (param.Direction == ParameterDirection.InputOutput &&
					param.Value == null)
					param.Value = DBNull.Value;

				command.Parameters.Add(param);
			}
		}

		void PrepareCommand(IDbCommand command, IDbConnection connection,
			IDbTransaction transaction, CommandType commandType,
			string commandTxt, IDbDataParameter[] commandParameters)
		{
			command.Connection = connection;
			command.CommandText = commandTxt;
			command.CommandType = commandType;

			if (transaction != null)
				command.Transaction = transaction;

			if (commandParameters != null)
				AttachParameters(command, commandParameters);
		}

		public int ExecuteNonQuery(CommandType commandType, string commandText)
		{
			this.command = DBManagerFactory.GetCommand(this.providerType);
			PrepareCommand(command, this.connection, this.transaction,
				commandType, commandText, this.parameters);
			int retValue = command.ExecuteNonQuery();
			command.Parameters.Clear();
			return retValue;
		}

		public object ExecuteScalar(CommandType commandType, string commandText)
		{
			this.command = DBManagerFactory.GetCommand(this.providerType);
			PrepareCommand(command, this.connection, this.transaction,
				commandType, commandText, this.parameters);
			object retValue = command.ExecuteScalar();
			this.command.Parameters.Clear();
			return retValue;
		}

		public void BeginTransaction()
		{
			if (this.transaction == null)
				transaction = DBManagerFactory.GetTransaction(this.ProviderType, this.connection);
			this.command.Transaction = transaction;
		}

		public void CommitTransaction()
		{
			if (this.transaction != null)
				this.transaction.Commit();
			transaction = null;
		}
	}

	public class ModelFactory
	{
		//--------------------------------------------------------------------------------
		public static IModelObject CreateModelObject<T, I>()
		{
			return (IModelObject)Activator.CreateInstance<T>();
		}
	}

	public sealed class DBManagerFactory
	{
		public static IDbConnection GetConn(DataProvider provider)
		{
			IDbConnection iDBConn = null;

			switch (provider)
			{
				case DataProvider.SqlClient:
					iDBConn = new SqlConnection();
					break;
				default:
					break;
			}

			return iDBConn;
		}

		public static IDbCommand GetCommand(DataProvider provider)
		{
			switch (provider)
			{
				case DataProvider.SqlClient:
					return new SqlCommand();
				default:
					return null;
			}
		}

		public static IDbTransaction GetTransaction(DataProvider provider)
		{
			IDbConnection conn = GetConn(provider);
			IDbTransaction trans = conn.BeginTransaction();
			return trans;
		}

		public static IDbTransaction GetTransaction(DataProvider provider, IDbConnection conn)
		{
			IDbTransaction trans = conn.BeginTransaction();
			return trans;
		}

		public static IDataParameter GetParameter(DataProvider provider)
		{
			IDataParameter iDataParameter = null;

			switch (provider)
			{
				case DataProvider.SqlClient:
					iDataParameter = new SqlParameter();
					break;
				default:
					break;
			}

			return iDataParameter;
		}

		public static IDbDataParameter[] GetParameters(DataProvider providerType,
			int paramsCount)
		{
			IDbDataParameter[] idbParams = new IDbDataParameter[paramsCount];

			switch (providerType)
			{
				case DataProvider.SqlClient:
					for (int i = 0; i < paramsCount; ++i)
						idbParams[i] = new SqlParameter();
					break;
				default:
					idbParams = null;
					break;
			}
			return idbParams;
		}
	}

	public interface IDBManager : IDisposable
	{
		string ConnectionString { get; set; }
		//IDbConnection Connection { get; set; }
		IDbTransaction Transaction { get; set; }
		IDataReader DataReader { get; }
		IDbCommand Command { get; }
		IDbDataParameter[] Parameters { get; }

		void Open();
		void BeginTransaction();
		void CommitTransaction();
		void CreateParameters(int count);
		void AddParameters(int idx, string name, object value);
		IDataReader ExecuteReader(CommandType cmdType, string cmdText);
		int ExecuteNonQuery(CommandType cmdType, string cmdText);
		void CloseReader();
		void Close();
		//void Dispose();
	}

	//================================================================================

	public enum DataProvider
	{
		SqlClient,
		Unknown
	}

	public class WhereCondition
	{
		string fieldName;
		object val;
		QueryComparingOperators comparingOperator;
		bool isNumber;

		public string FieldName { get { return this.fieldName; } }
		public object Val { get { return this.val; } }
		public QueryComparingOperators ComparingOperator { get { return this.comparingOperator; } }
		public bool IsNumber { get { return this.isNumber; } }

		//--------------------------------------------------------------------------------
		public WhereCondition(string fieldName, object val,
			QueryComparingOperators comparingOperator, bool isNumber)
		{
			this.fieldName = fieldName;
			this.val = val;
			this.comparingOperator = comparingOperator;
			this.isNumber = isNumber;
		}

		//--------------------------------------------------------------------------------
		public string GetTextCommand()
		{
			if (String.IsNullOrEmpty(this.fieldName) ||
				this.val == null)
			{
				return String.Empty;
			}

			string whereMask;

			if (this.isNumber)
			{
				whereMask = "{0} {1} {2}";
			}
			else
			{
				if (this.comparingOperator == QueryComparingOperators.Like)
				{
					whereMask = "{0} {1} '%{2}%'";
				}
				else
				{
					whereMask = "{0} {1} '{2}'";
				}
			}

			return String.Format(
				whereMask,
				this.fieldName,
				SqlScriptManager.GetOperatorText(this.comparingOperator),
				this.val);
		}
	}

	public enum SqlLogicOperators
	{
		AND,
		OR
	}

	public enum QueryComparingOperators
	{
		IsEqual,
		IsNotEqual,
		IsGreater,
		IsSmaller,
		IsGreaterOrEqual,
		IsSmallerOrEqual,
		Like
	}

	public enum ModelTables
	{
		None,
		Accounts,
		Subscriptions
	}
}
