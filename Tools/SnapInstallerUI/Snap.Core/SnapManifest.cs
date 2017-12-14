using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microarea.Snap.Core
{
    internal class SnapManifest : IManifest
    {
        public virtual string Description
        {
            get;
            private set;
        } = string.Empty;

        public virtual Uri IconUrl
        {
            get;
            private set;
        }

        public virtual string Id
        {
            get;
            private set;
        } = string.Empty;

        public virtual string Producer
        {
            get;
            private set;
        } = string.Empty;

        public virtual string Product
        {
            get;
            private set;
        } = string.Empty;

        public virtual string ProductVersion
        {
            get;
            private set;
        } = string.Empty;

        public virtual Uri ReleaseNotesUrl
        {
            get;
            private set;
        }

        public virtual string Title
        {
            get;
            private set;
        } = string.Empty;

        public virtual int Version
        {
            get;
            private set;
        }

        public virtual ICollection<IFile> Files { get; } = new List<IFile>();

        private void Init(XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
            {
                throw new ArgumentNullException("xmlDocument");
            }
            var xPathMask = "/snap:package/snap:metadata/snap:{0}";

            var nsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            nsManager.AddNamespace("snap", "http://schemas.microarea.it/snapinstaller");

            var formatProvider = System.Globalization.CultureInfo.InvariantCulture;

            var element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "id"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.Id = element.InnerText;
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "version"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.Version = int.Parse(element.InnerText, System.Globalization.CultureInfo.InvariantCulture);
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "title"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.Title = element.InnerText;
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "iconUrl"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.IconUrl = new Uri(element.InnerText);
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "producer"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.Producer = element.InnerText;
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "product"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.Product = element.InnerText;
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "productversion"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.ProductVersion = element.InnerText;
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "description"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.Description = element.InnerText;
            }

            element = xmlDocument.SelectSingleNode(string.Format(formatProvider, xPathMask, "releaseNotesUrl"), nsManager);
            if (element != null && !string.IsNullOrWhiteSpace(element.InnerText))
            {
                this.ReleaseNotesUrl = new Uri(element.InnerText);
            }

            element = xmlDocument.SelectSingleNode("/snap:package/snap:files", nsManager);
            if (element != null)
            {
                var fileElements = element.SelectNodes("/snap:package/snap:files/snap:file", nsManager);
                foreach (XmlNode fileElement in fileElements)
                {
                    this.Files.Add(IocFactory.Instance.GetInstance<IFile, string>(fileElement.InnerText));
                }
            }
        }

        public virtual void Init(Stream stream)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(stream);

            this.Init(xmlDocument);
        }

        public virtual void Init(string manifestContent)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(manifestContent);

            this.Init(xmlDocument);
        }
    }
}
