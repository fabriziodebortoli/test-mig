using Microarea.Snap.Core;
using Microarea.Snap.Installer.Properties;
using Microarea.Snap.Services;
using System;
using System.IO;
using System.Linq;

namespace Microarea.Snap.Installer
{
    class Program
    {
        internal const string LogFileName = "SnapInstaller.log";

        const string createPackageArg = "-CP";
        const string createManifestArg = "-CM";
        const string installArg = "-I";
        const string uninstallArg = "-U";
        const string listArg = "-L";
        const string helpArg = "-H";
        const string productFolderArg = "-PF";
        const string waitForExitArg = "-WAIT";

        static void Main(string[] args)
        {
            using (var iocFactory = new IocFactory())
            {
                RefreshLogConfiguration(iocFactory);

                bool waitForExit = false;

                ICommand command;
                if (args == null || args.Length == 0)
                {
                    command = new PrintHelpCommand(Console.Out, iocFactory);
                }
                else
                {
                    var parameters = args.Except(args.Take(1));
                    switch (args[0].ToUpperInvariant())
                    {
                        case createPackageArg:
                            command = new CreatePackageCommand(parameters, Console.Out, iocFactory);
                            break;
                        case createManifestArg:
                            command = new CreateManifestCommand(parameters, Console.Out, iocFactory);
                            break;
                        case installArg:
                            command = new InstallCommand(parameters, Console.Out, iocFactory);
                            break;
                        //case uninstallArg:
                        //    command = new UninstallCommand(parameters, Console.Out, iocFactory);
                        //    break;
                        case listArg:
                            command = new ListCommand(Console.Out, iocFactory);
                            break;
                        case productFolderArg:
                            command = new ProductFolderCommand(parameters, Console.Out, iocFactory);
                            break;
                        default:
                            command = new PrintHelpCommand(Console.Out, iocFactory);
                            break;
                    }

                    waitForExit = (parameters.Where(p => string.Compare(waitForExitArg, p, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault() != null);
                }

                try
                {
                    command.Execute();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }

                if (waitForExit)
                {
                    Console.WriteLine(Resources.PressAnyKeyToContinue);
                    Console.ReadLine();
                }
            }
        }

        static void RefreshLogConfiguration(IocFactory iocFactory)
        {
            var settings = iocFactory.GetInstance<ISettings>();
            var logsFolder = iocFactory.GetInstance<IO.IFolder, string>(settings.LogsFolder);
            if (!logsFolder.Exists)
            {
                logsFolder.Create();
            }

            var target = NLog.LogManager.Configuration.FindTargetByName("logfile") as NLog.Targets.FileTarget;
            if (target != null)
            {
                target.FileName = Path.Combine(logsFolder.FullName, LogFileName);
                NLog.LogManager.ReconfigExistingLoggers();
            }
        }
    }
}
