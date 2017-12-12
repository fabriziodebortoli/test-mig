using Alphaleonis.Win32.Filesystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.IO
{
    public class TransactionalFile : File
    {
        public TransactionalFile(string fullName)
            : base(fullName)
        {
        }

        public override IFolder ParentFolder
        {
            get
            {
                return new TransactionalFolder(Path.GetDirectoryName(this.FullName));
            }
        }

        public override System.IO.Stream OpenWrite(IFolder rootFolder)
        {
            var path = this.FullName;
            if (rootFolder != null)
            {
                path = Path.Combine(rootFolder.FullName, path);
            }

            System.IO.Stream returnValue = null;
            this.WrapWithTransaction(
                (KernelTransaction transaction)
                =>
                {
                    returnValue = Alphaleonis.Win32.Filesystem.File.OpenWriteTransacted(transaction, path);
                }
                );

            return returnValue;
        }

        public override void Delete(IFolder rootFolder)
        {
            this.WrapWithTransaction(
                (KernelTransaction transaction)
                =>
                {
                    if (!IsPathRooted && rootFolder != null)
                    {
                        var workingPath = Path.Combine(rootFolder.FullName, this.FullName);
                        if (Alphaleonis.Win32.Filesystem.File.Exists(workingPath))
                        {
                            Alphaleonis.Win32.Filesystem.File.DeleteTransacted(transaction, workingPath);
                        }
                    }
                    if (Exists)
                    {
                        Alphaleonis.Win32.Filesystem.File.DeleteTransacted(transaction, this.FullName);
                    }
                }
                );
        }

        public override void CopyTo(IFile destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (string.IsNullOrWhiteSpace(destination.FullName))
            {
                throw new ArgumentException("destination is empty");
            }
            if (string.CompareOrdinal(this.FullName, destination.FullName) == 0)
            {
                return;
            }

            this.WrapWithTransaction(
                (KernelTransaction transaction)
                =>
                {
                    Alphaleonis.Win32.Filesystem.File.CopyTransacted(transaction, this.FullName, destination.FullName);
                }
                );
        }
    }
}
