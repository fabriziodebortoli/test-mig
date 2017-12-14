using System;
using System.Collections;
using System.Xml;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for XTechParser.
	/// </summary>
	public class XTechParser : SolutionManagerItems
	{
		private XmlNode nModule = null;

		public XTechParser()
		{
			defaultLookUpType = LookUpFileType.XTech;
		}

		public override string ToString()
		{
			return "XTech DataUrl/Name parser";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(true);

			nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", mi.Name));

				nModule = CreaNodoModule(mi.Name, true, false);

				ParseXTech(mi);
			}

			EndRun(true);
		}

		//---------------------------------------------------------------------------
		private void ParseXTech(BaseModuleInfo mi)
		{
			string[] xdocs = mi.XTechDocuments;
			foreach (string xdocPath in xdocs)
			{
				DirectoryInfo di = new DirectoryInfo(xdocPath);
				string xdocName = di.Name;
				
				string xDescriptionFolder		= Path.Combine(xdocPath,		"Description");
				
				string ExternalReferencesFile	= Path.Combine(xDescriptionFolder,	"ExternalReferences.xml");
				ParseNamespace(
					ExternalReferencesFile, 
					"MainExternalReferences/DBT/ExternalReferences/ExternalReference/DataUrl");

				ExternalReferencesFile	= Path.Combine(xDescriptionFolder,	"Document.xml");
				ParseNamespace(
					ExternalReferencesFile, 
					"//DataUrl");

				string xProfilesFolder			= Path.Combine(xdocPath,		"ExportProfiles");
				if (Directory.Exists(xProfilesFolder))
				{
					string[] prifilesPaths = Directory.GetDirectories(xProfilesFolder);
					foreach (string xProfilePath in prifilesPaths)
					{
						ExternalReferencesFile	= Path.Combine(xProfilePath,	"ExternalReferences.xml");
						ParseNamespace(
							ExternalReferencesFile, 
							"MainExternalReferences/DBT/ExternalReferences/ExternalReference/ProfileName");
					}
				}
			}
		}

		//---------------------------------------------------------------------------
		private void ParseNamespace(string fileName, string xPathQuery)
		{
			if (!File.Exists(fileName))
				return;
			
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(fileName);
			}
			catch(Exception exc)
			{
				SetLogError(exc.Message, ToString());
				return;
			}

			XmlNodeList nodeList = doc.SelectNodes(xPathQuery);
			if (nodeList.Count == 0)
				return;

			foreach (XmlNode el in nodeList)
			{
				string tmp = el.InnerText;
				if (tmp.EndsWith(".xml") && xPathQuery.EndsWith("DataUrl"))
					tmp = tmp.Replace(".xml", "");

				CreaNodoLookUp(nModule, tmp, string.Empty, nModule.Attributes["source"].Value.ToString());
			}
		}
	}
}
