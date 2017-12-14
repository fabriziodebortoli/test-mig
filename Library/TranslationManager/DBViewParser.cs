using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for DBViewParser.
	/// </summary>
	public class DBViewParser : TableParser
	{
		public DBViewParser()
		{
			defaultLookUpType = LookUpFileType.Tables;
		}

		public override string ToString()
		{
			return "DB View parser";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(true);

			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				curModName = mi.Name;
				string dbObjFileName = mi.GetDatabaseObjectsPath();

				if (!File.Exists(dbObjFileName))
					continue;

				SetProgressMessage(string.Format("Elaborazione in corso del modulo: {0}", mi.Name));

				string netMigDir = mi.GetMigrationNetPath();

				if (!Directory.Exists(netMigDir))
					Directory.CreateDirectory(netMigDir);

				string netMigFile = Path.Combine(netMigDir, "MigrationInfo.xml");
				if (File.Exists(netMigFile))
				{
					xMigrationDoc.Load(netMigFile);
				}
				else
				{
					xMigrationDoc.LoadXml("<Database/>");
				}

				XmlDocument xDbObj = new XmlDocument();
				xDbObj.Load(dbObjFileName);

				foreach (XmlNode nView in xDbObj.SelectNodes("DatabaseObjects/Views/View"))
				{
					string ns = nView.Attributes["namespace"].Value.ToString();
					string[] tokens = ns.Split('.');
					string viewName = tokens[tokens.Length-1];

					AddTable(viewName);
					
					XmlNode nCreate = nView.SelectSingleNode("Create");
					int createRelease = int.Parse(nCreate.Attributes["release"].Value.ToString());
					int createStep = int.Parse(nCreate.Attributes["createstep"].Value.ToString());
					ParseScript(mi, viewName, createRelease, createStep);
					
				}

				try
				{
					xMigrationDoc.Save(netMigFile);
				}
				catch
				{
					SetLogError(string.Format("Non riesco a salvare il file {0}", netMigFile), ToString());
				}
			}
			EndRun(true);
		}

		private void ParseScript(BaseModuleInfo mi, string viewName, int createRelease, int createStep)
		{
			string createFolder = Path.Combine(mi.GetDatabaseScriptPath(), "Create");
			string createInfoFileName = Path.Combine(createFolder, "CreateInfo.xml");
			if (!File.Exists(createInfoFileName))
				return;

			XmlDocument xDoc = new XmlDocument();
			xDoc.Load(createInfoFileName);
			string xPath = string.Format("//Step[@numstep='{0}']", createStep.ToString());
			XmlNode nStep = xDoc.SelectSingleNode(xPath);
			string scriptName = nStep.Attributes["script"].Value.ToString();
			string scriptPath = Path.Combine(Path.Combine(createFolder, "All"), scriptName);

			if (!File.Exists(scriptPath))
				return;

			RichTextBox TxtReader = new RichTextBox();

			TxtReader.LoadFile(scriptPath, RichTextBoxStreamType.PlainText);
			int start = TxtReader.Text.IndexOf(string.Format("CREATE VIEW {0}", viewName));
			StringReader sr = new StringReader(TxtReader.Text.Substring(start));
			string riga = sr.ReadLine();
			while (!riga.StartsWith("GO") && riga != null)
			{
				riga = sr.ReadLine().Replace("\t", " ");
				if	(
					riga == string.Empty ||
					riga.Trim().StartsWith("FROM ") ||
					riga.Trim().StartsWith("ON ") ||
					riga.Trim().StartsWith("GROUP ") ||
					riga.Trim().StartsWith("UNION ") ||
					riga.Trim().StartsWith("ORDER ") ||
					riga.Trim().IndexOf("=") > 0 ||
					riga.Trim().IndexOf("<") > 0 ||
					riga.Trim().IndexOf(">") > 0
					)
					continue;

				string colName = string.Empty;
				int colStart = -1;

				if (riga.IndexOf("AS ") > 0)
					colStart = riga.IndexOf(" AS ") + 4;
				else
					colStart = riga.IndexOf(".") + 1;

				if (colStart > 0)
				{
					colName = riga.Substring(colStart).Trim().Replace(",", string.Empty).Trim();
					AddColumn(viewName, colName);
					AddMigrationColumn(viewName, colName, true, true);
				}
			}
		}
	}
}
