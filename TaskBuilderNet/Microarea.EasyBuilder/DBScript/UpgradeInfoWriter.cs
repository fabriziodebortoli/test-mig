using System;
using System.IO;
using System.Xml;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for UpgradeInfoWriter.
	/// </summary>
	//================================================================================
	public class UpgradeInfoWriter
	{
		private string mFilename = string.Empty;
		private string mAllPath = string.Empty;
		private string mOraclePath = string.Empty;
		private string mModule = string.Empty;

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public UpgradeInfoWriter(string aDatabaseScript, string aModule)
		{
			mFilename = Path.Combine(aDatabaseScript, "Upgrade\\UpgradeInfo.xml");
			mAllPath = Path.Combine(aDatabaseScript, "Upgrade\\All");
			mOraclePath = Path.Combine(aDatabaseScript, "Upgrade\\Oracle");
			mModule = aModule;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string GetFilename()
		{
			return mFilename;
		}

		//--------------------------------------------------------------------------------
		internal string GetUpgradeScriptPathAll(int releaseNumber)
		{
			return Path.Combine(mAllPath, "Release_" + releaseNumber.ToString());
		}
		//--------------------------------------------------------------------------------
		internal string GetUpgradeScriptPathOracle(int releaseNumber)
		{
			return Path.Combine(mOraclePath, "Release_" + releaseNumber.ToString());
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void AddSQLScript(string aSQLScriptFilename, int aReleaseNumber, string dependsUponApp, string dependsUponMod)
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xmlNode = null;
			XmlElement xmlElement = null;

			xmlDoc.Load(mFilename);

			int aStep = GetStep(xmlDoc, aSQLScriptFilename, aReleaseNumber);

			if (aStep < 0)
				return;

			if (aStep == 0)
				return;

			xmlNode = xmlDoc.SelectSingleNode("UpgradeInfo/DBRel[@numrel=\"" + aReleaseNumber.ToString() + "\"]/Level1");

			if (xmlNode == null)
			{
				xmlNode = xmlDoc.SelectSingleNode("UpgradeInfo");
				xmlElement = xmlDoc.CreateElement("DBRel");
				xmlElement.SetAttribute("numrel", aReleaseNumber.ToString());
				XmlNode previousEl = GetPreviousDBRelNode(xmlDoc, aReleaseNumber);
				xmlNode = xmlNode.InsertAfter(xmlElement, previousEl);
				xmlElement = xmlDoc.CreateElement("Level1");
				xmlNode = xmlNode.AppendChild(xmlElement);
			}
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
		}

		//--------------------------------------------------------------------------------
		private XmlNode GetPreviousDBRelNode(XmlDocument xmlDoc, int aReleaseNumber)
		{
			XmlElement node = null;
			int max = 0;
			foreach (XmlElement el in xmlDoc.SelectNodes("UpgradeInfo/DBRel"))
			{
				int rel;
				int.TryParse(el.GetAttribute("numrel"), out rel);

				if (rel > aReleaseNumber)
					continue;
				if (rel > max)
					node = el;
			}

			return node;

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
		/// <remarks/>
		public void New()
		{
			XmlDocument xmlDoc = new XmlDocument();
			XmlNode xmlNode = null;
			XmlElement xmlElement = null;

			xmlElement = xmlDoc.CreateElement("UpgradeInfo");
			xmlNode = xmlDoc.AppendChild(xmlElement);

			xmlElement = xmlDoc.CreateElement("ModuleInfo");
			xmlElement.SetAttribute("name", mModule);
			xmlNode.AppendChild(xmlElement);

			Save(xmlDoc);
		}
		//--------------------------------------------------------------------------------
		private int GetStep(XmlDocument xmlDoc, string aSQLScriptFilename, int aReleaseNumber)
		{
			XmlNodeList xmlList = null;

			int aStep = 0;
			xmlList = xmlDoc.SelectNodes("UpgradeInfo/DBRel[@numrel=\"" + aReleaseNumber.ToString() + "\"]/Level1/Step");

			for (int i = 0; i < xmlList.Count; i++)
			{
				if (xmlList[i].Attributes["script"].Value == aSQLScriptFilename)
					return 0;

				if (Convert.ToInt32(xmlList[i].Attributes["numstep"].Value) > aStep)
					aStep = Convert.ToInt32(xmlList[i].Attributes["numstep"].Value);
			}
			return ++aStep;
		}
	}
}
