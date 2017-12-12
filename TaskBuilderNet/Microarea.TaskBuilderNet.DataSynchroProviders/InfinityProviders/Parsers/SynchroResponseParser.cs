using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using System.Data.SqlClient;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
    //================================================================================
	internal class SynchroResponseParser
	{
		const string MAXS_NS = "maxs:";
		const string MAXS_NS_FULL = "xmlns:maxs";

		# region // Variabili per Clienti
		public string C_Customer = string.Empty;
		public string C_CustomerFullKey = string.Empty; // nel caso di codici clienti CRM > 12 chr
		public string C_CompanyName = string.Empty;
		public string C_TaxIdNumber = string.Empty;
        public string C_FiscalCode = string.Empty;
        public string C_Address = string.Empty;
		public string C_ZIPCode = string.Empty;
		public string C_City = string.Empty;
		public string C_County = string.Empty;
		public string C_Country = string.Empty;
		public string C_Telephone1 = string.Empty;
		public string C_Telephone2 = string.Empty;
		public string C_Fax = string.Empty;
		public string C_Telex = string.Empty;
		public string C_EMail = string.Empty;
		public string C_ISOCountryCode = string.Empty;
		public string C_ContactPerson = string.Empty;
		public string C_NaturalPerson = string.Empty;
		public string C_Currency = string.Empty;
		public string C_ExternalCode = string.Empty;
		public string C_Region = string.Empty;
		public string C_Internet = string.Empty;
		public string C_SkypeID = string.Empty;
        public string C_Language = string.Empty;

        public string C_IsAPrivatePerson = string.Empty;	// Privato
		public string C_LastName = string.Empty;
		public string C_Name = string.Empty;
		public string C_CityOfBirth = string.Empty;
		public string C_CountyOfBirth = string.Empty;
		public string C_DateOfBirth = string.Empty;
		public string C_Gender = string.Empty;

		// Variabili per Clienti, nodo 4
		public string C4_Payment = string.Empty;
		public string C4_CustSuppKind = string.Empty;
		public string C4_SuspendedTax = string.Empty;
		public string C4_DebitStampCharges = string.Empty;
		public string C4_TaxCode = string.Empty;
		public string C4_DebitCollectionCharges = string.Empty;

		// Variabili per Clienti, nodo 6
		public string C6_CustSuppBank = string.Empty;
		public string C6_CA = string.Empty;
		public string C6_CACheck = string.Empty;
		public string C6_IBAN = string.Empty;

        // Variabili per Clienti, nodo 7
        public string C7_SalePerson = string.Empty;
        public string C7_Area = string.Empty;

        //Variabili per Listino nodo 14
        public string C14_PriceList = string.Empty;

        #endregion

        #region // Variabili per le sedi dei clienti
        private string S_Branch = string.Empty;
		private string S_CustSupp = string.Empty;

		private string S_InfinityCode = string.Empty;
		private string S_Address = string.Empty;
		private string S_ZIPCode = string.Empty;
		private string S_City = string.Empty;
		private string S_County = string.Empty;
		private string S_Telephone1 = string.Empty;
		private string S_EMail = string.Empty;
		private string S_Tipsed = string.Empty;
		private string S_CompanyName = string.Empty;

		private string S_Primary = string.Empty;
		private string S_Payment = string.Empty;
		private string S_Salesperson = string.Empty;
		private string S_Shipping = string.Empty;
		private string S_ShowPricesOnDN = string.Empty;
		private string S_Carrier1 = string.Empty;
		private string S_Carrier2 = string.Empty;
		private string S_Carrier3 = string.Empty;
		# endregion

		#region// Variabili per Ordini Testa
		private string OT_ScontoDiRigaComm = string.Empty;	// il dato proviene dalle righe!
		private string OT_ScontoDiRigaFin = string.Empty;	// il dato proviene dalle righe!
		private string OT_InternalOrdNo = string.Empty;
		private string OT_ExternalOrdNo = string.Empty;
		private string OT_OrderDate = string.Empty;
		private string OT_ExpectedDeliveryDate = string.Empty;
		private string OT_ConfirmedDeliveryDate = string.Empty;
		private string OT_Customer = string.Empty;
		private string OT_Payment = string.Empty;
		private string OT_Currency = string.Empty;
		private string OT_ShipToAddress = string.Empty;
        private string OT_Order = string.Empty;

		private string OT_Fixing = string.Empty;
		private string OT_DOSTATRA = string.Empty;
		private string OT_InvoicingCustomer = string.Empty;
		private string OT_Priority = string.Empty;
		private string OT_SalesPerson = string.Empty;
        private string OT_LanguageOrder = string.Empty;
        private string OT_CollectionCharges = string.Empty;
		private string OT_ShippingCharges = string.Empty;
		private string OT_PackagingCharges = string.Empty;
		private string OT_StampsCharges = string.Empty;
		private string OT_Advance = string.Empty;
		private string OT_Carrier1 = string.Empty;
		private string OT_Carrier2 = string.Empty;
		private string OT_Carrier3 = string.Empty;
		private string OT_Transport = string.Empty;
		private string OT_ExpectedHours = string.Empty;
		private string OT_ExpectedMinuts = string.Empty;
		private string OT_ConfirmedHours = string.Empty;
		private string OT_ConfirmedMinuts = string.Empty;
		private string OT_GrossWeight = string.Empty;
		private string OT_NetWeight = string.Empty;
		private string OT_GrossVolume = string.Empty;
		private string OT_NoOfPacks = string.Empty;
        private string OT_CodPort = string.Empty;
        #endregion

        //--------------------------------------------------------------------------------
        public static SynchroResponseInfo GetResponseInfo(string xmlResponse, ActionToExport action)
		{
			XmlDocument xmlDoc = new XmlDocument();
			SynchroResponseInfo responseInfo = new SynchroResponseInfo();

			try
			{
				xmlDoc.LoadXml(xmlResponse);

				//root
				XmlElement root = xmlDoc.DocumentElement;
				if (string.Compare(root.Name, CRMInfinitySynchroResponseXML.Element.ExecuteSyncroResult, StringComparison.InvariantCultureIgnoreCase) != 0)
					throw (new Exception("No root 'ExecuteSyncroResult' available!"));

				// scorro i nodi figli: sicuramente trovo il nodo <Process> e poi il nodo della Action appena eseguita
				foreach (XmlNode xNode in root.ChildNodes)
				{
					// leggo il nodo Process e i suoi attributi
					if (string.Compare(xNode.Name, CRMInfinitySynchroResponseXML.Element.Process, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						XmlElement processElem = ((XmlElement)xNode);
						if (processElem != null)
						{
							responseInfo.ProcessInfo.Id = processElem.GetAttribute(CRMInfinitySynchroResponseXML.Attribute.Id);
							responseInfo.ProcessInfo.AtomicLevel = processElem.GetAttribute(CRMInfinitySynchroResponseXML.Attribute.AtomicLevel);
							responseInfo.ProcessInfo.GenericResult = processElem.GetAttribute(CRMInfinitySynchroResponseXML.Attribute.GenericResult);
							// se il GenericResult == "ko" dovrebbe essere presente un attributo ErrorMessage con il testo dell'errore (di solito si tratta di sintassi errate nel testo xml inviato)
							responseInfo.ProcessInfo.ErrorMessage = processElem.GetAttribute(CRMInfinitySynchroResponseXML.Attribute.ErrorMessage);

							if (responseInfo.ProcessInfo.GenericResult.Equals("ko"))
								return responseInfo;
						}

						// leggo il primo figlio di <Process> che identifica l'azione eseguita e relativi attributi
						XmlElement actionElem = ((XmlElement)xNode.FirstChild);
						if (actionElem != null)
						{
							// esempio: <MAGO_PRICELISTS_DET Result="(Insert/Update/Delete):Entities processed:13 - Errors:13 "/>
							// oppure: <MAGO_CURRENCIES Result="(Insert/Update/Delete):L&apos;applicazione registrata [DEMO] non è abilitata sulla company [001]"/>
							responseInfo.ProcessInfo.ActionResult.Name = actionElem.Name;
							responseInfo.ProcessInfo.ActionResult.Result = actionElem.GetAttribute(CRMInfinitySynchroResponseXML.Attribute.Result);

							try
							{
								// vado ad estrapolare il numero delle entita' processate ed il numero degli errori (essendo una stringa unica devo andare a lavorare con il Substring)
								// se cambia la stringa ce l'ho in un piede :-(
								int idx = responseInfo.ProcessInfo.ActionResult.Result.IndexOf("Entities processed:");
								if (idx != -1)
								{
									idx += 19;
									responseInfo.ProcessInfo.ActionResult.EntitiesProcessed = responseInfo.ProcessInfo.ActionResult.Result.Substring(idx, responseInfo.ProcessInfo.ActionResult.Result.IndexOf(" -") - idx).Trim();
									idx = responseInfo.ProcessInfo.ActionResult.Result.IndexOf("Errors:") + 7;
									responseInfo.ProcessInfo.ActionResult.Errors = responseInfo.ProcessInfo.ActionResult.Result.Substring(idx, responseInfo.ProcessInfo.ActionResult.Result.Length - idx).Trim();
								}
								else
									responseInfo.ProcessInfo.ActionResult.Errors = "1"; // se non ho trovato la stringa devo forzare le segnalazione di un errore (quale non si sa!!)

								// se nell'attributo Result la stringa e' tipo questa Entities processed:2 - Errors:0
								// imposto lo stato a tutte le entita' come Succeeded e torno subito
								if (responseInfo.ProcessInfo.ActionResult.Errors.Equals("0"))
								{
                                    for (int i = 0; i < action.IsSucceeded.Count; i++)
                                        action.IsSucceeded[i] = true;
                      
									return responseInfo;
								}
							}
							catch (System.ArgumentOutOfRangeException)
							{ }
						}
					}

					// ora parso i nodi della Action che sto eseguendo
					ActionDetail actionDetail = new ActionDetail();
					actionDetail.Name = xNode.Name;
					actionDetail.ApplicationId = ((XmlElement)xNode).GetAttribute(CRMInfinitySynchroResponseXML.Attribute.ApplicationId);

					responseInfo.ActionDetail = actionDetail;

					int countMainAction = 0; // conto il numero di elementi con il nome della action principale

					foreach (XmlNode node in xNode.ChildNodes)
					{
						XmlElement actionElem = ((XmlElement)node);
						
						// considero solo i nodi che finiscono con _ + nome azione + spazio (non guardo le subaction)
						if (!actionElem.Name.EndsWith(string.Format("_{0}", responseInfo.ProcessInfo.ActionResult.Name)))
							continue;

						ActionOperation actOperation = new ActionOperation();
						actOperation.Name = actionElem.Name;

                        bool errorFound = false;
						foreach (XmlAttribute attr in actionElem.Attributes)
						{

							switch (attr.Name)
							{
								case CRMInfinitySynchroResponseXML.Attribute.Crc:
									continue;

								case CRMInfinitySynchroResponseXML.Attribute.ErrorCode:
									actOperation.ErrorCode = attr.Value;
									errorFound = true;
									break;

								case CRMInfinitySynchroResponseXML.Attribute.ErrorOwnerCode:
									actOperation.ErrorOwnerCode = attr.Value;
									errorFound = true;
									break;

								case CRMInfinitySynchroResponseXML.Attribute.ErrorTable:
									actOperation.ErrorTable = attr.Value;
									errorFound = true;
									break;

								case CRMInfinitySynchroResponseXML.Attribute.Error:
									actOperation.Error = attr.Value;
									errorFound = true;
									break;

								default: // i nodi delle chiavi primarie di cui non so il nome
									KeyAttribute ka = new KeyAttribute();
									ka.Name = attr.Name;
									ka.Value = attr.Value;
									actOperation.Keys.Add(ka);
									break;
							}
						}

						if (errorFound)
						{
							if (action.ErrorMessages.Count > countMainAction)
								action.ErrorMessages[countMainAction] = actOperation.Error;
							else
								action.ErrorMessages.Add(actOperation.Error);
						}
						else
						{
							// se non ho trovato errori devo andare a modificare lo stato IsSucceeded
							if (action.IsSucceeded.Count > countMainAction)
								action.IsSucceeded[countMainAction] = true;
						}

						if (actOperation != null)
							responseInfo.ActionDetail.Operations.Add(actOperation);

						countMainAction++;
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Error in SynchroResponseParser::GetResponseInfo {0}", ex.Message));
				responseInfo.ProcessInfo.GenericResult = "ko";
				responseInfo.ProcessInfo.ErrorMessage = ex.Message + "\r\n" + string.Format(Resources.CheckBO, action.Name);
			}

			return responseInfo;
		}

		//--------------------------------------------------------------------------------
		public string GetProcessId(string xmlString)
		{
			try
			{
				XDocument xDoc = XDocument.Parse(xmlString);
				return xDoc.Element("ExecuteSyncroResult").Element("Process").Attribute("id").Value;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

		//--------------------------------------------------------------------------------
        public List<ActionToImport> GetCustomersString(string xmlString)
		{
			try
			{
                return ParseInputFilesCustomers(xmlString).ToList<ActionToImport>();
			}
			catch (Exception)
			{
                return new List<ActionToImport>();
			}
		}

		//--------------------------------------------------------------------------------
        public List<ActionToImport> GetBranchesString(string xmlString)
		{
			try
			{
                return ParseInputFilesBranches(xmlString).ToList<ActionToImport>();
			}
			catch (Exception)
			{
                return new List<ActionToImport>(); ;
			}
		}

        //--------------------------------------------------------------------------------
        public List<ActionToImport> GetOrdersString(string xmlString, string authenticationToken)
        {
            try
            {
                return ParseInputFilesOrders(xmlString, authenticationToken).ToList<ActionToImport>();
            }
            catch (Exception)
            {
                return new List<ActionToImport>(); ;
            }
        }

        //--------------------------------------------------------------------------------
        public List<ActionToImport> GetOrdersStringErp(string xmlString, string authenticationToken)
        {
            try
            {
                return ParseInputFilesOrdersErp(xmlString, authenticationToken).ToList<ActionToImport>();
            }
            catch (Exception)
            {
                return new List<ActionToImport>(); ;
            }
        }

        //-------------------------------------------------------------------------------
        //-----------------------------    CLIENTI    ------------------------------------
        //-------------------------------------------------------------------------------
        private IEnumerable<ActionToImport> ParseInputFilesCustomers(string xmlString)
		{
			XDocument xDoc = null;

			IEnumerable<XElement> add_MAGO_CUSTOMERS_OUT_Elem = null;
            string processId = GetProcessId(xmlString);
           

			try
			{
				xDoc = XDocument.Parse(xmlString);
				add_MAGO_CUSTOMERS_OUT_Elem = xDoc.Element("ExecuteSyncroResult").Element("MAGO_CUSTOMERS_OUT").Elements("Add_MAGO_CUSTOMERS_OUT");
			}
			catch (Exception)
			{
				yield break;
			}

			foreach (XElement addElem in add_MAGO_CUSTOMERS_OUT_Elem)
			{
                ActionToImport ati = new ActionToImport();
                ati.ProccessId = processId;
                ati.ActionName = "MAGO_CUSTOMERS_OUT";

				ParseCustomerMainAttribute(addElem);

				ati.InfinityKeys.Add(string.Format("KSCODSOG_K={0}", C_CustomerFullKey));
                ati.InfinityKeys.Add("KSCODSOG_K_APPREG={0}");
                ati.InfinityKeys.Add("KSTIPSOG_K=CLI");
                ati.InfinityKeys.Add("KSTIPSOG_K_APPREG=CLI");

                // se elemento continene errori non faccio il persing 
                // compongo solo le chiavi per fare il rollback e vado avanti
                if (IfResponseContainsErrors(addElem.ToString()))
                {
                    ati.DoRollback = true;
                    yield return ati;
                    continue;
                }
             
                try
                {
                    ParseCustomerNode00004(addElem.Element("BO_MAGO_CUSTOMERS_OUT_00004").Element("MAGO_CUSTOMERS_OUT_00004"));
                }
                catch (Exception) { }
				
                try
                {
                    ParseCustomerNode00006(addElem.Element("BO_MAGO_CUSTOMERS_OUT_00006").Element("MAGO_CUSTOMERS_OUT_00006"));
                }
                catch (Exception) { }

                try
                {
                    ParseCustomerNode00007(addElem.Element("BO_MAGO_CUSTOMERS_OUT_00007").Element("MAGO_CUSTOMERS_OUT_00007"));
                }
                catch (Exception) { }
                try
                {
                    ParseCustomerNode00014(addElem.Element("BO_MAGO_CUSTOMERS_OUT_00004").Element("MAGO_CUSTOMERS_OUT_00004").Element("BO_MAGO_CUSTOMERS_OUT_00014").Element("MAGO_CUSTOMERS_OUT_00014"));
                }

                catch (Exception) { }

                ati.MagoXml=GetMagoXmlForCustomers();

                 yield return ati;
			}

			yield break;
		}

		//-------------------------------------------------------------------------------
		private void ParseCustomerMainAttribute(XElement customerMainNode)
		{
			if (customerMainNode == null)
				return;

			C_Customer = string.Empty;
			C_CustomerFullKey = string.Empty;
			C_CompanyName = string.Empty;
			C_TaxIdNumber = string.Empty;
			C_FiscalCode = string.Empty;
            C_Address = string.Empty;
			C_ZIPCode = string.Empty;
			C_City = string.Empty;
			C_County = string.Empty;
			C_Country = string.Empty;
			C_Telephone1 = string.Empty;
			C_Telephone2 = string.Empty;
			C_Fax = string.Empty;
			C_Telex = string.Empty;
			C_EMail = string.Empty;
			C_ISOCountryCode = string.Empty;
			C_ContactPerson = string.Empty;
			C_NaturalPerson = string.Empty;
			C_Currency = string.Empty;
			C_ExternalCode = string.Empty;
			C_IsAPrivatePerson = string.Empty;	// Privato
			C_LastName = string.Empty;
			C_Name = string.Empty;
			C_CityOfBirth = string.Empty;
			C_CountyOfBirth = string.Empty;
			C_DateOfBirth = string.Empty;
			C_Gender = string.Empty;
			C_Region = string.Empty;
			C_Internet = string.Empty;
			C_SkypeID = string.Empty;
            C_Language = string.Empty;

            if (customerMainNode.Attribute("KSCODSOG_K") != null)
			{
				C_CustomerFullKey = customerMainNode.Attribute("KSCODSOG_K").Value;						// Ricerca di un attributo
				C_Customer = C_CustomerFullKey.Substring(0, Math.Min(12, C_CustomerFullKey.Length));	// MAX 12 caratteri
			}
			if (customerMainNode.Attribute("KSDESCRI") != null)
			{
				C_CompanyName = customerMainNode.Attribute("KSDESCRI").Value;
				if (customerMainNode.Attribute("KSDESADD") != null)
					C_CompanyName += " " + customerMainNode.Attribute("KSDESADD").Value;
			}
			if (customerMainNode.Attribute("KSCODIVA") != null)
				C_TaxIdNumber = customerMainNode.Attribute("KSCODIVA").Value;
			if (customerMainNode.Attribute("KSCODFIS") != null)
				C_FiscalCode = customerMainNode.Attribute("KSCODFIS").Value;

			if (customerMainNode.Attribute("Ofstate") != null)
				C_Region = customerMainNode.Attribute("Ofstate").Value;
			if (C_Region == "LOM") C_Region = "Lombardia";
			if (C_Region == "LIG") C_Region = "Liguria";

			if (customerMainNode.Attribute("Ofiptype") != null && customerMainNode.Attribute("Ofiptype").Value == "Skype")
				if (customerMainNode.Attribute("Ofipphone") != null)
					C_SkypeID = customerMainNode.Attribute("Ofipphone").Value;

            if (customerMainNode.Attribute("Oflang") != null)
            {
                C_Language = customerMainNode.Attribute("Oflang").Value;
            }

            if (customerMainNode.Attribute("Ofweb") != null)
			{
				C_Internet = customerMainNode.Attribute("Ofweb").Value;
				if (customerMainNode.Attribute("Ofweb2") != null && customerMainNode.Attribute("Ofweb2").Value != string.Empty)
					C_Internet += "; " + customerMainNode.Attribute("Ofweb2").Value;
			}

			if (customerMainNode.Attribute("Ofaddress") != null)
			{
				C_Address = customerMainNode.Attribute("Ofaddress").Value;
				if (customerMainNode.Attribute("Ofcivnum") != null)
					C_Address += " " + customerMainNode.Attribute("Ofcivnum").Value;
				if (customerMainNode.Attribute("Ofaddress2") != null)
					C_Address += " " + customerMainNode.Attribute("Ofaddress2").Value;
			}
			if (customerMainNode.Attribute("Ofcap") != null)
				C_ZIPCode = customerMainNode.Attribute("Ofcap").Value;
			if (customerMainNode.Attribute("Oflocal") != null)
				C_City = customerMainNode.Attribute("Oflocal").Value;
			if (customerMainNode.Attribute("Ofprov") != null)
				C_County = customerMainNode.Attribute("Ofprov").Value;
			if (customerMainNode.Attribute("Ofnation") != null && (customerMainNode.Attribute("Ofnation").Value == "ITA" || customerMainNode.Attribute("Ofnation").Value == "IT"))
				C_Country = "Italia"; // Da Infinity mi arriva ITA o IT

			// F=Fisso , C=Cellulare , X=Fax , Y=Altro
			if (customerMainNode.Attribute("Ofphonetyp") != null && customerMainNode.Attribute("Ofphone") != null)
			{
				if (customerMainNode.Attribute("Ofphonetyp").Value == "F")
					C_Telephone1 = customerMainNode.Attribute("Ofprephone") == null ? customerMainNode.Attribute("Ofphone").Value : customerMainNode.Attribute("Ofprephone").Value + customerMainNode.Attribute("Ofphone").Value;
				if (customerMainNode.Attribute("Ofphonetyp").Value == "C")
					C_Telephone2 = customerMainNode.Attribute("Ofprephone") == null ? customerMainNode.Attribute("Ofphone").Value : customerMainNode.Attribute("Ofprephone").Value + customerMainNode.Attribute("Ofphone").Value;
				if (customerMainNode.Attribute("Ofphonetyp").Value == "X")
					C_Fax = customerMainNode.Attribute("Ofprephone") == null ? customerMainNode.Attribute("Ofphone").Value : customerMainNode.Attribute("Ofprephone").Value + customerMainNode.Attribute("Ofphone").Value;
				if (customerMainNode.Attribute("Ofphonetyp").Value == "Y")
					C_Telex = customerMainNode.Attribute("Ofprephone") == null ? customerMainNode.Attribute("Ofphone").Value : customerMainNode.Attribute("Ofprephone").Value + customerMainNode.Attribute("Ofphone").Value;
			}

			if (customerMainNode.Attribute("Ofphonetyp2") != null && customerMainNode.Attribute("Ofphone2") != null)
			{
				if (customerMainNode.Attribute("Ofphonetyp2").Value == "F")
					C_Telephone1 = customerMainNode.Attribute("Ofprephone2") == null ? customerMainNode.Attribute("Ofphone2").Value : customerMainNode.Attribute("Ofprephone2").Value + customerMainNode.Attribute("Ofphone2").Value;
				if (customerMainNode.Attribute("Ofphonetyp2").Value == "C")
					C_Telephone2 = customerMainNode.Attribute("Ofprephone2") == null ? customerMainNode.Attribute("Ofphone2").Value : customerMainNode.Attribute("Ofprephone2").Value + customerMainNode.Attribute("Ofphone2").Value;
				if (customerMainNode.Attribute("Ofphonetyp2").Value == "X")
					C_Fax = customerMainNode.Attribute("Ofprephone2") == null ? customerMainNode.Attribute("Ofphone2").Value : customerMainNode.Attribute("Ofprephone2").Value + customerMainNode.Attribute("Ofphone2").Value;
				if (customerMainNode.Attribute("Ofphonetyp2").Value == "Y")
					C_Telex = customerMainNode.Attribute("Ofprephone2") == null ? customerMainNode.Attribute("Ofphone2").Value : customerMainNode.Attribute("Ofprephone2").Value + customerMainNode.Attribute("Ofphone2").Value;
			}

			if (customerMainNode.Attribute("Ofphonetyp3") != null && customerMainNode.Attribute("Ofphone3") != null)
			{
				if (customerMainNode.Attribute("Ofphonetyp3").Value == "F")
					C_Telephone1 = customerMainNode.Attribute("Ofprephone3") == null ? customerMainNode.Attribute("Ofphone3").Value : customerMainNode.Attribute("Ofprephone3").Value + customerMainNode.Attribute("Ofphone3").Value;
				if (customerMainNode.Attribute("Ofphonetyp3").Value == "C")
					C_Telephone2 = customerMainNode.Attribute("Ofprephone3") == null ? customerMainNode.Attribute("Ofphone3").Value : customerMainNode.Attribute("Ofprephone3").Value + customerMainNode.Attribute("Ofphone3").Value;
				if (customerMainNode.Attribute("Ofphonetyp3").Value == "X")
					C_Fax = customerMainNode.Attribute("Ofprephone3") == null ? customerMainNode.Attribute("Ofphone3").Value : customerMainNode.Attribute("Ofprephone3").Value + customerMainNode.Attribute("Ofphone3").Value;
				if (customerMainNode.Attribute("Ofphonetyp3").Value == "Y")
					C_Telex = customerMainNode.Attribute("Ofprephone3") == null ? customerMainNode.Attribute("Ofphone3").Value : customerMainNode.Attribute("Ofprephone3").Value + customerMainNode.Attribute("Ofphone3").Value;
			}

			if (customerMainNode.Attribute("Ofphonetyp4") != null && customerMainNode.Attribute("Ofphone4") != null)
			{
				if (customerMainNode.Attribute("Ofphonetyp4").Value == "F")
					C_Telephone1 = customerMainNode.Attribute("Ofprephone4") == null ? customerMainNode.Attribute("Ofphone4").Value : customerMainNode.Attribute("Ofprephone4").Value + customerMainNode.Attribute("Ofphone4").Value;
				if (customerMainNode.Attribute("Ofphonetyp4").Value == "C")
					C_Telephone2 = customerMainNode.Attribute("Ofprephone4") == null ? customerMainNode.Attribute("Ofphone4").Value : customerMainNode.Attribute("Ofprephone4").Value + customerMainNode.Attribute("Ofphone4").Value;
				if (customerMainNode.Attribute("Ofphonetyp4").Value == "X")
					C_Fax = customerMainNode.Attribute("Ofprephone4") == null ? customerMainNode.Attribute("Ofphone4").Value : customerMainNode.Attribute("Ofprephone4").Value + customerMainNode.Attribute("Ofphone4").Value;
				if (customerMainNode.Attribute("Ofphonetyp4").Value == "Y")
					C_Telex = customerMainNode.Attribute("Ofprephone4") == null ? customerMainNode.Attribute("Ofphone4").Value : customerMainNode.Attribute("Ofprephone4").Value + customerMainNode.Attribute("Ofphone4").Value;
			}

			if (customerMainNode.Attribute("Ofmail") != null)
			{
				C_EMail = customerMainNode.Attribute("Ofmail").Value;
				if (customerMainNode.Attribute("Ofemail2") != null && customerMainNode.Attribute("Ofemail2").Value != string.Empty)
					C_EMail += "; " + customerMainNode.Attribute("Ofemail2").Value;
			}
			if (customerMainNode.Attribute("Ofnation") != null)
				C_ISOCountryCode = customerMainNode.Attribute("Ofnation").Value;	// Su Infinity "ITA" ma arriva trascodificato come "IT"

			// Persona fisica (privato) o ditta individuale
			if (customerMainNode.Attribute("KSFORGIU") != null && (customerMainNode.Attribute("KSFORGIU").Value == "PER" || customerMainNode.Attribute("KSFORGIU").Value == "IND"))
			{
				C_NaturalPerson = "true";
				if (customerMainNode.Attribute("Flgprv") != null)
					C_IsAPrivatePerson = customerMainNode.Attribute("Flgprv").Value == "0" ? "false" : "true";	// privato
				if (customerMainNode.Attribute("Cosurname") != null)
					C_LastName = customerMainNode.Attribute("Cosurname").Value;
				if (customerMainNode.Attribute("Coname") != null)
					C_Name = customerMainNode.Attribute("Coname").Value;
				if (customerMainNode.Attribute("Cobornloc") != null)
					C_CityOfBirth = customerMainNode.Attribute("Cobornloc").Value;
				if (customerMainNode.Attribute("Coproborn") != null)
					C_CountyOfBirth = customerMainNode.Attribute("Coproborn").Value;
				if (customerMainNode.Attribute("Coborndate") != null)
					C_DateOfBirth = customerMainNode.Attribute("Coborndate").Value;
				if (C_DateOfBirth == "0100-01-01")
					C_DateOfBirth = string.Empty;
				if (customerMainNode.Attribute("Cosex") != null)
					C_Gender = customerMainNode.Attribute("Cosex").Value == "M" ? "2097152" : "2097153";
			}

			if (customerMainNode.Attribute("Codval") != null)
				C_Currency = customerMainNode.Attribute("Codval").Value;

			if (customerMainNode.Attribute("KSCODEST") != null)
				C_ExternalCode = customerMainNode.Attribute("KSCODEST").Value;	// campo di Infinity relativo al Codice esterno es. "CodiceEsterno"
			else
				if (customerMainNode.Attribute("KSCODCOM") != null)
					C_ExternalCode = customerMainNode.Attribute("KSCODCOM").Value;	// campo di Infinity relativo al Codice del cliente es. "000000000000558"
		}

		// Dati contabili (non della sede)
		//-------------------------------------------------------------------------------
		private void ParseCustomerNode00004(XElement customer00004Node)
		{
			C4_Payment = string.Empty;
			C4_CustSuppKind = string.Empty;
			C4_SuspendedTax = string.Empty;
			C4_DebitStampCharges = string.Empty;
			C4_TaxCode = string.Empty;
			C4_DebitCollectionCharges = string.Empty;

			XElement node = customer00004Node.Element("CCCODPAG");	// Ricerca di un sottonodo
			if (node != null)
				C4_Payment = node.Value;

            node = customer00004Node.Element("CCFLESIG");
			if (node != null)
				C4_SuspendedTax = node.Value == "0" ? "false" : "true";

			node = customer00004Node.Element("CCBOLFAT");
			if (node != null)
				C4_DebitStampCharges = node.Value == "0" ? "false" : "true";

			node = customer00004Node.Element("CCIVAESE");
			if (node != null)
				C4_TaxCode = node.Value;

			node = customer00004Node.Element("CCESCINC");
			{
				if (node.Value == "0")	// Se su Infinity il Flag "Escludi applicazione spese di incasso" = 0 allora su Mago il flag "Addebita spese di incasso" vale "1" 
					C4_DebitCollectionCharges = "true";
				else
					C4_DebitCollectionCharges = "false";
			}
		}

		// Dati banca (non della sede). Su infinity ci possono essere + banche su Mago no, quindi potrei avere + nodi di tipo 00006.
		// Cercando un sottonodo senza specificare in quale nodo di tipo 6 sto cercando, prendo sempre i dati del primo nodo
		//-------------------------------------------------------------------------------
		private void ParseCustomerNode00006(XElement customer00006Node)
		{
			C6_CustSuppBank = string.Empty;
			C6_CA = string.Empty;
			C6_CACheck = string.Empty;
			C6_IBAN = string.Empty;

			XElement node = customer00006Node.Element("BACODBAN");	// Ricerca di un sottonodo
			if (node != null)
				C6_CustSuppBank = node.Value;
			node = customer00006Node.Element("BANUMCOR");
			if (node != null)
				C6_CA = node.Value;
			node = customer00006Node.Element("BACODCIE");
			if (node != null)
				C6_CACheck = node.Value;
			node = customer00006Node.Element("BACODIBA");
			if (node != null)
				C6_IBAN = node.Value;
		}

        private void ParseCustomerNode00007(XElement customer00007Node)
        {
            C6_CustSuppBank = string.Empty;
            C6_CA = string.Empty;
            C6_CACheck = string.Empty;
            C6_IBAN = string.Empty;

            XElement node = customer00007Node.Element("DPCODAGE");  // Ricerca di un sottonodo
            if (node != null)
                C7_SalePerson = node.Value;
            node = customer00007Node.Element("DPZONCOM");
            if (node != null)
                C7_Area = node.Value;
        }

        private void ParseCustomerNode00014(XElement customer00014Node)
        {
            C6_CustSuppBank = string.Empty;
            C6_CA = string.Empty;
            C6_CACheck = string.Empty;
            C6_IBAN = string.Empty;

            XElement node = customer00014Node.Element("LICODLIS_K");  // Ricerca di un sottonodo
            if (node != null)
                C14_PriceList = node.Value;
        }

        //-------------------------------------------------------------------------------
        private string GetMagoXmlForCustomers()
		{
            #region EsempioXML
            /*
<?xml version="1.0"?>
<maxs:Customers xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfinitySyncroConnector.xsd" tbNamespace="Document.ERP.CustomersSuppliers.Documents.Customers" xTechProfile="Default">
	<maxs:Data>
		<maxs:CustomersSuppliers master="true">
			<maxs:CustSuppType>3211264</maxs:CustSuppType>
			<maxs:CustSupp>NUOVO-30</maxs:CustSupp>
			<maxs:CompanyName>NUOVO-30 modificato</maxs:CompanyName>
			<maxs:TaxIdNumber>12345678913</maxs:TaxIdNumber>
			<maxs:FiscalCode>12345678913</maxs:FiscalCode>
			<maxs:Address></maxs:Address>
			<maxs:ZIPCode></maxs:ZIPCode>
			<maxs:City></maxs:City>
			<maxs:County></maxs:County>
			<maxs:Country></maxs:Country>
			<maxs:Telephone1></maxs:Telephone1>
			<maxs:Telephone2></maxs:Telephone2>
			<maxs:Fax></maxs:Fax>
			<maxs:EMail></maxs:EMail>
			<maxs:ISOCountryCode>IT</maxs:ISOCountryCode>
			<maxs:ContactPerson></maxs:ContactPerson>
			<maxs:NaturalPerson>false</maxs:NaturalPerson>
			<maxs:Payment></maxs:Payment>
			<maxs:Currency></maxs:Currency>
			<maxs:Disabled>false</maxs:Disabled>
			<maxs:ExternalCode></maxs:ExternalCode>
		</maxs:CustomersSuppliers>
		<maxs:OtherBranches>
		... ... ... 
		</maxs:OtherBranches>
	</maxs:Data>
</maxs:Customers>			 
*/
            #endregion

            string xml = string.Empty;

			xml += "<?xml version=\"1.0\"?>";
			xml += "<" + MAXS_NS + "Customers " + MAXS_NS_FULL + "=\"http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfinitySyncroConnector";
			xml += ".xsd\" tbNamespace=\"Document.ERP.CustomersSuppliers.Documents.Customers\" xTechProfile=\"InfinitySyncroConnector\">";
			xml += "<" + MAXS_NS + "Data>";
			xml += "<" + MAXS_NS + "CustomersSuppliers master=\"true\">";
			xml += "<" + MAXS_NS + "CustSuppType>3211264</" + MAXS_NS + "CustSuppType>";		// Enumerativo cablato: 3211264=Clienti

			// il Customer non viene assegnato perche' consideriamo che ci sia l'autonumerazione
			if (!string.IsNullOrEmpty(C_CompanyName))
				xml += "<" + MAXS_NS + "CompanyName>" + C_CompanyName + "</" + MAXS_NS + "CompanyName>";
			if (!string.IsNullOrEmpty(C_TaxIdNumber))
				xml += "<" + MAXS_NS + "TaxIdNumber>" + C_TaxIdNumber + "</" + MAXS_NS + "TaxIdNumber>";
            if (!string.IsNullOrEmpty(C_FiscalCode))
                xml += "<" + MAXS_NS + "FiscalCode>" + C_FiscalCode + "</" + MAXS_NS + "FiscalCode>";
            if (!string.IsNullOrEmpty(C14_PriceList))
                xml += "<" + MAXS_NS + "PriceList>" + C14_PriceList + "</" + MAXS_NS + "PriceList>";
            if (!string.IsNullOrEmpty(C_Address))
				xml += "<" + MAXS_NS + "Address>" + C_Address + "</" + MAXS_NS + "Address>";
			if (!string.IsNullOrEmpty(C_ZIPCode))
				xml += "<" + MAXS_NS + "ZIPCode>" + C_ZIPCode + "</" + MAXS_NS + "ZIPCode>";
			if (!string.IsNullOrEmpty(C_City))
				xml += "<" + MAXS_NS + "City>" + C_City + "</" + MAXS_NS + "City>";
			if (!string.IsNullOrEmpty(C_County))
				xml += "<" + MAXS_NS + "County>" + C_County + "</" + MAXS_NS + "County>";
			if (!string.IsNullOrEmpty(C_Country))
				xml += "<" + MAXS_NS + "Country>" + C_Country + "</" + MAXS_NS + "Country>";
			if (!string.IsNullOrEmpty(C_Telephone1))
				xml += "<" + MAXS_NS + "Telephone1>" + C_Telephone1 + "</" + MAXS_NS + "Telephone1>";
			if (!string.IsNullOrEmpty(C_Telephone2))
				xml += "<" + MAXS_NS + "Telephone2>" + C_Telephone2 + "</" + MAXS_NS + "Telephone2>";
			if (!string.IsNullOrEmpty(C_Fax))
				xml += "<" + MAXS_NS + "Fax>" + C_Fax + "</" + MAXS_NS + "Fax>";
			if (!string.IsNullOrEmpty(C_Telex))
				xml += "<" + MAXS_NS + "Telex>" + C_Telex + "</" + MAXS_NS + "Telex>";
			if (!string.IsNullOrEmpty(C_EMail))
				xml += "<" + MAXS_NS + "EMail>" + C_EMail + "</" + MAXS_NS + "EMail>";
			if (!string.IsNullOrEmpty(C_ISOCountryCode))
				xml += "<" + MAXS_NS + "ISOCountryCode>" + C_ISOCountryCode + "</" + MAXS_NS + "ISOCountryCode>";
			if (!string.IsNullOrEmpty(C_ContactPerson))
				xml += "<" + MAXS_NS + "ContactPerson>" + C_ContactPerson + "</" + MAXS_NS + "ContactPerson>";
			if (!string.IsNullOrEmpty(C_NaturalPerson))
				xml += "<" + MAXS_NS + "NaturalPerson>" + C_NaturalPerson + "</" + MAXS_NS + "NaturalPerson>";
			if (!string.IsNullOrEmpty(C_Currency))
				xml += "<" + MAXS_NS + "Currency>" + C_Currency + "</" + MAXS_NS + "Currency>";
			if (!string.IsNullOrEmpty(C_ExternalCode))
				xml += "<" + MAXS_NS + "ExternalCode>" + C_ExternalCode + "</" + MAXS_NS + "ExternalCode>";
			if (!string.IsNullOrEmpty(C_Region))
				xml += "<" + MAXS_NS + "Region>" + C_Region + "</" + MAXS_NS + "Region>";

			// TODO Per una corretta gestione bisogna modificare le BO di Infinity (come fatto per ordini e sedi)
			// Pero' per ordini e sedi la lingua non e' gestita in questo programma!
			//			if (!string.IsNullOrEmpty(C_Language))
			//				xml += "<" + MAXS_NS + "Language>"			+ C_Language + "</"			+ MAXS_NS + "Language>";

			if (!string.IsNullOrEmpty(C_SkypeID))
				xml += "<" + MAXS_NS + "SkypeID>" + C_SkypeID + "</" + MAXS_NS + "SkypeID>";
			if (!string.IsNullOrEmpty(C_Internet))
				xml += "<" + MAXS_NS + "Internet>" + C_Internet + "</" + MAXS_NS + "Internet>";

            if (!string.IsNullOrEmpty(C_Language))
                xml += "<" + MAXS_NS + "Language>" + C_Language + "</" + MAXS_NS + "Language>";

            if (!string.IsNullOrEmpty(C4_Payment))
				xml += "<" + MAXS_NS + "Payment>" + C4_Payment + "</" + MAXS_NS + "Payment>";
			if (!string.IsNullOrEmpty(C4_CustSuppKind))
				xml += "<" + MAXS_NS + "CustSuppKind>" + C4_CustSuppKind + "</" + MAXS_NS + "CustSuppKind>";

			if (!string.IsNullOrEmpty(C6_CustSuppBank))
				xml += "<" + MAXS_NS + "CustSuppBank>" + C6_CustSuppBank + "</" + MAXS_NS + "CustSuppBank>";
			if (!string.IsNullOrEmpty(C6_CA))
				xml += "<" + MAXS_NS + "CA>" + C6_CA + "</" + MAXS_NS + "CA>";
			if (!string.IsNullOrEmpty(C6_CACheck))
				xml += "<" + MAXS_NS + "CACheck>" + C6_CACheck + "</" + MAXS_NS + "CACheck>";
			if (!string.IsNullOrEmpty(C6_IBAN))
				xml += "<" + MAXS_NS + "IBAN>" + C6_IBAN + "</" + MAXS_NS + "IBAN>";

			xml += "</" + MAXS_NS + "CustomersSuppliers>";

			// Se scrivo <maxs:Options></maxs:Options> senza indicare nulla, in pratica cancello tutti i campi del nodo Options !!!!!!
			if (!string.IsNullOrEmpty(C4_SuspendedTax) || !string.IsNullOrEmpty(C4_DebitStampCharges) || !string.IsNullOrEmpty(C4_TaxCode) ||
				!string.IsNullOrEmpty(C4_DebitCollectionCharges) || !string.IsNullOrEmpty(C_IsAPrivatePerson) || !string.IsNullOrEmpty(C7_SalePerson) || !string.IsNullOrEmpty(C7_Area))
			{
				xml += "<" + MAXS_NS + "Options>";	// MA_CustSuppCustomerOptions
				if (!string.IsNullOrEmpty(C4_SuspendedTax))
					xml += "<" + MAXS_NS + "SuspendedTax>" + C4_SuspendedTax + "</" + MAXS_NS + "SuspendedTax>";
				if (!string.IsNullOrEmpty(C4_DebitStampCharges))
					xml += "<" + MAXS_NS + "DebitStampCharges>" + C4_DebitStampCharges + "</" + MAXS_NS + "DebitStampCharges>";
				if (!string.IsNullOrEmpty(C4_TaxCode))
					xml += "<" + MAXS_NS + "TaxCode>" + C4_TaxCode + "</" + MAXS_NS + "TaxCode>";
				if (!string.IsNullOrEmpty(C4_DebitCollectionCharges))
					xml += "<" + MAXS_NS + "DebitCollectionCharges>" + C4_DebitCollectionCharges + "</" + MAXS_NS + "DebitCollectionCharges>";
				if (!string.IsNullOrEmpty(C_IsAPrivatePerson))
					xml += "<" + MAXS_NS + "IsAPrivatePerson>" + C_IsAPrivatePerson + "</" + MAXS_NS + "IsAPrivatePerson>";
                if (!string.IsNullOrEmpty(C7_SalePerson))
                    xml += "<" + MAXS_NS + "SalesPerson>" + C7_SalePerson + "</" + MAXS_NS + "SalesPerson>";
                if (!string.IsNullOrEmpty(C7_Area))
                    xml += "<" + MAXS_NS + "Area>" + C7_Area + "</" + MAXS_NS + "Area>";
                xml += "</" + MAXS_NS + "Options>";
			}

			// Se scrivo <maxs:NaturalPerson></maxs:NaturalPerson> senza indicare nulla, in pratica cancello tutti i campi del nodo NaturalPerson !!!!!!
			if (!string.IsNullOrEmpty(C_LastName) || !string.IsNullOrEmpty(C_Name) || !string.IsNullOrEmpty(C_CityOfBirth) ||
				!string.IsNullOrEmpty(C_CountyOfBirth) || !string.IsNullOrEmpty(C_DateOfBirth) || !string.IsNullOrEmpty(C_Gender))
			{
				xml += "<" + MAXS_NS + "NaturalPerson>";	// MA_CustSuppNaturalPerson
				if (!string.IsNullOrEmpty(C_LastName))
					xml += "<" + MAXS_NS + "LastName>" + C_LastName + "</" + MAXS_NS + "LastName>";
				if (!string.IsNullOrEmpty(C_Name))
					xml += "<" + MAXS_NS + "Name>" + C_Name + "</" + MAXS_NS + "Name>";
				if (!string.IsNullOrEmpty(C_CityOfBirth))
					xml += "<" + MAXS_NS + "CityOfBirth>" + C_CityOfBirth + "</" + MAXS_NS + "CityOfBirth>";
				if (!string.IsNullOrEmpty(C_CountyOfBirth))
					xml += "<" + MAXS_NS + "CountyOfBirth>" + C_CountyOfBirth + "</" + MAXS_NS + "CountyOfBirth>";
				if (!string.IsNullOrEmpty(C_DateOfBirth))
					xml += "<" + MAXS_NS + "DateOfBirth>" + C_DateOfBirth + "</" + MAXS_NS + "DateOfBirth>";
				if (!string.IsNullOrEmpty(C_Gender))
					xml += "<" + MAXS_NS + "Gender>" + C_Gender + "</" + MAXS_NS + "Gender>";
				xml += "</" + MAXS_NS + "NaturalPerson>";
			}

			xml += "</" + MAXS_NS + "Data>";
			xml += "</" + MAXS_NS + "Customers>";

			return xml;
		}


		//
		//-------------------------------------------------------------------------------
		//-------------------------    SEDI DEI CLIENTI    ------------------------------
		//-------------------------------------------------------------------------------
        private IEnumerable<ActionToImport> ParseInputFilesBranches(string xmlString)
		{
			XDocument xDoc = null;

			IEnumerable<XElement> add_MAGO_BRANCHES_OUT_Elem = null;
            string processId = GetProcessId(xmlString);

			try
			{
				xDoc = XDocument.Parse(xmlString);
				add_MAGO_BRANCHES_OUT_Elem = xDoc.Element("ExecuteSyncroResult").Element("MAGO_BRANCHES_OUT").Elements("Add_MAGO_BRANCHES_OUT");
			}
			catch (Exception)
			{
				yield break;
			}

			foreach (XElement addElem in add_MAGO_BRANCHES_OUT_Elem)
			{
                ActionToImport ati = new ActionToImport();
                ati.ProccessId = processId;
                ati.ActionName = "MAGO_BRANCHES_OUT";

				ParseBranchesMainAttribute(addElem);

                ati.InfinityKeys.Add("DPTIPSOG_K_APPREG=CLI");
                ati.InfinityKeys.Add("DPTIPSOG_K=CLI");
                ati.InfinityKeys.Add(string.Format("DPCODSOG_K={0}", S_CustSupp));
                ati.InfinityKeys.Add(string.Format("DPCODSOG_K_APPREG={0}", S_CustSupp));
                ati.InfinityKeys.Add(string.Format("DPCODCOM_K={0}", S_InfinityCode));
                ati.InfinityKeys.Add(string.Format("DPCODCOM_K_APPREG=CL-{0}", S_CustSupp));
                ati.InfinityKeys.Add(string.Format("DPCODSED_K={0}", S_Branch));
                ati.InfinityKeys.Add("DPCODSED_K_APPREG={0}");
                ati.MagoKey = S_Branch;
                // aggiungere le chiavi. Bisogna capire come


                // se elemento continene errori non faccio il persing 
                // compongo solo le chiavi per fare il rollback e vado avanti
                if (IfResponseContainsErrors(addElem.ToString()))
                {
                    ati.DoRollback = true;
                    yield return ati;
                    continue;
                }

                
                ati.MagoXml=GetMagoXmlForBranches();
                yield return ati;
			}
		}

		//-------------------------------------------------------------------------------
		private void ParseBranchesMainAttribute(XElement branchesMainNode)
		{
			if (branchesMainNode == null)
				return;

			S_Branch = string.Empty;	// DPCODSED_K="sede4"
			S_CustSupp = string.Empty;	// DPCODSOG_K="02101"

			S_InfinityCode = string.Empty;	// DPCODCOM_K="000000000006312"
			S_Address = string.Empty;	// Address="via sede4" + Civnum="" 
			S_ZIPCode = string.Empty;	// Cap="30010" 
			S_City = string.Empty;	// Local="VENEZIA" 
			S_County = string.Empty;	// Provin="VE" 
			S_Telephone1 = string.Empty;	// Prephone="" + Phone="" 
			S_EMail = string.Empty;	// Mail="" 
			S_Tipsed = string.Empty;	// Tipsed="MAG"
			S_CompanyName = string.Empty;	// Dessed="sede4" 

			S_Primary = string.Empty;	// Primary="N" 
			S_Payment = string.Empty;	// DPCODPAG="" 
			S_Salesperson = string.Empty;	// DPCODAGE="" 
			S_Shipping = string.Empty;	// DPMODTRA="" 
			S_ShowPricesOnDN = string.Empty;	// DPPREBOL="0" 
			S_Carrier1 = string.Empty;	// DPCODVE1="" 
			S_Carrier2 = string.Empty;	// DPCODVE2="" 
			S_Carrier3 = string.Empty;	// DPCODVE3="" 

			// Prendo gli 8 caratteri + a dx perche' Infinity e' di 10 e autonumera le sedi con 0000000010 , 0000000020 ecc. se prendo a sx prendo sempre 8 zeri!
			if (branchesMainNode.Attribute("DPCODSED_K") != null)
			{
				S_Branch = branchesMainNode.Attribute("DPCODSED_K").Value;			// Ricerca di un attributo
				if (S_Branch.Length > 8)
					S_Branch = S_Branch.Substring(S_Branch.Length - 8, 8);				// MAX 8 caratteri, prendo gli 8 caratteri + a destra
				else
					S_Branch = S_Branch.Substring(0, Math.Min(8, S_Branch.Length));		// MAX 8 caratteri
			}
			if (branchesMainNode.Attribute("DPCODSOG_K") != null)
			{
				S_CustSupp = branchesMainNode.Attribute("DPCODSOG_K").Value;
				S_CustSupp = S_CustSupp.Substring(0, Math.Min(12, S_CustSupp.Length));
			}

			if (branchesMainNode.Attribute("DPCODCOM_K") != null)
				S_InfinityCode = branchesMainNode.Attribute("DPCODCOM_K").Value;

			if (branchesMainNode.Attribute("Address") != null)
			{
				S_Address = branchesMainNode.Attribute("Address").Value;
				if (branchesMainNode.Attribute("Civnum") != null)
					S_Address = S_Address + " " + branchesMainNode.Attribute("Civnum").Value;
			}
			if (branchesMainNode.Attribute("Cap") != null)
				S_ZIPCode = branchesMainNode.Attribute("Cap").Value;
			if (branchesMainNode.Attribute("Local") != null)
				S_City = branchesMainNode.Attribute("Local").Value;
			if (branchesMainNode.Attribute("Provin") != null)
				S_County = branchesMainNode.Attribute("Provin").Value;

			if (branchesMainNode.Attribute("Phone") != null)
			{
				S_Telephone1 = branchesMainNode.Attribute("Phone").Value;
				if (branchesMainNode.Attribute("Prephone") != null)
					S_Telephone1 = branchesMainNode.Attribute("Prephone").Value + " " + S_Telephone1;
			}
			if (branchesMainNode.Attribute("Mail") != null)
				S_EMail = branchesMainNode.Attribute("Mail").Value;

			if (branchesMainNode.Attribute("Tipsed") != null)
				S_Tipsed = branchesMainNode.Attribute("Tipsed").Value;

			if (branchesMainNode.Attribute("Dessed") != null)
				S_CompanyName = branchesMainNode.Attribute("Dessed").Value;

			// Dati da considerare solo se appartenenti alla sede principale
			if (branchesMainNode.Attribute("Primary") != null)
				S_Primary = branchesMainNode.Attribute("Primary").Value;
			if (branchesMainNode.Attribute("DPCODPAG") != null)
				S_Payment = branchesMainNode.Attribute("DPCODPAG").Value;
			if (branchesMainNode.Attribute("DPCODAGE") != null)
				S_Salesperson = branchesMainNode.Attribute("DPCODAGE").Value;
			if (branchesMainNode.Attribute("DPMODTRA") != null)
				S_Shipping = branchesMainNode.Attribute("DPMODTRA").Value;
			if (branchesMainNode.Attribute("DPPREBOL") != null)
				S_ShowPricesOnDN = branchesMainNode.Attribute("DPPREBOL").Value;
			if (branchesMainNode.Attribute("DPCODVE1") != null)
				S_Carrier1 = branchesMainNode.Attribute("DPCODVE1").Value;
			if (branchesMainNode.Attribute("DPCODVE2") != null)
				S_Carrier2 = branchesMainNode.Attribute("DPCODVE2").Value;
			if (branchesMainNode.Attribute("DPCODVE3") != null)
				S_Carrier3 = branchesMainNode.Attribute("DPCODVE3").Value;
		}

		// OGNI SEDE è un xmlString a parte...
		// Se S_Primary è "S" allora ho in mano la sede principale e devo aggiornare solo alcuni dati di testa del cliente E NON TOCCO LE SEDI.
		// Se S_Primary è "N" devo aggiungere o aggiornare la sede.
		//-------------------------------------------------------------------------------
		private string GetMagoXmlForBranches()
		{
			#region EsempioXML
			/*
<?xml version="1.0"?>
<maxs:Customers xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/Default.xsd" tbNamespace="Document.ERP.CustomersSuppliers.Documents.Customers" xTechProfile="Default">
	<maxs:Data>
		<maxs:CustomersSuppliers master="true">
		... ... ... 
		</maxs:CustomersSuppliers>
		<maxs:OtherBranches updateType="InsertUpdate">
			<maxs:OtherBranchesRow>
				<maxs:CustSuppType>3211264</maxs:CustSuppType>
				<maxs:CustSupp>02100</maxs:CustSupp>
				<maxs:Branch>SEDE</maxs:Branch>
				<maxs:CompanyName>sede2</maxs:CompanyName>
				<maxs:Address>via sede2</maxs:Address>
				<maxs:ZIPCode>20100</maxs:ZIPCode>
				<maxs:City>Milano</maxs:City>
				<maxs:County>MI</maxs:County>
				<maxs:Telephone1></maxs:Telephone1>
				<maxs:EMail></maxs:EMail>
				<maxs:Salesperson></maxs:Salesperson>
			</maxs:OtherBranchesRow>
		</maxs:OtherBranches>
	</maxs:Data>
</maxs:Customers>			 
*/
			#endregion

			string xml = string.Empty;

			if (string.IsNullOrEmpty(S_Primary))	// dato fondamentale, se manca assumo che si tratti di una sede secondaria
				S_Primary = "N";

			xml += "<?xml version=\"1.0\"?>";
			xml += "<" + MAXS_NS + "Customers " + MAXS_NS_FULL + "=\"http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/Default";
			xml += ".xsd\" tbNamespace=\"Document.ERP.CustomersSuppliers.Documents.Customers\" xTechProfile=\"";
			xml += "Default";
			xml += "\">";
			xml += "<" + MAXS_NS + "Data>";
			xml += "<" + MAXS_NS + "CustomersSuppliers master=\"true\">";

			xml += "<" + MAXS_NS + "CustSuppType>3211264</" + MAXS_NS + "CustSuppType>";		// Enumerafivo cablato: 3211264=Clienti
			if (!string.IsNullOrEmpty(S_CustSupp))
				xml += "<" + MAXS_NS + "CustSupp>" + S_CustSupp + "</" + MAXS_NS + "CustSupp>";

			if (S_Primary == "S")
			{
				if (!string.IsNullOrEmpty(S_Payment))	// MA_CustSupp
					xml += "<" + MAXS_NS + "Payment>" + S_Payment + "</" + MAXS_NS + "Payment>";
				xml += "</" + MAXS_NS + "CustomersSuppliers>";	// chiudo la sezione relativa ai dati di testa del cliente

				// Se scrivo <maxs:Options></maxs:Options> senza indicare nulla, in pratica cancello tutti i campi del nodo Options !!!!!!
				if (!string.IsNullOrEmpty(S_Salesperson) || !string.IsNullOrEmpty(S_Shipping) || !string.IsNullOrEmpty(S_ShowPricesOnDN) ||
					!string.IsNullOrEmpty(S_Carrier1) || !string.IsNullOrEmpty(S_Carrier2) || !string.IsNullOrEmpty(S_Carrier3))
				{
					xml += "<" + MAXS_NS + "Options>";		// MA_CustSuppCustomerOptions
					if (!string.IsNullOrEmpty(S_Salesperson))
						xml += "<" + MAXS_NS + "Salesperson>" + S_Salesperson + "</" + MAXS_NS + "Salesperson>";
					if (!string.IsNullOrEmpty(S_Shipping))
						xml += "<" + MAXS_NS + "Shipping>" + S_Shipping + "</" + MAXS_NS + "Shipping>";
					if (!string.IsNullOrEmpty(S_ShowPricesOnDN))
						xml += "<" + MAXS_NS + "ShowPricesOnDN>" + S_ShowPricesOnDN + "</" + MAXS_NS + "ShowPricesOnDN>";
					if (!string.IsNullOrEmpty(S_Carrier1))
						xml += "<" + MAXS_NS + "Carrier1>" + S_Carrier1 + "</" + MAXS_NS + "Carrier1>";
					if (!string.IsNullOrEmpty(S_Carrier2))
						xml += "<" + MAXS_NS + "Carrier2>" + S_Carrier2 + "</" + MAXS_NS + "Carrier2>";
					if (!string.IsNullOrEmpty(S_Carrier3))
						xml += "<" + MAXS_NS + "Carrier3>" + S_Carrier3 + "</" + MAXS_NS + "Carrier3>";
					xml += "</" + MAXS_NS + "Options>";
				}
			}
			else
			{
				xml += "</" + MAXS_NS + "CustomersSuppliers>";	// chiudo la sezione relativa ai dati di testa del cliente
				xml += "<" + MAXS_NS + "OtherBranches updateType=\"InsertUpdate\">";		// MA_CustSuppBranches (con InsertUpdate se manca la aggiunge!)
				xml += "<" + MAXS_NS + "OtherBranchesRow>";

				xml += "<" + MAXS_NS + "CustSuppType>3211264</" + MAXS_NS + "CustSuppType>";
				if (!string.IsNullOrEmpty(S_CustSupp))
					xml += "<" + MAXS_NS + "CustSupp>" + S_CustSupp + "</" + MAXS_NS + "CustSupp>";
				if (!string.IsNullOrEmpty(S_Branch))
					xml += "<" + MAXS_NS + "Branch>" + S_Branch + "</" + MAXS_NS + "Branch>";
				if (!string.IsNullOrEmpty(S_CompanyName))
					xml += "<" + MAXS_NS + "CompanyName>" + S_CompanyName + "</" + MAXS_NS + "CompanyName>";
				if (!string.IsNullOrEmpty(S_Address))
					xml += "<" + MAXS_NS + "Address>" + S_Address + "</" + MAXS_NS + "Address>";
				if (!string.IsNullOrEmpty(S_ZIPCode))
					xml += "<" + MAXS_NS + "ZIPCode>" + S_ZIPCode + "</" + MAXS_NS + "ZIPCode>";
				if (!string.IsNullOrEmpty(S_City))
					xml += "<" + MAXS_NS + "City>" + S_City + "</" + MAXS_NS + "City>";
				if (!string.IsNullOrEmpty(S_County))
					xml += "<" + MAXS_NS + "County>" + S_County + "</" + MAXS_NS + "County>";
				if (!string.IsNullOrEmpty(S_Telephone1))
					xml += "<" + MAXS_NS + "Telephone1>" + S_Telephone1 + "</" + MAXS_NS + "Telephone1>";
				if (!string.IsNullOrEmpty(S_EMail))
					xml += "<" + MAXS_NS + "EMail>" + S_EMail + "</" + MAXS_NS + "EMail>";
				if (!string.IsNullOrEmpty(S_Salesperson))
					xml += "<" + MAXS_NS + "Salesperson>" + S_Salesperson + "</" + MAXS_NS + "Salesperson>";

				xml += "</" + MAXS_NS + "OtherBranchesRow>";
				xml += "</" + MAXS_NS + "OtherBranches>";
			}

			xml += "</" + MAXS_NS + "Data>";
			xml += "</" + MAXS_NS + "Customers>";

			return xml;
		}

        //-------------------------------------------------------------------------------
        //-----------------------------    ORDINI    ------------------------------------
        //-------------------------------------------------------------------------------
        //VECCHIA GESTIONE
        private IEnumerable<ActionToImport> ParseInputFilesOrdersErp(string xmlString, string authenticationToken)
        {
            XDocument xDoc = null;

            IEnumerable<XElement> add_MAGO_SALEORDERSERP_OUT_Elem = null;
            string processId = GetProcessId(xmlString);

            try
            {
                xDoc = XDocument.Parse(xmlString);
                add_MAGO_SALEORDERSERP_OUT_Elem = xDoc.Element("ExecuteSyncroResult").Element("MAGO_SALEORDERSERP_OUT").Elements("Add_MAGO_SALEORDERSERP_OUT");
            }
            catch (Exception)
            {
                yield break;
            }

            // per ogni nodo di tipo Add_MAGO_SALEORDERS_OUT
            foreach (XElement addElem in add_MAGO_SALEORDERSERP_OUT_Elem)
            {
                ActionToImport ati = new ActionToImport();
                ati.ProccessId = processId;
                ati.ActionName = "MAGO_SALEORDERSERP_OUT";

                ParseOrderMainAttributeErp(addElem);

                //la chiave che dopo andra sostituita con la chive di mago con _APPREG  DEVE ESSERE SEMPRE PRIMA NELLA LISTA!!!!!
                //la chiave fissa va SEMPRE DOPO
                ati.InfinityKeys.Add(string.Format("DOSERIAL_K={0}", OT_Order));
                ati.InfinityKeys.Add("DOSERIAL_K_APPREG={0}");

                // se elemento continene errori non faccio il persing 
                // compongo solo le chiavi per fare il rollback e vado avanti
                if (IfResponseContainsErrors(addElem.ToString()))
                {
                    ati.DoRollback = true;
                    yield return ati;
                    continue;
                }

                ati.MagoXml = GetMagoXmlForOrdersErp(addElem, authenticationToken);
                yield return ati;
            }

            yield break;
        }

        //-------------------------------------------------------------------------------
        private void ParseOrderMainAttributeErp(XElement orderMainNode)
        {
            if (orderMainNode == null)
                return;

            // Variabili per Ordini Testa
            OT_ScontoDiRigaComm = string.Empty;
            OT_ScontoDiRigaFin = string.Empty;
            OT_InternalOrdNo = string.Empty;
            OT_ExternalOrdNo = string.Empty;
            OT_OrderDate = string.Empty;
            OT_ExpectedDeliveryDate = string.Empty;
            OT_ConfirmedDeliveryDate = string.Empty;
            OT_Customer = string.Empty;
            OT_Payment = string.Empty;
            OT_Currency = string.Empty;
            OT_ShipToAddress = string.Empty;
            OT_Order = string.Empty;
            OT_Fixing = string.Empty;
            OT_DOSTATRA = string.Empty;
            OT_InvoicingCustomer = string.Empty;
            OT_Priority = string.Empty;
            OT_SalesPerson = string.Empty;
            OT_CollectionCharges = string.Empty;
            OT_ShippingCharges = string.Empty;
            OT_PackagingCharges = string.Empty;
            OT_StampsCharges = string.Empty;
            OT_Advance = string.Empty;
            OT_Carrier1 = string.Empty;
            OT_Carrier2 = string.Empty;
            OT_Carrier3 = string.Empty;
            OT_Transport = string.Empty;
            OT_ExpectedHours = string.Empty;
            OT_ExpectedMinuts = string.Empty;
            OT_ConfirmedHours = string.Empty;
            OT_ConfirmedMinuts = string.Empty;
            OT_GrossWeight = string.Empty;
            OT_NetWeight = string.Empty;
            OT_GrossVolume = string.Empty;
            OT_NoOfPacks = string.Empty;

            // Il protocollo Infinity diventa il numero ordine interno di Mago
            if (orderMainNode.Attribute("DONUMPRO") != null)
            {
                OT_InternalOrdNo = orderMainNode.Attribute("DONUMPRO").Value;               // Ricerca di un attributo
                if (orderMainNode.Attribute("DOALFPRO") != null && orderMainNode.Attribute("DOALFPRO").Value != string.Empty)
                    OT_InternalOrdNo += "/" + orderMainNode.Attribute("DOALFPRO").Value;
                OT_InternalOrdNo = OT_InternalOrdNo.Trim();
                OT_InternalOrdNo = OT_InternalOrdNo.Substring(0, Math.Min(10, OT_InternalOrdNo.Length));        // MAX 10 caratteri
            }

            // Il numero documento Infinity diventa il numero esterno di Mago
            if (orderMainNode.Attribute("DONUMDOC") != null)
                OT_ExternalOrdNo = orderMainNode.Attribute("DONUMDOC").Value;

            if (orderMainNode.Attribute("DOSERIAL_K") != null)
                OT_Order = orderMainNode.Attribute("DOSERIAL_K").Value;

            if (orderMainNode.Attribute("DODATDOC") != null)
                OT_OrderDate = orderMainNode.Attribute("DODATDOC").Value;
            if (OT_OrderDate == "0100-01-01")
                OT_OrderDate = string.Empty;

            if (orderMainNode.Attribute("DODATTRA") != null)
                OT_ExpectedDeliveryDate = OT_ConfirmedDeliveryDate = orderMainNode.Attribute("DODATTRA").Value;
            if (OT_ExpectedDeliveryDate == "0100-01-01")
                OT_ExpectedDeliveryDate = OT_ConfirmedDeliveryDate = string.Empty;

            //if (orderMainNode.Attribute("DODATCON") != null)
            //    OT_ConfirmedDeliveryDate = orderMainNode.Attribute("DODATCON").Value;
            //if (OT_ConfirmedDeliveryDate == "0100-01-01")
            //    OT_ConfirmedDeliveryDate = string.Empty;

            if (orderMainNode.Attribute("DOCODSOG") != null)
                OT_Customer = orderMainNode.Attribute("DOCODSOG").Value;    // "01185"

            if (orderMainNode.Attribute("DOCODPAG") != null)
                OT_Payment = orderMainNode.Attribute("DOCODPAG").Value;

            if (orderMainNode.Attribute("DOCODVAL") != null)
                OT_Currency = orderMainNode.Attribute("DOCODVAL").Value;

            // "CL-00003" cliente o contatto o altro dove viene consegnata la merce, in Mago si puo' indicare solo la sua sede.
            // Pertanto se il cliente di consegna e' uguale al cliente dell'ordine allora posso prenderne la sede come sede di consegna/spedizione della merce.
            if (orderMainNode.Attribute("DOANACON") != null && orderMainNode.Attribute("DOANACON").Value != string.Empty)
            {
                string dummy = orderMainNode.Attribute("DOANACON").Value;
                dummy = dummy.Replace("CL-", "");
                if ((orderMainNode.Attribute("DODIPCON") != null &&
                     orderMainNode.Attribute("DODIPCON").Value != string.Empty &&
                     orderMainNode.Attribute("DODIPCON").Value != "0000000010")) // e' considerata la sede legale di default, cioe' non e' una sede diversa di consegna.
                    OT_ShipToAddress = orderMainNode.Attribute("DODIPCON").Value;
            }

            if (orderMainNode.Attribute("DOCAOVAL") != null)    // Cambio rispetto alla valuta di conto  (fixing) es. "1.000000"
            {
                OT_Fixing = orderMainNode.Attribute("DOCAOVAL").Value;
                if (OT_Fixing == "1.000000" || OT_Fixing == "0.000000")
                    OT_Fixing = string.Empty;
            }

            if (orderMainNode.Attribute("DOSTATRA") != null)    // Stato transazione (0=normale, 1=eCommerce)
                OT_DOSTATRA = orderMainNode.Attribute("DOSTATRA").Value;

            if (orderMainNode.Attribute("DOSOGFAT") != null)    // Codice Soggetto di Fatturazione es. "NUOVO"  (TRASCODIFICA)
                OT_InvoicingCustomer = orderMainNode.Attribute("DOSOGFAT").Value;

            if (orderMainNode.Attribute("DOPRIORI") != null)    // Priorità ordine, di solito  "0"
                OT_Priority = orderMainNode.Attribute("DOPRIORI").Value;

            if (orderMainNode.Attribute("DOCODAGE") != null)    // Codice forza vendita  es. "NEWAGE"  (TRASCODIFICA)
                OT_SalesPerson = orderMainNode.Attribute("DOCODAGE").Value;

            if (orderMainNode.Attribute("DOCODLIN") != null)
                OT_LanguageOrder = orderMainNode.Attribute("DOCODLIN").Value;

            if (orderMainNode.Attribute("DOIMPINC") != null)    // Spese di incasso 
            {
                OT_CollectionCharges = orderMainNode.Attribute("DOIMPINC").Value;
                if (Convert.ToDecimal(OT_CollectionCharges) == decimal.Zero)
                    OT_CollectionCharges = string.Empty;
            }

            if (orderMainNode.Attribute("DOIMPTRA") != null)    // Spese di trasporto  
            {
                OT_ShippingCharges = orderMainNode.Attribute("DOIMPTRA").Value;
                if (Convert.ToDecimal(OT_ShippingCharges) == decimal.Zero)
                    OT_ShippingCharges = string.Empty;
            }

            if (orderMainNode.Attribute("DOIMPIMB") != null)    // Importo Spese di Imballo  
            {
                OT_PackagingCharges = orderMainNode.Attribute("DOIMPIMB").Value;
                if (Convert.ToDecimal(OT_PackagingCharges) == decimal.Zero)
                    OT_PackagingCharges = string.Empty;
            }

            if (orderMainNode.Attribute("DOIMPBOL") != null)    // Totale importo Bolli Esenti 
            {
                OT_StampsCharges = orderMainNode.Attribute("DOIMPBOL").Value;
                if (Convert.ToDecimal(OT_StampsCharges) == decimal.Zero)
                    OT_StampsCharges = string.Empty;
            }

            if (orderMainNode.Attribute("DOIMPPRE") != null)    // Importo prepagato 
            {
                OT_Advance = orderMainNode.Attribute("DOIMPPRE").Value;
                if (Convert.ToDecimal(OT_Advance) == decimal.Zero)
                    OT_Advance = string.Empty;
            }

            if (orderMainNode.Attribute("DOCODVET") != null)    // Codice Primo Vettore  es. "BARTOLIN"  (TRASCODIFICA)
                OT_Carrier1 = orderMainNode.Attribute("DOCODVET").Value;
            if (orderMainNode.Attribute("DOCODVE2") != null)
                OT_Carrier2 = orderMainNode.Attribute("DOCODVE2").Value;
            if (orderMainNode.Attribute("DOCODVE3") != null)
                OT_Carrier3 = orderMainNode.Attribute("DOCODVE3").Value;

            if (orderMainNode.Attribute("DOMODTRA") != null)    // Codice modalità di trasporto  es. "001" (TRASCODIFICA)
                OT_Transport = orderMainNode.Attribute("DOMODTRA").Value;

            if (orderMainNode.Attribute("DOORATRA") != null)    // Ora Inizio Trasporto  es. "06" oppure ""
                OT_ExpectedHours = orderMainNode.Attribute("DOORATRA").Value;
            if (orderMainNode.Attribute("DOMINTRA") != null)    // Minuto Inizio Trasporto  es. "06" oppure ""
                OT_ExpectedMinuts = orderMainNode.Attribute("DOMINTRA").Value;

            if (orderMainNode.Attribute("DOORACON") != null)    // Ora Consegna Effettiva  es. "06" oppure ""
                OT_ConfirmedHours = orderMainNode.Attribute("DOORACON").Value;
            if (orderMainNode.Attribute("DOMINCON") != null)    // Minuto Consegna Effettiva  es. "06" oppure ""
                OT_ConfirmedMinuts = orderMainNode.Attribute("DOMINCON").Value;

            if (orderMainNode.Attribute("DOPESLOR") != null)    // Peso Lordo (Kg)  es. "10.000" (10 kg)
                OT_GrossWeight = orderMainNode.Attribute("DOPESLOR").Value;

            if (orderMainNode.Attribute("DOPESNET") != null)    // Peso Netto (Kg) es. "9.000"
                OT_NetWeight = orderMainNode.Attribute("DOPESNET").Value;

            if (orderMainNode.Attribute("DOVOLUME") != null)    // Volume / cubaggio es. "8.000"
                OT_GrossVolume = orderMainNode.Attribute("DOVOLUME").Value;

            if (orderMainNode.Attribute("DONUMCOL") != null)    // Numero Colli  es. "6.000"  (6 colli)	
            {
                OT_NoOfPacks = orderMainNode.Attribute("DONUMCOL").Value;
                int start = OT_NoOfPacks.IndexOf('.');
                if (start != -1)
                    OT_NoOfPacks = OT_NoOfPacks.Substring(0, start); // Da "6.000" devo ottenere "6" perche' su Mago e' un intero
            }
            if (orderMainNode.Attribute("DOCODPOR") != null)    // Incoterm
                OT_CodPort = orderMainNode.Attribute("DOCODPOR").Value;

        }

        // Parso il xmlString di input leggendo le righe e le aggiungo alla stringa xml che arriva come parametro (essa contiene gia' la testa dell'ordine)
        //-------------------------------------------------------------------------------
        private bool ParseOrderRowsElementsErp(XElement orderMainElement, ref string xml)
        {
            try
            {
                IEnumerable<XElement> orderRowsElements = orderMainElement.Element("BO_MAGO_SALEORDERSERP_OUT_d").Elements("MAGO_SALEORDERSERP_OUT_d");

                xml += "<" + MAXS_NS + "Detail>";

                // Leggo tutti i sottonodi del nodo OrderRowsNode che contiene tutte le righe dell'ordine
                foreach (XElement rowElem in orderRowsElements)
                {
                    string Riga = "<" + MAXS_NS + "DetailRow>";
                    bool isServizioValore = false;
                    //					bool isServizioQta		= false;
                    bool isScontoComm = false;
                    bool isScontoFin = false;
                    bool isRigaToDelete = false;
                    bool isRigaNota = false;
                    string ScontoDiRiga = string.Empty;
                    string Sconto1 = string.Empty;
                    string Sconto2 = string.Empty;
                    string Sconto3 = string.Empty;
                    string Sconto4 = string.Empty;
                    string lineType = string.Empty;

                    // Leggo tutti i sottonodi del nodo OrderRow che contiene tutti i campi relativi ad una riga dell'ordine
                    foreach (XElement node in rowElem.Elements())
                    {
                        if (node != null && !string.IsNullOrEmpty(node.Value) && node.Value != "0100-01-01") // le date vuote non le considero
                        {
                            // Note di riga (MA_SaleOrdDetails.Notes)
                            if (node.Name == "DODESSUP")
                            {
                                Riga += "<" + MAXS_NS + "Notes>" + node.Value + "</" + MAXS_NS + "Notes>";
                            }

                            // Tipo RIGA (MA_SaleOrdDetails.LineType)
                            if (node.Name == "DOFLGART")
                            {
                                lineType = node.Value;
                                if (node.Value == "1")  // merce
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538947" + "</" + MAXS_NS + "LineType>";
                                if (node.Value == "3")  // Riga di tipo nota e non descrizione (che ammetterebbe qta valore e sconto)
                                {
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538944" + "</" + MAXS_NS + "LineType>";
                                    isRigaNota = true;  // questo e' uno dei primi TAG e condiziona i successivi ...
                                }
                                if (node.Value == "4")  // servizio a quantita' (es. lavorazione ad ore)
                                {
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538946" + "</" + MAXS_NS + "LineType>";
                                    //	isServizioQta = true;
                                }
                                if (node.Value == "5")  // servizio a valore o sconto commerciale o finanziario
                                {
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538946" + "</" + MAXS_NS + "LineType>";
                                    isServizioValore = true;
                                }
                            }

                            if (isRigaNota)
                            {
                                if (node.Name == "DOFLOMAG")
                                    Riga += "<" + MAXS_NS + "SaleType>" + "3670020" + "</" + MAXS_NS + "SaleType>";
                                if (node.Name == "CPROWNUM_K")
                                    Riga += "<" + MAXS_NS + "Line>" + node.Value + "</" + MAXS_NS + "Line>";
                                if (node.Name == "CPROWORD")
                                    Riga += "<" + MAXS_NS + "Position>" + node.Value + "</" + MAXS_NS + "Position>";
                                if (node.Name == "DODESART")
                                    Riga += "<" + MAXS_NS + "Description>" + node.Value + "</" + MAXS_NS + "Description>";
                            }
                            else
                            {
                                // Tipo vendita (MA_SaleOrdDetails.SaleType) : 0=Normale; 1=Sconto merce; 2=Omaggio Imp; 3=Omaggio Imp+IVA
                                if (node.Name == "DOFLOMAG")
                                {
                                    if (node.Value == "0")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670020" + "</" + MAXS_NS + "SaleType>";
                                    if (node.Value == "1")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670019" + "</" + MAXS_NS + "SaleType>";
                                    if (node.Value == "2")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670017" + "</" + MAXS_NS + "SaleType>";
                                    if (node.Value == "3")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670016" + "</" + MAXS_NS + "SaleType>";
                                }

                                // Flag spesa accessoria:  no="0"; sconto comm.="7"; sc.finanziario="8"
                                if (node.Name == "DOFLSPAC" && node.Value == "7")
                                    isScontoComm = true;
                                if (node.Name == "DOFLSPAC" && node.Value == "8")
                                    isScontoFin = true;
                                // Alcuni tipi di spesa su Infinity vengono indicati sulle righe e come conseguenza nei totali, per Mago considero solo i totali.
                                // 1=Spese trasporto, 2=Spese incasso, 3=Spese imballo, 5=Spese bolli esenti 
                                if (node.Name == "DOFLSPAC" && (node.Value == "1" || node.Value == "2" || node.Value == "3" || node.Value == "5"))
                                    isRigaToDelete = true;
                                if (node.Name == "CPROWNUM_K")
                                    Riga += "<" + MAXS_NS + "Line>" + node.Value + "</" + MAXS_NS + "Line>";
                                if (node.Name == "CPROWORD")
                                    Riga += "<" + MAXS_NS + "Position>" + node.Value + "</" + MAXS_NS + "Position>";
                                if (node.Name == "DODESART")
                                    Riga += "<" + MAXS_NS + "Description>" + node.Value + "</" + MAXS_NS + "Description>";
                                if (node.Name == "Codric" && lineType.Equals("1"))
                                    Riga += "<" + MAXS_NS + "Item>" + node.Value + "</" + MAXS_NS + "Item>";
                                if (node.Name == "DOUNIMIS" && !string.IsNullOrWhiteSpace(node.Value))
                                    Riga += "<" + MAXS_NS + "UoM>" + node.Value + "</" + MAXS_NS + "UoM>";
                                if (node.Name == "DOQTAMOV")
                                    Riga += "<" + MAXS_NS + "Qty>" + node.Value + "</" + MAXS_NS + "Qty>";
                                if (node.Name == "DOPREZZO")
                                { 
                                    Riga += "<" + MAXS_NS + "UnitValue>" + node.Value + "</" + MAXS_NS + "UnitValue>";
                                    ScontoDiRiga = node.Value;
                                }
                                if (node.Name == "DOCODIVA")
                                    Riga += "<" + MAXS_NS + "TaxCode>" + node.Value + "</" + MAXS_NS + "TaxCode>";

                                //	questo campo e' sempre a zero... ? ... quindi lo sconto verra' calcolato da Mago in base alle % di sconto 1 e 2
                                //	if (node.ProviderName == "DOVALSCO")	
                                //		Riga += "<" + MAXS_NS + "DiscountAmount>"		+ node.InnerText				+ "</"	+ MAXS_NS + "DiscountAmount>";

                                if (node.Name == "DODATRIC")
                                    Riga += "<" + MAXS_NS + "ExpectedDeliveryDate>" + node.Value + "</" + MAXS_NS + "ExpectedDeliveryDate>";
                                if (node.Name == "DODATCOC")
                                    Riga += "<" + MAXS_NS + "ConfirmedDeliveryDate>" + node.Value + "</" + MAXS_NS + "ConfirmedDeliveryDate>";
                            }

                            // Su Infinity gli sconti hanno il segno meno, su Mago no!
                            if (node.Name == "DOPERSC1")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto1 = node.Value;
                                    if (Sconto1.Substring(0, 1) == "-")         // "-6.05000"
                                        Sconto1 = Sconto1.Replace("-", "+");        // elimino  il segno meno e metto il +
                                    else
                                        Sconto1 = "-" + Sconto1;                    // aggiungo il segno meno

                                    // Riga += "<" + MAXS_NS + "Discount1>" + Sconto1 + "</" + MAXS_NS + "Discount1>";
                                }
                                else // metto zero altrimenti MAGO mette lo sconto tipico del cliente, se invece il nodo Discount1 e' 0.00 non lo puo' modificare.
                                    Riga += "<" + MAXS_NS + "Discount1>" + "0.00" + "</" + MAXS_NS + "Discount1>";

                            }
                            if (node.Name == "DOPERSC2")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto2 = node.Value;
                                    if (Sconto2.Substring(0, 1) == "-")
                                        Sconto2 = Sconto2.Replace("-", "+");
                                    else
                                        Sconto2 = "-" + Sconto2;

                                    //  Riga += "<" + MAXS_NS + "Discount2>" + Sconto2 + "</" + MAXS_NS + "Discount2>";
                                }
                                else
                                    Riga += "<" + MAXS_NS + "Discount2>" + "0.00" + "</" + MAXS_NS + "Discount2>";
                            }
                            if (node.Name == "DOPERSC3")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto3 = node.Value;
                                    if (Sconto3.Substring(0, 1) == "-")
                                        Sconto3 = Sconto3.Replace("-", "+");
                                    else
                                        Sconto3 = "-" + Sconto3;
                                }

                            }
                            if (node.Name == "DOPERSC4")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto4 = node.Value;
                                    if (Sconto4.Substring(0, 1) == "-")
                                        Sconto4 = Sconto4.Replace("-", "+");
                                    else
                                        Sconto4 = "-" + Sconto4;

                                }
                            }
                        }
                    }

                    // Es. "-5.00" e "-3.00" su Infinity diventano in Mago "+5.00" e "+3.00" e la formula e' "+5.00+3.00"
                    if (Sconto1 != string.Empty || Sconto2 != string.Empty || Sconto3 != string.Empty || Sconto4 != string.Empty)
                    {
                        string Sconto = Sconto1 + Sconto2 + Sconto3 + Sconto4;
                        if (Sconto.Substring(0, 1) == "+")
                            Sconto = Sconto.Substring(1, Sconto.Length - 1);

                        //elimino decimali non significativi per evitare problemi di dimensione stringa
                        Sconto = Sconto.Replace(".00", "");

                        Riga += "<" + MAXS_NS + "DiscountFormula>" + Sconto + "</" + MAXS_NS + "DiscountFormula>";
                    }
                    else
                        Riga += "<" + MAXS_NS + "DiscountFormula>" + " " + "</" + MAXS_NS + "DiscountFormula>";


                    // Ora ho letto tutta la riga, controllo se si tratta di una riga di sconto ...
                    // Le righe infinity di tipo sconto non le genero in Mago, ma prendo solo il campo DOPREZZO ("-5.00000") e valorizzo MA_SaleOrdSummary.Discounts o Allowances
                    if (isServizioValore)
                    {
                        if (isScontoComm)
                            OT_ScontoDiRigaComm = ScontoDiRiga;
                        if (isScontoFin)
                            OT_ScontoDiRigaFin = ScontoDiRiga;
                    }

                    // Se e' una riga servizio con sconto, oppure una riga di spese, NON la aggiungo al xmlString xml
                    if (!((isServizioValore && (isScontoComm || isScontoFin)) || isRigaToDelete))
                        xml = xml + Riga + "</" + MAXS_NS + "DetailRow>";
                }
            }
            catch (Exception)
            {
                return false;
            }

            xml += "</" + MAXS_NS + "Detail>";      // fine di tutte le righe

            return true;
        }

        private string GetMagoXmlForOrdersErp(XElement orderMainElement, string authenticationToken)
        {
            #region EsempioXML
            /*
<?xml version="1.0"?>
<maxs:SaleOrd xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfinitySyncroConnector.xsd" tbNamespace="Document.ERP.SaleOrders.Documents.SaleOrd" xTechProfile="Default">
	<maxs:Data>
		<maxs:SaleOrder master="true">
			<maxs:InternalOrdNo>12/02106</maxs:InternalOrdNo>
			<maxs:ExternalOrdNo>321</maxs:ExternalOrdNo>
			<maxs:OrderDate>2012-06-06</maxs:OrderDate>
			<maxs:ExpectedDeliveryDate>2012-06-06</maxs:ExpectedDeliveryDate>
			<maxs:ConfirmedDeliveryDate>2012-06-06</maxs:ConfirmedDeliveryDate>
			<maxs:Customer>00002</maxs:Customer>
			<maxs:OurReference></maxs:OurReference>
			<maxs:YourReference></maxs:YourReference>
			<maxs:Payment>RB120FM</maxs:Payment>
			<maxs:Currency>EUR</maxs:Currency>
			<maxs:AreaManager>01</maxs:AreaManager>
			<maxs:SaleOrdId>47846</maxs:SaleOrdId>
			<maxs:CompulsoryDeliveryDate></maxs:CompulsoryDeliveryDate>
			<maxs:ShipToAddress></maxs:ShipToAddress>
			<maxs:TBGuid>{D712B835-D7B3-4E29-AF52-859F09A71EE4}</maxs:TBGuid>
		</maxs:SaleOrder>
		<maxs:Detail>
			<maxs:DetailRow>
				<maxs:Line>1</maxs:Line>
				<maxs:Position>1</maxs:Position>
				<maxs:LineType>3538947</maxs:LineType>
				<maxs:Description>xxx</maxs:Description>
				<maxs:Item>XXX</maxs:Item>
				<maxs:UoM></maxs:UoM>
				<maxs:Qty>500.000000000000000</maxs:Qty>
				<maxs:UnitValue>1.000000000000000</maxs:UnitValue>
				<maxs:DiscountFormula></maxs:DiscountFormula>
				<maxs:DiscountAmount>0.000000000000000</maxs:DiscountAmount>
				<maxs:SaleOrdId>47846</maxs:SaleOrdId>
				<maxs:Notes></maxs:Notes>
			</maxs:DetailRow>
		</maxs:Detail>
	</maxs:Data>
</maxs:SaleOrd>
*/
            #endregion

            string xml = string.Empty;

            xml += "<?xml version=\"1.0\"?>";
            xml += "<" + MAXS_NS + "SaleOrd " + MAXS_NS_FULL + "=\"http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/";
            xml += "InfinitySyncroConnector";
            xml += ".xsd\" tbNamespace=\"Document.ERP.SaleOrders.Documents.SaleOrd\" xTechProfile=\"";
            xml += "InfinitySyncroConnector";
            xml += "\">";
            xml += "<" + MAXS_NS + "Data>";
            xml += "<" + MAXS_NS + "SaleOrder master=\"true\">";

            xml += "<" + MAXS_NS + "InternalOrdNo>" + OT_InternalOrdNo + "</" + MAXS_NS + "InternalOrdNo>";
            if (!string.IsNullOrEmpty(OT_ExternalOrdNo))
                xml += "<" + MAXS_NS + "ExternalOrdNo>" + OT_ExternalOrdNo + "</" + MAXS_NS + "ExternalOrdNo>";
            if (!string.IsNullOrEmpty(OT_OrderDate))
            {
                xml += "<" + MAXS_NS + "OrderDate>" + OT_OrderDate + "</" + MAXS_NS + "OrderDate>";
                //if (!string.IsNullOrEmpty(OT_ExpectedDeliveryDate))
                xml += "<" + MAXS_NS + "ExpectedDeliveryDate>" + OT_OrderDate + "</" + MAXS_NS + "ExpectedDeliveryDate>";
                //if (!string.IsNullOrEmpty(OT_ConfirmedDeliveryDate))
                xml += "<" + MAXS_NS + "ConfirmedDeliveryDate>" + OT_OrderDate + "</" + MAXS_NS + "ConfirmedDeliveryDate>";
            }
            if (!string.IsNullOrEmpty(OT_Customer))
                xml += "<" + MAXS_NS + "Customer>" + OT_Customer + "</" + MAXS_NS + "Customer>";
            if (!string.IsNullOrEmpty(OT_Payment))
                xml += "<" + MAXS_NS + "Payment>" + OT_Payment + "</" + MAXS_NS + "Payment>";
            if (!string.IsNullOrEmpty(OT_Currency))
                xml += "<" + MAXS_NS + "Currency>" + OT_Currency + "</" + MAXS_NS + "Currency>";

            if (!string.IsNullOrEmpty(GetAreaManager(authenticationToken, OT_Customer)))
                xml += "<" + MAXS_NS + "AreaManager>" + GetAreaManager(authenticationToken, OT_Customer) + "</" + MAXS_NS + "AreaManager>";

            if (!string.IsNullOrEmpty(OT_Fixing))
            {
                xml += "<" + MAXS_NS + "Fixing>" + OT_Fixing + "</" + MAXS_NS + "Fixing>";
                xml += "<" + MAXS_NS + "FixingIsManual>" + "true" + "</" + MAXS_NS + "FixingIsManual>";
            }
            else
                xml += "<" + MAXS_NS + "FixingIsManual>" + "false" + "</" + MAXS_NS + "FixingIsManual>";

            if (!string.IsNullOrEmpty(OT_InvoicingCustomer))
                xml += "<" + MAXS_NS + "InvoicingCustomer>" + OT_InvoicingCustomer + "</" + MAXS_NS + "InvoicingCustomer>";
            if (!string.IsNullOrEmpty(OT_Priority))
                xml += "<" + MAXS_NS + "Priority>" + OT_Priority + "</" + MAXS_NS + "Priority>";
            if (!string.IsNullOrEmpty(OT_SalesPerson))
                xml += "<" + MAXS_NS + "SalesPerson>" + OT_SalesPerson + "</" + MAXS_NS + "SalesPerson>";

            if (!string.IsNullOrEmpty(OT_LanguageOrder))
                xml += "<" + MAXS_NS + "Language>" + OT_LanguageOrder + "</" + MAXS_NS + "Language>";

            xml += "<" + MAXS_NS + "FromExternalProgram>32505860</" + MAXS_NS + "FromExternalProgram>";

            xml += "</" + MAXS_NS + "SaleOrder>";       // fine dati di testa

            bool bOk = ParseOrderRowsElementsErp(orderMainElement, ref xml);       // aggiungo le righe, se non ce ne sono ritorna false
            if (!bOk)
                return string.Empty;

            xml += "<" + MAXS_NS + "Charges>";      // inizio MA_SaleOrdSummary

            // Aggiungo lo sconto commerciale (decrementa l'imponibile) o finanziario (abbuono) che ho trovato nelle righe (MA_SaleOrdSummary.Discounts)
            if (OT_ScontoDiRigaComm != string.Empty)
            {
                // Cambio di segno, Infinity mette meno se decrementa e + se aumenta, in Mago il contrario!
                if (OT_ScontoDiRigaComm.Substring(0, 1) == "-") // "-6.05000"
                    OT_ScontoDiRigaComm = OT_ScontoDiRigaComm.Replace("-", "");     // elimino  il segno meno
                else
                    OT_ScontoDiRigaComm = "-" + OT_ScontoDiRigaComm;                    // aggiungo il segno meno
                xml += "<" + MAXS_NS + "Discounts>" + OT_ScontoDiRigaComm + "</" + MAXS_NS + "Discounts>";
                xml += "<" + MAXS_NS + "DiscountsIsAuto>" + "false" + "</" + MAXS_NS + "DiscountsIsAuto>";
            }
            else
                if (OT_ScontoDiRigaFin != string.Empty)
            {
                // Cambio di segno, Infinity mette meno se decrementa e + se aumenta, in Mago il contrario!
                if (OT_ScontoDiRigaFin.Substring(0, 1) == "-")  // "-6.05000"
                    OT_ScontoDiRigaFin = OT_ScontoDiRigaFin.Replace("-", "");       // elimino  il segno meno
                else
                    OT_ScontoDiRigaFin = "-" + OT_ScontoDiRigaFin;                  // aggiungo il segno meno
                xml += "<" + MAXS_NS + "Allowances>" + OT_ScontoDiRigaFin + "</" + MAXS_NS + "Allowances>";
                xml += "<" + MAXS_NS + "DiscountsIsAuto>" + "true" + "</" + MAXS_NS + "DiscountsIsAuto>";
            }

            if (!string.IsNullOrEmpty(OT_CollectionCharges))
            {
                xml += "<" + MAXS_NS + "CollectionCharges>" + OT_CollectionCharges + "</" + MAXS_NS + "CollectionCharges>";
                xml += "<" + MAXS_NS + "CollectionChargesIsAuto>" + "false" + "</" + MAXS_NS + "CollectionChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "CollectionChargesIsAuto>" + "true" + "</" + MAXS_NS + "CollectionChargesIsAuto>";

            if (!string.IsNullOrEmpty(OT_ShippingCharges))
            {
                xml += "<" + MAXS_NS + "ShippingCharges>" + OT_ShippingCharges + "</" + MAXS_NS + "ShippingCharges>";
                xml += "<" + MAXS_NS + "ShippingChargesIsAuto>" + "false" + "</" + MAXS_NS + "ShippingChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "ShippingChargesIsAuto>" + "true" + "</" + MAXS_NS + "ShippingChargesIsAuto>";

            if (!string.IsNullOrEmpty(OT_PackagingCharges))
            {
                xml += "<" + MAXS_NS + "PackagingCharges>" + OT_PackagingCharges + "</" + MAXS_NS + "PackagingCharges>";
                xml += "<" + MAXS_NS + "PackagingChargesIsAuto>" + "false" + "</" + MAXS_NS + "PackagingChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "PackagingChargesIsAuto>" + "true" + "</" + MAXS_NS + "PackagingChargesIsAuto>";

            if (!string.IsNullOrEmpty(OT_StampsCharges))
            {
                xml += "<" + MAXS_NS + "StampsCharges>" + OT_StampsCharges + "</" + MAXS_NS + "StampsCharges>";
                xml += "<" + MAXS_NS + "StampsChargesIsAuto>" + "false" + "</" + MAXS_NS + "StampsChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "StampsChargesIsAuto>" + "true" + "</" + MAXS_NS + "StampsChargesIsAuto>";

            // Acconto. Impostando a true il flag quando fatturo l'ordine la rata viene diminuita dell'acconto, altrimenti no.
            if (!string.IsNullOrEmpty(OT_Advance))
            {
                xml += "<" + MAXS_NS + "Advance>" + OT_Advance + "</" + MAXS_NS + "Advance>";
                xml += "<" + MAXS_NS + "PostAdvancesToAcc>" + "true" + "</" + MAXS_NS + "PostAdvancesToAcc>";
            }

            xml += "</" + MAXS_NS + "Charges>";     // fine MA_SaleOrdSummary

            xml += "<" + MAXS_NS + "Shipping>";     // inizio MA_SaleOrdShipping

            if (OT_ShipToAddress != string.Empty)   // Sede di consegna/spedizione MA_SaleOrdShipping.ShipToAddress
                xml += "<" + MAXS_NS + "ShipToAddress>" + OT_ShipToAddress + "</" + MAXS_NS + "ShipToAddress>";

            if (OT_Carrier1 != string.Empty)
                xml += "<" + MAXS_NS + "Carrier1>" + OT_Carrier1 + "</" + MAXS_NS + "Carrier1>";
            if (OT_Carrier2 != string.Empty)
                xml += "<" + MAXS_NS + "Carrier2>" + OT_Carrier2 + "</" + MAXS_NS + "Carrier2>";
            if (OT_Carrier3 != string.Empty)
                xml += "<" + MAXS_NS + "Carrier3>" + OT_Carrier3 + "</" + MAXS_NS + "Carrier3>";
            if (OT_Transport != string.Empty)
                xml += "<" + MAXS_NS + "Transport>" + OT_Transport + "</" + MAXS_NS + "Transport>";
            if (OT_GrossWeight != string.Empty)
            {
                xml += "<" + MAXS_NS + "GrossWeight>" + OT_GrossWeight + "</" + MAXS_NS + "GrossWeight>";
                xml += "<" + MAXS_NS + "GrossWeightIsAuto>" + "false" + "</" + MAXS_NS + "GrossWeightIsAuto>";
            }
            if (OT_NetWeight != string.Empty)
            {
                xml += "<" + MAXS_NS + "NetWeight>" + OT_NetWeight + "</" + MAXS_NS + "NetWeight>";
                xml += "<" + MAXS_NS + "NetWeightIsAuto>" + "false" + "</" + MAXS_NS + "NetWeightIsAuto>";
            }
            if (OT_GrossVolume != string.Empty)
            {
                xml += "<" + MAXS_NS + "GrossVolume>" + OT_GrossVolume + "</" + MAXS_NS + "GrossVolume>";
                xml += "<" + MAXS_NS + "GrossVolumeIsAuto>" + "false" + "</" + MAXS_NS + "GrossVolumeIsAuto>";
            }
            if (OT_NoOfPacks != string.Empty && !string.IsNullOrEmpty(OT_PackagingCharges))
            {
                xml += "<" + MAXS_NS + "NoOfPacks>" + OT_NoOfPacks + "</" + MAXS_NS + "NoOfPacks>";
                xml += "<" + MAXS_NS + "NoOfPacksIsAuto>" + "false" + "</" + MAXS_NS + "NoOfPacksIsAuto>";
            }

            if (OT_CodPort != string.Empty && !string.IsNullOrEmpty(OT_CodPort))
            {
                xml += "<" + MAXS_NS + "Port>" + GetPortFromIncoterms(authenticationToken, OT_CodPort) + "</" + MAXS_NS + "Port>";
                xml += "<" + MAXS_NS + "PortAuto>" + "false" + "</" + MAXS_NS + "PortAuto>";
            }

            xml += "</" + MAXS_NS + "Shipping>";    // fine MA_SaleOrdShipping

            xml += "</" + MAXS_NS + "Data>";        // fine di tutti i dati dell'ordine
            xml += "</" + MAXS_NS + "SaleOrd>";     // fine dell'ordine

            return xml;
        }



        //NUOVA GESTIONE
        private IEnumerable<ActionToImport> ParseInputFilesOrders(string xmlString, string authenticationToken)
        {
            XDocument xDoc = null;

            IEnumerable<XElement> add_MAGO_SALEORDERS_OUT_Elem = null;
            string processId = GetProcessId(xmlString);

            try
            {
                xDoc = XDocument.Parse(xmlString);
                add_MAGO_SALEORDERS_OUT_Elem = xDoc.Element("ExecuteSyncroResult").Element("MAGO_SALEORDERS_OUT").Elements("Add_MAGO_SALEORDERS_OUT");
            }
            catch (Exception)
            {
                yield break;
            }

            // per ogni nodo di tipo Add_MAGO_SALEORDERS_OUT
            foreach (XElement addElem in add_MAGO_SALEORDERS_OUT_Elem)
            {
                ActionToImport ati = new ActionToImport();
                ati.ProccessId = processId;
                ati.ActionName = "MAGO_SALEORDERS_OUT";

                ParseOrderMainAttribute(addElem);

                //la chiave che dopo andra sostituita con la chive di mago con _APPREG  DEVE ESSERE SEMPRE PRIMA NELLA LISTA!!!!!
                //la chiave fissa va SEMPRE DOPO
                ati.InfinityKeys.Add(string.Format("DOSERIAL_K={0}", OT_Order));
                ati.InfinityKeys.Add("DOSERIAL_K_APPREG={0}");

                // se elemento continene errori non faccio il persing 
                // compongo solo le chiavi per fare il rollback e vado avanti
                if (IfResponseContainsErrors(addElem.ToString()))
                {
                    ati.DoRollback = true;
                    yield return ati;
                    continue;
                }

                ati.MagoXml = GetMagoXmlForOrders(addElem, authenticationToken);
                yield return ati;
            }

            yield break;
        }
        //-------------------------------------------------------------------------------
        private void ParseOrderMainAttribute(XElement orderMainNode)
		{
            if (orderMainNode == null)
                return;

            // Variabili per Ordini Testa
            OT_ScontoDiRigaComm = string.Empty;
            OT_ScontoDiRigaFin = string.Empty;
            OT_InternalOrdNo = string.Empty;
            OT_ExternalOrdNo = string.Empty;
            OT_OrderDate = string.Empty;
            OT_ExpectedDeliveryDate = string.Empty;
            OT_ConfirmedDeliveryDate = string.Empty;
            OT_Customer = string.Empty;
            OT_Payment = string.Empty;
            OT_Currency = string.Empty;
            OT_ShipToAddress = string.Empty;
            OT_Order = string.Empty;
            OT_Fixing = string.Empty;
            OT_DOSTATRA = string.Empty;
            OT_InvoicingCustomer = string.Empty;
            OT_Priority = string.Empty;
            OT_SalesPerson = string.Empty;
            OT_LanguageOrder = string.Empty;
            OT_CollectionCharges = string.Empty;
            OT_ShippingCharges = string.Empty;
            OT_PackagingCharges = string.Empty;
            OT_StampsCharges = string.Empty;
            OT_Advance = string.Empty;
            OT_Carrier1 = string.Empty;
            OT_Carrier2 = string.Empty;
            OT_Carrier3 = string.Empty;
            OT_Transport = string.Empty;
            OT_ExpectedHours = string.Empty;
            OT_ExpectedMinuts = string.Empty;
            OT_ConfirmedHours = string.Empty;
            OT_ConfirmedMinuts = string.Empty;
            OT_GrossWeight = string.Empty;
            OT_NetWeight = string.Empty;
            OT_GrossVolume = string.Empty;
            OT_NoOfPacks = string.Empty;

            // Il protocollo Infinity diventa il numero ordine interno di Mago
            if (orderMainNode.Attribute("ORNUMDOC") != null)
            {
                OT_InternalOrdNo = orderMainNode.Attribute("ORNUMDOC").Value;               // Ricerca di un attributo
                if (orderMainNode.Attribute("ORALFDOC") != null && orderMainNode.Attribute("ORALFDOC").Value != string.Empty)
                    OT_InternalOrdNo += "/" + orderMainNode.Attribute("ORALFDOC").Value;
                OT_InternalOrdNo = OT_InternalOrdNo.Trim();
                OT_InternalOrdNo = OT_InternalOrdNo.Substring(0, Math.Min(10, OT_InternalOrdNo.Length));        // MAX 10 caratteri
            }

            // Il numero documento Infinity diventa il numero esterno di Mago
            if (orderMainNode.Attribute("ORNUMDOC") != null && orderMainNode.Attribute("ORALFDOC") != null)
                OT_ExternalOrdNo = orderMainNode.Attribute("ORNUMDOC").Value + "/" + orderMainNode.Attribute("ORALFDOC").Value;

            if (orderMainNode.Attribute("DOSERIAL_K") != null)
                OT_Order = orderMainNode.Attribute("DOSERIAL_K").Value;

            if (orderMainNode.Attribute("ORDATDOC") != null)
                OT_OrderDate = orderMainNode.Attribute("ORDATDOC").Value;
            if (OT_OrderDate == "0100-01-01")
                OT_OrderDate = string.Empty;

            /*   if (orderMainNode.Attribute("DODATTRA") != null)
                   OT_ExpectedDeliveryDate = OT_ConfirmedDeliveryDate = orderMainNode.Attribute("DODATTRA").Value;
               if (OT_ExpectedDeliveryDate == "0100-01-01")
                   OT_ExpectedDeliveryDate = OT_ConfirmedDeliveryDate = string.Empty;*/

            //if (orderMainNode.Attribute("DODATCON") != null)
            //    OT_ConfirmedDeliveryDate = orderMainNode.Attribute("DODATCON").Value;
            //if (OT_ConfirmedDeliveryDate == "0100-01-01")
            //    OT_ConfirmedDeliveryDate = string.Empty;

            if (orderMainNode.Attribute("ORCODCON") != null)
                OT_Customer = orderMainNode.Attribute("ORCODCON").Value;    // "01185"

            if (orderMainNode.Attribute("ORCODPAG") != null)
                OT_Payment = orderMainNode.Attribute("ORCODPAG").Value;

            if (orderMainNode.Attribute("ORCODVAL") != null)
                OT_Currency = orderMainNode.Attribute("ORCODVAL").Value;

            // "CL-00003" cliente o contatto o altro dove viene consegnata la merce, in Mago si puo' indicare solo la sua sede.
            // Pertanto se il cliente di consegna e' uguale al cliente dell'ordine allora posso prenderne la sede come sede di consegna/spedizione della merce.
            if (orderMainNode.Attribute("DOANACON") != null && orderMainNode.Attribute("DOANACON").Value != string.Empty)
            {
                string dummy = orderMainNode.Attribute("DOANACON").Value;
                dummy = dummy.Replace("CL-", "");
                if ((orderMainNode.Attribute("ORCODDES") != null &&
                     orderMainNode.Attribute("ORCODDES").Value != string.Empty &&
                     orderMainNode.Attribute("ORCODDES").Value != "0000000010")) // e' considerata la sede legale di default, cioe' non e' una sede diversa di consegna.
                    OT_ShipToAddress = orderMainNode.Attribute("ORCODDES").Value;
            }

            // Cambio rispetto alla valuta di conto  (fixing) es. "1.000000"
            if (orderMainNode.Attribute("DOCAOVAL") != null)    
             {
                decimal fix = 1 / Convert.ToDecimal(orderMainNode.Attribute("DOCAOVAL").Value);
                OT_Fixing = fix.ToString();
                 if (OT_Fixing == "1.000000" || OT_Fixing == "0.000000")
                     OT_Fixing = string.Empty;
             }


            //OT_Fixing = "1.000000";

            if (orderMainNode.Attribute("DOSTATRA") != null)    // Stato transazione (0=normale, 1=eCommerce)
                OT_DOSTATRA = orderMainNode.Attribute("DOSTATRA").Value;

            /* if (orderMainNode.Attribute("DOSOGFAT") != null)    // Codice Soggetto di Fatturazione es. "NUOVO"  (TRASCODIFICA)
                 OT_InvoicingCustomer = orderMainNode.Attribute("DOSOGFAT").Value;
                 */
            /*  if (orderMainNode.Attribute("DOPRIORI") != null)    // Priorità ordine, di solito  "0"
                  OT_Priority = orderMainNode.Attribute("DOPRIORI").Value;*/

            if (orderMainNode.Attribute("ORCODAGE") != null)    // Codice forza vendita  es. "NEWAGE"  (TRASCODIFICA)
                OT_SalesPerson = orderMainNode.Attribute("ORCODAGE").Value;

            if (orderMainNode.Attribute("DOCODLIN") != null)
                OT_LanguageOrder = orderMainNode.Attribute("DOCODLIN").Value;

            if (orderMainNode.Attribute("ORSPEINC") != null)    // Spese di incasso 
            {
                OT_CollectionCharges = orderMainNode.Attribute("ORSPEINC").Value;
                if (Convert.ToDecimal(OT_CollectionCharges) == decimal.Zero)
                    OT_CollectionCharges = string.Empty;
            }

            if (orderMainNode.Attribute("ORSPETRA") != null)    // Spese di trasporto  
            {
                OT_ShippingCharges = orderMainNode.Attribute("ORSPETRA").Value;
                if (Convert.ToDecimal(OT_ShippingCharges) == decimal.Zero)
                    OT_ShippingCharges = string.Empty;
            }

            if (orderMainNode.Attribute("ORSPEIMB") != null)    // Importo Spese di Imballo  
            {
                OT_PackagingCharges = orderMainNode.Attribute("ORSPEIMB").Value;
                if (Convert.ToDecimal(OT_PackagingCharges) == decimal.Zero)
                    OT_PackagingCharges = string.Empty;
            }

            if (orderMainNode.Attribute("ORSPEBOL") != null)    // Totale importo Bolli Esenti 
            {
                OT_StampsCharges = orderMainNode.Attribute("ORSPEBOL").Value;
                if (Convert.ToDecimal(OT_StampsCharges) == decimal.Zero)
                    OT_StampsCharges = string.Empty;
            }

            if (orderMainNode.Attribute("DOIMPPRE") != null)    // Importo prepagato 
            {
                OT_Advance = orderMainNode.Attribute("DOIMPPRE").Value;
                if (Convert.ToDecimal(OT_Advance) == decimal.Zero)
                    OT_Advance = string.Empty;
            }

            if (orderMainNode.Attribute("ORCODVET") != null)    // Codice Primo Vettore  es. "BARTOLIN"  (TRASCODIFICA)
                OT_Carrier1 = orderMainNode.Attribute("ORCODVET").Value;
            if (orderMainNode.Attribute("DOCODVE2") != null)
                OT_Carrier2 = orderMainNode.Attribute("DOCODVE2").Value;
            if (orderMainNode.Attribute("DOCODVE3") != null)
                OT_Carrier3 = orderMainNode.Attribute("DOCODVE3").Value;

            if (orderMainNode.Attribute("ORCODSPE") != null)    // Codice modalità di trasporto  es. "001" (TRASCODIFICA)
                OT_Transport = orderMainNode.Attribute("ORCODSPE").Value;

            /*  if (orderMainNode.Attribute("DOORATRA") != null)    // Ora Inizio Trasporto  es. "06" oppure ""
                  OT_ExpectedHours = orderMainNode.Attribute("DOORATRA").Value;
              if (orderMainNode.Attribute("DOMINTRA") != null)    // Minuto Inizio Trasporto  es. "06" oppure ""
                  OT_ExpectedMinuts = orderMainNode.Attribute("DOMINTRA").Value;*/

            /* if (orderMainNode.Attribute("DOORACON") != null)    // Ora Consegna Effettiva  es. "06" oppure ""
                 OT_ConfirmedHours = orderMainNode.Attribute("DOORACON").Value;
             if (orderMainNode.Attribute("DOMINCON") != null)    // Minuto Consegna Effettiva  es. "06" oppure ""
                 OT_ConfirmedMinuts = orderMainNode.Attribute("DOMINCON").Value;*/

            if (orderMainNode.Attribute("DOPESLOR") != null)    // Peso Lordo (Kg)  es. "10.000" (10 kg)
                OT_GrossWeight = orderMainNode.Attribute("DOPESLOR").Value;

            if (orderMainNode.Attribute("DOPESNET") != null)    // Peso Netto (Kg) es. "9.000"
                OT_NetWeight = orderMainNode.Attribute("DOPESNET").Value;

            if (orderMainNode.Attribute("DOVOLUME") != null)    // Volume / cubaggio es. "8.000"
                OT_GrossVolume = orderMainNode.Attribute("DOVOLUME").Value;

            if (orderMainNode.Attribute("DONUMCOL") != null)    // Numero Colli  es. "6.000"  (6 colli)	
            {
                OT_NoOfPacks = orderMainNode.Attribute("DONUMCOL").Value;
                int start = OT_NoOfPacks.IndexOf('.');
                if (start != -1)
                    OT_NoOfPacks = OT_NoOfPacks.Substring(0, start); // Da "6.000" devo ottenere "6" perche' su Mago e' un intero
            }

            if (orderMainNode.Attribute("ORCODPOR") != null)    // incoterm	
                OT_CodPort = orderMainNode.Attribute("ORCODPOR").Value;

        }

		// Parso il xmlString di input leggendo le righe e le aggiungo alla stringa xml che arriva come parametro (essa contiene gia' la testa dell'ordine)
		//-------------------------------------------------------------------------------
		
        private bool ParseOrderRowsElements(XElement orderMainElement, ref string xml)
        {
            try
            {
                IEnumerable<XElement> orderRowsElements = orderMainElement.Element("BO_MAGO_SALEORDERS_OUT_d").Elements("MAGO_SALEORDERS_OUT_d");

                xml += "<" + MAXS_NS + "Detail>";

                // Leggo tutti i sottonodi del nodo OrderRowsNode che contiene tutte le righe dell'ordine
                foreach (XElement rowElem in orderRowsElements)
                {
                    string Riga = "<" + MAXS_NS + "DetailRow>";
                    bool isServizioValore = false;
                    bool isScontoComm = false;
                    bool isScontoFin = false;
                    bool isRigaToDelete = false;
                    bool isRigaNota = false;
                    string ScontoDiRiga = string.Empty;
                    string Sconto1 = string.Empty;
                    string Sconto2 = string.Empty;
                    string Sconto3 = string.Empty;
                    string Sconto4 = string.Empty;
                    string lineType = string.Empty;

                    // Leggo tutti i sottonodi del nodo OrderRow che contiene tutti i campi relativi ad una riga dell'ordine
                    foreach (XElement node in rowElem.Elements())
                    {
                        if (node != null && !string.IsNullOrEmpty(node.Value) && node.Value != "0100-01-01") // le date vuote non le considero
                        {
                            // Note di riga (MA_SaleOrdDetails.Notes)
                            if (node.Name == "ORDESSUP")
                            {
                                Riga += "<" + MAXS_NS + "Notes>" + node.Value + "</" + MAXS_NS + "Notes>";
                            }

                            // Tipo RIGA (MA_SaleOrdDetails.LineType)
                            if (node.Name == "DOFLGART")
                            {
                                lineType = node.Value;
                                if (node.Value == "1")  // merce
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538947" + "</" + MAXS_NS + "LineType>";
                                if (node.Value == "3")  // Riga di tipo nota e non descrizione (che ammetterebbe qta valore e sconto)
                                {
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538944" + "</" + MAXS_NS + "LineType>";
                                    isRigaNota = true;  // questo e' uno dei primi TAG e condiziona i successivi ...
                                }
                                if (node.Value == "4")  // servizio a quantita' (es. lavorazione ad ore)
                                {
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538946" + "</" + MAXS_NS + "LineType>";
                                }
                                if (node.Value == "5")  // servizio a valore o sconto commerciale o finanziario
                                {
                                    Riga += "<" + MAXS_NS + "LineType>" + "3538946" + "</" + MAXS_NS + "LineType>";
                                    isServizioValore = true;
                                }
                            }

                            if (isRigaNota)
                            {
                                if (node.Name == "DOFLOMAG")
                                    Riga += "<" + MAXS_NS + "SaleType>" + "3670020" + "</" + MAXS_NS + "SaleType>";
                                if (node.Name == "CPROWNUM_K")
                                    Riga += "<" + MAXS_NS + "Line>" + node.Value + "</" + MAXS_NS + "Line>";
                                if (node.Name == "CPROWORD")
                                    Riga += "<" + MAXS_NS + "Position>" + node.Value + "</" + MAXS_NS + "Position>";
                                if (node.Name == "ORDESART")
                                    Riga += "<" + MAXS_NS + "Description>" + node.Value + "</" + MAXS_NS + "Description>";
                            }
                            else
                            {
                                // Tipo vendita (MA_SaleOrdDetails.SaleType) : 0=Normale; 1=Sconto merce; 2=Omaggio Imp; 3=Omaggio Imp+IVA
                                if (node.Name == "DOFLOMAG")
                                {
                                    if (node.Value == "0")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670020" + "</" + MAXS_NS + "SaleType>";
                                    if (node.Value == "1")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670019" + "</" + MAXS_NS + "SaleType>";
                                    if (node.Value == "2")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670017" + "</" + MAXS_NS + "SaleType>";
                                    if (node.Value == "3")
                                        Riga += "<" + MAXS_NS + "SaleType>" + "3670016" + "</" + MAXS_NS + "SaleType>";
                                }

                                // Flag spesa accessoria:  no="0"; sconto comm.="7"; sc.finanziario="8"
                                if (node.Name == "DOFLSPAC" && node.Value == "7")
                                    isScontoComm = true;
                                if (node.Name == "DOFLSPAC" && node.Value == "8")
                                    isScontoFin = true;
                                // Alcuni tipi di spesa su Infinity vengono indicati sulle righe e come conseguenza nei totali, per Mago considero solo i totali.
                                // 1=Spese trasporto, 2=Spese incasso, 3=Spese imballo, 5=Spese bolli esenti 
                                if (node.Name == "DOFLSPAC" && (node.Value == "1" || node.Value == "2" || node.Value == "3" || node.Value == "5"))
                                    isRigaToDelete = true;
                                if (node.Name == "CPROWNUM_K")
                                    Riga += "<" + MAXS_NS + "Line>" + node.Value + "</" + MAXS_NS + "Line>";
                                if (node.Name == "CPROWORD")
                                    Riga += "<" + MAXS_NS + "Position>" + node.Value + "</" + MAXS_NS + "Position>";
                                if (node.Name == "ORDESART")
                                    Riga += "<" + MAXS_NS + "Description>" + node.Value + "</" + MAXS_NS + "Description>";
                                if (node.Name == "ORCODICE" && (lineType.Equals("1") || lineType.Equals("4")))
                                    Riga += "<" + MAXS_NS + "Item>" + node.Value + "</" + MAXS_NS + "Item>";
                                if (node.Name == "ORUNIMIS" && !string.IsNullOrWhiteSpace(node.Value))
                                    Riga += "<" + MAXS_NS + "UoM>" + node.Value + "</" + MAXS_NS + "UoM>";
                                if (node.Name == "ORQTAMOV")
                                    Riga += "<" + MAXS_NS + "Qty>" + node.Value + "</" + MAXS_NS + "Qty>";
                                if (node.Name == "ORPREZZO")
                                {
                                    Riga += "<" + MAXS_NS + "UnitValue>" + node.Value + "</" + MAXS_NS + "UnitValue>";
                                    ScontoDiRiga = node.Value;
                                }
                                if (node.Name == "ORCODIVA")
                                    Riga += "<" + MAXS_NS + "TaxCode>" + node.Value + "</" + MAXS_NS + "TaxCode>";

                                //	questo campo e' sempre a zero... ? ... quindi lo sconto verra' calcolato da Mago in base alle % di sconto 1 e 2
                                //	if (node.ProviderName == "DOVALSCO")	
                                //		Riga += "<" + MAXS_NS + "DiscountAmount>"		+ node.InnerText				+ "</"	+ MAXS_NS + "DiscountAmount>";

                                if (node.Name == "DODATRIC")
                                    Riga += "<" + MAXS_NS + "ExpectedDeliveryDate>" + node.Value + "</" + MAXS_NS + "ExpectedDeliveryDate>";
                                if (node.Name == "DODATCOC")
                                    Riga += "<" + MAXS_NS + "ConfirmedDeliveryDate>" + node.Value + "</" + MAXS_NS + "ConfirmedDeliveryDate>";
                            }

                            // Su Infinity gli sconti hanno il segno meno, su Mago no!
                            if (node.Name == "ORSCONT1")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto1 = node.Value;
                                    if (Sconto1.Substring(0, 1) == "-")         // "-6.05000"
                                        Sconto1 = Sconto1.Replace("-", "+");        // elimino  il segno meno e metto il +
                                    else
                                        Sconto1 = "-" + Sconto1;                    // aggiungo il segno meno

                                    // Riga += "<" + MAXS_NS + "Discount1>" + Sconto1 + "</" + MAXS_NS + "Discount1>";
                                }
                                else // metto zero altrimenti MAGO mette lo sconto tipico del cliente, se invece il nodo Discount1 e' 0.00 non lo puo' modificare.
                                    Riga += "<" + MAXS_NS + "Discount1>" + "0.00" + "</" + MAXS_NS + "Discount1>";

                            }
                            if (node.Name == "ORSCONT2")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto2 = node.Value;
                                    if (Sconto2.Substring(0, 1) == "-")
                                        Sconto2 = Sconto2.Replace("-", "+");
                                    else
                                        Sconto2 = "-" + Sconto2;

                                    //  Riga += "<" + MAXS_NS + "Discount2>" + Sconto2 + "</" + MAXS_NS + "Discount2>";
                                }
                                else
                                    Riga += "<" + MAXS_NS + "Discount2>" + "0.00" + "</" + MAXS_NS + "Discount2>";
                            }
                            if (node.Name == "ORSCONT3")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto3 = node.Value;
                                    if (Sconto3.Substring(0, 1) == "-")
                                        Sconto3 = Sconto3.Replace("-", "+");
                                    else
                                        Sconto3 = "-" + Sconto3;
                                }

                            }
                            if (node.Name == "ORSCONT4")
                            {
                                if (Convert.ToDecimal(node.Value) != decimal.Zero)
                                {
                                    Sconto4 = node.Value;
                                    if (Sconto4.Substring(0, 1) == "-")
                                        Sconto4 = Sconto4.Replace("-", "+");
                                    else
                                        Sconto4 = "-" + Sconto4;

                                }
                            }
                        }
                    }

                    // Es. "-5.00" e "-3.00" su Infinity diventano in Mago "+5.00" e "+3.00" e la formula e' "+5.00+3.00"
                    if (Sconto1 != string.Empty || Sconto2 != string.Empty || Sconto3 != string.Empty || Sconto4 != string.Empty)
                    {
                        string Sconto = Sconto1 + Sconto2 + Sconto3 + Sconto4;
                        if (Sconto.Substring(0, 1) == "+")
                            Sconto = Sconto.Substring(1, Sconto.Length - 1);

                        //elimino decimali non significativi per evitare problemi di dimensione stringa
                        Sconto = Sconto.Replace(".00", "");

                        Riga += "<" + MAXS_NS + "DiscountFormula>" + Sconto + "</" + MAXS_NS + "DiscountFormula>";
                    }
                    else
                        Riga += "<" + MAXS_NS + "DiscountFormula>" + " " + "</" + MAXS_NS + "DiscountFormula>";

                    // Ora ho letto tutta la riga, controllo se si tratta di una riga di sconto ...
                    // Le righe infinity di tipo sconto non le genero in Mago, ma prendo solo il campo DOPREZZO ("-5.00000") e valorizzo MA_SaleOrdSummary.Discounts o Allowances
                    if (isServizioValore)
                    {
                        if (isScontoComm)
                            OT_ScontoDiRigaComm = ScontoDiRiga;
                        if (isScontoFin)
                            OT_ScontoDiRigaFin = ScontoDiRiga;
                    }

                    // Se e' una riga servizio con sconto, oppure una riga di spese, NON la aggiungo al xmlString xml
                    if (!((isServizioValore && (isScontoComm || isScontoFin)) || isRigaToDelete))
                        xml = xml + Riga + "</" + MAXS_NS + "DetailRow>";
                }
            }
            catch (Exception)
            {
                return false;
            }

            xml += "</" + MAXS_NS + "Detail>";      // fine di tutte le righe

            return true;
        }
        //-------------------------------------------------------------------------------
        private string GetMagoXmlForOrders(XElement orderMainElement, string authenticationToken)
		{
            #region EsempioXML
            /*
<?xml version="1.0"?>
<maxs:SaleOrd xmlns:maxs="http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfinitySyncroConnector.xsd" tbNamespace="Document.ERP.SaleOrders.Documents.SaleOrd" xTechProfile="Default">
	<maxs:Data>
		<maxs:SaleOrder master="true">
			<maxs:InternalOrdNo>12/02106</maxs:InternalOrdNo>
			<maxs:ExternalOrdNo>321</maxs:ExternalOrdNo>
			<maxs:OrderDate>2012-06-06</maxs:OrderDate>
			<maxs:ExpectedDeliveryDate>2012-06-06</maxs:ExpectedDeliveryDate>
			<maxs:ConfirmedDeliveryDate>2012-06-06</maxs:ConfirmedDeliveryDate>
			<maxs:Customer>00002</maxs:Customer>
			<maxs:OurReference></maxs:OurReference>
			<maxs:YourReference></maxs:YourReference>
			<maxs:Payment>RB120FM</maxs:Payment>
			<maxs:Currency>EUR</maxs:Currency>
			<maxs:AreaManager>01</maxs:AreaManager>
			<maxs:SaleOrdId>47846</maxs:SaleOrdId>
			<maxs:CompulsoryDeliveryDate></maxs:CompulsoryDeliveryDate>
			<maxs:ShipToAddress></maxs:ShipToAddress>
			<maxs:TBGuid>{D712B835-D7B3-4E29-AF52-859F09A71EE4}</maxs:TBGuid>
		</maxs:SaleOrder>
		<maxs:Detail>
			<maxs:DetailRow>
				<maxs:Line>1</maxs:Line>
				<maxs:Position>1</maxs:Position>
				<maxs:LineType>3538947</maxs:LineType>
				<maxs:Description>xxx</maxs:Description>
				<maxs:Item>XXX</maxs:Item>
				<maxs:UoM></maxs:UoM>
				<maxs:Qty>500.000000000000000</maxs:Qty>
				<maxs:UnitValue>1.000000000000000</maxs:UnitValue>
				<maxs:DiscountFormula></maxs:DiscountFormula>
				<maxs:DiscountAmount>0.000000000000000</maxs:DiscountAmount>
				<maxs:SaleOrdId>47846</maxs:SaleOrdId>
				<maxs:Notes></maxs:Notes>
			</maxs:DetailRow>
		</maxs:Detail>
	</maxs:Data>
</maxs:SaleOrd>
*/
            #endregion

            string xml = string.Empty;

			xml += "<?xml version=\"1.0\"?>";
			xml += "<" + MAXS_NS + "SaleOrd " + MAXS_NS_FULL + "=\"http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/";
			xml += "InfinitySyncroConnector";
			xml += ".xsd\" tbNamespace=\"Document.ERP.SaleOrders.Documents.SaleOrd\" xTechProfile=\"";
			xml += "InfinitySyncroConnector";
			xml += "\">";
			xml += "<" + MAXS_NS + "Data>";
			xml += "<" + MAXS_NS + "SaleOrder master=\"true\">";

			xml += "<" + MAXS_NS + "InternalOrdNo>" + OT_InternalOrdNo + "</" + MAXS_NS + "InternalOrdNo>";
			if (!string.IsNullOrEmpty(OT_ExternalOrdNo))
				xml += "<" + MAXS_NS + "ExternalOrdNo>" + OT_ExternalOrdNo + "</" + MAXS_NS + "ExternalOrdNo>";
            if (!string.IsNullOrEmpty(OT_OrderDate))
            {
                xml += "<" + MAXS_NS + "OrderDate>" + OT_OrderDate + "</" + MAXS_NS + "OrderDate>";
                //if (!string.IsNullOrEmpty(OT_ExpectedDeliveryDate))
                xml += "<" + MAXS_NS + "ExpectedDeliveryDate>" + OT_OrderDate + "</" + MAXS_NS + "ExpectedDeliveryDate>";
                //if (!string.IsNullOrEmpty(OT_ConfirmedDeliveryDate))
                xml += "<" + MAXS_NS + "ConfirmedDeliveryDate>" + OT_OrderDate + "</" + MAXS_NS + "ConfirmedDeliveryDate>";
            }
			if (!string.IsNullOrEmpty(OT_Customer))
				xml += "<" + MAXS_NS + "Customer>" + OT_Customer + "</" + MAXS_NS + "Customer>";
			if (!string.IsNullOrEmpty(OT_Payment))
				xml += "<" + MAXS_NS + "Payment>" + OT_Payment + "</" + MAXS_NS + "Payment>";
			if (!string.IsNullOrEmpty(OT_Currency))
				xml += "<" + MAXS_NS + "Currency>" + OT_Currency + "</" + MAXS_NS + "Currency>";

            if(!string.IsNullOrEmpty(GetAreaManager(authenticationToken, OT_Customer)))
                xml += "<" + MAXS_NS + "AreaManager>" + GetAreaManager(authenticationToken, OT_Customer) + "</" + MAXS_NS + "AreaManager>";

            if (!string.IsNullOrEmpty(OT_Fixing))
            {
                xml += "<" + MAXS_NS + "Fixing>" + OT_Fixing + "</" + MAXS_NS + "Fixing>";
                xml += "<" + MAXS_NS + "FixingIsManual>" + "true" + "</" + MAXS_NS + "FixingIsManual>";
            }
            else
                xml += "<" + MAXS_NS + "FixingIsManual>" + "false" + "</" + MAXS_NS + "FixingIsManual>";

			if (!string.IsNullOrEmpty(OT_InvoicingCustomer))
				xml += "<" + MAXS_NS + "InvoicingCustomer>" + OT_InvoicingCustomer + "</" + MAXS_NS + "InvoicingCustomer>";
			if (!string.IsNullOrEmpty(OT_Priority))
				xml += "<" + MAXS_NS + "Priority>" + OT_Priority + "</" + MAXS_NS + "Priority>";
			if (!string.IsNullOrEmpty(OT_SalesPerson))
				xml += "<" + MAXS_NS + "SalesPerson>" + OT_SalesPerson + "</" + MAXS_NS + "SalesPerson>";

            if (!string.IsNullOrEmpty(OT_LanguageOrder))
                xml += "<" + MAXS_NS + "Language>" + OT_LanguageOrder + "</" + MAXS_NS + "Language>";

            xml += "<" + MAXS_NS + "FromExternalProgram>32505860</" + MAXS_NS + "FromExternalProgram>";

            xml += "</" + MAXS_NS + "SaleOrder>";		// fine dati di testa

			bool bOk = ParseOrderRowsElements(orderMainElement, ref xml);		// aggiungo le righe, se non ce ne sono ritorna false
			if (!bOk)
				return string.Empty;

			xml += "<" + MAXS_NS + "Charges>";		// inizio MA_SaleOrdSummary

			// Aggiungo lo sconto commerciale (decrementa l'imponibile) o finanziario (abbuono) che ho trovato nelle righe (MA_SaleOrdSummary.Discounts)
			if (OT_ScontoDiRigaComm != string.Empty)
			{
				// Cambio di segno, Infinity mette meno se decrementa e + se aumenta, in Mago il contrario!
				if (OT_ScontoDiRigaComm.Substring(0, 1) == "-")	// "-6.05000"
					OT_ScontoDiRigaComm = OT_ScontoDiRigaComm.Replace("-", "");		// elimino  il segno meno
				else
					OT_ScontoDiRigaComm = "-" + OT_ScontoDiRigaComm;					// aggiungo il segno meno
				xml += "<" + MAXS_NS + "Discounts>" + OT_ScontoDiRigaComm + "</" + MAXS_NS + "Discounts>";
				xml += "<" + MAXS_NS + "DiscountsIsAuto>" + "false" + "</" + MAXS_NS + "DiscountsIsAuto>";
			}
			else
				if (OT_ScontoDiRigaFin != string.Empty)
				{
					// Cambio di segno, Infinity mette meno se decrementa e + se aumenta, in Mago il contrario!
					if (OT_ScontoDiRigaFin.Substring(0, 1) == "-")	// "-6.05000"
						OT_ScontoDiRigaFin = OT_ScontoDiRigaFin.Replace("-", "");		// elimino  il segno meno
					else
						OT_ScontoDiRigaFin = "-" + OT_ScontoDiRigaFin;					// aggiungo il segno meno
					xml += "<" + MAXS_NS + "Allowances>" + OT_ScontoDiRigaFin + "</" + MAXS_NS + "Allowances>";
					xml += "<" + MAXS_NS + "DiscountsIsAuto>" + "true" + "</" + MAXS_NS + "DiscountsIsAuto>";
				}

            if (!string.IsNullOrEmpty(OT_CollectionCharges))
            {
                xml += "<" + MAXS_NS + "CollectionCharges>" + OT_CollectionCharges + "</" + MAXS_NS + "CollectionCharges>";
                xml += "<" + MAXS_NS + "CollectionChargesIsAuto>" + "false" + "</" + MAXS_NS + "CollectionChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "CollectionChargesIsAuto>" + "true" + "</" + MAXS_NS + "CollectionChargesIsAuto>";

            if (!string.IsNullOrEmpty(OT_ShippingCharges))
            {
                xml += "<" + MAXS_NS + "ShippingCharges>" + OT_ShippingCharges + "</" + MAXS_NS + "ShippingCharges>";
                xml += "<" + MAXS_NS + "ShippingChargesIsAuto>" + "false" + "</" + MAXS_NS + "ShippingChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "ShippingChargesIsAuto>" + "true" + "</" + MAXS_NS + "ShippingChargesIsAuto>";

            if (!string.IsNullOrEmpty(OT_PackagingCharges))
            {
                xml += "<" + MAXS_NS + "PackagingCharges>" + OT_PackagingCharges + "</" + MAXS_NS + "PackagingCharges>";
                xml += "<" + MAXS_NS + "PackagingChargesIsAuto>" + "false" + "</" + MAXS_NS + "PackagingChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "PackagingChargesIsAuto>" + "true" + "</" + MAXS_NS + "PackagingChargesIsAuto>";

            if (!string.IsNullOrEmpty(OT_StampsCharges))
            {
                xml += "<" + MAXS_NS + "StampsCharges>" + OT_StampsCharges + "</" + MAXS_NS + "StampsCharges>";
                xml += "<" + MAXS_NS + "StampsChargesIsAuto>" + "false" + "</" + MAXS_NS + "StampsChargesIsAuto>";
            }
            else
                xml += "<" + MAXS_NS + "StampsChargesIsAuto>" + "true" + "</" + MAXS_NS + "StampsChargesIsAuto>";

            // Acconto. Impostando a true il flag quando fatturo l'ordine la rata viene diminuita dell'acconto, altrimenti no.
            if (!string.IsNullOrEmpty(OT_Advance))
			{
				xml += "<" + MAXS_NS + "Advance>" + OT_Advance + "</" + MAXS_NS + "Advance>";
				xml += "<" + MAXS_NS + "PostAdvancesToAcc>" + "true" + "</" + MAXS_NS + "PostAdvancesToAcc>";
			}

			xml += "</" + MAXS_NS + "Charges>";		// fine MA_SaleOrdSummary

			xml += "<" + MAXS_NS + "Shipping>";		// inizio MA_SaleOrdShipping

			if (OT_ShipToAddress != string.Empty)	// Sede di consegna/spedizione MA_SaleOrdShipping.ShipToAddress
				xml += "<" + MAXS_NS + "ShipToAddress>" + OT_ShipToAddress + "</" + MAXS_NS + "ShipToAddress>";

			if (OT_Carrier1 != string.Empty)
				xml += "<" + MAXS_NS + "Carrier1>" + OT_Carrier1 + "</" + MAXS_NS + "Carrier1>";
			if (OT_Carrier2 != string.Empty)
				xml += "<" + MAXS_NS + "Carrier2>" + OT_Carrier2 + "</" + MAXS_NS + "Carrier2>";
			if (OT_Carrier3 != string.Empty)
				xml += "<" + MAXS_NS + "Carrier3>" + OT_Carrier3 + "</" + MAXS_NS + "Carrier3>";
			if (OT_Transport != string.Empty)
				xml += "<" + MAXS_NS + "Transport>" + OT_Transport + "</" + MAXS_NS + "Transport>";
			if (OT_GrossWeight != string.Empty)
			{
				xml += "<" + MAXS_NS + "GrossWeight>" + OT_GrossWeight + "</" + MAXS_NS + "GrossWeight>";
				xml += "<" + MAXS_NS + "GrossWeightIsAuto>" + "false" + "</" + MAXS_NS + "GrossWeightIsAuto>";
			}
			if (OT_NetWeight != string.Empty)
			{
				xml += "<" + MAXS_NS + "NetWeight>" + OT_NetWeight + "</" + MAXS_NS + "NetWeight>";
				xml += "<" + MAXS_NS + "NetWeightIsAuto>" + "false" + "</" + MAXS_NS + "NetWeightIsAuto>";
			}
			if (OT_GrossVolume != string.Empty)
			{
				xml += "<" + MAXS_NS + "GrossVolume>" + OT_GrossVolume + "</" + MAXS_NS + "GrossVolume>";
				xml += "<" + MAXS_NS + "GrossVolumeIsAuto>" + "false" + "</" + MAXS_NS + "GrossVolumeIsAuto>";
			}
			if (OT_NoOfPacks != string.Empty && !string.IsNullOrEmpty(OT_PackagingCharges))
            {
				xml += "<" + MAXS_NS + "NoOfPacks>" + OT_NoOfPacks + "</" + MAXS_NS + "NoOfPacks>";
				xml += "<" + MAXS_NS + "NoOfPacksIsAuto>" + "false" + "</" + MAXS_NS + "NoOfPacksIsAuto>";
			}

            if (OT_CodPort != string.Empty && !string.IsNullOrEmpty(OT_CodPort))
            {
                xml += "<" + MAXS_NS + "Port>" + GetPortFromIncoterms(authenticationToken, OT_CodPort) + "</" + MAXS_NS + "Port>";
                xml += "<" + MAXS_NS + "PortAuto>" + "false" + "</" + MAXS_NS + "PortAuto>";
            }

            xml += "</" + MAXS_NS + "Shipping>";	// fine MA_SaleOrdShipping

			xml += "</" + MAXS_NS + "Data>";		// fine di tutti i dati dell'ordine
			xml += "</" + MAXS_NS + "SaleOrd>";		// fine dell'ordine

			return xml;
		}

		///<summary>
		/// Data la riposta di MagicLink vado a cercare l'ID del documento appen creato in Mago
		/// in modo da inserire i dati nella tabella di transcodifica
		///</summary>
		//--------------------------------------------------------------------------------
		public string GetKeyFromMago(string xml, string documentName)
		{
			XDocument xDoc = XDocument.Parse(xml);
			if (xDoc == null)
				return string.Empty;

			XNamespace maxs = null;
			XElement idElement = null;

			if (documentName.Equals("MAGO_CUSTOMERS_OUT", StringComparison.InvariantCultureIgnoreCase))
			{
				maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfinitySyncroConnector.xsd";
				idElement = xDoc.Element(maxs + "Customers").Element(maxs + "Data").Element(maxs + "CustomersSuppliers").Element(maxs + "CustSupp");
			}
            else if (documentName.Equals("MAGO_SALEORDERS_OUT", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfinitySyncroConnector.xsd";
                idElement = xDoc.Element(maxs + "SaleOrd").Element(maxs + "Data").Element(maxs + "SaleOrder").Element(maxs + "SaleOrdId");
            }
            else if (documentName.Equals("MAGO_SALEORDERSERP_OUT", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfinitySyncroConnector.xsd";
                idElement = xDoc.Element(maxs + "SaleOrd").Element(maxs + "Data").Element(maxs + "SaleOrder").Element(maxs + "SaleOrdId");
            }
            else
				return string.Empty;

			//@@TODO da gestire MAGO_BRANCHES_OUT

			return (idElement != null) ? idElement.Value : string.Empty;
		}

        //controlla se xml di risposta di infinity contiene tag <Error>
		//--------------------------------------------------------------------------------
		internal bool IfResponseContainsErrors(string action)
        {
            if (action.Contains("<EntityErrors>") || action.Contains("<EntityError>"))
                return true;
            return false;
        }

		//--------------------------------------------------------------------------------
		internal string GetCommit(ActionToImport action, string applicationId)
        {
            try
			{
                XElement xElem = new XElement(new XElement(action.ActionName, new XAttribute("applicationId",applicationId), new XAttribute("TypeResult","Commit")));
                XElement commitElem = new XElement("Add_" + action.ActionName);
                xElem.Add(commitElem);
                foreach (string key in action.InfinityKeys)
                { 
                   string[] appregKey = key.Split('=');
                   string newKey = string.Empty;
                   if (appregKey[0].Contains("APPREG") && appregKey[1].Contains("{0}"))
                       newKey = string.Format(appregKey[1], action.MagoKey);
                   else
                       newKey = appregKey[1];
                   commitElem.Add(new XAttribute(appregKey[0], newKey));

                }

                return "<?xml version=\"1.0\"?>" + xElem.ToString();
            }
            catch(Exception)
            {
                return string.Empty;
            }
        }

        ///<summary>
        /// Data la riposta di MagicLink vado a cercare TBGuid del documento appen creato in Mago
        /// in modo da inserire i dati nella tabella di transcodifica
        ///</summary>
        //--------------------------------------------------------------------------------
        public string GetTBGuidForMago(string xml, string entityName)
        {
            XDocument xDoc = XDocument.Parse(xml);
            if (xDoc == null)
                return string.Empty;

            XNamespace maxs = null;
            XElement TBGuidElement = null;

            if (entityName.Equals("MAGO_CUSTOMERS_OUT", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfinitySyncroConnector.xsd";
                TBGuidElement = xDoc.Element(maxs + "Customers").Element(maxs + "Data").Element(maxs + "CustomersSuppliers").Element(maxs + "TBGuid");
            }
            else if (entityName.Equals("MAGO_SALEORDERS_OUT", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfinitySyncroConnector.xsd";
                TBGuidElement = xDoc.Element(maxs + "SaleOrd").Element(maxs + "Data").Element(maxs + "SaleOrder").Element(maxs + "TBGuid");
            }

            else if (entityName.Equals("MAGO_SALEORDERSERP_OUT", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfinitySyncroConnector.xsd";
                TBGuidElement = xDoc.Element(maxs + "SaleOrd").Element(maxs + "Data").Element(maxs + "SaleOrder").Element(maxs + "TBGuid");
            }
            else
                return string.Empty;

            return (TBGuidElement != null) ? TBGuidElement.Value : string.Empty;
        }

        public string GetAreaManager(string token, string customer)
        {
            string AreaManger = "";

            ConnectionStringManager connectionStringManager = new ConnectionStringManager(token);
            var conn = new SqlConnection(connectionStringManager.CompanyConnectionString);
            
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT AreaManager FROM MA_CustSuppCustomerOptions WHERE Customer = @customer and CustSuppType='3211264'";
                cmd.Parameters.AddWithValue("@customer", customer);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        AreaManger = reader.GetString(reader.GetOrdinal("AreaManager"));
                    }
                }
            }
            conn.Close();

            return AreaManger;
        }


        public string GetPortFromIncoterms(string token, string incoterms)
        {
            string Port = "";

            ConnectionStringManager connectionStringManager = new ConnectionStringManager(token);
            var conn = new SqlConnection(connectionStringManager.CompanyConnectionString);

            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select Port from MA_Ports where Incoterm = @incoterms ";
                cmd.Parameters.AddWithValue("@incoterms", incoterms);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Port = reader.GetString(reader.GetOrdinal("Port"));
                    }
                }
            }
            conn.Close();

            return Port;
        }
    }
}
