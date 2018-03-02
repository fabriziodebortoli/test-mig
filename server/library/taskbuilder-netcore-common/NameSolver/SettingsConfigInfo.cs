using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

using Microarea.Common.StringLoader;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using System.Collections.Generic;

namespace Microarea.Common.NameSolver
{

    public enum SourceOfSettingsConfig
	{
		Standard						= 0,
		AllCompaniesAllUsers			= 1,
		AllCompaniesSpecificiUser		= 2,
		SpecificiCompanyAllUsers		= 3,
		SpecificiCompanySpecificiUser	= 4,
	}

	//=========================================================================
	public class SettingsConfigInfoTable : List<SettingsConfigInfo>
	{
		//---------------------------------------------------------------------
		public SettingsConfigInfoTable()
		{
		}

		//---------------------------------------------------------------------
		public SettingItem GetSettingItem(string sectionName, string settingName, ModuleInfo aModuleInfo)
		{
			SettingItem setting = null;

			//Scorro al mio interno x vedere se ho il SettingsConfigInfo di quel modulo
			foreach (SettingsConfigInfo info in this)
			{
				setting = info.GetSettingItemByName (sectionName, settingName);
				if (setting != null)
					return setting;
			}

            List<string> fileArray = aModuleInfo.GetConfigFileArray();
			foreach(string fileName in fileArray)
			{
				// non ho trovato niente quindi me lo dovrò parsare
				SettingsConfigInfo settingsConfigInfo  = new SettingsConfigInfo(fileName, aModuleInfo);
				settingsConfigInfo.Parse();
				if (settingsConfigInfo.GetSettingItemByName (sectionName, settingName) != null)
					setting = settingsConfigInfo.GetSettingItemByName (sectionName, settingName);
				this.Add(settingsConfigInfo);
			}

			return setting;
		}
	}

	//=========================================================================
	public class SettingsConfigInfo
	{
		private string			fileName = "";
		private	bool			valid;
		private	string			parsingError;

		private ModuleInfo	parentModuleInfo;
		private List<SectionInfo>		sections;

		public	string			FileName		{ get { return fileName; } }
		public	bool			Valid			{ get { return valid; } }
		public	string			ParsingError	{ get { return parsingError; } }

		public ModuleInfo	ParentModuleInfo	{ get { return parentModuleInfo; } }
		public List<SectionInfo> Sections			{ get { return sections; } }

		//---------------------------------------------------------------------
		public SettingsConfigInfo(string aFilename, ModuleInfo	aParentModuleInfo)
		{
			if (aFilename == null || aFilename == string.Empty || 
				aFilename == string.Empty || aParentModuleInfo == null)
			{
				Debug.Fail("Error in SettingsConfigInfo"); 
			}
			
			fileName		= aFilename;
			valid			= true;
			parsingError	= string.Empty;
			parentModuleInfo= aParentModuleInfo;
		}

		//---------------------------------------------------------------------
		public bool Parse()
		{
			string filePath = "";
			if (fileName == null || fileName == string.Empty) 
				return false;

			//Parso il file nella standard
			filePath = parentModuleInfo.GetStandardSettingsFullFilename(fileName);
			if (filePath == null || filePath == string.Empty)
				return false;
			ParseSingleFile(filePath, SourceOfSettingsConfig.Standard);

            ModuleInfo mi = parentModuleInfo as ModuleInfo;
            if (mi == null)
                return true;

			//Parso il file nella CUSTOM/COMPANIE/ALLUSER
			filePath = mi.GetCustomCompanyAllUserSettingsPathFullFilename(fileName);
			if (filePath == null || filePath == string.Empty)
				return false;
			ParseSingleFile(filePath, SourceOfSettingsConfig.SpecificiCompanyAllUsers);
            			
			return true;
		}
		//---------------------------------------------------------------------
		private bool ParseSingleFile(string aFilePath, SourceOfSettingsConfig source)
		{
			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(aFilePath))
				return false;

			LocalizableXmlDocument settingsDocument = 
									new LocalizableXmlDocument
									(
										parentModuleInfo.ParentApplicationInfo.Name,
										parentModuleInfo.Name,
										parentModuleInfo.CurrentPathFinder
                                    );
			try
			{
				//leggo il file
				settingsDocument.Load(aFilePath);
				
				//root del documento (ParameterSettings)
				XmlElement root = settingsDocument.DocumentElement;

				//array delle section
				XmlNodeList sectionElemnts = ((XmlElement) root).GetElementsByTagName(SettingsConfigXML.Element.Section);
				if (sectionElemnts == null)
					return true;

				ParseSection(sectionElemnts, source);
			}
			catch(XmlException e)
			{
				valid = false;
				parsingError = e.Message;
				return false;
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message);
				valid = false;
				parsingError = err.Message;
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		private void ParseSection(XmlNodeList sectionNodes, SourceOfSettingsConfig sourceType)
		{
			if (sectionNodes == null)
				return;

			//inizializzo l'array delle Section
			if (sections == null)
				sections = new List<SectionInfo>();
		
			bool tempBool = false;

			foreach (XmlElement sectionElement in sectionNodes)
			{
				//name della section
				string name = sectionElement.GetAttribute(SettingsConfigXML.Attribute.Name);
				//localize della section
				string localize = sectionElement.GetAttribute(SettingsConfigXML.Attribute.Localize);
				SectionInfo aSectionInfo = new SectionInfo(name, localize);
				//L'eventuale release
				string temp = sectionElement.HasAttribute(SettingsConfigXML.Attribute.Release) ?  sectionElement.GetAttribute(SettingsConfigXML.Attribute.Release) : string.Empty;
				if (temp != string.Empty)
					aSectionInfo.Release = Int32.Parse(temp);
				//Parso l'eventuale attributo hidden
				temp = sectionElement.HasAttribute(SettingsConfigXML.Attribute.Hidden) ?  sectionElement.GetAttribute(SettingsConfigXML.Attribute.Hidden) : string.Empty;
				if (temp != string.Empty && temp != "false")
					aSectionInfo.Hidden= true;
					
				//Parso l'evetuale allowNewSettings
				tempBool = sectionElement.HasAttribute(SettingsConfigXML.Attribute.AllowNewSettings); 
				if (tempBool )
					aSectionInfo.AllowNewSettings = true;

				//Voglio un unica section con tutti i setting
				SectionInfo sectionAdded = GetSectionInfoByName(aSectionInfo.Name);
				if (sectionAdded == null)
					sections.Add(aSectionInfo);
				

				//array dei Setting
				XmlNodeList settingElements = ((XmlElement) sectionElement).GetElementsByTagName(SettingsConfigXML.Element.Setting);
				if (settingElements == null || settingElements.Count == 0)
					continue;
				
				//scorro setting
				foreach (XmlElement settingElement in settingElements)
				{
					//attributo name
					string settingName = settingElement.GetAttribute(SettingsConfigXML.Attribute.Name);
					//attributo type
					string settingType = settingElement.GetAttribute(SettingsConfigXML.Attribute.Type);
					SettingItem aSettingItem = new SettingItem(settingName, settingType);

					//Attributi opzionali
					//Release
					temp = settingElement.HasAttribute(SettingsConfigXML.Attribute.Release) ?  settingElement.GetAttribute(SettingsConfigXML.Attribute.Release) : string.Empty;
					if (temp != string.Empty)
						aSettingItem.Release = Int32.Parse(temp);
					//Localize
					temp = settingElement.HasAttribute(SettingsConfigXML.Attribute.Localize) ?  settingElement.GetAttribute(SettingsConfigXML.Attribute.Localize) : string.Empty;
					aSettingItem.Localize = temp;
					//Idden
					temp = settingElement.HasAttribute(SettingsConfigXML.Attribute.Hidden) ?  settingElement.GetAttribute(SettingsConfigXML.Attribute.Hidden) : string.Empty;
					if (temp != string.Empty)
						aSettingItem.Hidden = Convert.ToBoolean(temp);

					//Parso l'evetuale usersetting
					tempBool = settingElement.HasAttribute(SettingsConfigXML.Attribute.UserSetting); 
					if (tempBool)
						aSettingItem.UserSetting = true;

					//BaseType
					temp = settingElement.HasAttribute(SettingsConfigXML.Attribute.BaseType) ?  settingElement.GetAttribute(SettingsConfigXML.Attribute.BaseType) : string.Empty;
					aSettingItem.BaseType = temp;
					if (temp == "array")
					{
						//array dei Value
						XmlNodeList valueElements = ((XmlElement) settingElement).GetElementsByTagName(SettingsConfigXML.Element.ValueTag);
						if (valueElements == null || valueElements.Count == 0)
							continue;
						foreach (XmlElement valueElement in valueElements)
						{
							aSettingItem.AddValue(valueElement.InnerText);
						}
					}
					else
					{
						temp = settingElement.HasAttribute(SettingsConfigXML.Attribute.Value) ?  settingElement.GetAttribute(SettingsConfigXML.Attribute.Value) : string.Empty;
						aSettingItem.AddValue(temp);
					}
					
					//SourceFile
					aSettingItem.SourceFileType = sourceType;
					if (sectionAdded == null)
						aSectionInfo.AddSetting(aSettingItem);
					else
						sectionAdded.AddSetting(aSettingItem);
				}
			}
		}

		//---------------------------------------------------------------------
		public SectionInfo GetSectionInfoByName(string aName)
		{
			foreach (SectionInfo aSectionInfo in sections)
			{
				if (string.Compare(aSectionInfo.Name,aName) == 0)
					return aSectionInfo;
			}
			return null;
		}
		//---------------------------------------------------------------------
		private SettingItem GetMax(SettingItem setting, SettingItem settingInArray)
		{
			if ((int)setting.SourceFileType > (int)settingInArray.SourceFileType)
				return setting;
			else
				return settingInArray;
		}
		//---------------------------------------------------------------------
		public XmlDocument CreateDocument()
		{
			try
			{	
				XmlDocument xmlDocument = new XmlDocument();
				// root node
				XmlDeclaration aDeclaration = xmlDocument.CreateXmlDeclaration("1.0", null, "yes");

				if (aDeclaration == null) 
					return null;

				xmlDocument.RemoveAll();

				// root di ParameterSettings
				xmlDocument.AppendChild(aDeclaration);
				XmlNode newNode = xmlDocument.CreateElement (SettingsConfigXML.Element.ParameterSettings);
				xmlDocument.AppendChild(newNode);
				return xmlDocument;
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message);
				return null;
			}
		}
		
		//---------------------------------------------------------------------
		public XmlDocument UnparseSection(XmlDocument xmlDocument, SectionInfo sectionInfo)
		{
			//Creo il nodo Section
			XmlNode sectionNode = xmlDocument.CreateElement (SettingsConfigXML.Element.Section);

			//gli aggiungo l'Attributo Name
			XmlNode attr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Name);
			attr.Value = sectionInfo.Name;
			sectionNode.Attributes.SetNamedItem(attr);

			//Aggiungo l'attributo Localize
			attr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Localize);
			attr.Value = sectionInfo.Localize;
			sectionNode.Attributes.SetNamedItem(attr);

			// campo opzionale Release
			if (sectionInfo.Release != 0)
			{
				attr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Release);
				attr.Value = sectionInfo.Release.ToString();
				sectionNode.Attributes.SetNamedItem(attr);
			}
			//Cerco il nodo ParameterSettings
			XmlNodeList XmlNodeParameterSettings = xmlDocument.GetElementsByTagName(SettingsConfigXML.Element.ParameterSettings);
			XmlNode settingContainerNode= XmlNodeParameterSettings[0].AppendChild(sectionNode);

			//Ora ciclo sui Setting e li inserisco sotto alla Section
			//che ho appena creato
			XmlNode settingNode = null;
			foreach (SettingItem settingItem in sectionInfo.Settings)
			{
				//Creo il nodo Setting
				settingNode = xmlDocument.CreateElement (SettingsConfigXML.Element.Setting);

				//gli aggiungo l'Attributo Name
				XmlNode settingAttr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Name);
				settingAttr.Value = settingItem.Name;
				settingNode.Attributes.SetNamedItem(settingAttr);

				//Aggiungo l'attributo Type
				settingAttr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Type);
				settingAttr.Value = settingItem.Type;
				settingNode.Attributes.SetNamedItem(settingAttr);

				//Aggiungo l'attributo Hidden
				settingAttr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Hidden);
				settingAttr.Value = settingItem.Hidden.ToString();
				settingNode.Attributes.SetNamedItem(settingAttr);

				//Aggiungo l'attributo Localize
				settingAttr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Localize);
				settingAttr.Value = settingItem.Localize;
				settingNode.Attributes.SetNamedItem(settingAttr);

				//Aggiungo l'attributo value
				settingAttr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Value);
				settingAttr.Value = settingItem.Values[0].ToString();
				settingNode.Attributes.SetNamedItem(settingAttr);

				// campo opzionale Release
				if (settingItem.Release != 0)
				{
					settingAttr = xmlDocument.CreateAttribute(SettingsConfigXML.Attribute.Release);
					settingAttr.Value = settingItem.Release.ToString();
					settingNode.Attributes.SetNamedItem(settingAttr);
				}
				settingContainerNode.AppendChild(settingNode);
			}
			return xmlDocument;
		}
		//---------------------------------------------------------------------
		public XmlDocument DeleteSection (XmlDocument xmlDocument, string sectionName)
		{
			XmlNodeList xmlSectionNode = xmlDocument.GetElementsByTagName(SettingsConfigXML.Element.ParameterSettings);
			XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName(SettingsConfigXML.Element.Section);
		
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				if (xmlNode.Attributes["name"].Value == sectionName)
				{
					xmlSectionNode[0].RemoveChild(xmlNode);
					return xmlDocument;
				}
			}
			return xmlDocument;
		}
		//---------------------------------------------------------------------
		public SettingItem GetSettingItemByName(string sectionName, string settingName)
		{
			foreach(SectionInfo section in this.Sections)
			{
				if (string.Compare(sectionName, section.Name)== 0)
					return section.GetSettingItemByName(settingName);

			}
			return null;
		}
		//---------------------------------------------------------------------
	}
	//=========================================================================
	public class SettingItem
	{
		private string					name;
		private string					localize;
		private int						release		= 0;
		private string					type;
		private string					baseType;
		private bool					hidden		= false;
		private bool					userSetting	= false;
		private List<string> values = new List<string>();
		private SourceOfSettingsConfig	sourceFileType;

		// proprietà
		//---------------------------------------------------------------------
		public string					Name			{ get { return name; }				set { name = value; } }
		public string					Localize		{ get { return localize; }			set { localize = value; } }
		public int						Release			{ get { return release; }			set { release = value; } }
		public string					Type			{ get { return type; }				set { type = value; } }
		public string					BaseType		{ get { return baseType; }			set { baseType = value; } }
		public bool						Hidden			{ get { return hidden; }			set { hidden = value; } }
		public bool						UserSetting		{ get { return userSetting; }		set { userSetting = value; } }
		public List<string> Values			{ get { return values; }  }
		public SourceOfSettingsConfig	SourceFileType	{ get { return sourceFileType; }	set { sourceFileType = value; } }

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName"></param>
		/// <param name="aType"></param>
		//---------------------------------------------------------------------
		public SettingItem(string aName, string aType)
		{
			name = aName;
			type = aType;
		}

		/// <summary>
		/// Aggiunge i Value nel caso di Array al SettingItem
		/// </summary>
		/// <param name="valueItem"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public void AddValue(string valueItem)
		{
			if (valueItem == null)
				return;

			if (values == null)
				values = new List<string>();

			values.Add(valueItem);
		}
	}

	//=========================================================================
	public class SectionInfo
	{
		protected	string					name				= "";
		protected	string					localize			= "";
		protected	int						release				= 0;
		protected	bool					hidden				= false;
		protected   bool					allowNewSettings	= false;
		protected List<SettingItem> settings;
	

		/// <summary>
		/// name della section
		/// </summary>
		public string Name { get { return name; } set { name = value; } }

		/// <summary>
		/// localize della section
		/// </summary>
		public string Localize { get { return localize; } set { localize = value; } }

		/// <summary>
		/// Release della section
		/// </summary>
		public int	   Release	{ get { return release; }	set { release = value; } }
		
		/// <summary>
		/// Attrivuto che mi indica se si tratta di unasection nascosta
		/// </summary>
		public bool	   Hidden	{ get { return hidden; }	set { hidden = value; } }
		
		public bool	   AllowNewSettings	{ get { return allowNewSettings; }	set { allowNewSettings = value; } }

		/// <summary>
		/// Array dei setting della section
		/// </summary>
		public List<SettingItem> Settings { get { return settings; } }

		
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aName">namespace del documento</param>
		/// <param name="aTitle">nome del documento</param>
		//---------------------------------------------------------------------
		public SectionInfo(string aName, string aLocalize)
		{
			name = aName;
			localize = aLocalize;
			if (settings == null)
				settings = new List<SettingItem>();

		}
		//---------------------------------------------------------------------
		public SectionInfo()
		{
			if (settings == null)
				settings = new List<SettingItem>();

		}

		/// <summary>
		/// Aggiunge i SettingItem all'Array di SectionInfo
		/// </summary>
		/// <param name="settingItem"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public void AddSetting(SettingItem settingItem)
		{
			if (settingItem == null)
				return ;

			if (settings == null)
				settings = new List<SettingItem>();

			settings.Add(settingItem);
		}

		//---------------------------------------------------------------------
		public SettingItem FindSetting(string aName)
		{
			return GetSettingItemByName(aName);
		}
		//---------------------------------------------------------------------
		public SettingItem GetSettingItemByName(string aName)
		{
			List<SettingItem> settings = new List<SettingItem>();

			foreach (SettingItem aSettingItem in this.Settings)
			{
				if (aSettingItem.Name == aName)
					settings.Add(aSettingItem);
			}
			
			//Ora devo trovare il maggiore
			SettingItem setting = new SettingItem("", "");
			if (settings.Count != 0)
			{
				setting = (SettingItem)settings[0];
				foreach (SettingItem item in settings)
				{
					setting = this.GetMaxSetting(setting, item);
				}
				return setting;
			}
			return null;
		}
		//---------------------------------------------------------------------
		public List<SettingItem> GetSettingsItemByName(string aName)
		{
            List<SettingItem> settings = new List<SettingItem>();

			foreach (SettingItem aSettingItem in this.Settings)
			{
				if (aSettingItem.Name == aName)
					settings.Add(aSettingItem);
			}
			return settings;
		}
		//---------------------------------------------------------------------
		public List<SettingItem> GetSettingsBySourceType(SourceOfSettingsConfig source)
		{
            List<SettingItem> arrayList = new List<SettingItem>();

			foreach (SettingItem aSettingItem in settings)
			{
				if (aSettingItem.SourceFileType == source)
					arrayList.Add(aSettingItem);
			}
			return arrayList;
		}
		//---------------------------------------------------------------------
		public static SettingItem GetSettingBySourceType(SourceOfSettingsConfig source, List<SettingItem> settingArray)
		{
			foreach (SettingItem aSettingItem in settingArray)
			{
				if (aSettingItem.SourceFileType == source)
					return aSettingItem;
			}
			return null;
			
		}
		//---------------------------------------------------------------------
		public XmlDocument DeleteSetting(XmlDocument xmlDocument,  string sectionName, string settingItemName)
		{
			XmlNodeList xmlRootNode = xmlDocument.GetElementsByTagName(SettingsConfigXML.Element.ParameterSettings);
			XmlNodeList xmlSectionNode = xmlDocument.GetElementsByTagName(SettingsConfigXML.Element.Section);
			XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName(SettingsConfigXML.Element.Setting);
		
			//Loop sulle Section
			foreach (XmlNode xmlNode in xmlSectionNode)
			{
				if (xmlNode.Attributes["name"].Value == sectionName)
				{
					//Ora che ho beccato quella giusta loop sui Setting
					foreach (XmlNode xmlNodeSetting in xmlNodeList)
					{
						if (xmlNodeSetting.Attributes["name"].Value == settingItemName)
						{
							xmlNode.RemoveChild(xmlNodeSetting);
							if (xmlNode.ChildNodes.Count == 0)
								xmlRootNode[0].RemoveChild(xmlNode);
							return xmlDocument;
						}
					}
				}
			}
			return xmlDocument;
		}

		//---------------------------------------------------------------------
		public SettingItem GetMaxSetting (SettingItem oldSetting, SettingItem newSetting)
		{
			if ((int)oldSetting.SourceFileType > (int)newSetting.SourceFileType)
				return oldSetting;
			else
				return newSetting;
		}

		//---------------------------------------------------------------------
		public bool isModify (SectionInfo newSection)
		{
			foreach(SettingItem oldSetting in this.Settings )
			{
				SettingItem item  = this.GetSettingItemByName(oldSetting.Name);
				if (item == null)
					return true;
				SettingItem item2 = newSection.GetSettingItemByName(oldSetting.Name);
				if (item2 == null)
					return true;

				if (string.Compare(item.Values[0].ToString(), item2.Values[0].ToString())!= 0)
					return true;
			}
			return false;
		}
	}

	//============================================================================
	public class ReadSetting
	{
		public static string GetMaxString(PathFinder pathFinder, string culture)
		{	
			ModuleInfo m = (ModuleInfo)pathFinder.GetModuleInfo(new NameSpace("Module.Framework.TbGenlib"));

			string path = m.GetCustomCompanyAllUserSettingsPathFullFilename("Settings.config");

			string upperLimit = GetUpperLimit(culture, m, path, false);

			if (upperLimit != string.Empty)
				return upperLimit;

			path = m.GetStandardSettingsFullFilename("Settings.config");

			upperLimit = GetUpperLimit(culture, m, path, true);

			return upperLimit;
		}

		//---------------------------------------------------------------------------
		private static string GetUpperLimit(string culture, ModuleInfo m, string path, bool mandatory)
		{	
			object s = GetEntry (path, "Culture_" + culture, "UpperLimit", m, false);
			if (s != null) 
				return s.ToString();
		    s = GetEntry (path, "Culture", "UpperLimit", m, mandatory);
			if (s != null)  
				return s.ToString();

			return String.Empty;
		}

		//---------------------------------------------------------------------------
		public static double GetDataDblDecimal(PathFinder pathFinder)
		{
			ModuleInfo m = (ModuleInfo)pathFinder.GetModuleInfo(new NameSpace("Module.Framework.TbGenlib"));

			string path = m.GetCustomCompanyAllUserSettingsPathFullFilename("Settings.config");

			double dec = GetDataDblDecimal(m, path, false);

			if (dec != -1)
				return dec;

			path = m.GetStandardSettingsFullFilename("Settings.config");

            dec = GetDataDblDecimal(m, path, true);

			return dec >= 0 ? Math.Pow(10, -dec) : 7;
		}

		//---------------------------------------------------------------------------
		private static int GetDataDblDecimal(ModuleInfo m, string path, bool mandatory)
		{
			//Monetary Decimals, Percentage Decimals, Quantity Decimals
			object s = GetEntry(path, "Data Type Epsilons", "Double Decimals", m, mandatory);
			if (s != null)
				return int.Parse(s as String);

			return -1;
		}

		//---------------------------------------------------------------------------
		public static object GetSettings (PathFinder pathFinder, string namespaceSetting, string sSection, string sEntry, object defaultSettingValue)
		{
			NameSpace nsSetting = new NameSpace(namespaceSetting, NameSpaceObjectType.Setting);
			NameSpace nsModule = new NameSpace(nsSetting.Application + '.' + nsSetting.Module, NameSpaceObjectType.Module);

			ModuleInfo m = (ModuleInfo)pathFinder.GetModuleInfo(nsModule);
            
            if (m == null)
				 return defaultSettingValue;

			string path = m.GetCustomCompanyAllUserSettingsPathFullFilename(nsSetting.Setting);
            object s = GetEntry (path, sSection, sEntry, m, false);
			if (s != null) 
				return s;

   			path = m.GetStandardSettingsFullFilename(nsSetting.Setting);
		    s = GetEntry (path, sSection, sEntry, m, true);
			if (s != null) 
				return s;

    		return defaultSettingValue;
		}

		//---------------------------------------------------------------------------
		public static object GetEntry (string path, string sSection, string sEntry, ModuleInfo m, bool mandatory)
		{
			//adding the extension if is missing
	        string extension = Path.GetExtension(path);
			if (String.Compare(extension, ".config",StringComparison.OrdinalIgnoreCase) != 0)
			  path = String.Concat(path, ".config");
					
			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(path))
			{
				if (mandatory) Debug.Fail("missing " + path);
				return null;
			}

			SettingsConfigInfo sci = new SettingsConfigInfo(path, m);
			if (!sci.Parse())
			{
				Debug.Fail("Error on parsing "+ path);
				return null;
			}

			SectionInfo si = sci.GetSectionInfoByName(sSection);
			SettingItem item = null;
			
			if (si == null)
			{
				if (mandatory) Debug.Fail("Section "+ sSection +" not found into "+ path);
				return null;
			}
			else 
			{
				item = si.GetSettingItemByName(sEntry);
			}
			if (item == null)
			{
				if (mandatory) Debug.Fail("Section "+ sSection +", entry"+ sEntry +" not found into "+ path);
				return null;
			}
			return item.Values[0];
		}
	}
}
