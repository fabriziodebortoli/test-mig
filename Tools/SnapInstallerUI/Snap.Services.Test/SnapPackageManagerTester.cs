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
using Ionic.Zip;

namespace Microarea.Snap.Services.Test
{
    public class SnapPackageManagerTester
    {
        [Fact]
        public void Uninstall_should_throw_if_package_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Uninstall(null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Uninstall_should_throw_if_rootFolder_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Uninstall(new SnapPackage(), null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Uninstall_should_throw_if_manifest_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Uninstall(new SnapPackage(), new Folder("C:\\folder")))
                .ShouldThrow<PackageException>();
        }

        [Fact]
        public void Uninstall_should_uninstall_all_files()
        {
            var folder = A.Fake<Folder>();
            A.CallTo(() => folder.FullName)
                .Returns("C:\\Folder");

            var file1 = A.Fake<File>();
            A.CallTo(() => file1.FullName)
                .Returns("installed\\prova.txt");

            var file2 = A.Fake<File>();
            A.CallTo(() => file2.FullName)
                .Returns("installed\\bin\\licenza.txt");

            var manifest = A.Fake<SnapManifest>();
            A.CallTo(() => manifest.Files)
                .Returns(new List<IFile>() { file1, file2 });

            var package = A.Fake<SnapPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var registryService = A.Fake<IRegistryService>();
            A.CallTo(() => registryService.RetrieveInstalledDictionaries())
                .Returns(new string[] { });

            var m = new SnapPackageManager(null, registryService);
            m.Uninstall(package, folder);

            A.CallTo(() => file1.Delete(folder))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => file2.Delete(folder))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Uninstall_should_uninstall_all_files_path_rooted()
        {
            var folder = A.Fake<Folder>();
            A.CallTo(() => folder.FullName)
                .Returns("C:\\Folder");

            var file1 = A.Fake<File>();
            A.CallTo(() => file1.FullName)
                .Returns("C:\\installed\\prova.txt");
            A.CallTo(() => file1.IsPathRooted)
                .Returns(true);

            var file2 = A.Fake<File>();
            A.CallTo(() => file2.FullName)
                .Returns("C:\\installed\\bin\\licenza.txt");
            A.CallTo(() => file2.IsPathRooted)
                .Returns(true);

            var manifest = A.Fake<SnapManifest>();
            A.CallTo(() => manifest.Files)
                .Returns(new List<IFile>() { file1, file2 });

            var package = A.Fake<SnapPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var registryService = A.Fake<IRegistryService>();
            A.CallTo(() => registryService.RetrieveInstalledDictionaries())
                .Returns(new string[] { });

            var m = new SnapPackageManager(null, registryService);
            m.Uninstall(package, folder);

            A.CallTo(() => file1.Delete())
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => file2.Delete())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Uninstall_should_not_delete_a_folder_if_it_has_a_subfolder_all_files()
        {
            var folder = A.Fake<Folder>();
            A.CallTo(() => folder.FullName)
                .Returns("C:\\Folder");
            A.CallTo(() => folder.IsEmpty())
                .WithAnyArguments()
                .Returns(false);

            var file1 = A.Fake<File>();
            A.CallTo(() => file1.FullName)
                .Returns("installed\\prova.txt");
            A.CallTo(() => file1.ParentFolder)
                .Returns(folder);

            var file2 = A.Fake<File>();
            A.CallTo(() => file2.FullName)
                .Returns("installed\\bin\\licenza.txt");
            A.CallTo(() => file2.ParentFolder)
                .Returns(folder);

            var manifest = A.Fake<SnapManifest>();
            A.CallTo(() => manifest.Files)
                .Returns(new List<IFile>() { file1, file2 });

            var subfolder = A.Fake<Folder>();
            A.CallTo(() => subfolder.FullName)
                .Returns("C:\\Folder\\subfolder");
            A.CallTo(() => subfolder.Delete(new Folder("")))
                .WithAnyArguments()
                .DoesNothing();
            A.CallTo(() => subfolder.ParentFolder)
               .Returns(folder);

            A.CallTo(() => folder.GetFolders(new Folder(""), "*"))
                .Returns(new List<IFolder>() { subfolder }.ToArray());

            var package = A.Fake<SnapPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var registryService = A.Fake<IRegistryService>();
            A.CallTo(() => registryService.RetrieveInstalledDictionaries())
                .Returns(new string[] { });

            var m = new SnapPackageManager(null, registryService);
            m.Uninstall(package, folder);

            A.CallTo(() => file1.Delete(folder))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => file2.Delete(folder))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => folder.Delete(new Folder("")))
                .WithAnyArguments()
                .MustNotHaveHappened();
        }

        [Fact]
        public void CreatePackage_should_throw_if_manifest_file_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.CreatePackage(null, null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreatePackage_should_throw_if_rootFolder_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.CreatePackage(new File("C:\\prova.txt"), null, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreatePackage_should_throw_if_rootFolder_does_not_exist()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.CreatePackage(new File("C:\\prova.txt"), new Folder("C:\\Path\\to\\not\\existing\\folder"), null))
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CreatePackage_should_throw_if_outputFile_is_null()
        {
            var rootFolder = A.Fake<IFolder>();
            A.CallTo(() => rootFolder.Exists)
                .Returns(true);

            var m = new SnapPackageManager(null, null);
            new Action(() => m.CreatePackage(new File("C:\\prova.txt"), rootFolder, null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreatePackage_should_create_a_package()
        {
            var outputFile = A.Fake<File>();
            A.CallTo(() => outputFile.FullName)
                .Returns("C:\\test.zip");
            A.CallTo(() => outputFile.Exists)
                .Returns(true);
            var output = new System.IO.MemoryStream();
            A.CallTo(() => outputFile.OpenWrite())
                .Returns(output);

            var rootFolder = A.Fake<Folder>();
            A.CallTo(() => rootFolder.FullName)
                .Returns("C:\\Dev");

            var file1 = A.Fake<IFile>();
            A.CallTo(() => file1.FullName)
                .Returns("C:\\Dev\\installed\\prova.txt");
            var file2 = A.Fake<IFile>();
            A.CallTo(() => file2.FullName)
                .Returns("C:\\Dev\\installed\\bin\\licenza.txt");

            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Files)
                .Returns(new List<IFile>() { file1, file2 });
            var manifestFile = A.Fake<IFile>();
            A.CallTo(() => manifestFile.Name)
                .Returns("RanorexTestSupport.tbfpspec");

            var package = SnapPackageManager.CreatePackage(manifest, manifestFile, rootFolder, outputFile);

            byte[] outputByteArray = output.ToArray();

            outputByteArray.Length.Should().BeGreaterThan(0);
            package.Should().NotBeNull();
            package.Manifest.Should().BeSameAs(manifest);
            package.PackageFile.Should().BeSameAs(outputFile);

            using (ZipFile zip = ZipFile.Read(new System.IO.MemoryStream(outputByteArray)))
            {
                foreach (ZipEntry e in zip)
                {
                    (e.FileName == "installed/prova.txt" || e.FileName == "installed/bin/licenza.txt" || e.FileName == "RanorexTestSupport.tbfpspec").Should().BeTrue();
                }
            }
        }

        [Fact]
        public void CreatePackage()
        {
            var outputFile = A.Fake<IFile>();
            A.CallTo(() => outputFile.FullName)
                .Returns("C:\\RanorexTestSupport.tbfp");
            A.CallTo(() => outputFile.Exists)
                .Returns(true);
            var output = new System.IO.MemoryStream();
            A.CallTo(() => outputFile.OpenWrite())
                .Returns(output);

            var manifestFile = A.Fake<IFile>();
            A.CallTo(() => manifestFile.FullName)
                .Returns("C:\\Development_1x\\RanorexTestSupport.tbfpspec");
            A.CallTo(() => manifestFile.Name)
                .Returns("RanorexTestSupport.tbfpspec");
            A.CallTo(() => manifestFile.ParentFolder)
                .Returns(new Folder("C:\\Development_1x"));

            var file1 = A.Fake<File>();
            A.CallTo(() => file1.FullName)
                .Returns("installed\\prova.txt");
            A.CallTo(() => file1.OpenRead())
                .WithAnyArguments()
                .Returns(new System.IO.MemoryStream(new byte[] { 1, 16, 23, 52, 12 }));

            var file2 = A.Fake<File>();
            A.CallTo(() => file2.FullName)
                .Returns("installed\\bin\\licenza.txt");
            A.CallTo(() => file2.OpenRead())
                .WithAnyArguments()
                .Returns(new System.IO.MemoryStream(new byte[] { 1, 16, 23, 52, 12 }));

            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Files)
                .Returns(new List<IFile>() { file1, file2 });

            var package = SnapPackageManager.CreatePackage(manifest, manifestFile, manifestFile.ParentFolder, outputFile);

            output.ToArray().Should().NotBeNull();
            output.ToArray().Length.Should().BeGreaterThan(0);
            package.Manifest.Should().BeSameAs(manifest);
            package.PackageFile.Should().BeSameAs(outputFile);
            A.CallTo(() => file1.OpenRead(null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => file2.OpenRead(null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        //[Fact]
        //public void InstallPackage()
        //{
        //    var manifest = new SnapManifest();
        //    manifest.Init(new File("C:\\Development_1x\\RanorexTestSupport.tbfpspec").OpenRead());

        //    var package = new SnapPackage();
        //    package.Init(new File("C:\\RanorexTestSupport.tbfp"), manifest);

        //    var m = new SnapPackageManager(new Settings(), new RegistryService());
        //    m.Install(package, new Folder("C:\\Dev"));
        //}

        [Fact]
        public void UninstallPackage()
        {
            var file1 = A.Fake<File>();
            A.CallTo(() => file1.FullName)
                .Returns("installed\\prova.txt");
            A.CallTo(() => file1.IsPathRooted)
                .Returns(false);
            A.CallTo(() => file1.Delete(new Folder("C:\\")))
                .WithAnyArguments();

            var file2 = A.Fake<File>();
            A.CallTo(() => file2.FullName)
                .Returns("installed\\bin\\licenza.txt");
            A.CallTo(() => file2.IsPathRooted)
                .Returns(false);
            A.CallTo(() => file2.Delete(new Folder("C:\\")))
                .WithAnyArguments();

            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Files)
                .Returns(new List<IFile>() { file1, file2 });

            var package = A.Fake<IPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var m = new SnapPackageManager(null, null);
            m.Uninstall(package, new Folder("C:\\Dev"));


            A.CallTo(() => file1.Delete(new Folder("C:\\")))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => file2.Delete(new Folder("C:\\")))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Install_should_throw_if_package_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Install(null, null))
                .ShouldThrow<ArgumentNullException>();
        }
        [Fact]
        public void Install_should_throw_if_rootFolder_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Install(new SnapPackage(), null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Install_with_dictionaries_should_not_throw_exception()
        {
            var r = A.Fake<IRegistryService>();
            A.CallTo(() => r.RetrieveInstalledDictionaries())
                .Returns(new string[] { "it-IT" });

            var rootFolder = new Folder(System.IO.Path.Combine(System.IO.Path.GetTempPath(), new Random().Next(0000, 9999).ToString()));
            if (!rootFolder.Exists)
            {
                rootFolder.Create();
            }

            var memoryStream1 = new System.IO.MemoryStream();
            System.IO.Stream inputStream1 = this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.MobileWebServicesProvider-3.13.1.P006.0422.tbfp");
            byte[] buffer1 = new byte[inputStream1.Length];
            inputStream1.Read(buffer1, 0, buffer1.Length);
            memoryStream1.Write(buffer1, 0, buffer1.Length);
            memoryStream1.Seek(0, System.IO.SeekOrigin.Begin);

            var memoryStream2 = new System.IO.MemoryStream();
            System.IO.Stream inputStream2 = this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.MobileWebServicesProvider-3.13.1.P006.0422.tbfp");
            byte[] buffer2 = new byte[inputStream2.Length];
            inputStream2.Read(buffer2, 0, buffer2.Length);
            memoryStream2.Write(buffer2, 0, buffer2.Length);
            memoryStream2.Seek(0, System.IO.SeekOrigin.Begin);


            var packageFile = A.Fake<IFile>();
            A.CallTo(() => packageFile.OpenRead())
                .ReturnsNextFromSequence(memoryStream1, memoryStream2);
            A.CallTo(() => packageFile.FullName)
                .Returns(@"C:\Development_3x\Standard\TaskBuilder\Tools\SnapInstallerUI\Snap.Services.Test\res\MobileWebServicesProvider-3.13.1.P006.0422.tbfp");
            A.CallTo(() => packageFile.Exists)
                .Returns(true);
            var pl = new SnapPackageLoader();
            var package = pl.LoadPackage(packageFile);

            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.WorkingFolder)
                .Returns(@"C:\Users\Canessa\AppData\Local\Temp\SnapInstaller\TaskBuilderNet\working");

            var m = new SnapPackageManager(settings, r);
            var action = new Action(() =>m.Install(package, rootFolder));

            action.ShouldNotThrow<System.IO.DirectoryNotFoundException>();

            A.CallTo(() => r.RetrieveInstalledDictionaries())
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Update_should_throw_if_oldPackage_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Update(null, null, null))
                .ShouldThrow<ArgumentNullException>();
        }
        [Fact]
        public void Update_should_throw_if_newPackage_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Update(new SnapPackage(), null, null))
                .ShouldThrow<ArgumentNullException>();
        }
        [Fact]
        public void Update_should_throw_if_rootFolder_is_null()
        {
            var m = new SnapPackageManager(null, null);
            new Action(() => m.Update(new SnapPackage(), new SnapPackage(), null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Update_should_delete_old_files_no_more_present_in_the_new_package()
        {
            var oldFile1 = A.Fake<IFile>();
            A.CallTo(() => oldFile1.FullName)
                .Returns("bin\\pippo.txt");//comune al nuovo package, deve essere sovrascritto
            var oldFile2 = A.Fake<IFile>();
            A.CallTo(() => oldFile2.FullName)
                .Returns("res\\pluto.bin");//non comune al nuovo package, deve essere cancellato
            A.CallTo(() => oldFile2.Delete(null))
                .WithAnyArguments()
                .DoesNothing();

            var oldManifest = A.Fake<IManifest>();
            A.CallTo(() => oldManifest.Files)
                .Returns(new List<IFile>() { oldFile1, oldFile2 });

            var oldPackage = A.Fake<IPackage>();
            A.CallTo(() => oldPackage.Manifest)
                .Returns(oldManifest);

            var newFile1 = A.Fake<IFile>();
            A.CallTo(() => newFile1.FullName)
                .Returns("bin\\pippo.txt");//comune al vecchio package
            var newFile2 = A.Fake<IFile>();
            A.CallTo(() => newFile2.FullName)
                .Returns("res\\paperino.toc");//non comune al vecchio package

            var newManifest = A.Fake<IManifest>();
            A.CallTo(() => newManifest.Files)
                .Returns(new List<IFile>() { newFile1, newFile2 });
            A.CallTo(() => newManifest.Id)
                .Returns("idTest");

            var newPackage = A.Fake<IPackage>();
            A.CallTo(() => newPackage.Manifest)
                .Returns(newManifest);

            var newPackagePackageFile = A.Fake<IFile>();
            A.CallTo(() => newPackagePackageFile.OpenRead())
                .Returns(this.GetType().Assembly.GetManifestResourceStream(this.GetType().Namespace + ".res.Cadi.tbfp"));

            A.CallTo(() => newPackage.PackageFile)
                .Returns(newPackagePackageFile);

            var workingFolderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testWorkingfolder");
            var settings = A.Fake<ISettings>();
            A.CallTo(() => settings.WorkingFolder)
                .Returns(workingFolderPath);
            if (!System.IO.Directory.Exists(workingFolderPath))
            {
                System.IO.Directory.CreateDirectory(workingFolderPath);
            }

            var rootFolderPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "testRootFolder");
            var rootFolder = A.Fake<IFolder>();
            A.CallTo(() => rootFolder.FullName)
                .Returns(rootFolderPath);
            if (!System.IO.Directory.Exists(rootFolderPath))
            {
                System.IO.Directory.CreateDirectory(rootFolderPath);
            }

            var registryService = A.Fake<IRegistryService>();
            A.CallTo(() => registryService.RetrieveInstalledDictionaries())
                .Returns(new string[] { });

            var m = new SnapPackageManager(settings, registryService);
            m.Update(oldPackage, newPackage, rootFolder);


            A.CallTo(() => oldFile2.Delete(null))
                .WithAnyArguments()
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => oldFile1.Delete(null))
                .WithAnyArguments()
                .MustNotHaveHappened();
        }

        [Fact]
        public void GetDictionary_should_return_null_if_null_is_passed_in()
        {
            var expected = SnapPackageManager.GetDictionary(null);

            expected.Should().BeNull();
        }
        [Fact]
        public void GetDictionary_should_return_null_if_string_empty_is_passed_in()
        {
            var expected = SnapPackageManager.GetDictionary(string.Empty);

            expected.Should().BeNull();
        }
        [Fact]
        public void GetDictionary_should_return_null_if_white_spaces_string_is_passed_in()
        {
            var expected = SnapPackageManager.GetDictionary("       ");

            expected.Should().BeNull();
        }
        [Fact]
        public void GetDictionary_should_return_null_if_no_dict_string_is_passed_in()
        {
            var expected = SnapPackageManager.GetDictionary("/prova/pippo");

            expected.Should().BeNull();
        }
        [Fact]
        public void GetDictionary_should_return_the_dictionary_string_if_dict_string_is_passed_in()
        {
            var expected = SnapPackageManager.GetDictionary("/prova/bg-BG/prova");

            expected.Should().Be("bg-BG");
        }
        [Fact]
        public void GetDictionary_should_return_the_dictionary_string_if_dict_string_is_passed_in_1()
        {
            var expected = SnapPackageManager.GetDictionary("/prova/zh-CHS/prova");

            expected.Should().Be("zh-CHS");
        }

        [Fact]
        public void GetDictionary_should_return_null_if_the_string_is_passed_in_is_contains_an_hyphen()
        {
            var expected = SnapPackageManager.GetDictionary("/prova/Manufacturing-Advanced/prova");

            expected.Should().BeNull();
        }
    }
}
