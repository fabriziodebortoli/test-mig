using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;


namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// SignParametersPreparerForVer3.
	/// </summary>
	//=========================================================================
	public class SignParametersPreparerForDemo : SignParametersPreparerForVer1
	{
		#region ISignParamsPreparer Members

		//---------------------------------------------------------------------
		public override XmlDocument PrepareParamsForSigning(XmlDocument xmlDoc)
		{
			// Istanza del documento XML da ritornare (xmlParam)
			// e istanza del suo nodo radice (xmlParamRootNode).
			XmlDocument xmlParam = new XmlDocument();
			XmlNode xmlParamRootNode = xmlParam.CreateNode(
				XmlNodeType.Element,
				"ActivationKeyGenerationData",
				""
				);
			// Nodo radice del documento XML in cui viene de-serializzata
			// la stringa passata come parametro al Web Service.
			XmlNode root = xmlDoc.DocumentElement;

			// Estrazione delle UserInfo.
			XmlNode tempNode;
			// Estrazione dei serial number.
			string XPathQuery = String.Format(CultureInfo.InvariantCulture, "descendant::{0}", WceStrings.Element.Serial);

			XPathNavigator SNNavigator = xmlDoc.CreateNavigator();
			XPathExpression expr = SNNavigator.Compile(XPathQuery);
			string sortingAttribute = ".";

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
				tempNode = xmlParam.CreateNode(XmlNodeType.Element, WceStrings.Element.Serial, "");
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
