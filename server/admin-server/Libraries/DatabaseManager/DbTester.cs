using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Libraries.DatabaseManager
{
	//============================================================================
	public class DbTester
	{
		private TBConnection myConnection = null;

		//---------------------------------------------------------------------------
		public void Run()
		{
			CreateDatabase();
		}

		//---------------------------------------------------------------------------
		private void CreateDatabase()
		{
			/*string encryptPw = Crypto.Encrypt("14");
			string decrypt = Crypto.Decrypt(encryptPw);*/

			ContextInfo.SystemDBConnectionInfo cp = new ContextInfo.SystemDBConnectionInfo();
			cp.DBName = "SystemDb4_Debug";
			cp.ServerName = "USR-DELBENEMIC";
			cp.Instance = string.Empty;
			cp.UserId = "sa";
			cp.Password = "14";

			Diagnostic dbTesterDiagnostic = new Diagnostic("DbTester");

			PathFinder pf = new PathFinder("USR-DELBENEMIC", "Development", "WebMago", "sa");
			pf.Edition = "Professional"; // 

			// creazione tabelle per il database aziendale
			DatabaseManager dbManager = new DatabaseManager
				(
				pf,
				dbTesterDiagnostic,
				(BrandLoader)InstallationData.BrandLoader,
				cp,
				DBNetworkType.Large,
				"IT",
				false // no ask credential
				);
			
			string lastCompanyId = "3";
			// mi connetto alla company appena creata (con l'Id appena inserito nella MSD_Companies)
			if (dbManager.ConnectAndCheckDBStructure(lastCompanyId))
			{
				dbManager.ImportDefaultData = true;
				dbManager.ImportSampleData = false;
				Debug.WriteLine("Start database creation: " + DateTime.Now.ToString("hh:mm:ss.fff"));
				bool result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
				Debug.WriteLine("End database creation: " + DateTime.Now.ToString("hh:mm:ss.fff"));
			}
		}

		//---------------------------------------------------------------------------
		private void Query()
		{
			
			var queryResults = new List<Dictionary<string, object>>();

			using (TBCommand myCommand = new TBCommand(myConnection))
			{
				myCommand.CommandText = "SELECT [Application], [AddOnModule] AS Module, [DBRelease] AS Release FROM [TB_DBMark] WHERE [Application] = 'TB'";

				using (IDataReader myReader = myCommand.ExecuteReader())
				{
					while (myReader.Read())
						queryResults.Add(Enumerable.Range(0, myReader.FieldCount).ToDictionary(myReader.GetName, myReader.GetValue));
				}
			}


			foreach (Dictionary<string, object> dict in queryResults)
			{
				foreach (KeyValuePair<string, object> item in dict)
				{
					Debug.WriteLine(item.Key + " " + item.Value);
				}
			}

		}

		//---------------------------------------------------------------------------
		private bool Open()
		{
			myConnection = new TBConnection(TaskBuilderNetCore.Interfaces.DBMSType.SQLSERVER);
			myConnection.ConnectionString = string.Format(TaskBuilderNetCore.Data.NameSolverDatabaseStrings.SQLConnection, "USR-DELBENEMIC", "SystemDB4_Debug", "sa", "14");
			try
			{
				myConnection.Open();
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
			return false;
		}

		//---------------------------------------------------------------------------
		private void Close()
		{
			if (myConnection != null && myConnection.State != System.Data.ConnectionState.Closed)
			{
				myConnection.Close();
				myConnection.Dispose();
			}
		}

		//---------------------------------------------------------------------------
		private List<Dictionary<string, object>> LoadDictionary()
		{
			List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
			string query = "select TABLE_NAME as name, '' as routineType, '' as definition from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";
			try
			{
				using (SqlCommand command = new SqlCommand(query, myConnection.SqlConnect))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
							results.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
					}
				}
			}
			catch (SqlException e)
			{
				throw (new TBException(e.Message, e));
			}

			return results;
		}
	}

	#region TBDictionary
	//============================================================================
	public class TBDictionary : IDictionary<string, object>
	{
		//---------------------------------------------------------------------------
		public object this[string key]
		{
			get { return this[key]; }
			set { this[key] = value; }
		}

		//---------------------------------------------------------------------------
		public int Count { get { return this.Count(); } }

		//---------------------------------------------------------------------------
		public bool IsReadOnly { get { return this.IsReadOnly; } }

		//---------------------------------------------------------------------------
		public ICollection<string> Keys { get { return this.Keys; } }

		//---------------------------------------------------------------------------
		public ICollection<object> Values { get { return this.Values; } }

		//---------------------------------------------------------------------------
		public void Add(KeyValuePair<string, object> item)
		{
			this.Add(item);
		}

		//---------------------------------------------------------------------------
		public void Add(string key, object value)
		{
			this.Add(key, value);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			this.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(KeyValuePair<string, object> item)
		{
			return this.Contains(item);
		}

		//---------------------------------------------------------------------------
		public bool ContainsKey(string key)
		{
			return this.ContainsKey(key);
		}

		//---------------------------------------------------------------------------
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			this.CopyTo(array, arrayIndex);
		}

		//---------------------------------------------------------------------------
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return this.GetEnumerator();
		}

		//---------------------------------------------------------------------------
		public bool Remove(KeyValuePair<string, object> item)
		{
			return this.Remove(item);
		}

		//---------------------------------------------------------------------------
		public bool Remove(string key)
		{
			return this.Remove(key);
		}

		//---------------------------------------------------------------------------
		public bool TryGetValue(string key, out object value)
		{
			return this.TryGetValue(key, out value);
		}

		//---------------------------------------------------------------------------
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
	#endregion

	#region TBDataTable
	//---------------------------------------------------------------------------
	public class TBDataTable : IList<TBDictionary>
	{
		public TBDictionary this[int index]
		{
			get { return this[index]; }
			set { this[index] = value; }
		}

		public int Count { get { return this.Count; } }

		public bool IsReadOnly { get { return this.IsReadOnly; } }

		public void Add(TBDictionary item)
		{
			this.Add(item);
		}

		public void Clear()
		{
			this.Clear();
		}

		public bool Contains(TBDictionary item)
		{
			return this.Contains(item);
		}

		public void CopyTo(TBDictionary[] array, int arrayIndex)
		{
			this.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TBDictionary> GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public int IndexOf(TBDictionary item)
		{
			return this.IndexOf(item);
		}

		public void Insert(int index, TBDictionary item)
		{
			this.Insert(index, item);
		}

		public bool Remove(TBDictionary item)
		{
			return this.Remove(item);
		}

		public void RemoveAt(int index)
		{
			this.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
	#endregion
}
