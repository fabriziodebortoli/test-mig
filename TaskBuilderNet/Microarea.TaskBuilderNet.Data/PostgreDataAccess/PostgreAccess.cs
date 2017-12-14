using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Npgsql;

namespace Microarea.TaskBuilderNet.Data.PostgreDataAccess
{
    /// <summary>
    /// PostgreAccess.
    /// Funzioni SQL per postgre
    /// @@Anastasia simplicimente modificato il file TransactSqlAccess per SQL server
    /// </summary>
    // ========================================================================
    public class PostgreAccess
    {
        #region Private Variables
        //---------------------------------------------------------------------
        private string currentStringConnection = string.Empty;
        private NpgsqlConnection currentConnection = null;
        private string nameSpace = string.Empty;

        private Diagnostic diagnostic = new Diagnostic("PostgreAccess");
        #endregion

        #region Properties
        public string CurrentStringConnection { get { return currentStringConnection; } set { currentStringConnection = value; } }

        public string NameSpace { get { return nameSpace; } set { nameSpace = value; } }

        public NpgsqlConnection CurrentConnection { get { return currentConnection; } set { currentConnection = value; } }
        public Diagnostic Diagnostic { get { return diagnostic; } }
        #endregion

        #region Delegates and Events
        public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string server);
        public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

        public delegate void AddUserAuthenticatedFromConsole(string login, string password, string server, DBMSType dbType);
        public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

        public delegate string GetUserAuthenticatedPwdFromConsole(string login, string server);
        public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

        public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
        public event CallHelpFromPopUp OnCallHelpFromPopUp;

        //---------------------------------------------------------------------
        //public delegate void EnableProgressBar(object sender);
        //public event EnableProgressBar OnEnableProgressBar;

        //public delegate void DisableProgressBar(object sender);
        //public event DisableProgressBar OnDisableProgressBar;

        //public delegate void SetProgressBarStep(object sender, int step);
        //public event SetProgressBarStep OnSetProgressBarStep;

        //public delegate void SetProgressBarValue(object sender, int currentValue);
        //public event SetProgressBarValue OnSetProgressBarValue;

        public delegate void SetProgressBarText(object sender, string message);
        public event SetProgressBarText OnSetProgressBarText;

        //public delegate void SetCyclicStepProgressBar();
        //public event SetCyclicStepProgressBar OnSetCyclicStepProgressBar;
        #endregion

        #region Constructor (empty)
        /// <summary>
        /// Constructor PostgreAccess
        /// </summary>
        //---------------------------------------------------------------------
        public PostgreAccess()
        {
        }
        #endregion

        #region Funzioni per la Connessione (Check e Open)

        #region TryToConnect - Prova ad aprire una connessione
        /// <summary>
        /// TryToConnect
        /// </summary>
        //---------------------------------------------------------------------
        public bool TryToConnect()
        {
            bool success = false;
            NpgsqlConnection testConnection = null;

            try
            {
                //tento la connessione
                testConnection = new NpgsqlConnection(CurrentStringConnection);
                testConnection.Open();
                success = true;
            }
            catch (NpgsqlException ex)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
                extendedInfo.Add(DatabaseLayerStrings.Server, ex.Source);
                //extendedInfo.Add(DatabaseLayerStrings.Number, ex.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "TryToConnect");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseConnection, DatabaseLayerConsts.postgreMasterDatabase), extendedInfo);
                return false;
            }
            finally
            {
                if (testConnection != null && testConnection.State == ConnectionState.Open)
                {
                    testConnection.Close();
                    testConnection.Dispose();
                }
            }

            return success;
        }
        #endregion

        #region OpenConnection - Apre la connessione impostata dalla stringa
        /// <summary>
        /// OpenConnection
        /// Apre la connessione impostata dalla stringa
        /// </summary>
        //---------------------------------------------------------------------
        public NpgsqlConnection OpenConnection()
        {
            CurrentConnection = new NpgsqlConnection(CurrentStringConnection);

            try
            {
                CurrentConnection.Open();
            }
            catch (NpgsqlException ex)
            {
                Debug.WriteLine(ex.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, ex.Where);
                //extendedInfo.Add(DatabaseLayerStrings.Number, ex.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "OpenConnection");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseConnection, DatabaseLayerConsts.postgreMasterDatabase), extendedInfo);
                CurrentConnection = null;
                return null;
            }

            return CurrentConnection;
        }
        #endregion

        #endregion


        public void CloseConnection()
        {
            

            try
            { 
                if (CurrentConnection !=null && CurrentConnection.State==ConnectionState.Open)
                    CurrentConnection.Close();
            }
            catch (NpgsqlException ex)
            {
                Debug.WriteLine(ex.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, ex.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, ex.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "CloseConnection");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseConnection, DatabaseLayerConsts.postgreMasterDatabase), extendedInfo);
                CurrentConnection = null;
                
            }
        }
        #region Funzioni di Verifica esistenza di Database, Ruoli e Utenti

        #region ExistLogin -  Si connette al master e verifica se la loginName esiste
        /// <summary>
        /// ExistLogin
        /// Si connette al master e verifica se la login identificata da loginName esiste
        /// </summary>
        //---------------------------------------------------------------------
        public bool ExistLogin(string loginName)
        {
            bool existLogin = false;
            NpgsqlCommand myCommand = null;
            NpgsqlConnection myConnection = null;

            string connectionString = CurrentStringConnection.Replace("Password=;", "Password=" + DatabaseLayerConsts.postgreDefaultPassword + ";");
            string query = "SELECT COUNT(*) FROM pg_roles WHERE rolname = @User";

            try
            {
                myConnection = new NpgsqlConnection(connectionString);
            
                if (myConnection.State != ConnectionState.Open)
                    myConnection.Open();

                myCommand = new NpgsqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@User", loginName.ToLower());

                if (Int32.Parse(myCommand.ExecuteScalar().ToString()) == 1)
                    existLogin = true;
               
            }
            catch (NpgsqlException e)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "ExistLogin");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseAccess, DatabaseLayerConsts.postgreMasterDatabase), extendedInfo);
            }
            finally
            {
                if (myConnection != null)
                {
                    myConnection.Close();
                }
                myCommand.Dispose();
            }

            return existLogin;
        }
        #endregion

        #region LoginIsSystemAdminRole - True se la loginName ha il ruolo roleName
        /// <summary>
        /// LoginIsSystemAdminRole
        /// Ritorna true se la loginName ha il ruolo SysAdmin, identificato da roleName, false altrimenti
        /// </summary>
        //---------------------------------------------------------------------
        public bool LoginIsSystemAdminRole(string loginName)
        {
            NpgsqlConnection connection = null; 
            NpgsqlCommand command = null;
            string ifSuperUser = string.Format("select usesuper from pg_user where usename = '{0}'", loginName.ToLower());
            try
            {
                if (!ExistLogin(loginName)) return false;
                connection = new NpgsqlConnection(currentStringConnection); 
                connection.Open();
                command = new NpgsqlCommand(ifSuperUser, connection);
                if (command.ExecuteScalar().ToString().Equals("True")) return true;
                return false;

            }
            catch (NpgsqlException)
            {
                return false;
            }
            finally{

                if (command != null) command.Dispose();
                if (connection != null && connection.State!=ConnectionState.Closed) connection.Close();
                
            }
        }
        #endregion

        #region ExistSysAdminDataBase - Verifica se il db di sistema esiste già
        /// <summary>
        ///  ExistSysAdminDataBase
        /// Verifica se il db di SysAdmin esiste già
        /// </summary>
        //---------------------------------------------------------------------
        public bool ExistSysAdminDataBase(string dbName)
        {
            bool existDataBase = false;
            NpgsqlConnection myConnection = null;

            string query = string.Format(@"select count(*) from information_schema.tables where 
                            (table_name = 'msd_companies' OR table_name = 'msd_logins' OR table_name = 'msd_providers') 
                            AND table_schema='{0}'",DatabaseLayerConsts.postgreDefaultSchema);

            try
            {
                myConnection = new NpgsqlConnection(CurrentStringConnection);
                myConnection.Open();
                myConnection.ChangeDatabase(dbName);

                NpgsqlCommand myCommand = new NpgsqlCommand(query, myConnection);
                if (myCommand.ExecuteScalar().ToString().Equals("3"))
                    existDataBase = true;
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "ExistSysAdminDataBase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, PostgreDataAccessStrings.ErrorDataBaseSysAdminConnection);
            }

            if (myConnection != null)
            {
                myConnection.Close();
                myConnection.Dispose();
            }

            return existDataBase;
        }
        #endregion

        #region ExistDataBase - Verifica se il db specificato esiste già (non di sistema)
        /// <summary>
        /// ExistDataBase
        /// Si connette al master e verifica se esiste il db specificato dal dbName
        /// </summary>
        //---------------------------------------------------------------------
        public bool ExistDataBase(string dbName)
        {
            bool existDataBase = false;
            NpgsqlConnection myConnection = null;
            string query = "SELECT count(*) FROM pg_database WHERE datistemplate = false AND name = @CompanyDbName";

            try
            {
                myConnection = new NpgsqlConnection(CurrentStringConnection);
                myConnection.Open();
                myConnection.ChangeDatabase(DatabaseLayerConsts.postgreMasterDatabase);

                NpgsqlCommand myCommand = new NpgsqlCommand(query, myConnection);
                myCommand.Parameters.AddWithValue("@CompanyDbName", dbName);
                if (Convert.ToInt32(myCommand.ExecuteScalar()) > 0)
                    existDataBase = true;
                myCommand.Dispose();
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "ExistDataBase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
            }

            if (myConnection != null)
            {
                myConnection.Close();
                myConnection.Dispose();
            }

            return existDataBase;
        }
        #endregion

        #region ExistObjectsInDatabase - Verifica se il db specificato contiene degli oggetti
        /// <summary>
        /// ExistObjectsInDatabase
        /// Si connette al master e verifica se esiste degli oggetti all'interno del database passato come parametro
        /// </summary>
        //---------------------------------------------------------------------
        public bool ExistObjectsInDatabase(string dbName)
        {
            bool existTables = false, existViews = false, existSP = false;

            string query1 = string.Format("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES where table_catalog = '{0}' and table_schema='{1}'", dbName, DatabaseLayerConsts.postgreDefaultSchema);
            string query2 = string.Format("SELECT count(*) FROM INFORMATION_SCHEMA.views where table_catalog = '{0}' and table_schema='{1}'", dbName, DatabaseLayerConsts.postgreDefaultSchema);
            string query3 = string.Format("SELECT count(*) FROM INFORMATION_SCHEMA.routines where specific_catalog = '{0}' and specific_schema='{1}'", dbName, DatabaseLayerConsts.postgreDefaultSchema);

            NpgsqlConnection myConnection = null;
            NpgsqlCommand myCommand = null;

            try
            {
                myConnection = new NpgsqlConnection(CurrentStringConnection);
                myConnection.Open();
                myConnection.ChangeDatabase(DatabaseLayerConsts.postgreMasterDatabase);

                myCommand = new NpgsqlCommand(query1, myConnection);
                if (Convert.ToInt32(myCommand.ExecuteScalar()) > 0)
                    existTables = true;

                myCommand.CommandText = query2;
                if (Convert.ToInt32(myCommand.ExecuteScalar()) > 0)
                    existViews = true;

                myCommand.CommandText = query3;
                if (Convert.ToInt32(myCommand.ExecuteScalar()) > 0)
                    existSP = true;
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "ExistObjectsInDatabase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
            }
            finally
            {
                myCommand.Dispose();

                if (myConnection != null && myConnection.State != ConnectionState.Closed)
                {
                    myConnection.Close();
                    myConnection.Dispose();
                }
            }

            return existTables || existViews || existSP;
        }
        #endregion



        #region Funzioni di Cancellazione (Database e oggetti di un Db)

        //#region DeleteDatabaseObjects - Cancella tutti gli oggetti di un db (di una azienda)
        ///// <summary>
        ///// DeleteDatabaseObjects
        ///// cancella tutte le tabelle, le view e le stored procedure in un database (esclusi gli oggetti di sistema)
        ///// </summary>
        ////---------------------------------------------------------------------
        //public bool DeleteDatabaseObjects(string serverName, string dbName, string dbOwnerLogin, string dbOwnerPwd, bool isWinAuth, int port=0)
        //{
        //    if (OnEnableProgressBar != null)
        //        OnEnableProgressBar(this);
        //    if (OnSetProgressBarStep != null)
        //        OnSetProgressBarStep(this, 3);
        //    if (OnSetProgressBarValue != null)
        //        OnSetProgressBarValue(this, 1);
        //    if (OnSetCyclicStepProgressBar != null)
        //        OnSetCyclicStepProgressBar();
        //    Cursor.Current = Cursors.WaitCursor;
        //    Application.DoEvents();

        //    bool companyDbObjectsDeleted = false;

        //    string connectionString = string.Empty;
        //    string serverDb = string.Empty;
        //    string istanceDb = string.Empty;
        //    string[] serverWithIstance = serverName.Split(Path.DirectorySeparatorChar);
        //    if (serverWithIstance.Length > 1)
        //    {
        //        serverDb = serverWithIstance[0];
        //        istanceDb = serverWithIstance[1];
        //    }
        //    else
        //        serverDb = serverWithIstance[0];

        //    if (port == 0) port = DatabaseLayerConsts.postgreDefaultPort;

        //    connectionString = (isWinAuth)
        //        ? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, serverName, port, dbName, DatabaseLayerConsts.postgreDefaultSchema)
        //        : string.Format(NameSolverDatabaseStrings.PostgreConnection, serverName, port, dbName, dbOwnerLogin.ToLower(), dbOwnerPwd, DatabaseLayerConsts.postgreDefaultSchema);

        //    NpgsqlConnection deleteDBConnection = null;
        //    NpgsqlCommand myDeleteCommand = null;
        //    NpgsqlDataReader mySqlDataReader = null;
        //    List<string> myObjectList = new List<string>();

        //    try
        //    {
        //        //tento la connessione
        //        deleteDBConnection = new NpgsqlConnection(connectionString);
        //        deleteDBConnection.Open();

        //        //oggetti da cancellare
        //        //-------------------------------------------------------------

        //        string myUserFKQuery = string.Format("SELECT constraint_name as name, table_name as tableName FROM INFORMATION_SCHEMA.table_constraints where constraint_type= 'FOREIGN KEY' and constraint_schema='{0}'",DatabaseLayerConsts.postgreDefaultSchema);
        //        //-------------------------------------------------------------
        //        string myUserTableQuery = string.Format("SELECT table_name as name FROM INFORMATION_SCHEMA.tables where table_schema='{0}' and table_type='BASE TABLE'",DatabaseLayerConsts.postgreDefaultSchema);
        //        //-------------------------------------------------------------
        //        string myUserSPQuery =string.Format( "SELECT routine_name as name FROM INFORMATION_SCHEMA.routines where specific_schema='{0}'",DatabaseLayerConsts.postgreDefaultSchema);
        //        //-------------------------------------------------------------
        //        string myUserViewQuery = string.Format("SELECT table_name as name FROM INFORMATION_SCHEMA.views where table_schema='{0}'",DatabaseLayerConsts.postgreDefaultSchema);
        //        //-------------------------------------------------------------
        //        string myUserTriggerQuery = "SELECT  trigger_name as name, event_object_table as tableName FROM INFORMATION_SCHEMA.triggers";

        //        myDeleteCommand = new NpgsqlCommand();
        //        myDeleteCommand.Connection = deleteDBConnection;

        //        //-------------------------------------------------------------
        //        // CANCELLAZIONE TRIGGER
        //        //-------------------------------------------------------------
        //        myDeleteCommand.CommandText = myUserTriggerQuery;
        //        mySqlDataReader = myDeleteCommand.ExecuteReader();
        //        List<string> triggerTableList = new List<string>();
        //        while (mySqlDataReader.Read())
        //        {
        //            myObjectList.Add(mySqlDataReader["name"].ToString());
        //            triggerTableList.Add(mySqlDataReader["tabelname"].ToString());
        //        }
        //        mySqlDataReader.Close();

        //        if (OnSetProgressBarText != null)
        //            OnSetProgressBarText(this, PostgreDataAccessStrings.DroppingTriggers);

        //        for (int i = 0; i < myObjectList.Count; i++)
        //        {
        //            myDeleteCommand.CommandText = string.Format("DROP TRIGGER IF EXISTS {0} ON {1}", myObjectList[i], triggerTableList[i]);
        //            myDeleteCommand.ExecuteNonQuery();
        //            myDeleteCommand.Dispose();
        //            Application.DoEvents();
        //        }

        //        //-------------------------------------------------------------
        //        // CANCELLAZIONE VIEW
        //        //-------------------------------------------------------------
        //        myDeleteCommand.CommandText = myUserViewQuery;
        //        mySqlDataReader = myDeleteCommand.ExecuteReader();
        //        myObjectList.Clear();

        //        while (mySqlDataReader.Read())
        //        {
        //            myObjectList.Add(mySqlDataReader["name"].ToString());
        //        }
        //        mySqlDataReader.Close();

        //        if (OnSetProgressBarText != null)
        //            OnSetProgressBarText(this, PostgreDataAccessStrings.DroppingViews);

        //        foreach (string view in myObjectList)
        //        {
        //            myDeleteCommand.CommandText = string.Format("DROP VIEW IF EXISTS {0}", view);
        //            myDeleteCommand.ExecuteNonQuery();
        //            myDeleteCommand.Dispose();
        //            Application.DoEvents();
        //        }

        //        //-------------------------------------------------------------
        //        // CANCELLAZIONE STORED PROCEDURE
        //        //-------------------------------------------------------------
        //        myDeleteCommand.CommandText = myUserSPQuery;
        //        mySqlDataReader = myDeleteCommand.ExecuteReader();
        //        myObjectList.Clear();

        //        while (mySqlDataReader.Read())
        //            myObjectList.Add(mySqlDataReader["name"].ToString());
        //        mySqlDataReader.Close();

        //        if (OnSetProgressBarText != null)
        //            OnSetProgressBarText(this, PostgreDataAccessStrings.DroppingProcedures);

        //        foreach (string procedure in myObjectList)
        //        {
        //            myDeleteCommand.CommandText = string.Format("DROP FUNCTION IF EXISTS {0}", procedure);
        //            myDeleteCommand.ExecuteNonQuery();
        //            myDeleteCommand.Dispose();
        //            Application.DoEvents();
        //        }

        //        //-------------------------------------------------------------
        //        // CANCELLAZIONE FOREIGN KEY
        //        //-------------------------------------------------------------
        //        myDeleteCommand.CommandText = myUserFKQuery;
        //        mySqlDataReader = myDeleteCommand.ExecuteReader();
        //        myObjectList.Clear();
        //        List<string> userFKTables = new List<string>();

        //        while (mySqlDataReader.Read())
        //        {
        //            myObjectList.Add(mySqlDataReader["name"].ToString());
        //            userFKTables.Add(mySqlDataReader["tableName"].ToString());
        //        }
        //        mySqlDataReader.Close();

        //        if (OnSetProgressBarText != null)
        //            OnSetProgressBarText(this, PostgreDataAccessStrings.DroppingConstraints);

        //        for (int i = 0; i < myObjectList.Count; i++)
        //        {
        //            myDeleteCommand.CommandText = string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", userFKTables[i], myObjectList[i]);
        //            myDeleteCommand.ExecuteNonQuery();
        //            myDeleteCommand.Dispose();
        //            Application.DoEvents();
        //        }

        //        //-------------------------------------------------------------
        //        // CANCELLAZIONE TABELLE
        //        //-------------------------------------------------------------
        //        myDeleteCommand.CommandText = myUserTableQuery;
        //        mySqlDataReader = myDeleteCommand.ExecuteReader();
        //        myObjectList.Clear();

        //        while (mySqlDataReader.Read())
        //            myObjectList.Add(mySqlDataReader["name"].ToString());
        //        mySqlDataReader.Close();

        //        if (OnSetProgressBarText != null)
        //            OnSetProgressBarText(this, PostgreDataAccessStrings.DroppingTables);

        //        foreach (string table in myObjectList)
        //        {
        //            myDeleteCommand.CommandText = string.Format("DROP TABLE IF EXISTS {0}", table);
        //            myDeleteCommand.ExecuteNonQuery();
        //            myDeleteCommand.Dispose();
        //            Application.DoEvents();
        //        }

        //        myDeleteCommand.Dispose();
        //        companyDbObjectsDeleted = true;
        //    }
        //    catch (NpgsqlException e)
        //    {
        //        Debug.WriteLine(e.Message);
        //        ExtendedInfo extendedInfo = new ExtendedInfo();
        //        extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
        //        extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
        //        extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
        //        extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDatabaseObjects");
        //        extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
        //        extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
        //        extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
        //        Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, dbName), extendedInfo);
        //    }
        //    catch (Exception aExc)
        //    {
        //        Debug.WriteLine(aExc.Message);
        //        ExtendedInfo extendedInfo = new ExtendedInfo();
        //        extendedInfo.Add(DatabaseLayerStrings.Description, aExc.Message);
        //        extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDatabaseObjects");
        //        extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
        //        extendedInfo.Add(DatabaseLayerStrings.Source, aExc.Source);
        //        extendedInfo.Add(DatabaseLayerStrings.StackTrace, aExc.StackTrace);
        //        Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, dbName), extendedInfo);
        //    }
        //    finally
        //    {
        //        if (myDeleteCommand != null)
        //            myDeleteCommand.Dispose();

        //        if (mySqlDataReader != null && !mySqlDataReader.IsClosed)
        //        {
        //            mySqlDataReader.Close();
        //            mySqlDataReader.Dispose();
        //        }

        //        if (deleteDBConnection != null && deleteDBConnection.State != ConnectionState.Closed)
        //        {
        //            deleteDBConnection.Close();
        //            deleteDBConnection.Dispose();
        //        }

        //        if (OnSetProgressBarText != null)
        //            OnSetProgressBarText(this, string.Empty);

        //        if (OnDisableProgressBar != null)
        //            OnDisableProgressBar(this);
        //    }
        //    return companyDbObjectsDeleted;
        //}
        //#endregion

        #region DeleteDataBase - Cancella un db
        /// <summary>
        /// DeleteDataBase
        /// </summary>
        //---------------------------------------------------------------------
        public bool DeleteDataBase
            (
                string serverName,
                string dataBaseName,
                string databaseOwnerLogin,
                string databaseOwnerPassword,
                bool integratedSecurity,
                int port=0
            )
        {
            if (OnSetProgressBarText != null)
                OnSetProgressBarText(this, PostgreDataAccessStrings.DroppingDatabase);

            bool companyDbDeleted = false;
            NpgsqlConnection sqlConnection = null;

            if (port == 0) port = DatabaseLayerConsts.postgreDefaultPort;

            if (string.IsNullOrEmpty(databaseOwnerPassword) || string.IsNullOrWhiteSpace(databaseOwnerPassword))
                databaseOwnerPassword = DatabaseLayerConsts.postgreDefaultPassword;
           
            string connectionString =
                (integratedSecurity)
                ? string.Format(NameSolverDatabaseStrings.PostgreWinNtConnection, serverName, port, DatabaseLayerConsts.postgreMasterDatabase, DatabaseLayerConsts.postgreDefaultSchema)
                : string.Format(NameSolverDatabaseStrings.PostgreConnection, serverName, port, DatabaseLayerConsts.postgreMasterDatabase, databaseOwnerLogin.ToLower(), databaseOwnerPassword, DatabaseLayerConsts.postgreDefaultSchema);

            try
            {
                sqlConnection = new NpgsqlConnection(connectionString);
                sqlConnection.Open();

                string countDb = string.Format("select count(*) from pg_database where datname='{0}'", dataBaseName);
                NpgsqlCommand countCmd = new NpgsqlCommand(countDb, sqlConnection);

                NpgsqlCommand dropDbCmd;

                if (Convert.ToInt32(countCmd.ExecuteScalar().ToString()) == 1)
                {
                    //@@Anastasia Controllo se esistono connessioni aperti verso il database
                    string ifConnectionsExist = string.Format("SELECT count(*) FROM pg_stat_activity WHERE datname = '{0}'", dataBaseName);
                    NpgsqlCommand connectionsCount = new NpgsqlCommand(ifConnectionsExist, sqlConnection);
                    if (Convert.ToInt32(connectionsCount.ExecuteScalar()) > 0)
                    {
                        // @@Anastasia chiudo le connessioni aperte se esistono
                        string alterDB = string.Format("SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'", dataBaseName);

                        dropDbCmd = new NpgsqlCommand(alterDB, sqlConnection); //@@ Anastasia devo per forza fare new !!!
                        dropDbCmd.ExecuteNonQuery();
                    }

                    // poi eseguo la vera e propria cancellazione
                    string dropDb = string.Format("DROP DATABASE IF EXISTS {0}", dataBaseName);
                    dropDbCmd = new NpgsqlCommand(dropDb, sqlConnection);
                    dropDbCmd.CommandTimeout = 60;
                    dropDbCmd.ExecuteNonQuery();

                    companyDbDeleted = true;
                    dropDbCmd.Dispose();
                }
                else
                {
                    string message =
                        string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, dataBaseName) + "\r\n"
                        + PostgreDataAccessStrings.SeeDetailsExtendendInfo;

                    ExtendedInfo extendedInfo = new ExtendedInfo();
                    extendedInfo.Add(DatabaseLayerStrings.Description, PostgreDataAccessStrings.DbNotExistsOnServer);
                    extendedInfo.Add(DatabaseLayerStrings.Server, serverName);
                    extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
                    extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                    Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
                    companyDbDeleted = true;
                }

                countCmd.Dispose();
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, dataBaseName), extendedInfo);
            }
            catch (System.ApplicationException aExc)
            {
                Debug.WriteLine(aExc.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, aExc.Message);
                extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, aExc.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, aExc.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, dataBaseName), extendedInfo);
            }

            if (sqlConnection != null)
            {
                sqlConnection.Close();
                sqlConnection = null;
            }

            return companyDbDeleted;
        }
        #endregion

        #region DeleteDataBase - Cancella un db (richiamato dal CompanyMigration)
        /// <summary>
        /// DeleteDataBase
        /// </summary>
        //---------------------------------------------------------------------
        public bool DeleteDataBase(string connectionString, string database)
        {
            bool success = false;

            NpgsqlConnection deleteDBConnection = null;
            NpgsqlCommand myCommandDeleteDB = null;

            string deleteCommand = string.Format("DROP DATABASE IF EXISTS \"{0}\"", database);
            try
            {
                //tento la connessione
                deleteDBConnection = new NpgsqlConnection(connectionString);
                deleteDBConnection.Open();

                string oldDb = deleteDBConnection.Database;
                deleteDBConnection.ChangeDatabase(DatabaseLayerConsts.postgreMasterDatabase);

                myCommandDeleteDB = new NpgsqlCommand(deleteCommand, deleteDBConnection);
                myCommandDeleteDB.CommandTimeout = 60;
                myCommandDeleteDB.ExecuteScalar();
                myCommandDeleteDB.Dispose();
                success = true;

                if (!success)
                    deleteDBConnection.ChangeDatabase(oldDb);
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, database), extendedInfo);
            }
            catch (System.ApplicationException aExc)
            {
                Debug.WriteLine(aExc.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, aExc.Message);
                extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, aExc.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, aExc.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorDataBaseDeleting, database), extendedInfo);
            }
            finally
            {
                if (myCommandDeleteDB != null)
                    myCommandDeleteDB.Dispose();

                if (deleteDBConnection != null && deleteDBConnection.State == ConnectionState.Open)
                {
                    deleteDBConnection.Close();
                    deleteDBConnection.Dispose();
                    deleteDBConnection = null;
                }
            }

            return success;
        }
        #endregion



        #region GetLogins - legge le logins di postgre dal server dove risiede il db aziendale
        /// <summary>
        /// GetLogins
        /// Si connnette al server dove risiede il dbcompany e legge le logins di postgre
        /// </summary>
        //---------------------------------------------------------------------
        public bool GetLogins(out ArrayList loginsDatabase)
        {
            ArrayList loginsList = new ArrayList();
            NpgsqlDataReader mySqlDataReader = null;
            NpgsqlCommand myCommand = new NpgsqlCommand();
            string connectionString = CurrentStringConnection.Replace("Password=;", "Password=" + DatabaseLayerConsts.postgreDefaultPassword + ";");
            NpgsqlConnection myConnection = new NpgsqlConnection(connectionString);

            try
            {
                if (myConnection.State == ConnectionState.Closed)
                    myConnection.Open();

                myCommand.Connection = myConnection;
                myCommand.CommandTimeout = 90;
                myCommand.CommandText = "SELECT usename  FROM pg_user ORDER BY usename";
                mySqlDataReader = myCommand.ExecuteReader();

                while (mySqlDataReader.Read())
                {
                    CompanyLoginPostgre companyLogin = new CompanyLoginPostgre();
                    companyLogin.Login = mySqlDataReader["usename"].ToString();

                    if (string.Compare(companyLogin.Login, DatabaseLayerConsts.postgreSuperUser, StringComparison.InvariantCultureIgnoreCase) == 0)
                        companyLogin.IsSuperUser = true;

                    loginsList.Add(companyLogin);
                }
                return true;
            }
            catch (NpgsqlException sqlException)
            {
                Debug.WriteLine(sqlException.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Routine);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, "SELECT usename  FROM pg_user");
                //extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Code);
                extendedInfo.Add(DatabaseLayerStrings.Function, "GetLogins");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, sqlException.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, sqlException.StackTrace);
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.ErrorLoadingSqlLogins), extendedInfo);
                return false;
            }
            finally
            {
                if (mySqlDataReader != null)
                    mySqlDataReader.Close();

                if (myConnection != null)
                {
                    myConnection.Close();
                    myConnection.Dispose();
                }

                if (myCommand != null)
                    myCommand.Dispose();

                loginsDatabase = loginsList;
            }

        }
        #endregion

        #endregion

        #region Funzioni che richiamano StoredProcedures




        #region ChangePassword - Modifica la password di una login
        //---------------------------------------------------------------------
        public bool ChangePassword(string loginName, string newPassword)
        {

            NpgsqlCommand myCommand = null;
            NpgsqlConnection myConnection = new NpgsqlConnection(CurrentStringConnection);
            string changePasswordQuery = string.Format("ALTER USER {0} ENCRYPTED PASSWORD '{1}'", loginName, newPassword);

            try
            {
                if (!ExistLogin(loginName)) return false;

                if (myConnection.State != ConnectionState.Open)
                    myConnection.Open();

                // @@Anastasia non si puo controllare old password, e' criptata
                myCommand = new NpgsqlCommand(changePasswordQuery, myConnection);
                myCommand.ExecuteNonQuery();

                return true;
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);

                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
                extendedInfo.Add(DatabaseLayerStrings.Function, "ChangePassword");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set
                    (DiagnosticType.Error,
                    PostgreDataAccessStrings.ErrorSPPassword + "\r\n" + PostgreDataAccessStrings.SeeDetailsExtendendInfo,
                    extendedInfo);

                return false;
            }
            finally
            {
                if (myConnection != null && myConnection.State == ConnectionState.Open)
                    myConnection.Close();
                if (myCommand != null)
                    myCommand.Dispose();
            }

        }
        #endregion

       

        #region AddLogin - Crea una nuova Login postgre
        /// <summary>
        /// SPAddLogin
        /// Crea un nuovo account di accesso di Postgre
        /// La login può essere lunga fino a 128 caratteri, non può essere nulla, o contenere '\';
        /// non può essere sa o public, che esistono già
        /// </summary>
        //---------------------------------------------------------------------
        public bool AddLogin(string login, string password)
        {

            if (string.IsNullOrEmpty(login) || login.Length > 128 || login.IndexOf("\\") > 0)
            {
                Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.DifferentCredential, login));
                return false;
            }
            if (string.IsNullOrEmpty(password)) 
                password = DatabaseLayerConsts.postgreDefaultPassword;

            string addUserQuery = string.Format("CREATE USER {0} WITH ENCRYPTED PASSWORD '{1}'", login.ToLower(), password);
    
            NpgsqlConnection myConnection = new NpgsqlConnection(CurrentStringConnection);
            NpgsqlCommand myCommand = null;

            try
            {
                //@@Anastasia Controllo se login esiste gia'
                //Se esiste, lo modifico
                if (ExistLogin(login)) return false;

                if (myConnection.State != ConnectionState.Open)
                    myConnection.Open();
                myCommand = new NpgsqlCommand(addUserQuery, myConnection);
                myCommand.ExecuteNonQuery();
                return true;

            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);

                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
                extendedInfo.Add(DatabaseLayerStrings.Function, "AddLogin");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                return false;
            }
            finally
            {
                if (myConnection != null && myConnection.State != ConnectionState.Closed)
                {
                    myConnection.Close();
                }
                if (myCommand != null)
                    myCommand.Dispose();
            }

        }
        #endregion



        #region GrantDbAccess - Granta accesso al db corrente al user
        /// <summary>
       
        /// </summary>
        //---------------------------------------------------------------------
         public bool GrantDbAccess(string login){

             if (string.IsNullOrEmpty(login) || login.Length > 128 || login.IndexOf("\\") > 0)
             {
                 Diagnostic.Set(DiagnosticType.Error, string.Format(PostgreDataAccessStrings.DifferentCredential, login));
                 return false;
             }

             string grantQuery = string.Format("GRANT USAGE ON SCHEMA {0} TO {1};GRANT SELECT, INSERT, DELETE, UPDATE ON ALL TABLES IN SCHEMA {0} TO {1}", DatabaseLayerConsts.postgreDefaultSchema, login.ToLower());
             NpgsqlConnection myConnection = new NpgsqlConnection(CurrentStringConnection);
             NpgsqlCommand myCommand = null;

             try
             {
                 //@@Anastasia Controllo se login esiste gia'
                 if (! ExistLogin(login)) return false;

                 if (myConnection.State != ConnectionState.Open)
                     myConnection.Open();
                 myCommand = new NpgsqlCommand(grantQuery, myConnection);
                 myCommand.ExecuteNonQuery();
                 return true;

             }
             catch (NpgsqlException e)
             {
                 Debug.WriteLine(e.Message);

                 ExtendedInfo extendedInfo = new ExtendedInfo();
                 extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                 //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                 //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                 extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
                 extendedInfo.Add(DatabaseLayerStrings.Function, "AddLogin");
                 extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                 extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                 extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                 return false;
             }
             finally
             {
                 if (myConnection != null && myConnection.State != ConnectionState.Closed)
                 {
                     myConnection.Close();
                 }
                 if (myCommand != null)
                     myCommand.Dispose();
             }
         }
            
       #endregion 


        #region Changedbowner - Cambia l'owner del database corrente
        /// <summary>
        /// Changedbowner
        /// Cambia l'owner del database corrente. 
        /// </summary>
        //---------------------------------------------------------------------
        private bool Changedbowner(string login, NpgsqlConnection currentConnection)
        {
            if ((string.Compare(currentConnection.Database, DatabaseLayerConsts.postgreMasterDatabase, true, CultureInfo.InvariantCulture) == 0))
                return false;

            NpgsqlConnection myConnection = new NpgsqlConnection(currentStringConnection);
            NpgsqlCommand myCommand = null; // new SqlCommand();
            string changeDbOwnerQuery = string.Format("ALTER DATABASE {0} OWNER TO {1}", currentConnection.Database, login);

            try
            {
                if (!ExistLogin(login)) return false;
                myConnection.Open();
                myCommand = new NpgsqlCommand(changeDbOwnerQuery, myConnection);
                myCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException e)
            {

                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
                extendedInfo.Add(DatabaseLayerStrings.Function, "Changedbowner");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set
                    (DiagnosticType.Error,
                    PostgreDataAccessStrings.ErrorSPChangedbowner + "\r\n" + PostgreDataAccessStrings.SeeDetailsExtendendInfo,
                    extendedInfo);

                return false;
            }

            finally
            {
                if (myCommand != null)
                    myCommand.Dispose();
                if (myConnection != null)
                    myConnection.Close();
            }
        }
        #endregion




        #region DropUser - Rimuove uno user
        /// <summary>
        /// DropUser
        /// </summary>
        //---------------------------------------------------------------------
        public bool DropUser(string userInDb)
        {
            

            NpgsqlCommand myCommand = null;
            NpgsqlConnection dropUserConnection = new NpgsqlConnection(CurrentStringConnection);
            string dropUserQuery = string.Format("DROP USER IF EXISTS {0}", userInDb);

            try
            {

                if (dropUserConnection.State != ConnectionState.Open)
                    dropUserConnection.Open();
                myCommand = new NpgsqlCommand(dropUserQuery, dropUserConnection);
                myCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, userInDb);
                extendedInfo.Add(DatabaseLayerStrings.Function, "DropUser");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set
                    (DiagnosticType.Error,
                    PostgreDataAccessStrings.ErrorSPDropUser + "\r\n" + PostgreDataAccessStrings.SeeDetailsExtendendInfo,
                    extendedInfo);
                return false;
            }
            finally
            {
                if (dropUserConnection != null && dropUserConnection.State == ConnectionState.Open)
                    dropUserConnection.Close();
                if (myCommand != null)
                    myCommand.Dispose();
            }
        }
        #endregion


        #region DropUser - Rimuove uno user
        /// <summary>
        /// DropUser
        /// </summary>
        //---------------------------------------------------------------------
        public bool RevokeDbAccess (string login, string dbCompanyName)
        {
            
            NpgsqlCommand myCommand = null;
            NpgsqlConnection revokeUserConnection = new NpgsqlConnection(CurrentStringConnection);
            string revokeAccessQuery = string.Format("REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA {0} FROM {1}; REVOKE ALL PRIVILEGES ON  SCHEMA {0} FROM {1}; REVOKE ALL PRIVILEGES ON DATABASE {2} FROM {1};", DatabaseLayerConsts.postgreDefaultSchema, login, dbCompanyName);

            try
            {

                if (revokeUserConnection.State != ConnectionState.Open)
                    revokeUserConnection.Open();
                myCommand = new NpgsqlCommand(revokeAccessQuery, revokeUserConnection);
                myCommand.ExecuteScalar();
                return true;
            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
                extendedInfo.Add(DatabaseLayerStrings.Function, "DropUser");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set
                    (DiagnosticType.Error,
                    PostgreDataAccessStrings.ErrorSPDropUser + "\r\n" + PostgreDataAccessStrings.SeeDetailsExtendendInfo,
                    extendedInfo);
                return false;
            }
            finally
            {
                if (revokeUserConnection != null && revokeUserConnection.State == ConnectionState.Open)
                    revokeUserConnection.Close();
                if (myCommand != null)
                    myCommand.Dispose();
            }
        }
        #endregion



        #region AddRoleMember
        /// <summary>
        /// aggiunge un utente ad un ruolo
        /// ddRoleMember
        /// </summary>
        //---------------------------------------------------------------------
        public bool AddRoleMember(string loginName, string roleName)
        {
            if (string.Compare(loginName, DatabaseLayerConsts.postgreSuperUser, true, CultureInfo.InvariantCulture) == 0 ||
                loginName.Equals(roleName))
                return false;
            if (!ExistLogin(loginName) || !ExistLogin(roleName)) return false;

            NpgsqlCommand myCommand = null;
            NpgsqlConnection myConnection = null; 
            string grantRoleQuery =null;

            if (LoginIsSystemAdminRole(roleName))
                grantRoleQuery = string.Format("ALTER USER {0} WITH SUPERUSER; ALTER USER {0} WITH CREATEDB;", loginName.ToLower());               
            else 
                grantRoleQuery=string.Format("GRANT {0} TO {1}", roleName.ToLower(), loginName.ToLower());

            try
            {
                myConnection = new NpgsqlConnection(CurrentStringConnection);
                myConnection.Open();

                myCommand = new NpgsqlCommand(grantRoleQuery, myConnection);
                myCommand.ExecuteNonQuery();
                return true;

            }
            catch (NpgsqlException e)
            {
                Debug.WriteLine(e.Message);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                //extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Routine);
                //extendedInfo.Add(DatabaseLayerStrings.Number, e.Code);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
                extendedInfo.Add(DatabaseLayerStrings.Parameters, roleName);
                extendedInfo.Add(DatabaseLayerStrings.Function, "AddRoleMember");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.PostgreDataAccess");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                Diagnostic.Set
                    (DiagnosticType.Error,
                    string.Format(PostgreDataAccessStrings.ErrorSPAddRoleMember, roleName, loginName)
                    + "\r\n" + PostgreDataAccessStrings.SeeDetailsExtendendInfo,
                    extendedInfo);
                return false;
            }
            finally
            {
                if (myConnection != null)
                    myConnection.Close();
                if (myCommand != null)
                    myCommand.Dispose();
            }
        }
        #endregion

        #region IsRoleMember EX SPhelpsrvrolemember
        /// <summary>
        /// SPhelpsrvrolemember
        /// </summary>
        //---------------------------------------------------------------------
        public bool IsRoleMember(string loginName, string roleName)
        {

            //se è postgres sono sicura che ha già la role quindi faccio tornare true oppure loginName==roleName
            if (string.Compare(loginName, DatabaseLayerConsts.postgreSuperUser, true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(loginName, roleName, true, CultureInfo.InvariantCulture) == 0)
                return true;

             if (!ExistLogin(loginName) || !ExistLogin(roleName)) 
                 return false;

            NpgsqlDataReader mySqlDataReader = null;
            NpgsqlCommand myCommand = new NpgsqlCommand();
            NpgsqlConnection myConnection = new NpgsqlConnection(CurrentStringConnection);

            //@@Anastasia controllo i ruoli di un utente
            string userInRole = string.Format("select rolname from pg_user join pg_auth_members on (pg_user.usesysid=pg_auth_members.member) join pg_roles on (pg_roles.oid=pg_auth_members.roleid) where pg_user.usename='{0}'", loginName);

            try
            {
                if (myConnection.State != ConnectionState.Open)
                    myConnection.Open();
                myCommand = new NpgsqlCommand(userInRole, myConnection);

                mySqlDataReader = myCommand.ExecuteReader();
                while (mySqlDataReader.Read())
                {
                    if (string.Compare(mySqlDataReader["rolname"].ToString(), roleName, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (NpgsqlException)
            {
                return false;
            }
            finally
            {
                if (mySqlDataReader != null)
                    mySqlDataReader.Close();
                if (myConnection != null)
                    myConnection.Close();
                if (myCommand != null)
                    myCommand.Dispose();
            }
        }
        #endregion


       
        public UserImpersonatedData LoginImpersonification
			(
			string login,
			string password,
			string domain,
			bool winAuth,
			bool silent,
            int port
			)
		{
			string currentUser = string.Empty;
			if (winAuth)
			{
				string[] loginAndDomain = login.Split(Path.DirectorySeparatorChar);

				currentUser = (loginAndDomain.Length == 1)
					? domain + Path.DirectorySeparatorChar + login
					: login;
			}
			else
				currentUser = login;

			PostgreCredential loginData = null;
			if ((string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, true, CultureInfo.InvariantCulture) != 0) && (winAuth))
			{
				loginData = new PostgreCredential(login, password, domain, winAuth, silent, port);
				loginData.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
				if (loginData.Success)
					return loginData.userImpersonated;
				else
					return null;
			}
			else
			{
				UserImpersonatedData impersonateUser = new UserImpersonatedData();
				impersonateUser.UserAfterImpersonate = System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
				impersonateUser.Login = login;
				impersonateUser.Password = password;
				impersonateUser.Domain = domain;
				impersonateUser.WindowsAuthentication = winAuth;
				return impersonateUser;
			}
		}


       
		#region LoginImpersonification - Permetto all'utente di ri-loggarsi e di impersonificare un altro utente NT
		/// <summary>
		/// LoginImpersonification
		/// Permetto all'utente di ri-loggarsi e di impersonificare
		/// un altro utente NT
		/// </summary>
		//---------------------------------------------------------------------
		public UserImpersonatedData LoginImpersonification
			(
			string login,
			string password,
			string domain,
			bool winAuth,
			string serverName,
			string serverIstanceName,
			bool enableChangeCredential,
            int port
			)
		{

			string serverToConnect =
				(serverIstanceName.Length == 0)
				? serverName
				: serverName + Path.DirectorySeparatorChar + serverIstanceName;

			string currentUser = string.Empty;

			//se l'utente correntemente loggato è diverso da quello che vuole diventare
			if (winAuth)
			{
				string[] loginAndDomain = login.Split(Path.DirectorySeparatorChar);
				currentUser = (loginAndDomain.Length == 1)
				? domain + Path.DirectorySeparatorChar + login
				: login;
			}
			else
				currentUser = login;

			PostgreCredential loginData = null;
			if ((string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, true, CultureInfo.InvariantCulture) != 0) && (winAuth))
			{
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					if (!OnIsUserAuthenticatedFromConsole(currentUser, password, serverToConnect))
					{
                        loginData = new PostgreCredential(serverToConnect, login, /*pw*/string.Empty,  domain, winAuth, enableChangeCredential, port);
                        loginData.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();

						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
								OnAddUserAuthenticatedFromConsole
									(
									(winAuth)
									? loginData.userImpersonated.Domain + Path.DirectorySeparatorChar + loginData.userImpersonated.Login
									: loginData.userImpersonated.Login,
									loginData.userImpersonated.Password,
									serverToConnect,
									DBMSType.POSTGRE
									);

							return loginData.userImpersonated;
						}
						else
							return null;
					}
					else
					{
						if (OnGetUserAuthenticatedPwdFromConsole != null)
						{
							string pwd = OnGetUserAuthenticatedPwdFromConsole(currentUser, serverToConnect);
                            loginData = new PostgreCredential(login, pwd, domain, winAuth, port);
                            loginData.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
						}
						else
						{
                            loginData = new PostgreCredential(serverToConnect, login,/*pw*/string.Empty, domain, winAuth, enableChangeCredential, port);
                            loginData.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
							loginData.ShowDialog();
						}
						if (loginData.Success)
							return loginData.userImpersonated;
						else
							return null;
					}
				}
				else
				{
                    loginData = new PostgreCredential(serverToConnect, login, /*pw*/string.Empty, domain, winAuth, enableChangeCredential, port);
                    loginData.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
					loginData.ShowDialog();
					if (loginData.Success)
						return loginData.userImpersonated;
					else
						return null;
				}
			}
			else
			{
				if (OnIsUserAuthenticatedFromConsole != null)
				{
                    if (password.Equals(DatabaseLayerConsts.postgreDefaultPassword))
                        password = "";
					if (!OnIsUserAuthenticatedFromConsole(currentUser, password, serverToConnect))
					 {
                        loginData = new PostgreCredential(serverToConnect, login, /*pw*/string.Empty, domain, winAuth, enableChangeCredential, port);
                        loginData.OnOpenHelpFromPopUp += new PostgreCredential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();

						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
								OnAddUserAuthenticatedFromConsole
									(
									(winAuth)
									? loginData.userImpersonated.Domain + Path.DirectorySeparatorChar + loginData.userImpersonated.Login
									: loginData.userImpersonated.Login,
									loginData.userImpersonated.Password,
									serverToConnect,
									DBMSType.POSTGRE
									);

							return loginData.userImpersonated;
						}
					}
					else
					{
						UserImpersonatedData impersonateUser = new UserImpersonatedData();
						impersonateUser.UserAfterImpersonate = System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
						impersonateUser.Login = login;
						impersonateUser.Password = password;
						impersonateUser.Domain = domain;
						impersonateUser.WindowsAuthentication = winAuth;
						return impersonateUser;
					}
				}
				else
					return null;
			}
			return null;
		}
		#endregion

		//---------------------------------------------------------------------
		private void SendHelp(object sender, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, NameSpace, searchParameter);
		}
		#endregion
	}

        #endregion
    //=========================================================================
    public class CompanyLoginPostgre
    {
        #region Variabili membro (privati)
        private string login = string.Empty;
        private string companyId = string.Empty;
        private bool isSuperUser = false;
        #endregion

        #region Proprietà
        //Properties
        //---------------------------------------------------------------------
        public string CompanyId { get { return companyId; } set { companyId = value; } }
        public string Login { get { return login; } set { login = value; } }
        public bool IsSuperUser { get { return isSuperUser; } set { isSuperUser = value; } }
        #endregion

        #region Costruttore
        /// <summary>
        /// Costruttore (vuoto
        /// </summary>
        //---------------------------------------------------------------------
        public CompanyLoginPostgre() { }
        #endregion


    }
}
        