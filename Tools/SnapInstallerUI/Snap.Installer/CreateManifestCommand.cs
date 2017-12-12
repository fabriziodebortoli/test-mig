using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;

namespace Microarea.Snap.Installer
{

    /// snapinstaller -cm -i input.xml -f folder -o manifest.xml: crea il file manifest.xml esplorando 'folder' secondo le indicazioni del file input.xml
    /// snapinstaller -cm -h: help
    internal class CreateManifestCommand : Command
    {
        const string productVersionArg = "-PV";
        const string buildArg = "-B";
        const string folderArg = "-F";
        const string outputFileArg = "-O";
        const string inputFileArg = "-I";
        string productVersion;
        string build;
        string folder;
        string outputFile;
        string inputFile;
        bool productVersionFound;
        bool buildFound;
        bool inputFileFound;
        bool folderFound;
        bool outputFileFound;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public CreateManifestCommand(IEnumerable<string> args, System.IO.TextWriter textWriter, IInversionOfControlFactory factory)
            : base(textWriter, factory, ensureProductFolder: false)
        {
            try
            {
                for (int i = 0; i < args.Count(); i += 2)
                {
                    switch (args.ElementAt(i).ToUpperInvariant())
                    {
                        case productVersionArg:
                            productVersion = args.ElementAt(i + 1);
                            productVersionFound = true;
                            break;
                        case buildArg:
                            build = args.ElementAt(i + 1);
                            buildFound = true;
                            break;
                        case folderArg:
                            folder = args.ElementAt(i + 1);
                            folderFound = true;
                            break;
                        case outputFileArg:
                            outputFile = args.ElementAt(i + 1);
                            outputFileFound = true;
                            break;
                        case inputFileArg:
                            inputFile = args.ElementAt(i + 1);
                            inputFileFound = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                TextWriter.WriteLine("Error during 'CreateManifest' command: " + exc.ToString());
            }
        }
        public override void Execute()
        {
            base.Execute();

            if (folderFound && outputFileFound && inputFileFound && buildFound && productVersionFound)
            {
                var manifestManager = Factory.GetInstance<IManifestManager>();
                manifestManager.CreateManifest(new Folder(folder), new File(outputFile), new File(inputFile), build, productVersion);
            }
            else
            {
                this.PrintHeader();
                TextWriter.WriteLine("Syntax:\tsnapinstaller.exe -cm [options]");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("Options:");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("\t -f folder\tpath to the folder to create the manifest");
                TextWriter.WriteLine("\t -i input.xml\tpath to the input file to browse 'folder'");
                TextWriter.WriteLine("\t -o output file\tmanifest file to create");
                TextWriter.WriteLine("\t -b build number for the manifest");
                TextWriter.WriteLine("\t -pv version of the supported product (e.g.: 3.13.0)");
                TextWriter.WriteLine("\t -h\t\tPrint this help");
            }
        }
    }

}