using Newtonsoft.Json;
using System.IO;

namespace Microarea.Common.NameSolver
{
    //=========================================================================
    public class EasyStudioSettings
    {
        bool    customizationsInCustom = true;
        string  homeName = "ESHome";
        string  templatesName = "Templates";

        //-----------------------------------------------------------------------------
        public string HomeName { get { return homeName; } set { homeName = value; } }
        //-----------------------------------------------------------------------------
        public bool CustomizationsInCustom { get => customizationsInCustom; set => customizationsInCustom = value; }

        //-------------------------------------------------------------------------------
        public string TemplatesName { get { return templatesName; } }
        //-----------------------------------------------------------------------------
        public EasyStudioSettings()
        {
        }
    }

    //=========================================================================
    public class EasyStudioConfiguration
    {
        string fileName = "EasyStudio.json";
        private EasyStudioSettings settings;

        //-----------------------------------------------------------------------------
        public EasyStudioSettings Settings { get => settings; set => settings = value; }

        //-----------------------------------------------------------------------------
        public EasyStudioConfiguration(PathFinder pathFinder)
        {
            string fullFileName = Path.Combine(pathFinder.GetCustomPath(), fileName);
            Settings = new EasyStudioSettings();

            if (File.Exists(fullFileName))
            {
                using (StreamReader r = new StreamReader(fullFileName))
                    JsonConvert.PopulateObject(r.ReadToEnd(), Settings);
            }
         }
    }
}
