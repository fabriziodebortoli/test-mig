using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;


namespace Microarea.Tools.TBLocalizer
{	
	
	/// <summary>
	/// Summary description for LowerXmlDocument.
	/// </summary>
	//=========================================================================
	public class LowerCaseXmlDocument: LocalizerDocument
	{
		//---------------------------------------------------------------------
		public override void Load(string fileName)
		{
			LocalizerDocument tmpDoc =  new LocalizerDocument ();
			tmpDoc.Load (fileName);
		
			XsltArgumentList argObjs = new XsltArgumentList();
			argObjs.AddExtensionObject(XSLTObject.Urn, new XSLTObject(String.Empty));

			Transform(tmpDoc, argObjs);
		}

		//---------------------------------------------------------------------
		public override void LoadXml(string xmlString)
		{
			LocalizerDocument tmpDoc =  new LocalizerDocument ();
			tmpDoc.LoadXml (xmlString);
		
			XsltArgumentList argObjs = new XsltArgumentList();
			argObjs.AddExtensionObject(XSLTObject.Urn, new XSLTObject(String.Empty));

			Transform(tmpDoc, argObjs);
		}

		//---------------------------------------------------------------------
		private bool Transform(LocalizerDocument input, XsltArgumentList argObjs)
		{
			try
			{	
				// Creo un XPathNavigator da usare per la trasformazione.
				XPathNavigator nav = input.CreateNavigator();
				
				// Trasformo il file di input e butto l'output in uno stream in memoria
				XslCompiledTransform xslt = LoadStyleSheet();
				
				MemoryStream ms = new MemoryStream ();
				xslt.Transform(nav, argObjs, ms);
				
				// Mi posiziono all'inizio delo stream
				ms.Seek (0,0);
				
				// Carico lo stream nel documento di output
				Load (ms);

				return true;
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
				return false;
			}
		}

		//-----------------------------------------------------------------------------
		private XslCompiledTransform LoadStyleSheet()
		{
			XslCompiledTransform xslt = null;
			try
			{
				xslt = new XslCompiledTransform();
				Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Tools.TBLocalizer.XSLT.ToLower.xslt");
				xslt.Load(new XmlTextReader(s));					
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
			}
			return xslt;
		}
	}
}
