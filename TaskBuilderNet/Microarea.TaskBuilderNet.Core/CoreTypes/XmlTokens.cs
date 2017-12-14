using System.Xml;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{	
	/// <summary>
	/// Summary description for XmlWriterHelper.
	/// </summary>
	//============================================================================
	public class XmlWriterHelper
	{
		XmlDocument	dom;
		XmlElement errorsNode;
		XmlElement warningsNode;
		XmlElement diagnosticNode;

		//------------------------------------------------------------------------------
		public XmlDocument	Dom { get { return dom; }}

		//------------------------------------------------------------------------------
		public XmlWriterHelper(XmlDocument dom)
		{
			this.dom = dom;
		}

		//------------------------------------------------------------------------------
		public XmlWriterHelper(string rootName, string tbNamespace, string namespaceURI, string prefix)
		{
			this.dom = new XmlDocument();

			XmlElement reportNode = dom.CreateElement(prefix, rootName, namespaceURI);
			reportNode.SetAttribute(XmlWriterTokens.Attribute.TbNamespace, tbNamespace);
			reportNode.SetAttribute(XmlWriterTokens.XmlNs + prefix, namespaceURI);
			dom.AppendChild(reportNode);	
		}

		//------------------------------------------------------------------------------
		private void CreateDiagnosticNode()
		{
			XmlNodeList list = dom.DocumentElement.GetElementsByTagName(XmlWriterTokens.Element.Diagnostic, dom.DocumentElement.NamespaceURI);
			if (list == null || list.Count <=0)
			{
				diagnosticNode = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Diagnostic, dom.DocumentElement.NamespaceURI);
				dom.DocumentElement.AppendChild(diagnosticNode);
				return;
			}

			diagnosticNode = (XmlElement)list[0];
		}

		//------------------------------------------------------------------------------
		private void CreateErrorsNode()
		{
			if (diagnosticNode == null)
				CreateDiagnosticNode();

			XmlNodeList list = dom.DocumentElement.GetElementsByTagName(XmlWriterTokens.Element.Errors, dom.DocumentElement.NamespaceURI);
			if (list == null || list.Count <=0)
			{
				errorsNode = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Errors, dom.DocumentElement.NamespaceURI);
				diagnosticNode.AppendChild(errorsNode);
				return;
			}

			errorsNode = (XmlElement)list[0];
		}

		//------------------------------------------------------------------------------
		private void CreateWarningsNode()
		{
			if (diagnosticNode == null)
				CreateDiagnosticNode();

			XmlNodeList list = dom.DocumentElement.GetElementsByTagName(XmlWriterTokens.Element.Warnings, dom.DocumentElement.NamespaceURI);
			if (list == null || list.Count <=0)
			{
				warningsNode = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Warnings, dom.DocumentElement.NamespaceURI);
				diagnosticNode.AppendChild(warningsNode);
				return;
			}

			warningsNode = (XmlElement)list[0];
		}

		//------------------------------------------------------------------------------
		public void AddError
			(
				int			errorCode, 
				string		source,
				string		message
				)
		{
			if (errorsNode == null)
				CreateErrorsNode();

			XmlElement errorNode = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Error, dom.DocumentElement.NamespaceURI);
			errorsNode.AppendChild(errorNode);

			XmlElement code = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Code, dom.DocumentElement.NamespaceURI);
			code.InnerXml = (errorCode == 0) ? SoapTypes.To("") : SoapTypes.To(errorCode);
			errorNode.AppendChild(code);

			XmlElement sourceElement = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Source, dom.DocumentElement.NamespaceURI);
			sourceElement.InnerXml = SoapTypes.To(source);
			errorNode.AppendChild(sourceElement);

			XmlElement messageElement = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Message, dom.DocumentElement.NamespaceURI);
			messageElement.InnerXml = SoapTypes.To(message);
			errorNode.AppendChild(messageElement);
		}
		
		//------------------------------------------------------------------------------
		public void AddWarning
			(
				int			warningCode, 
				string		source,
				string		message
			)
		{
			if (warningsNode == null)
				CreateWarningsNode();

			XmlElement warningNode = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Warning, dom.DocumentElement.NamespaceURI);
			warningsNode.AppendChild(warningNode);

			XmlElement code = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Code, dom.DocumentElement.NamespaceURI);
			code.InnerXml = (warningCode == 0) ? SoapTypes.To("") : SoapTypes.To(warningCode);
			warningNode.AppendChild(code);

			XmlElement sourceElement = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Source, dom.DocumentElement.NamespaceURI);
			sourceElement.InnerXml = SoapTypes.To(source);
			warningNode.AppendChild(sourceElement);

			XmlElement messageElement = dom.CreateElement(dom.DocumentElement.Prefix, XmlWriterTokens.Element.Message, dom.DocumentElement.NamespaceURI);
			messageElement.InnerXml = SoapTypes.To(message);
			warningNode.AppendChild(messageElement);
		}
	}

	/// <summary>
	/// Summary description for XmlWriterTokens.
	/// </summary>
	//============================================================================
	public class XmlWriterTokens
	{
		public const string SchemaPrefix = "http://www.microarea.it/Schema/2004/Smart/{0}/{1}/{2}{3}.xsd";
		public const string XmlNs = "xmlns:";
		public const string Prefix = "maxs";

		public class Element
		{
			public const string Data		= "Data";
			public const string ReportData	= "ReportData";
			public const string Parameters	= "Parameters";
			public const string Group		= "Group";
			public const string Diagnostic	= "Diagnostic";
			public const string CDiagnostic = "CDiagnostic";
			public const string Errors		= "Errors";
			public const string Error		= "Error";
			public const string Warnings	= "Warnings";
			public const string Warning		= "Warning";
			public const string Code		= "Code";
			public const string Source		= "Source";
			public const string Message		= "Message";
			public const string Table		= "Table";
			public const string Row			= "Row";
		}

		public class Attribute
		{
			public const string RowType			= "type";
			public const string GroupTitle		= "title";
			public const string TbNamespace		= "tbNamespace";
			public const string AskDialogTitle	= "title";
			public const string EntryType		= "type";
			public const string EntryLength		= "length";
			public const string EntryCaption	= "title";
			public const string InputLimit		= "inputLimit";
			public const string ControlType		= "controlType";
            public const string IsValid         = "IsValid";
		}

		public class AttributeValue
		{
			public const string Upper	= "Upper";
			public const string Lower	= "Lower";
			public const string Check	= "Check";
			public const string Radio	= "Radio";
			public const string Combo	= "Combo";
			public const string Text	= "Text";
		}
	}

	//============================================================================
	public class RdeWriterTokens
	{
		public class Element
		{
			public const string Report		= "Report";
			public const string SubTotal	= "SubTotal";
			public const string Total		= "Total";
			public const string Cell		= "Cell";
			public const string TotalPages	= "TotalPages";
			public const string Alias		= "Alias";
			public const string Graphics	= "Graphics";
			public const string LowerInput	= "LowerInput";
			public const string UpperInput	= "UpperInput";
			public const string NextLine	= "NextLine";
			public const string SpaceLine	= "SpaceLine";
			public const string Interline	= "Interline";
			public const string NewPage		= "NewPage";
			public const string Message		= "Message";
		}

		public class Attribute
		{
			public const string Number			= "Number";
			public const string Type			= "Type";
			public const string BaseType		= "BaseType";
			public const string WoormType		= "WoormType";
			public const string Message			= "Message";
			public const string ID				= "ID";
			public const string Value			= "Value";
			public const string CellTail		= "CellTail";
			public const string Release			= "Release";
			public const string Source			= "Source";
			public const string Name			= "Name";
			public const string Culture			= "Culture";
			public const string IsColumn		= "IsColumn";
  		}
	}
}
