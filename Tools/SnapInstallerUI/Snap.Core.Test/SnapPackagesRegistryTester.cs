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
    public class SnapPackagesRegistryTester
    {
        [Fact]
        public void Init_should_init_the_registry()
        {
            var files = new List<IFile>();

            var package1 = A.Fake<IFile>();
            A.CallTo(() => package1.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.Cadi.tbfp"));
            A.CallTo(() => package1.FullName)
                .Returns("C:\\Dev\\Cadi.tbfp");
            A.CallTo(() => package1.Exists)
                .Returns(true);
            files.Add(package1);

            var package2 = A.Fake<IFile>();
            A.CallTo(() => package2.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.RanorexTestSupport.tbfp"));
            A.CallTo(() => package2.FullName)
                .Returns("C:\\Dev\\RanorexTestSupport.tbfp");
            A.CallTo(() => package2.Exists)
                .Returns(true);
            files.Add(package2);

            var configFolder = A.Fake<IFolder>();
            A.CallTo(() => configFolder.Exists)
                .Returns(true);
            A.CallTo(() => configFolder.GetFiles(""))
                .WithAnyArguments()
                .Returns(files.ToArray());

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(configFolder.FullName);

            var r = new SnapPackagesRegistry(settings, new SnapPackageLoader(), configFolder);

            r.PackagesCount.Should().Be(files.Count);

            r["Cadi"].Should().NotBeNull();
            r["RanorexTestSupport"].Should().NotBeNull();
        }

        [Fact]
        public void IsInstalled_should_return_true_if_a_package_with_the_given_id_is_present_in_the_registry()
        {
            var configFolder = A.Fake<IFolder>();
            A.CallTo(() => configFolder.Exists)
                .Returns(true);

            var files = new List<IFile>();

            var package1 = A.Fake<IFile>();
            A.CallTo(() => package1.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.Cadi.tbfp"));
            A.CallTo(() => package1.FullName)
                .Returns("C:\\Dev\\Cadi.tbfp");
            A.CallTo(() => package1.Exists)
                .Returns(true);
            files.Add(package1);

            var package2 = A.Fake<IFile>();
            A.CallTo(() => package2.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.RanorexTestSupport.tbfp"));
            A.CallTo(() => package2.FullName)
                .Returns("C:\\Dev\\RanorexTestSupport.tbfp");
            A.CallTo(() => package2.Exists)
                .Returns(true);
            files.Add(package2);

            A.CallTo(() => configFolder.GetFiles(""))
                .WithAnyArguments()
                .Returns(files.ToArray());

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(configFolder.FullName);

            var r = new SnapPackagesRegistry(settings, new SnapPackageLoader(), configFolder);

            r.IsInstalled("Cadi").Should().BeTrue();
        }

        [Fact]
        public void IsInstalled_should_return_false_if_a_package_with_the_given_id_is_not_present_in_the_registry()
        {
            var configFolder = A.Fake<IFolder>();
            A.CallTo(() => configFolder.Exists)
                .Returns(true);

            var files = new List<IFile>();

            var package1 = A.Fake<IFile>();
            A.CallTo(() => package1.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.Cadi.tbfp"));
            A.CallTo(() => package1.FullName)
                .Returns("C:\\Dev\\Cadi.tbfp");
            A.CallTo(() => package1.Exists)
                .Returns(true);
            files.Add(package1);

            var package2 = A.Fake<IFile>();
            A.CallTo(() => package2.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.RanorexTestSupport.tbfp"));
            A.CallTo(() => package2.FullName)
                .Returns("C:\\Dev\\RanorexTestSupport.tbfp");
            A.CallTo(() => package2.Exists)
                .Returns(true);
            files.Add(package2);

            A.CallTo(() => configFolder.GetFiles(""))
                .WithAnyArguments()
                .Returns(files.ToArray());

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(configFolder.FullName);

            var r = new SnapPackagesRegistry(settings, new SnapPackageLoader(), configFolder);

            r.IsInstalled("pippo").Should().BeFalse();
        }


        [Fact]
        public void Add_should_throw_if_package_is_null()
        {
            var configFolder = A.Fake<IFolder>();
            A.CallTo(() => configFolder.Exists)
                .Returns(true);
            A.CallTo(() => configFolder.GetFiles(""))
                .WithAnyArguments()
                .Returns(new IFile[] { });

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(configFolder.FullName);

            var r = new SnapPackagesRegistry(settings, new SnapPackageLoader(), configFolder);
            new Action(() => r.Add(null))
                .ShouldThrow<ArgumentNullException>();
        }


        [Fact]
        public void Add_should_throw_if_a_package_is_already_present()
        {
            string newManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>1</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var newSnapManifest = new SnapManifest();
            newSnapManifest.Init(newManifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.FullName)
                .Returns("prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);

            var configFolder = A.Fake<IFolder>();
            A.CallTo(() => configFolder.Exists)
                .Returns(true);
            A.CallTo(() => configFolder.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => configFolder.GetFiles(""))
                .WithAnyArguments()
                .Returns(new List<IFile>() { }.ToArray());

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(configFolder.FullName);

            var r = new SnapPackagesRegistry(settings, new SnapPackageLoader(), configFolder);

            r.Add(newSnapPackage);

            new Action(() => r.Add(newSnapPackage))
                .ShouldThrow<RegistryException>();
        }

        [Fact]
        public void Add_should_add_a_package_if_it_is_not_yet_present()
        {
            string newManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>1</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var newSnapManifest = new SnapManifest();
            newSnapManifest.Init(newManifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.FullName)
                .Returns("prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);
            A.CallTo(() => file.CopyTo(new File("")))
                .WithAnyArguments();

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);

            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(true);
            A.CallTo(() => folder.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => folder.GetFiles(""))
                .WithAnyArguments()
                .Returns(new List<IFile>() {  }.ToArray());

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(folder.FullName);

            var r = new SnapPackagesRegistry(settings, new SnapPackageLoader(), folder);

            r.PackagesCount.Should().Be(0);

            r.Add(newSnapPackage);

            r.PackagesCount.Should().Be(1);

            A.CallTo(() => file.CopyTo(new File("")))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);

            var test = r["CADI"];
            test.Should().NotBeNull();
            test.Should().BeSameAs(newSnapPackage);
        }

        [Fact]
        public void Remove_should_remove_a_package_if_it_is_present()
        {
            string newManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>1</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var newSnapManifest = new SnapManifest();
            newSnapManifest.Init(newManifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Name)
                .Returns("prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);
            A.CallTo(() => file.Delete());

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);


            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(true);
            A.CallTo(() => folder.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => folder.GetFiles(""))
                .WithAnyArguments()
                .Returns(new List<IFile>() { file }.ToArray());

            var packageLoader = A.Fake<IPackageLoader>();
            A.CallTo(() => packageLoader.LoadPackage(file))
                .WithAnyArguments()
                .Returns(newSnapPackage);

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(folder.FullName);

            var r = new SnapPackagesRegistry(settings, packageLoader, folder);

            r.PackagesCount.Should().Be(1);

            r.Remove("CADI");

            r.PackagesCount.Should().Be(0);
            A.CallTo(() => file.Delete())
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Remove_should_do_nothing_a_package_if_is_not_present()
        {
            string newManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>1</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var newSnapManifest = new SnapManifest();
            newSnapManifest.Init(newManifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);
            A.CallTo(() => file.Delete());

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);

            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(true);
            A.CallTo(() => folder.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => folder.GetFiles(""))
                .WithAnyArguments()
                .Returns(new List<IFile>() { file }.ToArray());

            var packageLoader = A.Fake<IPackageLoader>();
            A.CallTo(() => packageLoader.LoadPackage(file))
                .WithAnyArguments()
                .Returns(newSnapPackage);

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns(folder.FullName);

            var r = new SnapPackagesRegistry(settings, packageLoader, folder);

            r.PackagesCount.Should().Be(1);

            r.Remove("CADI1");

            r.PackagesCount.Should().Be(1);
            A.CallTo(() => file.Delete())
                .MustNotHaveHappened();
        }
    }
}
