using System.IO;
using System.Xml;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.Framework.TBApplicationWrapper;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for DatabaseObjectsWriter.
	/// </summary>
	//================================================================================
	public class DatabaseObjectsWriter
	{
		private string mFilename = string.Empty;
		private string mModule = string.Empty;
		XmlDocument document = null;

		//-----------------------------------------------------------------------------
		/// <remarks/>
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
						
						xmlElement = document.CreateElement("DatabaseObjects");
						xmlNode = document.AppendChild(xmlElement);

						xmlElement = document.CreateElement("Signature");
						xmlElement.InnerText = mModule;
						xmlNode.AppendChild(xmlElement);

						xmlElement = document.CreateElement("Release");
						xmlElement.InnerText = "1";
						xmlNode.AppendChild(xmlElement);

						xmlElement = document.CreateElement("Tables");
						xmlNode.AppendChild(xmlElement);

						xmlElement = document.CreateElement("Views");
						xmlNode.AppendChild(xmlElement);
					}
				}
				return document;
			}

		}
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public DatabaseObjectsWriter(string filename, string module)
		{
			mFilename = filename;
			mModule = module;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string GetFilename()
		{
			return mFilename;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void SetReleaseNumber(int aReleaseNumber)
		{
			XmlNode xmlNode = Document.SelectSingleNode("DatabaseObjects/Release");
			xmlNode.InnerText = aReleaseNumber.ToString();
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void AddTable(NameSpace tableNamespace, IRecord record, int createStep)
		{
			XmlNode xmlNode = null;
			XmlElement xmlElement = null;

			string nsString = tableNamespace.GetNameSpaceWithoutType();
			if (AlreadyExists(Document, nsString))
				return;

			xmlNode = Document.SelectSingleNode("DatabaseObjects/Tables");

			if (xmlNode == null)
			{
				xmlNode = Document.SelectSingleNode("DatabaseObjects");
				xmlElement = Document.CreateElement("Tables");
				xmlNode = Document.AppendChild(xmlElement);
			}

			xmlElement = Document.CreateElement("Table");
			xmlElement.SetAttribute("namespace", nsString);
            xmlElement.SetAttribute("dynamic", "true");
            AddedRecord sqlRecord = record as AddedRecord;
            if (sqlRecord != null && sqlRecord.IsMasterTable)
                xmlElement.SetAttribute(Microarea.TaskBuilderNet.Interfaces.DataBaseObjectsXML.Attribute.Mastertable, "true");
            xmlNode = xmlNode.AppendChild(xmlElement);

			xmlElement = Document.CreateElement("Create");
			xmlElement.SetAttribute("release", record.CreationRelease.ToString());
			xmlElement.SetAttribute("createstep", createStep.ToString());
			xmlNode.AppendChild(xmlElement);

			xmlElement = xmlNode.OwnerDocument.CreateElement("Columns");
			xmlNode = xmlNode.AppendChild(xmlElement);
			foreach (IRecordField field in record.Fields)
				SerializeField(xmlNode, field);

		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public static XmlElement SerializeField(XmlNode xmlNode, IRecordField field)
		{
			XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("Column");
			xmlElement.SetAttribute("name", field.Name);
			xmlElement.SetAttribute("localize", field.Name);
			if (field.Length > 0)
				xmlElement.SetAttribute("lenght", field.Length.ToString());
			if (field.DataObjType.IsEnum)
			{
				xmlElement.SetAttribute("type", DataType.DataTypeStrings.Enum);
				xmlElement.SetAttribute("basetype", field.DataObjType.Tag.ToString());
			}
			else
			{
				xmlElement.SetAttribute("type", field.DataObjType.ToString());
			}
			xmlElement.SetAttribute("defaultvalue", field.DefaultValue);
			xmlElement.SetAttribute("release", field.CreationRelease.ToString());
			xmlNode.AppendChild(xmlElement);
			return xmlElement;
		}



		//--------------------------------------------------------------------------------
		private bool AlreadyExists(XmlDocument xmlDoc, string tableNamespace)
		{
			XmlNodeList xmlList = xmlDoc.SelectNodes("DatabaseObjects/Tables/Table");

			for (int i = 0; i < xmlList.Count; i++)
			{
				if (xmlList[i].Attributes["namespace"].Value == tableNamespace)
					return true;
			}
			return false;
		}

		//--------------------------------------------------------------------------------
		internal void ClearTables()
		{
			XmlNodeList xmlList = Document.SelectNodes("DatabaseObjects/Tables/Table");

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
