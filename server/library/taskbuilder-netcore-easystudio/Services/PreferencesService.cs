using Microarea.Common.NameSolver;
using System;
using System.ComponentModel;
using System.IO;
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
		public string GetPreferencesFullFileName(string user)
		{
			return Path.Combine(
				PathFinder.GetEasyStudioHomePath(),
				user + "." +
				EasyStudioPreferencesXML.Element.ESPreferences +
				NameSolverStrings.XmlExtension);
		}

		/*//---------------------------------------------------------------
		public Stream GetPreferencesFile(string user)
		{
			try
			{
				Stream stream = PathFinder.GetStream(GetPreferencesFullFileName(user), true);
				stream.Position = 0;
				return stream;
			}
			catch (Exception)
			{
				return null;
			}
		}*/

		//---------------------------------------------------------------
		public bool SetContextPreferences(string curApp, string curMod, bool isDefault, string user)
		{
			try
			{
				if (!ExistPreferencesFile(user))
				{
					if (!CreatePreferences(user))
						return false;
				}

				XmlDocument doc = new XmlDocument();
				doc = Services.PathFinder.LoadXmlDocument(doc, GetPreferencesFullFileName(user));
				XmlElement root = doc.DocumentElement;
				root= SetCurrentContext(root, curApp, curMod);
				if (isDefault)
					root = SetDefaultContext(root, curApp, curMod);
				doc.RemoveAll();
				doc.AppendChild(root);
				return Services.PathFinder.SaveTextFileFromXml(GetPreferencesFullFileName(user), doc);
			}
			catch
			{
				return false;
			}
		}

		//---------------------------------------------------------------
		public string GetContextPreferences(string user, bool getDefault)
		{
			if (!ExistPreferencesFile(user))
			{
				CreatePreferences(user);
				return null;
			}
			try
			{
				XmlDocument doc = new XmlDocument();
				doc = Services.PathFinder.LoadXmlDocument(doc, GetPreferencesFullFileName(user));
				var root = doc.DocumentElement;
				if (root == null) return null;
				string result = getDefault ? GetDefaultContext(root) : GetCurrentContext(root);
				doc = null;
				root = null;

				return (result == ";") ? null : result;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		//---------------------------------------------------------------
		private string GetDefaultContext(XmlElement root)
		{
			XmlElement defaultContext = (XmlElement)root.SelectSingleNode(EasyStudioPreferencesXML.Element.DefaultContext);
			var a = defaultContext.GetAttribute(EasyStudioPreferencesXML.Attribute.DefaultApplication);
			var b = defaultContext.GetAttribute(EasyStudioPreferencesXML.Attribute.DefaultModule);
			return a + Strings.Separator + b;
		}

		//---------------------------------------------------------------
		private string GetCurrentContext(XmlElement root)
		{
			XmlElement currentContext = (XmlElement)root.SelectSingleNode(EasyStudioPreferencesXML.Element.CurrentContext);
			var a = currentContext.GetAttribute(EasyStudioPreferencesXML.Attribute.CurrentApplication);
			var b = currentContext.GetAttribute(EasyStudioPreferencesXML.Attribute.CurrentModule);
			return a + Strings.Separator + b;
		}

		//---------------------------------------------------------------
		private XmlElement SetDefaultContext(XmlElement root, string app, string mod)
		{
			XmlElement defaultContext = (XmlElement)root.SelectSingleNode(EasyStudioPreferencesXML.Element.DefaultContext);
			if (defaultContext == null)
				return null;
			defaultContext.SetAttribute(EasyStudioPreferencesXML.Attribute.DefaultApplication, app);
			defaultContext.SetAttribute(EasyStudioPreferencesXML.Attribute.DefaultModule, mod);
			return root;
		}

		//---------------------------------------------------------------
		private XmlElement SetCurrentContext(XmlElement root, string app, string mod)
		{
			XmlElement currentContext = (XmlElement)root.SelectSingleNode(EasyStudioPreferencesXML.Element.CurrentContext);
			if (currentContext == null)
				return null;
			currentContext.SetAttribute(EasyStudioPreferencesXML.Attribute.CurrentApplication, app);
			currentContext.SetAttribute(EasyStudioPreferencesXML.Attribute.CurrentModule, mod);
			return root;

		}

		//---------------------------------------------------------------
		private bool CreatePreferences(string user)
		{
			if (ExistPreferencesFile(user)) return false;

			/*XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = false
			};*/
			XmlDocument doc = new XmlDocument();
			using (XmlWriter writer = doc.CreateNavigator().AppendChild())
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
				writer.WriteStartAttribute(EasyStudioPreferencesXML.Attribute.DefaultApplication);
				writer.WriteValue(null);
				writer.WriteEndAttribute();
				writer.WriteStartAttribute(EasyStudioPreferencesXML.Attribute.DefaultModule);
				writer.WriteValue(null);
				writer.WriteEndAttribute();
				writer.WriteEndElement();
				writer.Flush();
				writer.Dispose();
			}

			return Services.PathFinder.SaveTextFileFromXml(GetPreferencesFullFileName(user), doc);

		}

		//---------------------------------------------------------------
		public bool ExistPreferencesFile(string user)
		{
			return Services.PathFinder.ExistFile(GetPreferencesFullFileName(user));
		}
	}
}

