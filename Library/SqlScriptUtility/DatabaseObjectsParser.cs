using System.Collections;
using System.Xml;

namespace Microarea.Library.SqlScriptUtility
{
	public class DatabaseObjectsParser
	{
		private string libraryName = string.Empty;
		private string databaseObjectsPath = string.Empty;
		private int releaseNumber = -1;
		private int createStep = 1;

		public XmlDocument xDatabaseObjects = null;

		public delegate void WriteXml(string name, XmlDocument document);
		public event WriteXml OnWriteXml;

		public delegate XmlDocument ReadXml(string name, bool getLatest);
		public event ReadXml OnReadXml;

		public DatabaseObjectsParser(string aPath, string aLibraryName)
		{
			databaseObjectsPath = aPath;
			libraryName = aLibraryName;
		}

		public int UpgradeDbVersion(bool newTable, string tableName)
		{
			//TODO: Fabio aggiunta della tabella nel file
			if (xDatabaseObjects != null && !newTable)
				return releaseNumber;

			if (xDatabaseObjects == null)
			{
				if (OnReadXml != null)
					xDatabaseObjects = OnReadXml(databaseObjectsPath, true);
				else
				{
					xDatabaseObjects = new XmlDocument();
					//TODO: Fabio inserire controllo di esistenza del file (per il momento supponiamo che esista)
					xDatabaseObjects.Load(databaseObjectsPath);
				}

				XmlNode nRelease = xDatabaseObjects.SelectSingleNode("DatabaseObjects/Release");
				releaseNumber = int.Parse(nRelease.InnerText);
				releaseNumber++;
				nRelease.InnerText = releaseNumber.ToString();
			}

			if (newTable)
				CreateTable(tableName);

			return releaseNumber;
		}

		private void CreateTable(string tableName)
		{
			XmlNode nMain = xDatabaseObjects.SelectSingleNode("DatabaseObjects/Tables");
			XmlNode nTable = xDatabaseObjects.CreateNode(XmlNodeType.Element, "Table", string.Empty);
			XmlNode nCreate = xDatabaseObjects.CreateNode(XmlNodeType.Element, "Create", string.Empty);
			XmlAttribute aNs = xDatabaseObjects.CreateAttribute(string.Empty, "namespace", string.Empty);
			XmlAttribute aRelease = xDatabaseObjects.CreateAttribute(string.Empty, "release", string.Empty);
			XmlAttribute aCreateStep = xDatabaseObjects.CreateAttribute(string.Empty, "createstep", string.Empty);
			
			//aNs.Value = GetTableNamespace(tableName);
			aRelease.Value = releaseNumber.ToString();
			aCreateStep.Value = createStep.ToString();
			createStep++;

			nTable.Attributes.Append(aNs);
			nCreate.Attributes.Append(aRelease);
			nCreate.Attributes.Append(aCreateStep);

			nTable.AppendChild(nCreate);
			nMain.AppendChild(nTable);
		}

		private string GetModuleName()
		{
			int maxIdx = databaseObjectsPath.Split('\\').Length;
			
			for (int idx = maxIdx - 1; idx > 0; idx --)
			{
				if (idx == maxIdx - 3)
					return databaseObjectsPath.Split('\\')[idx];
			}
			return string.Empty;
		}

		private string GetApplicationName()
		{
			int maxIdx = databaseObjectsPath.Split('\\').Length;
			
			for (int idx = maxIdx - 1; idx > 0; idx --)
			{
				if (idx == maxIdx - 4)
					return databaseObjectsPath.Split('\\')[idx];
			}
			return string.Empty;
		}

		private string GetTableNamespace(string tableName)
		{
			return GetApplicationName() + "." + GetModuleName() + "." + libraryName + "." + tableName;
		}

		public bool Save()
		{
			if (xDatabaseObjects == null)
				return true;

			if (OnWriteXml != null)
			{
				OnWriteXml(databaseObjectsPath, xDatabaseObjects);
				return true;
			}

			try
			{
				xDatabaseObjects.Save(databaseObjectsPath);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	public class DatabaseObjectsManager
	{
		public Hashtable databaseObjectsParserList = new Hashtable();
		public delegate XmlDocument ReadXml(string name, bool getLatest);
		public event ReadXml OnReadXml;
		public delegate void WriteXml(string fileName, XmlDocument document);
		public event WriteXml OnWriteXml;

		public DatabaseObjectsManager()
		{
		}

		public void AddModule(string modName, string dbObjectsPath, string aLibraryName)
		{
			if (!databaseObjectsParserList.ContainsKey(modName))
			{
				DatabaseObjectsParser doParser = new DatabaseObjectsParser(dbObjectsPath, aLibraryName);
				doParser.OnWriteXml += new Microarea.Library.SqlScriptUtility.DatabaseObjectsParser.WriteXml(OnParserWriteXml);
				doParser.OnReadXml += new Microarea.Library.SqlScriptUtility.DatabaseObjectsParser.ReadXml(OnParserReadXml);
				databaseObjectsParserList.Add(modName, doParser);

			}
		}

		public int UpgradeDbVersion(string modName, bool newTable, string tableName)
		{
			DatabaseObjectsParser dop = (DatabaseObjectsParser) databaseObjectsParserList[modName];

			if (dop == null)
				return -1;

			return dop.UpgradeDbVersion(newTable, tableName);
		}

		public int UpgradeDbModuleVersion(string modName)
		{
			DatabaseObjectsParser dop = (DatabaseObjectsParser) databaseObjectsParserList[modName];

			if (dop == null)
				return -1;

			return dop.UpgradeDbVersion(false, string.Empty);
		}

		public bool Save()
		{
			bool bOk = true;
			foreach (DatabaseObjectsParser dop in databaseObjectsParserList.Values)
			{
				bOk = bOk && dop.Save();
			}

			return bOk;
		}

		private void OnParserWriteXml(string name, XmlDocument document)
		{
			if (OnWriteXml != null)
				OnWriteXml(name, document);
		}

		private XmlDocument OnParserReadXml(string name, bool getLatest)
		{
			if (OnReadXml != null)
				return OnReadXml(name, getLatest);

			return null;
		}
	}
}
