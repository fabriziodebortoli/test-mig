using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
	///<summary>
	/// Parser dei file dei profili di sincronizzazione
	///</summary>
	//================================================================================
    internal class SynchroProfileParser : BaseSynchroProfileParser
	{
		private XmlDocument xmlDoc = new XmlDocument();
		private SynchroProfileInfo syncProfileInfo = new SynchroProfileInfo();

		//--------------------------------------------------------------------------------
		public override SynchroProfileInfo SynchroProfileInfo { get { return syncProfileInfo; } }

		//--------------------------------------------------------------------------------
		public SynchroProfileParser()
		{
		}

		//--------------------------------------------------------------------------------
		public override bool ParseFile(string filePath, string addOnAppName)
		{
			if (!File.Exists(filePath))
				return false;

			string xmlText = string.Empty;
			using (StreamReader sr = new StreamReader(filePath))
				xmlText = sr.ReadToEnd();

			if (string.IsNullOrWhiteSpace(xmlText))
				return false;

			return ParseString(xmlText, addOnAppName);
		}

		//--------------------------------------------------------------------------------
		private bool ParseString(string xmlToParse,string addOnAppName)
		{
			try
			{
				xmlDoc.LoadXml(xmlToParse);
				
				//root
				XmlElement root = xmlDoc.DocumentElement;
				if (string.Compare(root.Name, CRMInfinitySynchroProfilesXML.Element.SynchroProfiles, StringComparison.InvariantCultureIgnoreCase) != 0)
					throw (new Exception("No root 'SynchroProfiles' available!"));

				//cerco il nodo Documents
				XmlNodeList docsElem = root.GetElementsByTagName(CRMInfinitySynchroProfilesXML.Element.Documents);
				if (docsElem != null && docsElem.Count > 0)
				{
					//cerco i nodi Document
					XmlNodeList docElements = ((XmlElement)docsElem[0]).GetElementsByTagName(CRMInfinitySynchroProfilesXML.Element.Document);

					// analizzo ogni Document e richiamo il parse
					foreach (XmlElement xdoc in docElements)
					{
						string ns = xdoc.GetAttribute(CRMInfinitySynchroProfilesXML.Attribute.Namespace);
						if (string.IsNullOrWhiteSpace(ns))
							continue;

						// la lista dei ns dei documenti da sincronizzare viene riempita considerando prima di tutto ERP
						// e poi gli altri verticali. se uno di questi va ad aggiungere un'altra azione al documento allora deve
						// essere eseguita dopo quella di ERP
						bool nsFound = false;
						foreach (DocumentToSync doc in syncProfileInfo.Documents)
						{
							if (string.Compare(doc.Name, ns, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								ParseActions(xdoc, doc);
								nsFound = true;
							}
						}
						if (nsFound)
							continue;

						// inserisco nella struttura solo se non esiste già
						DocumentToSync docInfo = new DocumentToSync(ns, addOnAppName);
						syncProfileInfo.Documents.Add(docInfo);

						ParseActions(xdoc, docInfo);
					}
				}

				//cerco il nodo SynchroActions
				XmlNodeList synchroActionsElem = root.GetElementsByTagName(CRMInfinitySynchroProfilesXML.Element.SynchroActions);
				if (synchroActionsElem != null && synchroActionsElem.Count > 0)
				{
					//cerco i nodi SynchroAction
					XmlNodeList synchroActionElements = ((XmlElement)synchroActionsElem[0]).GetElementsByTagName(CRMInfinitySynchroProfilesXML.Element.SynchroAction);

					// analizzo ogni SynchroAction e richiamo il parse
					foreach (XmlElement xdoc in synchroActionElements)
					{
						string actionType = xdoc.GetAttribute(CRMInfinitySynchroProfilesXML.Attribute.Type);
						if (string.IsNullOrWhiteSpace(actionType))
							continue;

						DocumentToSync attInfo = new DocumentToSync(actionType, addOnAppName);
						syncProfileInfo.Documents.Add(attInfo);

						ParseActions(xdoc, attInfo);
					}
				}
			}
			catch
			{
				throw;
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		private void ParseActions(XmlElement xDocNode, DocumentToSync docInfo)
		{
			//cerco i nodi Action
			XmlNodeList actionElements = (xDocNode).GetElementsByTagName(CRMInfinitySynchroProfilesXML.Element.Action);
			if (actionElements == null)
				return;

			foreach (XmlElement xAction in actionElements)
			{
				DSAction action = new DSAction(xAction.GetAttribute(CRMInfinitySynchroProfilesXML.Attribute.Name));
				docInfo.Actions.Add(action);
			}
		}

		//--------------------------------------------------------------------------------
		public bool IsValidActionForAddOnApp(string action)
		{
			foreach (DocumentToSync item in SynchroProfileInfo.Documents)
				foreach (DSAction dsAction in item.Actions)
				{
					if (string.Compare(dsAction.Name, action, StringComparison.InvariantCultureIgnoreCase) == 0)
						return true;
				}

			return false;
		}
	}

	///<summary>
	/// Nodo di tipo DocumentToSync
	///</summary>
	//================================================================================
	public class DocumentToSync : BaseDocumentToSync
	{
		public IList<DSAction> Actions = new List<DSAction>();

		//--------------------------------------------------------------------------------
		public DocumentToSync(string nameSpace, string addOnAppName)
			: base(nameSpace, addOnAppName)
		{
		}
	}

	///<summary>
	/// Nodo di tipo Action
	///</summary>
	//================================================================================
	public class DSAction
	{
		//--------------------------------------------------------------------------------
		public string Name { get; private set; }

		//--------------------------------------------------------------------------------
		public DSAction(string name)
		{
			Name = name;
		}
	}
}
