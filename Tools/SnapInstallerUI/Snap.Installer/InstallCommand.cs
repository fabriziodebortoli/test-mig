using Microarea.Snap.Core;
using Microarea.Snap.Installer.Properties;
using Microarea.Snap.IO;
using Microarea.Snap.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microarea.Snap.Installer
{

    /// snapinstaller -i -p packagefile: installa un pacchetto
    /// snapinstaller -i -h: help
    internal class InstallCommand : Command, ILogger
    {
        const string packageFileArg = "-P";
        string packageFilePath;
        bool packageFileFound;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public InstallCommand(IEnumerable<string> args, TextWriter textWriter, IInversionOfControlFactory factory)
            : base(textWriter, factory)
        {
            try
            {
                for (int i = 0; i < args.Count(); i += 2)
                {
                    switch (args.ElementAt(i).ToUpperInvariant())
                    {
                        case packageFileArg:
                            packageFilePath = args.ElementAt(i + 1);
                            packageFileFound = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                TextWriter.WriteLine("Error during 'Install' command: " + exc.ToString());
            }
        }
        public override void Execute()
        {
            base.Execute();

            if (packageFileFound)
            {
                var packageLoader = Factory.GetInstance<IPackageLoader>();

                var packageFile = Factory.GetInstance<IFile, string>(packageFilePath);
                if (!packageFile.Exists)
                {
                    TextWriter.WriteLine("Package file " + packageFilePath + " does not exist.");
                    return;
                }
                var package = packageLoader.LoadPackage(packageFile);

                var installerService = Factory.GetInstance<IInstallerService>();
                ListenTo(installerService);

                var settings = Factory.GetInstance<ISettings>();
                var productInstanceFolder = Factory.GetInstance<IFolder, string>(settings.ProductInstanceFolder);

                installerService.Install(package, productInstanceFolder);
                //Let the installer thread start
                System.Threading.Thread.Sleep(1000);
                installerService.Join();
                UnlistenTo(installerService);
            }
            else
            {
                TextWriter.WriteLine("Syntax:\tsnapinstaller.exe -i [options]");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("Options:");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("\t -p packagefile\tpath to the package file to install");
                TextWriter.WriteLine("\t -h\t\tPrint this help");
            }
        }

        private void UnlistenTo(IInstallerService installerService)
        {
            installerService.ErrorOccurred -= InstallerService_ErrorOccurred;
            installerService.Installed -= InstallerService_Installed;
            installerService.Installing -= InstallerService_Installing;
            installerService.Updated -= InstallerService_Updated;
            installerService.Updating -= InstallerService_Updating;
            installerService.Notification -= InstallerService_Notification;
            installerService.Starting -= InstallerService_Starting;
            installerService.Started -= InstallerService_Started;
            installerService.Stopping -= InstallerService_Stopping;
            installerService.Stopped -= InstallerService_Stopped;
        }

        private void ListenTo(IInstallerService installerService)
        {
            installerService.ErrorOccurred += InstallerService_ErrorOccurred;
            installerService.Installed += InstallerService_Installed;
            installerService.Installing += InstallerService_Installing;
            installerService.Updated += InstallerService_Updated;
            installerService.Updating += InstallerService_Updating;
            installerService.Notification += InstallerService_Notification;
            installerService.Starting += InstallerService_Starting;
            installerService.Started += InstallerService_Started;
            installerService.Stopping += InstallerService_Stopping;
            installerService.Stopped += InstallerService_Stopped;
        }

        private void InstallerService_Stopped(object sender, EventArgs e)
        {
            this.LogInfo(Resources.InstallerServiceStopped);
            this.LogInfo(Resources.Separator);
        }

        private void InstallerService_Stopping(object sender, EventArgs e)
        {
            this.LogInfo(Resources.StoppingInstallerService);
        }

        private void InstallerService_Started(object sender, EventArgs e)
        {
            this.LogInfo(Resources.InstallerServiceStarted);
        }

        private void InstallerService_Starting(object sender, EventArgs e)
        {
            this.LogInfo(Resources.Separator);

            var invariantCulture = CultureInfo.InvariantCulture;
            var snapInstallerVersionService = Factory.GetInstance<ISnapInstallerVersionService>();
            if (snapInstallerVersionService != null)
            {
                this.LogInfo(string.Format(invariantCulture, Resources.SnapInstallerHeader, snapInstallerVersionService.SnapInstallerProductName, snapInstallerVersionService.SnapInstallerVersion));
                this.LogInfo(string.Empty);
            }

            var systemInfoService = Factory.GetInstance<ISystemInfoService>();
            if (systemInfoService != null)
            {
                this.LogInfo(string.Format(invariantCulture, Resources.OperatingSystem, systemInfoService.OperatingSystem));
                this.LogInfo(string.Format(invariantCulture, Resources.NetFxVersion, systemInfoService.NetFxVersion));

                var settings = Factory.GetInstance<ISettings>();
                if (settings != null)
                {
                    this.LogInfo(string.Format(invariantCulture, Resources.WorkingIn, settings.ProductInstanceFolder));
                }
                this.LogInfo(string.Empty);
            }

            this.LogInfo(Resources.StartingInstallerService);
        }

        private void InstallerService_Updating(object sender, EventArgs e)
        {
            TextWriter.WriteLine(Resources.UpdateStarted);
            this.LogInfo(Resources.UpdateStarted);
        }

        private void InstallerService_Updated(object sender, EventArgs e)
        {
            TextWriter.WriteLine(Resources.UpdateCompleted);
            this.LogInfo(Resources.UpdateCompleted);
        }

        private void InstallerService_Notification(object sender, NotificationEventArgs e)
        {
            TextWriter.WriteLine(e.Message);
            this.LogInfo(e.Message);
        }

        private void InstallerService_Installing(object sender, EventArgs e)
        {
            TextWriter.WriteLine(Resources.InstallationStarted);
            this.LogInfo(Resources.InstallationStarted);
        }

        private void InstallerService_Installed(object sender, EventArgs e)
        {
            TextWriter.WriteLine(Resources.InstallationCompleted);
            this.LogInfo(Resources.InstallationCompleted);
        }

        private void InstallerService_ErrorOccurred(object sender, ErrorOccurredEventArgs e)
        {
            string exc = e.Exception != null ? e.Exception.ToString() : Resources.NoException;
            TextWriter.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.ErrorInstalling, e.Message, exc));
            this.LogInfo(string.Format(CultureInfo.InvariantCulture, Resources.ErrorInstalling, e.Message, exc));
        }
    }
}