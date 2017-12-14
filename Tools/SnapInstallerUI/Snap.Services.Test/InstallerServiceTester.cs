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

namespace Microarea.Snap.Services.Test
{
    public class InstallerServiceTester
    {
        [Fact]
        public void Install_should_throw_if_package_is_null()
        {
            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Install(null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Install_should_throw_if_manifest_package_is_null()
        {
            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Install(new SnapPackage(), null))
                .ShouldThrow<PackageException>();
        }
        [Fact]
        public void Install_should_throw_if_the_id_of_the_manifest_package_is_empty()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id></id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var snapmanifest = new SnapManifest();
            snapmanifest.Init(manifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var snappackage = new SnapPackage();
            snappackage.Init(file, snapmanifest);

            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Install(snappackage, null))
                .ShouldThrow<PackageException>();
        }

        [Fact]
        public void Install_should_throw_if_where_is_null()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var snapmanifest = new SnapManifest();
            snapmanifest.Init(manifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var snappackage = new SnapPackage();
            snappackage.Init(file, snapmanifest);

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();



            var installerService = new InstallerService(null, null, null, productCompatibilityService, null, null);
            new Action(() => installerService.Install(snappackage, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Install_should_create_the_folder_if_where_does_not_exist()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var snapmanifest = new SnapManifest();
            snapmanifest.Init(manifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var snappackage = new SnapPackage();
            snappackage.Init(file, snapmanifest);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.Exists)
                .Returns(false);
            A.CallTo(() => where.Create());

            var registry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => registry[""])
                .WithAnyArguments()
                .Returns(null);
            A.CallTo(() => registry.Add(null))
                .WithAnyArguments();

            var packageManager = A.Fake<IPackageManager>();
            A.CallTo(() => packageManager.Install(null, null))
                .WithAnyArguments();

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();



            var installerService = new InstallerService(packageManager, registry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());
            installerService.Install(snappackage, where);

            A.CallTo(() => where.Create())
                .MustHaveHappened(Repeated.Exactly.Once);

            System.Threading.Thread.Sleep(1000);
            A.CallTo(() => packageManager.Install(null, null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Uninstall_should_throw_if_packageId_is_null()
        {
            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Uninstall(null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Uninstall_should_throw_if_packageId_is_empty()
        {
            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Uninstall("", null))
                .ShouldThrow<ArgumentException>();
        }
        [Fact]
        public void Uninstall_should_throw_if_packageId_is_whitespace()
        {
            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Uninstall("       ", null))
                .ShouldThrow<ArgumentException>();
        }
        [Fact]
        public void Uninstall_should_notify_with_ErrorOccurred_if_the_id_of_the_package_is_not_present_in_registry()
        {
            var registry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => registry[""])
                .WithAnyArguments()
                .Returns(null);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.Exists)
                .Returns(true);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(null, registry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());
            bool error = false;
            installerService.ErrorOccurred += (s, e) => error = true;
            installerService.Uninstall("idNotPresent", where);

            error.Should().BeTrue();
        }

        [Fact]
        public void Uninstall_should_throw_if_where_is_null()
        {
            var installerService = new InstallerService(null, null, null, null, null, null);
            new Action(() => installerService.Uninstall("packageId", null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Uninstall_should_notify_with_ErrorOccurred_if_where_does_not_exist()
        {
            var where = A.Fake<IFolder>();
            A.CallTo(() => where.Exists)
                .Returns(false);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(null, null, clickOnceService, productCompatibilityService, null, new NoTransactionManager());

            bool error = false;
            installerService.ErrorOccurred += (s, e) => error = true;

            installerService.Uninstall("packageId", where);

            error.Should().BeTrue();
        }

        [Fact]
        public void Install_should_install_a_package_if_the_package_is_not_yet_present_in_registry()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var snapmanifest = new SnapManifest();
            snapmanifest.Init(manifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var snappackage = new SnapPackage();
            snappackage.Init(file, snapmanifest);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => where.Exists)
                .Returns(true);

            var packageManager = A.Fake<IPackageManager>();
            A.CallTo(() => packageManager.Install(snappackage, where));

            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments();
            A.CallTo(() => packagesRegistry["CADI"])
                .Returns(null);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();



            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());

            bool startingCalled = false;
            installerService.Starting += (s, e) => startingCalled = true;
            bool startedCalled = false;
            installerService.Started += (s, e) => startedCalled = true;
            bool stoppingCalled = false;
            installerService.Stopping += (s, e) => stoppingCalled = true;
            bool stoppedCalled = false;
            installerService.Stopped += (s, e) => stoppedCalled = true;
            bool installingCalled = false;
            installerService.Installing += (s, e) => installingCalled = true;
            bool installedCalled = false;
            installerService.Installed += (s, e) => installedCalled = true;
            bool updatingCalled = false;
            installerService.Updating += (s, e) => updatingCalled = true;
            bool updatedCalled = false;
            bool errorCalled = false;
            installerService.ErrorOccurred += (s, e) => errorCalled = true;

            installerService.Install(snappackage, where);

            //waiting for the worker thread to start...
            Thread.Sleep(1000);

            A.CallTo(() => packageManager.Install(snappackage, where))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .MustNotHaveHappened();
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);

            startingCalled.Should().BeTrue();
            startedCalled.Should().BeTrue();
            stoppingCalled.Should().BeTrue();
            stoppedCalled.Should().BeTrue();
            installingCalled.Should().BeTrue();
            installedCalled.Should().BeTrue();
            errorCalled.Should().BeFalse();
            updatingCalled.Should().BeFalse();
            updatedCalled.Should().BeFalse();

        }

        [Fact]
        public void Install_should_update_a_package_if_the_package_is_present_in_registry_and_its_version_is_lower_than_the_version_of_the_installing_one()
        {
            string newManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var newSnapManifest = new SnapManifest();
            newSnapManifest.Init(newManifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => where.Exists)
                .Returns(true);

            var packageManager = A.Fake<IPackageManager>();
            A.CallTo(() => packageManager.Install(newSnapPackage, where));




            string oldManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>1</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var oldSnapManifest = new SnapManifest();
            oldSnapManifest.Init(oldManifestString);

            var oldSnapPackage = new SnapPackage();
            oldSnapPackage.Init(file, oldSnapManifest);



            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments();
            A.CallTo(() => packagesRegistry["CADI"])
                .Returns(oldSnapPackage);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());

            bool startingCalled = false;
            installerService.Starting += (s, e) => startingCalled = true;
            bool startedCalled = false;
            installerService.Started += (s, e) => startedCalled = true;
            bool stoppingCalled = false;
            installerService.Stopping += (s, e) => stoppingCalled = true;
            bool stoppedCalled = false;
            installerService.Stopped += (s, e) => stoppedCalled = true;
            bool installingCalled = false;
            installerService.Installing += (s, e) => installingCalled = true;
            bool updatingCalled = false;
            installerService.Updating += (s, e) => updatingCalled = true;
            bool updatedCalled = false;
            installerService.Updated += (s, e) => updatedCalled = true;
            bool installedCalled = false;
            installerService.Installed += (s, e) => installedCalled = true;
            bool errorCalled = false;
            installerService.ErrorOccurred += (s, e) => errorCalled = true;

            installerService.Install(newSnapPackage, where);

            //waiting for the worker thread to start...
            Thread.Sleep(1000);

            A.CallTo(() => packageManager.Update(oldSnapPackage, newSnapPackage, where))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);

            startingCalled.Should().BeTrue();
            startedCalled.Should().BeTrue();
            stoppingCalled.Should().BeTrue();
            stoppedCalled.Should().BeTrue();
            updatingCalled.Should().BeTrue();
            updatedCalled.Should().BeTrue();
            installingCalled.Should().BeFalse();
            installedCalled.Should().BeFalse();
            errorCalled.Should().BeFalse();
        }

        [Fact]
        public void Install_should_notify_with_ErrorOccurred_if_the_package_is_present_in_registry_and_its_version_is_higher_than_the_version_of_the_installing_one()
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

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => where.Exists)
                .Returns(true);

            var packageManager = A.Fake<IPackageManager>();
            A.CallTo(() => packageManager.Install(newSnapPackage, where));




            string oldManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var oldSnapManifest = new SnapManifest();
            oldSnapManifest.Init(oldManifestString);

            var oldSnapPackage = new SnapPackage();
            oldSnapPackage.Init(file, oldSnapManifest);



            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments();
            A.CallTo(() => packagesRegistry["CADI"])
                .Returns(oldSnapPackage);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();



            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());

            bool startingCalled = false;
            installerService.Starting += (s, e) => startingCalled = true;
            bool startedCalled = false;
            installerService.Started += (s, e) => startedCalled = true;
            bool stoppingCalled = false;
            installerService.Stopping += (s, e) => stoppingCalled = true;
            bool stoppedCalled = false;
            installerService.Stopped += (s, e) => stoppedCalled = true;
            bool installingCalled = false;
            installerService.Installing += (s, e) => installingCalled = true;
            bool installedCalled = false;
            installerService.Installed += (s, e) => installedCalled = true;
            bool errorCalled = false;
            installerService.ErrorOccurred += (s, e) => errorCalled = true;

            installerService.Install(newSnapPackage, where);

            A.CallTo(() => packageManager.Install(newSnapPackage, where))
                .MustNotHaveHappened();
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .MustNotHaveHappened();

            startingCalled.Should().BeFalse();
            startedCalled.Should().BeFalse();
            stoppingCalled.Should().BeFalse();
            stoppedCalled.Should().BeFalse();
            installingCalled.Should().BeFalse();
            installedCalled.Should().BeFalse();
            errorCalled.Should().BeTrue();
        }

        [Fact]
        public void Install_should_install_a_package_if_the_package_is_present_in_registry_and_its_version_is_equal_to_the_version_of_the_installing_one_aka_RestorePackage()
        {
            string newManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var newSnapManifest = new SnapManifest();
            newSnapManifest.Init(newManifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var newSnapPackage = new SnapPackage();
            newSnapPackage.Init(file, newSnapManifest);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => where.Exists)
                .Returns(true);

            var packageManager = A.Fake<IPackageManager>();
            A.CallTo(() => packageManager.Install(newSnapPackage, where));




            string oldManifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var oldSnapManifest = new SnapManifest();
            oldSnapManifest.Init(oldManifestString);

            var oldSnapPackage = new SnapPackage();
            oldSnapPackage.Init(file, oldSnapManifest);



            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments();
            A.CallTo(() => packagesRegistry["CADI"])
                .Returns(oldSnapPackage);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());

            bool startingCalled = false;
            installerService.Starting += (s, e) => startingCalled = true;
            bool startedCalled = false;
            installerService.Started += (s, e) => startedCalled = true;
            bool stoppingCalled = false;
            installerService.Stopping += (s, e) => stoppingCalled = true;
            bool stoppedCalled = false;
            installerService.Stopped += (s, e) => stoppedCalled = true;
            bool installingCalled = false;
            installerService.Installing += (s, e) => installingCalled = true;
            bool updatingCalled = false;
            installerService.Updating += (s, e) => updatingCalled = true;
            bool updatedCalled = false;
            installerService.Updated += (s, e) => updatedCalled = true;
            bool installedCalled = false;
            installerService.Installed += (s, e) => installedCalled = true;
            bool errorCalled = false;
            installerService.ErrorOccurred += (s, e) => errorCalled = true;

            installerService.Install(newSnapPackage, where);

            //waiting for the worker thread to start...
            Thread.Sleep(1000);

            A.CallTo(() => packageManager.Update(oldSnapPackage, newSnapPackage, where))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => packagesRegistry.Add(null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);

            startingCalled.Should().BeTrue();
            startedCalled.Should().BeTrue();
            stoppingCalled.Should().BeTrue();
            stoppedCalled.Should().BeTrue();
            updatingCalled.Should().BeTrue();
            updatedCalled.Should().BeTrue();
            installingCalled.Should().BeFalse();
            installedCalled.Should().BeFalse();
            errorCalled.Should().BeFalse();
        }

        [Fact]
        public void Uninstall_should_uninstall_a_package()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file><file>prova\\provetta\\file.txt</file></files></package>";
            var snapmanifest = new SnapManifest();
            snapmanifest.Init(manifestString);

            var file = A.Fake<IFile>();
            A.CallTo(() => file.FullName)
                .Returns("C:\\prova.txt");
            A.CallTo(() => file.Exists)
                .Returns(true);

            var snappackage = new SnapPackage();
            snappackage.Init(file, snapmanifest);

            var where = A.Fake<IFolder>();
            A.CallTo(() => where.FullName)
                .Returns("C:\\prova");
            A.CallTo(() => where.Exists)
                .Returns(true);

            var packageManager = A.Fake<IPackageManager>();
            A.CallTo(() => packageManager.Uninstall(snappackage, where));

            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments();
            A.CallTo(() => packagesRegistry["CADI"])
                .WithAnyArguments()
                .Returns(snappackage);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();



            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());

            bool startingCalled = false;
            installerService.Starting += (s, e) => startingCalled = true;
            bool startedCalled = false;
            installerService.Started += (s, e) => startedCalled = true;
            bool stoppingCalled = false;
            installerService.Stopping += (s, e) => stoppingCalled = true;
            bool stoppedCalled = false;
            installerService.Stopped += (s, e) => stoppedCalled = true;
            bool uninstallingCalled = false;
            installerService.Uninstalling += (s, e) => uninstallingCalled = true;
            bool uninstalledCalled = false;
            installerService.Uninstalled += (s, e) => uninstalledCalled = true;
            bool errorCalled = false;
            installerService.ErrorOccurred += (s, e) => errorCalled = true;

            installerService.Uninstall("CADI", where);

            //waiting for the worker thread to start...
            Thread.Sleep(1000);

            A.CallTo(() => packageManager.Uninstall(snappackage, where))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);

            startingCalled.Should().BeTrue();
            startedCalled.Should().BeTrue();
            stoppingCalled.Should().BeTrue();
            stoppedCalled.Should().BeTrue();
            uninstallingCalled.Should().BeTrue();
            uninstalledCalled.Should().BeTrue();
            errorCalled.Should().BeFalse();
        }
    }
}
