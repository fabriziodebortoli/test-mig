using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Newtonsoft.Json;

namespace Microarea.TaskBuilderNet.Core.MenuManagerLoader
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
                using (MagoMenuParser mgmenu = new MagoMenuParser(connection, user, company, "GetMenuXmlInfinity"))
                {
                    MenuLoader menuLoader = new MenuLoader(pf, null, true);
                    int nMenuRows = mgmenu.ExistMenu(Int32.Parse(LoginId), Int32.Parse(CompanyId));
                    if (nMenuRows > -1)
                    {
                        MenuInfo.CachedMenuInfos pInfo = MenuInfo.CachedMenuInfos.Load(MenuLoader.CommandsTypeToLoad.All, menuLoader.LoginManager.GetConfigurationHash(), user);
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

            if (doc == null || !(String.IsNullOrEmpty(error)))
            {
                throw new Exception("Error parsing Mago Menu " + error);
            }
            return doc;
        }

        //---------------------------------------------------------------------
        private static XmlDocument GetMenuXml(string user, string company, string authenticationToken)
		{
			PathFinder pf = new PathFinder(company, user);
			XmlDocument doc = null;
			try
			{
				LoginManager lm = new LoginManager(pf.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut);
				lm.GetLoginInformation(authenticationToken);
				MenuLoader menuLoader = new MenuLoader(pf, lm, true);
				menuLoader.LoadAllMenus(false, false);
				doc = menuLoader.ProcessMenu();
			}
			catch (Exception ex)
			{
				Debug.Write(ex.Message);
			}

			if (doc == null)
				throw new Exception("new menu not found");

			ProcessMostUsedNodes(doc, user, company);

			ProcessFavoritesNodes(doc, user, company);

			ProcessHiddenNodes(doc, user, company);

			//assegna un guid ad ogni nodo, per la velocizzazione nell'utilizzo di angular
			EnumerateNodes(doc);

			return doc;
		}

		//---------------------------------------------------------------------
		private static void EnumerateNodes(XmlDocument doc)
		{
			XmlNodeList list =  doc.SelectNodes("//*");
			foreach (XmlNode item in list)
			{
				XmlAttribute s = doc.CreateAttribute("node_id");
				s.Value = Guid.NewGuid().ToString();
				item.Attributes.Append(s);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Se il menu è già stato caricato, ritorna il json al menu in caricamento (e lo cancella),
		/// altrimenti lo carica al volo e lo ritorna
		/// </summary>
		public static string LoadMenuWithFavoritesAsJson(string user, string company, string authenticationToken)
		{
			string originalStandardFile = MenuInfo.GetFullMenuCachingFullFileName(user);
			FileInfo originalStandardFileInfo = new FileInfo(originalStandardFile);

			if (originalStandardFileInfo.Exists)
			{
				string result = File.ReadAllText(originalStandardFileInfo.FullName);
				try
				{
					File.Delete(originalStandardFileInfo.FullName);
				}
				catch (Exception)
				{
				}
				return result;
			}

			XmlDocument doc = GetMenuXml(user, company, authenticationToken);
			return NewMenuFunctions.GetAngularJSSafeJson(doc);
		}

		//---------------------------------------------------------------------
		private static void ProcessMostUsedNodes(XmlDocument doc, string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string mostUsedFile = NewMenuFunctions.GetCustomUserMostUsedFile(pf);
			XmlDocument mostUsedDoc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(mostUsedFile);
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
		private static void ProcessFavoritesNodes(XmlDocument doc, string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string favoritesFile = NewMenuFunctions.GetCustomUserFavoriteFile(pf);
			XmlDocument favoritesDoc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(favoritesFile);
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
		private static void ProcessHiddenNodes(XmlDocument doc, string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string favoritesFile = NewMenuFunctions.GetCustomUserHiddenTilesFile(pf);
			XmlDocument favoritesDoc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(favoritesFile);


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

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file);
			string nrElementToShow = string.Empty;
			string nrRecordsToShow = string.Empty;

			return GetNrOfElementsToShow(doc);
		}

		//---------------------------------------------------------------------	
		public static string GetPreferencesAsJson(string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserPreferencesFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file);

			return NewMenuFunctions.GetAngularJSSafeJson(doc);
		}

		//---------------------------------------------------------------------	
		public static string GetPreference(string preferenceName, string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserPreferencesFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file);

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

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file);
			return GetNrOfElementsToShow(doc);
		}

		//---------------------------------------------------------------------
		public static string LoadHistoryAsJson(string user, string company)
		{
			PathFinder pf = new PathFinder(company, user);
			string file = NewMenuFunctions.GetCustomUserHistoryFile(pf);

			XmlDocument doc = NewMenuFunctions.GetCustomUserAppDataXmlDocument(file);

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
				string installationPath = BasePathFinder.BasePathFinderInstance.GetStandardPath() + Path.DirectorySeparatorChar;
				return BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(imageNamespace)).Replace(installationPath, "");
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
			string releaseInfoUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("ReleaseInfoUrl", true);
			string productInfoUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProductInfoUrl");
			string producerLogoUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerLogoUrl");

			string onlineCoursesImage = GetImagePathFromBrand("OnlineCoursesImage");
			string onlineCoursesUrl = InstallationData.BrandLoader.GetBrandedStringBySourceString("OnlineCoursesUrl", true);

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
			StringWriter sw = new StringWriter(sb);
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
			sw.Close();

			return output;
		}

		//---------------------------------------------------------------------
		public static void GetVersionAndUserInfos(out string userInfoName, out string userInfoCompany, out string version)
		{
			LoginManager lm = new LoginManager();

			userInfoName = null;
			userInfoCompany = null;
			version = null;

			XmlDocument xDoc = new XmlDocument();
			xDoc.LoadXml(lm.GetUserInfo());
			XmlNode node = xDoc.SelectSingleNode("//Name");
			if (node != null)
				userInfoName = node.InnerText;

			node = xDoc.SelectSingleNode("//Company");
			if (node != null)
				userInfoCompany = node.InnerText;

			version = lm.GetInstallationVersion();
		}

		//---------------------------------------------------------------------
		public static string GetLoginInitImage()
		{
            LoginManager lm = new LoginManager();
            if (lm.IsActivated("Erp", "imago"))
            return GetImagePathFromBrand("ILoginHeaderImage");
            else
            return GetImagePathFromBrand("LoginHeaderImage");
        }

		//---------------------------------------------------------------------
		public static string GetLoginBackgroundImage()
		{
			return GetImagePathFromBrand("LoginBackgroundImage");
		}

		

		//---------------------------------------------------------------------
		public static string GetLoginInitInformation()
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
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
			sw.Close();

			return output;
		}

		//---------------------------------------------------------------------
		public static string GetConnectionInformation()
		{
			string Yes = "Yes";
			string No = "No";
			LoginFacilities lf = new LoginFacilities();
			lf.Load();


			// la chiamata di questo metodo mi serve per caricare l'informazione EasyBuilderDeveloper
			LoginFacilities.loginManager.GetLoginInformation(LoginFacilities.loginManager.AuthenticationToken);
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);

			jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
			jsonWriter.WriteStartObject();

			if (LoginFacilities.loginManager.LoginManagerState == LoginManagerState.Logged)
			{
				jsonWriter.WritePropertyName("user");
				jsonWriter.WriteValue(LoginFacilities.loginManager.UserName);
				jsonWriter.WritePropertyName("admin");
				jsonWriter.WriteValue(LoginFacilities.loginManager.Admin ? Yes : No);
				jsonWriter.WritePropertyName("ebdev");
				bool ok = LoginFacilities.loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner) &&
												LoginFacilities.loginManager.IsEasyBuilderDeveloper(LoginFacilities.loginManager.AuthenticationToken);
				jsonWriter.WriteValue(ok ? Yes : No);
				jsonWriter.WritePropertyName("company");
				jsonWriter.WriteValue(LoginFacilities.loginManager.CompanyName);

				jsonWriter.WritePropertyName("dbserver");
				jsonWriter.WriteValue(LoginFacilities.loginManager.DbServer);
				jsonWriter.WritePropertyName("dbuser");
				jsonWriter.WriteValue(LoginFacilities.loginManager.DbUser);
				jsonWriter.WritePropertyName("dbname");
				jsonWriter.WriteValue(LoginFacilities.loginManager.DbName);

				jsonWriter.WritePropertyName("installation");
				jsonWriter.WriteValue(BasePathFinder.BasePathFinderInstance.Installation);
				jsonWriter.WritePropertyName("remotefileserver");
				jsonWriter.WriteValue(BasePathFinder.BasePathFinderInstance.RemoteFileServer);
				jsonWriter.WritePropertyName("remotewebserver");
				jsonWriter.WriteValue(BasePathFinder.BasePathFinderInstance.RemoteWebServer);

				jsonWriter.WritePropertyName("security");
				jsonWriter.WriteValue(LoginFacilities.loginManager.Security ? Yes : No);
				jsonWriter.WritePropertyName("auditing");
				jsonWriter.WriteValue(LoginFacilities.loginManager.Auditing ? Yes : No);

				bool showDBSizeControls = (LoginFacilities.loginManager.GetDBNetworkType() == DBNetworkType.Small &&
						string.Compare(LoginFacilities.loginManager.ProviderName, NameSolverDatabaseStrings.SQLOLEDBProvider, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(LoginFacilities.loginManager.ProviderName, NameSolverDatabaseStrings.SQLODBCProvider, StringComparison.InvariantCultureIgnoreCase) == 0);

				 jsonWriter.WritePropertyName("showdbsizecontrols");
				jsonWriter.WriteValue(showDBSizeControls ? Yes : No);

                {
                    float usagePercentage = Functions.GetDBPercentageUsedSize(LoginFacilities.loginManager.NonProviderCompanyConnectionString);
                    showDBSizeControls = showDBSizeControls && (usagePercentage == -1);
                    jsonWriter.WritePropertyName("freespace");
                    jsonWriter.WriteValue(showDBSizeControls ? (100 - usagePercentage).ToString() : "NA");
                    jsonWriter.WritePropertyName("usedspace");
                    jsonWriter.WriteValue(showDBSizeControls ? usagePercentage.ToString() : "NA");
                }
            }

			lf.Diagnostic.ToJson(jsonWriter, true);//se ci sono messaggi, li serializzo all'utente
			jsonWriter.WriteEndObject();

			string output = sw.ToString();

			jsonWriter.Close();
			sw.Close();

			return output;
		}


        //---------------------------------------------------------------------
        public static string GetJsonMenuSettings()
		{
			ITheme theme = DefaultTheme.GetTheme();
			if (theme == null)
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
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

			val = theme.GetStringThemeElement("LastApplicationName");
			if (!string.IsNullOrEmpty(val))
			{
				jsonWriter.WritePropertyName("lastApplicationName");
				jsonWriter.WriteValue(val);
			}

			val = theme.GetStringThemeElement("LastGroupName");
			if (!string.IsNullOrEmpty(val))
			{
				jsonWriter.WritePropertyName("lastGroupName");
				jsonWriter.WriteValue(val);
			}

			val = theme.GetStringThemeElement("LastMenuName");
			if (!string.IsNullOrEmpty(val))
			{
				jsonWriter.WritePropertyName("lastMenuName");
				jsonWriter.WriteValue(val);
			}
			jsonWriter.WriteEndObject();

			jsonWriter.WritePropertyName("OtherSettings");
			jsonWriter.WriteStartObject();

			bool ok = LoginFacilities.loginManager.IsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner) &&
											LoginFacilities.loginManager.IsEasyBuilderDeveloper(LoginFacilities.loginManager.AuthenticationToken);

			jsonWriter.WritePropertyName("isEasyStudioActivated");
			jsonWriter.WriteValue(ok);

            ok = LoginFacilities.loginManager.IsActivated("MicroareaConsole", "TaskScheduler") && LoginFacilities.loginManager.Admin;

            jsonWriter.WritePropertyName("isTaskSchedulerActivated");
            jsonWriter.WriteValue(ok);

            jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();

			string output = sw.ToString();

			jsonWriter.Close();
			sw.Close();

			return output;
		}

		//---------------------------------------------------------------------
		public static bool isDeveloperEdition()
		{
			return LoginFacilities.loginManager.IsActivated(NameSolverStrings.TBS, "DevelopmentEd");
		}

	}

}

