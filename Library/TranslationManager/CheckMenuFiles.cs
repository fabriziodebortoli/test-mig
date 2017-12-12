using System;
using System.Collections;
using System.Data;
using System.Xml;
using System.IO;
using System.Diagnostics;

using Microarea.Library.TranslationManager;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for CheckMenuFiles.
	/// </summary>
	public class CheckMenuFiles : BaseTranslator
	{
		public CheckMenuFiles()
		{
		}

		protected TranslationManager	tm;
		protected DataTable				totalDataTable = null;
		//---------------------------------------------------------------------------

		public override string ToString()
		{
			return "Check Menu Files";
		}

		#region Funzioni per controllare la congruenza dei menù
		//---------------------------------------------------------------------------
		private void CheckMenuDictionaries()
		{

			totalDataTable = new DataTable();
			totalDataTable.Columns.Add("Base",		typeof(string));
			totalDataTable.Columns.Add("Target",	typeof(string));
			totalDataTable.Columns.Add("PathFile", typeof(string));

			//Preparo le queries XPath

			string[] queries = {"AppMenu/Application/Title",
								   "AppMenu/Application/Group/Title",
								   "AppMenu/Application/Group/Menu/Title",
								    "AppMenu/Application/Group/Menu/Menu/Title",
								   "//Report/Title",
								   "//Document/Title",
								   "//Batch/Title",
								   "//Function/Title",};


			if(tm == null || tm.GetApplicationInfo().PathFinder == null)
				return;

			//Carico i dizionary in un ArrayList i tengo a perti ma mi devo ricordare 
			//di chiuderli
			ArrayList dictionaries = LoadAllDictionary();
			if (dictionaries == null || dictionaries.Count ==0)
				return;

			//Loop su ogni modulo dell'applizaione
			foreach(BaseModuleInfo mod in tm.GetApplicationInfo().Modules)
			{
				if (mod == null)
					return;

				string menuPath = Path.Combine(mod.Path, "Menu");
				if (!Directory.Exists(menuPath))
					continue;

				string[] menuFiles = Directory.GetFiles(menuPath, "*.menu");

				foreach(string fileName in menuFiles)
				{
					string fileToOpen = Path.Combine(menuPath , fileName);
					if (!File.Exists(fileToOpen))
						continue;

					XmlDocument menuXml = new XmlDocument();
					menuXml.Load(fileToOpen);
					foreach(string query in queries)
					{
						XmlNodeList nodeList = menuXml.SelectNodes(query);
						if (nodeList.Count == 0)
							continue;

						foreach(XmlNode selectedNode in nodeList)
						{
							ExistMostTargetString(selectedNode.InnerText, dictionaries);
						}
					}
				}
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ExistMostTargetString (string baseString, ArrayList dictionary)
		{

			DataTable findStringsDataTble = new DataTable("Dictionary");
			findStringsDataTble.Columns.Add("Base",		typeof(string));
			findStringsDataTble.Columns.Add("Target",	typeof(string));
			findStringsDataTble.Columns.Add("PathFile", typeof(string));
			
			string whereInXPath		= string.Empty;
			string whereInSelect = string.Empty;

			if(baseString.IndexOf("'")==-1)
				whereInXPath ="='" + baseString + "']";
			else
				whereInXPath =String.Concat("=concat('", baseString.Replace("'","', \"'\", '"), "')]");

			foreach(XmlDocument dom in dictionary)
			{
				
				XmlNodeList nodeList = dom.SelectNodes("xmls/xml/string[@base" + whereInXPath);
				if (nodeList != null && nodeList.Count == 0)
					continue;

				foreach(XmlNode node in nodeList)
				{

					if(node.Attributes["base"].InnerText.IndexOf("'")== -1)
						whereInSelect = node.Attributes["base"].InnerText;
					else
						whereInSelect =node.Attributes["base"].InnerText.Replace("'","''''");

					DataRow[] findDr = findStringsDataTble.Select("Base ='" + whereInSelect + "'");
					if (findDr == null || findDr.Length  == 0 )
						AddNewRow(ref findStringsDataTble, node, dom);
					else
					{
						bool find = false;
						foreach(DataRow dr in findDr)
						{
							if (string.Compare(node.Attributes["target"].InnerText.ToUpper(), dr["target"].ToString().ToUpper())==0)
							{
								find = true;
								continue;
							}	
						}
						if (find == false)
							AddNewRow(ref findStringsDataTble, node, dom); 
					}
				}
			}
			
			if (findStringsDataTble.Rows.Count > 1)
			{
				if (totalDataTable != null || totalDataTable.Rows.Count > 0)
				{

					DataRow[] mydr =totalDataTable.Select("Base='"+  findStringsDataTble.Rows[0][0].ToString() +"'");
					if (mydr.Length>0)
						return false;
				}

				string messageError = String.Concat( "Sono stare rilevate ",
					findStringsDataTble.Rows.Count,
					" traduzioni differenti per il temine ", 
					findStringsDataTble.Rows[0][0].ToString(),
					", i file di dizionario incriminati sono: ");

				foreach(DataRow dr in findStringsDataTble.Rows)
					messageError = string.Concat(messageError, 
						dr["PathFile"],
						" termine tradotto in ",
						dr["target"].ToString(),
						"  ");

				SetLogError(messageError, ToString());

				foreach(DataRow dr in findStringsDataTble.Rows)
				{
					DataRow newRow = totalDataTable.NewRow();
					newRow["base"] = dr["Base"].ToString();
					newRow["target"] = dr["target"].ToString();
					newRow["PathFile"] = dr["PathFile"].ToString();
					totalDataTable.Rows.Add(newRow);
				}

				return true;
			}
			else
				return false;
		}

		//---------------------------------------------------------------------------
		private void AddNewRow(ref DataTable findStringsDataTble, XmlNode node, XmlDocument dom)
		{
			DataRow dr = findStringsDataTble.NewRow();
			dr["Base"] = node.Attributes["base"].InnerText;
			dr["Target"] = node.Attributes["target"].InnerText;
			dr["PathFile"] = dom.BaseURI;
			findStringsDataTble.Rows.Add(dr);	
		}
		//---------------------------------------------------------------------------
		private ArrayList LoadAllDictionary()
		{
		
			ArrayList dictionaryArrayList = null;

			foreach (BaseModuleInfo aModInfo in tm.GetApplicationInfo().Modules)
			{
				//Compongo il path dei dizionari
				string dictionaryPath = Path.Combine(aModInfo.GetDictionaryPath(), "en");
				dictionaryPath = Path.Combine(dictionaryPath, "xml");
				if (!Directory.Exists(dictionaryPath))
					continue;

				string menuPath = Path.Combine(aModInfo.Path, "Menu");
				if (!Directory.Exists(menuPath))
					continue;

				string[] menuFiles = Directory.GetFiles(menuPath, "*.menu");

				if (menuFiles == null || menuFiles.Length== 0)
					continue;

				
				foreach(string fileName in menuFiles)
				{
					string a = Path.GetFileNameWithoutExtension(fileName);
					if (!File.Exists(Path.Combine(dictionaryPath, a + ".xml")))
						continue;

					XmlDocument dictionary = new XmlDocument();
					try
					{
						dictionary.Load(Path.Combine(dictionaryPath, a + ".xml"));
						if (dictionaryArrayList == null)
							dictionaryArrayList = new ArrayList();

						dictionaryArrayList.Add(dictionary);
					}
					catch(Exception exc)
					{
						Debug.Fail(exc.Message);
						SetProgressMessage(exc.Message);
						return null;
					}
				}
			}
			return dictionaryArrayList;
		}
		#endregion

		//---------------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;

			SetProgressMessage("	Controllo congruenza file di Menu ");
			CheckMenuDictionaries();
			EndRun(false);
		}
	}
}
