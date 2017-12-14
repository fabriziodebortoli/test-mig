using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	#region Class BrandLoader
	//============================================================================
	public class BrandLoader : IBrandLoader
    {
        #region Xml Tags
        public const string XML_TAG_BRANDS_ROOT			= "Brands";
		public const string XML_TAG_BRAND				= "Brand";
		public const string XML_TAG_COMPANY				= "Company";
        public const string XML_TAG_COMPANY_MENU_LOGO   = "CompanyMenuLogo";
		public const string XML_TAG_PRODUCT_TITLE		= "ProductTitle";
		public const string XML_TAG_MENUMANAGER_SPLASH	= "MenuManagerSplash";
		public const string XML_TAG_CONSOLE_SPLASH		= "ConsoleSplash";
		public const string XML_TAG_APPLICATIONS		= "Applications";
		public const string XML_TAG_APPLICATION			= "Application";
		public const string XML_TAG_MENUTITLE			= "MenuTitle";
		public const string XML_TAG_MENUIMAGE			= "MenuImage";
		public const string XML_TAG_BRANDEDKEYS			= "BrandedKeys";
		public const string XML_TAG_BRANDEDKEY			= "BrandedKey";
		public const string XML_ATTRIBUTE_SOURCE		= "source";
		public const string XML_ATTRIBUTE_BRANDED		= "branded";
		public const string XML_ATTRIBUTE_NAME			= "name";
		public const string XML_ATTRIBUTE_ISO			= "ISO";
		#endregion

        ArrayList brands = new ArrayList();
		private Icon brandedTbAppManagerApplicationIcon = null;
		private Icon brandedConsoleApplicationIcon = null;

        //-----------------------------------------------------------------------------
        public BrandLoader()
        {
            brands = new ArrayList();
            LoadFromFiles();
        }

        //-----------------------------------------------------------------------------
        public static void PreLoadMasterSolutionName()
        {
            LoginManager loginManager = new LoginManager();

            InstallationData.ServerConnectionInfo.MasterSolutionName = loginManager.GetMasterSolution();
            InstallationData.ServerConnectionInfo.UnParse(BasePathFinder.BasePathFinderInstance.ServerConnectionFile);
        }

        #region Load From files
        //-----------------------------------------------------------------------------
        private bool LoadFromFiles()
		{
			FileInfo[] filesToLoad = BasePathFinder.BasePathFinderInstance.GetBrandFiles();
			if (filesToLoad == null || filesToLoad.Length == 0)
				return false;
			
			foreach(FileInfo aFileToLoad in filesToLoad)
				LoadFromFile(aFileToLoad);
			
			return true;
		}
		
		//-----------------------------------------------------------------------------
		private bool LoadFromFile(FileInfo aFileToLoad)
		{
			if (aFileToLoad == null || !aFileToLoad.Exists)
				return false;

			// Le informazioni contenute nel file di brand principale "vincono"
			// sempre su quelle contenute in eventuali file di brand aggiuntivi
            bool isMainBrand = aFileToLoad.Name.CompareNoCase(InstallationData.ServerConnectionInfo.MasterSolutionName + NameSolverStrings.BrandExtension);

            XmlDocument brandXmlDocument = new XmlDocument();

			try
			{
				brandXmlDocument.Load(aFileToLoad.FullName);
				if 
					(
						brandXmlDocument.DocumentElement == null || 
						!brandXmlDocument.DocumentElement.HasChildNodes ||
						String.Compare(brandXmlDocument.DocumentElement.Name, BrandLoader.XML_TAG_BRANDS_ROOT) != 0
					)
					return true; // file vuoto o radice non compatibile

				XmlNodeList brandsNodes = brandXmlDocument.DocumentElement.SelectNodes("child::" + BrandLoader.XML_TAG_BRAND);

				if (brandsNodes == null || brandsNodes.Count == 0)
					return true;

				foreach (XmlNode aBrandNode in brandsNodes)
				{
					if (!(aBrandNode is XmlElement))
						continue;

					//string brandName = ((XmlElement)aBrandNode).GetAttribute(BrandLoader.XML_ATTRIBUTE_NAME);
                   // IBrandInfo brandInfo = GetBrandInfo(brandName);

                    //if (brandInfo == null)
                   // {
                        IBrandInfo brandInfo = new BrandInfo(aBrandNode, isMainBrand);
                       // if (!brandInfo.InfoLoaded)
                        //    continue;

                      
                        brands.Add(brandInfo);
                    //}
                    //else
                     //   brandInfo.AddInfoFromXmlNode(aBrandNode, isMainBrand);


				}
			}
			catch (Exception exception)
			{
				Debug.Fail("Error in BrandLoader.LoadFromFile: " + exception.Message);
				return false;
			}
			
			return true;
		}
		#endregion

		#region Public methods
	
		//-----------------------------------------------------------------------------
		public string FindBrandedStringValue(string propertyName)
		{
            IBrandInfo mainBrandInfo = GetMainBrandInfo();
			if (mainBrandInfo != null)
			{
				object propertyValue = mainBrandInfo.GetPropertyValue(propertyName);
				if (propertyValue != null && (propertyValue is string) && ((string)propertyValue).Length > 0)
					return (string)propertyValue;
			}
			
			return null;
		}

        //-----------------------------------------------------------------------------
        public BrandInfo GetMainBrandInfo()
        {
            foreach (BrandInfo aBrandInfo in brands)
            {
                if (aBrandInfo.IsMain)
                    return aBrandInfo;
            }
            return null;
        }

		//-----------------------------------------------------------------------------
		public string GetBrandedStringBySourceString(string source, bool allowNullWhenNotFound = false)
		{
			IList brandedKeysInfo = GetBrandedKeysInfo();
			if (brandedKeysInfo == null)
				return allowNullWhenNotFound ? null : source;
			foreach (BrandedKeyInfo bki in brandedKeysInfo)
			{
				if (String.Compare(bki.Source, source, true, CultureInfo.InvariantCulture) == 0)
					return bki.Branded;
			}
			return allowNullWhenNotFound ? null : source;
		}

		//-----------------------------------------------------------------------------
		public string GetApplicationBrandMenuTitle(string aApplicationName)
		{
			IBrandInfo mainBrandInfo = GetMainBrandInfo();
            if (mainBrandInfo != null)
            {
                IApplicationBrandInfo appBrandInfo = mainBrandInfo.GetApplicationBrandInfo(aApplicationName);
                if (appBrandInfo != null)
                    return appBrandInfo.MenuTitle;
            }

            foreach (IBrandInfo info in brands)
            {
                IApplicationBrandInfo abi = info.GetApplicationBrandInfo(aApplicationName);

                if (abi != null && String.Compare(abi.Name, aApplicationName) == 0)
                    return abi.MenuTitle;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        public string GetApplicationBrandMenuImage(string aApplicationName)
        {
            IBrandInfo mainBrandInfo = GetMainBrandInfo();
            if (mainBrandInfo != null)
            {
                IApplicationBrandInfo appBrandInfo = mainBrandInfo.GetApplicationBrandInfo(aApplicationName);
                if (appBrandInfo != null)
                    return appBrandInfo.MenuImage;
            }

            foreach (IBrandInfo info in brands)
            {
                IApplicationBrandInfo abi = info.GetApplicationBrandInfo(aApplicationName);

                if (abi != null && String.Compare(abi.Name, aApplicationName) == 0)
                    return abi.MenuImage;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        public IList GetBrandedKeysInfo()
		{
			IBrandInfo mainBrandInfo = GetMainBrandInfo();
			if (mainBrandInfo != null)
			{
				IList brandedKeysInfo = mainBrandInfo.GetBrandedKeysInfo();
				if (brandedKeysInfo != null)
					return brandedKeysInfo;
			}
			return new ArrayList();
		}

        //-----------------------------------------------------------------------------
        public string GetCompanyName()
        {
            return GetBrandedStringBySourceString("Company");
        }


        //-----------------------------------------------------------------------------
        public Image GetMenuManagerSplash()
        {
            string ns = GetBrandedStringBySourceString("MenuManagerSplash");
            return GetImageFromNamespace(ns);
        }

        //-----------------------------------------------------------------------------
        public Image GetConsoleSplash()
        {
            string ns = GetBrandedStringBySourceString("ConsoleSplash");
            return GetImageFromNamespace(ns);
        }
        //------------------------------------------------------------------------------
        public string GetMenuPage()
        {
            return GetBrandedStringBySourceString("MenuPage");
        }

        //------------------------------------------------------------------------------
        public Image GetCompanyLogo()
		{
            string ns = InstallationData.BrandLoader.GetBrandedStringBySourceString("CompanyLogo");
            return GetImageFromNamespace(ns);
		}

        //------------------------------------------------------------------------------
        private Image GetImageFromNamespace(string ns)
        {
            string image;
            try
            {
                image = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(ns));
            }
            catch (Exception)
            {
                return null;
            }

            if (image.IsNullOrWhiteSpace())
                return null;

            return ImagesHelper.LoadImageWithoutLockFile(image);
        }


        //--------------------------------------------------------------------------------
        public Icon GetTbAppManagerApplicationIcon()
		{
			try
			{
				if (brandedTbAppManagerApplicationIcon == null)
				{
					string appIconNS = GetBrandedStringBySourceString("TbAppManagerApplicationIcon");
                    if (!String.IsNullOrWhiteSpace(appIconNS) && (String.Compare(appIconNS, "TbAppManagerApplicationIcon", true) != 0))
					{
						string appIconPath = PathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(appIconNS));
						if (File.Exists(appIconPath))
							brandedTbAppManagerApplicationIcon = new Icon(appIconPath);

						if (brandedTbAppManagerApplicationIcon == null)
							return null;//errore
					}
				}
			}
			catch
			{
				return null;//errore
			}

			return brandedTbAppManagerApplicationIcon;

		}

		//--------------------------------------------------------------------------------
		public Icon GetConsoleApplicationIcon()
		{
			try
			{
				if (brandedConsoleApplicationIcon == null)
				{
					string appIconNS = GetBrandedStringBySourceString("ConsoleApplicationIcon");
					if (!String.IsNullOrWhiteSpace(appIconNS))
					{
						string appIconPath = PathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(appIconNS));
						if (File.Exists(appIconPath))
							brandedConsoleApplicationIcon = new Icon(appIconPath);

						if (brandedConsoleApplicationIcon == null)
							return null;//errore
					}
				}
			}
			catch
			{
				return null;//errore
			}

			return brandedConsoleApplicationIcon;

		}


		#endregion 
	}
	#endregion

	#region Class BrandInfo
	//============================================================================
	public class BrandInfo : IBrandInfo
    {
		#region Xml sample
		//<Brand name="OfficePass" ISO="JP,CN">
		//	<Applications>
		//		<Application name="MagoNet">
		//			<MenuTitle>OfficePass</MenuTitle>
		//			<MenuImage>OfficePass.bmp</MenuImage>
		//		</Application>
		//	</Applications>
        //  <BrandedKeys>
        //      <BrandedKey source = "Company" branded="Microarea S.p.A." />
        // 	</BrandedKeys>
		//</Brand>
		#endregion 

		#region Private members
		private bool		            isMain				= false;
		private string		            company				= String.Empty;
        private string  	            companyMenuLogo     = String.Empty;
		private string  	            productTitle        = String.Empty;
		private string		            menuManagerSplash	= String.Empty;
		private string		            consoleSplash		= String.Empty;
		private List<ApplicationBrandInfo> applicationInfos	= null;
		private List<BrandedKeyInfo>	brandedKeysInfo		= null;
		#endregion 

		#region Properties
		//-----------------------------------------------------------------------------
		public bool IsMain { get { return isMain; } }

		//-----------------------------------------------------------------------------
		public string Company { get { return company; } }

        //-----------------------------------------------------------------------------
        public string CompanyMenuLogo { get { return companyMenuLogo; } }
        
        //-----------------------------------------------------------------------------
		public string ProductTitle { get { return productTitle; } }

		//-----------------------------------------------------------------------------
		public string MenuManagerSplash { get { return menuManagerSplash; } }

		//-----------------------------------------------------------------------------
		public string ConsoleSplash { get { return consoleSplash; } }

		#endregion 

		#region Constructors
		//-----------------------------------------------------------------------------
		public BrandInfo(XmlNode aBrandXmlNode, bool isMainBrand)
		{
			if 
				(
				aBrandXmlNode == null || 
				!(aBrandXmlNode is XmlElement) ||
				String.Compare(aBrandXmlNode.Name, BrandLoader.XML_TAG_BRAND) != 0
				)
			{
				Debug.Fail("Error in BrandInfo constructor: invalid xml node.");
				return;
			}

            AddInfoFromXmlNode(aBrandXmlNode, isMainBrand);
		}
		#endregion

		#region Public methods

		//-----------------------------------------------------------------------------
		public IApplicationBrandInfo GetApplicationBrandInfo(string aApplicationName)
		{
			if (applicationInfos == null || applicationInfos.Count == 0 || aApplicationName == null || aApplicationName.Length == 0)
				return null;

			foreach(IApplicationBrandInfo info in applicationInfos)
			{
				if (String.Compare(info.Name, aApplicationName, true, CultureInfo.InvariantCulture) == 0)
				    return info;
			}
			return null;
		}
	
		
		//-----------------------------------------------------------------------------
		public IList GetBrandedKeysInfo ()
		{
			if (brandedKeysInfo == null)// || applicationInfos == null || applicationInfos.Count == 0)
				return null;

			foreach (BrandedKeyInfo info in brandedKeysInfo)
			{
				return brandedKeysInfo;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool AddInfoFromXmlNode(XmlNode aBrandXmlNode, bool overwrite)
		{
			if 
				(
				aBrandXmlNode == null || 
				!(aBrandXmlNode is XmlElement) ||
				String.Compare(aBrandXmlNode.Name, BrandLoader.XML_TAG_BRAND) != 0
				)
			{
				Debug.Fail("Error in BrandInfo.AddInfoFromXmlNode: invalid xml node.");
				return false;
			}

			isMain = overwrite;
		

			XmlNodeList applicationsNodes = aBrandXmlNode.SelectNodes("child::" + BrandLoader.XML_TAG_APPLICATIONS);

			if (applicationsNodes != null && applicationsNodes.Count > 0)
			{
				foreach (XmlNode applicationsNode in  applicationsNodes)
				{
					XmlNodeList applicationInfoNodes = applicationsNode.SelectNodes("child::" + BrandLoader.XML_TAG_APPLICATION);
					if (applicationInfoNodes == null || applicationInfoNodes.Count <= 0)
						continue;

					foreach (XmlNode aAppInfoNode in  applicationInfoNodes)
					{
						if (!(aAppInfoNode is XmlElement))
							continue;

						string applicationName = ((XmlElement)aAppInfoNode).GetAttribute(BrandLoader.XML_ATTRIBUTE_NAME);

						IApplicationBrandInfo appBrandInfo = GetApplicationBrandInfo(applicationName);

						if (appBrandInfo == null)
						{
							if (applicationInfos == null)
								applicationInfos = new List<ApplicationBrandInfo>();

							applicationInfos.Add(new ApplicationBrandInfo(aAppInfoNode));
						}
						else if (overwrite)
							appBrandInfo.SetInfoFromXmlNode(aAppInfoNode);
					}
				}
			}

			XmlNode brandedKeysNode = aBrandXmlNode.SelectSingleNode("child::" + BrandLoader.XML_TAG_BRANDEDKEYS);
			if (brandedKeysNode != null)
			{
				XmlNodeList brandedKeyNodes = brandedKeysNode.SelectNodes("child::" + BrandLoader.XML_TAG_BRANDEDKEY);
				if (brandedKeyNodes != null && brandedKeyNodes.Count > 0)
				{
					foreach (XmlNode aKeyInfoNode in  brandedKeyNodes)
					{
						if (!(aKeyInfoNode is XmlElement))
							continue;

						//vado sempre in append
						if (brandedKeysInfo == null)
							brandedKeysInfo = new List<BrandedKeyInfo>();

						brandedKeysInfo.Add(new BrandedKeyInfo(aKeyInfoNode));
					}
				}				
			}
            if (brandedKeysInfo == null) return true;
            BrandedKeyInfo bki = brandedKeysInfo.Find((current) => { return current.Source.CompareNoCase(BrandLoader.XML_TAG_COMPANY); });
            if (bki != null) company = bki.Branded;

            bki = brandedKeysInfo.Find((current) => { return current.Source.CompareNoCase(BrandLoader.XML_TAG_COMPANY_MENU_LOGO); });
            if (bki != null) companyMenuLogo = bki.Branded;

            bki = brandedKeysInfo.Find((current) => { return current.Source.CompareNoCase(BrandLoader.XML_TAG_PRODUCT_TITLE); });
            if (bki != null) productTitle = bki.Branded;

            bki = brandedKeysInfo.Find((current) => { return current.Source.CompareNoCase(BrandLoader.XML_TAG_MENUMANAGER_SPLASH); });
            if (bki != null) menuManagerSplash = bki.Branded;

            bki = brandedKeysInfo.Find((current) => { return current.Source.CompareNoCase(BrandLoader.XML_TAG_CONSOLE_SPLASH); });
            if (bki != null) consoleSplash = bki.Branded;

            return true;
		}

		//-----------------------------------------------------------------------------
		public object GetPropertyValue(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
				return null;

			PropertyInfo property = this.GetType().GetProperty(propertyName);
			if (property == null)
				return null;

			MethodInfo getMethod = property.GetGetMethod();
			if (getMethod == null)
				return null;

			return (object)getMethod.Invoke(this, null);
		}

		#endregion
	}
	#endregion 

	#region Class ApplicationBrandInfo
	//============================================================================
	public class ApplicationBrandInfo : IApplicationBrandInfo
	{
		#region Xml sample
		//	<Application name="MagoNet">
		//		<MenuTitle>OfficePass</MenuTitle>
		//		<MenuImage>OfficePass.bmp</MenuImage>
		//	</Application>
		#endregion

		#region Private members
		private string name = String.Empty;
		private string menuTitle = String.Empty;
		private string menuImage = String.Empty;
		#endregion 

		#region Properties
		//-----------------------------------------------------------------------------
		public string Name { get { return name; } }
		
		//-----------------------------------------------------------------------------
		public string MenuTitle { get { return menuTitle; } }
		
		//-----------------------------------------------------------------------------
		public string MenuImage { get { return menuImage; } }
		#endregion

		#region Constructors
		//-----------------------------------------------------------------------------
		public ApplicationBrandInfo(XmlNode aAppInfoXmlNode)
		{
			if 
				(
				aAppInfoXmlNode == null || 
				!(aAppInfoXmlNode is XmlElement) ||
				String.Compare(aAppInfoXmlNode.Name, BrandLoader.XML_TAG_APPLICATION) != 0
				)
			{
				Debug.Fail("Error in ApplicationBrandInfo constructor: invalid xml node.");
				return;
			}

			name = ((XmlElement)aAppInfoXmlNode).GetAttribute(BrandLoader.XML_ATTRIBUTE_NAME);


			SetInfoFromXmlNode(aAppInfoXmlNode);
		}
		#endregion

		#region Public methods
		//-----------------------------------------------------------------------------
		public void SetInfoFromXmlNode(XmlNode aAppBrandInfoXmlNode)
		{
			if 
				(
				aAppBrandInfoXmlNode == null || 
				!(aAppBrandInfoXmlNode is XmlElement) ||
				String.Compare(aAppBrandInfoXmlNode.Name, BrandLoader.XML_TAG_APPLICATION) != 0
				)
			{
				Debug.Fail("Error in ApplicationBrandInfo.SetInfoFromXmlNode: invalid xml node.");
				return;
			}

			XmlNode menuTitleNode = aAppBrandInfoXmlNode.SelectSingleNode("child::" + BrandLoader.XML_TAG_MENUTITLE);
			if (menuTitleNode != null)
				menuTitle = menuTitleNode.InnerText;

			XmlNode menuImageNode = aAppBrandInfoXmlNode.SelectSingleNode("child::" + BrandLoader.XML_TAG_MENUIMAGE);
			if (menuImageNode != null)
				menuImage = menuImageNode.InnerText;
		}
		#endregion
	}
	#endregion 

	#region Class BrandedKeyInfo
	//=========================================================================
	public class BrandedKeyInfo
	{
		#region Xml Sample
		//<BrandedKey source="Administrative Console on client"	branded="Administrative Console on client" />
		#endregion

		#region Private members
		private string source	= String.Empty;
		private string branded	= String.Empty;
		#endregion 

		#region Properties
		public string Source { get { return source; } }
		public string Branded { get { return branded; } } 
		#endregion

		#region Constructors
		//-----------------------------------------------------------------------------
		public BrandedKeyInfo(XmlNode aKeyInfoXmlNode)
		{
			if 
				(
				aKeyInfoXmlNode == null || 
				!(aKeyInfoXmlNode is XmlElement) ||
				String.Compare(aKeyInfoXmlNode.Name, BrandLoader.XML_TAG_BRANDEDKEY) != 0
				)
			{
				Debug.Fail("Error in BrandedKeynfo constructor: invalid xml node.");
				return;
			}


			SetInfoFromXmlNode(aKeyInfoXmlNode);
		}
		#endregion 

		#region Public Members
		
		//-----------------------------------------------------------------------------
		public void SetInfoFromXmlNode(XmlNode aKeyBrandInfoXmlNode)
		{
			if 
				(
				aKeyBrandInfoXmlNode == null || 
				!(aKeyBrandInfoXmlNode is XmlElement) ||
				String.Compare(aKeyBrandInfoXmlNode.Name, BrandLoader.XML_TAG_BRANDEDKEY) != 0
				)
			{
				Debug.Fail("Error in BrandedKeynfo.SetInfoFromXmlNode: invalid xml node.");
				return;
			}
			source = ((XmlElement)aKeyBrandInfoXmlNode).GetAttribute(BrandLoader.XML_ATTRIBUTE_SOURCE);
			branded = ((XmlElement)aKeyBrandInfoXmlNode).GetAttribute(BrandLoader.XML_ATTRIBUTE_BRANDED);

		}
		#endregion 
	}
	#endregion 
}
