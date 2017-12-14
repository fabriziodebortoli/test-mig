using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers
{
	///<summary>
	/// Parser dei file dei profili di sincronizzazione
	///</summary>
	//================================================================================
	internal class PatSynchroProfileParser: BaseSynchroProfileParser
	{
		private XmlDocument xmlDoc = new XmlDocument();
		private SynchroProfileInfo syncProfileInfo = new SynchroProfileInfo();

		//--------------------------------------------------------------------------------
		public override SynchroProfileInfo SynchroProfileInfo { get { return syncProfileInfo; } }

		//--------------------------------------------------------------------------------
		public PatSynchroProfileParser()
		{
		}

        //--------------------------------------------------------------------------------
		public override bool ParseFile(string filePath, string addOnApp = "")
        {
            if (!File.Exists(filePath))
                return false;

            string xmlText = string.Empty;
            using (StreamReader sr = new StreamReader(filePath))
                xmlText = sr.ReadToEnd();

            if (string.IsNullOrWhiteSpace(xmlText))
                return false;

			try
			{
				return ParseString(xmlText);
			}
			catch (XmlException xe)
			{
				Debug.WriteLine(string.Format("Error parsing file '{0}': {1}", filePath, xe.Message));
				return false;
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("Error parsing file '{0}': {1}", filePath, e.Message));
				return false;
			}
        }

        //--------------------------------------------------------------------------------
        private bool ParseString(string xmlToParse)
        {
            try
            {
                xmlDoc.LoadXml(xmlToParse);

                //root
                XmlElement root = xmlDoc.DocumentElement;
                if (string.Compare(root.Name, InfiniteCRMSynchroProfilesXML.Element.SynchroProfiles, StringComparison.InvariantCultureIgnoreCase) != 0)
                    throw (new Exception("No root 'SynchroProfiles' available!"));

                //cerco il nodo Documents
				XmlNodeList docsElem = root.GetElementsByTagName(InfiniteCRMSynchroProfilesXML.Element.Documents);
                if (docsElem != null && docsElem.Count > 0)
                {
                    //cerco i nodi Document
					XmlNodeList docElements = ((XmlElement)docsElem[0]).GetElementsByTagName(InfiniteCRMSynchroProfilesXML.Element.Document);

                    // analizzo ogni Document e richiamo il parse
                    foreach (XmlElement xdoc in docElements)
                    {
						string ns = xdoc.GetAttribute(InfiniteCRMSynchroProfilesXML.Attribute.Namespace);
                        if (string.IsNullOrWhiteSpace(ns))
							continue;

						string actionsAttr = xdoc.GetAttribute(InfiniteCRMSynchroProfilesXML.Attribute.Actions);
						string actions = !string.IsNullOrWhiteSpace(actionsAttr) ? actionsAttr : "111" /*(tutte le azioni del doc sono abilitate)*/;

						string direction = xdoc.GetAttribute(InfiniteCRMSynchroProfilesXML.Attribute.Direction);
                        // inserisco nella struttura
						PatDocumentToSync docInfo = new PatDocumentToSync(ns, direction, actions);
                        syncProfileInfo.Documents.Add(docInfo);

						ParseEntitiesForDocument(xdoc, docInfo);
                    }
                }
            }
            catch (XmlException xe)
            {
                throw (xe);
            }
            catch (Exception e)
            {
                throw (e);
            }

            return true;
        }

        //--------------------------------------------------------------------------------
		private void ParseEntitiesForDocument(XmlElement xDocNode, PatDocumentToSync docInfo)
		{
			//cerco il nodo Actions
			XmlNodeList entityNodes = xDocNode.GetElementsByTagName(InfiniteCRMSynchroProfilesXML.Element.ICRMEntity);
			if (entityNodes != null)
			{
				foreach (XmlElement xEntity in entityNodes)
				{
					DSEntity entity = new DSEntity(xEntity.GetAttribute(InfiniteCRMSynchroProfilesXML.Attribute.Name));
					docInfo.Entities.Add(entity);
				}
			}
		}
	}

	///<summary>
	/// Nodo di tipo DSEntity
	///</summary>
	//================================================================================
	public class DSEntity
	{
		//--------------------------------------------------------------------------------
		public string Name { get; private set; }

		//--------------------------------------------------------------------------------
		public DSEntity(string name)
		{
			Name = name;
		}
	}

	///<summary>
	/// Nodo di tipo PatDocumentToSync
	///</summary>
	//================================================================================
	public class PatDocumentToSync : BaseDocumentToSync
	{
		public IList<DSEntity> Entities = new List<DSEntity>();

		public string Direction = string.Empty;
        public string _actionsAttribute = string.Empty;// stringa composta da 3 chr: 1=insert 2=update 3=delete 
                                                       // 111 (default): tutte le azioni sono abilitate
                                                       // 011: l'azione di insert e' disabilitata e cosi via

        public string ActionsAttribute { get; protected set; }

        //--------------------------------------------------------------------------------
        public PatDocumentToSync(string nameSpace, string direction, string actions)
			: base(nameSpace)
		{
			this.ActionsAttribute = actions;
			this.Direction = direction;
		}
	}
}
