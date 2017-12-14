using System;
using System.Linq;
using System.Collections.Generic;
using Microarea.Snap.Core;
using Microarea.Snap.Services;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microarea.Snap.Installer;

namespace Microarea.Snap.Installer
{

    /// snapinstaller -l: elenca i pacchetti installati
    internal class ListCommand : Command
    {
        public ListCommand(TextWriter textWriter, IInversionOfControlFactory factory)
            : base(textWriter, factory)
        {}
        public override void Execute()
        {
            base.Execute();

            var packagesRegistry = Factory.GetInstance<IPackagesRegistry>();

            if (packagesRegistry.PackagesCount == 0)
            {
                TextWriter.WriteLine("No packages installed");
                return;
            }

            TextWriter.WriteLine("Installed packages:");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("Package ID (pid)\tversion");
            TextWriter.WriteLine("----------------\t-------");
            foreach (var p in packagesRegistry.InstalledPackages)
            {
                TextWriter.WriteLine(p.Manifest.Id + "\t\t\t" + p.Manifest.Version);
            }
        }
    }
}