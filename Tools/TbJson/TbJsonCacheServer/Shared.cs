using System;
using System.Collections.Generic;
using System.IO;

namespace SharedCode
{
    public static class Shared
    {
        internal static string namedPipe = "CLIENTFORMSPROVIDER_" + Path.GetFileName(GetInstallationFolder());
        internal const string clientFormsCommand = "clientforms";
        internal const string controlClassesCommand = "controlclasses";
        internal const string iconsMatchCommand = "iconsMatch";

        internal static string GetInstallationFolder()
        {
            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            while (path != null && !Path.GetFileName(path).Equals("standard", StringComparison.InvariantCultureIgnoreCase))
                path = Path.GetDirectoryName(path);
            return Path.GetDirectoryName(path);
        }
    }

    class ClientForm
    {
        public string name = "";
        public bool exclude = false;

        public ClientForm(string name, bool exclude)
        {
            this.name = name;
            this.exclude = exclude;
        }
    }

    class ClientFormMap : Dictionary<string, List<ClientForm>>
    {
    }

    class ControlClassMap : Dictionary<string, WebControl>
    {
    }

    class IconsMatchMap : Dictionary<string, string>
    {
    }

    public class ArgMap : Dictionary<string, string>
    {
    }
    public class PropertyMap : Dictionary<string, string>
    {
    }
    public class WebControl
    {
        public string Name { get; set; } = "";
        public ArgMap Args { get; set; } = new ArgMap();
        public PropertyMap Properties { get; set; } = new PropertyMap();
        public (string name, string value) Selector { get; set; } = ("", "");
        public string ColumnName { get; set; } = "";
        public WebControl(string name, string columnName = "")
        {
            Name = name;
            ColumnName = columnName;
        }
    }
}
