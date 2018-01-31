using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	class EbpInfo
	{
		public event EventHandler<UnpackStartedEventArgs> UnpackStarted;
		public event EventHandler<EventArgs> UnpackEnded;
		public event EventHandler<EventArgs> PackStarted;
		public event EventHandler<EventArgs> PackEnded;

		public event EventHandler<CompressEventArgs> BeginUncompressFile;
		public event EventHandler<CompressEventArgs> EndUncompressFile;
		public event EventHandler<CompressEventArgs> BeginCompressFile;
		public event EventHandler<CompressEventArgs> EndCompressFile;

		public event EventHandler<FileAlreadyExistsEventArgs> FileAlreadyExists;

		public event EventHandler<CompanyRelatedFileDetectedEventArgs> CompanyRelatedFileDetected;

		//---------------------------------------------------------------------
		protected void OnCompanyRelatedFileDetected(CompanyRelatedFileDetectedEventArgs e)
		{
			if (CompanyRelatedFileDetected != null)
				CompanyRelatedFileDetected(this, e);
		}

		//---------------------------------------------------------------------
		protected void OnUnpackStarted(UnpackStartedEventArgs e)
		{
			if (UnpackStarted != null)
				UnpackStarted(this, e);
		}
		//---------------------------------------------------------------------
		protected void OnUnpackEnded(EventArgs e)
		{
			if (UnpackEnded != null)
				UnpackEnded(this, e);
		}

		//---------------------------------------------------------------------
		protected void OnPackStarted(EventArgs e)
		{
			if (PackStarted != null)
				PackStarted(this, e);
		}
		//---------------------------------------------------------------------
		protected void OnPackEnded(EventArgs e)
		{
			if (PackEnded != null)
				PackEnded(this, e);
		}

		//---------------------------------------------------------------------
		protected void OnBeginUncompressFile(object sender, CompressEventArgs e)
		{
			if (BeginUncompressFile != null)
				BeginUncompressFile(sender, e);
		}

		//---------------------------------------------------------------------
		protected void OnEndUncompressFile(object sender, CompressEventArgs e)
		{
			if (EndUncompressFile != null)
				EndUncompressFile(sender, e);
		}

		//---------------------------------------------------------------------
		protected void OnBeginCompressFile(object sender, CompressEventArgs e)
		{
			if (BeginCompressFile != null)
				BeginCompressFile(sender, e);
		}

		//---------------------------------------------------------------------
		protected void OnEndCompressFile(object sender, CompressEventArgs e)
		{
			if (EndCompressFile != null)
				EndCompressFile(sender, e);
		}

		//---------------------------------------------------------------------
		protected void OnFileAlreadyExists(FileAlreadyExistsEventArgs e)
		{
			if (FileAlreadyExists != null)
				FileAlreadyExists(this, e);
		}

		public string EbpPath { get; set; }
		public ApplicationType ApplicationType { get; set; }
		public string ApplicationName { get; set; }
		public IList<string> EbpFiles { get; private set; }
		public int FilesNumber { get; private set; }
		public IList<ICustomList> CustomLists { get; private set; }
		public EnumTags EnumTags { get; private set; }
		public bool ContainsEnums { get { return EnumTags.Count > 0; } }
		public IList<string> ModulesName { get; private set; }

		//---------------------------------------------------------------------
		protected EbpInfo(string path)
		{
			EbpPath = path;
			EbpFiles = new List<string>();
			CustomLists = new List<ICustomList>();
			EnumTags = new EnumTags();
			ModulesName = new List<string>();
		}

		//---------------------------------------------------------------------
		public static EbpInfo InspectEbp(string path)
		{
			EbpInfo ebpInfo = new EbpInfo(path);
			ebpInfo.LoadEbp();

			return ebpInfo;
		}

		//---------------------------------------------------------------------
		public static EbpInfo CreateEbp(string outputFilePath)
		{
			return new EbpInfo(outputFilePath);
		}

		//---------------------------------------------------------------------
		private void LoadEbp()
		{
			bool appConfigFound = false;
			using (CompressedFile unzipFiles = new CompressedFile(EbpPath, CompressedFile.OpenMode.Read))
			{
				CompressedEntry[] allEntries = unzipFiles.GetAllEntries();
				this.FilesNumber = allEntries.Length;
				foreach (CompressedEntry file in allEntries)
				{
					if (!appConfigFound && file.Name.IndexOfNoCase(NameSolverStrings.Application + NameSolverStrings.ConfigExtension) > 0)
					{
						ApplicationConfigInfo appInfo = new ApplicationConfigInfo(null, null);
						appInfo.FromStream(file.CurrentStream);

						if (appInfo.Type == NameSolverStrings.EasyBuilderApplication)
							ApplicationType = ApplicationType.Standardization;
						else
							ApplicationType = ApplicationType.Customization;

						//Il nome dell'applicazione è il nome della cartella che contiene l'application.config.
						ApplicationName = Path.GetFileName(Path.GetDirectoryName(file.Name));

						appConfigFound = true;
					}

					string basePath =
						ApplicationType == TaskBuilderNet.Interfaces.ApplicationType.Customization
						?
						BasePathFinder.BasePathFinderInstance.GetCustomPath()
						:
						BasePathFinder.BasePathFinderInstance.GetStandardPath();

					string fileFullPath = new Uri(Path.Combine(basePath, file.Name)).LocalPath;

					ICustomListManager customListManager = null;
					if (fileFullPath.IndexOfNoCase(NameSolverStrings.CustomListFileExtension) > 0)
					{
						EbpFiles.Add(fileFullPath);
						customListManager = new CustomListManager(ApplicationName, String.Empty, Path.GetDirectoryName(fileFullPath));
					}
					else if (fileFullPath.IndexOfNoCase(NameSolverStrings.StandardListFileExtension) > 0)
					{
						EbpFiles.Add(fileFullPath);
						customListManager = new StandardListManager(ApplicationName, String.Empty, Path.GetDirectoryName(fileFullPath));
					}

					if (customListManager != null)
					{
						customListManager.LoadCustomList(file.CurrentStream);
						CustomLists.Add(customListManager.CustomList);
						customListManager = null;
					}

					if (String.Compare(Path.GetFileName(fileFullPath), "Enums.xml", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						Enums enums = new Enums();
						enums.LoadXml(file.CurrentStream);
						EnumTags.AddRange(enums.Tags);
					}

					if (String.Compare(Path.GetFileName(fileFullPath), "Module.config", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						ModulesName.Add(Path.GetFileNameWithoutExtension(Path.GetDirectoryName(fileFullPath)));
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		public void ExtractFiles(
			List<IRenamedCompany> renamedCompanies,
			ref OverwriteResult overWriteResult
			)
		{
			overWriteResult = OverwriteResult.None;

			OnUnpackStarted(new UnpackStartedEventArgs(this.FilesNumber));

			using (CompressedFile compressedFile = new CompressedFile(EbpPath, CompressedFile.OpenMode.Read))
			{
				compressedFile.BeginUncompressFile += new CompressedFile.CompressEventHandler(BeginUncompressFileInternal);
				compressedFile.EndUncompressFile += new CompressedFile.CompressEventHandler(EndUncompressFileInternal);

				CompressedEntry[] compressedEntries = compressedFile.GetAllEntries();

				List<CompanyRelatedUnzipFiles> fileInfos = SearchForCompanyOrUserFiles(compressedEntries, renamedCompanies);

				IList<ICustomListItem> items = null;
				ItemSource itemSource = ItemSource.Custom;
				string tempFolderPathForCurrentUser = Path.GetTempPath();
				string relativePath = null;
				string basePath = null;
				foreach (CompressedEntry compressedEntry in compressedEntries)
				{
					relativePath = compressedEntry.Name;
					items = FindItems(compressedEntry);

					//Se PurgePathIfAbsolute ha modificato relativePath allora devo calcolare basePath in base al nuovo valore di itemSource
					//Serve per retrocompatibilità nel caso ci fossero pacchetti con percorsi assoluti dovuti all'anomalia 19021.
					if (CustomListManager.PurgePathIfAbsolute(ref relativePath, ref itemSource))
					{
						switch (itemSource)
						{
							case ItemSource.Standard:
								basePath = BasePathFinder.BasePathFinderInstance.GetStandardPath();
								break;
							default:
								basePath = BasePathFinder.BasePathFinderInstance.GetCustomPath();
								break;
							
						}
					}
					//Altrimenti, se l'item che sto scompattando è nella custom list
					//(la normalità è questa) allora calcolo il percorso in base al suo ItemSource
					else if (items != null && items.Count == 1 )
					{
						switch (items[0].ItemSource)
						{
							case ItemSource.Standard:
								basePath = BasePathFinder.BasePathFinderInstance.GetStandardPath();
								break;
							default:
								basePath = BasePathFinder.BasePathFinderInstance.GetCustomPath();
								break;
						}
					}
					//Altrimenti devo ricalcolarlo in base alle logiche vecchie,
					//è solo una precauzione, non dovrebbe mai passare di qui.
					else
					{
						switch (ApplicationType)
						{
							case ApplicationType.Customization:
								basePath = BasePathFinder.BasePathFinderInstance.GetCustomPath();
								break;
							case ApplicationType.Standardization:
								basePath = BasePathFinder.BasePathFinderInstance.GetStandardPath();
								break;
							default:
								throw new Exception("Unrecognized application type");
						}
					}

					string fileFullPath = new Uri(Path.Combine(basePath, relativePath)).LocalPath;

					if (File.Exists(fileFullPath))
					{
						if (overWriteResult == OverwriteResult.NoToAll)
							continue;

						if (overWriteResult != OverwriteResult.YesToAll)
						{
							FileAlreadyExistsEventArgs e = new FileAlreadyExistsEventArgs(fileFullPath);
							OnFileAlreadyExists(e);

							if (e.OverwriteResult == OverwriteResult.No)
								continue;

							if (e.OverwriteResult == OverwriteResult.YesToAll)
								overWriteResult = OverwriteResult.YesToAll;

							if (e.OverwriteResult == OverwriteResult.NoToAll)
							{
								overWriteResult = OverwriteResult.NoToAll;
								continue;
							}
						}
					}

					//Estraggo il file nel percorso specificato se non fa parte di quelli legati ad un'azienda
					if (!fileInfos.Contains(new CompanyRelatedUnzipFiles(fileFullPath)))
					{
						using (Stream stream = compressedFile.ExtractFileAsStream(compressedEntry.Name))
						{
							SaveStreamToFile(fileFullPath, stream);
						}
					}
					else
					{
						//se il file che sto considerando fa parte di quelli di un'azienda specifica, allora li 
						//scompatto temporaneamente nella temp...
						fileFullPath = new Uri(Path.Combine(tempFolderPathForCurrentUser, relativePath)).LocalPath;

						using (Stream stream = compressedFile.ExtractFileAsStream(compressedEntry.Name))
						{
							SaveStreamToFile(fileFullPath, stream);
						}
					}
				}
				compressedFile.BeginUncompressFile -= new CompressedFile.CompressEventHandler(BeginUncompressFileInternal);
				compressedFile.EndUncompressFile -= new CompressedFile.CompressEventHandler(EndUncompressFileInternal);
			}

			OnUnpackEnded(EventArgs.Empty);
		}

		//-----------------------------------------------------------------------------
		public void CompressFiles(IList<IEasyBuilderApp> easyBuilderApps)
		{
			OnPackStarted(EventArgs.Empty);

			using (CompressedFile cf = new CompressedFile(EbpPath, CompressedFile.OpenMode.CreateAlways))
			{
				cf.BeginCompressFile += new CompressedFile.CompressEventHandler(BeginCompressFileInternal);
				cf.EndCompressFile += new CompressedFile.CompressEventHandler(EndCompressFileInternal);

				string rootPath = null;
				foreach (IEasyBuilderApp easyBuilderApp in easyBuilderApps)
				{
					//Custom list temporanea, conterrà solamente i file che sono stati effettivamente aggiunti allo zip
					CustomList tempCustomList = new CustomList();
					foreach (CustomListItem item in easyBuilderApp.EasyBuilderAppFileListManager.CustomList)
					{
						try
						{
                            // in ES la gestione dei source code è diventata una sottocartella completa
                            if (BaseCustomizationContext.CustomizationContextInstance.HasSourceCode(item.FilePath) && !Settings.Default.ExcludeSources)
                            {
                                string sourceCodePath = GetPathWithSources(item);
                                if (Directory.Exists(sourceCodePath))
                                    cf.AddFolder(sourceCodePath, true);
                            }

                            //In base al settings, se si vuole solo esportare le customizzazioni published, skippo quelle 
                            //che non lo sono
                            if (
								Settings.Default.ExportPublishedOnly &&
								BaseCustomizationContext.CustomizationContextInstance.IsSubjectedToPublication(item.FilePath) &&
								!item.PublishedUser.IsNullOrEmpty()
								)
								continue;

							//Escluso momentaneamente la custom list
							if (item.FilePath.IndexOfNoCase(NameSolverStrings.CustomListFileExtension) > 0)
							{
								//ma la aggiungo comunque alla temp custom list
								tempCustomList.Add(item);
								continue;
							}

							//Se e' un file valido per essere pacchettizzato, lo aggiungo
							if (BaseCustomizationContext.CustomizationContextInstance.IsFileToExport(item.FilePath))
							{
								tempCustomList.Add(item);

								rootPath = (item.ItemSource == ItemSource.Standard)
										? BasePathFinder.BasePathFinderInstance.GetStandardPath()
										: BasePathFinder.BasePathFinderInstance.GetCustomPath();

								cf.AddFile(item.FilePath, rootPath, string.Empty);
                            }
                        }
						catch
						{
							//il file potrebbe esserci già, nel caso la AddFile genera eccezione, occorrerebbe
							//controllare prima di fare la add
						}
					}

					using (Stream s = easyBuilderApp.EasyBuilderAppFileListManager.GetStreamFromCustomList(tempCustomList))
					{
						string file = easyBuilderApp.EasyBuilderAppFileListManager.CustomListFullPath
							.ReplaceNoCase(BasePathFinder.BasePathFinderInstance.GetStandardPath() + "\\", String.Empty)
							.ReplaceNoCase(BasePathFinder.BasePathFinderInstance.GetCustomPath() + "\\", String.Empty);
						cf.AddStream(file, s);
					}
				}

				cf.BeginCompressFile -= new CompressedFile.CompressEventHandler(BeginCompressFileInternal);
				cf.EndCompressFile -= new CompressedFile.CompressEventHandler(EndCompressFileInternal);
			}

			OnPackEnded(EventArgs.Empty);
		}

        //-----------------------------------------------------------------------------
        public string GetPathWithSources(CustomListItem item)
        {
            string customizationName = Path.GetFileNameWithoutExtension(item.FilePath);
            string sourceCodePath = Path.GetDirectoryName(item.FilePath);
            string customizationPath = sourceCodePath;
            string sourceFolderSuffix = "_Src";

            // come scaletta di priorità: o ho l'utente che l'ha realizzato,
            // oppure se e' pubblico provo con l'utente corrente
            if (item.PublishedUser.IsNullOrEmpty())
                sourceCodePath = Path.Combine(sourceCodePath, CUtility.GetUser());
            else
                // altrimenti mi metto da parte la customizationPath escludendo il percorso utente
                customizationPath = Path.GetDirectoryName(customizationPath);
            sourceCodePath = Path.Combine(sourceCodePath, customizationName + sourceFolderSuffix);
            // non ho trovato i sorgenti
            if (!Directory.Exists(sourceCodePath))
            {
                // non ho trovato i sorgenti allora cerco nelle sottocartelle utente
                foreach (string userFolder in Directory.GetDirectories(customizationPath))
                {
                    sourceCodePath = Path.Combine(userFolder, customizationName + sourceFolderSuffix);
                    if (Directory.Exists(sourceCodePath))
                       break;
                }
            }
            return sourceCodePath;

        }

        //-----------------------------------------------------------------------------
        private IList<ICustomListItem> FindItems(CompressedEntry compressedEntry)
		{
			List<ICustomListItem> items = new List<ICustomListItem>();
			foreach (var customList in CustomLists)
			{
				items.AddRange(customList.FindItemByPathPart(compressedEntry.Name.ReplaceNoCase("/", "\\")));
			}
			return items;
		}

		//-----------------------------------------------------------------------------
		private static void SaveStreamToFile(string fileFullPath, Stream fileStream)
		{
			DirectoryInfo targetDirInfo = new DirectoryInfo(Path.GetDirectoryName(fileFullPath));
			if (!targetDirInfo.Exists)
				targetDirInfo.Create();

			byte[] content = new byte[fileStream.Length];
			fileStream.Read(content, 0, content.Length);

			System.IO.File.WriteAllBytes(fileFullPath, content);
		}

		//-----------------------------------------------------------------------------
		void BeginUncompressFileInternal(object sender, CompressEventArgs arg)
		{
			OnBeginUncompressFile(sender, arg);
		}
		
		//-----------------------------------------------------------------------------
		void EndUncompressFileInternal(object sender, CompressEventArgs arg)
		{
			OnEndUncompressFile(sender, arg);
		}
		
		//-----------------------------------------------------------------------------
		void BeginCompressFileInternal(object sender, CompressEventArgs arg)
		{
			OnBeginCompressFile(sender, arg);
		}
		
		//-----------------------------------------------------------------------------
		void EndCompressFileInternal(object sender, CompressEventArgs arg)
		{
			OnEndCompressFile(sender, arg);
		}

		/// <summary>
		/// Analizza lo zip alla ricerca di path che siano legati ad un'azienda o ad un utente specifico.
		/// in modo che si possa proporre all'utente di installare il package per una diversa azienda
		/// </summary>
		//-----------------------------------------------------------------------------
		private List<CompanyRelatedUnzipFiles> SearchForCompanyOrUserFiles(
			CompressedEntry[] files,
			List<IRenamedCompany> renamedCompanies
			)
		{
			string basePath = BasePathFinder.BasePathFinderInstance.GetCustomPath();

			List<CompanyRelatedUnzipFiles> filesToMove = new List<CompanyRelatedUnzipFiles>();

			//Cerco tutti i file che sono collegati ad un'azienda specifica e li metto da parte
			foreach (CompressedEntry file in files)
			{
				//C:\Development\Custom\Companies\Dev\Applications...
				string fileFullPath = new Uri(Path.Combine(basePath, file.Name)).LocalPath;

				//Se non è un file che fa riferimento ad un'azienda, non mi interessa
				string company = GetCompanyFromFullPath(fileFullPath);
				if (company.IsNullOrEmpty())
					continue;

				filesToMove.Add(new CompanyRelatedUnzipFiles(fileFullPath, company, company));
			}

			//tra i file che fanno riferimento ad un'azienda, per ogni azienda diversa chiedo all'utente
			//se vuole usare un altro nome durante l'unzip, e me lo metto da parte
			foreach (CompanyRelatedUnzipFiles item in filesToMove)
			{
				if (renamedCompanies.Contains(new RenamedCompany(item.OldCompanyName)))
					continue;

				CompanyRelatedFileDetectedEventArgs e = new CompanyRelatedFileDetectedEventArgs(item.OldCompanyName);
				OnCompanyRelatedFileDetected(e);

				renamedCompanies.Add(new RenamedCompany(item.OldCompanyName, e.NewCompanyName));

				item.NewCompanyName = e.NewCompanyName;
			}

			return filesToMove;
		}

		/// <summary>
		/// Dato un fullpath, cerca di capire se far riferimento ad un'azienda particolare
		/// ritorna il nome dell'azienda se esiste, o string.empty se non esiste o se trova AllCompanies
		/// </summary>
		/// <param name="fullPath"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		private static string GetCompanyFromFullPath(string fullPath)
		{
			string pattern = @"(\\|/)Companies(\\|/)(?<Company>[^\\/]+)(\\|/)applications(\\|/)";
			Match match = Regex.Match(fullPath, pattern, RegexOptions.IgnoreCase);
			if (match == null)
				return string.Empty;

			Group group = match.Groups["Company"];
			if (group == null)
				return string.Empty;

			if (group.Value.CompareNoCase(NameSolverStrings.EasyStudioHome))
				return string.Empty;

			return group.Value;
		}
	}

	//=========================================================================
	internal class FileAlreadyExistsEventArgs : EventArgs
	{
		public OverwriteResult OverwriteResult { get; set; }
		public string FullFilePath { get; set; }

		//-----------------------------------------------------------------------------
		public FileAlreadyExistsEventArgs(string fullFilePath)
		{
			this.FullFilePath = fullFilePath;
		}
	}

	//=========================================================================
	internal class CompanyRelatedFileDetectedEventArgs : EventArgs
	{
		public string OldCompanyName { get; set; }
		public string NewCompanyName { get; set; }

		//-----------------------------------------------------------------------------
		public CompanyRelatedFileDetectedEventArgs(string oldCompanyName)
		{
			this.OldCompanyName = oldCompanyName;
		}
	}
	//=========================================================================
	internal class UnpackStartedEventArgs : EventArgs
	{
		public int FilesNumber { get; private set; }

		//-----------------------------------------------------------------------------
		public UnpackStartedEventArgs(int filesNumber)
		{
			this.FilesNumber = filesNumber;
		}
	}
}
