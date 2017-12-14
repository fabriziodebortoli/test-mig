using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Salva le impostazioni custom.
	/// </summary>
	public class LocalState
	{
		public const string CFG_CUSTOM_SETTING_TOOLS		= "Tools";
		public const string CFG_CUSTOM_SETTING_GLOSSARIES	= "Glossaries";
		
		private ArrayList		toolsList = new ArrayList();
		private Hashtable		glossariesList = new Hashtable();
		
		private string lastError;

		//--------------------------------------------------------------------------------------------------------------------------------
		public LocalState()
		{
			
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public LocalState(DictionaryCreator dictionaryCreator) : this()
		{
			if (dictionaryCreator == null)
				return;

			toolsList				= dictionaryCreator.ToolsList;
			glossariesList			= dictionaryCreator.ExternalGlossaries;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		internal ArrayList		ToolsList		{ get { return toolsList; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal Hashtable		GlossariesList	{ get { return glossariesList; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool LoadFromConfiguration()
		{
			// ConfigurationSettings.GetConfig returns configuration settings for a user-defined configuration section.
			// ConfigurationSettings.AppSettings gets configuration settings in the <appSettings> Element configuration section.
			NameValueCollection config = null;
			try
			{
				config = (NameValueCollection)ConfigurationManager.AppSettings;
			}
			catch (ConfigurationException exc)
			{
				Debug.Fail("LocalState - LoadFromConfiguration: " + exc.Message);
				return false;
			}
			IDictionary dicT = (IDictionary)ConfigurationManager.GetSection(CFG_CUSTOM_SETTING_TOOLS);
			if (dicT != null)
			{
				if (toolsList == null)
					toolsList = new ArrayList();
				foreach (DictionaryEntry de in dicT)
				{
					string val	= de.Value as string;
					string path = null;
					string args = null;
					ToolInfo.SplitPathAndArgs(val, out path, out args);
					toolsList.Add(new ToolInfo(path, de.Key as string, args));
				}
			}

			//glossaries
			IDictionary dicG = (IDictionary)ConfigurationManager.GetSection(CFG_CUSTOM_SETTING_GLOSSARIES);
			if (dicG != null) 
			{
				if (glossariesList == null)
					glossariesList = new Hashtable();
				foreach (DictionaryEntry de in dicG)
				{
					string language	= de.Key as string;
					string concat = de.Value as string;
					GlossaryFiles[] files = GlossaryFiles.SplitPaths(concat, language);
					glossariesList.Add(language, files);
				}
			}
			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void PrepareSections(ConfigXmlDocument cfgXmlDoc)
		{
			bool tools		= (toolsList != null && toolsList.Count > 0);
			bool glossaries = (glossariesList != null && glossariesList.Count > 0);

			//svuoto o creo il nodo sections
			XmlNode appSettingsNode = cfgXmlDoc.SelectSingleNode(ConfigStrings.Element.Configuration + "//" + ConfigStrings.Element.AppSettings);
			XmlNode confNode		= cfgXmlDoc.SelectSingleNode(ConfigStrings.Element.Configuration);
			XmlNode sectionsNode = confNode.SelectSingleNode(ConfigStrings.Element.ConfigSections);
			if (sectionsNode == null)
			{
				sectionsNode = cfgXmlDoc.CreateElement(ConfigStrings.Element.ConfigSections);
				confNode.InsertBefore(sectionsNode, confNode.FirstChild);
			}

			//aggiungo le section, se è il caso, al nodo sections
			if (tools)
			{
				XmlElement elT = sectionsNode.SelectSingleNode(ConfigStrings.Element.Section + "[@"+ConfigStrings.Attribute.Name+ "='"+ CFG_CUSTOM_SETTING_TOOLS +"']") as XmlElement;
				if (elT == null)
				{
					elT = cfgXmlDoc.CreateElement(ConfigStrings.Element.Section);
					elT.SetAttribute(ConfigStrings.Attribute.Name, CFG_CUSTOM_SETTING_TOOLS);
					sectionsNode.AppendChild(elT);
				}
				elT.SetAttribute(ConfigStrings.Attribute.Type, ConfigStrings.Declaration.ObjectType);
			}

			if (glossaries)
			{
				XmlElement elG = sectionsNode.SelectSingleNode(ConfigStrings.Element.Section + "[@"+ConfigStrings.Attribute.Name+ "='"+ CFG_CUSTOM_SETTING_GLOSSARIES +"']") as XmlElement;
				if (elG == null)
				{
					elG = cfgXmlDoc.CreateElement(ConfigStrings.Element.Section);
					elG.SetAttribute(ConfigStrings.Attribute.Name, CFG_CUSTOM_SETTING_GLOSSARIES);
					sectionsNode.AppendChild(elG);
				}
				
				elG.SetAttribute(ConfigStrings.Attribute.Type, ConfigStrings.Declaration.ObjectType);
				
			}

		

			//scrivo nelle sections specifiche se ho qualcosa da scrivere, altrimenti cancello solo il contenuto.
			AddTools(cfgXmlDoc, sectionsNode, confNode, appSettingsNode, tools);
			AddGlossaries(cfgXmlDoc, sectionsNode, confNode, appSettingsNode, glossaries);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddTools(ConfigXmlDocument cfgXmlDoc, XmlNode sectionsNode, XmlNode confNode, XmlNode appSettingsNode, bool add)
		{
			XmlNode toolsNode = confNode.SelectSingleNode(CFG_CUSTOM_SETTING_TOOLS);
			if (!add) 
			{
				if(toolsNode != null)
					confNode.RemoveChild(toolsNode);
				return;
			}
			if (toolsNode != null)
				toolsNode.RemoveAll();
			
			else 
				toolsNode = cfgXmlDoc.CreateElement(CFG_CUSTOM_SETTING_TOOLS);
			XmlElement el = toolsNode as XmlElement;
			foreach (ToolInfo ti in toolsList)
			{	
				string concat = ToolInfo.ConcatPathAndArgs(ti.Url, ti.Args);
				if (concat == String.Empty) continue;
				//trim perchè non accetta spazi prima e darebbe errore
				el.SetAttribute(ti.Name.Trim(), concat.Trim());
			}
			confNode.InsertAfter(el, sectionsNode);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddGlossaries(ConfigXmlDocument cfgXmlDoc, XmlNode sectionsNode, XmlNode confNode, XmlNode appSettingsNode, bool add)
		{
			XmlNode toolsNode = confNode.SelectSingleNode(CFG_CUSTOM_SETTING_GLOSSARIES);
			if (!add) 
			{
				if(toolsNode != null)
					confNode.RemoveChild(toolsNode);
				return;
			}
			if (toolsNode != null)
				toolsNode.RemoveAll();
			else 
				toolsNode = cfgXmlDoc.CreateElement(CFG_CUSTOM_SETTING_GLOSSARIES);
			XmlElement el = toolsNode as XmlElement;
			foreach (DictionaryEntry de in glossariesList)
			{
				string language = de.Key as string;			
				GlossaryFiles[] list = de.Value as GlossaryFiles[];
				string concat = GlossaryFiles.ConcatPaths(list);
				if (concat == null || concat == String.Empty)
					continue;
				//trim perchè non accetta spazi prima e darebbe errore
				el.SetAttribute(language.Trim(), concat.Trim());
			}
			confNode.InsertAfter(el, sectionsNode);
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SaveToConfiguration()
		{
			try
			{
				ConfigXmlDocument cfgXmlDoc = new ConfigXmlDocument();
				XmlProcessingInstruction newPI = cfgXmlDoc.CreateProcessingInstruction(ConfigStrings.Declaration.Xml, ConfigStrings.Declaration.Version);
				cfgXmlDoc.AppendChild(newPI);
		
				string cfgFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
				if (File.Exists(cfgFile))
				{
					LocalizerDocument tmpcfgXmlDoc = new LocalizerDocument();
					tmpcfgXmlDoc.Load(cfgFile);

					XmlNode newNode = cfgXmlDoc.ImportNode(tmpcfgXmlDoc.DocumentElement,true);
					cfgXmlDoc.AppendChild(newNode); 
				}

				XmlNode appSettingsNode = cfgXmlDoc.SelectSingleNode(ConfigStrings.Element.Configuration + "//" + ConfigStrings.Element.AppSettings);
				if (appSettingsNode == null)
				{
					if (cfgXmlDoc.DocumentElement == null)
					{
						XmlNode root = cfgXmlDoc.CreateElement(ConfigStrings.Element.Configuration);
						if (root != null)
							cfgXmlDoc.AppendChild(root); 
					}
					if (cfgXmlDoc.DocumentElement != null)
					{
						appSettingsNode = cfgXmlDoc.CreateElement(ConfigStrings.Element.AppSettings);
						cfgXmlDoc.DocumentElement.AppendChild(appSettingsNode);
					}
				}

				PrepareSections(cfgXmlDoc);

				if (appSettingsNode != null)
				{
					bool isReadOnly = false;
					if (File.Exists(cfgFile) && CommonUtilities.Functions.IsReadOnlyFile(cfgFile))
					{
						isReadOnly = true;
						File.SetAttributes(cfgFile, File.GetAttributes(cfgFile) & ~FileAttributes.ReadOnly);
					}
					
					cfgXmlDoc.Save(cfgFile);
					
					if (isReadOnly)
						File.SetAttributes(cfgFile, File.GetAttributes(cfgFile) | FileAttributes.ReadOnly);
				}
			}
			catch(Exception exception) 
			{
				lastError = exception.Message;
				Debug.Fail("LocalState.SaveToConfiguration Error: " + lastError);
				return false;
			}		
				
			return true;
		}
	}
	
	//=========================================================================
	public class ConfigStrings
	{
		public const string ArgsSeparator = "TBLArgs:";

		//=========================================================================
		public class Element
		{
			public const string Add				= "add";
			public const string Configuration	= "configuration";
			public const string AppSettings		= "appSettings";
			public const string ConfigSections	= "configSections";
			public const string Section			= "section";
		}

		//=========================================================================
		public class Attribute
		{
			
			public const string Key				= "key";
			public const string Value			= "value";
			public const string Name			= "name";
			public const string Type			= "type";
		}

		//=========================================================================
		public class Declaration
		{
			public const string Xml				= "xml";
			public const string Version			= "version=\"1.0\" encoding=\"utf-8\"";
			public const string ObjectType		= "System.Configuration.SingleTagSectionHandler";
		}
	}
}
