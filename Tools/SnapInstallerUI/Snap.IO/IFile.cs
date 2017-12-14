using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    public interface IFile : IFileSystemElement
    {
        Stream OpenRead();
        Stream OpenWrite();
        Stream OpenRead(IFolder rootFolder);
        Stream OpenWrite(IFolder rootFolder);

        void CopyTo(IFile destination);
        void Delete(IFolder rootFolder);
        void Delete();
        IFolder ParentFolder { get; }
    }
}