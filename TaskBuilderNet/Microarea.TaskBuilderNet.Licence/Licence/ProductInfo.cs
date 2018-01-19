using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;
using WceAttribute = Microarea.TaskBuilderNet.Licence.Activation.WceStrings.Attribute;
using WceElement = Microarea.TaskBuilderNet.Licence.Activation.WceStrings.Element;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	/// <summary>
	/// Oggetto che rappresenta il Licensed.Config relativo ad un prodotto.
	/// </summary>
	//=========================================================================
	[Serializable]
	public class ProductInfo : IComparable
	{
        public int Priority;
        public	string				CompleteName;
        public string ViewName;
		public string Application;
        public string ProductName;
		public	ArrayList			ActivationKeys;
		public	ArticleInfo[]		Articles;
		public	IncludePathInfo[]	IncludeModulesPathsFiltered;
		public	string				Country;
		public string ActivationKey;
		public  string				Release;
		public	int					ActivationVersion;
		public	string				ProductId;
        public string AcceptDemo;
        public string Family;
		public	string				EditionId;
        public bool DevelopVersion; //globale tbs e dviu
        public bool DevelopPlusVersion; //globale solo tbs
        public bool DevelopVersionIU;
        public bool DevelopmentPlusK; 
        public bool DevelopmentPlusUser;
        public bool PersonalPlusUser;
        public bool PersonalPlusK;

        public	bool				ResellerVersion;
		public	bool				DistributorVersion;

        public bool MultiVersion;
        public bool TestVersion;
        public bool BackupVersion;
        public bool StandaloneVersion;

		public bool DemoVersion;
        //public bool SpecialVersion { get { return DevelopVersion || ResellerVersion || DevelopVersionIU || DevelopVersionPlus || DistributorVersion || DemoVersion; } }
        public bool IntegrativeVersion { get { return MultiVersion || TestVersion || BackupVersion || StandaloneVersion; } }
		public bool HasLicensed = false;
		public bool IsComplacent = true;

		//---------------------------------------------------------------------
		public ProductInfo(){} // DO NOT REMOVE, used by serialization

		//---------------------------------------------------------------------
		public ProductInfo(XmlElement productEl, string productRelease)
		{
			if (productEl == null)
				throw new ArgumentNullException();

//			if (productRelease == null)
//				this.Release = productEl.GetAttribute(WceAttribute.Release);
//			else
			this.Release = productRelease;

			string actVersionString = productEl.GetAttribute(WceAttribute.ActivationVersion);
			ActivationKey = GetKey(productEl);
			if (actVersionString != null && actVersionString.Length != 0)
			{
				int i = 0;
				try
				{
					i = int.Parse(actVersionString, CultureInfo.InvariantCulture);
				}
				catch (Exception exc)
				{
					Debug.WriteLine("ACTIVATIONVERSION non leggibile " + actVersionString + exc.Message); 
					i = 0;
				}
				this.ActivationVersion = i;
			}
		}

		//---------------------------------------------------------------------
		public static StringCollection GetArticleNames(IConfigurationInfoProvider configurationInfoProvider, string product)
		{
			XmlElement sol = configurationInfoProvider.GetProductSolution(product);
			StringCollection articles = new StringCollection();
			foreach (XmlElement solArt in sol.GetElementsByTagName(WceStrings.Element.SalesModule))
			{
				string artName = solArt.GetAttribute(WceStrings.Attribute.Name);
				articles.Add(artName);
			}
			return articles;
		}

		//---------------------------------------------------------------------
		public ArticleInfo GetArticleByName(string articleName)
		{
			foreach (ArticleInfo articleInfo in this.Articles)
				if (string.Compare(articleName, articleInfo.Name, true, CultureInfo.InvariantCulture) == 0)
					return articleInfo;
			return null;
		}

//		//---------------------------------------------------------------------
//		public string[] Producers
//		{
//			get
//			{
//				ArrayList al = new ArrayList(Articles.Length);
//				foreach (ArticleInfo article in Articles)
//				{
//					string p = article.Producer;
//					if (p == null || p.Length == 0)
//					{
//						p = article.InternalCode;
//						if (p == null || p.Length == 0)
//							continue;
//					}
//					if (!al.Contains(p))
//						al.Add(p);
//				}
//				return (string[])al.ToArray(typeof(string));
//			}
//		}
		
		//---------------------------------------------------------------------
		public string[] LicensingProducers
		{
			get
			{
				ArrayList al = new ArrayList(Articles.Length);
				foreach (ArticleInfo article in Articles)
					if (article.Licensed && !al.Contains(article.Producer))
						al.Add(article.Producer);
				return (string[])al.ToArray(typeof(string));
			}
		}

		////---------------------------------------------------------------------
		//internal static string GetProductNameFromLicensedPath(string licensedPath, out string exceptionMessage)
		//{
		//    //Siccome il licensed è costituito da nome + licensed extension
		//    exceptionMessage = null;
		//    try
		//    {
		//        if (licensedPath.ToLower(CultureInfo.InvariantCulture).EndsWith(NameSolverStrings.LicensedExtension.ToLower(CultureInfo.InvariantCulture)))
		//            return licensedPath.Substring(0,(licensedPath.Length - NameSolverStrings.LicensedExtension.Length)); 
		//        exceptionMessage = String.Format(CultureInfo.InvariantCulture, Strings.InvalidLicensedPath, Path.GetFileName(licensedPath));
		//        return null;
		//    }
		//    catch (ArgumentOutOfRangeException exc) 
		//    {
		//        exceptionMessage = "GetProductNameFromLicensed: " + exc.Message;
		//        return null;
		//    }
			
		//}

		/// <summary>
		/// inizializza un XmlDocument con root e declaration
		/// </summary>
		/// <param name="currentDocument">documento da inizializzare</param>
		/// <param name="root">nome della root</param>
		//---------------------------------------------------------------------
		public static XmlNode InitDocument(XmlDocument currentDocument, string root)
		{
			if (currentDocument == null || currentDocument.HasChildNodes)
				return null;

			XmlDeclaration declaration = currentDocument.CreateXmlDeclaration(NameSolverStrings.XmlDeclarationVersion, NameSolverStrings.XmlDeclarationEncoding, null);
					
			currentDocument.AppendChild(declaration);
			XmlElement rootNode = currentDocument.CreateElement(root);
			currentDocument.AppendChild(rootNode);
			return rootNode;
		}

		//---------------------------------------------------------------------
		public static string GetKey(XmlElement productEl)
		{
			XmlElement keyNode = productEl.SelectSingleNode(WceElement.ActivationKeys + "/" + WceElement.ActivationKey) as XmlElement;
			if (keyNode == null) return null;
			string activationKey = keyNode.GetAttribute(WceAttribute.Key);
			return activationKey;
		}

		/*//---------------------------------------------------------------------
		public string GetActivationKey()
		{
			if (ActivationKeys == null)
				return string.Empty;
			foreach (ActivationKeyInfo ki in ActivationKeys)
			{
				foreach (string s in UserInfo.InternalCodes)
				{
					if (String.Compare(ki.InternalCode, s, true, CultureInfo.InvariantCulture) == 0)
						return ki.Key;
				}
			}
			return string.Empty;
		}*/

		//---------------------------------------------------------------------
		public DatabaseVersion GetDatabaseVersion()
		{
			//per ogni articolo recupero la lista di serialNumber e 
			//per ogni serial number recupero il database
			//se il database è diverso da undefined lo ritorno
			//altrimenti proseguo. 
			//Se nessun serial esprime un db valido ritornerà undefined
			//Se sono tutti all, tornerò all, altrimenti il primo valido che incontro.
			DatabaseVersion db = DatabaseVersion.Undefined;
			bool all = false;
			bool first = true;
			if (Articles != null && Articles.Length > 0)
			{
				foreach (ArticleInfo ai in Articles)
				{
					if (ai.SerialList != null && ai.SerialList.Count > 0)
					{
						foreach (SerialNumberInfo sni in ai.SerialList)
						{
							db = SerialNumberInfo.GetDatabaseVersion(sni);
							if (first)
							{
								all = (db == DatabaseVersion.All);
								first = false;
							}
							else
								all = all && (db == DatabaseVersion.All);
							if (all) continue;
							if (db != DatabaseVersion.Undefined)
								return db;
						}
					}
				}
			}
			return (all)? DatabaseVersion.All : DatabaseVersion.Undefined;
		}

		//---------------------------------------------------------------------
		public bool HasSerials()
		{
			bool has = false;
			
			foreach (ArticleInfo art in Articles)
			{
				if (!(art == null || !art.HasSerial || art.SerialList == null || art.SerialList.Count <= 0))
				{
					has = true;
					break;
				}
			}

			return has;
		}

		//---------------------------------------------------------------------
		public Edition GetEditionAttribute()
		{
			Edition edition = Edition.Undefined;
			if (Articles != null && Articles.Length > 0)
			{				
				foreach (ArticleInfo ai in Articles)
				{
					try
					{
						edition = (Edition) Enum.Parse(typeof(Edition), ai.Edition, true);
						if (edition != Edition.Undefined) return edition;
					}
					catch (Exception){}
				}
			}
			return edition;
		}

		//---------------------------------------------------------------------
		public Edition GetEdition()
		{
			Edition edition = Edition.Undefined;
			if (!HasSerials())
				return GetEditionAttribute();
			if (Articles != null && Articles.Length > 0)
			{
				foreach (ArticleInfo ai in Articles)
				{
					if (ai.SerialList != null && ai.SerialList.Count > 0)
					{
						foreach (SerialNumberInfo sni in ai.SerialList)
						{
							edition = SerialNumberInfo.GetEdition(sni);
							if (edition != Edition.Undefined)
								return edition;
						}
					}
				}
			}
			return edition;
		}
		
		//---------------------------------------------------------------------
		internal void EvaluateDepExp(ProductInfo[] allProducts)
		{
            List<ModuleDependecies> moduleDepList = new List<ModuleDependecies>();
			foreach (ProductInfo pi in allProducts)
				moduleDepList.AddRange(GetModuleDependenciesList(pi));	

			ArrayList tempList = new ArrayList();
			foreach (ArticleInfo ai in this.Articles)
			{
				if (!ai.Licensed)
					continue;
				EvaluateDepExpOnModule(ai, moduleDepList);
				tempList.AddRange(GetFilteredIncludeModulePaths(ai, moduleDepList));
			}
			this.IncludeModulesPathsFiltered = (IncludePathInfo[])tempList.ToArray(typeof(IncludePathInfo));

		}

		//---------------------------------------------------------------------
		private ArrayList GetFilteredIncludeModulePaths(ArticleInfo ai, List<ModuleDependecies>  moduleDepList)
		{
			ArrayList tempList = new ArrayList();
			foreach (IncludePathInfo ipi in ai.IncludeModulesPaths)
			{
				if (DepExpEvaluator.CheckDependenciesExpression(ipi.DependencyExpression, moduleDepList))
				{
					ipi.DepEvalStatus = DependencyEvaluationStatus.Satisfied;
					tempList.Add(ipi);
				}
				else
					ipi.DepEvalStatus = DependencyEvaluationStatus.NotSatisfied;
			}
			return tempList;
		}

		//---------------------------------------------------------------------
        private void EvaluateDepExpOnModule(ArticleInfo ai, List<ModuleDependecies> moduleDepList)
		{
			foreach (ModuleInfo mi in ai.Modules)
			{
				if (DepExpEvaluator.CheckDependenciesExpression(mi.DependencyExpression, moduleDepList))
					mi.DepEvalStatus = DependencyEvaluationStatus.Satisfied;
				else
					mi.DepEvalStatus = DependencyEvaluationStatus.NotSatisfied;
			}
		}

		//---------------------------------------------------------------------
        private static List<ModuleDependecies> GetModuleDependenciesList(ProductInfo pi)
		{
            List<ModuleDependecies> list = new List<ModuleDependecies>();
			foreach (ArticleInfo a in pi.Articles)
				list.Add(new ModuleDependecies(a.Name, a.LocalizedName, a.Licensed, a.DependencyExpression, a.IncludedSM));
			return list;
		}

		/// <summary>
		/// Parsa un configurationObj in un file di licenza che 
		/// contiene solo l'elenco dei nomi degli articoli licenziati
		/// </summary>
		/// <param name="aConfigurationObject">oggetto da parsare</param>
		//---------------------------------------------------------------------
		public static XmlDocument WriteToLicensed(ProductInfo aConfigurationObject, bool specialVersion, params string[] serialNumberS)
		{
			//bisogna usare le wcestrings
			if (aConfigurationObject == null) return null;
			XmlDocument currentDocument = new XmlDocument();
			XmlElement documentElement = 
				InitDocument(currentDocument, WceElement.Configuration)
				as XmlElement;
			//configuration
			XmlElement productNode = currentDocument.CreateElement(WceElement.Product);
            productNode.SetAttribute(WceAttribute.Name, aConfigurationObject.CompleteName);
			//productNode.SetAttribute(WceAttribute.Release, aConfigurationObject.Release);
			productNode.SetAttribute(WceAttribute.ActivationVersion, aConfigurationObject.ActivationVersion.ToString(CultureInfo.InvariantCulture));
            productNode.SetAttribute(WceAttribute.CompleteName, aConfigurationObject.ProductName);
            List<string> addedSerials = new List<string>();
			foreach (ArticleInfo aArticleObj in aConfigurationObject.Articles)
            {
                bool add = true;
                bool noAddMOdule = false;
                //licensed, name, producer, seriallist
                if (aArticleObj.IsBackModule()) continue;//
                if (aArticleObj.Obsolete) continue;//
                if (!aArticleObj.DefaultDemo && !aArticleObj.BasicServer) noAddMOdule = true ;

                XmlElement articleNode = currentDocument.CreateElement(WceElement.SalesModule);
                articleNode.SetAttribute(WceAttribute.Name, aArticleObj.Name);
                if (aArticleObj.InternalCode != null && aArticleObj.InternalCode.Length > 0)
                    articleNode.SetAttribute(WceAttribute.InternalCode, aArticleObj.InternalCode);
                else
                    articleNode.SetAttribute(WceAttribute.Producer, aArticleObj.Producer);
                bool hasserial = aArticleObj.HasSerial;
                if (!hasserial)
                    articleNode.SetAttribute(WceAttribute.HasSerial, bool.FalseString.ToLower(CultureInfo.InvariantCulture));

                if (serialNumberS == null || serialNumberS.Length==0)
                {
                    serialNumberS = new string[] { };
                    if ((!specialVersion && noAddMOdule) ||
                    (specialVersion && noAddMOdule))
                        //per chiarezza lasciamo l'if così grazie.
                        add = false;

                }

               foreach (string serialNumber in serialNumberS)
               {
                    if (addedSerials.Contains(serialNumber) && aArticleObj.BasicServer)
                        add = false;
                    if ((specialVersion && aArticleObj.BasicServer && !addedSerials.Contains(serialNumber)) ||
                        (!specialVersion && ShortNameCorrespond(serialNumber, aArticleObj.ShortNames)))
                    {
                        XmlNode child = currentDocument.CreateElement(WceElement.Serial);
                        child.InnerText = serialNumber;
                        articleNode.AppendChild(child);
                        addedSerials.Add(serialNumber);//in modo da non inserire più di un seriali alla volta.
                    }

                   if ((!specialVersion && noAddMOdule && !ShortNameCorrespond(serialNumber, aArticleObj.ShortNames) ) ||
                       (specialVersion && noAddMOdule))
                        //per chiarezza lasciamo l'if così grazie.
                       add = false;
                  
               }


                //il modulo lo devo aggiugere
                if (add)
                    productNode.AppendChild(articleNode);
                }
			documentElement.AppendChild(productNode);
			return currentDocument;
		}

		//---------------------------------------------------------------------
		public static bool ShortNameCorrespond(string serialNumber, ArrayList shortnames )
		{
			if (string.IsNullOrEmpty(serialNumber) || shortnames == null || shortnames.Count == 0)
				return false;
			SerialNumber s = new SerialNumber(serialNumber);
			foreach (string sn in shortnames)
				if (String.Compare(sn, s.RawData, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			return false;
		}

		//---------------------------------------------------------------------
		public static XmlDocument WriteToLicensed(ProductInfo aConfigurationObject)
		{
			//bisogna usare le wcestrings
			if (aConfigurationObject == null) return null;
			XmlDocument currentDocument = new XmlDocument();
			XmlElement documentElement =
				InitDocument(currentDocument, WceElement.Configuration)
				as XmlElement;
			//configuration
			XmlElement productNode = currentDocument.CreateElement(WceElement.Product);
            productNode.SetAttribute(WceAttribute.Name, aConfigurationObject.CompleteName);
			//productNode.SetAttribute(WceAttribute.Release, aConfigurationObject.Release);
			productNode.SetAttribute(WceAttribute.ActivationVersion, aConfigurationObject.ActivationVersion.ToString(CultureInfo.InvariantCulture));
            productNode.SetAttribute(WceAttribute.CompleteName, aConfigurationObject.ProductName);
            //write key
            if (!String.IsNullOrWhiteSpace(aConfigurationObject.ActivationKey))
            {
                XmlElement ks = currentDocument.CreateElement(WceStrings.Element.ActivationKeys);
                XmlElement k = currentDocument.CreateElement(WceStrings.Element.ActivationKey);
                k.SetAttribute(WceStrings.Attribute.Key, aConfigurationObject.ActivationKey);
                k.SetAttribute(WceStrings.Attribute.InternalCode, "0110G081");
                ks.AppendChild(k);
                productNode.AppendChild(ks);
            }

			foreach (ArticleInfo aArticleObj in aConfigurationObject.Articles)
			{

				//licensed, name, producer, seriallist
				
				if (!aArticleObj.Licensed) continue;//non licenziato
				if (aArticleObj.IsBackModule()) continue;//
				if (aArticleObj.Obsolete) continue;//
                if (!aArticleObj.DefaultDemo && !aArticleObj.BasicServer && ((aArticleObj.SerialList == null || aArticleObj.SerialList.Count == 0) && aArticleObj.HasSerial)) continue;
				
                
                XmlElement articleNode = currentDocument.CreateElement(WceElement.SalesModule);
				articleNode.SetAttribute(WceAttribute.Name, aArticleObj.Name);
				if (aArticleObj.InternalCode != null && aArticleObj.InternalCode.Length > 0)
					articleNode.SetAttribute(WceAttribute.InternalCode, aArticleObj.InternalCode);
				else
					articleNode.SetAttribute(WceAttribute.Producer, aArticleObj.Producer);
				bool hasserial = aArticleObj.HasSerial;
				if (!hasserial)
					articleNode.SetAttribute(WceAttribute.HasSerial, bool.FalseString.ToLower(CultureInfo.InvariantCulture));
				
				if (aArticleObj.SerialList != null)
				{
					foreach (SerialNumberInfo s in aArticleObj.SerialList)
					{
						XmlNode child	= currentDocument.CreateElement(WceElement.Serial);
						child.InnerText = s.GetSerialWOSeparator();
						if (s.PK != null && s.PK.Trim().Length > 0)
							((XmlElement)child).SetAttribute(WceAttribute.ProducerKey, s.PK);
						articleNode.AppendChild(child);
					}
				}
				if (aArticleObj.PKs != null)
				{
					foreach (string s in aArticleObj.PKs)
					{
						XmlNode child	= currentDocument.CreateElement(WceElement.ProducerKey);
						child.InnerText = s;
						articleNode.AppendChild(child);
					}
				}
				productNode.AppendChild(articleNode);
			}
			documentElement.AppendChild(productNode);
			return currentDocument;
		}

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			ProductInfo pInfo = obj as ProductInfo;
			if (pInfo == null)
				return 1; // This instance is greater than obj
            return string.Compare(this.CompleteName, pInfo.CompleteName, true, CultureInfo.InvariantCulture);
		}

		#endregion

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			ProductInfo pInfo = obj as ProductInfo;
			if (pInfo == null)
				return false;
            return string.Compare(this.CompleteName, pInfo.CompleteName, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
            return (this.CompleteName + "," + this.ActivationVersion).ToLower(CultureInfo.InvariantCulture).GetHashCode();
		}


        //---------------------------------------------------------------------
        internal string GetAcceptDemo()
        {
            foreach (ArticleInfo ai in Articles)
            {
                if (String.IsNullOrEmpty(ai.AcceptDEMO)) continue;
                return ai.AcceptDEMO;
            }
            return ProductId;
        }
    }
}
