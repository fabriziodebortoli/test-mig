using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    public interface IPackageLoader
    {
        IPackage LoadPackage(IFile packageFile);
    }
}
