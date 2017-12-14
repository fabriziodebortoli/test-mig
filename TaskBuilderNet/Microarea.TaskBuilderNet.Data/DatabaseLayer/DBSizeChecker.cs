using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
    ///<summary>
    /// DBSizeChecker
    /// Estrae tutte le company censite in un database di sistema che abbiano il provider SQL
    /// Compone la stringa di connessione, si connette e verifica che la Size dei file di dati non
    /// ecceda i 2GB
    ///</summary>
    //---------------------------------------------------------------------------
    public class DBSizeChecker
    {
        private string sysDBConnectionString = string.Empty;

        /// <summary>
        /// DBSizeChecker constructor
        /// </summary>
        /// <param name="sysDBConnectionString">connection string to systemdb</param>
        //---------------------------------------------------------------------------
        public DBSizeChecker(string sysDBConnectionString)
        {
            this.sysDBConnectionString = sysDBConnectionString;
        }

        ////---------------------------------------------------------------------------
        //public bool AreDatabasesValidForUpgrade(out StringCollection invalidCompanies)
        //{
        //    invalidCompanies = new StringCollection();

        //    StringCollection dbConnectionStrings = GetCompanyDBConnectionStrings();
        //    TBConnection myConnection = null;

        //    foreach (string dbConnection in dbConnectionStrings)
        //    {
        //        try
        //        {
        //            myConnection = new TBConnection(DBMSType.SQLSERVER);
        //            myConnection.ConnectionString = dbConnection;
        //            myConnection.Open();
        //        }
        //        catch (TBException)
        //        { 
        //            if (myConnection != null && 
        //                (myConnection.State == ConnectionState.Open || myConnection.State == ConnectionState.Broken))
        //            {
        //                myConnection.Close();
        //                myConnection.Dispose();
        //            }

        //            continue;
        //        }

        //        DatabaseCheckError dbError =
        //            TBCheckDatabase.CheckDatabaseVersion(myConnection, DBNetworkType.Small);

        //        if (dbError == DatabaseCheckError.DBSizeError)
        //            invalidCompanies.Add(myConnection.Database);

        //        if (myConnection != null && myConnection.State == ConnectionState.Open)
        //        {
        //            myConnection.Close();
        //            myConnection.Dispose();
        //        }
        //    }

        //    return (invalidCompanies.Count == 0);
        //}

        //---------------------------------------------------------------------------
        private StringCollection GetCompanyDBConnectionStrings()
        {
            ArrayList companiesInfoList = new ArrayList();
            StringCollection connections = new StringCollection();

            SqlConnection sysConn = new SqlConnection();

            try
            {
                sysConn.ConnectionString = sysDBConnectionString;
                sysConn.Open();
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                return connections;
            }

            string query = @"SELECT MSD_Companies.CompanyID, MSD_Companies.Company, MSD_Companies.CompanyDBServer, MSD_Companies.CompanyDBName, 
                            MSD_Companies.ProviderId, MSD_Companies.CompanyDBOwner, MSD_Providers.Provider
                            FROM MSD_Companies, MSD_Providers
                            WHERE MSD_Companies.Disabled = @Disabled AND
                            MSD_Companies.ProviderId = MSD_Providers.ProviderId";

            SqlCommand selectCompaniesCommand = new SqlCommand();
            SqlDataReader aSqlDataReader = null;

            try
            {
                selectCompaniesCommand.CommandText = query;
                selectCompaniesCommand.Connection = sysConn;
                selectCompaniesCommand.Parameters.AddWithValue("@Disabled", false); //??

                aSqlDataReader = selectCompaniesCommand.ExecuteReader();

                CompanyConnectionInfo companyInfo = null;

                while (aSqlDataReader.Read())
                {
                    if (string.Compare((string)aSqlDataReader["Provider"], NameSolverDatabaseStrings.SQLOLEDBProvider, true, CultureInfo.InvariantCulture) != 0 &&
                        string.Compare((string)aSqlDataReader["Provider"], NameSolverDatabaseStrings.SQLODBCProvider, true, CultureInfo.InvariantCulture) != 0)
                        continue;
                    
                    companyInfo = new CompanyConnectionInfo();

                    companyInfo.CompanyID = (int)aSqlDataReader["CompanyID"];
                    companyInfo.CompanyName  = (string)aSqlDataReader["Company"];
                    companyInfo.DBName = (string)aSqlDataReader["CompanyDBName"];
                    companyInfo.DBServer = (string)aSqlDataReader["CompanyDBServer"];
                    companyInfo.ProviderId = (int)aSqlDataReader["ProviderID"];
                    companyInfo.CompanyDBOwner = (int)aSqlDataReader["CompanyDBOwner"];
                    companyInfo.ProviderName = (string)aSqlDataReader["Provider"];
                    
                    companiesInfoList.Add(companyInfo);
                }
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.Message);
                return connections;
            }
            finally
            {
                if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                    aSqlDataReader.Close();
            }

            SqlCommand selectCompanyLoginDataCommand = null;
            SqlDataReader companyLoginDataReader = null;
            string selectQuery = string.Empty;

            foreach (CompanyConnectionInfo cci in companiesInfoList)
            {
   				if (cci.CompanyDBOwner > 0)
				{
					selectQuery = "SELECT MSD_CompanyLogins.DBUser,";
					selectQuery += " MSD_CompanyLogins.DBPassword,";
					selectQuery += " MSD_CompanyLogins.DBWindowsAuthentication";
					selectQuery += " FROM MSD_CompanyLogins WHERE MSD_CompanyLogins.CompanyId =" + cci.CompanyID;
					selectQuery += " AND MSD_CompanyLogins.LoginId =" + cci.CompanyDBOwner;
				
                    try
                    {
                        selectCompanyLoginDataCommand = new SqlCommand(selectQuery, sysConn);
					    companyLoginDataReader = selectCompanyLoginDataCommand.ExecuteReader();

					    if (companyLoginDataReader != null && companyLoginDataReader.Read())
					    {
							cci.CompanyDBOwnerLogin	=	(companyLoginDataReader["DBUser"] != System.DBNull.Value) 
												    ? (string)companyLoginDataReader["DBUser"] 
												    : string.Empty;
						    cci.CompanyDBOwnerPwd	=	(companyLoginDataReader["DBPassword"] != System.DBNull.Value) 
												    ? Crypto.Decrypt((string)companyLoginDataReader["DBPassword"]) 
												    : string.Empty;
						    cci.CompanyDBOwnerIsNT	=	(companyLoginDataReader["DBWindowsAuthentication"] != System.DBNull.Value) 
												    ? (bool)companyLoginDataReader["DBWindowsAuthentication"] 
												    : false;
					    }

	    				companyLoginDataReader.Close();
                        
                        if (cci.ConnectionString != null && cci.ConnectionString.Length > 0)
                            connections.Add(cci.ConnectionString);
                    }
			        catch(SqlException)
			        {		
				        continue;
			        }
                    finally
                    {
                        if (companyLoginDataReader != null && !companyLoginDataReader.IsClosed)
                            companyLoginDataReader.Close();
                    }
                }
			}

            return connections;
        }
    }

    ///<summary>
    /// CompanyConnectionInfo
    /// Classe per tenere traccia delle informazioni relative all'azienda
    ///</summary>
    //-----------------------------------------------------------------------
    public class CompanyConnectionInfo
    {
        // informazioni relative all'azienda
        public int CompanyID = -1;
        public string CompanyName = string.Empty;
        public string DBName = string.Empty;
        public string DBServer = string.Empty;
        public int CompanyDBOwner = -1;
        public int ProviderId = -1;
        public string ProviderName = string.Empty;
        
        // informazioni relative al dbowner per connettermi all'azienda
        public string CompanyDBOwnerLogin = string.Empty;
        public string CompanyDBOwnerPwd	= string.Empty;
        public bool CompanyDBOwnerIsNT	= false;

        // Proprietà che ritorna la stringa di connessione alla company
        //-----------------------------------------------------------------------
        public string ConnectionString
        {
            get 
            {
                if (CompanyDBOwnerIsNT &&
                    (DBServer == null || DBServer.Length == 0 ||
                    DBName == null || DBName.Length == 0))
                    return string.Empty;

                if (!CompanyDBOwnerIsNT &&
                    (DBServer == null || DBServer.Length == 0 ||
                    DBName == null || DBName.Length == 0 ||
                    CompanyDBOwnerLogin == null || CompanyDBOwnerLogin.Length == 0))
                    return string.Empty;

                return 
                    (CompanyDBOwnerIsNT) 
				    ? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, DBServer, DBName)
				    : string.Format(NameSolverDatabaseStrings.SQLConnection, DBServer, DBName, CompanyDBOwnerLogin, CompanyDBOwnerPwd);
            }
        }

        //-----------------------------------------------------------------------
        public CompanyConnectionInfo() { }
    }
}
