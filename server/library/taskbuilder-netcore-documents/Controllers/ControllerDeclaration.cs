using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TaskBuilderNetCore.Documents.Controllers
{
    //====================================================================================    
    public class ControllerDeclaration
    {
        string type;

        public string Type { get => type; set => type = value; }
    }

    //====================================================================================    
    public class ControllersDeclaration
    {
        static string configurationFileName = "orchestrator.settings.json";

        List<ControllerDeclaration> controllers;

        public List<ControllerDeclaration> Controllers { get => controllers; set => controllers = value; }

        //-----------------------------------------------------------------------------------------------------
        public static ControllersDeclaration LoadFromDefaultFile()
        {
            string settingFile = Path.Combine(Environment.CurrentDirectory, configurationFileName);
            if (!File.Exists(settingFile))
                return null;

            return JsonConvert.DeserializeObject<ControllersDeclaration>(File.ReadAllText(settingFile));
        }
    }
}
