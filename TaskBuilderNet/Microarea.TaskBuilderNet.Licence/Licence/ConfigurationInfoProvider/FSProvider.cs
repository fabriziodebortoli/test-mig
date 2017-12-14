using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider
{
	/// <summary>
	/// Summary description for FSConfigurationInfoProviderBase.
	/// </summary>
	public abstract class FSProvider : IConfigurationInfoProvider
	{
		private bool addFunctional;
		protected IBasePathFinder pathFinder;
		private Hashtable licenseds;
		private Hashtable solutions;

		private Diagnostic diagnostic = new Diagnostic("FsProvider");
		protected Diagnostic Diagnostic
		{
			get { return this.diagnostic; }
		}


		//---------------------------------------------------------------------
		protected FSProvider(IBasePathFinder pathFinder)
		{
			this.pathFinder = pathFinder;

			this.solutions = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		}
	
		//---------------------------------------------------------------------
		protected virtual void SetDiagnostic(DiagnosticType type, string message)
		{
			if (diagnostic == null || message == null || message.Length == 0)
				return;
			diagnostic.Set(DiagnosticType.LogInfo | type, message);
		}

		/// <summary>
		/// Legge il file dell'articolo.
		/// </summary>
		/// <param name="addFunctional">true aggiunge mappatura funzionale</param>
		//---------------------------------------------------------------------
		private XmlDocument GetArticleDocument(string product, string article)
		{
			string fileName = string.Concat(article, NameSolverStrings.CsmExtension);
			string solutionsModulesPath = pathFinder.GetSolutionsModulesPath(product);

            if (String.IsNullOrEmpty(solutionsModulesPath))
            {
                SetDiagnostic(DiagnosticType.Warning, String.Format("Product {0} is not loadable, but its Licensed exists.", product));
                return null;
            }

			fileName = Path.Combine(solutionsModulesPath, fileName);
			//Non permetto piu la lettura degli xml
			/*if (!File.Exists(fileName) && readXml)
			{
				fileName = Path.ChangeExtension(fileName, NameSolverStrings.XmlExtension);
				if (!File.Exists(fileName))
					return null;
			}*/

			//LETTURA DEL FILE CON LOCALIZABLEXMLDOCUMENT
			string dictionaryPath = Path.Combine(new DirectoryInfo(solutionsModulesPath).Parent.FullName, NameSolverStrings.Dictionary);
			LocalizableXmlDocument doc = new LocalizableXmlDocument(dictionaryPath, pathFinder);
			if (!LoadLocalizableXmlDocument(doc, fileName, product))
				return null;
			return doc;
		}
		//---------------------------------------------------------------------
		private bool LoadLocalizableXmlDocument(LocalizableXmlDocument doc, string fileName, string productname)
		{
			try
			{
				if (ActivationObjectHelper.HasOtherExtension(fileName))
				{
					ActivationObjectHelper aoh	= new ActivationObjectHelper();
					string articleString		= aoh.GetArticleStringByPath(fileName, productname);
					if (articleString == null || articleString == String.Empty)
						return false;
					doc.LoadXml(articleString, fileName);
				}
				else
				{
					doc.Load(fileName);
				}
				
			}
			catch (Exception exc)
			{
				Debug.WriteLine("FSProvider.LoadLocalizableXmlDocument - Error: " + exc.Message);
				string error = String.Format(CultureInfo.InvariantCulture, LicenceStrings.ErrorLoading, fileName);
				string message = String.Concat(error, " ", LicenceStrings.ExceptionMessage, exc.Message);
				SetDiagnostic(DiagnosticType.Error, message);
				return false;
			}
			return true;
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
			FileInfo[] solFiles = pathFinder.GetSolutionFiles();
			FileInfo[] licFiles = pathFinder.GetLicensedFiles();
			ArrayList list = new ArrayList();
			if (licFiles != null)
				foreach (FileInfo f in licFiles)
				{
					string key = f.Name.Split('.')[0];
					if (!list.Contains(key))
						list.Add(key);
				}
			if (solFiles != null)
				foreach (FileInfo f in solFiles)
				{
					string key = f.Name.Split('.')[0];
					if (!list.Contains(key))
						list.Add(key);
				}

			return (string[])list.ToArray(typeof(string));

		}

		//---------------------------------------------------------------------
		protected virtual FileInfo[] GetLicensedFiles()
		{
			return this.pathFinder.GetLicensedFiles();
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

            FileInfo[] prodFiles = GetLicensedFiles();
            string xPathMask = "Product[@name='{0}']";

            Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            if (prodFiles != null)
                foreach (FileInfo prodFile in prodFiles)
                {
                    if (!prodFile.Exists)
                        continue;
                    XmlDocument prodDoc = new XmlDocument();
                    try
                    {
                        string prodName = prodFile.Name.Split('.')[0];
                        prodDoc.Load(prodFile.FullName);
                        string xPath = string.Format(CultureInfo.InvariantCulture, xPathMask, prodName);
                        XmlElement prodEl = PseudoXPath.SelectSingleElement(prodDoc.DocumentElement, xPath);
                        ht[prodName] = prodEl;
                    }
                    catch (Exception exc)
                    {
                        string error = String.Format(CultureInfo.InvariantCulture, LicenceStrings.ErrorLoading, prodFile.FullName);
                        string message = String.Concat(error, " ", LicenceStrings.ExceptionMessage, exc.Message);
                        SetDiagnostic(DiagnosticType.Error, message);
                        Debug.WriteLine(message);
                        continue;
                    }
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
        Hashtable IConfigurationInfoProvider.GetArticles(string product, Hashtable ht)
		{
			if (this.licenseds == null)
				GetLicensedProducts(); // builds the hashtable

         

			XmlElement solEl = ((IConfigurationInfoProvider)this).GetProductSolution(product);
			if (solEl == null) // try using the licensed.config
				solEl = licenseds[product] as XmlElement;
			if (solEl == null)
				return ht;
			foreach (XmlElement artEl in solEl.GetElementsByTagName(WceStrings.Element.SalesModule))
			{
				string artName = artEl.GetAttribute(WceStrings.Attribute.Name);
				string artDemo = artEl.GetAttribute(WceStrings.Attribute.Demo);
				string artBasicServer = artEl.GetAttribute(WceStrings.Attribute.BasicServer);
				string artObsolete = artEl.GetAttribute(WceStrings.Attribute.Obsolete);
				XmlDocument artDoc = GetArticleDocument(product, artName);
				if (artDoc == null)
					continue;
				SolutionArticleProperties props = new SolutionArticleProperties();
				//default true, false solo se espresso esplicitamente
				props.DefaultDemo = !(String.Compare(bool.FalseString, artDemo, true, CultureInfo.InvariantCulture) == 0);
				props.Obsolete = (String.Compare(bool.TrueString, artObsolete, true, CultureInfo.InvariantCulture) == 0);
				props.BasicServer = (String.Compare(bool.TrueString, artBasicServer, true, CultureInfo.InvariantCulture) == 0);
				ht[artName] = new SolutionArticle(artDoc, props);
			}

			return ht;
		}

		//---------------------------------------------------------------------
		string IConfigurationInfoProvider.GetCountry()
		{
//			string userInfoFile = pathFinder.GetUserInfoFile();
//			if (!File.Exists(userInfoFile))
//				return string.Empty;
			UserInfo userInfo = ((IConfigurationInfoProvider)this).GetUserInfo();
			if (userInfo == null)
				return string.Empty;
			return userInfo.Country;
		}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.ArticlesLicensedByDefault { get { return false; } }

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.Xml.XmlException"></exception>
		/// <param name="product"></param>
		/// <returns></returns>

		//---------------------------------------------------------------------
		string IConfigurationInfoProvider.GetApplication(string product)
		{
			string path = pathFinder.GetSolutionFile(product);
			path = Path.GetDirectoryName(path);
			path = Path.GetDirectoryName(path);
			return Path.GetFileName(path);
		}

		//---------------------------------------------------------------------
		XmlElement IConfigurationInfoProvider.GetProductSolution(string product)
		{
			if (this.solutions.Contains(product))
				return solutions[product] as XmlElement;

			return LoadFileDocumentElement(product, pathFinder.GetSolutionFile(product));
		}

		//---------------------------------------------------------------------
		protected XmlElement LoadFileDocumentElement(string product, string filePath)
		{
			if (filePath == null || filePath.Length == 0 || !File.Exists(filePath))
				return null;

			XmlDocument doc = new XmlDocument();
            
			try
			{
                 //LETTURA DEL FILE CON LOCALIZABLEXMLDOCUMENT
                string dictionaryPath = Path.Combine(new DirectoryInfo(filePath).Parent.FullName, NameSolverStrings.Dictionary);
                LocalizableXmlDocument docx = new LocalizableXmlDocument(dictionaryPath, pathFinder);
                if (!LoadLocalizableXmlDocument(docx, filePath, product))
                {
                    //se fallisce provo a fare load senza dizionari
                    XmlDocument doc1 = new XmlDocument();
                    doc1.Load(filePath);
                    solutions[product] = doc1.DocumentElement;
                    return doc1.DocumentElement;
                }
                return docx.DocumentElement;
            }
			catch (Exception exc)
			{
				Debug.WriteLine("FSProvider.LoadFileDocumentElement - Error: " + exc.Message);

				string message =
					string.Concat
					(
						string.Format(CultureInfo.InvariantCulture, LicenceStrings.ErrorLoading, filePath),
						" ",
						LicenceStrings.ExceptionMessage,
						exc.Message
					);

				SetDiagnostic(DiagnosticType.Error, message);
			}

            return null;
        }

		//---------------------------------------------------------------------
		UserInfo IConfigurationInfoProvider.GetUserInfo()
		{
			string userInfoPath = pathFinder.GetUserInfoFile();
			if (!File.Exists(userInfoPath))
			{
				//SetDiagnostic(DiagnosticType.Error, String.Format(CultureInfo.InvariantCulture, "File {0} is missing.", userInfoPath));
				return null;
			}
			string message;
			UserInfo ui =  UserInfo.GetFromXml(userInfoPath, out message);
			if (message != null && message.Length > 0)
			{
				SetDiagnostic(DiagnosticType.Error, String.Format(CultureInfo.InvariantCulture, "File {0} not loadable: {1}", userInfoPath, message));
				return null;
			}
			return ui;
		}

		//---------------------------------------------------------------------
		void IConfigurationInfoProvider.InvalidateCaches()
		{
			this.licenseds = null;
			this.solutions.Clear();
		}

		//---------------------------------------------------------------------
		IBasePathFinder IConfigurationInfoProvider.GetPathFinder()
		{
			return this.pathFinder;
		}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.FilterByCountry { get { return true; } }

		#endregion
	}

	//=========================================================================
	[Serializable]
    public class SolutionArticle
	{
		public XmlDocument XmlDoc;
		public SolutionArticleProperties Properties;
		
		//---------------------------------------------------------------------
		public SolutionArticle(XmlDocument doc, SolutionArticleProperties props) 
		{
			XmlDoc = doc;
			Properties = props ; 
		}
	}

	//=========================================================================
	public class SolutionArticleProperties
	{
		public bool DefaultDemo = true;
		public bool Obsolete = false;
		public bool BasicServer = false;
	}
}
