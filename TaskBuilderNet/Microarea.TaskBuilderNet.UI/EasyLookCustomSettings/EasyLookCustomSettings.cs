using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomSettings;

namespace Microarea.TaskBuilderNet.UI.EasyLookCustomization
{
	/// <summary>
	/// Summary description for EasyLookCustomSettings.
	/// </summary>
	public class EasyLookCustomSettings
	{
		public const string SessionKey = "EasyLookCustomSettingsKey";
		
		public static string	DefaultLogoImageURL = String.Empty;
		public static Color		DefaultAppPanelBkgndColor = Color.FromArgb(132,148,232);
		public static Color		DefaultGroupsPanelBkgndColor = Color.FromArgb(230,230,250);
		public static string	DefaultGroupsPanelBkgndImageURL = String.Empty;
		public static Color		DefaultMenuTreeBkgndColor = Color.FromArgb(238,240,255);
		public static Color		DefaultCommandListBkgndColor = Color.White;
		public static string	DefaultFontFamily = "verdana,tahoma,helvetica";
		public static Color		DefaultAllUsersReportTitleColor = Color.RoyalBlue;
		public static Color		DefaultCurrentUserReportTitleColor = Color.CornflowerBlue;
		public static int		DefaultMaxWrmHistoryNumber = 40;
		
		[Flags]
			private enum WrmHistoryAutoDelTypeEnum : uint
		{
			Undefined				= 0x00000000,		
			OlderThanNYears			= 0x00000001,				
			OlderThanNMonths		= 0x00000002,
			OlderThanNWeeks			= 0x00000004,		
			OlderThanNDays			= 0x00000008,						
			ExpirationTypeMask		= 0x0000FFFF,
			ExpirationPeriodMask	= 0xFFFF0000
		};
		
		private const string EasyLookCustomSettingsTableName	= "MSD_EasyLookCustomSettings";
		
		private const string CompanyIdColumnName					= "CompanyId";
		private const string LoginIdColumnName						= "LoginId";
		private const string LogoImageURLColumnName					= "LogoImageURL";
		private const string AppPanelBkgndColorColumnName			= "AppPanelBkgndColor";
		private const string GroupsPanelBkgndColorColumnName		= "GroupsPanelBkgndColor";
		private const string GroupsPanelBkgndImageURLColumnName		= "GroupsPanelBkgndImageURL";
		private const string MenuTreeBkgndColorColumnName			= "MenuTreeBkgndColor";
		private const string CommandListBkgndColorColumnName		= "CommandListBkgndColor";
		private const string FontFamilyColumnName					= "FontFamily";
		private const string AllUsersReportTitleColorColumnName		= "AllUsersReportTitleColor";
		private const string CurrentUserReportTitleColorColumnName	= "CurrentUserReportTitleColor";
		private const string MaxWrmHistoryNumColumnName				= "MaxWrmHistoryNum";
		private const string WrmHistoryAutoDelEnabledColumnName		= "WrmHistoryAutoDelEnabled";
		private const string WrmHistoryAutoDelTypeColumnName		= "WrmHistoryAutoDelType";

		public static string[] EasyLookCustomSettingsTableColumns = new string[14]{
																					  CompanyIdColumnName,
																					  LoginIdColumnName,
																					  LogoImageURLColumnName,
																					  AppPanelBkgndColorColumnName,
																					  GroupsPanelBkgndColorColumnName,
																					  GroupsPanelBkgndImageURLColumnName,
																					  MenuTreeBkgndColorColumnName,
																					  CommandListBkgndColorColumnName,
																					  FontFamilyColumnName,
																					  AllUsersReportTitleColorColumnName,
																					  CurrentUserReportTitleColorColumnName,
																					  MaxWrmHistoryNumColumnName,
																					  WrmHistoryAutoDelEnabledColumnName,
																					  WrmHistoryAutoDelTypeColumnName
																				  };
		
		private string	consoleDBConnectionString = String.Empty;
		private bool	inheritedFromAllUsers = false;
		private IBasePathFinder	pathFinder = null;

		private int		companyId = -1;
		private int		loginId = -1;

		private string	logoImageURL = DefaultLogoImageURL;
		private string	logoImageFullPath = string.Empty;

		private int		appPanelBkgndColor = DefaultAppPanelBkgndColor.ToArgb();
		private bool	isGroupsPanelBkgndColorDefined = false;
		private int		groupsPanelBkgndColor = DefaultGroupsPanelBkgndColor.ToArgb();

		private string	groupsPanelBkgndImageURL = DefaultGroupsPanelBkgndImageURL;
		private string	groupsPanelBkgndImageFullPath = string.Empty;

		private int		menuTreeBkgndColor = DefaultMenuTreeBkgndColor.ToArgb();
		private int		commandListBkgndColor = DefaultCommandListBkgndColor.ToArgb();
		private string	fontFamily = DefaultFontFamily;
		private int		allUsersReportTitleColor = DefaultAllUsersReportTitleColor.ToArgb();
		private int		currentUserReportTitleColor = DefaultCurrentUserReportTitleColor.ToArgb();
		private int		maxWrmHistoryNum = DefaultMaxWrmHistoryNumber;
		private bool	wrmHistoryAutoDelEnabled = false;
		private WrmHistoryAutoDelTypeEnum	wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.Undefined;
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public EasyLookCustomSettings(string aDBConnectionString, int aCompanyId, int aLoginId)
		{
			companyId = aCompanyId;
			loginId = aLoginId;
		
			consoleDBConnectionString = aDBConnectionString;

			FillSettingsFromDB();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public EasyLookCustomSettings(IBasePathFinder aPathFinder, int aCompanyId, int aLoginId)
		{
			companyId = aCompanyId;
			loginId = aLoginId;

			if (aPathFinder != null && InstallationData.ServerConnectionInfo != null)
			{
				pathFinder = aPathFinder;
				consoleDBConnectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;

				FillSettingsFromDB();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public EasyLookCustomSettings(IBasePathFinder aPathFinder, string aCompany, string aUser)
		{
			if (aPathFinder != null && InstallationData.ServerConnectionInfo != null)
			{
				consoleDBConnectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;
				
				pathFinder = aPathFinder;
				if (GetLoginIds(aCompany, aUser))
					FillSettingsFromDB();
			}
		}

		#region EasyLookCustomSettings public properties
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool InheritedFromAllUsers { get { return inheritedFromAllUsers; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public int CompanyId { get { return companyId; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public int LoginId { get { return loginId; } }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public string	LogoImageURL				
		{
			get { return logoImageURL; } 
			set 
			{ 
				if (value == null || value == String.Empty)
					logoImageURL = DefaultLogoImageURL;
				else
					logoImageURL = value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string	LogoImageFullPath				
		{
			get { return logoImageFullPath; } 
			set 
			{ 
				if (value == null || value == String.Empty)
					logoImageFullPath = string.Empty;
				else
					logoImageFullPath = value;
			}
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		public int		AppPanelBkgndColor				{ get { return appPanelBkgndColor; } set { appPanelBkgndColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool		IsDefaultAppPanelBkgndColor		{ get { return appPanelBkgndColor == DefaultAppPanelBkgndColor.ToArgb(); } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public int		GroupsPanelBkgndColor			{ get { return groupsPanelBkgndColor; } set { groupsPanelBkgndColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool		IsDefaultGroupsPanelBkgndColor	{ get { return groupsPanelBkgndColor == DefaultGroupsPanelBkgndColor.ToArgb(); } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool		IsGroupsPanelBkgndColorDefined	{ get { return isGroupsPanelBkgndColorDefined; } set { isGroupsPanelBkgndColorDefined = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public string	GroupsPanelBkgndImageURL
		{
			get { return groupsPanelBkgndImageURL; } 
			set 
			{ 
				if (value == null || value == String.Empty)
					groupsPanelBkgndImageURL = DefaultLogoImageURL;
				else
					groupsPanelBkgndImageURL = value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string	GroupsPanelBkgndImageFullPath
		{
			get { return groupsPanelBkgndImageFullPath; } 
			set 
			{ 
				if (value == null || value == String.Empty)
					groupsPanelBkgndImageFullPath = DefaultLogoImageURL;
				else
					groupsPanelBkgndImageFullPath = value;
			}
		}
			 
		//--------------------------------------------------------------------------------------------------------------------------------
		public int		MenuTreeBkgndColor { get { return menuTreeBkgndColor; } set { menuTreeBkgndColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public int		CommandListBkgndColor { get { return commandListBkgndColor; } set { commandListBkgndColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public string	FontFamily { get { return fontFamily; } set { fontFamily = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public int		AllUsersReportTitleColor { get { return allUsersReportTitleColor; } set { allUsersReportTitleColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public int		CurrentUserReportTitleColor { get { return currentUserReportTitleColor; } set { currentUserReportTitleColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool ApplyDefaultLayout	
		{
			get 
			{
				return
					(
						String.Compare(logoImageURL, DefaultLogoImageURL, true, CultureInfo.InvariantCulture) == 0 &&
						appPanelBkgndColor == DefaultAppPanelBkgndColor.ToArgb() &&
						String.Compare(groupsPanelBkgndImageURL, DefaultGroupsPanelBkgndImageURL, true, CultureInfo.InvariantCulture) == 0 &&
						groupsPanelBkgndColor == DefaultGroupsPanelBkgndColor.ToArgb() &&
						menuTreeBkgndColor == DefaultMenuTreeBkgndColor.ToArgb() &&
						commandListBkgndColor == DefaultCommandListBkgndColor.ToArgb() &&
						String.Compare(fontFamily, DefaultFontFamily) == 0 &&
						allUsersReportTitleColor == DefaultAllUsersReportTitleColor.ToArgb() &&
						currentUserReportTitleColor == DefaultCurrentUserReportTitleColor.ToArgb()
					);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool ApplyDefaultWrmHistoryMng
		{
			get 
			{
				return
					(
					maxWrmHistoryNum == DefaultMaxWrmHistoryNumber &&
					!wrmHistoryAutoDelEnabled &&
					wrmHistoryAutoDelType == WrmHistoryAutoDelTypeEnum.Undefined
					);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool	ApplyDefault
		{
			get 
			{
				return ApplyDefaultLayout && ApplyDefaultWrmHistoryMng;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public int MaxWrmHistoryNum { get { return maxWrmHistoryNum; } set { maxWrmHistoryNum = value; } }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsWrmHistoryAutoDelEnabled	{ get { return wrmHistoryAutoDelEnabled; } set { wrmHistoryAutoDelEnabled = value; } }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool AutoDelWrmHistoryUndefined
		{
			get { return ((wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationTypeMask) == WrmHistoryAutoDelTypeEnum.Undefined); }
			set 
			{
				if (value)
					wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.Undefined | (wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationPeriodMask);
				else
					wrmHistoryAutoDelType &= ~WrmHistoryAutoDelTypeEnum.Undefined;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool AutoDelWrmHistoryYearly
		{
			get { return ((wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.OlderThanNYears) == WrmHistoryAutoDelTypeEnum.OlderThanNYears); } 
			set 
			{
				if (value)
					wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.OlderThanNYears | (wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationPeriodMask);
				else
					wrmHistoryAutoDelType &= ~WrmHistoryAutoDelTypeEnum.OlderThanNYears;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool AutoDelWrmHistoryMonthly
		{
			get { return ((wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.OlderThanNMonths) == WrmHistoryAutoDelTypeEnum.OlderThanNMonths); } 
			set 
			{
				if (value)
					wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.OlderThanNMonths | (wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationPeriodMask);
				else
					wrmHistoryAutoDelType &= ~WrmHistoryAutoDelTypeEnum.OlderThanNMonths;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool AutoDelWrmHistoryWeekly
		{
			get { return ((wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.OlderThanNWeeks) == WrmHistoryAutoDelTypeEnum.OlderThanNWeeks); } 
			set 
			{
				if (value)
					wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.OlderThanNWeeks | (wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationPeriodMask);
				else
					wrmHistoryAutoDelType &= ~WrmHistoryAutoDelTypeEnum.OlderThanNWeeks;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool AutoDelWrmHistoryDaily
		{
			get { return ((wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.OlderThanNDays) == WrmHistoryAutoDelTypeEnum.OlderThanNDays); } 
			set 
			{
				if (value)
					wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.OlderThanNDays | (wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationPeriodMask);
				else
					wrmHistoryAutoDelType &= ~WrmHistoryAutoDelTypeEnum.OlderThanNDays;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public UInt16	WrmHistoryAutoDelPeriod	 
		{
			get { return (UInt16)((UInt32)(wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationPeriodMask) >> 16); } 
			set 
			{
				UInt32 period = Convert.ToUInt32(value) << 16;

				wrmHistoryAutoDelType = (WrmHistoryAutoDelTypeEnum)(period | (UInt32)(wrmHistoryAutoDelType & WrmHistoryAutoDelTypeEnum.ExpirationTypeMask));
			}
		}

		#endregion

		#region EasyLookCustomSettings public methods

		//--------------------------------------------------------------------------------------------------------------------------------
		public void ResetLayoutDefaults()
		{
			logoImageURL					= DefaultLogoImageURL;
			logoImageFullPath				= string.Empty;
			appPanelBkgndColor				= DefaultAppPanelBkgndColor.ToArgb();
			isGroupsPanelBkgndColorDefined	= false;
			groupsPanelBkgndColor			= DefaultGroupsPanelBkgndColor.ToArgb();
			groupsPanelBkgndImageURL		= DefaultGroupsPanelBkgndImageURL;
			groupsPanelBkgndImageFullPath   = string.Empty;
			menuTreeBkgndColor				= DefaultMenuTreeBkgndColor.ToArgb();
			commandListBkgndColor			= DefaultCommandListBkgndColor.ToArgb();
			fontFamily						= DefaultFontFamily;
			allUsersReportTitleColor		= DefaultAllUsersReportTitleColor.ToArgb();
			currentUserReportTitleColor		= DefaultCurrentUserReportTitleColor.ToArgb();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void ResetWrmHistoryMngDefaults()
		{
			maxWrmHistoryNum = DefaultMaxWrmHistoryNumber;
			wrmHistoryAutoDelEnabled = false;
			wrmHistoryAutoDelType = WrmHistoryAutoDelTypeEnum.Undefined;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void ResetDefaults()
		{
			ResetLayoutDefaults();
			ResetWrmHistoryMngDefaults();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Save(ref bool inserted)
		{
			inserted = false;

			if (consoleDBConnectionString == null || consoleDBConnectionString == String.Empty)
			{
				Debug.Fail("EasyLookCustomSettings.Save Error: null or empty connection string.");
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.EmptyConnectionStringMsg);
			}

			if (companyId == -1)
				return false;

			SqlConnection connection = null;
			SqlTransaction  saveSqlTransaction = null;
			SqlCommand saveCommand = null;

			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();

				if (!ExistsCompanyLogin(connection, companyId, loginId))
					return false;
	
				// Se le impostazioni che sto salvando coincidono con quelle di default si 
				// deve cancellare il record corrispondente, a meno che non si tratti di
				// un utente particolare (loginId != -1) e che esistano delle impostazioni
				// relative ad AllUsers, e quindi ereditate da esso, che si vogliono 
				// sovrascrivere
				if (ApplyDefault && (loginId == -1 || !Exists(connection, companyId, -1)))
				{
					if (Exists(connection, companyId, loginId))
						return Delete();
					return true;
				}

				string saveQueryText = String.Empty;
				if (Exists(connection, companyId, loginId))
				{
					saveQueryText = "UPDATE ";
					saveQueryText += EasyLookCustomSettingsTableName;
					saveQueryText += " SET ";

					string fieldsToSet = String.Empty;
					foreach (string columnName in EasyLookCustomSettingsTableColumns)
					{
						if (IsGroupsPanelBkgndColorDefined)
						{
							if (String.Compare(columnName, GroupsPanelBkgndImageURLColumnName) == 0)
							{
								if (fieldsToSet.Length > 0)
									fieldsToSet += ",";
								fieldsToSet += columnName + " = NULL";
								continue;
							}
						}
						else if (String.Compare(columnName, GroupsPanelBkgndColorColumnName) == 0)
						{
							if (fieldsToSet.Length > 0)
								fieldsToSet += ",";
							fieldsToSet += columnName + " = NULL";
							continue;
						}
						if (fieldsToSet.Length > 0)
							fieldsToSet += ",";

						fieldsToSet += columnName + " = @" + columnName;
					}
					saveQueryText += fieldsToSet;				
					saveQueryText += " WHERE " + CompanyIdColumnName + " = " + companyId.ToString();
					saveQueryText += " AND " + LoginIdColumnName + " = " + loginId.ToString();
				}
				else
				{
					inserted = true;
					
					saveQueryText = "INSERT INTO ";
					saveQueryText += EasyLookCustomSettingsTableName;
					saveQueryText += " (";

					string fieldsToInsert = String.Empty;
					string parametersToInsert = String.Empty;
					foreach (string columnName in EasyLookCustomSettingsTableColumns)
					{
						if (IsGroupsPanelBkgndColorDefined)
						{
							if (String.Compare(columnName, GroupsPanelBkgndImageURLColumnName) == 0)
								continue;
						}
						else if (String.Compare(columnName, GroupsPanelBkgndColorColumnName) == 0)
							continue;

						if (fieldsToInsert.Length > 0)
							fieldsToInsert += ",";

						fieldsToInsert += columnName;

						if (parametersToInsert.Length > 0)
							parametersToInsert += ",";

						parametersToInsert += "@" +columnName;
					}
					saveQueryText += fieldsToInsert + ") VALUES (";
					saveQueryText += parametersToInsert + ")";
				}

				saveCommand = new SqlCommand(saveQueryText, connection);

				saveSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				saveCommand.Connection = connection;
				saveCommand.Transaction = saveSqlTransaction;
				
				SqlParameter param = saveCommand.Parameters.Add("@" + CompanyIdColumnName, SqlDbType.Int, 4, CompanyIdColumnName);
				param.Value = companyId;
				param = saveCommand.Parameters.Add("@" + LoginIdColumnName, SqlDbType.Int, 4, LoginIdColumnName);
				param.Value = loginId;
				param = saveCommand.Parameters.Add("@" + LogoImageURLColumnName, SqlDbType.NVarChar, 255, LogoImageURLColumnName);
				param.Value = logoImageURL;
				param = saveCommand.Parameters.Add("@" + AppPanelBkgndColorColumnName, SqlDbType.Int, 4, AppPanelBkgndColorColumnName);
				param.Value = appPanelBkgndColor;
				if (IsGroupsPanelBkgndColorDefined)
				{
					param = saveCommand.Parameters.Add("@" + GroupsPanelBkgndColorColumnName, SqlDbType.Int, 4, GroupsPanelBkgndColorColumnName);
					param.Value = groupsPanelBkgndColor;
				}
				else
				{
					param = saveCommand.Parameters.Add("@" + GroupsPanelBkgndImageURLColumnName, SqlDbType.NVarChar, 255, GroupsPanelBkgndImageURLColumnName);
					param.Value = groupsPanelBkgndImageURL;
				}
				param = saveCommand.Parameters.Add("@" + MenuTreeBkgndColorColumnName, SqlDbType.Int, 4, MenuTreeBkgndColorColumnName);
				param.Value = menuTreeBkgndColor;
				param = saveCommand.Parameters.Add("@" + CommandListBkgndColorColumnName, SqlDbType.Int, 4, CommandListBkgndColorColumnName);
				param.Value = commandListBkgndColor;
				param = saveCommand.Parameters.Add("@" + FontFamilyColumnName, SqlDbType.NVarChar, 50, FontFamilyColumnName);
				param.Value = fontFamily;
				param = saveCommand.Parameters.Add("@" + AllUsersReportTitleColorColumnName, SqlDbType.Int, 4, AllUsersReportTitleColorColumnName);
				param.Value = allUsersReportTitleColor;				 
				param = saveCommand.Parameters.Add("@" + CurrentUserReportTitleColorColumnName, SqlDbType.Int, 4, CurrentUserReportTitleColorColumnName);
				param.Value = currentUserReportTitleColor;
				param = saveCommand.Parameters.Add("@" + MaxWrmHistoryNumColumnName, SqlDbType.Int, 4, MaxWrmHistoryNumColumnName);
				param.Value = maxWrmHistoryNum;
				param = saveCommand.Parameters.Add("@" + WrmHistoryAutoDelEnabledColumnName, SqlDbType.Bit, 1, WrmHistoryAutoDelEnabledColumnName);
				param.Value = wrmHistoryAutoDelEnabled;
				param = saveCommand.Parameters.Add("@" + WrmHistoryAutoDelTypeColumnName, SqlDbType.Int, 4, WrmHistoryAutoDelTypeColumnName);
				param.Value = wrmHistoryAutoDelType;
				
				saveCommand.ExecuteNonQuery();
				
				saveSqlTransaction.Commit();
	
				if ((logoImageFullPath != null && this.logoImageFullPath.Length != 0) || 
					(groupsPanelBkgndImageFullPath != null && groupsPanelBkgndImageFullPath.Length != 0))
					CopyImageToServer();

				return true;
			}
			catch (Exception exception)
			{
				if (saveSqlTransaction != null)
					saveSqlTransaction.Rollback();

				inserted = false;

				Debug.Fail("Exception raised in EasyLookCustomSettings.Save: " + exception.Message);
				return false;
			}
			finally
			{
				if (saveCommand != null)
					saveCommand.Dispose();

				if (saveSqlTransaction != null)
					saveSqlTransaction.Dispose();

				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}				
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void CopyImageToServer()
		{
			if (pathFinder == null)
				return;
			
			IBaseModuleInfo mod = pathFinder.GetModuleInfoByName("WebFramework", "EasyLook");

			if (mod == null)
				return;

			string destinationPath = mod.Path; 

			destinationPath = Path.Combine(destinationPath, "Files");
			destinationPath = Path.Combine(destinationPath, "images");

			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			destinationPath = Path.Combine(destinationPath, "Companies");

			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			string companyName = GetCompanyName(this.CompanyId);
			if (companyName == null || companyName.Length == 0)
				return;

			destinationPath = Path.Combine(destinationPath, companyName);

			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			if (loginId == -1)
				destinationPath = Path.Combine(destinationPath, "AllUsers");
			else
			{
				string userName = GetUserName(loginId);
				if (userName == null || userName.Length == 0)
					return;

				destinationPath = Path.Combine(destinationPath, userName);
			}
				
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);

			try
			{
				if (LogoImageFullPath != null && LogoImageFullPath.Length != 0)
					File.Copy(LogoImageFullPath, Path.Combine(destinationPath, LogoImageURL), true);
			
				if (GroupsPanelBkgndImageFullPath != null || GroupsPanelBkgndImageFullPath.Length != 0)
					File.Copy(GroupsPanelBkgndImageFullPath , Path.Combine(destinationPath, GroupsPanelBkgndImageURL), true);
			}
			catch(Exception)
			{
			
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Delete()
		{
			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();

				return Delete(connection);
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.Delete: " + exception.Message);
				return false;
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}				
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Delete(SqlConnection connection)
		{
			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("EasyLookCustomSettings.Delete Error: invalid connection.");
			}

			if (!DeleteAllCompanyUserSettings(connection, loginId, companyId))
				return false;

			// Ripristino le generiche impostazioni predefinite
			ResetDefaults();

			// Se le impostazioni cancellate erano riferite ad un utente particolare, ora
			// devo vedere se esistono impostazioni per "AllUsers" da ereditare:
			if (loginId != -1)
				FillUserSettingsFromDB(connection, -1);

			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public DateTime GetWrmHistoryAutoDelExpirationDate()
		{
			DateTime expirationDate = DateTime.MinValue;
			
			if (wrmHistoryAutoDelEnabled)
			{
				if (this.AutoDelWrmHistoryYearly)
					expirationDate = DateTime.Today.AddYears(-(int)this.WrmHistoryAutoDelPeriod);
				else if (this.AutoDelWrmHistoryMonthly)
					expirationDate = DateTime.Today.AddMonths(-(int)this.WrmHistoryAutoDelPeriod);
				else if (this.AutoDelWrmHistoryWeekly)
					expirationDate = DateTime.Today.AddDays(-(int)this.WrmHistoryAutoDelPeriod * 7);
				else if (this.AutoDelWrmHistoryDaily)
					expirationDate = DateTime.Today.AddDays(-(int)this.WrmHistoryAutoDelPeriod);
			}
			
			return expirationDate;
		}
		
		#endregion

		#region EasyLookCustomSettings private methods
		

		//-------------------------------------------------------------------------------------------
		public bool GetLoginIds(string aCompany, string aUser)
		{
			if 
				(
				consoleDBConnectionString == null || 
				consoleDBConnectionString == String.Empty
				)
				return false;

			SqlConnection connection = null;
			SqlCommand selectCompanyIdCommand = null;
			SqlCommand selectUserIdCommand = null;
			SqlDataReader loginDataReader = null;

			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();

				// Per specificare nella query il valore del nome della company, trattandosi di 
				// una stringa, utilizzo un parametro (v. problemi Unicode)
				string selectCompanyId =@"SELECT MSD_Companies.CompanyId FROM MSD_Companies
											WHERE MSD_Companies.Company = @Company";
			
				selectCompanyIdCommand = new SqlCommand(selectCompanyId, connection);

				selectCompanyIdCommand.Parameters.AddWithValue("@Company", aCompany);

				loginDataReader = selectCompanyIdCommand.ExecuteReader();

				if (loginDataReader == null || !loginDataReader.Read() || loginDataReader["CompanyId"] == System.DBNull.Value)
					return false;
				
				companyId = (int)loginDataReader["CompanyId"];

				loginDataReader.Close();

				// Per specificare nella query il valore del nome dello user, trattandosi di 
				// una stringa, utilizzo un parametro (v. problemi Unicode)
				string selectUserId =@"SELECT MSD_Logins.LoginId FROM MSD_Logins
											WHERE MSD_Logins.Login = @User";
				
				selectUserIdCommand = new SqlCommand(selectUserId, connection);
			
				selectUserIdCommand.Parameters.AddWithValue("@User", aUser);
				
				loginDataReader = selectUserIdCommand.ExecuteReader();
				if (loginDataReader == null || !loginDataReader.Read() || loginDataReader["LoginId"] == System.DBNull.Value)
					return false;

				loginId = (int)loginDataReader["LoginId"];

				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.GetLoginIds: " + exception.Message);
				return false;
			}
			finally
			{
				if (loginDataReader != null && !loginDataReader.IsClosed)
					loginDataReader.Close();
				if (selectUserIdCommand != null)
					selectUserIdCommand.Dispose();
				if (selectCompanyIdCommand != null)
					selectCompanyIdCommand.Dispose();

				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		public string GetCompanyName(int aCompanyId)
		{
			if 
				(
				consoleDBConnectionString == null || 
				consoleDBConnectionString == String.Empty
				)
				return string.Empty;

			SqlConnection connection = null;
			SqlCommand selectCompanyCommand = null;
			SqlDataReader loginDataReader = null;

			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();

				string selectCompany =@"SELECT MSD_Companies.Company FROM MSD_Companies
											WHERE MSD_Companies.CompanyId = " + aCompanyId;
			
				selectCompanyCommand = new SqlCommand(selectCompany, connection);

				loginDataReader = selectCompanyCommand.ExecuteReader();

				if (loginDataReader == null || !loginDataReader.Read() || loginDataReader["Company"] == System.DBNull.Value)
					return string.Empty;
				
				return (string)loginDataReader["Company"];

			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.GetCompanyName: " + exception.Message);
				return string.Empty;
			}
			finally
			{
				if (loginDataReader != null && !loginDataReader.IsClosed)
					loginDataReader.Close();

				if (selectCompanyCommand != null)
					selectCompanyCommand.Dispose();

				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		
		//-------------------------------------------------------------------------------------------
		public string GetUserName(int userId)
		{
			if 
				(
				consoleDBConnectionString == null || 
				consoleDBConnectionString == String.Empty
				)
				return string.Empty;

			SqlConnection connection = null;
			SqlCommand selectUserCommand = null;
			SqlDataReader loginDataReader = null;

			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();
				string selectUser =@"SELECT MSD_Logins.Login FROM MSD_Logins
											WHERE MSD_Logins.LoginId = " + userId;
				
				selectUserCommand = new SqlCommand(selectUser, connection);
				
				loginDataReader = selectUserCommand.ExecuteReader();
				if (loginDataReader == null || !loginDataReader.Read() || loginDataReader["Login"] == System.DBNull.Value)
					return string.Empty;

				return (string)loginDataReader["Login"];

			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.GetUserName: " + exception.Message);
				return string.Empty;
			}
			finally
			{
				if (loginDataReader != null && !loginDataReader.IsClosed)
					loginDataReader.Close();
				
				if (selectUserCommand != null)
					selectUserCommand.Dispose();
				
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool FillSettingsFromDB()
		{
			if 
				(
				companyId == -1 ||
				consoleDBConnectionString == null || 
				consoleDBConnectionString == String.Empty
				)
				return false;

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();
				
				bool settingsFound = FillUserSettingsFromDB(connection, loginId);

				// Se non sto cercando le impostazioni relative a AllUsers e 
				// non ho trovato impostazioni particolari per l'utente devo
				// controllare se ne esistono per AllUsers (loginId = -1)
				if (!settingsFound && loginId != -1)
					settingsFound = FillUserSettingsFromDB(connection, -1);

				return settingsFound;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.FillSettingsFromDB: " + exception.Message);
				return false;
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool FillUserSettingsFromDB(SqlConnection connection, int aLoginId)
		{
			inheritedFromAllUsers = false;
			
			if 
				(
				companyId == -1 ||
				connection == null || 
				(connection.State & ConnectionState.Open) != ConnectionState.Open
				)
				return false;

			SqlCommand selectSqlCommand = null;
			SqlDataReader settingsDataReader = null;
			try
			{
				string query = "SELECT * FROM " + EasyLookCustomSettingsTableName; 
				query += " WHERE " + CompanyIdColumnName + " = " + companyId.ToString();
				query += " AND " + LoginIdColumnName + " = " + aLoginId.ToString();

				selectSqlCommand = new SqlCommand(query, connection);
		
				settingsDataReader  = selectSqlCommand.ExecuteReader();

				if (settingsDataReader == null || !settingsDataReader.Read())
				{
					if (settingsDataReader != null && !settingsDataReader.IsClosed)
						settingsDataReader.Close();

					return false;
				}
				
				FillFromDataReader(settingsDataReader);

				inheritedFromAllUsers = (loginId != -1 && aLoginId == -1);
				
				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.FillUserSettingsFromDB: " + exception.Message);
				return false;
			}
			finally
			{
				if (settingsDataReader != null && !settingsDataReader.IsClosed)
					settingsDataReader.Close();

				if (selectSqlCommand != null)
					selectSqlCommand.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool FillUserSettingsFromDB(int aLoginId)
		{
			if 
				(
				companyId == -1 ||
				consoleDBConnectionString == null || 
				consoleDBConnectionString == String.Empty
				)
				return false;

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(consoleDBConnectionString);
				connection.Open();
				
				return FillUserSettingsFromDB(connection, aLoginId);
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.FillUserSettingsFromDB: " + exception.Message);
				return false;
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void FillFromDataReader(SqlDataReader aSettingsDataReader)
		{
			if (aSettingsDataReader == null || aSettingsDataReader.IsClosed)
			{
				Debug.Fail("EasyLookCustomSettings.FillFromDataReader Error: invalid SqlDataReader.");
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.InvalidSqlDataReaderErrMsg);
			}

			try
			{
				logoImageURL					= (aSettingsDataReader[LogoImageURLColumnName] != System.DBNull.Value) ? ((string)aSettingsDataReader[LogoImageURLColumnName]).Substring(((string)aSettingsDataReader[LogoImageURLColumnName]).LastIndexOf("\\") + 1) : DefaultLogoImageURL;
				appPanelBkgndColor				= (aSettingsDataReader[AppPanelBkgndColorColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[AppPanelBkgndColorColumnName] : DefaultAppPanelBkgndColor.ToArgb();
				isGroupsPanelBkgndColorDefined	= (aSettingsDataReader[GroupsPanelBkgndColorColumnName] != System.DBNull.Value);
				groupsPanelBkgndColor			= (aSettingsDataReader[GroupsPanelBkgndColorColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[GroupsPanelBkgndColorColumnName] : DefaultGroupsPanelBkgndColor.ToArgb();
				groupsPanelBkgndImageURL		= (aSettingsDataReader[GroupsPanelBkgndImageURLColumnName] != System.DBNull.Value) ? ((string)aSettingsDataReader[GroupsPanelBkgndImageURLColumnName]).Substring(((string)aSettingsDataReader[GroupsPanelBkgndImageURLColumnName]).LastIndexOf("\\") + 1) : DefaultGroupsPanelBkgndImageURL;
				menuTreeBkgndColor				= (aSettingsDataReader[MenuTreeBkgndColorColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[MenuTreeBkgndColorColumnName] : DefaultMenuTreeBkgndColor.ToArgb();
				commandListBkgndColor			= (aSettingsDataReader[CommandListBkgndColorColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[CommandListBkgndColorColumnName] : DefaultCommandListBkgndColor.ToArgb();
				fontFamily						= (aSettingsDataReader[FontFamilyColumnName] != System.DBNull.Value) ? (string)aSettingsDataReader[FontFamilyColumnName] : DefaultFontFamily;
				allUsersReportTitleColor		= (aSettingsDataReader[AllUsersReportTitleColorColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[AllUsersReportTitleColorColumnName] : DefaultAllUsersReportTitleColor.ToArgb();
				currentUserReportTitleColor		= (aSettingsDataReader[CurrentUserReportTitleColorColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[CurrentUserReportTitleColorColumnName] : DefaultCurrentUserReportTitleColor.ToArgb();
				
				maxWrmHistoryNum				= (aSettingsDataReader[MaxWrmHistoryNumColumnName] != System.DBNull.Value) ? (int)aSettingsDataReader[MaxWrmHistoryNumColumnName] : DefaultMaxWrmHistoryNumber;
				wrmHistoryAutoDelEnabled		= (aSettingsDataReader[WrmHistoryAutoDelEnabledColumnName] != System.DBNull.Value) ? (bool)aSettingsDataReader[WrmHistoryAutoDelEnabledColumnName] : false;
				wrmHistoryAutoDelType			= (aSettingsDataReader[WrmHistoryAutoDelTypeColumnName] != System.DBNull.Value) ? (WrmHistoryAutoDelTypeEnum)(Convert.ToUInt32((int)aSettingsDataReader[WrmHistoryAutoDelTypeColumnName])) : WrmHistoryAutoDelTypeEnum.Undefined;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.FillFromDataReader: " + exception.Message);
				throw new EasyLookCustomSettingsException(exception.Message);
			}
		}
		
		#endregion

		#region EasyLookCustomSettings public static methods
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool Exists(string aDBConnectionString, int aCompanyId, int aLoginId)
		{
			if (aDBConnectionString == null || aDBConnectionString == string.Empty)
			{
				Debug.Fail("EasyLookCustomSettings.Exists Error: invalid connection.");
				return false;
			}

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(aDBConnectionString);
				connection.Open();

				return Exists(connection, aCompanyId, aLoginId);
				
			}
			catch(EasyLookCustomSettingsException exception)
			{
				throw exception;
			}
			catch(SqlException exception)
			{
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.GenericExceptionMsg, exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool Exists(SqlConnection aDBConnection, int aCompanyId, int aLoginId)
		{
			if (aDBConnection == null)
			{
				Debug.Fail("EasyLookCustomSettings.Exists Error: invalid connection.");
				return false;
			}

			if (aCompanyId == -1)
				return false;

			int recordsCount = 0;

			SqlCommand selectCommand = null;
			try
			{
				string query = "SELECT COUNT(*) FROM " + EasyLookCustomSettingsTableName;
				query += " WHERE " + CompanyIdColumnName + " = " + aCompanyId.ToString();
				query += " AND " + LoginIdColumnName + " = " + aLoginId.ToString();

				selectCommand = new SqlCommand(query, aDBConnection);
					
				recordsCount = (int)selectCommand.ExecuteScalar();
			}
			catch(SqlException exception)
			{
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.GenericExceptionMsg, exception);
			}
			finally
			{
				if (selectCommand != null)
					selectCommand.Dispose();
			}
			return recordsCount > 0;
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool ExistsCompanyLogin(SqlConnection aDBConnection, int aCompanyId, int aLoginId)
		{
			if (aDBConnection == null)
			{
				Debug.Fail("EasyLookCustomSettings.ExistsCompanyLogin Error: invalid connection.");
				return false;
			}

			if (aCompanyId == -1)
				return false;

			int recordsCount = 0;

			SqlCommand selectCommand = null;
			try
			{
				string query = "SELECT COUNT(*) FROM ";
				string selectLoginData = String.Empty;

				if (aLoginId == -1)
					query += "MSD_Companies WHERE MSD_Companies.CompanyId = " + aCompanyId.ToString();
				else
					query += "MSD_CompanyLogins WHERE MSD_CompanyLogins.CompanyId = " + aCompanyId.ToString() + " AND MSD_CompanyLogins.LoginId = " + aLoginId.ToString();		

				selectCommand = new SqlCommand(query, aDBConnection);
					
				recordsCount = (int)selectCommand.ExecuteScalar();
			}
			catch(SqlException exception)
			{
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.GenericExceptionMsg, exception);
			}
			finally
			{
				if (selectCommand != null)
					selectCommand.Dispose();
			}
			return recordsCount > 0;
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public static Uri GetValidURI(string aURI)
		{
			try
			{
				// A URI (Uniform Resource Identifier) is an address string referring to an object, typically 
				// on the Internet. The most common type of URI is the URL, in which the address maps onto an 
				// access algorithm using network protocols. Sometimes URI and URL are used interchangeably.
				// A Uniform Resource Name (URN) is also a URI and identifies a property or resource using a 
				// globally unique name. 


				// The Uri class constructor creates a Uri instance from a URI string. 
				// It parses the URI, puts it in canonical format, and makes any required escape encodings.
				Uri uri = new Uri(aURI);

				return uri;
			}
			catch(UriFormatException)
			{
				return null;
			}
		}

		//-------------------------------------------------------------------------------------------
		public static bool GetLoginDataFromIds(string aDBConnectionString, int aCompanyId, int aLoginId, out string loginName, out string companyName)
		{
			companyName	= String.Empty;
			loginName	= String.Empty;
			
			if (aDBConnectionString == null || aDBConnectionString == String.Empty)
			{
				Debug.Fail("EasyLookCustomSettings.GetLoginDataFromIds Error: empty connection string.");
				return false;
			}
			
			if (aCompanyId == -1)
				return false;

			SqlConnection	connection = null;

			try
			{
				connection = new SqlConnection(aDBConnectionString);
				connection.Open();

				return GetLoginDataFromIds(connection, aCompanyId, aLoginId, out loginName, out companyName);
			}
			catch(SqlException exception)
			{
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.GenericExceptionMsg, exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		public static bool GetLoginDataFromIds(SqlConnection connection, int aCompanyId, int aLoginId, out string loginName, out string companyName)
		{
			companyName	= String.Empty;
			loginName	= String.Empty;
			
			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("EasyLookCustomSettings.GetLoginDataFromIds Error: invalid connection.");
				return false;
			}
			
			if (aCompanyId == -1)
				return false;

			SqlDataReader	loginDataReader = null;
			SqlCommand		selectCommand = null;

			try
			{
				string selectLoginData = String.Empty;

				if (aLoginId == -1)
					selectLoginData = @"SELECT MSD_Companies.Company FROM MSD_Companies	WHERE MSD_Companies.CompanyId =" + aCompanyId.ToString();
				else
					selectLoginData = @"SELECT MSD_Companies.Company, MSD_Logins.Login FROM MSD_Companies, MSD_Logins
												WHERE MSD_Companies.CompanyId =" + aCompanyId.ToString() + " AND MSD_Logins.LoginId =" + aLoginId.ToString();
			
				selectCommand = new SqlCommand(selectLoginData, connection);
				loginDataReader = selectCommand.ExecuteReader();

				if (!loginDataReader.Read())
					return false;

				companyName = loginDataReader["Company"].ToString();

				loginName = (aLoginId == -1) ? NameSolverStrings.AllUsers : loginDataReader["Login"].ToString();

				return true;
			}
			catch(SqlException exception)
			{
				throw new EasyLookCustomSettingsException(EasyLookCustomSettingsStrings.GenericExceptionMsg, exception);
			}
			finally
			{
				if (selectCommand != null)
					selectCommand.Dispose();
		
				if (loginDataReader != null && !loginDataReader.IsClosed)
					loginDataReader.Close();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool DeleteAllCompanySettings(string aDBConnectionString, int aCompanyId)
		{
			if (aDBConnectionString == null || aDBConnectionString == String.Empty)
			{
				Debug.Fail("EasyLookCustomSettings.DeleteAllCompanySettings Error: empty connection string.");
				return false;
			}
		
			SqlConnection connection = null;
			SqlTransaction  deleteSqlTransaction = null;
			SqlCommand deleteCommand = null;

			try
			{
				connection = new SqlConnection(aDBConnectionString);
				connection.Open();

				string deleteQueryText = "DELETE FROM ";
				deleteQueryText += EasyLookCustomSettingsTableName;
				deleteQueryText += " WHERE " + CompanyIdColumnName + " = " + aCompanyId.ToString();

				deleteCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				deleteCommand.Connection = connection;
				deleteCommand.Transaction = deleteSqlTransaction;

				deleteCommand.ExecuteNonQuery();
			
				deleteSqlTransaction.Commit();

				return true;
			}
			catch (Exception exception)
			{
				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Rollback();

				Debug.Fail("Exception raised in EasyLookCustomSettings.DeleteAllCompanySettings: " + exception.Message);
				return false;
			}
			finally
			{
				if (deleteCommand != null)
					deleteCommand.Dispose();

				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Dispose();

				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}				
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool DeleteAllCompanyUserSettings(SqlConnection connection, int aLoginId, int aCompanyId)
		{
			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("EasyLookCustomSettings.DeleteAllCompanyUserSettings Error: invalid connection.");
				return false;
			}
			
			SqlTransaction  deleteSqlTransaction = null;
			SqlCommand		deleteCommand = null;

			try
			{
				string deleteQueryText = "DELETE FROM ";
				deleteQueryText += EasyLookCustomSettingsTableName;
				deleteQueryText += " WHERE " + CompanyIdColumnName + " = " + aCompanyId.ToString();
				deleteQueryText += " AND " + LoginIdColumnName + " = " + aLoginId.ToString();

				deleteCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				deleteCommand.Connection = connection;
				deleteCommand.Transaction = deleteSqlTransaction;

				deleteCommand.ExecuteNonQuery();
				
				deleteSqlTransaction.Commit();
	
				return true;
			}
			catch (Exception exception)
			{
				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Rollback();

				Debug.Fail("Exception raised in EasyLookCustomSettings.DeleteAllCompanyUserSettings: " + exception.Message);
				return false;
			}
			finally
			{
				if (deleteCommand != null)
					deleteCommand.Dispose();

				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool DeleteAllCompanyUserSettings(string aDBConnectionString, int aLoginId, int aCompanyId)
		{
			if (aDBConnectionString == null || aDBConnectionString == String.Empty)
			{
				Debug.Fail("EasyLookCustomSettings.DeleteAllCompanyUserSettings Error: empty connection string.");
				return false;
			}
			
			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(aDBConnectionString);
				connection.Open();

				return DeleteAllCompanyUserSettings(connection, aLoginId, aCompanyId);
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in EasyLookCustomSettings.DeleteAllCompanyUserSettings: " + exception.Message);
				return false;
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}				
			}
		}

		#endregion
	}
}
