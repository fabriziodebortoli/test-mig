using System;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.Console.Core.RegressionTestLibrary;
using Microarea.Console.Plugin.ApplicationDBAdmin.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ApplicationDBAdmin
{
	/// <summary>
	/// Summary description for RegressionTestEngine.
	/// </summary>
	//=========================================================================
	public class RegressionTestEngine
	{
		private DatabaseManager dbManager = null;
		private BrandLoader brandLoader = null;
		private string companyId = string.Empty;
		private RegressionTestSelections selezioni = null;
		private Diagnostic regressionDiagnostic = new Diagnostic("RegressionTestEngine");
		private string tempPath = "C:\\temp";
		private BasePathFinder regressionTestPathFinder = null;
		private StringCollection appList = new StringCollection();
		private ContextInfo contextInfo = null;

		private CompanyItem companyInfo = new CompanyItem();
		private NRTElaborationForm elabForm = null;
		private DatabaseTask dbTask = new DatabaseTask();

		// Events
		//---------------------------------------------------------------------
		public delegate bool IsUserAuthenticated(string login, string password, string serverName);
		public event IsUserAuthenticated OnIsUserAuthenticated;

		public delegate void AddUserAuthenticated(string login, string password, string serverName, DBMSType dbType);
		public event AddUserAuthenticated OnAddUserAuthenticated;

		public delegate string GetUserAuthenticatedPwd(string login, string serverName);
		public event GetUserAuthenticatedPwd OnGetUserAuthenticatedPwd;

		public delegate bool GetConnectionInfo(string companyId, CompanyItem companyItem, Diagnostic diagnostic);
		public event GetConnectionInfo OnGetConnectionInfo;

		/// <summary>
		/// Constructor
		/// </summary>
		//---------------------------------------------------------------------
		public RegressionTestEngine
			(
			PathFinder pathFinder,
			Diagnostic dbAdminDiagnostic,
			ContextInfo.SystemDBConnectionInfo contextParameters,
			string companyId,
			ContextInfo context,
			BrandLoader brandLoader,
			string sysDBConnectionString,
			DBNetworkType dbNetworkType,
			string isoState
			)
		{
			dbManager = new DatabaseManager(pathFinder, dbAdminDiagnostic, brandLoader, contextParameters, dbNetworkType, isoState, true);

			dbManager.OnIsUserAuthenticatedFromConsole		+= new DatabaseManager.IsUserAuthenticatedFromConsole(IsUserAuthenticatedFromConsole);
			dbManager.OnAddUserAuthenticatedFromConsole		+= new DatabaseManager.AddUserAuthenticatedFromConsole(AddUserAuthenticatedFromConsole);
			dbManager.OnGetUserAuthenticatedPwdFromConsole	+= new DatabaseManager.GetUserAuthenticatedPwdFromConsole(GetUserAuthenticatedPwdFromConsole);

			regressionTestPathFinder = pathFinder;
			contextInfo = context;
			this.brandLoader = brandLoader;

			this.companyId = companyId;
			dbTask.CurrentStringConnection = sysDBConnectionString;
		}

		# region Eventi legati all'autenticazione dell'utente
		//---------------------------------------------------------------------
		protected bool IsUserAuthenticatedFromConsole(string login, string password, string serverName)
		{
			bool result = false;
			if (OnIsUserAuthenticated != null)
				result = OnIsUserAuthenticated(login, password, serverName);
			return result;
		}

		/// <summary>
		/// AddUserAuthenticatedFromConsole
		/// </summary>
		//---------------------------------------------------------------------
		protected void AddUserAuthenticatedFromConsole(string login, string password, string serverName, DBMSType dbType)
		{
			OnAddUserAuthenticated?.Invoke(login, password, serverName, dbType);
		}

		/// <summary>
		/// GetUserAuthenticatedPwdFromConsole
		/// </summary>
		//---------------------------------------------------------------------
		protected string GetUserAuthenticatedPwdFromConsole(string login, string serverName)
		{
			string password = string.Empty;
			if (OnGetUserAuthenticatedPwd != null)
				password = OnGetUserAuthenticatedPwd(login, serverName);
			return password;
		}
		# endregion

		/// <summary>
		/// entry-point per richiamare la funzione di elaborazione del database 
		/// (effettua il restore del database, fa il check le opportune elaborazioni
		/// </summary>
		//---------------------------------------------------------------------
		public bool Go(RegressionTestSelections sel)
		{
			bool bOk = true;

			if (OnGetConnectionInfo != null)
				bOk = OnGetConnectionInfo(companyId, companyInfo, regressionDiagnostic);

			if (!bOk)
				return bOk;

			selezioni = sel;

			if (!Directory.Exists(tempPath))
				Directory.CreateDirectory(tempPath);

			elabForm = new NRTElaborationForm(this);
			elabForm.ShowDialog();

			return bOk;
		}

		//---------------------------------------------------------------------
		public void ThreadExecution()
		{
			foreach (AreaItem aItem in selezioni.AreaItems.Values)
			{
				elabForm.SetUnit(aItem.Name);

				foreach (DataSetItem dItem in aItem.DataSetItems.Values)
					Execute(dItem);
			}
			
			elabForm.StopExecution();
		}

		//---------------------------------------------------------------------
		private bool Execute(DataSetItem item)
		{
			bool bOk = false;

			elabForm.SetDataSet(item.Name);

			// 1. Unzip del file
			elabForm.SetStartOperation(NRTElaborationForm.Step.Unzip);
			bOk = ExpandZip(item);
			elabForm.SetEndOperation(NRTElaborationForm.Step.Unzip, bOk);

			if (!bOk)
				return false;

			// 2. Restore del db
			elabForm.SetStartOperation(NRTElaborationForm.Step.RestoreDb);
			bOk = RestoreDB();
			elabForm.SetEndOperation(NRTElaborationForm.Step.RestoreDb, bOk);

			if (!bOk)
				return false;

			// 3. connessione al database aziendale e check della sua struttura
			elabForm.SetStartOperation(NRTElaborationForm.Step.CheckDb);
			if (dbManager.ConnectAndCheckDBStructure(companyId))
			{
				elabForm.SetEndOperation(NRTElaborationForm.Step.CheckDb, true);

				// 4. effettua operazioni sul db (la chiusura della connessione avviene già all'interno del metodo)
				elabForm.SetStartOperation(NRTElaborationForm.Step.UpdateDb);
				bOk = dbManager.DatabaseManagement();
				elabForm.SetEndOperation(NRTElaborationForm.Step.UpdateDb, bOk);

				// 5. Esecuzione dello script di Extra Update
				elabForm.SetStartOperation(NRTElaborationForm.Step.ExtraUpdateDb);
				bOk = bOk && ExtraUpdate();
				elabForm.SetEndOperation(NRTElaborationForm.Step.ExtraUpdateDb, bOk);
			}
			else
			{
				elabForm.SetEndOperation(NRTElaborationForm.Step.CheckDb, false);
				return false;
			}

			if (!bOk)
				return false;

			// 6. backup del db
			elabForm.SetStartOperation(NRTElaborationForm.Step.BackupDb);
			bOk = BackupDB();
			elabForm.SetEndOperation(NRTElaborationForm.Step.BackupDb, bOk);

			if (!bOk)
				return false;

			// 7. export del db
			elabForm.SetStartOperation(NRTElaborationForm.Step.ExportDb);
			bOk = ExportDB(item);
			elabForm.SetEndOperation(NRTElaborationForm.Step.ExportDb, bOk);

			if (!bOk)
				return false;

			// 8. zip del file
			elabForm.SetStartOperation(NRTElaborationForm.Step.Zip);
			bOk = CompressZip(item);
			elabForm.SetEndOperation(NRTElaborationForm.Step.Zip, bOk);

			return bOk;
		}

		//---------------------------------------------------------------------
		private bool ExpandZip(DataSetItem item)
		{
			bool bOk = false;
			string zipFileName = string.Format("{0}.xml.zip", item.Path);

			if (File.Exists(zipFileName))
			{
				Directory.CreateDirectory(item.Path);

				Process myProcess = new Process();

				// VECCHIA GESTIONE WINZIP 	        
				// string commandLine = string.Format("{0}\\wzunzip", selezioni.WinZipPath);
				// string commandArgs = string.Format("-d -o \"{0}\" \"{1}\"", zipFileName, item.Path);

				// GESTIONE 7-ZIP
				string commandLine = string.Format("{0}\\7z", selezioni.WinZipPath);
				string commandArgs = string.Format("x -aoa \"{0}\" -o\"{1}\"", zipFileName, item.Path);

				try
				{
					myProcess.StartInfo.FileName = commandLine;
					myProcess.StartInfo.Arguments = commandArgs;
					myProcess.StartInfo.CreateNoWindow = true;
					myProcess.StartInfo.UseShellExecute = false;
					bOk = myProcess.Start();
					myProcess.WaitForExit();
				}
				catch (InvalidOperationException)
				{
					return false;
				}
			}

			try
			{
				File.Copy(Path.Combine(item.Path, string.Format("{0}.bak", item.Name)), Path.Combine(tempPath, "backup"), true);
				return bOk;
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
		public bool CompressZip(DataSetItem item)
		{
			bool bOk = false;

			if (Directory.Exists(item.Path))
			{
				// Copio il file backup nella cartella del set di dati
				try
				{
					File.Copy(Path.Combine(tempPath, "backup"), Path.Combine(item.Path, string.Format("{0}.bak", item.Name)), true);
				}
				catch
				{
					return false;
				}

				string zipFileName = string.Format("{0}.xml.zip", item.Path);
				string oldZipFileName = string.Format("{0}.xml.zip.old", item.Path);
				Process myProcess = new Process();

				try
				{
					if (File.Exists(oldZipFileName))
						File.Delete(oldZipFileName);

					if (File.Exists(zipFileName))
						File.Move(zipFileName, oldZipFileName);
				}
				catch
				{ }

				// VECCHIA GESTIONE WINZIP 
				// string commandLine = string.Format("{0}\\wzzip", selezioni.WinZipPath);
				// string commandArgs = string.Format("-p \"{0}\" \"{1}\\*.*\"", zipFileName, item.Path);

				// GESTIONE 7-ZIP
				string commandLine = string.Format("{0}\\7z", selezioni.WinZipPath);
				string commandArgs = string.Format("a -r \"{0}\" \"{1}\\*.*\"", zipFileName, item.Path);

				try
				{
					myProcess.StartInfo.FileName = commandLine;
					myProcess.StartInfo.Arguments = commandArgs;
					myProcess.StartInfo.CreateNoWindow = true;
					myProcess.StartInfo.UseShellExecute = false;
					bOk = myProcess.Start();
					myProcess.WaitForExit();
				}
				catch
				{
					return false;
				}

				// Cancello la cartella del set di dati e ed il backup dalla cartella temp
				try
				{
					Directory.Delete(item.Path, true);
					File.Delete(Path.Combine(tempPath, "backup"));

					return bOk;
				}
				catch
				{
					File.Move(oldZipFileName, zipFileName);
					return false;
				}
			}
			else
				return false;
		}

		//---------------------------------------------------------------------
		public bool RestoreDB()
		{
			string timeString = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() +
								DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();

			string bakPath = Path.Combine(tempPath, "backup");

			DataTable myDataTable = dbTask.LoadFileListOnly(bakPath);
			if (myDataTable == null)
			{
				elabForm.ShowDiagnostic(dbTask.Diagnostic);
				return false;
			}

			SQLRestoreDBParameters restoreParams = new SQLRestoreDBParameters();
			restoreParams.DatabaseName = companyInfo.DbName;
			restoreParams.ForceRestore = true;
			restoreParams.DataPhysicalName = Path.Combine(tempPath, timeString + DatabaseTaskConsts.DataFileSuffix);
			restoreParams.LogPhysicalName = Path.Combine(tempPath, timeString + DatabaseTaskConsts.LogFileSuffix);

			for (int i = 0; i < myDataTable.Rows.Count; i++)
			{
				if (i == 0)
					restoreParams.DataLogicalName = ((DataRow)(myDataTable.Rows[0]))[0].ToString();
				if (i == 1)
					restoreParams.LogLogicalName = ((DataRow)(myDataTable.Rows[1]))[0].ToString();
			}

			bool bOk = dbTask.RestoreWithMove(restoreParams);

			if (dbTask.Diagnostic.Error)
				elabForm.ShowDiagnostic(dbTask.Diagnostic);

			return bOk;
		}

		//---------------------------------------------------------------------
		public bool BackupDB()
		{
			string bakPath = Path.Combine(tempPath, "backup");
			SQLBackupDBParameters bakParams = new SQLBackupDBParameters();
			bakParams.DatabaseName = companyInfo.DbName;
			bakParams.BackupFilePath = bakPath;
			bakParams.Overwrite = true;

			bool bOk = dbTask.Backup(bakParams);

			// eseguo la verifica del backup solo se è andato a buon fine
			if (bOk)
				bOk = dbTask.VerifyBackupFile(bakPath);

			if (dbTask.Diagnostic.Error || dbTask.Diagnostic.Warning)
				elabForm.ShowDiagnostic(dbTask.Diagnostic);

			return bOk;
		}

		//---------------------------------------------------------------------
		private bool ExtraUpdate()
		{
			if (string.IsNullOrEmpty(selezioni.ExtraUpdateFilePath) || !File.Exists(selezioni.ExtraUpdateFilePath))
				return true;

			bool bOk = false;

			ScriptManager sm = new ScriptManager(false);

			try
			{
				using (TBConnection myConnection = new TBConnection(CreateConnectionString(), DBMSType.SQLSERVER))
				{
					myConnection.Open();

					string errors;
					sm.Connection = myConnection;
					bOk = sm.ExecuteFileSql(selezioni.ExtraUpdateFilePath, out errors);
				}
			}
			catch
			{
				elabForm.ShowDiagnostic(sm.Diagnostic);
			}

			return bOk;
		}

		//---------------------------------------------------------------------
		private string CreateConnectionString()
		{
			return (companyInfo.DBAuthenticationWindows)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyInfo.DbServer, companyInfo.DbName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, companyInfo.DbServer, companyInfo.DbName, companyInfo.DbDefaultUser, companyInfo.DbDefaultPassword);
		}

		//---------------------------------------------------------------------
		public bool ExportDB(DataSetItem item)
		{
			if (!CleanFolder(item))
				return false;

			try
			{
				using (TBConnection myConnection = new TBConnection(CreateConnectionString(), DBMSType.SQLSERVER))
				{
					ApplicationDBStructureInfo structureInfo = new ApplicationDBStructureInfo(regressionTestPathFinder, brandLoader);
					CatalogInfo catalogInfo = new CatalogInfo();

					catalogInfo.Load(myConnection, true);

					GetAllApplication();

					structureInfo.AddModuleInfoToCatalog(appList, ref catalogInfo);

					foreach (CatalogTableEntry entry in catalogInfo.TblDBList)
					{
						DataSet dataSet = new DataSet();

						string query = string.Format("SELECT * FROM [{0}] FOR XML AUTO, XMLDATA", entry.TableName);

						if (!ExportSqlTable(query, ref dataSet, myConnection))
							return false;

						dataSet.DataSetName = "DataTables";

						int rows = 0;
						foreach (DataTable myTable in dataSet.Tables)
							rows += myTable.Rows.Count;

						if (rows == 0)
							continue;

						string fileName = item.Path + Path.DirectorySeparatorChar + entry.TableName + NameSolverStrings.XmlExtension;

						try
						{
							dataSet.WriteXml(fileName, XmlWriteMode.IgnoreSchema);
						}
						catch
						{
							return false;
						}
					}
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		private bool ExportSqlTable(string query, ref DataSet dataSet, TBConnection connection)
		{
			TBCommand command = new TBCommand(query, connection);
			command.CommandTimeout = 0;
            XmlReader xmlReader = null;

			if (dataSet == null)
				return false;

			try
			{
                xmlReader = command.ExecuteXmlReader();

				dataSet.ReadXml(xmlReader, XmlReadMode.Fragment);
				xmlReader.Close();
				return true;
			}
			catch (Exception e)
			{
                Debug.WriteLine(e.Message);
				// se entra nel catch e l'XmlTextReader è in uno stato diverso da chiuso forzo la sua chiusura.
				if (xmlReader != null)
					if (xmlReader.ReadState != ReadState.Closed)
						xmlReader.Close();

				return false;
			}
		}

		//---------------------------------------------------------------------
		private bool CleanFolder(DataSetItem item)
		{
			try
			{
				DirectoryInfo dInfo = new DirectoryInfo(item.Path);

				foreach (FileInfo fInfo in dInfo.GetFiles("*.xml"))
					fInfo.Delete();
				return true;
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------------
		private void GetAllApplication()
		{
			appList.Clear();
			StringCollection supportList = new StringCollection();

			// prima guardo TaskBuilder
			regressionTestPathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
			foreach (string appName in supportList)
				appList.Add(appName);

			// poi guardo le TaskBuilderApplications
			regressionTestPathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
			for (int i = 0; i < supportList.Count; i++)
				appList.Add(supportList[i]);
		}
	}
}
