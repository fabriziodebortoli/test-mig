using System.Collections;
using System.Xml;
using System.Xml.XPath;
//
using Microarea.TaskBuilderNet.Licence.Activation.Components;




namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Espone metodi utili al trattamento dei parametri riguardanti l'attivazione e la verifica
	/// della configurazione di un prodotto.
	/// </summary>
	//=========================================================================
	public class SignParametersPreparerForVer1 : ISignParamsPreparer
	{
		//---------------------------------------------------------------------
		public SignParametersPreparerForVer1()
		{}

		/// <summary>
		/// Ritorna un documento XML mondato da tutte le informazioni
		/// che non devono partecipare alla generazione dell'ActivationKey.
		/// Allo stato attuale le informazioni che partecipano alla
		/// generazione dell'ActivationKey sono:
		/// - UserInfo/Company, UserInfo/VatNumber, UserInfo/Country;
		/// - TUTTI i Serial Number dei "SalesModule" il cui attributo
		///   "hasserial" o é "true" o non é presente (quindi "true"
		///   per default), ordinati alfabeticamente.
		/// Assume che i nodi <Serial> non abbiano nodi figli,
		/// ovvero che i nodi <Serial> siano delle foglie.
		/// </summary>
		/// <param name="xmlDoc">Documento XML su cui operare.</param>
		/// <returns>Documento XML modificato.</returns>
		/// <exception cref="XmlException">
		/// Lanciata se si riscontra un errore nei tags relativi alle
		/// informazioni circa l'utente o se si riscontra un errore
		/// estraendo la lista di serial number.
		/// </exception>
		//---------------------------------------------------------------------
		public virtual XmlDocument PrepareParamsForSigning(XmlDocument xmlDoc)
		{
			// Istanza del documento XML da ritornare (xmlParam)
			// e istanza del suo nodo radice (xmlParamRootNode).
			XmlDocument xmlParam			= new XmlDocument();
			XmlNode		xmlParamRootNode	= xmlParam.CreateNode(	XmlNodeType.Element,
																	"ActivationKeyGenerationData",
																	"");			
			// Nodo radice del documento XML in cui viene de-serializzata
			// la stringa passata come parametro al Web Service.
			XmlNode root = xmlDoc.DocumentElement;

			// Estrazione delle UserInfo.
			XmlNode	tempNode, selectedNode;
			string	XPathQuery;
			foreach (string tag in WceStrings.UserInfoTagsVatNr)
			{
				XPathQuery = "descendant::" +	WceStrings.Element.UserInfoFile	+ "/" +
												WceStrings.Element.UserInfo		+ "/" + 
												tag;
				tempNode = xmlParam.CreateNode(XmlNodeType.Element, tag, "");
				// Il metodo SelectSingleNode lancia un'eccezione nel caso l'espressione XPath sia
				// malformata o contenga prefissi. L'eccezione non é catch-ata perché l'espressione XPath
				// é cablata e quindi presumo sia corretta.
				selectedNode = root.SelectSingleNode(XPathQuery);

				if (selectedNode != null)
					// Elimina tutti i caratteri che non siano alfanumerici e maiuscoli.
					tempNode.InnerText	= SNFormatter.CleanSN(selectedNode.InnerText.ToUpperInvariant());
				else
					throw new XmlException(ErrorMessage.Error_In_UserInfo_Element);

				xmlParamRootNode.AppendChild(tempNode);
			}
			// Estrazione dei serial number.
			XPathQuery		= "descendant::" +	WceStrings.Element.LicensedFiles	+	"/"	+
												WceStrings.Element.LicensedFile		+	"/"	+
												WceStrings.Element.Configuration	+	"/"	+
												WceStrings.Element.Product			+	"/"	+
												WceStrings.Element.SalesModule;/*		+ 
												"[@" + WceStrings.Attribute.Producer+	"=\"Microarea\"]";*/

			XPathNavigator	SNNavigator		=	xmlDoc.CreateNavigator();
			XPathExpression	expr			=	SNNavigator.Compile(XPathQuery);
			string sortingAttribute			=	"@" + WceStrings.Attribute.Name;
			expr.AddSort(	sortingAttribute,
							XmlSortOrder.Ascending,
							XmlCaseOrder.UpperFirst,
							"",
							XmlDataType.Text);

			XPathNodeIterator SalesModuleIterator;
			try
			{
				SalesModuleIterator = SNNavigator.Select(expr);
			}
			catch
			{
				throw new XmlException(ErrorMessage.Error_In_LicensedFiles_Element);
			}

			ArrayList orderedSNList		= new ArrayList();
			int		actualLastPosition	= 0;
			int		numberOfInsertion	= 0;
			string	hasserialAttribute	= string.Empty;
			while (SalesModuleIterator.MoveNext())
			{
				hasserialAttribute =
					SalesModuleIterator.Current.GetAttribute(WceStrings.Attribute.HasSerial, "");

				if ((hasserialAttribute == string.Empty) || (hasserialAttribute == "true") )
				{
					XPathNodeIterator SerialNumberIterator =
							SalesModuleIterator.Current.SelectChildren(WceStrings.Element.Serial, "");

					while (SerialNumberIterator.MoveNext())
					{
						orderedSNList.Add(SerialNumberIterator.Current.Value);
						numberOfInsertion++;
					}
					if (numberOfInsertion > 1)
						orderedSNList.Sort(actualLastPosition, numberOfInsertion, null);
					actualLastPosition	= orderedSNList.Count;
					numberOfInsertion	= 0;
				}
			}

			// Istanzio un nodo XML per ogni Serial Number presente nell'ArrayList "orderedSNList"
			// e lo appendo come figlio al nodo radice "xmlParamRootNode" del documento.
			for (int i = 0; i < orderedSNList.Count; i++)
			{
				tempNode = xmlParam.CreateNode(XmlNodeType.Element, WceStrings.Element.Serial, "");
				tempNode.InnerText = ((string)orderedSNList[i]).ToUpperInvariant();
				xmlParamRootNode.AppendChild(tempNode);
			}
			// Appendo il nodo radice al documento XML e ritorno il documento stesso.
			xmlParam.AppendChild(xmlParamRootNode);
			return xmlParam;
		}

		/// <summary>
		/// Rimuove dalla stringa contenente il file xml inviato dall'utente
		/// per l'attivazione del prodotto gli spazi, i new line, gli
		/// horizontal, vertical tab e i carrage return in eccesso.
		/// </summary>
		/// <param name="xmlConfigFile">
		/// stringa contenente il file di configurazione.
		/// </param>
		/// <returns>stringa "pulita".</returns>
		/// <remarks>
		/// Vengono considerati spazi bianchi in eccesso tutti quelli oltre il primo.
		/// Per esempio:la stringa "  test     test" viene ridotta a " test test"
		/// </remarks>
		//---------------------------------------------------------------------
		public void PrepareXmlConfigFile(ref string xmlConfigFile)
		{/*
			xmlConfigFile = xmlConfigFile.Replace('\n', Convert.ToChar(" ", CultureInfo.InvariantCulture));	// rimuove tutti i new line.
			xmlConfigFile = xmlConfigFile.Replace('\t', Convert.ToChar(" ", CultureInfo.InvariantCulture));	// rimuove tutti gli horizontal tab.
			xmlConfigFile = xmlConfigFile.Replace('\r', Convert.ToChar(" ", CultureInfo.InvariantCulture));	// rimuove tutti i carriage return;
			xmlConfigFile = xmlConfigFile.Replace('\v', Convert.ToChar(" ", CultureInfo.InvariantCulture));	// rimuove tutti i vertical tab.

			StringBuilder strBuilder = new StringBuilder();
			StringReader  strReader  = new StringReader(xmlConfigFile);

			char[] buffer = new char[2];	// buffer: 0=current; 1=next.
			int nextChar = -1;

			while ((strReader.Read(buffer, 0, 1)) != 0)
			{
				buffer[1] = Convert.ToChar(((nextChar = strReader.Peek()) != -1 ? nextChar : 0));
				switch (buffer[0])
				{
					case ' ':	
					{
						if (	( buffer[1] == ' ' ) ||
								( buffer[1] == '<' ) ||
								( buffer[1] == '>' ) ||
								( buffer[1] == '=' ) ||
								( strBuilder.ToString().LastIndexOf('>') ==
								strBuilder.Length - 1 ) ||
								( strBuilder.ToString().LastIndexOf('<') ==
								strBuilder.Length - 1 ) ||
								( strBuilder.ToString().LastIndexOf('/') ==
								strBuilder.Length - 1 ) ||
								( strBuilder.ToString().LastIndexOf('=') ==
								strBuilder.Length - 1 ) )
							break;
						strBuilder.Append(buffer[0]);
						break;

					}
					default:	{ strBuilder.Append(buffer[0]);	break; }
				}
			}
			xmlConfigFile = (strBuilder.ToString());*/
		}
	}
}
