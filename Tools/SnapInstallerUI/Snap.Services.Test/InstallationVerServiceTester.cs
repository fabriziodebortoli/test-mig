using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microarea.Snap.Services;
using System.Threading;
using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System.Xml;
using System.IO;

namespace Microarea.Snap.Services.Test
{
    public class InstallationVerServiceTester
    {
        [Fact]
        public void Load_should_load_the_file()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var settings = A.Fake<ISettings>();

            var installationVerService = new InstallationVersionService(settings, null);

            installationVerService.Load(xmlDocument);

            installationVerService.ProductName.Should().Be("Magonet");
        }

        [Fact]
        public void Even_if_the_version_in_the_file_is_with_four_tokens_the_loaded_version_should_have_three_tokens()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var settings = A.Fake<ISettings>();

            var installationVerService = new InstallationVersionService(settings, null);

            installationVerService.Load(xmlDocument);

            installationVerService.Version.Should().Be("3.9.0");
        }

        [Fact]
        public void Changing_settings_ProductInstenceFolder_InstallationVersionService_should_reload_InstallationVer_file()
        {
            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            byte[] buffer2 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Mago4</ProductName><Version>1.4.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream2 = new MemoryStream(buffer2);
            inputStream2.Seek(0, SeekOrigin.Begin);

            var file2 = A.Fake<IFile>();
            A.CallTo(() => file2.OpenRead())
                .Returns(inputStream2);

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .ReturnsNextFromSequence(file1, file2);

            var settings = new Settings();

            var installationVerService = new InstallationVersionService(settings, fileSystemService);

            installationVerService.ProductName.Should().Be("Magonet");
            installationVerService.Version.Should().Be("3.9.0");

            settings.ProductInstanceFolder = "C:\\temp\\prova";

            installationVerService.ProductName.Should().Be("Mago4");
            installationVerService.Version.Should().Be("1.4.0");
        }

        [Fact]
        public void InvalidateMenuCache_should_change_CacheDate_value_and_save_the_file()
        {
            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            MemoryStream outputStream1 = new MemoryStream();
            A.CallTo(() => file1.OpenWrite())
                .Returns(outputStream1);

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .Returns(file1);

            var settings = A.Fake<ISettings>();

            var installationVerService = new InstallationVersionService(settings, fileSystemService);

            installationVerService.InvalidateMenuCache();

            var ms = new MemoryStream(outputStream1.GetBuffer());
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(ms);

            var xPathMask = "/InstallationVersion/CacheDate";
            var element = xmlDocument.SelectSingleNode(xPathMask);

            element.InnerText.Should().NotBe("20170302113435");
        }
    }
}
