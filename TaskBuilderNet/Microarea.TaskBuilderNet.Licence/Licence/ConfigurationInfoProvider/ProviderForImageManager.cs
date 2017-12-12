

namespace Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider
{
	/*//=========================================================================
	public class ProviderForImageManager : IConfigurationInfoProvider
	{
		private bool addFunctional;
		protected bool getInUpdatedFolder = false;
		protected string imagePath;
		protected string product2Update;
		private Hashtable licenseds;
		private Hashtable solutions;

		private string solutionsPath;
		private string modulesPath;

		private UserInfo userInfo;
		private XmlDocument licensed; // a dom grouping all the licended.config found on the client installation

		//---------------------------------------------------------------------
		public ProviderForImageManager
			(
			string imagePath, 
			string product2Update, 
			UserInfo userInfo,
			XmlDocument licensed // a dom grouping all the licended.config found on the client installation
			)
		{
			this.imagePath = imagePath;
			this.product2Update = product2Update;
			this.userInfo = userInfo;
			this.licensed = licensed;

			this.solutionsPath = Path.Combine(this.imagePath, Consts.DirSolutions);
			this.modulesPath = Path.Combine(solutionsPath, Consts.DirSolutionModules);

			this.solutions = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		}

		/// <summary>
		/// Legge il file dell'articolo.
		/// </summary>
		//---------------------------------------------------------------------
		private XmlDocument GetArticleDocument(string article)
		{
			string fileName = string.Concat(article, NameSolverStrings.CsmExtension);
			fileName = Path.Combine(modulesPath, fileName);
			if (!File.Exists(fileName))
				return null; // source image can only use cryptes files

			return ProvidersHelper.LoadArticleFromCsm(fileName);
		}

		#region IConfigurationInfoProvider Members

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.AddFunctional
		{
			get { return this.addFunctional; }
			set { this.addFunctional = value; }
		}

		//---------------------------------------------------------------------
		string[] IConfigurationInfoProvider.GetProductNames()
		{
			if (this.licenseds == null)
				GetLicensedProducts(); // builds the hashtable

			ArrayList names = new ArrayList();
			foreach (string name in this.licenseds.Keys)
				names.Add(name);
			if (!names.Contains(this.product2Update))
				names.Add(this.product2Update);
			names.Sort();

			return (string[])names.ToArray(typeof(string));
		}

		/// <summary>
		/// Gets the installed products list based on the *.Licensed.config found
		/// in the local Solutions folder
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private Hashtable GetLicensedProducts() // returns a collection of XmlElements
		{
			if (this.licenseds != null)
				return this.licenseds;

			Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

			foreach (XmlElement licEl in this.licensed.DocumentElement.GetElementsByTagName("Product"))
			{
				string prodName = licEl.GetAttribute("name");//"Product[@name='{0}']";
				ht[prodName] = licEl;
			}
			this.licenseds = ht;
			return this.licenseds;
		}

		//---------------------------------------------------------------------
		XmlElement IConfigurationInfoProvider.GetProductLicensed(string product)
		{
			if (this.licenseds == null)
				GetLicensedProducts(); // builds the hashtable
			return licenseds[product] as XmlElement;
		}

		//---------------------------------------------------------------------
		Hashtable IConfigurationInfoProvider.GetArticles(string product)
		{
			if (this.licenseds == null)
				GetLicensedProducts(); // builds the hashtable

			Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

			XmlElement solEl = ((IConfigurationInfoProvider)this).GetProductSolution(product);
			if (solEl == null) // try using the licensed.config
				solEl = licenseds[product] as XmlElement;
			if (solEl == null)
				return ht;

			if (string.Compare(product, this.product2Update, true, CultureInfo.InvariantCulture) == 0)
				foreach (XmlElement artEl in solEl.GetElementsByTagName(WceStrings.Element.SalesModule))
				{
					string artName = artEl.GetAttribute(WceStrings.Attribute.Name);
					XmlDocument artDoc = GetArticleDocument(artName);
					if (artDoc == null)
						continue;
					ht[artName] = artDoc;
				}
			else
				foreach (XmlElement artEl in solEl.GetElementsByTagName(WceStrings.Element.SalesModule))
				{
					string artName = artEl.GetAttribute(WceStrings.Attribute.Name);
					XmlDocument artDoc = new XmlDocument();
					artDoc.AppendChild(artDoc.ImportNode(artEl, true));
					ht[artName] = artDoc;
				}

			return ht;
		}

		//---------------------------------------------------------------------
		string IConfigurationInfoProvider.GetCountry()
		{
			return this.userInfo.Country;
		}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.ArticlesLicensedByDefault { get { return true; } }

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.Xml.XmlException"></exception>
		/// <param name="product"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		XmlElement IConfigurationInfoProvider.GetProductSolution(string product)
		{
			if (this.solutions.Contains(product))
				return solutions[product] as XmlElement;

			if (string.Compare(product, this.product2Update, true, CultureInfo.InvariantCulture) == 0)
			{
				string solName = string.Concat(product, NameSolverStrings.SolutionExtension);
				string solFilePath = Path.Combine(solutionsPath, solName); // pre-TB2.7 folders structure
				if (!File.Exists(solFilePath))
				{
					// search for the Solutions folder inside the application (TB2.7 folders structure)
					string appsPath = Path.Combine(this.imagePath, Consts.DirApplications);
					string excMsg = "Solution folder not found in deployment image."; // interned anyway as literal
					if (!Directory.Exists(appsPath))
						throw new DirectoryNotFoundException(excMsg);
					bool foundInApps = false;
					foreach (string app in Directory.GetDirectories(appsPath))
					{
						string aSolInAppsDirPath = Path.Combine(app, Consts.DirSolutions);
						string aSolInAppsFilePath = Path.Combine(aSolInAppsDirPath, solName);
						if (File.Exists(aSolInAppsFilePath))
						{
							solFilePath = aSolInAppsFilePath;
							this.modulesPath = Path.Combine(aSolInAppsDirPath, Consts.DirSolutionModules);
							foundInApps = true;
							break;
						}
					}
					if (!foundInApps)
						throw new DirectoryNotFoundException(excMsg);
				}
				XmlDocument doc = new XmlDocument();
				doc.Load(solFilePath);
				solutions[product] = doc.DocumentElement;
				return doc.DocumentElement;
			}

			// if the product is not the one to update, uses the received related licensed instead
			// to enumerate its articles
			if (this.licenseds == null)
				GetLicensedProducts(); // builds the hashtable
			return licenseds[product] as XmlElement;
		}

		//---------------------------------------------------------------------
		UserInfo IConfigurationInfoProvider.GetUserInfo()
		{
			return this.userInfo;
		}

		//---------------------------------------------------------------------
		void IConfigurationInfoProvider.InvalidateCaches()
		{
			this.licenseds = null;
			this.solutions.Clear();
		}

		//---------------------------------------------------------------------
		IPathFinder IConfigurationInfoProvider.GetPathFinder()
		{
			return null; // not needed in this provider
		}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.FilterByCountry { get { return true; } }

		//---------------------------------------------------------------------
		string IConfigurationInfoProvider.GetProductRelease(string product)
		{
			return new DirectoryInfo(this.imagePath).Name; // folder name represents the release number
		}

		#endregion
	}*/
}
