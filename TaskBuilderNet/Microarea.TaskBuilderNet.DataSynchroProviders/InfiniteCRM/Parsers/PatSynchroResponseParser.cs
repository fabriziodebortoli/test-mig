using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers
{
	//================================================================================
	internal static class PatSynchroResponseParser
	{
		//--------------------------------------------------------------------------------
		internal static PatSynchroResponseInfo GetResponseInfo(string xmlResponse)
		{
			PatSynchroResponseInfo responseInfo = new PatSynchroResponseInfo();
			if (string.IsNullOrWhiteSpace(xmlResponse))
				return responseInfo;

			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.LoadXml(xmlResponse);

				//root
				XmlElement root = xmlDoc.DocumentElement;
				if (string.Compare(root.Name, InfiniteCRMSynchroResponseXML.Element.Results, StringComparison.InvariantCultureIgnoreCase) != 0)
					throw (new Exception("No root 'Results' available!"));

				// cerco i nodi Result
				XmlNodeList resultElemList = root.GetElementsByTagName(InfiniteCRMSynchroResponseXML.Element.Result);
				if (resultElemList != null && resultElemList.Count > 0)
				{
					foreach (XmlElement resultElem in resultElemList)
						ParseResultNode(resultElem, responseInfo);
				}
			}
			catch
			{
				throw;
			}

			return responseInfo;
		}

		///<summary>
		/// Eseguo il parse di un nodo di tipo <Result>
		///</summary>
		//--------------------------------------------------------------------------------
		private static void ParseResultNode(XmlElement resultElem, PatSynchroResponseInfo responseInfo)
		{
			Result result = new Result();

			result.Code = resultElem.GetAttribute(InfiniteCRMSynchroResponseXML.Attribute.Code);
			result.Operation = resultElem.GetAttribute(InfiniteCRMSynchroResponseXML.Attribute.Operation);
			result.Description = resultElem.GetAttribute(InfiniteCRMSynchroResponseXML.Attribute.Desc);

			if (resultElem.HasChildNodes)
			{
				XmlElement child = (XmlElement)resultElem.FirstChild;

				if (string.Compare(result.Code, "ok", StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					if (string.Compare(child.LocalName, InfiniteCRMSynchroResponseXML.Element.Exception, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						foreach (XmlElement elem in child.ChildNodes)
						{
							if (string.Compare(elem.LocalName, InfiniteCRMSynchroResponseXML.Element.Message, StringComparison.InvariantCultureIgnoreCase) == 0)
								result.Message = elem.InnerText;

							if (string.Compare(elem.LocalName, InfiniteCRMSynchroResponseXML.Element.StackTrace, StringComparison.InvariantCultureIgnoreCase) == 0)
								result.StackTrace = elem.InnerText;
						}
					}
				}
				else
				{
					string id = child.GetAttribute(InfiniteCRMSynchroResponseXML.Attribute.Id);
					if (!string.IsNullOrWhiteSpace(id))
					{
						result.EntityName = child.Name;
						result.Id = id;
					}
				}
			}

			responseInfo.Results.Add(result);
		}

		///<summary>
		/// parsing di xml di risposta di PAT
		/// se non OK allora c'e' stato un errore oppure la risposta e' vuota
		///</summary>
		//--------------------------------------------------------------------------------
		public static bool ParseResponse(string responseXml, out string resultCode, string recordCountTag = "")
		{
			resultCode = string.Empty;

            try
            {
				XElement response = GetRootElement(responseXml);
				if (response == null)
					return false;

				// leggo il valore dell'attributo code del nodo Result
				resultCode = response.Attribute(XName.Get("code")).Value;

                if (response.Attribute(XName.Get("code")).Value.Equals("OK"))
                {
					// se il recordcount == 0 torno cmq false (mi serve per non procedere nella SynchronizeOutbound)
                    if (!string.IsNullOrEmpty(recordCountTag) && response.Element(XName.Get(recordCountTag)).Attribute(XName.Get("recordcount")).Value.Equals("0"))
                        return false;

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
		}

		/// <summary>
		/// ritorna frammento xml senza tag results e result
		/// </summary>
		//--------------------------------------------------------------------------------
		public static XElement GetRootElement(string xml)
		{
            try
            {
                XDocument xDoc = XDocument.Parse(xml);
				if (xDoc == null)
					return null;

                // estrae il frammento xml da parsare senza tag results e result
                return xDoc.Element(XName.Get("Results")).Element(XName.Get("Result"));
            }
            catch
            {
                return null;
            }
		}

		/// <summary>
		/// parsing del singolo account
        /// se non trovo account id nella tabella di transcodifica? cosa faccio?
		/// </summary>
		//--------------------------------------------------------------------------------
		public static SetDataInfo ParseSingleAccount(XElement accountXml, TranscodingManager TranscodingMng, string companyConnectionString)
		{
			try
			{
				SetDataInfo dataInfo = new SetDataInfo();
				XElement bankAccountElem = accountXml.Element(XName.Get("BankAccount")).Element(XName.Get("BankAccount"));
				string patID = accountXml.Element(XName.Get("ID")).Value;
				if (string.IsNullOrEmpty(patID))
					return null;
				// nel caso in cui il cliente esista gia' in ERP vado in update (deciso con Flavio)
				string magoKey = TranscodingMng.GetRecordKey(companyConnectionString, "Account", patID);

				string priceList = string.Empty;
				if (!accountXml.Element("PriceListID").Value.Equals("-1"))
					priceList = TranscodingMng.GetRecordKey(companyConnectionString, "PriceList", accountXml.Element("PriceListID").Value);
              
                string account = @"      
                <maxs:Customers tbNamespace=""Document.ERP.CustomersSuppliers.Documents.Customers"" xTechProfile=""InfiniteCRM"" xmlns:maxs=""http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfiniteCRM.xsd"" >
                <maxs:Data>
                <maxs:CustomersSuppliers master=""true"">" +
				(string.IsNullOrWhiteSpace(magoKey) ? string.Empty : string.Format("<maxs:CustSupp>{0}</maxs:CustSupp>", magoKey)) +  // se CustSupp esiste vado in update
                "<maxs:CompanyName>" + accountXml.Element("CompanyName").Value + @"</maxs:CompanyName>
                <maxs:InsertionDate>" + DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + @"</maxs:InsertionDate> 
                <maxs:TaxIdNumber>" + accountXml.Element("VATCode").Value + @"</maxs:TaxIdNumber>
                <maxs:FiscalCode>" + accountXml.Element("FiscalCode").Value + @"</maxs:FiscalCode>
                <maxs:Address>" + accountXml.Element("Address1").Value + @"</maxs:Address>      
                <maxs:Address2>" + accountXml.Element("Address2").Value + @"</maxs:Address2>      
                <maxs:ZIPCode>" + accountXml.Element("ZipCode").Value + @"</maxs:ZIPCode>
                <maxs:City>" + accountXml.Element("City").Value + @"</maxs:City>
                <maxs:County>" + (accountXml.Element("ProvinceID").Value.Equals("-1") ? string.Empty : accountXml.Element("ProvinceID").Value) + @"</maxs:County>
                <maxs:Country>" + accountXml.Element("CountryID").Value + @"</maxs:Country> 
                <maxs:Telephone1>" + accountXml.Element("Phone").Value + @"</maxs:Telephone1>  
                <maxs:Telephone2>" + accountXml.Element("Mobile").Value + @"</maxs:Telephone2>
                <maxs:Fax>" + accountXml.Element("FAX").Value + @"</maxs:Fax>
                <maxs:Internet>" + accountXml.Element("URL").Value + @"</maxs:Internet>
                <maxs:EMail>" + accountXml.Element("EMail").Value + @"</maxs:EMail>
                <maxs:ISOCountryCode>" + accountXml.Element("CountryID").Value + @"</maxs:ISOCountryCode>
                <maxs:Payment>" + (accountXml.Element("PaymentTypeID").Value.Equals("-1") ? string.Empty : accountXml.Element("PaymentTypeID").Value) + @"</maxs:Payment>
                <maxs:PriceList>" + priceList + @"</maxs:PriceList>
                <maxs:IBAN>" + bankAccountElem.Element("IBAN").Value + @"</maxs:IBAN>
                <maxs:IBANIsManual>true</maxs:IBANIsManual>
                <maxs:CA></maxs:CA>
                <maxs:CIN>" + bankAccountElem.Element("CIN").Value + @"</maxs:CIN>
                <maxs:Currency>EUR</maxs:Currency>
                <maxs:Notes>" + accountXml.Element("Note").Value + @"</maxs:Notes>
                <maxs:ExternalCode>" + patID + @"</maxs:ExternalCode>
                </maxs:CustomersSuppliers>
                </maxs:Data>
                </maxs:Customers>";
              
				dataInfo.PatID = patID;
				dataInfo.MagoID = magoKey; 
				dataInfo.MagoXml = account;
				dataInfo.EntityName = "Account";
				dataInfo.MagoTableName = "MA_CustSupp";
                dataInfo.Namespace = "Document.ERP.CustomersSuppliers.Documents.Customers";
				dataInfo.PatXml = accountXml.ToString();

				return dataInfo;
			}
			catch (Exception e)
			{
				throw new DSException("PatSingleResponseParser.ParseSingleAccount", e.Message);
			}
		}


		/// <summary>
		/// parsa un singolo order e richiede le sue righe
        /// Recovery per il customer id non esistemte in transcodifica
        /// Vado ad inserirlo? O cosa faccio?
		/// </summary>
		/// <param name="elem"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------
		public static SetDataInfo ParseSingleOrder(XElement orderElem, TranscodingManager TranscodingMng, string companyConnString)
		{
			string patId = string.Empty;

			try
			{
				patId = orderElem.Element(XName.Get("Order")).Element(XName.Get("ID")).Value;
			}
			catch (Exception)
			{
				patId = string.Empty;
			}
			
			if (string.IsNullOrEmpty(patId))
				return null;

			// nel caso in cui l'ordine esista gia' in ERP vado in update 
			// (anche se un ordine inserito gia' in Mago non dovrebbe essere modificato dopo in Pat, 
			// ma x evitare il proliferare di ordini in Mago che punto allo stesso ordine in Pat facciamo questo controllo)
			string magoKey = TranscodingMng.GetRecordKey(companyConnString, "Order", patId);

			string customer = string.Empty;
			string accountId = string.Empty;
            
			try 
			{	   
     			accountId = orderElem.Element(XName.Get("Order")).Element(XName.Get("AccountID")).Value;
				customer = TranscodingMng.GetRecordKey(companyConnString, "Account", accountId);
			}
			catch (Exception)
			{
				customer = string.Empty;
			}
			
			// non posso andare avanti nell'inserimento dell'ordine in Mago perche' non esiste nella tabella di transcodifica l'ID dell'Account referenziato nell'ordine di Pat
			if (string.IsNullOrEmpty(customer))
				throw new DSException("PatSynchroResponseParser.ParseSingleOrder", string.Format(Resources.MissingTranscodingKey, "Account"), string.Format(Resources.UnableToInsertOrder, "Account", accountId));

			IEnumerable<XElement> patOrderItems = null;

			try
			{
				patOrderItems = orderElem.Element(XName.Get("OrderItems")).Elements(XName.Get("OrderItem"));
				orderElem = orderElem.Element(XName.Get("Order"));
			}
			catch (Exception)
			{
				return null;
			}

			SetDataInfo orderHeadWithLines = new SetDataInfo();
			List<SetDataInfo> magoOrderItemsParsed = ParseOrderItems(patOrderItems, TranscodingMng, companyConnString);

            if (magoOrderItemsParsed == null || magoOrderItemsParsed.Count == 0)
                return null;

			string order = string.Empty;
			
			try
			{
				string externalOrdNo = orderElem.Element(XName.Get("NumberPrefix")).Value + orderElem.Element(XName.Get("Number")).Value + orderElem.Element(XName.Get("NumberVersion")).Value;
				externalOrdNo = externalOrdNo.Replace(" ", string.Empty);

				order = @"<maxs:SaleOrd tbNamespace=""Document.ERP.SaleOrders.Documents.SaleOrd"" xTechProfile=""InfiniteCRM"" xmlns:maxs=""http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfiniteCRM.xsd"">
                            <maxs:Data>
                            <maxs:SaleOrder master=""true"">" +
							(string.IsNullOrWhiteSpace(magoKey) ? string.Empty : string.Format("<maxs:SaleOrdId>{0}</maxs:SaleOrdId>", magoKey)) + // se OrderID esiste vado in update
                            "<maxs:ExternalOrdNo>" + externalOrdNo + @"</maxs:ExternalOrdNo>
                            <maxs:OrderDate>" + XmlConvert.ToDateTime(orderElem.Element(XName.Get("Date")).Value, XmlDateTimeSerializationMode.Local).ToString("s") + @"</maxs:OrderDate>
							<maxs:ConfirmedDeliveryDate>" +  XmlConvert.ToDateTime(orderElem.Element(XName.Get("ExpirationDate")).Value, XmlDateTimeSerializationMode.Local).ToString("s") + @"</maxs:ConfirmedDeliveryDate>
							<maxs:ExpectedDeliveryDate>" + XmlConvert.ToDateTime(orderElem.Element(XName.Get("ExpirationDate")).Value, XmlDateTimeSerializationMode.Local).ToString("s") + @"</maxs:ExpectedDeliveryDate>
                            <maxs:Customer>" + customer + @"</maxs:Customer>
                            <maxs:YourReference>" + orderElem.Element(XName.Get("AccountReferenceOrder")).Value + @"</maxs:YourReference>
                            <maxs:Payment>" + (orderElem.Element(XName.Get("PaymentTypeID")).Value.Equals("-1") ? string.Empty : orderElem.Element(XName.Get("PaymentTypeID")).Value) + @"</maxs:Payment> 
                            <maxs:Currency>EUR</maxs:Currency>
                            </maxs:SaleOrder>
                            <maxs:Detail>";

				foreach (SetDataInfo data in magoOrderItemsParsed)
					order += data.MagoXml;

				order += @"</maxs:Detail>
                    <maxs:Shipping>
                    <maxs:Port>" + (orderElem.Element(XName.Get("PortTypeID")).Value.Equals("-1") ? string.Empty : orderElem.Element(XName.Get("PortTypeID")).Value) + @"</maxs:Port>
                    <maxs:Package>" + (orderElem.Element(XName.Get("PackTypeID")).Value.Equals("-1") ? string.Empty : orderElem.Element(XName.Get("PackTypeID")).Value) + @"</maxs:Package> 
                    <maxs:Transport>" + (orderElem.Element(XName.Get("TransportTypeID")).Value.Equals("-1") ? string.Empty : orderElem.Element(XName.Get("TransportTypeID")).Value) + @"</maxs:Transport>
                    </maxs:Shipping>
                    <maxs:Notes><maxs:Notes>" + orderElem.Element(XName.Get("Note")).Value + @"</maxs:Notes></maxs:Notes>
                    </maxs:Data>
                    </maxs:SaleOrd>";
			}
			catch (Exception)
			{
				return null;
			}

			orderHeadWithLines.MagoXml = order;
			orderHeadWithLines.PatID = patId;
			orderHeadWithLines.MagoID = magoKey;
			orderHeadWithLines.EntityName = "Order";
			orderHeadWithLines.MagoTableName = "MA_SaleOrd";
            orderHeadWithLines.Namespace = "Document.ERP.SaleOrders.Documents.SaleOrd";
			orderHeadWithLines.PatXml = orderElem.ToString();
            orderHeadWithLines.PatRows.AddRange(magoOrderItemsParsed);

			return orderHeadWithLines;
		}

        //Qui serve la recovery. Se non trovo id nella tabella di transcodifica allora vado ad inserire l'articiolo in pat
		//--------------------------------------------------------------------------------
		private static List<SetDataInfo> ParseOrderItems(IEnumerable<XElement> patOrderItems, TranscodingManager TranscodingMng, string companyConnectionString)
		{
			List<SetDataInfo> orderItems = new List<SetDataInfo>();

            foreach (XElement orderItem in patOrderItems)
            {
				string item = string.Empty;
				string itemId = string.Empty;
				bool isDescriptionRow = false;

                try
                {
                    // row Total/Subtotal no considered
					if (orderItem.Element(XName.Get("RowType")).Value.Equals("Total") || orderItem.Element(XName.Get("RowType")).Value.Equals("Subtotal"))
                        continue;

					// row Description
					if (orderItem.Element(XName.Get("RowType")).Value.Equals("Description"))
						isDescriptionRow = true;
					
					// row Item
					if (orderItem.Element(XName.Get("RowType")).Value.Equals("Item"))
					{
						itemId = orderItem.Element(XName.Get("ItemID")).Value;
						item = TranscodingMng.GetRecordKey(companyConnectionString, "Item", itemId);
					}
                }
                catch (Exception)
                {
                    continue;
                }

				if (string.IsNullOrWhiteSpace(item) && !isDescriptionRow)
					throw new DSException("PatSynchroResponse.ParseOrderItems", string.Format(Resources.MissingTranscodingKey, "Item"), string.Format(Resources.UnableToInsertOrder, "Item", itemId));

                try
                {
					SetDataInfo singleOrderItem = new SetDataInfo();
					singleOrderItem.PatID = orderItem.Element(XName.Get("ID")).Value; // l'ID viene ritornato anche per le righe descrittive

					string lineType = string.Empty;
					string saleType = string.Empty;

                    switch (orderItem.Element(XName.Get("RowType")).Value)
                    {
						case "Item": lineType = "3538947"; break; // in Mago inserisco una riga di tipo Merce
						case "Description": lineType = "3538944"; break; // in Mago inserisco una riga di tipo Nota
                        default: lineType = string.Empty; break;
                    }

                    switch (orderItem.Element(XName.Get("InTotal")).Value)
                    {
                        case "true": saleType = "3670020"; break;
                        case "false": saleType = "3670019"; break;
                        default: saleType = string.Empty; break;
                    }

                    string magoOrderItem = @"
                                    <maxs:DetailRow>
                                    <maxs:Line>" + (Int32.Parse(orderItem.Element(XName.Get("RowIndex")).Value) + 1) + @"</maxs:Line>
                                    <maxs:LineType>" + lineType + @"</maxs:LineType> 
                                    <maxs:Description>" + orderItem.Element(XName.Get("Description")).Value + @"</maxs:Description> 
                                    <maxs:Item>" + item + @"</maxs:Item> 
                                    <maxs:Qty>" + orderItem.Element(XName.Get("Quantity")).Value + @"</maxs:Qty>
                                    <maxs:UnitValue>" + orderItem.Element(XName.Get("UnitPrice")).Value + @"</maxs:UnitValue>
                                    <maxs:TaxableAmount>" + orderItem.Element(XName.Get("TotalPrice")).Value + @"</maxs:TaxableAmount> 
                                    <maxs:DiscountFormula>" + orderItem.Element(XName.Get("DiscountPercentage")).Value + @"</maxs:DiscountFormula>
                                    <maxs:DiscountAmount>" + orderItem.Element(XName.Get("TotalDiscount")).Value + @"</maxs:DiscountAmount>
                                    <maxs:SaleType>" + saleType + @"</maxs:SaleType>
                                    <maxs:Notes>" + orderItem.Element(XName.Get("Note")).Value + @"</maxs:Notes>
                                    </maxs:DetailRow>";

                    singleOrderItem.MagoTableName = "MA_SaleOrdDetails";
                    singleOrderItem.EntityName = "OrderItem";
                    singleOrderItem.MagoXml = magoOrderItem;
 
                    orderItems.Add(singleOrderItem);
                }
                catch (Exception)
                {
                    return null;
                }
            }

			return orderItems;
		}

		///<summary>
		/// Data la stringa xml ritornata da Pat con l'elenco degli accounts di tipo Prospect
		/// con degli ordini aperti, vado ad estrarre gli AccountID e compongo la query da eseguire
		/// per estrarre i dati degli Account (da inserire poi in Mago)
		///</summary>
		//--------------------------------------------------------------------------------
		public static string GetProspectsQuery(string xml)
		{
			string query =
				@"<Operations><Operation>
				  <Get>
					<Accounts>
					  <Filters>
						<Filter field=""ID"" op=""EqualTo"" value=""{0}"" />
					  </Filters>
					</Accounts>
				  </Get></Operation></Operations>";

			XElement rootElem = GetRootElement(xml);
			if (rootElem == null)
				return string.Empty;

			// cerco tutti i nodi di tipo <Order>
			IEnumerable<XElement> orderList = null;

			try
			{
				orderList = rootElem.Element(XName.Get("Orders")).Elements(XName.Get("Order"));
			}
			catch (Exception)
			{
				orderList = null;
			}

			if (orderList == null)
				return string.Empty;

			List<string> accountIdsList = new List<string>();

			// cerco tutti i nodi di tipo <AccountID> e compongo la stringa per il filtro 
			foreach (XElement xOrder in orderList)
			{
				string accountID = string.Empty;

				try
				{
					accountID = xOrder.Element(XName.Get("AccountID")).Value;
				}
				catch (Exception)
				{
					accountID = string.Empty;
				}

				if (!string.IsNullOrWhiteSpace(accountID) && !accountIdsList.Contains(accountID))
					accountIdsList.Add(accountID);
			}

			if (accountIdsList.Count == 0)
				return string.Empty;

			// compongo la stringa completa della query con i filtri
			string ids = string.Join("|", accountIdsList);

			return string.Format(query, ids);
		}

		///<summary>
		/// Data la riposta di MagicLink vado a cercare l'ID del documento appen creato in Mago
		/// in modo da inserire i dati nella tabella di transcodifica
		///</summary>
		//--------------------------------------------------------------------------------
		public static string GetKeyFromMago(string xml, string entityName)
		{
			XDocument xDoc = XDocument.Parse(xml);
			if (xDoc == null)
				return string.Empty;

			XNamespace maxs = null;
			XElement idElement = null;

			if (entityName.Equals("Account", StringComparison.InvariantCultureIgnoreCase))
			{
				maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfiniteCRM.xsd";
				idElement = xDoc.Element(maxs + "Customers").Element(maxs + "Data").Element(maxs + "CustomersSuppliers").Element(maxs + "CustSupp");
			}
			else if (entityName.Equals("Order", StringComparison.InvariantCultureIgnoreCase))
			{
				maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfiniteCRM.xsd";
				idElement = xDoc.Element(maxs + "SaleOrd").Element(maxs + "Data").Element(maxs + "SaleOrder").Element(maxs + "SaleOrdId");
			}
			else
				return string.Empty;
			
			return (idElement != null) ? idElement.Value : string.Empty;
		}


        ///<summary>
        /// Data la riposta di MagicLink vado a cercare TBGuid del documento appen creato in Mago
        /// in modo da inserire i dati nella tabella di transcodifica
        ///</summary>
        //--------------------------------------------------------------------------------
        public static string GetTBGuidForMago(string xml, string entityName)
        {
            XDocument xDoc = XDocument.Parse(xml);
            if (xDoc == null)
                return string.Empty;

            XNamespace maxs = null;
            XElement TBGuidElement = null;

            if (entityName.Equals("Account", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/CustomersSuppliers/Customers/Standard/InfiniteCRM.xsd";
                TBGuidElement = xDoc.Element(maxs + "Customers").Element(maxs + "Data").Element(maxs + "CustomersSuppliers").Element(maxs + "TBGuid");
            }
            else if (entityName.Equals("Order", StringComparison.InvariantCultureIgnoreCase))
            {
                maxs = "http://www.microarea.it/Schema/2004/Smart/ERP/SaleOrders/SaleOrd/Standard/InfiniteCRM.xsd";
                TBGuidElement = xDoc.Element(maxs + "SaleOrd").Element(maxs + "Data").Element(maxs + "SaleOrder").Element(maxs + "TBGuid");
            }
            else
                return string.Empty;

            return (TBGuidElement != null) ? TBGuidElement.Value : string.Empty;
        }

		/// <summary>
		/// Customer: ObjectTypeID da MAGO_PROSPECT a MAGO_CUSTOMER - set AccountCode con la pk assegnata in mago
		/// Order: OrderStatusID da MAGO_CONFIRMING to MAGO_OPENED
		/// </summary>
		//--------------------------------------------------------------------------------
		public static string GetPatUpdateString(List<SetDataInfo> dataInfoList)
		{
			XDocument xDoc = null;
			string updateAccount = "<Operations>";
            try
            {
                foreach (SetDataInfo elem in dataInfoList)
                {
                    if (!elem.Inserted) // se il documento non e' stato inserito in mago non vado a cambiare lo stato in pat
                        continue;

                    xDoc = XDocument.Parse(elem.PatXml);
					if (elem.EntityName.Equals("Account"))
					{
						xDoc.Element(XName.Get(elem.EntityName)).SetElementValue(XName.Get("ObjectTypeID"), "MAGO_CUSTOMER");
						xDoc.Element(XName.Get(elem.EntityName)).SetElementValue(XName.Get("AccountCode"), elem.MagoID);
					}
					else if (elem.EntityName.Equals("Order"))
						xDoc.Element(XName.Get(elem.EntityName)).SetElementValue(XName.Get("OrderStatusID"), "MAGO_OPENED");
					else continue;

                    updateAccount += "<Operation><Set>";
                    updateAccount += xDoc.ToString();
                    updateAccount += "</Set></Operation>";
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

			updateAccount += " </Operations>";

			return updateAccount;
		}
	}
}
