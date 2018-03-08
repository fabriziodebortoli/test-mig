using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio
{
	//=========================================================================
	public class EasyStudioPreferences
	{
		public string PreferencesPath
		{
			get
			{
				return PathFinder.PathFinderInstance.GetEasyStudioHomePath();
			}
		}
		public string PreferencesFullFileName
		{
			get
			{
				return Path.Combine(
					PathFinder.PathFinderInstance.GetEasyStudioHomePath(),
					EasyStudioPreferencesXML.Element.ESPreferences +
					NameSolverStrings.XmlExtension);
			}
		}


		//---------------------------------------------------------------
		public Stream GetPreferencesFile()
		{
			try
			{
				return PathFinder.PathFinderInstance.GetStream(PreferencesFullFileName, true);
			}
			catch (Exception)
			{
				return null;
			}

		}

		//---------------------------------------------------------------
		public bool SetPreferences(string curApp, string curMod, bool isDefault)
		{
			if (!ExistPreferencesFile())
			{
				CreatePreferences();
			}
			XmlDocument doc = new XmlDocument();
			doc.Load(GetPreferencesFile());
			XmlElement root = doc.DocumentElement;
			if (root == null) return false;
			var preferAttrib = root.GetAttributeNode(EasyStudioPreferencesXML.Element.ESPreferences);
			root.SetAttribute(EasyStudioPreferencesXML.Element.CurrentApplication, curApp);
			root.SetAttribute(EasyStudioPreferencesXML.Element.CurrentModule, curMod);
			if (isDefault)
			{
				root.SetAttribute(EasyStudioPreferencesXML.Element.DefaultContextApplication, curApp);
				root.SetAttribute(EasyStudioPreferencesXML.Element.DefaultContextModule, curMod);
			}
			return PathFinder.PathFinderInstance.SaveTextFileFromXml(PreferencesFullFileName, doc);
		}

		//---------------------------------------------------------------
		internal string GetCurrentContext(string user)
		{
			if (!ExistPreferencesFile())
			{
				CreatePreferences();
			}
			try
			{
				XmlDocument doc = new XmlDocument();
				var f = GetPreferencesFile();
				doc.Load(PreferencesFullFileName);
				XmlElement root = doc.DocumentElement;
				if (root == null) return "";
				var res = root.GetAttribute(EasyStudioPreferencesXML.Element.CurrentApplication) +	";" +
					root.GetAttribute(EasyStudioPreferencesXML.Element.CurrentModule);
				if (res == ";")
					return null;
				return res;
			}
			catch (Exception)
			{
				return "";
			}


		}

		//---------------------------------------------------------------
		private bool CreatePreferences()
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement(EasyStudioPreferencesXML.Element.ESPreferences);
			doc.AppendChild(root);
			return PathFinder.PathFinderInstance.SaveTextFileFromXml(PreferencesFullFileName, doc);
		}

		//---------------------------------------------------------------
		public bool ExistPreferencesFile()
		{
			return PathFinder.PathFinderInstance.ExistFile(PreferencesFullFileName);
		}



	}
}
