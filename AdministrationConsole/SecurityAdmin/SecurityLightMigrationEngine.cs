using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//================================================================================
	public class SecurityLightMigrationEngine
	{
		private string connectionString = string.Empty;
		private int sourceCompanyId = -1;
		private int destinationCompanyId = -1;

		private SqlConnection connection = null;
		private bool isProcessing = false;
		private Thread migrationThread = null;
		private ManualResetEvent procedureAbortRequestEvent = null;
		private ManualResetEvent procedureAbortedEvent = null;

		//---------------------------------------------------------------------
		public bool IsProcessing { get { return isProcessing; } }

		//---------------------------------------------------------------------
		public delegate void SetProgressBarMaxValueEventHandler(object sender, int value);
		public event SetProgressBarMaxValueEventHandler SetProgressBarMaxValue;
		public delegate void ChangeProgressBarTextEventHandler(object sender, string message);
		public event ChangeProgressBarTextEventHandler ChangeProgressBarText;
		public delegate void PerformProgressBarStepEventHandler(object sender, string nameSpace);
		public event PerformProgressBarStepEventHandler PerformProgressBarStep;
		public event System.EventHandler ProcedureEnded;

		//--------------------------------------------------------------------------------
		public SecurityLightMigrationEngine(string aConnectionString, int sourceCompanyId, int desctinationCompanyId)
		{
			this.sourceCompanyId = sourceCompanyId;
			this.destinationCompanyId = desctinationCompanyId;
			connectionString = aConnectionString;

			if (string.IsNullOrWhiteSpace(aConnectionString))
				return;

			try
			{
				connection = new SqlConnection(aConnectionString);
				connection.Open();
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException raised in SecurityLightMigrationEngine constructor: " + e.Message);

				if (connection != null && connection.State != ConnectionState.Closed)
				{
					connection.Close();
					connection.Dispose();
				}

				connection = null;
			}
		}

		//---------------------------------------------------------------------
		public void StartMigrationThread()
		{
			try
			{
				migrationThread = new Thread(new ThreadStart(StartMigration));
				// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
				// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
				migrationThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				migrationThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				migrationThread.Start();
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in SecurityLightMigrationEngine.StartMigrationThread: " + exception.Message);
				migrationThread = null;
			}
		}

		//---------------------------------------------------------------------
		public void SuspendMigrationThread()
		{
			if (migrationThread == null || !migrationThread.IsAlive)
				return;

			//migrationThread.Suspend();
			try
			{
				migrationThread.Priority = ThreadPriority.Lowest;
			}
			catch (Exception)
			{
			}
		}

		//---------------------------------------------------------------------
		public void AbortMigrationThread()
		{
			if (migrationThread == null)
				return;

			bool rc = false;
			if (procedureAbortRequestEvent != null)
				rc = procedureAbortRequestEvent.Set();

			//if ((migrationThread.ThreadState & System.Threading.ThreadState.Suspended) == System.Threading.ThreadState.Suspended)
			//    migrationThread.Resume();

			if (procedureAbortedEvent != null)
				procedureAbortedEvent.WaitOne();

			rc = procedureAbortedEvent.Reset();

			isProcessing = false;

			if ((migrationThread.ThreadState & System.Threading.ThreadState.Running) == System.Threading.ThreadState.Running)
				migrationThread.Abort();

			migrationThread = null;
		}

		///<summary>
		/// Entry point migrazione users
		///</summary>
		//---------------------------------------------------------------------
		private void StartMigration()
		{
			if (procedureAbortRequestEvent == null)
				procedureAbortRequestEvent = new ManualResetEvent(false);//the initial state is set to nonsignaled
			procedureAbortRequestEvent.Reset();

			if (procedureAbortedEvent == null)
				procedureAbortedEvent = new ManualResetEvent(false);//the initial state is set to nonsignaled
			procedureAbortedEvent.Reset();

			isProcessing = true;

			// Batches
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingBatchAccessRightsProgressBarText);

			INameSpace[] nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleBatches(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.Batch))
				return;

			// Data Entry Forms
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingFormAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleForms(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.DataEntry))
				return;

			// Reports
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingReportAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleReports(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.Report))
				return;

			// Functions
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingFunctionAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleFunctions(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.Function))
				return;

			// Word Documents
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingWordDocumentAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleWordDocuments(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.WordDocument))
				return;

			// Word Templates
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingWordTemplateAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleWordTemplates(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.WordTemplate))
				return;

			// Excel Documents
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingExcelDocumentAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleExcelDocuments(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.ExcelDocument))
				return;

			// Excel Templates
			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ImportingExcelTemplateAccessRightsProgressBarText);

			nameSpaces = SecurityLightManager.GetAllCompanyInaccessibleExcelTemplates(sourceCompanyId, connection);

			if (!SetProtection(nameSpaces, SecurityType.ExcelTemplate))
				return;

			if (ChangeProgressBarText != null)
				ChangeProgressBarText(this, Strings.ProcedureEndedProgressBarText);

			if (PerformProgressBarStep != null)
				PerformProgressBarStep(this, String.Empty);

			if (procedureAbortedEvent != null)
				procedureAbortedEvent.Set();

			isProcessing = false;

			if (this.sourceCompanyId != -1) // Qui devo fare la cancellazione dei record x quella company
				SecurityLightManager.DeleteCompanyAccessRights(sourceCompanyId, connection);

			//Setto il Flag della migrazione da Light a Full a true
			SecurityLightManager.SetSecurityLightMigratedFlag(destinationCompanyId, connection);

			if (ProcedureEnded != null)
				ProcedureEnded(this, System.EventArgs.Empty);
		}


		//---------------------------------------------------------------------
		private bool SetProtection(INameSpace[] nameSpaces, SecurityType type)
		{
			if (nameSpaces == null || nameSpaces.Length == 0 || connection == null || connection.State != ConnectionState.Open)
				return true;

			int objectId = -1;
			int allowGrant = 0;

			if (SetProgressBarMaxValue != null)
				SetProgressBarMaxValue(this, nameSpaces.Length);
			
			int typeId = CommonObjectTreeFunction.GetObjectTypeId(Convert.ToInt32(type), connection);

			foreach (NameSpace nameSpace in nameSpaces)
			{
				ArrayList users = GetCurrentCompanyUsers();
				if (procedureAbortRequestEvent != null && procedureAbortRequestEvent.WaitOne(10, false))
				{
					if (procedureAbortedEvent != null)
						procedureAbortedEvent.Set();

					return false;
				}

				if (PerformProgressBarStep != null)
					PerformProgressBarStep(this, nameSpace.GetNameSpaceWithoutType());

				objectId = CommonObjectTreeFunction.GetObjectId(nameSpace.GetNameSpaceWithoutType(), typeId, connectionString);

				if (objectId == -1)
					continue;

                if (!ImportExportFunction.IsProtected(nameSpace.GetNameSpaceWithoutType(), typeId, destinationCompanyId, connection))
					CommonObjectTreeFunction.ProtectObject(destinationCompanyId, objectId, connection, false);

				//Per prima cosa controllo se lo ha negato a tutti
				if (SecuredCommand.IsAccessToObjectNameSpaceDenied(nameSpace, destinationCompanyId, -1, connection))
				{
					AllUserGrants(users, objectId, 0);
					continue;
				}

				foreach (int userId in users)
				{
					//int userId = (int)users[i];
					if (procedureAbortRequestEvent != null && procedureAbortRequestEvent.WaitOne(10, false))
					{
						if (procedureAbortedEvent != null)
							procedureAbortedEvent.Set();

						continue;
					}

					if (!SecuredCommand.IsAccessToObjectNameSpaceDenied(nameSpace, sourceCompanyId, userId, connection))
						continue;

					if (!ExistUserGrant(objectId, userId))
					{
						SaveGrant(userId, objectId, 0);
						allowGrant = SetAllowGrants(typeId);

						if (allowGrant == -1)
							continue;

						AllUserGrants(users, objectId, allowGrant, userId);
						continue;
					}
					else
					{
						if (!ExistDenyUserGrant(objectId, userId))
						{
							UpdateGrant(userId, objectId, 0);
							allowGrant = SetAllowGrants(typeId);

							if (allowGrant == -1)
								continue;

							AllUserGrants(users, objectId, allowGrant, userId);
							continue;
						}
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------
		private int SetAllowGrants(int type)
		{
			int grant = 0;

			if (connection == null)
				return -1;

			if (connection.State != ConnectionState.Open)
				return -1;

			string select = @"SELECT MSD_ObjectTypeGrants.GrantMask FROM MSD_ObjectTypeGrants
								WHERE MSD_ObjectTypeGrants.TypeId = @TypeId";

			SqlCommand mySqlCommand = new SqlCommand(select, connection);
			mySqlCommand.Parameters.AddWithValue("@TypeId", type);
			SqlDataReader myReader = null;

			try
			{
				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
					grant = Bit.SetUno(grant, Convert.ToInt32(myReader["GrantMask"]));

				return grant;
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception raised in SecurityLightMigrationEngine.SetAllowGrants: " + e.Message);
				return -1;
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}

				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private bool AllUserGrants(ArrayList users, int objectId, int grant)
		{
			if (users == null || users.Count == 0)
				return false;

			if (objectId == -1)
				return false;

			foreach (int userId in users)
			{
				if (procedureAbortRequestEvent != null && procedureAbortRequestEvent.WaitOne(10, false))
				{
					if (procedureAbortedEvent != null)
						procedureAbortedEvent.Set();

					return false;
				}

				if (!ExistUserGrant(objectId, userId))
					SaveGrant(userId, objectId, grant);
				else
					if (!ExistDenyUserGrant(objectId, userId))
						UpdateGrant(userId, objectId, grant);
			}

			return true;
		}

		//---------------------------------------------------------------------
		private bool AllUserGrants(ArrayList users, int objectId, int grant, int aUserId)
		{
			if (users == null || users.Count == 0)
				return false;

			if (objectId == -1)
				return false;

			foreach (int userId in users)
			{
				if (procedureAbortRequestEvent != null && procedureAbortRequestEvent.WaitOne(10, false))
				{
					if (procedureAbortedEvent != null)
						procedureAbortedEvent.Set();

					return false;
				}

				if (userId == aUserId)
					continue;

				if (!ExistUserGrant(objectId, userId))
					SaveGrant(userId, objectId, grant);
				else
					if (!ExistDenyUserGrant(objectId, userId))
						UpdateGrant(userId, objectId, grant);
			}

			return true;
		}

		//---------------------------------------------------------------------
		private bool ExistDenyUserGrant(int aObjectId, int aUserId)
		{

			if (
				aObjectId == -1 ||
				aUserId == -1 ||
				connection == null ||
				connection.State != ConnectionState.Open
				)
				return false;

			string sSelect = string.Format
				(
				"SELECT * FROM MSD_ObjectGrants WHERE CompanyId = {0} AND ObjectId = {1} AND LoginId = {2}", 
				destinationCompanyId.ToString(),
				aObjectId.ToString(), 
				aUserId.ToString()
				);

			SqlCommand mySqlCommandSel = null;
			SqlDataReader myReader = null;
			try
			{
				mySqlCommandSel = new SqlCommand(sSelect, connection);
				myReader = mySqlCommandSel.ExecuteReader();

				if (myReader.Read())
				{
					int result = Convert.ToInt32(myReader["Grants"]);
					return (result == 0);
				}

				return false;
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception raised in SecurityLightMigrationEngine.ExistUserGrant: " + e.Message);
				return false;
			}
			finally
			{
				if (mySqlCommandSel != null)
					mySqlCommandSel.Dispose();
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}
			}
		}

		//---------------------------------------------------------------------
		private bool ExistUserGrant(int aObjectId, int aUserId)
		{
			if	(
				aObjectId == -1 ||
				aUserId == -1 ||
				connection == null ||
				connection.State != ConnectionState.Open
				)
				return false;

			string sSelect = string.Format
				(
				"SELECT COUNT(*) FROM MSD_ObjectGrants WHERE CompanyId = {0} AND ObjectId = {1} AND LoginId = {2}",
				destinationCompanyId.ToString(),
				aObjectId.ToString(),
				aUserId.ToString()
				);

			SqlCommand mySqlCommandSel = null;

			try
			{
				mySqlCommandSel = new SqlCommand(sSelect, connection);

				int result = (int)mySqlCommandSel.ExecuteScalar();
				return (result > 0);
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception raised in SecurityLightMigrationEngine.ExistUserGrant: " + e.Message);
				return false;
			}
			finally
			{
				if (mySqlCommandSel != null)
					mySqlCommandSel.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private void UpdateGrant(int userId, int objectId, int grant)
		{
			string sInsert = string.Format
				(
					"UPDATE MSD_ObjectGrants SET Grants = {0} WHERE CompanyId = {1} AND ObjectId = {2} AND LoginId = {3}", 
					grant, 
					destinationCompanyId.ToString() ,
					objectId.ToString(),
					userId.ToString()
				);

			SqlCommand mySqlCommand = new SqlCommand(sInsert, connection);

			try
			{
				mySqlCommand.ExecuteNonQuery();
				mySqlCommand.Dispose();
			}
			catch (SqlException)
			{
				mySqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private void SaveGrant(int userId, int objectId, int grant)
		{
			string sInsert = @"INSERT INTO MSD_ObjectGrants
						(CompanyId, ObjectId, LoginId, RoleId, Grants, InheritMask)
						VALUES 
						(@CompanyId, @ObjectId, @LoginId,  @RoleId, @Grants, @InheritMask)";

			SqlCommand mySqlCommand = new SqlCommand(sInsert, connection);
			mySqlCommand.Parameters.AddWithValue("@CompanyId", destinationCompanyId);
			mySqlCommand.Parameters.AddWithValue("@ObjectId", objectId);
			mySqlCommand.Parameters.AddWithValue("@Grants", grant);
			mySqlCommand.Parameters.AddWithValue("@InheritMask", SqlDbType.Int);
			mySqlCommand.Parameters.AddWithValue("@LoginId", userId);
			mySqlCommand.Parameters.AddWithValue("@RoleId", SqlDbType.Int);

			mySqlCommand.Parameters["@InheritMask"].Value = 0;
			mySqlCommand.Parameters["@RoleId"].Value = 0;

			try
			{
				mySqlCommand.ExecuteNonQuery();
				mySqlCommand.Dispose();
			}
			catch (SqlException)
			{
				mySqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private ArrayList GetCurrentCompanyUsers()
		{
			ArrayList users = new ArrayList();

			if (connection == null)
				return users;

			if (connection.State != ConnectionState.Open)
				connection.Open();

			SqlCommand selectCommand = null;
			SqlDataReader myReader = null;
			SqlConnection conn = null;

			int EasyLookLoginId = CommonObjectTreeFunction.GetApplicationUserID("EasyLookSystem", connection);

			string sSelect = string.Format
				(
				"SELECT LoginId FROM  MSD_CompanyLogins where CompanyId = {0} AND LoginId != {1}", 
				destinationCompanyId,
				EasyLookLoginId
				);

			try
			{
				conn = new SqlConnection(connectionString);
				conn.Open();
				selectCommand = new SqlCommand(sSelect, conn);
				myReader = selectCommand.ExecuteReader();

				while (myReader.Read())
					users.Add(Convert.ToInt32(myReader["LoginId"]));
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception thrown in GetCurrentCompanyUsers: " + e.Message);
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}
				
				if (selectCommand != null)
					selectCommand.Dispose();
				
				if (conn != null && conn.State != ConnectionState.Closed)
				{
					conn.Close();
					conn.Dispose();
				}
			}

			return users;
		}
	}
}
