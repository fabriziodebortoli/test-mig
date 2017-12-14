using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Core.NameSolver;

using Microarea.Library.Licence;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;
using Microarea.TaskBuilderNet.Core.Generic;

namespace SecurityObjectsReleaseCompare
{
	public class ProviderNoInstallation : IConfigurationInfoProvider
	{
		private readonly string product2build;
		private readonly CompressedFile zip;
		private readonly string countryIsoCode;

		private string solFolderRelPath;
		private string solFileRelPath;
		private string modulesFolderRelPath;

		//---------------------------------------------------------------------
        public ProviderNoInstallation(string product2build, CompressedFile zip, string countryIsoCode)
		{
			this.product2build = product2build;
			this.zip = zip;
			this.countryIsoCode = countryIsoCode;

			this.solFolderRelPath = "Applications/ERP/Solutions/";
			this.solFileRelPath = this.solFolderRelPath + product2build + ".Solution.xml";
			this.modulesFolderRelPath = this.solFolderRelPath + "Modules/";
		}

		//---------------------------------------------------------------------
		private XmlElement ExtractXmlElement(string relFilePath)
		{
			XmlDocument dom = ExtractXmlDocument(relFilePath);
			return dom != null ? dom.DocumentElement : null;
		}

		//---------------------------------------------------------------------
		private XmlDocument ExtractXmlDocument(string relFilePath)
		{
			CompressedEntry theEntry = zip.GetEntry(relFilePath.Replace('\\', '/'));

            if (theEntry == null)
                return null;

            Stream zipStream = theEntry.CurrentStream;
			if (zipStream == null)
				return null;
			XmlDocument dom = new XmlDocument();
			dom.Load(zipStream);
			return dom;
		}

		//---------------------------------------------------------------------
		private XmlDocument ExtractEncryptedXmlDocument(string relFilePath)
		{
			CompressedEntry theEntry = zip.GetEntry(relFilePath);
            Stream zipStream = theEntry.CurrentStream;
			if (zipStream == null)
				return null;
			int size = (int)theEntry.Size;//todo rischio!
			string tmpPath = Path.GetTempFileName();
			ExtractFile(zipStream, tmpPath);
			Microarea.Library.SMHandlerOff.HandlerOff reader = new Microarea.Library.SMHandlerOff.HandlerOff();
			string articleString = reader.GetArticleStringByPath(tmpPath);
			File.Delete(tmpPath);
			if (articleString == null || articleString == String.Empty)
				return null;
			XmlDocument dom = new XmlDocument();
			dom.LoadXml(articleString);

			return dom;
		}

		//---------------------------------------------------------------------
		private void ExtractFile(Stream zipInputStream, string fullFileName)
		{
			using (FileStream streamWriter = File.Create(fullFileName))
			{

				int size = 2048;
				byte[] data = new byte[2048];
				while (true)
				{
					size = zipInputStream.Read(data, 0, data.Length);
					if (size > 0)
						streamWriter.Write(data, 0, size);
					else
						break;
				}
			}
		}

		#region IConfigurationInfoProvider Members

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.AddFunctional
		{
			get { return true; }
			set { }
		}

		//---------------------------------------------------------------------
		string[] IConfigurationInfoProvider.GetProductNames()
		{
			return new string[] { this.product2build };
		}

		//---------------------------------------------------------------------
		XmlElement IConfigurationInfoProvider.GetProductLicensed(string product)
		{
			return null; // it's ok to return null
		}

		//---------------------------------------------------------------------
		Hashtable IConfigurationInfoProvider.GetArticles(string product)
		{
			Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

			XmlElement solEl = ExtractXmlElement(this.solFileRelPath);
			foreach (XmlElement artEl in solEl.GetElementsByTagName(WceStrings.Element.SalesModule))
			{
				string artName = artEl.GetAttribute(WceStrings.Attribute.Name);
				string artRelPath = this.modulesFolderRelPath + artName + NameSolverStrings.CsmExtension;
				XmlDocument artDoc = ExtractEncryptedXmlDocument(artRelPath);
				ht[artName] = artDoc;
			}

			return ht;
		}

		//---------------------------------------------------------------------
		string IConfigurationInfoProvider.GetCountry()
		{
			return this.countryIsoCode;
		}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.ArticlesLicensedByDefault { get { return false; } }

		//---------------------------------------------------------------------
		XmlElement IConfigurationInfoProvider.GetProductSolution(string product)
		{
			return ExtractXmlElement(this.solFileRelPath);
		}

		//---------------------------------------------------------------------
		UserInfo IConfigurationInfoProvider.GetUserInfo()
		{
			UserInfo userInfo = new UserInfo();
			userInfo.Country = this.countryIsoCode;
			return userInfo;
		}

		//---------------------------------------------------------------------
		void IConfigurationInfoProvider.InvalidateCaches()
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------
		BasePathFinder IConfigurationInfoProvider.GetPathFinder()
		{
			return null;
		}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.FilterByCountry { get { return true; } }

		//---------------------------------------------------------------------
		string IConfigurationInfoProvider.GetProductRelease(string product)
		{
			return null;
		}

		#endregion
	}
}
