using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    public interface IInstallerService
    {
        event EventHandler<EventArgs> Starting;
        event EventHandler<EventArgs> Started;
        event EventHandler<EventArgs> Stopping;
        event EventHandler<EventArgs> Stopped;
        event EventHandler<EventArgs> Installing;
        event EventHandler<EventArgs> Installed;
        event EventHandler<EventArgs> Updating;
        event EventHandler<EventArgs> Updated;
        event EventHandler<EventArgs> Uninstalling;
        event EventHandler<EventArgs> Uninstalled;
        event EventHandler<ErrorOccurredEventArgs> ErrorOccurred;
        event EventHandler<NotificationEventArgs> Notification;

        bool IsRunning { get; }

        void Install(IPackage package, IFolder where);
        void Uninstall(string packageId, IFolder where);
        void Join();
    }
}
