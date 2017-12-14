using FakeItEasy;
using Microarea.Snap.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using LightInject;
using Microarea.Snap.IO;
using System.IO;

namespace Microarea.Snap.Services.Test
{
    public class ProductFolderServiceTester
    {
        internal class IocFactoryForTest : IocFactory
        {
            IFile fakeFile;
            IFolder fakeFolder;

            public IocFactoryForTest(IFile fakeFile, IFolder fakeFolder)
            {
                this.fakeFile = fakeFile;
                this.fakeFolder = fakeFolder;
            }

            public override T GetInstance<T>()
            {
                if (typeof(T) == typeof(IFile))
                {
                    return (T)fakeFile;
                }
                if (typeof(T) == typeof(IFolder))
                {
                    return (T)fakeFolder;
                }

                return default(T);
            }

            public override T GetInstance<T, TParam>(TParam param)
            {
                if (typeof(T) == typeof(IFile))
                {
                    if (fakeFile == null)
                    {
                        fakeFile = new IO.File(param as string);
                    }
                    return (T)fakeFile;
                }
                if (typeof(T) == typeof(IFolder))
                {
                    if (fakeFolder == null)
                    {
                        fakeFolder = new IO.Folder(param as string);
                    }
                    return (T)fakeFolder;
                }

                return default(T);
            }
        }

        /// <summary>
        /// If settings.ProductInstanceFolder is set and the folder exists then IsProductFolderSet should be true
        /// </summary>
        [Fact]
        public void EnsureProductFolder()
        {
            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.ProductInstanceFolder)
                .Returns("C:\\TEMP\\Prova");
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns("C:\\Scambi");
            A.CallTo(() => settings.UseTransactions)
                .Returns(false);

            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(true);
            A.CallTo(() => folder.Create())
                .WithAnyArguments()
                .DoesNothing();

            var iocContainer = new IocFactoryForTest(null, folder);

            var settingsLoader = A.Fake<ISettingsLoader>();
            A.CallTo(() => settingsLoader.Save(null))
                .WithAnyArguments()
                .DoesNothing();

            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .Returns(file1);

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                settingsLoader,
                null,
                new InstallationVersionService(settings, fileSystemService)
                );

            productFolderService.EnsureProductFolder();

            productFolderService.IsProductFolderSet.Should().BeTrue();
            A.CallTo(() => folder.Exists)
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => folder.Create())
                .MustNotHaveHappened();
        }
        /// <summary>
        /// If settings.ProductInstanceFolder is set and the folder does not exist then IsProductFolderSet should be true and the folder should be created
        /// </summary>
        [Fact]
        public void EnsureProductFolder_01()
        {
            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.ProductInstanceFolder)
                .Returns("C:\\TEMP\\Prova");
            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns("C:\\Scambi");
            A.CallTo(() => settings.UseTransactions)
                .Returns(false);

            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(false);
            A.CallTo(() => folder.Create())
                .WithAnyArguments()
                .DoesNothing();

            var iocContainer = new IocFactoryForTest(null, folder);

            var settingsLoader = A.Fake<ISettingsLoader>();
            A.CallTo(() => settingsLoader.Save(null))
                .WithAnyArguments()
                .DoesNothing();

            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .Returns(file1);

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                settingsLoader,
                null,
                new InstallationVersionService(settings, fileSystemService)
                );

            productFolderService.EnsureProductFolder();

            productFolderService.IsProductFolderSet.Should().BeTrue();
            A.CallTo(() => folder.Exists)
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => folder.Create())
                .MustHaveHappened(Repeated.Exactly.Once);
        }
        /// <summary>
        /// If settings.ProductInstanceFolder is not set then it should be retrieved from snap installer path
        /// </summary>
        [Fact]
        public void EnsureProductFolder_02()
        {
            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.ProductInstanceFolder)
                .Returns(null);

            var iocContainer = new IocFactoryForTest(null, null);

            var settingsLoader = A.Fake<ISettingsLoader>();
            A.CallTo(() => settingsLoader.Save(null))
                .WithAnyArguments()
                .DoesNothing();

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.CalculateProductInstallationPath())
                .Returns(null);

            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .Returns(file1);

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                settingsLoader,
                fileSystemService,
                new InstallationVersionService(settings, fileSystemService)
                );

            productFolderService.EnsureProductFolder();

            productFolderService.IsProductFolderSet.Should().BeFalse();
            A.CallTo(() => fileSystemService.CalculateProductInstallationPath())
                .MustHaveHappened(Repeated.Exactly.Once);
        }
        /// <summary>
        /// If settings.ProductInstanceFolder is not set but it can be retrieved from file system then IsProductFolderSet should be true. 
        /// settings.ProductInstanceFolder should be set but the value should not be saved into the settings file.
        /// </summary>
        [Fact]
        public void EnsureProductFolder_03()
        {
            var settings = A.Fake<ISettings>();
            string productInstanceFolder = null;
            A.CallTo(settings)
                .WithReturnType<string>()
                .Where(fake => fake.Method.Name == "get_ProductInstanceFolder")
                .ReturnsNextFromSequence(new[] { null, "C:\\Prova\\Pippo" });
            A.CallTo(settings)
                .Where(fake => fake.Method.Name == "set_ProductInstanceFolder")
                .Invokes(fake => productInstanceFolder = (string)fake.Arguments[0]);

            A.CallTo(() => settings.SnapPackagesRegistryFolder)
                .Returns("C:\\Scambi");

            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(true);

            var iocContainer = new IocFactoryForTest(null, folder);

            var settingLoader = A.Fake<ISettingsLoader>();
            A.CallTo(() => settingLoader.Save(null))
                .WithAnyArguments()
                .DoesNothing();

            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .Returns(file1);
            A.CallTo(() => fileSystemService.CalculateProductInstallationPath())
                .Returns("C:\\Prova\\Pippo");

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                settingLoader,
                fileSystemService,
                new InstallationVersionService(settings, fileSystemService)
                );
            productFolderService.EnsureProductFolder();



            productFolderService.IsProductFolderSet.Should().BeTrue();

            productInstanceFolder.Should().Be("C:\\Prova\\Pippo");

            A.CallTo(() => settingLoader.Save(null))
                 .WithAnyArguments()
                 .MustNotHaveHappened();
        }

        /// <summary>
        /// If settings.ProductInstanceFolder is not set and cannot be retrieved from the registry but it can be retrieved from the file system then IsProductFolderSet should be true. 
        /// settings.ProductInstanceFolder should be set but the value should not be saved into the settings file.
        /// </summary>
        [Fact]
        public void EnsureProductFolder_04()
        {
            var settings = A.Fake<ISettings>();
            string productInstanceFolder = null;
            A.CallTo(settings)
                .WithReturnType<string>()
                .Where(fake => fake.Method.Name == "get_ProductInstanceFolder")
                .ReturnsNextFromSequence(new[] { null, "C:\\Prova\\Pippo" });
            A.CallTo(settings)
                .Where(fake => fake.Method.Name == "set_ProductInstanceFolder")
                .Invokes(fake => productInstanceFolder = (string)fake.Arguments[0]);

            var folder = A.Fake<IFolder>();
            A.CallTo(() => folder.Exists)
                .Returns(true);

            var iocContainer = new IocFactoryForTest(null, folder);

            var settingsLoader = A.Fake<ISettingsLoader>();
            A.CallTo(() => settingsLoader.Save(null))
                .WithAnyArguments()
                .DoesNothing();

            var fileSystemService = A.Fake<IFileSystemService>();
            A.CallTo(() => fileSystemService.CalculateProductInstallationPath())
                .Returns("C:\\Prova\\Pippo");

            byte[] buffer1 = System.Text.Encoding.Default.GetBytes("<?xml version=\"1.0\"?><InstallationVersion xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><ProductName>Magonet</ProductName><Version>3.9.0.0223</Version><BuildDate>20170223</BuildDate><InstallationDate>20151029125759</InstallationDate><CacheDate>20170302113435</CacheDate><Build>223</Build></InstallationVersion>");

            var inputStream1 = new MemoryStream(buffer1);
            inputStream1.Seek(0, SeekOrigin.Begin);

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.OpenRead())
                .Returns(inputStream1);

            A.CallTo(() => fileSystemService.InstallationVersionFile)
                .Returns(file1);

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                settingsLoader,
                fileSystemService,
                new InstallationVersionService(settings, fileSystemService)
                );

            productFolderService.EnsureProductFolder();

            productFolderService.IsProductFolderSet.Should().BeTrue();

            productInstanceFolder.Should().Be("C:\\Prova\\Pippo");

            A.CallTo(() => fileSystemService.CalculateProductInstallationPath())
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => settingsLoader.Save(null))
                 .WithAnyArguments()
                 .MustNotHaveHappened();
        }

        [Fact]
        public void ProductInstanceFolder_set_to_null_should_throw()
        {
            var productFolderSvc = new ProductFolderService(null, null, null, null, null);
            new Action(() => productFolderSvc.ProductInstanceFolder = null)
                .ShouldThrow<ProductFolderException>();
        }
        [Fact]
        public void ProductInstanceFolder_set_to_empty_should_throw()
        {
            var productFolderSvc = new ProductFolderService(null, null, null, null, null);
            new Action(() => productFolderSvc.ProductInstanceFolder = "")
                .ShouldThrow<ProductFolderException>();
        }
        [Fact]
        public void ProductInstanceFolder_set_to_empty_should_throw_01()
        {
            var productFolderSvc = new ProductFolderService(null, null, null, null, null);
            new Action(() => productFolderSvc.ProductInstanceFolder = "         ")
                .ShouldThrow<ProductFolderException>();
        }
        [Fact]
        public void ProductInstanceFolder_set_to_not_existing_folder_should_throw()
        {
            var productFolderSvc = new ProductFolderService(null, null, null, null, null);
            new Action(() => productFolderSvc.ProductInstanceFolder = "C:\\path\\to\\not\\existing\\folder")
                .ShouldThrow<ProductFolderException>();
        }

        [Fact]
        public void CalculatePackageRegistryFolder_the_path_should_also_contains_the_instance_name()
        {
            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.ProductInstanceFolder)
                .Returns(@"C:\Program Files (x86)\Microarea\LaMiaInstallazioneDiMagonet\\");

            var installationVersionService = A.Fake<IInstallationVersionService>();
            A.CallTo(() => installationVersionService.ProductName)
                .Returns("Magonet");
            A.CallTo(() => installationVersionService.Version)
                .Returns("3.13.1");

            var iocContainer = new IocFactoryForTest(null, null);

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                null,
                null,
                installationVersionService
                );

            productFolderService
                .CalculatePackageRegistryFolder()
                .Name
                .Should().Be(
                    "LaMiaInstallazioneDiMagonet-Magonet-3.13.1"
                    );
        }
        [Fact]
        public void CalculatePackageRegistryFolder_the_path_should_also_contains_the_instance_name_1()
        {
            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.ProductInstanceFolder)
                .Returns(@"C:\Program Files (x86)\Microarea\LaMiaInstallazioneDiMagonet");

            var installationVersionService = A.Fake<IInstallationVersionService>();
            A.CallTo(() => installationVersionService.ProductName)
                .Returns("Magonet");
            A.CallTo(() => installationVersionService.Version)
                .Returns("3.13.1");

            var iocContainer = new IocFactoryForTest(null, null);

            var productFolderService = new ProductFolderService(
                settings,
                iocContainer,
                null,
                null,
                installationVersionService
                );

            productFolderService
                .CalculatePackageRegistryFolder()
                .Name
                .Should().Be(
                    "LaMiaInstallazioneDiMagonet-Magonet-3.13.1"
                    );
        }
    }
}
