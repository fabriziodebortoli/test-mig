using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// RoleDb
	/// Classe per gestire i record della tabella MSD_CompanyRoles
	/// </summary>
	//=========================================================================
	public class RoleDb : DataBaseItem
	{
		#region Costruttori
		/// <summary>
		/// Costruttore 1
		/// (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public RoleDb()	{}
		
		/// <summary>
		/// Costruttore 2
		/// Imposto la stringa di connessione
		/// </summary>
		//---------------------------------------------------------------------
		public RoleDb (string connectionString)
		{
			ConnectionString = connectionString;
		}
		#endregion 

		#region Add - Inserimento di un nuovo Ruolo
		/// <summary>
		/// Add
		/// Inserisce un nuovo ruolo della tabella MSD_CompanyRoles
		/// </summary>
		/// <param name="Role">Nome del Ruolo</param>
		/// <param name="Description">Descrizione</param>
		/// <param name="CompanyId">Id dell'azienda</param>
		/// <param name="Disabled">true se il ruolo è disabilitato, 
		///							false altrimenti</param>
		//---------------------------------------------------------------------
		public bool Add(string role, string description, string companyId, bool disabled)
		{
			bool result = false;
			SqlTransaction  myTransRoleSql;
			SqlCommand myCommand	= new SqlCommand();
			myTransRoleSql			= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransRoleSql;
			
			try
			{
				string strQuery = 
				@"INSERT INTO MSD_CompanyRoles(Role, Description, CompanyId, LastModifyGrants, Disabled) 
                  VALUES 
                 (@Role, @Description, @CompanyId, @LastModifyGrants, @Disabled)"; 
			
				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@Role",		       role));
				myCommand.Parameters.Add(new SqlParameter("@Description",      description));
				myCommand.Parameters.Add(new SqlParameter("@CompanyId",		   Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants", DateTime.Now.Date));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",	       disabled));
				myCommand.ExecuteNonQuery();
				myTransRoleSql.Commit();
				result = true;
			}
			catch (SqlException sqlException)
			{
				myTransRoleSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.RoleInserting, role), extendedInfo);
			}

			return result;
		}
		#endregion 

		#region Modify - Modifica di un Ruolo esistente
		/// <summary>
		/// Modify
		/// Modifica un ruolo di una azienda nella tabella MSD_CompanyRoles
		/// </summary>
		/// <param name="RoleId">Id Del ruolo</param>
		/// <param name="Role">Nome del ruolo</param>
		/// <param name="Description">Descrizione del ruolo</param>
		/// <param name="CompanyId">Id dell'azienda</param>
		/// <param name="Disabled">true se è disabilitato,
		///					       false altrimenti</param>
		//---------------------------------------------------------------------
		public bool Modify(string roleId, string role, string description, string companyId, bool disabled)
		{
			bool result = false;
			SqlCommand myCommand      = new SqlCommand();
			SqlTransaction myTransSql = CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	  = CurrentSqlConnection;
			myCommand.Transaction	  = myTransSql;
			
			try
			{
				string strQuery = 
				@"UPDATE MSD_CompanyRoles SET Role = @Role, Description = @Description, LastModifyGrants = @LastModifyGrants,
                  Disabled = @Disabled WHERE RoleId = @RoleId AND CompanyId = @CompanyId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@RoleId",	       Int32.Parse(roleId)));
				myCommand.Parameters.Add(new SqlParameter("@Role",		       role));
				myCommand.Parameters.Add(new SqlParameter("@Description",      description));
				myCommand.Parameters.Add(new SqlParameter("@CompanyId",        Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants", DateTime.Now.Date));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",		   disabled));
				myCommand.ExecuteNonQuery();                           
				myTransSql.Commit();
				result = true;
			}
			catch(SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.RoleModify, role), extendedInfo);
			}

			return result;
		}
		#endregion
		
		#region Delete - Cancellazione di un Ruolo 
		/// <summary>
		/// Delete
		/// Cancella il ruolo identificato da roleId della companyId richiamando
		/// la stored procedure MSD_DeleteCompanyRole
		/// </summary>
		/// <param name="roleId">RoleId del ruolo da cancellare</param>
		/// <param name="companyId">CompanyId del ruolo da cancellare</param>
		//---------------------------------------------------------------------
		public bool Delete(string roleId, string companyId)
		{
			bool result = false;
			SqlCommand myCommand  = new SqlCommand();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.CommandText = "MSD_DeleteCompanyRole";
			myCommand.CommandType = CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@par_companyid", Int32.Parse(companyId));
			myCommand.Parameters.AddWithValue("@par_roleid",    Int32.Parse(roleId));
			
			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.Delete");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.RoleDeleted, roleId), extendedInfo);
			}

			return result;
		}
		#endregion

		#region Clone - Clonazione di un Ruolo 
		/// <summary>
		/// Clone
		/// Clona il ruolo di una company richiamando la stored procedure MSD_CloneCompanyRole
		/// </summary>
		/// <param name="srcCompanyId">Id dell'azienda sorgente</param>
		/// <param name="srcRoleId">Id del Ruolo sorgente</param>
		/// <param name="dstCompanyId">Id dell'azienda di destinazione</param>
		/// <param name="dstRole">nome del ruolo di destinazione</param>
		//---------------------------------------------------------------------
		public bool Clone(string srcCompanyId, string srcRoleId, string dstCompanyId, string dstRole)
		{
			bool result = false;
			SqlCommand myCommand  = new SqlCommand();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.CommandText = "MSD_CloneCompanyRole";
			myCommand.CommandType = CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@par_srccompanyid", Int32.Parse(srcCompanyId));
			myCommand.Parameters.AddWithValue("@par_srcroleid",    Int32.Parse(srcRoleId));
			myCommand.Parameters.AddWithValue("@par_dstcompanyid", Int32.Parse(dstCompanyId));
			myCommand.Parameters.AddWithValue("@par_dstrolename",  dstRole.Trim());
			
			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.Clone");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.RoleCloned, dstRole), extendedInfo);
			}

			return result;
		}
		#endregion

		#region Funzioni di Ricerca e Selezione

		#region SelectIdRole - Ritorna l'Id del ruolo identificato da RoleName assegnato all'azienda companyId
		/// <summary>
		/// SelectIdRole
		/// Ritorna l'Id del ruolo identificato da RoleName assegnato all'azienda companyId
		/// </summary>
		//---------------------------------------------------------------------
		public int SelectIdRole(string companyId, string roleName)
		{
			int roleId = -1;

			try
			{
				roleId = GetIdRole(companyId, roleName);
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.SelectIdRole");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleReading, extendedInfo);
			}

			return roleId;
		}

		/// <summary>
		/// GetIdRole
		/// </summary>
		//---------------------------------------------------------------------
		private int GetIdRole(string companyId, string roleName)
		{
			SqlDataReader mylocalDataReader = null;
			string myQuery = @"SELECT MSD_CompanyRoles.RoleId FROM MSD_CompanyRoles
								WHERE MSD_CompanyRoles.CompanyId = @CompanyId AND MSD_CompanyRoles.Role = @RoleName";
			
			int roleId = -1;
						
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@RoleName", roleName));
				mylocalDataReader = myCommand.ExecuteReader();
				while(mylocalDataReader.Read())
					roleId = Convert.ToInt32(mylocalDataReader["RoleId"].ToString());				
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetIdRole");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyRoles"), extendedInfo);
			}
			finally
			{
				if (mylocalDataReader != null)
					mylocalDataReader.Close();
			}

			return roleId;
		}
		#endregion

		#region SelectAllRolesOfCompany - Trova tutti i ruoli di una azienda 
		/// <summary>
		/// SelectAllRolesOfCompany
		/// Seleziona tutti i Ruoli di una azienda e li mette in un ArrayList
		/// </summary>
		/// <param name="roles">ArrayList con i ruoli trovati</param>
		/// <param name="companyId">CompanyId dei ruoli che si vuole cercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllRolesOfCompany (out ArrayList roles, string companyId)
		{
			ArrayList localRoles = new ArrayList();
			bool mySuccessFlag   = false;
			
			try
			{
				SqlDataReader myDataReader;
				if (GetAllRolesOfCompany(out myDataReader, companyId))
				{
					while(myDataReader.Read())
					{
						RoleItem roleItem		  = new RoleItem();
						roleItem.CompanyId		  = myDataReader["CompanyId"].ToString();
						roleItem.RoleId			  = myDataReader["RoleId"].ToString();
						roleItem.Company		  = myDataReader["Company"].ToString();
						roleItem.Role			  = myDataReader["Role"].ToString();
						roleItem.Description	  = myDataReader["Description"].ToString();
						roleItem.LastModifyGrants = myDataReader["LastModifyGrants"].ToString();
						roleItem.Disabled	      = bool.Parse(myDataReader["Disabled"].ToString());
                        roleItem.ReadOnly         = bool.Parse(myDataReader["ReadOnly"].ToString());
						localRoles .Add(roleItem);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.SelectAllRolesOfCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleReading, extendedInfo);
				mySuccessFlag = false;
			}

			roles = localRoles;
			return mySuccessFlag;
		}
		
		/// <summary>
		/// GetAllRolesOfCompany
		/// Riempie un dataReader con i dati dei ruoli di una azienda
		/// </summary>
		/// <param name="myDataReader">DataReader con i ruoli</param>
		/// <param name="companyId">CompanyId dei ruoli</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllRolesOfCompany(out SqlDataReader myDataReader, string companyId)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = @"SELECT * FROM MSD_CompanyRoles, MSD_Companies 
								WHERE MSD_CompanyRoles.CompanyId = @CompanyId AND 
								MSD_CompanyRoles.CompanyId = MSD_Companies.CompanyId ORDER BY MSD_CompanyRoles.Role";
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllRolesOfCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyRoles, MSD_Companies"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region GetAllRoleFieldsById - Seleziona un ruolo tramite il suo Id 
		/// <summary>
		/// GetAllRoleFieldsById
		/// Seleziona i dati di un Ruolo assegnato a un azienda, identificato attraverso la copia di chiavi companyId, RoleId
		/// </summary>
		/// <param name="roles">ArrayList con i dati del ruolo</param>
		/// <param name="roleId">RoleId che identifica il ruolo</param>
		/// <param name="companyId">CompanyId che identifica l'azienda di 
		///						    appartenenza del ruolo</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllRoleFieldsById(out ArrayList roles, string roleId, string companyId)
		{
			ArrayList localRoles = new ArrayList();
			RoleItem roleItem    = new RoleItem();
			bool mySuccessFlag   = false;
			
			try
			{
				SqlDataReader myDataReader;
				if (GetRoleId(out myDataReader, roleId, companyId))
				{
					while(myDataReader.Read())
					{
						roleItem.CompanyId   = myDataReader["CompanyId"].ToString();
						roleItem.RoleId		 = myDataReader["RoleId"].ToString();
						roleItem.Role	     = myDataReader["Role"].ToString();
						roleItem.Description = myDataReader["Description"].ToString();
						roleItem.Disabled	 = bool.Parse(myDataReader["Disabled"].ToString());
						localRoles.Add(roleItem);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.GetAllRoleFieldsById");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleReading, extendedInfo);
				mySuccessFlag = false;
			}

			roles = localRoles;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetRoleId
		/// Riempie un dataReader con i dati relativi al ruolo assegnato a una azienda, reperito attraverso 
		/// la coppia di chiavi companyId, roleId
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati del ruolo</param>
		/// <param name="roleId">RoleId che identifica il ruolo</param>
		/// <param name="companyId">CompanyId che identifica l'azienda a cui
		///					        il ruolo appartiene</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------------
		public bool GetRoleId(out SqlDataReader myDataReader, string roleId, string companyId)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = @"SELECT * FROM MSD_CompanyRoles WHERE roleId = @RoleId AND companyId = @CompanyId";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@Roleid",    Int32.Parse(roleId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetRoleId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyRoles"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region ExistRole - True se il ruolo roleName esiste nell'azienda companyId
		/// <summary>
		/// ExistRole
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistRole(string companyId, string roleName)
		{
			int result = 0;
			string myQuery = @"SELECT COUNT(*) FROM MSD_CompanyRoles WHERE CompanyId = @CompanyId 
							  AND Role = @RoleName AND Disabled = @Disabled";
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", companyId);
				myCommand.Parameters.AddWithValue("@RoleName", roleName);
				myCommand.Parameters.AddWithValue("@Disabled", false);
				result = (int) myCommand.ExecuteScalar();
			}
			catch(SqlException)
			{}
			
			return (result > 0);
		}
		#endregion

		#endregion

		#region LastRoleIdInserted - Trova ultimo ruolo inserito 
		/// <summary>
		/// LastRoleIdInserted
		/// Ritorna la RoleId dell'ultimo Ruolo inserito (è un campo autoincrementato, quindi il Max corrisponde all'ultimo)
		/// </summary>
		/// <returns>result, rappresenta la RoleId</returns>
		//---------------------------------------------------------------------
		public string LastRoleIdInserted()
		{
			int result = 0;
			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;

			string strQuery = "SELECT MAX(RoleId) FROM MSD_CompanyRoles";

			try
			{
				myCommand.CommandText = strQuery;
				result = (int) myCommand.ExecuteScalar();
			}
			catch( SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "RoleDb.LastRoleIdInserted");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyRoles"), extendedInfo);
				if (myCommand != null)
					myCommand.Dispose();
				return result.ToString();
			}

			myCommand.Dispose();
			return result.ToString();
		}
		#endregion
	}

	/// <summary>
	/// RoleItem
	/// Classe che gestisce l'elemento ruolo (letto dal db)
	/// Utilizzata nelle liste
	/// </summary>
	//=========================================================================
	public class RoleItem
	{
		#region Variabili membro (private)
		private string companyId		 = string.Empty;
		private string company			 = string.Empty;
		private string roleId			 = string.Empty;
		private string role				 = string.Empty;
		private string description		 = string.Empty;
		private string lastModifyGrants	 = string.Empty;
		private bool   disabled          = false;
        private bool   readOnly          = false;
		#endregion

		#region Proprietà
		//Properties
		//---------------------------------------------------------------------
		public string CompanyId		   { get { return companyId;		} set { companyId		 = value; } }
		public string Company		   { get { return company;			} set { company			 = value; } }
		public string RoleId		   { get { return roleId;			} set { roleId			 = value; } }
		public string Role			   { get { return role;				} set { role			 = value; } }
		public string Description	   { get { return description;		} set { description		 = value; } }
		public string LastModifyGrants { get { return lastModifyGrants; } set { lastModifyGrants = value; } }
		public bool	  Disabled         { get { return disabled;         } set { disabled         = value; } }
        public bool   ReadOnly         { get { return readOnly;         } set { readOnly         = value; } }
		#endregion

		#region Costruttori
		/// <summary>
		/// Costruttore 1 (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public RoleItem() {}

		/// <summary>
		/// Costruttore 2
		/// </summary>
		//---------------------------------------------------------------------
		public RoleItem
			(
			string companyId, 
			string company,
			string roleId, 
			string role,
			string description, 
			string lastModify,
			bool  disabled,
            bool  readOnly
			)
		{
			this.CompanyId		  = companyId;
			this.Company		  = company;
			this.RoleId			  = roleId;
			this.Role			  = role;
			this.Description	  = description;
			this.LastModifyGrants = lastModify;
			this.Disabled		  = disabled;
            this.ReadOnly         = readOnly;
		}
		#endregion
	}
}