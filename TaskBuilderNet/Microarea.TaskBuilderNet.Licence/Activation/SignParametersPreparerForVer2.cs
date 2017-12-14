using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Microarea.TaskBuilderNet.Licence.Activation.Components;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// SignParametersPreparerForVer3.
	/// </summary>
	//=========================================================================
	public class SignParametersPreparerForVer2 : SignParametersPreparerForVer1
	{
		//---------------------------------------------------------------------
		public SignParametersPreparerForVer2()
		{}

		#region ISignParamsPreparer Members

		//---------------------------------------------------------------------
		public override System.Xml.XmlDocument PrepareParamsForSigning(System.Xml.XmlDocument xmlDoc)
		{
			// Istanza del documento XML da ritornare (xmlParam)
			// e istanza del suo nodo radice (xmlParamRootNode).
			XmlDocument xmlParam			= new XmlDocument();
			XmlNode		xmlParamRootNode	= xmlParam.CreateNode(
				XmlNodeType.Element,
				"ActivationKeyGenerationData",
				""
				);			
			// Nodo radice del documento XML in cui viene de-serializzata
			// la stringa passata come parametro al Web Service.
			XmlNode root = xmlDoc.DocumentElement;

			// Estrazione delle UserInfo.
			XmlNode	tempNode, selectedNode;
			string	XPathQuery;

			XPathQuery = String.Format(
				CultureInfo.InvariantCulture,
				"descendant::{0}/{1}/{2}",
				WceStrings.Element.UserInfoFile,
				WceStrings.Element.UserInfo,
				WceStrings.Element.CodFisc
				);

			string[] list = WceStrings.UserInfoTagsVatNr;
			selectedNode = root.SelectSingleNode(XPathQuery);
			if (selectedNode != null && selectedNode.InnerText.Length > 0)
				list = WceStrings.UserInfoTagsCodFisc;

			foreach (string tag in list)
			{
				XPathQuery = String.Format(
					CultureInfo.InvariantCulture,
					"descendant::{0}/{1}/{2}",
					WceStrings.Element.UserInfoFile,
					WceStrings.Element.UserInfo,
					tag
					);
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
			XPathQuery = String.Format(CultureInfo.InvariantCulture, "descendant::{0}", WceStrings.Element.Serial);

			XPathNavigator	SNNavigator		=	xmlDoc.CreateNavigator();
			XPathExpression	expr			=	SNNavigator.Compile(XPathQuery);
			string sortingAttribute			=	".";

			expr.AddSort(
				sortingAttribute,
				Comparer.DefaultInvariant
				);

			XPathNodeIterator SerialNumberIterator;
			try
			{
				SerialNumberIterator = SNNavigator.Select(expr);
			}
			catch
			{
				throw new XmlException(ErrorMessage.Error_In_LicensedFiles_Element);
			}

			// Istanzio un nodo XML per ogni Serial Number estratto in maniera ordinata
			// e lo appendo come figlio al nodo radice "xmlParamRootNode" del documento.
			while (SerialNumberIterator.MoveNext())
			{
				tempNode = xmlParam.CreateNode(	XmlNodeType.Element, WceStrings.Element.Serial, "");
				tempNode.InnerText = SerialNumberIterator.Current.Value.ToUpperInvariant();
				xmlParamRootNode.AppendChild(tempNode);
			}
			// Appendo il nodo radice al documento XML e ritorno il documento stesso.
			xmlParam.AppendChild(xmlParamRootNode);
			return xmlParam;
		}

		#endregion
	}
}
