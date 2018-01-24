using Microarea.Common.NameSolver;
using System.IO;
using System.Xml;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.StringLoader
{
    /// <summary>
    /// Summary description for WoormLocalizer.
    /// </summary>
    //================================================================================
    public class WoormLocalizer : ILocalizer
    {
        private const string report = "report";
        private PathFinder pf = null;
        private DictionaryStringBlock dictionary;
        private string dictionaryPath;


        //---------------------------------------------------------------------
        public WoormLocalizer(XmlDocument xmlDictionary) : this(xmlDictionary, null, null) { }

        //---------------------------------------------------------------------
        public WoormLocalizer(XmlDocument xmlDictionary, string filePath, PathFinder pf)
        {
            this.pf = pf;
            this.dictionary = Helper.RetrieveStrings(xmlDictionary);

            if (filePath == null || pf == null) return;

            dictionaryPath = pf.GetDictionaryFilePathFromWoormFile(filePath);
        }

        //---------------------------------------------------------------------
        public WoormLocalizer(string filePath, PathFinder pf)
        {
            Build(filePath);
        }

        //---------------------------------------------------------------------
        public void Build(string filePath)
        {
            if (filePath == null || filePath == string.Empty || pf == null) return;

            dictionaryPath = pf.GetDictionaryFilePathFromWoormFile(filePath);

            dictionary = StringLoader.GetDictionary(dictionaryPath, GlobalConstants.report, Path.GetFileNameWithoutExtension(filePath), GlobalConstants.report);
        }

        //-------------------------------------------------------- -------------
        public string Translate(string baseString)
        {
            if (string.IsNullOrEmpty(baseString))
                return null;

            if (dictionary == null) return baseString;

            return Helper.FindString(dictionary, baseString);
        }
    }

}
