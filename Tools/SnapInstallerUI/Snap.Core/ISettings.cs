using Microarea.Snap.IO;
using System;

namespace Microarea.Snap.Core
{
    public interface ISettings
    {
        string WorkingFolder { get; }
        string SnapPackagesRegistryFolder { get; set; }

        string ProductInstanceFolder { get; set; }

        string LogsFolder { get; set; }
        bool UseTransactions { get; set; }

        event EventHandler<EventArgs> ProductInstanceFolderChanged;
    }
}