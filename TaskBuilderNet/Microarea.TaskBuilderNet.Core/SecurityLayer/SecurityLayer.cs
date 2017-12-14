using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.SecurityLayer
{

	/// <summary>
	/// Summary description for Security.
	/// </summary>
		public class Security : ISecurity
		{
			private int				companyId					= -1;
			private int				userId						= -1;
			private bool			isAdmin						= false;
			private bool			isRole						= false;
			private bool			securityLicensed			= false;
			private bool			companyProtected			= false;

			private SqlConnection	sqlConnection				= null;
			private bool			bConnectionSelfEstablished	= true;
			private bool			disposed = false;	// Track whether Dispose has been called.

			//---------------------------------------------------------------------
			public bool	IsSecurityLicensed			{ get { return securityLicensed; }}
			public bool	IsAdmin						{ get { return isAdmin; }}
			public bool	IsCompanyProtected			{ get { return companyProtected; }}

			//---------------------------------------------------------------------
			public Security (SqlConnection aSqlConnection, int aCompanyId, int aUserId, bool securityLicensed, bool isRoleId) 
			{
				sqlConnection = aSqlConnection;

				companyId = aCompanyId;
				userId    = aUserId;

				this.securityLicensed = securityLicensed;
				isRole = isRoleId;
			
				if (!isRole)
					CheckAdmin();
			}

			//---------------------------------------------------------------------
			public Security(string aConnectionString, string aCompany, string aUser, bool securityLicensed)
			{
				try
				{
					sqlConnection = new SqlConnection(aConnectionString);
					sqlConnection.Open();

					bConnectionSelfEstablished = true;
					this.securityLicensed = securityLicensed;
				
				}
				catch (SqlException e)
				{
					Debug.Fail("SqlException raised in MenuSecurityFilter constructor: " + e.Message);
					if (sqlConnection != null)
					{
						if (sqlConnection.State == ConnectionState.Open)
							sqlConnection.Close();
					}
					sqlConnection = null;
					bConnectionSelfEstablished = false;
				}

				FindLoginProperties(aCompany, aUser);
			}
		
		
			//------------------------------------------------------------------------------
			~Security()	
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
					// se arrivo dal distruttore non so l'ordine di distruzione
					if(disposing && bConnectionSelfEstablished)
					{
						if (sqlConnection != null)
						{
							sqlConnection.Close();
							sqlConnection = null;
						}
					}

				}
				disposed = true;         
			}

			//---------------------------------------------------------------------
			private bool FindLoginProperties(string aCompany, string aUser)
			{
				if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
					return false;
			
				// Per specificare nella query il valore del nome della company e dello user, 
				// trattandosi di stringhe,utilizzo dei parametri (v. problemi Unicode)
				string selectQuery = @"SELECT MSD_CompanyLogins.CompanyId, MSD_CompanyLogins.LoginId, MSD_CompanyLogins.Admin FROM 
										MSD_CompanyLogins INNER JOIN
										MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId INNER JOIN
										MSD_Companies ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId
										WHERE MSD_Companies.Company = @Company AND MSD_Logins.Login = @User";

				SqlCommand mySqlCommand = null;
				SqlDataReader myReader = null;
				try
				{
					mySqlCommand = new SqlCommand(selectQuery, sqlConnection);

                    mySqlCommand.Parameters.AddWithValue("@Company", aCompany);
					mySqlCommand.Parameters.AddWithValue("@User", aUser);
				
					myReader = mySqlCommand.ExecuteReader();
					myReader.Read();

					companyId = Convert.ToInt32(myReader["CompanyId"]);
					userId = Convert.ToInt32(myReader["LoginId"]);
					isAdmin = Convert.ToBoolean(myReader["Admin"]);

					return true;
				}
				catch (SqlException e)
				{
					Debug.Fail("SqlException raised in MenuSecurityFilter.FindLoginProperties: " + e.Message);
					return false;
				}
				finally
				{
					if (myReader != null && !myReader.IsClosed)
						myReader.Close();
					if (mySqlCommand != null)
						mySqlCommand.Dispose();
				}
			}
			//---------------------------------------------------------------------
			private int GetObjectTypeFromDB(SecurityType aType)
			{
				if 
					(
					aType == SecurityType.Undefined || 
					sqlConnection == null || 
					sqlConnection.State != ConnectionState.Open
					)
					return -1;
			
				SqlCommand mySqlCommand = null;
				SqlDataReader myReader = null;
				try
				{
					string sSelect = @"SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + ((int)aType).ToString();
					mySqlCommand = new SqlCommand(sSelect, sqlConnection);

					myReader = mySqlCommand.ExecuteReader();
					myReader.Read();
					int typeId = Convert.ToInt32(myReader["TypeId"]);
					return typeId;
				}
				catch (SqlException e)
				{
					Debug.Fail("SqlException raised in MenuSecurityFilter GetObjectTypeFromDB method: " + e.Message);
					return -1;
				}
				finally
				{
					if (myReader != null && !myReader.IsClosed)
						myReader.Close();
					if (mySqlCommand != null)
						mySqlCommand.Dispose();
				}
			}
		
			//---------------------------------------------------------------------
			private void CheckAdmin()
			{
				if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
					return;

				SqlCommand mySqlCommand = null;
				SqlDataReader myReader = null;
				try
				{
					string sSelect =@"SELECT MSD_CompanyLogins.admin FROM 
								MSD_CompanyLogins
							WHERE MSD_CompanyLogins.CompanyId = " + companyId.ToString() +  
						" AND MSD_CompanyLogins.loginId = " + userId.ToString() ;

					mySqlCommand = new SqlCommand(sSelect, sqlConnection);
					myReader = mySqlCommand.ExecuteReader();
					myReader.Read();

					isAdmin = Convert.ToBoolean(myReader["Admin"]);
				}
				catch (SqlException e)
				{
					Debug.Fail("SqlException raised in MenuSecurityFilter CheckAdmin method: " + e.Message);
				}
				finally
				{
					if (myReader != null && !myReader.IsClosed)
						myReader.Close();
					if (mySqlCommand != null)
						mySqlCommand.Dispose();
				}
			}

			//---------------------------------------------------------------------
			public bool ExistExecuteGrant(string nameSpace, SecurityType aType)
			{
				//Prendo il tipo di oggetto
				int type = GetObjectTypeFromDB(aType);

				return ExistExecuteGrant(nameSpace, type);
			}

			
			//---------------------------------------------------------------------
			public bool ExistGrant(int grants, GrantType type)
			{	
				if (!securityLicensed)
					return true;

				return (grants & ((int)type)) == (int)type ;
			}

			//---------------------------------------------------------------------
			public bool ExistExecuteGrant(string aNameSpace, int type)
			{
				int grants = 0;
				int isProtected = 0;

				if (!securityLicensed)
					return true;

				bool bOk = GetObjectUserGrant(aNameSpace, type, out isProtected, out grants);
				if (!bOk)
					return true; //evita di bloccare tutto se c'e' una anomalia

				return isProtected == 1 ? (grants & 1) == 1 : true;
			}


			//---------------------------------------------------------------------
			public bool GetObjectUserGrant(string aNameSpace, int type, out	int isProtected, out int grants)
			{
				grants = 0;
				isProtected = 0;

				if (!securityLicensed)
					return true;

				if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
					return false;
			
				SqlCommand storedProcCommand = null;

				try
				{
					storedProcCommand = new SqlCommand();
					storedProcCommand.Connection  = sqlConnection;
					storedProcCommand.CommandType = CommandType.StoredProcedure;

					//Parametri imput
					storedProcCommand.Parameters.Add("@par_companyid", SqlDbType.Int).Value = companyId;
					if (isRole)
					{
						storedProcCommand.CommandText = "MSD_GetObjectRoleGrant";
						storedProcCommand.Parameters.Add("@par_roleid", SqlDbType.Int).Value = userId;
					}	
					else
					{
						storedProcCommand.CommandText = "MSD_GetObjectUserGrant";
						storedProcCommand.Parameters.Add("@par_userid", SqlDbType.Int).Value = userId;
					}

					storedProcCommand.Parameters.Add("@par_objectNameSpace", SqlDbType.NVarChar).Value   = (aNameSpace != null) ? aNameSpace : "";
					storedProcCommand.Parameters.Add("@par_objectType", SqlDbType.Int).Value   = type;

					//Parametri Output
					storedProcCommand.Parameters.Add("@parout_isprotected", SqlDbType.Int).Direction = ParameterDirection.Output;
					storedProcCommand.Parameters.Add("@parout_grant", SqlDbType.Int).Direction = ParameterDirection.Output;

					storedProcCommand.ExecuteNonQuery();

					grants = Convert.ToInt32(storedProcCommand.Parameters["@parout_grant"].Value);
					isProtected = Convert.ToInt32(storedProcCommand.Parameters["@parout_isprotected"].Value);
				}
				catch(SqlException sqlException)
				{
					Debug.Fail("SqlException raised in MenuSecurityFilter.ExistOSLGrants: " + sqlException.Message);
					return false;
				}
				finally
				{
					if (storedProcCommand != null)
						storedProcCommand.Dispose();
				}
				return true;
			}

			//--------------------------------------------------------------------------
			static public SecurityType GetSecurityTypeByNSType(NameSpaceObjectType nsType)
			{
				switch (nsType)
				{
					case NameSpaceObjectType.Report:
						return SecurityType.Report;

					case NameSpaceObjectType.Function:
						return SecurityType.Function;

                    //case NameSpaceObjectType.Hotlink:
                    //    return SecurityType.HotLink;
                    case NameSpaceObjectType.HotKeyLink:
                        return SecurityType.HotKeyLink;

					case NameSpaceObjectType.Document:
					case NameSpaceObjectType.ExcelDocument:
					case NameSpaceObjectType.WordDocument:
					case NameSpaceObjectType.ExcelTemplate:
					case NameSpaceObjectType.WordTemplate:
						return SecurityType.DataEntry;

					default:
						return 0;
				}
			}

		}
}
