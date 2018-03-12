using Microarea.Common.NameSolver;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using TaskBuilderNetCore.Common.CustomAttributes;
using TaskBuilderNetCore.EasyStudio.Interfaces;
using TaskBuilderNetCore.Interfaces;

namespace TaskBuilderNetCore.EasyStudio.Services
{
	//====================================================================
	[Name("prefSvc"), Description("This service manages preferences structure info and serialization.")]
	public class PreferencesService : Service
	{
		//---------------------------------------------------------------
		public string PreferencesFullFileName
		{
			get
			{
				return Path.Combine(
					PathFinder.GetEasyStudioHomePath(),
					EasyStudioPreferencesXML.Element.ESPreferences +
					NameSolverStrings.XmlExtension);
			}
		}


		//---------------------------------------------------------------
		public Stream GetPreferencesFile()
		{
			try
			{
				Stream stream = PathFinder.GetStream(PreferencesFullFileName, true);

				stream.Position = 0;
				return stream;

			}
			catch (Exception)
			{
				return null;
			}
		}

		//---------------------------------------------------------------
		public bool SetPreferences(string curApp, string curMod, bool isDefault)
		{
			try
			{
				if (!ExistPreferencesFile())
				{
					CreatePreferences();
				}
				XmlElement root = null;

				XmlDocument doc = new XmlDocument();
				doc = PathFinder.PathFinderInstance.LoadXmlDocument(doc, PreferencesFullFileName);
				root = doc.DocumentElement;

				XmlElement currentContext = (XmlElement)root.SelectSingleNode(EasyStudioPreferencesXML.Element.CurrentContext);
				if (currentContext == null)
					return false;
				currentContext.SetAttribute(EasyStudioPreferencesXML.Attribute.CurrentApplication, curApp);
				currentContext.SetAttribute(EasyStudioPreferencesXML.Attribute.CurrentModule, curMod);

				if (isDefault)
				{
					XmlElement defaultContext = (XmlElement)root.SelectSingleNode(EasyStudioPreferencesXML.Element.DefaultContext);
					if (defaultContext == null)
						return false;
					defaultContext.SetAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextApplication, curApp);
					defaultContext.SetAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextModule, curMod);
				}

				string writepath = PreferencesFullFileName;
				Encoding enc = new UTF8Encoding(false);
				doc.Save(writepath);
				return true;

			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------
		public string GetCurrentContext(string user, bool getDefault)
		{
			if (!ExistPreferencesFile())
			{
				CreatePreferences();
			}
			try
			{
				XmlDocument doc = new XmlDocument();
				if (File.Exists(PreferencesFullFileName))
				{
					try
					{
						doc.Load(PreferencesFullFileName);
					}
					catch { }
				}
				
				XmlElement root = doc.DocumentElement;
				if (root == null) return "";
				string result = string.Empty;
				if (getDefault)
				{
					result = root.GetAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextApplication) + ";" +
					root.GetAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextModule);
				}
				else result = root.GetAttribute(EasyStudioPreferencesXML.Attribute.CurrentApplication) + ";" +
					root.GetAttribute(EasyStudioPreferencesXML.Attribute.CurrentModule);
				doc = null;
				root = null;
				if (result == ";")
					return null;
				return result;
			}
			catch (Exception)
			{
				return "";
			}
		}

		//---------------------------------------------------------------
		private bool CreatePreferences()
		{

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = false;
			if (!File.Exists(PreferencesFullFileName))
			{
				using (XmlWriter writer = XmlWriter.Create(PreferencesFullFileName, settings))
				{
					writer.WriteStartDocument(true);
					writer.WriteStartElement(EasyStudioPreferencesXML.Element.ESPreferences);
					writer.WriteStartElement(EasyStudioPreferencesXML.Element.CurrentContext);
					writer.WriteStartAttribute(EasyStudioPreferencesXML.Attribute.CurrentApplication);
					writer.WriteValue(null);
					writer.WriteEndAttribute();
					writer.WriteStartAttribute(EasyStudioPreferencesXML.Attribute.CurrentModule);
					writer.WriteValue(null);
					writer.WriteEndAttribute();
					writer.WriteEndElement();
					writer.WriteStartElement(EasyStudioPreferencesXML.Element.DefaultContext);
					writer.WriteStartAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextApplication);
					writer.WriteValue(null);
					writer.WriteEndAttribute();
					writer.WriteStartAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextModule);
					writer.WriteValue(null);
					writer.WriteEndAttribute();
					writer.WriteEndElement();
					writer.Flush();
					writer.Dispose();
				}
			}
			return true;

			/*XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement(EasyStudioPreferencesXML.Element.ESPreferences);
			var n1 = doc.CreateElement(EasyStudioPreferencesXML.Element.CurrentContext);
			var n2 = doc.CreateElement(EasyStudioPreferencesXML.Element.DefaultContext);
			n1.SetAttribute(EasyStudioPreferencesXML.Attribute.CurrentApplication, null);
			n1.SetAttribute(EasyStudioPreferencesXML.Attribute.CurrentModule, null);
			n2.SetAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextApplication, null);
			n2.SetAttribute(EasyStudioPreferencesXML.Attribute.DefaultContextModule, null);
			root.AppendChild(n1);
			root.AppendChild(n2);

			doc.AppendChild(root);
			return PathFinder.SaveTextFileFromXml(PreferencesFullFileName, doc);*/
		}

		//---------------------------------------------------------------
		public bool ExistPreferencesFile()
		{
			return PathFinder.ExistFile(PreferencesFullFileName);
		}
	}
}

