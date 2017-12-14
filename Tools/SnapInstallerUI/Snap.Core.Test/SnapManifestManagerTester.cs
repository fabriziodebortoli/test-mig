using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using FluentAssertions;

using Microarea.Snap.Core;
using Microarea.Snap.IO;

namespace Microarea.Snap.Core.Test
{
    public class SnapManifestManagerTester
    {
        [Fact]
        public void CreateManifest_should_throw_if_null_toExplore_folder_is_passed_in()
        {
            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(null, null, null, null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_null_toExplore_folder_does_not_exist()
        {
            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(new Folder("C:\\not\\existing\\folder"), null, null, null, null))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_null_output_file_is_passed_in()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, null, null, null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_null_intput_file_is_passed_in()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), null, null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_intput_file_does_not_exist()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), new File("C:\\Path\\to\\Not\\Existing\\File.xml"), null, null))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_build_is_null()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_build_is_empty()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, null, null))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_build_is_white_spaces()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, "        ", null))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_build_is_not_a_number()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, "notanumber", null))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_version_is_null()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, "1717", null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_version_is_empty()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, "1717", ""))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_version_is_white_spaces()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, "1717", "        "))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_throw_if_version_is_not_a_version()
        {
            var toExplore = A.Fake<Folder>();
            A.CallTo(() => toExplore.Exists)
                .Returns(true);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);

            var m = new SnapManifestManager();
            new Action(() => m.CreateManifest(toExplore, new File("prova.txt"), inputFile, "1717", "notaversion"))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreateManifest_should_create_and_save_a_manifest()
        {
            var outputstream = new System.IO.MemoryStream();
            var outputFile = A.Fake<File>();
            A.CallTo(() => outputFile.OpenWrite())
                .Returns(outputstream);

            var inputFile = A.Fake<File>();
            A.CallTo(() => inputFile.Exists)
                .Returns(true);
            A.CallTo(() => inputFile.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.mdc.xml"));

            string codeBase = this.GetType().Assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            IFolder toExplore = new Folder(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(path)))), "res\\TestInstance"));

            var m = new SnapManifestManager();
            var manifest = m.CreateManifest(toExplore, outputFile, inputFile, "1717", "3.13.0");

            outputstream.ToArray().Length.Should().BeGreaterThan(0);
            manifest.Id.Should().Be("mdc");
            manifest.Version.Should().Be(1717);
            manifest.Title.Should().Be("Comunicazioni Digitali");
            manifest.Producer.Should().Be("Microarea");
            manifest.Product.Should().Be("Mago.net");
            manifest.ProductVersion.Should().Be("3.13.0");
            manifest.Description.Should().Be("Comunicazioni Digitali è una nuova applicazione verticale di Mago per la gestione delle comunicazioni digitali quali la fatturazione elettronica PA e B2B e prossimamente il CADI (Comunicazione analitica dati IVA). L'applicazione viene prodotta insieme ad ogni aggiornamento di Mago in unico msi, o separatamente da sola per aggiornamenti solo ad essa relativi con uno zip autoinstallante.");
            manifest.IconUrl.Should().BeNull();
            manifest.ReleaseNotesUrl.Should().BeNull();
            manifest.Files.Should().NotBeNull();
            manifest.Files.Should().NotBeEmpty();
            manifest.Files.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ParseVersion_should_skip_HF_tokens()
        {
            var version = SnapManifestManager.ParseVersion("3.12.16.HF3");
            version.Major.Should().Be(3);
            version.Minor.Should().Be(12);
            version.Build.Should().Be(16);
            version.Revision.Should().Be(-1);
        }

        [Fact]
        public void ParseVersion_should_skip_X_tokens()
        {
            var version = SnapManifestManager.ParseVersion("2.x");
            version.Major.Should().Be(2);
            version.Minor.Should().Be(0);
            version.Build.Should().Be(0);
            version.Revision.Should().Be(0);
        }

        [Fact]
        public void Version_should_output_versions_with_two_tokens_only()
        {
            var version = SnapManifestManager.ParseVersion("1.4");

            var output = version.ToStringForManifest();

            output.Should().Be("1.4");
        }
        [Fact]
        public void Version_should_output_versions_with_three_tokens()
        {
            var version = SnapManifestManager.ParseVersion("1.4.3");

            var output = version.ToStringForManifest();

            output.Should().Be("1.4.3");
        }
    }
}
