using System;
using System.Xml;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.SqlScriptUtility;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Library.TranslationManager
{
	public class TableParser : SolutionManagerItems
	{
		private SqlParser sqlParser = new SqlParser();
		protected XmlDocument xMigrationDoc = new XmlDocument();
		protected string curModName = string.Empty;

		public TableParser()
		{
			defaultLookUpType = LookUpFileType.Tables;
		}

		public override string ToString()
		{
			return "Table parser";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			if (!OpenLookUpDocument(true))
				xLookUpDoc.LoadXml("<Tables/>");

			nMain = xLookUpDoc.SelectSingleNode("Tables");
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				SetProgressMessage(string.Format("Elaborazione in corso: modulo {0}", mi.Name));

				curModName = mi.Name;
				string sqlCreatePath = mi.GetProviderCreateScriptPath(NameSolverStrings.All);
					
				if (!Directory.Exists(sqlCreatePath))
					continue;

				DirectoryInfo di = new DirectoryInfo(sqlCreatePath);

				FileInfo[] files = di.GetFiles("*.sql");

				SqlParser tmpParser = new SqlParser();

				foreach (FileInfo fi in files)
				{
					sqlParser.Parse(fi.FullName);
					tmpParser.Parse(fi.FullName);
				}

				string migDir = Path.Combine(mi.Path, "Migration");
				string netMigDir = Path.Combine(mi.Path, "Migration_NET");
				string xpMigDir = Path.Combine(mi.Path, "Migration_XP");

				if (Directory.Exists(migDir))
				{
					if (!Directory.Exists(xpMigDir))
						Directory.CreateDirectory(xpMigDir);

					DirectoryInfo migDI = new DirectoryInfo(migDir);
					foreach (FileInfo fi in migDI.GetFiles("MigrationInfo.xml"))
					{
						try
						{
							File.Copy(fi.FullName, Path.Combine(xpMigDir, fi.Name), true);
							File.SetAttributes(Path.Combine(xpMigDir, fi.Name), FileAttributes.Normal);
						}
						catch (Exception ex)
						{
							SetLogError("Errore: " + ex.Message, ToString());
						}
					}
				}

				string migDirDS = Path.Combine(migDir, "DatabaseScript");
				
				if (Directory.Exists(migDirDS))
				{
					string xpMigDirDS = Path.Combine(xpMigDir, "DatabaseScript");
					if (!Directory.Exists(xpMigDirDS))
					{
						Directory.CreateDirectory(xpMigDirDS);
					}

					DirectoryInfo migDIDS = new DirectoryInfo(migDirDS);
					string migDirAll = Path.Combine(migDirDS, "All");
					if (Directory.Exists(migDirAll))
					{
						string xpMigDirAll = Path.Combine(xpMigDirDS, "All");
						if (!Directory.Exists(xpMigDirAll))
						{
							Directory.CreateDirectory(xpMigDirAll);
						}

						DirectoryInfo migDIDSAll = new DirectoryInfo(migDirAll);
						foreach (FileInfo fi in migDIDSAll.GetFiles("*.sql"))
						{
							if (!File.Exists(Path.Combine(xpMigDirAll, fi.Name)))
							{
								File.Copy(fi.FullName, Path.Combine(xpMigDirAll, fi.Name));
								File.SetAttributes(Path.Combine(xpMigDirAll, fi.Name), FileAttributes.Normal);
							}
						}
					}
					
					string migDirOracle = Path.Combine(migDirDS, "Oracle");
					if (Directory.Exists(migDirOracle))
					{
						string xpMigDirOracle = Path.Combine(xpMigDirDS, "Oracle");
						if (!Directory.Exists(xpMigDirOracle))
						{
							Directory.CreateDirectory(xpMigDirOracle);
						}

						DirectoryInfo migDIDSOracle = new DirectoryInfo(migDirOracle);
						foreach (FileInfo fi in migDIDSOracle.GetFiles("*.sql"))
						{
							if (!File.Exists(Path.Combine(xpMigDirOracle, fi.Name)))
							{
								File.Copy(fi.FullName, Path.Combine(xpMigDirOracle, fi.Name));
								File.SetAttributes(Path.Combine(xpMigDirOracle, fi.Name), FileAttributes.Normal);
							}
						}
					}
				}

				if (!Directory.Exists(migDir))
					Directory.CreateDirectory(migDir);

				if (!Directory.Exists(netMigDir))
					Directory.CreateDirectory(netMigDir);

				if (!Directory.Exists(xpMigDir))
					Directory.CreateDirectory(xpMigDir);

				string netMigFile = Path.Combine(netMigDir, "MigrationInfo.xml");
				string xpMigFile = Path.Combine(xpMigDir, "MigrationInfo.xml");

				if (File.Exists(netMigFile))
				{
					xMigrationDoc.Load(netMigFile);
				}
				else
				{
					xMigrationDoc.LoadXml("<Database/>");
				}

				foreach (SqlTable aTable in tmpParser.Tables)
					foreach (TableColumn aColumn in aTable.Columns)
					{
						AddMigrationColumn(aTable.ExtendedName, aColumn.Name, true, false);
						AddColumn(aTable.ExtendedName, aColumn.Name);
					}

				try
				{
					xMigrationDoc.Save(netMigFile);
				}
				catch (Exception ex)
				{
					SetLogError("Errore: " + ex.Message, ToString());
				}
			}

			/*foreach (SqlTable sTable in sqlParser.GetTableList())
				foreach (TableColumn tColumn in sTable.m_Colonne)
					AddColumn(sTable.GetTableName(), tColumn.m_NomeColonna);*/

			EndRun(true);
		}

		private bool ExistsTable(string tableName)
		{
			foreach (XmlNode n in xLookUpDoc.SelectNodes("Tables/Table"))
			{
				if (n.Attributes["oldName"].Value.ToString() == tableName)
					return true;
			}
			return false;
		}

		private XmlNode GetTableNode(string tableName)
		{
			foreach (XmlNode n in xLookUpDoc.SelectNodes("Tables/Table"))
			{
				if (n.Attributes["oldName"].Value.ToString() == tableName)
					return n;
			}
			return null;
		}

		protected XmlNode AddTable(string tableName)
		{
			XmlNode nTable = GetTableNode(tableName);
			if (nTable == null)
			{
				string newName = string.Empty;

				nTable = xLookUpDoc.CreateNode(XmlNodeType.Element, "Table", string.Empty);
				XmlAttribute aOldName = xLookUpDoc.CreateAttribute(string.Empty, "oldName", string.Empty);
				aOldName.Value = tableName;
				XmlAttribute aNewName = xLookUpDoc.CreateAttribute(string.Empty, "newName", string.Empty);

				newName = transManager.AddGlossaryItem(defaultLookUpType, tableName, curModName);

				aNewName.Value = newName;

				nTable.Attributes.Append(aOldName);
				nTable.Attributes.Append(aNewName);
				xLookUpDoc.SelectSingleNode("Tables").AppendChild(nTable);
			}
			
			return nTable;
		}

		private bool ExistsColumn(string tableName, string columnName)
		{
			XmlNode tNode = GetTableNode(tableName);

			if (tNode == null)
				return false;

			foreach (XmlNode n in tNode.SelectNodes("Column"))
			{
				if (n.Attributes["oldName"].Value.ToString() == columnName)
					return true;
			}
			return false;
		}

		private XmlNode GetColumn(string tableName, string columnName)
		{
			XmlNode tNode = GetTableNode(tableName);

			if (tNode == null)
				return null;

			foreach (XmlNode n in tNode.SelectNodes("Column"))
			{
				if (n.Attributes["oldName"].Value.ToString() == columnName)
					return n;
			}
			return null;
		}

		protected void AddColumn(string tableName, string columnName)
		{
			XmlNode nTable = AddTable(tableName);

			XmlNode nColumn = GetColumn(tableName, columnName);

			if (nColumn != null)
				return;

			
			string newName = string.Empty;

			nColumn = xLookUpDoc.CreateNode(XmlNodeType.Element, "Column", string.Empty);
			XmlAttribute aOldName = xLookUpDoc.CreateAttribute(string.Empty, "oldName", string.Empty);
			aOldName.Value = columnName;
			XmlAttribute aNewName = xLookUpDoc.CreateAttribute(string.Empty, "newName", string.Empty);

			newName = transManager.AddGlossaryItem(defaultLookUpType, columnName, curModName);

			aNewName.Value = newName;

			nColumn.Attributes.Append(aOldName);
			nColumn.Attributes.Append(aNewName);
			nTable.AppendChild(nColumn);
		}

		private bool ExistsMigrationTable(string tableName)
		{
			foreach (XmlNode n in xMigrationDoc.SelectNodes("Database/SourceTable"))
			{
				if (n.Attributes["name"].Value.ToString() == tableName)
					return true;
			}
			return false;
		}

		private XmlNode GetMigrationTableNode(string tableName)
		{
			foreach (XmlNode n in xMigrationDoc.SelectNodes("Database/SourceTable"))
			{
				if (n.Attributes["name"].Value.ToString() == tableName)
					return n;
			}
			return null;
		}

		protected XmlNode AddMigrationTable(string tableName, bool isMaster, bool isView)
		{
			XmlNode nTable = GetMigrationTableNode(tableName);
			if (nTable == null)
			{
				nTable = xMigrationDoc.CreateNode(XmlNodeType.Element, "SourceTable", string.Empty);
				XmlAttribute aName = xMigrationDoc.CreateAttribute(string.Empty, "name", string.Empty);
				aName.Value = tableName;
				nTable.Attributes.Append(aName);
				
				if (isView || (tableName.ToLower().IndexOf("_tmp") > 0))
				{
					XmlAttribute aTemp = xMigrationDoc.CreateAttribute(string.Empty, "temp", string.Empty);
					aTemp.Value = bool.TrueString;
					nTable.Attributes.Append(aTemp);
				}

				XmlNode nDestinationTable = xMigrationDoc.CreateNode(XmlNodeType.Element, "DestinationTable", string.Empty);
				aName = xMigrationDoc.CreateAttribute(string.Empty, "name", string.Empty);
				aName.Value = tableName;
				nDestinationTable.Attributes.Append(aName);

				if (isMaster)
				{
					XmlAttribute aMaster = xMigrationDoc.CreateAttribute(string.Empty, "master", string.Empty);
					aMaster.Value = bool.TrueString;
					nDestinationTable.Attributes.Append(aMaster);
				}

				nTable.AppendChild(nDestinationTable);

				xMigrationDoc.SelectSingleNode("Database").AppendChild(nTable);
			}
			return nTable.SelectSingleNode("DestinationTable");
		}

		private bool ExistsMigrationColumn(string tableName, string columnName)
		{
			XmlNode tNode = GetMigrationTableNode(tableName);

			if (tNode == null)
				return false;

			foreach (XmlNode n in tNode.SelectNodes("DestinationTable/Column"))
			{
				if (n.Attributes["oldname"].Value.ToString() == columnName)
					return true;
			}
			return false;
		}

		private XmlNode GetMigrationColumn(string tableName, string columnName)
		{
			XmlNode tNode = GetMigrationTableNode(tableName);

			if (tNode == null)
				return null;

			foreach (XmlNode n in tNode.SelectNodes("DestinationTable/Column"))
			{
				if (n.Attributes["oldname"].Value.ToString() == columnName)
					return n;
			}
			return null;
		}

		protected void AddMigrationColumn(string tableName, string columnName, bool isMaster, bool isView)
		{
			XmlNode nTable = AddMigrationTable(tableName, isMaster, isView);

			XmlNode nColumn = GetMigrationColumn(tableName, columnName);

			if (nColumn != null)
				return;

			nColumn = xMigrationDoc.CreateNode(XmlNodeType.Element, "Column", string.Empty);
			XmlAttribute aOldName = xMigrationDoc.CreateAttribute(string.Empty, "oldname", string.Empty);
			aOldName.Value = columnName;
			XmlAttribute aNewName = xMigrationDoc.CreateAttribute(string.Empty, "newname", string.Empty);
			aNewName.Value = columnName;
			nColumn.Attributes.Append(aOldName);
			nColumn.Attributes.Append(aNewName);
			nTable.AppendChild(nColumn);
		}
	}
}
