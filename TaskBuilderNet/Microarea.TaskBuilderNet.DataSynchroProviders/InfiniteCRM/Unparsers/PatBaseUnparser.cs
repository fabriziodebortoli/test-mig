using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using System.Text.RegularExpressions;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Unparsers
{
	///<summary>
	/// Classe base per gli unparser
	/// Il metodo GetXml e' definito nel padre, i metodi Add/Update/Delete devono essere reimplementati nei figli
	/// I figli DEVONO avere il nome composto da nomeAzione_Unparser
	///</summary>
	//================================================================================
	internal class PatBaseUnparser
	{
		//--------------------------------------------------------------------------------
		public string GetXml(PatUnparserParams paramsHelper)
		{
			string xmlText = string.Empty;

			switch (paramsHelper.SyncActionType)
			{
				case SynchroActionType.Insert:
					xmlText = Insert(paramsHelper);
					break;
				case SynchroActionType.Update:
					xmlText = Update(paramsHelper);
					break;
				case SynchroActionType.Delete:
					xmlText = (paramsHelper.ActionDataInfo != null) ? Delete(paramsHelper) : DeleteSubEntity(paramsHelper);
					break;
				case SynchroActionType.Exclude:
					xmlText = Exclude(paramsHelper);
					break;
				default:
					break;
			}

			return xmlText;
		}

		// ritorna il nome della classe da caricare via reflection (namespacecompleto.nomeClasse_Unparser)
		//--------------------------------------------------------------------------------
		public static string GetClassName(string className)
		{
			// attenzione: indicare il namespace corretto
			return string.Concat("Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Unparsers.", className, "_Unparser");
		}

		//--------------------------------------------------------------------------------
		protected string Insert(PatUnparserParams paramsHelper)
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement(InfiniteCRMConsts.Operations);
			doc.AppendChild(root);

			try
			{
				foreach (DTPatValuesToImport dtToImport in paramsHelper.ValDataTablesList)
				{
					XmlElement operationNode = doc.CreateElement(InfiniteCRMConsts.Operation);
					doc.DocumentElement.AppendChild(operationNode);

					XmlElement setNode = doc.CreateElement(InfiniteCRMConsts.Set);
					operationNode.AppendChild(setNode);

					// scorro i master
					foreach (DataRow dr in dtToImport.MasterDt.Rows)
					{
						XmlElement elem = doc.CreateElement(paramsHelper.CRMEntity.Name);

						if (!AddChildNodesToMasterElement(paramsHelper, doc, elem, dr))
							return string.Empty;

						setNode.AppendChild(elem);
					}
				}
			}
			catch (Exception e)
			{
				throw (new DSException("PatBaseUnparser.Insert", e.Message, paramsHelper.GetResponseXml));
			}
		
			return doc.InnerXml;
		}

		///<summary>
		/// Cancellazione di un'entita' master
		/// Non cancello ma vado a disabilitare l'oggetto in Pat
		///</summary>
		//--------------------------------------------------------------------------------
		protected string Delete(PatUnparserParams paramsHelper)
		{
			XElement operations = new XElement(InfiniteCRMConsts.Operations);

			try
			{
				// parso la risposta senza Results\Result fino al nome dell'entita'
				XElement elem = PatSynchroResponseParser.GetRootElement(paramsHelper.GetResponseXml).Element(paramsHelper.CRMEntity.Name);
				if (elem == null)
					return string.Empty;

				XElement operation = new XElement(InfiniteCRMConsts.Operation);
				XElement set = new XElement(InfiniteCRMConsts.Set);
				operations.Add(operation);
				operation.Add(set);

				try
				{
					// vado a sostituire il valore del target in modo da settare il disable in pat
					elem.Element(paramsHelper.CRMEntity.DeleteTarget).SetValue(paramsHelper.CRMEntity.DeleteValue);
				}
				catch
				{ 
					// se fallisce significa che il nodo col nome del target non esiste e quindi lo aggiungo d'ufficio
					elem.Add(new XElement(paramsHelper.CRMEntity.DeleteTarget, paramsHelper.CRMEntity.DeleteValue));
				}

				set.Add(elem);
			}
			catch (Exception e)
			{
				throw (new DSException("PatBaseUnparser.Delete", e.Message, paramsHelper.GetResponseXml));
			}

			return operations.ToString(SaveOptions.DisableFormatting);
		}

		///<summary>
		/// Cancellazione di subentity
		/// Elimino tutte le righe e le ri-creo
		///</summary>
		//--------------------------------------------------------------------------------
		protected string DeleteSubEntity(PatUnparserParams paramsHelper)
		{
			string val = string.Empty;
			XElement operations = new XElement(InfiniteCRMConsts.Operations);

			try
			{
				foreach (DTPatValuesToImport dtToImport in paramsHelper.ValDataTablesList)
				{
					foreach (DataRow dr in dtToImport.MasterDt.Rows)
					{
						XElement operation = new XElement(InfiniteCRMConsts.Operation);
						XElement delete = new XElement(InfiniteCRMConsts.Delete);
						operations.Add(operation);
						operation.Add(delete);
						XElement elem = null;
						foreach (CRMField crmF in paramsHelper.CRMEntity.Fields)
						{
							if ((paramsHelper.CRMEntity.IsPrimary && string.Compare(crmF.Target, InfiniteCRMConsts.ID, StringComparison.InvariantCultureIgnoreCase) == 0) ||
								(paramsHelper.CRMEntity.IsPrimary && string.Compare(crmF.Target, InfiniteCRMConsts.ERPKey, StringComparison.InvariantCultureIgnoreCase) == 0))
							{
								val = FormatDateTimeAndDecimal(dr, crmF.Target);
								paramsHelper.CRMEntity.PKValues.Add(val);
								elem = new XElement(paramsHelper.CRMEntity.Name, new XAttribute(InfiniteCRMConsts.ID.ToLower(), val));
								delete.Add(elem);
								break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				throw (new DSException("PatBaseUnparser.DeleteSubEntity", e.Message, paramsHelper.GetResponseXml));
			}

			return operations.ToString(SaveOptions.DisableFormatting);
		}

		///<summary>
		/// Esclusione di un'entita' master: vado a disabilitare l'oggetto in Pat
		/// Potrei avere n-elementi
		///</summary>
		//--------------------------------------------------------------------------------
		protected string Exclude(PatUnparserParams paramsHelper)
		{
			XElement operations = new XElement(InfiniteCRMConsts.Operations);

			int countElem = 0;

			try
			{
				XDocument res = null;
				IEnumerable<XElement> elemList = new List<XElement>();
				if (!string.IsNullOrWhiteSpace(paramsHelper.GetResponseXml))
				{
					res = XDocument.Parse(paramsHelper.GetResponseXml);
					elemList = res.Element(XName.Get("Results")).Elements(XName.Get("Result"));
				}

				// mi faccio ritornare i soli nodi con il nome uguale all'entita' e skippo gli altri (potrebbe essere ObjectNotFound, etc)
				elemList = GetExistingElements(elemList, paramsHelper.CRMEntity.Name);
			
				foreach (XElement e in elemList)
				{
					XElement operation = new XElement(InfiniteCRMConsts.Operation);
					XElement set = new XElement(InfiniteCRMConsts.Set);
					operations.Add(operation);
					operation.Add(set);

					try
					{
						// vado a sostituire il valore del target in modo da settare il disable in pat
						e.Element(paramsHelper.CRMEntity.DeleteTarget).SetValue(paramsHelper.CRMEntity.DeleteValue);
					}
					catch
					{
						// se fallisce significa che il nodo col nome del target non esiste e quindi lo aggiungo d'ufficio
						e.Add(new XElement(paramsHelper.CRMEntity.DeleteTarget, paramsHelper.CRMEntity.DeleteValue));
					}

					set.Add(e);
					countElem++;
				}
			}
			catch (Exception e)
			{
				throw (new DSException("PatBaseUnparser.Exclude", e.Message, paramsHelper.GetResponseXml));
			}

			return (countElem > 0) ? operations.ToString(SaveOptions.DisableFormatting) : string.Empty;
		}

		//--------------------------------------------------------------------------------
		private IEnumerable<XElement> GetExistingElements(IEnumerable<XElement> elemList, string entityName)
		{
			foreach (XElement elem in elemList)
			{
				if (!elem.Attribute(XName.Get("code")).Value.Equals("OK"))
					continue;
				yield return elem.Element(entityName);
			}
		}

		///<summary>
		/// Update generica
		///</summary>
		//--------------------------------------------------------------------------------
		protected string Update(PatUnparserParams paramsHelper)
		{
			string fragment = string.Empty;

			XElement operations = new XElement(InfiniteCRMConsts.Operations);

			try
			{
				XDocument res = null;
				IEnumerable<XElement> elemList = new List<XElement>();

				if (!string.IsNullOrWhiteSpace(paramsHelper.GetResponseXml))
				{
					res = XDocument.Parse(paramsHelper.GetResponseXml);
					elemList = res.Element(XName.Get("Results")).Elements(XName.Get("Result"));
				}

				// scorro i dati estratti dalla nostra query e vado a sostituire il valore nell'xml ritornato dalla Get
				foreach (DTPatValuesToImport dtToImport in paramsHelper.ValDataTablesList)
				{
					bool isGetSucceeded = false;
					bool hasBank = false;

					XElement operation = new XElement(InfiniteCRMConsts.Operation);
					XElement set = new XElement(InfiniteCRMConsts.Set);
					XElement bAcc = new XElement(InfiniteCRMConsts.BankAccount); //gestione di bankaccount
					XElement finalBAcc = new XElement(InfiniteCRMConsts.BankAccount); //gestione di bankaccount
					finalBAcc.Add(bAcc);
					operations.Add(operation);
					operation.Add(set);

					XElement elem = GetElementByID(elemList, dtToImport.PatID, paramsHelper.CRMEntity.Name);
					if (elem == null)
					{
						isGetSucceeded = false;
						elem = new XElement(paramsHelper.CRMEntity.Name);
					}
					else
						isGetSucceeded = true;

					foreach (DataRow dr in dtToImport.MasterDt.Rows)
					{
						foreach (CRMField crmF in paramsHelper.CRMEntity.Fields)
						{
							// skippo l'ID
							if (string.Compare(crmF.Target, InfiniteCRMConsts.ID, StringComparison.InvariantCultureIgnoreCase) == 0)
								continue;

							string colValue = FormatDateTimeAndDecimal(dr, crmF.Target);

							try
							{
								if (isGetSucceeded)
								{
									if (crmF.Target.Equals(InfiniteCRMConsts.LastUpdate) || crmF.Target.Equals(InfiniteCRMConsts.LastUpdateUserID) ||
										crmF.Target.Equals(InfiniteCRMConsts.FirstUpdateUserID) || crmF.Target.Equals(InfiniteCRMConsts.FirstUpdate))
									{
										elem.Element(paramsHelper.CRMEntity.Name).Element(XName.Get(crmF.Target)).Remove();
										continue;
									}

									if (crmF.Target.Equals("Bank") || crmF.Target.Equals("IBAN") || crmF.Target.Equals("ABI") ||
										crmF.Target.Equals("CAB") || crmF.Target.Equals("CC") || crmF.Target.Equals("CIN"))
									{
										elem.Element(paramsHelper.CRMEntity.Name).Element(InfiniteCRMConsts.BankAccount).Element(InfiniteCRMConsts.BankAccount).Element(XName.Get(crmF.Target)).SetValue(colValue);
										continue;
									}
									elem.Element(paramsHelper.CRMEntity.Name).Element(XName.Get(crmF.Target)).SetValue(colValue);
								}
								else
								{
									if (crmF.Target.Equals("Bank") || crmF.Target.Equals("IBAN") || crmF.Target.Equals("ABI") ||
										crmF.Target.Equals("CAB") || crmF.Target.Equals("CC") || crmF.Target.Equals("CIN"))
									{
										bAcc.Add(new XElement(XName.Get(crmF.Target), colValue));
										hasBank = true;
										continue;
									}

									elem.Add(new XElement(XName.Get(crmF.Target), colValue));
								}
							}
							catch (Exception)
							{
								continue;
							}
						}

					}

					if (isGetSucceeded)
						set.Add(elem.FirstNode);
					else
					{
						// se non e' primaria alla fine aggiungo l'ID
						if (!paramsHelper.CRMEntity.IsPrimary)
							elem.Add(new XElement(XName.Get(InfiniteCRMConsts.ID), dtToImport.PatID)); 

						set.Add(elem);

						if (hasBank)
							elem.Add(finalBAcc);
					}
				}
			}
			catch (Exception e)
			{
				throw (new DSException("PatBaseUnparser.Update", e.Message, paramsHelper.GetResponseXml));
			}

			return operations.ToString(SaveOptions.DisableFormatting);
		}

		//--------------------------------------------------------------------------------
		private XElement GetElementByID(IEnumerable<XElement> elemList, string id, string name)
		{
			foreach (XElement elem in elemList)
			{
				if (elem.Attribute(XName.Get("code")).Value.Equals("ObjectNotFound"))
					continue;
				if (elem.Element(name).Element(InfiniteCRMConsts.ID).Value.Equals(id))
					return elem;
			}
			return null;
		}

		///<summary>
		/// aggiunge ad un XmlElement master tutti i suoi attributi letti dal datatable
		/// Di default il test dell'attributo mandatory viene fatto sempre (in update non e' necessario)
		///</summary>
		//--------------------------------------------------------------------------------
		protected bool AddChildNodesToMasterElement(PatUnparserParams paramsHelper, XmlDocument doc, XmlElement masterElem, DataRow dr, bool testIsMandatory = true)
		{
			XmlDocumentFragment xfrag = null;
			XmlNode ba1OpenNode = null;
			XmlNode ba2OpenNode = null;
			bool baTagFound = false;

			if (paramsHelper.CRMEntity.HasBankAccountNode)
			{
				// creo il fragment per gestire il nodo BankAccount
				xfrag = doc.CreateDocumentFragment();
				ba1OpenNode = xfrag.AppendChild(doc.CreateNode(XmlNodeType.Element, InfiniteCRMConsts.BankAccount, string.Empty));
				ba2OpenNode = ba1OpenNode.AppendChild(doc.CreateNode(XmlNodeType.Element, InfiniteCRMConsts.BankAccount, string.Empty));
			}

			foreach (CRMField crmF in paramsHelper.CRMEntity.Fields)
			{
				try
				{
					if (
						paramsHelper.SyncActionType == SynchroActionType.Insert &&
						paramsHelper.CRMEntity.IsPrimary &&
						(string.Compare(crmF.Target, InfiniteCRMConsts.ID, StringComparison.InvariantCultureIgnoreCase) == 0 || crmF.InternalUse)
						)
						continue;

					// gestione ad-hoc per i nodi figli di BankAccount
					if (paramsHelper.CRMEntity.HasBankAccountNode)
					{
						if (string.Compare(crmF.ParentField, InfiniteCRMConsts.BankAccount, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							baTagFound = true;
							XmlNode baNodeOpen = doc.CreateNode(XmlNodeType.Element, crmF.Target, string.Empty);
							baNodeOpen.InnerText = FormatDateTimeAndDecimal(dr, crmF.Target);
							ba2OpenNode.AppendChild(baNodeOpen);
							continue;
						}
					}

					string targetValue = FormatDateTimeAndDecimal(dr, crmF.Target);

					if (string.IsNullOrWhiteSpace(targetValue))
						continue;

					XmlElement colElem = doc.CreateElement(crmF.Target);
					colElem.InnerText = targetValue;
					masterElem.AppendChild(colElem);
				}
				catch (Exception)
				{
					// nel caso non ci fosse quella colonna (ovvero la dr[x] genera un'eccezione)
					// se il Field e' key o e' mandatory ritorno subito false, altrimenti vado avanti
					if (crmF.Key || (crmF.Mandatory && testIsMandatory))
						return false;
				}
			}

			if (paramsHelper.CRMEntity.HasBankAccountNode && baTagFound)
				masterElem.AppendChild(xfrag);

			return true;
		}

		//--------------------------------------------------------------------------------
		protected static string FormatDateTimeAndDecimal(DataRow dr, string target)
		{
            string targetValue=string.Empty;
            try
            {
                targetValue = dr[target].ToString();
            }
            catch
            {               
                return string.Empty;
            }

			if (string.IsNullOrWhiteSpace(targetValue))
				return string.Empty;

			// visto che il DataTable formatta la data come vuole lui, devo rimetterla nel formato atteso (yyyy-mm-dd)

			DataColumn col = null;
            try
            {
                col=dr.Table.Columns[target];
            }
            catch
            {
                return string.Empty;
            }

			if (col.DataType == typeof(DateTime))
				targetValue = DateTime.Parse(targetValue).ToString(InfiniteCRMConsts.InfiniteCRMDateFormat);
			// i double devono avere il punto e non la virgola!
			if (col.DataType == typeof(Double))
				targetValue = targetValue.Replace(",", ".");

            targetValue = Regex.Replace(targetValue, "[^ -~]", "?");

			return targetValue;
		}
	}

	///<summary>
	/// Classe generica per i parametri da passare alle classi di Unparser
	///</summary>
	//================================================================================
	internal class PatUnparserParams
	{
		public CRMEntity CRMEntity { get; set; }
		public SynchroActionType SyncActionType { get; set; }

		public string GetResponseXml { get; set; }

		public List<DTPatValuesToImport> ValDataTablesList { get; set; }

		// TODO: da gestire (chiavi per la cancellazione)
		public ActionDataInfo ActionDataInfo { get; set; } // TODO: passare direttamente la lista con le chiavi, spostare la creazione di questa lista nel padre

		//--------------------------------------------------------------------------------
		public PatUnparserParams(CRMEntity entity, SynchroActionType syncActionType, List<DTPatValuesToImport> dtValuesList, ActionDataInfo actionDataInfo, string getResponseXml)
		{
			CRMEntity = entity;
			SyncActionType = syncActionType;
			ValDataTablesList = dtValuesList;
			ActionDataInfo = actionDataInfo;
			GetResponseXml = getResponseXml;
		}

		///<summary>
		/// Metodo da richiamare per avere l'elenco dei valori delle chiavi da eliminare
		/// ma non e' detto che vada bene per tutti gli unparser! (vedi il caso delle banche)
		///</summary>
		//--------------------------------------------------------------------------------
		public Dictionary<string, string> GetKeyValuesToDelete()
		{
			Dictionary<string, string> keyValuesToDelete = new Dictionary<string, string>(); // elenco valori delle chiavi da eliminare

			// TODO: DA IMPLEMENTARE!

			return keyValuesToDelete;
		}
	}
}
