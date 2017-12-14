using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Microarea.Snap.Core
{
    public class SettingsLoader : ISettingsLoader
    {
        const string settingsFileName = "Settings.yml";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public Settings Load()
        {
            var settingsFileInfo = new FileInfo(Path.Combine(Settings.ConfigRootFolderPath, settingsFileName));

            if (!settingsFileInfo.Exists)
            {
                var settings = new Settings();
                Save(settings);
                return settings;
            }

            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            using (var inputStream = settingsFileInfo.OpenRead())
            using (var streamReader = new StreamReader(inputStream))
            {
                return deserializer.Deserialize<Settings>(streamReader);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void Save(ISettings settings)
        {
            var settingsFileInfo = new FileInfo(Path.Combine(Settings.ConfigRootFolderPath, settingsFileName));
            var settingsDirInfo = settingsFileInfo.Directory;
            if (!settingsDirInfo.Exists)
            {
                settingsDirInfo.Create();
            }

            var serializer = new SerializerBuilder()
                .DisableAliases()
                .EmitDefaults()
                .Build();

            using (var outputStream = System.IO.File.Create(settingsFileInfo.FullName))
            using (var streamWriter = new StreamWriter(outputStream))
            {
                serializer.Serialize(streamWriter, settings);
            }
        }
    }
}
