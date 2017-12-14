using Microarea.Snap.Core;
using Microarea.Snap.IO;
using Microarea.Snap.Services.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal class InstallerService : IInstallerService, ILogger
    {
        readonly object lockTicket = new object();

        public event EventHandler<EventArgs> Starting;
        public event EventHandler<EventArgs> Started;
        public event EventHandler<EventArgs> Stopping;
        public event EventHandler<EventArgs> Stopped;
        public event EventHandler<EventArgs> Installing;
        public event EventHandler<EventArgs> Installed;
        public event EventHandler<EventArgs> Updating;
        public event EventHandler<EventArgs> Updated;
        public event EventHandler<EventArgs> Uninstalling;
        public event EventHandler<EventArgs> Uninstalled;
        public event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;

        public event EventHandler<NotificationEventArgs> Notification;

        Queue<Request> requests = new Queue<Request>();

        Thread workingThread;

        IPackagesRegistry packagesRegistry;
        IPackageManager packageManager;
        IClickOnceService clickOnceDeployerService;
        IProductCompatibilityService productCompatibilityService;
        IInstallationVersionService installationVersionService;
        ITransactionManager transactionManager;

        public bool IsRunning { get { return this.workingThread != null; } }

        public InstallerService(
            IPackageManager packageManager,
            IPackagesRegistry packagesRegistry,
            IClickOnceService clickOnceDeployerService,
            IProductCompatibilityService productCompatibilityService,
            IInstallationVersionService installationVersionService,
            ITransactionManager transactionManager
            )
        {
            this.packageManager = packageManager;
            this.packagesRegistry = packagesRegistry;
            this.clickOnceDeployerService = clickOnceDeployerService;
            this.productCompatibilityService = productCompatibilityService;
            this.installationVersionService = installationVersionService;
            this.transactionManager = transactionManager;
        }

        protected virtual void OnStarting()
        {
            Starting?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStopping()
        {
            Stopping?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnInstalling()
        {
            Installing?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnInstalled()
        {
            Installed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUpdating()
        {
            Updating?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUninstalling()
        {
            Uninstalling?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUninstalled()
        {
            Uninstalled?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnNotification(NotificationEventArgs e)
        {
            Notification?.Invoke(this, e);
        }

        protected virtual void OnError(ErrorOccurredEventArgs args)
        {
            ErrorOccurred?.Invoke(this, args);
        }

        void Enqueue(Request request)
        {
            lock (this.lockTicket)
            {
                this.requests.Enqueue(request);
            }
        }
        Request Dequeue()
        {
            lock (this.lockTicket)
            {
                if (this.requests.Count > 0)
                {
                    return this.requests.Dequeue();
                }

                return null;
            }
        }

        public void Install(IPackage package, IFolder where)
        {
            if (package == null)
            {
                var exc = new ArgumentNullException("package");
                this.LogError(Resources.NullPackageReceived, exc);
                throw exc;
            }
            if (package.Manifest == null)
            {
                var exc = new PackageException("unable to find any manifest in the given package");
                this.LogError(Resources.NoManifest, exc);
                throw exc;
            }
            if (string.IsNullOrWhiteSpace(package.Manifest.Id))
            {
                var exc = new PackageException("unable to find any manifest id in the given package");
                this.LogError(Resources.NoManifestId, exc);
                throw exc;
            }

            this.productCompatibilityService.EnsureProductCompatibility(package);

            if (where == null)
            {
                var exc = new ArgumentNullException("where");
                this.LogError(Resources.NullWhereFolderReceived, exc);
                throw exc;
            }
            if (!where.Exists)
            {
                this.LogInfo(Resources.NoWhereFolder);
                where.Create();
            }

            var installedPackage = packagesRegistry[package.Manifest.Id];
            if (installedPackage != null)
            {
                if (package.Manifest.Version < installedPackage.Manifest.Version)
                {
                    this.OnError(new ErrorOccurredEventArgs() { Message = string.Format(CultureInfo.InvariantCulture, Resources.PackageIdAlreadyPresentDifferentVersion, package.Manifest.Id, installedPackage.Manifest.Version, package.Manifest.Version) });
                    return;
                }
                this.Enqueue(new Request()
                {
                    Package = package,
                    InstalledPackage = installedPackage,
                    RequestType = RequestType.Update,
                    RootFolder = where
                });
            }
            else
            {
                this.Enqueue(new Request()
                {
                    Package = package,
                    RequestType = RequestType.Install,
                    RootFolder = where
                });
            }

            StartService();
        }

        public void Uninstall(string packageId, IFolder where)
        {
            if (packageId == null)
            {
                var exc = new ArgumentNullException("packageId");
                this.LogError(Resources.NullPackageIdReceived, exc);
                throw exc;
            }
            if (string.IsNullOrWhiteSpace(packageId))
            {
                var exc = new ArgumentException("packageId is whitespace");
                this.LogError(Resources.PackageIdWhiteSpaces, exc);
                throw exc;
            }
            if (where == null)
            {
                var exc = new ArgumentNullException("where");
                this.LogError(Resources.WhereNull, exc);
                throw exc;
            }
            if (!where.Exists)
            {
                this.OnError(new ErrorOccurredEventArgs() { Message = string.Format(CultureInfo.InvariantCulture, Resources.CannotUninstallWhereNotExist, where) });
                return;
            }

            var installedPackage = packagesRegistry[packageId];
            if (installedPackage == null)
            {
                this.OnError(new ErrorOccurredEventArgs() { Message = string.Format(CultureInfo.InvariantCulture, Resources.PackageIdNotInstalled, packageId) });
                return;
            }

            this.Enqueue(new Request()
            {
                Package = installedPackage,
                RequestType = RequestType.Uninstall,
                RootFolder = where
            });
            StartService();
        }

        public void Join()
        {
            if (this.workingThread != null)
            {
                this.workingThread.Join();
            }
        }

        private void StartService()
        {
            lock (this.lockTicket)
            {
                if (this.workingThread != null)
                {
                    return;
                }

                this.OnStarting();
                ThreadPool.QueueUserWorkItem(Worker);
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Worker(object state)
        {
            lock (this.lockTicket)
            {
                if (this.workingThread != null)
                {
                    return;
                }
                this.workingThread = Thread.CurrentThread;
            }
            this.OnStarted();

            bool clickOnceDeployerNeeded = false;
            bool invalidateMenuCache = false;

            var currentRequest = this.Dequeue();

            while (currentRequest != null)
            {
                switch (currentRequest.RequestType)
                {
                    case RequestType.Install:
                        {
                            this.OnInstalling();

                            if (!clickOnceDeployerNeeded)
                            {
                                clickOnceDeployerNeeded = IsClickOnceDeployerNeeded(currentRequest);
                            }
                            if (!invalidateMenuCache)
                            {
                                invalidateMenuCache = IsMenuCacheToBeInvalidated(currentRequest);
                            }

                            this.Install(currentRequest);

                            this.OnInstalled();

                            break;
                        }
                    case RequestType.Update:
                        {
                            this.OnUpdating();

                            if (!clickOnceDeployerNeeded)
                            {
                                clickOnceDeployerNeeded = IsClickOnceDeployerNeeded(currentRequest);
                            }
                            if (!invalidateMenuCache)
                            {
                                invalidateMenuCache = IsMenuCacheToBeInvalidated(currentRequest);
                            }

                            this.Update(currentRequest);

                            this.OnUpdated();

                            break;
                        }
                    case RequestType.Uninstall:
                        {
                            this.OnUninstalling();

                            if (!clickOnceDeployerNeeded)
                            {
                                clickOnceDeployerNeeded = IsClickOnceDeployerNeeded(currentRequest);
                            }
                            if (!invalidateMenuCache)
                            {
                                invalidateMenuCache = IsMenuCacheToBeInvalidated(currentRequest);
                            }

                            this.Uninstall(currentRequest);

                            this.OnUninstalled();
                            break;
                        }
                    default:
                        break;
                }

                currentRequest = this.Dequeue();
            }

            this.OnNotification(new Services.NotificationEventArgs() { Message = Resources.SettingUpLastDetails });
            if (clickOnceDeployerNeeded)
            {
                try
                {
                    this.LogInfo(Resources.LaunchingClickOnce);
                    clickOnceDeployerService.Deploy();
                    clickOnceDeployerService.UpdateDeployment();
                    this.LogInfo(Resources.ClickOnceTerminated);
                }
                catch (Exception exc)
                {
                    this.OnError(new Services.ErrorOccurredEventArgs() { Message = Resources.ClickOnceTerminatedWithErrors, Exception = exc });
                }
            }

            if (invalidateMenuCache)
            {
                try
                {
                    this.LogInfo(Resources.InvalidatingMenuCache);
                    this.installationVersionService.InvalidateMenuCache();
                    this.LogInfo(Resources.MenuCacheInvalidated);
                }
                catch (Exception exc)
                {
                    this.OnError(new Services.ErrorOccurredEventArgs() { Message = Resources.ClickOnceTerminatedWithErrors, Exception = exc });
                }
            }
            this.OnNotification(new Services.NotificationEventArgs() { Message = Resources.ChangesSuccessfullyApplied });

            this.OnStopping();
            lock (this.lockTicket)
            {
                this.workingThread = null;
            }
            this.OnStopped();
        }

        private static bool IsClickOnceDeployerNeeded(Request currentRequest)
        {
            //E` necessario lanciare ClickOnceDeployer se e` stata modificata la cartella Publish
            return currentRequest
                .Package
                .Manifest
                .Files
                .Where(f => f.FullName.IndexOf("publish", StringComparison.OrdinalIgnoreCase) > -1)
                .FirstOrDefault() != null;
        }

        private static bool IsMenuCacheToBeInvalidated(Request currentRequest)
        {
            //E` necessario invalidare la cache del menu` se nel pacchetto sono presenti file di menu
            return currentRequest
                .Package
                .Manifest
                .Files
                .Where(f => string.Compare(Path.GetExtension(f.FullName), ".menu", StringComparison.OrdinalIgnoreCase) == 0)
                .FirstOrDefault() != null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Install(Request currentRequest)
        {
            transactionManager.Start();

            var invariantCulture = CultureInfo.InvariantCulture;
            this.OnNotification(new Services.NotificationEventArgs() { Message = string.Format(invariantCulture, Resources.InstallingPackage, currentRequest.Package.Manifest.Id, currentRequest.Package.PackageFile.FullName) });
            try
            {
                this.packageManager.Install(currentRequest.Package, currentRequest.RootFolder);

                this.packagesRegistry.Add(currentRequest.Package);

                transactionManager.Commit();

                this.OnNotification(new Services.NotificationEventArgs() { Message = string.Format(invariantCulture, Resources.PackageInstalled, currentRequest.Package.Manifest.Id) });
            }
            catch (Exception exc)
            {
                this.OnError(new ErrorOccurredEventArgs() { Message = string.Format(invariantCulture, Resources.ErrorInstallingPackage, currentRequest.Package.Manifest.Id), Exception = exc });
                transactionManager.Rollback();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Update(Request currentRequest)
        {
            transactionManager.Start();

            var invariantCulture = CultureInfo.InvariantCulture;
            this.OnNotification(new Services.NotificationEventArgs() { Message = string.Format(invariantCulture, Resources.UpdatingPackage, currentRequest.Package.Manifest.Id, currentRequest.Package.PackageFile.FullName) });
            try
            {
                this.packageManager.Update(currentRequest.InstalledPackage, currentRequest.Package, currentRequest.RootFolder);

                this.packagesRegistry.Remove(currentRequest.InstalledPackage.Manifest.Id);
                this.packagesRegistry.Add(currentRequest.Package);

                transactionManager.Commit();

                this.OnNotification(new Services.NotificationEventArgs() { Message = string.Format(invariantCulture, Resources.PackageUpdated, currentRequest.Package.Manifest.Id) });
            }
            catch (Exception exc)
            {
                this.OnError(new ErrorOccurredEventArgs() { Message = string.Format(invariantCulture, Resources.ErrorUpdatingPackage, currentRequest.Package.Manifest.Id), Exception = exc });
                transactionManager.Rollback();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Uninstall(Request currentRequest)
        {
            transactionManager.Start();

            var invariantCulture = CultureInfo.InvariantCulture;
            this.OnNotification(new Services.NotificationEventArgs() { Message = string.Format(invariantCulture, Resources.UninstallingPackage, currentRequest.Package.Manifest.Id) });
            try
            {
                this.packageManager.Uninstall(currentRequest.Package, currentRequest.RootFolder);

                this.packagesRegistry.Remove(currentRequest.Package.Manifest.Id);

                transactionManager.Commit();
            }
            catch (Exception exc)
            {
                this.OnError(new ErrorOccurredEventArgs() { Message = string.Format(invariantCulture, Resources.ErrorUninstallingPackage, currentRequest.Package.Manifest.Id), Exception = exc });
                transactionManager.Rollback();
            }
            this.OnNotification(new Services.NotificationEventArgs() { Message = string.Format(invariantCulture, Resources.PackageUninstalled, currentRequest.Package.Manifest.Id) });
        }
    }
}
