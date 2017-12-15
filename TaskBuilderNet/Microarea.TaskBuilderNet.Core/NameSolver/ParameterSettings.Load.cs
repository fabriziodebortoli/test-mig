﻿namespace Microarea.TaskBuilderNet.Core.NameSolver
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System;

    //=========================================================================
    /// <summary>
    /// ParameterSettings
    /// </summary>
    public partial class ParameterSettings
    {
        public const string ColorSectionName = "Colors";

        //---------------------------------------------------------------------
        public static ParameterSettings Load(string themeFilePath)
        {
            if (!File.Exists(themeFilePath))
            {
                return null;
            }
            using (Stream inputStream = File.OpenRead(themeFilePath))
            {
                using (XmlReader xmlReader = XmlReader.Create(inputStream))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ParameterSettings));
                    return serializer.Deserialize(xmlReader) as ParameterSettings;
                }
            }
        }
    }
}