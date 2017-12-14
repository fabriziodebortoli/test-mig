using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for CPPTranslator.
	/// </summary>
	//================================================================================
	public class AddOnFieldParser : TableParser
	{
		public AddOnFieldParser()
		{
			defaultLookUpType = LookUpFileType.Tables;
		}

		public override string ToString()
		{
			return "AddOnField parser";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(true);

			//nMain = CreaNodoApplication(transManager.GetApplicationInfo().Name, false);
			
			foreach (BaseModuleInfo mi in transManager.GetApplicationInfo().Modules)
			{
				SetProgressMessage(string.Format("Elaborazione in corso del modulo: {0}", mi.Name));
				curModName = mi.Name;
				string addOnFileName = mi.GetAddOnDatabaseObjectsPath();

				if (!File.Exists(addOnFileName))
					continue;

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

				XmlDocument xAddOn = new XmlDocument();
				xAddOn.Load(addOnFileName);

				foreach (XmlNode nTable in xAddOn.SelectNodes("AddOnDatabaseObjects/AdditionalColumns/Table"))
				{
					foreach (XmlNode nAlterTable in nTable.SelectNodes("AlterTable"))
					{
						try
						{
							string ns = nAlterTable.Attributes["namespace"].Value.ToString();
							ILibraryInfo li = mi.GetLibraryInfoByName(ns.Split('.')[ns.Split('.').Length-1]);
							if (li == null)
							{
								string errore = string.Format("Namespace di AlterTable {0} errato.", ns);
								SetLogError(errore, ToString());
								continue;
							}
							// recupero il nome dell'interface di library
							string interfaceFileName = Path.Combine(li.FullPath, li.Name + "Interface.cpp");
							if (!File.Exists(interfaceFileName))
							{
								string errore = string.Format("Non trovo il file {0}.", interfaceFileName);
								SetLogError(errore, ToString());
								continue;
							}
							// recupero il nome della tabella su cui aggiungo i field e della classe relativa
							string tableClass = nTable.Attributes["namespace"].Value.ToString();
							string tableName = tableClass = tableClass.Split('.')[tableClass.Split('.').Length-1];
							int idx = tableName.IndexOf("_") + 1;
							if (idx < 0) idx = 0;
							tableClass = "T" + tableName.Substring(idx);

							FindMacro(li, interfaceFileName, tableClass, tableName);
						}
						catch
						{
							string ns = nAlterTable.Attributes["namespace"].Value.ToString();
							string errore = string.Format("Non riesco trovare la definizione degli AddonsFiled della tabella {0}", ns);
							SetLogError(errore, ToString());
						}
					}
				}

				try
				{
					xMigrationDoc.Save(netMigFile);
				}
				catch
				{
				}
			}
			EndRun(true);
		}

		private void FindMacro(ILibraryInfo li, string interfaceFileName, string tableClass, string tableName)
		{
			System.Windows.Forms.RichTextBox RTB = new System.Windows.Forms.RichTextBox();
			RTB.LoadFile(interfaceFileName, RichTextBoxStreamType.PlainText);
			string interfaceText = RTB.Text;
			
			//cerco la WHEN_TABLE
			int inizio = interfaceText.IndexOf("(" + tableClass);
			int fine = interfaceText.IndexOf("END_TABLE", inizio);
			while (true)
			{
				inizio = ParseAddonClass(li, interfaceText, tableName, inizio, fine);
				if (inizio < 0)
					return;
			}
		}

		private int ParseAddonClass(ILibraryInfo li, string interfaceText, string tableName, int inizio, int fine)
		{
			int i = interfaceText.IndexOf("ADDON_COLUMNS_CLASS(", inizio, fine - inizio) + 20;
			if (i <= 19)
				return -1;
			int f = interfaceText.IndexOf(")", i, fine - inizio);
			if (f < 0)
				return -1;
			string classeAddon = interfaceText.Substring(i, f-i);

			ParseInclude(li, interfaceText, classeAddon, tableName);

			return f;
		}

		private void ParseInclude(ILibraryInfo li, string interfaceText, string classeAddon, string tableName)
		{
			ArrayList includedFiles = GetIncludedFiles(li, interfaceText);

			foreach (string fileName in includedFiles)
			{
				if (ParseIncludedFile(fileName, classeAddon, tableName))
					return;
			}
		}

		private bool ParseIncludedFile(string fileName, string classeAddon, string tableName)
		{
			System.Windows.Forms.RichTextBox RTB = new System.Windows.Forms.RichTextBox();
			RTB.LoadFile(fileName, RichTextBoxStreamType.PlainText);
			string fileText = RTB.Text;

			int inizio = fileText.IndexOf(classeAddon + "::BindAddOnFields");
			if (inizio < 0)
				return false;

			int fine = fileText.IndexOf("END_BIND_ADDON_FIELDS", inizio);
			while (true)
			{
				int i = fileText.IndexOf("_NS_FLD(\"", inizio, fine - inizio) + 9;
				if (i <= 8)
					break;
				int f = fileText.IndexOf(")", i) - 1;
				if (f < 0)
					break;
				string campoAddon = fileText.Substring(i, f-i);
				AddColumn(tableName, campoAddon);
				AddMigrationColumn(tableName, campoAddon, false, false);
				inizio = f;
			}

			return true;
		}

		private ArrayList GetIncludedFiles(ILibraryInfo li, string interfaceText)
		{
			ArrayList retValue = new ArrayList();
			int ini = 0;
			while (true)
			{
				ini = interfaceText.IndexOf("#include \"T", ini);
				if (ini < 0)
					break;

				ini += 10;
				string fileClasseAddon = Path.Combine(li.FullPath, interfaceText.Substring(ini).Split('.')[0] + ".cpp");
				if (File.Exists(fileClasseAddon))
				{
					retValue.Add(fileClasseAddon);
				}
			}

			return retValue;
		}
	}
}
