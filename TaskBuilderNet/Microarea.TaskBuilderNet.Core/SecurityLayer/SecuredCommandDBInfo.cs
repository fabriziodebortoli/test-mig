using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects
{
	/// <summary>
	/// Summary description for SecuredCommandDBInfo.
	/// </summary>
	public class SecuredCommandDBInfo : IDisposable
	{
		[Flags]
		private enum AccessAttributes
		{
			Undefined				= 0x00000000,
			UnattendedModeAllowed	= 0x00000001
		}

		private class AccessAttributesInfo
		{
			private int companyId = -1;
			private int userId = -1;
			private int attributesMask = 0;

			public AccessAttributesInfo (int aCompanyId, int aUserId, int aAttributesMask)
			{
				companyId = aCompanyId;
				userId = aUserId;
				attributesMask = aAttributesMask;
			}

			public int CompanyId		{ get { return companyId; } }
			public int UserId			{ get { return userId; } }
			public int AttributesMask	{ get { return attributesMask; } }
		}
		
		#region SecuredCommandDBInfo private data members

		private string				nameSpace = String.Empty;
		private SecuredCommandType	type = SecuredCommandType.Undefined;
        
		private SqlConnection systemDBConnection = null;	

		private string[] userNamesToSkip = null;

		private static SqlCommand checkDeniedAccessCommand = null;
		private static SqlCommand getAccessAttributesCommand = null;
		private const string ObjectIdSqlCommandParameterName = "@ObjectId";

		private bool disposed = false;	// Track whether Dispose has been called.

		#endregion // SecuredCommandDBInfo private data members
		
		//---------------------------------------------------------------------
		public SecuredCommandDBInfo(string aObjectNameSpace, SecuredCommandType aObjectType, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			nameSpace = aObjectNameSpace;
			type = aObjectType;
			if (type == SecuredCommandType.Report && nameSpace.EndsWith(NameSolverStrings.WrmExtension))
				nameSpace = nameSpace.Substring(0, nameSpace.Length - NameSolverStrings.WrmExtension.Length);

			systemDBConnection = aConnection;
			userNamesToSkip = aUserNamesToSkipList;
		}
		
		//------------------------------------------------------------------------------
		~SecuredCommandDBInfo()
		{
			Dispose(false);
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				// add sensitive resource garbage collection
			}
				
			disposed = true;         
		}
		
		#region SecuredCommandDBInfo private methods
		
		//---------------------------------------------------------------------
		private int GetId(bool createIfNotExist)
		{
			return GetObjectId(nameSpace, type, systemDBConnection, createIfNotExist);
		}

		//---------------------------------------------------------------------
		private SqlDataReader GetAccessAttributesReader()
		{
			return GetAccessAttributesReader(GetObjectId(nameSpace, type, systemDBConnection), systemDBConnection); 
		}
		
		//---------------------------------------------------------------------
		private bool SetAccessAttributeFlag(int aCompanyId, int aUserId, AccessAttributes aAccessAttributeToSet, bool setAttribute)
		{
			if (aAccessAttributeToSet == AccessAttributes.Undefined || systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				return false;

			int objectId = GetObjectId(nameSpace, type, systemDBConnection);
			if (objectId == -1)
				return false; // l'accesso all'oggetto non risulta vietato!
		
			int attributesMask = 0;
			SqlCommand sqlCommand = null;
			SqlDataReader accessAttributesReader = GetAccessAttributesReader(objectId, systemDBConnection);

			try
			{
				ArrayList existingAccessAttributes = new ArrayList();

				if (accessAttributesReader != null)
				{
					while (accessAttributesReader.Read())
					{
						int companyId = Convert.ToInt32(accessAttributesReader["CompanyId"]);
						int userId = Convert.ToInt32(accessAttributesReader["UserId"]);
					
						if ((aCompanyId == companyId || companyId == -1) && (aUserId == userId || userId == -1))
						{
							attributesMask = Convert.ToInt32(accessAttributesReader["AttributesMask"]);
						
							bool currentSetting = ((attributesMask & (int)aAccessAttributeToSet) == (int)aAccessAttributeToSet);
							if (currentSetting == setAttribute)
								return true; // l'accesso all'oggetto in modalità silente risulta già impostato nel modo voluto

							existingAccessAttributes.Add(new AccessAttributesInfo(companyId, userId, attributesMask));
						}
					}
				}

				accessAttributesReader.Close();

				if (setAttribute)
					attributesMask |= (int)aAccessAttributeToSet;
				else
					attributesMask &= ~(int)aAccessAttributeToSet;
		
				sqlCommand = new SqlCommand();
				sqlCommand.Connection = systemDBConnection;

				if (aUserId == -1 && aCompanyId == -1) // devo estendere la proprietà del divieto di accesso a tutti gli utenti e a tutte le aziende
				{
					// Cancello tutte le preesistenti proprietà dei divieti di accesso dell'oggetto
					sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString();
					sqlCommand.ExecuteNonQuery();

					if (attributesMask != 0)
					{
						sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, -1, " + attributesMask.ToString() + ")";
						sqlCommand.ExecuteNonQuery();
					}

					OnAccessAttributesChanged(-1, -1, attributesMask);

					return true;
				}
			
				if (aUserId != -1 && aCompanyId == -1) // devo estendere la proprietà del divieto di accesso per un certo utente a tutte le aziende
				{
					// Cancello tutte le possibili proprietà dei divieti di accesso dell'oggetto riferiti all'utente in questione
					sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND UserId = " + aUserId.ToString();
					sqlCommand.ExecuteNonQuery();
	
					// se esistono già le stesse proprietà di accesso per tutti gli utenti tranne quello correntemente in esame, 
					// devo eliminare questi record preesistenti ed inserire direttamente un divieto del tipo "a tutti gli utenti  
					// e per tutte le aziende"
					bool setForAll = true;
                    int[] userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
					if (userIds != null && userIds.Length > 0)
					{
						foreach(int anotherUserId in userIds)
						{
							if (anotherUserId == aUserId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = " + anotherUserId.ToString() + " AND AttributesMask = " + attributesMask.ToString();
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								setForAll = false;
								break;
							}						
						}
					}
					
					if (setForAll)
					{
						// Cancello tutte le preesistenti proprietà dei divieti di accesso dell'oggetto
						sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString();
						sqlCommand.ExecuteNonQuery();
						
						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, -1, " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
					else
					{
						if (existingAccessAttributes != null && existingAccessAttributes.Count > 0 && userIds != null && userIds.Length > 0)
						{
							bool manageExistingAttributes = false;
							int existingAttributesMask = 0;
							foreach (AccessAttributesInfo accessAttributes in existingAccessAttributes)
							{
								existingAttributesMask = accessAttributes.AttributesMask;
					
								if  (accessAttributes.CompanyId == -1 && accessAttributes.UserId == -1)
								{	
									manageExistingAttributes = true;
									break;
								}
							}

							if (manageExistingAttributes)
							{
								sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = -1";
								sqlCommand.ExecuteNonQuery();
						
								foreach(int anotherUserId in userIds)
								{
									if (anotherUserId == aUserId)
										continue;
		
									sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, " + anotherUserId.ToString() + ", " + existingAttributesMask.ToString() + ")";
									sqlCommand.ExecuteNonQuery();
								}
							}
						}

						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, " + aUserId.ToString() + ", " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
				}
				else if (aUserId == -1 && aCompanyId != -1) // devo estendere la proprietà del divieto di accesso a tutti gli utenti nell'ambito di una data azienda
				{
					// Cancello tutte le possibili proprietà dei divieti di accesso dell'oggetto riferiti all'azienda in questione
					sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString();
					sqlCommand.ExecuteNonQuery();
	
					// se esistono già le stesse proprietà di accesso per tutte le aziende tranne quella correntemente in esame, 
					// devo eliminare questi record preesistenti ed inserire direttamente un divieto del tipo "a tutti gli utenti  
					// e per tutte le aziende"
					bool setForAll = true;
                    int[] companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
					if (companyIds != null && companyIds.Length > 0)
					{
						foreach(int anotherCompanyId in companyIds)
						{
							if (anotherCompanyId == aCompanyId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + anotherCompanyId.ToString() + " AND UserId = -1 AND AttributesMask = " + attributesMask.ToString();
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								setForAll = false;
								break;
							}						
						}
					}
					
					if (setForAll)
					{
						// Cancello tutte le preesistenti proprietà dei divieti di accesso dell'oggetto
						sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString();
						sqlCommand.ExecuteNonQuery();

						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, -1, " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
					else
					{
						if (existingAccessAttributes != null && existingAccessAttributes.Count > 0 && companyIds != null && companyIds.Length > 0)
						{
							bool manageExistingAttributes = false;
							int existingAttributesMask = 0;
							foreach (AccessAttributesInfo accessAttributes in existingAccessAttributes)
							{
								existingAttributesMask = accessAttributes.AttributesMask;
					
								if  (accessAttributes.CompanyId == -1 && accessAttributes.UserId == -1)
								{	
									manageExistingAttributes = true;
									break;
								}
							}

							if (manageExistingAttributes)
							{
								sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = -1";
								sqlCommand.ExecuteNonQuery();
						
								foreach(int anotherCompanyId in companyIds)
								{
									if (anotherCompanyId == aCompanyId)
										continue;

									sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + anotherCompanyId.ToString() + ", -1, " + existingAttributesMask.ToString() + ")";
									sqlCommand.ExecuteNonQuery();
								}
							}
						}
						
						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", -1, " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
				}
				else // devo impostare la proprietà divieto di accesso per un certo utente nell'ambito di una data azienda
				{			
					bool setForAllCompanies = true;
                    int[] companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
					if (companyIds != null && companyIds.Length > 0)
					{
						foreach(int anotherCompanyId in companyIds)
						{
							if (anotherCompanyId == aCompanyId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + anotherCompanyId.ToString() + " AND UserId = " + aUserId.ToString() + "  AND AttributesMask = " + attributesMask.ToString();
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								setForAllCompanies = false;
								break;
							}						
						}
					}
					bool setForAllUsers = true;
                    int[] userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
					if (userIds != null && userIds.Length > 0)
					{
						foreach(int anotherUserId in userIds)
						{
							if (anotherUserId == aUserId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString() + " AND UserId = " + anotherUserId.ToString() + " AND AttributesMask = " + attributesMask.ToString();;
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								setForAllUsers = false;
								break;
							}						
						}
					}

					if (setForAllCompanies && setForAllUsers)
					{
						// Cancello tutte le preesistenti proprietà dei divieti di accesso dell'oggetto
						sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString();
						sqlCommand.ExecuteNonQuery();
						
						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, -1, " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
					else if (setForAllCompanies)
					{
						// Cancello tutte le possibili proprietà dei divieti di accesso dell'oggetto riferiti all'utente in questione
						sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND UserId = " + aUserId.ToString();
						sqlCommand.ExecuteNonQuery();
						
						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", -1, " + aUserId.ToString() + ", " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
					else if (setForAllUsers)
					{
						// Cancello tutte le possibili proprietà dei divieti di accesso dell'oggetto riferiti all'azienda in questione
						sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString();
						sqlCommand.ExecuteNonQuery();
						
						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", -1, " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
					else
					{
						if (existingAccessAttributes != null && existingAccessAttributes.Count > 0)
						{
							bool manageExistingAttributes = false;
							int existingCompanyId = -1;
							int existingUserId = -1;
							int existingAttributesMask = 0;
							foreach (AccessAttributesInfo accessAttributes in existingAccessAttributes)
							{
								existingCompanyId = accessAttributes.CompanyId;
								existingUserId = accessAttributes.UserId;
								existingAttributesMask = accessAttributes.AttributesMask;
					
								if 
									(
									(existingCompanyId == -1 && existingUserId == -1) ||
									(existingCompanyId == aCompanyId && existingUserId == -1) ||
									(existingCompanyId == -1 && existingUserId == aUserId) ||
									(existingCompanyId == aCompanyId && existingUserId == aUserId)
									)
								{	
									manageExistingAttributes = true;
									break;
								}
							}

							if (manageExistingAttributes)
							{
								sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + existingCompanyId.ToString() + " AND UserId = " + existingUserId.ToString();
								sqlCommand.ExecuteNonQuery();
								
								if (existingCompanyId == -1)
								{
									if (companyIds != null && companyIds.Length > 0)
									{
										foreach(int anotherCompanyId in companyIds)
										{
											if (existingUserId == -1)
											{
												if (userIds != null && userIds.Length > 0)
												{
													foreach(int anotherUserId in userIds)
													{
														if (anotherCompanyId == aCompanyId && anotherUserId == aUserId)
															continue;
								
														sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + anotherCompanyId.ToString() + ", " + anotherUserId.ToString() + ", " + existingAttributesMask.ToString() + ")";
														sqlCommand.ExecuteNonQuery();
													}
												}
											}
											else if (existingUserId == aUserId)
											{
												sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + anotherCompanyId.ToString() + ", " + aUserId.ToString() + ", " + existingAttributesMask.ToString() + ")";
												sqlCommand.ExecuteNonQuery();
											}
										}
									}
									else if (existingCompanyId == aCompanyId && existingUserId == -1)
									{
										if (userIds != null && userIds.Length > 0)
										{
											foreach(int anotherUserId in userIds)
											{
												if (anotherUserId == aUserId)
													continue;
					
												sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", " + anotherUserId.ToString() + ", " + existingAttributesMask.ToString() + ")";
												sqlCommand.ExecuteNonQuery();
											}
										}
									}

								}
							}
						}
					
						if (attributesMask != 0)
						{
							sqlCommand.CommandText = "INSERT INTO MSD_SLAccessAttributes(ObjectId, CompanyId, UserId, AttributesMask) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", " + aUserId.ToString() + ", " + attributesMask.ToString() + ")";
							sqlCommand.ExecuteNonQuery();
						}
					}
				}

				OnAccessAttributesChanged(aCompanyId, aUserId, attributesMask);
			
				return true;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.SetAccessInUnattendedMode: " + e.Message);

				return false;	
			}
			finally
			{
				if (accessAttributesReader != null && !accessAttributesReader.IsClosed)
					accessAttributesReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		#region SecuredCommandDBInfo private static methods

		//---------------------------------------------------------------------
		private static int GetObjectId(string aObjectNameSpace, SecuredCommandType aObjectType, SqlConnection aConnection, bool createIfNotExist)
		{
			if 
				(
				aObjectNameSpace == null || 
				aObjectNameSpace == String.Empty || 
				aObjectType == SecuredCommandType.Undefined ||
				aConnection == null || 
				aConnection.State != ConnectionState.Open
				)
				return -1;

			if (aObjectType == SecuredCommandType.Report && aObjectNameSpace.EndsWith(NameSolverStrings.WrmExtension))
			{
				aObjectNameSpace = aObjectNameSpace.Substring(0, aObjectNameSpace.Length - NameSolverStrings.WrmExtension.Length);
				if (aObjectNameSpace == null || aObjectNameSpace == String.Empty)
					return -1;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader objectReader = null;

			try
			{
				string queryText = "SELECT ObjectId FROM MSD_SLObjects WHERE NameSpace = '" + aObjectNameSpace + "' AND Type = " + ((int)aObjectType).ToString();

				sqlCommand = new SqlCommand(queryText, aConnection);
				
				objectReader = sqlCommand.ExecuteReader(System.Data.CommandBehavior.SingleRow);

				if (objectReader.Read())
					return Convert.ToInt32(objectReader["ObjectId"]);

				objectReader.Close();

				if (createIfNotExist)
				{
					sqlCommand.CommandText = "INSERT INTO MSD_SLObjects(NameSpace, Type) VALUES ('" + aObjectNameSpace + "', " + ((int)aObjectType).ToString() + ")";

					// For UPDATE, INSERT, and DELETE statements, the return value of the sqlCommand.ExecuteNonQuery method is
					// the number of rows affected by the command.
					if (sqlCommand.ExecuteNonQuery() != 1)
						return -1;

					return GetObjectId(aObjectNameSpace, aObjectType, aConnection);
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.GetObjectId: " + e.Message);
			}
			finally
			{
				if (objectReader != null && !objectReader.IsClosed)
					objectReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
			return -1;
		}

		//---------------------------------------------------------------------
		private static bool IsAccessToObjectDenied(int aObjectId, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			if 
				(
				aObjectId == -1 ||
				aConnection == null || 
				aConnection.State != ConnectionState.Open
				)
				return false;

			SqlDataReader deniedAccessesReader = null;

			try
			{
				if (checkDeniedAccessCommand == null || checkDeniedAccessCommand.Connection != aConnection)
				{
					if (checkDeniedAccessCommand != null)
					{
						checkDeniedAccessCommand.Dispose();
						checkDeniedAccessCommand = null;
					}
					
					string queryText = "SELECT CompanyId, UserId FROM MSD_SLDeniedAccesses WHERE ObjectId = " + ObjectIdSqlCommandParameterName;

					checkDeniedAccessCommand = new SqlCommand(queryText, aConnection);

					checkDeniedAccessCommand.Parameters.Add(ObjectIdSqlCommandParameterName, SqlDbType.Int, 4);
					checkDeniedAccessCommand.Prepare();
				}

				checkDeniedAccessCommand.Parameters[ObjectIdSqlCommandParameterName].Value = aObjectId;
				
				deniedAccessesReader = checkDeniedAccessCommand.ExecuteReader();

				while (deniedAccessesReader.Read())
				{
					int companyId = Convert.ToInt32(deniedAccessesReader["CompanyId"]);
					int userId = Convert.ToInt32(deniedAccessesReader["UserId"]);

					if ((aCompanyId == companyId || companyId == -1) && (aUserId == userId || userId == -1))
						return true;
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.IsAccessToObjectDenied: " + e.Message);

				if (checkDeniedAccessCommand != null)
				{
					checkDeniedAccessCommand.Dispose();
					checkDeniedAccessCommand = null;
				}
			}
			finally
			{
				if (deniedAccessesReader != null && !deniedAccessesReader.IsClosed)
					deniedAccessesReader.Close();
			}
			
			return false;
		}
		
		//---------------------------------------------------------------------
		private static bool IsAccessToObjectInUnattendedModeAllowed(int aObjectId, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			if 
				(
				aObjectId == -1 ||
				aConnection == null || 
				aConnection.State != ConnectionState.Open ||
				!IsAccessToObjectDenied(aObjectId, aCompanyId, aUserId, aConnection)
				)
				return true;

			SqlDataReader unattendedModeAccessesReader = GetAccessAttributesReader(aObjectId, aConnection);

			try
			{
				while (unattendedModeAccessesReader.Read())
				{
					int companyId = Convert.ToInt32(unattendedModeAccessesReader["CompanyId"]);
					int userId = Convert.ToInt32(unattendedModeAccessesReader["UserId"]);

					if ((aCompanyId == companyId || companyId == -1) && (aUserId == userId || userId == -1))
					{
						int attributesMask = Convert.ToInt32(unattendedModeAccessesReader["AttributesMask"]);
						return ((attributesMask & (int)AccessAttributes.UnattendedModeAllowed) == (int)AccessAttributes.UnattendedModeAllowed);
					}
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.IsAccessToObjectDenied: " + e.Message);

				if (getAccessAttributesCommand != null)
				{
					getAccessAttributesCommand.Dispose();
					getAccessAttributesCommand = null;
				}
			}
			finally
			{
				if (unattendedModeAccessesReader != null && !unattendedModeAccessesReader.IsClosed)
					unattendedModeAccessesReader.Close();
			}
			
			return false;
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToObjectInUnattendedModeAllowed(string aNameSpaceTextWithoutType, SecuredCommandType aObjectType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			if 
				(
				aNameSpaceTextWithoutType == null || 
				aNameSpaceTextWithoutType == String.Empty || 
				aObjectType == SecuredCommandType.Undefined ||
				aConnection == null || 
				aConnection.State != ConnectionState.Open
				)
				return false;

			return IsAccessToObjectInUnattendedModeAllowed(GetObjectId(aNameSpaceTextWithoutType, aObjectType, aConnection), aCompanyId, aUserId,  aConnection);
		}
		
		//---------------------------------------------------------------------
		private static SqlDataReader GetAccessAttributesReader(int aObjectId, SqlConnection aConnection)
		{
			if (aObjectId == -1 || aConnection == null || aConnection.State != ConnectionState.Open)
				return null;

			try
			{
				if (getAccessAttributesCommand == null || getAccessAttributesCommand.Connection != aConnection)
				{
					if (getAccessAttributesCommand != null)
					{
						getAccessAttributesCommand.Dispose();
						getAccessAttributesCommand = null;
					}
					
					string queryText = "SELECT * FROM MSD_SLAccessAttributes WHERE ObjectId = " + ObjectIdSqlCommandParameterName;

					getAccessAttributesCommand = new SqlCommand(queryText, aConnection);

					getAccessAttributesCommand.Parameters.Add(ObjectIdSqlCommandParameterName, SqlDbType.Int, 4);
					getAccessAttributesCommand.Prepare();
				}

				getAccessAttributesCommand.Parameters[ObjectIdSqlCommandParameterName].Value = aObjectId;
				
				return getAccessAttributesCommand.ExecuteReader();
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.IsAccessToObjectDenied: " + e.Message);

				if (getAccessAttributesCommand != null)
				{
					getAccessAttributesCommand.Dispose();
					getAccessAttributesCommand = null;
				}

				return null;
			}
		}
		
		#endregion // SecuredCommandDBInfo private static methods
		
		#endregion // SecuredCommandDBInfo private methods

		#region SecuredCommandDBInfo protected properties

		//---------------------------------------------------------------------
		protected SqlConnection SystemDBConnection { get { return systemDBConnection; } }

		//---------------------------------------------------------------------
		protected string[] UserNamesToSkip { get { return userNamesToSkip; } }

		#endregion // SecuredCommandDBInfo protected properties

		#region SecuredCommandDBInfo protected virtual methods

		//---------------------------------------------------------------------
		protected virtual void OnAccessDenied(int aCompanyId, int aUserId)
		{
		}

		//---------------------------------------------------------------------
		protected virtual void OnAccessAllowed(int aCompanyId, int aUserId)
		{
		}

		//---------------------------------------------------------------------
		protected virtual void OnAccessAttributesChanged(int aCompanyId, int aUserId, int attributesMask)
		{
		}

		#endregion // SecuredCommandDBInfo protected virtual methods

		#region SecuredCommandDBInfo public properties

		//---------------------------------------------------------------------
		public string NameSpace { get { return nameSpace; } }

		//---------------------------------------------------------------------
		public SecuredCommandType Type { get { return type; } }

		#endregion // SecuredCommandDBInfo public properties

		#region SecuredCommandDBInfo public methods
		
		//---------------------------------------------------------------------
		public int GetId()
		{
			return GetId(false);
		}

		//---------------------------------------------------------------------
		public bool IsAccessDenied(int aCompanyId, int aUserId)
		{
			return IsAccessToObjectDenied(nameSpace, type, aCompanyId, aUserId, systemDBConnection);
		}

		//---------------------------------------------------------------------
		public bool IsAccessInUnattendedModeAllowed(int aCompanyId, int aUserId)
		{
			return IsAccessToObjectInUnattendedModeAllowed(nameSpace, type, aCompanyId, aUserId, systemDBConnection);
		}
		
		#region SecuredCommandDBInfo public virtual methods
		//---------------------------------------------------------------------
		//  DenyAccess
		//---------------------------------------------------------------------
		//		- Se l'accesso all'oggetto risulta già negato, non faccio nulla.
		//
		//		- Se gli id di utente e di azienda sono entrambi uguali a -1 (divieto d'accesso per tutti gli utenti
		//		e per tutte le aziende): 
		//		-> Cancello tutti i divieti preesistenti riferiti all'oggetto e 
		//		-> Inserisco il divieto con id di utente e di azienda uguali a -1.
		//
		//		- Se l'utente ha un id specifico, ma il divieto deve valere per tutte le aziende (id di azienda uguale
		//		  a -1):
		//		    -> Cancello tutti i divieti preesistenti riferiti all'oggetto ed all'utente in questione
		//		    -> Se esistono già divieti di accesso all'oggetto per tutti gli utenti tranne quello correntemente 
		//		       in esame, li elimino tutti e inserisco direttamente un divieto del tipo "a tutti gli utenti e 
		//		       per tutte le aziende".
		//		    -> Altrimenti, se c'è almeno un altro utente per il quale l'accesso all'oggetto non risulta 
		//		       vietato, inserisco il divieto solo per l'utente in questione e id di azienda uguale a -1.
		//
		//		- Se l'azienda ha un id specifico, ma il divieto deve valere per tutti gli utenti (id di utente uguale
		//		  a -1):
		//		    -> Cancello tutti i divieti di accesso all'oggetto riferiti all'azienda in questione.
		//		    -> Se esistono già divieti di accesso all'oggetto per tutte le aziende tranne quella correntemente
		//		       in esame, li elimino tutti e inserisco direttamente un divieto del tipo "a tutti gli utenti e 
		//		       per tutte le aziende".
		//		    -> Altrimenti, se c'è almeno un'altra azienda per la quale l'accesso all'oggetto non risulta 
		//			   vietato, inserisco il divieto solo per l'azienda in questione e id di utente uguale a -1.
		//
		//		- Se devo impostare il divieto di accesso per un certo utente nell'ambito di una data azienda (gli id 
		//		  di utente e di azienda sono entrambi diversi da -1):
		//		    -> Controllo se l'accesso all'oggetto risulta già vietato per tutte le aziende tranne quella 
		//			   in esame e anche se risulta già vietato per tutti gli utenti tranne quello in esame. 
		//			   In caso affermativo, li elimino tutti e inserisco direttamente un divieto del tipo "a tutti gli
		//		       utenti e per tutte le aziende".
		//		    -> Se l'accesso all'oggetto risulta già vietato per tutte le aziende tranne quella correntemente 
		//			   in esame, ma non per tutti gli altr utenti, elimino tutti i divieti riferiti alle varie altre 
		//  		   aziende (con utente uguale a quello corrente) e inserisco un divieto per l'utente corrente riferito
		//	           a tutte le aziende (id di azienda uguale a -1, denyForAllCompanies).
		//		    -> Altrimenti, se l'accesso all'oggetto risulta già vietato per tutti gli utenti tranne quello  
		//			   correntemente in esame, ma non per tutte le altre aziende, elimino tutti i divieti riferiti ai vari 
		//			   altri utenti (con azienda uguale a quella corrente) e inserisco un divieto per l'azienda corrente 
		//		       riferito a tutti gli utenti (id di utente uguale a -1, denyForAllUsers).
		//		    -> Altrimenti, inserisco solo il divieto di accesso per l'azienda e l'utente correnti.  
		//		
		//---------------------------------------------------------------------
		public virtual bool DenyAccess(int aCompanyId, int aUserId)
		{
			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				return false;

			int objectId = GetObjectId(nameSpace, type, systemDBConnection, true);
		
			if  (objectId == -1)
				return false;

			SqlCommand sqlCommand = null;
			SqlDataReader deniedAccessesReader = null;

			try
			{
				string queryText = "SELECT * FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString();

				sqlCommand = new SqlCommand(queryText, systemDBConnection);
				
				deniedAccessesReader = sqlCommand.ExecuteReader();

				while (deniedAccessesReader.Read())
				{
					int companyId = Convert.ToInt32(deniedAccessesReader["CompanyId"]);
					int userId = Convert.ToInt32(deniedAccessesReader["UserId"]);

					if ((aCompanyId == companyId || companyId == -1) && (aUserId == userId || userId == -1))
						return true; // l'accesso all'oggetto risulta già vietato
				}

				deniedAccessesReader.Close();
				
				if (aUserId == -1 && aCompanyId == -1) // devo estendere il divieto di accesso a tutti gli utenti e a tutte le aziende
				{
					// Cancello tutti i possibili divieti di accesso dell'oggetto preesistenti
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString();
					sqlCommand.ExecuteNonQuery();

					sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", -1, -1)";
					sqlCommand.ExecuteNonQuery();

					OnAccessDenied(-1, -1);

					return true;
				}
			
				if (aUserId != -1 && aCompanyId == -1) // devo estendere il divieto di accesso per un certo utente a tutte le aziende
				{
					// Cancello tutti i possibili divieti di accesso dell'oggetto riferiti all'utente in questione
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND UserId = " + aUserId.ToString();
					sqlCommand.ExecuteNonQuery();
	
					// se esistono già i divieti analoghi per tutti gli utenti tranne quello correntemente in esame, li devo eliminare ed
					// inserire direttamente un divieto del tipo "a tutti gli utenti e per tutte le aziende"
					bool denyForAll = true;
                    int[] userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
					if (userIds != null && userIds.Length > 0)
					{
						foreach(int anotherUserId in userIds)
						{
							if (anotherUserId == aUserId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = " + anotherUserId.ToString();
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								denyForAll = false;
								break;
							}						
						}
					}
					
					if (denyForAll)
					{
						// Cancello tutti i possibili divieti di accesso dell'oggetto preesistenti
						sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString();
						sqlCommand.ExecuteNonQuery();

						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", -1, -1)";
					}
					else
						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", -1, " + aUserId.ToString() + ")";
					
					sqlCommand.ExecuteNonQuery();
				}
				else if (aUserId == -1 && aCompanyId != -1) // devo estendere il divieto di accesso a tutti gli utenti nell'ambito di una data azienda
				{
					// Cancello tutti i possibili divieti di accesso dell'oggetto riferiti all'azienda in questione
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString();
					sqlCommand.ExecuteNonQuery();
	
					bool denyForAll = true;
                    int[] companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
					if (companyIds != null && companyIds.Length > 0)
					{
						foreach(int anotherCompanyId in companyIds)
						{
							if (anotherCompanyId == aCompanyId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + anotherCompanyId.ToString() + " AND UserId = -1";
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								denyForAll = false;
								break;
							}						
						}
					}
					
					if (denyForAll)
					{
						// Cancello tutti i possibili divieti di accesso dell'oggetto preesistenti
						sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString();
						sqlCommand.ExecuteNonQuery();

						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", -1, -1)";
					}
					else
						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", -1)";
					
					sqlCommand.ExecuteNonQuery();
				}
				else // devo impostare il divieto di accesso per un certo utente nell'ambito di una data azienda
				{			
					bool denyForAllCompanies = true;
                    int[] companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
					if (companyIds != null && companyIds.Length > 0)
					{
						foreach(int anotherCompanyId in companyIds)
						{
							if (anotherCompanyId == aCompanyId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + anotherCompanyId.ToString() + " AND UserId = " + aUserId.ToString();
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								denyForAllCompanies = false;
								break;
							}						
						}
					}
					bool denyForAllUsers = true;
                    int[] userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
					if (userIds != null && userIds.Length > 0)
					{
						foreach(int anotherUserId in userIds)
						{
							if (anotherUserId == aUserId)
								continue;
								
							sqlCommand.CommandText = "SELECT COUNT(*) FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString() + " AND UserId = " + anotherUserId.ToString();
							if ((int)sqlCommand.ExecuteScalar() == 0)
							{
								denyForAllUsers = false;
								break;
							}						
						}
					}

					if (denyForAllCompanies && denyForAllUsers)
					{
						// Cancello tutti i possibili divieti di accesso dell'oggetto preesistenti
						sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString();
						sqlCommand.ExecuteNonQuery();

						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", -1, -1)";
					}
					else if (denyForAllCompanies)
					{
						// Cancello tutti i possibili divieti di accesso dell'oggetto riferiti all'utente in questione
						sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND UserId = " + aUserId.ToString();
						sqlCommand.ExecuteNonQuery();
						
						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", -1, " + aUserId.ToString() + ")";
					}
					else if (denyForAllUsers)
					{
						// Cancello tutti i possibili divieti di accesso dell'oggetto riferiti all'azienda in questione
						sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString();
						sqlCommand.ExecuteNonQuery();
						
						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", -1)";
					}
					else
						sqlCommand.CommandText = "INSERT INTO MSD_SLDeniedAccesses(ObjectId, CompanyId, UserId) VALUES (" + objectId.ToString() + ", " + aCompanyId.ToString() + ", " + aUserId.ToString() + ")";
					
					sqlCommand.ExecuteNonQuery();
				}

				OnAccessDenied(aCompanyId, aUserId);

				return true;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.DenyAccess: " + e.Message);

				return false;	
			}
			finally
			{
				if (deniedAccessesReader != null && !deniedAccessesReader.IsClosed)
					deniedAccessesReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public virtual bool AllowAccess(int aCompanyId, int aUserId)
		{
			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				return false;

			int objectId = GetObjectId(nameSpace, type, systemDBConnection);
			if (objectId == -1)
				return true; // l'accesso all'oggetto non risulta vietato!
		
			SqlCommand sqlCommand = null;

			try
			{
				sqlCommand = new SqlCommand();
				sqlCommand.Connection = systemDBConnection;
				
				if (aUserId == -1 && aCompanyId == -1)
				{
					// Cancello tutti i possibili divieti di accesso dell'oggetto preesistenti
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString();
					sqlCommand.ExecuteNonQuery();

					OnAccessAllowed(-1, -1);

					DeleteAllAllowedObjectId(objectId, systemDBConnection);
				
					return true;
				}
		
				if (aUserId != -1 && aCompanyId == -1) 
				{
					// Devo eliminare qualsiasi divieto di accesso all'oggetto per un dato utente (indipendentemente dall'azienda)
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND UserId = " + aUserId.ToString();
					sqlCommand.ExecuteNonQuery();

					OnAccessAllowed(-1, aUserId);
					
					// Se esiste il divieto di accesso del tipo "a tutti gli utenti e per tutte le aziende" devo cancellarlo e 
					// inserire il divieto relativo ad ogni altro utente
							
					// For UPDATE, INSERT, and DELETE statements, the return value of the sqlCommand.ExecuteNonQuery method is
					// the number of rows affected by the command.
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = -1";
					if (sqlCommand.ExecuteNonQuery() == 1)
					{
						OnAccessAllowed(-1, -1);

                        int[] userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
						if (userIds != null && userIds.Length > 0)
						{
							foreach(int anotherUserId in userIds)
							{
								if (anotherUserId == aUserId)
									continue;
								
								DenyAccess(-1, anotherUserId);
							}
						}
					}
				}
				else if (aUserId == -1 && aCompanyId != -1) // devo estendere il divieto di accesso a tutti gli utenti nell'ambito di una data azienda
				{
					// Devo eliminare qualsiasi divieto di accesso all'oggetto per una data azienda (indipendentemente dall'utente)
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString();
					sqlCommand.ExecuteNonQuery();
					
					OnAccessAllowed(aCompanyId, -1);
					
					// Se esiste il divieto di accesso del tipo "a tutti gli utenti e per tutte le aziende" devo cancellarlo e 
					// inserire il divieto relativo ad ogni altra azienda

					// For UPDATE, INSERT, and DELETE statements, the return value of the sqlCommand.ExecuteNonQuery method is
					// the number of rows affected by the command.
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = -1";
					if (sqlCommand.ExecuteNonQuery() == 1)
					{
						OnAccessAllowed(-1, -1);

                        int[] companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
						if (companyIds != null && companyIds.Length > 0)
						{
							foreach(int anotherCompanyId in companyIds)
							{
								if (anotherCompanyId == aCompanyId)
									continue;
								
								DenyAccess(anotherCompanyId, -1);
							}
						}
					}
				}
				else // devo cancellare il divieto di accesso per un certo utente nell'ambito di una data azienda
				{
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString() + " AND UserId = " + aUserId.ToString();
					sqlCommand.ExecuteNonQuery();
	
					OnAccessAllowed(aCompanyId, aUserId);

					int[] companyIds = null;
					int[] userIds = null;

					// For UPDATE, INSERT, and DELETE statements, the return value of the sqlCommand.ExecuteNonQuery method is
					// the number of rows affected by the command.
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = -1";
					if (sqlCommand.ExecuteNonQuery() == 1)
					{
						OnAccessAllowed(-1, -1);

                        companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
                        userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
						if (companyIds != null && companyIds.Length > 0)
						{
							foreach(int anotherCompanyId in companyIds)
							{
								if (userIds != null && userIds.Length > 0)
								{
									foreach(int anotherUserId in userIds)
									{
										if (anotherCompanyId == aCompanyId && anotherUserId == aUserId)
											continue;
								
										DenyAccess(anotherCompanyId, anotherUserId);
									}
								}
								else if (anotherCompanyId != aCompanyId)							
									DenyAccess(anotherCompanyId, -1);
							}
						}
					}
					
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = -1 AND UserId = " + aUserId.ToString();
					if (sqlCommand.ExecuteNonQuery() == 1)
					{
						OnAccessAllowed(-1, aUserId);
						
						if (companyIds == null)
                            companyIds = SecurityLightManager.GetAllCompanyIds(systemDBConnection);
						if (companyIds != null && companyIds.Length > 0)
						{
							foreach(int anotherCompanyId in companyIds)
							{
								if (anotherCompanyId == aCompanyId)
									continue;
								
								DenyAccess(anotherCompanyId, aUserId);
							}
						}
					}
					
					sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE ObjectId = " + objectId.ToString() + " AND CompanyId = " + aCompanyId.ToString() + " AND UserId = -1";
					if (sqlCommand.ExecuteNonQuery() == 1)
					{
						OnAccessAllowed(aCompanyId, -1);
						
						if (userIds == null)
                            userIds = SecurityLightManager.GetAllUserIds(systemDBConnection, userNamesToSkip);
						if (userIds != null && userIds.Length > 0)
						{
							foreach(int anotherUserId in userIds)
							{
								if (anotherUserId == aUserId)
									continue;
								
								DenyAccess(aCompanyId, anotherUserId);
							}
						}
					}
				}

				DeleteAllAllowedObjectId(objectId, systemDBConnection);

				return true;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.AllowAccess: " + e.Message);

				return false;	
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public virtual bool IsAccessInUnattendedModeDefined(int aCompanyId, int aUserId)
		{
			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				return false;

			SqlDataReader unattendedModeAccessesReader = GetAccessAttributesReader();

			try
			{
				while (unattendedModeAccessesReader.Read())
				{
					int companyId = Convert.ToInt32(unattendedModeAccessesReader["CompanyId"]);
					int userId = Convert.ToInt32(unattendedModeAccessesReader["UserId"]);

					if ((aCompanyId == companyId || companyId == -1) && (aUserId == userId || userId == -1))
						return true;
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecuredCommandDBInfo.IsAccessToObjectDenied: " + e.Message);

				if (getAccessAttributesCommand != null)
				{
					getAccessAttributesCommand.Dispose();
					getAccessAttributesCommand = null;
				}
			}
			finally
			{
				if (unattendedModeAccessesReader != null && !unattendedModeAccessesReader.IsClosed)
					unattendedModeAccessesReader.Close();
			}

			return false;
		}

		//---------------------------------------------------------------------
		public virtual bool SetAccessInUnattendedMode(int aCompanyId, int aUserId, bool allowed)
		{
			return SetAccessAttributeFlag(aCompanyId, aUserId, AccessAttributes.UnattendedModeAllowed, allowed);
		}

		#endregion // SecuredCommandDBInfo public virtual methods

		#region SecuredCommandDBInfo public static methods
		
		//---------------------------------------------------------------------
		public static int GetObjectId(string aObjectNameSpace, SecuredCommandType aObjectType, SqlConnection aConnection)
		{
			return GetObjectId(aObjectNameSpace, aObjectType, aConnection, false);
		}
		
		//---------------------------------------------------------------------
		public static void DeleteAllAllowedObjectId(int aObjectId, SqlConnection aConnection)
		{
			if 
				(
				aObjectId == -1 ||
				aConnection == null || 
				aConnection.State != ConnectionState.Open
				)
				return;

			SqlCommand sqlCommand = null;

			try
			{
				string queryText = "SELECT COUNT(*) FROM MSD_SLDeniedAccesses WHERE ObjectId = " + aObjectId.ToString();

				sqlCommand = new SqlCommand(queryText, aConnection);

				int recordsCount = (int)sqlCommand.ExecuteScalar();

				if (recordsCount > 0)
					return;

				sqlCommand.CommandText = "DELETE FROM MSD_SLAccessAttributes WHERE ObjectId = " + aObjectId.ToString();
				sqlCommand.ExecuteNonQuery();

				sqlCommand.CommandText = "DELETE FROM MSD_SLObjects WHERE ObjectId = " + aObjectId.ToString();
				sqlCommand.ExecuteNonQuery();
			}
			catch(SqlException exception)
			{
				Debug.Fail("SqlException raised in SecuredCommandDBInfo.DeleteAllAllowedObjectId: " + exception.Message);
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToObjectDenied(string aNameSpaceTextWithoutType, SecuredCommandType aObjectType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			if 
				(
				aNameSpaceTextWithoutType == null || 
				aNameSpaceTextWithoutType == String.Empty || 
				aObjectType == SecuredCommandType.Undefined ||
				aConnection == null || 
				aConnection.State != ConnectionState.Open
				)
				return false;

			return IsAccessToObjectDenied(GetObjectId(aNameSpaceTextWithoutType, aObjectType, aConnection), aCompanyId, aUserId,  aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToFormDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.Form, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToBatchDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.Batch, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToDocumentDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToFormDenied(aNameSpaceTextWithoutType, aCompanyId, aUserId, aConnection) ||
				IsAccessToBatchDenied(aNameSpaceTextWithoutType, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToReportDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.Report, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToFunctionDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.Function, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToExcelDocumentDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.ExcelDocument, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToExcelTemplateDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.ExcelTemplate, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToWordDocumentDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.WordDocument, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToWordTemplateDenied(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectDenied(aNameSpaceTextWithoutType, SecuredCommandType.WordTemplate, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToObjectNameSpaceDenied(INameSpace aObjectNameSpace, int aCompanyId, int aUserId, SqlConnection aConnection)
		{
			if (aObjectNameSpace == null || !aObjectNameSpace.IsValid())
				return false;

			switch(aObjectNameSpace.NameSpaceType.Type)
			{
				case NameSpaceObjectType.Document:
					return IsAccessToDocumentDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.Report:
					return IsAccessToReportDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.Function:
					return IsAccessToFunctionDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.ExcelDocument:
					return IsAccessToExcelDocumentDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.ExcelTemplate:
					return IsAccessToExcelTemplateDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.WordDocument:
					return IsAccessToWordDocumentDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.WordTemplate:
					return IsAccessToWordTemplateDenied(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				default:
					break;
			}

			return false;
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToDocumentInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.Form, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToReportInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.Report, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToFunctionInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.Function, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToExcelDocumentInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.ExcelDocument, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToExcelTemplateInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.ExcelTemplate, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToWordDocumentInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.WordDocument, aCompanyId, aUserId, aConnection);
		}
		
		//---------------------------------------------------------------------
		public static bool IsAccessToWordTemplateInUnattendedModeAllowed(string aNameSpaceTextWithoutType, int aCompanyId, int aUserId,  SqlConnection aConnection)
		{
			return IsAccessToObjectInUnattendedModeAllowed(aNameSpaceTextWithoutType, SecuredCommandType.WordTemplate, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToObjectNameSpaceInUnattendedModeAllowed(INameSpace aObjectNameSpace, int aCompanyId, int aUserId, SqlConnection aConnection)
		{
			if (aObjectNameSpace == null || !aObjectNameSpace.IsValid())
				return false;

			switch(aObjectNameSpace.NameSpaceType.Type)
			{
				case NameSpaceObjectType.Document:
					return IsAccessToDocumentInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.Report:
					return IsAccessToReportInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.Function:
					return IsAccessToFunctionInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.ExcelDocument:
					return IsAccessToExcelDocumentInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.ExcelTemplate:
					return IsAccessToExcelTemplateInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.WordDocument:
					return IsAccessToWordDocumentInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				case NameSpaceObjectType.WordTemplate:
					return IsAccessToWordTemplateInUnattendedModeAllowed(aObjectNameSpace.GetNameSpaceWithoutType(), aCompanyId, aUserId, aConnection);

				default:
					break;
			}

			return false;
		}

		#endregion // SecuredCommandDBInfo public static methods

		#endregion // SecuredCommandDBInfo public methods
	}
}
