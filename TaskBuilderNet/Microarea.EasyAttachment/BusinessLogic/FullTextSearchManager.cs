using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;

namespace Microarea.EasyAttachment.BusinessLogic
{
	//classe di ausilio per la costruzione della query di FullTextSearch
	// vi è un unica istanza per tutti i DMS_Orchestrator
	//================================================================================
	public class FullTextFilterManager : BaseManager
	{
		//Dictionary<string, string> noise;
			List<string> iFilterDocType = new List<string>();

		string connectionString = string.Empty;

		private DMSOrchestrator dmsOrchestrator = null;
	
		//--------------------------------------------------------------------------------
		public FullTextFilterManager(DMSOrchestrator orchestrator)
		{
			dmsOrchestrator = orchestrator;
			LoadIFilterDocType(dmsOrchestrator.DMSConnectionString);
			//noise = LoadNoiseDictionary();				
		}
	
		//load all ifilter document type used by SqlServer Full Text Search manager
		//--------------------------------------------------------------------------------
		private void LoadIFilterDocType(string connectionString)
		{ 			
			SqlConnection connection = null;
			try
			{

				connection = new SqlConnection(connectionString);
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText = @"Select document_type from sys.fulltext_document_types";

					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
							iFilterDocType.Add(reader[0].ToString());
					}
				}
			}

			catch (SqlException)
			{
				iFilterDocType.Clear();
			}
			finally
			{
				if (connection != null && connection.State != ConnectionState.Closed)
				{
					connection.Close();
					connection.Dispose();
				}
			}			
		}
			
		//--------------------------------------------------------------------------------
		private Dictionary<string, string> LoadNoiseFromFile()
		{
			Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			return dic;
		}

		//--------------------------------------------------------------------------------
		private Dictionary<string, string> LoadNoiseDictionary()
		{
			Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

			SqlConnection connection = null;
			SqlCommand command = null;
			SqlDataReader reader = null;
			try
			{
				
				connection = new SqlConnection(dmsOrchestrator.DMSConnectionString);
				connection.Open();
				string serverVersion = connection.ServerVersion;
				int versionNumber = int.Parse(serverVersion.Split(new string[] { "." }, StringSplitOptions.None)[0]);
				if (versionNumber == 9) //SQL Server 2005
				{
					connection.Close();
					return LoadNoiseFromFile();
				}
				
				string stopListName = string.Format("{0}_SL", connection.Database);
				string query = @"SELECT stopword FROM sys.fulltext_stopwords X, sys.fulltext_stoplists Y 
								WHERE X.stoplist_id = Y.stoplist_id AND X.language_id = {0} AND Y.Name = '{1}'";

				command = new SqlCommand(string.Format(query, dmsOrchestrator.SID.ToString(), stopListName), connection);
				reader = command.ExecuteReader();
				
				while (reader.Read())
				{
					dic.Add(reader[0].ToString(), reader[0].ToString());
				}
				reader.Close();
			}

			catch (SqlException)
			{
				dic.Clear();
				//SetMessage(Strings.ErrorDeletingBinaryContent, e, "DeleteBinaryContent");
			}
			finally
			{
				if (command != null)
					command.Dispose();

				if (connection != null && connection.State != ConnectionState.Closed)
				{
					connection.Close();
					connection.Dispose();
				}
				if (reader != null)
					reader.Close();
			}

			return dic;
		}

		//--------------------------------------------------------------------------------
		public static string Prepare(string txt)
		{
			txt = txt.Trim();
			if (string.IsNullOrWhiteSpace(txt) || (txt.Count() > 2 && txt.First() == '\"' && txt.Last() == '\"'))
				return txt;
			StringBuilder sb = new StringBuilder();
			AppendAllWords(txt, sb, " AND ");
			return sb.ToString();
		}

		//--------------------------------------------------------------------------------
		private static void AppendAllWords(string txt, StringBuilder sb, string separator)
		{
			Dictionary<string, string> noiseDic = new Dictionary<string,string>(); // = noise;
			Dictionary<string, string> keywords = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			string[] kwds = { "AND", "OR", "NOT", "NEAR", "~", "ISABOUT", "WEIGHT" };
			foreach (string keyword in kwds)
				keywords[keyword] = keyword;

			//string.Compare(language, "it", StringComparison.InvariantCultureIgnoreCase) == 0
			//? noiseIT : noise;

			string[] toks = txt.Split(new char[] { ' ', ',', ':', ';', '!', '?', '\'' }, StringSplitOptions.RemoveEmptyEntries);
			//new char[] { ' ', '\t', '\'', ':', ',', ';', '.', '?', '!', '+', '–', '-', '(', ')', '%', '#', '&', '[', ']', '<', '@', '=' }, 

			List<string> words = new List<string>();
			foreach (string tok in toks)
			{
				string word;
				bool bw =
					tok.Length >= 2 &&
					tok.StartsWith("\"", StringComparison.Ordinal) &&
					tok.EndsWith("\"", StringComparison.Ordinal);
				if (bw)
				{
					word = tok.Substring(1, tok.Length - 2);
					if (noiseDic.ContainsKey(word))
						continue;
				}
				else
				{
					word = tok.Replace("\"", string.Empty);
					if (keywords.ContainsKey(word) && noiseDic.ContainsKey(word))
						continue; // even if it is a keyword, skip it if it is a noise word
					if (keywords.ContainsKey(word))
						bw = true; // puts between commas
					else if (noiseDic.ContainsKey(word)
							|| noiseDic.ContainsKey(word + '\'') // hack: "to'" is noise word for IT, and seems to matter even for "to" word
						)
						continue;
				}
				word = word.Replace("\"", string.Empty);
				if (bw)
					word = '"' + word + '"'; // replace the trailing ones

				// if word is made of * chars and noise words (or * chars only), skip it
				if (word.IndexOf('*') != -1)
				{
					string[] ins = word.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
					int goods = ins.Length;
					foreach (string inW in ins)
						if (noiseDic.ContainsKey(inW))
							--goods;
					if (goods == 0)
						continue;
				}

				if (word.Length != 0)
					words.Add(word);
			}
			for (int i = 0; i < words.Count; ++i)
			{
				string word = words[i];
				bool star = word.EndsWith("*", StringComparison.Ordinal);
				if (star)
					sb.Append('\"');
				sb.Append(word);
				if (star)
					sb.Append('\"');
				if (i < words.Count - 1)
					sb.Append(separator);
			}
		}

		//--------------------------------------------------------------------------------
		private string PrepareAllWordsForAbstract(string txt, string language)
		{
			StringBuilder sb = new StringBuilder();
			AppendAllWords(txt, sb, ", ");
			sb.Replace("\"", string.Empty);
			return sb.ToString();
		}

		//--------------------------------------------------------------------------------
		public static IEnumerable<DMS_Attachment> SearchForAttachments(SearchRules searchRules, string textToSearch,  DMSOrchestrator dmsOrchestrator)
		{
			string formattedText = Prepare(textToSearch);

			if (string.IsNullOrEmpty(formattedText))
				return null;

			int collectorID = -1;
			int collectionID = -1;
			int erpDocumentID = -1;
			string extType = "All";

			List<DMS_Attachment> contentResult = new List<DMS_Attachment>();
			DMSModelDataContext dc = new DMSModelDataContext(dmsOrchestrator.DMSConnectionString);
			try
			{
				switch (searchRules.SearchContext)
				{
					case SearchContext.Current:
                        erpDocumentID = dmsOrchestrator.ERPDocumentID;
                        if (erpDocumentID == -1) //vuol dire che il documento non ha alcun allegato
                            return null;
						break;

					case SearchContext.Collection:
						collectionID = dmsOrchestrator.CollectionID;
						break;

					case SearchContext.Collector:
						collectorID = dmsOrchestrator.CollectorID;
						break;

					case SearchContext.Custom:
						break;					
				}

				//document type filter
				if (
						!string.IsNullOrEmpty(searchRules.DocExtensionType) &&
						string.Compare(searchRules.DocExtensionType, CommonStrings.AllExtensions, StringComparison.InvariantCultureIgnoreCase) != 0
					)
					extType = searchRules.DocExtensionType;

				IQueryable<udf_FTS_SearchAttachmentsResult> attachmentsId =
				dc.udf_FTS_SearchAttachments(collectorID, collectionID, erpDocumentID, extType,
											(searchRules.StartDate == DateTime.MinValue) ? (DateTime)SqlDateTime.MinValue : searchRules.StartDate,
											(searchRules.EndDate == DateTime.MaxValue) ? (DateTime)SqlDateTime.MaxValue : searchRules.EndDate,
											formattedText);
				
				IEnumerable<int> resultList = null;
				if (attachmentsId != null || attachmentsId.Any())
					resultList = attachmentsId.Select(a => a.AttachmentID).AsEnumerable();

				foreach (int attachmentID in resultList)
				{
					var attach = DMS_Attachment.AttachmentById(dc, attachmentID);
					if (attach != null)
						contentResult.Add((DMS_Attachment)attach.Single());
				}
			}
			catch(SqlException e)
			{
				contentResult = null;
				//SQL Server encountered error 0x80070218 while communicating with full-text filter daemon host (FDHost) process. 
				//Make sure that the FDHost process is running. To re-start the FDHost process, run the sp_fulltext_service 
				//'restart_all_fdhosts' command or restart the SQL Server instance.
				//if (e.Number == 30046)
					throw (e);
			}
			catch(Exception e)
			{
				contentResult = null;
				throw (e);
			}

			return contentResult;
		}

		//--------------------------------------------------------------------------------
		public static IEnumerable<DMS_ArchivedDocument> SearchForArchivedDoc(FilterEventArgs filter, string textToSearch, DMSOrchestrator dmsOrchestrator)
		{
			string formattedText = Prepare(textToSearch);

			if (string.IsNullOrEmpty(formattedText))
				return null;

			string extType = "All";

			List<DMS_ArchivedDocument> contentResult = new List<DMS_ArchivedDocument>();
			DMSModelDataContext dc = new DMSModelDataContext(dmsOrchestrator.DMSConnectionString);		
			try
			{
				//document type filter
				if (
						!string.IsNullOrEmpty(filter.DocExtensionType) &&
						string.Compare(filter.DocExtensionType, CommonStrings.AllExtensions, StringComparison.InvariantCultureIgnoreCase) != 0
					)
					extType = filter.DocExtensionType;

				IQueryable<udf_FTS_SearchArchivedDocResult> archivedDocsId = 
					dc.udf_FTS_SearchArchivedDoc(extType,
												(filter.StartDate == DateTime.MinValue) ? (DateTime)SqlDateTime.MinValue : filter.StartDate,
												(filter.EndDate == DateTime.MaxValue) ? (DateTime)SqlDateTime.MaxValue : filter.EndDate,
												filter.OnlyReport, filter.OnlyNonAttached, filter.OnlyAttached, filter.OnlyPending, formattedText);

				IEnumerable<int> resultList = null;
				if (archivedDocsId != null || archivedDocsId.Any())
					resultList = archivedDocsId.Select(a => a.ArchivedDocID).AsEnumerable();

				foreach (int archivedDocID in resultList)
				{
					var attach = DMS_ArchivedDocument.ArchivedDocById(dc, archivedDocID);
					if (attach != null)
						contentResult.Add((DMS_ArchivedDocument)attach.Single());
				}
			}
			catch(SqlException e)
			{
				contentResult = null;
				//SQL Server encountered error 0x80070218 while communicating with full-text filter daemon host (FDHost) process. 
				//Make sure that the FDHost process is running. To re-start the FDHost process, run the sp_fulltext_service 
				//'restart_all_fdhosts' command or restart the SQL Server instance.
				//if (e.Number == 30046)
					throw (e);
			}
			catch(Exception e)
			{
				contentResult = null;
				throw (e);
			}

			return contentResult;
		}

		//-----------------------------------------------------------------------------------
		public bool IsIFilterDocType(string ext)
		{
            // dai ns test l'IFilter Adobe per il pdf non funziona. 
            // mentre siamo sicuri che con il ns algoritmo funzioniamo
            if (string.Compare(ext, FileExtensions.DotPdf, StringComparison.InvariantCultureIgnoreCase) == 0)
                return dmsOrchestrator.SettingsManager.UsersSettingState.Options.FTSOptionsState.FTSNotConsiderPdF;

            return iFilterDocType.Exists(a => a == ext);
		}
	}
}
