using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder
{
	//===================================================================================
	class SourcesBuilder : IDisposable
	{
		private const string pdbFolderName = "Pdb";
		
		private readonly object buildLock = new object();

		private EBCompilerResults lastBuildResults;

		/// <summary>
		/// Notifies the start of a new build
		/// </summary>
		public event EventHandler<EventArgs> BuildStarting;
		/// <summary>
		/// Notifies the completion of a build
		/// </summary>
		public event EventHandler<BuildEventArgs> BuildCompleted;

		//-------------------------------------------------------------------------------
		protected void OnBuildStarting()
		{
			if (BuildStarting != null)
				BuildStarting(this, EventArgs.Empty);
		}

		//-------------------------------------------------------------------------------
		protected void OnBuildCompleted(BuildEventArgs e)
		{
			if (BuildCompleted != null)
				BuildCompleted(this, e);
		}

		/// <summary>
		/// The result of last performed build
		/// </summary>
		//-------------------------------------------------------------------------------
		public EBCompilerResults LastBuildResults { get { return lastBuildResults; } }

		/// <summary>
		/// Cancella tutti i pdb che non servono più dalla cartella temporanea
		/// </summary>
		//-----------------------------------------------------------------------------
		public static void DeleteUnusedPdbs()
		{
			string pdbDirectory = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), pdbFolderName);
			if (!Directory.Exists(pdbDirectory))
				return;

			try
			{
				DirectoryInfo di = new DirectoryInfo(pdbDirectory);

				//estraggo tutti i pdf files
				FileInfo[] files = di.GetFiles(NameSolverStrings.PdbExtensionSearchCriteria, SearchOption.AllDirectories);

				//creo una lista di file relativi a documenti diversi (potrei ad esempio avere 10 pdb relativi
				//a customers, ma a me interessa solo sapere quali sono
				List<string> differentFiles = new List<string>();
				foreach (FileInfo fileInfo in files)
				{
					if (!differentFiles.Contains(fileInfo.Name))
						differentFiles.Add(fileInfo.Name);
				}

				//ordino per data i file che hanno lo stesso nome (es customers.pdb anche se stanno in cartelle diverse)
				foreach (string file in differentFiles)
				{
					FileInfo[] documentFile = di.GetFiles(file, SearchOption.AllDirectories);
					IOrderedEnumerable<FileInfo> f = from s in documentFile
													 orderby s.LastWriteTime
													 select s;
					//e prendo l'ultimo (il più recente è quello che deve sopravvivere ad una cancellazione
					FileInfo mostRecent = f.Last();

					//tra tutti i file che hanno lo stesso nome, provo a cancellare tutti 
					//quelli che hanno una data minore dell'ultimo
					foreach (FileInfo item in documentFile)
					{
						if (item.LastWriteTime >= mostRecent.LastWriteTime)
							continue;

						try
						{
							item.Directory.Delete(true);
						}
						catch { }
					}
				}
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// Builds customization controllerSources.
		/// </summary>
		//--------------------------------------------------------------------------------
		internal EBCompilerResults Build(Sources sources, bool debug, string filePath = "")
		{
			OnBuildStarting();

			bool generateInMemory = string.IsNullOrEmpty(filePath);

			lock (buildLock)
			{
                IBuildStrategy buildStrategy = null;
                if (generateInMemory)
                {
                    buildStrategy = new InMemoryBuildStrategy();
                }
                else
                {
                    buildStrategy = EBLicenseManager.GenerateCsproj && Settings.Default.UseMsBuild
                        ? new MsBuild_BuildStrategy(filePath, debug) as IBuildStrategy
                        : new OnDiskBuildStrategy(filePath, debug);
                }

                lastBuildResults = buildStrategy.Build(sources);

				OnBuildCompleted(new BuildEventArgs(LastBuildResults, generateInMemory));

				return lastBuildResults;
			}
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			EventHandlers.RemoveEventHandlers(ref BuildStarting);
			EventHandlers.RemoveEventHandlers(ref BuildCompleted);
			GC.SuppressFinalize(this);
		}
	}
}
