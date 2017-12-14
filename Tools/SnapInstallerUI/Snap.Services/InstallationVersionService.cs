using Microarea.Snap.Core;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microarea.Snap.Services
{
    internal class InstallationVersionService : IInstallationVersionService
    {
        ISettings settings;
        IFileSystemService fileSystemService;

        string productName;
        string version;

        public string ProductName
        {
            get
            {
                if (productName == null)
                {
                    Init();
                }
                return productName;
            }
        }
        public string Version
        {
            get
            {
                if (version == null)
                {
                    Init();
                }

                return version;
            }
        }

        public InstallationVersionService(ISettings settings, IFileSystemService fileSystemService)
        {
            this.settings = settings;
            this.settings.ProductInstanceFolderChanged += Settings_ProductInstanceFolderChanged;
            this.fileSystemService = fileSystemService;
        }

        private void Settings_ProductInstanceFolderChanged(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            var xmlDocument = new XmlDocument();
            using (var inputStream = this.fileSystemService.InstallationVersionFile.OpenRead())
            {
                xmlDocument.Load(inputStream);
            }

            Load(xmlDocument);
        }

        internal void Load(XmlDocument xmlDocument)
        {
            var xPathMask = "/InstallationVersion/{0}";

            var formatProvider = System.Globalization.CultureInfo.InvariantCulture;

            var element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "ProductName"));
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.productName = element.InnerText;
            }
            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "Version"));
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                Version v = new System.Version();
                if (!System.Version.TryParse(element.InnerText, out v))
                {
                    throw new ArgumentException("Invalid version from " + Path.Combine(this.settings.ProductInstanceFolder, "Standard", "Installation.ver") + ": " + element.InnerText);
                }
                this.version = v.ToString(3);
            }
        }

        public void InvalidateMenuCache()
        {
            var xmlDocument = new XmlDocument();
            using (var inputStream = this.fileSystemService.InstallationVersionFile.OpenRead())
            {
                xmlDocument.Load(inputStream);
            }

            var xPathMask = "/InstallationVersion/CacheDate";
            var element = xmlDocument.SelectSingleNode(xPathMask);
            if (element != null)
            {
                element.InnerText = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }

            using (var outputStream = this.fileSystemService.InstallationVersionFile.OpenWrite())
            {
                xmlDocument.Save(outputStream);
            }
        }
    }
}