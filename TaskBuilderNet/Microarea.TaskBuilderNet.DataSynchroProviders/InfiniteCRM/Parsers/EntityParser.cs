using System;
using System.IO;
using System.Xml;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers
{
	/// <summary>
	/// Parser dei file delle Entities
	///</summary>
	//================================================================================
	internal class EntityParser
	{
		private XmlDocument xmlDoc = new XmlDocument();
		private CRMEntityInfo crmEntityInfo = new CRMEntityInfo();
		private bool isEntityAppendFile = false;

		//--------------------------------------------------------------------------------
		public CRMEntityInfo CrmEntityInfo { get { return crmEntityInfo; } }

		//--------------------------------------------------------------------------------
		public bool ParseFile(string filePath, CRMEntityInfo parentEntity = null)
		{
			if (!File.Exists(filePath))
				return false;

			string xmlText = string.Empty;
			using (StreamReader sr = new StreamReader(filePath))
				xmlText = sr.ReadToEnd();

			if (string.IsNullOrWhiteSpace(xmlText))
				return false;

			return ParseString(xmlText, parentEntity);
		}

		//--------------------------------------------------------------------------------
		private bool ParseString(string xmlToParse, CRMEntityInfo parentEntity)
		{
			try
			{
				xmlDoc.LoadXml(xmlToParse);

				//root
				XmlElement root = xmlDoc.DocumentElement;
				if (string.Compare(root.Name, CRMEntityXML.Element.Entity, StringComparison.InvariantCultureIgnoreCase) != 0)
					throw (new Exception("No root 'Entity' available!"));

				// entity name
				string entityName = root.GetAttribute(CRMEntityXML.Attribute.Name);
				if (string.IsNullOrWhiteSpace(entityName))
					throw (new Exception("No 'Entity' name available!"));
				// ulteriore controllo: devo verificare che corrispondano i nomi dell'entita' padre con quella del figlio
				if (parentEntity != null && string.Compare(parentEntity.Entity.Name, entityName, StringComparison.InvariantCultureIgnoreCase) != 0)
					return true;

				string appendAttribute = root.GetAttribute(CRMEntityXML.Attribute.Append);
				isEntityAppendFile = (!string.IsNullOrWhiteSpace(appendAttribute) && string.Compare(appendAttribute, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);

				// riferimento alla table di ERP (serve per la transcodifica e cmq per comporre la where clause con il filtro sul TBGuid)
				string entityTable = root.GetAttribute(CRMEntityXML.Attribute.Table);
				if (string.IsNullOrWhiteSpace(entityTable) && !isEntityAppendFile) // puo' non esserci solo se sono in append
					throw (new Exception("No 'Entity' table available!"));

				if (parentEntity == null)
				{
					CRMEntity entity = new CRMEntity(entityName, entityTable);
					ParseEntityChilds(root, entity);
					crmEntityInfo.Entity = entity;
				}
				else
					ParseEntityChilds(root, parentEntity.Entity);
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
		private bool ParseEntityChilds(XmlElement root, CRMEntity entity)
		{
			if (root == null)
				return false;

			//cerco il nodo Transcoding (e' presente solo per le entita' primarie (ovvero quelle che hanno un ID autogenerato da Pat))
			XmlNodeList transcodingElem = root.GetElementsByTagName(CRMEntityXML.Element.Transcoding);
			if (transcodingElem != null && transcodingElem.Count > 0)
			{
				entity.TranscodingField = ((XmlElement)transcodingElem[0]).GetAttribute(CRMEntityXML.Attribute.Field);
				entity.TranscodingExternalField = ((XmlElement)transcodingElem[0]).GetAttribute(CRMEntityXML.Attribute.ExternalField);
			}

			//cerco il nodo Fields
			XmlNodeList fieldsElem = root.GetElementsByTagName(CRMEntityXML.Element.Fields);
			if (fieldsElem != null && fieldsElem.Count > 0)
			{
				//cerco il tag Field
				XmlNodeList fieldElements = ((XmlElement)fieldsElem[0]).GetElementsByTagName(CRMEntityXML.Element.Field);
				// richiamo il parse dei nodi Table
				ParseField(fieldElements, entity);
			}

			//cerco il nodo Query
			XmlNodeList queryElem = root.GetElementsByTagName(CRMEntityXML.Element.Query);
			if (queryElem != null && queryElem.Count > 0)
				ParseQueryChilds(queryElem, entity);

			//cerco il nodo SubEntities
			XmlNodeList subElem = root.GetElementsByTagName(CRMEntityXML.Element.SubEntities);
			if (subElem != null && subElem.Count > 0)
			{
				//cerco il tag SubEntity
				XmlNodeList subElements = ((XmlElement)subElem[0]).GetElementsByTagName(CRMEntityXML.Element.SubEntity);
				if (subElements != null)
				{
					foreach (XmlElement xSub in subElements)
					{
						string subEntity = xSub.GetAttribute(CRMEntityXML.Attribute.Name);
						if (!string.IsNullOrWhiteSpace(subEntity))
							entity.SubEntities.Add(subEntity);
					}
				}
			}

			// cerco il nodo Delete e leggo i suoi attributi
			XmlNodeList deleteElem = root.GetElementsByTagName(CRMEntityXML.Element.Delete);
			if (deleteElem != null && deleteElem.Count > 0)
			{
				XmlElement delete = ((XmlElement)(deleteElem[0]));
				entity.DeleteTarget = delete.GetAttribute(CRMEntityXML.Attribute.Target);
				entity.DeleteValue = delete.GetAttribute(CRMEntityXML.Attribute.Value);
			}
				
			return true;
		}

		// Parse dei nodi di tipo Field
		//--------------------------------------------------------------------------------
		private bool ParseField(XmlNodeList fieldElements, CRMEntity action)
		{
			if (fieldElements == null)
				return false;

			foreach (XmlElement xField in fieldElements)
			{
				CRMField crmField = new CRMField();
				crmField.Target = xField.GetAttribute(CRMEntityXML.Attribute.Target);

				string key = xField.GetAttribute(CRMEntityXML.Attribute.Key);
				crmField.Key = (!string.IsNullOrWhiteSpace(key) && string.Compare(key, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);
				string mandatory = xField.GetAttribute(CRMEntityXML.Attribute.Mandatory);
				crmField.Mandatory = (!string.IsNullOrWhiteSpace(mandatory) && string.Compare(mandatory, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);
				string entity = xField.GetAttribute(CRMEntityXML.Attribute.Entity);
				crmField.Entity = string.IsNullOrWhiteSpace(entity) ? string.Empty : entity;
				string parentField = xField.GetAttribute(CRMEntityXML.Attribute.ParentField);
				crmField.ParentField = string.IsNullOrWhiteSpace(parentField) ? string.Empty : parentField;
				string internalUse = xField.GetAttribute(CRMEntityXML.Attribute.InternalUse);
				crmField.InternalUse = (!string.IsNullOrWhiteSpace(internalUse) && string.Compare(internalUse, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0);
			
				action.Fields.Add(crmField);

				// se esiste l'attributo entita valorizzato 
				if (!string.IsNullOrWhiteSpace(crmField.Entity))
					action.FKFields.Add(crmField);
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		private bool ParseQueryChilds(XmlNodeList xQueryChilds, CRMEntity action)
		{
			foreach (XmlElement child in xQueryChilds[0].ChildNodes)
			{
				if (string.Compare(child.Name, CRMEntityXML.Element.Select, StringComparison.InvariantCultureIgnoreCase) == 0)
					if (isEntityAppendFile)
						action.Select += "," + child.InnerText;
					else
						action.Select = child.InnerText;

				if (string.Compare(child.Name, CRMEntityXML.Element.From, StringComparison.InvariantCultureIgnoreCase) == 0)
					if (isEntityAppendFile)
						action.From += "," + child.InnerText;
					else
						action.From = child.InnerText;

				if (string.Compare(child.Name, CRMEntityXML.Element.Where, StringComparison.InvariantCultureIgnoreCase) == 0)
					if (isEntityAppendFile)
						action.Where += " AND " + child.InnerText;
					else
						action.Where = child.InnerText;

				if (string.Compare(child.Name, CRMEntityXML.Element.MassiveWhere, StringComparison.InvariantCultureIgnoreCase) == 0)
					if (isEntityAppendFile)
						action.MassiveWhereClause += " AND " + child.InnerText;
					else
						action.MassiveWhereClause = child.InnerText;
			}

			return true;
		}
	}
}
