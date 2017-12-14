using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.StringLoader
{
	/// <summary>
	/// Summary description for WoormLocalizer.
	/// </summary>
	//================================================================================
	public class WoormLocalizer : ILocalizer
	{
		private const string report = "report";

		private DictionaryStringBlock dictionary;
		private string dictionaryPath;
		
		
		//---------------------------------------------------------------------
		public WoormLocalizer (XmlDocument xmlDictionary) : this (xmlDictionary, null, null) {}

		//---------------------------------------------------------------------
		public WoormLocalizer (XmlDocument xmlDictionary, string filePath, IBasePathFinder pf)
		{
			this.dictionary = Helper.RetrieveStrings(xmlDictionary);

			if (filePath == null || pf == null) return;
				
			dictionaryPath = pf.GetDictionaryFilePathFromWoormFile(filePath);
		}

		//---------------------------------------------------------------------
		public WoormLocalizer (string filePath, IBasePathFinder pf)
		{
            Build (filePath, pf);
		}

        //---------------------------------------------------------------------
        public void Build(string filePath, IBasePathFinder pf)
        {
            if (filePath == null || filePath == string.Empty || pf == null) return;

            dictionaryPath = pf.GetDictionaryFilePathFromWoormFile(filePath);

            dictionary = StringLoader.GetDictionary(dictionaryPath, GlobalConstants.report, Path.GetFileNameWithoutExtension(filePath), GlobalConstants.report);
        }

		//---------------------------------------------------------------------
		public string Translate(string baseString)
		{
			if (string.IsNullOrEmpty (baseString))
				return null;
			
			if (dictionary == null) return baseString;

			return Helper.FindString(dictionary, baseString);
		}
	}
	
}
