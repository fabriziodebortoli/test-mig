using Microarea.Snap.Core;
using Microarea.Snap.IO;
using Microarea.Snap.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microarea.Snap.Installer
{

    /// snapinstaller -cp -m manifestfile -o outputfile: crea un pacchetto a  partire da un manifest
    /// snapinstaller -cp -h: help
    internal class CreatePackageCommand : Command
    {
        const string rootFolderArg = "-F";
        const string manifestFileArg = "-M";
        const string outputFileArg = "-O";
        string rootFolder;
        string manifestFile;
        string outputFile;
        bool rootFolderFound;
        bool manifestFileFound;
        bool outputFileFound;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public CreatePackageCommand(IEnumerable<string> args, System.IO.TextWriter textWriter, IInversionOfControlFactory factory)
            : base(textWriter, factory, ensureProductFolder: false)
        {
            try
            {
                for (int i = 0; i < args.Count(); i += 2)
                {
                    switch (args.ElementAt(i).ToUpperInvariant())
                    {
                        case rootFolderArg:
                            rootFolder = args.ElementAt(i + 1);
                            rootFolderFound = true;
                            break;
                        case manifestFileArg:
                            manifestFile = args.ElementAt(i + 1);
                            manifestFileFound = true;
                            break;
                        case outputFileArg:
                            outputFile = args.ElementAt(i + 1);
                            outputFileFound = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                TextWriter.WriteLine("Error during CreatePackage command: " + exc.ToString());
            }
        }
        public override void Execute()
        {
            base.Execute();

            if (manifestFileFound && outputFileFound && rootFolderFound)
            {
                var packetManager = Factory.GetInstance<IPackageManager>();

                var manifest = new File(manifestFile);
                if (!manifest.IsPathRooted)
                {
                    var uri = new UriBuilder(this.GetType().Assembly.CodeBase);
                    string basePath = System.IO.Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

                    manifest = new File(System.IO.Path.Combine(basePath, manifestFile));
                }

                packetManager.CreatePackage(manifest, new Folder(rootFolder), new File(outputFile));
            }
            else
            {
                this.PrintHeader();
                TextWriter.WriteLine("Syntax:\tsnapinstaller.exe -cp [options]");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("Options:");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("\t -m manifestfile\tpath to the manifest to create a package with");
                TextWriter.WriteLine("\t -f rootfolder\t\tpath to the root folder to create the package");
                TextWriter.WriteLine("\t -o outputfile\t\toutput file");
                TextWriter.WriteLine("\t -h\t\t\tPrint this help");
            }
        }
    }

}