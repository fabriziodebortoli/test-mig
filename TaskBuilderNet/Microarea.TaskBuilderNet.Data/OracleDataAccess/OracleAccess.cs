using System;
using System.Collections;
using System.Data;
using System.Data.OracleClient;
using System.Diagnostics;
using System.IO;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

#pragma warning disable 0618
// disabilito temporaneamente warning CS0618: 'System.Data.OracleClient.OracleConnection' is obsolete: 
// 'OracleConnection has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260'

namespace Microarea.TaskBuilderNet.Data.OracleDataAccess
{
	[Flags]
	public enum Role { Administrator = 1, User = 2, DbOwner = 4};

	/// <summary>
	/// OracleAccess.
	/// Classe per la connessione ad Oracle
	/// </summary>
	// ========================================================================
	public class OracleAccess
	{
		private OracleUserInfo oracleUserInfo;
		private OracleConnection currentConnection = null;
		private Diagnostic diagnostic = new Diagnostic("OracleDataAccess");
		
		private string	nameSpace = string.Empty;

		//---------------------------------------------------------------------
		public OracleUserInfo	ContextOracleInfo		{ get { return oracleUserInfo; }}
		public OracleConnection CurrentConnection		{ get { return currentConnection; } set { currentConnection	= value; } }
		public Diagnostic	Diagnostic	{ get { return diagnostic; }}

		//mi serve per specificare da quale assembly viene chiamata la libreria così riesco a caricare l'help correttamente per le popUp
		public string NameSpace { get { return nameSpace; } set { nameSpace = value; } }

		//---------------------------------------------------------------------
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string server);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;
		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string server, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;
		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string server);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		/// <summary>
		/// Costruttore (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public OracleAccess()
		{
		}

		#region LoadUserData - Carica le info nella oracleUserInfo (con cui effettua la connessione)
		/// <summary>
		/// LoadUserData
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadUserData(string oracleService, string oracleUserId, string oracleUserPwd, bool oracleUserIsWinNT)
		{
			oracleUserInfo = new OracleUserInfo();
			oracleUserInfo.OracleService	  = oracleService;
			oracleUserInfo.OracleCompanyName  = oracleUserId;
			oracleUserInfo.OracleUserId		  = oracleUserId;
			oracleUserInfo.OracleUserPwd	  = oracleUserPwd;
			oracleUserInfo.OracleUserIsWinNT  = oracleUserIsWinNT;
			//costruisco la stringa di connessione
			oracleUserInfo.BuildStringConnection();
		}
		
		/// <summary>
		/// LoadUserData
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadUserData
			(
			string oracleService, 
			string oracleCompanyName,
			string oracleUserId, 
			string oracleUserPwd, 
			bool   oracleUserIsWinNT
			)
		{
			oracleUserInfo = new OracleUserInfo();
			oracleUserInfo.OracleService	  = oracleService;
			oracleUserInfo.OracleCompanyName  = oracleCompanyName;
			oracleUserInfo.OracleUserId		  = oracleUserId;
			oracleUserInfo.OracleUserPwd	  = oracleUserPwd;
			oracleUserInfo.OracleUserIsWinNT  = oracleUserIsWinNT;
			//costruisco la stringa di connessione
			oracleUserInfo.BuildStringConnection();
		}
		#endregion

		#region LoadSystemData
		/// <summary>
		/// LoadSystemData
		/// </summary>
		//---------------------------------------------------------------------
		public OracleUserImpersonatedData LoadSystemData(string oracleService)
		{
			OracleUserImpersonatedData systemData	= new OracleUserImpersonatedData();
			systemData.Login						= "system";
			systemData.Password						= string.Empty;
			systemData.OracleService				= oracleService;
			systemData.IsDba						= true;
			systemData.WindowsAuthentication		= false;
			return systemData;
		}
		#endregion

		#region TryToConnect - Prova ad aprire una connessione
		/// <summary>
		/// TryToConnect
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool TryToConnect(OracleUserImpersonatedData userImpersonate)
		{
			bool success = false;
			
			if (string.IsNullOrWhiteSpace(oracleUserInfo.ConnectionString))
			{
				Diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleCompanyName));
				return success;
			}

			TBConnection compDBConn = null;
			
			try
			{
				// apro la connessione
				compDBConn = new TBConnection(oracleUserInfo.ConnectionString, DBMSType.ORACLE);
				compDBConn.Open();

				if (compDBConn.State != ConnectionState.Open)
				{
					// questo e' un caso particolare....
					// con le macchine x64 e un'installazione di Mago in un path che contiene caratteri speciali (ad es. c:\Program Files (x86)\)
					// puo' accadere che nonostante la Open non ritorni errori in realta' la connessione al server Oracle abbia lo stato Closed
					// in questo modo diamo un dettaglio dell'errore che magari puo' essere utile per individuare la causa
					// N.B. e' un problema a basso livello del Networking di Oracle, noi per ora non possiamo fare nulla
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, string.Format(OracleDataAccessStrings.PathWithSpecialChars, Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));
					extendedInfo.Add(DatabaseLayerStrings.Function, "TryToConnect");
					extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.OracleDataAccess");
					Diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleCompanyName), extendedInfo);
					return success;
				}

				Diagnostic.Set(DiagnosticType.Information, OracleDataAccessStrings.SuccessOracleConnection);

				TBCommand command = new TBCommand("SELECT * FROM NLS_INSTANCE_PARAMETERS", compDBConn);
				IDataReader reader = command.ExecuteReader();

				while (reader.Read())
				{
					if (string.Compare(reader["PARAMETER"].ToString(), "NLS_LANGUAGE", StringComparison.InvariantCultureIgnoreCase) == 0)
						userImpersonate.Language = reader["VALUE"].ToString();

					if (string.Compare(reader["PARAMETER"].ToString(), "NLS_TERRITORY", StringComparison.InvariantCultureIgnoreCase) == 0)
						userImpersonate.Territory = reader["VALUE"].ToString();
				}

				reader.Close();
				success = true;
			}
			catch (TBException tbExc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	tbExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Source,		tbExc.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace,	tbExc.StackTrace);
				if (tbExc.TargetSite != null)
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	tbExc.TargetSite.Name);
				extendedInfo.Add(DatabaseLayerStrings.Function,		"TryToConnect");
				extendedInfo.Add(DatabaseLayerStrings.Library,		"Microarea.TaskBuilderNet.Data.OracleDataAccess");
				Diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleCompanyName), extendedInfo);
			}
			finally
			{
				if (compDBConn != null && compDBConn.State != ConnectionState.Closed)
				{
					compDBConn.Close();
					compDBConn.Dispose();
					compDBConn = null;
				}
			}
			return success;
		}
		#endregion

		#region OpenConnection - Apre la connessione
		/// <summary>
		/// OpenConnection
		/// Apre la connessione
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public void OpenConnection()
		{
			CurrentConnection = new OracleConnection(oracleUserInfo.ConnectionString);

			try
			{
				CurrentConnection.Open();
			}
			catch(OracleException exOracleConnection)
			{
				Debug.Fail(exOracleConnection.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				if (exOracleConnection.InnerException != null)
				{
					extendedInfo.Add(DatabaseLayerStrings.Description,	exOracleConnection.InnerException.Message);
					extendedInfo.Add(DatabaseLayerStrings.Source,		exOracleConnection.InnerException.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace,	exOracleConnection.InnerException.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	exOracleConnection.InnerException.TargetSite.Name);
				}
				else
				{
					extendedInfo.Add(DatabaseLayerStrings.Description,	exOracleConnection.Message);
					extendedInfo.Add(DatabaseLayerStrings.Source,		exOracleConnection.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace,	exOracleConnection.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	exOracleConnection.TargetSite.Name);
				}
				extendedInfo.Add(DatabaseLayerStrings.Function,	 "OpenConnection");
				extendedInfo.Add(DatabaseLayerStrings.Library,	 "Microarea.TaskBuilderNet.Data.OracleDataAccess");

				Diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleUserId), extendedInfo);
				CurrentConnection = null;
			}
		}
		#endregion

		#region CloseConnection - chiude la connessione
		/// <summary>
		/// CloseConnection
		/// </summary>
		//---------------------------------------------------------------------
		public void CloseConnection()
		{
			if (CurrentConnection == null) 
				return;

			try
			{
				CurrentConnection.Close();
			}
			catch(OracleException exOracleConnection)
			{
				Debug.Fail(exOracleConnection.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				if (exOracleConnection.InnerException != null)
				{
					extendedInfo.Add(DatabaseLayerStrings.Description,	exOracleConnection.InnerException.Message);
					extendedInfo.Add(DatabaseLayerStrings.Source,		exOracleConnection.InnerException.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace,	exOracleConnection.InnerException.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	exOracleConnection.InnerException.TargetSite.Name);
				}
				else
				{
					extendedInfo.Add(DatabaseLayerStrings.Description,	exOracleConnection.Message);
					extendedInfo.Add(DatabaseLayerStrings.Source,		exOracleConnection.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace,	exOracleConnection.StackTrace);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	exOracleConnection.TargetSite.Name);
				}
				extendedInfo.Add(DatabaseLayerStrings.Function,	 "CloseConnection");
				extendedInfo.Add(DatabaseLayerStrings.Library,	 "Microarea.TaskBuilderNet.Data.OracleDataAccess");

				Diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ConnectionFailed, oracleUserInfo.OracleService, oracleUserInfo.OracleUserId), extendedInfo);
			}
		}
		#endregion

		#region Funzioni per leggere gli utenti di Oracle

		#region GetFreeDBUsersForAttach - Restituisce tutti gli utenti presenti nel server Oracle liberi per effettuare un attach
		/// <summary>
		/// GetFreeDBUsersForAttach
		/// Definito in DatabaseLayer
		/// Restituisce tutti gli utenti presenti nel database per effettuare un attach
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList GetFreeDBUsersForAttach()
		{
			ArrayList allDatabaseOracleUsers = new ArrayList();

			try
			{
				TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(CurrentConnection);
				UserDataTable allDatabaseUsers =  oraAdminFunct.GetFreeDBUsersForAttach();
				foreach (DataRow dR in allDatabaseUsers.Rows)
					allDatabaseOracleUsers.Add(new OracleUser((string)dR[DBSchemaStrings.UserName], (bool)dR[DBSchemaStrings.OSAuthent]));
			}
			catch(TBException tbExc)
			{
				Debug.Fail(tbExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Function, tbExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Description, tbExc.ExtendedMessage);
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.UnableReadUsers, extendedInfo);
			}
			return allDatabaseOracleUsers;
		}
		#endregion

		#region GetFreeDBUsersForAssociation - Restituisce tutti gli utenti presenti nel server Oracle liberi per effettuare un associazione
		/// <summary>
		/// GetFreeDBUsersForAssociation
		/// Definito in DatabaseLayer
		/// Restituisce tutti gli utenti presenti nel database liberi per effettuare un'associazione
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList GetFreeDBUsersForAssociation()
		{
			ArrayList allDatabaseOracleUsers = new ArrayList();

			try
			{
				TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(CurrentConnection);
				UserDataTable allDatabaseUsers =  oraAdminFunct.GetFreeDBUsersForAssociation();
				foreach (DataRow dR in allDatabaseUsers.Rows)
					allDatabaseOracleUsers.Add(new OracleUser((string)dR[DBSchemaStrings.UserName], (bool)dR[DBSchemaStrings.OSAuthent]));
			}
			catch(TBException tbExc)
			{
				Debug.Fail(tbExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Function, tbExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Description, tbExc.ExtendedMessage);
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.UnableReadUsers, extendedInfo);
			}
			return allDatabaseOracleUsers;
		}
		#endregion

		#region ExistUserIntoDatabase - true se l'utente specificato esiste tra gli utenti oracle
		/// <summary>
		/// ExistUserIntoDatabase
		/// il secondo parametro forAttach è per stabilire se devo cercare l'utente per fare un'associazione
		/// utente o per fare un'attach ad un'azienda specifica
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistUserIntoDatabase(string userName, bool forAttach)
		{
			bool existUser = false;
			
			ArrayList allDatabaseOracleUsers = new ArrayList();
			
			try
			{
				TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(CurrentConnection);
				UserDataTable allDatabaseUsers = (forAttach) 
												? oraAdminFunct.GetFreeDBUsersForAttach() 
												: oraAdminFunct.GetFreeDBUsersForAssociation();

				foreach (DataRow dR in allDatabaseUsers.Rows)
					if (string.Compare((string)dR[DBSchemaStrings.UserName], userName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						existUser = true;
						break;
					}
			}
			catch(TBException tbExc)
			{
				Debug.Fail(tbExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Function, tbExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Description, tbExc.ExtendedMessage);
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.UnableReadUsers, extendedInfo);
			}

			return existUser;
		}
		#endregion

		#region GetAllUsersToSchema Restituisce tutti gli utenti che hanno almeno un synonym sull'azienda
		/// <summary>
		/// GetAllUsersToSchema
		/// Definito in DatabaseLayer (Anna Bauzone)
		/// Restituisce tutti gli utenti che hanno almeno un synonym allo schema passato come parametro 
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList GetAllUsersToSchema(string dbName)
		{
			ArrayList allSchemaOracleUsers = new ArrayList();

			try
			{
				TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(CurrentConnection);
				UserDataTable allSchemaUsers =  oraAdminFunct.GetAllUsersToSchema(dbName);
			
				foreach (DataRow dR in allSchemaUsers.Rows)
					allSchemaOracleUsers.Add(new OracleUser((string)dR[DBSchemaStrings.UserName], (bool)dR[DBSchemaStrings.OSAuthent]));
			}
			catch(TBException tbExc)
			{
				Debug.Fail(tbExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Function, tbExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Description, tbExc.ExtendedMessage);
				diagnostic.Set(DiagnosticType.Error, OracleDataAccessStrings.UnableReadUsers, extendedInfo);
			}
			return allSchemaOracleUsers;
		}
		#endregion

		#endregion

		#region Funzioni per la creazione degli utenti

		#region CreateUser - Inserimento di un nuovo utente oracle
		/// <summary>
		/// CreateUser
		/// Creo l'utente in oracle (e quindi anche il db perchè USERID = Database)
		/// </summary>
		//---------------------------------------------------------------------
		public bool CreateUser(string ownerName, string ownerPwd)
		{
			OracleCommand command = null; 
			bool result = false;
	
			try				
			{
				string query = string.Format(OracleDBSchemaStrings.CreateUser, ownerName.ToUpperInvariant(), ownerPwd);
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.AlterUserQuotaOnTableSpace, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantTablespaceToUser, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantConnect, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantResource, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateMaterializedView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				result = true;
			}
			catch (OracleException e)
			{
				throw(new TBException(e.Message, e));
			}
			finally
			{
				command.Dispose();
			}
			
			return result;
		}
		#endregion

		#region CreateAssociatedUser - Inserimento di un nuovo utente oracle x associazione (CREATE ANY SYNONYM)
		/// <summary>
		/// CreateAssociatedUser
		/// </summary>
		//---------------------------------------------------------------------
		public bool CreateAssociatedUser(string ownerName, string ownerPwd)
		{
			OracleCommand command = null; 
			bool result = false;
	
			try				
			{
				string query = string.Format(OracleDBSchemaStrings.CreateUser, ownerName.ToUpperInvariant(), ownerPwd);
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.AlterUserQuotaOnTableSpace, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantTablespaceToUser, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantConnect, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantResource, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateAnySynonym, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateMaterializedView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				result = true;
			}
			catch (OracleException e)
			{
				throw(new TBException(e.Message, e));
			}
			finally
			{
				command.Dispose();
			}

			return result;
		}
		#endregion

		#region CreateNTUser - Inserimento di un nuovo utente oracle
		/// <summary>
		/// CreateNTUser
		/// Creo l'utente NT in oracle (e quindi anche il db perchè USERID = Database)
		/// </summary>
		//---------------------------------------------------------------------
		public bool CreateNTUser(string ownerName, string ownerPwd)
		{
			OracleCommand command = null; 
			bool result = false;
	
			try				
			{
				string query = string.Format(OracleDBSchemaStrings.CreateNTUser, ownerName.ToUpperInvariant(), ownerPwd);
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.AlterUserQuotaOnTableSpace, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantTablespaceToUser, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantConnect, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantResource, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateMaterializedView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				result = true;
			}
			catch (OracleException e)
			{
				throw(new TBException(e.Message, e));
			}
			finally
			{
				command.Dispose();
			}

			return result;
		}
		#endregion

		#region CreateNTAssociatedUser - Inserimento di un nuovo utente NT oracle x associazione (CREATE ANY SYNONYM)
		/// <summary>
		/// CreateNTAssociatedUser
		/// </summary>
		//---------------------------------------------------------------------
		public bool CreateNTAssociatedUser(string ownerName, string ownerPwd)
		{
			OracleCommand command = null; 
			bool result = false;
	
			try				
			{
				string query = string.Format(OracleDBSchemaStrings.CreateNTUser, ownerName.ToUpperInvariant(), ownerPwd);
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.AlterUserQuotaOnTableSpace, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantTablespaceToUser, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantConnect, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantResource, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateAnySynonym, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				query = string.Format(OracleDBSchemaStrings.GrantCreateMaterializedView, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				result = true;
			}
			catch (OracleException e)
			{
				throw(new TBException(e.Message, e));
			}
			finally
			{
				command.Dispose();
			}

			return result;
		}
		#endregion

		#endregion

		#region Funzioni per i Sinonimi (Creazione / Cancellazione)

		#region CreateSynonym - Crea Sinonimo
		/// <summary>
		/// CreateSynonym
		/// </summary>
		//---------------------------------------------------------------------
		public bool CreateSynonyms(string ownerName, string userName)
		{
			TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(CurrentConnection);
			
			try
			{
				oraAdminFunct.CreateMissingSynonyms(ownerName, userName);
				return true;
			}
			catch(TBException tbExc)
			{
				Debug.Fail(tbExc.ExtendedMessage);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Function, tbExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Description, tbExc.ExtendedMessage);
				diagnostic.Set(DiagnosticType.Error, tbExc.Message, extendedInfo);
				return false;
			}
		}
		#endregion
		
		#region DropSynonym - Elimina un sinonimo
		/// <summary>
		/// DropSynonym
		/// </summary>
		//---------------------------------------------------------------------
		public bool DropSynonym(string ownerName, string userName)
		{
			TBOracleAdminFunction oraAdminFunct = new TBOracleAdminFunction(CurrentConnection);

			try
			{
				return oraAdminFunct.DropSynonyms(ownerName, userName);
			}
			catch(TBException tbExc)
			{
				Debug.Fail(tbExc.Message);
				diagnostic.Set(DiagnosticType.Error,tbExc.Message);
				return false;
			}
		}
		#endregion

		#endregion

		#region Funzioni per lo Schema (Cancellazioni)
		/// <summary>
		/// DeleteSchema
		/// Cancella uno schema dal db oracle
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteSchema(string ownerName)
		{
			bool deletedSchema = false;
			OracleCommand command = null; 

			try				
			{
				string query = string.Format(OracleDBSchemaStrings.DropUser, ownerName.ToUpperInvariant());
				command = new OracleCommand(query, CurrentConnection);
				command.ExecuteNonQuery();
				deletedSchema = true;
			}
			catch (OracleException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteSchema");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.OracleDataAccess");
				diagnostic.Set(DiagnosticType.Error, string.Format(OracleDataAccessStrings.ErrorDeletingUser, ownerName), extendedInfo);
			}
			finally
			{
				command.Dispose();
			}

			return deletedSchema;
		}
		#endregion

		#region Impersonificazione Utente con privilegi di amministrazione
		/// <summary>
		/// AdminImpersonification
		/// </summary>
		/// <param name="candidateAdmin"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public OracleUserImpersonatedData AdminImpersonification(OracleUserImpersonatedData candidateAdmin)
		{
			string currentUser = string.Empty;
			//forzo l'utente proposto ad essere un Dba
			candidateAdmin.IsDba = true;
			
			currentUser = (candidateAdmin.WindowsAuthentication)
				? candidateAdmin.Domain + Path.DirectorySeparatorChar + candidateAdmin.Login 
				: candidateAdmin.Login;

			if (OnIsUserAuthenticatedFromConsole != null)
			{
				if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateAdmin.Password, candidateAdmin.OracleService))
				{
					//è la prima volta che l'Admin si logga
					OracleCredential loginData = new OracleCredential(candidateAdmin, Role.Administrator);
					loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
					loginData.ShowDialog();

					if (loginData.Success)
					{
						if (OnAddUserAuthenticatedFromConsole != null)
						{
							//Aggiungo l'utente alla collection degli utenti loggati
							if (loginData.oracleUserImpersonated.WindowsAuthentication)
								OnAddUserAuthenticatedFromConsole
									(
									loginData.oracleUserImpersonated.Domain + Path.DirectorySeparatorChar + loginData.oracleUserImpersonated.Login, 
									loginData.oracleUserImpersonated.Password,
									loginData.oracleUserImpersonated.OracleService,
									DBMSType.ORACLE
									);
							else
								OnAddUserAuthenticatedFromConsole
									(
									loginData.oracleUserImpersonated.Login, 
									loginData.oracleUserImpersonated.Password,
									loginData.oracleUserImpersonated.OracleService,
									DBMSType.ORACLE
									);
						}
						return loginData.oracleUserImpersonated;
					}
					return null;
				}
				else
				{
					if (OnGetUserAuthenticatedPwdFromConsole != null)
					{
						//recupero la pwd dell'utente precedentemente loggato
						candidateAdmin.Password	= OnGetUserAuthenticatedPwdFromConsole(candidateAdmin.Login, candidateAdmin.OracleService);
						OracleCredential loginData = new OracleCredential(candidateAdmin);
						loginData.OnOpenHelpFromPopUp  += new OracleCredential.OpenHelpFromPopUp(SendHelp);
						//non mostro la popUp ma tento la connessione
						if (loginData.Success)
							return loginData.oracleUserImpersonated;
						else
							return null;
					}
					else
					{
						//non posso contattare la collection degli utenti loggati
						//mostro la popUp delle credenziali per chiedere la pwd
						OracleCredential loginData = new OracleCredential(candidateAdmin, Role.Administrator);
						loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();
						if (loginData.Success)
							return loginData.oracleUserImpersonated;
						else
							return null;
					}
				}
			}
			return null;	
		}
		#endregion		
		
		#region Impersonificazione Utente Senza privilegi di amministrazione
		/// <summary>
		/// UserImpersonificationWithGui
		/// Impersonificazione con interfaccia (se necessario)
		/// </summary>
		//---------------------------------------------------------------------
		private OracleUserImpersonatedData UserImpersonificationWithGui(OracleUserImpersonatedData candidateUser, bool enabledUserChange)
		{
			string currentUser = string.Empty;

			if (candidateUser.WindowsAuthentication)
			{
				if (candidateUser.Login.Split(Path.DirectorySeparatorChar).Length == 1)
					currentUser = candidateUser.Domain + Path.DirectorySeparatorChar + candidateUser.Login;
				else
					currentUser = candidateUser.Login;
			}
			else
				currentUser = candidateUser.Login;

			candidateUser.IsCurrentUser = 
				string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, StringComparison.InvariantCultureIgnoreCase) == 0;

			OracleCredential loginData = null;
			//sono in sicurezza integrata ma tento di connettermi con un utete NT <> CurrentUserNT,
			//devo effetturare l'impersonificazione
			if (!candidateUser.IsCurrentUser && candidateUser.WindowsAuthentication)
			{
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					//se l'utente non è autenticato in Console
					if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
					{
						loginData = new OracleCredential(candidateUser, Role.User, enabledUserChange);
						loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
						loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();
						//se ho avuto successo e l'utente non c'è, lo aggiungo alla collection degli utenti loggati
						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
							{
								if (loginData.oracleUserImpersonated.WindowsAuthentication)
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Domain + Path.DirectorySeparatorChar + loginData.oracleUserImpersonated.Login,
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
								else
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Login,
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
							}

							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return loginData.oracleUserImpersonated;
						}
						else
						{
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return null;
						}
					}
					else
					{
						//leggo la sua pwd già memorizzata nella collection degli utenti connessi
						if (OnGetUserAuthenticatedPwdFromConsole != null)
						{
							string pwd = OnGetUserAuthenticatedPwdFromConsole(candidateUser.Login, candidateUser.OracleService);
							loginData = new OracleCredential(candidateUser);
							loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
							loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);

							//se ho avuto successo ritorno i dati dell'utente
							if (loginData.Success)
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return loginData.oracleUserImpersonated;
							}
							else
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return null;
							}
						}
						else
						{
							//non sono in grado di contattare la collection degli utenti loggati della console
							//mostro la popUp per richiedere la pwd
							loginData = new OracleCredential(candidateUser, Role.User, enabledUserChange);
							loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
							loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
							loginData.ShowDialog();

							if (loginData.Success)
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return loginData.oracleUserImpersonated;
							}
							else
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return null;
							}
						}
					}
				}
				else
				{
					loginData = new OracleCredential(candidateUser, Role.User, enabledUserChange);
					loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
					loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
					loginData.ShowDialog();

					if (loginData.Success)
					{
						if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
							diagnostic.Set(loginData.diagnostic);
						return loginData.oracleUserImpersonated;
					}
					else
					{
						if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
							diagnostic.Set(loginData.diagnostic);
						return null;
					}
				}

			}
			//mi sto loggando con l'utente NT corrente .. non ho necessità di impersonificare nè di chiedere la pwd (che quindi è blank)
			else if (candidateUser.IsCurrentUser)
			{
				OracleUserImpersonatedData oracleUserImpersonated	= new OracleUserImpersonatedData();
				oracleUserImpersonated.UserAfterImpersonate			= System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
				oracleUserImpersonated.Login						= candidateUser.Login;
				oracleUserImpersonated.OracleService				= candidateUser.OracleService;
				oracleUserImpersonated.Password						= candidateUser.Password;
				oracleUserImpersonated.Domain						= candidateUser.Domain;
				oracleUserImpersonated.WindowsAuthentication		= candidateUser.WindowsAuthentication;
				oracleUserImpersonated.IsCurrentUser				= candidateUser.IsCurrentUser;
				loginData = new OracleCredential(oracleUserImpersonated);
				loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);

				if (loginData.Success)
				{
					//se non esiste ancora nella collection lo aggiungo
					if (OnIsUserAuthenticatedFromConsole != null)
					{
						if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
						{
							if (OnAddUserAuthenticatedFromConsole != null)
								OnAddUserAuthenticatedFromConsole
									(
									currentUser,
									candidateUser.Password,
									candidateUser.OracleService,
									DBMSType.ORACLE
									);
						}
					}
					if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
						diagnostic.Set(loginData.diagnostic);
					return oracleUserImpersonated;
				}
				else
				{
					if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
						diagnostic.Set(loginData.diagnostic);
					return null;
				}
			}
			else
			{
				//non sono in sicurezza integrata, mi sto loggando con un utente Oracle
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
					{
						//è la prima volta che l'utente si connette
						loginData = new OracleCredential(candidateUser, Role.User, enabledUserChange);
						loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
						loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
						loginData.ShowDialog();

						if (loginData.Success)
						{
							//eventualmente lo aggiungo alla collection degli utenti loggati
							if (OnAddUserAuthenticatedFromConsole != null)
							{
								if (loginData.oracleUserImpersonated.WindowsAuthentication)
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Domain + Path.DirectorySeparatorChar + loginData.oracleUserImpersonated.Login,
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
								else
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Login,
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
							}
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return loginData.oracleUserImpersonated;
						}
						else
						{
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return null;
						}
					}
					else
					{
						//l'utente si è già loggato, devo leggere la pwd dalla collection degli utenti in Console
						OracleUserImpersonatedData oracleUserImpersonated	= new OracleUserImpersonatedData();

						if (OnGetUserAuthenticatedPwdFromConsole != null)
						{
							candidateUser.Password = OnGetUserAuthenticatedPwdFromConsole(candidateUser.Login, candidateUser.OracleService);
							oracleUserImpersonated.UserAfterImpersonate	= System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
							oracleUserImpersonated.Login				= candidateUser.Login;
							oracleUserImpersonated.OracleService		= candidateUser.OracleService;
							oracleUserImpersonated.Password				= candidateUser.Password;
							oracleUserImpersonated.Domain				= candidateUser.Domain;
							oracleUserImpersonated.WindowsAuthentication= candidateUser.WindowsAuthentication;
							loginData = new OracleCredential(oracleUserImpersonated);
							loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
						}
						else
						{
							//chiedo la pwd perchè non posso controllarla (evento OnGetUserAuthenticatedPwdFromConsole non agganciato)
							loginData = new OracleCredential(candidateUser, Role.User, enabledUserChange);
							loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
							loginData.OnOpenHelpFromPopUp += new OracleCredential.OpenHelpFromPopUp(SendHelp);
							loginData.ShowDialog();
						}

						if (loginData.Success)
						{
							if (OnIsUserAuthenticatedFromConsole != null)
							{
								if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
								{
									if (OnAddUserAuthenticatedFromConsole != null)
										OnAddUserAuthenticatedFromConsole
											(
											currentUser,
											candidateUser.Password,
											candidateUser.OracleService,
											DBMSType.ORACLE
											);
								}
							}
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
						}

						oracleUserImpersonated.Territory	= loginData.oracleUserImpersonated.Territory;
						oracleUserImpersonated.Language		= loginData.oracleUserImpersonated.Language;
						return oracleUserImpersonated;
					}
				}
				else
					return null;
			}
		}

		/// <summary>
		/// UserImpersonificationSilent
		/// Impersonificazione silente (senza popUp per la richiesta delle credenziali)
		/// </summary>
		//---------------------------------------------------------------------
		private OracleUserImpersonatedData UserImpersonificationSilent(OracleUserImpersonatedData candidateUser)
		{
			string currentUser = string.Empty;
			if (candidateUser.WindowsAuthentication)
			{
				if (candidateUser.Login.Split(Path.DirectorySeparatorChar).Length == 1)
					currentUser = candidateUser.Domain + Path.DirectorySeparatorChar + candidateUser.Login;
				else
					currentUser = candidateUser.Login;
			}
			else
				currentUser = candidateUser.Login;

			candidateUser.IsCurrentUser = 
				string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, StringComparison.InvariantCultureIgnoreCase) == 0;

			OracleCredential loginData = new OracleCredential(candidateUser);
			loginData.OnSendDiagnostic += new OracleCredential.SendDiagnostic(ReceiveDiagnostic);
			if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
				diagnostic.Set(loginData.diagnostic);

			if (loginData.Success)
				return loginData.oracleUserImpersonated;
			else
				return null;
		}

		/// <summary>
		/// UserImpersonification
		/// Se askCredential=false non chiede le credenziali
		/// altrimenti le chiede: se enabledUserChange=true sarà possibile,
		/// dalla popUp, selezionare un utente differente, altrimenti no.
		/// </summary>
		//---------------------------------------------------------------------
		public OracleUserImpersonatedData UserImpersonification(OracleUserImpersonatedData candidateUser, bool askCredential, bool enabledUserChange)
		{
			if (!askCredential)
				return UserImpersonificationSilent(candidateUser);
			else
				return UserImpersonificationWithGui(candidateUser, enabledUserChange);
		}

		/// <summary>
		/// UserImpersonification
		/// se askCredential=true appare la popUp per la richiesta delle credenziali
		/// Abilitata la possibilità di logginarsi con un utente diverso da quello proposto
		/// </summary>
		/// <param name="candidateUser"></param>
		/// <param name="askCredential"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public OracleUserImpersonatedData UserImpersonification(OracleUserImpersonatedData candidateUser, bool askCredential)
		{
			return UserImpersonification(candidateUser, askCredential, true);
		}

		/// <summary>
		/// UserImpersonification (silente)
		/// Non chiede le credenziali, tenta di connetteresi con l'utente candidateUser
		/// </summary>
		/// <param name="candidateUser"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public OracleUserImpersonatedData UserImpersonification(OracleUserImpersonatedData candidateUser)
		{
			return UserImpersonification(candidateUser, false, false);
		}
		#endregion

		#region Funzioni di Help e Diagnostica
		/// <summary>
		/// SendHelp
		/// Premuto il bottone Aiuto della PopUp delle credenziali
		/// </summary>
		//---------------------------------------------------------------------
		private void SendHelp(object sender, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, NameSpace, searchParameter);
		}

		/// <summary>
		/// ReceiveDiagnostic
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="diagnosticElements"></param>
		//---------------------------------------------------------------------
		private void ReceiveDiagnostic(object sender, Diagnostic diagnosticElements)
		{
			if (diagnosticElements.Error || diagnosticElements.Warning || diagnosticElements.Information)
				diagnostic.Set(diagnosticElements);
			diagnosticElements.Clear();
		}
		#endregion

		#region Funzioni per Impersonificazione commentate
		/* LE HO FATTO SPECULARI ALLA PARTE DI IMPERSONIFICAZIONE UTENTE MA PER IL MOMENTO SEMBRANO NON SERVIRE */
		/*
		//---------------------------------------------------------------------
		private OracleUserImpersonatedData AdminImpersonificationWithGui(OracleUserImpersonatedData candidateUser, bool enabledUserChange)
		{
			string currentUser = string.Empty;
			if (candidateUser.WindowsAuthentication)
			{
				if (candidateUser.Login.Split(Path.DirectorySeparatorChar).Length == 1)
					currentUser = candidateUser.Domain + Path.DirectorySeparatorChar + candidateUser.Login;
				else
					currentUser = candidateUser.Login;
			}
			else
				currentUser = candidateUser.Login;
			if (string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, true) ==0)
				candidateUser.IsCurrentUser = true;
			else
				candidateUser.IsCurrentUser = false;
			candidateUser.IsDba = true;
			Credential loginData = null;
			if (!candidateUser.IsCurrentUser && candidateUser.WindowsAuthentication)
			{
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					//se l'utente non è autenticato in Console
					if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
					{
						loginData = new Credential(candidateUser, Role.Administrator, enabledUserChange);
						loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
						loginData.OnOpenHelpFromPopUp += new Credential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();
						//se ho avuto successo aggiungo l'utente alla collection degli utenti loggati
						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
							{
								if (loginData.oracleUserImpersonated.WindowsAuthentication)
								{
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Domain + Path.DirectorySeparatorChar + loginData.oracleUserImpersonated.Login, 
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
								}
								else
								{
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Login, 
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
								}
							}
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return loginData.oracleUserImpersonated;
						}
						else
						{
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return null;
						}
					}
					else
					{
						if (OnGetUserAuthenticatedPwdFromConsole != null)
						{
							string pwd = OnGetUserAuthenticatedPwdFromConsole(candidateUser.Login, candidateUser.OracleService);
							loginData = new Credential(candidateUser);
							loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
							loginData.OnOpenHelpFromPopUp += new Credential.OpenHelpFromPopUp(SendHelp);
							if (loginData.Success)
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return loginData.oracleUserImpersonated;
							}
							else
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return null;
							}

						}
						else
						{
							loginData = new Credential(candidateUser, Role.Administrator, enabledUserChange);
							loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
							loginData.OnOpenHelpFromPopUp += new Credential.OpenHelpFromPopUp(SendHelp);
							loginData.ShowDialog();
					
							if (loginData.Success)
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return loginData.oracleUserImpersonated;
							}
							else
							{
								if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
									diagnostic.Set(loginData.diagnostic);
								return null;
							}
						}
					}
				}
				else
				{
					loginData = new Credential(candidateUser, Role.Administrator, enabledUserChange);
					loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
					loginData.OnOpenHelpFromPopUp += new Credential.OpenHelpFromPopUp(SendHelp);
					loginData.ShowDialog();
					
					if (loginData.Success)
					{
						if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
							diagnostic.Set(loginData.diagnostic);
						return loginData.oracleUserImpersonated;
					}
					else
					{
						if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
							diagnostic.Set(loginData.diagnostic);
						return null;
					}
				}
				
			}
			else if (candidateUser.IsCurrentUser)
			{
				OracleUserImpersonatedData oracleUserImpersonated	= new OracleUserImpersonatedData();
				oracleUserImpersonated.UserAfterImpersonate			= System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
				oracleUserImpersonated.Login						= candidateUser.Login;
				oracleUserImpersonated.OracleService				= candidateUser.OracleService;
				oracleUserImpersonated.Password						= candidateUser.Password;
				oracleUserImpersonated.Domain						= candidateUser.Domain;
				oracleUserImpersonated.WindowsAuthentication		= candidateUser.WindowsAuthentication;
				oracleUserImpersonated.IsCurrentUser				= candidateUser.IsCurrentUser;
				oracleUserImpersonated.IsDba						= true;
				loginData = new Credential(oracleUserImpersonated);
				loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
				
				if (loginData.Success)
				{
					if (OnIsUserAuthenticatedFromConsole != null)
					{
						if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
						{
							if (OnAddUserAuthenticatedFromConsole != null)
							{
								OnAddUserAuthenticatedFromConsole
									(
									currentUser, 
									candidateUser.Password,
									candidateUser.OracleService,
									DBMSType.ORACLE
									);
							}
						}
					}
					if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
						diagnostic.Set(loginData.diagnostic);
					return oracleUserImpersonated;	
				}
				else
				{
					if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
						diagnostic.Set(loginData.diagnostic);
					return null;
				}
			}
			else
			{
				//non sono in sicurezza integrata 
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
					{
						loginData = new Credential(candidateUser, Role.Administrator, enabledUserChange);
						loginData.OnOpenHelpFromPopUp += new Credential.OpenHelpFromPopUp(SendHelp);
						loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
						loginData.ShowDialog();
						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
							{
								if (loginData.oracleUserImpersonated.WindowsAuthentication)
								{
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Domain + Path.DirectorySeparatorChar + loginData.oracleUserImpersonated.Login, 
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
								}
								else
								{
									OnAddUserAuthenticatedFromConsole
										(
										loginData.oracleUserImpersonated.Login, 
										loginData.oracleUserImpersonated.Password,
										loginData.oracleUserImpersonated.OracleService,
										DBMSType.ORACLE
										);
								}
							}
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return loginData.oracleUserImpersonated;
						}
						else
						{
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
							return null;
						}
					}
					else
					{
						//sono l'utente corrente non devo neppure testare la connessione
						OracleUserImpersonatedData oracleUserImpersonated	= new OracleUserImpersonatedData();
						oracleUserImpersonated.UserAfterImpersonate			= System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
						oracleUserImpersonated.Login						= candidateUser.Login;
						oracleUserImpersonated.OracleService				= candidateUser.OracleService;
						oracleUserImpersonated.Password						= candidateUser.Password;
						oracleUserImpersonated.Domain						= candidateUser.Domain;
						oracleUserImpersonated.WindowsAuthentication		= candidateUser.WindowsAuthentication;
						oracleUserImpersonated.IsDba						= true;
						loginData = new Credential(oracleUserImpersonated);
						loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
				
						if (loginData.Success)
						{
							if (OnIsUserAuthenticatedFromConsole != null)
							{
								if (!OnIsUserAuthenticatedFromConsole(currentUser, candidateUser.Password, candidateUser.OracleService))
								{
									if (OnAddUserAuthenticatedFromConsole != null)
									{
										OnAddUserAuthenticatedFromConsole
											(
											currentUser, 
											candidateUser.Password,
											candidateUser.OracleService,
											DBMSType.ORACLE
											);
									}
								}
							}
							if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
								diagnostic.Set(loginData.diagnostic);
						}
						return oracleUserImpersonated;	
					}
				}
				else
					return null;
			}
		}

		//---------------------------------------------------------------------
		private OracleUserImpersonatedData AdminImpersonificationSilent(OracleUserImpersonatedData candidateUser)
		{
			string currentUser = string.Empty;
			
			if (candidateUser.WindowsAuthentication)
			{
				if (candidateUser.Login.Split(Path.DirectorySeparatorChar).Length == 1)
					currentUser = candidateUser.Domain + Path.DirectorySeparatorChar + candidateUser.Login;
				else
					currentUser = candidateUser.Login;
			}
			else
				currentUser = candidateUser.Login;

			candidateUser.IsDba = true;

			if (string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, true) ==0)
				candidateUser.IsCurrentUser = true;
			else
				candidateUser.IsCurrentUser = false;

			
			Credential loginData = new Credential(candidateUser);
			loginData.OnSendDiagnostic += new Credential.SendDiagnostic(ReceiveDiagnostic);
			if (loginData.diagnostic.Error || loginData.diagnostic.Warning || loginData.diagnostic.Information)
				diagnostic.Set(loginData.diagnostic);
			if (loginData.Success)
			{
				return loginData.oracleUserImpersonated;
			}
			else
				return null;
		}

		//---------------------------------------------------------------------
		public OracleUserImpersonatedData AdminImpersonification(OracleUserImpersonatedData candidateUser, bool askCredential, bool enabledUserChange)
		{
			if (!askCredential)
				return AdminImpersonificationSilent(candidateUser);
			else
				return AdminImpersonificationWithGui(candidateUser, enabledUserChange);
		}

		//---------------------------------------------------------------------
		public OracleUserImpersonatedData AdminImpersonification(OracleUserImpersonatedData candidateUser, bool askCredential)
		{
			return AdminImpersonification(candidateUser, askCredential, true);
		}

		//---------------------------------------------------------------------
		public OracleUserImpersonatedData AdminImpersonification(OracleUserImpersonatedData candidateUser)
		{
			return AdminImpersonification(candidateUser, false, false);
		}
		*/
		#endregion
	}
}

#pragma warning restore 0618