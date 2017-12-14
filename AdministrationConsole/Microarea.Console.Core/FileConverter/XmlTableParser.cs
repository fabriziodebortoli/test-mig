using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.FileConverter
{
	
	//================================================================================
	public class XMLTableParser : FileParser
	{

		private XmlDocument xmlDocument;
		private bool		backupFile = true;
		private bool		changeFileName = false;
		
		//--------------------------------------------------------------------------------
		public bool BackupFile { set { backupFile = value; } get { return backupFile; }}
		//--------------------------------------------------------------------------------
		public bool ChangeFileName { set { changeFileName = value; } get { return changeFileName; }}
	
		//---------------------------------------------------------------------------------------------------
		public XMLTableParser(string fileName) : base(fileName)
		{

		}

		//---------------------------------------------------------------------------------------------------
		public XMLTableParser(string fileName, bool changeFileName, bool backupFile ) : base(fileName)
		{
			this.changeFileName = changeFileName;
			this.backupFile = backupFile;
		}

		//--------------------------------------------------------------------------------
		private bool IsDataManagerFile()
		{
			return fileName.ToLower().Replace("/", "\\").IndexOf("\\datamanager\\") != -1;
		}

		//--------------------------------------------------------------------------------
		private bool IsReferenceObjectFile()
		{
			return fileName.ToLower().Replace("/", "\\").IndexOf("\\referenceobjects\\") != -1;
		}

		//--------------------------------------------------------------------------------
		private bool IsScriptDictionaryFile()
		{
			return fileName.ToLower().Replace("/", "\\").IndexOf("dictionary\\en\\databasescript\\scripts.xml") != -1;
		}
		//--------------------------------------------------------------------------------
		private bool IsMigrationFolder()
		{
			return fileName.ToLower().Replace("/", "\\").IndexOf("\\migration\\") != -1;
		}

		//---------------------------------------------------------------------------------------------------
		public override bool Parse()
		{
			try
			{
				if (IsDataManagerFile())
					ParseDataManagerFile();
				else if (IsReferenceObjectFile())
					ParseReferenceObjects();
				else if (IsScriptDictionaryFile())
					ParseScriptDictionaryFile();
				else
				{
					string file = Path.GetFileNameWithoutExtension(fileName).ToLower();
					switch (file)
					{
						case "databaseobjects"		: ParseDatabaseObjects(); break;
						case "addondatabaseobjects"	: ParseDatabaseObjects(); break;
						case "dbts"					: ParseDbtsObjects(); break;
						case "actions"				: ParseActionsOrRules(); break;
						case "codingrules"			: ParseActionsOrRules(); break;
						case "externalreferences"	: ParseExternalReferences(); break;
						case "field"				: ParseField(); break;
						case "userexportcriteria"	: ParseUserExportCriteria(); break;
						case "migrationinfo"		: if (!IsMigrationFolder()) ParseMigrationInfo(); break;
						case "securitymigrationinfo": ParseSecurityMigrationInfo(); break;
					}
				}
			}
			catch (Exception ex)
			{
				GlobalContext.LogManager.Message(ex.Message, ex.Source, DiagnosticType.Error, null);
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------------------------------------
		private bool LoadDocument()
		{
			xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);
			return true;			
		}
		
		//---------------------------------------------------------------------------------------------------
		private void SaveDocument()
		{
			if (!modified) return;

			TryToCheckOut();

			xmlDocument.Save(destinationFileName);

			if (ChangeFileName)
				SafeRemoveFile(BackupFile);
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDatabaseObjects()
		{
			if (!LoadDocument()) return;

			XmlNodeList nodes = xmlDocument.SelectNodes("//Table[@namespace]");
			foreach (XmlElement node in nodes)
			{
				TranslateTableAttribute(node, "namespace", true);
			}

			nodes = xmlDocument.SelectNodes("//View[@namespace]");
			foreach (XmlElement node in nodes)
			{
				TranslateTableAttribute(node, "namespace", true);
			}

			SaveDocument();			
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDbtsObjects()
		{
			if (!LoadDocument()) return;

			XmlNodeList nodes = xmlDocument.SelectNodes("//node()[Table]/UniversalKeys/UniversalKey");
			foreach (XmlElement node in nodes)
			{
				string table = node.SelectSingleNode("../../Table").FirstChild.Value;
				foreach (XmlElement seg in node.SelectNodes("Segment"))
				{
					TranslateColumnAttribute(seg, table, "name");
				}
			}

			nodes = xmlDocument.SelectNodes("//node()[Table]/FixedKeys");
			foreach (XmlElement node in nodes)
			{
				string table = node.SelectSingleNode("../Table").FirstChild.Value;
				foreach (XmlElement seg in node.SelectNodes("Segment"))
				{
					TranslateColumnAttribute(seg, table, "name");
				}
			}

			nodes = xmlDocument.SelectNodes("//Table[@namespace]");
			foreach (XmlElement node in nodes)
			{
				TranslateTableText(node);
				string aNamespace = node.GetAttribute("namespace");
				string newNamespace = aNamespace.Substring(0, aNamespace.LastIndexOf(".") + 1) + node.FirstChild.Value;
				if (aNamespace != newNamespace)
				{
					//					GlobalContext.LogManager.Message(
					//						string.Format (
					//						"Sostituzione testo dell'attributo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
					//						aNamespace, 
					//						newNamespace), 
					//						string.Empty, DiagnosticType.Information, null);

					node.SetAttribute("namespace", newNamespace);
					modified = true;
				}
			}

			SaveDocument();	
		}

		
		//---------------------------------------------------------------------------------------------------
		private void ParseActionsOrRules()
		{
			if (!LoadDocument()) return;

			XmlNodeList tables = xmlDocument.SelectNodes("//Table[@name]");
			foreach (XmlElement table in tables)
			{
				string tableName = table.GetAttribute("name");
				TranslateTableAttribute(table, "name");
				XmlNodeList fields = table.SelectNodes("Field[@name]");
				foreach (XmlElement field in fields)
				{
					TranslateColumnAttribute(field, tableName, "name");
				}
			}

			SaveDocument();	
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDataManagerFile()
		{
			if (!LoadDocument()) return;

			bool firstTable = true;

			XmlNodeList tables = xmlDocument.SelectNodes("DataTables/*");
			
			ArrayList tableList = new ArrayList();
		
			foreach (XmlElement table in tables)
				tableList.Add(table);

			foreach (XmlElement table in tableList)
			{	
				string tableName = table.Name;
				if (ChangeFileName && firstTable)
				{
					firstTable = false;
					destinationFileName = Path.Combine(Path.GetDirectoryName(fileName), TranslateTable(tableName, true) + ".xml");
				}

				XmlNode n = RenameNodeName(table, TranslateTable(tableName, true));
				
				XmlNodeList fields = n.SelectNodes("@*");
				ArrayList fieldList = new ArrayList();
	
				foreach (XmlAttribute field in fields)
					fieldList.Add(field);

				foreach (XmlAttribute field in fieldList)
					RenameAttributeName(field, TranslateColumn(tableName, field.Name, true));
			}

			SaveDocument();	
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseExternalReferences()
		{
			
			if (!LoadDocument()) return;		
			
			XmlNodeList dbts = xmlDocument.SelectNodes("MainExternalReferences/DBT[@namespace]");
			foreach (XmlElement dbt in dbts)
			{
				XmlNodeList extRefs = dbt.SelectNodes("ExternalReferences/ExternalReference");
				if (extRefs.Count == 0)
					continue;

				string dbtNamespace = dbt.GetAttribute("namespace");
				string foreignTableName = GetDbtMasterTableName("Dbt." + dbtNamespace);
				if (foreignTableName == null || foreignTableName.Length == 0)
					GlobalContext.LogManager.Message(string.Format(FileConverterStrings.ExternalReferencesNotFound, dbtNamespace), DiagnosticType.Warning); 
				
				foreach (XmlElement extRef in extRefs)
				{
					string extRefNS = extRef.GetAttribute("namespace");
					if (extRefNS == null || extRefNS == string.Empty)
						extRefNS = GetStandardExtRefNamespace(extRef);
					
					string primaryTableName = null;
					if (extRefNS == null || extRefNS.Length == 0)
					{
						GlobalContext.LogManager.Message(FileConverterStrings.ExternalReferencesUnrecovered, DiagnosticType.Warning);
						GlobalContext.LogManager.Message(extRef.InnerXml, DiagnosticType.Warning);
					}
					else
						primaryTableName = GetDbtMasterTableName("Document." + extRefNS);
					
					XmlNodeList keys = extRef.SelectNodes("Keys/KeySegment");
					foreach (XmlElement key in keys)	
					{
						XmlNode seg = key.SelectSingleNode("Foreign");
						TranslateColumnText(seg, foreignTableName);
						seg = key.SelectSingleNode("Primary");
						TranslateColumnText(seg, primaryTableName);
					}

					XmlNode exp = extRef.SelectSingleNode("Expression");
					if (exp != null)
						exp.InnerText = TranslateRawLine(exp.InnerText);

				}				
			}

			SaveDocument();	
		}

		//--------------------------------------------------------------------------------
		string GetStandardExtRefNamespace(XmlElement extRef)
		{
			try
			{
				string name = extRef.SelectSingleNode("Name").InnerText;
				
				string tmpPath = Path.GetDirectoryName(fileName);
				while (tmpPath.Length > 0 && !Directory.Exists(Path.Combine(tmpPath, "Description")))
					tmpPath = Path.GetDirectoryName(tmpPath);

				tmpPath = Path.Combine(tmpPath, "Description\\ExternalREferences.xml");
				XmlDocument aDoc = new XmlDocument();
				aDoc.Load(tmpPath);

				XmlElement stdExtRef = aDoc.SelectSingleNode("//ExternalReferences/ExternalReference[Name='" + name + "']") as XmlElement;
				if (stdExtRef == null)
					return null;
				return stdExtRef.GetAttribute("namespace");
			}
			catch(Exception ex)
			{
				GlobalContext.LogManager.Message(ex.Message, ex.Source, DiagnosticType.Error, null);
				return null;
			}
					
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseField()
		{
			if (!LoadDocument()) return;		
			
			XmlNodeList dbts = xmlDocument.SelectNodes("DBTS/DBT[@namespace]");
			foreach (XmlElement dbt in dbts)
			{
				XmlNodeList fields = dbt.SelectNodes("Fields/Field");
				if (fields.Count == 0)
					continue;

				string foreignTableName = GetDbtTableName(new NameSpace("Dbt." + dbt.GetAttribute("namespace")));
				
				foreach (XmlElement field in fields)
				{
					TranslateColumnAttribute(field, foreignTableName, "name");
				}				
			}

			SaveDocument();	
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseUserExportCriteria()
		{	
			if (!LoadDocument()) return;		
			
			XmlNodeList criteria = xmlDocument.SelectNodes("UserCriteria/String");
			foreach (XmlElement criterium in criteria)
			{
				string text = criterium.InnerText;
				
				Parser parser = new Parser(Parser.SourceType.FromString);
				parser.Open(text);
				while (!parser.Parsed(Token.TABLE) && !parser.Eof)
					parser.SkipToken();
				
				string name;
				parser.ParseID(out name);
				parser.Close();
				string newName = TranslateTable(name, true);
				if (newName != name && newName != string.Empty)
				{
					//					GlobalContext.LogManager.Message(
					//						string.Format (
					//						"Sostituzione testo del nodo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
					//						name, 
					//						newName), 
					//						string.Empty, DiagnosticType.Information, null);
					
					criterium.InnerText = text.Replace(string.Format("Table {0}", name), string.Format("Table {0}", newName));
					modified = true;
				}				
			}

			SaveDocument();	
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseMigrationInfo()
		{	
			if (!LoadDocument()) return;		
			
			XmlNodeList destinationTables = xmlDocument.SelectNodes("//DestinationTable");
			foreach (XmlElement table in destinationTables)
			{
				string tableName = table.GetAttribute("name");
				TranslateTableAttribute(table, "name");
				XmlNodeList columns = table.SelectNodes("Column");
				foreach (XmlElement column in columns)
					TranslateColumnAttribute(column, tableName, "newname");
			}

			SaveDocument();	
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseSecurityMigrationInfo()
		{	
			if (!LoadDocument()) return;		
			
			XmlNodeList tables = xmlDocument.SelectNodes("//Object[@objectType='Table']");
			foreach (XmlElement table in tables)
				TranslateTableAttribute(table, "namespace", true);

			SaveDocument();	
		}
		
		//---------------------------------------------------------------------------------------------------
		private string TranslateRawLine(string text)
		{
			string retVal = text.Clone() as string;
			Parser parser = new Parser(Parser.SourceType.FromString);
			parser.Open(text);
			while (!parser.Eof)
			{
				if (parser.LookAhead(Token.ID))
				{
					string name;
					parser.ParseID(out name);
					string newName = TranslateQualifiedColumn(name);
					if (newName != name && newName != string.Empty)
					{
						//						GlobalContext.LogManager.Message(
						//							string.Format (
						//							"Sostituzione testo del nodo Expression: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
						//							name, 
						//							newName), 
						//							string.Empty, DiagnosticType.Information, null);
					
						retVal = retVal.Replace(name, newName);
						modified = true;
					}
				}
				else
					parser.SkipToken();
				
			}
			return retVal;
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseReferenceObjects()
		{	
			if (!LoadDocument()) return;		
			
			XmlNodeList hkls = xmlDocument.SelectNodes("HotKeyLink");
			foreach (XmlElement hkl in hkls)
			{
				XmlNode dbField = hkl.SelectSingleNode("DbField");
				if (dbField != null)
					TranslateQualifiedColumnAttribute(dbField as XmlElement, "name");
				
				XmlNodeList columns = hkl.SelectNodes("ComboBox/Column");
				foreach (XmlElement column in columns)
				{
					TranslateQualifiedColumnAttribute(column, "source");
	
					string when = column.GetAttribute("when");
					if (when != string.Empty)
					{
						Parser parser = new Parser(Parser.SourceType.FromString);
						parser.Open(when);
						ArrayList oldColums = new ArrayList();
						ArrayList newColums = new ArrayList();
						while (!parser.Eof)
						{
							if (parser.LookAhead(Token.ID))
							{
								string id;
								parser.ParseID(out id);
								
								string newId = TranslateQualifiedColumn(id);
								if (newId != id && newId != string.Empty)
								{
									oldColums.Add(id);
									newColums.Add(newId);
								}
							}
							else
								parser.SkipToken();
						}
						parser.Close();

						for (int i = 0; i < oldColums.Count; i++)
						{
							when = when.Replace(oldColums[i] as string, newColums[i] as string);
							modified = true;
						}

						if (modified)
							column.SetAttribute("when", when);
					}
				}
					
			}

			SaveDocument();	
		}
		
		//---------------------------------------------------------------------------------------------------
		private void ParseScriptDictionaryFile()
		{	
			if (!LoadDocument()) return;		
			
			XmlNodeList tables = xmlDocument.SelectNodes("scripts/script");
			foreach (XmlElement table in tables)
			{
				string tableName = table.GetAttribute("name");

				TranslateTableAttribute(table, "name");
				foreach (XmlElement stringNode in table.SelectNodes("string"))
				{
					string baseAttr = stringNode.GetAttribute("base");
					string targetAttr;
					if (string.Compare(baseAttr, tableName, true) == 0)
						targetAttr = TranslateTable(baseAttr, true);
					else
						targetAttr = TranslateColumn(tableName, baseAttr, false);

					stringNode.SetAttribute("target", targetAttr);
					modified = true;
					
				}
			}

			SaveDocument();	
		}

		//---------------------------------------------------------------------------------------------------
		private string GetDbtTableName(INameSpace aNS)
		{
			try
			{
				string DbtPath = GetStandardDocumentDescriptionPath(aNS);
				DbtPath = Path.Combine(DbtPath, "Dbts.xml");
	            
				XmlDocument aDoc = new XmlDocument();
				aDoc.Load(DbtPath);
				string ns = aNS.ToString().Substring(4);//tolgo "DBT."
				XmlNode aNode = aDoc.SelectSingleNode("//Table[../@namespace='" + ns + "']");
				if (aNode == null)
					return null;

				return aNode.FirstChild.Value;
			}
			catch(Exception ex)
			{
				GlobalContext.LogManager.Message(ex.Message, ex.Source, DiagnosticType.Error, null);
				return null;
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void TranslateTableAttribute(XmlElement aNode, string attributeName)
		{
			TranslateTableAttribute(aNode, attributeName, false);
		}

		//---------------------------------------------------------------------------------------------------
		private void TranslateTableAttribute(XmlElement aNode, string attributeName, bool isNamespace)
		{
			string name = aNode.GetAttribute(attributeName);
			if (name != string.Empty)
			{
				string oldName = name;
				string prefix = string.Empty;
				if (isNamespace)
				{
					int index = oldName.LastIndexOf('.');
					if (index != -1)
					{
						prefix = oldName.Substring(0, index + 1);
						oldName = oldName.Substring(index + 1);
					}
				}
				string newName = prefix + TranslateTable(oldName, true);
				if (newName != name && newName != string.Empty)
				{
					//					GlobalContext.LogManager.Message(
					//						string.Format (
					//						"Sostituzione testo dell'attributo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
					//						name, 
					//						newName), 
					//						string.Empty, DiagnosticType.Information, null);

					aNode.SetAttribute(attributeName, newName);
					modified = true;			
				}
			}		
		}

		//---------------------------------------------------------------------------------------------------
		private XmlNode RenameNodeName(XmlNode node, string newName)
		{
			if (node.Name == newName || newName == string.Empty) return node;

			XmlElement el = node.OwnerDocument.CreateElement(newName);
			node.ParentNode.AppendChild(el);
			foreach (XmlNode n in node.ChildNodes)
				el.AppendChild(n);
			foreach (XmlAttribute a in node.Attributes)
				el.SetAttribute(a.Name, a.Value);
			node.ParentNode.RemoveChild(node);

			//			GlobalContext.LogManager.Message(
			//				string.Format (
			//				"Sostituzione nome del nodo \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
			//				node.Name, 
			//				newName), 
			//				string.Empty, DiagnosticType.Information, null);

			modified = true;		

			return el;
		}

		//---------------------------------------------------------------------------------------------------
		private void RenameAttributeName(XmlAttribute attr, string newName)
		{
			if (attr.Name == newName || newName == string.Empty) return;
			
			XmlElement n = attr.OwnerElement;
			n.RemoveAttribute(attr.Name);
			n.SetAttribute(newName, attr.Value);

			//			GlobalContext.LogManager.Message(
			//				string.Format (
			//				"Sostituzione nome dell'attributo \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
			//				attr.Name, 
			//				newName), 
			//				string.Empty, DiagnosticType.Information, null);

			modified = true;	

		}

		//---------------------------------------------------------------------------------------------------
		private void TranslateColumnAttribute(XmlElement aNode, string tableName, string attributeName)
		{
			string name = aNode.GetAttribute(attributeName);
			if (name != string.Empty)
			{
				string newName = TranslateColumn(tableName, name, true);
				if (newName != name && newName != string.Empty)
				{
					//					GlobalContext.LogManager.Message(
					//						string.Format (
					//						"Sostituzione testo dell'attributo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
					//						name, 
					//						newName), 
					//						string.Empty, DiagnosticType.Information, null);

					aNode.SetAttribute(attributeName, newName);
					modified = true;			
				}
			}		
		}

		
		//---------------------------------------------------------------------------------------------------
		private void TranslateQualifiedColumnAttribute(XmlElement aNode, string attributeName)
		{
			string name = aNode.GetAttribute(attributeName);
			if (name != string.Empty)
			{
				string newName = TranslateQualifiedColumn(name);
				if (newName != name && newName != string.Empty)
				{
					//					GlobalContext.LogManager.Message(
					//						string.Format (
					//						"Sostituzione testo dell'attributo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
					//						name, 
					//						newName), 
					//						string.Empty, DiagnosticType.Information, null);

					aNode.SetAttribute(attributeName, newName);
					modified = true;			
				}
			}		
		}

		//---------------------------------------------------------------------------------------------------
		private void TranslateColumnText(XmlNode aNode, string tableName)
		{
			foreach (XmlNode child in aNode.ChildNodes)
			{
				if (child.NodeType != XmlNodeType.Text) continue;

				string name = child.Value;
				if (name != string.Empty)
				{
					string newName = TranslateColumn(tableName, name, true);
					if (newName != name && newName != string.Empty)
					{
						//						GlobalContext.LogManager.Message
						//							(
						//							string.Format (
						//							"Sostituzione testo del nodo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
						//							name, 
						//							newName), 
						//							string.Empty,
						//							DiagnosticType.Information,
						//							null
						//							);

						child.Value = newName;
						modified = true;
						return;
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void TranslateTableText(XmlElement aNode)
		{
			foreach (XmlNode child in aNode.ChildNodes)
			{
				if (child.NodeType != XmlNodeType.Text) continue;

				string name = child.Value;
				if (name != string.Empty)
				{
					string newName = TranslateTable(name, true);
					if (newName != name && newName != string.Empty)
					{
						//						GlobalContext.LogManager.Message(
						//							string.Format (
						//							"Sostituzione testo del nodo: \r\n'{0}' \r\ndiventa\r\n'{1}'\r\n", 
						//							name, 
						//							newName), 
						//							string.Empty, DiagnosticType.Information, null);

						child.Value = newName;
						modified = true;
						return;
					}
				}
			}
		}
	}
}
