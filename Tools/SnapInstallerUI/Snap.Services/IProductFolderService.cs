using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    public interface IProductFolderService
    {
        bool IsProductFolderSet { get; }

        string ProductInstanceFolder { get; set; }

        void EnsureProductFolder();
    }
}
