using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Xml;
using System.Xml.Schema;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using WindowsInstaller;
using Microarea.TaskBuilderNet.Core.SoapCall;
using Microarea.TaskBuilderNet.Interfaces;
using System.Security;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.Security.Principal;
using Microarea.TaskBuilderNet.ParametersManager;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Runtime.Serialization.Formatters.Binary;

namespace ManifestGenerator
{
	//================================================================================
	/// <summary>
	/// Genera i manifest per il setup di un'applicazione ClickOnce
	/// </summary>
	/// <remarks>
	/// Questa classe può essere utilizzata per generare il pacchetto di installazione ClickOnce di MagoNet e della Microarea Console.
	/// Parte dal presupposto che esista una cartella 
	/// C:\installazione\Apps contentente le seguenti sottocartelle:
	/// MagoNet - cartella che contiene il programma di gestione del menu
	/// TBApps - cartella che contiene le librerie di TaskBuilder C++ e di ERP, oltre ad eventuali dll di verticali
	/// MicroareaConsole - cartella conenente le librerie della console
	/// e genera una cartella Publish contenente tutte le componenti (dll ed eseguibili) da installare sui client ed i file di manifest di applicazione (Cosole e Mago).
	/// I file di manifest di installazione sono invece generati nella cartella Apps, uniformemente a quanto fatto da Visual Studio
	/// </remarks>
	public class Generator : IDisposable
	{
		/// <summary>
		/// file generati dinamicamente da cancellare
		/// </summary>
		private static readonly string[] dynamicFiles = 
		{
			"StringLoader.bin", 
			"TBStringLoader.bin", 
			"StandardMenu*.xml",
			"LoggedUser.xml",
			"Microarea.Library.TbDynamicWCFServices*.dll",
			"TbDynamicWCF*.dll",
            "openNoterel.exe"
		};
		private const string AppExt = ".app";
        private const string payloadExt = ".payload";
		/// <summary>
		/// estensioni dei file da coinvolgere nel deploy
		/// </summary>
		private static readonly string[] deployExt = 
		{
			"*.exe", 
			"*.dll", 
			"*.bin", 
			"*.xml", 
			"*.pak",
            "*.dat",
            "*.exe.config",
            "*.dll.config",
            "*" + payloadExt
		};
		private const string tbManifestExt = ".setup";
		
		private const int toolsTimeout = 120000;//2 minuti
		private const string splitterToken = "<!--SPLIT-->";
		private const string titlePlaceholder = "[TITLE]";
        private const string producerWebSitePlaceholder = "[PRODUCERWEBSITE]";
        private const string producerPlaceholder = "[PRODUCER]";
        private const string producerPngPlaceholder = "[PRODUCERPNG]";
        private const string introPlaceholder = "[INTRO]";
		private const string indexPage = "Index.htm";

		private static ILogger logger = new Logger();

		private static readonly List<CultureInfo> Cultures = InitCultures();
		//--------------------------------------------------------------------------------
		private static List<CultureInfo> InitCultures()
		{
			try
			{
				return new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.AllCultures));
			}
			catch
			{
				return null;
			}
		}

        readonly UrlManager urlManager = new UrlManager();

        class UrlManager
        {
            public bool UseBackupUrl { get; set; }
            public bool CanSwitchToUseBackupUrl { get { return !UseBackupUrl; } }

            public string Url
            {
                get
                {
#if !DEBUG
                    if (!UseBackupUrl)
                    {
                        return InstallationData.GenericBrand.DigitalSignatureUrl;//"http://www.microarea.it/DigitalSigner/DigitalSigner.asmx";
                    }
                    else
                    {
                        return InstallationData.GenericBrand.DigitalSignatureBackupUrl;//"http://ping.microarea.eu/DigitalSigner/DigitalSigner.asmx";
                    }
#else
                    return "http://spp-hotfix/DigitalSigner/DigitalSigner.asmx";
#endif
                }
            }
        }

        /// <summary>
        /// Specifica se inserire il nome del publisher all'inizio del titolo dell'applicazione
        /// </summary>
        bool prependPublisherToAppTitle = true;

		/// <summary>
		/// Percorso radice contenente i file da impacchettare (es. c:\development\Apps)
		/// </summary>
		string rootPath = null;
		/// <summary>
		/// Percorso a MSBuild.exe
		/// </summary>
		string msBuildFile = null;
        /// <summary>
		/// Cartella di destinazione
		/// </summary>
		string deployFolder = null;
		/// <summary>
		/// namespace del file xml per i task msbuild
		/// </summary>
		private const string msBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
		/// <summary>
		/// namespace del file xml per la configurazione del deploy clickonce
		/// </summary>
		private const string clientSetupNamespace = "http://schemas.microarea.it/clientsetup";

		//ad ogni operazione effettuata eseguo uno step di progress bar, ogni metodo
		//puo` avere un suo numero di steps
		private const int fixedSteps = 5; /*primo di incoraggiamento + setup easylook + serverconnection + wcf assembly + refresh login manager*/
		
		public enum DeployType { Deploy, Update }
		//--------------------------------------------------------------------------------
		private string ComponentsLocalPath
		{
			get { return Path.Combine(rootPath, "Prerequisites\\Packages\\"); }
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Costruisce l'oggetto
		/// </summary>
		/// <param name="rootPath">Percorso contenente le cartelle di libreria necessarie per generare il package di deploy (es. c:\development\Apps)</param>
		public Generator(string rootPath)
		{
			if (!Directory.Exists(rootPath))
				throw new Exception(string.Format("Invalid root folder: {0}", rootPath));

			this.rootPath = rootPath;
			this.msBuildFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"MSBuild.exe");
			this.deployFolder = Path.Combine(rootPath, NameSolverStrings.Publish);

			GenerateFiles();
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Pulisce la cartella di deployment
		/// </summary>
		public void Clean()
		{
			if (Directory.Exists(deployFolder))
				Directory.Delete(deployFolder, true);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Cancella i file dinamici generati in automatico da TBLoader, così da causarne la rigenerazione e conseguente aggiornamento
		/// </summary>
		private void DeleteDynamicFiles()
		{
			if (Directory.Exists(deployFolder))
				DeleteDynamicFiles(deployFolder);
			//for each product folder...
			foreach (string productFolder in Directory.GetDirectories(rootPath))
			{
				//for each version folder (debug/release)
				foreach (string versionFolder in Directory.GetDirectories(productFolder))
				{
					DeleteDynamicFiles(versionFolder);
				}
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Cancella i file dinamici generati in automatico da TBLoader, così da causarne la rigenerazione e conseguente aggiornamento
		/// </summary>
		private static void DeleteDynamicFiles(string containingFolder)
		{
			foreach (string fileName in dynamicFiles)
			{
				foreach (string fileToDelete in Directory.GetFiles(containingFolder, fileName))
					File.Delete(fileToDelete);
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Esegue l'azione di deploy
		/// </summary>
		public bool PerformDeployAction(
			string installationName,
			string fileHostName,
			string webHostName,
			string uiCulture,
			string appCulture,
			int webServicesPort,
			string debugOrRelease,
			string prerequisitesPath,
			bool multiInstance,
			bool clean
			)
		{
			if (clean)
				Clean();

			return DeployOrUpdate(installationName, fileHostName, webHostName, uiCulture, appCulture, webServicesPort, debugOrRelease, prerequisitesPath, multiInstance, DeployType.Deploy);
		}


		//--------------------------------------------------------------------------------
		public bool PerformUpdateDeploymentAction(
			string installationName,
			string fileHostName,
			string webHostName,
			string uiCulture,
			string appCulture,
			int webServicesPort,
			string debugOrRelease,
			string prerequisitesPath,
			bool multiInstance
			)
		{
			return DeployOrUpdate(installationName, fileHostName, webHostName, uiCulture, appCulture, webServicesPort, debugOrRelease, prerequisitesPath, multiInstance, DeployType.Update);
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Genera i manifest oppure li aggiorna a seconda delle informazioni passate
		/// </summary>
		public bool DeployOrUpdate(
			string installationName,
			string fileHostName,
			string webHostName,
			string uiCulture,
			string appCulture,
			int webServicesPort,
			string debugOrRelease,
			string prerequisitesPath,
			bool multiInstance,
			DeployType type
			)
		{
			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(uiCulture);
				logger.Start();

				logger.PerformStep();

				switch (type)
				{
					case DeployType.Deploy:
						{
							//Modifica la data presente le file Installation.ver che regola l'invalidazione di tutte le cache
							//(menu, dll dinamiche ecc.)
							BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

							bool updating = Directory.Exists(deployFolder);
							
                            //prima cancello i file dinamici ovunque si trovino
							DeleteDynamicFiles();

							//devo farlo prima della GenerateWCFAssembly che mi locca le dll
							if (updating)
								CopyOldDeployFiles(debugOrRelease);

							//devo generarlo altrimenti non parte tbloader
							GenerateServerConnection(uiCulture, appCulture, webServicesPort);

							//Genero il web.config per disabilitare il request filtering sui file *.exe.config per il deploy via clickonce
							GenerateWebConfig();

                            //Genero il web.config di istanza per la configurazione dei mime types
                            GenerateWebConfigForMimeTypes();

                            //se la cartella esiste, aggiorno il manifest con eventuali nuove dll (verticali)
                            bool ok = false;
                            if (updating)
                            {
                                ok = UpdateAppManifestContentAndBootstrappers(installationName, webHostName, uiCulture, appCulture, webServicesPort, debugOrRelease);
                            }
                            else
                            {
                                ok = Deploy(installationName, fileHostName, webHostName, uiCulture, appCulture, webServicesPort, debugOrRelease, multiInstance);
                            }
                            logger.PerformStep();

                            //poi li rigenero
                            GenerateWCFAssembly(debugOrRelease, updating); //se sto aggiornando

                            return ok;
						}
					case DeployType.Update:
						{

							Thread setupAdjustmentThread = null;
							try
							{
								setupAdjustmentThread = StartSetupServer(uiCulture, appCulture, webServicesPort, true);

                                //Genero il web.config per disabilitare il request filtering sui file *.exe.config per il deploy via clickonce
                                GenerateWebConfig();

                                //Genero il web.config di istanza per la configurazione dei mime types
                                GenerateWebConfigForMimeTypes();

                                return UpdateDeployment(installationName, fileHostName, webHostName, uiCulture, appCulture, webServicesPort, debugOrRelease, multiInstance);
							}
							finally
							{
								UpdateInstallationVersion();

								//aspetto che il thread di preparazione del setup abbia finito
								if (setupAdjustmentThread != null)
									setupAdjustmentThread.Join();
							}
						}
					default:
						return false;
				}
			}
			catch (Exception ex)
			{
				logger.WriteLine(ex.ToString());
				return false;
			}
			finally
			{
				logger.Stop();
			}
		}

		//--------------------------------------------------------------------------------
		private void UpdateInstallationVersion()
		{
			string file = BasePathFinder.BasePathFinderInstance.GetInstallationVersionPath();
			InstallationVersion info = InstallationVersion.LoadFromOrCreate(file);
            info.IDate = DateTime.Now;
            info.Save(file);
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiorna il contenuto dei manifest delle applicazioni (non degli addin)
		/// per includere eventuali verticali aggiunti nel frattempo
		/// </summary>
		/// <param name="uiCulture"></param>
		/// <param name="appCulture"></param>
		/// <param name="webServicesPort"></param>
		/// <returns></returns>
		public bool UpdateAppManifestContentAndBootstrappers(string installationName, string webHostName, string uiCulture, string appCulture, int webServicesPort, string debugOrRelease)
		{

			Thread setupAdjustmentThread = null;
			try
			{
				setupAdjustmentThread = StartSetupServer(uiCulture, appCulture, webServicesPort, false);

				string appName = null;
				string appTitle = null;
				string publisher = null;

				string[] files = Directory.GetFiles(deployFolder, "*" + tbManifestExt);

				const int updateSteps = 7;
				logger.SetProgressTop(files.Length * updateSteps + fixedSteps);

				//lista di tutit i file presenti in qualche manifest
				List<string> manifestFiles = new List<string>();

				foreach (string configurationPath in files)
				{
					int steps = updateSteps;
					try
					{
						bool officeAddin = false;
						List<string> optionalFolders = null;
						List<string> requiredFolders = null;
						List<string> prerequisiteFolders = null;
                        List<string> payloadNames = new List<string>();

						//leggo le informazioni per generare il setup
						if (!ReadSetupInfo(configurationPath, out appName, out appTitle, out publisher, out officeAddin, out optionalFolders, out requiredFolders, out prerequisiteFolders))
							continue;
						if (officeAddin)
							continue;

						string appFile = string.Format("{0}.Exe", appName);
						string appPath = Path.Combine(deployFolder, appFile);
						string appManifestFile = appPath + ".manifest";
						ApplicationManifest appManifest = (ApplicationManifest)ManifestReader.ReadManifest(appManifestFile, true);

						//solo la prima volta popolo l'array di tutti i file di manifest presenti nella cartella
						if (manifestFiles.Count == 0)
						{
							logger.WriteLine("Detecting manifest files...");
                            AddToFiles(appManifest, manifestFiles, payloadNames);
							foreach (string file in Directory.GetFiles(deployFolder, "*.manifest"))
							{
								if (string.Compare(file, appManifestFile, StringComparison.InvariantCultureIgnoreCase) == 0)
									continue;
								AddToFiles((ApplicationManifest)ManifestReader.ReadManifest(file, true), manifestFiles, payloadNames);
							}
							PerformStep(ref steps);
						}

						logger.WriteLine("Removing old files...");

						RemovePhantomReferences(appManifest.AssemblyReferences);
						RemovePhantomReferences(appManifest.FileReferences);
						
						PerformStep(ref steps);

						logger.WriteLine("Adding files not listed in manifest...");
						List<string> orphanFiles = DetectOrphanFiles(manifestFiles, payloadNames);
						List<string> addedFiles = new List<string>();
						foreach (string orphan in orphanFiles)
						{
							BaseReference ar = AddFileToManifest(appManifest, Path.Combine(deployFolder, orphan), appFile, addedFiles);
							if (ar == null)
								continue;

							ar.Group = "TBApps";
							//euristica: se il file si trova in una sottocartella, allora è un dizionario
							SetOptionalIfDictionary(ar);
						}
						PerformStep(ref steps);

						logger.WriteLine("Resolving manifest files...");

                        var entryPoint = appManifest.EntryPoint;//Chiamata fittizia per mantenere valorizzato EntryPoint, in alcuni casi mi veniva cancellato
						appManifest.ResolveFiles();
						appManifest.UpdateFileInfo();
						appManifest.UpdateAssemblyVersions();
						appManifest.Validate();
						PerformStep(ref steps);

						string appVersion = CalculateAppVersion(appPath);

						appManifest.AssemblyIdentity.Version = appVersion;
						//salva il file modificato
						ManifestWriter.WriteManifest(appManifest, appManifestFile);

						//firmo il manifest di applicazione
						logger.WriteLine("Signing {0}...", appManifestFile);
						SignFile(appManifestFile);
						PerformStep(ref steps);

						string deployFile = Path.Combine(rootPath, string.Format("{0}.Application", appName));
						logger.WriteLine("Updating deployment manifest {0}...", deployFile);
						//aggiorno il manifest di deploy perche la signature del manifest di applicazione e` cambiata
						DeployManifest man = (DeployManifest)ManifestReader.ReadManifest(deployFile, true);
						man.MinimumRequiredVersion = appVersion;
						man.AssemblyIdentity.Version = appVersion;
						man.AssemblyIdentity.Culture = "neutral";
						man.MapFileExtensions = false;
						man.UpdateMode = UpdateMode.Foreground;
						man.CreateDesktopShortcut = true;

						man.AssemblyReferences.Clear();
						man.EntryPoint = man.AssemblyReferences.Add(appManifestFile);
						man.ResolveFiles(new string[] { deployFolder });
						man.UpdateFileInfo();
						string name = man.AssemblyIdentity.Name;
						if (name.EndsWith(AppExt))//potrebbe terminare per .app, prima della 3.5.2 era cosi
							name = name.Substring(0, name.Length - 4);
						man.EntryPoint.AssemblyIdentity.Name = name;
						man.EntryPoint.AssemblyIdentity.Version = man.AssemblyIdentity.Version;
						AdjustTargetPath(man.EntryPoint, rootPath);

						man.Validate();

						ManifestWriter.WriteManifest(man, deployFile);
						
						PerformStep(ref steps);

						//firmo il manifest di deploy
						logger.WriteLine("Signing {0}...", deployFile);
						SignFile(deployFile);
						PerformStep(ref steps);

						logger.WriteLine("Updating bootstrapper {0}...", deployFile);
						string url = GetAppsUrl(installationName, webHostName, webServicesPort);
						CreateBootstrapper(Path.GetFileName(deployFile), rootPath, appTitle, uiCulture, url, url, prerequisiteFolders.ToArray(), null);

					}
					catch (Exception ex)
					{
						logger.WriteLine(ex.ToString());
					}
					finally
					{
						//consumo eventuali step che non sono stati effettuati (alcuni setup hanno meno passi)
						while (steps > 0)
							PerformStep(ref steps);
					}
				}
				return true;
			}
			finally
			{
				UpdateInstallationVersion();

				//aspetto che il thread di preparazione del setup abbia finito
				if (setupAdjustmentThread != null)
					setupAdjustmentThread.Join();
			}
			
		}
		

		//--------------------------------------------------------------------------------
		private BaseReference AddFileToManifest(ApplicationManifest appManifest, string file, string appFile, List<string> addedFiles)
		{
			if (addedFiles.Contains(file, StringComparer.InvariantCultureIgnoreCase))
				return null;

			addedFiles.Add(file);
			BaseReference ar = IsAssembly(file, appFile)
				? (BaseReference)appManifest.AssemblyReferences.Add(file)
				: (BaseReference)appManifest.FileReferences.Add(file);

			AdjustTargetPath(ar, deployFolder);
			return ar;
		}

		//--------------------------------------------------------------------------------
		private void RemovePhantomReferences(IEnumerable list)
		{
			List<BaseReference> phantomAssemblyes = new List<BaseReference>();
			foreach (BaseReference ar in list)
				if (ar.TargetPath != null && !File.Exists(Path.Combine(deployFolder, ar.TargetPath)))
					phantomAssemblyes.Add(ar);

			foreach (BaseReference ar in phantomAssemblyes)
			{
				if (list is AssemblyReferenceCollection)
					((AssemblyReferenceCollection)list).Remove((AssemblyReference) ar);
				else if (list is FileReferenceCollection)
					((FileReferenceCollection)list).Remove((FileReference)ar);
			}
		}

		//--------------------------------------------------------------------------------
		private bool IsAssembly(string file, string appFile)
		{
			//prima scrematura in base all'estensione
			if ((string.Compare(Path.GetExtension(file), ".dll", true) != 0) && file != appFile)
				return false;

			//metodo becero per capire se sono di fronte a un assembly o un file di altro tipo (dll unmanaged o altro)
			try
			{
				AssemblyName name = AssemblyName.GetAssemblyName(file);
				return name != null;
			}
			catch (Exception)
			{

				return false;
			}
		}

		/// <summary>
		/// Copia i file dalla tbapps alla cartella di deploy per mantenere
		/// compatibilità all'indietro con la logica di deploy dei verticali
		/// </summary>
		/// <param name="debugOrRelease"></param>
		//--------------------------------------------------------------------------------
		private void CopyOldDeployFiles(string debugOrRelease)
		{
			string tbApps = Path.Combine(rootPath, string.Format("TBApps\\{0}", debugOrRelease));
			if (Directory.Exists(tbApps))
				CopyManifestFiles(tbApps, deployFolder, true);
		}

		//--------------------------------------------------------------------------------
		private List<string> DetectOrphanFiles(List<string> manifestFiles, List<string> payloadNames)
		{
			List<string> orphans = new List<string>();

			foreach (string ext in deployExt)
			{
				foreach (string file in Directory.GetFiles(deployFolder, "*" + ext, SearchOption.AllDirectories))
				{
					string relFile = file.Substring(deployFolder.Length + 1);

					if (manifestFiles.Exists((Predicate<string>)delegate(string s)
						{
							return string.Compare(relFile, s, StringComparison.InvariantCultureIgnoreCase) == 0;
						}))

						continue;

					if (orphans.Exists((Predicate<string>)delegate(string s)
					{
						return string.Compare(relFile, s, StringComparison.InvariantCultureIgnoreCase) == 0;
					}))
						continue;

                    string folderName = Path.GetDirectoryName(relFile);
                    while (folderName.IndexOf(Path.DirectorySeparatorChar) != -1)
                    {
                        folderName = Path.GetDirectoryName(folderName);
                    }
                    if (folderName != null && folderName.Trim().Length > 0 && payloadNames.Exists((Predicate<string>)delegate(string s)
                    {
                        return string.Compare(folderName, s, StringComparison.InvariantCultureIgnoreCase) == 0;
                    }))
                        continue;

					orphans.Add(relFile);
				}
			}
			return orphans;
		}

		//--------------------------------------------------------------------------------
        private void AddToFiles(ApplicationManifest magoManifest, List<string> manifestFiles, List<string> payloadNames)
		{
			foreach (AssemblyReference ar in magoManifest.AssemblyReferences)
				AddReferencedFile(manifestFiles, ar);

            foreach (FileReference fr in magoManifest.FileReferences)
            {
                if (String.Compare(Path.GetExtension(fr.TargetPath), payloadExt, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    payloadNames.Add(Path.GetFileNameWithoutExtension(fr.TargetPath));
                }
                AddReferencedFile(manifestFiles, fr);
            }
		}

		//--------------------------------------------------------------------------------
		private static void AddReferencedFile(List<string> manifestFiles, BaseReference ar)
		{
			if (ar.TargetPath == null)
				return;

			if (!manifestFiles.Exists
				(
					(Predicate<string>)delegate(string s)
					{
						return (s == ar.TargetPath);
					}
				))
				manifestFiles.Add(ar.TargetPath);
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Effettua il deploy dell'applicazione, generando i manifest da zero oppure semplicemente aggiornandoli
		/// </summary>
		private bool UpdateDeployment(
			string installationName,
			string fileHostName,
			string webHostName,
			string uiCulture,
			string appCulture,
			int webServicesPort,
			string debugOrRelease,
			bool multiInstance
			)
		{

			bool result = true;
			string appName = null;
			string appTitle = null;
			string publisher = null;
			string indexPath = Path.Combine(Path.GetDirectoryName(rootPath), indexPage);

			//cancello eventuali file della precedente versione di deploy (*.deploy)
			CleanOldDeploymentFiles();

			//se almeno uno dei due nomi di server (web o file system) non corrisponde 
			//a quello locale, devo generare il file installationinfo.xml a beneficio del 
			//path finder
			GenerateInstallationInfo(deployFolder, fileHostName, webHostName, installationName);

            var brandName = "Mago4";
            try
            {
                brandName = InstallationData.GenericBrand.ProductTitle;
            }
            catch
            {

            }
            var producerWebSite = "http://www.microarea.it";
            try
            {
                producerWebSite = InstallationData.GenericBrand.ProducerWebSite;
            }
            catch
            {

            }
            var producer = "Microarea";
            try
            {
                producer = InstallationData.GenericBrand.ProductPublisher;
            }
            catch
            {

            }
            var producerPng = "Apps/Images/LogoMicroarea.png";
            try
            {
                producerPng = InstallationData.GenericBrand.ProducerPng;
            }
            catch
            {

            }

            using (StreamWriter sw = new StreamWriter(indexPath))
			{
				using (HtmlTextWriter writer = new HtmlTextWriter(sw))
				{
					string template = Resource.Template
                        .Replace(titlePlaceholder, String.Format(Resource.Title, brandName))
                        .Replace(producerWebSitePlaceholder, producerWebSite)
                        .Replace(producerPlaceholder, producer)
                        .Replace(producerPngPlaceholder, producerPng)
                        .Replace(introPlaceholder, Resource.Intro);
					writer.Write(template.Substring(0, template.IndexOf(splitterToken)));

					string[] files = Directory.GetFiles(deployFolder, "*" + tbManifestExt);
					//li ordino in modo che per primo ci sia mago
					SortProductFiles(files);

					const int updateSteps = 3;
					logger.SetProgressTop(files.Length * updateSteps + fixedSteps);

					foreach (string configurationPath in files)
					{
						int steps = updateSteps;
						try
						{
							bool officeAddin = false;
							List<string> optionalFolders = null;
							List<string> requiredFolders = null;
							List<string> prerequisiteFolders = null;

							//leggo le informazioni per generare il setup
							if (!ReadSetupInfo(configurationPath, out appName, out appTitle, out publisher, out officeAddin, out optionalFolders, out requiredFolders, out prerequisiteFolders))
								return false;

							logger.WriteLine("Updating clickonce setup for application '{0}'...", appName);

							result = UpdateDeployment(
								appName,
								appTitle,
								installationName,
								webHostName,
								webServicesPort,
								uiCulture,
								debugOrRelease,
								publisher,
								officeAddin,
								prerequisiteFolders.ToArray(),
								multiInstance,
								writer,
								ref steps) && result;
						}
						catch (Exception ex)
						{
							logger.WriteLine(ex.ToString());
							result = false;
						}
						finally
						{
							//consumo eventuali step che non sono stati effettuati (alcuni setup hanno meno passi)
							while (steps > 0)
								PerformStep(ref steps);
						}
					}

					if (!DeployRemoteClients(fileHostName, webHostName, installationName))
						result = false;
					logger.PerformStep();

					writer.Write(template.Substring(template.IndexOf(splitterToken) + splitterToken.Length));

				}
			}
			return result;
		}
  
		//--------------------------------------------------------------------------------
		private bool DeployRemoteClients(string fileHostName, string webHostName, string installationName)
		{
			try
			{
				foreach (RemoteClient client in RemoteClient.Read(false))
				{
                    if (!RemoteClient.IsRemote(client))
                        continue;

                    SafeTokenHandle safeTokenHandle = RemoteClient.ImpersonateAs(client.User, client.Password);
                    using (safeTokenHandle)
					{
						// Use the token handle returned by LogonUser. 
						using (WindowsImpersonationContext impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle()))
						{

							string destination = "\\\\" + client.Host + "\\" + client.RemotePath.Replace(":", "$");
							logger.WriteLine("Updating remote client on '{0}'...", destination);
							string source = client.EasyLook 
								? BasePathFinder.BasePathFinderInstance.GetApplicationModulePath(NameSolverStrings.WebFramework, NameSolverStrings.EasyLook) 
								: deployFolder;
							CopyFolderTree(source, destination, client.EasyLook);
                            if (client.EasyLook)
                            {
                                string installationInfoDestination = Path.Combine(destination, "bin");
                                GenerateInstallationInfo(installationInfoDestination, fileHostName, webHostName, installationName);
						}
					}
				}
				}
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLine(ex.ToString());
				return false;
			}
		}

        //--------------------------------------------------------------------------------
        private static void DeleteFileSystemInfo(FileSystemInfo fileSystemInfo)
        {
            var directoryInfo = fileSystemInfo as DirectoryInfo;
            if (directoryInfo != null)
            {
                foreach (var childInfo in directoryInfo.GetFileSystemInfos())
                {
                    DeleteFileSystemInfo(childInfo);
                }
            }

            fileSystemInfo.Attributes = FileAttributes.Normal;
            fileSystemInfo.Delete();
        }
        
        //--------------------------------------------------------------------------------
		private void CopyFolderTree(string source, string destination, bool easyLook)
		{
			string realDestination = destination.TrimEnd('\\');
            destination = realDestination + "__tmp";

			try
			{
                if (Directory.Exists(destination))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(destination);
                    DeleteFileSystemInfo(directoryInfo);
                }
                else
                    Directory.CreateDirectory(destination);

                //Now Create all of the directories
				foreach (string dirPath in Directory.GetDirectories(source, "*",
					SearchOption.AllDirectories))
				{
					string folder = dirPath.Replace(source, destination);
					if (!Directory.Exists(folder))
					{
						logger.WriteLine("Creating {0}", folder);
						Directory.CreateDirectory(folder);
					}
				}

				//Copy all the files
				foreach (string newPath in Directory.GetFiles(source, "*.*",
					SearchOption.AllDirectories))
				{
					string file = newPath.Replace(source, destination);
					logger.WriteLine("Copying {0}", file);
					File.Copy(newPath, file, true);
				}

				if (Directory.Exists(realDestination))
				{
                    DirectoryInfo directoryInfo = new DirectoryInfo(realDestination);
                    DeleteFileSystemInfo(directoryInfo);
				}
				Directory.Move(destination, realDestination);
			}
			finally
			{
                if (Directory.Exists(destination))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(destination);
                    DeleteFileSystemInfo(directoryInfo);
                }
            }
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Cancello eventuali file della precedente versione di deploy (*.deploy)
		/// </summary>
		private void CleanOldDeploymentFiles()
		{
			foreach (string file in Directory.GetFiles(deployFolder, "*.deploy", SearchOption.AllDirectories))
				File.Delete(file);

            CleanOldDeploymentFiles(Path.Combine(rootPath, "Mago.Net"));
            CleanOldDeploymentFiles(Path.Combine(rootPath, "TbAppManager"));
			CleanOldDeploymentFiles(Path.Combine(rootPath, "TBApps"));
			CleanOldDeploymentFiles(Path.Combine(rootPath, "MicroareaConsole"));
			
			foreach (string file in Directory.GetFiles(rootPath, "Microarea.Library.TbDynamicWCFServices*.dll", SearchOption.AllDirectories))
				File.Delete(file);
			
		}

		//--------------------------------------------------------------------------------
		private void CleanOldDeploymentFiles(string folder)
		{
			if (!Directory.Exists(folder))
				return;
			
			//cancello i file temporanei
			DeleteDynamicFiles(folder);
			//se non rimane più nulla, tolgo anche la struttura di cartelle
			if (Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Length == 0)
				Directory.Delete(folder, true);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Effettua il deploy dell'applicazione, generando i manifest da zero oppure semplicemente aggiornandoli
		/// </summary>
		private bool Deploy(
			string installationName,
			string fileHostName,
			string webHostName,
			string uiCulture,
			string appCulture,
			int webServicesPort,
			string debugOrRelease,
			bool multiInstance
			)
		{
			if (!Directory.Exists(deployFolder))
				Directory.CreateDirectory(deployFolder);

			bool result = true;
			string appName = null;
			string appTitle = null;
			string publisher = null;

			string[] folders = Directory.GetDirectories(rootPath);
			const int deploySteps = 10;
			logger.SetProgressTop(folders.Length * deploySteps + fixedSteps);

			foreach (string folder in folders)
			{
				int steps = deploySteps;
				try
				{
					//ogni cartella di prodotto deve contenere il file che descrive come generare il manifest
					string configurationPath = Path.Combine(folder, string.Format("{0}\\{1}{2}", debugOrRelease, Path.GetFileName(folder), tbManifestExt));
					if (!File.Exists(configurationPath))
						continue;

					bool officeAddin = false;
					List<string> optionalFolders = null;
					List<string> requiredFolders = null;
					List<string> prerequisiteFolders = null;

					//leggo le informazioni per generare il setup
					if (!ReadSetupInfo(configurationPath, out appName, out appTitle, out publisher, out officeAddin, out optionalFolders, out requiredFolders, out prerequisiteFolders))
						return false;

					logger.WriteLine("Generating clickonce setup for application '{0}'...", appName);

					//genero i file di manifest
					result = Deploy(
						appName,
						appTitle,
						installationName,
						fileHostName,
						webHostName,
						webServicesPort,
						optionalFolders.ToArray(),
						requiredFolders.ToArray(),
						debugOrRelease,
						publisher,
						officeAddin,
						multiInstance,
						ref steps) && result;

					PerformStep(ref steps);

					//copio nella cartella di deploy il file di configurazione del setup a beneficio di
					//futuri aggiornamenti dei manifest
					File.Copy(configurationPath, Path.Combine(deployFolder, Path.GetFileName(configurationPath)));

				}
				finally
				{
					//consumo eventuali step che non sono stati effettuati (alcuni setup hanno meno passi)
					while (steps > 0)
						PerformStep(ref steps);
				}

			}
			return result;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Prepara il server per l'utilizzo WEB (genera il server connection se non esiste, inizializza LoginManager)
		/// </summary>
		/// <param name="uiCulture"></param>
		/// <param name="appCulture"></param>
		/// <param name="debugOrRelease"></param>
		/// <returns></returns>
		public Thread StartSetupServer(string uiCulture, string appCulture, int webServicesPort, bool registerWcf)
		{
			//lancio in un thread separato le operazioni di preparazione che non hanno nulla a che fare 
			//con la generazione dei manifest, cosi' miglioro le prestazioni
			ThreadStart ts = new ThreadStart(() =>
			{
				GenerateServerConnection(uiCulture, appCulture, webServicesPort);
				logger.PerformStep();

				//questo metodo registra i namespace WCF solo se non è già stato lanciato tbloader, se invece
				//è già stato lanciato, allora lo ha già fatto lui (motivi di performance)
				ReloadConfigurationAndRegisterWCF(webServicesPort, registerWcf);

				logger.PerformStep();

			});
			Thread setupAdjustmentThread = new Thread(ts);
			setupAdjustmentThread.Start();
			return setupAdjustmentThread;

		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Comunica a LoginMAnager di ricaricare la configurazione dei prodotti installati
		/// </summary>
		private void ReloadConfigurationAndRegisterWCF(int webServicesPort, bool registerWcf)
		{
			try
			{
				logger.WriteLine("Reloading product configuration...");
				LoginManager lm = new LoginManager(
					BasePathFinder.BasePathFinderInstance.LoginManagerUrl,
					300000);//cinque minuti di timeout

				lm.ReloadConfiguration();

				if (registerWcf)
					WCFServiceRegister.RegisterIstallation(Path.GetDirectoryName(rootPath), 10000, lm.GetAspNetUser(), logger);

			}
			catch (Exception ex)
			{
				logger.WriteLine(ex.ToString());
			}
		}

		//--------------------------------------------------------------------------------
		public static string GetAspNetUser()
		{
			LoginManager lm = new LoginManager(
						BasePathFinder.BasePathFinderInstance.LoginManagerUrl,
						300000);//cinque minuti di timeout
			return lm.GetAspNetUser();
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Lancia TBLoader in-process per fargli generare le dll dinamiche di WCF e registrare i namespace di WCF
		/// </summary>
		private bool GenerateWCFAssembly(string debugOrRelease, bool updating)
		{
			try
			{
				logger.WriteLine("Generating WCF assembly...");
				BasePathFinder pf = BasePathFinder.BasePathFinderInstance;
				//imposto la build per trovare la versione giusta di dll di tb da caricare
				pf.Build = debugOrRelease;
				TbApplicationClientInterface tb = new TbApplicationClientInterface("", pf, 0, "", WCFBinding.None);
				//TB deve registrare i namespace per l'utente che esegue ASPNET (altrimenti per motivi di security di WCF, TBLoader non si puo' 
				//mettere in ascolto per ricevere chiamate quando eseguito dai TBServices)
				//lo faccio solo se sto aggiornando le dll di un verticale
				tb.RegisterWCFNamespacesOnStart = updating;
				//faccio partire un tb che genera l'assembly WCF a partire da tutti i web methods.
				//non posso farlo separatamente perche' TB conosce tutta una serie di dettagli che mi servono e che 
				//sarebbe troppo oneroso separare in un algoritmo a se' stante (es. namespace degli oggetti, Dll associata ai namespace, ecc.)
				logger.WriteLine("Starting Task Builder Application...");
				ServiceClientCache.ServiceAssemblyFolder = pf.GetTBLoaderPath();
				tb.StartTbLoader("", true);
				logger.WriteLine("Task Builder Application started successfully");

				Diagnostic d = tb.GetApplicationContextDiagnostic(true);
				foreach (DiagnosticItem item in d.AllMessages())
					logger.WriteLine(item.FullExplain);
				//chiudo tb, adesso che ha generato l'assembly WCF non mi serve piu`
				logger.WriteLine("Task Builder Application Closing...");
				tb.CloseTB();
				logger.WriteLine("Task Builder Application Closed");

				if (d.Error)
				{
					logger.WriteLine("Error generating WCF assembly");
					return false;
				}
				logger.WriteLine("WCF assembly generated successfully");
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLine("Error generating WCF assembly: {0}", ex.Message);
				return false;
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiorna l'msi di Easylook aggiungendo il noem di installazione e del server
		/// </summary>
		/// <param name="installationName"></param>
		/// <param name="instPlaceHolder"></param>
		/// <param name="serverPlaceHolder"></param>
		/// <param name="msiFile"></param>
		private bool UpdateMSI(
			string installationName,
			string instPlaceHolder,
			string fileServerPlaceHolder,
			string webServerPlaceHolder,
			string msiFile,
			string fileHostName,
			string webHostName,
			bool multipleInstance)
		{
			Type InstallerType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
			Installer installer = null;
			Database database = null;
			WindowsInstaller.View view = null;
			try
			{
				//creo l'istanza dell'installer
				installer = (Installer)Activator.CreateInstance(InstallerType);
				
				//apro il database
				database = installer.OpenDatabase(msiFile, MsiOpenDatabaseMode.msiOpenDatabaseModeTransact);

				/*SummaryInfo info = database.SummaryInformation[1];
				object oo = info.get_Property(9);
				info.set_Property(9, Guid.NewGuid().ToString("B").ToUpper());
				info.Persist();*/
				
				//apro la tabella delle custo actions
				view = database.OpenView("SELECT * from CustomAction");
				view.Execute(null);
				while (true)
				{
					Record r = null;
					try
					{
						r = view.Fetch();
						if (r == null)
							break;
						//sostituisco i placeholder nel record della cusom action che genera il setupinfo.xml
						string field4 = r.get_StringData(4);
						//se trovo il record che contiene il segnaposto dell'installazione, lo sostituisco con i dati veri
						if (field4.IndexOf(instPlaceHolder) != -1)
						{
							field4 = field4.Replace(instPlaceHolder, installationName);
							field4 = field4.Replace(fileServerPlaceHolder, fileHostName);
							field4 = field4.Replace(webServerPlaceHolder, webHostName);
							r.set_StringData(4, field4);
							view.Modify(MsiViewModify.msiViewModifyReplace, r);
							break;
						}
					}
					finally
					{
						if (r != null)
							Marshal.ReleaseComObject(r);
					}
				}
				view.Close();

				Marshal.ReleaseComObject(view);

				//aggiorno il nome prodotto
				view = database.OpenView("SELECT * from Property where Property='ProductName'");
				view.Execute(null);

				Record rv = view.Fetch();
				if (rv != null)
				{
					string name = rv.get_StringData(2);
					rv.set_StringData(2, MakeSpecificTitle(name, webHostName, multipleInstance ? installationName : null));
					view.Modify(MsiViewModify.msiViewModifyUpdate, rv);

					Marshal.ReleaseComObject(rv);
				}
				view.Close();

				/*
				//aggiorno il numero di versione
				view = database.OpenView("SELECT * from Property where Property='ProductVersion'");
				view.Execute(null);

				rv = view.Fetch();
				if (rv != null)
				{
					Version v = Assembly.GetExecutingAssembly().GetName().Version;
					rv.set_StringData(2, string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Revision));
					view.Modify(MsiViewModify.msiViewModifyUpdate, rv);

					Marshal.ReleaseComObject(rv);
				}
				view.Close();


				//aggiorno il product code
				view = database.OpenView("SELECT * from Property where Property='ProductCode'");
				view.Execute(null);

				rv = view.Fetch();
				if (rv != null)
				{
					rv.set_StringData(2, Guid.NewGuid().ToString("B").ToUpper());
					view.Modify(MsiViewModify.msiViewModifyUpdate, rv);

					Marshal.ReleaseComObject(rv);
				}
				view.Close();*/
				database.Commit();
			}
			catch (Exception ex)
			{
				logger.WriteLine("Error updating msi: '{0}'", ex.Message);
				return false;
			}
			finally
			{
				if (view != null)
					Marshal.ReleaseComObject(view);
				if (database != null)
					Marshal.ReleaseComObject(database);
				if (installer != null)
					Marshal.ReleaseComObject(installer);
		}

			return true;
		}

		//--------------------------------------------------------------------------------
		private void SortProductFiles(string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];
                if (string.Compare(Path.GetFileNameWithoutExtension(file), "TbAppManager", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (i > 0)
					{
						files[i] = files[0];
						files[0] = file;
					}
					continue;
				}
				if (string.Compare(Path.GetFileNameWithoutExtension(file), "MicroareaConsole", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (i > 1)
					{
						files[i] = files[1];
						files[1] = file;
					}
					continue;
				}
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Legge le informazioni di setup dai file clientsetup.xml
		/// </summary>
		private bool ReadSetupInfo(
					string configurationPath,
					out string appName,
					out string appTitle,
					out string publisher,
					out bool officeAddin,
					out List<string> optionalFolders,
					out List<string> requiredFolders,
					out List<string> prerequisiteFolders
					)
		{
			optionalFolders = new List<string>();
			requiredFolders = new List<string>();
			prerequisiteFolders = new List<string>();
			appName = null;
			appTitle = null;
			publisher = null;
			officeAddin = false;


			try
			{
				using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ManifestGenerator.ClientSetup.xsd"))
				{
					using (XmlTextReader tr = new XmlTextReader(s))
					{
						//Implement the reader. 
						XmlReaderSettings settings = new XmlReaderSettings();
						settings.Schemas.Add(clientSetupNamespace, tr);
						settings.ValidationType = ValidationType.Schema;
						using (XmlReader reader = XmlReader.Create(configurationPath, settings))
						{
							bool isPrerequisite = false;
							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									switch (reader.LocalName)
									{
										case "Folders":
											{
												isPrerequisite = false;
												break;
											}
										case "Prerequisites":
											{
												isPrerequisite = true;
												break;
											}
										case "Application":
											{
												while (reader.MoveToNextAttribute())
													switch (reader.LocalName)
													{
														case "name": appName = reader.Value; break;
														case "title": appTitle = reader.Value; break;
														case "publisher": publisher = reader.Value; break;
														case "officeAddin": officeAddin = ParseXmlBool(reader.Value); break;
                                                        case "prependPublisherToAppTitle":
                                                            {
                                                                if (String.Compare(reader.Value, "[PrependPublisherToAppTitle]", StringComparison.InvariantCultureIgnoreCase) == 0)
                                                                {
                                                                    prependPublisherToAppTitle = ParseXmlBool(InstallationData.GenericBrand.PrependPublisherToAppTitle);
                                                                }
                                                                else
                                                                {
                                                                    prependPublisherToAppTitle = ParseXmlBool(reader.Value);
                                                                }
                                                                break;
                                                            }
													}
                                                if (String.Compare(appTitle, "[ProductTitle]", StringComparison.InvariantCultureIgnoreCase) == 0)
                                                {
                                                    appTitle = InstallationData.GenericBrand.ProductTitle;
                                                }
                                                if (String.Compare(publisher, "[ProductPublisher]", StringComparison.InvariantCultureIgnoreCase) == 0)
                                                {
                                                    publisher = InstallationData.GenericBrand.ProductPublisher;
                                                }
                                                break;
											}
										case "Folder":
											{
												string folderName = null;
												bool optional = false;
												while (reader.MoveToNextAttribute())
													switch (reader.LocalName)
													{
														case "name": folderName = reader.Value; break;
														case "optional": optional = ParseXmlBool(reader.Value); break;
													}
												if (isPrerequisite)
													prerequisiteFolders.Add(folderName);
												else if (optional)
													optionalFolders.Add(folderName);
												else
													requiredFolders.Add(folderName);
												break;
											}
									}
								}
							}
						}
					}
				}

			}
			catch (XmlException XmlExp)
			{
				logger.WriteLine("Error reading file {0}: {1}", configurationPath, XmlExp.Message);
				return false;
			}
			//XMLSchemaExceptions are thrown when the XML document does not match the schema provided.
			catch (XmlSchemaException XmlSchExp)
			{
				logger.WriteLine("Invalid content for file {0}: {1}", configurationPath, XmlSchExp.Message);
				return false;
			}
			//Catch all other exceptions and report them to the user:
			catch (Exception GenExp)
			{
				logger.WriteLine("Error reading file {0}: {1}", configurationPath, GenExp.Message);
				return false;
			}

            if (this.prependPublisherToAppTitle && !string.IsNullOrEmpty(publisher))
				appTitle = string.Format("{0} {1}", publisher, appTitle);

			return true;
		}

		//--------------------------------------------------------------------------------
		private bool GenerateWebConfig()
		{
			string filePath = Path.Combine(Path.GetDirectoryName(rootPath), "Apps\\Publish\\web.config");
			if (!File.Exists(filePath))
			{
				string folder = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				XmlDocument doc = new XmlDocument();
				using (MemoryStream ms = new MemoryStream(Resource.web_config))
					doc.Load(ms);

				doc.Save(filePath);
			}

			return true;
		}

        //--------------------------------------------------------------------------------
        private bool GenerateWebConfigForMimeTypes()
        {
            var instanceDirInfo = new DirectoryInfo(Path.GetDirectoryName(rootPath));
            var webConfigFileInfo = new FileInfo(Path.Combine(instanceDirInfo.FullName, "web.config"));

            if (webConfigFileInfo.Exists)
            {
                File.Move(
                    webConfigFileInfo.FullName,
                    Path.Combine(
                        instanceDirInfo.FullName,
                        string.Format(CultureInfo.InvariantCulture, "web.config.{0}", DateTime.Now.ToString("yyyyMMddHHmmss"))
                        )
                    );
            }

            XmlDocument doc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(Resource.MimeTypesWeb_config))
            {
                doc.Load(ms);
            }

            doc.Save(webConfigFileInfo.FullName);

            return true;
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Genera il file ServerConnection.config (se non esiste)
        /// </summary>
        private bool GenerateServerConnection(string uiCulture, string appCulture, int webServicesPort)
		{
			string filePath = Path.Combine(Path.GetDirectoryName(rootPath), "Custom\\ServerConnection.config");
            XmlDocument doc = new XmlDocument();
            if (!File.Exists(filePath))
			{
				string folder = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(folder))
					Directory.CreateDirectory(folder);

				using (MemoryStream ms = new MemoryStream(Resource.ServerConnection))
					doc.Load(ms);
			}
            else
            {
                doc.Load(filePath);
            }

            XmlElement el = doc.DocumentElement.GetElementsByTagName("PreferredLanguage")[0] as XmlElement;
            el.SetAttribute("value", uiCulture);
            el = doc.DocumentElement.GetElementsByTagName("ApplicationLanguage")[0] as XmlElement;
            el.SetAttribute("value", appCulture);

            el = doc.DocumentElement.GetElementsByTagName("WebServicesPort")[0] as XmlElement;
            el.SetAttribute("value", webServicesPort.ToString(CultureInfo.InvariantCulture));

            doc.Save(filePath);

            return true;
		}

		//--------------------------------------------------------------------------------
		public static int ReadPortFromServerConnection(string rootPath)
		{
			try
			{
				string filePath = Path.Combine(Path.GetDirectoryName(rootPath), "Custom\\ServerConnection.config");
				if (!File.Exists(filePath))
					return 80;
				XmlDocument doc = new XmlDocument();
				doc.Load(filePath);
				XmlElement el = doc.DocumentElement.GetElementsByTagName("WebServicesPort")[0] as XmlElement;
				return int.Parse(el.GetAttribute("value"));
			}
			catch
			{
				return 80;
			}
		}
		//--------------------------------------------------------------------------------
		private static bool ParseXmlBool(string val)
		{
            return string.Compare(val, "true", StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(val, "1", StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		/// <summary>
		/// Crea il pacchetto di deploy per click once
		/// </summary>
		/// <param name="appName">nome dell'applicazione</param>
		/// <param name="appTitle">titolo dell'applicazione</param>
		/// <param name="installationName">nome dell'installazione</param>
		/// <param name="appCulture">Lingua dell'applicazione</param>
		/// <param name="requiredFolders">array di cartelle contenenti gli eseguibili e le dll installati di default</param>
		/// <param name="optionalFolders">array di cartelle contenenti gli eseguibili e le dll installati 'on demand'</param>
		/// <param name="debugOrRelease">stringa che indica se devono essere prese le componenti di debug o di release</param>
		/// <param name="publisher">nome dell'azienda che effettua la pubblicazione del prodotto</param>
		/// <param name="prerequisitesPath">percorso esterno da cui copiare i prerequisiti</param>
		//--------------------------------------------------------------------------------
		public bool Deploy(string appName,
			string appTitle,
			string installationName,
			string fileHostName,
			string webHostName,
			int port,
			string[] optionalFolders,
			string[] requiredFolders,
			string debugOrRelease,
			string publisher,
			bool officeAddin,
			bool multipleInstance,
			ref int steps
			)
		{
			try
			{
				string deployFile;
				string url;
				deployFile = officeAddin
								? Path.Combine(rootPath, string.Format("{0}\\{1}\\{0}.vsto", appName, debugOrRelease))
								: Path.Combine(rootPath, string.Format("{0}.application", appName));

				url = GetAppsUrl(installationName, webHostName, port);				//nel caso degli addin di office, occorre sempre e solo aggiornare
				//i file di manifest, senza crearli da zero (li creiamo durante la fase 
				//di masterizzazione
				return officeAddin
					 ? DeployAddin( appName, ref steps, deployFile )
					: DeployApplication(
						appName,
						appTitle,
						installationName,
						optionalFolders,
						requiredFolders,
						debugOrRelease,
						publisher,
						multipleInstance,
						ref steps,
						deployFile,
						url
						);
			}
			catch (Exception ex)
			{
				logger.WriteLine(ex.ToString());
				return false;
			}
			finally
			{

			}
		}

		//--------------------------------------------------------------------------------
		private bool DeployApplication(
			string appName,
			string appTitle,
			string installationName,
			string[] optionalFolders,
			string[] requiredFolders,
			string debugOrRelease,
			string publisher,
			bool multipleInstance,
			ref int steps,
			string deployFile,
			string url
			)
		{
			//aggiungo i file contenuti nelle cartelle di assembly 'on demand'
			List<ManifestFile> optionalFiles = new List<ManifestFile>();
			foreach (string sourceFolder in optionalFolders)
				foreach (string ext in deployExt)
					optionalFiles.AddRange(CopyFiles(sourceFolder, deployFolder, GetSourcePath(debugOrRelease, sourceFolder), ext, ""));

			//aggiungo i file contenuti nelle cartelle di assembly 'obbligatori'
			List<ManifestFile> requiredFiles = new List<ManifestFile>();
			foreach (string sourceFolder in requiredFolders)
				foreach (string ext in deployExt)
					requiredFiles.AddRange(CopyFiles(sourceFolder, deployFolder, GetSourcePath(debugOrRelease, sourceFolder), ext, ""));

			//determino la versione dell'eseguibile di applicazione
			string appFileName = string.Format("{0}.exe", appName);
			string appFileNamePath = Path.Combine(deployFolder, appFileName);
			if (!File.Exists(appFileNamePath))
				throw new ApplicationException(string.Format("Invalid application: '{0}'", appName));

			string appVersion = CalculateAppVersion(appFileNamePath);

			string iconFile = CreateIconFile(appName, appFileNamePath);

			//creo il file di manifest dell'applicazione a partire dalla cartella di lavoro creata in precedenza
			string appFile = Path.Combine(deployFolder, string.Format("{0}.exe.manifest", appName));
			logger.WriteLine("Generating application manifest for {0}...", appTitle);
			
			ApplicationManifest man = new ApplicationManifest();
			List<string> addedFiles = new List<string>();
			foreach (ManifestFile file in requiredFiles)
			{
				BaseReference br = AddFileToManifest(man, Path.Combine(deployFolder, file.Name), appFileNamePath, addedFiles);
				if (br == null)
					continue;
				br.Group = file.Group;
				if (br.SourcePath == appFileNamePath)
					man.EntryPoint = (AssemblyReference) br;
			}

			foreach (ManifestFile file in optionalFiles)
				AddFileToManifest(man, Path.Combine(deployFolder, file.Name), appFile, addedFiles);

			man.AssemblyIdentity.Version = appVersion;
			
			man.AssemblyIdentity.Name = appTitle;
			man.AssemblyIdentity.ProcessorArchitecture = "x86";
			man.TrustInfo = new TrustInfo();
			man.TrustInfo.PermissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.Unrestricted);
			man.UseApplicationTrust = true;
			man.Publisher = publisher;
			man.Product = appTitle;
			
			//aggiungo il carattere di opzionale ad alcuni assembly nel manifest
			AddReferencesAttribute(man, optionalFiles.ToArray(), requiredFiles.ToArray());

			//aggiungo il nome del file di icona
			man.IconFile = Path.GetFileName(iconFile);
			man.FileReferences.Add(iconFile);

			//aggiorna le informazioni del file di applicazione
			man.ResolveFiles();
			man.UpdateFileInfo();
			man.UpdateAssemblyVersions();
			man.Validate();
			//salva il file modificato
			ManifestWriter.WriteManifest(man, appFile);
			PerformStep(ref steps);

			//firmo il manifest di applicazione
			logger.WriteLine("Signing {0}...", appFile);
			SignFile(appFile);
			PerformStep(ref steps);

			//cancello la cartella di lavoro
			//Directory.Delete(tmpFolder, true);

			return true;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Effettua l'aggiornamento dei manifest di deployment
		/// </summary>
		private bool UpdateDeployment(
			string appName,
			string appTitle,
			string installationName,
			string webHostName,
			int port,
			string uiCulture,
			string debugOrRelease,
			string publisher,
			bool officeAddin,
			string[] prerequisiteFolders,
			bool multipleInstance,
			HtmlTextWriter writer,
			ref int steps
			)
		{
			string deployFile, url;
			deployFile = officeAddin
								? Path.Combine(rootPath, string.Format("{0}.vsto", appName))
								: Path.Combine(rootPath, string.Format("{0}.application", appName));

            url = GetAppsUrl(installationName, webHostName, port);
     


			return officeAddin
				? UpdateAddin(
						appName,
						appTitle,
						installationName,
						webHostName,
						uiCulture,
						debugOrRelease,
						prerequisiteFolders,
						writer,
						multipleInstance,
						ref steps,
						deployFile,
						url,
						publisher
						)
				: UpdateApplication(
						appName,
						appTitle,
						uiCulture,
						prerequisiteFolders,
						ref steps,
						deployFile,
						url,
						webHostName,
						installationName,
						multipleInstance,
						publisher,
						writer
						);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Calcola il nome del file di deploy e l'url di deploy a partire dalle informazioni ricevute in ingresso
		/// </summary>
		private void CalculateDeployData(string appName, string installationName, string webHostName, int port, string debugOrRelease, bool officeAddin, out string deployFile, out string url)
		{
			deployFile = officeAddin
								? Path.Combine(rootPath, string.Format("{0}\\{1}\\{0}.vsto", appName, debugOrRelease))
								: Path.Combine(rootPath, string.Format("{0}.application", appName));

			url = GetAppsUrl(installationName, webHostName, port);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiorna i manifest delle applicazioni
		/// </summary>
		private bool UpdateApplication(
			string appName,
			string appTitle,
			string uiCulture,
			string[] prerequisiteFolders,
			ref int steps,
			string deployFile,
			string url,
			string webHostName,
			string installationName,
			bool multipleInstance,
			string publisher,
			HtmlTextWriter writer
			)
		{
			string appFileName = Path.Combine(deployFolder, string.Format("{0}.exe", appName));
			string appFileManifest = Path.Combine(deployFolder, string.Format("{0}.exe.manifest", appName));

			if (!File.Exists(appFileName))
				throw new ApplicationException(string.Format("Invalid application: '{0}'", appName));

			CreateIndexPageImage(appName, appFileName);
			PerformStep(ref steps);

			//aggiorno il manifesto di applicazione e di deploy
			UpdateAppManifest(
				appFileName,
				appTitle,
				deployFile,
				appFileManifest,
				webHostName,
				installationName,
				multipleInstance,
				publisher,
				url);
			PerformStep(ref steps);

			//creo il bootstrapper
			CreateBootstrapper(Path.GetFileName(deployFile), rootPath, appTitle, uiCulture, url, url, prerequisiteFolders, writer);

			PerformStep(ref steps);
			return true;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiorna i manifest delle applicazioni
		/// </summary>
		private void UpdateAppManifest(
			string appFileName,
			string appTitle,
			string deployFile,
			string appManifest,
			string webHostName,
			string installationName,
			bool multipleInstance,
			string publisher,
			string url
			)
		{
			logger.WriteLine("Updating application manifest {0}...", appManifest);

			ApplicationManifest appMan = ManifestReader.ReadManifest(appManifest, true) as ApplicationManifest;

			string version = CalculateAppVersion(appFileName);
			string specificTitle = MakeSpecificTitle(appTitle, webHostName, multipleInstance ? installationName : null);
			//per ogni nodo di tipo assemblyIdentity, prendo quello che contiene il nome della dll dell'addin e vi inietto il nome del server 
			//e del producer

			string appName = Path.GetFileNameWithoutExtension(deployFile);
			string exeName = appName + ".exe";
			appMan.AssemblyIdentity.Name = specificTitle;
			appMan.AssemblyIdentity.Version = version;
			appMan.Description = specificTitle;
			appMan.Product = specificTitle;
			appMan.AssemblyIdentity.Culture = "neutral";

			AddInstallationInfoIfNeeded(appMan);

			logger.WriteLine("Removing old files...");

			RemovePhantomReferences(appMan.AssemblyReferences);
			RemovePhantomReferences(appMan.FileReferences);
			appMan.ResolveFiles(new string[] { deployFolder });
			appMan.UpdateFileInfo();
			appMan.UpdateAssemblyVersions();

			ManifestWriter.WriteManifest(appMan, appManifest);
			//firmo il manifest di applicazione
			logger.WriteLine("Signing {0}...", appManifest);
			SignFile(appManifest);

			logger.WriteLine("Generating deployment manifest for {0}...", specificTitle);

			string realName = specificTitle;
			DeployManifest man;
			if (File.Exists(deployFile))//se esiste gia`, leggo quello e recupero il name dall'assembly identity (prima della 3.5.2 era diverso e questo causa problemi di aggiornamento)
			{
				man = (DeployManifest)ManifestReader.ReadManifest(deployFile, true);
				realName = man.AssemblyIdentity.Name;
			}
					
			man = new DeployManifest(".NETFramework,Version=v4.0");
			man.AssemblyIdentity.Name = realName;
			man.AssemblyIdentity.Version = version;
			man.MapFileExtensions = false;
			man.AssemblyIdentity.ProcessorArchitecture = "x86";
			man.AssemblyIdentity.Culture = "neutral";
			man.Publisher = publisher;
			man.Product = specificTitle;
			man.CreateDesktopShortcut = true;
			man.Install = true;
			man.UpdateEnabled = true;
			man.UpdateMode = UpdateMode.Foreground;
			man.MapFileExtensions = false;
			man.MinimumRequiredVersion = version;
			man.DeploymentUrl = url + appName + ".application";
			man.EntryPoint = man.AssemblyReferences.Add(appManifest);

			man.ResolveFiles(new string[] { rootPath });
			man.UpdateFileInfo();
			AdjustTargetPath(man.EntryPoint, rootPath);
			man.Validate();
			//creo il manifest di deploy
			ManifestWriter.WriteManifest(man, deployFile);
			
			//firmo il manifest di deploy
			logger.WriteLine("Signing {0}...", deployFile);
			SignFile(deployFile);
		}

		//--------------------------------------------------------------------------------
		private void AdjustTargetPath(BaseReference baseReference, string rootPath)
		{
			string path = baseReference.ResolvedPath;
			if (string.IsNullOrEmpty(path))
				path = baseReference.SourcePath;
			if (string.IsNullOrEmpty(path))
				return;
			baseReference.TargetPath = path.Substring(rootPath.Length + 1);
		}

		//--------------------------------------------------------------------------------
		private bool AddInstallationInfoIfNeeded(ApplicationManifest man)
		{
			string installationInfofile = Path.Combine(deployFolder, InstallationInfo.FileName);
			if (!File.Exists(installationInfofile))
				return false;

			
			//aggiorna le informazioni del file di applicazione
			foreach (FileReference f in man.FileReferences)
				if (string.Compare(f.TargetPath, InstallationInfo.FileName, StringComparison.InvariantCultureIgnoreCase) == 0)
					return false;
			
			FileReference fr = man.FileReferences.Add(installationInfofile);
			man.ResolveFiles();
			man.UpdateFileInfo();

			return true;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Calcola la versione dell'applicazione, i primi due segmenti dipendono 
		/// dalla versione dell'eseguibile dell'applicativo, gli ultimi due token dipendono dall'ora di aggiornamento
		/// </summary>
		/// <param name="appFileName">Nome del file di applicazione</param>
		/// <returns></returns>
		private static string CalculateAppVersion(string appFileName)
		{
			AssemblyName asm = AssemblyName.GetAssemblyName(appFileName);

			// giorni trascorsi dal 1 gennaio 2009. Siccome ho a disposizione 65535 numeri, questo algoritmo non
			// funzionera` piu` fra 179 anni e mezzo. Siccome e' presumibile che in quella data
			// nessuno abbia interesse a venirmi a cercare, me la rischio...
			DateTime start = DateTime.Now;
			TimeSpan daysElapsed = start - new DateTime(2009, 1, 1);
			TimeSpan minutesElapsed = start - new DateTime(start.Year, start.Month, start.Day);
			//Il quarto numero deve essere progressivo, in quanto anche se l'installazione di mago non
			//e` cambiata, qualche verticale potrebbe aver aggiunto dll quindi la versione del manifest deve essere maggiore
			string appVersion = string.Format("{0}.{1}.{2}.{3}", asm.Version.Major, asm.Version.Minor, daysElapsed.Days, Convert.ToInt32(minutesElapsed.TotalMinutes));
			return appVersion;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Genera i manifest degli addin di Office nella giusta cartella e sposta le dll nella cartella di deploy
		/// </summary>
		private bool DeployAddin(string appName, ref int steps, string deployFile)
		{
			string dllPath = Path.GetDirectoryName(deployFile);
			//copio i file dell'addin nella cartella di deploy
			CopyManifestFiles(dllPath, deployFolder, true);
			string addinDll = string.Format("{0}.dll", appName);
			//determino la versione dell'eseguibile di applicazione
			string dllFileName = Path.Combine(deployFolder, addinDll);
			if (!File.Exists(dllFileName))
				throw new ApplicationException(string.Format("Invalid application: '{0}', file not found: '{1}'", appName, dllFileName));

			string targetManifestFile = dllFileName + ".manifest";
			string sourceManifestFile = Path.Combine(dllPath, addinDll + ".manifest");

			//copio il file di manifest di applicazione nella cartella di deploy
			File.Copy(sourceManifestFile, targetManifestFile, true);

			string targetManifestPath = Path.Combine(rootPath, Path.GetFileName(deployFile));
			File.Copy(deployFile, targetManifestPath, true);

			PerformStep(ref steps);
			return true;
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Genera i manifest degli addin di Office nella giusta cartella e sposta le dll nella cartella di deploy
		/// </summary>
		private bool UpdateAddin(
					string appName,
					string appTitle,
					string installationName,
					string webHostName,
					string uiCulture,
					string debugOrRelease,
					string[] prerequisiteFolders,
					HtmlTextWriter writer,
					bool multipleInstance,
					ref int steps,
					string deployFile,
					string url,
					string publisher
			)
		{
			string dllPath = Path.GetDirectoryName(deployFile);
			//determino la versione dell'eseguibile di applicazione
			string dllFileName = Path.Combine(deployFolder, string.Format("{0}.dll", appName));
			if (!File.Exists(dllFileName))
				throw new ApplicationException(string.Format("Invalid application: '{0}'", appName));
			//aggiorno il manifesto di office per rendere opzionali i dizionari
			UpdateAddinManifest(appTitle, deployFile, deployFolder, dllFileName + ".manifest", webHostName, multipleInstance, installationName, publisher);
			PerformStep(ref steps);

			CreateIndexPageImage(appName, dllFileName);
			PerformStep(ref steps);

			//creo il bootstrapper
			CreateBootstrapper(Path.GetFileName(deployFile), rootPath, appTitle, uiCulture, url, url, prerequisiteFolders, writer);

			PerformStep(ref steps);
			return true;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Genera il file xml contenente le informazioni per le connessioni ai server remoti (utile in caso di clustering)
		/// </summary>
		/// <param name="tmpFolder"></param>
		/// <param name="fileHostName"></param>
		/// <param name="webHostName"></param>
		/// <param name="installationName"></param>
		private void GenerateInstallationInfo(string tmpFolder, string fileHostName, string webHostName, string installationName)
		{
			InstallationInfo info = new InstallationInfo(webHostName, fileHostName, installationName);
			info.SaveToFolder(tmpFolder);
		}

		//--------------------------------------------------------------------------------
		private string MakeSpecificTitle(string appTitle, string hostName, string installationName)
		{
			string end = string.IsNullOrEmpty(installationName)
				? string.Format(" ({0})", hostName)
				: string.Format(" ({0} - {1})", hostName, installationName);

			if (!appTitle.EndsWith(end))
				return appTitle + end;

			return appTitle;
		}

		//--------------------------------------------------------------------------------
		private void PerformStep(ref int steps)
		{
			logger.PerformStep();
			steps--;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiorna i manifest degli addin di Office
		/// </summary>
		private void UpdateAddinManifest(
			string appTitle,
			string deployFile,
			string dllPath,
			string appManifest,
			string hostName,
			bool multipleInstance,
			string installationName,
			string publisher
			)
		{
			logger.WriteLine("Updating application manifest {0}...", appManifest);

			ApplicationManifest appMan = ManifestReader.ReadManifest(appManifest, true) as ApplicationManifest;

			AddInstallationInfoIfNeeded(appMan);

			//per ogni nodo di tipo assemblyIdentity, prendo quello che contiene il nome della dll dell'addin e vi inietto il nome del server 
			//e del producer
			string appFileName = Path.Combine(dllPath, Path.GetFileNameWithoutExtension(deployFile) + ".dll");
			string dllName = Path.GetFileName(appFileName);

			string version = CalculateAppVersion(appFileName);
			string title = MakeSpecificTitle(appTitle, hostName, multipleInstance ? installationName : null);
			appMan.ResolveFiles();
			appMan.UpdateFileInfo();
			//aggiorno nome e versione dell'applicazione
			appMan.AssemblyIdentity.Name = title;
			appMan.AssemblyIdentity.Version = version;
			appMan.AssemblyIdentity.Culture = "neutral";
			
			//per ogni nodo di tipo dependency, se si trova in una sottocartella
			//significa che è un file di risorsa, allora lo marco opzionale e gli assegno il gruppo
			//della culture di appartenenza
			foreach (BaseReference ar in appMan.AssemblyReferences)
				SetOptionalIfDictionary(ar);
			foreach (BaseReference ar in appMan.FileReferences)
				SetOptionalIfDictionary(ar);
			
			ManifestWriter.WriteManifest(appMan, appManifest);

			//firmo il manifest di applicazione
			logger.WriteLine("Signing {0}...", appManifest);
			SignFile(appManifest);

			logger.WriteLine("Updating deploy manifest {0}...", deployFile);

			//aggiorno il manifest di deploy perche la signature del manifest di applicazione e` cambiata
			DeployManifest man = (DeployManifest)ManifestReader.ReadManifest(deployFile, true);

			man.AssemblyIdentity.Name = title; 
			man.AssemblyIdentity.Version = version; 
			man.AssemblyIdentity.PublicKeyToken = appMan.AssemblyIdentity.PublicKeyToken;
			man.AssemblyIdentity.Culture = "neutral";
			man.UpdateMode = UpdateMode.Foreground;
			man.Install = false;
			man.AssemblyReferences.Clear();
			man.EntryPoint = man.AssemblyReferences.Add(appManifest);

			man.ResolveFiles(new string[] { deployFolder });
			man.UpdateFileInfo();
			man.EntryPoint.AssemblyIdentity.Name = title;
			man.EntryPoint.AssemblyIdentity.Version = version;
			AdjustTargetPath(man.EntryPoint, rootPath);
			man.Publisher = publisher;
			man.Product = title;
			man.Validate();
			ManifestWriter.WriteManifest(man, deployFile);

			//firmo il manifest di deploy
			logger.WriteLine("Signing {0}...", deployFile);
			SignFile(deployFile);
		}
	
		//--------------------------------------------------------------------------------
		private string GetSourcePath(string debugOrRelease, string sourceFolder)
		{
			return Path.Combine(rootPath, sourceFolder + Path.DirectorySeparatorChar + debugOrRelease);
		}

		//--------------------------------------------------------------------------------
		private static string GetAppsUrl(string installationName, string hostName, int port)
		{
			return GetInstallationUrl(installationName, hostName, port) + "Apps/";
		}
		//--------------------------------------------------------------------------------
		private static string GetInstallationUrl(string installationName, string hostName, int port)
		{
			return string.Format("http://{0}:{1}/{2}/", hostName, port, installationName);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Installa i prerequisiti se non presenti
		/// </summary>
		private void InstallPrerequisites(string prerequisitesPath)
		{
			string prerequisitesFolder = Path.Combine(rootPath, "Prerequisites");
			if (!Directory.Exists(prerequisitesFolder))
			{
				logger.WriteLine("Installing prerequisites...");
				//provvisorio: li pesco dalla mia macchina (dovremo renderla disponibile ai rivenditori...)
				CopyDirectory(prerequisitesPath, prerequisitesFolder);
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Controlla l'esistenza in locale di tutti i prerequisiti
		/// </summary>
		private bool ExistAllPrerequisites(string packagesUrl)
		{
			return File.Exists(Path.Combine(packagesUrl, "vcredist_x86\\vcredist_x86.exe"))
				&& File.Exists(Path.Combine(packagesUrl, "WindowsInstaller3_1\\WindowsInstaller-KB893803-v2-x86.exe"))
				&& File.Exists(Path.Combine(packagesUrl, "DotNetFX35SP1\\dotnetfx35.exe"));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Copia ricorsivamente la directory
		/// </summary>
		private void CopyDirectory(string source, string target)
		{
			if (!Directory.Exists(target))
				Directory.CreateDirectory(target);

			foreach (string file in Directory.GetFiles(source))
				File.Copy(file, Path.Combine(target, Path.GetFileName(file)));

			foreach (string folder in Directory.GetDirectories(source))
				CopyDirectory(folder, Path.Combine(target, Path.GetFileName(folder)));
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Genera il file che effettua l'installazione dei prerequisiti (setup.exe)
		/// </summary>
		/// <param name="appFile">Nome del file di applicazione</param>
		/// <param name="appTitle">Titolo dell'applicazione</param>
		/// <param name="appCulture">Lingua dell'applicazione</param>
		private void CreateBootstrapper(string appFileName, string outPath, string appTitle, string uiCulture, string appUrl, string url, string[] prerequisiteFolders, HtmlTextWriter writer)
		{
			logger.WriteLine("Generating bootstrapper...");
			string componentsUrl = Path.Combine(url, "Prerequisites/Packages/");
			//creo il file di progetto template da modificare
			string projectFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup.csproj");
			using (FileStream fs = new FileStream(projectFile, FileMode.Create, FileAccess.Write))
				fs.Write(Resource.Setup_csproj, 0, Resource.Setup_csproj.Length);

			string componentsLocalPath = ComponentsLocalPath;
			//cambio gli attributi del file template in base ai dati effettivi di installazione
			XmlDocument doc = new XmlDocument();
			doc.Load(projectFile);
			XmlNamespaceManager mng = new XmlNamespaceManager(doc.NameTable);
			mng.AddNamespace("ms", msBuildNamespace);
			XmlElement el = doc.SelectSingleNode("//ms:GenerateBootstrapper", mng) as XmlElement;
			el.SetAttribute("ApplicationFile", Path.GetFileName(appFileName));
			el.SetAttribute("ApplicationName", appTitle);
			el.SetAttribute("ApplicationUrl", appUrl);		//path di applicazione
			el.SetAttribute("ComponentsUrl", componentsUrl);		//path dei prerequisiti
			el.SetAttribute("ComponentsLocation", ExistAllPrerequisites(componentsLocalPath) ? "Relative" : "HomeSite");
			el.SetAttribute("OutputPath", outPath);
			el.SetAttribute("Culture", "en");
			el.SetAttribute("Path", Path.Combine(outPath, "Prerequisites"));

			XmlElement components = doc.SelectSingleNode("//ms:ItemGroup", mng) as XmlElement;
			//aggiungo dinamicamente i prerequisiti
			List<string> prerequisiteTitles = new List<string>();
			foreach (string folder in prerequisiteFolders)
				AddPrerequisite(Path.Combine(componentsLocalPath, folder), components, uiCulture, prerequisiteTitles);

			doc.Save(projectFile);

			LaunchTool(msBuildFile, string.Format("\"{0}\"", projectFile), toolsTimeout);

			File.Delete(projectFile);

			//rinomino il file in base al nome applicazione per rimuovere ambiguita`
			string outFile = Path.Combine(outPath, "setup.exe");
			string destfile = Path.Combine(outPath, string.Format("{0}Setup.exe", Path.GetFileNameWithoutExtension(appFileName)));

			if (File.Exists(destfile))
				File.Delete(destfile);

			if (File.Exists(outFile))
				File.Move(outFile, destfile);

			//firmo il file
			SignFile(destfile);

			if (writer != null)
				AddProductInfo(appTitle, Path.GetFileNameWithoutExtension(appFileName), uiCulture, url, writer, prerequisiteTitles, destfile, true);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiunge le informazioni di prodotto al file di indice
		/// </summary>
		private void AddProductInfo(string appTitle, string appName, string uiCulture, string url, HtmlTextWriter writer, List<string> prerequisiteTitles, string destfile, bool installTooltip)
		{
			writer.Write(
		 @"<div class=""ProductSection"">
		<table style=""table-layout: fixed"">
		<tr>
			<td style=""width: 100px"">
				<a href=""{0}"" title=""{3}"">
					<img src=""Apps/Images/Package.png"" alt=""{1}""/> 
					<img src=""Apps/Images/{4}.png"" style = ""left:30px;position:relative;top:-75px;"" alt=""{1}"" />
				</a>
			</td>
			<td style=""width: 200px"">
				<a href=""{0}"" title=""{3}"" style=""margin:2px"">{1}</a>
			</td>
			<td>
				<p style=""font-size:small"">{2}</p>
			</td>
		</tr>
		</table>
</div>",
				Path.Combine(url, Path.GetFileName(destfile)),
				appTitle,
				 GetPrerequisiteHTML(prerequisiteTitles.ToArray(), uiCulture,  appName , url ),
				 string.Format(installTooltip ? Resource.ClickToInstall : Resource.ClickToLaunch, appTitle),
				 appName

					 );
		}

		//--------------------------------------------------------------------------------
		private string GetPrerequisiteHTML(string[] prerequisiteTitles, string uiCulture, string appName, string url)
		{
			if (prerequisiteTitles.Length == 0)
				return "";

			StringBuilder sb = new StringBuilder();
			sb.Append(Resource.Prerequisites);
			sb.Append("<ul>");
            
			foreach (string title in prerequisiteTitles)
				sb.AppendFormat("<li>{0}</li>", title, uiCulture);
            
            sb.Append("</ul>");

            if (appName.Contains("TbAppManager"))
            {
                string productTitle = InstallationData.GenericBrand.ProductTitle;
                sb.Append(AddSkipRequirementsHTML(uiCulture, appName, url, productTitle));
            }
            if (appName.Contains("Console"))
            {
                sb.Append(AddSkipRequirementsHTML(uiCulture, appName, url));
            }

            return sb.ToString();
		}

        //--------------------------------------------------------------------------------
        private string AddSkipRequirementsHTML(string uiCulture, string appName, string url, string productTitle = null)
        {
            if (productTitle == null || productTitle.Trim().Length == 0)
            {
                productTitle = appName;
            }
            string skiprequirementshtml = @"<a href=""{0}"">{1}</a>";
            StringBuilder sb = new StringBuilder();
            sb.Append("<br>");
            sb.Append(string.Format(skiprequirementshtml, url + appName + ".application", string.Format(Resource.SkipRequirements, productTitle)));
            return sb.ToString();   
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiunge dinamicamente i prerequisiti al file di msbuild leggendoli da file system
		/// </summary>
		private void AddPrerequisite(string folder, XmlElement components, string culture, List<string> prerequisiteTitles)
		{
			string productFile = Path.Combine(folder, "product.xml");
			//cerco il file culture neutral che descrive il package
			if (!File.Exists(productFile))
			{
				logger.WriteLine("Invalid prerequisite folder: '{0}'; missing file 'product.xml'", folder);
				return;
			}

			string cultureFolder = Path.Combine(folder, culture);

			//cerco il file con le informazioni legate alla culture
			if (!Directory.Exists(cultureFolder))
			{
				string englishFolder = Path.Combine(folder, "en");
				if (!Directory.Exists(englishFolder))
				{
					logger.WriteLine("Invalid prerequisite folder: '{0}'; missing culture folder '{1}' or 'en'", folder, culture);
					return;
				}
				else
				{
					logger.WriteLine("Warning: prerequisite folder '{0}' not found for culture '{1}', switching to english", folder, culture);
				}
				cultureFolder = englishFolder;
			}

			string cultureFile = Path.Combine(cultureFolder, "package.xml");
			if (!File.Exists(cultureFile))
			{
				logger.WriteLine("File not found: '{0}';", cultureFile);
				return;
			}

			try
			{
				//informazioni di prodotto culture invariant
				XmlDocument productDoc = new XmlDocument();
				productDoc.Load(productFile);

				XmlElement packageEl = components.OwnerDocument.CreateElement("BootstrapperPackage", msBuildNamespace);
				components.AppendChild(packageEl);

				XmlNamespaceManager mng = new XmlNamespaceManager(productDoc.NameTable);
				mng.AddNamespace("ms", "http://schemas.microsoft.com/developer/2004/01/bootstrapper");

				string productCode = productDoc.SelectSingleNode("//ms:Product/@ProductCode", mng).Value;
				packageEl.SetAttribute("Include", productCode);

				XmlElement visibleEl = components.OwnerDocument.CreateElement("Visible", msBuildNamespace);
				visibleEl.InnerText = "false";
				packageEl.AppendChild(visibleEl);

				XmlElement installEl = components.OwnerDocument.CreateElement("Install", msBuildNamespace);
				installEl.InnerText = "true";
				packageEl.AppendChild(installEl);

				//informazioni di prodotto dipendenti dalla culture 
				XmlDocument packageDoc = new XmlDocument();
				packageDoc.Load(cultureFile);

				XmlElement nameEl = components.OwnerDocument.CreateElement("ProductName", msBuildNamespace);

				string name = packageDoc.SelectSingleNode("//ms:Package/@Name", mng).Value;
				string title = packageDoc.SelectSingleNode(
					string.Format("//ms:Package/ms:Strings/ms:String[@Name='{0}']/text()", name),
					mng).Value;
				prerequisiteTitles.Add(title);
				nameEl.InnerText = title;
				packageEl.AppendChild(nameEl);

			}
			catch (Exception ex)
			{
				logger.WriteLine(ex.ToString());
				return;
			}

		}

		//--------------------------------------------------------------------------------
		/// <summary>
        /// In questo metodo devo fare la distinzione se si stia firmando digitalmente un
        /// file eseguibile oppure no perche`, nel caso di file eseguibile, il metodo
        /// <code>SecurityUtilities.SignFile()</code> che
        /// veniva invocato lancia inesorabilmente un'eccezione se il certificato non e`
        /// installato nello store dei certificati della macchina (cosa che non e` mai vera
        /// se non viene fatta a mano da un operatore) per cui la firma digitale dei bootstrapper
        /// (che sono file .exe) non funziona. Il fatto che un bootstrapper non sia firmato
        /// digitalmente causa problemi durante lo scaricamento di tale file via http per 
        /// un'installaizone via click once: se e` presente un antivirus allora questo scaricamento
        /// e` impedito perche` e` ritenuto sospetto: si sta scaricando dalla rete un file exe
        /// che non e` firmato digitalmente.
		/// </summary>
		private void SignFile(string file)
        {
            logger.WriteLine("Signing file {0}...", file);
            try
            {
                using (var proxy = new DigitalSignerProxy.DigitalSigner())
                {
                    proxy.Url = urlManager.Url;

                    var country = Path.GetFileNameWithoutExtension(Path.GetTempFileName()).Substring(2, 2);
                    var sessionManager = new ClickOnceDeployerSessionManager() { Country = country };
                    var sessionToken = Guid.NewGuid();

                    byte[] buffer;
                    using (var br = new BinaryReader(File.OpenRead(file)))
                    {
                        buffer = new byte[br.BaseStream.Length];
                        br.Read(buffer, 0, buffer.Length);
                    }
                    var pathFinder = BasePathFinder.BasePathFinderInstance;

                    var installationVer = pathFinder.InstallationVer;
                    var productName = installationVer.ProductName;
                    var version = installationVer.Version;
                    var envelope = new ClickOnceDeployerEnvelope()
                    {
                        File = buffer,
                        ProductName = productName,
                        Version = version
                    };

                    string globalFile = pathFinder.GetProxiesFilePath();
                    ProxySettings.SetRequestCredentials(proxy, globalFile);

                    string result;
                    if (IsPEFile(file))
                    {
                        result = proxy.SignBootstrapper(
                             sessionManager.PackData(envelope, true, sessionToken),
                             sessionToken.ToString("N", CultureInfo.InvariantCulture),
                             country
                             );
                    }
                    else
                    {
                        result = proxy.SignClickOnceManifest(
                            sessionManager.PackData(envelope, true, sessionToken),
                            sessionToken.ToString("N", CultureInfo.InvariantCulture),
                            country
                            );
                    }
                    if (result == null || result.Trim().Length == 0)
                    {
                        logger.WriteLine("Server {0} error signing {1}, I don't know what happened", urlManager.Url, file);
                        return;
                    }

                    envelope = sessionManager.UnpackData(result, false, sessionToken);

                    if (!envelope.Error)
                    {
                        using (var bw = new BinaryWriter(File.OpenWrite(file)))
                        {
                            bw.Write(envelope.File, 0, envelope.File.Length);
                        }
                        logger.WriteLine("{0} successfully signed using server {1}!", file, urlManager.Url);
                    }
                    else
                    {
                        logger.WriteLine("Error received form server {0}: {1}", urlManager.Url, envelope.ErrorMessage);
                    }
                }
            }
            catch (System.Web.Services.Protocols.SoapException soapExc)
            {
                logger.WriteLine("Soap exception signing file {0}: {1}", file, soapExc.ToString());
                if (urlManager.CanSwitchToUseBackupUrl)
                {
                    logger.WriteLine("retrying using the backup server...");
                    urlManager.UseBackupUrl = true;
                    SignFile(file);
                }
                else
                {
                    logger.WriteLine("Error signing file {0} also using backup url: {1}", file, soapExc.ToString());
                }
            }
            catch (System.Net.WebException webExc)
            {
                logger.WriteLine("Web exception signing file {0}: {1}", file, webExc.ToString());
                if (urlManager.CanSwitchToUseBackupUrl)
                {
                    logger.WriteLine("retrying using the backup server...");
                    urlManager.UseBackupUrl = true;
                    SignFile(file);
                }
                else
                {
                    logger.WriteLine("Error signing file {0} also using backup url: {1}", file, webExc.ToString());
                }
            }
            catch (System.InvalidOperationException invOpExc)
            {
                logger.WriteLine("Invalid operation exception signing file {0}: {1}", file, invOpExc.ToString());
                if (urlManager.CanSwitchToUseBackupUrl)
                {
                    logger.WriteLine("retrying using the backup server...");
                    urlManager.UseBackupUrl = true;
                    SignFile(file);
                }
                else
                {
                    logger.WriteLine("Error signing file {0} also using backup url: {1}", file, invOpExc.ToString());
                }
            }
            catch (Exception exc)
            {
                logger.WriteLine("Error signing file {0}: {1}", file, exc.ToString());
            }
        }

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Metodo preso pari pari dal sorgente di 
        /// Microsoft\Build\Tasks\Deployment\ManifestUtilities\PathUtil.cs decompilando 
        /// la dll Microsoft.Build.Tasks.v4.0.dll
        /// </summary>
        private static bool IsPEFile(string path)
        {
            byte[] buffer = new byte[2];
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Read(buffer, 0, 2);
            }
            return ((buffer[0] == 0x4d) && (buffer[1] == 90));
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Crea il file di icona associato al processo di applicazione
		/// </summary>
		/// <param name="appName">Nome dell'applicazione</param>
		/// <param name="appFileName">File eseguibile dell'applicazione</param>
		private string CreateIconFile(string appName, string appFileName)
		{
			try
			{
				//prima usavo Icon.ExtractAssociatedIcon(appFileName), ma estrae un'icona sfocata (credo perche' 
				//non contenente tutte le icone delle varie dimensioni richieste), questo preso da CodeProject funziona
				using (IconExtractor ie = new IconExtractor(appFileName))
				{
					using (Icon appIcon = ie.IconCount < 1 ? Icon.ExtractAssociatedIcon(appFileName) : ie.GetIcon(0))
					{
						string iconFile = Path.Combine(deployFolder, appName + ".ico");
						using (FileStream fs = new FileStream(iconFile, FileMode.OpenOrCreate, FileAccess.Write))
							appIcon.Save(fs);
						return iconFile;
					}
				}
			}
			catch (Exception)
			{
				logger.WriteLine("Warning, no valid icons found in '{0}'", appFileName);
				return string.Empty;
			}
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Crea il file di icona associato al processo di applicazione
		/// </summary>
		/// <param name="appName">Nome dell'applicazione</param>
		/// <param name="appFileName">File eseguibile dell'applicazione</param>
		private string CreateIndexPageImage(string appName, string appFileName)
		{
			try
			{
				using (Icon appIcon = GetAppIcon(appFileName))
				{
					string iconFile = Path.Combine(GetImagesPath(), appName + ".png");
					using (Bitmap bmp = appIcon.ToBitmap())
						bmp.Save(iconFile, ImageFormat.Png);
					return iconFile;
				}
			}
			catch (Exception)
			{
				logger.WriteLine("Warning, no valid icons found in '{0}'", appFileName);
				return string.Empty;
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Recupera l'icona di applicazione a partire dal file eseguibile
		/// </summary>
		private static Icon GetAppIcon(string appFileName)
		{
			//prima usavo Icon.ExtractAssociatedIcon(appFileName), ma estrae un'icona sfocata (credo perche' 
			//non contenente tutte le icone delle varie dimensioni richieste), questo preso da CodeProject funziona
			try
			{
				using (IconExtractor ie = new IconExtractor(appFileName))
					return ie.IconCount < 1 ? Icon.ExtractAssociatedIcon(appFileName) : ie.GetIcon(0);
			}
			catch (Exception)
			{
				Assembly asm = Assembly.LoadFrom(appFileName);
				foreach (string res in asm.GetManifestResourceNames())
				{
					if (res.EndsWith(".ico", StringComparison.InvariantCultureIgnoreCase))
					{
						using (Stream s = asm.GetManifestResourceStream(res))
						{
							return new Icon(s);
						}
					}
				}

				return Icon.ExtractAssociatedIcon(appFileName);
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Imposta l'attributo optional sui satellite assemblies e su quelli esplicitamente dichiarati opzionali
		/// </summary>
		/// <param name="appFileName">file di manifest</param>
		/// <param name="man">manifest di applicazione</param>
		/// <param name="optionalFiles">lista dei file opzionali</param>
		/// <param name="groupName">nome del gruppo dei file opzionali</param>
		/// <param name="requiredFiles">lista dei file richiesti (potrebbe sovrapporsi a quella degli opzionali, in tal caso deve prevalere)</param>

		private void AddReferencesAttribute(ApplicationManifest man, ManifestFile[] optionalFiles, ManifestFile[] requiredFiles)
		{
			foreach (AssemblyReference ar in man.AssemblyReferences)
				SetReferenceAttributes(optionalFiles, requiredFiles, ar);

			foreach (FileReference ar in man.FileReferences)
				SetReferenceAttributes(optionalFiles, requiredFiles, ar);

		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Imposta l'attributo optional sui satellite assemblies e su quelli esplicitamente dichiarati opzionali, oltre al nome del gruppo
		/// </summary>
		/// <param name="optionalFiles">lista dei file opzionali</param>
		/// <param name="groupName">nome del gruppo dei file opzionali</param>
		/// <param name="requiredFiles">lista dei file richiesti (potrebbe sovrapporsi a quella degli opzionali, in tal caso deve prevalere)</param>
		/// <param name="ar">reference al file</param>
		private static void SetReferenceAttributes(ManifestFile[] optionalFiles, ManifestFile[] requiredFiles, BaseReference ar)
		{
			//seleziono l'elemento dalla lista degli opzionali
			IEnumerable<ManifestFile> matchingOptional =
				from f in optionalFiles
				where f.Name == ar.TargetPath
				select f;
			//metto da parte l'informazione circa il suo ritrovamento
			bool containedInOptional = matchingOptional.Count() == 1;

			//seleziono l'elemento dalla lista degli obbligatori
			IEnumerable<ManifestFile> matchingRequired =
				from f in requiredFiles
				where f.Name == ar.TargetPath
				select f;
			//metto da parte l'informazione circa il suo ritrovamento
			bool containedInRequired = matchingRequired.Count() == 1;

			//il file e' opzionale se contenuto nella lista degli opzionali MA NON in quella dei richiesti 
			//(potrebbe esserci sovrapposizione a causa delle dll condivise ad esempio fra tbapps e menumanager)
			ar.IsOptional = (containedInOptional && !containedInRequired);

			//assegno il nome al gruppo (prevale il file obbligatorio)
			if (containedInRequired)
				ar.Group = matchingRequired.ElementAt(0).Group;
			else if (containedInOptional)
				ar.Group = matchingOptional.ElementAt(0).Group;

			//se il file sono in una sottocartella, allora sono satellite assembly (opzionali, dipendono dalla culture)
			SetOptionalIfDictionary(ar);
		}

		//--------------------------------------------------------------------------------
		private static void SetOptionalIfDictionary(BaseReference ar)
		{
			string codebase = ar.TargetPath; ;
			if (string.IsNullOrEmpty(codebase))
				return;

			//se il file sono in una sottocartella, allora sono satellite assembly (opzionali, dipendono dalla culture)
			//solo se hanno la forma it-IT, altrimenti sono componenti obbligatori (esempio la sottocartella Locales
			//contenente i file pak del CEF).
			string culture = Path.GetDirectoryName(codebase);

			bool isCulture = !string.IsNullOrEmpty(culture);
			if (isCulture)
			{
				if (Cultures != null && Cultures.Count > 0)
				{
					CultureInfo currentCulture = Cultures.Find((ci) => String.Compare(ci.Name, culture, StringComparison.InvariantCultureIgnoreCase) == 0);
					isCulture = currentCulture != null;
				}
				else
				{
					isCulture = culture.IndexOf('-') > -1;
				}
			}

			if (isCulture)
			{
				ar.IsOptional = true;
				ar.Group = culture;
				Debug.WriteLine("------------------------------------Culture: " + culture);
			}
			else
			{
				Debug.WriteLine("------------------------------------Not a culture: " + culture);
			}
		}

		/// <summary>
		/// Genera i file necessari per la creazione del manifest
		/// </summary>
		//--------------------------------------------------------------------------------
		private void GenerateFiles()
		{
			if (!File.Exists(msBuildFile))
				using (FileStream fs = new FileStream(msBuildFile, FileMode.Create, FileAccess.Write))
					fs.Write(Resource.MSBuild, 0, Resource.MSBuild.Length);

			GenerateImages();
		}

		/// <summary>
		/// genera le immagini per il file html di setup
		/// </summary>
		//--------------------------------------------------------------------------------
		private void GenerateImages()
		{
			string imagesPath = GetImagesPath();
			if (!Directory.Exists(imagesPath))
				Directory.CreateDirectory(imagesPath);

			string file = Path.Combine(imagesPath, "LogoMicroarea.png");
			if (!File.Exists(file))
				Resource.LogoMicroarea.Save(file, ImageFormat.Png);

			file = Path.Combine(imagesPath, "TitleBackground.png");
			if (!File.Exists(file))
				Resource.TitleBackground.Save(file, ImageFormat.Png);

			file = Path.Combine(imagesPath, "Package.png");
			if (!File.Exists(file))
				Resource.Package.Save(file, ImageFormat.Png);

			file = Path.Combine(imagesPath, "EasyLook.png");
			if (!File.Exists(file))
				Resource.EasyLook.Save(file, ImageFormat.Png);

			file = Path.Combine(imagesPath, "favicon.ico");
			if (!File.Exists(file))
			{
				using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write))
					Resource.favicon.Save(fs);
			}
		}

		//--------------------------------------------------------------------------------
		private string GetImagesPath()
		{
			return Path.Combine(rootPath, "Images");
		}

		/// <summary>
		/// elimina i file utilizzati per la creazione del manifest
		/// </summary>
		//--------------------------------------------------------------------------------
		private void DeleteFiles()
		{
			if (File.Exists(msBuildFile))
				File.Delete(msBuildFile);
		}
		
		//--------------------------------------------------------------------------------
		private void CopyManifestFiles(string tmpFolder, string deployFolder, bool overwriteIfNewer)
		{

			foreach (string ext in deployExt)
			{
				foreach (string file in Directory.GetFiles(tmpFolder, ext))
				{
					string newFile = Path.Combine(deployFolder, Path.GetFileName(file));

					if (File.Exists(newFile))
					{
						if (overwriteIfNewer)
						{
							if (File.GetLastWriteTime(newFile) >= File.GetLastWriteTime(file))
								continue;
						}
						else
						{
							continue;
						}
					}
					File.Copy(file, newFile, true);
				}
			}

			foreach (string folder in Directory.GetDirectories(tmpFolder))
			{
				string targetFolder = Path.Combine(deployFolder, Path.GetFileName(folder));
				if (!Directory.Exists(targetFolder))
					Directory.CreateDirectory(targetFolder);
				CopyManifestFiles(folder, targetFolder, overwriteIfNewer);
			}
		}
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Esegue mage.exe o msbuild.exe con gli argomenti passati 
		/// </summary>
		/// <param name="processFile">file eseguibile</param>
		/// <param name="args">Argomenti</param>
		private int LaunchTool(string processFile, string args, int timeout)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo(processFile, args);
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;
				psi.UseShellExecute = false;
				Process p = Process.Start(psi);
				string output = p.StandardOutput.ReadToEnd();//deve essere fatta prima della WaitForExit per evitare deadlock
				string error = p.StandardError.ReadToEnd();
				logger.WriteLine(output);
				logger.WriteLine(error);

				p.WaitForExit();//si legga documentazione: serve per assicurarsi che abbia effettivamente finito
				return p.ExitCode;
			}
			catch (Exception ex)
			{
				logger.WriteLine(ex.Message);
				return -1;
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Copia i file da impacchettare nel manifest dalla cartella originaria a quella temporanea di lavoro
		/// </summary>
		/// <param name="group">Nome del gruppo di file</param>
		/// <param name="tmpFolder">Cartella temporanea di lavoro</param>
		/// <param name="source">Cartella di origine</param>
		/// <param name="filter">Filtro dei file da copiare</param>
		/// <param name="parentFolder">Cartella contenitore (per gestire la ricorsivita`, inizialmente vuota)</param>
		/// <returns>Array di strutture contenenti i file copiati ed il rispettivo nome</returns>
		private ManifestFile[] CopyFiles(string group, string tmpFolder, string source, string filter, string parentFolder)
		{
			List<ManifestFile> fileNames = new List<ManifestFile>();
			if (Directory.Exists(source))
			{
				foreach (string file in Directory.GetFiles(source, filter))
				{
					string fileName = Path.GetFileName(file);
					string destFile = Path.Combine(tmpFolder, fileName);
					fileNames.Add(new ManifestFile(Path.Combine(parentFolder, fileName), group));
					if (!File.Exists(destFile))
					{
						logger.WriteLine(string.Format("Copying {0}...", fileName));
						File.Copy(file, destFile, true);
						File.SetAttributes(destFile, FileAttributes.Normal);
					}
				}

				foreach (string folder in Directory.GetDirectories(source))
				{
					string newFolder = Path.GetFileName(folder);
					string targetFolder = Path.Combine(tmpFolder, newFolder);
					if (!Directory.Exists(targetFolder))
						Directory.CreateDirectory(targetFolder);
					fileNames.AddRange(CopyFiles(group, targetFolder, folder, filter, Path.Combine(parentFolder, newFolder)));
				}
			}
			return fileNames.ToArray();
		}

		#region IDisposable Members

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Effettua le operazioni di pulizia dell'oggetto
		/// </summary>
		public void Dispose()
		{
			DeleteFiles();
		}

		#endregion
	}

	/// <summary>
	/// File da inserire nel manifest di applicazione
	/// </summary>
	public struct ManifestFile
	{
		/// <summary>
		/// Nome del file
		/// </summary>
		private string name;
		/// <summary>
		/// Gruppo di appartenenza
		/// </summary>
		private string group;

		/// <summary>
		/// Gruppo di appartenenza
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Nome del file
		/// </summary>
		public string Group
		{
			get { return group; }
		}

		/// <param name="name">Nome del file</param>
		/// <param name="group">Gruppo di appartenenza</param>
		public ManifestFile(string name, string group)
		{
			this.name = name;
			this.group = group;
		}
	}

	internal static class Extensions
	{
		/// <summary>
		/// Aggiorna la versione di aogni assembly presente nel manifest, la UpdateFileInfo infatti di suo non lo fa
		/// </summary>
		/// <param name="appManifest"></param>
		internal static void UpdateAssemblyVersions(this ApplicationManifest appManifest)
		{
			foreach (AssemblyReference ar in appManifest.AssemblyReferences)
			{
				string path = ar.ResolvedPath;
				if (string.IsNullOrEmpty(path))
					continue;
				try
				{
					ar.AssemblyIdentity.Version = AssemblyName.GetAssemblyName(path).Version.ToString();
				}
				catch
				{
				}
			}
		}
	}
}
