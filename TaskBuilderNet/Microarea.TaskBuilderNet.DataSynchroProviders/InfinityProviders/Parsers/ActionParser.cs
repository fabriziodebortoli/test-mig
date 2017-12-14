using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
	/// <summary>
	/// Parser dei file delle azioni
	///</summary>
	//================================================================================
    internal class ActionParser
	{
		private XmlDocument xmlDoc = new XmlDocument();
		private CRMActionInfo crmActionsInfo = new CRMActionInfo();

		//--------------------------------------------------------------------------------
		public CRMActionInfo CRMActionsInfo { get { return crmActionsInfo; } }

		//--------------------------------------------------------------------------------
		public ActionParser()
		{
		}

		//--------------------------------------------------------------------------------
        public bool ParseFile(string filePath, CRMActionInfo parentAction = null)
		{
			if (!File.Exists(filePath))
				return false;

			string xmlText = string.Empty;
			using (StreamReader sr = new StreamReader(filePath))
				xmlText = sr.ReadToEnd();

			if (string.IsNullOrWhiteSpace(xmlText))
				return false;

			return ParseString(xmlText, parentAction);
		}

		//--------------------------------------------------------------------------------
		private bool ParseString(string xmlToParse, CRMActionInfo parentAction)
		{
			try
			{
                XDocument xDoc = XDocument.Parse(xmlToParse);

                IEnumerable<XElement> actions = null;
                try
                {
                    actions = xDoc.Element(CRMActionXML.Element.Actions).Elements(CRMActionXML.Element.Action);
                }
                catch
                {
                    throw (new Exception("No root 'Action' available!"));
                }

                XAttribute masterTableAttr = null;
                // scorro tutte le azioni definite nel file
                foreach (XElement xElem in actions)
                {
                    CRMAction aInfo = new CRMAction();
                    try
                    {
                        aInfo.ActionName = xElem.Attribute(CRMActionXML.Attribute.Name).Value;
                    }
                    catch
                    {
                        continue;
                    }

					bool foundInParentAction = false;

					// ulteriore controllo se ho passato una parentAction (ovvero l'action che sto analizzando e' figlia di un'action definita in ERP)
					if (parentAction != null)
					{
						// devo controllare che tra le n-azioni di ERP ce ne sia una con lo stesso nome
						foreach (CRMAction erpAction in parentAction.Actions)
						{
							if (string.Compare(erpAction.ActionName, aInfo.ActionName, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								// se l'ho trovata devo creare una azione di append con tutti i suoi dati e l'aggiungo alla lista delle AppendActions
								if (ParseActionFields(xElem, aInfo) && ParseQuery(xElem, aInfo) && ParseSubActions(xElem, aInfo))
									erpAction.AppendActions.Add(aInfo);
								foundInParentAction = true;
								break;
							}
						}
					}

					// se ho gia' aggiunto l'action corrente nelle AppendAction del parent faccio continue
					if (foundInParentAction)
						continue;

                    aInfo.IgnoreEmptyValue = xElem.Attribute(CRMActionXML.Attribute.IgnoreEmptyValue) == null ? false : true;
                    aInfo.SkipForDelete = xElem.Attribute(CRMActionXML.Attribute.SkipForDelete) == null ? false : true;
                    aInfo.IgnoreError = xElem.Attribute(CRMActionXML.Attribute.IgnoreError) == null ? false : true;
                    aInfo.BaseAction = xElem.Attribute(CRMActionXML.Attribute.BaseAction) == null ? false : true;
                    masterTableAttr = xElem.Attribute(CRMActionXML.Attribute.MasterTable);
                    aInfo.MasterTable = masterTableAttr == null ? string.Empty : masterTableAttr.Value;

                    if (!ParseActionFields(xElem, aInfo))
                        continue;
                    if (!ParseQuery(xElem, aInfo))
                        continue;
                    if (!ParseSubActions(xElem, aInfo))
                        continue;

                    crmActionsInfo.Actions.Add(aInfo);                 
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
        private bool ParseActionFields(XElement elem, CRMAction aInfo)
		{
            IEnumerable<XElement> fields = null; 
            try
            {
                fields =  elem.Element(CRMActionXML.Element.Fields).Elements(CRMActionXML.Element.Field);
                if (fields == null) throw new Exception();
            }
            catch
            {
                return false;
            }

            foreach (XElement fElem in fields)
            { 
                CRMField crmField=new CRMField();
                try
                {
                    crmField.Target = fElem.Attribute(CRMActionXML.Attribute.Target).Value;
                }
                catch
                {
                    return false;
                }

                crmField.Key = fElem.Attribute(CRMActionXML.Attribute.Key) == null ? false : true;
                crmField.Mandatory = fElem.Attribute(CRMActionXML.Attribute.Mandatory) == null ? false : true;
                crmField.InternalUse = fElem.Attribute(CRMActionXML.Attribute.InternalUse) == null ? false : true;
                crmField.Source = fElem.Attribute(CRMActionXML.Attribute.Source) == null ? string.Empty : fElem.Attribute(CRMActionXML.Attribute.Source).Value;
				crmField.DeletePrefix = fElem.Attribute(CRMActionXML.Attribute.DeletePrefix) == null ? string.Empty : fElem.Attribute(CRMActionXML.Attribute.DeletePrefix).Value;
				crmField.DeleteKey = fElem.Attribute(CRMActionXML.Attribute.DeleteKey) == null ? string.Empty : fElem.Attribute(CRMActionXML.Attribute.DeleteKey).Value;

                aInfo.Fields.Add(crmField);
            }

			return true;
		}

		//--------------------------------------------------------------------------------
		private bool ParseSubActions(XElement elem, CRMAction action)
		{
            IEnumerable<XElement> subActions = null;
            try
            {
               subActions = elem.Element(CRMActionXML.Element.Subactions).Elements(CRMActionXML.Element.Subaction);
               if (subActions == null)
                   throw new Exception();
            }
            catch 
            {
                return true;
            }

            foreach (XElement aElem in subActions)
            {
                CRMAction aInfo = new CRMAction();
                try
                {
                    aInfo.ActionName = aElem.Attribute(CRMActionXML.Attribute.Name).Value;
                    if(aElem.Attribute(CRMActionXML.Attribute.Father) != null)
                        aInfo.FatherActionName = aElem.Attribute(CRMActionXML.Attribute.Father).Value;
                }
                catch
                {
                    continue;
                }

                if (!ParseActionFields(aElem, aInfo))
                    continue;
                if (!ParseQuery(aElem, aInfo))
                    continue;

                action.Subactions.Add(aInfo);
            }

            return true;
		}

		//--------------------------------------------------------------------------------
        private bool ParseQuery(XElement elem, CRMAction aInfo)
		{

            XElement queryElem=null;
            try
            {
                queryElem = elem.Element(CRMActionXML.Element.Query);
                if (queryElem == null) throw new Exception();             
            }
            catch
            {
                return false;
            }

            try
            {
                IEnumerable<XAttribute> attrs = queryElem.Attributes();
                foreach (XAttribute attr in attrs)
                    aInfo.SubactionsParams.Add(attr.Value);
            }
            catch { }

            try
            {
                aInfo.Select = queryElem.Element(CRMActionXML.Element.Select).Value;
            }
            catch
            {
                return false;
            }

            try
            {
                aInfo.From = queryElem.Element(CRMActionXML.Element.From).Value;
            }
            catch
            {
                return false;
            }

            try
            {
                XElement incrFromElem = queryElem.Element(CRMActionXML.Element.IncrFrom);
                if (incrFromElem == null || string.IsNullOrEmpty(incrFromElem.Value))
                    aInfo.IncrFrom = aInfo.From;
                else
                    aInfo.IncrFrom = incrFromElem.Value;
            }
            catch
            {
                return false;
            }

            try
            {
                aInfo.Where = queryElem.Element(CRMActionXML.Element.Where).Value;
            }
            catch
            {
                aInfo.Where = string.Empty;
            }

            try
            {
                aInfo.MassiveWhere = queryElem.Element(CRMActionXML.Element.MassiveWhere).Value;
            }
            catch
            {
                aInfo.MassiveWhere = string.Empty;
            }

            try
            {
                XElement incrWhereElem = queryElem.Element(CRMActionXML.Element.IncrWhere);
                if (incrWhereElem == null || string.IsNullOrEmpty(incrWhereElem.Value))
                    aInfo.IncrWhere = aInfo.MassiveWhere;
                else
                    aInfo.IncrWhere = incrWhereElem.Value;
            }
            catch
            {
                return false;
            }

            return true;
		}
	}
}
