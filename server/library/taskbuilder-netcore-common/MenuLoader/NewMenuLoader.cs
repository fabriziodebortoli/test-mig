using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.Common.Generic;
using Microarea.Common.GenericForms;
using Microarea.Common.NameSolver;
using Microarea.Common.WebServicesWrapper;
using Newtonsoft.Json;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.MenuLoader.MenuInfo;
using static Microarea.Common.MenuLoader.MenuLoader;

namespace Microarea.Common.MenuLoader
{
	/// <summary>
	/// NewMenuLoader
	/// </summary>
	public static class NewMenuLoader
	{

		//---------------------------------------------------------------------
		public static XmlDocument GetMenuXmlInfinity(string user, string company, string connection, string filename, string LoginId, string CompanyId)
		{
			PathFinder pf = new PathFinder(company, user);
			XmlDocument doc = null;
			string error = "";
			try
			{
				using (MagoMenuParser mgmenu = new MagoMenuParser(connection, filename))
				{
					MenuLoader menuLoader = new MenuLoader(pf, null, true);
					int nMenuRows = mgmenu.ExistMenu(Int32.Parse(LoginId), Int32.Parse(CompanyId));
					if (nMenuRows > -1)
					{
                        //Lara
                        CachedMenuInfos cachedMenuInfos = new CachedMenuInfos(CommandsTypeToLoad.All, LoginFacilities.loginManager.GetConfigurationHash(), pf);
                        MenuInfo.CachedMenuInfos pInfo = cachedMenuInfos.Load(CommandsTypeToLoad.All, LoginFacilities.loginManager.GetConfigurationHash(), company);
                        if (pInfo != null && nMenuRows > 0)
                            return null;
                        menuLoader.LoadAllMenus(false, false);
						doc = menuLoader.ProcessMenu();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Write(ex.Message);
				error = ex.Message;
			}

			if (doc == null)
				throw new Exception("Error parsing Mago Menu " + error);
			return doc;
		}
		//---------------------------------------------------------------------
		private static XmlDocument GetMenuXml(string user, string company, string authenticationToken, bool clearCachedData, PathFinder pf)
		{
            XmlDocument doc = null;
			try
			{
                MenuLoader menuLoader = new MenuLoader(pf, authenticationToken, true);
				menuLoader.LoadAllMenus(false, clearCachedData);
				doc = menuLoader.ProcessMenu();
			}
			catch (Exception ex)
			{
				Debug.Write(ex.Message);
			}

			if (doc == null)
				throw new Exception("new menu not found");

			ProcessMostUsedNodes(doc, user, company, pf);

			ProcessFavoritesNodes(doc, user, company, pf);

			ProcessHiddenNodes(doc, user, company, pf);

			return doc;
		}

        //---------------------------------------------------------------------------------
        public static bool IsOldMenuFile(string user, string company, DateTime dateTime, string authenticationToken)
        {
            PathFinder pf = new PathFinder(company, user);
            MenuLoader menuLoader = new MenuLoader(pf, authenticationToken, true);
            return !menuLoader.IsCacheValid(dateTime);

            //TODO LARA NN LA CANCELLO ANCORA
            //parte x il file dell utente 
            //string originalStandardFile = pf.GetCustomUserCachedMenuFile();

            //if (pf.ExistFile(originalStandardFile))
            //{
            //    TBFile file = pf.GetTBFile(originalStandardFile);
            //    if (file == null)
            //        return false;

            //    if (DateTime.Compare(file.LastWriteTime, dateTime) > 0)
            //        return true;
            //}
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Se il menu è già stato caricato, ritorna il json al menu in caricamento (e lo cancella),
        /// altrimenti lo carica al volo e lo ritorna
        /// </summary>
        public static string LoadMenuWithFavoritesAsJson(string user, string company, string authenticationToken, bool clearCachedData)
		{ 
            //TODO LARA taengo ancora
             PathFinder pf = new PathFinder(company, user);

   //         string originalStandardFile = pf.GetCustomUserCachedMenuFile();
           
			//if (pf.ExistFile(originalStandardFile))
			//{
                
   //             return pf.GetFileTextFromFileName(originalStandardFile);
   //         }

			XmlDocument doc = GetMenuXml(user, company, authenticationToken, clearCachedData, pf);
            return NewMenuFunctions.GetAngularJSSafeJson(doc);

            //byte[] byteArray = Encoding.UTF8.GetBytes(result);
            //MemoryStream stream = new MemoryStream(byteArray);
          //  pf.SaveTextFileFromStream(originalStandardFile, stream);

        //    return result;

        }

		//---------------------------------------------------------------------
		private static void ProcessMostUsedNodes(XmlDocument doc, string user, string company, PathFinder pf)
		{
			string mostUsedFile = NewMenuFunctions.GetCustomUserMostUsedFile(pf);
			XmlDocument mostUsedDoc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(mostUsedFile, pf);
			XmlNodeList nodeList = mostUsedDoc.SelectNodes("//MostUsed");
			foreach (XmlNode nodeToProcess in nodeList)
			{
				XmlAttribute objectTypeAttribute = nodeToProcess.Attributes["objectType"];
				XmlAttribute targetAttribute = nodeToProcess.Attributes["target"];
				if (objectTypeAttribute == null || targetAttribute == null)
					return;

				XmlNode searchNode = doc.SelectSingleNode(
				string.Concat(
					"//Object[",
					string.Format(MenuTranslatorStrings.translateTemplate, "target", targetAttribute.Value) + " and ",
					string.Format(MenuTranslatorStrings.translateTemplate, "objectType", objectTypeAttribute.Value) + " ] "));
                 
				if (searchNode == null)
					continue;

				XmlAttribute newMostUsedAttribute = searchNode.Attributes["isMostUsed"];
				if (newMostUsedAttribute == null)
				{
					newMostUsedAttribute = doc.CreateAttribute("isMostUsed");
					searchNode.Attributes.Append(newMostUsedAttribute);
				}

				newMostUsedAttribute.Value = "true";


				XmlAttribute lastModifiedAttribute = nodeToProcess.Attributes["lastModified"];
				if (lastModifiedAttribute == null)
					return;

				XmlAttribute newlastModifiedAttribute = searchNode.Attributes["lastModified"];
				if (newlastModifiedAttribute == null)
				{
					newlastModifiedAttribute = doc.CreateAttribute("lastModified");
					searchNode.Attributes.Append(newlastModifiedAttribute);
				}
				newlastModifiedAttribute.Value = lastModifiedAttribute.Value;
			}
		}

		//---------------------------------------------------------------------
		private static void ProcessFavoritesNodes(XmlDocument doc, string user, string company, PathFinder pf)
        {
			string favoritesFile = NewMenuFunctions.GetCustomUserFavoriteFile(pf);
			XmlDocument favoritesDoc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(favoritesFile, pf);
			XmlNodeList nodeList = favoritesDoc.SelectNodes("//Favorite");
			foreach (XmlNode nodeToProcess in nodeList)
			{
				XmlAttribute objectTypeAttribute = nodeToProcess.Attributes["objectType"];
				XmlAttribute targetAttribute = nodeToProcess.Attributes["target"];
				XmlAttribute isFavoriteAttribute = nodeToProcess.Attributes["isFavorite"];
				if (objectTypeAttribute == null || targetAttribute == null)
					continue;

				XmlNode searchNode = doc.SelectSingleNode(
				string.Concat(
					"//Object[",
					string.Format(MenuTranslatorStrings.translateTemplate, "target", targetAttribute.Value) + " and ",
					string.Format(MenuTranslatorStrings.translateTemplate, "objectType", objectTypeAttribute.Value) + " ] "));

				if (searchNode == null)
					continue;

				XmlAttribute newIsFavoriteAttribute = searchNode.Attributes["isFavorite"];
				if (newIsFavoriteAttribute == null)
				{
					newIsFavoriteAttribute = doc.CreateAttribute("isFavorite");
					searchNode.Attributes.Append(newIsFavoriteAttribute);
				}

				newIsFavoriteAttribute.Value = isFavoriteAttribute == null ? "true" : isFavoriteAttribute.Value;

				XmlAttribute positionFavoriteSourceAttribute = nodeToProcess.Attributes["position"];
				if (positionFavoriteSourceAttribute != null)
				{
					XmlAttribute favoritePositionAttribute = searchNode.Attributes["position"];
					if (favoritePositionAttribute == null)
					{
						favoritePositionAttribute = doc.CreateAttribute("position");
						searchNode.Attributes.Append(favoritePositionAttribute);
					}

					favoritePositionAttribute.Value = positionFavoriteSourceAttribute.Value;
				}
			}
		}



		//---------------------------------------------------------------------
		private static void ProcessHiddenNodes(XmlDocument doc, string user, string company, PathFinder pf)
        {
			string favoritesFile = NewMenuFunctions.GetCustomUserHiddenTilesFile(pf);
			XmlDocument favoritesDoc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(favoritesFile, pf);


			XmlNodeList nodeList = favoritesDoc.SelectNodes("//HiddenTile");
			foreach (XmlNode nodeToProcess in nodeList)
			{
				XmlAttribute applicationNameAttribute = nodeToProcess.Attributes["applicationName"];
				XmlAttribute groupNameAttribute = nodeToProcess.Attributes["groupName"];
				XmlAttribute menuNameAttribute = nodeToProcess.Attributes["menuName"];
				XmlAttribute tileNameAttribute = nodeToProcess.Attributes["tileName"];
				if (applicationNameAttribute == null || groupNameAttribute == null || menuNameAttribute == null || tileNameAttribute == null)
					continue;

				XmlNode searchNode = doc.SelectSingleNode(
					string.Concat(
						"//Application[" + string.Format(MenuTranslatorStrings.translateTemplate, "name", applicationNameAttribute.Value) + "]",
						"/Group[" + string.Format(MenuTranslatorStrings.translateTemplate, "name", groupNameAttribute.Value) + "]",
						"/Menu[" + string.Format(MenuTranslatorStrings.translateTemplate, "name", menuNameAttribute.Value) + "]",
						"/Menu[" + string.Format(MenuTranslatorStrings.translateTemplate, "name", tileNameAttribute.Value) + "]")
						);


				if (searchNode == null)
					continue;

				XmlAttribute hiddenTileAttribute = searchNode.Attributes["hiddenTile"];
				if (hiddenTileAttribute == null)
				{
					hiddenTileAttribute = doc.CreateAttribute("hiddenTile");
					searchNode.Attributes.Append(hiddenTileAttribute);
				}

				hiddenTileAttribute.Value = "true";
			}
		}

		//---------------------------------------------------------------------	
		private static string GetNrOfElementsToShow(XmlDocument doc)
		{
			XmlNode node = doc.SelectSingleNode("/Root");
			XmlAttribute attributeNrElementToShow = node.Attributes["nrElementToShow"];
			if (attributeNrElementToShow != null)
				return attributeNrElementToShow.Value;

			return string.Empty;
		}

		//---------------------------------------------------------------------	
		public static string GetMostUsedShowNrElements(string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserMostUsedFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);
			string nrElementToShow = string.Empty;
			string nrRecordsToShow = string.Empty;

			return GetNrOfElementsToShow(doc);
		}

		//---------------------------------------------------------------------	
		public static string GetPreferencesAsJson(string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserPreferencesFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

			return NewMenuFunctions.GetAngularJSSafeJson(doc);
		}

		//---------------------------------------------------------------------	
		public static string GetPreference(string preferenceName, string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserPreferencesFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

			//cerco il nodo per namespace
			XmlNode node = doc.SelectSingleNode(
				string.Concat(
							"//Preference[",
							string.Format(MenuTranslatorStrings.translateTemplate, "name", preferenceName) + " ] "
							)
					);

			if (node == null)
				return string.Empty;

			XmlAttribute valueAttribute = node.Attributes["value"];
			return (valueAttribute != null) ? valueAttribute.Value : string.Empty;
		}

		//---------------------------------------------------------------------	
		public static string GetHistoryShowNrElements(string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserHistoryFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);
			return GetNrOfElementsToShow(doc);
		}

		//---------------------------------------------------------------------
		public static string LoadHistoryAsJson(string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserHistoryFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file, pf);

			return NewMenuFunctions.GetAngularJSSafeJson(doc);
		}

		//---------------------------------------------------------------------
		public static string GetImagePathFromBrand(string brandedKey)
		{
			string imageNamespace = InstallationData.BrandLoader.GetBrandedStringBySourceString(brandedKey);
			if (string.IsNullOrEmpty(imageNamespace) || imageNamespace.CompareNoCase(brandedKey))
				return string.Empty;

			try
			{
				string installationPath = PathFinder.PathFinderInstance.GetStandardPath + NameSolverStrings.Directoryseparetor;
				return PathFinder.PathFinderInstance.GetImagePath(new NameSpace(imageNamespace)).Replace(installationPath, "");
			}
			catch
			{
				return string.Empty;
			}
		}


		//---------------------------------------------------------------------
		public static string LoadBrandInfoAsJson()
		{
			string producerSite = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSite");
			string releaseInfoUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("ReleaseInfoUrl");
			string productInfoUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProductInfoUrl");
			string producerLogoUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerLogoUrl");

			string onlineCoursesImage = GetImagePathFromBrand("OnlineCoursesImage");
			string onlineCoursesUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("OnlineCoursesUrl");

			string flowerUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("FlowerUrl");
			string flowerImage = GetImagePathFromBrand("FlowerImage");

			IBrandInfo info = InstallationData.BrandLoader.GetMainBrandInfo();
			string companyName = string.Empty;
			string productTitle = string.Empty;
			if (info == null)
			{
				companyName = info.Company;
				productTitle = info.ProductTitle;
			}


			string companyMenuBanner = GetImagePathFromBrand("MenuBannerImage");
			string producerLogo = GetImagePathFromBrand("ProducerLogo");


			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				JsonWriter jsonWriter = new JsonTextWriter(sw);

				jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
				jsonWriter.WriteStartObject();

				jsonWriter.WritePropertyName("BrandInfos");

				jsonWriter.WriteStartObject();

				jsonWriter.WritePropertyName("producerSite");
				jsonWriter.WriteValue(producerSite);

				jsonWriter.WritePropertyName("releaseInfoUrl");
				jsonWriter.WriteValue(releaseInfoUrl);

				jsonWriter.WritePropertyName("companyName");
				jsonWriter.WriteValue(companyName);

				jsonWriter.WritePropertyName("productTitle");
				jsonWriter.WriteValue(productTitle);

				if (!string.IsNullOrEmpty(companyMenuBanner))
				{
					jsonWriter.WritePropertyName("companyMenuBanner");
					jsonWriter.WriteValue(companyMenuBanner);
				}

				if (!string.IsNullOrEmpty(producerLogo))
				{
					jsonWriter.WritePropertyName("producerLogo");
					jsonWriter.WriteValue(producerLogo);
				}

				if (!string.IsNullOrEmpty(productInfoUrl))
				{
					jsonWriter.WritePropertyName("productInfoUrl");
					jsonWriter.WriteValue(productInfoUrl);
				}

				if (!string.IsNullOrEmpty(producerLogoUrl))
				{
					jsonWriter.WritePropertyName("producerLogoUrl");
					jsonWriter.WriteValue(producerLogoUrl);
				}

				if (!string.IsNullOrEmpty(onlineCoursesImage))
				{
					jsonWriter.WritePropertyName("onlineCoursesImage");
					jsonWriter.WriteValue(onlineCoursesImage);
				}

				if (!string.IsNullOrEmpty(onlineCoursesUrl))
				{
					jsonWriter.WritePropertyName("onlineCoursesUrl");
					jsonWriter.WriteValue(onlineCoursesUrl);
				}

				if (!string.IsNullOrEmpty(flowerImage))
				{
					jsonWriter.WritePropertyName("flowerImage");
					jsonWriter.WriteValue(flowerImage);
				}

				if (!string.IsNullOrEmpty(flowerUrl))
				{
					jsonWriter.WritePropertyName("flowerUrl");
					jsonWriter.WriteValue(flowerUrl);
				}


				string userInfoName, userInfoCompany, version;
				GetVersionAndUserInfos(out userInfoName, out userInfoCompany, out version);

				if (!string.IsNullOrEmpty(userInfoName))
				{
					jsonWriter.WritePropertyName("userInfoName");
					jsonWriter.WriteValue(userInfoName);
				}

				if (!string.IsNullOrEmpty(userInfoCompany))
				{
					jsonWriter.WritePropertyName("userInfoCompany");
					jsonWriter.WriteValue(userInfoCompany);
				}

				if (!string.IsNullOrEmpty(version))
				{
					jsonWriter.WritePropertyName("version");
					jsonWriter.WriteValue(version);
				}

				jsonWriter.WriteEndObject();
				jsonWriter.WriteEndObject();

				string output = sw.ToString();

				jsonWriter.Close();
				return output;
			}
		}

		//---------------------------------------------------------------------
		public static void GetVersionAndUserInfos(out string userInfoName, out string userInfoCompany, out string version)
		{
			userInfoName = null;
			userInfoCompany = null;
			version = null;


			XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(LoginManager.LoginManagerInstance.GetUserInfo());
			XmlNode node = xDoc.SelectSingleNode("//Name");
			if (node != null)
				userInfoName = node.InnerText;

			node = xDoc.SelectSingleNode("//Company");
			if (node != null)
				userInfoCompany = node.InnerText;

			version = LoginManager.LoginManagerInstance.GetInstallationVersion();
		}

		//---------------------------------------------------------------------
		public static string GetLoginInitImage()
		{
			if (LoginManager.LoginManagerInstance.IsActivated("Erp", "imago"))
				return GetImagePathFromBrand("ILoginHeaderImage");
			else
				return GetImagePathFromBrand("LoginHeaderImage");
		}

		//---------------------------------------------------------------------
		public static string GetLoginInitInformation()
		{
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				JsonWriter jsonWriter = new JsonTextWriter(sw);
				jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
				jsonWriter.WriteStartObject();

				LoginFacilities lf = new LoginFacilities();
				lf.Load();

				jsonWriter.WritePropertyName("userName");
				jsonWriter.WriteValue(lf.User);

				jsonWriter.WritePropertyName("passWord");
				jsonWriter.WriteValue(lf.Password);

				jsonWriter.WritePropertyName("company");
				jsonWriter.WriteValue(lf.Company);

				jsonWriter.WritePropertyName("rememberMe");
				jsonWriter.WriteValue(lf.RememberMe);

				jsonWriter.WritePropertyName("windowsAuthentication");
				jsonWriter.WriteValue(lf.Canusewinnt);

				jsonWriter.WritePropertyName("windowsAuthenticationSelected");
				jsonWriter.WriteValue(lf.WinNTSelected);

				jsonWriter.WritePropertyName("NTcompany");
				jsonWriter.WriteValue(lf.NTcompany);
				jsonWriter.WritePropertyName("NTLoginName");
				jsonWriter.WriteValue(lf.NTLoginName);
				jsonWriter.WritePropertyName("NTpassword");
				jsonWriter.WriteValue(lf.NTpassword);
				jsonWriter.WritePropertyName("clearCachedDataVisible");
				jsonWriter.WriteValue(lf.ClearCachedDataVisible);

				jsonWriter.WritePropertyName("autoLoginVisible");
				jsonWriter.WriteValue(lf.Autologinable);

				lf.Diagnostic.ToJson(jsonWriter, true);//se ci sono messaggi, li serializzo all'utente
				jsonWriter.WriteEndObject();
				string output = sw.ToString();

				jsonWriter.Close();
				return output;
			}

		}

		//---------------------------------------------------------------------
		public static string GetConnectionInformation(string authenticationToken)
		{
			LoginFacilities lf = new LoginFacilities();
			lf.Load();

			// la chiamata di questo metodo mi serve per caricare l'informazione EasyStudioDeveloper
			LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
			StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				JsonWriter jsonWriter = new JsonTextWriter(sw);

				jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
				jsonWriter.WriteStartObject();

				if (loginManagerSession.LoginManagerSessionState == LoginManagerState.Logged)
				{
					jsonWriter.WritePropertyName("user");
					jsonWriter.WriteValue(loginManagerSession.UserName);
					jsonWriter.WritePropertyName("admin");
					jsonWriter.WriteValue(loginManagerSession.Admin ? MenuStrings.Yes : MenuStrings.No);
					jsonWriter.WritePropertyName("ebdev");
					bool ok = loginManagerSession.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner) &&
                                                    LoginManager.LoginManagerInstance.IsEasyStudioDeveloper(loginManagerSession.AuthenticationToken);
					jsonWriter.WriteValue(ok ? MenuStrings.Yes : MenuStrings.No);
					jsonWriter.WritePropertyName("company");
					jsonWriter.WriteValue(loginManagerSession.CompanyName);

					jsonWriter.WritePropertyName("dbserver");
					jsonWriter.WriteValue(loginManagerSession.DbServer);
					jsonWriter.WritePropertyName("dbuser");
					jsonWriter.WriteValue(loginManagerSession.DbUser);
					jsonWriter.WritePropertyName("dbname");
					jsonWriter.WriteValue(loginManagerSession.DbName);

					jsonWriter.WritePropertyName("installation");
					jsonWriter.WriteValue(PathFinder.PathFinderInstance.Installation);
					jsonWriter.WritePropertyName("remotefileserver");
					jsonWriter.WriteValue(PathFinder.PathFinderInstance.RemoteFileServer);
					jsonWriter.WritePropertyName("remotewebserver");
					jsonWriter.WriteValue(PathFinder.PathFinderInstance.RemoteWebServer);

					jsonWriter.WritePropertyName("security");
					jsonWriter.WriteValue(loginManagerSession.Security ? MenuStrings.Enabled : MenuStrings.Disabled);
					jsonWriter.WritePropertyName("auditing");
					jsonWriter.WriteValue(loginManagerSession.Auditing ? MenuStrings.Enabled : MenuStrings.Disabled);

					bool showDBSizeControls = (LoginManager.LoginManagerInstance.GetDBNetworkType() == DBNetworkType.Small &&
							string.Compare(loginManagerSession.ProviderName, NameSolverDatabaseStrings.SQLOLEDBProvider, StringComparison.OrdinalIgnoreCase) == 0 ||
							string.Compare(loginManagerSession.ProviderName, NameSolverDatabaseStrings.SQLODBCProvider, StringComparison.OrdinalIgnoreCase) == 0);

					jsonWriter.WritePropertyName("showdbsizecontrols");
					jsonWriter.WriteValue(showDBSizeControls ? MenuStrings.Yes : MenuStrings.No);
					float usagePercentage = LoginManager.LoginManagerInstance.GetUsagePercentageOnDBSize(loginManagerSession.ConnectionString);
					showDBSizeControls = showDBSizeControls && (usagePercentage == -1);
					jsonWriter.WritePropertyName("freespace");
					jsonWriter.WriteValue(showDBSizeControls ? (100 - usagePercentage).ToString() : "");
					jsonWriter.WritePropertyName("usedspace");
					jsonWriter.WriteValue(showDBSizeControls ? usagePercentage.ToString() : "");
				}

				lf.Diagnostic.ToJson(jsonWriter, true);//se ci sono messaggi, li serializzo all'utente
				jsonWriter.WriteEndObject();

				string output = sw.ToString();

				jsonWriter.Close();
				return output;
			}

		}

        //---------------------------------------------------------------------
        public static string GetJsonProductInfo(string authenticationToken)
        {
            LoginManagerSession session = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                JsonWriter jsonWriter = new JsonTextWriter(sw);
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("ProductInfos");

                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("installationVersion");
                jsonWriter.WriteValue(LoginManager.LoginManagerInstance.GetInstallationVersion());

                jsonWriter.WritePropertyName("providerDescription");
                jsonWriter.WriteValue(session == null ? "" : session.ProviderDescription);

                jsonWriter.WritePropertyName("edition");
                jsonWriter.WriteValue(LoginManager.LoginManagerInstance.GetEditionType());

				jsonWriter.WritePropertyName("userLogged");
				jsonWriter.WriteValue(session != null);

				jsonWriter.WritePropertyName("installationName");
				jsonWriter.WriteValue(PathFinder.PathFinderInstance.Installation); ////jsonWriter.WriteString(_T("installationName"), AfxGetPathFinder()->GetInstallationName());

                jsonWriter.WritePropertyName("activationState");
                jsonWriter.WriteValue(LoginManager.LoginManagerInstance.GetActivationStateInfo());

                string debugState = string.Empty;
#if DEBUG
                debugState += MenuStrings.DebugVersion;
#endif
				jsonWriter.WritePropertyName("debugState");
				jsonWriter.WriteValue(debugState);
				
				jsonWriter.WritePropertyName("Applications");
				jsonWriter.WriteStartArray();
				
				PathFinder. PathFinderInstance.GetApplicationsList(ApplicationType.All, out StringCollection apps);
			    BrandLoader brand = new BrandLoader();


              foreach (string app in apps)
				{
					ApplicationInfo appInfo = PathFinder.PathFinderInstance.GetApplicationInfoByName(app);
					//appInfo.Name
                    if (appInfo.Modules.Count <= 0)
                        continue;

                    IEnumerator enumerator = appInfo.Modules.GetEnumerator();
                    if (!enumerator.MoveNext())
                        continue;

                    ModuleInfo firstModule = enumerator.Current as ModuleInfo;
                    if (firstModule == null)
                        continue;
                    string sActive = session != null && session.IsActivated(appInfo.Name, firstModule.Name)
                        ? EnumsStateStrings.Licensed
                        : EnumsStateStrings.NotLicensed;

                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("application");
                    string appName = brand.GetApplicationBrandMenuTitle(appInfo.Name);
                    string appWithVersion = string.Format("{0} rel. {1}", !string.IsNullOrEmpty(appName) ? appName : appInfo.Name, appInfo.ApplicationConfigInfo.Version);
                    jsonWriter.WriteValue(appWithVersion);  // TODOLUCA la versione accanto al nome non mi convince, potrebbe essere sbagliata

                    jsonWriter.WritePropertyName("licensed");
                    jsonWriter.WriteValue(sActive);
                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.WriteEndObject();

                string output = sw.ToString();

                jsonWriter.Close();
                return output;
            }
        }

		//---------------------------------------------------------------------
		public static string GetJsonMenuSettings(string authenticationToken)
		{
			ITheme theme = DefaultTheme.GetTheme();
			if (theme == null)
				return string.Empty;

            LoginManagerSession loginManagerSession = LoginManagerSessionManager.GetLoginManagerSession(authenticationToken);
            if (loginManagerSession == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
			using (StringWriter sw = new StringWriter(sb))
			{
				JsonWriter jsonWriter = new JsonTextWriter(sw);

				jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
				jsonWriter.WriteStartObject();

				jsonWriter.WritePropertyName("ThemedSettings");

				jsonWriter.WriteStartObject();

				string val = theme.GetStringThemeElement("NrMaxItemsSearch");
				if (!string.IsNullOrEmpty(val))
				{
					jsonWriter.WritePropertyName("nrMaxItemsSearch");
					jsonWriter.WriteValue(val);
				}

				val = theme.GetStringThemeElement("ShowWorkerImage");
				if (!string.IsNullOrEmpty(val))
				{
					jsonWriter.WritePropertyName("showWorkerImage");
					jsonWriter.WriteValue(val);
				}

				val = theme.GetStringThemeElement("ShowSearchBox");
				if (!string.IsNullOrEmpty(val))
				{
					jsonWriter.WritePropertyName("showSearchBox");
					jsonWriter.WriteValue(val);
				}

				val = theme.GetStringThemeElement("ShowListIcons");
				if (!string.IsNullOrEmpty(val))
				{
					jsonWriter.WritePropertyName("showListIcons");
					jsonWriter.WriteValue(val);
				}

				jsonWriter.WriteEndObject();

				jsonWriter.WritePropertyName("OtherSettings");
				jsonWriter.WriteStartObject();

                bool ok = loginManagerSession.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner) &&
                                                LoginFacilities.loginManager.IsEasyStudioDeveloper(loginManagerSession.AuthenticationToken);

				jsonWriter.WritePropertyName("isEasyStudioActivated");
				jsonWriter.WriteValue(ok);

				ok = loginManagerSession.IsActivated("MicroareaConsole", "TaskScheduler") && loginManagerSession.Admin;

				jsonWriter.WritePropertyName("isTaskSchedulerActivated");
				jsonWriter.WriteValue(ok);

				jsonWriter.WriteEndObject();
				jsonWriter.WriteEndObject();

				string output = sw.ToString();

				jsonWriter.Close();
				return output;
			}

		}
	}

	public class MagoMenuParser : IDisposable
	{
		private XmlDocument _doc = null;
		private string _SQLConnection;// = "Data Source={0}; Initial Catalog='{1}';User ID='{2}';Password='{3}'";

		private string sqlInsert = "insert into MSD_IMagoMenu (MMMENUID, MMLSTUPD, MM__VOCE, MM_LEVEL, MMDIRECT, MMINFMOD, MMLVLKEY, MMELEMEN, MM_PROGR, MMINFPRO) values ('{0}', '{1}', '{2}', {3}, {4}, 1, '{5}',{6}, {7}, '{8}')";
		private string sqlDelete = "delete from MSD_IMagoMenu where MMMENUID='{0}' AND MMLSTUPD='{1}'";
		private string sqlSelect = "select count(*) from MSD_IMagoMenu where MMMENUID='{0}' AND MMLSTUPD='{1}'";
		private string sqlSelCompUsr = "select MMMENUID from MSD_CompanyLogins where LoginId={0} AND CompanyId={1}";
		private string magofunc = "function:openMago(\"{0}\", \"\",\"\",\"{1}\")";
		private string prefix = "001.";
		private char pad = '0';
		private int nIdx = 0;
		private string _mmenuid;
		private string _mmlstupid;
		private string _logfile;
		private int _loginId;
		private int _CompanyId;
		SqlConnection connection = null;
		SqlCommand sqlRowInsert = null;
		StreamWriter sw = null;
		SqlTransaction transaction = null;
		int nrows;

		//-----------------------------------------------------------------
		public MagoMenuParser(
								string connection,
								string logfilename
							   )
		{
			string[] tk = connection.Split(';');

			_SQLConnection += tk[1];

			for (int i = 2; i < tk.Length; i++)
				_SQLConnection += ';' + tk[i];

			_logfile = logfilename;
		}

		//-----------------------------------------------------------------
		public MagoMenuParser(
								string connectstr,
								string xmldoc,
								string loginid,
								string companyid,
								string logfilename
								)
		{
			string[] tk = connectstr.Split(';');

			_SQLConnection += tk[1];

			for (int i = 2; i < tk.Length; i++)
				_SQLConnection += ';' + tk[i];

			_logfile = logfilename;

			_loginId = Int32.Parse(loginid);
			_CompanyId = Int32.Parse(companyid);

            //Lara
			_doc = new XmlDocument();
			_doc.LoadXml(xmldoc);

		}

		//--------------------------------------------------------------------------------------------------------
		public void Dispose()
		{
			if (sqlRowInsert != null)
				sqlRowInsert.Dispose();
			if (transaction != null)
				transaction.Dispose();
			if (connection != null)
			{
				connection.Close();
				connection.Dispose();
			}
			if (sw != null)
			{
				sw.Flush();
				//sw.Close();
				sw.Dispose();
			}
		}

		//--------------------------------------------------------------------------------------------------------
		private bool RetrieveMMENUID(int LoginID, int CompanyId)
		{
			if (connection == null)
			{
				connection = new SqlConnection(_SQLConnection);
				connection.Open();
			}

			try
			{
				string SqlSelect = string.Format(sqlSelCompUsr, LoginID, CompanyId);
				SqlCommand sqlSel = new SqlCommand(SqlSelect, connection);
				using (SqlDataReader reader = sqlSel.ExecuteReader())
				{
					try
					{
						while (reader.Read())
						{
							_mmenuid = (string)reader["MMMENUID"];
							_mmlstupid = "";

						}
					}
					finally
					{
						// Always call Close when done reading.
						//reader.Close();
					}
				}
			}
			catch (Exception ex)
			{
				_mmenuid = "";
				_mmlstupid = "";
				sw.WriteLine("RetrieveMMENUID(): Exception Type: {0}", ex.GetType());
				sw.WriteLine("RetrieveMMENUID():  Message: {0}", ex.Message);
				sw.WriteLine("RetrieveMMENUID():  Parametri: LoginID: {0} - CompanyId: {1}", LoginID.ToString(), CompanyId.ToString());
				return false;
			}
			return true;
		}

		//-----------------------------------------------------------------
		public int ExistMenu(int LoginID, int CompanyId)
		{
			int rownum = -1;

			if (sw == null)
				try
				{
					string dirPath = Path.GetDirectoryName(_logfile);
					if (!PathFinder.PathFinderInstance.ExistPath(dirPath))
                        PathFinder.PathFinderInstance.CreateFolder(dirPath, true);
					sw = File.AppendText(_logfile);
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}

			bool bOk = RetrieveMMENUID(LoginID, CompanyId);

			if (bOk)
			{
				try
				{
					string SqlSelect = string.Format(sqlSelect, _mmenuid, _mmlstupid);
					SqlCommand sqlSel = new SqlCommand(SqlSelect, connection);
					rownum = (int)sqlSel.ExecuteScalar();

					if (rownum > 0)
						connection.Close();

				}
				catch (Exception ex)
				{
					sw.WriteLine("ExistMenu(): Exception Type: {0}", ex.GetType());
					sw.WriteLine("ExistMenu(): Message: {0}", ex.Message);
				}
			}

			return rownum;
		}

		//-----------------------------------------------------------------
		public void menu_insert(XmlNodeList nodeList, string prefix, int lvl, ref int nIdx)
		{
			int mylvl = lvl + 1;
			int menu_idx = 1;
			string proc = "function:javascript(0)";
			int mm_direct = 1;
			int mm_elem = 0;
			string padded_prefix;

			foreach (XmlNode xn in nodeList)
			{
				if (xn.Name != "Title")
				{
					if (xn.Name == "Menu" && xn.ChildNodes.Count == 0)
						continue;

					string sql;

					padded_prefix = menu_idx.ToString();
					string str_lvlkey = prefix + '.' + padded_prefix.PadLeft(3, pad);
					XmlAttributeCollection Attributes = xn.Attributes;
					if (xn.Attributes["title"] == null)
						return;
					if (xn.Name == "Object")
					{
						if (xn.Attributes["objectType"].Value == "Report")
							proc = string.Format(magofunc, xn.Attributes["target"].Value, "report");
						else if (xn.Attributes["objectType"].Value == "Function")
							proc = string.Format(magofunc, xn.Attributes["target"].Value, "function");
						else
							proc = string.Format(magofunc, xn.Attributes["target"].Value, "document");
						mm_direct = 1;
						mm_elem = 0;
					}
					else
					{
						mm_direct = 2;
						mm_elem = 1;
					}

					string escaped_cmd = xn.Attributes["title"].Value.Replace("'", "''");
					if (xn.Name == "Object" && xn.Attributes["objectType"].Value == "Report")
						escaped_cmd += " (Report)";
					sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, escaped_cmd, mylvl, mm_direct, str_lvlkey, mm_elem, nIdx, proc);

					try
					{
						sqlRowInsert = new SqlCommand(sql, connection, transaction);
						nrows = sqlRowInsert.ExecuteNonQuery();
						nIdx += 1;
						menu_idx += 1;
						if (xn.ChildNodes.Count > 0)
							menu_insert(xn.ChildNodes, str_lvlkey, mylvl, ref nIdx);
					}
					catch (Exception ex)
					{
						sw.WriteLine("menu_insert(): Query Exception Type: {0}", ex.GetType());
						sw.WriteLine("menu_insert(): Exception Message: {0}", ex.Message);
						sw.WriteLine("menu_insert(): Query: {0}", sql);
					}
				}
			}
		}
		//-----------------------------------------------------------------
		public bool Parse()
		{
			if (sw == null)
				try
				{
					string dirPath = Path.GetDirectoryName(_logfile);
					if (!PathFinder.PathFinderInstance.ExistPath(dirPath))
                        PathFinder.PathFinderInstance.CreateFolder(dirPath, false);
					sw = File.AppendText(_logfile);
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}

			RetrieveMMENUID(_loginId, _CompanyId);

			string SqlDelete = string.Format(sqlDelete, _mmenuid, _mmlstupid);
			SqlCommand sqlDel = new SqlCommand(SqlDelete, connection);
			nrows = sqlDel.ExecuteNonQuery();

			transaction = connection.BeginTransaction();

			try
			{

				XmlNodeList nodeList;
				XmlElement root = _doc.DocumentElement;

				string initprefix = "001";

				string sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, "Menu Principale", 1, 1, initprefix, 0, nIdx, "function:javascript(0)");
				sqlRowInsert = new SqlCommand(sql, connection, transaction);
				nrows = sqlRowInsert.ExecuteNonQuery();
				nIdx += 1;

				XmlNodeList nodeAppList = root.SelectNodes("/Root/ApplicationMenu/AppMenu/Application");
				int macro_menu_idx = 1;
				string macro_menu_prefix = string.Empty;
				int macro_level;
				string padded_prefix;
				string lvl_prefix;

				foreach (XmlNode xnApp in nodeAppList)
				{
					if (xnApp.Attributes["name"].Value != "ERP")
					{
						padded_prefix = macro_menu_idx.ToString();
						macro_menu_prefix = prefix + padded_prefix.PadLeft(3, pad);
						sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xnApp.Attributes["name"].Value, 2, 2, macro_menu_prefix, 1, nIdx, "function:javascript(0)");
						sqlRowInsert = new SqlCommand(sql, connection, transaction);
						nrows = sqlRowInsert.ExecuteNonQuery();
						nIdx += 1;
					}

					nodeList = root.SelectNodes("/Root/ApplicationMenu/AppMenu/Application[@name='" + xnApp.Attributes["name"].Value + "']/Group");

					if (xnApp.Attributes["name"].Value != "ERP")
						lvl_prefix = macro_menu_prefix + ".";
					else
						lvl_prefix = prefix;

					foreach (XmlNode xn in nodeList)
					{
						XmlAttributeCollection Attributes = xn.Attributes;
						XmlNode title = xn.SelectSingleNode(".//Title");

						macro_level = 2;

						padded_prefix = macro_menu_idx.ToString();
						macro_menu_prefix = lvl_prefix + padded_prefix.PadLeft(3, pad);
						if (xn.ChildNodes.Count > 0)
						{
							sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xn.Attributes["title"].Value, macro_level, 2, macro_menu_prefix, 1, nIdx, "");
							sqlRowInsert = new SqlCommand(sql, connection, transaction);
							nrows = sqlRowInsert.ExecuteNonQuery();
							menu_insert(xn.ChildNodes, macro_menu_prefix, macro_level, ref nIdx);
							nIdx += 1;
							macro_menu_idx += 1;
						}
					}
				}
				//nodeList = root.SelectNodes("/Root/ApplicationMenu/AppMenu/Application[@name='TBS']/Group");

				//foreach (XmlNode xn in nodeList)
				//{
				//    XmlAttributeCollection Attributes = xn.Attributes;
				//    XmlNode title = xn.SelectSingleNode(".//Title");
				//    macro_level = 2;
				//    padded_prefix = macro_menu_idx.ToString();
				//    macro_menu_prefix = prefix + padded_prefix.PadLeft(3, pad);
				//    if (xn.ChildNodes.Count > 0)
				//    {
				//        sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xn.Attributes["title"].Value, macro_level, 2, macro_menu_prefix, 1, nIdx, "");
				//        sqlRowInsert = new SqlCommand(sql, connection, transaction);
				//        nrows = sqlRowInsert.ExecuteNonQuery();
				//        menu_insert(xn.ChildNodes, macro_menu_prefix, macro_level, ref nIdx);
				//        nIdx += 1;
				//        macro_menu_idx += 1;
				//    }
				//}
				nodeList = root.SelectNodes("/Root/EnvironmentMenu/AppMenu/Application[@name='Framework']");
				if (nodeList.Count > 0)
				{
					padded_prefix = macro_menu_idx.ToString();
					macro_menu_prefix = prefix + padded_prefix.PadLeft(3, pad);
					sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, "TBF", 2, 2, macro_menu_prefix, 1, nIdx, "function:javascript(0)");
					sqlRowInsert = new SqlCommand(sql, connection, transaction);
					nrows = sqlRowInsert.ExecuteNonQuery();
					nIdx += 1;
				}
				nodeList = root.SelectNodes("/Root/EnvironmentMenu/AppMenu/Application[@name='Framework']/Group");

				macro_menu_idx = 1;
				string tbf_prefix = macro_menu_prefix + ".";

				foreach (XmlNode xn in nodeList)
				{
					XmlAttributeCollection Attributes = xn.Attributes;
					XmlNode title = xn.SelectSingleNode(".//Title");
					macro_level = 2;

					padded_prefix = macro_menu_idx.ToString();
					macro_menu_prefix = tbf_prefix + padded_prefix.PadLeft(3, pad);
					if (xn.ChildNodes.Count > 0)
					{
						sql = string.Format(sqlInsert, _mmenuid, _mmlstupid, xn.Attributes["title"].Value, macro_level, 2, macro_menu_prefix, 1, nIdx, "");
						sqlRowInsert = new SqlCommand(sql, connection, transaction);
						nrows = sqlRowInsert.ExecuteNonQuery();
						menu_insert(xn.ChildNodes, macro_menu_prefix, macro_level, ref nIdx);
						nIdx += 1;
						macro_menu_idx += 1;
					}
				}
				transaction.Commit();
			}
			catch (Exception ex)
			{
				sw.WriteLine("Parse(): Commit Exception Type: {0}", ex.GetType());
				sw.WriteLine("Parse(): Exception Message: {0}", ex.Message);
				try
				{
					transaction.Rollback();
				}
				catch (Exception ex2)
				{
					// This catch block will handle any errors that may have occurred
					// on the server that would cause the rollback to fail, such as
					// a closed connection.
					sw.WriteLine("Parse(): Rollback Exception Type: {0}", ex2.GetType());
					sw.WriteLine("Parse(): Message: {0}", ex2.Message);
				}
				return false;
			}
			connection.Close();
			sw.WriteLine("==================================================");
			return true;
		}



	}
}

