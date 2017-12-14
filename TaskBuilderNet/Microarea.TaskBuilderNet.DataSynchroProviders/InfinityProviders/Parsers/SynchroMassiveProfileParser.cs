using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
	///<summary>
	/// Parser dei file dei profili di sincronizzazione
	///</summary>
	//================================================================================
    internal class SynchroMassiveProfileParser 
	{
		private SynchroProfileInfo syncProfileInfo = new SynchroProfileInfo();
		private SynchroProfileInfo namespaceMapInfo = new SynchroProfileInfo(); // ad-hoc per il mapping dei namespace del DMS

		//--------------------------------------------------------------------------------
		public SynchroProfileInfo SynchroProfileInfo { get { return syncProfileInfo; } }
		public SynchroProfileInfo NamespaceMapInfo { get { return namespaceMapInfo; } }

		//--------------------------------------------------------------------------------
        public SynchroMassiveProfileParser()
		{
		}

		//--------------------------------------------------------------------------------
		public  bool ParseFile(string filePath, LogInfo logInfo)
		{
            try
            {
                if (!File.Exists(filePath))
                    return false;

                string xmlText = string.Empty;
                using (StreamReader sr = new StreamReader(filePath))
                    xmlText = sr.ReadToEnd();

                if (string.IsNullOrWhiteSpace(xmlText))
                    return false;

                return ParseString(xmlText, logInfo);
            }
            catch (Exception e)
            {
                logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, e.Message, $"SynchroMassiveProfileParser.ParseFile(FilePath: {filePath})");
                return false;
            }
		}

		//--------------------------------------------------------------------------------
		private bool ParseString(string xmlText, LogInfo logInfo)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(xmlText);
                if (xDoc == null)
                    return false;

                ParseActions(xDoc, logInfo);

                ParseNamespaceMappings(xDoc, logInfo);

                return true;
            }
            catch (Exception e)
            {
                logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, e.Message, $"SynchroMassiveProfileParser.ParseFile(XmlText: {xmlText})");
                return false;
            }
        }

		//--------------------------------------------------------------------------------
		private void ParseActions(XDocument xDoc, LogInfo logInfo)
		{
			IEnumerable<XElement> actionList = null;

			try
			{
				actionList = xDoc.Element(CRMInfinitySynchroProfilesXML.Element.SynchroMassiveProfiles).Element(CRMInfinitySynchroProfilesXML.Element.Actions).Elements(CRMInfinitySynchroProfilesXML.Element.Action);
			}
			catch
			{
				throw new Exception("No root 'Actions' available!");
			}

			foreach (XElement xElem in actionList)
			{
				ActionToMassiveSync dms = null;
                string error = string.Empty;
				try
				{
					dms = new ActionToMassiveSync
						(
						(xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.Namespace) == null) ? string.Empty : xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.Namespace).Value,
						xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.Name).Value,
						xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.File).Value,
						(xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.OnlyMassive) == null) ? false : true,
                        (xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.OnlyForDMS) == null) ? false : Convert.ToBoolean(xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.OnlyForDMS).Value),
                        (xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.iMagoConfigurations) == null) ? IMagoModulesConfiguration.DMS : InfinityProviders.Parsers.Utilities.GetIMagoCoonfigurationModules(xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.iMagoConfigurations).Value, logInfo)
                        );
				}
				catch(Exception e)
				{
                    logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, e.Message, $"SynchroMassiveProfileParser.ParseActions");
                    continue;
				}

				syncProfileInfo.Documents.Add(dms);
			}
		}

		//--------------------------------------------------------------------------------
		private void ParseNamespaceMappings(XDocument xDoc, LogInfo logInfo)
		{
			IEnumerable<XElement> nsMapList = null;

			try
			{
				nsMapList = xDoc.Element(CRMInfinitySynchroProfilesXML.Element.SynchroMassiveProfiles).Element(CRMInfinitySynchroProfilesXML.Element.NamespaceMappings).
					Elements(CRMInfinitySynchroProfilesXML.Element.Map);
			}
			catch
			{
				return;
			}

			foreach (XElement xElem in nsMapList)
			{
				NamespaceMap map = null;

				try
				{
					map = new NamespaceMap
						(
						xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.Namespace).Value,
						xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.MasterTable).Value,
						xElem.Attribute(CRMInfinitySynchroProfilesXML.Attribute.PKName).Value
						);
				}
				catch(Exception e)
				{
                    logInfo?.ProviderLogWriter?.WriteToLog(logInfo?.CompanyName, logInfo?.ProviderName, e.Message, $"SynchroMassiveProfileParser.ParseNamespaceMappings");
                    continue;
                }

				namespaceMapInfo.Documents.Add(map);
			}
		}
 	}

	//================================================================================
    internal class ActionToMassiveSync : BaseDocumentToSync
    {
        //--------------------------------------------------------------------------------
        public string ActionName { get; private set; }
        public string File { get; private set; }
		public bool IsSynchronized { get; set; }
		public bool OnlyMassive { get; set; }

        public bool OnlyForDMS { get; set; }

        public IMagoModulesConfiguration IMagoModulesConfiguration { get; set; }

        //--------------------------------------------------------------------------------
        public ActionToMassiveSync(string nameSpace, string actionName, string file, bool onlyMassive, bool onlyForDMS, IMagoModulesConfiguration iMagoModulesConfiguration)
			: base(nameSpace)
		{
			this.ActionName = actionName;
			this.File = file;
			IsSynchronized = true; 
			OnlyMassive = onlyMassive;
            OnlyForDMS = onlyForDMS;
            this.IMagoModulesConfiguration = iMagoModulesConfiguration;
        }
    }

	//================================================================================
	internal class NamespaceMap : BaseDocumentToSync
	{
		//--------------------------------------------------------------------------------
		public string MasterTable { get; private set; }
		//--------------------------------------------------------------------------------
		public string PKName { get; private set; }

		//--------------------------------------------------------------------------------
		public NamespaceMap(string name, string masterTable, string pkName)
			: base(name)
		{
			this.MasterTable = masterTable;
			this.PKName = pkName;
		}
	}
}
