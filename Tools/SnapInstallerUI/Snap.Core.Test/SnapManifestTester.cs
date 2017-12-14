using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microarea.Snap.Core;

namespace Microarea.Snap.Core.Test
{
    public class SnapManifestTester
    {
        [Fact]
        public void Init_should_initialize_the_package()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(manifestString));

            var manifest = new SnapManifest();
            manifest.Init(stream);

            manifest.Description.ShouldBeEquivalentTo("What changed with this version of Accounting?");
            manifest.IconUrl.ShouldBeEquivalentTo("http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png");
            manifest.Id.ShouldBeEquivalentTo("CADI");
            manifest.Product.ShouldBeEquivalentTo("Mago4");
            manifest.ProductVersion.ShouldBeEquivalentTo("1.3.1");
            manifest.ReleaseNotesUrl.ShouldBeEquivalentTo("http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt");
            manifest.Title.ShouldBeEquivalentTo("Digital Communication");
            manifest.Version.ShouldBeEquivalentTo(2);

            manifest.Files.Should().NotBeEmpty();
            manifest.Files.Count.ShouldBeEquivalentTo(2);
            foreach (var file in manifest.Files)
            {
                (file.FullName == "prova\\file.txt" || file.FullName == "prova\\provetta\\file.txt").Should().BeTrue();
            }
        }

        [Fact]
        public void Init_should_not_throw_exception_if_some_fields_are_not_passed()
        {
            string manifestString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<package xmlns=""http://schemas.microarea.it/snapinstaller"">
    <metadata>
        <id></id>
        <title>Digital Communication</title>
        <iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl>
        <product>Mago4</product>
        <description></description>
        <releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl>
    </metadata>
</package>";
            var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(manifestString));

            var manifest = new SnapManifest();
            manifest.Init(stream);

            manifest.Id.ShouldBeEquivalentTo(string.Empty);
            manifest.Title.ShouldBeEquivalentTo("Digital Communication");
            manifest.IconUrl.ShouldBeEquivalentTo("http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png");
            manifest.Product.ShouldBeEquivalentTo("Mago4");
            manifest.Description.ShouldBeEquivalentTo(string.Empty);
            manifest.ReleaseNotesUrl.ShouldBeEquivalentTo("http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt");
            manifest.ProductVersion.ShouldBeEquivalentTo("");
            manifest.Version.ShouldBeEquivalentTo(0);
        }
    }
}
