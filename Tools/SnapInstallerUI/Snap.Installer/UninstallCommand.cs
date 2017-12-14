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
using Microarea.Snap.Installer.Properties;
using System.Globalization;

namespace Microarea.Snap.Installer
{

    /// snapinstaller -u -pid packageid: disinstalla un pacchetto
    /// snapinstaller -u -h: help
    internal class UninstallCommand : Command, ILogger
    {
        const string packageIdArg = "-PID";
        string packageId;
        bool packageIdFound;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public UninstallCommand(IEnumerable<string> args, TextWriter textWriter, IInversionOfControlFactory factory)
            : base(textWriter, factory)
        {
            try
            {
                for (int i = 0; i < args.Count(); i += 2)
                {
                    switch (args.ElementAt(i).ToUpperInvariant())
                    {
                        case packageIdArg:
                            packageId = args.ElementAt(i + 1);
                            packageIdFound = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                TextWriter.WriteLine("Error during 'Uninstall' command: " + exc.ToString());
            }
        }
        public override void Execute()
        {
            base.Execute();

            if (packageIdFound)
            {
                var installerService = Factory.GetInstance<IInstallerService>();

                ListenTo(installerService);

                var settings = Factory.GetInstance<ISettings>();
                var productInstanceFolder = Factory.GetInstance<IO.IFolder, string>(settings.ProductInstanceFolder);

                installerService.Uninstall(packageId, productInstanceFolder);
                //Let the installer thread start
                System.Threading.Thread.Sleep(1000);
                installerService.Join();
                UnlistenTo(installerService);
            }
            else
            {
                TextWriter.WriteLine("Syntax:\tsnapinstaller.exe -cp [options]");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("Options:");
                TextWriter.WriteLine("");
                TextWriter.WriteLine("\t -pid packageid\tid of the package to uninstall");
                TextWriter.WriteLine("\t -h\t\tPrint this help");
            }
        }

        private void UnlistenTo(IInstallerService installerService)
        {
            installerService.ErrorOccurred -= InstallerService_ErrorOccurred;
            installerService.Installed -= InstallerService_Installed;
            installerService.Installing -= InstallerService_Installing;
            installerService.Notification -= InstallerService_Notification;
            installerService.Uninstalled -= InstallerService_Uninstalled;
            installerService.Uninstalling -= InstallerService_Uninstalling;
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
            installerService.Notification += InstallerService_Notification;
            installerService.Uninstalled += InstallerService_Uninstalled;
            installerService.Uninstalling += InstallerService_Uninstalling;
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
            this.LogInfo(Resources.StartingInstallerService);
        }

        private void InstallerService_Uninstalling(object sender, EventArgs e)
        {
            TextWriter.WriteLine(Resources.UninstallationStarted);
            this.LogInfo(Resources.UninstallationStarted);
        }

        private void InstallerService_Uninstalled(object sender, EventArgs e)
        {
            TextWriter.WriteLine(Resources.UninstallationCompleted);
             this.LogInfo(Resources.UninstallationCompleted);
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