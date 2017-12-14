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
    public class ProductCompatibilityServiceTester
    {
        [Fact]
        public void EnsureProductCompatibility_should_throw_if_package_is_null()
        {
            var p = new ProductCompatibilityService(null);
            new Action(() => p.EnsureProductCompatibility(null))
                .ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void EnsureProductCompatibility_should_throw_if_product_is_different()
        {
            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Product)
                .Returns("Mago.net");

            var package = A.Fake<IPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var i = A.Fake<IInstallationVersionService>();
            A.CallTo(() => i.ProductName)
                .Returns("Mago4");

            var p = new ProductCompatibilityService(i);
            new Action(() => p.EnsureProductCompatibility(package))
                .ShouldThrow<PackageException>();
        }

        [Fact]
        public void EnsureProductCompatibility_should_throw_if_product_version_is_different()
        {
            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Product)
                .Returns("Mago4");
            A.CallTo(() => manifest.ProductVersion)
                .Returns("1.4.1.302");

            var package = A.Fake<IPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var i = A.Fake<IInstallationVersionService>();
            A.CallTo(() => i.ProductName)
                .Returns("Mago4");
            A.CallTo(() => i.Version)
                .Returns("1.3.1.30");

            var p = new ProductCompatibilityService(i);
            new Action(() => p.EnsureProductCompatibility(package))
                .ShouldThrow<PackageException>();
        }

        [Fact]
        public void EnsureProductCompatibility_should_not_throw_if_product_and_product_version_are_equals()
        {
            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Product)
                .Returns("Mago4");
            A.CallTo(() => manifest.ProductVersion)
                .Returns("1.4.1.302");

            var package = A.Fake<IPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var i = A.Fake<IInstallationVersionService>();
            A.CallTo(() => i.ProductName)
                .Returns("Mago4");
            A.CallTo(() => i.Version)
                .Returns("1.4.1.302");

            var p = new ProductCompatibilityService(i);
            new Action(() => p.EnsureProductCompatibility(package))
                .ShouldNotThrow<PackageException>();
        }

        [Fact]
        public void EnsureProductCompatibility_should_not_throw_if_product_differs_only_for_dots()
        {
            var manifest = A.Fake<IManifest>();
            A.CallTo(() => manifest.Product)
                .Returns("Mago.net");
            A.CallTo(() => manifest.ProductVersion)
                .Returns("3.13.2.2");

            var package = A.Fake<IPackage>();
            A.CallTo(() => package.Manifest)
                .Returns(manifest);

            var i = A.Fake<IInstallationVersionService>();
            A.CallTo(() => i.ProductName)
                .Returns("Magonet");
            A.CallTo(() => i.Version)
                .Returns("3.13.2.2");

            var p = new ProductCompatibilityService(i);
            new Action(() => p.EnsureProductCompatibility(package))
                .ShouldNotThrow<PackageException>();
        }
    }
}
