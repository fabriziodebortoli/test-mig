using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Microarea.EasyAttachment.Core
{
	//================================================================================
	public class FullTextInfoEventArgs : EventArgs
	{
		private string message = string.Empty;
		private bool processResult = true;
			
		public string Message		{ get { return message; } set { message = value; } }
		public bool   ProcessResult { get { return processResult; } set { processResult = value; } }
	}

    //================================================================================
    public class FullTextManager
    {
		private DMSModelDataContext dc = null;

		private string tempPath = string.Empty;
		private int lcid = 0;
    
		//-------------------------------------------------------------------------------
        public event EventHandler<FullTextInfoEventArgs> UpdateTextContentCompleted;

        //-------------------------------------------------------------------------------
		public FullTextManager(DMSModelDataContext dataContext, string path, int lcid)
		{
			dc = dataContext;
			tempPath = path;
			this.lcid = lcid;
		}
		
		//---------------------------------------------------------------------
		internal static byte[] GetBinaryContent(SqlConnection connection, int archivedDocId, ref string fileName, ref string ext, ref bool veryLargeFile)
		{
			byte[] contentArray = null;
			fileName = string.Empty;
			ext = string.Empty;

			try
			{		
				using (SqlCommand myCommand = connection.CreateCommand())
				{
					myCommand.CommandText = @"SELECT BinaryContent, Name, X.ExtensionType 
											FROM DMS_ArchivedDocContent X, DMS_ArchivedDocument Y 
											WHERE X.ArchivedDocID = @archivedDocID AND X.ArchivedDocID = Y.ArchivedDocID";

					SqlParameter archDocIDParam = new SqlParameter("@archivedDocID", SqlDbType.Int);
					archDocIDParam.Value = Convert.ToInt32(archivedDocId);
					myCommand.Parameters.Add(archDocIDParam);
					using (SqlDataReader reader = myCommand.ExecuteReader())
					{
						if (reader != null && reader.Read())
						{
							contentArray = (byte[])reader["BinaryContent"];
							fileName = reader["Name"].ToString();
							ext = reader["ExtensionType"].ToString();
						}
					}
				}
			}
			catch (SqlException e)
			{
				throw(e);
				//SetMessage(Strings.ErrorLoadingArchivedDoc, e, "GetBinaryContent");
			}
			catch (OutOfMemoryException)
			{
				// nel caso di eccezione OutOfMemoryException tento nuovamente di leggere
				// il documento dividendolo prima in piccoli pezzi
				contentArray = null;
				veryLargeFile = true;
			}

			return contentArray;
		}

		//---------------------------------------------------------------------
		public static string GetTemporaryFileName(int archivedDocId, SqlConnection connection, string tempPath, out bool veryLargeFile)
		{
			veryLargeFile = false;

			if (archivedDocId < 1)
				return string.Empty;

			byte[] contentArray = null;
			string fileName = string.Empty;
			string ext = string.Empty;

			try
			{
				contentArray = GetBinaryContent(connection, archivedDocId, ref fileName, ref ext, ref veryLargeFile);
				//per i file di grosse dimensioni non posso eseguire il processo di OCR
				if (contentArray == null && veryLargeFile)
					return string.Empty;

				if (contentArray == null || contentArray.Length == 0)
					return string.Empty;

				string tempFileName = String.Format(@"{0}\{1}_({2}){3}", tempPath, Path.GetFileNameWithoutExtension(fileName), archivedDocId.ToString(), ext);
				
				FileInfo f = new FileInfo(tempFileName);
				if (f.Exists)
					f.IsReadOnly = false;

				using (FileStream s = new FileStream(tempFileName, FileMode.OpenOrCreate))
					s.Write(contentArray, 0, contentArray.Length);

				contentArray = null;

				return tempFileName;
			}
			catch (Exception exc) 
			{
				throw (exc);
			}
		}

		//---------------------------------------------------------------------
		private static MemoryStream GetStream(int archivedDocId, SqlConnection connection, ref string fileName)
		{
			if (archivedDocId < 1)
				return null;

			byte[] contentArray = null;
			bool veryLargeFile = false;
			string ext = string.Empty;

			try
			{
				contentArray = GetBinaryContent(connection, archivedDocId, ref fileName, ref ext, ref veryLargeFile);
				//per i file di grosse dimensioni non posso eseguire il processo di OCR
				if (contentArray == null && veryLargeFile)
					return null;

				if (contentArray == null || contentArray.Length == 0)
					return null;

				MemoryStream s = new MemoryStream(contentArray);		
				contentArray = null;
				return s;
			}
			catch (Exception exc)
			{
				throw (exc);
			}
		}

		//---------------------------------------------------------------------
		internal void UpdateTextContent(DMSModelDataContext dataContext, List<int> archivedDocIds, string connectionString)
		{
			string ocrResultFile = string.Empty;		
			
			try
			{
				CoreOCRManager coreOCRMng = null;
				coreOCRMng = new CoreOCRManager(dataContext, tempPath, OCRDictionaryHelper.GetOCRDictionaryFromLCID(lcid));

				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					foreach (int archivedDocId in archivedDocIds)
					{
						bool existingArchivedDocId = false;
						bool isVeryLargeFile = false;
						string fileName = GetTemporaryFileName(archivedDocId, connection, tempPath, out isVeryLargeFile);

						string fName = string.Empty;
						using (MemoryStream myStream = GetStream(archivedDocId, connection, ref fName))
						{
							// eseguo l'OCR del file solo se si tratta di un file immagine o di un pdf
							if (CoreUtils.HasImageFormat(myStream.ToArray()) || FileExtensions.IsPdfPath(fileName))
								ocrResultFile = coreOCRMng.OCRDocumentContent(fileName);
						}

						using (IDbCommand myCommand = connection.CreateCommand())
						{
							myCommand.CommandText = string.Format(@"SELECT ArchivedDocID FROM DMS_ArchivedDocTextContent WHERE ArchivedDocID = {0}", archivedDocId.ToString());
							object obj = myCommand.ExecuteScalar();
							existingArchivedDocId = (obj != null);
						}

						if (string.IsNullOrWhiteSpace(ocrResultFile) || !File.Exists(ocrResultFile))
						{
							using (IDbCommand myCommand = connection.CreateCommand())
							{
								myCommand.CommandText = string.Format(@"UPDATE DMS_ArchivedDocContent SET OCRProcess = 1 WHERE ArchivedDocID = {0}", archivedDocId.ToString());
								myCommand.ExecuteNonQuery();
								if (existingArchivedDocId)
								{
									myCommand.CommandText = string.Format(@"DELETE FROM DMS_ArchivedDocTextContent WHERE ArchivedDocID = {0}", archivedDocId.ToString());
									myCommand.ExecuteNonQuery();
								}
							}
							File.Delete(fileName);
						}
						else
							using (IDbCommand myCommand = connection.CreateCommand())
							{
								myCommand.CommandText = (existingArchivedDocId)
									? string.Format(@"UPDATE DMS_ArchivedDocTextContent SET TextContent = @TextCont WHERE ArchivedDocID = {0}", archivedDocId.ToString())
									: string.Format(@"INSERT INTO DMS_ArchivedDocTextContent (ArchivedDocID, TextContent) VALUES ({0}, @TextCont)", archivedDocId.ToString());

								myCommand.Parameters.Add(new SqlParameter("@TextCont", File.ReadAllText(ocrResultFile)));
								myCommand.ExecuteNonQuery();

								myCommand.CommandText = string.Format(@"UPDATE DMS_ArchivedDocContent SET OCRProcess = 1 WHERE ArchivedDocID = {0}", archivedDocId.ToString());
								myCommand.ExecuteNonQuery();

								File.Delete(fileName);
								File.Delete(ocrResultFile);
							}
					}
				}
			}
			catch (SqlException ex)
			{
				throw (ex);
			}
		}

		///<summary>
		/// Metodo richiamato da EasyAttachmentSync per aggiornare il text content per il FullText Search
		///</summary>
		//---------------------------------------------------------------------
		public void CreateDocumentsContent(string connectionString, string extTypeCollation)
		{
			// al posto di connectionString prima utilizzavo dc.Connection.ConnectionString ma la stringa restituita era priva della password
			FullTextInfoEventArgs args = new FullTextInfoEventArgs();

			try
			{
				DirectoryInfo dir = new DirectoryInfo(tempPath);
				foreach (string fileName in Directory.GetFiles(tempPath))
					File.Delete(fileName);
			}
			catch (IOException ioExc)
			{
				args.ProcessResult = false;
				args.Message = "01 FullTextManager::CreateDocumentsContent exception: " + ioExc.Message;
				UpdateTextContentCompleted?.Invoke(this, args);
				return;
			}
			catch (Exception e)
			{
				args.ProcessResult = false;
				args.Message = args.Message = "02 FullTextManager::CreateDocumentsContent exception: " + e.Message;
				UpdateTextContentCompleted?.Invoke(this, args);
				return;
			}

			List<int> ids = new List<int>();
			string dbName = string.Empty;
			string dbServer = string.Empty;
			long startTime = DateTime.Now.Ticks;
			long endTime = 0;

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					dbName = connection.Database;
					dbServer = connection.DataSource;

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = connection;

						// imposto OCRProcess = 1 a tutti i file con ExtensionType presente in sys.fulltext_document_types, perche' SqlServer crea gli indici di ricerca utilizzando l'IFilter,
						// escludendo i pdf perchè dai ns. test l'IFilter Adobe per il pdf non funziona bene e siamo sicuri che con il ns. algoritmo funziona meglio
						// ed escludendo tutte le estensioni diverse da '.pdf', '.bmp', '.gif', '.jpeg', '.jpg', '.png', '.tiff', '.tif'
						myCommand.CommandText = string.Format
							(@"UPDATE DMS_ArchivedDocContent SET OCRProcess = 1 
							WHERE 
							((ExtensionType IN (SELECT document_type {0} FROM sys.fulltext_document_types) AND ExtensionType <> '.pdf') OR 
							ExtensionType NOT IN ('.pdf', '.bmp', '.gif', '.jpeg', '.jpg', '.png', '.tiff', '.tif')) AND OCRProcess = 0",
							string.IsNullOrEmpty(extTypeCollation) ? string.Empty : string.Format("COLLATE {0} ", extTypeCollation)
							);
						myCommand.ExecuteNonQuery();

						// poi seleziono tutti i file con le altre estensioni che non sono stati ancora processati (consideriamo solo i pdf e le estensioni di tipo immagine)
						myCommand.CommandText = string.Format
							(@"SELECT ArchivedDocID FROM DMS_ArchivedDocContent 
							WHERE (ExtensionType NOT IN (SELECT document_type {0} FROM sys.fulltext_document_types) OR 
							ExtensionType IN ('.pdf', '.bmp', '.gif', '.jpeg', '.jpg', '.png', '.tiff', '.tif')) AND OCRProcess = 0",
							string.IsNullOrEmpty(extTypeCollation) ? string.Empty : string.Format("COLLATE {0} ", extTypeCollation)
							);

						using (SqlDataReader reader = myCommand.ExecuteReader())
							while (reader.Read())
								ids.Add((int)reader[0]);
					}
				}
			}
			catch (SqlException sE)
			{
				args.ProcessResult = false;
				args.Message = args.Message = "03 FullTextManager::CreateDocumentsContent exception: " + sE.Message;
				UpdateTextContentCompleted?.Invoke(this, args);
			}
			catch (Exception e)
			{
				args.ProcessResult = false;
				args.Message = args.Message = "04 FullTextManager::CreateDocumentsContent exception: " + e.Message;
				UpdateTextContentCompleted?.Invoke(this, args);
				return;
			}

			//no text content to update 
			if (ids.Count == 0)
				return;

			int taskCount = 0;
			int idCount = 0;
			//I use task to parallel the update
			if (1 <= ids.Count && ids.Count < 100)
			{
				taskCount = 1;
				idCount = ids.Count;
			}
			else
				if (101 <= ids.Count && ids.Count < 1000)
				{
					taskCount = 5;
					idCount = ids.Count / 5;
				}
				else
					if (1001 <= ids.Count && ids.Count < 10000)
					{
						taskCount = 10;
						idCount = ids.Count / 10;
					}
					else
						if (10001 <= ids.Count && ids.Count < 100000)
						{
							taskCount = 20;
							idCount = ids.Count / 20;
						}
						else
							if (ids.Count > 100001)
							{
								taskCount = 50;
								idCount = ids.Count / 50;
							}

			try
			{
				Task[] updateTextContentTasks = new Task[taskCount];

				int start = 0;
				int end = idCount;
				for (int t = 0; t < taskCount; t++)
				{
					List<int> taskIds = new List<int>();

					if (t == taskCount)
						end = ids.Count;

					for (int i = start; i < end; i++)
						taskIds.Add(ids[i]);

					updateTextContentTasks[t] = Task.Factory.StartNew(interator => UpdateTextContent(dc, taskIds, connectionString), t);
					start += idCount;
					end += idCount;
				}

				Task.WaitAll(updateTextContentTasks);
				args.ProcessResult = true;
				args.Message = string.Format(Strings.TextContentUpdate, dbName, dbServer, ids.Count, taskCount);
				endTime = DateTime.Now.Ticks;
				args.Message += string.Format(" Elapsed Time: {0}", new TimeSpan(endTime - startTime).ToString());			
			}
			catch (AggregateException e)
			{
				string msg = string.Empty;
				foreach (var v in e.InnerExceptions)
					msg += v.Message;
				args.ProcessResult = false;
				args.Message = "05 FullTextManager::CreateDocumentsContent exception: " + msg;
			}
			catch (Exception ex)
			{
				args.ProcessResult = false;
				args.Message = "06 FullTextManager::CreateDocumentsContent exception: " + ex.Message;
			}
			finally
			{
				UpdateTextContentCompleted?.Invoke(this, args);
			}
		}
    }
}
