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
    public class SnapPackageTester
    {
        [Fact]
        public void Init_should_throw_if_packageFilePath_is_null()
        {
            var p = new SnapPackage();

            new Action(() => p.Init(null, new SnapManifest()))
                .ShouldThrow<ArgumentNullException>("because 'packageFilePath' is null");
        }

        [Fact]
        public void Init_should_throw_if_packageFilePath_FullName_is_null()
        {
            var p = new SnapPackage();

            new Action(() => p.Init(new File(null), new SnapManifest()))
                .ShouldThrow<ArgumentNullException>("because 'packageFilePath' is null");
        }

        [Fact]
        public void Init_should_throw_if_packageFilePath_is_empty()
        {
            var p = new SnapPackage();

            new Action(() => p.Init(new File(string.Empty), new SnapManifest()))
                .ShouldThrow<ArgumentException>("because 'packageFilePath' is empty");
        }

        [Fact]
        public void Init_should_throw_if_packageFilePath_is_white_spaces()
        {
            var p = new SnapPackage();

            new Action(() => p.Init(new File("       "), new SnapManifest()))
                .ShouldThrow<ArgumentException>("because 'packageFilePath' is white spaces");
        }

        [Fact]
        public void Init_should_throw_if_packageFilePath_does_not_exist()
        {
            var p = new SnapPackage();

            new Action(() => p.Init(new File("C:\\file\\che\\non\\esiste.txt"), new SnapManifest()))
                .ShouldThrow<ArgumentException>("because 'packageFilePath' does not exist");
        }

        [Fact]
        public void Init_should_throw_if_manifest_is_null()
        {
            var p = new SnapPackage();

            var existingFile = this.GetType().Assembly.Location;

            new Action(() => p.Init(new File(existingFile), null))
                .ShouldThrow<ArgumentNullException>("because 'manifest' does not exist");
        }
    }
}
