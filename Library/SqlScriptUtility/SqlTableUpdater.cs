using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Microarea.Library.SqlScriptUtility
{
	/// <summary>
	/// Summary description for SqlTableUpgrader.
	/// </summary>
	public class SqlTableUpdater : SqlParser
	{
		private string tableName = string.Empty;
		private string oracleTableName = string.Empty;
		private TableColumnList newColumns = new TableColumnList();
		private TableColumnList modifiedColumns = new TableColumnList();
		private bool isNewTable = false;

		public override event WriteTextEventHandler OnWriteText;

		//-----------------------------------------------------------------------------
		public string TableName { get { return tableName; } }
		//-----------------------------------------------------------------------------
		public bool IsNewTable { get { return isNewTable; } }

		//-----------------------------------------------------------------------------
		public SqlTableUpdater(SqlTable aTable)
		{
			tableName = aTable.ExtendedName;
			oracleTableName = aTable.OracleTableName;
		}
		
		//-----------------------------------------------------------------------------
		public void AddNewColumn(TableColumn aColumn)
		{
			newColumns.Add(aColumn);
		}

		//-----------------------------------------------------------------------------
		public void InsertTable(SqlTable aTable)
		{
			tables.Add(aTable);
			isNewTable = true;
		}

		//-----------------------------------------------------------------------------
		public void EditField(TableColumn aColumn)
		{
			if (aColumn.IsToChange())
			{
				if (!modifiedColumns.Contains(aColumn))
					modifiedColumns.Add(aColumn);
			}
			else
			{
				if (modifiedColumns.Contains(aColumn))
					modifiedColumns.Remove(aColumn);
			}
		}

		//-----------------------------------------------------------------------------
		private string MakeExistenceCheck(string nomeColonna)
		{
			string res = string.Empty;
			res +=               "if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where \r\n";
			res += string.Format("    dbo.sysobjects.name = '{0}' and dbo.sysobjects.id = dbo.syscolumns.id \r\n", tableName);
			res += string.Format("    and dbo.syscolumns.name = '{0}')", nomeColonna);
			res +=               "\r\n";
			return res;
		}

		//-----------------------------------------------------------------------------
		private string UpdateStringDefault(TableColumn aColumn)
		{
			string res = "\r\n";

			res += string.Format("UPDATE [dbo].[{0}] ", tableName);
			res += string.Format("SET [dbo].[{0}].[{1}] = {2} ", tableName, aColumn.Name, aColumn.DefaultValue);
			res += string.Format("WHERE [dbo].[{0}].[{1}] IS NULL\r\n", tableName, aColumn.Name);
			res +=               "GO\r\n";

			return res;
		}

		//-----------------------------------------------------------------------------
		private bool SqlFileWrite(string sqlFile, string sqlFileText)
		{
			if (OnWriteText != null)
			{
				OnWriteText(sqlFile, sqlFileText, FileMode.Create);
				return true;
			}

			char[] sqlFileTextArray = sqlFileText.ToCharArray();

			try
			{
				FileStream fw = new FileStream(sqlFile, FileMode.Create, FileAccess.Write);
				for (int i = 1; i<=sqlFileText.Length; i++)
					fw.WriteByte((byte) sqlFileTextArray[i-1]);
				fw.Close();
				return true;
			}
			catch (Exception ex)
			{
				errorsList.Add(ex.Message);
				return false;
			}
		}

		//-----------------------------------------------------------------------------
		public override bool UnParseSql(string scriptName)
		{
			string res = string.Empty;

			if (isNewTable)
			{
				base.UnParseSql(out res);
				return SqlFileWrite(scriptName, res);
			}
			
			//unparso le colonne aggiunte
			foreach (TableColumn aColumn in newColumns)
			{
				res += MakeExistenceCheck(aColumn.Name);
				res += string.Format("ALTER TABLE [dbo].[{0}]\r\n", tableName);
				res += "   ADD " + base.MakeInsertColumnSql(aColumn.Name, aColumn.DataType, aColumn.DataLength, aColumn.IsNullable, aColumn.DefaultValue, aColumn.DefaultConstraintName) + "\r\n";
				res +=               "GO\r\n\r\n";

				if (aColumn.DefaultValue != string.Empty)
					res += UpdateStringDefault(aColumn);
			}

			//unparso le colonne modificate
			foreach (TableColumn aColumn in modifiedColumns)
			{
				if (aColumn.IsNameToChange())
					errorsList.Add("Il rename delle colonne non è gestito!");

				if (aColumn.IsLengthToChange())
				{
					res += string.Format("ALTER TABLE [dbo].[{0}]\r\n", tableName);
					res += "   ALTER COLUMN " + base.MakeInsertColumnSql(aColumn.Name, aColumn.DataType, aColumn.DataLength, aColumn.IsNullable, string.Empty, string.Empty) + "\r\n";
					res += "GO\r\n";
				}

				if (aColumn.IsDefaultValueToChange())
				{
					res += string.Format("ALTER TABLE [dbo].[{0}]\r\n", tableName);
					res += string.Format("   DROP CONSTRAINT [{0}]\r\n", aColumn.DefaultConstraintName);
					res += "\r\n";
					res += string.Format("ALTER TABLE [dbo].[{0}]\r\n", tableName);
					res += string.Format("   ADD CONSTRAINT [{0}] DEFAULT ({1}) FOR [{2}]\r\n", aColumn.DefaultConstraintName, aColumn.DefaultValue, aColumn.Name);
					res += "GO\r\n";
					res += "\r\n";
				}
			}

			return SqlFileWrite(scriptName, res);
		}

		//-----------------------------------------------------------------------------
		public override bool UnParseOracle(string scriptName)
		{
			string res = string.Empty;

			if (isNewTable)
				return base.UnParseOracle(out res) && SqlFileWrite(scriptName, res);

			//unparso le colonne aggiunte
			foreach (TableColumn aColumn in newColumns)
			{
				res += string.Format("ALTER TABLE \"{0}\"\r\n", oracleTableName);
				res += "ADD " + base.MakeInsertColumnOracle(aColumn.Name, aColumn.DataType, aColumn.DataLength, aColumn.IsNullable, aColumn.DefaultValue, aColumn.DefaultConstraintName) + "\r\n";
				res +=               "GO\r\n";
			}

			//upparso le colonne modificate
			foreach (TableColumn aColumn in modifiedColumns)
			{
				if (aColumn.IsLengthToChange() || aColumn.IsDefaultValueToChange())
				{
					res += string.Format("ALTER TABLE \"{0}\"\r\n", oracleTableName);
					res += "MODIFY (" + base.MakeInsertColumnOracle(aColumn.Name, aColumn.DataType, aColumn.DataLength, aColumn.IsNullable, aColumn.DefaultValue, aColumn.DefaultConstraintName) + ")\r\n";
				}
			}

			return SqlFileWrite(scriptName, res);
		}
	}

	//-----------------------------------------------------------------------------
	public class SqlParserUpdater : SqlParser
	{
		private string upgradeFolder = string.Empty;
		private string sqlScriptFileName = string.Empty;
		private string oracleScriptFileName = string.Empty;
		private string moduleName = string.Empty;
		private string dbSignature = string.Empty;
		private int releaseNumber = -1;

		private Hashtable tableUpdaterList = new Hashtable();

		public delegate int UpgradeDbModuleReleaseEventHandler(string aModuleName);
		public event UpgradeDbModuleReleaseEventHandler OnUpgradeDbModuleRelease;
		public delegate int UpgradeDbReleaseEventHandler(bool newTable, string tableName);
		public event UpgradeDbReleaseEventHandler OnUpgradeDbRelease;

		public delegate void WriteXmlEventHandler(string name, XmlDocument document);
		public event WriteXmlEventHandler OnWriteXml;

		public delegate XmlDocument ReadXmlEventHandler(string name, bool getLatest);
		public event ReadXmlEventHandler OnReadXml;

		//-----------------------------------------------------------------------------
		public string ModuleName { get { return moduleName; } }
		//-----------------------------------------------------------------------------
		public string DBSignature { get { return dbSignature; } set { dbSignature = value; }}
		
		//-----------------------------------------------------------------------------
		public SqlParserUpdater(string aFolder)
		{
			upgradeFolder = aFolder;

			string[] splittedPath = upgradeFolder.Split(Path.DirectorySeparatorChar);
			moduleName = splittedPath[splittedPath.Length - 4];
		}

		//-----------------------------------------------------------------------------
		public SqlParserUpdater(string aFolder, string aSqlFileName, string aOracleFileName)
		{
			upgradeFolder = aFolder;
			
			sqlScriptFileName = aSqlFileName;
			oracleScriptFileName = aOracleFileName;

			string[] splittedPath = upgradeFolder.Split(Path.DirectorySeparatorChar);
			moduleName = splittedPath[splittedPath.Length - 4];
		}

		//-----------------------------------------------------------------------------
		public void EditRow(SqlTable aTable, TableColumn aColumn)
		{
			if (OnUpgradeDbRelease != null)
				releaseNumber = OnUpgradeDbRelease(false, string.Empty);
			else if (OnUpgradeDbModuleRelease != null)
				releaseNumber = OnUpgradeDbModuleRelease(moduleName);

			if (!tableUpdaterList.Contains(aTable.ExtendedName))
			{
				SqlTableUpdater tableUpdater = new SqlTableUpdater(aTable);
				tableUpdaterList.Add(aTable.ExtendedName, tableUpdater);
				tableUpdater.OnWriteText += new Microarea.Library.SqlScriptUtility.SqlTableUpdater.WriteTextEventHandler(OnFileWrite);
			}

			SqlTableUpdater stu =(SqlTableUpdater)tableUpdaterList[aTable.ExtendedName];
			
			if (stu.IsNewTable)
				return;

			stu.EditField(aColumn);
		}

		//-----------------------------------------------------------------------------
		public void CreateRow(SqlTable aTable, TableColumn aColumn)
		{
			if (OnUpgradeDbRelease != null)
				releaseNumber = OnUpgradeDbRelease(false, string.Empty);

			if (OnUpgradeDbModuleRelease != null)
				releaseNumber = OnUpgradeDbModuleRelease(moduleName);

			if (!tableUpdaterList.Contains(aTable.ExtendedName))
			{
				SqlTableUpdater tableUpdater = new SqlTableUpdater(aTable);
				tableUpdaterList.Add(aTable.ExtendedName, tableUpdater);
				tableUpdater.OnWriteText += new Microarea.Library.SqlScriptUtility.SqlTableUpdater.WriteTextEventHandler(OnFileWrite);
			}
			
			SqlTableUpdater stu =(SqlTableUpdater)tableUpdaterList[aTable.ExtendedName];

			if (stu == null || stu.IsNewTable)
				return;

			stu.AddNewColumn(aColumn);
		}

		//-----------------------------------------------------------------------------
		public void CreateTable(SqlTable aTable)
		{
			if (OnUpgradeDbRelease != null)
				releaseNumber = OnUpgradeDbRelease(true, aTable.ExtendedName);

			if (OnUpgradeDbModuleRelease != null)
				releaseNumber = OnUpgradeDbModuleRelease(moduleName);

			if (!tableUpdaterList.Contains(aTable.ExtendedName))
			{
				SqlTableUpdater tableUpdater = new SqlTableUpdater(aTable);
				tableUpdaterList.Add(aTable.ExtendedName, tableUpdater);
				tableUpdater.InsertTable(aTable);
				tableUpdater.OnWriteText += new Microarea.Library.SqlScriptUtility.SqlTableUpdater.WriteTextEventHandler(OnFileWrite);
			}
		}

		//-----------------------------------------------------------------------------
		public override bool ParseWithLexan(string fileName)
		{
			sqlScriptFileName = fileName;
			return base.ParseWithLexan(fileName);
		}

		//-----------------------------------------------------------------------------
		public override bool Parse(string fileName)
		{
			sqlScriptFileName = fileName;
			return base.Parse(fileName);
		}

		//-----------------------------------------------------------------------------
		public override bool ParseOracle(string fileName)
		{
			oracleScriptFileName = fileName;
			return base.ParseOracle(fileName);
		}

		//-----------------------------------------------------------------------------
		public bool UnParseAll()
		{
			if (
				sqlScriptFileName != null &&
				sqlScriptFileName.Length > 0 &&
				oracleScriptFileName != null &&
				oracleScriptFileName.Length > 0 
				)
				return base.UnParseAll(sqlScriptFileName, oracleScriptFileName);

			errorsList.Add(Strings.MissingScriptFileNameErrorMessage);
			return false;
		}

		//-----------------------------------------------------------------------------
		public override bool UnParseSql(string fileName)
		{
			if (tableUpdaterList == null || tableUpdaterList.Count == 0)
				return true;

			bool bOk = true;

			string releaseFolder = string.Format("Release_{0}", releaseNumber.ToString());
			string releaseFolderFullPath = Path.Combine(Path.Combine(upgradeFolder, "All"), releaseFolder);

			if (!Directory.Exists(releaseFolderFullPath))
				Directory.CreateDirectory(releaseFolderFullPath);

			bOk = bOk && SaveUpgradeInfoSql(releaseFolderFullPath);

			if (bOk)
				return bOk && base.UnParseSql(fileName);

			return bOk;
		}

		//-----------------------------------------------------------------------------
		public override bool UnParseOracle(string fileName)
		{
			if (tableUpdaterList == null || tableUpdaterList.Count == 0)
				return true;

			bool bOk = true;

			string releaseFolder = string.Format("Release_{0}", releaseNumber.ToString());
			string releaseFolderFullPath = Path.Combine(Path.Combine(upgradeFolder, "Oracle"), releaseFolder);

			if (!Directory.Exists(releaseFolderFullPath))
				Directory.CreateDirectory(releaseFolderFullPath);

			bOk = bOk && SaveUpgradeInfoOracle(releaseFolderFullPath);

			if (bOk)
				return bOk && base.UnParseOracle(fileName);

			return bOk;
		}

		//-----------------------------------------------------------------------------
		private bool SaveUpgradeInfoSql(string releaseFolderFullPath)
		{
			bool bOk = true;

			XmlDocument xDoc = null;
			string fileName = Path.Combine(upgradeFolder, "UpgradeInfo.xml");

			if (OnReadXml != null)
				xDoc = OnReadXml(fileName, true);
			else
			{
				if (File.Exists(fileName))
				{
					xDoc = new XmlDocument();
					xDoc.Load(fileName);
				}
			}

			if (xDoc == null)
			{
				xDoc = new XmlDocument();
				xDoc.LoadXml(string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><UpgradeInfo><ModuleInfo name=\"{0}\" /></UpgradeInfo>", moduleName));
			}
			
			XmlNode nLevel = CreateReleaseNode(xDoc);

			foreach(SqlTableUpdater stu in tableUpdaterList.Values)
			{
				string scriptName = string.Format("Alter_{0}.sql", stu.TableName);

				int step = GetStepNumber(nLevel);
				XmlNode nStep = xDoc.CreateNode(XmlNodeType.Element, "Step", "");
				XmlAttribute aNumStep = xDoc.CreateAttribute("", "numstep", "");
				aNumStep.Value = step.ToString();
				XmlAttribute aScript = xDoc.CreateAttribute("", "script", "");
				aScript.Value = scriptName;
				
				nStep.Attributes.Append(aNumStep);
				nStep.Attributes.Append(aScript);
				nLevel.AppendChild(nStep);

				bOk = bOk && stu.UnParseSql(Path.Combine(releaseFolderFullPath, scriptName));
			}

			if (OnWriteXml != null)
			{
				OnWriteXml(fileName, xDoc);
				return true;
			}

			if (bOk)
			{
				try
				{
					xDoc.Save(Path.Combine(upgradeFolder, "UpgradeInfo.xml"));
					return true;
				}
				catch (Exception ex)
				{
					errorsList.Add(ex.Message);
					return false;
				}
			}
			
			errorsList.Add(Strings.UnparsingErrorsEncounteredMessage);
			return false;
		}

		//-----------------------------------------------------------------------------
		private bool SaveUpgradeInfoOracle(string releaseFolderFullPath)
		{
			bool bOk = true;

			foreach(SqlTableUpdater stu in tableUpdaterList.Values)
			{
				string scriptName = string.Format("Alter_{0}.sql", stu.TableName);

				bOk = bOk && stu.UnParseOracle(Path.Combine(releaseFolderFullPath, scriptName));
			}

			return bOk;
		}

		//-----------------------------------------------------------------------------
		private XmlNode CreateReleaseNode(XmlDocument xDoc)
		{
			XmlNode nMain = xDoc.SelectSingleNode("UpgradeInfo");
			foreach (XmlNode n in nMain.SelectNodes("DBRel"))
			{
				if (n.Attributes["numrel"].Value == releaseNumber.ToString())
					return n.SelectSingleNode("Level1");
			}

			XmlNode nRel = xDoc.CreateNode(XmlNodeType.Element, "DBRel", "");
			XmlAttribute aNumRel = xDoc.CreateAttribute("", "numrel", "");
			aNumRel.Value = releaseNumber.ToString();

			nRel.Attributes.Append(aNumRel);

			XmlNode nLevel = xDoc.CreateNode(XmlNodeType.Element, "Level1", "");
			nRel.AppendChild(nLevel);

			nMain.AppendChild(nRel);
			
			return nLevel;
		}

		//-----------------------------------------------------------------------------
		private int GetStepNumber(XmlNode nLevel)
		{
			int result = 0;
			foreach (XmlNode n in nLevel.SelectNodes("Step"))
			{
				string stringStep = n.Attributes["numstep"].Value.ToString();
				if (int.Parse(stringStep) > result)
					result = int.Parse(stringStep);
			}
			return ++result;
		}
	}
}