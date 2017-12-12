using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using WceAttribute = Microarea.TaskBuilderNet.Licence.Activation.WceStrings.Attribute;
using WceElement = Microarea.TaskBuilderNet.Licence.Activation.WceStrings.Element;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	/// <summary>
	/// Gestisce le operazioni di esportazione della configurazione.
	/// </summary>
	//=========================================================================
	public class ExportManager
	{
		private		string			userInfoPath	= null;
		private		FileInfo[]		licensedPaths	= null;
		protected		XmlDocument		wceDoc			= null;
		private		bool			userInfoComplete;
		private		bool			agreementComplete;
		private		IBasePathFinder		pathFinder;
		private		Diagnostic diagnostic;

		public string	UserInfoPath		{ get {return userInfoPath;}	set {userInfoPath	= value;}}
		public FileInfo[]	LicensedPaths	{ get {return licensedPaths;}	set {licensedPaths	= value;}}
		public bool		UserInfoComplete	{ get {return userInfoComplete;}}
		public bool		AgreementComplete	{ get {return agreementComplete;}}

		//---------------------------------------------------------------------
		public ExportManager(Diagnostic diagnostic, IBasePathFinder pathFinder)
		{
			this.pathFinder		 = pathFinder;
			this.diagnostic		 = diagnostic;
			if (pathFinder != null)
			{
				UserInfoPath = pathFinder.GetUserInfoFile();
				LicensedPaths = GetAllLicensed();
			}
		}

		//---------------------------------------------------------------------
		private FileInfo[] GetAllLicensed()
		{	//devo prendere tutti i licensed che hanno una corrispettiva solution, 
			//per evitare che vengano considerati anche file cadavere.
			try
			{
				FileInfo[] allLicensed = null;
				if (pathFinder == null) return null;
				allLicensed = pathFinder.GetLicensedFiles();
				if (allLicensed == null || allLicensed.Length ==0)
				{
					Debug.WriteLine("ExportManager.GetAllLicensed - Warning: It is impossible to find the *.Licensed.config files.");
					SetDiagnostic(DiagnosticType.Warning, "It is impossible to find the *.Licensed.config files.");
					return null;
				}
				FileInfo[] allSolutions = pathFinder.GetSolutionFiles();
				if (allSolutions == null || allSolutions.Length ==0)
				{
					Debug.WriteLine("ExportManager.GetAllLicensed - Warning: It is impossible to find a VALID *.Licensed.config files.");
					SetDiagnostic(DiagnosticType.Warning, "It is impossible to find a VALID *.Licensed.config files.");
					return null;
				}
				ArrayList validLicensed = new ArrayList();
				char splitter = '.';
				foreach (FileInfo fis in allSolutions)
				{
					string names = fis.Name.Split(splitter)[0];
					foreach (FileInfo fil in allLicensed)
					{
						string namel = fil.Name.Split(splitter)[0];
						if (String.Compare(names, namel, true, CultureInfo.InvariantCulture) == 0)
						{
							validLicensed.Add(fil);
							break;
						}
					}
				}
				return (FileInfo[])validLicensed.ToArray(typeof(FileInfo));
			}
			catch (Exception exc)
			{
				string error = String.Concat(LicenceStrings.LicensedsSearchError, " ", LicenceStrings.ExceptionMessage, exc.Message);
				SetDiagnostic(DiagnosticType.Error, error);
				Debug.WriteLine("ActivationManager.GetAllLicensed: "+ exc.Message);
				return null;
			}
		}

		//---------------------------------------------------------------------
		private void SetDiagnostic(DiagnosticType type, string message)
		{
			if (diagnostic == null || message == null || message.Length == 0)
				return;
			diagnostic.Set(DiagnosticType.LogInfo | type, message);
		}

		//---------------------------------------------------------------------
		public string GetExportWceString(string installation, string product, string licensedConfigurationXml)
		{
			CreateDocument(installation, product, GetNodeFromXml(licensedConfigurationXml));
			return wceDoc.OuterXml;
		}

		//---------------------------------------------------------------------
		private XmlNode GetNodeFromXml(string licensedConfigurationXml)
		{
			XmlNode licensedConfigurationNode = null;
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(licensedConfigurationXml);
				licensedConfigurationNode = doc.DocumentElement;
			}
			catch (Exception exc)
			{
				Debug.WriteLine("ImportExportManager.GetNodeFromXml: impossibile generare un XmlNode a partire dalla stringa passata. " + exc.Message);
				return null;
			}
			return licensedConfigurationNode;
		}

		//---------------------------------------------------------------------
		public string GetInstallationWce(string installation, out string key, out string exceptionMessage)
		{
			return GetExportWceStringAndActivation(installation, null, out key, out exceptionMessage);
		}
		
		//---------------------------------------------------------------------
		public XmlNode GetInstallationWceNode(string installation, out string key, out string exceptionMessage)
		{
			XmlDocument wceDoc = GetExportWceAndActivation(installation, null, out key, out exceptionMessage);
			return wceDoc.DocumentElement;
		}

		//---------------------------------------------------------------------
		private XmlDocument GetExportWceAndActivation(string installation, string product, out string key, out string exceptionMessage)
		{
			key = String.Empty;
			exceptionMessage = null;
			try
			{
				CreateDocument(installation, product, null);
				return wceDoc;
			}
			catch (Exception exc)
			{
				key				 = String.Empty;
				exceptionMessage = "(ExportManager) " + exc.Message;
				return null;			
			}
		}

		//---------------------------------------------------------------------
		internal string GetExportWceStringAndActivation(string installation, string product, out string key, out string exceptionMessage)
		{
			XmlDocument wceDoc = GetExportWceAndActivation(installation, product, out key, out exceptionMessage);
			return wceDoc.OuterXml;
		}

		//---------------------------------------------------------------------
		private void CreateDocument(string installation, string product, XmlNode licensedPortion)
		{	
			wceDoc = new XmlDocument();
			AppendDocumentElement(installation, product);
			CreateUserInfoPortion();
			CreateLicensedPortion(licensedPortion);
		}

		//---------------------------------------------------------------------
		private void AppendDocumentElement(string installation, string product)
		{
			XmlElement root = wceDoc.CreateElement(WceElement.Export);
			root.SetAttribute(WceAttribute.Installation, installation);
			root.SetAttribute(WceAttribute.Product, product);
			wceDoc.AppendChild(root);
		}

		//---------------------------------------------------------------------
		public string GetExportWceString(XmlDocument user, XmlDocument lic)
		{
			wceDoc = new XmlDocument();
			AppendDocumentElement("3.0", "");
			wceDoc.DocumentElement.AppendChild(GetUserInfoPortion(user));
			wceDoc.DocumentElement.AppendChild(GetLicensedPortion(lic));
			return wceDoc.DocumentElement.OuterXml;
		}
		#region USERINFO
		//---------------------------------------------------------------------
		private XmlNode GetUserInfoPortion()
		{
			//lo user info viene caricato in xml automaticamente poichè è uno state.
			//per cui non ho le stringhe dei suoi tag, accedo con i metodi di xmldoc.
			XmlNode portion = null;
			try
			{
				XmlDocument doc = new XmlDocument();
                if (!File.Exists(UserInfoPath)) return null;
				doc.Load(UserInfoPath);
				portion = doc.DocumentElement;//SelectSingleNode("//UserInfo");
				string message;
				UserInfo ui = UserInfo.GetFromXml(UserInfoPath, out message);
				if (message != null && message.Length > 0)
					SetDiagnostic(DiagnosticType.Error, String.Format(CultureInfo.InvariantCulture, "File {0} not loadable: {1}", UserInfoPath, message));
				userInfoComplete = ui.PersonalDataComplete();
				agreementComplete = ui.AgreementComplete();
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Unable to load UserInfo file: " + exc.Message);
				SetDiagnostic(DiagnosticType.Error, String.Format(CultureInfo.InvariantCulture, "File {0} not loadable: {1}", UserInfoPath, exc.Message));
				return null;
			}
			return wceDoc.ImportNode(portion, true);
		}


		//---------------------------------------------------------------------
		private XmlNode GetUserInfoPortion(XmlDocument userDoc)
		{
			if (userDoc == null) return null;	
			XmlNode portion = userDoc.DocumentElement;//SelectSingleNode("//UserInfo");
			XmlNode n =  wceDoc.ImportNode(portion, true);
			try
			{
				XmlElement header = wceDoc.CreateElement(WceElement.UserInfoFile);
				header.SetAttribute(WceAttribute.Name, "UserInfo.config");
				header.AppendChild(n);
				return header;
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Problemi nella creazione della parte 'UserInfo'. " + exc.Message);
				return null;
			}
		}
		//---------------------------------------------------------------------
		private void CreateUserInfoPortion()
		{
			if (wceDoc == null) return;
			try
			{
				XmlElement header = wceDoc.CreateElement(WceElement.UserInfoFile);
				header.SetAttribute(WceAttribute.Name, Path.GetFileName(UserInfoPath));
                XmlNode userinfoportion = GetUserInfoPortion();
                if (userinfoportion != null)
                    header.AppendChild(userinfoportion);
				wceDoc.DocumentElement.AppendChild(header);
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Problemi nella creazione della parte 'UserInfo'. " + exc.Message);
				return;
			}
		}
	
		#endregion

		#region LICENSED
		//---------------------------------------------------------------------
		private XmlNode GetLicensedPortion(string licensed)
		{
			if (string.IsNullOrEmpty(licensed)) return null;
			XmlNode portion = null;
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(licensed);
				VerifyReleaseNumber(ref doc);
				portion = doc.SelectSingleNode(WceElement.Configuration);
                SwapSignature(ref portion);
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Licensed file non caricato: " + exc.Message);
				SetDiagnostic(DiagnosticType.Error, "Unable to load file: "+ licensed + " due to the following reason: " + exc.Message);
				return null;
			}
			
			return wceDoc.ImportNode(portion, true);
		}

        //---------------------------------------------------------------------
        private void SwapSignature(ref XmlNode portion)
        {
            //se presente un attributo completename lo swappo con l'attributo name
            //cosi` al backend attivazione dentro l 'attributo name gli arriva la signature come per le versioni precedentio
            //( adesso col fatto che abbiamo una unica signature per tutte le edition il nome del licensed non è piu`aderente alla signature di PAI)
            string completename = null;
            XmlNode n = portion.SelectSingleNode("/Configuration/Product");
            if (n == null) return;
            XmlNode cn  = n.SelectSingleNode("@completename");
            if (cn == null) return;
          
            completename = cn.Value;
            if (String.IsNullOrWhiteSpace(completename)) return;
            string name = n.SelectSingleNode("@name").Value;
            if (String.Compare(name, completename, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                ((XmlElement)n).SetAttribute("name", completename);
                ((XmlElement)n).SetAttribute("completename", name);
            }

        }

		//---------------------------------------------------------------------
		private XmlNode GetLicensedPortion(XmlDocument licDoc)
		{
			if (licDoc == null) return null;
			XmlNode portion = licDoc.SelectSingleNode(WceElement.Configuration);
			XmlNode licensedtoinsert= wceDoc.ImportNode(portion, true);
			string name = licensedtoinsert.SelectSingleNode("/Product/@name").Value;
			XmlNode header = null; 
			try
			{
				 header = wceDoc.CreateElement(WceElement.LicensedFiles);
				/*foreach (FileInfo licensed in LicensedPaths)
				{*/
					XmlElement underHeader = wceDoc.CreateElement(WceElement.LicensedFile);
					underHeader.SetAttribute(WceAttribute.Name, String.Format("{0}.Licensed.config",name));
					underHeader.AppendChild(licensedtoinsert);
					header.AppendChild(underHeader);
				/*}*/
					
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Problemi nella creazione della parte 'Licensed'. " + exc.Message);
				
			}
			return header;			
		}

		//---------------------------------------------------------------------
		private void VerifyReleaseNumber(ref XmlDocument licensed)
		{
			XmlNodeList nodes = licensed.SelectNodes("Configuration/Product");
			foreach (XmlElement licnode in nodes)
			{

				string name = licnode.GetAttribute(Activation.WceStrings.Attribute.Name);
				if (String.IsNullOrEmpty(name)) return;
                string rel = BasePathFinder.BasePathFinderInstance.GetInstallationVersionFromInstallationVer(name);
				if (!String.IsNullOrEmpty(rel))
					licnode.SetAttribute(Activation.WceStrings.Attribute.Release, rel);
			}
		}

		//---------------------------------------------------------------------
		private void CreateLicensedPortion(XmlNode licensedPortion)
		{
			try
			{
				XmlElement header = wceDoc.CreateElement(WceElement.LicensedFiles);
				foreach (FileInfo licensed in LicensedPaths)
				{
					XmlElement underHeader = wceDoc.CreateElement(WceElement.LicensedFile);
					underHeader.SetAttribute(WceAttribute.Name, licensed.Name);
					if (licensedPortion == null)
						underHeader.AppendChild(GetLicensedPortion(licensed.FullName));
					else
						underHeader.AppendChild(wceDoc.ImportNode(licensedPortion, true));
					header.AppendChild(underHeader);
				}
				wceDoc.DocumentElement.AppendChild(header);
			}
			catch (Exception exc)
			{
				Debug.WriteLine("Problemi nella creazione della parte 'Licensed'. " + exc.Message);
				return;
			}
		}		
		#endregion

	}
	//a volte ritornano, liberamente ispirato alla "fu importazione di wce"
	//=========================================================================
	public static class ImportManager  
	{/*
		//---------------------------------------------------------------------
		public static void SaveUserInfo(XmlNode wce, string path)
		{
			if (wce == null)
				return;
			try
			{
				DocumentInfo di = ExtrapolateUserInfo(wce);
				if (di == null) return;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				//se tolgo il readonly dovrei anche rimetterlo! todo
				string fullpath = Path.Combine(path, di.Name);
				if (File.Exists(fullpath) && ((File.GetAttributes(fullpath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
					File.SetAttributes(fullpath, File.GetAttributes(fullpath) & ~FileAttributes.ReadOnly);
				di.Doc.Save(fullpath);

			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
			}
		}

//---------------------------------------------------------------------
		public static void SaveUserInfo(XmlDocument doc, string path)
		{
			if (doc == null || string.IsNullOrEmpty(path))
				return;
			try
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				//se tolgo il readonly dovrei anche rimetterlo! todo
				string fullpath = Path.Combine(path, "UserInfo.config");
				if (File.Exists(fullpath) && ((File.GetAttributes(fullpath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
					File.SetAttributes(fullpath, File.GetAttributes(fullpath) & ~FileAttributes.ReadOnly);
				doc.Save(fullpath);

			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
			}
		}*/

	/*	//---------------------------------------------------------------------
		public static void SaveWCE(string wce, string path)
		{
			if (String.IsNullOrEmpty(wce) || String.IsNullOrEmpty(path))
				return ;
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(wce);
			SaveWCE(doc.DocumentElement, path);

		}*/

		//---------------------------------------------------------------------
		public static void SaveWCE(XmlNode wce, string path, Diagnostic diagnostic)
		{
			if (wce == null) 
				return;
			try
			{
				IList <DocumentInfo> list = GetWCEPortions(wce);
				if (list == null) return;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				foreach (DocumentInfo di in list)
				{//se tolgo il readonly dovrei anche rimetterlo! ? todo
					string fullpath = Path.Combine(path, di.Name);
					if (File.Exists(fullpath) && ((File.GetAttributes(fullpath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
						File.SetAttributes(fullpath, File.GetAttributes(fullpath) & ~FileAttributes.ReadOnly);
					SwapAndSave(di.Doc,fullpath);
                    if (diagnostic != null)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, fullpath + " saved");
                    }
				}
			}
			catch (Exception exc)
			{
                if (diagnostic != null)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Error saving wce: " + exc.ToString());
                }
			}
		}
       


        //---------------------------------------------------------------------
        private static void SwapAndSave(XmlDocument xmlDocument, string fullpath)
        {
           //se presente un attributo completename lo swappo con l'attributo name
            //cosi` al backend attivazione dentro l 'attributo name gli arriva la signature come per le versioni precedentio
            //( adesso col fatto che abbiamo una unica signature per tutte le edition il nome del licensed non è piu`aderente alla signature di PAI)
            string completename = null;
            XmlNode n = xmlDocument.SelectSingleNode("/Configuration/Product");
            if (n == null) {xmlDocument.Save(fullpath); return;}
            XmlNode cn = n.SelectSingleNode("@completename");
            if (cn == null) { xmlDocument.Save(fullpath); return; }

            completename = cn.Value;
            if (String.IsNullOrWhiteSpace(completename)) { xmlDocument.Save(fullpath); return; }
            string name = n.SelectSingleNode("@name").Value;
            if (String.Compare(name, completename, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                ((XmlElement)n).SetAttribute("name", completename);
                ((XmlElement)n).SetAttribute("completename", name);
            }

            xmlDocument.Save(fullpath);
        }

		//---------------------------------------------------------------------
		private static IList<DocumentInfo> GetWCEPortions(XmlNode wce)
		{
			if (wce == null) 
				return null;
			IList <DocumentInfo> allDocs =  ExtrapolateLicenseds(wce);
			DocumentInfo docInfo = ExtrapolateUserInfo(wce);
			if(docInfo!= null)	
				allDocs.Add(docInfo);
			return allDocs;
		}

		//---------------------------------------------------------------------
		private static DocumentInfo GetDocumentInfo(XmlElement n)
		{
			if (n == null) 
				return null;
			
			string name = n.GetAttribute(WceAttribute.Name);
			XmlNode node = n.FirstChild;
			if (string.IsNullOrEmpty(name) || node == null)
				return null;

			XmlDocument doc = new XmlDocument();
			XmlNode import = doc.ImportNode(node, true);
			doc.AppendChild(import);
			try
			{
				return new DocumentInfo(name, doc);
			}
			catch 
			{
				return null;
			}
		}

		//---------------------------------------------------------------------
		private static DocumentInfo ExtrapolateUserInfo(XmlNode wce)
		{
			if (wce == null) 
				return null;
			XmlElement n = wce.SelectSingleNode("//" + WceElement.UserInfoFile) as XmlElement;
			return GetDocumentInfo(n as XmlElement);
		}

		
		//---------------------------------------------------------------------
		private static IList<DocumentInfo> ExtrapolateLicenseds(XmlNode wce)
		{
			if (wce == null) 
				return null;
			IList<DocumentInfo> list = new List<DocumentInfo>();
			XmlNodeList licenseds = wce.SelectNodes("//" + WceElement.LicensedFile)  ;
			foreach (XmlElement n in licenseds)
			{
				DocumentInfo docInfo = GetDocumentInfo(n);
				if (docInfo != null)
					list.Add(docInfo);
			}
			return list;
		}

	}

	//=========================================================================
	public class DocumentInfo 
	{
		public  string Name;
		public XmlDocument Doc;

		//---------------------------------------------------------------------
		public DocumentInfo(string name, XmlDocument doc)
		{
			if (string.IsNullOrEmpty(name) || doc == null)
				throw new ArgumentNullException();
			this.Name = name;
			this.Doc = doc;
		}		
	}


}

