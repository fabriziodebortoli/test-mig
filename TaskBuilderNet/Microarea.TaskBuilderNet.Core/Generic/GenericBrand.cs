using Microarea.TaskBuilderNet.Core.NameSolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microarea.TaskBuilderNet.Core.Generic
{
    public class GenericBrand
    {
        public string ProductTitle { get; private set; } = "Mago4";
        public string ProductPublisher { get; set; } = "Microarea";
        public string DigitalSignatureUrl { get; private set; } = "http://www.microarea.it/DigitalSigner/DigitalSigner.asmx";
        public string DigitalSignatureBackupUrl { get; private set; } = "http://ping.microarea.eu/DigitalSigner/DigitalSigner.asmx";

        public string PrependPublisherToAppTitle { get; set; } = "true";

        public string ProducerWebSite { get; private set; } = "http://www.microarea.it";
        public string ProducerPng { get; private set; } = "Apps/Images/LogoMicroarea.png";

        internal GenericBrand()
        {

        }

        internal static GenericBrand Load()
        {
            var genericBrand = new GenericBrand();

            string filePath = BasePathFinder.BasePathFinderInstance.GetGenericBrandFilePath();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return genericBrand;
            }
            XmlDocument genericBrandXmlDoc = new XmlDocument();
            genericBrandXmlDoc.Load(filePath);

            var xPathQuery = "//BrandedKey[@source='ProductTitle']";

            var xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.ProductTitle = brandedAttribute.Value;
                }
            }

            xPathQuery = "//BrandedKey[@source='DigitalSignatureUrl']";

            xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.DigitalSignatureUrl = brandedAttribute.Value;
                }
            }

            xPathQuery = "//BrandedKey[@source='DigitalSignatureBackupUrl']";

            xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.DigitalSignatureBackupUrl = brandedAttribute.Value;
                }
            }

            xPathQuery = "//BrandedKey[@source='ProductPublisher']";

            xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.ProductPublisher = brandedAttribute.Value;
                }
            }

            xPathQuery = "//BrandedKey[@source='PrependPublisherToAppTitle']";

            xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.PrependPublisherToAppTitle = brandedAttribute.Value;
                }
            }

            xPathQuery = "//BrandedKey[@source='ProducerWebSite']";

            xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.ProducerWebSite = brandedAttribute.Value;
                }
            }

            xPathQuery = "//BrandedKey[@source='ProducerPng']";

            xmlNode = genericBrandXmlDoc.SelectSingleNode(xPathQuery);
            if (xmlNode != null)
            {
                var brandedAttribute = xmlNode.Attributes["branded"];
                if (brandedAttribute != null && !string.IsNullOrWhiteSpace(brandedAttribute.Value))
                {
                    genericBrand.ProducerPng = brandedAttribute.Value;
                }
            }

            return genericBrand;
        }
    }
}
