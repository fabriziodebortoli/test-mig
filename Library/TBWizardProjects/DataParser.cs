using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
// per caricamento server SQL
using Microsoft.Win32;
using System.Data.Sql;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBWizardProjects
{
	/// <summary>
	/// Summary description for DataParser.
	/// </summary>
	public class DataParser
	{
		#region Connection string tokens
		
		public const string CONNECTION_STRING_DATASOURCE_KEYWORD			= "Data source";
		public const string CONNECTION_STRING_INITIAL_CATALOG_KEYWORD		= "Initial Catalog";
		public const string CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD			= "User Id";
		public const string CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD		= "Password";
		public const string CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN		= "Integrated Security";
		public const string CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE = "SSPI";

		#endregion

		#region DBInfo xml file constant strings

		private const string DBInfoXmlVersion	= "1.0";
		private const string DBInfoXmlEncoding	= "UTF-8";

		private const string XML_DBINFO_TABLE_TAG		= "Table";
		private const string XML_DBINFO_OVERVIEW_TAG	= "Overview";
		private const string XML_DBINFO_FIELDS_TAG		= "Fields";
		private const string XML_DBINFO_COLUMN_TAG		= "Column";
		private const string XML_DBINFO_KEYS_TAG		= "Keys";
		private const string XML_DBINFO_KEY_TAG			= "Key";
		
		private const string XML_DBINFO_TABLE_NAME_ATTRIBUTE			= "Name";
		private const string XML_DBINFO_MODULE_NAME_ATTRIBUTE			= "Module";
		private const string XML_DBINFO_TABLE_NAMESPACE_ATTRIBUTE		= "Namespace";
		private const string XML_DBINFO_TABLE_RUNTIMECLASS_ATTRIBUTE	= "RuntimeClass";
		private const string XML_DBINFO_COLUMN_NAME_ATTRIBUTE			= "Name";
		private const string XML_DBINFO_COLUMN_TYPE_ATTRIBUTE			= "Type";
		private const string XML_DBINFO_COLUMN_LENGTH_ATTRIBUTE			= "Length";
		private const string XML_DBINFO_COLUMN_ENUM_ATTRIBUTE			= "Enum";
		private const string XML_DBINFO_COLUMN_MANDATORY_ATTRIBUTE		= "Mandatory";
        private const string XML_DBINFO_COLUMN_DEFAULT_ATTRIBUTE        = "Default";
        private const string XML_DBINFO_COLUMN_READONLY_ATTRIBUTE       = "ReadOnly";
        private const string XML_DBINFO_COLUMN_REFERENCE_ATTRIBUTE      = "Reference";
        private const string XML_DBINFO_COLUMN_LOCALIZABLE_ATTRIBUTE    = "localizable";
        private const string XML_DBINFO_KEY_NAME_ATTRIBUTE              = "Name";
		private const string XML_DBINFO_KEY_TYPE_ATTRIBUTE				= "Type";
		
		private const string XML_DBINFO_PRIMARY_KEY_ATTRIBUTE_VALUE		= "Primary";
		private const string XML_DBINFO_COLUMN_ENUM_ATTRIBUTE_FORMAT	= "{0}/{1}";

		#endregion

		#region DataParser private data members

		//private WizardApplicationInfo applicationInfo = null;

		private string serverName = String.Empty;
		private string dbName = String.Empty;
		
		#endregion

		//---------------------------------------------------------------------------
		public DataParser()
		{
		}

		//--------------------------------------------------------------------------------------
		public static bool CheckSQLDMOInstallation()
		{
			try
			{
                // TODO ITRI RIvedere logiche con DMO 
                ////Istanzio un oggetto ed eseguo una chiamata
                //SQLDMO.SQLServer2 sqlServerConnected = new SQLDMO.SQLServer2();
                //string hostName =  sqlServerConnected.HostName;
				return true;
			}
			catch(COMException )
			{
				return false;
			}
		}

//#warning commentata la gestione DMO, rivedere con chiamate SQL

		//--------------------------------------------------------------------------------------
		public static string[] GetSQLServers()
		{
            // TODO ITRI RIvedere logiche con DMO 
            //return null;

            //if (!CheckSQLDMOInstallation())
            //    return null;
			
            ArrayList serverNames = new ArrayList();
            //try
            //{
            //    // Uso SQLDMO per trovare i server di SQL disponibili in rete
            //    SQLDMO.SQLServer2 mySqlServer = new SQLDMO.SQLServer2();

            //    SQLDMO.NameList availableServers = mySqlServer.Application.ListAvailableSQLServers();

            //    if (availableServers != null && availableServers.Count > 0)
            //    {
            //        for (int i = 1; i <= availableServers.Count; i++)
            //            serverNames.Add(availableServers.Item(i));	
            //    }
				
            //    //Registry for local
            //    RegistryKey sqlServerKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
            //    if (sqlServerKey != null)
            //    {
            //        String[] instances = (String[])sqlServerKey.GetValue("InstalledInstances");
            //        if (instances != null && instances.Length > 0)
            //        {
            //            foreach (string element in instances)
            //            {
            //                String aServerName = String.Empty;
						
            //                //only add if it doesn't exist
            //                if (element == "MSSQLSERVER")
            //                    aServerName = "(local)";
            //                else
            //                    aServerName = System.Environment.MachineName + @"\" + element;

            //                bool serverFound = false;
            //                if (serverNames.Count > 0)
            //                {
            //                    foreach (string addedServerName in serverNames)
            //                    {
            //                        if (String.Compare(addedServerName, aServerName) == 0)
            //                        {
            //                            serverFound = true;
            //                            break;
            //                        }
            //                    }
            //                }
            //                if (!serverFound)
            //                    serverNames.Add(aServerName);
            //            }
            //        }
            //    }
            //}
            //catch(Exception)
            //{
            //}
            //return (serverNames != null && serverNames.Count > 0) ? (string[])serverNames.ToArray(typeof(string)) : null;

/*
            // PRIMA CARICA I SERVER LOCALI
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
                String[] instances = (String[])rk.GetValue("InstalledInstances");

                if (instances != null && instances.Length > 0)
                {
                    foreach (String element in instances)
                    {
                        if (string.Compare(element, "MSSQLSERVER", StringComparison.InvariantCultureIgnoreCase) == 0)
                            serverNames.Add(System.Net.Dns.GetHostName().ToUpperInvariant());
                        else
                            serverNames.Add(Path.Combine(System.Net.Dns.GetHostName().ToUpperInvariant(), element));
                    }
                }
            }
            catch
            {
            }
*/

            // QUINDI A SEGUIRE I SERVER IN RETE
            try
            {
                SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                DataTable table = instance.GetDataSources();
                if (table == null)
                    return null;

                string serverName, instanceName, version;

                foreach (DataRow row in table.Rows)
                {
                    serverName = string.Empty;
                    instanceName = string.Empty;
                    version = string.Empty;

                    foreach (DataColumn col in table.Columns)
                    {
                        if (string.Compare(col.ColumnName, "ServerName", StringComparison.InvariantCultureIgnoreCase) == 0)
                            serverName = row[col].ToString();

                        if (string.Compare(col.ColumnName, "InstanceName", StringComparison.InvariantCultureIgnoreCase) == 0)
                            instanceName = row[col].ToString();

                        if (string.Compare(col.ColumnName, "Version", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            string[] tokenVer = row[col].ToString().Split('.');
                            if (tokenVer != null && tokenVer.Length >= 2)
                                version = string.Concat("(", tokenVer[0], ".", tokenVer[1], ")");
                        }
                    }

                    // aggiungo, se esistenti, il nome istanza e i primi 2 numeri di versione
                    if (!string.IsNullOrEmpty(serverName))
                        serverNames.Add
                        //networkServers.Add
                            (
                            (instanceName.Length > 0)
                            ? Path.Combine(serverName, instanceName) /*+ ((version.Length > 0) ? ("\t" + version) : string.Empty)*/
                            : serverName /*+ ((version.Length > 0) ? ("\t" + version) : string.Empty)*/
                            );
                }
            }
            catch
            {
            }

            return (serverNames != null && serverNames.Count > 0) ? (string[])serverNames.ToArray(typeof(string)) : null;
		}

		//--------------------------------------------------------------------------------------
		public static string[] GetSQLDatabaseNames
			(
			string	selectedServer, 
			string	instanceName,
			string	serverName, 
			string	user, 
			string	password,
			bool	useWindowsAuthentication
			)
		{
            // TODO ITRI RIvedere logiche con DMO 

            //if 
            //    (
            //    serverName == null ||
            //    serverName.Length == 0 || 
            //    (!useWindowsAuthentication && (user == null || user.Length == 0)) ||
            //    !CheckSQLDMOInstallation()
            //    )
            //    return null;
			
            //// Uso SQLDMO per trovare i database disponibili per il server selezionato
            //SQLDMO.SQLServer2 mySqlServer = new SQLDMO.SQLServer2();
            //try
            //{
            //    string serverToConnect = serverName;
            //    if (instanceName != null && instanceName.Length > 0)
            //        serverToConnect = serverToConnect + Path.DirectorySeparatorChar + instanceName;

            //    mySqlServer.Name = serverToConnect;

            //    mySqlServer.LoginSecure = useWindowsAuthentication;

            //    mySqlServer.Connect(selectedServer, user , password);

            //    SQLDMO.Databases availableDatabases = mySqlServer.Databases;

            //    if (availableDatabases != null && availableDatabases.Count > 0)
            //    {
            //        ArrayList databaseNames = new ArrayList();
            //        foreach (SQLDMO.Database2 db in availableDatabases)
            //            databaseNames.Add(db.Name);	

            //        return (string[])databaseNames.ToArray(typeof(string));
            //    }
            //}
            //catch(Exception)
            //{
            //}
            //finally
            //{
            //    mySqlServer.DisConnect();
            //}
			//return null;

            ArrayList databaseNames = new ArrayList();

            if (serverName.Length == 0)
            {
                // Il nome del server è vuoto, non carico niente
                return null;
            }

            if (user.Length == 0)
            {
                // Il nome della login è vuoto, non carico niente
                return null;
            }

            string connectionString = (useWindowsAuthentication)
                ? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
                : string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, user, password);

            string dbName = string.Empty;

            TBConnection myConnection = null;
            TBCommand myCommand = null;
            IDataReader myReader = null;

            try
            {
                myConnection = new TBConnection(connectionString, DBMSType.SQLSERVER);
                myConnection.Open();

                // skippo di database di sistema e di esempio di SQL e controllo che lo stato sia ONLINE
                string query =
                    @"SELECT name FROM sysdatabases
					WHERE name NOT IN 
					('master','model','msdb','tempdb','pubs','Northwind','AdventureWorks','AdventureWorksLT','ReportServer','ReportServerTempDB')
					AND DATABASEPROPERTYEX(name,'status') = 'ONLINE'";

                myCommand = new TBCommand(query, myConnection);
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    dbName = myReader["name"].ToString();

                    databaseNames.Add(dbName);
                }

                myReader.Close();
            }
            catch /*(TBException e)*/
            {
/*
                string message = string.Format(DatabaseWinControlsStrings.CannotContactSQLServer, serverName);
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
                extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
                extendedInfo.Add(DatabaseLayerStrings.Server, serverName);
                extendedInfo.Add(DatabaseLayerStrings.Function, "LoadAllSQLDatabases");
                extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseWinControls");
                extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
                extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
                extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
                diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
                return false;
 */
                return null;
            }
            finally
            {
                if (myReader != null && !myReader.IsClosed)
                    myReader.Close();

                if (myConnection.State == ConnectionState.Open ||
                    myConnection.State == ConnectionState.Broken)
                {
                    myConnection.Close();
                    myConnection.Dispose();
                }
            }

            return (string[])databaseNames.ToArray(typeof(string));
		}
		
		//--------------------------------------------------------------------------------------------------------
		public static string BuildConnectionString
			(
			string	serverName, 
			string	instanceName,
			string	user, 
			string	password,
			bool	useWindowsAuthentication,
			string	databaseName
			)
		{
			if (databaseName == null || databaseName.Length == 0)
				return String.Empty;
	
			string connectionString = String.Empty;

			string fullServerName =  serverName;
			if (instanceName != null && instanceName.Length > 0)
				fullServerName += "\\" + instanceName;
		
			connectionString += CONNECTION_STRING_DATASOURCE_KEYWORD + "=" + fullServerName + ";";
			connectionString +=  CONNECTION_STRING_INITIAL_CATALOG_KEYWORD + "=" + databaseName + ";";

			if (useWindowsAuthentication)
			{
				connectionString +=  CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN;// the connection should use Windows integrated security (NT authentication)
				connectionString +=  "=";
				connectionString +=  CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE;
				connectionString +=  ";";
			}
			else if (user != null && user.Length > 0)
			{
				connectionString +=  CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD + "=" + user;

				if (password != null && password.Length > 0)
					connectionString +=  ";" + CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD + "=" + password;
				connectionString +=  ";";
			}

			return connectionString;
		}			

		//-----------------------------------------------------------------------------
		public static string[] GetAllTables(string aConnectionString)
		{ 
			if 
				(
				aConnectionString == null || 
				aConnectionString.Length == 0
				)
				return null;

			TBConnection connection = null;

			try
			{
				connection = new TBConnection(aConnectionString, DBMSType.SQLSERVER);

				if (connection == null)
					return null;
				
				connection.Open();

				TBDatabaseSchema dbSchema = new TBDatabaseSchema(connection);

				SchemaDataTable tables = dbSchema.GetAllSchemaObjects(DBObjectTypes.TABLE);

				if (tables.Rows == null || tables.Rows.Count == 0)
					return null;

				ArrayList tableNames = new ArrayList();
				foreach (DataRow aRow in tables.Rows)
					tableNames.Add(aRow[DBSchemaStrings.Name]);	

				return (string[])tableNames.ToArray(typeof(string));

			}
			catch (TBException e)
			{
				Debug.Fail("Exception raised during tables retrieval: " + e.Message);
				throw e;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}

		//-----------------------------------------------------------------------------
		public static System.Data.DataTable GetTableSchema(string aTableName, string aConnectionString)
		{ 
			if 
				(
				aTableName == null ||
				aTableName.Length == 0 ||
				aConnectionString == null || 
				aConnectionString.Length == 0
				)
				return null;

			TBConnection connection = null;

			try
			{
				connection = new TBConnection(aConnectionString, DBMSType.SQLSERVER);

				if (connection == null)
					return null;
				
				connection.Open();

				TBDatabaseSchema dbSchema = new TBDatabaseSchema(connection);

				return dbSchema.GetTableSchema(aTableName, true);
			}
			catch (TBException e)
			{
				Debug.Fail("Exception raised during table schema retrieval: " + e.Message);
				return null;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}

		//---------------------------------------------------------------------------
		public static DefaultDataTable GetTableDefaults(string aTableName, string aConnectionString)
		{ 
			if 
				(
				aTableName == null ||
				aTableName.Length == 0 ||
				aConnectionString == null || 
				aConnectionString.Length == 0
				)
				return null;

			TBConnection connection = null;

			try
			{
				connection = new TBConnection(aConnectionString, DBMSType.SQLSERVER);

				if (connection == null)
					return null;
				
				connection.Open();

				TBDatabaseSchema dbSchema = new TBDatabaseSchema(connection);

				return dbSchema.LoadDefaults(aTableName);
			}
			catch (TBException e)
			{
				Debug.Fail("Exception raised during table defaults retrieval: " + e.Message);
				return null;
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
					connection.Close();
			}
		}
		
		//---------------------------------------------------------------------------
		internal static bool GenerateTableDBInfoFile(WizardTableInfo aTableInfo, string aFileName)
		{
			if 
				(
				aTableInfo == null ||
				aTableInfo.Name == null || 
				aTableInfo.Name.Length == 0 || 
				aTableInfo.Library == null ||
				aTableInfo.Library.Module == null ||
				aFileName == null ||
				aFileName.Length == 0
				)
				return false;
			
			try
			{
				XmlDocument dbInfoDocument = new XmlDocument();

				//Dichiarazione XML
				XmlDeclaration configDeclaration = dbInfoDocument.CreateXmlDeclaration(DBInfoXmlVersion, DBInfoXmlEncoding, null);
				if (configDeclaration != null)
					dbInfoDocument.AppendChild(configDeclaration);
			
				// <?xml-stylesheet type='text/xsl' href='../../../dbInfoTemplate.xslt'?>
				//string text = String.Format("type='text/xsl' href='{0}'", xslPath);
				//XmlProcessingInstruction processingInstruction = dbInfoDocument.CreateProcessingInstruction("xml-stylesheet", text);
				//dbInfoDocument.AppendChild(processingInstruction);

				// Creazione della root ovvero <Emuns>
				XmlElement tableElement = dbInfoDocument.CreateElement(XML_DBINFO_TABLE_TAG);
				
				tableElement.SetAttribute(XML_DBINFO_TABLE_NAME_ATTRIBUTE, aTableInfo.Name);
				tableElement.SetAttribute(XML_DBINFO_MODULE_NAME_ATTRIBUTE, aTableInfo.Library.Module.Name);
				tableElement.SetAttribute(XML_DBINFO_TABLE_NAMESPACE_ATTRIBUTE, aTableInfo.GetNameSpace());
				tableElement.SetAttribute(XML_DBINFO_TABLE_RUNTIMECLASS_ATTRIBUTE, aTableInfo.ClassName);
						
				dbInfoDocument.AppendChild(tableElement);

				XmlElement overviewElement = dbInfoDocument.CreateElement(XML_DBINFO_OVERVIEW_TAG);
				if (overviewElement != null)
					tableElement.AppendChild(overviewElement);

				XmlElement fieldsElement = dbInfoDocument.CreateElement(XML_DBINFO_FIELDS_TAG);
				if (fieldsElement != null)
				{
					//Ora inizio a inserire le colonne
					if (aTableInfo.ColumnsCount > 0)
					{
						foreach(WizardTableColumnInfo aColumnInfo in aTableInfo.ColumnsInfo)
						{
							XmlElement columnElement =  dbInfoDocument.CreateElement(XML_DBINFO_COLUMN_TAG);
						
							columnElement.SetAttribute(XML_DBINFO_COLUMN_NAME_ATTRIBUTE, aColumnInfo.Name);
							columnElement.SetAttribute(XML_DBINFO_COLUMN_TYPE_ATTRIBUTE, aColumnInfo.GetWoormBaseDataTypeText());
							columnElement.SetAttribute(XML_DBINFO_COLUMN_LENGTH_ATTRIBUTE, aColumnInfo.DatabaseLength.ToString(NumberFormatInfo.InvariantInfo));
							if (aColumnInfo.EnumInfo != null)
							{
								if (aColumnInfo.EnumInfo.Application == null || String.Compare(aColumnInfo.EnumInfo.Application.Name, aTableInfo.Library.Application.Name) == 0)
									columnElement.SetAttribute(XML_DBINFO_COLUMN_ENUM_ATTRIBUTE, String.Format(XML_DBINFO_COLUMN_ENUM_ATTRIBUTE_FORMAT, aColumnInfo.EnumInfo.Module.Name, aColumnInfo.EnumInfo.Value.ToString(NumberFormatInfo.InvariantInfo)));
								else
									columnElement.SetAttribute(XML_DBINFO_COLUMN_ENUM_ATTRIBUTE, String.Format(XML_DBINFO_COLUMN_ENUM_ATTRIBUTE_FORMAT, aColumnInfo.EnumInfo.Module.GetNameSpace(), aColumnInfo.EnumInfo.Value.ToString(NumberFormatInfo.InvariantInfo)));
							}
                            // altri attributi
                            if (aColumnInfo.IsPrimaryKeySegment)
                                columnElement.SetAttribute(XML_DBINFO_COLUMN_MANDATORY_ATTRIBUTE,   "X");
                            else
                                columnElement.SetAttribute(XML_DBINFO_COLUMN_MANDATORY_ATTRIBUTE,   "");
                            columnElement.SetAttribute(XML_DBINFO_COLUMN_DEFAULT_ATTRIBUTE,         "");
                            columnElement.SetAttribute(XML_DBINFO_COLUMN_READONLY_ATTRIBUTE,        "");
                            columnElement.SetAttribute(XML_DBINFO_COLUMN_REFERENCE_ATTRIBUTE,       "");
                            columnElement.SetAttribute(XML_DBINFO_COLUMN_LOCALIZABLE_ATTRIBUTE,     "true");
							fieldsElement.AppendChild(columnElement);
						}
					}

					tableElement.AppendChild(fieldsElement);

					XmlElement keysElement = dbInfoDocument.CreateElement(XML_DBINFO_KEYS_TAG);
					if (keysElement != null)
					{
						if (aTableInfo.IsPrimaryKeyDefined)
						{
							XmlElement primaryKeyElement = dbInfoDocument.CreateElement(XML_DBINFO_KEY_TAG);

							primaryKeyElement.SetAttribute(XML_DBINFO_KEY_NAME_ATTRIBUTE, aTableInfo.PrimaryKeyConstraintName);
							primaryKeyElement.SetAttribute(XML_DBINFO_KEY_TYPE_ATTRIBUTE, XML_DBINFO_PRIMARY_KEY_ATTRIBUTE_VALUE);

							foreach(WizardTableColumnInfo aColumnInfo in aTableInfo.ColumnsInfo)
							{
								if (!aColumnInfo.IsPrimaryKeySegment)
									continue;
								
								XmlElement columnElement =  dbInfoDocument.CreateElement(XML_DBINFO_COLUMN_TAG);
						
								columnElement.SetAttribute(XML_DBINFO_COLUMN_NAME_ATTRIBUTE, aColumnInfo.Name);
                                columnElement.SetAttribute(XML_DBINFO_COLUMN_LOCALIZABLE_ATTRIBUTE, "true");
						
								primaryKeyElement.AppendChild(columnElement);
							}
							keysElement.AppendChild(primaryKeyElement);
                            tableElement.AppendChild(keysElement);
                        }
					}
				}

				dbInfoDocument.Save(aFileName);

				return true;
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in DataParser.GenerateTableDBInfoFile:", exception.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------------
		internal static void AdjustTableInfo(WizardTableInfo aTableInfo, string aFileName)
		{
			if 
				(
				aTableInfo == null ||
				aFileName == null ||
				aFileName.Length == 0 ||
				!File.Exists(aFileName)
				)
				return;

			try
			{
				XmlDocument dbInfoDocument = new XmlDocument();

				dbInfoDocument.Load(aFileName);

				if (String.Compare(dbInfoDocument.DocumentElement.Name, XML_DBINFO_TABLE_TAG) != 0)
					return;
				
				if (dbInfoDocument.DocumentElement.HasAttribute(XML_DBINFO_TABLE_NAME_ATTRIBUTE))
				{
					string tableName = dbInfoDocument.DocumentElement.GetAttribute(XML_DBINFO_TABLE_NAME_ATTRIBUTE);
					if (tableName != null && tableName.Length > 0)
					{
						if (aTableInfo.Name != null && aTableInfo.Name.Length > 0)
						{
							if (String.Compare(tableName, aTableInfo.Name, true) != 0)
								return; // i nomi non corrispondono
						}
						else
							aTableInfo.Name = tableName;
					}
				}

				if (dbInfoDocument.DocumentElement.HasAttribute(XML_DBINFO_TABLE_RUNTIMECLASS_ATTRIBUTE))
				{
					string tableClassname = dbInfoDocument.DocumentElement.GetAttribute(XML_DBINFO_TABLE_RUNTIMECLASS_ATTRIBUTE);
					if (tableClassname != null && tableClassname.Length > 0)
						aTableInfo.ClassName = tableClassname;
				}

				// Posso "mettere a posto" i tipi di dato di ciascuna colonna. Ad esempio, i campi di
				// tipo booleano sul database vengono creati come char di lunghezza 1 e non come tipi 
				// logici. Allo stesso modo posso anche aggiustare eventuali dati enumerativi.
				if (aTableInfo.ColumnsCount > 0)
				{
					XmlNode fieldsNode = dbInfoDocument.DocumentElement.SelectSingleNode("child::" + XML_DBINFO_FIELDS_TAG);

					if (fieldsNode != null && (fieldsNode is XmlElement) && fieldsNode.HasChildNodes)
					{
						foreach(WizardTableColumnInfo aColumnInfo in aTableInfo.ColumnsInfo)
						{
							XmlNode columnNode = fieldsNode.SelectSingleNode("child::" + XML_DBINFO_COLUMN_TAG + "[@" + XML_DBINFO_COLUMN_NAME_ATTRIBUTE +"='" + aColumnInfo.Name + "']");
							if (columnNode == null || !(columnNode is XmlElement))
								continue;

							string typeReadText = ((XmlElement)columnNode).GetAttribute(XML_DBINFO_COLUMN_TYPE_ATTRIBUTE);
							if (typeReadText == null || typeReadText.Length == 0)
								continue;

							WizardTableColumnDataType.DataType typeRead = WizardTableColumnDataType.GetDataTypeFromWoormText(typeReadText);
							if (typeRead == WizardTableColumnDataType.DataType.Undefined)
								continue;

							WizardEnumInfo enumInfo = null;
							if (typeRead == WizardTableColumnDataType.DataType.Enum)
							{
								if (aTableInfo.Library == null || aTableInfo.Library.Application == null)
									continue;

								string enumText = ((XmlElement)columnNode).GetAttribute(XML_DBINFO_COLUMN_ENUM_ATTRIBUTE);
								if (enumText != null && enumText.Length > 0)
								{
									int slashIdx = enumText.LastIndexOf('/');
									if (slashIdx >= 0 && slashIdx < (enumText.Length - 1))
									{
										string enumValueText = enumText.Substring(slashIdx + 1);
										if (enumValueText == null || enumValueText.Length == 0)					
											continue;

										try
										{
											ushort enumValue = UInt16.Parse(enumValueText);
											if 
												(
												aColumnInfo.EnumInfo != null && 
												aColumnInfo.EnumInfo.Value == enumValue
												)
												continue;
											
											enumInfo = aTableInfo.Library.Application.GetEnumInfoByValue(enumValue);

										}
										catch(FormatException exception)
										{
											Debug.Fail("FormatException thrown in DataParser.AdjustTableInfo during enum evaluation (enum value = " + enumValueText + "):", exception.Message);
										}
										catch(OverflowException exception)
										{
											Debug.Fail("OverflowException thrown in DataParser.AdjustTableInfo during enum evaluation (enum value = " + enumValueText + "):", exception.Message);
										}
									}
								}	
							}
							aColumnInfo.DataType = new WizardTableColumnDataType(typeRead);
							aColumnInfo.EnumInfo = enumInfo;
						}
					}
				}
			}
			catch(XmlException exception)
			{
				Debug.Fail("Exception thrown in DataParser.AdjustTableInfo (File: " + aFileName + "):", exception.Message);
			}
		}
	}
}
