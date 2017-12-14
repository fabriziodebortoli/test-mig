using System;
using System.Linq;
using System.Collections.Generic;
using Microarea.Snap.Core;
using Microarea.Snap.Services;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Installer
{

    /// snapinstaller -h: help
    internal class PrintHelpCommand : Command
    {
        public PrintHelpCommand(TextWriter textWriter, IInversionOfControlFactory factory)
            : base (textWriter, factory, ensureProductFolder: false)
        {
        }
        public override void Execute()
        {
            base.Execute();

            TextWriter.WriteLine("Syntax:\tsnapinstaller.exe [switch] [options]");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("Switches:");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("\t -cp\tCreate a package");
            TextWriter.WriteLine("\t -cp -h\tOptions for creating packages");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("\t -cm\tCreate a manifest");
            TextWriter.WriteLine("\t -cm -h\tOptions for creating manifests");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("\t -i\tInstall a package");
            TextWriter.WriteLine("\t -i -h\tOptions for installing packages");
            TextWriter.WriteLine("");
            //TextWriter.WriteLine("\t -u\tUninstall a package");
            //TextWriter.WriteLine("\t -u -h\tOptions for uninstalling packages");
            //TextWriter.WriteLine("");
            TextWriter.WriteLine("\t -l\tList all installed packages");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("\t -pf\t\t\tPrint current product folder");
            TextWriter.WriteLine("\t -pf <product folder>\tSet current product folder");
            TextWriter.WriteLine("");
            TextWriter.WriteLine("\t -h\tPrint this help");
        }
    }

}