using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects
{
	/// <summary>
	/// Summary description for SecurityManager.
	/// </summary>
	//================================================================================
	public class SecurityLightManager : IDisposable
	{
		public enum ChildrenProtectionType
		{
			Undefined = 0x0000,
			None = 0x0001,	// Non esiste nessun divieto di accesso
			Partial = 0x0002,	// Esiste almeno un divieto di accesso
			All = 0x0003	// È tutto vietato
		};

		private SqlConnection systemDBConnection = null;
		private SecurityLightMenuLoader menuLoader = null;
		private string[] userNamesToSkip = null;


		private static SqlCommand getAllCompaniesCommand = null;
		private static SqlCommand getUserCompaniesCommand = null;
		private static string UserIdSqlCommandParameterName = "@UserId";

		private bool ownConnection = false;

		//---------------------------------------------------------------------
		public SecurityLightManager(SecurityLightMenuLoader aMenuLoader, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
				Debug.Fail("Invalid connection argument passed to SecurityManager constructor");

			systemDBConnection = aConnection;

			menuLoader = aMenuLoader;
			userNamesToSkip = aUserNamesToSkipList;
		}

		//---------------------------------------------------------------------
		public SecurityLightManager(SecurityLightMenuLoader aMenuLoader, string sConnectionString, string[] aUserNamesToSkipList)
		{
			systemDBConnection = new SqlConnection(sConnectionString);
			systemDBConnection.Open();

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
				Debug.Fail("Invalid connection argument passed to SecurityManager constructor");

			ownConnection = true;

			menuLoader = aMenuLoader;
			userNamesToSkip = aUserNamesToSkipList;
		}

		//--------------------------------------------------------------------------------
		~SecurityLightManager()
		{
			Dispose();
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			Close();
		}

		//--------------------------------------------------------------------------------
		public void Close()
		{
			if (systemDBConnection != null && ownConnection)
			{
				try
				{
					if ((systemDBConnection.State & ConnectionState.Open) == ConnectionState.Open)
						systemDBConnection.Close();
					systemDBConnection.Dispose();
				}
				catch
				{
				}
				finally
				{
					systemDBConnection = null;
				}
			}
		}

		#region SecurityManager private methods

		//---------------------------------------------------------------------
		private bool ExistDeniedMenuCommand(MenuXmlNode aMenuNode, int aCompanyId, int aUserId)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu)
				throw new ArgumentException("Invalid menu node passed as argument to the SecurityManager.ExistDeniedMenuCommand method.");

			ArrayList commandChildren = aMenuNode.CommandItems;
			if (commandChildren == null || commandChildren.Count == 0)
				return false;

			foreach (MenuXmlNode aCommandNode in commandChildren)
			{
				if (aCommandNode == null)
					continue;

				SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
				if (commandType == SecuredCommandType.Undefined)
					continue;

				if (SecuredCommand.IsAccessToObjectDenied(aCommandNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------
		private bool ExistAllowedMenuCommand(MenuXmlNode aMenuNode, int aCompanyId, int aUserId)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu)
				throw new ArgumentException("Invalid menu node passed as argument to the SecurityManager.ExistAllowedMenuCommand method.");

			ArrayList commandChildren = aMenuNode.CommandItems;
			if (commandChildren == null || commandChildren.Count == 0)
				return false;

			foreach (MenuXmlNode aCommandNode in commandChildren)
			{
				if (aCommandNode == null)
					continue;

				SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
				if (commandType == SecuredCommandType.Undefined)
					continue;

				if (!SecuredCommand.IsAccessToObjectDenied(aCommandNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection))
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------
		private bool AreAllMenuCommandsDenied(MenuXmlNode aMenuNode, int aCompanyId, int aUserId)
		{
			if (aMenuNode == null || !aMenuNode.IsMenu)
				throw new ArgumentException("Invalid menu node passed as argument to the SecurityManager.AreAllMenuCommandsDenied method.");

			if (!aMenuNode.HasCommandChildNodes())
				return false;

			return !ExistAllowedMenuCommand(aMenuNode, aCompanyId, aUserId);
		}

		//-----------------------------------------------------------------------
		private static bool ExistTable(string aTableName, SqlConnection aConnection)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.ExistSqlTable.");
				return false;
			}

			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = new SqlCommand("SELECT COUNT(*) FROM sysobjects WHERE id = object_id(N'" + aTableName + "') AND OBJECTPROPERTY(id, N'IsUserTable') = 1", aConnection);
				return (int)sqlCommand.ExecuteScalar() > 0;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.ExistSqlTable: " + e.Message);
				return false;
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private static string GetSecurityLightDenyMenuFileName
			(
			IBasePathFinder aPathFinder,
			string aCompanyName,
			string aUserName
			)
		{
			if (aPathFinder == null || aCompanyName == null || aCompanyName.Length == 0 || aUserName == null || aUserName.Length == 0)
				return String.Empty;

			string denyMenuFileName = aPathFinder.GetCustomCompanyPath(aCompanyName);
			if (denyMenuFileName == null || denyMenuFileName.Length == 0)
				return String.Empty;

			if (denyMenuFileName[denyMenuFileName.Length - 1] != Path.DirectorySeparatorChar)
				denyMenuFileName += Path.DirectorySeparatorChar;

			denyMenuFileName += (SLDenyFile.SecurityLightCustomMenuFolderName + Path.DirectorySeparatorChar);
			denyMenuFileName += (NameSolverStrings.Menu + Path.DirectorySeparatorChar);
			denyMenuFileName += (aUserName + Path.DirectorySeparatorChar);
			denyMenuFileName += SLDenyFile.SecurityLightDenyMenuFileName;
			denyMenuFileName += NameSolverStrings.MenuExtension;

			return denyMenuFileName;
		}

		//---------------------------------------------------------------------
		private static string[] GetAllCustomPathNames
			(
			IBasePathFinder aPathFinder,
			int aCompanyId,
			int aUserId,
			SqlConnection aConnection,
			string[] aUserNamesToSkipList
			)
		{
			if (aPathFinder == null)
				return null;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetAllCustomPathNames.");
				return null;
			}

			ArrayList customCompanyUserPaths = new ArrayList();

			if (aCompanyId != -1)
			{
				string companyName = GetCompanyName(aCompanyId, aConnection);
				string customCompanyPath = (companyName != null && companyName.Length > 0) ? aPathFinder.GetCustomCompanyPath(companyName) : String.Empty;
				if (customCompanyPath == null)
					return null;
				customCompanyPath = customCompanyPath.Trim();
				if (customCompanyPath.Length == 0)
					return null;

				if (customCompanyPath[customCompanyPath.Length - 1] != Path.DirectorySeparatorChar)
					customCompanyPath += Path.DirectorySeparatorChar;
				customCompanyPath += (SLDenyFile.SecurityLightCustomMenuFolderName + Path.DirectorySeparatorChar + NameSolverStrings.Menu + Path.DirectorySeparatorChar);
				if (aUserId != -1)
				{
					string userName = SecurityLightManager.GetUserName(aUserId, aConnection);
					if (userName == null || userName.Length == 0)
						return null;

					return new string[] { customCompanyPath + userName };
				}

				customCompanyUserPaths.Add(customCompanyPath + NameSolverStrings.AllUsers);
				string[] companyUserNames = SecurityLightManager.GetCompanyUserNames(aCompanyId, aConnection);
				if (companyUserNames != null && companyUserNames.Length > 0)
				{
					foreach (string aCompanyUserName in companyUserNames)
					{
						if (aUserNamesToSkipList != null && aUserNamesToSkipList.Length > 0)
						{
							bool skipUser = false;
							foreach (string aUserNameToSkip in aUserNamesToSkipList)
							{
								if (aUserNameToSkip != null && aUserNameToSkip.Length > 0 && String.Compare(aCompanyUserName, aUserNameToSkip) == 0)
								{
									skipUser = true;
									break;
								}
							}
							if (skipUser)
								continue;
						}

						customCompanyUserPaths.Add(customCompanyPath + aCompanyUserName);
					}
				}
			}
			else
			{
				int[] companyIds = GetAllCompanyIds(aConnection);
				if (companyIds == null || companyIds.Length == 0)
					return null;

				foreach (int companyId in companyIds)
				{
					string[] customPathNames = GetAllCustomPathNames(aPathFinder, companyId, aUserId, aConnection, aUserNamesToSkipList);
					if (customPathNames == null || customPathNames.Length == 0)
						continue;
					customCompanyUserPaths.AddRange(customPathNames);
				}
			}

			return (customCompanyUserPaths.Count > 0) ? (string[])customCompanyUserPaths.ToArray(typeof(string)) : null;
		}

		#endregion // SecurityManager private methods

		#region SecurityManager public methods

		//---------------------------------------------------------------------
		public bool DenyAccessToMenuCommand(MenuXmlNode aCommandNode, int aCompanyId, int aUserId)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Cannot deny access to the command object '" + aCommandNode.ItemObject + "': Invalid connection.");
				return false;
			}

			SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
			if (commandType == SecuredCommandType.Undefined)
				return false;

			SecuredCommand securedCommand = new SecuredCommand(menuLoader, aCommandNode, systemDBConnection, userNamesToSkip);

			bool commandAccessDenied = securedCommand.DenyAccess(aCompanyId, aUserId);

			aCommandNode.AccessDeniedState = commandAccessDenied;
			bool setAccessInUnattendedModeAllowedState = false;
			if (
				commandAccessDenied &&
				!securedCommand.IsAccessInUnattendedModeDefined(aCompanyId, aUserId) &&
				securedCommand.SetAccessInUnattendedMode(aCompanyId, aUserId, true))
			{
				aCommandNode.AccessInUnattendedModeAllowedState = true;
				setAccessInUnattendedModeAllowedState = true;
			}

			// È necessario controllare se nel menù caricato esistano anche altre voci riferite allo  
			// stesso comando, cioè che puntino al medesimo namespace, ed impostare automaticamente 
			// lo stato anche di quest’ultime
			if (menuLoader != null && menuLoader.CurrentMenuParser != null)
			{
				ArrayList menuCommandsToDeny = menuLoader.CurrentMenuParser.GetEquivalentCommandsList(aCommandNode);
				if (menuCommandsToDeny != null && menuCommandsToDeny.Count > 0)
				{
					foreach (MenuXmlNode aCommandToDenyNode in menuCommandsToDeny)
					{
						aCommandToDenyNode.AccessDeniedState = commandAccessDenied;
						if (setAccessInUnattendedModeAllowedState)
							aCommandToDenyNode.AccessInUnattendedModeAllowedState = true;
					}
				}
			}

			return commandAccessDenied;
		}

		//---------------------------------------------------------------------
		public bool AllowAccessToMenuCommand(MenuXmlNode aCommandNode, int aCompanyId, int aUserId)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Cannot allow access to the command object '" + aCommandNode.ItemObject + "': Invalid connection.");
				return false;
			}

			SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
			if (commandType == SecuredCommandType.Undefined)
				return false;

			SecuredCommand securedCommand = new SecuredCommand(menuLoader, aCommandNode, systemDBConnection, userNamesToSkip);

			bool commandAccessAllowed = securedCommand.AllowAccess(aCompanyId, aUserId);

			aCommandNode.AccessAllowedState = commandAccessAllowed;

			// È necessario controllare se nel menù caricato esistano anche altre voci riferite allo  
			// stesso comando, cioè che puntino al medesimo namespace, ed impostare automaticamente 
			// lo stato anche di quest’ultime
			if (menuLoader != null && menuLoader.CurrentMenuParser != null)
			{
				ArrayList menuCommandsToAllow = menuLoader.CurrentMenuParser.GetEquivalentCommandsList(aCommandNode);
				if (menuCommandsToAllow != null && menuCommandsToAllow.Count > 0)
				{
					foreach (MenuXmlNode aCommandToAllowNode in menuCommandsToAllow)
						aCommandToAllowNode.AccessAllowedState = commandAccessAllowed;
				}
			}

			return commandAccessAllowed;
		}

		//---------------------------------------------------------------------
		public bool SetAccessToMenuCommandInUnattendedMode(MenuXmlNode aCommandNode, int aCompanyId, int aUserId, bool allowed)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Cannot allow access to the command object '" + aCommandNode.ItemObject + "': Invalid connection.");
				return false;
			}

			SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
			if (commandType == SecuredCommandType.Undefined)
				return false;

			SecuredCommand securedCommand = new SecuredCommand(menuLoader, aCommandNode, systemDBConnection, userNamesToSkip);

			if (!securedCommand.SetAccessInUnattendedMode(aCompanyId, aUserId, allowed))
				return false;

			aCommandNode.AccessInUnattendedModeAllowedState = allowed;

			// È necessario controllare se nel menù caricato esistano anche altre voci riferite allo  
			// stesso comando, cioè che puntino al medesimo namespace, ed impostare automaticamente 
			// lo stato anche di quest’ultime
			if (menuLoader != null && menuLoader.CurrentMenuParser != null)
			{
				ArrayList menuCommandsToChange = menuLoader.CurrentMenuParser.GetEquivalentCommandsList(aCommandNode);
				if (menuCommandsToChange != null && menuCommandsToChange.Count > 0)
				{
					foreach (MenuXmlNode aCommandToChangeNode in menuCommandsToChange)
						aCommandToChangeNode.AccessAllowedState = allowed;
				}
			}

			return true;
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToObjectDenied(INameSpace aObjectNameSpace, int aCompanyId, int aUserId, SqlConnection aConnection)
		{
			return SecuredCommand.IsAccessToObjectNameSpaceDenied(aObjectNameSpace, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToObjectNameSpaceInUnattendedModeAllowed(INameSpace aObjectNameSpace, int aCompanyId, int aUserId, SqlConnection aConnection)
		{
			return SecuredCommand.IsAccessToObjectNameSpaceInUnattendedModeAllowed(aObjectNameSpace, aCompanyId, aUserId, aConnection);
		}

		//---------------------------------------------------------------------
		public static bool IsAccessToObjectNameSpaceDenied(INameSpace aObjectNameSpace, int aCompanyId, int aUserId, SqlConnection aConnection, bool unattendedMode)
		{
			bool isAccessDenied = IsAccessToObjectDenied(aObjectNameSpace, aCompanyId, aUserId, aConnection);
			if (isAccessDenied && unattendedMode)
			{
				isAccessDenied = !IsAccessToObjectNameSpaceInUnattendedModeAllowed(aObjectNameSpace, aCompanyId, aUserId, aConnection);
			}
			return isAccessDenied;
		}

		//---------------------------------------------------------------------
		public bool IsAccessToMenuCommandDenied(MenuXmlNode aCommandNode, int aCompanyId, int aUserId, bool checkCurrentNodeState)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			if (checkCurrentNodeState)
			{
				if (aCommandNode.AccessDeniedState)
					return true;

				if (aCommandNode.AccessAllowedState)
					return false;
			}

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Cannot check if the access to the command object '" + aCommandNode.ItemObject + "' is denied: Invalid connection.");
				return false;
			}

			SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
			if (commandType == SecuredCommandType.Undefined)
				return false;

			SecuredCommand securedCommand = new SecuredCommand(menuLoader, aCommandNode, systemDBConnection, userNamesToSkip);

			return securedCommand.IsAccessDenied(aCompanyId, aUserId);
		}

		//---------------------------------------------------------------------
		public bool IsAccessToMenuCommandDenied(MenuXmlNode aCommandNode, int aCompanyId, int aUserId)
		{
			return IsAccessToMenuCommandDenied(aCommandNode, aCompanyId, aUserId, true);
		}

		//---------------------------------------------------------------------
		public bool IsAccessToMenuCommandInUnattendedModeAllowed(MenuXmlNode aCommandNode, int aCompanyId, int aUserId, bool checkCurrentNodeState)
		{
			if (aCommandNode == null || !aCommandNode.IsCommand)
				return false;

			if (checkCurrentNodeState)
				return aCommandNode.AccessInUnattendedModeAllowedState;

			if (systemDBConnection == null || systemDBConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Cannot check if the access to the command object '" + aCommandNode.ItemObject + "' in unattended mode is allowed: Invalid connection.");
				return false;
			}

			SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
			if (commandType == SecuredCommandType.Undefined)
				return false;

			SecuredCommand securedCommand = new SecuredCommand(menuLoader, aCommandNode, systemDBConnection, userNamesToSkip);

			return securedCommand.IsAccessInUnattendedModeAllowed(aCompanyId, aUserId);
		}

		//---------------------------------------------------------------------
		public bool IsAccessToMenuCommandInUnattendedModeAllowed(MenuXmlNode aCommandNode, int aCompanyId, int aUserId)
		{
			return IsAccessToMenuCommandInUnattendedModeAllowed(aCommandNode, aCompanyId, aUserId, true);
		}

		//---------------------------------------------------------------------
		public ChildrenProtectionType GetChildrenProtectionType(MenuXmlNode aMenuNode, int aCompanyId, int aUserId)
		{
			if (aMenuNode == null || !(aMenuNode.IsApplication || aMenuNode.IsGroup || aMenuNode.IsMenu))
				return ChildrenProtectionType.Undefined;

			if (aMenuNode.IsGroup || aMenuNode.IsMenu)
			{
				ArrayList menuChildren = aMenuNode.MenuItems;
				ArrayList commandChildren = (aMenuNode.IsMenu) ? aMenuNode.CommandItems : null;

				if ((menuChildren == null || menuChildren.Count == 0) && (commandChildren == null || commandChildren.Count == 0))
					return ChildrenProtectionType.None;

				bool areAllChildMenusFullDenied = true;
				bool areAllChildMenusFullAllowed = true;

				if (menuChildren != null && menuChildren.Count > 0)
				{
					foreach (MenuXmlNode aChildMenuNode in menuChildren)
					{
						if (aChildMenuNode == null || (!aChildMenuNode.HasMenuChildNodes() && !aChildMenuNode.HasCommandChildNodes()))
							continue;

						ChildrenProtectionType childMenuProtectionType = GetChildrenProtectionType(aChildMenuNode, aCompanyId, aUserId);

						if (childMenuProtectionType == ChildrenProtectionType.Partial)
							return ChildrenProtectionType.Partial;

						areAllChildMenusFullDenied &= (childMenuProtectionType == ChildrenProtectionType.All);
						areAllChildMenusFullAllowed &= (childMenuProtectionType == ChildrenProtectionType.None);
					}
				}

				if (commandChildren == null || commandChildren.Count == 0)
				{
					if (areAllChildMenusFullDenied)
						return ChildrenProtectionType.All;

					if (areAllChildMenusFullAllowed)
						return ChildrenProtectionType.None;

					return ChildrenProtectionType.Partial;
				}

				bool areAllCommandsDenied = true;
				bool areAllCommandsAllowed = true;

				foreach (MenuXmlNode aCommandNode in commandChildren)
				{
					if (aCommandNode == null)
						continue;

					SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aCommandNode);
					if (commandType == SecuredCommandType.Undefined)
						continue;

					bool isCommandDenied = SecuredCommand.IsAccessToObjectDenied(aCommandNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection);

					areAllCommandsDenied &= isCommandDenied;
					areAllCommandsAllowed &= !isCommandDenied;
				}

				if (areAllCommandsDenied)
				{
					if (areAllChildMenusFullDenied)
						return ChildrenProtectionType.All;

					return ChildrenProtectionType.Partial;
				}

				if (areAllCommandsAllowed)
				{
					if (areAllChildMenusFullAllowed)
						return ChildrenProtectionType.None;

					return ChildrenProtectionType.Partial;
				}

				return ChildrenProtectionType.Partial;
			}

			if (aMenuNode.IsApplication)
			{
				bool areAllChildGroupsFullDenied = true;
				bool areAllChildGroupsFullAllowed = true;

				ArrayList groupChildren = aMenuNode.GroupItems;
				if (groupChildren == null || groupChildren.Count == 0)
					return ChildrenProtectionType.None;

				foreach (MenuXmlNode aChildGroupNode in groupChildren)
				{
					if (aChildGroupNode == null)
						continue;

					ChildrenProtectionType childGroupProtectionType = GetChildrenProtectionType(aChildGroupNode, aCompanyId, aUserId);

					if (childGroupProtectionType == ChildrenProtectionType.Partial)
						return ChildrenProtectionType.Partial;

					areAllChildGroupsFullDenied &= (childGroupProtectionType == ChildrenProtectionType.All);
					areAllChildGroupsFullAllowed &= (childGroupProtectionType == ChildrenProtectionType.None);
				}

				if (areAllChildGroupsFullDenied)
					return ChildrenProtectionType.All;

				if (areAllChildGroupsFullAllowed)
					return ChildrenProtectionType.None;

				return ChildrenProtectionType.Partial;
			}

			return ChildrenProtectionType.Undefined;
		}

		//--------------------------------------------------------------------------------------------------------
		public bool HasSameAccessRightsForBothCompanies(MenuXmlNode aMenuNode, int aUserId, int aCompanyId, int anotherCompanyId)
		{
			if (aMenuNode == null)
				return false;

			if (aMenuNode.IsCommand)
			{
				SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aMenuNode);
				if (commandType == SecuredCommandType.Undefined)
					return false;

				bool isCommandDenied = SecuredCommand.IsAccessToObjectDenied(aMenuNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection);

				return (isCommandDenied == SecuredCommand.IsAccessToObjectDenied(aMenuNode.ItemObject, commandType, anotherCompanyId, aUserId, systemDBConnection));
			}

			if (aMenuNode.IsApplication)
			{
				ArrayList groupChildren = aMenuNode.GroupItems;
				if (groupChildren != null && groupChildren.Count > 0)
				{
					foreach (MenuXmlNode aChildGroupNode in groupChildren)
					{
						if (aChildGroupNode == null)
							continue;

						if (!HasSameAccessRightsForBothCompanies(aChildGroupNode, aUserId, aCompanyId, anotherCompanyId))
							return false;
					}
				}
				return true;
			}

			if (aMenuNode.IsGroup || aMenuNode.IsMenu)
			{
				ArrayList menuChildren = aMenuNode.MenuItems;
				if (menuChildren != null && menuChildren.Count > 0)
				{
					foreach (MenuXmlNode aChildMenuNode in menuChildren)
					{
						if (aChildMenuNode == null)
							continue;

						if (!HasSameAccessRightsForBothCompanies(aChildMenuNode, aUserId, aCompanyId, anotherCompanyId))
							return false;
					}
				}

				ArrayList commandChildren = aMenuNode.CommandItems;
				if (commandChildren != null && commandChildren.Count > 0)
				{
					foreach (MenuXmlNode aCommandNode in commandChildren)
					{
						if (aCommandNode == null)
							continue;

						if (!HasSameAccessRightsForBothCompanies(aCommandNode, aUserId, aCompanyId, anotherCompanyId))
							return false;
					}
				}

				return true;
			}

			return false;
		}

		//--------------------------------------------------------------------------------------------------------
		public bool HasSameAccessRightsForBothUsers(MenuXmlNode aMenuNode, int aCompanyId, int aUserId, int anotherUserId)
		{
			if (aMenuNode == null)
				return false;

			if (aMenuNode.IsCommand)
			{
				SecuredCommandType commandType = SecuredCommand.GetSecuredCommandType(aMenuNode);
				if (commandType == SecuredCommandType.Undefined)
					return false;

				bool isCommandDenied = SecuredCommand.IsAccessToObjectDenied(aMenuNode.ItemObject, commandType, aCompanyId, aUserId, systemDBConnection);

				return (isCommandDenied == SecuredCommand.IsAccessToObjectDenied(aMenuNode.ItemObject, commandType, aCompanyId, anotherUserId, systemDBConnection));
			}

			if (aMenuNode.IsApplication)
			{
				ArrayList groupChildren = aMenuNode.GroupItems;
				if (groupChildren != null && groupChildren.Count > 0)
				{
					foreach (MenuXmlNode aChildGroupNode in groupChildren)
					{
						if (aChildGroupNode == null)
							continue;

						if (!HasSameAccessRightsForBothUsers(aChildGroupNode, aCompanyId, aUserId, anotherUserId))
							return false;
					}
				}
				return true;
			}

			if (aMenuNode.IsGroup || aMenuNode.IsMenu)
			{
				ArrayList menuChildren = aMenuNode.MenuItems;
				if (menuChildren != null && menuChildren.Count > 0)
				{
					foreach (MenuXmlNode aChildMenuNode in menuChildren)
					{
						if (aChildMenuNode == null)
							continue;

						if (!HasSameAccessRightsForBothUsers(aChildMenuNode, aCompanyId, aUserId, anotherUserId))
							return false;
					}
				}

				ArrayList commandChildren = aMenuNode.CommandItems;
				if (commandChildren != null && commandChildren.Count > 0)
				{
					foreach (MenuXmlNode aCommandNode in commandChildren)
					{
						if (aCommandNode == null)
							continue;

						if (!HasSameAccessRightsForBothUsers(aCommandNode, aCompanyId, aUserId, anotherUserId))
							return false;
					}
				}

				return true;
			}

			return false;
		}

		//--------------------------------------------------------------------------------------------------------
		public INameSpace[] GetAllCompanyInaccessibleObjects(int aCompanyId, SecuredCommandType aCommandType)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, aCommandType, systemDBConnection);
		}

		//---------------------------------------------------------------------
		public DirectoryInfo[] GetAllCustomPaths(int aCompanyId, int aUserId, bool checkExistence)
		{
			if (menuLoader == null || menuLoader.PathFinder == null)
				return null;

			return GetAllCustomPaths(menuLoader.PathFinder, aCompanyId, aUserId, systemDBConnection, userNamesToSkip, checkExistence);
		}

		//---------------------------------------------------------------------
		public DirectoryInfo[] GetAllCustomPaths(int aCompanyId, int aUserId)
		{
			return GetAllCustomPaths(aCompanyId, aUserId, false);
		}

		//---------------------------------------------------------------------
		public FileInfo[] GetAllExistingCustomMenuFiles(int aCompanyId, int aUserId)
		{
			if (menuLoader == null || menuLoader.PathFinder == null)
				return null;

			return GetAllExistingCustomMenuFiles(menuLoader.PathFinder, aCompanyId, aUserId, systemDBConnection, userNamesToSkip);
		}

		#region SecurityManager static public methods

		//-----------------------------------------------------------------------
		public static bool ExistAccessRights(SqlConnection aConnection)
		{
			if (!ExistTable("MSD_SLObjects", aConnection) || !ExistTable("MSD_SLDeniedAccesses", aConnection))
				return false;

			SqlCommand sqlCommand = null;
			try
			{
				sqlCommand = new SqlCommand("SELECT COUNT(*) FROM MSD_SLObjects", aConnection);
				return (int)sqlCommand.ExecuteScalar() > 0;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.ExistAccessRights: " + e.Message);
				return false;
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		///<summary> 
		/// Funzione che ritorna se l'azienda identificata dal companyId ha dei permessi della
		/// Security Light da migrare (se la colonna IsSecurityLightMigrated == 0)
		///</summary>
		//-----------------------------------------------------------------------
		public static bool IsCompanyWithSecurityLightGrants(SqlConnection aConnection, int companyId)
		{
			string select = "SELECT COUNT(*) FROM MSD_Companies WHERE CompanyId = {0} AND IsSecurityLightMigrated = 0";

			SqlCommand sqlCommand = null;
			
			try
			{
				sqlCommand = new SqlCommand(string.Format(select, companyId), aConnection);
				return (int)sqlCommand.ExecuteScalar() > 0;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.IsCompanyWithSecurityLightGrants: " + e.Message);
				return false;
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		///<summary>
		/// Estraggo tutti i record presenti nella tabella MSD_SLDeniedAccesses
		/// con CompanyId uguale a -1 (valido per AllCompanies) oppure uguale all'id della company corrente
		/// di cui si vuole fare la migrazione dei permessi della Security Light
		///</summary>
		//-----------------------------------------------------------------------
		public static bool ExistDeniedAccessesToMigrated(SqlConnection aConnection, int companyId)
		{
			string query = @"SELECT COUNT(*) FROM MSD_SLDeniedAccesses
							WHERE dbo.MSD_SLDeniedAccesses.CompanyId = {0} OR dbo.MSD_SLDeniedAccesses.CompanyId = -1";

			SqlCommand sqlCommand = null;

			try
			{
				sqlCommand = new SqlCommand(string.Format(query, companyId), aConnection);
				return (int)sqlCommand.ExecuteScalar() > 0;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.ExistDeniedAccessesToMigrated: " + e.Message);
				return false;
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public static void DeleteCompanyAccessRights(int aCompanyId, SqlConnection aConnection)
		{
			if (aCompanyId == -1)
				return;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.DeleteCompanyAccessRights.");
				return;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader deniedAccessesReader = null;

			try
			{
				ArrayList objectIds = new ArrayList();

				string queryText = "SELECT DISTINCT ObjectId FROM MSD_SLDeniedAccesses WHERE CompanyId = " + aCompanyId.ToString();
				sqlCommand = new SqlCommand(queryText, aConnection);

				deniedAccessesReader = sqlCommand.ExecuteReader();

				while (deniedAccessesReader.Read())
					objectIds.Add(Convert.ToInt32(deniedAccessesReader["ObjectId"]));

				deniedAccessesReader.Close();

				sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE CompanyId = " + aCompanyId.ToString();
				sqlCommand.ExecuteNonQuery();

				if (objectIds.Count > 0)
				{
					foreach (int aObjectId in objectIds)
						SecuredCommandDBInfo.DeleteAllAllowedObjectId(aObjectId, aConnection);
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.DeleteCompanyAccessRights: " + e.Message);
			}
			finally
			{
				if (deniedAccessesReader != null && !deniedAccessesReader.IsClosed)
					deniedAccessesReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public static void SetSecurityLightMigratedFlag(int aCompanyId, SqlConnection aConnection)
		{
			if (aCompanyId == -1)
				return;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.DeleteCompanyAccessRights.");
				return;
			}

			SqlCommand sqlCommand = null;

			try
			{
				sqlCommand = new SqlCommand("UPDATE MSD_Companies SET IsSecurityLightMigrated = 1 WHERE CompanyId = " + aCompanyId, aConnection);
				sqlCommand.ExecuteNonQuery();

			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.DeleteCompanyAccessRights: " + e.Message);
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public static void DeleteUserAccessRights(int aUserId, SqlConnection aConnection)
		{
			if (aUserId == -1)
				return;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.DeleteUserAccessRights.");
				return;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader deniedAccessesReader = null;

			try
			{
				ArrayList objectIds = new ArrayList();

				string queryText = "SELECT DISTINCT ObjectId FROM MSD_SLDeniedAccesses WHERE UserId = " + aUserId.ToString();
				sqlCommand = new SqlCommand(queryText, aConnection);

				deniedAccessesReader = sqlCommand.ExecuteReader();

				while (deniedAccessesReader.Read())
					objectIds.Add(Convert.ToInt32(deniedAccessesReader["ObjectId"]));

				deniedAccessesReader.Close();

				sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE UserId = " + aUserId.ToString();
				sqlCommand.ExecuteNonQuery();

				if (objectIds.Count > 0)
				{
					foreach (int aObjectId in objectIds)
						SecuredCommandDBInfo.DeleteAllAllowedObjectId(aObjectId, aConnection);
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.DeleteUserAccessRights: " + e.Message);
			}
			finally
			{
				if (deniedAccessesReader != null && !deniedAccessesReader.IsClosed)
					deniedAccessesReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public static void DeleteCompanyUserAccessRights(int aCompanyId, int aUserId, SqlConnection aConnection)
		{
			if (aCompanyId == -1 || aUserId == -1)
				return;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.DeleteUserAccessRights.");
				return;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader deniedAccessesReader = null;

			try
			{
				ArrayList objectIds = new ArrayList();

				string queryText = "SELECT DISTINCT ObjectId FROM MSD_SLDeniedAccesses WHERE CompanyId = " + aCompanyId.ToString() + " AND UserId = " + aUserId.ToString();
				sqlCommand = new SqlCommand(queryText, aConnection);

				deniedAccessesReader = sqlCommand.ExecuteReader();

				while (deniedAccessesReader.Read())
					objectIds.Add(Convert.ToInt32(deniedAccessesReader["ObjectId"]));

				deniedAccessesReader.Close();

				sqlCommand.CommandText = "DELETE FROM MSD_SLDeniedAccesses WHERE CompanyId = " + aCompanyId.ToString() + " AND UserId = " + aUserId.ToString();
				sqlCommand.ExecuteNonQuery();

				if (objectIds.Count > 0)
				{
					foreach (int aObjectId in objectIds)
						SecuredCommandDBInfo.DeleteAllAllowedObjectId(aObjectId, aConnection);
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.DeleteCompanyUserAccessRights: " + e.Message);
			}
			finally
			{
				if (deniedAccessesReader != null && !deniedAccessesReader.IsClosed)
					deniedAccessesReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleObjects
			(
			int aCompanyId,
			SecuredCommandType aCommandType,
			SqlConnection aConnection
			)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetAllCompanyInaccessibleObjects.");
				return null;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader deniedAccessesReader = null;

			try
			{
				string queryText = @"SELECT DISTINCT MSD_SLObjects.NameSpace, MSD_SLObjects.Type FROM MSD_SLObjects INNER JOIN
								MSD_SLDeniedAccesses ON MSD_SLObjects.ObjectId = MSD_SLDeniedAccesses.ObjectId";

				string whereClause = String.Empty;

				if (aCompanyId != -1)
					whereClause += " WHERE (MSD_SLDeniedAccesses.CompanyId = " + aCompanyId.ToString() + " OR MSD_SLDeniedAccesses.CompanyId = -1)";
				else
					whereClause += " WHERE MSD_SLDeniedAccesses.CompanyId = -1";

				if (aCommandType != SecuredCommandType.Undefined)
					whereClause += " AND MSD_SLObjects.Type = " + ((int)aCommandType).ToString();

				queryText += whereClause;

				sqlCommand = new SqlCommand(queryText, aConnection);

				deniedAccessesReader = sqlCommand.ExecuteReader();

				ArrayList inaccessibleObjects = new ArrayList();
				while (deniedAccessesReader.Read())
				{
					string objectNameSpace = (string)deniedAccessesReader["NameSpace"];
					if (objectNameSpace == null || objectNameSpace.Length == 0)
						continue;

					SecuredCommandType objectType = (SecuredCommandType)deniedAccessesReader["Type"];

					NameSpaceObjectType nameSpaceObjectType = NameSpaceObjectType.NotValid;
					switch (objectType)
					{
						case SecuredCommandType.Form:
						case SecuredCommandType.Batch:
							nameSpaceObjectType = NameSpaceObjectType.Document;
							break;
						case SecuredCommandType.Report:
							nameSpaceObjectType = NameSpaceObjectType.Report;
							break;
						case SecuredCommandType.Function:
							nameSpaceObjectType = NameSpaceObjectType.Function;
							break;
						case SecuredCommandType.ExcelDocument:
							nameSpaceObjectType = NameSpaceObjectType.ExcelDocument;
							break;
						case SecuredCommandType.ExcelTemplate:
							nameSpaceObjectType = NameSpaceObjectType.ExcelTemplate;
							break;
						case SecuredCommandType.WordDocument:
							nameSpaceObjectType = NameSpaceObjectType.WordDocument;
							break;
						case SecuredCommandType.WordTemplate:
							nameSpaceObjectType = NameSpaceObjectType.WordTemplate;
							break;
						default:
							break;
					}

					if (nameSpaceObjectType == NameSpaceObjectType.NotValid)
						continue;

					NameSpace aNameSpace = new NameSpace(objectNameSpace, nameSpaceObjectType);
					inaccessibleObjects.Add(aNameSpace);
				}

				deniedAccessesReader.Close();

				return (inaccessibleObjects.Count > 0) ? (NameSpace[])inaccessibleObjects.ToArray(typeof(NameSpace)) : null;
			}
			catch (Exception e)
			{
				Debug.Fail("Exception thrown in SecurityManager.GetAllCompanyInaccessibleObjects: " + e.Message);
				return null;
			}
			finally
			{
				if (deniedAccessesReader != null && !deniedAccessesReader.IsClosed)
					deniedAccessesReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleForms(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.Form, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleBatches(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.Batch, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleReports(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.Report, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleFunctions(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.Function, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleExcelDocuments(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.ExcelDocument, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleExcelTemplates(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.ExcelTemplate, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleWordDocuments(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.WordDocument, aConnection);
		}

		//--------------------------------------------------------------------------------------------------------
		public static INameSpace[] GetAllCompanyInaccessibleWordTemplates(int aCompanyId, SqlConnection aConnection)
		{
			return GetAllCompanyInaccessibleObjects(aCompanyId, SecuredCommandType.WordTemplate, aConnection);
		}

		//-----------------------------------------------------------------------
		public static string GetCompanyName(int aCompanyId, SqlConnection aConnection)
		{
			if (aCompanyId == -1)
				return String.Empty;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetCompanyName.");
				return String.Empty;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader companyReader = null;

			try
			{
				string queryText = "SELECT MSD_Companies.Company FROM MSD_Companies WHERE MSD_Companies.CompanyId = " + aCompanyId.ToString();

				sqlCommand = new SqlCommand(queryText, aConnection);

				companyReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);

				string companyName = String.Empty;
				if (companyReader.Read())
					companyName = (string)companyReader["Company"];

				companyReader.Close();

				return companyName;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetCompanyName: " + e.Message);
				return String.Empty;
			}
			finally
			{
				if (companyReader != null && !companyReader.IsClosed)
					companyReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//-----------------------------------------------------------------------
		public static int GetCompanyId(string aCompanyName, SqlConnection aConnection)
		{
			if (aCompanyName == null || aCompanyName.Length == 0)
				return -1;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetCompanyId.");
				return -1;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader companyReader = null;

			try
			{
				string queryText = "SELECT MSD_Companies.CompanyId FROM MSD_Companies WHERE MSD_Companies.Company = '" + aCompanyName.ToString() + "'";

				sqlCommand = new SqlCommand(queryText, aConnection);

				companyReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);

				int companyId = -1;
				if (companyReader.Read())
					companyId = (int)companyReader["CompanyId"];

				companyReader.Close();

				return companyId;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetCompanyId: " + e.Message);
				return -1;
			}
			finally
			{
				if (companyReader != null && !companyReader.IsClosed)
					companyReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//-----------------------------------------------------------------------
		public static string GetUserName(int aUserId, SqlConnection aConnection)
		{
			if (aUserId == -1)
				return String.Empty;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetUserName.");
				return String.Empty;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader userReader = null;

			try
			{
				string queryText = "SELECT MSD_Logins.Login FROM MSD_Logins WHERE MSD_Logins.LoginId = " + aUserId.ToString();

				sqlCommand = new SqlCommand(queryText, aConnection);

				userReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);

				string userName = String.Empty;
				if (userReader.Read())
					userName = (string)userReader["Login"];

				userReader.Close();

				return userName;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetUserName: " + e.Message);
				return String.Empty;
			}
			finally
			{
				if (userReader != null && !userReader.IsClosed)
					userReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//-----------------------------------------------------------------------
		public static int GetUserId(string aUserName, SqlConnection aConnection)
		{
			if (aUserName == null || aUserName.Length == 0)
				return -1;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetUserId.");
				return -1;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader userReader = null;

			try
			{
				string queryText = "SELECT MSD_Logins.LoginId FROM MSD_Logins WHERE MSD_Logins.Login = '" + aUserName.ToString() + "'";

				sqlCommand = new SqlCommand(queryText, aConnection);

				userReader = sqlCommand.ExecuteReader(CommandBehavior.SingleRow);

				int userId = -1;
				if (userReader.Read())
					userId = (int)userReader["LoginId"];

				userReader.Close();

				return userId;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetUserId: " + e.Message);
				return -1;
			}
			finally
			{
				if (userReader != null && !userReader.IsClosed)
					userReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//-----------------------------------------------------------------------
		public static string[] GetCompanyUserNames(int aCompanyId, SqlConnection aConnection)
		{
			if (aCompanyId == -1)
				return null;

			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetCompanyUserNames.");
				return null;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader companyUsersReader = null;

			try
			{
				string queryText = @"SELECT MSD_Logins.Login FROM MSD_CompanyLogins 
								INNER JOIN MSD_Logins ON MSD_CompanyLogins.LoginId = MSD_Logins.LoginId 
								WHERE MSD_CompanyLogins.CompanyId = " + aCompanyId.ToString() + " ORDER BY MSD_Logins.Login";

				sqlCommand = new SqlCommand(queryText, aConnection);

				companyUsersReader = sqlCommand.ExecuteReader();

				ArrayList userNames = new ArrayList();

				while (companyUsersReader.Read())
				{
					string userName = (string)companyUsersReader["Login"];
					if (userName != null && userName.Length > 0)
						userNames.Add(userName);
				}

				companyUsersReader.Close();

				return (userNames.Count > 0) ? (string[])userNames.ToArray(typeof(string)) : null;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetCompanyUserNames: " + e.Message);
				return null;
			}
			finally
			{
				if (companyUsersReader != null && !companyUsersReader.IsClosed)
					companyUsersReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//===========================================================================
		public class User
		{
			private int id = -1;
			private string name = String.Empty;

			//-----------------------------------------------------------------------
			public User(int aId, string aName)
			{
				id = aId;
				name = aName;
			}

			//-----------------------------------------------------------------------
			public int Id { get { return id; } }
			//-----------------------------------------------------------------------
			public string Name { get { return name; } }
		}

		//-----------------------------------------------------------------------
		public static User[] GetCompanyUsers(int aCompanyId, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetCompanyUsers.");
				return null;
			}

			SqlCommand sqlCommand = null;
			SqlDataReader companyUsersReader = null;

			try
			{
				string queryText = @"SELECT DISTINCT MSD_Logins.LoginId, MSD_Logins.Login FROM MSD_Logins 
									INNER JOIN MSD_CompanyLogins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId";

				if (aCompanyId != -1)
					queryText += " WHERE MSD_CompanyLogins.CompanyId = " + aCompanyId.ToString();
				
				queryText += " ORDER BY MSD_Logins.Login";

				sqlCommand = new SqlCommand(queryText, aConnection);

				companyUsersReader = sqlCommand.ExecuteReader();

				ArrayList userNames = new ArrayList();

				while (companyUsersReader.Read())
				{
					string userName = (string)companyUsersReader["Login"];
					if (userName != null && userName.Length > 0)
					{
						if (aUserNamesToSkipList != null && aUserNamesToSkipList.Length > 0)
						{
							bool skipUser = false;
							foreach (string aUserNameToSkip in aUserNamesToSkipList)
							{
								if (aUserNameToSkip != null && aUserNameToSkip.Length > 0 && String.Compare(userName, aUserNameToSkip) == 0)
								{
									skipUser = true;
									break;
								}
							}
							if (skipUser)
								continue;
						}
						userNames.Add(new User((int)companyUsersReader["LoginId"], userName));
					}
				}

				companyUsersReader.Close();

				return (userNames.Count > 0) ? (User[])userNames.ToArray(typeof(User)) : null;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetCompanyUsers: " + e.Message);
				return null;
			}
			finally
			{
				if (companyUsersReader != null && !companyUsersReader.IsClosed)
					companyUsersReader.Close();

				if (sqlCommand != null)
					sqlCommand.Dispose();
			}
		}

		//-----------------------------------------------------------------------
		public static User[] GetAllUsers(SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			return GetCompanyUsers(-1, aConnection, aUserNamesToSkipList);
		}

		//===========================================================================
		public class Company
		{
			private int id = -1;
			private string name = String.Empty;

			//-----------------------------------------------------------------------
			public Company(int aId, string aName)
			{
				id = aId;
				name = aName;
			}

			//-----------------------------------------------------------------------
			public int Id { get { return id; } }
			//-----------------------------------------------------------------------
			public string Name { get { return name; } }
		}
		
		//-----------------------------------------------------------------------
		public static Company[] GetUserCompanies(int aUserId, SqlConnection aConnection)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
			{
				Debug.Fail("Invalid connection passed as argument to SecurityManager.GetUserCompanies.");
				return null;
			}

			SqlDataReader userCompaniesReader = null;

			try
			{
				if (aUserId != -1)
				{
					if (getUserCompaniesCommand == null || getUserCompaniesCommand.Connection != aConnection)
					{
						if (getUserCompaniesCommand != null)
						{
							getUserCompaniesCommand.Dispose();
							getUserCompaniesCommand = null;
						}

						string queryText = @"SELECT DISTINCT MSD_Companies.CompanyId, MSD_Companies.Company FROM MSD_Companies 
											INNER JOIN MSD_CompanyLogins ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId";
						
						queryText += " WHERE MSD_CompanyLogins.LoginId = " + UserIdSqlCommandParameterName + " ORDER BY MSD_Companies.Company";

						getUserCompaniesCommand = new SqlCommand(queryText, aConnection);

						getUserCompaniesCommand.Parameters.Add(UserIdSqlCommandParameterName, SqlDbType.Int, 4);
						getUserCompaniesCommand.Prepare();
					}

					getUserCompaniesCommand.Parameters[UserIdSqlCommandParameterName].Value = aUserId;

					userCompaniesReader = getUserCompaniesCommand.ExecuteReader();
				}
				else
				{
					if (getAllCompaniesCommand == null || getAllCompaniesCommand.Connection != aConnection)
					{
						if (getAllCompaniesCommand != null)
						{
							getAllCompaniesCommand.Dispose();
							getAllCompaniesCommand = null;
						}

						string queryText = @"SELECT DISTINCT MSD_Companies.CompanyId, MSD_Companies.Company FROM MSD_Companies 
											INNER JOIN MSD_CompanyLogins ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId";

						queryText += " ORDER BY MSD_Companies.Company";

						getAllCompaniesCommand = new SqlCommand(queryText, aConnection);

						getAllCompaniesCommand.Prepare();
					}

					userCompaniesReader = getAllCompaniesCommand.ExecuteReader();
				}

				ArrayList userCompanies = new ArrayList();

				while (userCompaniesReader.Read())
				{
					string companyName = (string)userCompaniesReader["Company"];
					if (companyName != null && companyName.Length > 0)
						userCompanies.Add(new Company((int)userCompaniesReader["CompanyId"], companyName));
				}
				userCompaniesReader.Close();

				return (userCompanies.Count > 0) ? (Company[])userCompanies.ToArray(typeof(Company)) : null;
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.GetUserCompanies: " + e.Message);

				if (getAllCompaniesCommand != null)
				{
					getAllCompaniesCommand.Dispose();
					getAllCompaniesCommand = null;
				}

				if (getUserCompaniesCommand != null)
				{
					getUserCompaniesCommand.Dispose();
					getUserCompaniesCommand = null;
				}

				return null;
			}
			finally
			{
				if (userCompaniesReader != null && !userCompaniesReader.IsClosed)
					userCompaniesReader.Close();
			}
		}

		//-----------------------------------------------------------------------
		public static Company[] GetAllCompanies(SqlConnection aConnection)
		{
			return GetUserCompanies(-1, aConnection);
		}

		//---------------------------------------------------------------------
		public static int[] GetAllCompanyIds(SqlConnection aConnection)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
				return null;

			SqlCommand selectCompaniesSqlCommand = null;
			SqlDataReader companiesReader = null;

			try
			{
				string queryText = @"SELECT MSD_Companies.CompanyId FROM MSD_Companies ORDER BY MSD_Companies.CompanyId";

				selectCompaniesSqlCommand = new SqlCommand(queryText, aConnection);

				companiesReader = selectCompaniesSqlCommand.ExecuteReader();

				ArrayList companysIds = new ArrayList();

				while (companiesReader.Read())
					companysIds.Add((int)companiesReader["CompanyId"]);

				companiesReader.Close();

				return (int[])companysIds.ToArray(typeof(int));
			}
			catch (SqlException exception)
			{
				Debug.Fail("SqlException raised in SecuredCommandDBInfo.GetAllCompanyIds: " + exception.Message);

				return null;
			}
			finally
			{
				if (companiesReader != null && !companiesReader.IsClosed)
					companiesReader.Close();

				if (selectCompaniesSqlCommand != null)
					selectCompaniesSqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public static int[] GetAllUserIds(SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			if (aConnection == null || aConnection.State != ConnectionState.Open)
				return null;

			SqlCommand selectUsersSqlCommand = null;
			SqlDataReader usersReader = null;

			try
			{
				string queryText = @"SELECT MSD_Logins.LoginId, MSD_Logins.Login FROM MSD_Logins ORDER BY MSD_Logins.LoginId";

				selectUsersSqlCommand = new SqlCommand(queryText, aConnection);

				usersReader = selectUsersSqlCommand.ExecuteReader();

				ArrayList userIds = new ArrayList();

				while (usersReader.Read())
				{
					string userName = usersReader["Login"].ToString();
					if (userName == null || userName.Length == 0)
						continue;

					if (aUserNamesToSkipList != null && aUserNamesToSkipList.Length > 0)
					{
						bool skipUser = false;
						foreach (string aUserNameToSkip in aUserNamesToSkipList)
						{
							if (aUserNameToSkip != null && aUserNameToSkip.Length > 0 && String.Compare(userName, aUserNameToSkip) == 0)
							{
								skipUser = true;
								break;
							}
						}
						if (skipUser)
							continue;
					}

					userIds.Add((int)usersReader["LoginId"]);
				}

				usersReader.Close();

				return (int[])userIds.ToArray(typeof(int));
			}
			catch (SqlException exception)
			{
				Debug.Fail("SqlException raised in SecuredCommandDBInfo.GetAllUserIds: " + exception.Message);

				return null;
			}
			finally
			{
				if (usersReader != null && !usersReader.IsClosed)
					usersReader.Close();

				if (selectUsersSqlCommand != null)
					selectUsersSqlCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public static DirectoryInfo[] GetAllCustomPaths
			(
			IBasePathFinder aPathFinder,
			int aCompanyId,
			int aUserId,
			SqlConnection aConnection,
			string[] aUserNamesToSkipList,
			bool checkExistence
			)
		{
			string[] customPathNames = GetAllCustomPathNames(aPathFinder, aCompanyId, aUserId, aConnection, aUserNamesToSkipList);
			if (customPathNames == null || customPathNames.Length == 0)
				return null;

			ArrayList customPaths = new ArrayList();

			foreach (string aCustomPathName in customPathNames)
			{
				if
					(
					aCustomPathName == null ||
					aCustomPathName.Length == 0 ||
					(checkExistence && !Directory.Exists(aCustomPathName))
					)
					continue;
				customPaths.Add(new DirectoryInfo(aCustomPathName));
			}

			return (customPaths.Count > 0) ? (DirectoryInfo[])customPaths.ToArray(typeof(DirectoryInfo)) : null;
		}

		//---------------------------------------------------------------------
		public static DirectoryInfo[] GetAllCustomPaths(IBasePathFinder aPathFinder, int aCompanyId, int aUserId, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			return GetAllCustomPaths(aPathFinder, aCompanyId, aUserId, aConnection, aUserNamesToSkipList, false);
		}

		//---------------------------------------------------------------------
		public static FileInfo[] GetAllCustomMenuFiles
			(
			IBasePathFinder aPathFinder,
			int aCompanyId,
			int aUserId,
			SqlConnection aConnection,
			string[] aUserNamesToSkipList,
			bool checkExistence
			)
		{
			DirectoryInfo[] customPaths = GetAllCustomPaths(aPathFinder, aCompanyId, aUserId, aConnection, aUserNamesToSkipList, checkExistence);
			if (customPaths == null || customPaths.Length == 0)
				return null;

			ArrayList customMenuFiles = new ArrayList();

			foreach (DirectoryInfo aCustomDirectoryInfo in customPaths)
			{
				string fullSLDenyMenuFileName = aCustomDirectoryInfo.FullName;
				if (fullSLDenyMenuFileName == null || fullSLDenyMenuFileName.Length == 0)
					continue;

				if (fullSLDenyMenuFileName[fullSLDenyMenuFileName.Length - 1] != Path.DirectorySeparatorChar)
					fullSLDenyMenuFileName += Path.DirectorySeparatorChar;

				fullSLDenyMenuFileName += SLDenyFile.SecurityLightDenyMenuFileName;
				fullSLDenyMenuFileName += NameSolverStrings.MenuExtension;

				if (checkExistence && !File.Exists(fullSLDenyMenuFileName))
					continue;

				customMenuFiles.Add(new FileInfo(fullSLDenyMenuFileName));
			}
			return (customMenuFiles.Count > 0) ? (FileInfo[])customMenuFiles.ToArray(typeof(FileInfo)) : null;
		}

		//---------------------------------------------------------------------
		public static FileInfo[] GetAllCustomMenuFiles(IBasePathFinder aPathFinder, int aCompanyId, int aUserId, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			return GetAllCustomMenuFiles(aPathFinder, aCompanyId, aUserId, aConnection, aUserNamesToSkipList, false);
		}

		//---------------------------------------------------------------------
		public static FileInfo[] GetAllExistingCustomMenuFiles(IBasePathFinder aPathFinder, int aCompanyId, int aUserId, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			return GetAllCustomMenuFiles(aPathFinder, aCompanyId, aUserId, aConnection, aUserNamesToSkipList, true);
		}

		//---------------------------------------------------------------------
		public static void CleanDeniedAccesses
			(
			MenuXmlParser aMenuXmlParser,
			MenuLoader.CommandsTypeToLoad commandTypesToClean,
			IPathFinder aPathFinder,
			string aConnectionString
			)
		{
			if
				(
				aMenuXmlParser == null ||
				aPathFinder == null ||
				aConnectionString == null ||
				aConnectionString.Trim().Length == 0
				)
				return;

			SqlConnection consoleDBConnection = null;

			try
			{
				consoleDBConnection = new SqlConnection(aConnectionString);
				consoleDBConnection.Open();

				int aCompanyId = GetCompanyId(((PathFinder)aPathFinder).Company, consoleDBConnection);
				if (aCompanyId == -1)
					return;
				int aUserId = GetUserId(((PathFinder)aPathFinder).User, consoleDBConnection);
				if (aUserId == -1)
					return;

				FileInfo[] customMenuFiles = GetAllExistingCustomMenuFiles(aPathFinder, aCompanyId, aUserId, consoleDBConnection, null);
				if (customMenuFiles == null || customMenuFiles.Length == 0)
					return;

				foreach (FileInfo aCustomFileInfo in customMenuFiles)
				{
					if (aCustomFileInfo == null)
						continue;

					SLDenyFile denyFile = new SLDenyFile(aCustomFileInfo);

					denyFile.CleanDeniedAccesses(aMenuXmlParser, commandTypesToClean);
				}
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException thrown in SecurityManager.ExistSqlTable: " + e.Message);
			}
			finally
			{
				if (consoleDBConnection != null)
				{
					if ((consoleDBConnection.State & ConnectionState.Open) == ConnectionState.Open)
						consoleDBConnection.Close();

					consoleDBConnection.Dispose();
				}
			}
		}

		//---------------------------------------------------------------------
		public static void RefreshDeniedAccessesInSLDenyFiles
			(
			int aCompanyId,
			int aUserId,
			SecuredCommandType aSecuredCommandTypeToRefresh,
			IPathFinder aPathFinder,
			SqlConnection aConnection,
			string[] aUserNamesToSkipList
			)
		{
			if (aPathFinder == null || aConnection == null || aConnection.State != ConnectionState.Open)
				return;

			if (aCompanyId == -1)
			{
				int[] companyIds = GetAllCompanyIds(aConnection);
				if (companyIds == null || companyIds.Length == 0)
					return;

				foreach (int aCompanyIdToRefresh in companyIds)
					RefreshDeniedAccessesInSLDenyFiles(aCompanyIdToRefresh, aUserId, aSecuredCommandTypeToRefresh, aPathFinder, aConnection, aUserNamesToSkipList);

				return;
			}

			if (aSecuredCommandTypeToRefresh == SecuredCommandType.Undefined)
			{
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.Form, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.Batch, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.Report, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.Function, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.WordDocument, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.WordTemplate, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.ExcelDocument, aPathFinder, aConnection, aUserNamesToSkipList);
				RefreshDeniedAccessesInSLDenyFiles(aCompanyId, aUserId, SecuredCommandType.ExcelTemplate, aPathFinder, aConnection, aUserNamesToSkipList);

				return;
			}

			string companyname = GetCompanyName(aCompanyId, aConnection);
			if (companyname == null || companyname.Length == 0)
				return;

			User[] companyUsers = GetCompanyUsers(aCompanyId, aConnection, aUserNamesToSkipList);

			SLDenyFile allUsersSLDenyFile = new SLDenyFile(GetSecurityLightDenyMenuFileName(aPathFinder, companyname, NameSolverStrings.AllUsers));
			allUsersSLDenyFile.RemoveAllDeniedAccesses(aSecuredCommandTypeToRefresh);

			INameSpace[] inaccessibleObjects = GetAllCompanyInaccessibleObjects(aCompanyId, aSecuredCommandTypeToRefresh, aConnection);
			if (inaccessibleObjects != null && inaccessibleObjects.Length > 0)
			{
				foreach (INameSpace deniedObjectNameSpace in inaccessibleObjects)
				{
					if (IsAccessToObjectDenied(deniedObjectNameSpace, aCompanyId, -1, aConnection))
						allUsersSLDenyFile.AddDeniedAccess(deniedObjectNameSpace.GetNameSpaceWithoutType(), aSecuredCommandTypeToRefresh);
				}
			}

			if (companyUsers != null && companyUsers.Length > 0)
			{
				foreach (User aUser in companyUsers)
				{
					if (aUser.Id == -1 || aUser.Name == null || aUser.Name.Length == 0)
						continue;

					if (aUserId == -1 || aUserId == aUser.Id)
					{
						SLDenyFile userSLDenyFile = new SLDenyFile(GetSecurityLightDenyMenuFileName(aPathFinder, companyname, aUser.Name));
						userSLDenyFile.RemoveAllDeniedAccesses(aSecuredCommandTypeToRefresh);

						if (inaccessibleObjects != null && inaccessibleObjects.Length > 0)
						{
							foreach (NameSpace deniedObjectNameSpace in inaccessibleObjects)
							{
								if (IsAccessToObjectDenied(deniedObjectNameSpace, aCompanyId, aUser.Id, aConnection))
								{
									userSLDenyFile.AddDeniedAccess(deniedObjectNameSpace.GetNameSpaceWithoutType(), aSecuredCommandTypeToRefresh);
								}
							}
						}
					}
				}
			}
		}

		//---------------------------------------------------------------------
		public static void RefreshDeniedAccessesInSLDenyFiles
			(
			int aCompanyId,
			SecuredCommandType aSecuredCommandTypeToRefresh,
			IPathFinder aPathFinder,
			SqlConnection aConnection,
			string[] aUserNamesToSkipList
			)
		{
			RefreshDeniedAccessesInSLDenyFiles(aCompanyId, -1, aSecuredCommandTypeToRefresh, aPathFinder, aConnection, aUserNamesToSkipList);
		}

		//---------------------------------------------------------------------
		public static void RebuildAllSLDenyFiles(IPathFinder aPathFinder, SqlConnection aConnection, string[] aUserNamesToSkipList)
		{
			RefreshDeniedAccessesInSLDenyFiles(-1, SecuredCommandType.Undefined, aPathFinder, aConnection, aUserNamesToSkipList);
		}

		//---------------------------------------------------------------------
		public static void RebuildAllSLDenyFiles(IPathFinder aPathFinder, string aConnectionString, string[] aUserNamesToSkipList)
		{
			if (aPathFinder == null || aConnectionString == null || aConnectionString.Length == 0)
				return;

			SqlConnection consoleDBConnection = null;

			try
			{
				consoleDBConnection = new SqlConnection(aConnectionString);
				consoleDBConnection.Open();

				RebuildAllSLDenyFiles(aPathFinder, consoleDBConnection, aUserNamesToSkipList);
			}
			catch (Exception)
			{
			}
			finally
			{
				if (consoleDBConnection != null)
				{
					if ((consoleDBConnection.State & ConnectionState.Open) == ConnectionState.Open)
						consoleDBConnection.Close();

					consoleDBConnection.Dispose();
				}
			}
		}

		#endregion // SecurityManager static public methods

		#endregion // SecurityManager public methods
	}
}
