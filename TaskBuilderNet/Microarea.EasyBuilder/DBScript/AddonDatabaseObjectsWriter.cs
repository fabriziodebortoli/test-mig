using System.IO;
using System.Xml;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Responsible for creation of addons database objects files.
	/// </summary>
	//================================================================================
	public class AddonDatabaseObjectsWriter
	{
		private string mFilename = string.Empty;
		private string mModule = string.Empty;
		XmlDocument document = null;

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Gets the xml document describing all addon database objects.
		/// </summary>
		public XmlDocument Document
		{
			get
			{
				if (document == null)
				{
					document = new XmlDocument();
					if (File.Exists(mFilename))
						document.Load(mFilename);
					else
					{
						XmlNode xmlNode = null;
						XmlElement xmlElement = null;
						document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
						xmlElement = document.CreateElement("AddOnDatabaseObjects");
						xmlNode = document.AppendChild(xmlElement);

						xmlElement = document.CreateElement("AdditionalColumns");
						xmlNode.AppendChild(xmlElement);
					}
				}
				return document;
			}

		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of AddonDatabaseObjectsWriter.
		/// </summary>
		/// <param name="filename">The name of the file to write to.</param>
		/// <param name="module">The name of the main module</param>
		public AddonDatabaseObjectsWriter(string filename, string module)
		{
			mFilename = filename;
			mModule = module;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Returns the file name.
		/// </summary>
		/// <returns>The file name</returns>
		public string GetFilename()
		{
			return mFilename;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Creates an alter table script.
		/// </summary>
		/// <param name="tableToAlter">Name of the table to alter</param>
		/// <param name="alteringLibrary">Name of the library</param>
		/// <param name="field">Name of the field to add</param>
		/// <param name="createStep">Number of step</param>
		public void AddAlterTable(NameSpace tableToAlter, NameSpace alteringLibrary, IRecordField field, int createStep)
		{
			XmlElement xmlElement = CreateTableIfNeeded(tableToAlter.GetNameSpaceWithoutType());
			xmlElement = CreateAlterTableIfNeeded(xmlElement, alteringLibrary.GetNameSpaceWithoutType(), field.CreationRelease, createStep);//TODOPERASSO parametrizzare

			DatabaseObjectsWriter.SerializeField(xmlElement, field);
		}

		//--------------------------------------------------------------------------------
		private XmlElement CreateAlterTableIfNeeded(XmlElement xmlElement, string alteringLibrary, int releaseNumber, int step)
		{
			
			XmlNodeList alterNodes = xmlElement.SelectNodes("AlterTable");
			foreach (XmlElement el in alterNodes)
			{
				if (el.GetAttribute("namespace") == alteringLibrary &&
					el.GetAttribute("release") == releaseNumber.ToString())
					return (XmlElement)el.SelectSingleNode("Columns");
			}
			XmlElement newEl = Document.CreateElement("AlterTable");
			newEl.SetAttribute("namespace", alteringLibrary);
			newEl.SetAttribute("release", releaseNumber.ToString());
			newEl.SetAttribute("createstep", step.ToString());
			xmlElement.AppendChild(newEl);
			return (XmlElement)newEl.AppendChild(Document.CreateElement("Columns"));
		}

		//--------------------------------------------------------------------------------
		private XmlElement CreateTableIfNeeded(string tableNamespace)
		{
			XmlNodeList xmlList = Document.SelectNodes("AddOnDatabaseObjects/AdditionalColumns/Table");

			for (int i = 0; i < xmlList.Count; i++)
			{
				XmlNode node = xmlList[i];
				if (node.Attributes["namespace"].Value == tableNamespace)
					return (XmlElement)node;
			}
			XmlNode columnsNode = Document.SelectSingleNode("AddOnDatabaseObjects/AdditionalColumns");
			XmlElement el = Document.CreateElement("Table");
			el.SetAttribute("namespace", tableNamespace);
			columnsNode.AppendChild(el);
			return el;
		}

		//--------------------------------------------------------------------------------
		internal void ClearTables()
		{
			XmlNodeList xmlList = Document.SelectNodes("AddOnDatabaseObjects/AdditionalColumns/Table");

			for (int i = xmlList.Count - 1; i >= 0; i--)
			{
				XmlElement node = (XmlElement)xmlList[i];
				node.ParentNode.RemoveChild(node);
			}
		}

		//--------------------------------------------------------------------------------
		internal void Save()
		{
			string path = Path.GetDirectoryName(mFilename);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			Document.Save(mFilename);
			BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(mFilename);
		}
	}
}
