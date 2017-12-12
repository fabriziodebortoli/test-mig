using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;
using FluentAssertions;

namespace Microarea.Snap.Services.Test
{
    public class FileSystemServiceTester
    {
        [Fact]
        public void CalculateProductInstallationPath_should_throw_argument_exception_if_the_given_path_is_empty()
        {
            var p = A.Fake<IPathProviderService>();
            A.CallTo(() => p.RetrievePathToCalculateOn())
                .Returns(string.Empty);

            var f = new FileSystemService(p, null);

            new Action(() => f.CalculateProductInstallationPath())
                .ShouldThrow<ArgumentException>();
        }
        [Fact]
        public void CalculateProductInstallationPath_should_throw_argument_exception_if_the_given_path_is_empty_01()
        {
            var p = A.Fake<IPathProviderService>();
            A.CallTo(() => p.RetrievePathToCalculateOn())
                .Returns("          ");

            var f = new FileSystemService(p, null);

            new Action(() => f.CalculateProductInstallationPath())
                .ShouldThrow<ArgumentException>();
        }
        [Fact]
        public void CalculateProductInstallationPath_should_throw_argument_exception_if_the_given_path_is_not_rooted()
        {
            var p = A.Fake<IPathProviderService>();
            A.CallTo(() => p.RetrievePathToCalculateOn())
                .Returns("prova\\Mago");

            var f = new FileSystemService(p, null);

            new Action(() => f.CalculateProductInstallationPath())
                .ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void CalculateProductInstallationPath_should_return_the_installation_path_if_the_given_path_is_correct()
        {
            var p = A.Fake<IPathProviderService>();
            A.CallTo(() => p.RetrievePathToCalculateOn())
                .Returns(@"C:\Microarea\M4\MN313\Apps\SnapInstaller\SnapInstaller.exe");

            var f = new FileSystemService(p, null);

            var res = f.CalculateProductInstallationPath();
            res.Should().Be(@"C:\Microarea\M4\MN313");
        }
        [Fact]
        public void CalculateProductInstallationPath_should_return_null_if_the_given_path_is_not_correct()
        {
            var p = A.Fake<IPathProviderService>();
            A.CallTo(() => p.RetrievePathToCalculateOn())
                .Returns(@"C:\Development\Standard\TaskBuilder\Tools\SnapInstallerUI\Snap.Installer.UI\bin\x86\Debug\SnapInstaller.exe");

            var f = new FileSystemService(p, null);

            var res = f.CalculateProductInstallationPath();
            res.Should().BeNull();
        }
    }
}
