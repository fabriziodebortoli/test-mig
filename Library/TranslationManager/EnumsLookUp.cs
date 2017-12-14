using System;
using System.Xml;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.TranslationManager;
using Microarea.TaskBuilderNet.Core.Applications;

namespace Microarea.Library.TranslationManager
{
	public class EnumsLookUp: BaseTranslator
	{

		protected TranslationManager	tm;

		public EnumsLookUp()
		{

		}

		public override string ToString()
		{
			return "Enums LookUp Generator ";
		}

		//---------------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;

			SetProgressMessage(" Creazione dei file di LookUp per gli enumerativi ");
			LoadEnumsFiles();
			EndRun(false);
		}

		//---------------------------------------------------------------------------
		private void LoadEnumsFiles()
		{
			if(tm == null || tm.GetApplicationInfo().PathFinder == null)
				return;

			string enumsFile = string.Empty;
			//Loop su ogni modulo dell'applizaione
			foreach(BaseModuleInfo mod in tm.GetApplicationInfo().Modules)
			{
				if (mod == null)
					return;
				
				if(!Directory.Exists(mod.GetModuleObjectPath()))
					continue;

				enumsFile = Path.Combine(mod.GetModuleObjectPath(), "Enums.xml");

				if (enumsFile == string.Empty || !File.Exists(enumsFile))
					continue;

				Translate(enumsFile, mod);
			}
		}

		//---------------------------------------------------------------------
		private void Translate(string enumsFile, BaseModuleInfo mod)
		{
			Enums enums = new Enums();
			enums.LoadXml(enumsFile, mod);

			//Compongo il path dei dizionari
			string dictionaryPath = Path.Combine(mod.GetDictionaryPath(), "en");
			dictionaryPath = Path.Combine(dictionaryPath, "other");
			if (!Directory.Exists(dictionaryPath))
				return;

			//File dei dizionari
			if (!File.Exists(Path.Combine(dictionaryPath, "enums.xml")))
				return;
		
			XmlDocument dictionaryDocument = new XmlDocument();
			dictionaryDocument.Load(Path.Combine(dictionaryPath, "enums.xml"));

			XmlDocument lookUpDocument = new XmlDocument();

			//Dichiarazione XML
			XmlDeclaration configDeclaration = lookUpDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			if (configDeclaration != null)
				lookUpDocument.AppendChild(configDeclaration);
			
			// Creazione della root ovvero <Emuns>
			XmlElement emunsElement = lookUpDocument.CreateElement("Enums");
			if (emunsElement == null)
				return;
			lookUpDocument.AppendChild(emunsElement);

			foreach(EnumTag enumTags in enums.Tags)
			{
				//<Tag>
				XmlElement tagElement =  lookUpDocument.CreateElement("Tag");
				if (tagElement == null)
					return;

				//Setto l'attributo name
				tagElement.SetAttribute("oldName", enumTags.Name);
				//Setto l'attributo value
				tagElement.SetAttribute("newName", GetEnglishName(enumTags.Name, dictionaryDocument));
				//Setto l'attributo value
				tagElement.SetAttribute("value", enumTags.Value.ToString());
										
				emunsElement.AppendChild(tagElement);
				foreach(EnumItem enummItem in enumTags.EnumItems)
				{
					//<Tag>
					XmlElement itemElement =  lookUpDocument.CreateElement("Item");
					if (tagElement == null)
						return;

					//Setto l'attributo name
					itemElement.SetAttribute("oldName", enummItem.Name);
					//Setto l'attributo value
					itemElement.SetAttribute("newName", GetEnglishName(enummItem.Name, dictionaryDocument));
					//Setto l'attributo value
					itemElement.SetAttribute("value", enummItem.Value.ToString());
										
					tagElement.AppendChild(itemElement);
				}
				
			}
			string destinationPath = mod.GetMigrationNetPath();
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);
			
			lookUpDocument.Save(Path.Combine(destinationPath, "enums.xml"));

			destinationPath = mod.GetMigrationXpPath();
			if (!Directory.Exists(destinationPath))
				Directory.CreateDirectory(destinationPath);
			
			lookUpDocument.Save(Path.Combine(destinationPath, "enums.xml"));

		}
		//---------------------------------------------------------------------
		private string GetEnglishName(string italiaName, XmlDocument dictionaryDocument)
		{
			string whereInXPath		= string.Empty;

			if(italiaName.IndexOf("'")==-1)
				whereInXPath ="='" + italiaName + "']";
			else
				whereInXPath =String.Concat("=concat('", italiaName.Replace("'","', \"'\", '"), "')]");

			XmlNodeList nodeList = dictionaryDocument.SelectNodes("enums/enum/string[@base" + whereInXPath);
			if (nodeList != null && nodeList.Count == 0)
					return string.Empty;

			return nodeList[0].Attributes["target"].InnerText;
		}

	}
}
