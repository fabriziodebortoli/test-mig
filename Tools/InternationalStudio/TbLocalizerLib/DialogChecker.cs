using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	
	
	/// <summary>
	/// Summary description for DialogChecker.
	/// </summary>
	public class DialogChecker
	{
		//---------------------------------------------------------------------
		public static void Check(LocalizerTreeNode node, IWin32Window owner)
		{
			try
			{
				CheckDialogForm d = new CheckDialogForm();
				if (d.ShowDialog(owner) == DialogResult.OK)
				{
					LocalizerDocument doc = new LocalizerDocument();
					doc.AppendChild(doc.CreateElement(AllStrings.applicationsCap));
			
					Check(float.Parse(d.Ratio.Text)/100, node, doc, d.Translate.Checked);
					doc.Save(d.OutputFile.Text);
					MessageBox.Show(Form.ActiveForm, Strings.OperationCompleted);
					Process.Start(d.OutputFile.Text);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, ex.Message);
			}
			
			node.Collapse();
		}
		
		//---------------------------------------------------------------------
		public static void Check(float ratio, LocalizerTreeNode node, LocalizerDocument doc, bool translate)
		{
			if (node == null) return;

			if (node.Type == NodeType.RESOURCE &&
				node.Name != AllStrings.dialog) return;
	
			node.Expand();

			if (node.Type == NodeType.LASTCHILD)
			{
				try
				{
					LocalizerTreeNode res = node.GetTypedParentNode(NodeType.RESOURCE);
					if (res == null || res.Name != AllStrings.dialog) return;

					string[] modules = CommonFunctions.GetModules(node);
					
					uint id = CommonFunctions.GetID((DictionaryTreeNode) node);
					
					DemoDialog d = new DemoDialog(modules, id, null);
					LocalizerDocument output;
					string dictionaryPath = translate 
												? CommonFunctions.GetCultureFolder((DictionaryTreeNode) node)
												: null;
					string culture = (dictionaryPath == null)
														? null
														: CommonFunctions.GetCulture(dictionaryPath);

					if (d.Check(ratio, dictionaryPath, out output)) 
					{
						if (doc!=null && output!=null) 
							AddStringsToXml(output, doc, (DictionaryTreeNode) node, culture);
					}
					else
					{
						LocalizerDocument docOutput = new LocalizerDocument();
						XmlElement root = docOutput.CreateElement(AllStrings.stringTag);
						docOutput.AppendChild(root);
						XmlElement message = docOutput.CreateElement(AllStrings.error);
						root.AppendChild(message);
						message.AppendChild(docOutput.CreateTextNode("Error creating dialog"));
						if (doc!=null) 
							AddStringsToXml(docOutput, doc, (DictionaryTreeNode) node, culture);
					}
				}
				catch (Exception ex)
				{
					Debug.Fail(ex.Message);
				}
			}
			else
			{
				foreach (LocalizerTreeNode child in node.Nodes)
				{
					Check(ratio, child, doc, translate);
				}
			}
		}

		//---------------------------------------------------------------------
		private static void AddStringsToXml(LocalizerDocument strings, LocalizerDocument doc, DictionaryTreeNode node, string culture)
		{
			if (doc == null || doc.DocumentElement == null ) return;
			if (strings == null || strings.DocumentElement == null || !strings.DocumentElement.HasChildNodes) return;

			string fileName = node.FileSystemPath;
			
			XmlNode root = doc.DocumentElement;
							
			XmlNode application = GetChild(root, AllStrings.applicationCap, CommonFunctions.GetApplicationName(node));
			XmlNode module = GetChild(application, AllStrings.moduleCap, CommonFunctions.GetModuleName(node));
			XmlNode file = GetChild(module, AllStrings.fileCap, Path.GetFileNameWithoutExtension(fileName));
			XmlNode resource = GetChild(file, AllStrings.resourceCap, node.Name);
			
			if (culture == null) culture = "Default";

			XmlNode stringsNode = GetChild(resource, AllStrings.cultureCap, culture);
			foreach (XmlNode child in strings.DocumentElement.ChildNodes)
			{
				stringsNode.AppendChild(doc.ImportNode(child, true));
			}
			
		}

		//---------------------------------------------------------------------
		private static XmlNode GetChild(XmlNode node, string tag, string name)
		{
			XmlNode child = node.SelectSingleNode(tag + CommonFunctions.XPathWhereClause(AllStrings.name, name));
			if (child == null)
			{
				XmlElement el = node.OwnerDocument.CreateElement(tag);
				el.SetAttribute(AllStrings.name, name);
				child = node.AppendChild(el);						
			}

			return child;
		}
	}

}
