using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
//
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Library.CommonDeploymentFunctions;
using Microarea.Library.CommonDeploymentFunctions.States;
using Microarea.Library.Licence;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;
using Microarea.TaskBuilderNet.Core.NameSolver;

using Consts = Microarea.Library.CommonDeploymentFunctions.Strings.Consts;
using ConfConsts = Microarea.Library.Licence.Consts;
using ImageVersion = Microarea.Library.CommonDeploymentFunctions.Version;

namespace Microarea.Library.ImagesManagement
{
	/// <summary>
	/// La classe comprende una collezione di metodi per potere gestire immagini e installazioni.
	/// Quanto implementato in questa deve essere abbastanza generico, client e server
	/// devono poi ereditare da essa se necessitano eventuali specializzazioni.
	/// </summary>
	public abstract class ImagesManager
	{
		// Paths
		protected string	imagesPath;
		protected string	servicesImagesPath;		// path assoluto dir immagine dei Services
		protected string	universalImagesPath;	// path assoluto dir immagini prodotti
		protected bool		usePatches = false;		// se true il metodo CopyUpdatesImageFiles() potrà decidere di copiare
													// dei files di patch in luogo dei files veri

		protected string assemblyName;

		protected const string TagApplication		= ConfConsts.TagApplication;
		protected const string TagContainer			= ConfConsts.TagContainer;
		protected const string TagModule			= ConfConsts.TagModule;
		protected const string TagDictionary		= ConfConsts.TagDictionary;
		protected const string TagIncludeModulesPath	= ConfConsts.TagIncludeModulesPath;
		protected const string TagSolution			= ConfConsts.TagSolution;
		protected const string TagMicroareaServices	= ConfConsts.TagMicroareaServices;
		protected const string DirSolutions			= ConfConsts.DirSolutions;
		protected const string DirSolutionModules	= ConfConsts.DirSolutionModules;
		private readonly string deploymentManifestFull = string.Empty;
		private readonly string deploymentManifestBase = string.Empty;

		private bool aborted = false;

		// helper object to access FS image rels, replaceble with a mock object for unit testing
		private IProductsRepository repository = null;
		public IProductsRepository ImagesRepository { set { repository = value; }}

		//---------------------------------------------------------------------
		public ImagesManager(string assemblyName)
		{
			this.assemblyName = assemblyName;
			deploymentManifestFull = ManifestManager.GetManifestFileName(PolicyType.Full);//"DeploymentManifest.Full.xml";
			deploymentManifestBase = ManifestManager.GetManifestFileName(PolicyType.Base);//"DeploymentManifest.Base.xml";
		}

		//---------------------------------------------------------------------
		protected void Init()
		{
			if (imagesPath == null || imagesPath.Length == 0)
			{
				Debug.Fail("Inizializzazione di ImageManager fallita!");
				return;
			}
			servicesImagesPath	= Path.Combine(imagesPath, Consts.DirServices);
			universalImagesPath	= Path.Combine(imagesPath, Consts.DirUniversalImages);

			repository = new ProductsRepository(universalImagesPath);
		}

		//---------------------------------------------------------------------
		public void Abort()
		{
			this.aborted = true;
		}

		//---------------------------------------------------------------------
		public bool Aborted	{ get { return this.aborted; }}
		
		/// <summary>
		/// ritorna il path della directory contenente una data immagine
		/// </summary>
		/// <param name="storageName"></param>
		/// <param name="storageRelease"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		protected internal string GetImagePath(string storageName, string storageRelease)
		{
			string imageSubPath = Path.Combine(storageName, storageRelease);
			return Path.Combine(universalImagesPath, imageSubPath);
		}

		/// <summary>
		/// dato il nome di un prodotto, ritorna un array contenente tutte
		/// le immagine ufficiali supportate dello stesso, filtrate per maxRelease
		/// e filtrate in modo da tenere per ogni M.m solo la SP più alta
		/// </summary>
		/// <param name="storageName">nome del prodotto</param>
		/// <param name="maxRelease">max release concessa all'utente. String.Empty per non porre limiti</param>
		/// <returns>array contenente i nomi delle immagini associate al prodotto (filtrato)</returns>
		//---------------------------------------------------------------------
		public string[] GetProductTopMostSPImagesRels(string storageName, string maxRelease)
		{
			string[] prodRels = repository.GetProductImagesRels(storageName);
			return GetProductTopMostSPImagesRels(storageName, maxRelease, prodRels);
		}

		private string[] GetProductTopMostSPImagesRels(string storageName, string maxRelease, string[] prodRels)
		{
			if (prodRels.Length == 0)
				return prodRels;

			// Pubblicazione di release solo a gruppi limitati di utenti (es. beta tester)
			// il WS di auth mi indica la maxRelease concessa in visibilità all'utente
			// per il prodotto indicato (string.Empty vuole dire nessuna limitazione, usata
			// anche per deployment da CD/Network
			// occorre che:
			//		*	non vi siano più di una M.m uguali (anche se cambia SP)
			//			se ve ne fossero più di una, scegliere la più alta SP per
			//			le M.m inferiori alla M.m di maxRelease
			//		*	...tranne nel caso della la M.m di MaxRelease, per cui il
			//			massimo valore è maxRelease (se esiste su file system)
			// NOTE - adesso le storageRelease solo comprensive di tutti i quattro campi

			ImageVersion maxVersion = null;
			if (maxRelease != null && maxRelease.Length != 0)
				maxVersion = new ImageVersion(maxRelease);

			Hashtable table = new Hashtable();
			foreach (string prodRel in prodRels)
			{
				ImageVersion aImgVersion = new ImageVersion(prodRel);

				// tolgo le immagini superiori a maxRelease
				if (maxVersion != null && aImgVersion > maxVersion)
					continue;

				// aggiungo la release se non presente o maggiore della M.m presente
				string hash = string.Concat(aImgVersion.Major, ".", aImgVersion.Minor);
				ImageVersion prevStoredValue = table[hash] as ImageVersion;
				if (prevStoredValue == null || prevStoredValue < aImgVersion)
					table[hash] = aImgVersion;
			}

			ArrayList imgs = new ArrayList(table.Count);
			foreach (string hashKey in table.Keys)
				imgs.Add(table[hashKey]);
			imgs.Sort();
			imgs.Reverse();

			ArrayList dirs = new ArrayList(imgs.Count);
			foreach (ImageVersion ver in imgs)
			{
				string v = ver.ToString();
				if (repository.ImageExists(storageName, v))
					dirs.Add(v);
			}
			return (string[])dirs.ToArray(typeof(string));
		}

		/// <summary>
		/// Restituisce l'elenco dei prodotti disponibili nelle directories
		/// contenenti le Universal Images
		/// </summary>
		/// <returns>l'elenco di prodotti richiesto</returns>
		//---------------------------------------------------------------------
		public string[] GetAvailableProducts(string isoCountry, ProductType productType, out string[] brandedNames)
		{
			ArrayList productsArray = GetAvailableProductsSignatures(productType);	// might need impersonation

			// Problem:		Branded name is detailed at single-release level, 
			//				so where should we pick it?
			// Answer:		Since this method is invoked at installation time only, when the
			//				product image to be presented to the user is the max allowed for
			//				the user, that is the image where to look for it

			string[] productsList;
			// this method call, when isu, requires a webmethod call
			// WARNING: ISU overload should not use impersonation (due to a WSE1.0 tech limit)
			string[] maxProductReleasesAllowed = GetMaxReleasesAllowedForProducts(productsArray, out productsList);

			brandedNames = GetBrandedNames(productsList, maxProductReleasesAllowed, isoCountry);	// might need impersonation

			return productsList;
		}

		//---------------------------------------------------------------------
		protected virtual ArrayList GetAvailableProductsSignatures(ProductType productType)
		{
			DirectoryInfo di = new DirectoryInfo(universalImagesPath);
			if (!di.Exists)
				throw new DirectoryNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cannot find directory '{0}'.", universalImagesPath));	// LOCALIZE ?
			DirectoryInfo[] dirs = di.GetDirectories();
			int dLength = dirs.Length;
			ArrayList productsArray	= new ArrayList(dLength);
			for (int i = 0; i < dLength; i++)
			{
				DirectoryInfo prodDir = dirs[i];
				string productSignature = prodDir.Name;
				bool toAdd;
				if (productType == ProductType.Unknown) // means "show both masters and addons"
					toAdd = true;
				else
				{
					// just get the first release to see if it is a vertical
					DirectoryInfo[] prodRels = prodDir.GetDirectories();
					if (prodRels.Length == 0)
						continue;
					DirectoryInfo firstRel = prodRels[0];

					string compFileRelPath = FindCompatibilityListRelFile(productSignature, firstRel.Name);
					bool isAddOn = compFileRelPath != null;
					toAdd = 
						(isAddOn && productType == ProductType.AddOn) || 
						(!isAddOn && productType == ProductType.Master);
				}

				if (toAdd)
					productsArray.Add(productSignature);
			}
			return productsArray;
		}

		//---------------------------------------------------------------------
		protected virtual string[] GetBrandedNames
			(
			string[] productsList, 
			string[] maxProductReleasesAllowed,
			string isoCountry
			)
		{
			string[] brandedNames = new string[productsList.Length];
			for (int i = 0; i < productsList.Length; i++)
			{
				string storageName = productsList[i];

				// Picks a branded product name, if existing.
				string maxProductReleaseAllowed = maxProductReleasesAllowed[i];
				string topRel = GetTopImageRelease(storageName, maxProductReleaseAllowed); // can return null
				if (topRel == null)
					throw new IndexOutOfRangeException(); // thrown to comply with older implementation, after refactoring
				string imgPath = GetImagePath(storageName, topRel);

				string brandFilePath = BasePathFinder.FindSolutionTitleBrandFile(imgPath, storageName);
				if (brandFilePath != null && brandFilePath.Length != 0)
				{
					BrandLoader brand = new BrandLoader(new FileInfo(brandFilePath), isoCountry);
					string brandedName = brand.GetSolutionTitle();
					if (brandedName != null && brandedName.Length > 0)
						brandedNames[i] = brandedName;
				}
				else
					brandedNames[i] = storageName;
			}
			return brandedNames;
		}

		/// <summary>
		/// Picks a branded installation name, if existing; otherwise returns unbranded default name.
		/// </summary>
		/// <param name="isoCountry"></param>
		/// <param name="product"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public virtual string GetBrandedDefaultInstallationName
			(
			string isoCountry,
			string product
			)
		{
			// locates the image path where to look for product brand info
			string imgPath = GetMaxImagePath(product);
			string storageRelease = new DirectoryInfo(imgPath).Name;
			string brandRelFile = FindBrandProductRelFile(product, storageRelease);
			if (brandRelFile == null || brandRelFile.Length == 0)
				return NameSolverStrings.DefaultInstallationName;//"myERP";
			string brandFilePath = Path.Combine(imgPath, brandRelFile);
			return BasePathFinder.GetBrandedDefaultInstallationName(brandFilePath, isoCountry);
		}

		//---------------------------------------------------------------------
		private string FindBrandProductRelFile(string storageName, string storageRelease)
		{
			if (!ImageExists(storageName, storageRelease))
				return null;

			// since TB2.7, <Product>.Brand.xml is inside the Solutions folder inside the application (TB2.7 folders structure)
			// before TB2.7 <Product>.Brand.xml was inside the image root folder

			string fileName = BasePathFinder.GetBrandProductFileName(storageName);
			using (ISourceImage sourceImage = new SourceImage(this, storageName, storageRelease, false))
			{
				// first, search at root level (preTB2.7)
				if (RelativeFile.Exists(fileName, sourceImage))
					return fileName;
				// then look for file in pre-TB2.7 folders structure
				string tb2_7RelSolutionsDir = FindSolutionsRelPathInApps(sourceImage);
				if (tb2_7RelSolutionsDir == null || tb2_7RelSolutionsDir.Length == 0)
					return null; // this should be the case of a pre-TB2.7 master product
				string relFilePath = string.Concat(tb2_7RelSolutionsDir, Path.DirectorySeparatorChar, fileName);
				if (RelativeFile.Exists(relFilePath, sourceImage))
					return relFilePath;
			}
			return null; // this should be the case of a TB2.7 master product
		}

		//---------------------------------------------------------------------
		public virtual string GetEula(string isoCountry, string product, out LocalizedString error)
		{
			error = null;
			string txt = GetAgreementContent
							(
							product,
							Agreements.Eula,
							isoCountry,
							Events.UnableToReadEula,
							out error
							);
			return txt;
		}
		public virtual string GetMlu(string isoCountry, string product, out LocalizedString error)
		{
			error = null;
			string txt = GetAgreementContent
				(
				product,
				Agreements.Mlu,
				isoCountry,
				Events.UnableToReadMlu,
				out error
				);
			return txt;
		}
		public virtual string GetPrivacy(string isoCountry, string product, out LocalizedString error)
		{
			error = null;
			string txt = GetAgreementContent
				(
				product,
				Agreements.Privacy,
				isoCountry,
				Events.UnableToReadMlu,	// TEMP - use proper on when able to touch dictionaries!
				out error
				);
			return txt;
		}

		//---------------------------------------------------------------------
		private string GetAgreementContent
			(
			string product, 
			Agreements agreement,
			string country,
			string fileErrorName,
			out LocalizedString error
			)
		{
			error = null;

			// locates the image path where to look for product eula
			string imgPath = GetMaxImagePath(product);
			string licensesPath = Path.Combine(imgPath, NameSolverStrings.Licenses);
			if (!Directory.Exists(licensesPath)) // pre-TB2.7 folders structure
			{
				// search for the Licensed folder inside the Application\<appname>\Solutions (TB2.7 folders structure)
				string appsPath = Path.Combine(imgPath, Consts.DirApplications);
				if (!Directory.Exists(appsPath))
				{
					//Debug.Fail("Applications folder not found in deployment image.");
					//Debug.Fail above commented, because we are looking for clear unzipped Applications 
					// in a TB2.7 structure, but we might happen to be in a pre-TB2.7 structure where
					// the Applications folder is zipped only
					return string.Empty;
				}
				bool foundInApps = false;
				foreach (string app in Directory.GetDirectories(appsPath))
				{
					string aSolInAppsDirPath = Path.Combine(app, Consts.DirSolutions);
					if (Directory.Exists(aSolInAppsDirPath))
					{
						licensesPath = Path.Combine(aSolInAppsDirPath, NameSolverStrings.Licenses);
						if (!Directory.Exists(licensesPath))
							return string.Empty;
						foundInApps = true;
						break;
					}
				}
				if (!foundInApps)
					return string.Empty;
			}

			// locate agreement file
			AgreementsManager am = new AgreementsManager(licensesPath);
			string agreementFilePath = am.GetAgreementFilePath(product, agreement, country);
			if (agreementFilePath == null || agreementFilePath.Length == 0)
				return string.Empty;
			string filePath = agreementFilePath;

			// read agreement text
			StreamReader sr = null;
			try
			{
				sr = new StreamReader(File.OpenRead(filePath)); // read-only mode
				string txt = sr.ReadToEnd();
				return txt;
			}
			catch (IOException exc)
			{
				error = new LocalizedString(fileErrorName, null, exc.Message);
				return string.Empty;
			}
			finally
			{
				if (sr != null)
					sr.Close();
			}
		}

		//---------------------------------------------------------------------
		private string GetMaxImagePath(string product)
		{
			ArrayList productsList = new ArrayList(1);
			productsList.Add(product);
			string[] dummyNames;
			string[] maxRels = GetMaxReleasesAllowedForProducts(productsList, out dummyNames); // note: overridden on ISU
			if (dummyNames.Length != 1)
				return NameSolverStrings.DefaultInstallationName;
			string maxProductReleaseAllowed = maxRels[0];	// should be empty string for MSU, and a real value for ISU
			string topRel = GetTopImageRelease(product, maxProductReleaseAllowed); // can return null
			if (topRel == null)
				throw new IndexOutOfRangeException(); // thrown to comply with older implementation, after refactoring
			string imgPath = GetImagePath(product, topRel);

			return imgPath;
		}

		//---------------------------------------------------------------------
		protected virtual string[] GetMaxReleasesAllowedForProducts(ArrayList productsList, out string[] storageNames)
		{
			// This method definition run only for MSU (CD/Network) image sources,
			// For ISU (WS) image source will be overridden to handle auth constrains
			
			// for MSU, all found product are returned
			storageNames = (string[])productsList.ToArray(typeof(string));

			// for MSU, every product has no auth constrain
			ArrayList relList = new ArrayList(storageNames.Length);
			for (int i = 0; i < storageNames.Length; i++)
				relList.Add(string.Empty);	// sorry, can't know here
			return (string[])relList.ToArray(typeof(string));

			// NOTE: string empty is ok for MSU, but do override for ISU, and cut unknown products off!
		}

		/// <summary>
		/// Dato il nome di un'immagine, ritorna un array contenente tutte
		/// le immagine ufficiali supportate dello stesso prodotto di release
		/// superiore a quella indicata (e uguale, se parametro booleano = true).
		/// L'elenco è restituito ordinato in ordine di release crescente.
		/// </summary>
		/// <param name="storageName">nome del prodotto</param>
		/// <param name="storageRelease">release nella forma major.minor</param>
		/// <param name="includeEqualRelease">booleano che indica se includere l'immagine stessa</param>
		/// <param name="maxRelease">max release concessa all'utente. String.Empty per non porre limiti</param>
		/// <returns>elenco delle immagini che soddisfano la richiesta</returns>
		//---------------------------------------------------------------------
		public string[] GetMatchingProductReleases
			(
			string storageName, 
			string storageRelease, 
			bool includeEqualRelease,
			string maxRelease
			)
		{
			string[] productImagesRels = GetProductTopMostSPImagesRels(storageName, maxRelease);
			ImageVersion ver = new ImageVersion(storageRelease);
			ArrayList al1 = new ArrayList();
			foreach (string imageName in productImagesRels)
			{
				// TODO - irrobustire
				ImageVersion aVer = new ImageVersion(imageName);
				if (aVer > ver)
					al1.Add(aVer);
				if (includeEqualRelease && aVer == ver)
					al1.Add(aVer);
			}
			al1.Sort();

			uint clientRelDate = ver.Revision; // TEMP when we'll use ReleaseVersion will be named better and be a string
			ArrayList al2 = new ArrayList();
			foreach (ImageVersion v in al1)
				if (v.Revision > clientRelDate // even as strings they are sortable dates, so > operator is ok
					|| v == ver) // ver is the only one allowed to have the same release date
					al2.Add(v.ToString());
			return (string[])al2.ToArray(typeof(string));
		}

		//---------------------------------------------------------------------
		public ImageVersion GetHighestMajorMinorRelease(string product, string maxRelease)
		{
			string topRel = GetTopImageRelease(product, maxRelease);
			return topRel != null ? new ImageVersion(topRel) : null;
		}

		/// <summary>
		/// indica se esiste l'immagine richiesta
		/// </summary>
		/// <param name="storageName">nome dell'immagine richiesta</param>
		/// <param name="storageRelease">release nella forma major.minor</param>
		/// <returns>un booleano</returns>
		//---------------------------------------------------------------------
		public bool ImageExists(string storageName, string storageRelease)
		{
			string fullPath = GetImagePath(storageName, storageRelease);
			return Directory.Exists(fullPath);
		}

		/// <summary>
		/// indica se almeno un'immagine relativa al prodotto indicato esiste
		/// </summary>
		/// <param name="product">nome del prodotto</param>
		/// <param name="maxRelease">max release concessa all'utente. String.Empty per non porre limiti</param>
		/// <returns>un booleano</returns>
		//---------------------------------------------------------------------
		public bool IsProductSupported(string product, string maxRelease)
		{
			string topImg = GetTopImageRelease(product, maxRelease);
			return !string.IsNullOrEmpty(topImg);
		}

		/// <summary>
		/// Confronta la data di ultimo aggiornamento dei services installati
		/// sul client con la data di ultimo aggiornamento dei services sul server
		/// leggendole dai rispettivi manifest
		/// </summary>
		/// <param name="physicalConfiguration"></param>
		/// <returns>false se i services sul client non sono aggiornati</returns>
		//---------------------------------------------------------------------
		public bool ServicesAreUpToDate(XmlDocument physicalConfiguration)
		{
			// reperisce la data di ultimo aggiornamento dei services sul client
			XmlElement srvEl = physicalConfiguration.SelectSingleNode("//" + TagMicroareaServices) as XmlElement;
			return ServicesAreUpToDate(srvEl);
		}

		//---------------------------------------------------------------------
		public bool ServicesAreUpToDate(XmlElement services)
		{
			if (Functions.IgnoreServicesCheck()) return true;

			// reperisce la data di ultimo aggiornamento dei services sul client
			if (services == null)
			{
				Debug.Fail("MicroareaServices non specificati nella configurazione");
				return false;
			}

			if (!Directory.Exists(servicesImagesPath))
				throw new DirectoryNotFoundException(string.Format(CultureInfo.InvariantCulture, "Cannot find directory '{0}'.", servicesImagesPath));

			using (ISourceImage sourceImage = new SourceImage(servicesImagesPath))
			{
				// commented out: see note in HACK section
				// crea serverServicesElement e decorarlo con le date di services
				//XmlElement serverServicesEl = ProductInfo.GetServerServicesElement(servicesImagesPath);
				//ConfigurationManager.DecorateApplicationWithDates(serverServicesEl, services);
				//return !IsApplicationUpdated(sourceImage, container, serverServicesEl);

				// HACK - this is a workaround
				// Fred: the use of IsApplicationUpdated() to implement ServicesAreUpToDate()
				//       has a serious drawback with dictionaries: whenever we just add simply
				//       a dictionary,  services would result non update. Moreover, since
				//       Services 2.0 base language was switched form "it" to "en", the new
				//       services image did not have the "en" dictionary, but older product CDs
				//       had, which resulted in being unable to update a 1.x product form its cd
				//       having services installed!
				//       The problem is that the method structure matches the IsImageUpdated() one
				//       because it was conceived to perform automatic update, but until the
				//       time this note is written, it is used to check whether the installed services
				//       are able to install/update from the data source, without caring about dictionaries.
				//       I do not want to cancel previous implementation (I just commented two lines
				//       of code), and I consider this just a temporary hack: when we will perform
				//       automatic services update (if ever, I hope so), the right solution will be to add
				//       a "deepCheck" boolean parameter, using true for auto-update purpose, where the
				//       IsApplicationUpdated() way is used, and false for simple checks, where the now
				//       "hacked" way is used
				string serverManifestName = ManifestManager.BuildDirectoryManifestFileName(PolicyType.Full);
				string serverManifestRelPath = serverManifestName;
				if (!RelativeFile.Exists(serverManifestRelPath, sourceImage))
				{
					string fileFullPath = Path.Combine(servicesImagesPath, serverManifestRelPath);
					throw new FileNotFoundException
						(
						string.Format(CultureInfo.InvariantCulture, "Cannot find file '{0}'.", fileFullPath),
						fileFullPath
						);
				}

				bool imgNewer = IsDeploymentServerPathUpdated(sourceImage, serverManifestRelPath, services);
				return !imgNewer;
				// end-hack
			}
		}

		//---------------------------------------------------------------------
		public virtual bool CopyUpdatedServicesFiles(XmlElement servicesEl, TargetImage targetImage)
		{
			string microareaServerPath = new DirectoryInfo(imagesPath).Parent.FullName;
			string container = NameSolver.NameSolverStrings.Images;	// TEMP
			int nModules = CountModules(servicesEl);
			int currentModule = 0;
			using (ISourceImage sourceImage = new SourceImage(microareaServerPath))
			{
				// crea serverServicesElement e decorarlo con le date di services
				XmlElement serverServicesEl = ProductInfo.GetServerServicesElement(servicesImagesPath);
				ConfigurationManager.DecorateApplicationWithDates(serverServicesEl, servicesEl);
				
				CopyUpdatedApplicationFiles
					(
					sourceImage, 
					false,
					null,
					container, 
					serverServicesEl, 
					targetImage,
					nModules, 
					ref currentModule,
					true
					);
			}
			return true;	// TODOFEDE - gestire anche casi di false!
		}

		/// <summary>
		/// Indica se almeno uno dei moduli delle applicazioni contenute all'interno
		/// di un'immagine ed esplicitate nel ClientManifest contiene degli aggiornamenti
		/// </summary>
		/// <param name="storageName"></param>
		/// <param name="storageRelease"></param>
		/// <param name="configurationDoc">configurazione del client</param>
		/// <returns>un booleano</returns>
		//---------------------------------------------------------------------
		public bool IsImageUpdated(string storageName, string storageRelease, XmlDocument configurationDoc)
		{
			using (ISourceImage sourceImage = new SourceImage(this, storageName, storageRelease))
			{
				if (IsImageRootUpdated(sourceImage, configurationDoc))
					return true;

				// controllo aggiornamento Solutions
				if (IsSolutionBranchUpdated(sourceImage, configurationDoc))
					return true;

				// controllo aggiornamento applicazioni
				foreach (XmlElement cont in configurationDoc.GetElementsByTagName(TagContainer))
				{
					string container = cont.GetAttribute(Consts.AttributeName);
					XmlNodeList apps = cont.GetElementsByTagName(TagApplication);
					foreach (XmlElement app in apps)
						if (IsApplicationUpdated(sourceImage, container, app))
							return true;
				}
				return false;
			}
		}

		//---------------------------------------------------------------------
		private bool IsImageRootUpdated(ISourceImage sourceImage, XmlDocument configurationDoc)
		{
			XmlElement productEl = ConfigurationManager.RetrieveClientProductDetailsElement(configurationDoc.DocumentElement, sourceImage.Product);
			XmlElement imageRoot = productEl.SelectSingleNode(ConfConsts.TagImageRoot) as XmlElement;
			return IsUnitUpdated(sourceImage, string.Empty, imageRoot, sourceImage.Product);
		}

		//---------------------------------------------------------------------
		public bool IsClientSolutionBranchUpdated
			(
			XmlDocument physicalSolutionConfiguration,
			string product,
			string uiCultureName,
			string maxRelease,
			out string servedRelease
			)
		{
			if (!IsProductSupported(product, maxRelease))
			{
				servedRelease = null;
				return true;
			}
			servedRelease = GetTopImageRelease(product, maxRelease);
			using (ISourceImage sourceImage = new SourceImage(this, product, servedRelease, false))
			{
				string includePath = Consts.DirDictionary + Path.DirectorySeparatorChar + uiCultureName;
				XmlDocument functionalSolutionConfiguration = ConfigurationManager.GetServerSolutionConfiguration(includePath);
				ConfigurationManager.DecorateSolutionsWithDates(ref functionalSolutionConfiguration, physicalSolutionConfiguration);

				XmlElement solution = functionalSolutionConfiguration.SelectSingleNode("//" + TagSolution) as XmlElement;
				
				return !IsSolutionBranchUpdated(sourceImage, solution);
			}
		}

		//---------------------------------------------------------------------
		private bool IsSolutionBranchUpdated(ISourceImage sourceImage, XmlElement solution)	// TODOFEDE - DEPRECATED
		{
			if (solution == null)
				return true;
			return IsUnitUpdated(sourceImage, string.Empty, solution, sourceImage.Product);
		}
		private bool IsSolutionBranchUpdated(ISourceImage sourceImage, XmlDocument configurationDoc)
		{
			// NOTE - il manifest della solutions riporta anche il nome del prodotto
			//		per disambiguare il caso di + prodotti installati nel client
			//		all'interno di una sola installazione

			XmlElement productEl = ConfigurationManager.RetrieveClientProductDetailsElement(configurationDoc.DocumentElement, sourceImage.Product);
			if (productEl == null)
				return true;
			XmlElement solution = productEl.SelectSingleNode(ConfConsts.TagSolution) as XmlElement;
			
			if (solution == null)
				return true;
			return IsUnitUpdated(sourceImage, string.Empty, solution, sourceImage.Product);
		}

		/// <summary>
		/// Indica se almeno uno dei moduli di una singola applicazione contenuta
		/// all'interno di un'immagine ed esplicitate nel ClientManifest contiene
		/// degli aggiornamenti
		/// </summary>
		/// <param name="sourceImage"></param>
		/// <param name="container">nome della container directory nell'immagine di distribuzione</param>
		/// <param name="applicationElement">elemento del documento di configurazione associato all'applicazione in oggetto</param>
		/// <returns>un booleano</returns>
		//---------------------------------------------------------------------
		private bool IsApplicationUpdated(ISourceImage sourceImage, string container, XmlElement applicationElement)
		{
			string appDir = applicationElement.GetAttribute(Consts.AttributeName);
			string applicationRelPath = Path.Combine(container, appDir);
			
			if (!RelativeDirectory.Exists(applicationRelPath, sourceImage))
				// potrebbe essere il caso di un'applicazione non più supportata
				return false;

			// controllo se vi sono aggiornamenti nell'applicazione
			string serverAppManifestName = ManifestManager.BuildDirectoryManifestFileName(PolicyType.Full);
			string serverAppManifestRelPath = Path.Combine(applicationRelPath, serverAppManifestName);
			if (IsDeploymentServerPathUpdated(sourceImage, serverAppManifestRelPath, applicationElement))
				return true;

			// controllo che non vi siano moduli aggiornati tra quelli installati
			XmlNodeList mods = applicationElement.GetElementsByTagName(TagModule);
			foreach (XmlElement mod in mods)
				if (IsModuleUpdated(sourceImage, applicationRelPath, mod))
					return true;

			return false;
		}

		/// <summary>
		/// Indica se un modulo contiene degli aggiornamenti rispetto alla data
		/// indicata per esso nel documento di configurazione
		/// </summary>
		/// <param name="sourceImage"></param>
		/// <param name="applicationRelPath">path relativo dell'applicazione nell'immagine distribuita</param>
		/// <param name="moduleElement">elemento del documento di configurazione associato al modulo in oggetto</param>
		/// <returns>un booleano</returns>
		//---------------------------------------------------------------------
		private bool IsModuleUpdated(ISourceImage sourceImage, string applicationRelPath, XmlElement moduleElement)
		{
			return IsUnitUpdated(sourceImage, applicationRelPath, moduleElement, null);
		}

		//---------------------------------------------------------------------
		private bool IsUnitUpdated(ISourceImage sourceImage, string parentRelPath, XmlElement unitElement, string relatedSpecificProduct)
		{
			string strUnitDate	= unitElement.GetAttribute(Consts.AttributeDate);
			string unitSubPath	= unitElement.GetAttribute(Consts.AttributeName);
			string unitRelPath	= Path.Combine(parentRelPath, unitSubPath);
			
			if (unitRelPath.Length > 0 && !RelativeDirectory.Exists(unitRelPath, sourceImage))
				// potrebbe essere il caso di un modulo non più supportato
				return false;

			// OPTIMIZE
			if (strUnitDate.Length == 0 ||
				strUnitDate == DateTime.MinValue.ToString("s"))
				return true;

			PolicyType policy = ManifestManager.GetModulePolicy(unitElement);
			if (policy == PolicyType.Unknown) // non specificata, va default-ata a Full
				policy = PolicyType.Full;

			// controllo se vi sono aggiornamenti nella unit
			string serverUnitManifestName = ManifestManager.BuildDirectoryManifestFileName(relatedSpecificProduct, policy);
			string serverUnitManifestRelPath = Path.Combine(unitRelPath, serverUnitManifestName);
			if (IsDeploymentServerPathUpdated(sourceImage, serverUnitManifestRelPath, unitElement))
				return true;

			// controllo se vi sono includePaths (es. dictionaries) aggiornati tra quelli licenziati
			// TEMP ? - così controlla tra quelli installati, occorre farlo sui licenziati!
			XmlNodeList incs = unitElement.GetElementsByTagName(TagIncludeModulesPath);
			foreach (XmlElement inc in incs)
			{
				string incSubPath = inc.GetAttribute(Consts.AttributeName);
				string incRelPath = Path.Combine(unitRelPath, incSubPath);
				if (IsIncludePathUpdated(sourceImage, incRelPath, inc, relatedSpecificProduct))
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		private bool IsIncludePathUpdated(ISourceImage sourceImage, string incRelPath, XmlElement includeElement, string relatedSpecificProduct)
		{
			if (!RelativeDirectory.Exists(incRelPath, sourceImage))
				// potrebbe essere il caso di un dizionario non più supportato
				return false;

			string serverIncManifestName = ManifestManager.BuildDirectoryManifestFileName(relatedSpecificProduct, PolicyType.Full);
			string serverIncManifestRelPath = Path.Combine(incRelPath, serverIncManifestName);
			return IsDeploymentServerPathUpdated(sourceImage, serverIncManifestRelPath, includeElement);
		}

		//---------------------------------------------------------------------
		private bool IsDeploymentServerPathUpdated(ISourceImage sourceImage, string serverManifestRelPath, XmlElement clientElement)
		{
			if (!RelativeFile.Exists(serverManifestRelPath, sourceImage))
				// potrebbe essere il caso di un dictionary non più supportato
				return false;

			// reperisce la release indicata nel manifest sul client
			string clientRelease = clientElement.GetAttribute(Consts.AttributeRelease);
			if (clientRelease.Length == 0)
				return true;	// non presente sul client
			// reperisce la release indicata nel manifest sul server
			string serverRelease = sourceImage.GetManifestRelease(serverManifestRelPath);
			
			// se la release sul server è maggiore, il client è da aggiornare (vero dalla 1.0.0.20040129)
			System.Version clientVersion = new System.Version(clientRelease);	// TODO - try-catch
			System.Version serverVersion = new System.Version(serverRelease);
			return clientVersion < serverVersion;
			// NOTE - il solo cambio di manifestVersion (con release immutata)
			//		non mi scateni una richiesta di aggiornamenti, quindi qui
			//		non lo controllo
		}

		/// <summary>
		/// copia in una directory temporanea i files che rappresentano un
		/// aggiornamento per l'utente
		/// </summary>
		/// <param name="configurationDoc">configurazione dell'utente</param>
		/// <param name="clientImageVersion"></param>
		/// <param name="storageName">nome immagine a cui effettuare l'aggiornamento</param>
		/// <param name="storageRelease">release immagine a cui effettuare l'aggiornamento</param>
		/// <param name="targetImage">contesto immagine di destinazione</param>
		/// <param name="usePatches">booleano che indica se il back-end debba fornire ove possibile gli aggiornamenti sotto forma di patches</param>
		/// <param name="maxRelease">max release concessa all'utente. String.Empty per non porre limiti</param>
		/// <returns>un booleano che indica il successo dell'operazione</returns>
		//---------------------------------------------------------------------
		public virtual bool CopyUpdatedImageFiles
			(
				XmlDocument configurationDoc, 
				ImageVersion clientImageRel,
				string storageName,
				string storageRelease,
				TargetImage targetImage,
				bool usePatches,
				string maxRelease
			)
		{
			if (this.aborted)
				return false;

			// NOTE - I file manifest di applicazione e modulo non sono listati in sé
			//		stessi, per cui le date di aggiornamenti riportate nei macro
			//		elementi del documento di configurazione non li considerano; essi sono tuttavia
			//		copiati nell'aggiornamento perché essendo generati per ultimi saranno
			//		sempre più recenti di tutti gli altri files. Perché ciò sia sempre
			//		vero occorre che l'accesso alle universal images sia blindato, ed
			//		eseguito solo da da MasterMaker che al termine della generazione
			//		della immagine aggiornata rigenera i manifest e ve li aggiunge.
			//		WARNING ! (presupposto al corretto funzionamento)

			// NOTE - il try-catch è gestito + in alto

			// reperisco l'elenco delle immagini a disposizione per trovarvi patches
			VersionedSourceImage[] deploymentImages = null;
			ISourceImage sourceImage;

			string requiredExtended = GetImageReleaseExtended(storageName, storageRelease);
			ImageVersion requiredVer = new ImageVersion(requiredExtended);
			if (requiredVer == clientImageRel)
				usePatches = false;
			if (usePatches)
			{
				deploymentImages = GetSourceImages(clientImageRel, storageName, storageRelease, maxRelease);
				sourceImage = deploymentImages[deploymentImages.Length - 1].sourceImage;	// deve esisterne almeno una
			}
			else
			{
				sourceImage = new SourceImage(this, storageName, storageRelease);
				VersionedSourceImage vsi = new VersionedSourceImage(requiredVer, sourceImage);
				deploymentImages = new VersionedSourceImage[] {vsi};
			}

			XmlElement productEl = ConfigurationManager.RetrieveClientProductDetailsElement(configurationDoc.DocumentElement, sourceImage.Product);
			// copia files specifici della root di immagine
			CopyUpdatedImageRootFiles
				(
				productEl,
				sourceImage,
				//clientImageVersion,
				targetImage,
				storageName
				);

			int nModules = CountModules(configurationDoc);
			int currentModule = 0;

			// copia i files di solution ed articoli commerciali
			XmlElement solution = productEl.SelectSingleNode(ConfConsts.TagSolution) as XmlElement;
			if (solution == null) return false;
			CopyUpdatedSolutionArticlesFiles(sourceImage, targetImage, solution, storageName, nModules, ref currentModule);

			// copia gli aggiornamenti delle applicazioni denunciate nella configurazione
			foreach (XmlElement cont in configurationDoc.GetElementsByTagName(TagContainer))
			{
				string container = cont.GetAttribute(Consts.AttributeName);
				foreach (XmlElement app in cont.GetElementsByTagName(TagApplication))
					CopyUpdatedApplicationFiles
						(
						sourceImage, 
						usePatches, 
						deploymentImages,
						container, 
						app, 
						targetImage,
						nModules, 
						ref currentModule,
						false
						);
			}

			foreach (VersionedSourceImage deploymentImage in deploymentImages)
				deploymentImage.sourceImage.Dispose();

			if (this.aborted)
				return false;

			return true;
		}

		//---------------------------------------------------------------------
		private void CopyUpdatedImageRootFiles
			(
			XmlElement productEl,
			ISourceImage sourceImage,
			//ImageVersion clientImageVersion,
			TargetImage targetImage,
			string product
			)
		{
			if (this.aborted)
				return;

			// BACKWARDCOMPATIBILITY - dalla 1.1.0.20040430 il manifest di immagine è prefissato con
			//		il nome del prodotto ed include il file DependanciesMap, quindi si può usarlo per
			//		deploy-are i files di root dell'immagine.
			//		si garantisce compatibilità con la 1.0, non con le build successive e mai pubblicate,
			//		precedenti alla build indicata
			if (string.Compare(sourceImage.Product, "MagoNet-Pro", true, CultureInfo.InvariantCulture) == 0 &&
				string.Compare(sourceImage.Release, "1.0", true, CultureInfo.InvariantCulture) == 0)
			{
				// BACKWARDCOMPATIBILITY - gestisce installazione di MagoNet-Pro 1.0
				// all'epoca manifest e DependenciesMap non venivano deploy-ati, né servivano,
				// Il DependenciesMap sul client serve per eliminare moduli cadaveri in seguito
				// ad un aggiornamento, quindi nella prima installazione non serve
				return;
			}

			// root manifest is product related
			Debug.Assert(productEl != null);
			XmlElement imageRoot = productEl.SelectSingleNode(ConfConsts.TagImageRoot) as XmlElement;
			Debug.Assert(imageRoot != null);
			string strLastRootUpdate		= imageRoot.GetAttribute(Consts.AttributeDate);
			string clientReleaseExt			= imageRoot.GetAttribute(Consts.AttributeRelease);
			string clientManifestVersion	= imageRoot.GetAttribute(Consts.AttributeVersion);

			// il manifest devo copiarlo sempre e comunque, quindi non mi curo troppo
			// della versione/release
			CopyUpdatedManifestFiles
				(
				sourceImage, 
				false, 
				clientReleaseExt,
				clientManifestVersion,
				null,
				string.Empty, 
				targetImage,
				PolicyType.Full, 
				strLastRootUpdate,
				product,
				false
				);
		}

		/// <summary>
		/// conta il numero dei moduli funzionali che costituiscono la configurazione completa
		/// </summary>
		/// <param name="configurationDoc"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private int CountModules(XmlDocument configurationDoc)
		{
			return CountModules(configurationDoc.DocumentElement) + 1;	// aggiungo quello di Solution
		}
		private int CountModules(XmlElement el)
		{
			string xPath = "//" + ConfConsts.TagModule;
			return el.SelectNodes(xPath).Count;
		}

		/// <summary>
		/// Copia applicatione e moduli licenziati, e manifest contenuti
		/// in un'immagine in una directory temporanea indicata
		/// </summary>
		/// <param name="sourceImage"></param>
		/// <param name="usePatches"></param>
		/// <param name="deploymentImages"></param>
		/// <param name="container">nome della container directory</param>
		/// <param name="applicationElement">elemento del documento di configurazione associato all'applicazione in oggetto</param>
		/// <param name="targetImage">contesto immagine di destinazione</param>
		/// <param name="totModules"></param>
		/// <param name="currentModule"></param>
		/// <param name="services"></param>
		//---------------------------------------------------------------------
		private void CopyUpdatedApplicationFiles
			(
				ISourceImage sourceImage,
				bool usePatches,
				VersionedSourceImage[] deploymentImages,
				string container,
				XmlElement applicationElement,
				TargetImage targetImage,
				int totModules,
				ref int currentModule,
				bool services
			)
		{
			if (this.aborted)
				return;

			string appDir = applicationElement.GetAttribute(Consts.AttributeName);
			string applicationRelPath = Path.Combine(container, appDir);
			
			if (!RelativeDirectory.Exists(applicationRelPath, sourceImage))
			{
				// potrebbe essere un'applicazione di una vecchia configurazione,
				// non + presente nell'immagine
				// NOTE - potrebbe capitare spesso, non penso sia il caso di registrare evento, ma nel caso farlo qui
				return;
			}

			string strLastAppUpdate = applicationElement.GetAttribute(Consts.AttributeDate);
			string clientReleaseExt = applicationElement.GetAttribute(Consts.AttributeRelease);
			string clientManifestVersion = applicationElement.GetAttribute(Consts.AttributeVersion);
			
			// copia i files più recenti nella directory dell'immagine
			CopyUpdatedManifestFiles
				(
				sourceImage, 
				usePatches, 
				clientReleaseExt,
				clientManifestVersion,
				deploymentImages,
				applicationRelPath, 
				targetImage,
				PolicyType.Full, 
				strLastAppUpdate, 
				null,
				services
				);
			
			if (this.aborted)
				return;

			// Copia gli aggiornamenti dei moduli
			XmlNodeList mods = applicationElement.GetElementsByTagName(TagModule);
			foreach (XmlElement mod in mods)
			{
				// copia i files aggiornati del modulo
				// il paths lo ottiene concatenando i path relativi dichiarati nel documento di configurazione
				string modDir	= mod.GetAttribute(Consts.AttributeName);
				string strDate	= mod.GetAttribute(Consts.AttributeDate);
				string moduleRelPath = Path.Combine(applicationRelPath, modDir);
				CopyUpdatedModuleFiles
					(
					sourceImage, 
					usePatches, 
					deploymentImages,
					mod, 
					moduleRelPath, 
					targetImage,
					strDate, 
					null, 
					totModules, 
					ref currentModule,
					services
					);
			}
		}

		/// <summary>
		/// copia i files più recenti della data di riferimento contenuti nel subPath
		/// da imagePath a tempDir. La struttura delle sottodirectory è riprodotta solo
		/// per le applicazioni/moduli installati sul client
		/// </summary>
		/// <param name="sourceImage">contesto immagine sorgente</param>
		/// <param name="usePatches"></param>
		/// <param name="deploymentImages"></param>
		/// <param name="moduleElement">elemento del documento di configurazione associato al modulo in oggetto</param>
		/// <param name="moduleRelPath"></param>
		/// <param name="targetImage">contesto immagine di destinazione</param>
		/// <param name="lastUpdateString">data ultimo aggiornamento modulo del client</param>
		/// <param name="relatedSpecificProduct"></param>
		/// <param name="totModules"></param>
		/// <param name="currentModule"></param>
		/// <param name="services"></param>
		//---------------------------------------------------------------------
		private void CopyUpdatedModuleFiles
			(
				ISourceImage sourceImage,
				bool usePatches,
				VersionedSourceImage[] deploymentImages,
				XmlElement moduleElement,
				string moduleRelPath, 
				TargetImage targetImage,
				string lastUpdateString,
				string relatedSpecificProduct,
				int totModules,
				ref int currentModule,
				bool services
			)
		{
			if (this.aborted)
				return;

			RaiseCopyingModuleUpdates(moduleRelPath, totModules, ++currentModule);

			if (!RelativeDirectory.Exists(moduleRelPath, sourceImage))
			{
				// potrebbe essere un modulo di una vecchia configurazione,
				// non + presente nell'immagine
				// NOTE - potrebbe capitare spesso, non penso sia il caso di registrare evento, ma nel caso farlo qui
				// NOTE - in realtà questo controllo è ridondante perché il metodo invocato controlla anch'esso
				return;
			}

			string clientReleaseExt			= moduleElement.GetAttribute(Consts.AttributeRelease);	// TEMP - usa head
			string clientManifestVersion	= moduleElement.GetAttribute(Consts.AttributeVersion);

			// switch-a tra copia files da directory per policy Full
			// e tra la lettura dei files del manifest se policy Base
			string policyString = ManifestManager.GetModulePolicyString(moduleElement);
			PolicyType policy;
			if (policyString.Length == 0)
				policy = PolicyType.Full;	// se non specificato si intende full
			else
				policy = ManifestManager.GetModulePolicy(moduleElement);

			CopyUpdatedManifestFiles
				(
				sourceImage, 
				usePatches, 
				clientReleaseExt,
				clientManifestVersion,
				deploymentImages,
				moduleRelPath, 
				targetImage,
				policy, 
				lastUpdateString, 
				relatedSpecificProduct,
				services
				);

			if (this.aborted)
				return;

			// Copia degli IncludeModulesPath
			foreach (XmlElement inc in moduleElement.GetElementsByTagName(TagIncludeModulesPath))
			{
				string incDir	= inc.GetAttribute(Consts.AttributeName);
				string strDate	= inc.GetAttribute(Consts.AttributeDate);
				string incRel	= inc.GetAttribute(Consts.AttributeRelease);
				string incVer	= inc.GetAttribute(Consts.AttributeVersion);
				
				string sourceRelPath = Path.Combine(moduleRelPath, incDir);
				if (!RelativeDirectory.Exists(sourceRelPath, sourceImage))
					continue;

				CopyUpdatedManifestFiles
					(
					sourceImage, 
					usePatches, 
					incRel,
					incVer,
					deploymentImages,
					sourceRelPath, 
					targetImage,
					PolicyType.Full, 
					strDate, 
					relatedSpecificProduct,
					services
					);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceImage"></param>
		/// <param name="usePatches">Bool che indica se usare ove possibile aggiornamenti sotto forma di patches</param>
		/// <param name="clientReleaseExt">Release della unit sul client. Se !usePatches, può essere string.Empty</param>
		/// <param name="clientManifestVersion">Versione del manifest di unit sul client. Se !usePatches, può essere null</param>
		/// <param name="deploymentImages">elenco immagini di deployment disponibili. Se !usePatches, può essere null</param>
		/// <param name="sourceRelPath"></param>
		/// <param name="targetImage">contesto dove copiare gli aggiornamenti</param>
		/// <param name="policy"></param>
		/// <param name="refDateString"></param>
		/// <param name="relatedSpecificProduct"></param>
		/// <param name="services"></param>
		//---------------------------------------------------------------------
		private void CopyUpdatedManifestFiles
			(
			ISourceImage sourceImage, 
			bool usePatches, 
			string clientReleaseExt,
			string clientManifestVersion,
			VersionedSourceImage[] deploymentImages,
			string sourceRelPath, 
			TargetImage targetImage,
			PolicyType policy, 
			string refDateString, 
			string relatedSpecificProduct,
			bool services
			)
		{
			if (this.aborted)
				return;

			// costruisce nome manifest
			string manifestName = ManifestManager.BuildDirectoryManifestFileName(relatedSpecificProduct, policy);
			string manifestRelPath = Path.Combine(sourceRelPath, manifestName);

			ManifestObject manifestObj = null;
			ManifestFileObject[] updatedFiles = null;
			
			// TODOFEDE - ora la serverVersion non è uguale in tutta l'immagine di deployment,
			//			meglio beccare velocemente l'head per capire se ci sono aggiornamenti
			//			ed evitare di caricare tutto l'oggetto nel caso (ottimizzazione)
			//if (!IsDeploymentServerPathUpdated(sourceImage, manifestRelPath, 

			// Ottimizzazione: se usePatches forse posso evitare di costruirmi l'oggetto intero
			if (usePatches						&&
				policy == PolicyType.Full		&& // ho visto che in alcuni casi non posso assicurare patching corretto se Base
				refDateString.Length != 0		&&
				deploymentImages != null		&&
				deploymentImages.Length > 0)
			{
				// effettua per tutte le deploymentImages la copia del file di patch
				// Il fatto che ciò sia compatibile con le logiche di deployment è affidato
				// alla correttezza della lista deploymentImages[] ricevuta.
				// Se il server non disponesse delle patch per aggiornare la versione su client,
				// allora usa direttamente i files.
				// Se la dimensione dei file di patch supera quella dei file di aggiornamento,
				// usa allora questi ultimi.

				ImageVersion clientManifestRelease = new ImageVersion(clientReleaseExt);
				ImageVersion topImageRelease = deploymentImages[deploymentImages.Length - 1].version;

				if (topImageRelease < clientManifestRelease)	// NOTE - non dovrebbe capitare mai
				{
					Debug.Fail("client image + recente di quella di deployment?");
					return;
				}

				// controllo preventivo per il quale se non trova nelle immagini le patch
				// sufficienti ad upgradare la unit allora fornisce direttamente i files
				if (ServerCanUpdateUnitWithPatches(deploymentImages, manifestRelPath, clientManifestRelease))
				{
					// crea elenco files di patch
					ArrayList patchesToCopy = new ArrayList(deploymentImages.Length);

					// copia files di patch
					foreach (VersionedSourceImage vsi in deploymentImages)
					{
						// se nell'immagine non esiste l'unità di deployment, passo
						if (!RelativeFile.Exists(manifestRelPath, vsi.sourceImage))
							continue;

						string vsiManifestReleaseString = vsi.sourceImage.GetManifestRelease(manifestRelPath);

						// se nell'immagine la patch esiste, ha nome omonimo alla release (M.m.SP) del manifest,
						// ossia della unit cui fa riferimento
						string historyPatchName = GetPatchFileName(vsiManifestReleaseString);
						string historyPatchRelPath = Path.Combine(sourceRelPath, historyPatchName);
						// se nell'immagine non esiste la patch, passo
						if (!RelativeFile.Exists(historyPatchRelPath, vsi.sourceImage))
							continue;

						ImageVersion vsiManifestRelease = new ImageVersion(vsiManifestReleaseString);

						// devo paragonare clientManifestVersion con vsiManifestVersion
						// se vsiManifestVersion < clientManifestVersion, continue
						// se vsiManifestVersion = clientManifestVersion, continue
						// se vsiManifestVersion > clientManifestVersion, copia la patch
						if (vsiManifestRelease <= clientManifestRelease)
							continue;

						// aggiunge historyPatchRelPath e suo size in arraylist
						patchesToCopy.Add(new HistoryPatch(historyPatchRelPath, vsi.sourceImage, services));
					}

					// se non si hanno patches per aggiornare la unit, si usano i files
					if (patchesToCopy.Count != 0)	// si hanno patches
					{
						// reperisce size di ogni singola patch e le somma
						long patchesTotalSize = 0;
						foreach (HistoryPatch historyPatch in patchesToCopy)
							patchesTotalSize += historyPatch.Size;

						manifestObj = sourceImage.GetManifestObject(manifestRelPath);
						if (manifestObj == null)
						{
							//Debug.Fail("Immagine corrotta, manca manifest");
							return;
						}
						
						// computo dimensione files aggiornati
						updatedFiles = manifestObj.GetUpdatedFiles(refDateString);
						long updatedFilesSize = ManifestObject.GetFilesSize(updatedFiles);

						// applico l'euristica per decidere se l'uso delle patches è conveniente
						// rispetto all'uso dei files (solo per quanto riguarda il peso nel download)
						if (patchesTotalSize < updatedFilesSize * 3 / 7)
						{
							foreach (HistoryPatch historyPatch in patchesToCopy)
								historyPatch.Copy(targetImage, this);
							RelativeFile.CopyWithPath(manifestRelPath, targetImage, sourceImage, this, services);
							return;
						}
						else	// altrimenti copia tutti i files!
							Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "l'uso dei files di patch non è conveniente per {0}: patches: {1}, files: {2}", manifestRelPath, patchesTotalSize, updatedFilesSize));
					}	// altrimenti copia tutti i files!
				}
			}

			// se si è giunti a questo punto, l'aggiornamento viene effettuato tramite files (e non patches)
			if (manifestObj == null)
			{
				manifestObj = sourceImage.GetManifestObject(manifestRelPath);
				if (manifestObj == null)
				{
					//Debug.Fail("Immagine corrotta, manca manifest");
					return;
				}
				updatedFiles = manifestObj.GetUpdatedFiles(refDateString);
			}

			int copied = 0;
			foreach (ManifestFileObject fileObj in updatedFiles)
			{
				string sourceFileRelPath = Path.Combine(sourceRelPath, fileObj.Name);
				RelativeFile.CopyWithPath(sourceFileRelPath, targetImage, sourceImage, this, services);
				++copied;
			}

			// copia anche il manifest (esiste, se no manifestObj sarebbe stato null)
			if (copied > 0													||
				manifestObj.Head.UnitRelease != clientReleaseExt			||
				manifestObj.Head.ManifestVersion != clientManifestVersion)
				RelativeFile.CopyWithPath(manifestRelPath, targetImage, sourceImage, this, services);
		}

		//---------------------------------------------------------------------
		private bool ServerCanUpdateUnitWithPatches
			(
			VersionedSourceImage[] deploymentImages,
			string manifestRelPath,
			ImageVersion clientUnitRel
			)
		{
			// controllo preventivo per il quale se non trova nelle immagini le patch
			// sufficienti ad upgradare la unit allora fornisce direttamente i files
			//
			// deploymentImages[] viene prodotto da GetSourceImages con immagini la cui
			// ver di imag è:
			//					clientImageVersion  <=  ver  <=  requiredVer
			// ogni unità di deployment può avere release inferiore a quella della propria img,
			// devo quindi controllare che l'immagine più bassa, che contiene l'ultima SP della
			// propria M.m, mi permetta di aggiornare l'unità a tale SP, deve quindi essere di
			// di uguale M.m (ma avere SP + alta)

			VersionedSourceImage bottomImage = deploymentImages[0];
			if (!RelativeFile.Exists(manifestRelPath, bottomImage.sourceImage))
				return false;	// potrebbe capitare che il modulo funzionale viene abbandonato e poi riesumato?

			string bottomUnitManifestRel = bottomImage.sourceImage.GetManifestRelease(manifestRelPath);
			ImageVersion buRel = new ImageVersion(bottomUnitManifestRel);

			// se nell'immagine di deployment più vecchia la unit ha M.m più bassa, il server non può
			// usare RTPatch per aggiornare il client in tale unit
			if (buRel.Major > clientUnitRel.Major	||
				(buRel.Major == clientUnitRel.Major && buRel.Minor > clientUnitRel.Minor))
				return false;

			// NOTE - magari in realtà potrebbe, quando la rel sul client è già la max SP
			//		della M.m precedente, ma il server non può discriminare il caso e nel
			//		dubbio non usa patch

			// a questo punto la unit nella bottom image M.m >= di quella del client e quindi
			// il server può usare patches per aggiornarlo.
			// NOTE - il caso di = non dovrebbe verificarsi ma non comprometterebbe il funzionamento.

			// handle case of a dep unit which disappears in the official release history, and then appears back
			// - get upperUnit "born" attribute
			// - if born is empty, no problem using the patch, as the module existed since M.m.0 (or ever?????)
			// - if born is greater than clientUnitRel, return false
			VersionedSourceImage upperImage = deploymentImages[deploymentImages.Length - 1];
			Debug.Assert(RelativeFile.Exists(manifestRelPath, upperImage.sourceImage));
			string upperUnitBornRel = upperImage.sourceImage.GetManifestBorn(manifestRelPath);
			if (upperUnitBornRel != null && upperUnitBornRel.Length != 0)
			{
				ImageVersion bRel = new ImageVersion(upperUnitBornRel);
				if (bRel > clientUnitRel)
				{
					Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Cannot use patch in {0} to update client.", Path.GetDirectoryName(manifestRelPath)));
					return false;
				}
			}
			return true;
		}

		//---------------------------------------------------------------------
		private ArrayList GetUpdatedManifestFilesList
			(
			ISourceImage sourceImage, 
//			string clientReleaseExt,
			string clientManifestVersion,
			string sourceRelPath, 
			string refDateString, 
			string productName
			)
		{
			ArrayList list = new ArrayList();

			// costruisce nome manifest
			string manifestName = ManifestManager.BuildDirectoryManifestFileName(productName, PolicyType.Full);
			string manifestRelPath = Path.Combine(sourceRelPath, manifestName);

			ManifestObject manifestObj = sourceImage.GetManifestObject(manifestRelPath);
			if (manifestObj == null)
			{
				//Debug.Fail("Immagine corrotta, manca manifest");
				return list;
			}

			int nFiles = 0;
			foreach (ManifestFileObject fileObj in manifestObj.Files)
				if (string.Compare(fileObj.DateString, refDateString, true, CultureInfo.InvariantCulture) > 0)
				{
					//string sourceFileRelPath = Path.Combine(sourceRelPath, fileObj.Name);
					list.Add(fileObj.Name);
					nFiles++;
				}

			// copia anche il manifest (esiste, se no manifestObj sarebbe stato null)
			if (nFiles > 0 ||
				manifestObj.Head.ManifestVersion != clientManifestVersion)
				list.Add(manifestName);
			
			return list;
		}

		//---------------------------------------------------------------------
		private static string GetPatchFileName(string release)
		{
			string[] items = release.Split('.');
			return string.Join("_", items, 0, 3) + Consts.ExtensionRtp;
		}

		/*
		/// <summary>
		/// restituisce l'array di versioni (in vormato esteso) delle immagini del prodotto
		/// presenti sulla sorgentedi deployment fino alla versione richiesta
		/// </summary>
		/// <param name="product"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public ImageVersion[] GetDeploymentVersions(string product, ImageVersion clientVersion, ImageVersion requiredVersion)
		{
			// prendo tutte le release disponibili
			ImageVersion[] productReleases = GetSortedProductReleasesExtended(product);
			// di queste prendo per ogni M.m quella con SP + alta
			// (sul server di deployment dovrebbe essercene una sola per M.m, ma non si sa mai)
			ImageVersion[] topMostReleases = GetTopMostProductReleasesExtended(productReleases);
			// di queste prendo solo quelle <= all'immagine richiesta al server
			bool present;
			ImageVersion[] filteredReleases = GetReleasesUpTo(topMostReleases, requiredVersion, out present);
			Debug.Assert(present, "immagine richiesta non trovata");

			return filteredReleases;
		}
		*/

		//---------------------------------------------------------------------
		// needs to be public for unit testing
		public VersionedSourceImage[] GetSourceImages
			(
			ImageVersion clientImageVersion, 
			string product, 
			string requiredRelease,
			string maxRelease
			)
		{
			ImageVersion[] imgs4Source = 
				GetImagesVersionsForSource(clientImageVersion, product, requiredRelease, maxRelease);
			List<VersionedSourceImage> list = new List<VersionedSourceImage>(imgs4Source.Length);
			foreach (ImageVersion ver in imgs4Source)
			{
				ISourceImage sourceImage = new SourceImage(this, product, ver.ToString());
				VersionedSourceImage vsi = new VersionedSourceImage(ver, sourceImage);
				list.Add(vsi);
			}
			list.Sort();
			return list.ToArray();
		}

		//---------------------------------------------------------------------
		// needs to be public for unit testing
		public ImageVersion[] GetImagesVersionsForSource
			(
			ImageVersion clientImageVersion,
			string product,
			string requiredRelease,
			string maxRelease
			)
		{
			ImageVersion requiredVer = new ImageVersion(requiredRelease);

			List<ImageVersion> allRelsFiltered = FilterByPreviousRel(product, requiredRelease);
			string[] prodRels = new string[allRelsFiltered.Count];
			for (int i = 0; i < allRelsFiltered.Count; ++i)
				prodRels[i] = allRelsFiltered[i];
			string[] storageReleases = GetProductTopMostSPImagesRels(product, maxRelease, prodRels);

			List<ImageVersion> list = new List<ImageVersion>();
			foreach (string storageRelease in storageReleases)
			{
				ImageVersion ver = new ImageVersion(storageRelease);
				if (ver > requiredVer)
					continue;	// non sono sicuro sia una lista ordinata
				if (ver < clientImageVersion)
					continue;	// non sono sicuro sia una lista ordinata
				list.Add(ver);
			}
			list.Sort();
			return list.ToArray();
		}

		public List<ImageVersion> FilterByPreviousRel(string product)
		{
			return FilterByPreviousRel(product, string.Empty);
		}
		public List<ImageVersion> FilterByPreviousRel(string product, string requiredRelease)
		{
			string[] releases = repository.GetProductImagesRels(product);
			ImageVersion requiredVer = requiredRelease != string.Empty
				? new ImageVersion(requiredRelease)
				: null;

			// The method must return a collection of version of images consistent;
			// consistency means that it takes into account the previous release each release
			// starts from.
			// The starting collection has thus to be the complete universal images array,
			// and not the top most SPs only.

			// starting from the higher release, checks the declared prevRelease of the image
			// and cuts out the releases between the current and its prevRelease
			List<ImageVersion> unfiltered = new List<ImageVersion>(releases.Length);
			foreach (string rel in releases)
			{
				ImageVersion ver = new ImageVersion(rel);
				if (requiredVer != null && requiredVer < ver)
					continue;
				if (!unfiltered.Contains(ver))
					unfiltered.Add(ver);
			}
			unfiltered.Sort();
			unfiltered.Reverse();

			// now we have a sorted list, highest first
			List<ImageVersion> filteredList = new List<ImageVersion>(releases.Length);
			ImageVersion prev = null;
			for (int i = 0; i < unfiltered.Count; ++i)
			{
				bool added = false;
				ImageVersion curr = unfiltered[i];
				if (prev == null || curr == prev)
				{
					filteredList.Add(curr);
					if (i == unfiltered.Count - 1)
						break; // no point inspecting its previous release
					added = true;
				}
				if (!added && prev != null)
				{
					if (curr < prev)
						break; // cuts away lower versions in case of hole
					if (curr > prev)
						continue; // curr is part of another release branch
				}
				string prevRel = repository.GetPreviousRelease(product, curr.ToString());
				if (string.IsNullOrEmpty(prevRel))
					prev = null; // will add the previous
				else
					prev = new ImageVersion(prevRel);
			}
			return filteredList;
		}

		/*
		//---------------------------------------------------------------------
		protected static string Get2LettersVersion(ImageVersion version)
		{
			return version.Major.ToString() + "." + version.Minor.ToString();
		}
		*/

		/*
		//---------------------------------------------------------------------
		protected ImageVersion[] GetSortedProductReleasesExtended(string product)
		{
			string[] storageReleases = GetProductTopMostSPImagesRels(product);
			ArrayList al1 = new ArrayList();
			foreach (string storageRelease in storageReleases)
			{
				string extended = GetImageReleaseExtended(product, storageRelease);
				ImageVersion ver = new ImageVersion(extended);
				al1.Add(ver);
			}
			al1.Sort();
			return (ImageVersion[])al1.ToArray(typeof(ImageVersion));
		}

		/// <summary>
		/// di tutte le release tiene solo la più alta di ogni M.m
		/// </summary>
		/// <param name="productReleases">array ordinato delle release</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		protected ImageVersion[] GetTopMostProductReleasesExtended(ImageVersion[] productReleases)
		{
			Debug.Assert
				(
				productReleases.Length > 0, 
				"deve esistere almeno un elemento nell'array passato", 
				"ImagesManager.GetTopMostProductReleasesExtended()"
				);
			ArrayList list = new ArrayList();

			int i = productReleases.Length - 1;
			ImageVersion prevRelease = productReleases[i];	// ne deve sempre esistere una
			list.Add(prevRelease);	// la più alta la aggiungo sempre
			ImageVersion currRelease;
			do
			{
				currRelease = productReleases[i];
				if (currRelease.Major != prevRelease.Major ||
					currRelease.Minor != prevRelease.Minor)
					list.Add(currRelease);
				prevRelease = currRelease;
			}
			while (--i >= 0);

			list.Sort();
			return (ImageVersion[])list.ToArray(typeof(ImageVersion));
		}

		//---------------------------------------------------------------------
		protected ImageVersion[] GetReleasesUpTo(ImageVersion[] productReleases, ImageVersion topRelease, out bool present)
		{
			present = false;
			ArrayList list = new ArrayList();
			ImageVersion currRelease;
			int i = 0;
			while (i < productReleases.Length)
			{
				currRelease = productReleases[i++];
				if (currRelease.Equals(topRelease))
					present = true;
				if (currRelease > topRelease)
					break;
				list.Add(currRelease);
			}
			list.Sort();
			return (ImageVersion[])list.ToArray(typeof(ImageVersion));
		}
		*/

		//---------------------------------------------------------------------
		public virtual void RaiseCopyEvents
			(
			string relativeFilePath,
			string originBasePath, 
			string destinationBasePath
			)
		{
		}
		
		//---------------------------------------------------------------------
		public virtual void RaiseCopyingModuleUpdates
			(
			string moduleRelPath,
			int totModules,
			int currentModule
			)
		{
		}
		
		//---------------------------------------------------------------------
		public void CopyFileWithPath
			(
			string relativeFilePath,
			string originBasePath, 
			string destinationBasePath,
			bool services
			)
		{
			string origFileFullName = Path.Combine(originBasePath, relativeFilePath);
			string destFileFullName;
			if (services)
				destFileFullName = Path.Combine(destinationBasePath, GetRelativeFilePathInServices(relativeFilePath));
			else
				destFileFullName = Path.Combine(destinationBasePath, relativeFilePath);
			
			if (File.Exists(destFileFullName))
				File.SetAttributes(destFileFullName, FileAttributes.Normal);
			else
				Directory.CreateDirectory(Path.GetDirectoryName(destFileFullName));
			
			File.Copy(origFileFullName, destFileFullName, true);

			// si assicura che non sia read-only (deve poterlo poi cancellare)
			File.SetAttributes(destFileFullName, File.GetAttributes(destFileFullName) & (~FileAttributes.ReadOnly));

			// TODOFEDE - serve???
			// copia le date del file
			File.SetCreationTimeUtc(	destFileFullName, File.GetCreationTimeUtc(origFileFullName)   );
			File.SetLastAccessTimeUtc(	destFileFullName, File.GetLastAccessTimeUtc(origFileFullName) );
			File.SetLastWriteTimeUtc(	destFileFullName, File.GetLastWriteTimeUtc(origFileFullName)  );
		}
		public static string GetRelativeFilePathInServices(string relativeFileName)
		{
			// toglie "Images/"
			return relativeFileName.Substring(relativeFileName.IndexOfAny(new char[]{Path.DirectorySeparatorChar, '/'}) + 1);
		}
		
		protected virtual void MyQuickCopyFile(string sourceFileName, string destFileName, string destinationBasePath, ImageTask task)
		{
			File.Copy(sourceFileName, destFileName, true);
			File.SetAttributes(destFileName, FileAttributes.Normal);
		}

		//---------------------------------------------------------------------
		public static void MyDeleteFile(string file)
		{
			MyDeleteFile(new FileInfo(file));
		}
		protected static void MyDeleteFile(FileInfo file)
		{
			if (!file.Exists) return;
			// si assicura che non sia read-only per cancellarlo
			file.Attributes = ~FileAttributes.ReadOnly;
			file.Delete();
		}

		//---------------------------------------------------------------------
		public static void MyDeleteDirectory(DirectoryInfo directory)
		{
			if (!directory.Exists) return;
			foreach (DirectoryInfo subDir in directory.GetDirectories())
				MyDeleteDirectory(subDir);
			foreach (FileInfo file in directory.GetFiles())
				MyDeleteFile(file);
			// si assicura che non sia read-only per cancellarlo
			directory.Attributes = FileAttributes.Directory & ~FileAttributes.ReadOnly;
			directory.Delete();
		}

		/// <summary>
		/// Copia le versioni aggiornate dei files di solution ed articoli commerciali
		/// </summary>
		/// <param name="sourceImage">contesto immagine sorgente</param>
		/// <param name="targetImage">contesto immagine di destinazione</param>
		/// <param name="solution">Elemento Xml che identifica la solution del prodotto</param>
		/// <param name="relatedSpecificProduct">prodotto cui sono riferiti i files di solution (indicati in questo ramo come prefisso nel nome del manifest)</param>
		/// <param name="totModules"></param>
		/// <param name="currentModule"></param>
		//---------------------------------------------------------------------
		private void CopyUpdatedSolutionArticlesFiles
			(
			ISourceImage sourceImage,
			TargetImage targetImage,
			XmlElement solution, 
			string relatedSpecificProduct, 
			int totModules, 
			ref int currentModule
			)
		{
			if (this.aborted)
				return;

			// NOTE - non posso usare bool usePatches perché mi serve siano sempre disponibili in chiaro
			// copia i files aggiornati del ramo Solutions
			// il paths lo ottiene concatenando i path relativi dichiarati nel documento di configurazione

			string solutionsDir	= solution.GetAttribute(Consts.AttributeName); // pre-TB2.7 folders structure
			if (!RelativeDirectory.Exists(solutionsDir, sourceImage))
			{
				// search for the Solutions folder inside the application (TB2.7 folders structure)
				solutionsDir = FindSolutionsRelPathInApps(sourceImage);
				if (solutionsDir == null)
					throw new DirectoryNotFoundException(ImagesManager.NoSolutionFolderFoundMsg);
			}
			string strDate	= solution.GetAttribute(Consts.AttributeDate);
			CopyUpdatedModuleFiles
				(
				sourceImage, 
				false, 
				null,
				solution, 
				solutionsDir, 
				targetImage,
				strDate, 
				relatedSpecificProduct, 
				totModules, 
				ref currentModule,
				false
				);
		}

		private const string NoSolutionFolderFoundMsg = "Solution folder not found in deployment image."; // interned anyway as literal

		//---------------------------------------------------------------------
		private string FindSolutionsRelPathInApps(ISourceImage sourceImage)
		{
			// search for the Solutions folder inside the application (TB2.7 folders structure)
			string relAppsPath = Consts.DirApplications;
			if (!RelativeDirectory.Exists(relAppsPath, sourceImage))
				return null;
			foreach (string app in RelativeDirectory.GetRelativeDirectories(relAppsPath, sourceImage))
			{
				string aSolInAppsDirPath = Path.Combine(app, Consts.DirSolutions);
				if (RelativeDirectory.Exists(aSolInAppsDirPath, sourceImage))
					return aSolInAppsDirPath.Replace('/', '\\');
			}
			return null;
		}

		// TODO - spostare in Licence.productInfo
		//---------------------------------------------------------------------
		public static string GetArticleNameFromFile(FileInfo articleFile)
		{
			return articleFile.Name.Substring(0, articleFile.Name.Length - articleFile.Extension.Length);
		}

		// TODO - spostare in Licence.productInfo
		//---------------------------------------------------------------------
		public static string GetSolutionFileName(string productName)
		{
			return productName + Consts.ExtensionProductSolution;
		}

		//---------------------------------------------------------------------
		public void EmptyDirectory(DirectoryInfo directory)
		{
			if (!directory.Exists) return;
			foreach (DirectoryInfo subDir in directory.GetDirectories())
				MyDeleteDirectory(subDir);
			foreach (FileInfo file in directory.GetFiles())
				MyDeleteFile(file);
		}

		//---------------------------------------------------------------------
		public static bool IsDirectoryEmpty(string dirPath)
		{
			if (!Directory.Exists(dirPath))
				return true;
			if (Directory.GetFiles(dirPath).Length == 0 && 
				Directory.GetDirectories(dirPath).Length == 0) // dir vuota
				return true;
			return false;
		}

		//---------------------------------------------------------------------
		public string GetTopImageRelease(string product, string maxRelease)
		{
			string[] productRels = repository.GetProductImagesRels(product);
			if (productRels.Length == 0)
				return null;
			ImageVersion maxRel = maxRelease.Length == 0 ? null : new ImageVersion(maxRelease);
			ImageVersion topRel = null;
			foreach (string strRel in productRels)
			{
				ImageVersion rel = new ImageVersion(strRel);
				if (maxRel != null && rel > maxRel)
					continue;
				if (topRel == null || rel > topRel)
					topRel = rel;
			}
			return topRel != null ? topRel.ToString() : null;
		}

		//---------------------------------------------------------------------
		public XmlDocument GetSolutionFilesDocument
			(
			XmlDocument physicalSolutionConfiguration,
			string storageName, 
			string storageRelease, 
			string uiCultureName,
			string maxRelease,
			out string servedRelease
			)
		{
			// Ottimizzata - delle lingue si richiede quella corrente sulla GUI.

			// TODO - controllare che storageRelease non sia superiore a maxRelease. però qui storageRelease dovrebbe arrivare sempre string.Empty
			if (storageRelease == null || storageRelease.Length == 0)
			{
				// NOTE - la condizione può capitare nel caso di setup di installazione
				//		partendo da installazione vuota. Nel caso reperisce il template
				//		dell'immagine con storageRelease più alta tra quelle disponibili
				//		per lo storageName indicato
				storageRelease = GetTopImageRelease(storageName, maxRelease);
			}
			servedRelease = storageRelease;

			// TEMP - se la sorgente dati non è raggiungibile ora storageRelease è null
			//		e si avrà un'eccezione trappata da un try-catch d'emergenza. gestire meglio!

			// gestito con una logica di deploymentmanifest in cui mando la richiesta
			// corredata da date di tag solution ed includes.
			// Si richiede solo la lingua corrente, per evitare di tirare giù tutto ogni volta.

			using (ISourceImage sourceImage = new SourceImage(this, storageName, storageRelease, false))
			{
				string includePath = Consts.DirDictionary + Path.DirectorySeparatorChar + uiCultureName;
				XmlDocument functionalSolutionConfiguration = ConfigurationManager.GetServerSolutionConfiguration(includePath);
				ConfigurationManager.DecorateSolutionsWithDates(ref functionalSolutionConfiguration, physicalSolutionConfiguration);

				XmlDocument dom = new XmlDocument();
				XmlElement root = dom.CreateElement(ConfConsts.TagFiles);
				dom.AppendChild(root);

				XmlElement solutions = functionalSolutionConfiguration.SelectSingleNode("//" + TagSolution) as XmlElement;

				// find Solutions folder relative path (sourceRelPath parameter)
				string sourceRelPath = ConfConsts.DirSolutions;
				if (!RelativeDirectory.Exists(sourceRelPath, sourceImage))
				{
					string[] relApps = RelativeDirectory.GetRelativeDirectories("Applications", sourceImage);
					foreach (string relApp in relApps)
					{
						string solRelPath = relApp + "/" + ConfConsts.DirSolutions;
						if (RelativeDirectory.Exists(solRelPath, sourceImage))
						{
							sourceRelPath = solRelPath;
							break;
						}
					}
					Debug.Assert(sourceRelPath != ConfConsts.DirSolutions);
				}

				AppendElementToSolutionsDocument
				(
					sourceRelPath,
					root,
					solutions,
					sourceImage,
					storageName,
					string.Empty
				);

				if (string.Compare(uiCultureName, NameSolverStrings.DefaultLanguage, true, CultureInfo.InvariantCulture) == 0)
					return dom;

				// aggiunge a lista elenco files uiCultureName
				XmlElement uiInclude = null;
				if (solutions != null)
					uiInclude = ProductInfo.GetIncludeElement(solutions, includePath);
				
				AppendElementToSolutionsDocument
					(
					sourceRelPath + Path.DirectorySeparatorChar + includePath,
					root,
					uiInclude,
					sourceImage,
					storageName,
					includePath + Path.DirectorySeparatorChar
					);

				return dom;
			}
		}

		//---------------------------------------------------------------------
		private void AppendElementToSolutionsDocument
			(
				string sourceRelPath,
				XmlElement root,
				XmlElement el,
				ISourceImage sourceImage,
				string productName,
				string prePath
			)
		{
			string clientManifestVersion	= string.Empty;
			string refDateString			= string.Empty;

			if (el != null)
			{
				clientManifestVersion	= el.GetAttribute(Consts.AttributeVersion);
				refDateString			= el.GetAttribute(Consts.AttributeDate);
			}

			ArrayList list = GetUpdatedManifestFilesList
				(
				sourceImage,
				clientManifestVersion,
				sourceRelPath,
				refDateString,
				productName
				);

			foreach (string relativeFilePath in list)
				AppendXmlFile
					(
					sourceImage, 
					root, 
					relativeFilePath, 
					ConfConsts.TagFile,
					sourceRelPath,
					prePath
					);
		}

		//---------------------------------------------------------------------
		private bool AppendXmlFile
			(
			ISourceImage sourceImage, 
			XmlElement root, 
			string relativeFilePath,
			string TagName,
			string sourceRelPath,
			string prePath
			)
		{
			try
			{
				string sourceFileRelPath = sourceRelPath + "/" + relativeFilePath;
				string filePathName = prePath + relativeFilePath;
				XmlElement el = root.OwnerDocument.CreateElement(TagName);
				el.SetAttribute(ConfConsts.AttributeName, filePathName);
				el.SetAttribute(ConfConsts.AttributeDate, RelativeFile.GetTruncatedLastWriteTimeUtcAsString(sourceFileRelPath, sourceImage));
				sourceImage.AppendBinaryFile(sourceFileRelPath, el);
				if (root == null)
				{
					Debug.Fail("aggiungere nodo Solution!");
				}
				root.AppendChild(el);
				return true;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return false;
			}
		}

		//---------------------------------------------------------------------
		public bool SaveSolutionFilesLocally
			(
			XmlDocument files, 
			string productName, 
			string solutionUpdatesPath
			)
		{
			if (Directory.Exists(solutionUpdatesPath))
				MyDeleteDirectory(new DirectoryInfo(solutionUpdatesPath));
			// salva i files di Solutions in standard con date corrette
			foreach (XmlElement fileElement in files.SelectNodes("//" + ConfConsts.TagFiles + "/" + ConfConsts.TagFile))
				SaveFileLocally(fileElement, solutionUpdatesPath);
			return true;
		}

		//---------------------------------------------------------------------
		private bool SaveFileLocally(XmlElement fileElement, string solutionUpdatesPath)
		{
			string name = fileElement.GetAttribute(ConfConsts.AttributeName);	
			string dateString = fileElement.GetAttribute(ConfConsts.AttributeDate);
			DateTime date = DateTime.MinValue;
			try { date = DateTime.Parse(dateString); } 
			catch {}
			string fileName = Path.Combine(solutionUpdatesPath, name);
			if (File.Exists(fileName) && ManifestManager.GetTruncatedFileDate(fileName) >= date)
				return false;
			try
			{
				FileInfo fileInfo = new FileInfo(fileName);
				//se il file esiste tolgo l'eventuale readOnly.
				if (File.Exists(fileName))
					fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.ReadOnly;
				else//se non esiste potrebbe non esistere la cartella, allora la creo.
					fileInfo.Directory.Create();
				string  cdata = fileElement.FirstChild.Value;
				XmlTextReader r = new XmlTextReader(new StringReader(cdata));
				// First position reader on <SalesModule> element and get the size attribute
				r.Read(); 
				int len = Int32.Parse(r.GetAttribute(Microarea.Library.Licence.Consts.AttributeSize));
				// Allocate a buffer of the correct length and call ReadBase64.
		
				byte[] buffer = new byte[len];
				int found = r.ReadBase64(buffer, 0, len); 
				FileStream fs = new FileStream(fileName, FileMode.Create);
				BinaryWriter sw = new BinaryWriter(fs);
				sw.Write(buffer, 0, found);
				fs.Close();
				sw.Close();
				fileInfo.LastWriteTimeUtc = date;
			}
			catch (Exception exc)
			{
				Debug.Fail("ImagesManager.SaveFileLocally Exception: " + exc.Message);
			}
			return true;
		}

		//---------------------------------------------------------------------
		protected ActivationObject GetActivationData
			(
			string storageName, 
			string storageRelease, 
			XmlDocument licensed,
			bool addFunctional, 
			UserInfo userInfo,
			string maxRelease
			)
		{
			if (storageRelease == null || storageRelease.Length == 0)
			{
				// NOTE - la condizione può capitare nel caso di setup di installazione
				//		partendo da installazione vuota. Nel caso considera l'immagine 
				//		con storageRelease più alta tra quelle disponibili
				//		per lo storageName indicato
				storageRelease = GetTopImageRelease(storageName, maxRelease);
			}

			Debug.Assert(storageRelease != null && storageRelease.Length != 0);
			string imgPath = GetImagePath(storageName, storageRelease);

			IConfigurationInfoProvider provider = new ProviderForImageManager(imgPath, storageName, userInfo, licensed);
			provider.AddFunctional = addFunctional;

			return new ActivationObject(provider);
		}

		//---------------------------------------------------------------------
		private ProductInfo GetImageLicensedProductInfo
			(
			string storageName, 
			string storageRel, 
			XmlDocument licensed,
			UserInfo userInfo,
			string maxRelease
			)
		{
			ActivationObject activationObj = GetActivationData
				(
				storageName, 
				storageRel, 
				licensed, 
				true, 
				userInfo, 
				maxRelease
				);
			return activationObj.GetProductByName(storageName);
		}

		//---------------------------------------------------------------------
		public XmlDocument GetFunctionalConfiguration
			(
			string storageName, 
			string storageRel, 
			XmlDocument licensed,
			XmlDocument physicalConfiguration,
			UserInfo userInfo,
			string maxRelease
			)
		{
			//DateTime start = DateTime.Now;
			ProductInfo commercial = 
				GetImageLicensedProductInfo(storageName, storageRel, licensed, userInfo, maxRelease);
			//TimeSpan time = DateTime.Now.Subtract(start);
			//Debug.Fail(time.ToString());
			
			return GetFunctionalConfiguration
				(
				storageName,
				storageRel,
				commercial,
				physicalConfiguration,
				true,
				true,
				true
				);
		}

		//---------------------------------------------------------------------
		public XmlDocument GetFunctionalConfiguration
			(
			string storageName, 
			string storageRel, 
			ProductInfo commercial,
			XmlDocument physicalConfiguration,
			bool addProduct,
			bool addDependencies,
			bool decorateConfigurationWithDates
			)
		{
			//DateTime start = DateTime.Now;

			string dependenciesMap = GetFunctionalDependenciesMap(storageName, storageRel);
			XmlDocument dependenciesMapDom = new XmlDocument();
			try { dependenciesMapDom.Load(dependenciesMap); }
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				// TODO - immagine corrotta, che fare?
			}
			StringCollection includePaths = null;
			XmlDocument functionalConfiguration = ConfigurationManager.GetMappedConfiguration
				(
				commercial,
				dependenciesMapDom,
				out includePaths,
				addProduct,
				addDependencies,
				true
				);
			if (decorateConfigurationWithDates)
				ConfigurationManager.DecorateConfigurationWithDates
					(
					storageName, 
					functionalConfiguration, 
					physicalConfiguration,
					includePaths
					);

			//TimeSpan time = DateTime.Now.Subtract(start);
			//Debug.Fail(time.ToString());

			return functionalConfiguration;
		}

		//---------------------------------------------------------------------
		public XmlDocument GetDependenciesOnlyConfiguration
			(
			string verticalName,
			string verticalRequiredRelease,
			XmlDocument verticalCommercialConfiguration,
			UserInfo userInfo,
			string maxRelease
			)
		{
			if (verticalRequiredRelease == null || verticalRequiredRelease.Length == 0)
			{
				// NOTE - la condizione può capitare nel caso di setup di installazione
				//		partendo da installazione vuota. Nel caso reperisce il template
				//		dell'immagine con storageRelease più alta tra quelle disponibili
				//		per lo storageName indicato
				verticalRequiredRelease = GetTopImageRelease(verticalName, maxRelease);
			}
			// oggetto con configurazione funzionale licenziata del verticale
			ProductInfo verticalProductInfo = 
				GetImageLicensedProductInfo
				(
				verticalName, 
				verticalRequiredRelease, 
				verticalCommercialConfiguration, 
				userInfo, // serve per ricavarne isoStato x localizzazione normativa
				maxRelease
				);

			//dependencies only
			XmlDocument functionalConfigurationDepsOnly = 
				GetFunctionalConfiguration
				(
				verticalName, 
				verticalRequiredRelease, 
				verticalProductInfo, 
				null, 
				false,
				true,
				false
				);

			// functional configuration of the product only without dependencies
			XmlDocument functionalConfigurationProdOnly = 
				GetFunctionalConfiguration
				(
				verticalName, 
				verticalRequiredRelease, 
				verticalProductInfo, 
				null, 
				true,
				false,
				false
				);

			// functional configuration of the product  with dependencies
			XmlDocument functionalConfigurationDepsAndProd = 
				GetFunctionalConfiguration
				(
				verticalName, 
				verticalRequiredRelease, 
				verticalProductInfo, 
				null, 
				true,
				true,
				false
				);
			//TODO Exception handling and optimazation

			//Verify that  for every application of the product applicazione the modules 
			//necesary for dependency are present in DepOnly xml otherwise it will be impossible install

			try
			{
				XmlNodeList appList = functionalConfigurationProdOnly.GetElementsByTagName("Application");
				ArrayList names = new ArrayList();
				foreach (XmlElement el in appList)
				{
					string name = el.GetAttribute("name");
					if (!names.Contains(name) && name.Length > 0)
						names.Add(name);
				}
				foreach (string n in names)
				{
					string xPath = string.Format(CultureInfo.InvariantCulture, "//Application[@name='{0}']", n);
					XmlNode appEl =  functionalConfigurationDepsAndProd.SelectSingleNode(xPath);
					if (appEl == null)
						continue;
					XmlNode app = functionalConfigurationDepsOnly.SelectSingleNode(xPath);
					if (app == null)
						continue;
					XmlNodeList modules = appEl.SelectNodes("Module");
					foreach (XmlElement e in modules)
					{
						if (e == null || e.GetAttribute("name").Length == 0)
							continue;
						if ( ((XmlElement)app).GetElementsByTagName("Module").Count > 0)
						{
							XmlNode modules1 = app.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "Module[@name='{0}']", e.GetAttribute("name")));
							if (modules1 == null)
								continue;
							app.RemoveChild(modules1);
						}
						if (((XmlElement)app).GetElementsByTagName("Module").Count == 0)
						{
							app.ParentNode.RemoveChild(app);
							break;
						}
					}
				}
			} 
			catch (Exception exc)//TODO: not good, the exception will be ignored!!!!
			{
				Debug.Fail("ImagesManager.GetDependenciesOnlyConfiguration "+ exc.Message);
			}
			return functionalConfigurationDepsOnly;
		}

		//---------------------------------------------------------------------
		public string GetImageReleaseExtended(string storageName, string storageRelease)
		{
			// NOTE - per un singolo file non accetto l'overhead di leggere tutto lo ZIPpone,
			//		accetto che ne sia fuori, passo parametro compressed = false
			using (ISourceImage sourceImage = new SourceImage(this, storageName, storageRelease, false))
			{
				return sourceImage.GetImageReleaseExtended();
			}
		}
		
		//---------------------------------------------------------------------
		public string FindImageSolutionFileFullPath(string product, string release)
		{
			string fileRelPath = FindImageSolutionFileRelPath(product, release);
			if (fileRelPath == null || fileRelPath.Length == 0)
				return null;
			string imgPath = GetImagePath(product, release);
			return Path.Combine(imgPath, fileRelPath);
		}
		private string FindImageSolutionFileRelPath(string product, string release)
		{
			if (!ImageExists(product, release))
				return null;

			// since TB2.7, Solutions folder is inside the application (TB2.7 folders structure)
			// before TB2.7 Solutiosn folder was at image root level

			string fileName = GetSolutionFileName(product);
			string relFilePath;
			using (ISourceImage sourceImage = new SourceImage(this, product, release, false))
			{
				// first, look for file in pre-TB2.7 folders structure
				string tb2_7RelSolutionsDir = FindSolutionsRelPathInApps(sourceImage);
				if (tb2_7RelSolutionsDir != null && tb2_7RelSolutionsDir.Length != 0)
				{
					relFilePath = string.Concat(tb2_7RelSolutionsDir, Path.DirectorySeparatorChar, fileName);
					if (RelativeFile.Exists(relFilePath, sourceImage))
						return relFilePath;
				}

				// then, search in old Solutions folder (pre-TB2.7)
				relFilePath = string.Concat(Consts.DirSolutions, Path.DirectorySeparatorChar, fileName);

				if (RelativeFile.Exists(relFilePath, sourceImage))
					return relFilePath;
			}
			return null; // this shouldn't happen, it would mean a bad master image
		}

		/// <summary>
		/// Ritorna la lista di tutti i SalesModules.
		/// </summary>
		/// <param name="pathInfo">Directory nella quale cercare</param>
		//---------------------------------------------------------------------
		private FileInfo[] GetAllSolutionModulesFiles(DirectoryInfo pathInfo)
		{
			FileInfo[] encrypted = pathInfo.GetFiles(NameSolverStrings.MaskFileEncryptedArticle);
			FileInfo[] decrypted = pathInfo.GetFiles(NameSolverStrings.MaskFileDecryptedArticle);
			ArrayList allList = new ArrayList();
			if (encrypted != null)
				allList.AddRange(encrypted);
			if (decrypted != null)
				allList.AddRange(decrypted);
			return (FileInfo[])allList.ToArray(typeof(FileInfo));		
		}

		/// <summary>
		/// Restituisce la mappa di dipendenze funzionali associata
		/// all'immagine sorgente
		/// </summary>
		/// <param name="storageName">storage name</param>
		/// <param name="storageRelease">storage release</param>
		/// <returns>file della mappa richiesta</returns>
		//---------------------------------------------------------------------
		private string GetFunctionalDependenciesMap(string storageName, string storageRelease)
		{
			// TEMP - implementazione brutale - reperisco la mappa non criptata, in chiaro
			if (!ImageExists(storageName, storageRelease))
				return null;

			string tb2_7RelsolutionsDir;
			using (ISourceImage sourceImage = new SourceImage(this, storageName, storageRelease, false))
			{
				// search for the Solutions folder inside the application (TB2.7 folders structure)
				tb2_7RelsolutionsDir = FindSolutionsRelPathInApps(sourceImage);
			}
			string dirPath = GetImagePath(storageName, storageRelease); // pre-TB2.7 folders structure
			if (tb2_7RelsolutionsDir != null)
				dirPath = Path.Combine(dirPath, tb2_7RelsolutionsDir); // TB2.7 folders structure

			// BACKWARDCOMPATIBILITY - dalla 1.1.0.20040429 il file DependancyMap è prefissato con
			//		il nome del prodotto.
			//		si garantisce compatibilità con la 1.0, non con le build successive e mai pubblicate,
			//		precedenti alla build indicata
			string fileName;
			if (string.Compare(storageName, "MagoNet-Pro", true, CultureInfo.InvariantCulture) == 0 &&
				string.Compare(storageRelease, "1.0", true, CultureInfo.InvariantCulture) == 0)
				fileName = Consts.FileDependenciesMap;
			else
				fileName = storageName + "." + Consts.FileDependenciesMap;
			return Path.Combine(dirPath, fileName);
		}

		//---------------------------------------------------------------------
		public string GetSourceFrameworkVersion(string product, string release)
		{
			if (!ImageExists(product, release))
				return null;
			using (ISourceImage sourceImage = new SourceImage(this, product, release, true))
			{
				string relPath = "TaskBuilder/Framework/Application.config";
				XmlDocument dom = sourceImage.GetFileDom(relPath);
				if (dom == null)
					return null;
				// read version
				string xPath = "/ApplicationInfo/Version";
				XmlElement el = dom.SelectSingleNode(xPath) as XmlElement;
				if (el == null)
					return null;
				return el.InnerText;
			}
		}


		#region Controlli di dipendenza e compatibilità per verticali

		//---------------------------------------------------------------------
		public static string GetCompatibilityListFileName(string product)
		{
			return product + ConfConsts.ExtensionCompatibilityList;
		}

		//---------------------------------------------------------------------
		private string FindCompatibilityListRelFile(string storageName, string storageRelease)
		{
			if (!ImageExists(storageName, storageRelease))
				return null;

			// since TB2.7, compatibility list is inside the Solutions folder inside  the application (TB2.7 folders structure)
			// before TB2.7 compatibility list was inside the image root folder

			string compFileName = BasePathFinder.GetCompatibilityListFileName(storageName);
			using (ISourceImage sourceImage = new SourceImage(this, storageName, storageRelease, false))
			{
				// first, search at root level (preTB2.7)
				if (RelativeFile.Exists(compFileName, sourceImage))
					return compFileName;
				// then look for file in pre-TB2.7 folders structure
				string tb2_7RelSolutionsDir = FindSolutionsRelPathInApps(sourceImage);
				if (tb2_7RelSolutionsDir == null || tb2_7RelSolutionsDir.Length == 0)
					return null; // this should be the case of a pre-TB2.7 master product
				string relFilePath = string.Concat(tb2_7RelSolutionsDir, Path.DirectorySeparatorChar, compFileName);
				if (RelativeFile.Exists(relFilePath, sourceImage))
					return relFilePath;
			}
			return null; // this should be the case of a TB2.7 master product
		}

		//---------------------------------------------------------------------
		public bool IsVerticalProduct(string product, string release, out XmlDocument depDom)
		{
			depDom = null;
			// an AddOn product image containd the file <Product>.CompatibilityList.config, a master product does not.
			string compFileRelPath = FindCompatibilityListRelFile(product, release);
			bool isAddOn = compFileRelPath != null;
			if (isAddOn)
				// NOTE - per un singolo file non accetto l'overhead di leggere tutto lo ZIPpone,
				//		accetto che ne sia fuori, passo parametro compressed = false
				using (ISourceImage sourceImage = new SourceImage(this, product, release, false))
					depDom = sourceImage.GetFileDom(compFileRelPath);
			return isAddOn;
		}

		/*
		//---------------------------------------------------------------------
		public XmlDocument GetDependenciesMap(string product, string release)
		{
			// se nella root dell'immagine di deployment esiste file <Product>.MasterProduct.config,
			// allora il prodotto che si cerca di installare/aggiornare è un verticale
			XmlDocument dependenciesMap = null;
			string fileName = BasePathFinder.GetDependenciesMapFileName(product);

			// NOTE - per un singolo file non accetto l'overhead di leggere tutto lo ZIPpone,
			//		accetto che ne sia fuori, passo parametro compressed = false
			using (SourceImage sourceImage = new SourceImage(this, product, release, false))
			{
				if (RelativeFile.Exists(fileName, sourceImage))
					dependenciesMap = sourceImage.GetFileDom(fileName);
			}

			return dependenciesMap;
		}
		*/

		#endregion
	}

	//=========================================================================
	public class HistoryPatch
	{
		private	readonly	string		historyPatchRelPath;
		private readonly	ISourceImage sourceImage;
		private	readonly	bool		services;

		//---------------------------------------------------------------------
		public HistoryPatch(string historyPatchRelPath, ISourceImage sourceImage, bool services)
		{
			this.historyPatchRelPath	= historyPatchRelPath;
			this.sourceImage			= sourceImage;
			this.services				= services;
		}

		//---------------------------------------------------------------------
		public void Copy(TargetImage targetImage, ImagesManager imagesManager)
		{
			RelativeFile.CopyWithPath(historyPatchRelPath, targetImage, sourceImage, imagesManager, services);
		}

		//---------------------------------------------------------------------
		public long Size
		{
			get
			{
				return RelativeFile.GetFileSize(historyPatchRelPath, sourceImage);
			}
		}
	}

	//=========================================================================
	public class VersionedSourceImage : IComparable
	{
		public readonly ImageVersion	version;
		public readonly ISourceImage	sourceImage;
		public VersionedSourceImage(ImageVersion version, ISourceImage sourceImage)
		{
			this.version		= version;
			this.sourceImage	= sourceImage;
		}

		#region IComparable Members
		public int CompareTo(object obj)
		{
			VersionedSourceImage vsi = obj as VersionedSourceImage;
			if (vsi == null)
				return 1;
			return this.version.CompareTo(vsi.version);
		}
		#endregion
	}

	//============================================================================
	public interface IProductsRepository
	{
		string[] GetProductImagesRels(string product);
		bool ImageExists(string product, string release);
		string GetPreviousRelease(string product, string release);
	}

	//============================================================================
	public class ProductsRepository : IProductsRepository
	{
		private readonly string universalImagesPath;
		
		//---------------------------------------------------------------------
		public ProductsRepository(string universalImagesPath)
		{
			this.universalImagesPath = universalImagesPath;
		}
		
		#region IProductsRepository Members
		//---------------------------------------------------------------------
		public string[] GetProductImagesRels(string product)
		{
			string productImagesPath = Path.Combine(universalImagesPath, product);
			if (!Directory.Exists(productImagesPath))
				return new string[] {};

			ArrayList list = new ArrayList();
			DirectoryInfo di = new DirectoryInfo(productImagesPath);
			foreach (DirectoryInfo sdi in di.GetDirectories())
				list.Add(sdi.Name);
			return (string[])list.ToArray(typeof(string));
		}
		
		//---------------------------------------------------------------------
		public bool ImageExists(string product, string release)
		{
			string productImagesPath = Path.Combine(universalImagesPath, product);
			return Directory.Exists(Path.Combine(productImagesPath, release));
		}
		
		//---------------------------------------------------------------------
		public string GetPreviousRelease(string product, string release)
		{
			// Note: string.Empty (or null) means that the previous found minor in M.m.SP 
			// was the previous at MasterMaker time
			
			// Retrieval of prevRelase attribute in main manifest file
			string productImagesPath = Path.Combine(universalImagesPath, product);
			if (!Directory.Exists(productImagesPath))
				throw new DirectoryNotFoundException("Cannot find product image directory: " + productImagesPath);
			string imageReleasePath = Path.Combine(productImagesPath, release);
			if (!Directory.Exists(imageReleasePath))
				throw new DirectoryNotFoundException("Cannot find image release directory: "+ imageReleasePath);
			string mainManifestName = ManifestManager.BuildDirectoryManifestFileName(product, PolicyType.Full);
			string mainManifestPath = Path.Combine(imageReleasePath, mainManifestName);
			if (!File.Exists(mainManifestPath))
				throw new FileNotFoundException("Cannot find main image manifest file.", mainManifestPath);
			ManifestObject mo = ManifestManager.GetManifestObject(mainManifestPath, false); // do not include files
			return mo.Head.PrevRelease;
		}
		#endregion
	}

	//============================================================================
}
