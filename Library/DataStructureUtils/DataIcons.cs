using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace Microarea.Library.DataStructureUtils
{
    public class DataIcons
    {
        private static Dictionary<string, Icon> icons = null;
        //--------------------------------------------------------------------------------------------------------------------------------
        private static Icon GetIconByName(string iconName)
        { 
            if (String.IsNullOrEmpty(iconName))
                return null;

            if (icons != null && icons.ContainsKey(iconName))
                return icons[iconName];

            Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Library.DataStructureUtils.Icons." + iconName + ".ico");
            if (iconStream == null)
                return null;

            Icon icon = new Icon(iconStream);
            if (icons == null)
                icons = new Dictionary<string, Icon>();
            icons.Add(iconName, icon);

            return icon;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon BlueBrick
        {
            get { return GetIconByName("BlueBrick"); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon Bricks
        {
            get { return GetIconByName("Bricks"); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon Database
        {
            get { return GetIconByName("Database"); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon DataStructureBinFile
        {
            get { return GetIconByName("DataStructureBinFile"); }
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon DataStructureFile
        {
            get { return GetIconByName("DataStructureFile"); }
        }
       
        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon DBScript
        {
            get { return GetIconByName("DBScript"); }
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon GreenBrick
        {
            get { return GetIconByName("GreenBrick"); }
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon ModuleDBObjs
        {
            get { return GetIconByName("ModuleDBObjs"); }
        }
        
        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon OracleScript
        {
            get { return GetIconByName("OracleScript"); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon RedBrick
        {
            get { return GetIconByName("RedBrick"); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon SQLServerScript
        {
            get { return GetIconByName("SQLServerScript"); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static Icon YellowBrick
        {
            get { return GetIconByName("YellowBrick"); }
        }
    }
}
