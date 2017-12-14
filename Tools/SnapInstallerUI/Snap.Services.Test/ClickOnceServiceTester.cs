using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microarea.Snap.Services;
using Microarea.Snap.Core;
using Microarea.Snap.IO;

namespace Microarea.Snap.Services.Test
{
    public class ClickOnceServiceTester
    {
        [Fact]
        public void Uninstall_should_use_ClickOnceService_if_it_uninstalls_files_from_the_publish_folder()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>publish\\file.txt</file></files></package>";
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
            A.CallTo(() => packageManager.Uninstall(snappackage, where))
                .DoesNothing();

            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .DoesNothing();
            A.CallTo(() => packagesRegistry["CADI"])
                .WithAnyArguments()
                .Returns(snappackage);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .DoesNothing();

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());
            installerService.Uninstall("CADI", where);

            System.Threading.Thread.Sleep(1000);
            installerService.Join();

            A.CallTo(() => clickOnceService.Deploy())
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Uninstall_should_not_use_ClickOnceService_if_it_doesnt_uninstall_files_from_the_publish_folder()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file></files></package>";
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
            A.CallTo(() => packageManager.Uninstall(snappackage, where))
                .DoesNothing();

            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .DoesNothing();
            A.CallTo(() => packagesRegistry["CADI"])
                .WithAnyArguments()
                .Returns(snappackage);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .DoesNothing();

            var installedVerService = A.Fake<IInstallationVersionService>();
            A.CallTo(() => installedVerService.ProductName)
                .Returns("Magonet");
            A.CallTo(() => installedVerService.Version)
                .Returns("3.13.0.236");

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());
            installerService.Uninstall("CADI", where);

            System.Threading.Thread.Sleep(1000);
            installerService.Join();

            A.CallTo(() => clickOnceService.Deploy())
                .MustNotHaveHappened();
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .MustNotHaveHappened();
        }

        [Fact]
        public void Install_should_use_ClickOnceService_if_it_installs_files_into_the_publish_folder()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>publish\\file.txt</file></files></package>";
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
            A.CallTo(() => packageManager.Uninstall(snappackage, where))
                .DoesNothing();

            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .DoesNothing();
            A.CallTo(() => packagesRegistry["CADI"])
                .WithAnyArguments()
                .Returns(null);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .DoesNothing();

            var installedVerService = A.Fake<IInstallationVersionService>();
            A.CallTo(() => installedVerService.ProductName)
                .Returns("Mago4");
            A.CallTo(() => installedVerService.Version)
                .Returns("1.3.1");

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());
            installerService.Install(snappackage, where);

            System.Threading.Thread.Sleep(1000);
            installerService.Join();

            A.CallTo(() => clickOnceService.Deploy())
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Install_should_not_use_ClickOnceService_if_it_doesnt_install_files_into_the_publish_folder()
        {
            string manifestString =
"<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microarea.it/snapinstaller\"><metadata><id>CADI</id><version>2</version><title>Digital Communication</title><iconUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/CADI.png</iconUrl><product>Mago4</product><productversion>1.3.1</productversion><description>What changed with this version of Accounting?</description><releaseNotesUrl>http://www.microarea.it/snapinstaller/Mago4/1.3.1/CADI/cadi.Release-Notes.txt</releaseNotesUrl></metadata><files><file>prova\\file.txt</file></files></package>";
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
            A.CallTo(() => packageManager.Uninstall(snappackage, where))
                .DoesNothing();

            var packagesRegistry = A.Fake<IPackagesRegistry>();
            A.CallTo(() => packagesRegistry.Remove(""))
                .WithAnyArguments()
                .DoesNothing();
            A.CallTo(() => packagesRegistry["CADI"])
                .WithAnyArguments()
                .Returns(null);

            var clickOnceService = A.Fake<IClickOnceService>();
            A.CallTo(() => clickOnceService.Deploy())
                .DoesNothing();
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .DoesNothing();

            var installedVerService = A.Fake<IInstallationVersionService>();
            A.CallTo(() => installedVerService.ProductName)
                .Returns("Mago4");
            A.CallTo(() => installedVerService.Version)
                .Returns("1.3.1");

            var productCompatibilityService = A.Fake<IProductCompatibilityService>();
            A.CallTo(() => productCompatibilityService.EnsureProductCompatibility(null))
                .DoesNothing();

            var installerService = new InstallerService(packageManager, packagesRegistry, clickOnceService, productCompatibilityService, null, new NoTransactionManager());
            installerService.Install(snappackage, where);

            System.Threading.Thread.Sleep(1000);
            installerService.Join();

            A.CallTo(() => clickOnceService.Deploy())
                .MustNotHaveHappened();
            A.CallTo(() => clickOnceService.UpdateDeployment())
                .MustNotHaveHappened();
        }
    }
}
