using System;
using System.IO;
using System.Xml;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Responsible for creation of addons database objects files.
	/// </summary>
	//================================================================================
	public class CreateInfoWriter
	{
		private string mFilename = string.Empty;
		private string mAllPath = string.Empty;
		private string mOraclePath = string.Empty;
		private string mCreatePath = string.Empty;
		private string mModule = string.Empty;

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Initilize a new instance of CreateInfoWriter
		/// </summary>
		/// <param name="aDatabaseScript">The database script</param>
		/// <param name="aModule">The owner module</param>
		public CreateInfoWriter(string aDatabaseScript, string aModule)
		{
			mCreatePath = Path.Combine(aDatabaseScript, "Create");
			mFilename = Path.Combine(mCreatePath, "CreateInfo.xml");
			mAllPath = Path.Combine(mCreatePath, "All");
			mOraclePath = Path.Combine(mCreatePath, "Oracle");
			mModule = aModule;
		}

		//--------------------------------------------------------------------------------
		internal string CreateScriptPath
		{
			get { return mCreatePath; }
		}

		//--------------------------------------------------------------------------------
		internal string CreateScriptPathAll
		{
			get { return mAllPath; }
		}

		//--------------------------------------------------------------------------------
		internal string CreateScriptPathOracle
		{
			get { return mOraclePath; }
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Returns the name of the file
		/// </summary>
		/// <returns>The name of the file</returns>
		public string GetFilename()
		{
			return mFilename;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Adds a sql script.
		/// </summary>
		/// <param name="aSQLScriptFilename">The name of the file</param>
		/// <param name="dependsUponApp">The name of the application this one depends on</param>
		/// <param name="dependsUponMod">The name of the module this one depends on</param>
		/// <returns>The creation step</returns>
		public int AddSQLScript(string aSQLScriptFilename, string dependsUponApp, string dependsUponMod)
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xmlNode = null;
			XmlElement xmlElement = null;


			xmlDoc.Load(mFilename);
			int aStep;
			if (GetStep(xmlDoc, aSQLScriptFilename, out aStep))
				return aStep;

			xmlNode = xmlDoc.SelectSingleNode("CreateInfo/Level1");
			xmlElement = xmlDoc.CreateElement("Step");
			xmlElement.SetAttribute("numstep", aStep.ToString());
			xmlElement.SetAttribute("script", aSQLScriptFilename);
			xmlNode.AppendChild(xmlElement);

			if (!string.IsNullOrEmpty(dependsUponApp) && !string.IsNullOrEmpty(dependsUponMod))
			{
				xmlElement = (XmlElement)xmlElement.AppendChild(xmlDoc.CreateElement("Dependency"));
				xmlElement.SetAttribute("app", dependsUponApp);
				xmlElement.SetAttribute("module", dependsUponMod);
			}
			Save(xmlDoc);
			return aStep;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Creates e new database script.
		/// </summary>
		public void New()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xmlNode = null;
			XmlElement xmlElement = null;

			xmlElement = xmlDoc.CreateElement("CreateInfo");
			xmlNode = xmlDoc.AppendChild(xmlElement);

			xmlElement = xmlDoc.CreateElement("ModuleInfo");
			xmlElement.SetAttribute("name", mModule);
			xmlNode.AppendChild(xmlElement);

			xmlElement = xmlDoc.CreateElement("Level1");
			xmlNode.AppendChild(xmlElement);

			xmlElement = xmlDoc.CreateElement("Level2");
			xmlNode.AppendChild(xmlElement);
			Save(xmlDoc);
		}

		//--------------------------------------------------------------------------------
		private void Save(XmlDocument xmlDoc)
		{
			string path = System.IO.Path.GetDirectoryName(mFilename);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			xmlDoc.Save(mFilename);
			BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(mFilename);
		}

		//--------------------------------------------------------------------------------
		private bool GetStep(XmlDocument xmlDoc, string aSQLScriptFilename, out int aStep)
		{
			XmlNodeList xmlList = null;
			aStep = 0;
			xmlList = xmlDoc.SelectNodes("/CreateInfo/Level1/Step");

			for (int i = 0; i < xmlList.Count; i++)
			{
				if (xmlList[i].Attributes["script"].Value == aSQLScriptFilename)
				{
					aStep = Convert.ToInt32(xmlList[i].Attributes["numstep"].Value);
					return true;
				}
			}
			aStep = xmlList.Count + 1;
			return false;
		}
	}
}
