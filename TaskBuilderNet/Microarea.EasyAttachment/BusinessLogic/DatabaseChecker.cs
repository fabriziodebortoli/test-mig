using System.IO;
using System.Data.SqlClient;
using System.Linq;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;

namespace Microarea.EasyAttachment.BusinessLogic
{
	//================================================================================
	public class DMSChecker
	{
		public enum DMSStatus { Valid, StorageInvalid, DBInvalid }

		//------------------------------------------------------------------------------------------
		public static DMSStatus CheckDMSStatus(string connectionString, int dbRelease, int workerId, out string msg)
		{
			DMSModelDataContext dc = new DMSModelDataContext(connectionString);

			msg = string.Empty;

			try
			{
				// check esistenza db
				if (!dc.DatabaseExists())
					msg = Strings.EADatabaseNotExistError;
				else
				{
					var dmsModule = from mod in dc.TB_DBMarks
									where mod.Application == "TBExtensions" && mod.AddOnModule == "DMS"
									select mod;

					if (dmsModule != null && dmsModule.Any())
					{
						TB_DBMark dbMark = (TB_DBMark)dmsModule.Single();

						// check numero di release di modulo
						if (dbMark.DBRelease != dbRelease)
							msg = string.Format(Strings.EADatabaseWrongRelError, dbRelease, dbMark.DBRelease);
						else
						{
							// check stato modulo
							if (dbMark.Status != '1')
								msg = Strings.EADatabaseWrongStatus;
							else
							{
								// check path storage vuoto
								bool check = CheckBinaryStorage(dc, workerId, out msg);
								return (check) ? DMSStatus.Valid : DMSStatus.StorageInvalid;
							}
						}
					}
				}
			}
			catch (SqlException sqlExc)
			{
				// l'errore 208 viene scatenato quando la tabella coinvolta nella query non esiste sul database
				if (sqlExc.Number == 208)
					msg = Strings.EATBDBMarkNotExistError;
				else
					msg = sqlExc.Message;
			}
			finally
			{
				if (dc.Connection.State != System.Data.ConnectionState.Closed)
				{
					dc.Connection.Close();
					dc.Connection.Dispose();
				}
			}

			return DMSStatus.DBInvalid;
		}

		//------------------------------------------------------------------------------------------
		public static bool CheckBinaryStorage(DMSModelDataContext dc, int workerId, out string msg)
		{
			msg = string.Empty;

			try
			{
				SettingsManager settingsManager = new SettingsManager(dc, workerId);

				if (settingsManager.UsersSettingState.Options.StorageOptionsState.StorageToFileSystem)
				{
					string storagePath = settingsManager.UsersSettingState.Options.StorageOptionsState.StorageFolderPath;
					DirectoryInfo di = new DirectoryInfo(settingsManager.UsersSettingState.Options.StorageOptionsState.StorageFolderPath);
					if (!di.Exists)
					{
						msg = string.Format(Strings.EAStorageFolderPathNotExistError, storagePath);
						return false;
					}
				}
			}
			catch (SqlException sqlExc)
			{
				throw (sqlExc);
			}

			return true;
		}

		/// <summary>
		/// Check presenza di righe nella tabella DMS_ErpDocument con la colonna TBGuid contenente valori
		/// nulli o vuoti ('00000000-0000-0000-0000-000000000000')
		/// </summary>
		//------------------------------------------------------------------------------------------
		public static bool ExistEmptyTBGuidValuesInERPDocument(string connectionString)
		{
			DMSModelDataContext dc = new DMSModelDataContext(connectionString);

			try
			{
				var emptyDocs = (from erpDoc in dc.DMS_ErpDocuments
								 where erpDoc.TBGuid == null || erpDoc.TBGuid.Equals(System.Guid.Empty) || erpDoc.TBGuid.Equals("00000000-0000-0000-0008-000000000000")
								 // todo check guid con 0008!!!
								 select erpDoc).Count();

				return ((int)emptyDocs > 0);
			}
			catch (SqlException)
			{
				return false;
			}
			finally
			{
				if (dc.Connection.State != System.Data.ConnectionState.Closed)
				{
					dc.Connection.Close();
					dc.Connection.Dispose();
				}
			}
		}
	}
}