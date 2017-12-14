using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// OperationTraced.
	/// Funzioni per selezionare/visualizzare il contenuto della tabella
	/// MSD_Trace (popolata dalla MConsole e dal LoginManager)
	/// </summary>
	// ========================================================================
	public class OperationTracedDb : DataBaseItem
	{
		/// <summary>
		/// OperationTracedDb
		/// </summary>
		//---------------------------------------------------------------------
		public OperationTracedDb(){}

		/// <summary>
		/// OperationTracedDb
		/// </summary>
		/// <param name="connectionString"></param>
		//---------------------------------------------------------------------
		public OperationTracedDb(string connectionString)
		{
			ConnectionString = connectionString;
		}

		/// <summary>
		/// SelectedTraced
		/// Selezione di uno o più records presente nella tabella MSD_Trace
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectedTraced
			(
				out ArrayList	recordsTraced, 
				string			allString, 
				string			company, 
				string			user, 
				TraceActionType operationType, 
				DateTime		fromDate, 
				DateTime		toDate
			)
		{
			SqlDataReader myDataReader   = null;
			ArrayList localRecordsTraced = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				if (GetRecordsTraced(out myDataReader, allString, company, user, operationType, fromDate, toDate))
				{
					while(myDataReader.Read())
					{
						TracedItem tracedItem            = new TracedItem();
						tracedItem.CompanyName           = myDataReader["Company"].ToString();
						tracedItem.LoginName			 = myDataReader["Login"].ToString();
						tracedItem.OperationDate         = (DateTime)myDataReader["Data"];
						tracedItem.OperationType         = Convert.ToUInt16(myDataReader["Type"].ToString());
						tracedItem.ProcessName           = myDataReader["ProcessName"].ToString();
						tracedItem.WinUser               = myDataReader["WinUser"].ToString();
						tracedItem.Location              = myDataReader["Location"].ToString();
						localRecordsTraced.Add(tracedItem);
					}
					myDataReader.Close();
					mySuccessFlag = true;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "OperationTracedDb.SelectedTraced");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.OperationTracedReading, extendedInfo);
				mySuccessFlag = false;
				if (myDataReader != null)
					myDataReader.Close();
			}
			
			recordsTraced = localRecordsTraced;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetRecordsTraced
		/// Esegue la query
		/// </summary>
		//---------------------------------------------------------------------
		public bool GetRecordsTraced
			(
				out SqlDataReader	myDataReader, 
				string				allString, 
				string				company, 
				string				user, 
				TraceActionType		operationType, 
				DateTime			fromDate, 
				DateTime			toDate
			)
		{
			SqlDataReader mylocalDataReader = null;

			string myQueryOperation = "SELECT * FROM MSD_Trace WHERE (( Data >= @FromDate AND Data <= @ToDate))";
			string addOperationType = " AND Type = @OperationType";
			string addCompany       = " AND Company = @Company";
			string addUser			= " AND Login = @Login";
			
			bool mySuccessFlag = false;

			try
			{
				if (operationType != TraceActionType.All)
					myQueryOperation = myQueryOperation + addOperationType;
				if (string.Compare(company, allString, true, CultureInfo.InvariantCulture) != 0)
					myQueryOperation = myQueryOperation + addCompany;
				if (string.Compare(user, allString, true, CultureInfo.InvariantCulture) != 0)
					myQueryOperation = myQueryOperation + addUser;

				SqlCommand myCommand = new SqlCommand(myQueryOperation, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@FromDate", fromDate.ToString(@"yyyy-MM-ddT00\:00\:00"));
				myCommand.Parameters.AddWithValue("@ToDate", toDate.ToString(@"yyyy-MM-ddT23\:59\:00"));
				
				if (operationType != TraceActionType.All)
					myCommand.Parameters.Add("@OperationType", SqlDbType.SmallInt, 2, "Type").Value = Convert.ToUInt16(operationType);
				if (string.Compare(company, allString, true, CultureInfo.InvariantCulture) != 0)
					myCommand.Parameters.AddWithValue("@Company", company);
				if (string.Compare(user, allString, true, CultureInfo.InvariantCulture) != 0)
					myCommand.Parameters.AddWithValue("@Login", user);

				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
				if (myCommand != null)
					myCommand.Dispose();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "OperationTracedDb.GetRecordsTraced");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");

				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.OperationTracedReading, extendedInfo);
				mySuccessFlag = false;
			}
			catch(Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}

		/// <summary>
		/// DeleteToDateRecordsTraces
		/// Cancellazione dei record (fino a una certa data)
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteToDateRecordsTraces(DateTime toDate)
		{
			SqlDataReader	mylocalDataReader = null;
			SqlCommand		myCommand = null;

			bool mySuccessFlag = true;

			string myQueryOperation = "DELETE FROM MSD_Trace WHERE Data <= @ToDate";

			try
			{
				myCommand = new SqlCommand(myQueryOperation, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ToDate", toDate.ToString(@"yyyy-MM-ddT23\:59\:00"));
				int rowsAffected = myCommand.ExecuteNonQuery();

				if (rowsAffected > 0)
					Diagnostic.Set(DiagnosticType.Information, DatabaseItemsStrings.RecordsTracedDeleated);
				else
				{
					Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.NoneRecordsTracedDeleated);
					mySuccessFlag = false;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "OperationTracedDb.DeleteToDateRecordsTraces");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.OperationTracedDeleting, extendedInfo);
				mySuccessFlag = false;
			}
			finally
			{
				if (myCommand != null)
					myCommand.Dispose();
				if (mylocalDataReader != null)
					mylocalDataReader.Close();
			}
			return mySuccessFlag;
		}
	}

	//=========================================================================
	public class TracedItem
	{
		#region Variabili membro (private)
		private string		companyName		= string.Empty;
		private string		loginName		= string.Empty;
		private DateTime	operationDate	= DateTime.Now;
		private int			operationType	= 0;
		private string		processName		= string.Empty;
		private string		winUser			= string.Empty;
		private string		location		= string.Empty;
		#endregion

		#region Proprietà
		//properties
		//---------------------------------------------------------------------
		public string	CompanyName		{ get { return companyName;		} set { companyName		= value; } }
		public string	LoginName		{ get { return loginName;		} set { loginName		= value; } }
		public DateTime OperationDate	{ get { return operationDate;	} set { operationDate	= value; }}
		public int		OperationType	{ get { return operationType;	} set { operationType	= value; }}
		public string	ProcessName		{ get { return processName;		} set { processName		= value; } }
		public string	WinUser			{ get { return winUser;			} set { winUser			= value; } }
		public string	Location		{ get { return location;		} set { location		= value; } }
		#endregion

		#region Costruttori
		/// <summary>
		/// ProviderItem
		/// Costruttore (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public TracedItem() {}

		/// <summary>
		/// Costruttore
		/// Inizializzo il ProviderId, Provider e Description
		/// </summary>
		/// <param name="id"></param>
		/// <param name="text"></param>
		/// <param name="description"></param>
		//---------------------------------------------------------------------
		public TracedItem (string companyName, string loginName, int operationType, DateTime operationDate, string processName, string winUser, string location)
		{
			this.companyName	= companyName;
			this.loginName		= loginName;
			this.operationType  = operationType;
			this.operationDate  = operationDate;
			this.processName    = processName;
			this.winUser        = winUser;
			this.location       = location;
		}
		#endregion
	}
}