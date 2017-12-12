using System;
using System.Globalization;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.MenuManager
{
    //=========================================================================
    /// <summary>
    /// Classe che incapsula le funzionalita` per gestire pacchettini da 
    /// deploy-are sui client via ClickOnce e da scompattare 
    /// successivamente in loco all'avvio dell'applicazione.
    /// </summary>
    public class Payload
    {
        const string timestampFileName = "timestamp";

        DateTime? payloadTimestamp;

        //---------------------------------------------------------------------
        public string FilePath      { get; private set; }
        public string FileName      { get; private set; }
        public string Name          { get; private set; }
        public bool IsValid         { get; private set; }

        //---------------------------------------------------------------------
        public Payload(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Name = Path.GetFileNameWithoutExtension(filePath);

            bool timestampFound = false;
            using (var compressedFile = new CompressedFile(FilePath, CompressedFile.OpenMode.Read))
            {
                foreach (var compressedEntry in compressedFile.GetAllEntries())
	            {
                    if (String.Compare(Path.GetFileName(compressedEntry.Name), timestampFileName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        payloadTimestamp = ReadTimestampFileContent(compressedEntry.CurrentStream);
                        if (payloadTimestamp != null)
                        {
                            timestampFound = true;
                        }
                        break;
                    }
	            }
            }

            IsValid = timestampFound;
        }

        //---------------------------------------------------------------------
        DateTime? ReadTimestampFileContent(string timestampFilePath)
        {
            if (!File.Exists(timestampFilePath))
            {
                return null;
            }
            using (Stream str = File.OpenRead(timestampFilePath))
            {
                return ReadTimestampFileContent(str);
            }
        }

        //---------------------------------------------------------------------
        DateTime? ReadTimestampFileContent(Stream timestampFileStream)
        {
            try
            {
                string valueOfThefile = new StreamReader(timestampFileStream).ReadToEnd();

                return DateTime.ParseExact(valueOfThefile, "yyyyMMddTHHmm", CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------
        public void InstallOrUpdate(string where)
        {
            if (!IsAlreadyInstalled(where))
            {
                Install(where);
                return;
            }

            DateTime? installedTimestamp = ReadTimestampFileContent(Path.Combine(where, timestampFileName));
            if (installedTimestamp == null || installedTimestamp.Value < this.payloadTimestamp)
            {
                Update(where);
            }
        }

        //---------------------------------------------------------------------
        private bool IsAlreadyInstalled(string where)
        {
            DirectoryInfo outputDirInfo = new DirectoryInfo(Path.Combine(where, Name));

            return outputDirInfo.Exists;
        }

        //---------------------------------------------------------------------
        private void Update(string where)
        {
            Uninstall(where);
            Install(where);
        }

        //---------------------------------------------------------------------
        private void Install(string where)
        {
            using (var compressedFile = new CompressedFile(FilePath, CompressedFile.OpenMode.Read))
            {
                compressedFile.ExtractAll(where);
            }
        }

        //---------------------------------------------------------------------
        private void Uninstall(string where)
        {
            string toBeDeletedFolder = Path.Combine(where, Name);
            Directory.Delete(toBeDeletedFolder, true);
        }
    }
}
